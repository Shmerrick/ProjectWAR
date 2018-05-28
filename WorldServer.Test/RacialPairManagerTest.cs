using System;
using GameData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldServer.World.Battlefronts.NewDawn;

namespace WorldServer.Test
{
    [TestClass]
    public class RacialPairManagerTest
    {
        [TestMethod]
        public void UpperTierRacialManagerBuild()
        {
            var pairMgr = new UpperTierRacialPairManager();
            Assert.IsNotNull(pairMgr);
        }

        [TestMethod]
        public void UpperTierRacialManagerFindByPair()
        {
            var pairMgr = new UpperTierRacialPairManager();
            var tier2elf = pairMgr.GetByPair(GameData.Pairing.PAIRING_ELVES_DARKELVES, 2);
            Assert.IsNotNull(tier2elf);
            Assert.IsTrue(tier2elf.Pairing == Pairing.PAIRING_ELVES_DARKELVES);
            Assert.IsTrue(tier2elf.Tier == 2);
        }

        [TestMethod]
        public void RandomRaceTest()
        {
            var result1 = new RacialPairManager().GetRandomRace(GameData.Pairing.PAIRING_GREENSKIN_DWARVES);
            Assert.IsTrue(result1 != GameData.Pairing.PAIRING_GREENSKIN_DWARVES);
            var result2 = new RacialPairManager().GetRandomRace(GameData.Pairing.PAIRING_EMPIRE_CHAOS);
            Assert.IsTrue(result2 != GameData.Pairing.PAIRING_EMPIRE_CHAOS);
            var result3 = new RacialPairManager().GetRandomRace(GameData.Pairing.PAIRING_ELVES_DARKELVES);
            Assert.IsTrue(result3 != GameData.Pairing.PAIRING_ELVES_DARKELVES);
        }
    }
}
