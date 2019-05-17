using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using WorldServer.Services.World;
using WorldServer.World.Objects;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public class LootBagBuilder : ILootBagBuilder
    {

        /// <summary>
        /// Builds the bag containing the players items.
        /// </summary>
        /// <param name="lootBag"></param>
        /// <param name="player"></param>
        /// <param name="playerRenownBand"></param>
        /// <returns></returns>
        public KeyValuePair<Item_Info, List<Talisman>> BuildChestLootBag(LootBagTypeDefinition lootBag, Player player)
        {
            var lootRewardDescription = string.Empty;
            // Get the bag item id
            var lootBagItemId = Convert.ToInt32(LootBagTypeDefinition.GetDescription(lootBag.BagRarity));
            // Get the bag item object
            var lootBagItem = ItemService.GetItem_Info((uint)lootBagItemId);
            var lootBagContents = new List<Talisman>
            {
                new Talisman(lootBag.ItemId, (byte) lootBag.ItemCount, 0, 0)
            };

            // RewardLogger.Info($"Distributing reward of {lootBagItem.Name}, containing {lootBag.ItemId} ({lootBag.ItemCount}) to {player.Name}. Result = {result}");
            
            return new KeyValuePair<Item_Info, List<Talisman>>(lootBagItem, lootBagContents);

        }

        /// <summary>
        /// Builds the bag containing the players items.
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<Item_Info, List<Talisman>> BuildChestLootBag(LootBagRarity rarity, uint itemId, Player player)
        {
            var lootRewardDescription = string.Empty;
            // Get the bag item id
            var lootBagItemId = Convert.ToInt32(LootBagTypeDefinition.GetDescription(rarity));
            // Get the bag item object
            var lootBagItem = ItemService.GetItem_Info(itemId);
            var lootBagContents = new List<Talisman>
            {
                new Talisman(itemId, (byte) 1, 0, 0)
            };

            return new KeyValuePair<Item_Info, List<Talisman>>(lootBagItem, lootBagContents);
        }
    }
}
