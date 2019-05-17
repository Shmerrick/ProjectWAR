
using System.Collections.Generic;
using Common;
using WorldServer.World.Objects;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public interface ILootBagBuilder
    {
        KeyValuePair<Item_Info, List<Talisman>> BuildChestLootBag(LootBagTypeDefinition lootBag, Player player);
        KeyValuePair<Item_Info, List<Talisman>> BuildChestLootBag(LootBagRarity rarity, uint itemId, Player player);
    }
}
