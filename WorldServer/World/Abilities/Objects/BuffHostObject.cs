using Common;
using FrameWork;
using GameData;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using WorldServer.World.Positions;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Abilities.Objects
{
    class BuffHostObject : Unit
    {
        protected Unit Owner;
        protected Creature_proto Proto;
        protected long ExpireTime;

        public BuffHostObject()
        {
            
        }

        public BuffHostObject(Unit owner, Point3D spawnVec, Creature_proto proto, long expireTime)
        {
            Owner = owner;
            Proto = proto;
            Name = proto.Name;
            Realm = owner.Realm;
            WorldPosition.X = spawnVec.X;
            WorldPosition.Y = spawnVec.Y;
            WorldPosition.Z = spawnVec.Z;
            Level = Owner.EffectiveLevel;
            Faction = Realm == Realms.REALMS_REALM_ORDER ? (byte)0x40 : (byte)0x80;

            ExpireTime = expireTime;

            IsInvulnerable = true;

            MaxHealth = 9999;
            Health = 9999;
        }

        public override void OnLoad()
        {
            X = Zone.CalculPin((uint)(WorldPosition.X), true);
            Y = Zone.CalculPin((uint)(WorldPosition.Y), false);
            Z = (ushort)(WorldPosition.Z);

            Heading = 0;

            SetOffset((ushort)(WorldPosition.X >> 12), (ushort)(WorldPosition.Y >> 12));
            base.OnLoad();

            IsActive = true;
            if (ExpireTime != 0)
                EvtInterface.AddEvent(Destroy, (int)(ExpireTime - TCPManager.GetTimeStampMS()), 1);
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
            PacketOut Out = new PacketOut((byte)Opcodes.F_CREATE_MONSTER, 128);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(0);

            Out.WriteUInt16(Heading);
            Out.WriteUInt16((ushort)WorldPosition.Z);
            Out.WriteUInt32((uint)WorldPosition.X);
            Out.WriteUInt32((uint)WorldPosition.Y);
            Out.WriteUInt16(0); // Speed Z
            Out.WriteUInt16(Proto.Model1); // model ID

            Out.WriteByte(0x32); // scale

            Out.WriteByte(Owner.Level);
            Out.WriteByte(Faction);
            Out.Fill(0, 6);
            Out.WriteUInt16(1); // Unk5
            Out.Fill(0, 13);

            // State 19?
            Out.WriteByte(1); // Spawn Length
            Out.WriteByte(0x19); // Spawn Data

            Out.WriteByte(0);

            Out.WriteCString(Name);
            Out.WriteUInt32(3);
            Out.WriteUInt16(0x010A); // Fig leaf data
            Out.WriteByte(0);
            Out.WriteUInt16(Owner.Oid);
            Out.WriteByte(18); // Cheating - length of embedded OBJECT_STATE packet which is for heading only
            Out.WriteByte(5);  // Cheating - length of embedded PLAYER_INVENTORY packet which is empty
            Out.WriteByte(0);

            // F_OBJECT_STATE 00
            Out.WriteUInt16(Oid);
            Out.WriteUInt16((ushort)X);
            Out.WriteUInt16((ushort)Y);
            Out.WriteUInt16((ushort)Z);
            Out.WriteByte(PctHealth);
            Out.WriteByte(0); // flags
            Out.WriteByte((byte)Zone.ZoneId);
            Out.WriteByte(4); // 0
            Out.WriteUInt32(3); // 0
            Out.WriteUInt16R(Heading);

            // F_PLAYER_INVENTORY
            Out.WriteUInt16(Oid);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(0);

            plr.SendPacket(Out);

            Out = new PacketOut((byte) Opcodes.F_UPDATE_STATE);
            Out.WriteUInt16(Oid);
            Out.WriteByte(1);
            Out.WriteByte(7);
            Out.Fill(0, 6);
			plr.SendPacket(Out);

            base.SendMeTo(plr);
        }

        protected override void SetDeath(Unit killer)
        {
            EvtInterface.AddEvent(Destroy, 1500, 0);
        }

        public override void RezUnit()
        {
            Destroy();
        }

        public void SafePinTeleport(ushort pinX, ushort pinY, ushort pinZ, ushort worldO)
        {
            if (pinX == 0 || pinY == 0)
                return;

            Point3D world = ZoneService.GetWorldPosition(Zone.Info, pinX, pinY, pinZ);
            SafeWorldTeleport((uint)world.X, (uint)world.Y, (ushort)world.Z, worldO);
        }

        public void SafeWorldTeleport(uint worldX, uint worldY, ushort worldZ, ushort worldO)
        {
            if (worldX == 0 || worldY == 0)
                return;

            X = Zone.CalculPin(worldX, true);
            Y = Zone.CalculPin(worldY, true);
            SetPosition((ushort)X, (ushort)Y, worldZ, worldO, Zone.ZoneId);
        }
    }
}
