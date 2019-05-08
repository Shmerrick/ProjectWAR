using System;
using System.Collections.Generic;
using FrameWork;
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
            var rewardAssigner = new RewardAssigner(StaticRandom.Instance, null);
            Assert.IsNotNull(rewardAssigner);
        }

        [TestMethod]
        public void DetermineNumberOfAwards_Range()
        {
            var rewardAssigner = new RewardAssigner(StaticRandom.Instance, null);
            Assert.IsTrue(rewardAssigner.DetermineNumberOfAwards(0) == 0);
            Assert.IsTrue(rewardAssigner.DetermineNumberOfAwards(1) == 1);
            Assert.IsTrue(rewardAssigner.DetermineNumberOfAwards(2) == 2);
            Assert.IsTrue(rewardAssigner.DetermineNumberOfAwards(4) == 4);
            Assert.IsTrue(rewardAssigner.DetermineNumberOfAwards(9) == 4);
            Assert.IsTrue(rewardAssigner.DetermineNumberOfAwards(10) == 6);
            Assert.IsTrue(rewardAssigner.DetermineNumberOfAwards(11) == 6);
            Assert.IsTrue(rewardAssigner.DetermineNumberOfAwards(39) == 12);
            Assert.IsTrue(rewardAssigner.DetermineNumberOfAwards(40) == 20);
            Assert.IsTrue(rewardAssigner.DetermineNumberOfAwards(140) == 20);
        }


     
    }
}
