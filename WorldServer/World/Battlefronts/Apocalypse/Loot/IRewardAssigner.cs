using Common.Database.World.Battlefront;
using System.Collections.Generic;
using WorldServer.Configs;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public interface IRewardAssigner
    {
        List<LootBagTypeDefinition> AssignLootToPlayers(int numberOfBagsToAward,
            List<LootBagTypeDefinition> bagDefinitions
            , List<KeyValuePair<uint, int>> eligiblePlayers, IList<RVRPlayerBagBonus> bagBonuses,
            Dictionary<uint, int> randomRollBonuses, Dictionary<uint, int> pairingContributionBonuses, WorldConfig config);

        byte DetermineNumberOfAwards(int eligiblePlayers);

        List<LootBagTypeDefinition> DetermineBagTypes(int numberOfBags);
    }
}