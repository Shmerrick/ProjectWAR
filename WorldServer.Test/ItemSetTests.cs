using Common;
using FrameWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WorldServer.Test
{
    [TestClass]
    public class ItemSetTests
    {
        [TestMethod]
        public void BreakdownItemSetBonus()
        {
            var itemSet = new Item_Set();
            itemSet.BonusString = "34:86,2,1|35:5,30,0|36:80,60,0|37:76,2,1|86:10696|";

            var random = StaticRandom.Instance.Next(1, 25);

            var x =  (200 * (1 + (float)random / 100));

            var random2 = StaticRandom.Instance.Next(80, 120);

            var y = 2000 * 4 * random2/100;


        }
    }
}
