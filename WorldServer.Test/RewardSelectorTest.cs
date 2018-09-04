using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Battlefronts.Apocalypse.Loot;


namespace WorldServer.Test
{
    [TestClass]
    public class RewardSelectorTest
    {
        public List<uint> SampleUserList { get; set; }

        [TestInitialize]
        public void Init()
        {
            SampleUserList = new List<uint> {10, 12, 14, 16, 18, 20};
        }

        [TestMethod]
        public void CTOR_Null()
        {
            var rewardSelector = new RewardSelector(new RandomGenerator());
            Assert.IsNotNull(rewardSelector);
        }

        [TestMethod]
        public void DetermineNumberOfAwards_Range()
        {
            var rewardSelector = new RewardSelector(new RandomGenerator());
            Assert.IsTrue(rewardSelector.DetermineNumberOfAwards(0) == 0);
            Assert.IsTrue(rewardSelector.DetermineNumberOfAwards(1) == 1);
            Assert.IsTrue(rewardSelector.DetermineNumberOfAwards(2) == 2);
            Assert.IsTrue(rewardSelector.DetermineNumberOfAwards(4) == 4);
            Assert.IsTrue(rewardSelector.DetermineNumberOfAwards(9) == 4);
            Assert.IsTrue(rewardSelector.DetermineNumberOfAwards(10) == 6);
            Assert.IsTrue(rewardSelector.DetermineNumberOfAwards(11) == 6);
            Assert.IsTrue(rewardSelector.DetermineNumberOfAwards(39) == 12);
            Assert.IsTrue(rewardSelector.DetermineNumberOfAwards(40) == 20);
            Assert.IsTrue(rewardSelector.DetermineNumberOfAwards(140) == 20);
        }

        [TestMethod]
        public void SelectAwardedPlayers_Range()
        {
            var rewardSelector = new RewardSelector(new RandomGenerator());
            var selectedPlayers1 = rewardSelector.SelectAwardedPlayers(SampleUserList, 4);
            Assert.IsTrue(selectedPlayers1.Count == 4);
            var selectedPlayers2 = rewardSelector.SelectAwardedPlayers(SampleUserList, 1);
            Assert.IsTrue(selectedPlayers2.Count == 1);
            var selectedPlayers3 = rewardSelector.SelectAwardedPlayers(SampleUserList, 10);
            Assert.IsTrue(selectedPlayers3.Count == SampleUserList.Count);

        }

        [TestMethod]
        public void RandomisePlayerList_null()
        {
            var rewardSelector = new RewardSelector(new RandomGenerator());
            var shuffledList = rewardSelector.RandomisePlayerList(null);
            Assert.IsNull(shuffledList);

        }

        /// <summary>
        /// Test to discover and prove shuffle functionality. 
        /// </summary>
        [TestMethod, Ignore]
        public void RandomisePlayerList_ManualTest()
        {
            var rewardSelector = new RewardSelector(new RandomGenerator());
            var shuffledList = rewardSelector.RandomisePlayerList(SampleUserList);
            Assert.IsNull(shuffledList);
        }
    }
}
