using Common;
using FrameWork;
using System;
using System.Collections.Generic;
using SystemData;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using static WorldServer.Managers.Commands.GMUtils;
using Object = WorldServer.World.Objects.Object;

namespace WorldServer.Managers.Commands
{
    public class TestCommands
    {
        //Testing update quest state
        [CommandAttribute(EGmLevel.DatabaseDev, "Check UPDATE_STATE work")]
        public static bool SetStateLong(Player plr, ref List<string> values)
        {
            byte state = Convert.ToByte(values[0]);
            Object obj = GetObjectTarget(plr);
            if (!obj.IsCreature())
            {
                plr.SendClientMessage($"GAMEOBJECT REMOVE: Target is not a creature", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                return true;
            }
            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE, 11);
            Out.WriteUInt16(obj.Oid);
            Out.WriteByte((byte)StateOpcode.QuestStatus);
            Out.WriteByte(state);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.Fill(0, 5);
            plr.SendPacket(Out);
            return true;
        }

        [CommandAttribute(EGmLevel.DatabaseDev, "Check UPDATE_STATE work")]
        public static bool SetStateShort(Player plr, ref List<string> values)
        {
            byte state = Convert.ToByte(values[0]);

            Object obj = GetObjectTarget(plr);
            if (!obj.IsCreature())
            {
                plr.SendClientMessage($"GAMEOBJECT REMOVE: Target is not a creature", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                return true;
            }
            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE, 6);
            Out.WriteUInt16(obj.Oid);
            Out.WriteByte((byte)StateOpcode.QuestStatus);
            Out.WriteByte(state);
            Out.WriteByte(0);
            Out.WriteByte(0);
            plr.SendPacket(Out);
            return true;
        }
    }
}