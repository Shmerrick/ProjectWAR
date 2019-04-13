using Common;
using FrameWork;
using System;
using System.Collections.Generic;
using SystemData;
using System.Linq;
using GameData;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Guild;
using WorldServer.World.Map;
using WorldServer.World.Objects;
using static WorldServer.Managers.Commands.GMUtils;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.Managers.Commands
{
    /// <summary>Unit modification commands under .modify</summary>
    internal class ModifyCommands
    {

        /// <summary>
        /// Changes the speed of the targeted player (int Speed, 0-1000)
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ModifySpeed(Player plr, ref List<string> values)
        {
            int speed = GetInt(ref values);
            if (speed > 1000 && !Utils.HasFlag(plr.GmLevel, (int)EGmLevel.SourceDev))
                speed = 1000;

            plr = GetTargetOrMe(plr) as Player;
            if (plr == null)
                return false;
            plr.Speed = (ushort)speed;
            plr.UpdateSpeed();
            return true;
        }

        /// <summary>
        /// Changes players name
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ModifyPlayerName(Player plr, ref List<string> values)
        {
            if (values.Count < 2)
            {
                plr.SendClientMessage("Usage: .modify playername old_player_name new_player_name");
                return true;
            }

            var charToRename = CharMgr.GetCharacter(Player.AsCharacterName(values[0]), false);
            if (charToRename == null)
            {
                plr.SendClientMessage("Player with name '" + values[0] + "' not found.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return true;
            }

            var existingChar = CharMgr.GetCharacter(Player.AsCharacterName(values[1]), false);
            if (existingChar != null)
            {
                plr.SendClientMessage("Player with name '" + existingChar.Name + "' already exists.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return true;
            }

            if (values[1].Length < 3)
            {
                plr.SendClientMessage("Player name must be at least 3 characters long.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return true;
            }

            if (!values[1].All(c => char.IsLetter(c) && c <= 0x7A))
            {
                plr.SendClientMessage("Player names may not contain special characters.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return true;
            }

            string newName = values[1][0].ToString().ToUpper() + values[1].ToLower().Substring(1);


            CharMgr.UpdateCharacterName(charToRename, newName);

            var player = Player.GetPlayer(values[0]);

            LogSanction(player.Info.AccountId, plr, "GM issued Name Change", "", $"From {charToRename.Name} to {newName}");

            GMCommandLog log = new GMCommandLog
            {
                PlayerName = plr.Name,
                AccountId = (uint)plr.Client._Account.AccountId,
                Command = "Changed player name FROM " + values[0] + " TO " + charToRename.Name,
                Date = DateTime.Now
            };
            CharMgr.Database.AddObject(log);

            if (player != null)
            {
                player.Name = charToRename.Name;
                player.Quit(false, false);
            }

            plr.SendClientMessage(log.Command);
            return true;
        }

        /// <summary>
        /// Temporarily changes players name until server restart.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ModifyPlayerNameTemp(Player plr, ref List<string> values)
        {
            string name = null;

            if (values.Count > 0)
            {
                foreach (var t in values)
                    name += t + " ";
            }
            if (name != null)
                name = name.Trim();

            if (plr.CbtInterface.GetCurrentTarget() is Player)
            {
                var player = plr.CbtInterface.GetCurrentTarget().GetPlayer();
                player.Info.TempFirstName = name;

                var Out = new PacketOut((byte)Opcodes.F_REMOVE_PLAYER); //F_PLAYER_INVENTORY
                Out.WriteUInt16(player.Oid);
                Out.WriteUInt16((ushort)plr.CbtInterface.GetCurrentTarget().Oid);
                Out.Fill(0, 18);
                player.DispatchPacket(Out, true);

                foreach (Player p in player.PlayersInRange)
                {
                    player.SendMeTo(p);
                }
            }
            return true;
        }

        /// <summary>
        /// Changes the leader of the guild (string newLeader, string guildName)
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ModifyGuildLeader(Player plr, ref List<string> values)
        {
            if (values.Count < 2)
            {
                plr.SendClientMessage("Usage: .modify guildleader <newLeaderName> <guildName>");
                return true;
            }

            string playerName = GetString(ref values);

            if (string.IsNullOrEmpty(playerName))
            {
                plr.SendClientMessage("Invalid character name specified.");
                return true;
            }

            string guildName = GetTotalString(ref values);

            Guild guild = Guild.GetGuild(guildName);
            if (guild == null)
            {
                plr.SendClientMessage("Guild " + guildName + " doesn't exist.");
                return true;
            }

            guild.AssignLeader(plr, playerName);

            return true;
        }

        /// <summary>
        /// Changes the name of the guild by ID (int guildID string guildName)
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ModifyGuildNameByID(Player plr, ref List<string> values)
        {
            if (values.Count < 2)
            {
                plr.SendClientMessage("Usage: .modify guildnamebyid <guildID> <guildName>");
                return true;
            }

            uint guildID = (uint)(int)GetInt(ref values);
            string guildName = GetTotalString(ref values).Trim();

            if (string.IsNullOrEmpty(guildName))
            {
                plr.SendClientMessage("you need to specify a new guild name");
                return true;
            }

            Guild guild = Guild.GetGuild(guildID);

            if (guild == null)
            {
                plr.SendClientMessage("The Specified guild does not exist");
                return true;
            }

            if (Guild.GetGuild(guildName) != null)
            {
                plr.SendClientMessage("That guildname is already taken");
                return true;
            }

            else
            {
                plr.SendClientMessage("Changing from " + guild.Info.Name + " to " + guildName);
                if (guild.Info.AllianceId != 0)
                {
                    guild.LeaveAlliance();  //sanity in case of packet changes with guild name change on guild inside alliance
                }

                GMCommandLog log = new GMCommandLog
                {
                    PlayerName = plr.Name,
                    AccountId = (uint)plr.Client._Account.AccountId,
                    Command = "CHANGED GUILDNAME OF: " + guild.Info.Name + " TO: " + guildName,
                    Date = DateTime.UtcNow
                };
                CharMgr.Database.AddObject(log);

                guild.Info.Name = guildName;
                CharMgr.ChangeGuildName(guild.Info, guildName);

                return true;
            }

        }

        /// <summary>
        /// Changes the level of the targeted player (int Rank)
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ModifyLevel(Player plr, ref List<string> values)
        {
            int level = GetInt(ref values);
            plr = GetTargetOrMe(plr) as Player;
            plr.SetLevel((byte)level);

            GMCommandLog log = new GMCommandLog();
            log.PlayerName = plr.Name;
            log.AccountId = (uint)plr.Client._Account.AccountId;
            log.Command = "SET LEVEL TO " + plr.Name + " " + level;
            log.Date = DateTime.Now;
            CharMgr.Database.AddObject(log);

            return true;
        }

        /// <summary>
        /// Changes the renown rank of a player (string playerName, int RenownRank)
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ModifyRenown(Player plr, ref List<string> values)
        {
            string playerName = GetString(ref values);

            int renownLevel = GetInt(ref values);

            Player target = Player.GetPlayer(playerName);
            Character chara = CharMgr.GetCharacter(playerName, false);

            if (chara == null)
            {
                plr.SendClientMessage($"MODIFY RENOWN: The player {playerName} in question does not exist.");
                return true;
            }

            int desiredRenownRank = renownLevel > 0 ? renownLevel : Math.Max(1, chara.Value.RenownRank + renownLevel);
            desiredRenownRank = Math.Min(100, desiredRenownRank);

            if (target != null)
                target.SetRenownLevel((byte)desiredRenownRank);
            else
            {
                chara.Value.Renown = 0;
                chara.Value.RenownRank = (byte)desiredRenownRank;
                CharMgr.Database.SaveObject(chara.Value);
            }

            if (target != plr)
                plr.SendClientMessage($"MODIFY RENOWN: {playerName}'s renown rank is now {chara.Value.RenownRank}.");

            GMCommandLog log = new GMCommandLog
            {
                PlayerName = plr.Name,
                AccountId = (uint)plr.Client._Account.AccountId,
                Command = renownLevel > 0 ? $"SET {playerName}'S RENOWN TO {renownLevel}" : $"REDUCED {playerName}'S RENOWN BY {-renownLevel}",
                Date = DateTime.Now
            };
            CharMgr.Database.AddObject(log);

            return true;
        }

        /// <summary>
        /// Changes the access level of the designated account (string username, int newAccessLevel)
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ModifyAccess(Player plr, ref List<string> values)
        {
            string username = GetString(ref values);

            if (string.IsNullOrEmpty(username))
            {
                plr.SendClientMessage("MODIFY ACCESS: Bad account name.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return true;
            }

            Account acct = Program.AcctMgr.GetAccount(username);

            if (acct == null)
            {
                plr.SendClientMessage("MODIFY ACCESS: The specified account does not exist.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return true;
            }

            sbyte newAccess = (sbyte)GetInt(ref values);
            acct.GmLevel = newAccess;
            Program.AcctMgr.UpdateAccount(acct);

            plr.SendClientMessage($"MODIFY ACCESS: The access level of {username} has been changed to {newAccess}.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);

            return true;
        }

        /// <summary>
        /// Changes the morale of the selected player (int Morale)
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ModifyMorale(Player plr, ref List<string> values)
        {
            int morale = GetInt(ref values);
            plr.SetMorale(morale);
            return true;
        }

        /// <summary>
        /// Changes the current faction of selected Unit (byte Faction)
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ModifyFaction(Player plr, ref List<string> values)
        {
            byte faction = (byte)GetInt(ref values);
            byte save = (byte)(values.Count > 0 ? GetInt(ref values) : 0);

            Object obj = GetObjectTarget(plr);

            RegionMgr region = obj.Region;
            ushort zoneId = obj.Zone.ZoneId;

            obj.RemoveFromWorld();
            obj.GetUnit().SetFaction(faction);
            region.AddObject(obj.GetUnit(), zoneId, true);

            if (save > 0)
            {
                if (obj.IsCreature())
                {
                    Creature crea = obj.GetCreature();
                    crea.Spawn.Faction = faction;
                    WorldMgr.Database.SaveObject(crea.Spawn);
                }
            }

            return true;
        }

        /// <summary>
        /// Change the Influence Chaptter Value
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ModifyInf(Player plr, ref List<string> values)
        {
            int chapter = GetInt(ref values);
            int value = GetInt(ref values);
            plr = GetTargetOrMe(plr) as Player;
            plr.SetInfluence((ushort)chapter, (ushort)value);

            GMCommandLog log = new GMCommandLog();
            log.PlayerName = plr.Name;
            log.AccountId = (uint)plr.Client._Account.AccountId;
            log.Command = "SET Influence TO " + plr.Name + " Chapter " + chapter + " Value " + value;
            log.Date = DateTime.Now;
            CharMgr.Database.AddObject(log);

            return true;
        }

        /// <summary>
        /// Modify your career resource value (byte careerResource)
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ModifyCrrRes(Player plr, ref List<string> values)
        {
            int crrRes = GetInt(ref values);
            plr.CrrInterface.SetResource((byte)crrRes, true);
            return true;
        }

        /// <summary>
        /// Modify a players contribution
        /// </summary>
        /// <param name="plr"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool ModifyContribution(Player plr, ref List<string> values)
        {
            Player target = GetTargetOrMe(plr) as Player; 

            var activeCampaign = WorldMgr.UpperTierCampaignManager.GetActiveCampaign();
            var status = activeCampaign?.ActiveBattleFrontStatus;
            if (status != null)
            {
                lock (status)
                {
                    status.ContributionManagerInstance.UpdateContribution(target.CharacterId,
                        (byte) ContributionDefinitions.DESTROY_INNER_DOOR);
                    status.ContributionManagerInstance.UpdateContribution(target.CharacterId,
                        (byte)ContributionDefinitions.DESTROY_OUTER_DOOR);
                    status.ContributionManagerInstance.UpdateContribution(target.CharacterId,
                        (byte)ContributionDefinitions.BO_TAKE_BIG_TICK);
                    status.ContributionManagerInstance.UpdateContribution(target.CharacterId,
                        (byte)ContributionDefinitions.BO_TAKE_BIG_TICK);
                    status.ContributionManagerInstance.UpdateContribution(target.CharacterId,
                        (byte)ContributionDefinitions.BO_TAKE_BIG_TICK);
                    status.ContributionManagerInstance.UpdateContribution(target.CharacterId,
                        (byte)ContributionDefinitions.BO_TAKE_BIG_TICK);
                    status.ContributionManagerInstance.UpdateContribution(target.CharacterId,
                        (byte)ContributionDefinitions.GROUP_LEADER_BO_BIG_TICK);
                    status.ContributionManagerInstance.UpdateContribution(target.CharacterId,
                        (byte)ContributionDefinitions.KILL_KEEP_LORD);
                    status.ContributionManagerInstance.UpdateContribution(target.CharacterId,
                        (byte)ContributionDefinitions.KEEP_DEFENCE_TICK);
                    status.ContributionManagerInstance.UpdateContribution(target.CharacterId,
                        (byte)ContributionDefinitions.PLAYER_KILL_DEATHBLOW);
                    status.ContributionManagerInstance.UpdateContribution(target.CharacterId,
                        (byte)ContributionDefinitions.PLAYER_KILL_DEATHBLOW);
                    status.ContributionManagerInstance.UpdateContribution(target.CharacterId,
                        (byte)ContributionDefinitions.PLAYER_KILL_DEATHBLOW);
                    status.ContributionManagerInstance.UpdateContribution(target.CharacterId,
                        (byte)ContributionDefinitions.PLAYER_KILL_DEATHBLOW);
                    status.ContributionManagerInstance.UpdateContribution(target.CharacterId,
                        (byte)ContributionDefinitions.PLAYER_KILL_DEATHBLOW);
                    status.ContributionManagerInstance.UpdateContribution(target.CharacterId,
                        (byte)ContributionDefinitions.PLAYER_KILL_DEATHBLOW);

                    return true;
                }
            }

            return true;
        }

        public static bool ModifyHonorRank(Player plr, ref List<string> values)
        {
            Player target = GetTargetOrMe(plr) as Player;

            target.Info.HonorRank = Convert.ToUInt16(values[0]);

            plr.SendClientMessage($"Updated Honor Rank for {target.Name} to {values[0]}");

            return true;
        }


        

        /// <summary>
        /// Changes your proficiency in your current gathering skill (byte Skill)
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ModifyGath(Player plr, ref List<string> values)
        {
            Player target = GetTargetOrMe(plr) as Player;
            if (target == null)
            {
                plr.SendClientMessage("MODIFY GATHERING: Not a player.");
                return true;
            }

            int lvl = GetInt(ref values);
            target._Value.GatheringSkillLevel = (byte)lvl;
            if (target.CultivInterface != null)
                target.CultivInterface.UpdateTradeSkills();
            target.SendTradeSkill(target._Value.GatheringSkill, target._Value.GatheringSkillLevel);
            plr.SendClientMessage("MODIFY GATHERING: Changed " + target.Name + "'s crafting skill to " + lvl + ".");
            return true;
        }

        /// <summary>
        /// Changes your proficiency in your current crafting skill (byte Skill)
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ModifyCraf(Player plr, ref List<string> values)
        {
            Player target = GetTargetOrMe(plr) as Player;
            if (target == null)
            {
                plr.SendClientMessage("MODIFY CRAFTING: Not a player.");
                return true;
            }

            int lvl = GetInt(ref values);
            target._Value.CraftingSkillLevel = (byte)lvl;
            target.SendTradeSkill(target._Value.CraftingSkill, target._Value.CraftingSkillLevel);

            plr.SendClientMessage("MODIFY CRAFTING: Changed " + target.Name + "'s crafting skill to " + lvl + ".");
            return true;
        }

        public static bool ModifyKeepGuild(Player plr, ref List<string> values)
        {
            var keepId = Convert.ToInt32(values[0]);
            var guildId = Convert.ToInt32(values[1]);

            var selectedKeep = plr.Region.Campaign.Keeps.SingleOrDefault(x => x.Info.KeepId == keepId);
            if (selectedKeep == null)
            {
                plr.SendClientMessage($"MODIFY KEEP GUILD: Keep {keepId} not found");
                return true;
            }

            var guild = Guild.GetGuild((uint) guildId);
            if (guild == null)
            {
                plr.SendClientMessage($"MODIFY KEEP GUILD: Guild {guildId} not found");
                return true;
            }

            selectedKeep.GuildFlag.BeginInteraction(plr);
            return true;
        }

        /// <summary>
        /// Changes your stat (byte Stat)
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ModifyStat(Player plr, ref List<string> values)
        {
            Unit target = GetTargetOrMe(plr);
            if (target == null)
            {
                plr.SendClientMessage("MODIFY STAT: Wrong Target.");
                return true;
            }

            int stat = GetInt(ref values);
            int value = GetInt(ref values);

            if (value < 0)
            {
                value = value * -1;
                target.StsInterface.RemoveItemBonusStat((Stats)stat, (ushort)value);
            }
            else
                target.StsInterface.AddItemBonusStat((Stats)stat, (ushort)value);

            plr.SendClientMessage("MODIFY STAT: Changed " + target.Name + "'s stat " + stat + " to " + value + ".");
            return true;
        }

    }
}
