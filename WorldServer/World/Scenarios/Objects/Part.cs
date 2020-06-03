using System.Collections.Generic;
using System.Linq;
using Common;
using FrameWork;
using GameData;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using WorldServer.World.Positions;
using Object = WorldServer.World.Objects.Object;

namespace WorldServer.World.Scenarios.Objects
{
    public class Part: GameObject
    {
        private int _pickupTime = 0;
        private int _dropTime = 0;
        private readonly BuffInfo _buff;
        public Point3D Position;
        class Interact
        {
            public Player Player;
            public EventDelegateEx Del;
            public Part Part;
            public EventDelegate DamageDel;
            public Interact(Part part, Player player, EventDelegateEx del)
            {
                Player = player;
                Del = del;
                Part = part;
                player.EvtInterface.AddEventNotify(EventName.OnReceiveDamage, OnPlayerDamage);
            }

            public bool OnPlayerDamage(Object player, object a)
            {
                Part.StopInteract((Player)Player);
                return false;
            }

            public void Stop()
            {
                Player.EvtInterface.RemoveEventNotify(EventName.OnReceiveDamage, OnPlayerDamage);
            }
        }


        private List<Interact> _interacting = new List<Interact>();
        private Player _carrier = null;
        private GameObject _targetCart = null;
        private EventDelegateEx _dropDel;
        private FLAG_EFFECTS? _flagEffect;
        public delegate void PartDelegate(Player plr, Part part);

        public PartDelegate PickedUp;
        public PartDelegate Lost;
        public PartDelegate DroppedOff;

        public Part(GameObject_spawn spawn, FLAG_EFFECTS? flagEffect, int pickUpTime=0, int dropTime = 0) :base(spawn)
        {
            _pickupTime = pickUpTime;
            _dropTime = dropTime;
            _flagEffect = flagEffect;
            Position = new Point3D(spawn.WorldX, spawn.WorldY, spawn.WorldZ);
        }

        public void BeginPickup(Player plr)
        {
            if (_carrier == null)
            {
                if (plr.IsMounted)
                {
                    plr.Dismount();
                }
                if (_pickupTime != 0)
                {
                    Interact interact = null;
                    lock (_interacting)
                    {
                        if (!_interacting.Any(e => e.Player == plr))
                        {
                            interact = new Interact(this, plr, null);
                            _interacting.Add(interact);
                        }
                    }
                    if (interact != null)
                    {
                        plr.KneelDown(Oid, true, (ushort)_pickupTime );
                        plr.EvtInterface.AddEventNotify(EventName.OnMove, OnPlayerMove);
                        plr.EvtInterface.AddEventNotify(EventName.OnDie, OnPlayerDied);
                        plr.EvtInterface.AddEventNotify(EventName.OnReceiveDamage, OnPlayerDamage);
                        interact.Del = new EventDelegateEx(OnPickedUp);
                        EvtInterface.AddEvent(interact.Del, _pickupTime, 1, plr);
                    }
                }
                else
                    Pickup(plr);
            }
        }

        private void StopInteract(Player plr)
        {
          
#if (DEBUG)
                Log.Info("Part.StopInteract()", "Stopping interaction for " + plr.Name);
#endif
            plr.EvtInterface.RemoveEventNotify(EventName.OnMove, OnPlayerMove);
            plr.EvtInterface.RemoveEventNotify(EventName.OnDie, OnPlayerDied);
            plr.EvtInterface.RemoveEventNotify(EventName.OnReceiveDamage, OnPlayerDamage);

            if (_dropDel != null && plr == _carrier)
            {
                if (_targetCart == null)
                    plr.KneelDown(Oid, false);
                plr.KneelDown(Oid, false);

                plr.EvtInterface.RemoveEvent(_dropDel);
                _dropDel = null;
            }

            Interact interact = null;
            lock (_interacting)
            {
                foreach (var i in _interacting.Where(e => e.Player == plr).ToList())
                {
#if (DEBUG)
                    Log.Info("Part.StopInteract()", "Removing Interaction for " + i.Player.Name);
#endif
                    interact = i;
                    i.Stop();
                    _interacting.Remove(i);
                }
            }

            if (interact != null)
            {
           
                EvtInterface.RemoveEvent(interact.Del);
            }
            if (_targetCart == null)
                plr.KneelDown(Oid, false);
            else
            {
#if (DEBUG)
                Log.Info("Part.StopInteract()", "Ending kneeling animation for " + plr.Name);
#endif
                plr.KneelDown(_targetCart.Oid, false);
                _targetCart = null;
            }
        }

        private bool OnPlayerMove(Object player, object a)
        {
            StopInteract((Player)player);
            return false;
        }
        private bool OnPlayerDied(Object player, object a)
        {
            PartLost((Player)player);
            StopInteract((Player)player);
            return false;
        }

        private bool OnPlayerDamage(Object player, object a)
        {
            StopInteract((Player)player);
            return false;
        }

        public void BeginDropOff(Player plr, GameObject targetCart)
        {
#if (DEBUG)
            Log.Info("Part.BeginDropOff()", plr.Name + " is dropping off the part at Oid " + targetCart.Oid);
#endif

            if (_carrier == plr)
            {
                if (_dropTime != 0)
                {
                    _targetCart = targetCart;
                    plr.KneelDown(_targetCart.Oid, true, (ushort)_dropTime);

                    plr.EvtInterface.AddEventNotify(EventName.OnMove, OnPlayerMove);
                    plr.EvtInterface.AddEventNotify(EventName.OnDie, OnPlayerDied);
                    plr.EvtInterface.AddEventNotify(EventName.OnReceiveDamage, OnPlayerDamage);
                    _dropDel = new EventDelegateEx(OnDroppedOff);
                    plr.EvtInterface.AddEvent(_dropDel, _dropTime, 1, plr);

#if (DEBUG)
                    Log.Info("Part.BeginDropOff()", "Event Added: OnDroppedOff should be called in 2s");
#endif


                }
                else
                    DropOff(plr);
            }
        }

        private void OnDroppedOff(object obj)
        {
#if (DEBUG)
            Log.Info("Part.OnDroppedOff()", "OnDroppedOff successfully called");
#endif
            Player plr = (Player)obj;
            DropOff(plr);
        }

        private void OnPickedUp(object obj)
        {
            Player plr = (Player)obj;
           
            if (_carrier == null)
            {
                //cancel pickup by everyone
                var players = new List<Interact>();
                lock(_interacting)
                {
                    players = _interacting.ToList();
                    _interacting.Clear();
                }

                foreach (var p in players)
                    StopInteract(p.Player);

                Pickup(plr);
                _carrier = plr;
            }
        }


        public void Pickup(Player plr)
        {
            _carrier = plr;
            plr.CanMount = false;
            plr.Dismount();
            plr.EvtInterface.AddEventNotify(EventName.OnDie, OnPlayerDied);

            if (_flagEffect.HasValue)
            {
                plr.OSInterface.AddEffect(0xB, (byte)_flagEffect.Value);
            }

            if (PickedUp != null)
                PickedUp(plr, this);
        }

        public void PartLost(Player plr)
        {
            _carrier = null;
         
            plr.CanMount = true;

            if (_flagEffect.HasValue)
                plr.OSInterface.RemoveEffect(0xB);


            if (Lost != null)
                Lost(plr, this);
        }

        public void DropOff(Player plr)
        {
#if (DEBUG)
            Log.Info("Part.DropOff()", "Part Dropped off.");
#endif
            plr.CanMount = true;
            if (_targetCart != null)
                plr.KneelDown(_targetCart.Oid, false);
            plr.KneelDown(Oid, false);

            _carrier = null;
            plr.EvtInterface.RemoveEventNotify(EventName.OnDie, OnPlayerDied);
            plr.EvtInterface.RemoveEventNotify(EventName.OnMove, OnPlayerMove);
            plr.EvtInterface.RemoveEventNotify(EventName.OnReceiveDamage, OnPlayerDamage);
            if (_dropDel != null)
            {
                plr.EvtInterface.RemoveEvent(_dropDel);
                _dropDel = null;
            }
            if (_dropDel != null)

            if (_flagEffect.HasValue)
                plr.OSInterface.RemoveEffect(0xB);


            if (DroppedOff != null)
                DroppedOff(plr, this);
        }
    }
}
