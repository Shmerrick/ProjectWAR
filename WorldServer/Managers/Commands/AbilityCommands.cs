using FrameWork;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using static WorldServer.Managers.Commands.GMUtils;
using GameData;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.Managers.Commands
{
    /// <summary>Ability commands under .ability</summary>
    internal class AbilityCommands
    {
        /// <summary>
        /// Increases a given stat by a given value.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool AddStatBonus(Player plr, ref List<string> values)
        {
            Stats statType = (Stats)GetInt(ref values);
            int statValue = GetInt(ref values);

            plr.StsInterface.AddBonusStat(statType, (ushort)statValue, BuffClass.Tactic);

            plr.SendClientMessage("Added " + statValue + " to " + Enum.GetName(typeof(Stats), statType));
            return true;

        }

        /// <summary>
        /// Sends a fake buff start packet (int buffId)
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool SendBuffAppearance(Player plr, ref List<string> values)
        {
            // Actual buff
            PacketOut Out = new PacketOut((byte)Opcodes.F_INIT_EFFECTS);
            Out.WriteByte(1);
            Out.WriteByte(1);
            Out.WriteUInt16(0xF4FF);
            Out.WriteUInt16(plr.Oid);
            Out.WriteByte(0);//(byte)GetInt(ref Values));
            Out.WriteByte(0);
            Out.WriteUInt16R((ushort)GetInt(ref values));
            Out.WriteZigZag((ushort)GetInt(ref values));
            Out.WriteUInt16R(plr.Oid);
            Out.WriteByte(10);
            for (byte i = 0; i < 10; i++)
            {
                Out.WriteByte(i);
                Out.WriteZigZag((i + 1) * 10);
            }
            Out.WriteByte(0);
            plr.SendPacket(Out);

            return true;

        }

        /// <summary>
        /// Sends a cast player effect packet.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool SendCastPlayerEffect(Player plr, ref List<string> values)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT);
            Out.WriteUInt16(plr.Oid);
            Out.WriteUInt16(GetTargetOrMe(plr).Oid);
            Out.WriteUInt16((ushort)GetInt(ref values)); // 00 00 07 D D
            Out.WriteByte((byte)GetInt(ref values));
            Out.WriteByte(0);
            Out.WriteByte(7);   //7
            Out.WriteZigZag(-100);
            Out.WriteZigZag(100);
            Out.WriteByte(0);
            plr.SendPacket(Out);

            return true;
        }

        /// <summary>
        /// Sends a buff effect start packet.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool SendCastPlayerStart(Player plr, ref List<string> values)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT);
            Out.WriteUInt16(plr.Oid);
            Out.WriteUInt16(plr.Oid);
            Out.WriteUInt16((ushort)GetInt(ref values)); // 00 00 07 D D
            Out.WriteByte((byte)GetInt(ref values));
            Out.WriteByte(0);
            Out.WriteByte(1);   //7
            Out.WriteZigZag(100);
            Out.WriteZigZag(100);
            Out.WriteByte(0);
            plr.SendPacket(Out);

            return true;
        }

        /// <summary>
        /// Send a buff effect end packet.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool SendCastPlayerEnd(Player plr, ref List<string> values)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT);
            Out.WriteUInt16(plr.Oid);
            Out.WriteUInt16(plr.Oid);
            Out.WriteUInt16((ushort)GetInt(ref values)); // 00 00 07 D D
            Out.WriteByte((byte)GetInt(ref values));
            Out.WriteByte(0);
            Out.WriteByte(0);   //7
            Out.WriteZigZag(100);
            Out.WriteZigZag(100);
            Out.WriteByte(0);
            plr.SendPacket(Out);

            return true;
        }

        /// <summary>
        /// Send an ability to the client.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool SendTestAbility(Player plr, ref List<string> values)
        {
            ushort abilityId = (ushort)GetInt(ref values);

            if (abilityId > 0)
                plr.AbtInterface.SendTest(abilityId);

            return true;
        }

        /// <summary>
        /// Enables experimental mode on the current target if the current class supports it.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ExperimentalMode(Player plr, ref List<string> values)
        {
         /*   if (plr.CbtInterface.IsInCombat)
            {
                plr.SendClientMessage("This command cannot be invoked if you are in combat.");
                return true;
            }

            plr.CrrInterface.SetExperimentalMode(true);

            return true;*/
            plr.SendClientMessage("This command is no longer available");
            return true;
        }


        /// <summary>
        /// Displays a list of changes made to the career.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool CareerChangeList(Player plr, ref List<string> values)
        {
            plr.CrrInterface.DisplayChangeList();

            return true;
        }

        /// <summary>
        /// If possible, casts the ability of the specified ID.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool InvokeAbility(Player plr, ref List<string> values)
        {
            plr.AbtInterface.StartCast(plr, (ushort)GetInt(ref values), 0);

            return true;
        }

        /// <summary>
        /// Invokes the buff of the specified ID.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool InvokeBuff(Player plr, ref List<string> values)
        {
            Unit target = GetTargetOrMe(plr);

            ushort abilityID = (ushort)GetInt(ref values);

            switch (GetInt(ref values))
            {
                case 1:
                    BuffInfo b = AbilityMgr.GetBuffInfo(abilityID, plr, target);
                    //b.Duration = 3600;
                    target.BuffInterface.QueueBuff(new BuffQueueInfo(plr, plr.Level, b));
                    return true;

                case 0:
                    target.BuffInterface.RemoveBuffByEntry(abilityID); return true;
                default: return true;
            }
        }

        /// <summary>
        /// Sends zero stats to the client for debug purposes.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool SendZeroStats(Player plr, ref List<string> values)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAYER_STATS);
            plr.ItmInterface.BuildStats(ref Out);
            Out.WriteByte(0); //bolster
            Out.WriteByte(10); //effective level
            for (byte i = 0; i < (int)Stats.BaseStatCount; ++i)
            {
                Out.WriteByte(i);
                Out.WriteUInt16(0);
            }
            plr.SendPacket(Out);

            return true;
        }

		/// <summary>
		/// Gets the complete ability list of target (creature).
		/// </summary>
		/// <param name="plr">Player that initiated the command</param>
		/// <param name="values">NPCAbility list of target</param>
		/// <returns></returns>
		public static bool GetAbilityList(Player plr, ref List<string> values)
		{
			Creature obj = GetObjectTarget(plr) as Creature;
			if (obj == null)
				return false;

			plr.SendClientMessage("All loaded abilities of target <Entry, Name>:");

			foreach (NPCAbility ab in obj.AbtInterface.NPCAbilities)
			{
				plr.SendClientMessage("<" + ab.Entry + ", " + AbilityMgr.GetAbilityInfo(ab.Entry).Name + ">");
			}

			return true;
		}
	}
}
