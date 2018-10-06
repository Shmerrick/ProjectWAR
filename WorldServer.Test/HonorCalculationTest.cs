using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WorldServer.Test
{
    [TestClass]
    public class HonorCalculationTest
    {
        [TestMethod]
        public void ZeroReturnsZero()
        {
            var honor = new Common.HonorCalculation();
            Assert.IsTrue(honor.GetHonorLevel(0) == 0);
        }
        [TestMethod]
        public void OneHunderedReturnsZero()
        {
            var honor = new Common.HonorCalculation();
            Assert.IsTrue(honor.GetHonorLevel(100) == 0);
        }
        [TestMethod]
        public void OneHunderedAndOneReturnsZero()
        {
            var honor = new Common.HonorCalculation();
            Assert.IsTrue(honor.GetHonorLevel(101) == 1);
        }
        [TestMethod]
        public void BigNumberReturnsTen()
        {
            var honor = new Common.HonorCalculation();
            Assert.IsTrue(honor.GetHonorLevel(122201) == 10);
        }
        [TestMethod]
        public void TwoHundredReturnsTwo()
        {
            var honor = new Common.HonorCalculation();
            Assert.IsTrue(honor.GetHonorLevel(201) == 2);
        }
    }
}
