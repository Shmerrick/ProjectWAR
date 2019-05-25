using System.Collections.Generic;
using Common.Database.World.Battlefront;
using WorldServer.Configs;
using WorldServer.World.Battlefronts.Bounty;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public interface IRewardAssigner
    {
        List<LootBagTypeDefinition> AssignLootToPlayers(int numberOfBagsToAward,
            List<LootBagTypeDefinition> bagDefinitions
            , List<KeyValuePair<uint, int>> eligiblePlayers, IList<RVRPlayerBagBonus> bagBonuses,
            Dictionary<uint, int> randomRollBonuses, Dictionary<uint, int> pairingContributionBonuses, WorldConfigs config);
        byte DetermineNumberOfAwards(int eligiblePlayers);
        List<LootBagTypeDefinition> DetermineBagTypes(int numberOfBags);
    }
}