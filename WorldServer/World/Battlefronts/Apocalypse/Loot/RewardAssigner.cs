using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameWork;
using NLog;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    /// <summary>
    /// Responsibility : To calculate the number of rewards, select players and assign 
    /// </summary>
    public class RewardAssigner : IRewardAssigner
    {
        public Random RandomGenerator { get; set; }

        private static readonly Logger RewardLogger = LogManager.GetLogger("RewardLogger");
        public RewardAssigner(Random randomGenerator)
        {
            RandomGenerator = randomGenerator;
        }

        /// <summary>
        /// Increase the number of possible awards based upon the number of eligible Players.
        /// </summary>
        /// <param name="eligiblePlayers"></param>
        /// <returns></returns>
        public byte DetermineNumberOfAwards(int eligiblePlayers)
        {
            byte numberOfAwards = 0;
            // Simple set for low pop for now. TODO base this upon population sizes and % chance to win a bag per flip.
            if (eligiblePlayers == 0)
                numberOfAwards = 0;
            else
            {
                if (eligiblePlayers < 10)
                    numberOfAwards = 4;
                else
                {
                    if (eligiblePlayers < 20)
                        numberOfAwards = 6;
                    else
                    {
                        numberOfAwards = (byte)(eligiblePlayers < 40 ? 12 : 20);
                    }
                }
            }
            if (eligiblePlayers < numberOfAwards)
                numberOfAwards = (byte)eligiblePlayers;

            return numberOfAwards;
        }


        public List<LootBagTypeDefinition> DetermineBagTypes(int numberOfBags, int forceNumberRewards = 0)
        {
            int numberLootBags = 0;
            // Determine the number of awards to give - allowing for overrides.
            numberLootBags = forceNumberRewards == 0 ? numberOfBags : forceNumberRewards;

            // Define the types of bags to give
            var lootBagDefinitions = new LootBagTypeDefinition().BuildLootBagTypeDefinitions(numberLootBags);
            RewardLogger.Debug($"Number loot bags {lootBagDefinitions.Count} to award.");

            return lootBagDefinitions;
        }

        /// <summary>
        /// For a list of player Ids, select those getting a reward, and assign a colored loot bag (reward) to them.
        /// </summary>
        /// <param name="eligiblePlayers"></param>
        /// <param name="bagDefinitions"></param>
        /// <returns></returns>
        public List<LootBagTypeDefinition> AssignLootToPlayers(List<uint> eligiblePlayers, List<LootBagTypeDefinition> bagDefinitions)
        {
            if (eligiblePlayers.Count == 0)
                return null;

            foreach (var lootBagTypeDefinition in bagDefinitions)
            {
                try
                {
                    var selectedPlayer = eligiblePlayers[RandomGenerator.Next((eligiblePlayers.Count))];
                    lootBagTypeDefinition.Assignee = selectedPlayer;
                    RewardLogger.Debug($"Selected player {selectedPlayer} {eligiblePlayers.Count} for reward");
                }
                catch (Exception e)
                {
                    RewardLogger.Warn($"{e.Message}");
                }
            }
            return bagDefinitions;
        }
    }
}
