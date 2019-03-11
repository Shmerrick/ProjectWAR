using System;
using Common;
using FrameWork;
using WorldServer.Services.World;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Objects.Instances
{
    public class InstanceObject : GameObject
	{
        public readonly Instance_Object Info;
        protected readonly Instance Instance;

        protected byte VfxState = 0;

        public InstanceObject(Instance instance, Instance_Object info)
        {
            Info = info;
            Instance = instance;
            Name = info.Name;
            Zone_Info zone = ZoneService.GetZone_Info(instance.Info.ZoneID);
            X = (ushort)(info.WorldX - (zone.OffX << 12));
            Y = (ushort)(info.WorldY - (zone.OffY << 12));
            Z = (ushort)info.WorldZ;
            WorldPosition.X = (int)info.WorldX;
            WorldPosition.Y = (int)info.WorldY;
            WorldPosition.Z = (int)info.WorldZ;

            XOffset = (ushort)Math.Truncate((decimal)(X / 4096 + zone.OffX));
            YOffset = (ushort)Math.Truncate((decimal)(Y / 4096 + zone.OffY));
          
            IsActive = true;
            VfxState = (byte)info.VfxState;
        }

        public override void SendMeTo(Player plr)
        {
            // Log.Info("STATIC", "Creating static oid=" + Oid + " name=" + Name + " x=" + Spawn.WorldX + " y=" + Spawn.WorldY + " z=" + Spawn.WorldZ + " doorID=" + Spawn.DoorId);
            PacketOut Out = new PacketOut((byte)Opcodes.F_CREATE_STATIC);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16((ushort)(VfxState)); //ie: red glow, open door, lever pushed, etc

            Out.WriteUInt16((ushort)Info.WorldO);
            Out.WriteUInt16((ushort)Info.WorldZ);
            Out.WriteUInt32((uint)Info.WorldX);
            Out.WriteUInt32((uint)Info.WorldY);
            Out.WriteUInt16((ushort)Info.DisplayID);

            Out.WriteByte((byte)(0));// Spawn.GetUnk(0) >> 8));

            // Get the database if the value hasnt been changed (currently only used for keep doors)

            Out.WriteByte((byte)0); //realm

            Out.WriteUInt16(0);// Spawn.GetUnk(1));
            Out.WriteUInt16(0);// Spawn.GetUnk(2));
            Out.WriteByte(0);// Spawn.Unk1);

            Out.WriteUInt16((ushort)0);
            Out.WriteByte(0);
            Out.WriteUInt32(0);
            Out.WriteUInt16(0);
            Out.WriteUInt16(0);
            Out.WriteUInt32(0);

            Out.WritePascalString(Info.Name);

            if (Info.DoorID != 0)
            {
                Out.WriteByte(0x04);
                Out.WriteUInt32(Info.DoorID);
            }
            else
            {
                Out.WriteByte(0x00);
                Out.WriteUInt32(0);
            }

            plr.SendPacket(Out);

            base.SendMeTo(plr);
        }

        public void UpdateVfxState(byte state)
        {
            VfxState = state;
            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE, 20);
            Out.WriteUInt16(Oid);
            Out.WriteByte(6); //state
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(8);
            Out.WriteByte(0);
            Out.WriteByte(state);
            Out.Fill(0, 10);
            DispatchPacket(Out, false);
        }
    }
}
