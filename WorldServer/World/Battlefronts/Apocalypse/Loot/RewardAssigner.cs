using Common.Database.World.Battlefront;
using FrameWork;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using WorldServer.Configs;
using WorldServer.World.Objects;

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
        /// <param name="pairingContributionBonuses"></param>
        /// <param name="configSettings"></param>
        /// <returns></returns>
        public List<LootBagTypeDefinition> AssignLootToPlayers(int numberOfBagsToAward,
            List<LootBagTypeDefinition> bagDefinitions, List<KeyValuePair<uint, int>> eligiblePlayers,
            IList<RVRPlayerBagBonus> bagBonuses, Dictionary<uint, int> randomRollBonuses,
            Dictionary<uint, int> pairingContributionBonuses, WorldConfigs configSettings)
        {
            var characterKeepTrackerList = new List<KeepLockTracker>();
            // Preload the tracker
            foreach (var eligiblePlayer in eligiblePlayers)
            {
                var k = new KeepLockTracker { CharacterId = eligiblePlayer.Key, ZoneContribution = eligiblePlayer.Value };
                characterKeepTrackerList.Add(k);
            }

            // Sort the bagDefinitions by rarity descending
            bagDefinitions = bagDefinitions.OrderBy(x => x.BagRarity).ToList();
            bagDefinitions.Reverse();


            Logger.Debug($"=== Pairing Contributions");
            foreach (var bonus in pairingContributionBonuses)
            {
                Logger.Debug($"{bonus.Key}:{bonus.Value}");
            }

            Logger.Debug($"Eligible Player Count = {eligiblePlayers.Count()} for maximum {numberOfBagsToAward} Bags");

            if (eligiblePlayers.Count == 0)
                return null;

            if (bagDefinitions == null)
            {
                Logger.Warn("BagDefinitions is null");
                return null;
            }

            Logger.Debug($"Assigning loot. Number of Bags : {bagDefinitions.Count} Number of players : {eligiblePlayers.Count}");

            foreach (var lootBagTypeDefinition in bagDefinitions)
            {

                var comparisonDictionary = new Dictionary<uint, int>();
                foreach (var eligiblePlayer in eligiblePlayers)
                {
                    var klt = characterKeepTrackerList.SingleOrDefault(x => x.CharacterId == eligiblePlayer.Key);
                    var randomForCharacter = 0;
                    if ((randomRollBonuses != null) && (configSettings.AllowRandomContribution == "Y"))
                    {
                        if (randomRollBonuses.ContainsKey(eligiblePlayer.Key))
                        {
                            randomForCharacter = randomRollBonuses[eligiblePlayer.Key];
                            if (klt != null) klt.RandomBonus = randomForCharacter;
                        }
                    }

                    var pairingContributionForCharacter = 0;
                    if ((pairingContributionBonuses != null) && (configSettings.AllowPairingContribution == "Y"))
                    {
                        if (pairingContributionBonuses.ContainsKey(eligiblePlayer.Key))
                        {
                            pairingContributionForCharacter = pairingContributionBonuses[eligiblePlayer.Key];
                            if (klt != null) klt.PairingBonus = pairingContributionForCharacter;
                        }
                    }


                    var characterId = eligiblePlayer.Key;
                    var bonus = bagBonuses.SingleOrDefault(x => x.CharacterId == characterId);
                    if ((bonus != null) && (configSettings.AllowBagBonusContribution == "Y"))
                    {
                        switch (lootBagTypeDefinition.BagRarity)
                        {
                            case LootBagRarity.White:
                                {
                                    comparisonDictionary.Add(characterId, eligiblePlayer.Value + bonus.WhiteBag + randomForCharacter + pairingContributionForCharacter);
                                    if (klt != null) klt.WhiteBagBonus = bonus.WhiteBag;
                                    break;
                                }
                            case LootBagRarity.Green:
                                {
                                    comparisonDictionary.Add(characterId, eligiblePlayer.Value + bonus.GreenBag + randomForCharacter + pairingContributionForCharacter);
                                    if (klt != null) klt.GreenBagBonus = bonus.GreenBag;
                                    break;
                                }
                            case LootBagRarity.Blue:
                                {
                                    comparisonDictionary.Add(characterId, eligiblePlayer.Value + bonus.BlueBag + randomForCharacter + pairingContributionForCharacter);
                                    if (klt != null) klt.BlueBagBonus = bonus.BlueBag;
                                    break;
                                }
                            case LootBagRarity.Purple:
                                {
                                    comparisonDictionary.Add(characterId, eligiblePlayer.Value + bonus.PurpleBag + randomForCharacter + pairingContributionForCharacter);
                                    if (klt != null) klt.PurpleBagBonus = bonus.PurpleBag;
                                    break;
                                }
                            case LootBagRarity.Gold:
                                {
                                    comparisonDictionary.Add(characterId, eligiblePlayer.Value + bonus.GoldBag + randomForCharacter + pairingContributionForCharacter);
                                    if (klt != null) klt.GoldBagBonus = bonus.GoldBag;
                                    break;
                                }
                        }
                    }
                    else
                    {
                        comparisonDictionary.Add(characterId, eligiblePlayer.Value + randomForCharacter + pairingContributionForCharacter);
                    }
                    //Logger.Debug($"===== Loot Assignment Bonuses : Character {characterId}, Base {eligiblePlayer.Value} Random {randomForCharacter} Pairing {pairingContributionForCharacter}");
                }

                // Sort the comparison dictionary
                var comparisonList = comparisonDictionary.OrderBy(x => x.Value).ToList();
                comparisonList.Reverse();

                foreach (var keyValuePair in comparisonList)
                {
                    Logger.Debug($"======= Post modification values for comparison : Character {keyValuePair.Key}, Value {keyValuePair.Value}");
                }

                if (comparisonList.Count > 0)
                {
                    lootBagTypeDefinition.Assignee = comparisonList[0].Key;
                    // remove this assignee from future comparisons.
                    eligiblePlayers.RemoveAll(x => x.Key == comparisonList[0].Key);
                    Logger.Info(
                        $"===== Selected player {lootBagTypeDefinition.Assignee} selected for reward. LootBag Id : {lootBagTypeDefinition.LootBagNumber} ({lootBagTypeDefinition.BagRarity}).");
                }
                else
                {
                    Logger.Info(
                        $"===== No player available for reward assignment. LootBag Id : {lootBagTypeDefinition.LootBagNumber} ({lootBagTypeDefinition.BagRarity}).");
                }

            }

            foreach (var keepLockTracker in characterKeepTrackerList)
            {
                Logger.Debug($"===== Loot Assignment Bonuses (KLT) : {keepLockTracker.ToString()}");
                if (configSettings.DebugLootRolls == "Y")
                {
                    var player = Player._Players.SingleOrDefault(x => x.CharacterId == keepLockTracker.CharacterId);
                    if (player != null) player.SendClientMessage($"{player.Name} Loot Rolls: {keepLockTracker.ToString()}");
                }
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

        public List<LootBagTypeDefinition> DetermineAdditionalBagTypes(int additionalBags)
        {
            var result = new List<LootBagTypeDefinition>();

            for (var i = 0; i < additionalBags; i++)
            {
                var rand = StaticRandom.Instance.Next(100);
                if (rand < 60)
                    result.Add(new LootBagTypeDefinition { BagRarity = LootBagRarity.Green });
                else
                {
                    if (rand < 80)
                        result.Add(new LootBagTypeDefinition { BagRarity = LootBagRarity.Blue });
                    else
                    {
                        result.Add(rand < 90
                            ? new LootBagTypeDefinition {BagRarity = LootBagRarity.Purple}
                            : new LootBagTypeDefinition {BagRarity = LootBagRarity.Gold});
                    }
                }
            }

            return result;
        }
    }
}
