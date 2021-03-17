using Common;
using Common.Database.World.Battlefront;
using FrameWork;
using GameData;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using WorldServer.Configs;
using WorldServer.Managers;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Battlefronts.Apocalypse.Loot;
using WorldServer.World.Objects;

namespace WorldServer.World.Battlefronts.Bounty
{
    public partial class RewardManager : IRewardManager
    {
        private static readonly Logger RewardLogger = LogManager.GetLogger("RewardLogger");
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public const float BOUNTY_BASE_RP_MODIFIER = 180f;
        public const float BOUNTY_BASE_XP_MODIFIER = 5.5f;
        public const float BOUNTY_BASE_INF_MODIFIER = 0.4f;
        public const float BOUNTY_BASE_MONEY_MODIFIER = 10.0f;
        public const float BASE_RP_CEILING = 700.0f;
        public const int BASE_MONEY_REWARD = 1000;
        public const int BASE_INFLUENCE_REWARD = 200;
        public const int BASE_XP_REWARD = 1000;
        public const int INSIGNIA_ITEM_ID = 208470;
        public const int PLAYER_DROP_TIER = 50;
        public const float RVR_GEAR_DROP_MINIMUM_IMPACT_FRACTION = 0.1f;

        public IContributionManager ContributionManager { get; }
        public IStaticWrapper StaticWrapper { get; }
        public List<RewardPlayerKill> PlayerKillRewardBand { get; }
        public IImpactMatrixManager ImpactMatrixManagerInstance { get; set; }

        public KeepLockRewardDistributor KeepLockRewardDistributor { get; set; }
        public ZoneLockRewardDistributor ZoneLockRewardDistributor { get; set; }

        public RewardManager(IContributionManager contributionManager, IStaticWrapper staticWrapper, List<RewardPlayerKill> playerKillRewardBand, IImpactMatrixManager impactMatrixManagerInstance)
        {
            ContributionManager = contributionManager;
            StaticWrapper = staticWrapper;
            PlayerKillRewardBand = playerKillRewardBand;
            ImpactMatrixManagerInstance = impactMatrixManagerInstance;

            KeepLockRewardDistributor = new KeepLockRewardDistributor(new RandomGenerator(), RVRZoneRewardService.RVRKeepLockRewards);
            ZoneLockRewardDistributor = new ZoneLockRewardDistributor(new RandomGenerator(), RVRZoneRewardService.RVRZoneLockRewards);
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
            float ASSIST_CREST_CHANCE = 12f;
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

                            if (!member.IsValidForReward(victim))
                                continue;

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
                        if (!playerToBeRewarded.IsValidForReward(victim))
                            continue;

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

            /* Determine and send Player RVR Gear Drop. */

            var selectedKillerCharacterId = GetPlayerRVRDropCandidate(impactFractions);
            if (selectedKillerCharacterId != 0)
            {
                var selectedKiller = Player.GetPlayer(selectedKillerCharacterId);
                if (selectedKiller != null)
                {
                    if (selectedKiller.PriorityGroup != null)
                    {
                        var selectedPartyMember = selectedKiller.PriorityGroup.SelectRandomPlayer();
                        if (selectedPartyMember != null)
                        {
                            if (selectedPartyMember.IsValidForReward(victim))
                            {
                                PlayerKillPVPDrop(selectedPartyMember, victim);
                                RewardLogger.Debug(
                                    $"{selectedPartyMember.Name} selected for group loot - linked from {selectedKiller.Name}");
                            }
                        }
                    }
                    else
                    {
                        PlayerKillPVPDrop(selectedKiller, victim);
                    }
                }
            }

            RewardLogger.Info($"=============== FINISHED : {victim.Name} killed by {killer.Name}. ===============");
        }

        public uint GetPlayerRVRDropCandidate(ConcurrentDictionary<uint, float> impactFractions, int forceSelectedInstance = -1)
        {
            var selectedInstance = 0;
            if (forceSelectedInstance == -1)
                selectedInstance = StaticRandom.Instance.Next(0, impactFractions.Count());
            else
            {
                selectedInstance = forceSelectedInstance;
            }

            var minimumImpactCharacterList =
                impactFractions.Where(x => x.Value > RVR_GEAR_DROP_MINIMUM_IMPACT_FRACTION).ToList();

            if (minimumImpactCharacterList.Count >= selectedInstance)
            {
                try
                {
                    return minimumImpactCharacterList[selectedInstance].Key;
                }
                catch (Exception e)
                {
                    Logger.Debug($"{e.Message} {e.StackTrace}. Selected Instance {selectedInstance} Count {minimumImpactCharacterList.Count}");
                    return 0;
                }
            }
            else
            {
                return 0;
            }
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
        /// Distribute RR XP INF rewards to players that have some contribution
        /// </summary>
        /// <param name="eligibleLosingRealmPlayers">non-zero contribution losing realm players</param>
        /// <param name="eligibleWinningRealmPlayers">non-zero contribution winning realm playes</param>
        /// <param name="lockingRealm"></param>
        /// <param name="baselineContribution">The baseline expected of an 'average' player. If player is below this amount, lower reward, above, increase.</param>
        /// <param name="tierRewardScale"></param>
        /// <param name="allPlayersInZone"></param>
        /// <param name="rvrKeepRewards"></param>
        public void DistributeKeepTakeBaseRewards(ConcurrentDictionary<Player, int> eligibleLosingRealmPlayers,
            ConcurrentDictionary<Player, int> eligibleWinningRealmPlayers,
            Realms lockingRealm,
            int baselineContribution,
            float tierRewardScale,
            List<Player> allPlayersInZone,
            List<RVRKeepLockReward> rvrKeepRewards)
        {
            // Distribute rewards to losing players with eligibility - halve rewards.
            foreach (var losingRealmPlayer in eligibleLosingRealmPlayers)
            {
                // Scale of player contribution against the highest contributor
                double contributionScale = CalculateContributonScale(losingRealmPlayer.Value, baselineContribution);
                KeepLockRewardDistributor.DistributeNonBagAwards(
                    losingRealmPlayer.Key,
                    PlayerUtil.CalculateRenownBand(losingRealmPlayer.Key.RenownRank),
                    (1f + contributionScale) * tierRewardScale);
            }

            // Distribute rewards to winning players with eligibility - full rewards.
            foreach (var winningRealmPlayer in eligibleWinningRealmPlayers)
            {
                double contributionScale = CalculateContributonScale(winningRealmPlayer.Value, baselineContribution);
                KeepLockRewardDistributor.DistributeNonBagAwards(
                     winningRealmPlayer.Key,
                     PlayerUtil.CalculateRenownBand(winningRealmPlayer.Key.RenownRank),
                     (1.5f + contributionScale) * tierRewardScale);
            }

            if (allPlayersInZone != null)
            {
                foreach (var player in allPlayersInZone)
                {
                    if (player.Realm == lockingRealm)
                    {
                        // Ensure player is not in the eligible list.
                        if (eligibleWinningRealmPlayers.All(x => x.Key.CharacterId != player.CharacterId))
                        {
                            // Give player no bag, but half rewards
                            KeepLockRewardDistributor.DistributeNonBagAwards(
                                 player,
                                 PlayerUtil.CalculateRenownBand(player.RenownRank),
                                 0.5 * tierRewardScale);
                        }
                    }
                    else
                    {
                        // Ensure player is not in the eligible list.
                        if (eligibleLosingRealmPlayers.All(x => x.Key.CharacterId != player.CharacterId))
                        {
                            // Give player no bag, but quarter rewards
                            KeepLockRewardDistributor.DistributeNonBagAwards(
                                 player,
                                 PlayerUtil.CalculateRenownBand(player.RenownRank),
                                 0.25 * tierRewardScale);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculate the contribution scale for this player. This is to vary the reward given for individual contribution to the zone lock.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maximumContribution"></param>
        /// <returns></returns>
        private double CalculateContributonScale(int value, int maximumContribution)
        {
            if (maximumContribution == 0)
                return 0;
            return (double)value / (double)maximumContribution;
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

            if (killer.PriorityGroup != null)
            {
                foreach (var groupMember in killer.PriorityGroup.Members)
                {
                    groupMember.SendClientMessage($"Awarded {1} Crest(s) to " + killer.Name + " for killing realm captain");
                    RewardLogger.Trace($"Awarded {1} Crest(s) to " + killer.Name + " for killing realm captain");
                    groupMember.ItmInterface.CreateItem(208470, (ushort)Program.Config.REALM_CAPTAIN_ASSIST_CRESTS);
                    groupMember.AddRenown((uint)Program.Config.REALM_CAPTAIN_RENOWN_KILL_PARTY, 1f, false);
                    groupMember.SendClientMessage($"Awarded {Program.Config.REALM_CAPTAIN_RENOWN_KILL_PARTY} RR {(ushort)Math.Floor((double)(Program.Config.REALM_CAPTAIN_INFLUENCE_KILL / killer.PriorityGroup.Members.Count))} INF to " + groupMember.Name + " for killing realm captain");
                    groupMember.AddInfluence(influenceId, (ushort)Math.Floor((double)(Program.Config.REALM_CAPTAIN_INFLUENCE_KILL / killer.PriorityGroup.Members.Count)));
                    groupMember.UpdatePlayerBountyEvent((byte)ContributionDefinitions.REALM_CAPTAIN_KILL);
                }
            }
            else
            {
                ushort crests = (ushort)StaticRandom.Instance.Next(Program.Config.REALM_CAPTAIN_KILL_CRESTS);
                RewardLogger.Trace($"Awarded {crests} Crest(s) to " + killer.Name + " for killing realm captain");

                killer.AddRenown((uint)Program.Config.REALM_CAPTAIN_RENOWN_KILL_SOLO, 1f, false);
                killer.AddInfluence(influenceId, (ushort)Program.Config.REALM_CAPTAIN_INFLUENCE_KILL);
                killer.UpdatePlayerBountyEvent((byte)ContributionDefinitions.REALM_CAPTAIN_KILL);
                killer.SendClientMessage($"Awarded {crests} Crest(s) to " + killer.Name + " for killing realm captain");
                killer.SendClientMessage($"You have been awarded additional contribution in assisting with the downfall of the enemy");

                killer.ItmInterface.CreateItem(208470, crests);
            }
        }

        /// <summary>
        /// Determine whether killing the player should drop a piece of gear for the killer. The gear should be matched to the killer.
        /// </summary>
        /// <param name="killer"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public void PlayerKillPVPDrop(Player killer, Player victim)
        {
            var rand = 0;

            //In a scenario, leave.
            if (killer.ScnInterface.Scenario != null)
                return;

            var randomScaleMultiplier = 1;

            RewardLogger.Debug($"### Multiplier {randomScaleMultiplier}");

            //if (Math.Abs(randomScaleMultiplier) < 0.3)
            //    randomScaleMultiplier = 1;

            //if (Math.Abs(randomScaleMultiplier) > 1.3)
            //    randomScaleMultiplier = 1;

            RewardLogger.Debug($"### {victim.Name} / {victim.RenownRank} : AAOBonus {killer.AAOBonus} Multiplier {randomScaleMultiplier}");

            if (Player._Players.Count < PLAYER_DROP_TIER)
                rand = StaticRandom.Instance.Next(0, (int)(6000 * randomScaleMultiplier));
            else
            {
                rand = StaticRandom.Instance.Next(0, (int)(10000 * randomScaleMultiplier));
            }

            var availableGearDrops = RewardService._PlayerRVRGearDrops
                .Where(x => x.MinimumRenownRank < victim.RenownRank)
                .Where(x => x.MaximumRenownRank >= victim.RenownRank)
                .Where(x => x.Realm == (int)killer.Realm)
                .Where(x => x.DropChance >= rand);

            //Randomise list
            availableGearDrops = availableGearDrops?.OrderBy(a => StaticRandom.Instance.Next()).ToList();
            RewardLogger.Debug($"### {victim.Name} / {victim.RenownRank} Nbr Gear Drops : {availableGearDrops.Count()} RVR Gear items available for killer {killer}. PlayerCount = {Player._Players.Count}");
            var playerItemList = (from item in killer.ItmInterface.Items where item != null select item.Info.Entry).ToList();

            foreach (var availableGearDrop in availableGearDrops)
            {
                // Drop item into GOld Chest
                //var lootChest = LootChest.Create(
                //    victim.Region,
                //    new Point3D(victim.WorldPosition.X, victim.WorldPosition.Y, victim.WorldPosition.Z),
                //    (ushort)victim.ZoneId, false);

                //var generatedLootBag =KeepLockRewardDistributor.BuildChestLootBag(LootBagRarity.Gold, availableGearDrop.ItemId, killer);

                //lootChest.Add(killer.CharacterId, generatedLootBag);

                // Add item to victim as loot
                //victim.lootContainer = new LootContainer { Money = availableGearDrop.Money };
                //victim.lootContainer.LootInfo.Add(
                //    new LootInfo(ItemService.GetItem_Info(availableGearDrop.ItemId)));
                //if (victim.lootContainer != null)
                //{
                //    victim.SetLootable(true, killer);
                //}
                var item = ItemService.GetItem_Info((uint)availableGearDrop.ItemId);

                killer.ItmInterface.CreateItem(item, (ushort)1);

                RecordPlayerKillRewardHistory(killer, victim, availableGearDrop.ItemId);

                RewardLogger.Info($"### {victim.Name} / {victim.RenownRank} dropped {availableGearDrop.ItemId} for killer {killer}");
                killer.SendClientMessage($"You have been awarded an item of rare worth from {victim.Name} - {item.Name}", ChatLogFilters.CHATLOGFILTERS_LOOT);

                return;
            }
        }

        private void RecordPlayerKillRewardHistory(Player killer, Player victim, uint itemId)
        {
            RewardLogger.Debug($"Recording Player Kill history - item drop {killer.Name} kills {victim.Name}, receives {itemId}");
            try
            {
                var history = new PlayerKillRewardHistory
                {
                    VictimCharacterId = (int)victim.CharacterId,
                    KillerCharacterId = (int)killer.CharacterId,
                    VictimCharacterName = victim.Name,
                    KillerCharacterName = killer.Name,
                    Timestamp = DateTime.UtcNow,
                    ZoneId = (int)killer.ZoneId,
                    ItemName = ItemService.GetItem_Info(itemId).Name,
                    ItemId = (int)itemId,
                    ZoneName = ZoneService.GetZone_Info((ushort)killer.ZoneId).Name
                };
                RewardLogger.Debug($"=> {history.ItemName}");

                WorldMgr.Database.AddObject(history);
            }
            catch (Exception e)
            {
                RewardLogger.Error($"{e.Message} {e.StackTrace}");
            }
        }

        private bool ItemExistsForPlayer(uint itemId, List<uint> playerItemList)
        {
            return playerItemList.Count(x => x == itemId) > 0;
        }

        public List<LootBagTypeDefinition> GenerateBagDropAssignments(ConcurrentDictionary<Player, int> realmPlayers,
            int forceNumberBags, string leadInZones, int additionalBags, int forceDropChance = 100)
        {
            List<LootBagTypeDefinition> bagDefinitions = new List<LootBagTypeDefinition>();
            var numberOfBagsToAward = 0;
            var rewardAssigner = new RewardAssigner(StaticRandom.Instance, RewardLogger);

            var pairs = new List<KeyValuePair<uint, int>>();
            foreach (var winningRealmPlayer in realmPlayers)
            {
                pairs.Add(new KeyValuePair<uint, int>((uint)winningRealmPlayer.Key.CharacterId, winningRealmPlayer.Value));
            }
            // sort the pairs
            var sortedPairs = pairs.OrderBy(x => x.Value).Reverse().ToList();

            foreach (var pair in sortedPairs)
            {
                try
                {
                    var name = realmPlayers.SingleOrDefault(x => x.Key.CharacterId == pair.Key);
                    Logger.Debug($"===== Character / Contribution pre bonus : {pair.Key}:{pair.Value} ({name.Key.Name})");
                }
                catch (Exception e)
                {
                    Logger.Debug($"{e.Message})");
                }
            }
            // The number of bags to award is based upon the number of eligible players.
            numberOfBagsToAward = rewardAssigner.GetNumberOfBagsToAward(forceNumberBags, sortedPairs);
            // forceDropChance should alter the number of bags dropped.
            numberOfBagsToAward = (int)Math.Ceiling(numberOfBagsToAward * (forceDropChance / 100f));
            Logger.Debug($"===== Number Of Awards (post dropchance {forceDropChance}) : {numberOfBagsToAward}");
            // Determine and build out the bag types to be assigned
            bagDefinitions = rewardAssigner.DetermineBagTypes(numberOfBagsToAward);

            // Calculate AdditionalBag types
            Logger.Debug($"===== Calculating Additional Loot Defintions based on additional Bags : {additionalBags}");
            var additionalBagDefintions = rewardAssigner.DetermineAdditionalBagTypes(additionalBags);
            foreach (var additionalBagDefintion in additionalBagDefintions)
            {
                Logger.Debug($"===== Additional Loot Defintion : {additionalBagDefintion.BagRarity}");
            }

            if (additionalBagDefintions != null)
                bagDefinitions.AddRange(additionalBagDefintions);

            // No bags, leaving.
            if (bagDefinitions.Count == 0)
                return bagDefinitions;

            var characterList = (from kvp in sortedPairs select kvp.Key).Distinct().ToList();
            var characterJoinedList = string.Join(",", characterList);
            var bagBonusCharacters = new List<RVRPlayerBagBonus>();
            if (characterJoinedList != "")
            {
                bagBonusCharacters =
                    CharMgr.Database.SelectObjects<RVRPlayerBagBonus>($"CharacterId in ({characterJoinedList})").ToList();
            }

            Logger.Debug(characterJoinedList);
            Logger.Debug(leadInZones);

            // Generate random rolls for each of the sortedPairs (characters).
            var randomRollList = new Dictionary<uint, int>();

            // Assign eligible players to the bag definitions.
            var pairingContributions = new Dictionary<uint, int>();
            var zoneEligibiltyCharacters = new List<ZoneLockEligibilityHistory>();
            if ((leadInZones != "") && (characterJoinedList != ""))
            {
                var query =
                    $"CharacterId in ({characterJoinedList}) and ZoneId in ({leadInZones}) and timestamp BETWEEN DATE_SUB(UTC_TIMESTAMP(), INTERVAL {Program.Config.PairingContributionTimeIntervalHours} HOUR) AND UTC_TIMESTAMP() ";
                zoneEligibiltyCharacters = (List<ZoneLockEligibilityHistory>)WorldMgr.Database.SelectObjects<ZoneLockEligibilityHistory>(query);
                Logger.Debug($"{query}");
                Logger.Debug($"zoneEligibiltyCharacters : {zoneEligibiltyCharacters.Count}");
            }

            foreach (var character in sortedPairs)
            {
                var random = StaticRandom.Instance.Next(Program.Config.BagRollRandomLowerLimit, Program.Config.BagRollRandomUpperLimit);
                randomRollList.Add(character.Key, random);

                if (leadInZones != "")
                {
                    var pairingBonus = Program.Config.PairingBonusIncrement *
                                       zoneEligibiltyCharacters.Count(x => x.CharacterId == character.Key);
                    Logger.Debug($"Pairing Bonus : {pairingBonus}");
                    pairingContributions.Add(character.Key, pairingBonus);
                }
            }

            return rewardAssigner.AssignLootToPlayers(numberOfBagsToAward, bagDefinitions, sortedPairs, bagBonusCharacters, randomRollList, pairingContributions, new WorldConfigs { AllowBagBonusContribution = "Y", AllowPairingContribution = "Y", AllowRandomContribution = "Y" });
        }

        public void GenerateKeepTakeLootBags(
            ILogger logger,
            ConcurrentDictionary<Player, int> allEligiblePlayers,
            ConcurrentDictionary<Player, int> winningEligiblePlayers,
            ConcurrentDictionary<Player, int> losingEligiblePlayers,
            Realms lockingRealm,
            int zoneId,
            List<RVRRewardItem> lootOptions,
            LootChest destructionLootChest,
            LootChest orderLootChest,
            Keep_Info keep, int playersKilledInRange,
          int forceNumberBags = 0)
        {
            logger.Info($"*************************GenerateKeepTakeLootBags*************************");

            // Calculate no rewards
            if (forceNumberBags == -1)
                return;

            var bagBonus = new Dictionary<Player, RVRPlayerBagBonus>();

            try
            {
                var zone = ZoneService.GetZone_Info((ushort)zoneId);
                if (zone == null)
                {
                    logger.Warn($"Zone is null! {zoneId}");
                    return;
                }

                var applicableZones = ZoneService._Zone_Info.Where(x => x.Pairing == zone.Pairing).Select(y => y.ZoneId);
                var leadInZones = String.Join(",", applicableZones);
                logger.Warn("Lead In Zones : " + leadInZones);

                var additionalBags = CalculateAdditionalBagsDueToKills(playersKilledInRange, Program.Config.AdditionalBagKillCountStep);
                logger.Debug($"Additional Bags is now {additionalBags} - kill count");
                additionalBags += CalculateAdditionalBagsDueToEnemyRatio(winningEligiblePlayers.Count, losingEligiblePlayers.Count, Program.Config);
                logger.Debug($"Additional Bags is now {additionalBags} - winner {winningEligiblePlayers.Count}/loser ratio {losingEligiblePlayers.Count}");

                foreach (var allEligiblePlayer in allEligiblePlayers)
                {
                    if (additionalBags > 0)
                    {
                        var message =
                            $"Due to intense fighting, an additional reward cache has been unlocked. {additionalBags} bags have been discovered.";
                        (allEligiblePlayer.Key as Player).SendClientMessage($"{message}");
                    }

                    //(allEligiblePlayer.Key as Player).SendClientMessage($"players Killed in range {playersKilledInRange}");
                    //(allEligiblePlayer.Key as Player).SendClientMessage($"Additional Bags {additionalBags} - kill count");
                    //(allEligiblePlayer.Key as Player).SendClientMessage($"Additional Bags {additionalBags} - winner {winningEligiblePlayers.Count}/loser ratio {losingEligiblePlayers.Count}");
                }

                var rewardAssignments = CalculateRewardAssignments(winningEligiblePlayers, losingEligiblePlayers, forceNumberBags, leadInZones, additionalBags);

                if (rewardAssignments == null)
                {
                    logger.Warn("No reward assignments created");
                    return;
                }

                logger.Info($"Number of total rewards assigned : {rewardAssignments.Count}");

                foreach (var eligiblePlayersAllRealm in allEligiblePlayers)
                {
                    logger.Debug($"eligible : {eligiblePlayersAllRealm.Key.Name} ({eligiblePlayersAllRealm.Key.CharacterId}) {eligiblePlayersAllRealm.Key.Realm}");
                    bagBonus.Add(eligiblePlayersAllRealm.Key, CharMgr.Database.SelectObject<RVRPlayerBagBonus>($"CharacterId = {eligiblePlayersAllRealm.Key.CharacterId}"));
                }

                foreach (var bonus in bagBonus.Keys.ToList())
                {
                    bagBonus[bonus] = UpdatePlayerBagBonus(bonus.CharacterId, bonus.Name, bagBonus[bonus], Program.Config);
                }

                var bagContentSelector = new BagContentSelector(lootOptions, StaticRandom.Instance);
                var lootBagReportList = new List<KeyValuePair<Item_Info, List<Talisman>>>();

                foreach (var reward in rewardAssignments)
                {
                    if (reward.Assignee != 0)
                    {
                        logger.Debug($"Assigning reward to {reward.Assignee} => {reward.BagRarity}");

                        try
                        {
                            var assignedPlayer = Player.GetPlayer(reward.Assignee);
                            var playerItemList = (from item in assignedPlayer.ItmInterface.Items where item != null select item.Info.Entry).ToList();
                            var playerRenown = assignedPlayer.CurrentRenown.Level;
                            var playerClass = assignedPlayer.Info.CareerLine;
                            var playerRenownBand = PlayerUtil.CalculateRenownBand(playerRenown);

                            var lootDefinition = bagContentSelector.SelectBagContentForPlayer(Logger, reward, playerRenownBand, playerClass, playerItemList.ToList(), true);
                            logger.Debug($"Award to be handed out : {lootDefinition.ToString()}");
                            if (lootDefinition.IsValid())
                            {
                                logger.Debug($"{assignedPlayer.Info.Name} has received {lootDefinition.FormattedString()}");
                                logger.Debug($"{lootDefinition.ToString()}");
                                // Only distribute if loot is valid
                                var generatedLootBag = KeepLockRewardDistributor.BuildChestLootBag(lootDefinition, assignedPlayer);

                                lootBagReportList.Add(generatedLootBag);
                                switch (assignedPlayer.Realm)
                                {
                                    case Realms.REALMS_REALM_DESTRUCTION:
                                        {
                                            if (destructionLootChest == null)
                                            {
                                                logger.Warn("Destruction Loot Chest is null");
                                                MailLootBag(assignedPlayer.CharacterId, generatedLootBag, $"{zone.Name} keep take.", "Reward", "Reward");
                                            }
                                            else
                                            {
                                                destructionLootChest?.Add(assignedPlayer.CharacterId, generatedLootBag);
                                            }

                                            break;
                                        }
                                    case Realms.REALMS_REALM_ORDER:
                                        {
                                            if (orderLootChest == null)
                                            {
                                                logger.Warn("Order Loot Chest is null");
                                                MailLootBag(assignedPlayer.CharacterId, generatedLootBag, $"{zone.Name} keep take.", "Reward", "Reward");
                                            }
                                            else
                                            {
                                                orderLootChest?.Add(assignedPlayer.CharacterId, generatedLootBag);
                                            }

                                            break;
                                        }
                                }

                                RecordKeepTakeRewardHistory(logger, assignedPlayer, generatedLootBag, lockingRealm, keep);
                                var characterBagBonus = bagBonus.SingleOrDefault(x => x.Key.CharacterId == assignedPlayer.CharacterId);
                                characterBagBonus = ResetBagBonus(assignedPlayer, generatedLootBag.Key, characterBagBonus);
                                bagBonus[assignedPlayer] = characterBagBonus.Value;
                                assignedPlayer.SendClientMessage($"For your efforts, you have received a {generatedLootBag.Key.Name}. Pick up your rewards at your Warcamp.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                            }
                            else
                            {
                                logger.Warn($"{assignedPlayer.Info.Name} has received [INVALID for Player] {lootDefinition.FormattedString()}");
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Warn($"Could not locate player {reward.Assignee} {e.Message} {e.StackTrace}");
                            continue;
                        }
                    }
                }

                foreach (var bonus in bagBonus.Keys.ToList())
                {
                    try
                    {
                        var bonusToWrite = bagBonus[bonus];
                        if (bonusToWrite != null)
                        {
                            bonusToWrite.Dirty = true;
                            CharMgr.Database.SaveObject(bonusToWrite);
                            logger.Info($"Finalisation (writing) - bag Bonus {bonusToWrite.ToString()}");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn($"{ex.Message} {ex.StackTrace}");
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn($"Unexpectedexception {zoneId} {keep.KeepId} {e.Message} {e.StackTrace}");
            }
        }

        public int CalculateAdditionalBagsDueToEnemyRatio(int winningEligiblePlayers, int losingEligiblePlayers, WorldConfigs config)
        {
            if (winningEligiblePlayers <= config.AdditionalBagRatioMinimumWinners)
                return 0;
            if (losingEligiblePlayers <= config.AdditionalBagRatioMinimumLosers)
                return 0;

            if ((winningEligiblePlayers * 3) <= losingEligiblePlayers)
                return 3;

            if ((winningEligiblePlayers * 2) <= losingEligiblePlayers)
                return 2;

            if (winningEligiblePlayers <= losingEligiblePlayers)
                return 1;

            return 0;
        }

        public int CalculateAdditionalBagsDueToKills(int playersKilledInRange, double divisor)
        {
            if (playersKilledInRange < divisor)
                return 0;
            return (int)Math.Floor(playersKilledInRange / divisor);
        }

        /// <summary>
        /// Bag assigned to player, reset their bag bonus for that bag
        /// </summary>
        /// <param name="player"></param>
        /// <param name="item"></param>
        /// <param name="singleOrDefault"></param>
        private KeyValuePair<Player, RVRPlayerBagBonus> ResetBagBonus(Player player, Item_Info item,
            KeyValuePair<Player, RVRPlayerBagBonus> bagBonus)
        {
            var bagDescription = "";
            if (bagBonus.Value != null)
            {
                switch (item.Entry)
                {
                    case 9940:
                        bagBonus.Value.WhiteBag = 0;
                        bagDescription = "White";
                        break;

                    case 9941:
                        bagBonus.Value.GreenBag = 0;
                        bagDescription = "Green";
                        break;

                    case 9942:
                        bagBonus.Value.BlueBag = 0;
                        bagDescription = "Blue";
                        break;

                    case 9943:
                        bagBonus.Value.PurpleBag = 0;
                        bagDescription = "Purple";
                        break;

                    case 9980:
                        bagBonus.Value.GoldBag = 0;
                        bagDescription = "Gold";
                        break;
                }
                bagBonus.Value.Timestamp = DateTime.UtcNow;

                Logger.Debug($"Resetting bag bonus for {item.Entry} {bagDescription} ({player.CharacterId}). {bagBonus.Value.ToString()} {bagBonus.Value.Timestamp.ToShortDateString()}");
            }

            return bagBonus;
        }

        /// <summary>
        /// Add bag bonus value to each eligible player (ie increment their bonus)
        /// </summary>

        /// <param name="bonus"></param>
        public RVRPlayerBagBonus UpdatePlayerBagBonus(uint characterId, string characterName, RVRPlayerBagBonus bonus, WorldConfigs settings)
        {
            var increment = settings.EligiblePlayerBagBonusIncrement;

            if (bonus == null)
            {
                var bagBonus = new RVRPlayerBagBonus
                {
                    CharacterId = (int)characterId,
                    GoldBag = increment,
                    BlueBag = increment,
                    PurpleBag = increment,
                    GreenBag = increment,
                    WhiteBag = increment,
                    Timestamp = DateTime.UtcNow,
                    CharacterName = characterName,
                    Dirty = true
                };

                CharMgr.Database.AddObject(bagBonus);
                Logger.Debug($"Adding bag bonus for {bagBonus.ToString()} ({characterId})");
            }
            else
            {
                bonus.GoldBag += increment;
                bonus.BlueBag += increment;
                bonus.PurpleBag += increment;
                bonus.GreenBag += increment;
                bonus.WhiteBag += increment;
                bonus.Timestamp = DateTime.UtcNow;
                bonus.CharacterName = characterName;
                bonus.Dirty = true;
                Logger.Debug($"Updating bag bonus for {characterName} ({characterId})");
            }

            return bonus;
        }

        private void RecordKeepTakeRewardHistory(ILogger logger, Player assignedPlayer, KeyValuePair<Item_Info, List<Talisman>> generatedLootBag, Realms lockingRealm, Keep_Info keep)
        {
            logger.Debug($"Recording zone lock bag reward history for {assignedPlayer.Name} ({assignedPlayer.CharacterId}) {generatedLootBag.Key.Name}");
            try
            {
                var zone = ZoneService.GetZone_Info((ushort)assignedPlayer.ZoneId);
                if (zone == null)
                {
                    logger.Warn($"Zone {assignedPlayer.ZoneId} returns null");
                    return;
                }

                var item = generatedLootBag.Value[0];
                if (item == null)
                {
                    logger.Warn($"Item for {assignedPlayer.Name} returns null");
                    return;
                }
                var itemDetails = ItemService.GetItem_Info(item.Entry);
                if (itemDetails == null)
                {
                    logger.Warn($"Item {item.Entry} does not exist");
                    return;
                }

                var history = new KeepLockBagRewardHistory
                {
                    CharacterId = (int)assignedPlayer.CharacterId,
                    BagRarity = (int)generatedLootBag.Key.Entry,
                    CharacterName = assignedPlayer.Name,
                    ItemId = (int)item.Entry,
                    ItemName = itemDetails.Name,
                    LockingRealm = (int)lockingRealm,
                    ZoneId = (int)assignedPlayer.ZoneId,
                    ZoneName = zone.Name,
                    Timestamp = DateTime.UtcNow,
                    KeepId = keep.KeepId,
                    KeepName = keep.Name
                };
                WorldMgr.Database.AddObject(history);
            }
            catch (Exception e)
            {
                logger.Error($"{e.Message}{e.StackTrace}");
            }
        }

        private void MailLootBag(uint keyCharacterId, KeyValuePair<Item_Info, List<Talisman>> lootBag, string senderName, string title = "Reward", string content = "Reward")
        {
            var character = CharMgr.GetCharacter(keyCharacterId, false);
            var characterName = character?.Name;

            Character_mail mail = new Character_mail
            {
                Guid = CharMgr.GenerateMailGuid(),
                CharacterId = keyCharacterId, //CharacterId
                SenderName = senderName,
                ReceiverName = characterName,
                SendDate = (uint)TCPManager.GetTimeStamp(),
                Title = title,
                Content = content,
                Money = 0,
                Opened = false,
                CharacterIdSender = keyCharacterId
            };

            MailItem item = new MailItem(lootBag.Key.Entry, lootBag.Value, 0, 0, 1);
            if (item != null)
            {
                mail.Items.Add(item);
                CharMgr.AddMail(mail);
            }
        }

        public void MailItem(uint keyCharacterId, Item_Info itemToSend, int count, string senderName, string title = "", string content = "mail")
        {
            try
            {
                var character = CharMgr.GetCharacter(keyCharacterId, false);
                var characterName = character?.Name;

                Character_mail mail = new Character_mail
                {
                    Guid = CharMgr.GenerateMailGuid(),
                    CharacterId = keyCharacterId, //CharacterId
                    SenderName = senderName,
                    ReceiverName = characterName,
                    SendDate = (uint)TCPManager.GetTimeStamp(),
                    Title = title,
                    Content = content,
                    Money = 0,
                    Opened = false,
                    CharacterIdSender = keyCharacterId
                };

                Logger.Info($"Mail : {characterName} ({keyCharacterId}) {itemToSend.Name} {senderName}");

                MailItem item = new MailItem(itemToSend.Entry, (ushort)count);
                if (item != null)
                {
                    mail.Items.Add(item);
                    CharMgr.AddMail(mail);
                }
            }
            catch (Exception e)
            {
                Logger.Warn($"{keyCharacterId} :: {e.Message} {e.StackTrace}");
            }
        }

        /// <summary>
        /// Given lists of winning and losing players, return bag drop assignments for winners and losers.
        /// </summary>
        /// <param name="winningEligiblePlayers"></param>
        /// <param name="losingEligiblePlayers"></param>
        /// <param name="forceNumberBags"></param>
        /// <param name="leadinZones"></param>
        /// <param name="additionalBags"></param>
        /// <param name="eligiblePlayers"></param>
        /// <returns></returns>
        private List<LootBagTypeDefinition> CalculateRewardAssignments(
            ConcurrentDictionary<Player, int> winningEligiblePlayers,
            ConcurrentDictionary<Player, int> losingEligiblePlayers, int forceNumberBags, string leadinZones,
            int additionalBags)
        {
            Logger.Info($"*** Generating WINNING REALM rewards for {winningEligiblePlayers.Count} players ***");
            var rewardAssignments = GenerateBagDropAssignments(winningEligiblePlayers, forceNumberBags, leadinZones, additionalBags, 100);
            Logger.Info($"*** Generating LOSING REALM rewards for {losingEligiblePlayers.Count} players ***");
            var losingRewardAssignments = GenerateBagDropAssignments(losingEligiblePlayers, forceNumberBags, leadinZones, additionalBags, 50);
            if (rewardAssignments != null)
            {
                if (losingRewardAssignments != null)
                {
                    foreach (var losingRewardAssignment in losingRewardAssignments)
                    {
                        rewardAssignments.Add(losingRewardAssignment);
                    }
                }
                Logger.Debug($"rewardAssignments count = {rewardAssignments.Count}");
            }
            else
            {
                rewardAssignments = losingRewardAssignments;
            }

            return rewardAssignments;
        }

        public void DistributeZoneFlipBaseRewards(
            ConcurrentDictionary<Player, int> eligibleLosingRealmPlayers,
            ConcurrentDictionary<Player, int> eligibleWinningRealmPlayers,
            Realms lockingRealm,
            int baselineContribution,
            float tierRewardScale,
            List<Player> allPlayersInZone
            )
        {
            // Distribute rewards to losing players with eligibility - halve rewards.
            foreach (var losingRealmPlayer in eligibleLosingRealmPlayers)
            {
                // Scale of player contribution against the highest contributor
                double contributionScale = CalculateContributonScale(losingRealmPlayer.Value, baselineContribution);
                ZoneLockRewardDistributor.DistributeNonBagAwards(
                     losingRealmPlayer.Key,
                     PlayerUtil.CalculateRenownBand(losingRealmPlayer.Key.RenownRank),
                     (1f + contributionScale) * tierRewardScale);
            }

            // Distribute rewards to winning players with eligibility - full rewards.
            foreach (var winningRealmPlayer in eligibleWinningRealmPlayers)
            {
                double contributionScale = CalculateContributonScale(winningRealmPlayer.Value, baselineContribution);
                ZoneLockRewardDistributor.DistributeNonBagAwards(
                    winningRealmPlayer.Key,
                    PlayerUtil.CalculateRenownBand(winningRealmPlayer.Key.RenownRank),
                    (1.5f + contributionScale) * tierRewardScale);
            }

            if (allPlayersInZone != null)
            {
                foreach (var player in allPlayersInZone)
                {
                    if (player.Realm == lockingRealm)
                    {
                        // Ensure player is not in the eligible list.
                        if (eligibleWinningRealmPlayers.All(x => x.Key.CharacterId != player.CharacterId))
                        {
                            // Give player no bag, but half rewards
                            ZoneLockRewardDistributor.DistributeNonBagAwards(
                                player,
                                PlayerUtil.CalculateRenownBand(player.RenownRank),
                                0.5 * tierRewardScale);
                        }
                    }
                    else
                    {
                        // Ensure player is not in the eligible list.
                        if (eligibleLosingRealmPlayers.All(x => x.Key.CharacterId != player.CharacterId))
                        {
                            // Give player no bag, but quarter rewards
                            ZoneLockRewardDistributor.DistributeNonBagAwards(
                                player,
                                PlayerUtil.CalculateRenownBand(player.RenownRank),
                                0.25 * tierRewardScale);
                        }
                    }
                }
            }
        }
    }
}