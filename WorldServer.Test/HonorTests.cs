using System.Collections.Concurrent;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Common;
using FrameWork;
using NLog;
using WorldServer.World.Battlefronts.Apocalypse.Loot;
using WorldServer.World.Battlefronts.Bounty;

namespace WorldServer.Test
{
    [TestClass]
    public class HonorTests
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        [TestMethod]
        public void Rank0PercentTest()
        {
            Assert.IsTrue(HonorCalculation.CalculateRank0Percent(0) == 0);
            Assert.IsTrue(HonorCalculation.CalculateRank0Percent(1) == 0);
            Assert.IsTrue(HonorCalculation.CalculateRank0Percent(100) == 10);
            Assert.IsTrue(HonorCalculation.CalculateRank0Percent(256) == 20);
            Assert.IsTrue(HonorCalculation.CalculateRank0Percent(999) == 100);

        }

        [TestMethod]
        public void Rank1PercentTest()
        {
            Assert.IsTrue(HonorCalculation.CalculateRank1Percent(1000) == 0);
            Assert.IsTrue(HonorCalculation.CalculateRank1Percent(1001) == 0);
            Assert.IsTrue(HonorCalculation.CalculateRank1Percent(1200) == 20);
            Assert.IsTrue(HonorCalculation.CalculateRank1Percent(1500) == 50);
            Assert.IsTrue(HonorCalculation.CalculateRank1Percent(1999) == 100);
        }

        [TestMethod]
        public void Rank2PercentTest()
        {
            Assert.IsTrue(HonorCalculation.CalculateRank2Percent(2000) == 0);
            Assert.IsTrue(HonorCalculation.CalculateRank2Percent(2200) == 10);
            Assert.IsTrue(HonorCalculation.CalculateRank2Percent(3000) == 50);
            Assert.IsTrue(HonorCalculation.CalculateRank2Percent(3999) == 100);

            var x= StaticRandom.Instance.Next(2);
            var y = StaticRandom.Instance.Next(2);
            var z = StaticRandom.Instance.Next(2);


        }

    }


}
