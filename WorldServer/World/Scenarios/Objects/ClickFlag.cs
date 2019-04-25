using System.Collections.Generic;
using System.Linq;
using SystemData;
using Common;
using FrameWork;
using WorldServer.NetWork.Handler;
using WorldServer.Services.World;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.Buffs.SpecialBuffs;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Objects;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Scenarios.Objects
{
    public class ClickFlag : Object
    {
        public int ObjectiveID;
        public string ObjectiveName;
        private int _x, _y, _z;
        public int Owner;
        public int HoldOwner;
        private bool _Open = true;
        private GameObject _glowObject = null;
        public bool _showGlow = false;
        public ushort CaptureCastTime = 10;
        public bool ShowGlow
        {
            get
            {
                return _showGlow;
            }
            set

            {
                _showGlow = value;
                if (value && _glowObject == null)
                {
                    CreateGlow();
                }
                else if (!value && _glowObject != null)
                {
                    _glowObject.RemoveFromWorld();
                    _glowObject = null;
                }
            }
        }

        public int GlowOwner
        {
            get
            {
                return _glowObject.VfxState;
            }
            set
            {
                if (_glowObject == null)
                {
                    CreateGlow();
                }
                _glowObject.VfxState = (byte)value;
            }
        }

        public byte CapturePoints { get; private set; }
        public byte TickPoints { get; private set; }
        public int HoldDuration = 59000;
        public bool Open
        {
            get
            {
                return _Open;
            }

            set
            {
                _Open = value;
                UpdateInteractState((byte)(_Open ? 1 : 15));
            }
        }

        private List<Player> _capturing = new List<Player>();
        private long HoldStartTime = 0;

        public delegate void ClickFlagDelegate(ClickFlag flag);
        private ClickFlagDelegate OnHold;
        private ClickFlagDelegate OnCaptured;

        public ClickFlag(int objectiveID, string objectiveName, int x, int y, int z, int o, byte capturePoints, byte tickPoints, ClickFlagDelegate onHold, ClickFlagDelegate onCaptured)
        {
            ObjectiveID = objectiveID;
            ObjectiveName = objectiveName;
            _x = x;
            _y = y;
            _z = z;
            Heading = (ushort)o;

            CapturePoints = capturePoints;
            TickPoints = tickPoints;
            OnHold = onHold;
            OnCaptured = onCaptured;
        }

        public void CreateGlow()
        {
            if (Region != null)
            {
                if (_glowObject != null)
                {
                    _glowObject.RemoveFromWorld();
                }
                // Spawn objective glow (Big Poofy Glowy Nuet)
                GameObject_proto glowProto = GameObjectService.GetGameObjectProto(99858);

                if (glowProto != null)
                {
                    GameObject_spawn spawn = new GameObject_spawn
                    {
                        Guid = (uint)GameObjectService.GenerateGameObjectSpawnGUID(),
                        WorldO = Heading,
                        WorldX = _x,
                        WorldY = _y,
                        WorldZ = _z,
                        ZoneId = Region.RegionId,
                    };
                    spawn.BuildFromProto(glowProto);

                    _glowObject = new GameObject(spawn);
                    _glowObject.VfxState = (byte)GlowOwner;

                    Region.AddObject(_glowObject, spawn.ZoneId);
                }
            }
        }

        public override void OnLoad()
        {
            X = Zone.CalculPin((uint)(_x), true);
            Y = Zone.CalculPin((uint)(_y), false);
            Z = _z;

            base.OnLoad();

            WorldPosition.X = _x;
            WorldPosition.Y = _y;
            WorldPosition.Z = Z;

            SetOffset((ushort)(_x >> 12), (ushort)(_y >> 12));

            IsActive = true;
        }

        public override void SendMeTo(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CREATE_STATIC);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(0);

            Out.WriteUInt16(Heading);
            Out.WriteUInt16((ushort)_z);
            Out.WriteUInt32((uint)_x);
            Out.WriteUInt32((uint)_y);

            int displayId = 3442;
            if (Owner == 0)
                displayId = 3442;
            else if (Owner == 1)
                displayId = 3443;
            else
                displayId = 3438;

            Out.WriteUInt16((ushort)displayId);

            Out.WriteUInt16(0x1E00);
            Out.WriteUInt16(0);
            Out.WriteUInt16(0);
            Out.WriteByte(0);

            // flags
            if (Open)
            {
                Out.WriteUInt16(0x24);
            }
            else
            {
                Out.WriteUInt16(0x21);
            }

            Out.WriteByte(0);

            Out.WriteUInt16(0);
            Out.WriteByte(0);
            Out.WriteByte(100);

            Out.WriteUInt16(0x91C0);
            Out.WriteUInt16(0xCB42);
            Out.WriteUInt32(0);

            Out.WritePascalString(ObjectiveName);
            Out.WriteByte(0);

            plr.SendPacket(Out);

            base.SendMeTo(plr);
        }

        public void Capture()
        {
            HoldStartTime = 0;
            EvtInterface.RemoveEvent(Capture); //remove anyone capturing
            Owner = HoldOwner;
            List<Player> capturing = new List<Player>();
            lock (_capturing)
            {
                capturing = _capturing.ToList();
            }

            //stop everyone from trying to capture
            foreach (var c in capturing)
            {
                var captureBuff = c.BuffInterface.GetBuff(60000, c);
                if (captureBuff != null)
                {
                    captureBuff.RemoveBuff(true);
                }
            }

            if (OnCaptured != null)
                OnCaptured(this);
        }

        public override bool AllowInteract(Player player)
        {
            if (!Open || HoldOwner == (int)player.Realm)
                return false;

            if (player.StealthLevel > 0)
            {
                player.SendClientMessage("Cannot capture a flag while in stealth.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return false;
            }

            return base.AllowInteract(player);
        }

        public override void SendInteract(Player player, InteractMenu menu)
        {
            lock (_capturing)
            {
                if(!_capturing.Contains(player))
                    _capturing.Add(player);
            }

            BuffInfo b = AbilityMgr.GetBuffInfo((ushort)GameBuffs.Interaction);

            b.Duration = CaptureCastTime;
            player.BuffInterface.QueueBuff(new BuffQueueInfo(player, player.Level, b, InteractionBuff.GetNew, LinkToCaptureBuff));

            if (player.IsMounted)
                player.Dismount();
        }

        public void SendFlagState(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_STATE, 16);
            Out.WriteUInt32((uint)ObjectiveID);
            Out.Fill(0xFF, 6);
            Out.WriteUInt16(0);
            Out.WriteByte((byte)Owner);
            Out.Fill(0, 3); // 2nd byte can be meaningful!
            plr.SendPacket(Out);
        }


        public void SendFlagInfo(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_INFO);
            Out.WriteUInt32((uint)ObjectiveID);
            Out.WriteByte(0);
            Out.WriteByte((byte)Owner);
            Out.WriteByte(1);
            Out.WriteUInt16(0);
            Out.WritePascalString(ObjectiveName);
            Out.WriteByte(2);
            Out.WriteUInt32(0x0000348F);
            Out.WriteUInt32(0x0000FF00);

            if ((int)plr.Realm == HoldOwner)
                Out.WritePascalString("Maintain control of " + ObjectiveName);
            else
                Out.WritePascalString("Interact with the flag to capture " + ObjectiveName + "!");

            Out.WriteByte(0);

            if ((int)plr.Realm == HoldOwner)
                Out.WritePascalString("Your realm controls " + ObjectiveName + ".");
            else
                Out.WritePascalString("Fight for control of " + ObjectiveName + "!");

            if (HoldStartTime > 0)
            {
                Out.WriteUInt32(60); //total
                var remaining = 60 - ((TCPManager.GetTimeStampMS() - HoldStartTime) / 1000);
                Out.WriteUInt32((byte)remaining);
            }
            else
            {
                Out.WriteUInt32(0);
                Out.WriteUInt32(0);
            }

            Out.Fill(0, 4);
            Out.WriteUInt16(0x3101);
            Out.Fill(0, 3);
            plr.SendPacket(Out);
        }

        public void SendFlagLeft(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_UPDATE);

            Out.WriteUInt32((uint)ObjectiveID);
            Out.WriteUInt32(0);
            plr.SendPacket(Out);
        }

        public override void NotifyInteractionComplete(NewBuff b)
        {
           if (Open)
            {
                if (HoldOwner != (int)b.Caster.Realm)
                {
                    EvtInterface.RemoveEvent(Capture);
                }
                else
                    return; //no need to capture twice

                HoldOwner = (int)b.Caster.Realm;
                Owner = 0;
                List<Player> capturing = new List<Player>();
                lock (_capturing)
                {
                    if (b.Caster is Player && _capturing.Contains((Player)b.Caster))
                        _capturing.Remove((Player)b.Caster);

                    capturing = _capturing.ToList();
                }

                //abort anyone else trying to capture
                foreach (var c in capturing)
                {
                    var captureBuff = c.BuffInterface.GetBuff(60000, c);
                    if (captureBuff != null)
                    {
                        captureBuff.RemoveBuff(true);
                    }
                }

                HoldStartTime = TCPManager.GetTimeStampMS();

                if (OnHold != null)
                    OnHold(this);

                if (HoldDuration == 0)
                    Capture();
                else
                    EvtInterface.AddEvent(Capture, HoldDuration, 1);

            }

        }

        public override void NotifyInteractionBroken(NewBuff b)
        {
            lock (_capturing)
            {
                if (b.Caster is Player && _capturing.Contains((Player)b.Caster))
                    _capturing.Remove((Player)b.Caster);
            }
        }
    }
}
