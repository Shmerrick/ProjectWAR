using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Common.Database.World.Battlefront;
using WorldServer.Managers;
using WorldServer.World.Battlefronts.Bounty;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    /// <summary>
    /// Responsibility : To calculate the number of rewards, select players and assign 
    /// </summary>
    public class RewardAssigner : IRewardAssigner
    {
        public Random RandomGenerator { get; set; }
        public Logger Logger { get; set; }

        public RewardAssigner(Random randomGenerator, Logger logger)
        {
            RandomGenerator = randomGenerator;
            Logger = logger;
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
                if (eligiblePlayers <= 10)
                    numberOfAwards = (byte)Math.Ceiling(eligiblePlayers / 2f);
                else
                {
                    if (eligiblePlayers <= 20)
                    {
                        numberOfAwards = (byte)Math.Ceiling(eligiblePlayers / 1.8f);
                    }
                    else
                    {
                        numberOfAwards = (byte)Math.Ceiling(eligiblePlayers / 1.6f);
                    }
                    
                }
            }
            if (eligiblePlayers < numberOfAwards)
                numberOfAwards = (byte)eligiblePlayers;

            Logger.Debug($"Number of eligible players {eligiblePlayers}, number of Awards {numberOfAwards}");

            return numberOfAwards;
        }


        public List<LootBagTypeDefinition> DetermineBagTypes(int numberOfBags)
        {
            // Define the types of bags to give
            var lootBagDefinitions = new LootBagTypeDefinition().BuildLootBagTypeDefinitions(numberOfBags);
            Logger.Debug($"Number loot bags {lootBagDefinitions.Count} to award.");

            return lootBagDefinitions;
        }

        /// <summary>
        /// Assign a bagdefinition to a player that is eligible.
        /// </summary>
        /// <param name="numberOfBagsToAward"></param>
        /// <param name="bagDefinitions"></param>
        /// <param name="eligiblePlayers">List of Character and Eligibility values</param>
        /// <param name="bagBonuses"></param>
        /// <param name="randomRollBonuses"></param>
        /// <returns></returns>
        public List<LootBagTypeDefinition> AssignLootToPlayers(int numberOfBagsToAward,
            List<LootBagTypeDefinition> bagDefinitions, List<KeyValuePair<uint, int>> eligiblePlayers,
            IList<RVRPlayerBagBonus> bagBonuses, Dictionary<uint, int> randomRollBonuses, Dictionary<uint, int> pairingContributionBonuses)
        {
            Logger.Trace($"Eligible Player Count = {eligiblePlayers.Count()} for maximum {numberOfBagsToAward} Bags");

            if (eligiblePlayers.Count == 0)
                return null;

            if (bagDefinitions == null)
            {
                Logger.Warn("BagDefinitions is null");
                return null;
            }

            Logger.Trace($"Assigning loot. Number of Bags : {bagDefinitions.Count} Number of players : {eligiblePlayers.Count}");

            foreach (var lootBagTypeDefinition in bagDefinitions)
            {
                
                var comparisonDictionary = new Dictionary<uint, int>();
                foreach (var eligiblePlayer in eligiblePlayers)
                {
                    var randomForCharacter = 0;
                    if (randomRollBonuses != null)
                    {
                        if (randomRollBonuses.ContainsKey(eligiblePlayer.Key))
                        {
                            randomForCharacter = randomRollBonuses[eligiblePlayer.Key];
                        }
                    }

                    var pairingContributionForCharacter = 0;
                    if (pairingContributionBonuses != null)
                    {
                        if (pairingContributionBonuses.ContainsKey(eligiblePlayer.Key))
                        {
                            pairingContributionForCharacter = pairingContributionBonuses[eligiblePlayer.Key];
                        }
                    }

                    var characterId = eligiblePlayer.Key;
                    var bonus = bagBonuses.SingleOrDefault(x => x.CharacterId == characterId);
                    if (bonus != null)
                    {
                        switch (lootBagTypeDefinition.BagRarity)
                        {
                            case LootBagRarity.White:
                            {
                                comparisonDictionary.Add(characterId, eligiblePlayer.Value + bonus.WhiteBag+randomForCharacter+pairingContributionForCharacter);
                                break;
                            }
                            case LootBagRarity.Green:
                            {
                                comparisonDictionary.Add(characterId, eligiblePlayer.Value + bonus.GreenBag+randomForCharacter+pairingContributionForCharacter);
                                break;
                            }
                            case LootBagRarity.Blue:
                            {
                                comparisonDictionary.Add(characterId, eligiblePlayer.Value + bonus.BlueBag+randomForCharacter+pairingContributionForCharacter);
                                break;
                            }
                            case LootBagRarity.Purple:
                            {
                                comparisonDictionary.Add(characterId, eligiblePlayer.Value + bonus.PurpleBag+randomForCharacter+pairingContributionForCharacter);
                                break;
                            }
                            case LootBagRarity.Gold:
                            {
                                comparisonDictionary.Add(characterId, eligiblePlayer.Value + bonus.GoldBag+randomForCharacter+pairingContributionForCharacter);
                                break;
                            }
                        }
                    }
                    else
                    {
                        comparisonDictionary.Add(characterId,  eligiblePlayer.Value+randomForCharacter+pairingContributionForCharacter);
                    }
                    Logger.Debug($"===== Loot Assignment Bonuses : Character {characterId}, Base {eligiblePlayer.Value} Random {randomForCharacter} Pairing {pairingContributionForCharacter}");
                }
                // Sort the comparison dictionary
                var comparisonList = comparisonDictionary.OrderBy(x => x.Value).ToList();
                comparisonList.Reverse();

                foreach (var keyValuePair in comparisonList)
                {
                    Logger.Debug($"======= Post modification values for comparison : Character {keyValuePair.Key}, Value {keyValuePair.Value}");
                }

                lootBagTypeDefinition.Assignee = comparisonList[0].Key;
                // remove this assignee from future comparisons.
                eligiblePlayers.RemoveAll(x=>x.Key==comparisonList[0].Key);
                Logger.Info(
                    $"===== Selected player {lootBagTypeDefinition.Assignee} selected for reward. LootBag Id : {lootBagTypeDefinition.LootBagNumber} ({lootBagTypeDefinition.BagRarity}).");
                
            }
            return bagDefinitions;
        }

        public int GetNumberOfBagsToAward(int forceNumberBags, List<KeyValuePair<uint, int>> allContributingPlayers)
        {
            if (forceNumberBags != 0)
                Logger.Debug($"forceNumberBags = {forceNumberBags}");

            // Force the number of bags to hand out.
            var numberOfBags = forceNumberBags;
            if (forceNumberBags == 0)
                numberOfBags = (int)DetermineNumberOfAwards(allContributingPlayers.Count());

            return numberOfBags;
        }
    }
}
