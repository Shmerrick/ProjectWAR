using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Database.World.Battlefront;
using FrameWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Battlefronts.Apocalypse.Loot;
using WorldServer.World.Objects;

namespace WorldServer.Test
{
    [TestClass]
    public class LootDeciderTests
    {
        public List<RVRRewardItem> SampleZoneItemOptions { get; set; }
        public List<uint> SamplePlayerItems { get; set; }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [TestInitialize]
        public void Init()
        {
            SamplePlayerItems = new List<uint> {1, 2, 3, 4, 5, 6, 100, 200};

            SampleZoneItemOptions = new List<RVRRewardItem>
            {
                new RVRRewardItem
                {
                    ItemId = 100,
                    ItemCount = 1,
                    CanAwardDuplicate = 0,
                    Class = 64,
                    RRBand = 10,
                    Rarity = (int) LootBagRarity.Gold,
                    RewardId = 1
                },
                new RVRRewardItem
                {
                    ItemId = 200,
                    ItemCount = 1,
                    CanAwardDuplicate = 0,
                    Class = 128,
                    RRBand = 10,
                    Rarity = (int) LootBagRarity.Green,
                    RewardId = 2
                },
                new RVRRewardItem
                {
                    ItemId = 300,
                    ItemCount = 1,
                    CanAwardDuplicate = 0,
                    Class = 64,
                    RRBand = 50,
                    Rarity = (int) LootBagRarity.Blue,
                    RewardId = 3
                },
                new RVRRewardItem
                {
                    ItemId = 400,
                    ItemCount = 2,
                    CanAwardDuplicate = 0,
                    Class = 64,
                    RRBand = 10,
                    Rarity = (int) LootBagRarity.Purple,
                    RewardId = 4
                },
                new RVRRewardItem
                {
                    ItemId = 500,
                    ItemCount = 1,
                    CanAwardDuplicate = 0,
                    Class = 128,
                    RRBand = 20,
                    Rarity = (int) LootBagRarity.Purple,
                    RewardId = 5
                },
                new RVRRewardItem
                {
                    ItemId = 301,
                    ItemCount = 1,
                    CanAwardDuplicate = 0,
                    Class = 64,
                    RRBand = 10,
                    Rarity = (int) LootBagRarity.Blue,
                    RewardId = 6
                },
                new RVRRewardItem
                {
                    ItemId = 302,
                    ItemCount = 1,
                    CanAwardDuplicate = 0,
                    Class = 64,
                    RRBand = 10,
                    Rarity = (int) LootBagRarity.Blue,
                    RewardId = 7
                },
                new RVRRewardItem
                {
                    ItemId = 303,
                    ItemCount = 1,
                    CanAwardDuplicate = 0,
                    Class = 64,
                    RRBand = 10,
                    Rarity = (int) LootBagRarity.Blue,
                    RewardId = 8
                }
                ,
                new RVRRewardItem
                {
                    ItemId = 601,
                    ItemCount = 1,
                    CanAwardDuplicate = 1,
                    Class = 64,
                    RRBand = 20,
                    Rarity = (int) LootBagRarity.Blue,
                    RewardId = 9
                },
                new RVRRewardItem
                {
                    ItemId = 602,
                    ItemCount = 1,
                    CanAwardDuplicate = 1,
                    Class = 64,
                    RRBand = 20,
                    Rarity = (int) LootBagRarity.Blue,
                    RewardId = 10
                },
                new RVRRewardItem
                {
                    ItemId = 603,
                    ItemCount = 1,
                    CanAwardDuplicate = 1,
                    Class = 64,
                    RRBand = 20,
                    Rarity = (int) LootBagRarity.Blue,
                    RewardId = 11
                }
            };


        }

        public class LockedRandomNumberGenerator : Random
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

        [TestMethod]
        public void DetermineRVRZoneReward_NoMatches()
        {
            var lootDecider = new BagContentSelector(this.SampleZoneItemOptions, StaticRandom.Instance);
            var bag = new LootBagTypeDefinition();

            var result = lootDecider.SelectBagContentForPlayer(Logger,bag, 70, 64, new List<uint>());
            Assert.IsFalse(result.IsValid());
        }

        [TestMethod]
        public void DetermineRVRZoneReward_1Match_ValidBag()
        {
            var lootDecider = new BagContentSelector(this.SampleZoneItemOptions, new LockedRandomNumberGenerator(10));
            var bag = new LootBagTypeDefinition { BagRarity = LootBagRarity.Blue, Assignee = 1, LootBagNumber = 1 };

            // SHould match 1 record
            var result = lootDecider.SelectBagContentForPlayer(Logger,bag, 20, 64, new List<uint>(), false);
            Assert.IsTrue(result.IsValid());
            Assert.IsTrue(result.RenownBand == 20);
            Assert.IsTrue(result.ItemId == 601);
        }

        [TestMethod]
        public void DetermineRVRZoneReward_1Match_ValidBagPlayerItems()
        {
            var lootDecider = new BagContentSelector(this.SampleZoneItemOptions, new LockedRandomNumberGenerator(10));
            var bag = new LootBagTypeDefinition { BagRarity = LootBagRarity.Blue, Assignee = 1, LootBagNumber = 1 };

            // SHould match 1 record
            var result = lootDecider.SelectBagContentForPlayer(Logger,bag, 20, 64, SamplePlayerItems, false);
            Assert.IsTrue(result.IsValid());
            Assert.IsTrue(result.RenownBand == 20);
            Assert.IsTrue(result.ItemId == 601);
        }

        [TestMethod]
        public void DetermineRVRZoneReward_NMatches_ValidBag()
        {
            var lootDecider = new BagContentSelector(this.SampleZoneItemOptions, new LockedRandomNumberGenerator(10));
            var bag = new LootBagTypeDefinition { BagRarity = LootBagRarity.Blue, Assignee = 1, LootBagNumber = 1 };

            // SHould match 3 record
            var result = lootDecider.SelectBagContentForPlayer(Logger,bag, 10, 64, new List<uint>(), false);
            Assert.IsTrue(result.IsValid());
            Assert.IsTrue(result.RenownBand == 10);
            Assert.IsTrue(result.ItemId == 301);
            Assert.IsTrue(result.Assignee==1);
            Assert.IsTrue(result.LootBagNumber == 1);
        }

        [TestMethod]
        public void DetermineRVRZoneReward_ManualTest_ValidBag()
        {
            var lootDecider = new BagContentSelector(this.SampleZoneItemOptions, new LockedRandomNumberGenerator(10));
            var bag = new LootBagTypeDefinition { BagRarity = LootBagRarity.Blue, Assignee = 1, LootBagNumber = 1 };

            // SHould match 3 record
            var result = lootDecider.SelectBagContentForPlayer(Logger,bag, 10, 64, new List<uint>(), true);
            Assert.IsTrue(1==1);
        }

        [TestMethod]
        public void DetermineRVRZoneReward_NMatches_ValidBag_PlayerItemsNoMatch()
        {
            var lootDecider = new BagContentSelector(this.SampleZoneItemOptions, new LockedRandomNumberGenerator(10));
            var bag = new LootBagTypeDefinition { BagRarity = LootBagRarity.Blue, Assignee = 1, LootBagNumber = 1 };

            // SHould match 3 record
            var result = lootDecider.SelectBagContentForPlayer(Logger,bag, 10, 64, SamplePlayerItems, false);
            Assert.IsTrue(result.IsValid());
            Assert.IsTrue(result.RenownBand == 10);
            Assert.IsTrue(result.ItemId == 301);
        }

        [TestMethod]
        public void DetermineRVRZoneReward_NMatches_ValidBag_PlayerItemsMatch()
        {
            var lootDecider = new BagContentSelector(this.SampleZoneItemOptions, new LockedRandomNumberGenerator(10));
            var bag = new LootBagTypeDefinition { BagRarity = LootBagRarity.Blue, Assignee = 1, LootBagNumber = 1 };

            SamplePlayerItems.Add(301);

            // SHould match 3 record
            var result = lootDecider.SelectBagContentForPlayer(Logger,bag, 10, 64, SamplePlayerItems, false);
            Assert.IsTrue(result.IsValid());
            Assert.IsTrue(result.RenownBand == 10);
            Assert.IsTrue(result.ItemId == 302);

            SamplePlayerItems.Add(302);

            // SHould match 3 record
            var result2 = lootDecider.SelectBagContentForPlayer(Logger,bag, 10, 64, SamplePlayerItems, false);
            Assert.IsTrue(result2.IsValid());
            Assert.IsTrue(result2.RenownBand == 10);
            Assert.IsTrue(result2.ItemId == 303);
        }

        [TestMethod]
        public void DetermineRVRZoneReward_NMatches_ValidBag_PlayerItemsMatchDupeOk()
        {
            var lootDecider = new BagContentSelector(this.SampleZoneItemOptions, new LockedRandomNumberGenerator(10));
            var bag = new LootBagTypeDefinition { BagRarity = LootBagRarity.Blue, Assignee = 1, LootBagNumber = 1 };

            SamplePlayerItems.Add(301);
            SamplePlayerItems.Add(601);

            // SHould match 3 record
            var result = lootDecider.SelectBagContentForPlayer(Logger,bag, 20, 64, SamplePlayerItems, false);
            Assert.IsTrue(result.IsValid());
            Assert.IsTrue(result.RenownBand == 20);
            Assert.IsTrue(result.ItemId == 601);

            SamplePlayerItems.Add(602);

            // SHould match 3 record
            var result2 = lootDecider.SelectBagContentForPlayer(Logger,bag, 20, 64, SamplePlayerItems, false);
            Assert.IsTrue(result2.IsValid());
            Assert.IsTrue(result2.RenownBand == 20);
            Assert.IsTrue(result2.ItemId == 601);
        }
    }
}
