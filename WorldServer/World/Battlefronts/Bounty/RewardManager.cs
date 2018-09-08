using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameWork;
using NLog;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.NewDawn.Rewards;

namespace WorldServer.World.Battlefronts.Bounty
{
    public class RewardManager
    {
        private static readonly Logger RewardLogger = LogManager.GetLogger("RewardLogger");

        public const float BOUNTY_BASE_RP_MODIFIER = 2.5f;
        public const float BOUNTY_BASE_XP_MODIFIER = 5.5f;
        public const float BOUNTY_BASE_INF_MODIFIER = 0.4f;
        public const float BOUNTY_BASE_MONEY_MODIFIER = 10f;

        public IBountyManager BountyManager { get; }
        public IContributionManager ContributionManager { get; }
        public IImpactMatrixManager ImpactMatrixManager { get; }
        public IStaticWrapper StaticWrapper { get; }
        public List<RewardPlayerKill> PlayerKillRewardBand { get; }

        public RewardManager(IBountyManager bountyManager, IContributionManager contributionManager, IImpactMatrixManager impactMatrixManager, IStaticWrapper staticWrapper, List<RewardPlayerKill>  playerKillRewardBand)
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
        /// Calculate the base reward for all impacters upon the target.
        /// </summary>
        /// <returns>List of impacter characterId's and their reward.</returns>
        public ConcurrentDictionary<uint, Reward> GenerateBaseRewardForKill(uint targetCharacterId, int randomNumber)
        {
            var characterBounty = BountyManager.GetBounty(targetCharacterId);
            var contributionValue = ContributionManager.GetContributionValue(targetCharacterId);
            var impacts = ImpactMatrixManager.GetKillImpacts(targetCharacterId);
            var totalImpact = ImpactMatrixManager.GetTotalImpact(targetCharacterId);

            RewardLogger.Info($"Calculating Reward for impacting {targetCharacterId} (the target)");
            RewardLogger.Debug($"Target Character Bounty : {characterBounty.ToString()}");
            RewardLogger.Debug($"Target Character Contribution : {contributionValue}");
            RewardLogger.Debug($"Impacts upon Target Character : {String.Join(",", impacts)}");

     
            if (totalImpact == 0)
                throw new BountyException("Total Impact == 0");

            var rewardDictionary = new ConcurrentDictionary<uint, Reward>();

            // return the bounty of the killed player. 
            var modifiedBountyValue = CalculateModifiedBountyValue(characterBounty, contributionValue);
            RewardLogger.Debug($"Target Modified Bounty Value : {modifiedBountyValue}");

            foreach (var playerImpact in impacts)
            {
                var impactFraction = CalculateImpactFraction(playerImpact.ImpactValue, totalImpact);

                int insigniaCount = 0;
                int insigniaItemId = 0;

                // Select type of insignia
                var renownBand = Reward.DetermineRenownBand(characterBounty.RenownLevel);
                var playerKillReward = PlayerKillRewardBand.Single(x => x.RenownBand == renownBand);

                // Chance of insignia drop is the impact Fraction.
                if (randomNumber < (impactFraction * 100))
                {
                    insigniaCount = StaticRandom.Instance.Next(1, playerKillReward.CrestCount);
                    insigniaItemId = playerKillReward.CrestId;
                }
                var reward = new Reward
                {
                    Description = $"Player {playerImpact.CharacterId} Kills {targetCharacterId} ",
                    BaseInf = (int) (BOUNTY_BASE_INF_MODIFIER * modifiedBountyValue * impactFraction * playerImpact.ModificationValue) + playerKillReward.BaseInf,
                    BaseXP = (int) (BOUNTY_BASE_XP_MODIFIER * modifiedBountyValue * impactFraction * playerImpact.ModificationValue) + playerKillReward.BaseXP,
                    BaseRP = (int) (BOUNTY_BASE_RP_MODIFIER * modifiedBountyValue * impactFraction * playerImpact.ModificationValue) + playerKillReward.BaseRP,
                    InsigniaCount = insigniaCount,
                    InsigniaItemId = insigniaItemId,
                    BaseMoney = (int) (playerKillReward.Money * impactFraction),
                    RenownBand = renownBand

                };
                rewardDictionary.TryAdd(playerImpact.CharacterId, reward);
                RewardLogger.Debug($"Reward : {reward.ToString()} applied to {playerImpact.CharacterId}");
            }

            return rewardDictionary;
        }

        private float CalculateImpactFraction(float impactValue, float totalImpact)
        {
            return impactValue / totalImpact;
        }

        private float CalculateModifiedBountyValue(CharacterBounty characterBounty, short contributionValue)
        {
            return characterBounty.BaseBountyValue + contributionValue;
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
            return $"Reward {RenownBand}. {InsigniaCount}x{InsigniaItemId}, {BaseMoney}, {BaseRP}, {BaseXP}, {BaseInf} for {Description}";
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
