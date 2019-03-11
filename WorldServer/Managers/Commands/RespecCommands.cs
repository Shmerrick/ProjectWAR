using System.Collections.Generic;
using WorldServer.World.Objects;

namespace WorldServer.Managers.Commands
{
    /// <summary>Commands under .</summary>
    internal class RespecCommands
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool RespecGathering(Player plr, ref List<string> values)
        {
            plr.SendTradeSkill(plr._Value.GatheringSkill, 0);
            plr._Value.GatheringSkill = 0;
            plr._Value.GatheringSkillLevel = 0;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool RespecCrafting(Player plr, ref List<string> values)
        {
            plr.SendTradeSkill(plr._Value.CraftingSkill, 0);
            plr._Value.CraftingSkill = 0;
            plr._Value.CraftingSkillLevel = 0;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool MasteryRespecialize(Player plr, ref List<string> values)
        {
            plr.AbtInterface.RespecializeMastery(true);

            return true;
        }

    }
}
