using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public class RewardAssigner : IRewardAssigner
    {
        public IRandomGenerator RandomGenerator { get; set; }


        public RewardAssigner(IRandomGenerator randomGenerator)
        {
            RandomGenerator = randomGenerator;
        }

        /// <summary>
        /// For a list of player Ids, select those getting a reward, and assign a colored loot bag (reward) to them.
        /// </summary>
        /// <param name="eligiblePlayers"></param>
        /// <returns></returns>
        public List<LootBagTypeDefinition> AssignLootToPlayers(List<uint> eligiblePlayers)
        {
            var rewardSelector = new RewardSelector(RandomGenerator);
            // Randomise the players
            var randomisedPlayerList = rewardSelector.RandomisePlayerList(eligiblePlayers);
            // Determine the number of awards to give
            var numberLootBags = rewardSelector.DetermineNumberOfAwards((uint) randomisedPlayerList.Count());
            // Define the types of awards to give
            var lootBagDefinitions = new LootBagTypeDefinition().BuildLootBagTypeDefinitions(numberLootBags);

            foreach (var lootBagTypeDefinition in lootBagDefinitions)
            {
                foreach (var player in randomisedPlayerList)
                {
                    lootBagTypeDefinition.Assignee = player;
                }
            }
            return lootBagDefinitions;
        }
    }
}
