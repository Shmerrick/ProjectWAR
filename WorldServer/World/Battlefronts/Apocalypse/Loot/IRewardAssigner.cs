using System.Collections.Generic;
using WorldServer.World.Battlefronts.Bounty;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public interface IRewardAssigner
    {
        List<LootBagTypeDefinition> AssignLootToPlayers(ContributionManager contributionManager,
            int numberOfBagsToAward,
            List<LootBagTypeDefinition> bagDefinitions);
        byte DetermineNumberOfAwards(int eligiblePlayers);
        List<LootBagTypeDefinition> DetermineBagTypes(int numberOfBags);
    }
}