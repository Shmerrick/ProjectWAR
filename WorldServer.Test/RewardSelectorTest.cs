using System;
using System.Collections.Generic;
using FrameWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Battlefronts.Apocalypse.Loot;
using WorldServer.World.Objects;


namespace WorldServer.Test
{
    [TestClass]
    public class RewardSelectorTest
    {
        public List<uint> SampleUserList { get; set; }
        public List<RVRRewardItem> SampleRewardList { get; set; }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        [TestInitialize]
        public void Init()
        {
            SampleUserList = new List<uint> {10, 12, 14, 16, 18, 20};
           
        }

        [TestMethod]
        public void CTOR_Null()
        {
            var rewardAssigner = new RewardAssigner(StaticRandom.Instance, null);
            Assert.IsNotNull(rewardAssigner);
        }

        [TestMethod]
        public void DetermineNumberOfAwards_Range()
        {
            var rewardAssigner = new RewardAssigner(StaticRandom.Instance, Logger);
            Assert.IsTrue(rewardAssigner.DetermineNumberOfAwards(0) == 0);
            Assert.IsTrue(rewardAssigner.DetermineNumberOfAwards(1) == 1);
            Assert.IsTrue(rewardAssigner.DetermineNumberOfAwards(2) ==1);
            Assert.IsTrue(rewardAssigner.DetermineNumberOfAwards(4) == 2);
            Assert.IsTrue(rewardAssigner.DetermineNumberOfAwards(9) == 5);
            Assert.IsTrue(rewardAssigner.DetermineNumberOfAwards(10) == 5);
            Assert.IsTrue(rewardAssigner.DetermineNumberOfAwards(11) == 7);
            Assert.IsTrue(rewardAssigner.DetermineNumberOfAwards(17) == 10);
            Assert.IsTrue(rewardAssigner.DetermineNumberOfAwards(20) == 12);
            Assert.IsTrue(rewardAssigner.DetermineNumberOfAwards(22) == 14);
            Assert.IsTrue(rewardAssigner.DetermineNumberOfAwards(33) == 21);
            Assert.IsTrue(rewardAssigner.DetermineNumberOfAwards(44) == 28);
        }

        [TestMethod]
        public void FillBagItemExists()
        {
            SampleRewardList = new List<RVRRewardItem>
            {
                new RVRRewardItem {ItemId = 1,CanAwardDuplicate = 0,Class = 2,ItemCount = 1,RRBand = 0,Rarity = 2,RewardId = 123123},
                new RVRRewardItem {ItemId = 2,CanAwardDuplicate = 0,Class = 2,ItemCount = 1,RRBand = 0,Rarity = 2,RewardId = 456465},
                new RVRRewardItem {ItemId = 3,CanAwardDuplicate = 0,Class = 2,ItemCount = 1,RRBand = 0,Rarity = 1,RewardId = 789789},
            };

            // Dont use random - turn off shuffle
            var bagContentSelector = new BagContentSelector(SampleRewardList, new Random(1) );

            var emptyBag = new LootBagTypeDefinition {Assignee = 1, BagRarity = LootBagRarity.Blue, LootBagNumber = 1};

            var playerExistingItems = new List<uint> {1, 2, 3};
            
            var filledBag = bagContentSelector.SelectBagContentForPlayer(Logger, emptyBag, 40, 6, playerExistingItems, false);

            Assert.IsFalse(filledBag.IsValid());
        }

        [TestMethod]
        public void FillBagNoClassMatch()
        {
            SampleRewardList = new List<RVRRewardItem>
            {
                new RVRRewardItem {ItemId = 1,CanAwardDuplicate = 0,Class = 2,ItemCount = 1,RRBand = 0,Rarity = 2,RewardId = 123123},
                new RVRRewardItem {ItemId = 2,CanAwardDuplicate = 0,Class = 2,ItemCount = 1,RRBand = 0,Rarity = 2,RewardId = 456465},
                new RVRRewardItem {ItemId = 3,CanAwardDuplicate = 0,Class = 2,ItemCount = 1,RRBand = 0,Rarity = 1,RewardId = 789789},
            };

            // Dont use random - turn off shuffle
            var bagContentSelector = new BagContentSelector(SampleRewardList, new Random(1));

            var emptyBag = new LootBagTypeDefinition { Assignee = 1, BagRarity = LootBagRarity.Blue, LootBagNumber = 1 };

            var playerExistingItems = new List<uint> { 1 };

            var filledBag = bagContentSelector.SelectBagContentForPlayer(Logger, emptyBag, 40, 6, playerExistingItems, false);

            Assert.IsFalse(filledBag.IsValid());
        }


        [TestMethod]
        public void FillBagNoRRBandMatch()
        {
            SampleRewardList = new List<RVRRewardItem>
            {
                new RVRRewardItem {ItemId = 1,CanAwardDuplicate = 0,Class = 2,ItemCount = 1,RRBand = 0,Rarity = 2,RewardId = 123123},
                new RVRRewardItem {ItemId = 2,CanAwardDuplicate = 0,Class = 2,ItemCount = 1,RRBand = 10,Rarity = 2,RewardId = 456465},
                new RVRRewardItem {ItemId = 3,CanAwardDuplicate = 0,Class = 2,ItemCount = 1,RRBand = 90,Rarity = 1,RewardId = 789789},
            };

            // Dont use random - turn off shuffle
            var bagContentSelector = new BagContentSelector(SampleRewardList, new Random(1));

            var emptyBag = new LootBagTypeDefinition { Assignee = 1, BagRarity = LootBagRarity.Blue, LootBagNumber = 1 };

            var playerExistingItems = new List<uint> { 1 };

            var filledBag = bagContentSelector.SelectBagContentForPlayer(Logger, emptyBag, 40, 6, playerExistingItems, false);

            Assert.IsFalse(filledBag.IsValid());
        }

        [TestMethod]
        public void FillBagNoRRBandMatchEmptyInventory()
        {
            SampleRewardList = new List<RVRRewardItem>
            {
                new RVRRewardItem {ItemId = 1,CanAwardDuplicate = 0,Class = 2,ItemCount = 1,RRBand = 0,Rarity = 2,RewardId = 123123},
                new RVRRewardItem {ItemId = 2,CanAwardDuplicate = 0,Class = 2,ItemCount = 1,RRBand = 10,Rarity = 2,RewardId = 456465},
                new RVRRewardItem {ItemId = 3,CanAwardDuplicate = 0,Class = 2,ItemCount = 1,RRBand = 90,Rarity = 1,RewardId = 789789},
            };

            // Dont use random - turn off shuffle
            var bagContentSelector = new BagContentSelector(SampleRewardList, new Random(1));

            var emptyBag = new LootBagTypeDefinition { Assignee = 1, BagRarity = LootBagRarity.Blue, LootBagNumber = 1 };

            var playerExistingItems = new List<uint>( );

            var filledBag = bagContentSelector.SelectBagContentForPlayer(Logger, emptyBag, 40, 6, playerExistingItems, false);

            Assert.IsFalse(filledBag.IsValid());
        }

        [TestMethod]
        public void FillBagNoDuplicate()
        {
            SampleRewardList = new List<RVRRewardItem>
            {
                new RVRRewardItem {ItemId = 1,CanAwardDuplicate = 0,Class = 2,ItemCount = 1,RRBand = 0,Rarity = 2,RewardId = 123123},
                new RVRRewardItem {ItemId = 2,CanAwardDuplicate = 0,Class = 0,ItemCount = 1,RRBand = 0,Rarity = 2,RewardId = 456465},
                new RVRRewardItem {ItemId = 3,CanAwardDuplicate = 0,Class = 0,ItemCount = 1,RRBand = 90,Rarity = 1,RewardId = 789789},
            };

            // Dont use random - turn off shuffle
            var bagContentSelector = new BagContentSelector(SampleRewardList, new Random(1));

            var emptyBag = new LootBagTypeDefinition { Assignee = 1, BagRarity = LootBagRarity.Blue, LootBagNumber = 1 };

            var playerExistingItems = new List<uint> {2};

            var filledBag = bagContentSelector.SelectBagContentForPlayer(Logger, emptyBag, 40, 6, playerExistingItems, false);

            Assert.IsFalse(filledBag.IsValid());
            
        }


        [TestMethod]
        public void FillBagHasItemsNoDuplicate()
        {
            SampleRewardList = new List<RVRRewardItem>
            {
                new RVRRewardItem {ItemId = 1,CanAwardDuplicate = 0,Class = 2,ItemCount = 1,RRBand = 0,Rarity = 2,RewardId = 123123},
                new RVRRewardItem {ItemId = 2,CanAwardDuplicate = 0,Class = 0,ItemCount = 1,RRBand = 0,Rarity = 2,RewardId = 456465},
                new RVRRewardItem {ItemId = 3,CanAwardDuplicate = 0,Class = 0,ItemCount = 1,RRBand = 90,Rarity = 1,RewardId = 789789},
            };

            // Dont use random - turn off shuffle
            var bagContentSelector = new BagContentSelector(SampleRewardList, new Random(1));

            var emptyBag = new LootBagTypeDefinition { Assignee = 1, BagRarity = LootBagRarity.Blue, LootBagNumber = 1 };

            var playerExistingItems = new List<uint> { 1 };

            var filledBag = bagContentSelector.SelectBagContentForPlayer(Logger, emptyBag, 40, 6, playerExistingItems, false);

            Assert.IsTrue(filledBag.IsValid());
            Assert.IsTrue(filledBag.ItemId == 2);

        }

        [TestMethod]
        public void FillBagHasItemsRarityFailure()
        {
            SampleRewardList = new List<RVRRewardItem>
            {
                new RVRRewardItem {ItemId = 1,CanAwardDuplicate = 0,Class = 2,ItemCount = 1,RRBand = 0,Rarity = 2,RewardId = 123123},
                new RVRRewardItem {ItemId = 2,CanAwardDuplicate = 0,Class = 0,ItemCount = 1,RRBand = 0,Rarity = 3,RewardId = 456465},
                new RVRRewardItem {ItemId = 3,CanAwardDuplicate = 0,Class = 0,ItemCount = 1,RRBand = 90,Rarity = 3,RewardId = 789789},
            };

            // Dont use random - turn off shuffle
            var bagContentSelector = new BagContentSelector(SampleRewardList, new Random(1));

            var emptyBag = new LootBagTypeDefinition { Assignee = 1, BagRarity = LootBagRarity.Blue, LootBagNumber = 1 };

            var playerExistingItems = new List<uint> { 1 };

            var filledBag = bagContentSelector.SelectBagContentForPlayer(Logger, emptyBag, 40, 6, playerExistingItems, false);

            Assert.IsFalse(filledBag.IsValid());

        }


        [TestMethod]
        public void FillBagHasItemsTwoChoices()
        {
            SampleRewardList = new List<RVRRewardItem>
            {
                new RVRRewardItem {ItemId = 1,CanAwardDuplicate = 0,Class = 2,ItemCount = 1,RRBand = 0,Rarity = 2,RewardId = 123123},
                new RVRRewardItem {ItemId = 2,CanAwardDuplicate = 0,Class = 0,ItemCount = 1,RRBand = 0,Rarity = 2,RewardId = 456465},
                new RVRRewardItem {ItemId = 3,CanAwardDuplicate = 0,Class = 0,ItemCount = 1,RRBand = 0,Rarity = 2,RewardId = 789789},
            };

            // Dont use random - turn off shuffle
            var bagContentSelector = new BagContentSelector(SampleRewardList, new Random(1));

            var emptyBag = new LootBagTypeDefinition { Assignee = 1, BagRarity = LootBagRarity.Blue, LootBagNumber = 1 };

            var playerExistingItems = new List<uint> { 1 };

            var filledBag = bagContentSelector.SelectBagContentForPlayer(Logger, emptyBag, 40, 6, playerExistingItems, false);

            Assert.IsTrue(filledBag.IsValid());
            Assert.IsTrue(filledBag.ItemId == 2);  //assumes SampleRewardList ordering


        }

        [TestMethod]
        public void FillBagHasNoItemsTwoChoices()
        {
            SampleRewardList = new List<RVRRewardItem>
            {
                new RVRRewardItem {ItemId = 1,CanAwardDuplicate = 0,Class = 2,ItemCount = 1,RRBand = 0,Rarity = 2,RewardId = 123123},
                new RVRRewardItem {ItemId = 2,CanAwardDuplicate = 0,Class = 0,ItemCount = 1,RRBand = 0,Rarity = 2,RewardId = 456465},
                new RVRRewardItem {ItemId = 3,CanAwardDuplicate = 0,Class = 0,ItemCount = 1,RRBand = 0,Rarity = 2,RewardId = 789789},
            };

            // Dont use random - turn off shuffle
            var bagContentSelector = new BagContentSelector(SampleRewardList, new Random(1));

            var emptyBag = new LootBagTypeDefinition { Assignee = 1, BagRarity = LootBagRarity.Blue, LootBagNumber = 1 };

            var playerExistingItems = new List<uint>();

            var filledBag = bagContentSelector.SelectBagContentForPlayer(Logger, emptyBag, 40, 6, playerExistingItems, false);

            Assert.IsTrue(filledBag.IsValid());
            Assert.IsTrue(filledBag.ItemId == 2);  //assumes SampleRewardList ordering


        }

        [TestMethod]
        public void FillBagHasNoItemsThreeChoices()
        {
            SampleRewardList = new List<RVRRewardItem>
            {
                new RVRRewardItem {ItemId = 1,CanAwardDuplicate = 0,Class =0,ItemCount = 1,RRBand = 0,Rarity = 2,RewardId = 123123},
                new RVRRewardItem {ItemId = 2,CanAwardDuplicate = 0,Class = 0,ItemCount = 1,RRBand = 0,Rarity = 2,RewardId = 456465},
                new RVRRewardItem {ItemId = 3,CanAwardDuplicate = 0,Class = 0,ItemCount = 1,RRBand = 0,Rarity = 2,RewardId = 789789},
            };

            // Dont use random - turn off shuffle
            var bagContentSelector = new BagContentSelector(SampleRewardList, new Random(1));

            var emptyBag = new LootBagTypeDefinition { Assignee = 1, BagRarity = LootBagRarity.Blue, LootBagNumber = 1 };

            var playerExistingItems = new List<uint>();

            var filledBag = bagContentSelector.SelectBagContentForPlayer(Logger, emptyBag, 40, 6, playerExistingItems, false);

            Assert.IsTrue(filledBag.IsValid());
            Assert.IsTrue(filledBag.ItemId ==1);  //assumes SampleRewardList ordering


        }

        [TestMethod]
        public void FillBagHasDuplicateItemsThreeChoices()
        {
            SampleRewardList = new List<RVRRewardItem>
            {
                new RVRRewardItem {ItemId = 1,CanAwardDuplicate = 1,Class =0,ItemCount = 1,RRBand = 0,Rarity = 2,RewardId = 123123},
                new RVRRewardItem {ItemId = 2,CanAwardDuplicate = 0,Class = 0,ItemCount = 1,RRBand = 0,Rarity = 2,RewardId = 456465},
                new RVRRewardItem {ItemId = 3,CanAwardDuplicate = 0,Class = 0,ItemCount = 1,RRBand = 0,Rarity = 2,RewardId = 789789},
            };

            // Dont use random - turn off shuffle
            var bagContentSelector = new BagContentSelector(SampleRewardList, new Random(1));

            var emptyBag = new LootBagTypeDefinition { Assignee = 1, BagRarity = LootBagRarity.Blue, LootBagNumber = 1 };

            var playerExistingItems = new List<uint> {1};

            var filledBag = bagContentSelector.SelectBagContentForPlayer(Logger, emptyBag, 40, 6, playerExistingItems, false);

            Assert.IsTrue(filledBag.IsValid());
            Assert.IsTrue(filledBag.ItemId == 1);  //assumes SampleRewardList ordering


        }

    }
}
