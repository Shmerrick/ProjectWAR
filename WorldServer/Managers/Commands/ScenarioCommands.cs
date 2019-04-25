using FrameWork;
using System.Collections.Generic;
using WorldServer.World.Objects;
using static WorldServer.Managers.Commands.GMUtils;

namespace WorldServer.Managers.Commands
{
    /// <summary>Scenario commands under .scenario</summary>
    internal class ScenarioCommands
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ScenarioStatus(Player plr, ref List<string> values)
        {
            WorldMgr.ScenarioMgr.ScenariosInfo(plr);

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool CheckBalance(Player plr, ref List<string> values)
        {
            if (plr.ScnInterface.Scenario != null)
            {
                plr.SendClientMessage("Balance Vector Order: " + plr.ScnInterface.Scenario.BalanceVectors[0]);
                plr.SendClientMessage("Balance Vector Destro: " + plr.ScnInterface.Scenario.BalanceVectors[1]);

                if (!plr.ScnInterface.Scenario.BalanceVectors[0].IsNullVector())
                {
                    float bias = plr.ScnInterface.Scenario.BalanceVectors[0].CosineOfAngleWithUp();

                    plr.SendClientMessage("Order Bias: " + bias);

                    if (bias >= 0.866)
                        plr.SendClientMessage("Order Priority: No Healers");
                    else if (bias <= -0.866)
                        plr.SendClientMessage("Order Priority: Healers");
                    else if (bias >= 0)
                        plr.SendClientMessage(plr.ScnInterface.Scenario.BalanceVectors[0].X < 0 ? "Order Priority: DPS" : "Order Priority: Tanks");
                    else
                        plr.SendClientMessage(plr.ScnInterface.Scenario.BalanceVectors[0].X < 0 ? "Order Priority: No Tanks" : "Order Priority: No DPS");
                }

                if (!plr.ScnInterface.Scenario.BalanceVectors[1].IsNullVector())
                {
                    float bias = plr.ScnInterface.Scenario.BalanceVectors[1].CosineOfAngleWithUp();

                    plr.SendClientMessage("Destro Bias: " + bias);

                    if (bias >= 0.866)
                        plr.SendClientMessage("Destro Priority: No Healers");
                    else if (bias <= -0.866)
                        plr.SendClientMessage("Destro Priority: Healers");
                    else if (bias >= 0)
                        plr.SendClientMessage(plr.ScnInterface.Scenario.BalanceVectors[1].X < 0 ? "Destro Priority: DPS" : "Destro Priority: Tanks");
                    else
                        plr.SendClientMessage(plr.ScnInterface.Scenario.BalanceVectors[1].X < 0 ? "Destro Priority: No Tanks" : "Destro Priority: No DPS");
                }
            }

            else
            {
                Vector2 balanceVect = new Vector2(GetInt(ref values), GetInt(ref values));

                plr.SendClientMessage("Supplied Vector: " + balanceVect);

                float bias = balanceVect.CosineOfAngleWithUp();

                plr.SendClientMessage("Supplied Bias: " + bias);

                if (bias >= 0.866)
                    plr.SendClientMessage("Supplied Priority: No Healers");
                else if (bias <= -0.866)
                    plr.SendClientMessage("Supplied Priority: Healers");
                else if (bias >= 0)
                    plr.SendClientMessage(balanceVect.X < 0 ? "Supplied Priority: DPS" : "Supplied Priority: Tanks");
                else
                    plr.SendClientMessage(balanceVect.X < 0 ? "Supplied Priority: No Tanks" : "Supplied Priority: No DPS");

            }

            return true;

        }

        /// <summary>
        /// Returns the Scenario score information for yourself
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool GetScenarioScore(Player plr, ref List<string> values)
        {
            if (plr.ScnInterface.Scenario == null)
            {
                plr.SendClientMessage("SCENARIO SCORES: You are not in a scenario.");
                return true;
            }

            plr.ScnInterface.Scenario.GetScenarioScore(plr);

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool CheckDomination(Player plr, ref List<string> values)
        {
            if (plr.ScnInterface.Scenario == null)
            {
                plr.SendClientMessage("SCENARIO DOMINATION: You are not in a scenario.");
                return true;
            }

            plr.ScnInterface.Scenario.CheckDominationStatus(plr);

            return true;
        }

        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ScenarioRotate(Player plr, ref List<string> values)
        {
            WorldMgr.ScenarioMgr.RotateScenarios();

            return true;
        }
        */
    }
}
