using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

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

            if (bagDefinitions == null)
            {
                RewardLogger.Warn("BagDefinitions is null");
                return null;
            }

            RewardLogger.Info($"Assigning loot. Number of Bags : {bagDefinitions.Count} Number of players : {eligiblePlayers.Count}");

            var bagIndex = 0;
            foreach (var selectedPlayer in eligiblePlayers)
            {
                var lootBagTypeDefinition = bagDefinitions[bagIndex];
                if (lootBagTypeDefinition == null)
                {
                    RewardLogger.Warn($"lootBagTypeDefinition (index = {bagIndex}) is null");
                    continue;
                }

                try
                {
                    lootBagTypeDefinition.Assignee = selectedPlayer;
                    RewardLogger.Debug($"Selected player {selectedPlayer} selected for reward. LootBag Id : {lootBagTypeDefinition.LootBagNumber} ({lootBagTypeDefinition.BagRarity}). Index = {bagIndex}");
                    // player assigned, go to next bag
                    bagIndex++;
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
