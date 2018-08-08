using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldServer.World.Battlefronts.Apocalypse;

namespace WorldServer.Test
{
    [TestClass]
    public class LootBagBuilderTests
    {
        public List<KeyValuePair<uint, uint>> SamplePlayers { get; set; }
        public Dictionary<uint, List<LootOption>> SampleLootItems { get; set; }

        [TestInitialize]
        public void Init()
        {
            SamplePlayers = new List<KeyValuePair<uint, uint>>
            {
                new KeyValuePair<uint, uint>(10, 102),
                new KeyValuePair<uint, uint>(20, 104),
                new KeyValuePair<uint, uint>(30, 106),
                new KeyValuePair<uint, uint>(40, 108),
            };


            var sampleLootOptions1 = new List<LootOption>
            {
                new LootOption {ItemId = 208772, RenownBand = 10, WinChance = 10},
                new LootOption {ItemId = 208777, RenownBand = 20, WinChance = 8},
                new LootOption {ItemId = 208774, RenownBand = 30, WinChance = 10}
            };

            var sampleLootOptions2 = new List<LootOption>
            {
                new LootOption {ItemId = 554654, RenownBand = 10, WinChance = 10},
                new LootOption {ItemId = 455646, RenownBand = 20, WinChance = 8},
                new LootOption {ItemId = 454654, RenownBand = 30, WinChance = 15}
            };


            SampleLootItems = new Dictionary<uint, List<LootOption>> {{1, sampleLootOptions1}, {2, sampleLootOptions2} };

        }

        [TestMethod]
        public void RandomisePlayerList_null()
        {
            var lootbagBuilder = new LootBagBuilder(SamplePlayers, SampleLootItems, new LockedRandomNumberGenerator(10));
            var shuffledList = lootbagBuilder.RandomisePlayerList(null);
            Assert.IsNull(shuffledList);

        }

        /// <summary>
        /// Test to discover and prove shuffle functionality. 
        /// </summary>
        [TestMethod]
        public void RandomisePlayerList_ManualTest()
        {
            var lootbagBuilder = new LootBagBuilder(SamplePlayers, SampleLootItems, new RandomGenerator());
            var shuffledList = lootbagBuilder.RandomisePlayerList(SamplePlayers);


        }

    }

    public class LockedRandomNumberGenerator : IRandomGenerator
    {
        public int OverrideNumber { get; set; }


        public LockedRandomNumberGenerator(int overrideNumber)
        {
            OverrideNumber = overrideNumber;
        }

        public int Generate(int max)
        {
            return OverrideNumber;
        }
    }


}
