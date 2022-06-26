using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldServer.World.Objects;

namespace WorldServer.NetWork.Packets
{
    ///TODO: check for 0x20, 0x40, 0x80 states
    public enum QuestStateOpcode : byte
    {
        None = 0x00,
        QuestAvailable = 0x01,
        QuestsTaken = 0x02,
        QuestCompleted = 0x04,
        DailyAvailable = 0x08,
        DailyCompleted = 0x10,
    }

    ///TODO : check for another states
    public enum CreatureStateOpcode : byte
    {
        Interactable = 0x01,
        Unknown = 0x03,
        Invulnerable = 0x07,
        Disabled = 0x0F,
    }

    public partial class Packets
    {
        /// TODO : rework network code with SpanWriters and add Agressive Inlining here
        public static PacketOut UpdateCreatureState(ushort oid, CreatureStateOpcode state)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE, 10);
            Out.WriteUInt16(oid);
            Out.WriteByte((byte)StateOpcode.CreatureState);
            Out.WriteByte((byte)state);
            Out.Fill(0, 6); /// TODO : check and remove if needed
            return Out;
        }

        public static PacketOut UpdateQuestState(ushort oid, QuestStateOpcode state)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE, 6);
            Out.WriteUInt16(oid);
            Out.WriteByte((byte)StateOpcode.QuestStatus);
            Out.WriteByte((byte)state);
            Out.WriteByte(0); /// TODO : remove if needed
            Out.WriteByte(0);
            return Out;
        }

        public static PacketOut SetCastCompleted(ushort oid, ushort abilityId)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE, 10);
            Out.WriteUInt16(oid);
            Out.WriteByte((byte)StateOpcode.CastCompletion);
            Out.WriteUInt16(0);
            Out.WriteByte(0);
            Out.WriteUInt16(abilityId);
            Out.WriteByte(0); /// TODO : remove these if needed
            Out.WriteByte(0);
            return Out;
        }

        public static PacketOut UpdateZone(ushort zoneid, byte val1, byte val2)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE, 10);
            Out.WriteUInt16(zoneid);
            Out.WriteByte((byte)StateOpcode.ZoneEntry);
            Out.WriteByte(val1);
            Out.WriteByte(val2);
            Out.WriteByte(0); /// TODO : check and remove empty bytes if needed
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.Fill(0, 2);
            return Out;
        }

        public static PacketOut UPDATE_STATE(ushort oid, byte stateId, byte val1, byte val2 = 0, byte val3 = 0)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE, 10);
            Out.WriteUInt16(oid);
            Out.WriteByte(stateId);
            Out.WriteByte(val1);
            Out.WriteByte(val2);
            Out.WriteByte(val3);
            Out.Fill(0, 5); /// TODO : remove if needed
            return Out;
        }
    }
}