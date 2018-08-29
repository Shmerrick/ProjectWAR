using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Database.World.Battlefront;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public class LootDecider : ILootDecider
    {


        public List<RVRZoneLockItemOptionReward> RVRZoneRewards { get; private set; }
        public IRandomGenerator RandomGenerator { get; set; }

        public LootDecider(List<RVRZoneLockItemOptionReward> rvrZoneRewards, IRandomGenerator randomGenerator)
        {
            RVRZoneRewards = rvrZoneRewards;
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
        /// <param name="platerRRBand"></param>
        /// <returns></returns>
        public LootBagTypeDefinition DetermineRVRZoneReward(LootBagTypeDefinition lootBag, byte playerRRBand, int playerClass, List<uint> playerItems, bool shuffleRewards = true)
        {
            // get a closer list of matching items.
            var matchingRewards = RVRZoneRewards.Where(x => x.Class == playerClass && x.RRBand == playerRRBand && lootBag.BagRarity == (LootBagRarity) x.Rarity);
            if (matchingRewards == null)
                return lootBag;

            if (matchingRewards.Count() == 0)
                return lootBag;

            if (shuffleRewards)
                matchingRewards = matchingRewards.OrderBy(a => Guid.NewGuid()).ToList();
            //matchingRewards = Shuffle(matchingRewards.ToList());



            foreach (var matchingReward in matchingRewards)
            {
                // Does this matching reward exist in player's items? And we cannot duplicate move on.
                if (playerItems.Count(x => x == matchingReward.ItemId) > 0 && matchingReward.CanAwardDuplicate == 0)
                {
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

        public IList<T> Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = RandomGenerator.Generate(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
    }
}
