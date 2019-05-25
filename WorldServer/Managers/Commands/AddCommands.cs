using Common;
using FrameWork;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using Common.Database.World.Battlefront;
using GameData;
using NLog;
using WorldServer.Configs;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Battlefronts.Apocalypse.Loot;
using WorldServer.World.Battlefronts.Bounty;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using static WorldServer.Managers.Commands.GMUtils;
using PlayerContribution = WorldServer.World.Battlefronts.Bounty.PlayerContribution;

namespace WorldServer.Managers.Commands
{
    /// <summary>Addition commands under .add</summary>
    internal class AddCommands
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
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

        public static bool AddRewardEligibility(Player plr, ref List<string> values)
        {
            var activeBattleFrontId = WorldMgr.UpperTierCampaignManager.ActiveBattleFront.BattleFrontId;
            var activeBattleFrontStatus = WorldMgr.UpperTierCampaignManager.GetBattleFrontStatus(activeBattleFrontId);

            plr = GetTargetOrMe(plr) as Player;

            activeBattleFrontStatus.KillContributionSet.Add(plr.CharacterId);

            plr.SendClientMessage(plr.Name + " added to Eligibility");

            return true;
        }

        public static bool AddZoneLockBags(Player plr, ref List<string> values)
        {
            var destructionLootChest = LootChest.Create(plr.Region, plr.WorldPosition, (ushort)plr.ZoneId);

            var orderLootChest = LootChest.Create(plr.Region, plr.WorldPosition, (ushort)plr.ZoneId);
            plr = GetTargetOrMe(plr) as Player;
            if (plr == null)
            {
                return true;
            }
            int numberBags = GetInt(ref values);
            var _rewardManager = new RVRRewardManager();

            var rewardAssigner = new RewardAssigner(StaticRandom.Instance, null);

            var bagDefinitions = rewardAssigner.DetermineBagTypes(numberBags);
            // Assign eligible players to the bag definitions.
            foreach (var lootBagTypeDefinition in bagDefinitions)
            {
                var listPlayerContributions = new List<PlayerContribution>();
                var pc = new PlayerContribution();
                pc.ContributionId = 4;
                pc.Timestamp = TCPManager.GetTimeStamp();
                listPlayerContributions.Add(pc);

                var cd = new ConcurrentDictionary<uint, List<PlayerContribution>>();
                cd.TryAdd(plr.CharacterId, listPlayerContributions);
                var cm = new ContributionManager(cd, BountyService._ContributionDefinitions);

                var eligPlayer = new List<KeyValuePair<uint, int>>();
                eligPlayer.Add(new KeyValuePair<uint, int>(plr.CharacterId, 10));
                var bonuses = new List<RVRPlayerBagBonus>();
                var randomRolls = new Dictionary<uint, int>();
                var pairingContributions = new Dictionary<uint, int>();
                var rewardAssignments = rewardAssigner.AssignLootToPlayers(numberBags, new List<LootBagTypeDefinition> { lootBagTypeDefinition }, eligPlayer, bonuses, randomRolls,pairingContributions, new WorldConfigs { AllowBagBonusContribution = "Y", AllowPairingContribution = "Y", AllowRandomContribution = "Y"});

                var bagContentSelector = new BagContentSelector(RVRZoneRewardService.RVRRewardKeepItems, StaticRandom.Instance);

                foreach (var reward in rewardAssignments)
                {
                    if (reward.Assignee != 0)
                    {
                        var playerItemList = (from item in plr.ItmInterface.Items where item != null select item.Info.Entry).ToList();
                        var playerRenown = plr.CurrentRenown.Level;
                        var playerClass = plr.Info.CareerLine;
                        var playerRenownBand = PlayerUtil.CalculateRenownBand(playerRenown);

                        var lootDefinition = bagContentSelector.SelectBagContentForPlayer(_logger, reward, playerRenownBand, playerClass, playerItemList.ToList(), true);
                        if (lootDefinition.IsValid())
                        {
                            plr.SendClientMessage("Lootdefinition is valid");
                            // Only distribute if loot is valid
                            var generatedLootBag = new LootBagBuilder().BuildChestLootBag(lootDefinition, plr);

                            if (plr.Realm == Realms.REALMS_REALM_DESTRUCTION)
                                destructionLootChest.Add(plr.CharacterId, generatedLootBag);
                            if (plr.Realm == Realms.REALMS_REALM_ORDER)
                                orderLootChest.Add(plr.CharacterId, generatedLootBag);

                        }
                        else
                        {
                            plr.SendClientMessage("Lootdefinition is NOT valid");
                        }
                    }
                }
            }
            return true;

        }
    }
}
