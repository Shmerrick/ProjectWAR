using System.Collections.Generic;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public interface IRewardAssigner
    {
        List<LootBagTypeDefinition> AssignLootToPlayers(List<uint> eligiblePlayers, List<LootBagTypeDefinition> bagDefinitions);
        byte DetermineNumberOfAwards(int eligiblePlayers);
        List<LootBagTypeDefinition> DetermineBagTypes(int numberOfBags, int forceNumberRewards = 0);
    }
}