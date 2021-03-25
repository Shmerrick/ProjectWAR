﻿using FrameWork;
using System.Collections.Generic;
using WorldServer.World.Objects;
using static WorldServer.Managers.Commands.GMUtils;

namespace WorldServer.Managers.Commands
{
    /// <summary>State modification commands under .state</summary>
    internal class StatesCommand
    {
        /// <summary>
        /// Add State To Target <Id>
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool StatesAdd(Player plr, ref List<string> values)
        {
            int id = GetInt(ref values);

            Creature crea = GetObjectTarget(plr) as Creature;
            if (crea == null)
                return false;

            crea.States.Add((byte)id);
            crea.SendMeTo(plr);
            return true;
        }

        /// <summary>
        /// Remove state from target <Id>
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool StatesRemove(Player plr, ref List<string> values)
        {
            int id = GetInt(ref values);

            Creature crea = GetObjectTarget(plr) as Creature;
            if (crea == null)
                return false;

            crea.States.Remove((byte)id);
            crea.SendMeTo(plr);
            return true;
        }

        /// <summary>
        /// Show target States List
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool StatesList(Player plr, ref List<string> values)
        {
            Creature crea = GetObjectTarget(plr) as Creature;
            if (crea == null)
                return false;

            plr.SendClientMessage($"[{crea.Name}] - Proto states: {crea.Spawn.Proto.States.Length} Unit states: {crea.States.Count}");
            foreach (byte b in crea.Spawn.Proto.States)
                plr.SendClientMessage("Proto state: " + b);
            foreach (byte b in crea.States)
                plr.SendClientMessage("Unit state: " + b);
            return true;
        }

        /// <summary>
        /// Send F_STATE_UPDATE packet to target
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>

        public static bool StatesUpdate(Player plr, ref List<string> values)
        {
            Object obj = GetObjectTarget(plr);
            if (!obj.IsCreature())
                return false;

            Creature pCreature = (Creature)obj;

            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE);
            Out.WriteUInt16(pCreature.Oid);
            Out.WriteByte((byte)System.Convert.ToInt32(values[0]));
            Out.WriteByte((byte)System.Convert.ToInt32(values[1]));
            Out.WriteByte((byte)System.Convert.ToInt32(values[2]));
            Out.Fill(0, 4);
            plr.SendPacket(Out);

            return true;
        }
    }
}