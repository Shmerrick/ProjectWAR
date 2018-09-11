using System.Collections.Generic;
using Common;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public interface IBagContentSelector
    {
        LootBagTypeDefinition SelectBagContentForPlayer(LootBagTypeDefinition lootBag, byte playerRRBand, int playerClass, List<uint> playerItems, bool shuffleRewards = true);
    }
}