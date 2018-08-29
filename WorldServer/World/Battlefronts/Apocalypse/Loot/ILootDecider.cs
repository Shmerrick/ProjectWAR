using System.Collections.Generic;
using Common;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public interface ILootDecider
    {
        LootBagTypeDefinition DetermineRVRZoneReward(LootBagTypeDefinition lootBag, byte playerRRBand, int playerClass, List<uint> playerItems, bool shuffleRewards = true);
        IList<T> Shuffle<T>(IList<T> list);
    }
}