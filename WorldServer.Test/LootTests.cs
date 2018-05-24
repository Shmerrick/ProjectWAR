using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WorldServer.Test
{
    [TestClass]
    public class LootTests
    {
        [TestMethod]
        public void SampleTestMethod()
        {
            var lootResponse = LootsMgr.GenerateLoot(null, null, 0);

            Assert.IsNull(lootResponse);


        }
    }
}
