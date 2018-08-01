using Common;
using System;
using System.Collections.Generic;
using WorldServer.Services.World;
using static WorldServer.Managers.Commands.GMUtils;

namespace WorldServer.Managers.Commands
{
    /// <summary>Addition commands under .add</summary>
    internal class AddCommands
    {

        /// <summary>
        /// Add xp to player
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool AddXp(Player plr, ref List<string> values)
        {
            int xp = GetInt(ref values);
            plr = GetTargetOrMe(plr) as Player;
            plr.AddXp((uint)xp, false, false);

            GMCommandLog log = new GMCommandLog();
            log.PlayerName = plr.Name;
            log.AccountId = (uint)plr.Client._Account.AccountId;
            log.Command = "ADD XP TO " + plr.Name + " " + xp;
            log.Date = DateTime.Now;
            CharMgr.Database.AddObject(log);

            return true;
        }

        /// <summary>
        /// Add item to player
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool AddItem(Player plr, ref List<string> values)
        {
            int itemId = GetInt(ref values);
            int count = 1;
            if (values.Count > 0)
                count = GetInt(ref values);

            Player targetPlr = GetTargetOrMe(plr) as Player;
            if (targetPlr.ItmInterface.CreateItem((uint)itemId, (ushort)count) == ItemResult.RESULT_OK)
            {
                GMCommandLog log = new GMCommandLog();
                log.PlayerName = plr.Name;
                log.AccountId = (uint)plr.Client._Account.AccountId;
                log.Command = "ADDED " + count + " OF " + ItemService.GetItem_Info((uint)itemId).Name + " TO " + targetPlr.Name;
                log.Date = DateTime.Now;
                CharMgr.Database.AddObject(log);
            }

            else
                plr.SendClientMessage($"Item creation failed: {itemId}");

            return true;
        }

        /// <summary>
        /// Add money to player
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool AddMoney(Player plr, ref List<string> values)
        {
            int money = GetInt(ref values);
            plr = GetTargetOrMe(plr) as Player;
            plr.AddMoney((uint)money);

            GMCommandLog log = new GMCommandLog();
            log.PlayerName = plr.Name;
            log.AccountId = (uint)plr.Client._Account.AccountId;
            log.Command = "ADDED MONEY TO " + plr.Name + " " + money;
            log.Date = DateTime.Now;
            CharMgr.Database.AddObject(log);

            return true;
        }

        /// <summary>
        /// Add tok to player
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool AddTok(Player plr, ref List<string> values)
        {
            int tokEntry = GetInt(ref values);

            Tok_Info info = TokService.GetTok((ushort)tokEntry);
            if (info == null)
                return false;

            plr = GetTargetOrMe(plr) as Player;
            plr.TokInterface.AddTok(info.Entry);

            GMCommandLog log = new GMCommandLog
            {
                PlayerName = plr.Name,
                AccountId = (uint)plr.Client._Account.AccountId,
                Command = "ADD TOK TO " + plr.Name + " " + tokEntry,
                Date = DateTime.Now
            };
            CharMgr.Database.AddObject(log);

            return false;
        }

        /// <summary>
        /// Add renown to player
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool AddRenown(Player plr, ref List<string> values)
        {
            int value = GetInt(ref values);
            plr = GetTargetOrMe(plr) as Player;
            plr.AddRenown((uint)value, false);

            GMCommandLog log = new GMCommandLog();
            log.PlayerName = plr.Name;
            log.AccountId = (uint)plr.Client._Account.AccountId;
            log.Command = "ADD RENOWN TO " + plr.Name + " " + value;
            log.Date = DateTime.Now;
            CharMgr.Database.AddObject(log);

            return true;
        }

        /// <summary>
        /// Add Influence to player
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool AddInf(Player plr, ref List<string> values)
        {
            int chapter = GetInt(ref values);
            int inf = GetInt(ref values);

            plr = GetTargetOrMe(plr) as Player;
            plr.AddInfluence((byte)chapter, (ushort)inf);

            GMCommandLog log = new GMCommandLog();
            log.PlayerName = plr.Name;
            log.AccountId = (uint)plr.Client._Account.AccountId;
            log.Command = "ADD Infl TO " + plr.Name + " Chapter " + chapter + " Value " + inf;
            log.Date = DateTime.Now;
            CharMgr.Database.AddObject(log);

            return true;
        }

        /// <summary>
        /// Add Contribution to player FOR TESTING ONLY
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool AddContrib(Player plr, ref List<string> values)
        {
            int value = GetInt(ref values);

            plr = GetTargetOrMe(plr) as Player;
            plr.Region.Campaign.AddContribution(plr, (uint)value);

            GMCommandLog log = new GMCommandLog();
            log.PlayerName = plr.Name;
            log.AccountId = (uint)plr.Client._Account.AccountId;
            log.Command = "ADD Infl TO " + plr.Name + " contribution Value " + value;
            log.Date = DateTime.Now;
            CharMgr.Database.AddObject(log);

            plr.SendClientMessage(value + " contribution added for " + plr.Name);
            return true;
        }
    }
}
