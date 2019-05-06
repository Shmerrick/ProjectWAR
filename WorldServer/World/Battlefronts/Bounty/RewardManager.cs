using FrameWork;
using GameData;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WorldServer.Managers;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Apocalypse.Loot;
using WorldServer.World.Objects;
using WorldServer.World.Positions;

namespace WorldServer.World.Battlefronts.Bounty
{
    public class RewardManager
    {
        private static readonly Logger RewardLogger = LogManager.GetLogger("RewardLogger");

        public const float BOUNTY_BASE_RP_MODIFIER = 200f;
        public const float BOUNTY_BASE_XP_MODIFIER = 5.5f;
        public const float BOUNTY_BASE_INF_MODIFIER = 0.4f;
        public const float BOUNTY_BASE_MONEY_MODIFIER = 10f;
        public const float BASE_RP_CEILING = 800f;
        public const int BASE_MONEY_REWARD = 1000;
        public const int BASE_INFLUENCE_REWARD = 200;
        public const int BASE_XP_REWARD = 2000;
        public const int REALM_CAPTAIN_INFLUENCE_KILL = 500;
        public const int REALM_CAPTAIN_RENOWN_KILL = 2000;
        public const int INSIGNIA_ITEM_ID = 208470;
        public const int PLAYER_DROP_TIER = 50;

        public IContributionManager ContributionManager { get; }
        public IStaticWrapper StaticWrapper { get; }
        public List<RewardPlayerKill> PlayerKillRewardBand { get; }
        public ImpactMatrixManager ImpactMatrixManagerInstance { get; set; }

        public RewardManager(IContributionManager contributionManager, IStaticWrapper staticWrapper, List<RewardPlayerKill> playerKillRewardBand, ImpactMatrixManager impactMatrixManagerInstance)
        {
            ContributionManager = contributionManager;
            StaticWrapper = staticWrapper;
            PlayerKillRewardBand = playerKillRewardBand;
            ImpactMatrixManagerInstance = impactMatrixManagerInstance;
        }

        public virtual int GetInsigniaItemId(int renownBand)
        {
            return (int)PlayerKillRewardBand.Single(x => x.RenownBand == renownBand).CrestId;
        }

        public virtual int GetInsigniaItemCount(int renownBand)
        {
            return (int)PlayerKillRewardBand.Single(x => x.RenownBand == renownBand).CrestCount;
        }

        public virtual double GetRenownBandMoneyBase(int renownBand)
        {
            return StaticWrapper.GetRenownBandReward(renownBand).Money;
        }


        /// <summary>
        /// Determine which players should get insignias for impact.
        /// </summary>
        public bool GetInsigniaRewards(float insigniaChance)
        {
            return insigniaChance > StaticRandom.Instance.Next(100);
        }

        /// <summary>
        /// Given a victim, return the impact fractions for all those hitting them.
        /// </summary>
        /// <param name="victim"></param>
        /// <param name="playerDictionary"></param>
        /// <returns></returns>
        public ConcurrentDictionary<uint, Single> GetImpactFractionsForKill(Player victim, Dictionary<uint, Player> playerDictionary)
        {
            var resultDictionary = new ConcurrentDictionary<uint, Single>();
            var impacts = ImpactMatrixManagerInstance.GetKillImpacts(victim.CharacterId);
            var totalImpact = ImpactMatrixManagerInstance.GetTotalImpact(victim.CharacterId);
            if (totalImpact == 0)
            {
                RewardLogger.Error($"+Total Impact == 0 {victim.Name} ({victim.CharacterId})");
                return resultDictionary;
            }

            foreach (var playerImpact in impacts)
            {
                var impactFraction = CalculateImpactFraction(playerImpact.ImpactValue, totalImpact);
                resultDictionary.TryAdd(playerImpact.CharacterId, impactFraction);
            }

            RewardLogger.Debug($"+Impacts upon victim {victim.Name} ({victim.CharacterId})");
            foreach (var impact in impacts)
            {
                RewardLogger.Debug($"++{impact.ToString()}");
            }

            return resultDictionary;
        }


        private float CalculateImpactFraction(float impactValue, float totalImpact)
        {
            return impactValue / totalImpact;
        }

        private float CalculateModifiedBountyValue(CharacterBounty characterBounty, short contributionValue)
        {
            return CalculateImpactFraction(characterBounty.BaseBountyValue, contributionValue);
        }

        private float CalculateModifiedBountyValue(int baseBounty, short contributionValue)
        {
            return baseBounty + contributionValue;
        }

        public void DistributePlayerKillRewards(Player victim, Player killer, float aaoBonus, ushort influenceId, Dictionary<uint, Player> playerDictionary)
        {
            float ASSIST_CREST_CHANCE = 15f;
            RewardLogger.Info($"=============== START : {victim.Name} killed by {killer.Name}. AAO = {aaoBonus}===============");
            var repeatKillReward = GetRepeatKillModifier(victim, killer);
            RewardLogger.Trace($"+repeatKillReward={repeatKillReward}");

            // Dictionary of attackers and the impact fraction they have on the victim.
            var impactFractions = GetImpactFractionsForKill(victim, playerDictionary);

            // impactFractions is CharacterId, and ImpactFraction.
            foreach (var playerReward in impactFractions)
            {
                // reward key is the characterId
                if (playerDictionary.ContainsKey(playerReward.Key))
                {
                    var playerToBeRewarded = playerDictionary[playerReward.Key];
                    RewardLogger.Info($"+ Assessing rewards for {playerToBeRewarded.Name} ({playerToBeRewarded.CharacterId})");
                    /*
                     * Generate rewards for all those involved in killing the victim, including those out of the group and those within the group of the killer
                     */
                    if (playerToBeRewarded.PriorityGroup != null)
                    {
                        RewardLogger.Trace($"++ Assigning rewards to Group");
                        var shares = playerToBeRewarded.PriorityGroup.Members.Count;
                        foreach (var member in playerToBeRewarded.PriorityGroup.Members)
                        {
                            RewardLogger.Trace($"+++ Group Rewards for {member.Name} ({member.CharacterId})");

                            var modificationValue = ImpactMatrixManagerInstance.CalculateModificationValue(victim.BaseBountyValue, member.BaseBountyValue);
                            var shareModifier = (1f / shares);

                            modificationValue = shareModifier * modificationValue * playerReward.Value;

                            RewardLogger.Debug($"+++ Modification Value {modificationValue} Share Modifier {shareModifier} Number Shares {shares}");
                            RewardLogger.Info($"+++ Assessing rewards for party member {member.Name} ({member.CharacterId})");

                            var xp = CalculateXpReward(playerReward.Value, modificationValue * repeatKillReward);
                            var money = CalculateMoneyReward(playerReward.Value, modificationValue * repeatKillReward);
                            var racialInfluenceModifier = CalculateRacialInfluenceModifier(member, victim);
                            var scaledRenownPoints = (int)CalculateScaledRenownPoints(modificationValue * repeatKillReward);
                            var baseRenownPoints = (int)CalculateBaseRenownPoints(member, victim, modificationValue);
                            var influence = CalculateInfluenceReward(playerReward.Value, modificationValue * repeatKillReward * racialInfluenceModifier);

                            DistributeBaseRewardsForPlayerKill(member, money, scaledRenownPoints, baseRenownPoints, influence, $"Party assist {killer.Name} in killing {victim.Name}", xp, influenceId);

                            RewardLogger.Info($"+++ XP:{xp}, ScaledRP:{scaledRenownPoints} BaseRP:{baseRenownPoints} Inf:{influence} Money:{money}  RacialInfluence:{racialInfluenceModifier} AAO:{aaoBonus}");

                            // Distribute contribution and possible crest
                            var hasInsigniaReward = GetInsigniaRewards(ASSIST_CREST_CHANCE * repeatKillReward);
                            var insigniaName = ItemService.GetItem_Info((uint)INSIGNIA_ITEM_ID).Name;

                            if (hasInsigniaReward)
                            {
                                DistributeInsigniaReward(member,
                                    $"++ You have been awarded 1 {insigniaName} for a kill assist on {victim.Name}",
                                    1);
                            }

                            DistributeKillAssistContributionForPlayerKill(member);
                        }
                        RewardLogger.Trace($"++ End Group Rewards");
                    }
                    else // No group
                    {
                        RewardLogger.Trace($"++ Assigning rewards to Solo");
                        var modificationValue = ImpactMatrixManagerInstance.CalculateModificationValue(victim.BaseBountyValue, killer.BaseBountyValue);

                        RewardLogger.Debug($"+++ Modification Value {modificationValue}");
                        RewardLogger.Info($"+++ Assessing rewards for {playerToBeRewarded.Name} ({playerToBeRewarded.CharacterId}) modvalue:{modificationValue} repeatkill:{repeatKillReward}");

                        var xp = CalculateXpReward(playerReward.Value, modificationValue * repeatKillReward);
                        var money = CalculateMoneyReward(playerReward.Value, modificationValue * repeatKillReward);
                        var racialInfluenceModifier = CalculateRacialInfluenceModifier(playerToBeRewarded, victim);
                        var scaledRenownPoints = (int)CalculateScaledRenownPoints(modificationValue * repeatKillReward);
                        var baseRenownPoints = (int)CalculateBaseRenownPoints(playerToBeRewarded, victim);
                        var influence = CalculateInfluenceReward(playerReward.Value, modificationValue * repeatKillReward * racialInfluenceModifier);

                        // Get the modification value (multiplier for victim vs target)
                        DistributeBaseRewardsForPlayerKill(playerToBeRewarded, money, scaledRenownPoints, baseRenownPoints, influence, $"Solo Assist in killing {victim.Name}", xp,
                            influenceId);

                        RewardLogger.Info(
                            $"+++ XP:{xp}, ScaledRP:{scaledRenownPoints} + BaseRP:{baseRenownPoints} = {(uint)baseRenownPoints + scaledRenownPoints} Inf:{influence} Money:{money} RacialInfluence:{racialInfluenceModifier} modvalue:{modificationValue} repeatkill:{repeatKillReward} AAO:{aaoBonus}");
                    }

                    /*
                     * Give players contribution and possibly insignia rewards
                     */
                    // If this player is the killer (ie Deathblow), give them a different contribution.
                    if (playerReward.Key == killer.CharacterId)
                    {
                        var modificationValue = ImpactMatrixManagerInstance.CalculateModificationValue(victim.BaseBountyValue, killer.BaseBountyValue);

                        var hasInsigniaReward = GetInsigniaRewards(100 * repeatKillReward);
                        var insigniaName = ItemService.GetItem_Info((uint)INSIGNIA_ITEM_ID).Name;

                        if (hasInsigniaReward)
                        {
                            DistributeInsigniaReward(playerToBeRewarded, $"++ You have been awarded 1 {insigniaName} for a deathblow on {victim.Name}", 1);
                        }

                        if (killer.GetBattlefrontManager(killer.Region.RegionId).ActiveBattleFront.ZoneId == killer.ZoneId)
                            DistributeDeathBlowContributionForPlayerKill(killer, victim.Name);
                        else
                        {
                            RewardLogger.Warn($"{killer.Name} performed deathblow not in active BF");
                        }
                    }
                    else // An assist only.
                    {

                        var hasInsigniaReward = GetInsigniaRewards(ASSIST_CREST_CHANCE * repeatKillReward);
                        var insigniaName = ItemService.GetItem_Info((uint)INSIGNIA_ITEM_ID).Name;

                        if (hasInsigniaReward)
                        {
                            DistributeInsigniaReward(playerToBeRewarded,
                                $"++ You have been awarded 1 {insigniaName} for a kill assist on {victim.Name}",
                                1);
                        }

                        if (killer.GetBattlefrontManager(playerToBeRewarded.Region.RegionId).ActiveBattleFront.ZoneId == playerToBeRewarded.ZoneId)
                            DistributeKillAssistContributionForPlayerKill(playerToBeRewarded);
                        else
                        {
                            RewardLogger.Warn($"{killer.Name} performed assist not in active BF");
                        }



                    }
                }

            }
            RewardLogger.Info($"=============== FINISHED : {victim.Name} killed by {killer.Name}. ===============");

        }
        private int CalculateXpReward(float impactFraction, float externalModifier)
        {
            // Add 0-50% XP to the base amount
            var xpRandom = StaticRandom.Instance.Next(50);
            if (xpRandom != 0)
            {
                var xpMultipler = (xpRandom / 100f) + 1f;
                return (int)Math.Floor(BASE_XP_REWARD * xpMultipler * externalModifier * impactFraction);
            }
            return 0;
        }

        private float CalculateBaseRenownPoints(Player killer, Player victim, float modificationValue = 1)
        {
            var victimContributionValue = ContributionManager.GetContributionValue(victim.CharacterId);
            var killerContributionValue = ContributionManager.GetContributionValue(killer.CharacterId);

            return (BASE_RP_CEILING -
                (CalculateModifiedBountyValue(victim.BaseBountyValue, victimContributionValue) +
                CalculateModifiedBountyValue(killer.BaseBountyValue, killerContributionValue))) * modificationValue;
        }

        private float CalculateScaledRenownPoints(float externalModifier)
        {
            return (externalModifier * BOUNTY_BASE_RP_MODIFIER);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="playerToBeRewarded"></param>
        /// <param name="victim"></param>
        /// <returns></returns>
        private float CalculateRacialInfluenceModifier(Player playerToBeRewarded, Player victim)
        {


            //if (playerToBeRewarded.Info.CareerLine == )
            //CAREERLINE_IRON_BREAKER = 1,
            //CAREERLINE_SLAYER = 2,
            //CAREERLINE_RUNE_PRIEST = 3,
            //CAREERLINE_ENGINEER = 4,

            //CAREERLINE_BLACK_ORC = 5,
            //CAREERLINE_CHOPPA = 6,
            //CAREERLINE_SHAMAN = 7,
            //CAREERLINE_SQUIG_HERDER = 8,

            //CAREERLINE_WITCH_HUNTER = 9,
            //CAREERLINE_KNIGHT = 10,
            //CAREERLINE_BRIGHT_WIZARD = 11,
            //CAREERLINE_WARRIOR_PRIEST = 12,

            //CAREERLINE_CHOSEN = 13,
            //CAREERLINE_WARRIOR = 14,
            //CAREERLINE_ZEALOT = 15,
            //CAREERLINE_MAGUS = 16,

            //CAREERLINE_SWORDMASTER = 17,
            //CAREERLINE_SHADOW_WARRIOR = 18,
            //CAREERLINE_WHITELION = 19,
            //CAREERLINE_ARCHMAGE = 20,

            //CAREERLINE_BLACKGUARD = 21,
            //CAREERLINE_WITCHELF = 22,
            //CAREERLINE_DISCIPLE = 23,
            //CAREERLINE_SORCERER = 24,

            //var pairing = 

            //if (playerToBeRewarded.Zone.Info.Pairing == 

            return 1f;
        }

        /// <summary>
        /// Calculate the influence reward for the player kill
        /// </summary>
        /// <param name="impactFraction">Fraction of the kill vs the victim</param>
        /// <param name="externalModifier">Repeat Kill, Bounty variance modifiers</param>
        /// <returns></returns>
        private int CalculateInfluenceReward(float impactFraction, float externalModifier)
        {
            // Add 0-25% Influence to the base amount
            var influenceRandom = StaticRandom.Instance.Next(25);
            var baseInfluence = 0;
            if (influenceRandom != 0)
            {
                var influenceModifier = (influenceRandom / 100f) + 1f;
                return (int)Math.Floor(BASE_INFLUENCE_REWARD * influenceModifier * externalModifier * impactFraction);
            }
            return baseInfluence;
        }

        /// <summary>
        /// Calculate the monetary reward for the player kill
        /// </summary>
        /// <param name="impactFraction">Fraction of the kill vs the victim</param>
        /// <param name="externalModifier">Repeat Kill, Bounty variance modifiers</param>
        /// <returns></returns>
        private int CalculateMoneyReward(float impactFraction, float externalModifier)
        {
            // Add 0-50% Money to the base amount
            var moneyRandom = StaticRandom.Instance.Next(50);
            if (moneyRandom != 0)
            {
                var moneyMultipler = (moneyRandom / 100f) + 1f;
                return (int)Math.Floor(BASE_MONEY_REWARD * moneyMultipler * externalModifier * impactFraction);
            }
            return 0;
        }

        /// <summary>
        /// For a given player, killer combination - what modification to the reward should be given.
        /// </summary>
        /// <param name="victim"></param>
        /// <param name="killer"></param>
        /// <returns></returns>
        private float GetRepeatKillModifier(Player victim, Player killer)
        {
            var repeatKillReward = 1.0f;

            foreach (var item in victim._recentLooters)
            {
                RewardLogger.Trace($"Repeat Kill {item.Key} {item.Value} ");
            }

            // TODO - replace with something smarter than this.
            // If the same player kills the same victim within a short period, ignore.
            var currentTime = TCPManager.GetTimeStampMS();
            if (victim._recentLooters.ContainsKey(killer.CharacterId) && victim._recentLooters[killer.CharacterId] > currentTime)
            {
                RewardLogger.Trace($"Repeat Kill {victim._recentLooters[killer.CharacterId]} {currentTime} ");
                // Lowering rewards for repeat kills
                repeatKillReward = 0.25f;
            }

            return repeatKillReward;
        }

        /// <summary>
        /// The killer is the player that performed the deathblow. 
        /// </summary>
        private void DistributeDeathBlowContributionForPlayerKill(Player killer, string victimName)
        {

            RewardLogger.Info($"++ Death Blow contribution given to {killer.Name} ({killer.CharacterId}) for DB to {victimName}");

            killer.UpdatePlayerBountyEvent((byte)ContributionDefinitions.PLAYER_KILL_DEATHBLOW);

            if (killer.AAOBonus > 0)
            {
                // Add contribution for this kill under AAO to the killer.
                killer.UpdatePlayerBountyEvent((byte)ContributionDefinitions.PLAYER_KILL_DEATHBLOW_UNDER_AAO);
            }

            // If the deathblow comes while the target is near a BattlefieldObjective
            if (killer.CurrentObjectiveFlag != null)
            {
                killer.UpdatePlayerBountyEvent((byte)ContributionDefinitions.PLAYER_KILL_ON_BO);
            }
        }




        /// <summary>
        /// Distribute rewards for a group involved in a player kill
        /// </summary>
        /// <param name="groupMembers"></param>
        /// <param name="reward"></param>
        /// <param name="influenceId"></param>
        //private void DistributeGroupSharedRewards(Player victim, List<Player> groupMembers, Reward reward, ushort influenceId, string description)
        //{
        //    float shares = groupMembers.Count;

        //    foreach (var groupMember in groupMembers)
        //    {
        //        // As we are giving rewards for groupmembers, these rewards should be modified by the differences between their basebounty level and that of the victims
        //        var modificationValue =
        //            ImpactMatrixManager.CalculateModificationValue(victim.BaseBountyValue, groupMember.BaseBountyValue);

        //        RewardLogger.Info($"Awarding group member {groupMember.Name} ({groupMember.CharacterId}) group assist reward. ModificationValue= {modificationValue}");
        //        DistributeBaseRewardsForPlayerKill(reward, groupMember, (1f / shares) * modificationValue, influenceId, description);
        //        groupMember.UpdatePlayerBountyEvent((byte)ContributionDefinitions.PARTY_KILL_ASSIST);
        //    }
        //}

        private void DistributeInsigniaReward(Player killer, string description, int number)
        {
            killer.ItmInterface.CreateItem(ItemService.GetItem_Info((uint)INSIGNIA_ITEM_ID), (ushort)number);
            RewardLogger.Info(description);
            killer.SendClientMessage(description);
        }

        private void DistributeKillAssistContributionForPlayerKill(Player killer)
        {
            RewardLogger.Trace($"++ Kill Assist contribution given to {killer.Name} ({killer.CharacterId})");

            killer.UpdatePlayerBountyEvent((byte)ContributionDefinitions.PARTY_KILL_ASSIST);

            if (killer.AAOBonus > 0)
            {
                // Add contribution for this kill under AAO to the killer.
                killer.UpdatePlayerBountyEvent((byte)ContributionDefinitions.PLAYER_KILL_ASSIST_UNDER_AAO);
            }

            // If the deathblow comes while the target is near a BattlefieldObjective
            if (killer.CurrentObjectiveFlag != null)
            {
                killer.UpdatePlayerBountyEvent((byte)ContributionDefinitions.PLAYER_KILL_ASSIST_ON_BO);
            }
        }
        private void DistributeBaseRewardsForPlayerKill(Player killer, int money, int scaledRenownPoints, int baseRenownPoints, int influence, string description, int xp, int influenceId)
        {

            killer.AddXp((uint)((uint)xp), true, true);
            // AAO is applied within this method
            killer.AddRenown((uint)baseRenownPoints + (uint)scaledRenownPoints, true, RewardType.None, description);
            killer.AddInfluence((ushort)influenceId, (ushort)(influence));
            killer.AddMoney((uint)((uint)money));

            var logMessage = $"+++ PlayerKill Base Reward (player:{killer.Name} ({killer.CharacterId})) ## {description} ## " +
                               $"XP : {xp} " +
                               $"RP : {baseRenownPoints} + {scaledRenownPoints} " +
                               $"INF : {influence} ";

            RewardLogger.Info(logMessage);


        }

        /// <summary>
        /// The killer has killed a realm captain. 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="killer"></param>
        /// <param name="influenceId"></param>
        /// <param name="playersByCharId"></param>
        public void RealmCaptainKill(Player victim, Player killer, ushort influenceId, Dictionary<uint, Player> playersByCharId)
        {
            RewardLogger.Info($"Death Blow rewards given to {killer.Name} ({killer.CharacterId}) for realm captain kill");
            killer.AddInfluence(influenceId, REALM_CAPTAIN_INFLUENCE_KILL);
            killer.AddRenown(REALM_CAPTAIN_RENOWN_KILL, 1f, false);
            killer.UpdatePlayerBountyEvent((byte)ContributionDefinitions.REALM_CAPTAIN_KILL);

            ushort crests = (ushort)StaticRandom.Instance.Next(10);

            killer.SendClientMessage($"Awarded {crests} Crest(s) to " + killer.Name + " for killing realm captain");
            killer.SendClientMessage($"Awarded {REALM_CAPTAIN_RENOWN_KILL} RR {REALM_CAPTAIN_INFLUENCE_KILL} INF to " + killer.Name + " for killing realm captain");
            killer.SendClientMessage($"You have been awarded additional contribution in assisting with the downfall of the enemy");
            RewardLogger.Trace($"Awarded {crests} Crest(s) to " + killer.Name + " for killing realm captain");
            killer.ItmInterface.CreateItem(208470, crests);

            if (killer.PriorityGroup != null)
            {
                foreach (var groupMember in killer.PriorityGroup.Members)
                {
                    killer.SendClientMessage($"Awarded {1} Crest(s) to " + killer.Name + " for killing realm captain");
                    RewardLogger.Trace($"Awarded {1} Crest(s) to " + killer.Name + " for killing realm captain");
                    killer.ItmInterface.CreateItem(208470, 1);

                    killer.AddInfluence(influenceId, (ushort)Math.Floor((double)(REALM_CAPTAIN_INFLUENCE_KILL / killer.PriorityGroup.Members.Count)));
                    killer.UpdatePlayerBountyEvent((byte)ContributionDefinitions.REALM_CAPTAIN_KILL);
                }
            }
        }

        public class Reward
        {
            public int RenownBand { get; set; }
            public int InsigniaItemId { get; set; }
            public int BaseMoney { get; set; }
            public int InsigniaCount { get; set; }
            public int BaseRP { get; set; }
            public int BaseXP { get; set; }
            public int BaseInf { get; set; }
            public string Description { get; set; }

            public override string ToString()
            {
                return $"Reward RRBand : {RenownBand}. {InsigniaCount}x{InsigniaItemId}, Money:{BaseMoney}, RP:{BaseRP}, XP:{BaseXP}, Inf:{BaseInf} for {Description}";
            }

            public static int DetermineRenownBand(int playerRenownLevel)
            {
                // Add extra bounds. 
                if (playerRenownLevel == 0)
                    return 10;
                if (playerRenownLevel >= 100)
                    return 100;

                if (playerRenownLevel % 10 == 0) return playerRenownLevel;
                return (10 - playerRenownLevel % 10) + playerRenownLevel;
            }
        }

        /// <summary>
        /// Determine whether killing the player should drop a piece of gear for the killer. The gear should be matched to the killer.
        /// </summary>
        /// <param name="killer"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public void SetPlayerRVRGearDrop(Player killer, Player victim)
        {
            var rand = 0;
            if (Player._Players.Count < PLAYER_DROP_TIER)
                rand = StaticRandom.Instance.Next(0, 6000);
            else
            {
                rand = StaticRandom.Instance.Next(0, 10000);
            }
            var availableGearDrops = RewardService._PlayerRVRGearDrops
                .Where(x => x.MinimumRenownRank < victim.RenownRank)
                .Where(x => x.MaximumRenownRank >= victim.RenownRank)
                .Where(x => x.DropChance >= rand)
                .Where(x => x.Career == killer.Info.CareerLine);
            //Randomise list
            availableGearDrops = availableGearDrops?.OrderBy(a => StaticRandom.Instance.Next()).ToList();
            RewardLogger.Debug($"### {victim.Name} / {victim.RenownRank} {availableGearDrops.Count()} RVR Gear items available for killer {killer}. PlayerCount = {Player._Players.Count}");
            var playerItemList = (from item in killer.ItmInterface.Items where item != null select item.Info.Entry).ToList();

            foreach (var availableGearDrop in availableGearDrops)
            {
                if (!ItemExistsForPlayer(availableGearDrop.ItemId, playerItemList))
                {
                    var lootChest = LootChest.Create(
                        victim.Region,
                        new Point3D(victim.WorldPosition.X, victim.WorldPosition.Y, victim.WorldPosition.Z),
                        (ushort)victim.ZoneId, false);


                    var generatedLootBag = WorldMgr.RewardDistributor.BuildChestLootBag(LootBagRarity.Gold, availableGearDrop.ItemId, killer);

                    lootChest.Add(killer.CharacterId, generatedLootBag);

                    // Add item to victim as loot
                    //victim.lootContainer = new LootContainer { Money = availableGearDrop.Money };
                    //victim.lootContainer.LootInfo.Add(
                    //    new LootInfo(ItemService.GetItem_Info(availableGearDrop.ItemId)));
                    //if (victim.lootContainer != null)
                    //{
                    //    victim.SetLootable(true, killer);

                    //}



                    RewardLogger.Info($"### {victim.Name} / {victim.RenownRank} dropped {availableGearDrop.ItemId} for killer {killer}");
                    killer.SendClientMessage($"You have scavenged an item of rare worth from {victim.Name}");

                    return;
                }
            }

            // If there were items, but player had them, give them some crests as a reward
            if (availableGearDrops.Count() > 0)
            {
                victim.lootContainer = new LootContainer { Money = 1000 };
                victim.lootContainer.LootInfo.Add(new LootInfo(ItemService.GetItem_Info(208470)));
                victim.lootContainer.LootInfo.Add(new LootInfo(ItemService.GetItem_Info(208470)));
                victim.lootContainer.LootInfo.Add(new LootInfo(ItemService.GetItem_Info(208470)));
                victim.lootContainer.LootInfo.Add(new LootInfo(ItemService.GetItem_Info(208470)));
                victim.lootContainer.LootInfo.Add(new LootInfo(ItemService.GetItem_Info(208470)));
                if (victim.lootContainer != null)
                    victim.SetLootable(true, killer);
            }
        }

        private bool ItemExistsForPlayer(uint itemId, List<uint> playerItemList)
        {
            return playerItemList.Count(x => x == itemId) > 0;
        }
    }

    public enum CharacterRewardStatus
    {
        WINNING_ELIGIBLE,
        WINNING_NON_ELIGIBLE,
        LOSING_ELIGIBLE,
        LOSING_NON_ELIGIBLE
    }

}
