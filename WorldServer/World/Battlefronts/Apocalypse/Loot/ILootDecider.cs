using System.Collections.Generic;
using Common;
using NLog;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public interface IBagContentSelector
    {
        LootBagTypeDefinition SelectBagContentForPlayer(ILogger logger, LootBagTypeDefinition lootBag, byte playerRRBand, int playerClass, List<uint> playerItems, bool shuffleRewards = true);
    }
}