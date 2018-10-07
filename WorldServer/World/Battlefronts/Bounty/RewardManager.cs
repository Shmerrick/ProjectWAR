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

        public IBountyManager BountyManager { get; }
        public IContributionManager ContributionManager { get; }
        public IImpactMatrixManager ImpactMatrixManager { get; }
        public IStaticWrapper StaticWrapper { get; }
        public List<RewardPlayerKill> PlayerKillRewardBand { get; }

        public RewardManager(IBountyManager bountyManager, IContributionManager contributionManager, IImpactMatrixManager impactMatrixManager, IStaticWrapper staticWrapper, List<RewardPlayerKill> playerKillRewardBand)
        {
            BountyManager = bountyManager;
            ContributionManager = contributionManager;
            ImpactMatrixManager = impactMatrixManager;
            StaticWrapper = staticWrapper;
            PlayerKillRewardBand = playerKillRewardBand;
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
       /// <param name="impacts"></param>
       /// <param name="rewardBand"></param>
       /// <param name="totalImpact"></param>
       /// <param name="insigniaChance"></param>
       /// <returns></returns>
        public ConcurrentDictionary<uint, int> GetInsigniaRewards(List<PlayerImpact> impacts, RewardPlayerKill rewardBand, int totalImpact, int insigniaChance)
        {
            var resultDictionary = new ConcurrentDictionary<uint, int>();
            foreach (var playerImpact in impacts)
            {
                var impactFraction = CalculateImpactFraction(playerImpact.ImpactValue, totalImpact);

                // Chance of insignia drop is the impact Fraction.
                if (insigniaChance < (impactFraction * 100))
                {
                    resultDictionary.TryAdd(playerImpact.CharacterId, rewardBand.CrestId);
                }
            }
            return resultDictionary;
        }

        /// <summary>
        /// Calculate the base reward for all impacters upon the target. Doesnt include player modification value.
        /// </summary>
        /// <returns>List of impacter characterId's and their reward.</returns>
        public ConcurrentDictionary<uint, Reward> GenerateBaseRewardForKill(Player victim, Player killer, int randomNumber, Dictionary<uint, Player> playerDictionary)
        {
            var characterBounty = BountyManager.GetBounty(victim.CharacterId);
            var victimContributionValue = ContributionManager.GetContributionValue(victim.CharacterId);
            var killerContributionValue = ContributionManager.GetContributionValue(killer.CharacterId);
            var impacts = ImpactMatrixManager.GetKillImpacts(victim.CharacterId);
            var totalImpact = ImpactMatrixManager.GetTotalImpact(victim.CharacterId);

            RewardLogger.Info($"Calculating Reward for killing the target {victim.Name} ({victim.CharacterId}) by {killer.Name}");
            RewardLogger.Debug($"Target Character Bounty : {characterBounty.ToString()}");
            RewardLogger.Debug($"Target Character Contribution : {victimContributionValue}");
            RewardLogger.Debug($"Impacts upon victim :");
            foreach (var impact in impacts)
            {
                RewardLogger.Debug($"++{impact.ToString()}");
            }
            
            if (totalImpact == 0)
                throw new BountyException("Total Impact == 0");

            var rewardDictionary = new ConcurrentDictionary<uint, Reward>();

            var renownBand = Reward.DetermineRenownBand(characterBounty.RenownLevel);
            var playerKillReward = PlayerKillRewardBand.Single(x => x.RenownBand == renownBand);

            // Determine who gets insignias
            var insigniaDictionary = GetInsigniaRewards(impacts, playerKillReward, totalImpact, StaticRandom.Instance.Next(100));

            foreach (var playerImpact in impacts)
            {
                var impactFraction = CalculateImpactFraction(playerImpact.ImpactValue, totalImpact);

                //int insigniaCount = 0;
                //int insigniaItemId = 0;

                //// Select type of insignia (should it be based on the victims RR??)
                //var renownBand = Reward.DetermineRenownBand(characterBounty.RenownLevel);
                //var playerKillReward = PlayerKillRewardBand.Single(x => x.RenownBand == renownBand);

                //// Chance of insignia drop is the impact Fraction.
                //if (randomNumber < (impactFraction * 100))
                //{
                //    insigniaCount = StaticRandom.Instance.Next(1, playerKillReward.CrestCount);
                //    insigniaItemId = playerKillReward.CrestId;
                //}

                

                // Add 0-50% Money to the base amount
                var moneyRandom = StaticRandom.Instance.Next(50);
                if (moneyRandom != 0)
                    playerKillReward.Money = (int) (playerKillReward.Money * (moneyRandom / 100));

                var baseRP = BASE_RP_CEILING - CalculateModifiedBountyValue(victim.BaseBountyValue, victimContributionValue) +
                             CalculateModifiedBountyValue(killer.BaseBountyValue, killerContributionValue);

                // Add 0-25% Influence to the base amount
                var influenceRandom = StaticRandom.Instance.Next(25);
                var baseInfluence = 0;
                if (influenceRandom != 0)
                    baseInfluence = (int) (baseRP * BOUNTY_BASE_INF_MODIFIER * (moneyRandom / 100));
                else
                    baseInfluence = 0;

                var insigniaCount = 0;
                if (insigniaDictionary.TryGetValue(playerImpact.CharacterId, out var insigniaItemId))
                    insigniaCount = 1;

                var reward = new Reward
                {
                    Description = $"Player {victim.Name} ({victim.CharacterId}) impacts {killer.Name} ",
                    BaseInf = (int)impactFraction + baseInfluence,
                    BaseXP = (int)(BOUNTY_BASE_XP_MODIFIER * impactFraction ) + playerKillReward.BaseXP,
                    BaseRP = (int)(BOUNTY_BASE_RP_MODIFIER * impactFraction ) + (int)baseRP,
                    InsigniaCount = insigniaCount,
                    InsigniaItemId = insigniaItemId,
                    BaseMoney = (int)(playerKillReward.Money * impactFraction),
                    RenownBand = renownBand

                };

                if (!rewardDictionary.TryAdd(playerImpact.CharacterId, reward))
                {
                    RewardLogger.Error($"Could not add reward to rewardDictionary");
                }

                RewardLogger.Debug($"Reward : [{reward.ToString()}] awarded to {playerDictionary[playerImpact.CharacterId].Name} ({playerDictionary[playerImpact.CharacterId].CharacterId})");
            }

            return rewardDictionary;
        }

        private float CalculateImpactFraction(float impactValue, float totalImpact)
        {
            return impactValue / totalImpact;
        }

        private float CalculateModifiedBountyValue(CharacterBounty characterBounty, short contributionValue)
        {
            return CalculateImpactFraction(characterBounty.BaseBountyValue,contributionValue);
        }

        private float CalculateModifiedBountyValue(int baseBounty, short contributionValue)
        {
            return baseBounty + contributionValue;
        }

        public void DistributePlayerKillRewards(Player victim, Player killer, float aaoBonus, ushort influenceId, Dictionary<uint, Player> playerDictionary)
        {
            RewardLogger.Debug($"*********** {victim.Name} killed by {killer.Name}. AAO = {aaoBonus} *******************");
            

            // TODO : Ensure the player is actually in the active battlefront.
            // List of players involved in the kill
            var playersToReward = GenerateBaseRewardForKill(victim, killer, StaticRandom.Instance.Next(1, 100), playerDictionary);

            foreach (var playerReward in playersToReward)
            {
                // reward key is the characterId
                if (playerDictionary.ContainsKey(playerReward.Key))
                {
                    var playerToBeRewarded = playerDictionary[playerReward.Key];

                    RewardLogger.Info($"Assessing rewards for {playerDictionary[playerToBeRewarded.CharacterId].Name}");

                    // Assign non-contrib/insignia rewards to player involved in the death of the victim
                    if (playerToBeRewarded.PriorityGroup != null)
                    {
                        DistributeGroupSharedRewards(
                             victim,
                            playerToBeRewarded.PriorityGroup.Members, 
                            playerReward.Value, 
                            influenceId, 
                            $"Assisting {killer.Name} in killing {victim.Name}");
                    }
                    else  // No group
                    {

                        // Get the modification value (multiplier for victim vs target)
                        var modificationValue =
                            ImpactMatrixManager.CalculateModificationValue(victim.BaseBountyValue, killer.BaseBountyValue);

                        DistributeBaseRewardsForPlayerKill(playerReward.Value, playerToBeRewarded, 1* modificationValue, influenceId, $"Deathblow to {victim.Name}");
                    }

                    // If this player is the killer (ie Deathblow), give them a different contribution.
                    if (playerReward.Key == killer.CharacterId)
                    {
                        // Give the insignia and contrib rewards to the killer.
                        DistributeDeathBlowRewardsForPlayerKill(playerReward.Value, killer, victim.Name);
                    }
                    else // An assist
                    {
                        DistributKillAssistRewardsForPlayerKill(playerReward.Value, playerToBeRewarded, victim.Name);
                    }
                }

            }

        }

        /// <summary>
        /// The killer is the player that performed the deathblow. 
        /// </summary>
        /// <param name="reward"></param>
        /// <param name="killer"></param>
        private void DistributeDeathBlowRewardsForPlayerKill(Reward reward, Player killer, string victimName)
        {

            RewardLogger.Info($"Death Blow rewards given to {killer.Name} ({killer.CharacterId})");
            DistributeInsigniaRewardForPlayerKill(reward, killer, victimName);

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
        private void DistributeGroupSharedRewards(Player victim, List<Player> groupMembers, Reward reward, ushort influenceId, string description)
        {
            float shares = groupMembers.Count;

            foreach (var groupMember in groupMembers)
            {
                // As we are giving rewards for groupmembers, these rewards should be modified by the differences between their basebounty level and that of the victims
                var modificationValue =
                    ImpactMatrixManager.CalculateModificationValue(victim.BaseBountyValue, groupMember.BaseBountyValue);
                
                RewardLogger.Info($"Awarding group member {groupMember.Name} ({groupMember.CharacterId}) group assist reward. ModificationValue= {modificationValue}");
                DistributeBaseRewardsForPlayerKill(reward, groupMember, (1f / shares)* modificationValue, influenceId, description);
                groupMember.UpdatePlayerBountyEvent((byte)ContributionDefinitions.PARTY_KILL_ASSIST);
            }
        }

        private void DistributeInsigniaRewardForPlayerKill(Reward reward, Player killer, string victimName)
        {
            if (reward.InsigniaCount > 0)
            {
                var insigniaName = ItemService.GetItem_Info((uint)reward.InsigniaItemId).Name;
                killer.ItmInterface.CreateItem(ItemService.GetItem_Info((uint)reward.InsigniaItemId), (ushort)reward.InsigniaCount);
                killer.SendClientMessage($"You have been awarded {reward.InsigniaCount} {insigniaName} for killing {victimName}");
                RewardLogger.Debug($"{killer.Name} has been awarded {reward.InsigniaCount} {insigniaName} for killing {victimName}");
            }
        }

        private void DistributKillAssistRewardsForPlayerKill(Reward reward, Player killer, string victimName)
        {
            if (reward.InsigniaCount > 0)
            {
                // 15% chance of an assist giving a WC
                if (StaticRandom.Instance.Next(100) < 15)
                {
                    var insigniaName = ItemService.GetItem_Info((uint)reward.InsigniaItemId).Name;
                    killer.ItmInterface.CreateItem(ItemService.GetItem_Info((uint)reward.InsigniaItemId), (ushort)reward.InsigniaCount);
                    killer.SendClientMessage($"You have been awarded {reward.InsigniaCount} {insigniaName} for assisting in killing {victimName}");
                    RewardLogger.Debug($"{killer.Name} has been awarded {reward.InsigniaCount} {insigniaName} for assisting in killing {victimName}");
                }
            }


            if (killer.AAOBonus > 0)
            {
                // Add contribution for this kill under AAO to the killer.
                killer.UpdatePlayerBountyEvent((byte)ContributionDefinitions.PLAYER_KILL_ASSIST_UNDER_AAO);
            }

            // If the deathblow comes while the target is near a BO
            if (killer.CurrentObjectiveFlag != null)
            {
                killer.UpdatePlayerBountyEvent((byte)ContributionDefinitions.PLAYER_KILL_ON_BO);
            }
        }

        private void DistributeBaseRewardsForPlayerKill(Reward reward, Player killer, float shareRewardScale, ushort influenceId, string description)
        {

            killer.AddXp((uint)((uint)reward.BaseXP * shareRewardScale), true, true);
            killer.AddRenown((uint)((uint)reward.BaseRP * shareRewardScale), true, RewardType.None, description);
            killer.AddInfluence((ushort)influenceId, (ushort)((ushort)reward.BaseInf * shareRewardScale));
            killer.AddMoney((uint)((uint)reward.BaseMoney * shareRewardScale));

            RewardLogger.Debug($"PlayerKill Base Reward (player:{killer.Name} ({killer.CharacterId})) ## {description} ## " +
                               $"Share Scale : {shareRewardScale} " +
                               $"XP : {reward.BaseXP}*{shareRewardScale}={reward.BaseXP * shareRewardScale} " +
                               $"RP : {reward.BaseRP}*{shareRewardScale}={reward.BaseRP * shareRewardScale} " +
                               $"INF : {reward.BaseInf}*{shareRewardScale}={reward.BaseInf * shareRewardScale} ");
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
