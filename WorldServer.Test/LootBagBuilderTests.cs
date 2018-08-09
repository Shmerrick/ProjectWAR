using System;
using System.Collections.Generic;
using GameData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldServer.World.Battlefronts.Apocalypse;

namespace WorldServer.Test
{
    [TestClass]
    public class LootBagBuilderTests
    {
        public List<KeyValuePair<uint, PlayerRewardOptions>> SamplePlayers { get; set; }
        public List<KeyValuePair<uint, PlayerRewardOptions>> SinglePlayer { get; set; }
        public List<KeyValuePair<uint, PlayerRewardOptions>> TwoPayersSameRRBand { get; set; }
        
        public Dictionary<uint, List<LootOption>> SampleLootItems { get; set; }

        [TestInitialize]
        public void Init()
        {
            SamplePlayers = new List<KeyValuePair<uint, PlayerRewardOptions>>
            {
                new KeyValuePair<uint, PlayerRewardOptions>(10, new PlayerRewardOptions {CharacterId = 102, CharacterName = "X", CharacterRealm = Realms.REALMS_REALM_DESTRUCTION, RenownBand = 10, RenownLevel = 4, ItemList = []}),
                new KeyValuePair<uint, uint>(20, 104),
                new KeyValuePair<uint, uint>(30, 106),
                new KeyValuePair<uint, uint>(40, 108),
            };

            SinglePlayer = new List<KeyValuePair<uint, PlayerRewardOptions>>
            {
                new KeyValuePair<uint, uint>(10, 999)
            };

            TwoPayersSameRRBand = new List<KeyValuePair<uint, PlayerRewardOptions>>
            {
                new KeyValuePair<uint, uint>(10, 999),
                new KeyValuePair<uint, uint>(10, 1000)
            };

            SamplePlayers = new List<KeyValuePair<uint, PlayerRewardOptions>>
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

            var sampleLootOptions3 = new List<LootOption>
            {
                new LootOption {ItemId = 998982, RenownBand = 40, WinChance = 10},
                new LootOption {ItemId = 998388, RenownBand = 50, WinChance = 12},
                new LootOption {ItemId = 998332, RenownBand = 60, WinChance = 15}
            };


            SampleLootItems = new Dictionary<uint, List<LootOption>> {{1, sampleLootOptions1}, {2, sampleLootOptions2}, {3, sampleLootOptions3} };

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

        [TestMethod]
        public void SelectLootBagWinnersPlayersNullReturnsNull()
        {
            var lootbagBuilder = new LootBagBuilder(SamplePlayers, SampleLootItems, new RandomGenerator());
            Assert.IsNull(lootbagBuilder.SelectLootBagWinners(null, SampleLootItems));
        }

        [TestMethod]
        public void SelectLootBagWinnersItemsNullReturnsNull()
        {
            var lootbagBuilder = new LootBagBuilder(SamplePlayers, SampleLootItems, new RandomGenerator());
            Assert.IsNull(lootbagBuilder.SelectLootBagWinners(SamplePlayers, null));
        }

        [TestMethod]
        public void GetPlayerCentricLootList_NonMatch()
        {
            var lootbagBuilder = new LootBagBuilder(SamplePlayers, SampleLootItems, new LockedRandomNumberGenerator(80));
            // Attempts to match RRBand 20 with items with no RRBand 20 (ie item 3)
            Assert.IsNull(lootbagBuilder.GetPlayerCentricLootList(20, SampleLootItems[3]));
        }

        [TestMethod]
        public void GetPlayerCentricLootList_Match()
        {
            var lootbagBuilder = new LootBagBuilder(SamplePlayers, SampleLootItems, new LockedRandomNumberGenerator(80));
            // Attempts to match RRBand 20 with items with an RRBand 20
            var result  = lootbagBuilder.GetPlayerCentricLootList(20, SampleLootItems[1]);

            Assert.IsTrue(result.ItemId == 208777);
            Assert.IsTrue(result.RenownBand == 20);

        }


        [TestMethod]
        public void SelectWinners_HighRoll_MatchingRRBand()
        {
            var lootbagBuilder = new LootBagBuilder(SamplePlayers, SampleLootItems, new LockedRandomNumberGenerator(11));
            var result = lootbagBuilder.SelectLootBagWinners(SinglePlayer, SampleLootItems, false);

            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void SelectWinners_EqualRoll_MatchingRRBand()
        {
            var lootbagBuilder = new LootBagBuilder(SamplePlayers, SampleLootItems, new LockedRandomNumberGenerator(10));
            var result = lootbagBuilder.SelectLootBagWinners(SinglePlayer, SampleLootItems, false);

            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void SelectWinners_LowRoll_MatchingRRBand()
        {
            var lootbagBuilder = new LootBagBuilder(SamplePlayers, SampleLootItems, new LockedRandomNumberGenerator(2));
            var result = lootbagBuilder.SelectLootBagWinners(SinglePlayer, SampleLootItems, false);

            Assert.IsTrue(result.Count == 1);
            // Player 999
            Assert.IsTrue(result.ContainsKey(999));
            // Item 208772 (even though the roll could have given 2 loot items).
            uint val = 0;
            result.TryGetValue(999, out val);
            Assert.IsTrue(val == 208772);
        }

        [TestMethod]
        public void SelectLootBagWinnersItemsHighRollNoWinners()
        {
            var lootbagBuilder = new LootBagBuilder(SamplePlayers, SampleLootItems,new LockedRandomNumberGenerator(80));
            var result = lootbagBuilder.SelectLootBagWinners(SamplePlayers, SampleLootItems, false);

            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void SelectWinners_LowRoll_TwoPlayersSameRRBand()
        {

            var lootbagBuilder = new LootBagBuilder(SamplePlayers, SampleLootItems, new LockedRandomNumberGenerator(2));
            var result = lootbagBuilder.SelectLootBagWinners(TwoPayersSameRRBand, SampleLootItems, false);

            Assert.IsTrue(result.Count == 2);
            // Player 999
            Assert.IsTrue(result.ContainsKey(999));
            Assert.IsTrue(result.ContainsKey(1000));
            uint val = 0;
            result.TryGetValue(999, out val);
            Assert.IsTrue(val == 208772);

            
            result.TryGetValue(1000, out val);
            Assert.IsTrue(val == 554654);

        }

        [TestMethod]
        public void SelectWinners_LowRoll_MultiplePlayersSameRRBand()
        {
            var duplicatedRRBandPlayers = new List<KeyValuePair<uint, uint>>
            {
                new KeyValuePair<uint, uint>(10, 102),
                new KeyValuePair<uint, uint>(10, 109),
                new KeyValuePair<uint, uint>(10, 110),
                new KeyValuePair<uint, uint>(20, 104),
                new KeyValuePair<uint, uint>(30, 106),
                new KeyValuePair<uint, uint>(40, 108),
            };

            var lootbagBuilder = new LootBagBuilder(duplicatedRRBandPlayers, SampleLootItems, new LockedRandomNumberGenerator(2));
            var result = lootbagBuilder.SelectLootBagWinners(duplicatedRRBandPlayers, SampleLootItems, false);

            Assert.IsTrue(result.Count == 3);
            // Player 999
            Assert.IsTrue(result.ContainsKey(102));
            Assert.IsTrue(result.ContainsKey(109));
            Assert.IsTrue(result.ContainsKey(108));
            uint val = 0;
            result.TryGetValue(102, out val);
            Assert.IsTrue(val == 208772);
            
            result.TryGetValue(109, out val);
            Assert.IsTrue(val == 554654);

            result.TryGetValue(108, out val);
            Assert.IsTrue(val == 998982);

        }

        [TestMethod]
        public void SelectWinners_LowRoll_MultiplePlayersSameRRBandOutOfOrder()
        {
            var duplicatedRRBandPlayers = new List<KeyValuePair<uint, uint>>
            {
                new KeyValuePair<uint, uint>(10, 102),
                new KeyValuePair<uint, uint>(30, 106),
                new KeyValuePair<uint, uint>(10, 109),
                new KeyValuePair<uint, uint>(20, 104),
                new KeyValuePair<uint, uint>(10, 110),
                new KeyValuePair<uint, uint>(40, 108),
            };

            var lootbagBuilder = new LootBagBuilder(duplicatedRRBandPlayers, SampleLootItems, new LockedRandomNumberGenerator(2));
            var result = lootbagBuilder.SelectLootBagWinners(duplicatedRRBandPlayers, SampleLootItems, false);

            Assert.IsTrue(result.Count == 3);
            // Player 999
            Assert.IsTrue(result.ContainsKey(102));
            Assert.IsTrue(result.ContainsKey(106));
            Assert.IsTrue(result.ContainsKey(108));
            uint val = 0;
            result.TryGetValue(102, out val);
            Assert.IsTrue(val == 208772);

            result.TryGetValue(106, out val);
            Assert.IsTrue(val == 454654);

            result.TryGetValue(108, out val);
            Assert.IsTrue(val == 998982);

        }

        [TestMethod]
        public void SelectWinners_LowRoll_NoEligiblePlayers()
        {
            var duplicatedRRBandPlayers = new List<KeyValuePair<uint, uint>>
            {
                new KeyValuePair<uint, uint>(10, 102),
                new KeyValuePair<uint, uint>(30, 106),
                new KeyValuePair<uint, uint>(10, 109),
                new KeyValuePair<uint, uint>(20, 104),
                new KeyValuePair<uint, uint>(10, 110)

            };

            var sampleLootOptions3 = new List<LootOption>
            {
                new LootOption {ItemId = 998982, RenownBand = 40, WinChance = 10},
                new LootOption {ItemId = 998388, RenownBand = 50, WinChance = 8},
                new LootOption {ItemId = 998332, RenownBand = 60, WinChance = 15}
            };

            var availableLootItems = new Dictionary<uint, List<LootOption>> {{1, sampleLootOptions3}};

            var lootbagBuilder = new LootBagBuilder(duplicatedRRBandPlayers, availableLootItems, new LockedRandomNumberGenerator(2));
            var result = lootbagBuilder.SelectLootBagWinners(duplicatedRRBandPlayers, availableLootItems, false);

            Assert.IsTrue(result.Count == 0);
            // Player 999
            Assert.IsFalse(result.ContainsKey(102));
            Assert.IsFalse(result.ContainsKey(106));
        }

        [TestMethod]
        public void SelectWinners_MidRoll_MultiplePlayersSameRRBandOutOfOrder()
        {
            var duplicatedRRBandPlayers = new List<KeyValuePair<uint, uint>>
            {
                new KeyValuePair<uint, uint>(10, 102),
                new KeyValuePair<uint, uint>(30, 106),
                new KeyValuePair<uint, uint>(10, 109),
                new KeyValuePair<uint, uint>(20, 104),
                new KeyValuePair<uint, uint>(10, 110),
                new KeyValuePair<uint, uint>(40, 108),
            };

            var lootbagBuilder = new LootBagBuilder(duplicatedRRBandPlayers, SampleLootItems, new LockedRandomNumberGenerator(12));
            var result = lootbagBuilder.SelectLootBagWinners(duplicatedRRBandPlayers, SampleLootItems, false);

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.ContainsKey(106));
            uint val = 0;
            result.TryGetValue(106, out val);
            Assert.IsTrue(val == 454654);
        }


        [TestMethod]
        public void SelectWinners_MidRoll_MultiplePlayers()
        {
            var duplicatedRRBandPlayers = new List<KeyValuePair<uint, uint>>
            {
                new KeyValuePair<uint, uint>(10, 102),
                new KeyValuePair<uint, uint>(30, 106),
                new KeyValuePair<uint, uint>(50, 111),
                new KeyValuePair<uint, uint>(10, 109),
                new KeyValuePair<uint, uint>(20, 104),
                new KeyValuePair<uint, uint>(10, 110),
                new KeyValuePair<uint, uint>(40, 108),
            };

            var lootbagBuilder = new LootBagBuilder(duplicatedRRBandPlayers, SampleLootItems, new LockedRandomNumberGenerator(10));
            var result = lootbagBuilder.SelectLootBagWinners(duplicatedRRBandPlayers, SampleLootItems, false);

            Assert.IsTrue(result.Count == 3);
            Assert.IsTrue(result.ContainsKey(102));
            uint val = 0;
            result.TryGetValue(102, out val);
            Assert.IsTrue(val == 208772);

            Assert.IsTrue(result.ContainsKey(106));
            result.TryGetValue(106, out val);
            Assert.IsTrue(val == 454654);

            Assert.IsTrue(result.ContainsKey(111));
            result.TryGetValue(111, out val);
            Assert.IsTrue(val == 998388);
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
