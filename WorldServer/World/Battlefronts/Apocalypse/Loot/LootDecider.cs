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
        /// <param name="platerRRBand"></param>
        /// <returns></returns>
        public LootBagTypeDefinition DetermineRVRZoneReward(LootBagTypeDefinition lootBag, byte playerRRBand, int playerClass, List<Item> playerItems)
        {
            // get a closer list of matching items.
            var matchingRewards = RVRZoneRewards.Where(x => x.Class == playerClass && x.RRBand == playerRRBand && lootBag.BagRarity == (LootBagRarity) x.Rarity);
            if (matchingRewards == null)
                return lootBag;
            
            Shuffle(matchingRewards.ToList());

            foreach (var matchingReward in matchingRewards)
            {
                // Does this matching reward exist in player's items? And we cannot duplicate move on.
                if (playerItems.Exists(x => x.Info.Entry == matchingReward.ItemId || matchingReward.CanAwardDuplicate == 0))
                {
                    continue;
                }
                else
                {
                    lootBag.ItemCount = (uint) matchingReward.ItemCount;
                    lootBag.ItemId = (uint) matchingReward.ItemId;
                    break;
                }
            }

            return lootBag;
        }

        public void Shuffle<T>(IList<T> list)
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
        }
    }
}
