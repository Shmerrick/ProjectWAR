using System;
using Common;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using static WorldServer.Managers.Commands.GMUtils;

namespace WorldServer.Managers.Commands
{
    /// <summary>Search commands under .search</summary>
    internal class SearchCommands
    {

        /// <summary>
        /// Search an item by name <name>
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool SearchItem(Player plr, ref List<string> values)
        {
            string str = GetTotalString(ref values);
            str = str.Replace(" ", "%");

            List<Item_Info> l = WorldMgr.Database.SelectObjects<Item_Info>("Name Like '%" + WorldMgr.Database.Escape(str) + "%' LIMIT 0,30") as List<Item_Info>;
            plr.SendMessage(0, "", "Items : " + (l != null ? l.Count : 0), ChatLogFilters.CHATLOGFILTERS_SHOUT);
            if (l != null)
            {
                foreach (Item_Info proto in l)
                    plr.SendMessage(0, "", "ID:" + proto.Entry + ",Name:" + proto.Name + ",Type:" + proto.Type + ",Race:" + proto.Race + ",Career:" + proto.Career + ",Armor:" + proto.Armor + ",Speed:" + proto.Speed + ",Script:" + proto.ScriptName, ChatLogFilters.CHATLOGFILTERS_EMOTE);
            }

            return true;
        }

        /// <summary>
        /// Seach an npc by name <name>
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool SearchNpc(Player plr, ref List<string> values)
        {
            string str = GetTotalString(ref values);
            str = str.Replace(" ", "%");

            List<Creature_proto> l = WorldMgr.Database.SelectObjects<Creature_proto>("Name Like '%" + WorldMgr.Database.Escape(str) + "%' LIMIT 0,30") as List<Creature_proto>;
            plr.SendMessage(0, "", "Creatures : " + (l?.Count ?? 0), ChatLogFilters.CHATLOGFILTERS_SHOUT);
            if (l != null)
            {
                foreach (Creature_proto proto in l)
                    plr.SendMessage(0, "", "ID:" + proto.Entry + ",Name:" + proto.Name + ",Faction:" + proto.Faction + ",Flag:" + proto.Flag + ",Icon:" + proto.Icone + ",Model:" + proto.Model1 + ",Script:" + proto.ScriptName, ChatLogFilters.CHATLOGFILTERS_EMOTE);
            }

            return true;
        }

        /// <summary>
        /// Seach an gameobject by name <name>
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool SearchGameObject(Player plr, ref List<string> values)
        {
            string str = GetTotalString(ref values);
            str = str.Replace(" ", "%");

            List<GameObject_proto> l = WorldMgr.Database.SelectObjects<GameObject_proto>("Name Like '%" + WorldMgr.Database.Escape(str) + "%' LIMIT 0,30") as List<GameObject_proto>;
            plr.SendMessage(0, "", "GameObjects : " + (l != null ? l.Count : 0), ChatLogFilters.CHATLOGFILTERS_SHOUT);
            if (l != null)
            {
                foreach (GameObject_proto proto in l)
                    plr.SendMessage(0, "", "ID:" + proto.Entry + ",Name:" + proto.Name + ",Faction:" + proto.Faction + ",Scale:" + proto.Scale + ",Model:" + proto.DisplayID + ",Script:" + proto.ScriptName, ChatLogFilters.CHATLOGFILTERS_EMOTE);
            }

            return true;
        }

        /// <summary>
        /// Seach an quest by name <name>
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool SearchQuest(Player plr, ref List<string> values)
        {
            string str = GetTotalString(ref values);
            str = str.Replace(" ", "%");

            List<Quest> l = WorldMgr.Database.SelectObjects<Quest>("Name Like '%" + WorldMgr.Database.Escape(str) + "%' LIMIT 0,30") as List<Quest>;
            plr.SendMessage(0, "", "Quests : " + (l != null ? l.Count : 0), ChatLogFilters.CHATLOGFILTERS_SHOUT);
            if (l != null)
            {
                foreach (Quest proto in l)
                    plr.SendMessage(0, "", "ID:" + proto.Entry + ",Name:" + proto.Name + ",Level:" + proto.MinLevel + ",Objectives:" + proto.Objectives.Count + ",Prev:" + proto.PrevQuest + ",Xp:" + proto.Xp + ",Rewars:" + proto.Rewards.Count + ",Type:" + proto.Type, ChatLogFilters.CHATLOGFILTERS_EMOTE);
            }

            return true;
        }

        /// <summary>
        /// Seach an zone by name <name>
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool SearchZone(Player plr, ref List<string> values)
        {
            string str = GetTotalString(ref values);
            str = str.Replace(" ", "%");

            List<Zone_Info> l = WorldMgr.Database.SelectObjects<Zone_Info>("Name Like '%" + WorldMgr.Database.Escape(str) + "%' LIMIT 0,30") as List<Zone_Info>;
            plr.SendMessage(0, "", "Zones : " + (l != null ? l.Count : 0), ChatLogFilters.CHATLOGFILTERS_SHOUT);
            if (l != null)
            {
                foreach (Zone_Info proto in l)
                    plr.SendMessage(0, "", "ID:" + proto.ZoneId + ",Name:" + proto.Name + ",X:" + proto.OffX + ",Y:" + proto.OffY + ",Region:" + proto.Region + ",Level:" + proto.MinLevel + ",Price:" + proto.Price + ",Tier:" + proto.Tier, ChatLogFilters.CHATLOGFILTERS_EMOTE);
            }

            return true;
        }
        /// <summary>
        /// Seach a player's inventory by name <name>
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool SearchInventory(Player plr, ref List<string> values)
        {
            var target = plr.CbtInterface.GetCurrentTarget();
            var filter = String.Empty;

            if (target == null)
            {
                plr.SendClientMessage($"SEARCH INVENTORY: No target selected.");
            }
            else
            {
                var character = CharMgr.GetCharacter((target as Player).CharacterId, false);
                if (character == null)
                {
                    plr.SendClientMessage($"SEARCH INVENTORY: The player {(target as Player).Name} in question does not exist.");
                    return true;
                }
                var characterItemList = CharMgr.GetItemsForCharacter(character);
                var itemList = new List<Item_Info>();

                if (values.Count > 0)
                {
                    // First argument is a wildcard filter.
                    filter = values[0];
                }

                foreach (CharacterItem itm in characterItemList)
                {
                    if (itm != null)
                        itemList.Add(ItemService.GetItem_Info(itm.Entry));
                }

                itemList.OrderBy(x => x.Name);

                foreach (Item_Info proto in itemList)
                {
                    if (!String.IsNullOrEmpty(filter))
                    {
                        if (proto.Name.ToLower().Contains(filter.ToLower()))
                        {
                            plr.SendMessage(0, "", $"[{proto.Entry}] {proto.Name} ", ChatLogFilters.CHATLOGFILTERS_EMOTE);
                        }
                    }
                    else
                    {
                        plr.SendMessage(0, "", $"[{proto.Entry}] {proto.Name} ", ChatLogFilters.CHATLOGFILTERS_EMOTE);
                    }
                }
            }



            return true;
        }
    }
}
