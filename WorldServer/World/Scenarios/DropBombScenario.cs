using System;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using Common;
using FrameWork;
using GameData;
using WorldServer.NetWork.Handler;
using WorldServer.Services.World;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Objects;
using WorldServer.World.Positions;
using WorldServer.World.Scenarios.Objects;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Scenarios
{
    public class DropBombScenario : Scenario
    {
        private GameObject _centerGlow;
        private Part _bomb;
        private GameObject _orderGunPowder;
        private GameObject _destroGunPowder;
        private GameObject _plantedBomb;

        private Player _carrier2;

        private Player _carrier
        {
            get
            {
                return _carrier2;
            }
            set
            {
                _carrier2 = value;
            }
        }
        private Point3D _bombSpawn = new Point3D();
     
        public DropBombScenario(Scenario_Info info, int tier)
            : base(info, tier)
        {
            foreach (Scenario_Object scenarioObject in info.ScenObjects)
            {
                if (scenarioObject.ObjectiveName == "Powder Keg")
                {
                    CreateBomb(scenarioObject.WorldPosX, scenarioObject.WorldPosY, scenarioObject.PosZ, scenarioObject.Heading);
                    _bombSpawn = new Point3D(scenarioObject.WorldPosX, scenarioObject.WorldPosY, scenarioObject.PosZ);
                    _centerGlow = AddObject(scenarioObject.WorldPosX, scenarioObject.WorldPosY, scenarioObject.PosZ, scenarioObject.Heading, 99858);
                }

                if (scenarioObject.ObjectiveName == "Gun Powder" && scenarioObject.Identifier == 1)
                    _orderGunPowder = AddObject(scenarioObject.WorldPosX, scenarioObject.WorldPosY, scenarioObject.PosZ, scenarioObject.Heading, 100331);

                if (scenarioObject.ObjectiveName == "Gun Powder" && scenarioObject.Identifier == 2)
                    _destroGunPowder = AddObject(scenarioObject.WorldPosX, scenarioObject.WorldPosY, scenarioObject.PosZ, scenarioObject.Heading, 100331);




            }
            _orderGunPowder.Flags = 0x24;
            _destroGunPowder.Flags = 0x24;
            _bomb.Flags = 0x24;
        }

        private void CreateBomb(int x, int y, int z, int o)
        {
            _carrier = null;
            if (_bomb != null)
            {
                _bomb.RemoveFromWorld();
                _bomb = null;
            }
            if (_carrier != null)
            {
                _carrier.OSInterface.RemoveEffect((byte)0xB);
                _carrier = null;
            }

            GameObject_proto glowProto = GameObjectService.GetGameObjectProto(100112);

            GameObject_spawn spawn = new GameObject_spawn
            {
                Guid = (uint)GameObjectService.GenerateGameObjectSpawnGUID(),
                WorldO = 0,
                WorldX = x,
                WorldY = y,
                WorldZ = (ushort)z,
                ZoneId = Region.RegionId,
               
                
            };
            spawn.BuildFromProto(glowProto);

            _bomb = new Part(spawn, FLAG_EFFECTS.Bomb, 2000, 2000)
            {
                PickedUp = new Part.PartDelegate(BombPickedUp),
                DroppedOff = new Part.PartDelegate(BombDroppedOff),
                Lost = new Part.PartDelegate(BombLost)
            };
            Region.AddObject(_bomb, spawn.ZoneId);
            if(_centerGlow != null)
                _centerGlow.VfxState = 0;

        }

        public override void OnClose()
        {
            for (int i = 0; i < 2; ++i)
                foreach (Player plr in Players[i])
                {
                    plr.OSInterface.RemoveEffect((byte)0xB);
                    plr.CanMount = true;
                }
        }

        public override void RemovePlayer(Player plr, bool logout)
        {
            base.RemovePlayer(plr, logout);

            plr.OSInterface.RemoveEffect((byte)0xB);
            plr.CanMount = true;


            if (_carrier == plr)
            {
               CreateBomb();
            }
        }
        private void BombPickedUp(Player plr, Part part)
        {

            RemoveBombFromWorld();

            _carrier = plr;
            EvtInterface.RemoveEvent(CarrierBombTimer);
            EvtInterface.AddEvent(CarrierBombTimer, RandomMgr.Next(50000, 80000), 1);

            if (_carrier.Realm == Realms.REALMS_REALM_DESTRUCTION)
            {
                _centerGlow.VfxState = 2;
            }
            else
                _centerGlow.VfxState = 1;

            Broadcast(new[] { plr.GenderedName, (plr.Realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction"), _bomb.Name }, ChatLogFilters.CHATLOGFILTERS_C_WHITE, Localized_text.TEXT_FLAG_CAPTURE);

        }


        private void BombDroppedOff(Player plr, Part part)
        {
            if (plr == null)
            {
                Log.Error("BombDroppedOff", "NULL player");
                return;
            }
            if (_bomb == null)
            {
                Log.Error("BombDroppedOff", "NULL bomb");
                return;
            }
            GivePoints((_carrier.Realm == Realms.REALMS_REALM_ORDER ? 1 : 2), 75);
            _carrier.CanMount = true;
            RemoveBombBuff(_carrier);
            EvtInterface.AddEvent(DetonateStockpile, RandomMgr.Next(3000, 5000), 1);
            Broadcast(new[] { plr.GenderedName, (plr.Realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction"), _bomb.Name }, ChatLogFilters.CHATLOGFILTERS_C_WHITE, Localized_text.TEXT_BOMB_CAPTURE);
        }

        private void BombLost(Player plr, Part part)
        {
            RemoveBombBuff(plr);
        }

        private GameObject AddObject(int x, int y, int z, int o, uint entry)
        {
            GameObject_proto glowProto = GameObjectService.GetGameObjectProto(entry);

            GameObject_spawn spawn = new GameObject_spawn
            {
                Guid = (uint)GameObjectService.GenerateGameObjectSpawnGUID(),
                WorldO = o,
                WorldX = x,
                WorldY = y,
                WorldZ = (ushort)z,
                ZoneId = Region.RegionId,
            };
            spawn.BuildFromProto(glowProto);

            var obj = new GameObject(spawn);
            Region.AddObject(obj, spawn.ZoneId);

            return obj;
        }

       
        public override void Interact(GameObject obj, Player plr, InteractMenu menu)
        {
            base.Interact(obj, plr, menu);

            if ((obj == _destroGunPowder  && plr.Realm == Realms.REALMS_REALM_ORDER && plr == _carrier) ||
                (obj == _orderGunPowder && plr.Realm == Realms.REALMS_REALM_DESTRUCTION && plr == _carrier))
            {
                //drop part
                PutPartInCart(plr, obj);
            }
            else if (_carrier == null && obj.Name == _bomb.Name)
            {
                if (plr.StealthLevel > 0)
                {
                    plr.SendClientMessage("You can't interact with objects while in stealth.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    return;
                }


                if (_carrier == null)
                    _bomb.BeginPickup(plr);
            
            }
        }

        private void RemoveBombFromWorld()
        {
         
            var bombs = Region.GetObjects<GameObject>();
            if (bombs != null)
                foreach (var obj in bombs.Where(e => e.Name == "Powder Keg").ToList())
                {
                    obj.RemoveFromWorld();
                }

            EvtInterface.RemoveEvent(CarrierBombTimer);

        }
        private void RemoveBombBuff(Player player)
        {
            if (player != null)
            {
                player.OSInterface.RemoveEffect((byte)0xB);
                EvtInterface.RemoveEvent(CarrierBombTimer);
            }
            _carrier = null;
        }

        public void CreateBomb()
        {
            RemoveBombFromWorld();
            _plantedBomb = null;

            CreateBomb(_bombSpawn.X, _bombSpawn.Y, _bombSpawn.Z, 0);
     
        }

        public void DropPart()
        {
        }

        public void PutPartInCart(Player plr, GameObject powder)
        {
            if (_carrier == plr)
            {
                if ((powder == _destroGunPowder && plr.Realm == Realms.REALMS_REALM_ORDER)
                    || (powder == _orderGunPowder &&  plr.Realm == Realms.REALMS_REALM_DESTRUCTION))
                {
                    _bomb.BeginDropOff(plr, powder);
                    _plantedBomb = powder;
                }
            }

        }

        private void Explosion(Point3D location, Player target = null)
        {
            var killPlayers = new List<Player>();

            if (target == null)
            {
                killPlayers = Region.WorldQuery<Player>(location, 10);
            }

            if (target != null && killPlayers.Contains(target))
                killPlayers.Add(target);

            AbilityDamageInfo damageThisPass = new AbilityDamageInfo
            {
                Entry = 14050,
                DisplayEntry = 0,
                DamageType = DamageTypes.RawDamage,
                MinDamage = (ushort)(30000),
                CastPlayerSubID = 0
            };


            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAY_EFFECT, 30);
            Out.WriteUInt16(264);
            Out.WriteUInt16(0);
            Out.WriteUInt32((uint)location.X);
            Out.WriteUInt32((uint)location.Y);
            Out.WriteUInt32((uint)location.Z);
            Out.WriteUInt16(100);
            Out.WriteUInt16(100);
            Out.WriteUInt16(100);
            Out.WriteUInt16(0);


            Region.DispatchPacket(Out, location, 400);


            foreach (var player in killPlayers)
            {
                CombatManager.InflictDamage(damageThisPass, 20, player, player);
               
            }

            foreach (var player in Region.WorldQuery<Player>(location, 50))
            {
                if (!player.IsDead)
                {
                    int val = RandomMgr.Next(2000, 8000);
                    player.ApplyKnockback(location, 700, 50, 0, 2, 1);
                    CombatManager.InflictDamage(new AbilityDamageInfo
                    {
                        Entry = 14050,
                        DisplayEntry = 0,
                        DamageType = DamageTypes.RawDamage,
                        MinDamage = (ushort)(val),
                        CastPlayerSubID = 0
                    }, player.Level, player, player);

                }
            }

            if(_carrier != null)
            {
                _carrier.CanMount = true;
                RemoveBombBuff(_carrier);
            }

            CreateBomb();
        }

        private void DetonateStockpile()
        {
            if (_plantedBomb != null)
                Explosion(_plantedBomb.WorldPosition);

        }

        public void Broadcast(string msg)
        {
            for (int i = 0; i < 2; ++i)
                foreach (Player plr in Players[i])
                {
                    plr.SendClientMessage(msg, ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    SendObjectiveStates(plr);
                }
        }
        public void Broadcast(string[] Msgs, ChatLogFilters filter, Localized_text localizeEntry)
        {
            for (int i = 0; i < 2; ++i)
                foreach (Player plr in Players[i])
                {
                    plr.SendLocalizeString(Msgs, filter, localizeEntry);
                    SendObjectiveStates(plr);
                }
        }


        public override bool OnPlayerKilled(Object pkilled, object instigator)
        {
        
            if (pkilled == _carrier)
            {
                ((Player)pkilled).OSInterface.RemoveEffect(0xB);
                _carrier.CanMount = true;
                _carrier = null;
                CreateBomb();
            }

            return base.OnPlayerKilled(pkilled, instigator);
        }

        public void CarrierBombTimer()
        {
            if (_carrier != null)
            {
                Explosion(_carrier.WorldPosition);
            }
        }

        protected override void UpdateScenario()
        {
            try
            {
                for (int i = 0; i < 2; ++i)
                    foreach (Player plr in Players[i])
                    {
                        if (_carrier != plr && plr.OSInterface.HasEffect((byte)0xB))
                        {
                            plr.OSInterface.RemoveEffect((byte)0xB);
                            plr.CanMount = true;
                        }
                    }

                if (_carrier != null && !_carrier.IsInWorld())
                {
                    CreateBomb();
                }

                if (_carrier != null)
                {

                    if (_carrier.StealthLevel > 0 || _carrier.IsMounted) //if player mounts or stealths with bomb, explode it
                    {
                        _carrier.Uncloak();
                        Explosion(_carrier.WorldPosition, _carrier);

                    }
                  
                }

            }
            catch (Exception e)
            {
                Broadcast(e.Message);
            }

            base.UpdateScenario();
        }
    }
}
