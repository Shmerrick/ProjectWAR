using FrameWork;
using GameData;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WorldServer.Services.World;

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
        public const int INSIGNIA_ITEM_ID = 208470;

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
                throw new BountyException("Total Impact == 0");

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

        /// <summary>
        /// Calculate the base reward for all impacters upon the target. Doesnt include player modification value.
        /// </summary>
        /// <returns>List of impacter characterId's and their reward.</returns>
        //public ConcurrentDictionary<uint, Reward> GenerateBaseRewardForKill(Player victim, Player killer, int randomNumber, Dictionary<uint, Player> playerDictionary, float repeatKillReward)
        //{
        //    var characterBounty = BountyManager.GetBounty(victim.CharacterId);
        //    var victimContributionValue = ContributionManager.GetContributionValue(victim.CharacterId);
        //    var killerContributionValue = ContributionManager.GetContributionValue(killer.CharacterId);
        //    var impacts = ImpactMatrixManager.GetKillImpacts(victim.CharacterId);
        //    var totalImpact = ImpactMatrixManager.GetTotalImpact(victim.CharacterId);

        //    RewardLogger.Info($"Calculating Reward for killing the target {victim.Name} ({victim.CharacterId}) by {killer.Name}");
        //    RewardLogger.Debug($"Target Character Bounty : {characterBounty.ToString()}");
        //    RewardLogger.Debug($"Target Character Contribution : {victimContributionValue}");
        //    RewardLogger.Debug($"Killer Character Contribution : {killerContributionValue}");
        //    RewardLogger.Debug($"Impacts upon victim :");
        //    foreach (var impact in impacts)
        //    {
        //        RewardLogger.Debug($"++{impact.ToString()}");
        //    }

        //    if (totalImpact == 0)
        //        throw new BountyException("Total Impact == 0");

        //    var rewardDictionary = new ConcurrentDictionary<uint, Reward>();

        //    var renownBand = Reward.DetermineRenownBand(characterBounty.RenownLevel);
        //    var playerKillReward = PlayerKillRewardBand.Single(x => x.RenownBand == renownBand);


        //    foreach (var playerImpact in impacts)
        //    {
        //        var impactFraction = CalculateImpactFraction(playerImpact.ImpactValue, totalImpact);



        //        var baseRP = BASE_RP_CEILING - (CalculateModifiedBountyValue(victim.BaseBountyValue, victimContributionValue) +
        //                                        CalculateModifiedBountyValue(killer.BaseBountyValue, killerContributionValue));

        //        RewardLogger.Debug($"Target Character Modified Bounty : {CalculateModifiedBountyValue(victim.BaseBountyValue, victimContributionValue)}");
        //        RewardLogger.Debug($"Killer Character Modified Bounty : {CalculateModifiedBountyValue(killer.BaseBountyValue, killerContributionValue)}");
        //        RewardLogger.Debug($"BaseRP : {baseRP}");



        //        var insigniaCount = 0;
        //        if (insigniaDictionary.TryGetValue(playerImpact.CharacterId, out var insigniaItemId))
        //            insigniaCount = 1;

        //        var reward = new Reward
        //        {
        //            Description = $"Player {victim.Name} ({victim.CharacterId}) impacts {killer.Name} ",
        //            BaseInf = (int)((int)impactFraction * baseInfluence * repeatKillReward),
        //            BaseXP = (int)(BOUNTY_BASE_XP_MODIFIER * impactFraction * repeatKillReward) + playerKillReward.BaseXP,
        //            // This is wrong
        //            impact and repeat should be applied to whole amount.
        //            BaseRP = (int)(BOUNTY_BASE_RP_MODIFIER * impactFraction * repeatKillReward) + (int)baseRP,
        //            InsigniaCount = insigniaCount,
        //            InsigniaItemId = insigniaItemId,
        //            BaseMoney = (int)(playerKillReward.Money * impactFraction * repeatKillReward),
        //            RenownBand = renownBand

        //        };

        //        if (!rewardDictionary.TryAdd(playerImpact.CharacterId, reward))
        //        {
        //            RewardLogger.Error($"Could not add reward to rewardDictionary");
        //        }

        //        RewardLogger.Debug(
        //            $"Reward : [{reward.ToString()}] awarded to {playerDictionary[playerImpact.CharacterId].Name} ({playerDictionary[playerImpact.CharacterId].CharacterId}) (repeatKill : {repeatKillReward})");
        //    }

        //    return rewardDictionary;
        //}

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
            RewardLogger.Info($"=============== START : {victim.Name} killed by {killer.Name}. AAO = {aaoBonus}===============");
            var repeatKillReward = GetRepeatKillModifier(victim, killer);
            RewardLogger.Trace($"+repeatKillReward={repeatKillReward}");

            // Dictionary of attackers and the impact fraction they have on the victim.
            var impactFractions = GetImpactFractionsForKill(victim, playerDictionary);

            // TODO : Ensure the player is actually in the active battlefront.
            // List of players involved in the kill
            //var playersToReward = GenerateBaseRewardForKill(victim, killer, StaticRandom.Instance.Next(1, 100), playerDictionary, repeatKillReward);

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
                            modificationValue = shareModifier * modificationValue;

                            RewardLogger.Debug($"+++ Modification Value {modificationValue} Share Modifier {shareModifier} Number Shares {shares}");
                            RewardLogger.Info($"+++ Assessing rewards for party member {member.Name} ({member.CharacterId})");

                            var xp = CalculateXpReward(playerReward.Value, modificationValue * repeatKillReward);
                            var money = CalculateMoneyReward(playerReward.Value, modificationValue * repeatKillReward);
                            var racialInfluenceModifier = CalculateRacialInfluenceModifier(member, victim);
                            var scaledRenownPoints = (int)CalculateScaledRenownPoints(modificationValue * repeatKillReward);
                            var baseRenownPoints = (int)CalculateBaseRenownPoints(member, victim);
                            var influence = CalculateInfluenceReward(playerReward.Value, modificationValue * repeatKillReward * racialInfluenceModifier);

                            DistributeBaseRewardsForPlayerKill(member, money, scaledRenownPoints, baseRenownPoints, influence, $"Party assist {killer.Name} in killing {victim.Name}", xp, influenceId);

                            RewardLogger.Info($"+++ XP:{xp}, ScaledRP:{scaledRenownPoints} BaseRP:{baseRenownPoints} Inf:{influence} Money:{money}  RacialInfluence:{racialInfluenceModifier} AAO:{aaoBonus}");
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

                        DistributeDeathBlowContributionForPlayerKill(killer, victim.Name);
                    }
                    else // An assist only.
                    {
                        
                        var hasInsigniaReward = GetInsigniaRewards(15 * repeatKillReward);
                        var insigniaName = ItemService.GetItem_Info((uint)INSIGNIA_ITEM_ID).Name;

                        if (hasInsigniaReward)
                        {
                            DistributeInsigniaReward(playerToBeRewarded, $"++ You have been awarded 1 {insigniaName} for a kill assist on {victim.Name}", 1);
                        }

                        DistributeKillAssistContributionForPlayerKill(killer);
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

        private float CalculateBaseRenownPoints(Player killer, Player victim)
        {
            var victimContributionValue = ContributionManager.GetContributionValue(victim.CharacterId);
            var killerContributionValue = ContributionManager.GetContributionValue(killer.CharacterId);

            return BASE_RP_CEILING -
                (CalculateModifiedBountyValue(victim.BaseBountyValue, victimContributionValue) +
                CalculateModifiedBountyValue(killer.BaseBountyValue, killerContributionValue));
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

            // TODO - replace with something smarter than this.
            //// If the same player kills the same victim within a short period, ignore.
            //if (victim._recentLooters.ContainsKey(killer.CharacterId) && victim._recentLooters[killer.CharacterId] > TCPManager.GetTimeStampMS())
            //{
            //    // Lowering rewards for repeat kills
            //    repeatKillReward = 0.5f;
            //}

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

            // If the deathblow comes while the target is near a BO
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
            RewardLogger.Info($"++ Kill Assist contribution given to {killer.Name} ({killer.CharacterId})");

            killer.UpdatePlayerBountyEvent((byte)ContributionDefinitions.PARTY_KILL_ASSIST);

            if (killer.AAOBonus > 0)
            {
                // Add contribution for this kill under AAO to the killer.
                killer.UpdatePlayerBountyEvent((byte)ContributionDefinitions.PLAYER_KILL_ASSIST_UNDER_AAO);
            }

            // If the deathblow comes while the target is near a BO
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

            killer.SendClientMessage(logMessage);
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
            killer.UpdatePlayerBountyEvent((byte)ContributionDefinitions.REALM_CAPTAIN_KILL);

            ushort crests = (ushort)StaticRandom.Instance.Next(10);

            killer.SendClientMessage($"Awarded {crests} Crest(s) to " + killer.Name + " for killing realm captain");
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
    }

    public enum CharacterRewardStatus
    {
        WINNING_ELIGIBLE,
        WINNING_NON_ELIGIBLE,
        LOSING_ELIGIBLE,
        LOSING_NON_ELIGIBLE
    }

}
