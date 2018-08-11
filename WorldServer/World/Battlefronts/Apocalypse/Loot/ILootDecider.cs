using System.Collections.Generic;
using Common;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public interface ILootDecider
    {
        LootBagTypeDefinition DetermineRVRZoneReward(LootBagTypeDefinition lootBag, byte playerRRBand, int playerClass, List<Item_Info> playerItems);
        void Shuffle<T>(IList<T> list);
    }
}