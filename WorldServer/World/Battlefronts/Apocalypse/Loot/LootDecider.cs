using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Database.World.Battlefront;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public class LootDecider
    {
        public List<RVRZoneReward> RVRZoneRewards { get; private set; }

        public LootDecider(List<RVRZoneReward> rvrZoneRewards)
        {
            RVRZoneRewards = rvrZoneRewards;
        }

        /// <summary>
        /// Given a lootbag assigned to a player, their class, RR Band and existing items, select a reward for them
        /// </summary>
        /// <param name="lootBag"></param>
        /// <param name="platerRRBand"></param>
        /// <param name="playerClass"></param>
        /// <param name="playerItems"></param>
        /// <returns></returns>
        public LootBagTypeDefinition DetermineRVRZoneReward(LootBagTypeDefinition lootBag, byte playerRRBand, int playerClass, List<Item_Info> playerItems)
        {
            // get a closer list of matching items.
            var matchingRewards = RVRZoneRewards.Where(x => x.Class == playerClass && x.RRBand == playerRRBand && lootBag.BagRarity == (LootBagRarity) x.Rarity);
            


        }
    }
}
