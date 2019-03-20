using Common;
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

        }
    }
}
