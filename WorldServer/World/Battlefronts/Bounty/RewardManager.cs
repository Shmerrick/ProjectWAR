using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameWork;

namespace WorldServer.World.Battlefronts.Bounty
{
    public class RewardManager
    {
        public const float BOUNTY_BASE_RP_MODIFIER = 2.5f;
        public const float BOUNTY_BASE_XP_MODIFIER = 5.5f;
        public const float BOUNTY_BASE_INF_MODIFIER = 0.4f;
        public const float BOUNTY_BASE_MONEY_MODIFIER = 10f;
        public const int BOUNTY_ADDITIONAL_MONEY_MODIFIER = 25;



        public IBountyManager BountyManager { get; }
        public IContributionManager ContributionManager { get; }
        public IImpactMatrixManager ImpactMatrixManager { get; }

        public RewardManager(IBountyManager bountyManager, IContributionManager contributionManager, IImpactMatrixManager impactMatrixManager)
        {
            BountyManager = bountyManager;
            ContributionManager = contributionManager;
            ImpactMatrixManager = impactMatrixManager;
        }

        /// <summary>
        /// Calculate the base reward for all impacters upon the target.
        /// </summary>
        /// <returns>List of impacter characterId's and their reward.</returns>
        public ConcurrentDictionary<uint, Reward> GenerateBaseReward(uint targetCharacterId)
        {
            var characterBounty = BountyManager.GetBounty(targetCharacterId);
            var contribution = ContributionManager.GetContribution(targetCharacterId);
            var impacts = ImpactMatrixManager.GetKillImpacts(targetCharacterId);
            var totalImpact = ImpactMatrixManager.GetTotalImpact(targetCharacterId);

            if (totalImpact == 0)
                throw new BountyException("Total Impact == 0");

            var rewardDictionary = new ConcurrentDictionary<uint, Reward>();

            foreach (var playerImpact in impacts)
            {
                var impactFraction = playerImpact.ImpactValue / totalImpact;
                var baseReward = CalculateBaseReward(characterBounty, contribution);

                int insigniaCount = 0;
                int insigniaItemId = 0;
                int additionalMoneyPercentage = StaticRandom.Instance.Next(1, BOUNTY_ADDITIONAL_MONEY_MODIFIER);

                // Chance of insignia drop is the impact Fraction.
                var randomNumber = StaticRandom.Instance.Next(1, 100);
                if (randomNumber < impactFraction)
                {
                    insigniaCount = StaticRandom.Instance.Next(1, 3);
                    // Select type of insignia
                    var renownBand = Reward.DetermineRenownBand(characterBounty.RenownLevel);
                    insigniaItemId = Reward.GetInsigniaItemId(renownBand);
                }

                rewardDictionary.TryAdd(playerImpact.CharacterId, new Reward
                {
                    Description = $"Player {playerImpact.CharacterId} Kills {targetCharacterId} ",
                    BaseInf = (int) (BOUNTY_BASE_INF_MODIFIER * baseReward * impactFraction),
                    BaseXP = (int) (BOUNTY_BASE_XP_MODIFIER * baseReward * impactFraction),
                    BaseRP = (int) (BOUNTY_BASE_RP_MODIFIER * baseReward * impactFraction),
                    InsigniaCount = insigniaCount,
                    InsigniaItemId = insigniaItemId,
                    Money = (int) (BOUNTY_BASE_MONEY_MODIFIER * baseReward * impactFraction * (1 + additionalMoneyPercentage / 100))
                });
            }

            return rewardDictionary;
        }

        private float CalculateBaseReward(CharacterBounty characterBounty, uint contribution)
        {
            return characterBounty.BountyAmount + contribution;
        }
    }

    public class Reward
    {
        public string RenownBand { get; set; }
        public int InsigniaItemId { get; set; }
        public int Money { get; set; }
        public int InsigniaCount { get; set; }
        public int BaseRP { get; set; }
        public int BaseXP { get; set; }
        public int BaseInf { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return $"Reward {RenownBand}. {InsigniaCount}x{InsigniaItemId}, {Money}, {BaseRP}, {BaseXP}, {BaseInf} for {Description}";
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

        public static int GetInsigniaItemId(int renownBand)
        {
            throw new NotImplementedException();
        }
    }

    
}
