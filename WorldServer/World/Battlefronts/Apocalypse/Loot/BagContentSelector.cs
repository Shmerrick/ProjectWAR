using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Database.World.Battlefront;
using NLog;
using WorldServer.World.Objects;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public class BagContentSelector : IBagContentSelector
    {
        public List<RVRRewardItem> RVRRewards { get; private set; }
        public Random RandomGenerator { get; }
        private static readonly Logger RewardLogger = LogManager.GetLogger("RewardLogger");

        public BagContentSelector(List<RVRRewardItem> rvrRewards, Random randomGenerator)
        {
            RVRRewards = rvrRewards;
            RandomGenerator = randomGenerator;
        }

        /// <summary>
        /// Given a lootbag assigned to a player, their class, RR Band and existing items, select a reward for them
        /// </summary>
        /// <param name="lootBag"></param>
        /// <param name="playerRRBand"></param>
        /// <param name="playerClass"></param>
        /// <param name="playerItems"></param>
        /// <param name="shuffleRewards"></param>
        /// <returns></returns>
        public LootBagTypeDefinition SelectBagContentForPlayer(LootBagTypeDefinition lootBag, byte playerRRBand, int playerClass, List<uint> playerItems, bool shuffleRewards = true)
        {
            RewardLogger.Debug($"SelectBagContentForPlayer. Assignee {lootBag.Assignee} Rarity {lootBag.BagRarity} Player RR Band {playerRRBand} Class {playerClass} Shuffle {shuffleRewards} ");
            // get a closer list of matching items.
            var matchingRewards = RVRRewards.Where(x => lootBag.BagRarity == (LootBagRarity) x.Rarity);
            RewardLogger.Debug($"Matching Rewards = {matchingRewards.Count()}");
            if (matchingRewards == null)
                return lootBag;

            if (matchingRewards.Count() == 0)
                return lootBag;

            if (shuffleRewards)
            {
                matchingRewards = matchingRewards.OrderBy(a => RandomGenerator.Next()).ToList();
            }

            // Filter rewards to match the player's need
            var rrBandMatches = matchingRewards.Where(x => x.RRBand == 0 || x.RRBand == playerRRBand);
            RewardLogger.Debug($"rrBandMatches Rewards = {rrBandMatches.Count()}");
            var rrClassMatches = rrBandMatches.Where(x => x.Class == 0 || x.Class == playerClass);
            RewardLogger.Debug($"rrClassMatches Rewards = {rrClassMatches.Count()}");

            // Works but not good for unit tests
            //matchingRewards = matchingRewards.OrderBy(a => Guid.NewGuid()).ToList();

            foreach (var matchingReward in rrClassMatches)
            {
                // Does this matching reward exist in player's items? And we cannot duplicate move on.
                if (playerItems.Count(x => x == matchingReward.ItemId) > 0 && matchingReward.CanAwardDuplicate == 0)
                {
                    RewardLogger.Debug($"Duplicate found...");
                    continue;
                }
                else
                {
                    lootBag.ItemCount = (uint) matchingReward.ItemCount;
                    lootBag.ItemId = (uint) matchingReward.ItemId;
                    lootBag.RenownBand = playerRRBand;
                    break;
                }
            }

            return lootBag;
        }

      
    }
}
