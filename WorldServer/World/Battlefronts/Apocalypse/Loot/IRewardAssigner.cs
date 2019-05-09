using System.Collections.Generic;
using WorldServer.World.Battlefronts.Bounty;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public interface IRewardAssigner
    {
        List<LootBagTypeDefinition> AssignLootToPlayers(int numberOfBagsToAward,
            List<LootBagTypeDefinition> bagDefinitions
            , List<KeyValuePair<uint, int>> eligiblePlayers);
        byte DetermineNumberOfAwards(int eligiblePlayers);
        List<LootBagTypeDefinition> DetermineBagTypes(int numberOfBags);
    }
}