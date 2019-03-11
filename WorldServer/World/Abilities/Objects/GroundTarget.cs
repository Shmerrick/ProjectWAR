using Common;
using FrameWork;
using WorldServer.World.Objects;
using WorldServer.World.Positions;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Abilities.Objects
{
    /// <summary>
    /// An object used as a host for buffs representing ground-targeted attacks.
    /// </summary>
    public class GroundTarget : Unit
    {
        private GameObject_spawn _spawn = new GameObject_spawn();

        public GroundTarget(Unit instigator, Point3D spawnLoc, GameObject_proto proto)
        {
            IsInvulnerable = true;
            WorldPosition.X = spawnLoc.X;
            WorldPosition.Y = spawnLoc.Y;
            WorldPosition.Z = spawnLoc.Z;
            Realm = GameData.Realms.REALMS_REALM_NEUTRAL;
            Level = instigator.EffectiveLevel;
            //player.Realm;
            Faction = Realm == GameData.Realms.REALMS_REALM_ORDER ? (byte)0x41 : (byte)0x81;

            Health = 1;
            MaxHealth = 1;

            IsInvulnerable = true;

            _spawn.BuildFromProto(proto);
        }

        public GroundTarget(Point3D spawnLoc, GameObject_proto proto)
        {
            IsInvulnerable = true;
            WorldPosition.X = spawnLoc.X;
            WorldPosition.Y = spawnLoc.Y;
            WorldPosition.Z = spawnLoc.Z;
            Realm = GameData.Realms.REALMS_REALM_NEUTRAL;
            Faction = Realm == GameData.Realms.REALMS_REALM_ORDER ? (byte)0x41 : (byte)0x81;

            Health = 1;
            MaxHealth = 1;

            IsInvulnerable = true;

            _spawn.BuildFromProto(proto);
        }

        public override void OnLoad()
        {
            X = Zone.CalculPin((uint)(WorldPosition.X), true);
            Y = Zone.CalculPin((uint)(WorldPosition.Y), false);
            Z = (ushort)(WorldPosition.Z);

            SetOffset((ushort)(WorldPosition.X >> 12), (ushort)(WorldPosition.Y >> 12));
            base.OnLoad();

            IsActive = true;
        }

        public void SetExpiry(long expireTime)
        {
            EvtInterface.AddEvent(Destroy, (int)(expireTime - TCPManager.GetTimeStampMS()), 1);
        }

        public override void Update(long msTick)
        {
            if (PendingDisposal)
            {
                Dispose();
                return;
            }

            BuffInterface.Update(msTick);
            EvtInterface.Update(msTick);
        }

        public override void SendMeTo(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CREATE_STATIC, 128);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(0);

            Out.WriteUInt16(Heading);
            Out.WriteUInt16((ushort)WorldPosition.Z);
            Out.WriteUInt32((uint)WorldPosition.X);
            Out.WriteUInt32((uint)WorldPosition.Y);
            Out.WriteUInt16((ushort)_spawn.DisplayID);

            Out.WriteByte((byte)(_spawn.GetUnk(0) >> 8));

            Out.WriteByte((byte)Realm);

            Out.WriteUInt16(_spawn.GetUnk(1));
            Out.WriteUInt16(_spawn.GetUnk(2));
            Out.WriteByte(_spawn.Unk1);

            int flags = _spawn.GetUnk(3);

            Out.WriteUInt16((ushort)flags);
            Out.WriteByte(_spawn.Unk2);
            Out.WriteUInt32(_spawn.Unk3);
            Out.WriteUInt16(_spawn.GetUnk(4));
            Out.WriteUInt16(_spawn.GetUnk(5));
            Out.WriteUInt32(_spawn.Unk4);

            Out.WritePascalString(Name);

            if (_spawn.DoorId != 0)
            {
                Out.WriteByte(0x04);
                Out.WriteUInt32(_spawn.DoorId);
            }
            else
                Out.WriteByte(0x00);

            plr.SendPacket(Out);

            base.SendMeTo(plr);
        }
    }
}
