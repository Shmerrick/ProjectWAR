using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using GameData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldServer.Managers.Commands;
using WorldServer.World.Battlefronts.NewDawn;

namespace WorldServer.Test
{
    [TestClass]
    public class LowerTierBattlefrontManagerTest
    {
        public LowerTierBattlefrontManager manager { get; set; }
        public LowerTierRacialPairManager RacialPairManager { get; set; }

        [TestInitialize]
        public void Setup()
        {
            manager = new LowerTierBattlefrontManager();
            RacialPairManager = new LowerTierRacialPairManager();
        }
        [TestMethod]
        public void SetInitialPairActive_ReturnsT1Emp()
        {
            manager.SetInitialPairActive();
            Assert.IsTrue(RacialPairHelper.Equals(manager.GetActivePairing(), RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 1)));
        }

       
        [TestMethod]
        public void ResetActivePairing()
        {
            manager.SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 1));
            manager.ResetActivePairings();

            Assert.IsNull(manager.GetActivePairing());

        }

        [TestMethod]
        public void ActivePairingLocated()
        {
            manager.SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 1));
            Assert.IsTrue(RacialPairHelper.Equals(manager.GetActivePairing(), RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 1)));
            Assert.IsFalse(RacialPairHelper.Equals(manager.GetActivePairing(), RacialPairManager.GetByPair(Pairing.PAIRING_GREENSKIN_DWARVES, 1)));
            Assert.IsFalse(RacialPairHelper.Equals(manager.GetActivePairing(), RacialPairManager.GetByPair(Pairing.PAIRING_ELVES_DARKELVES, 1)));

            manager.SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_GREENSKIN_DWARVES, 1));
            Assert.IsFalse(RacialPairHelper.Equals(manager.GetActivePairing(), RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 1)));
            Assert.IsTrue(RacialPairHelper.Equals(manager.GetActivePairing(), RacialPairManager.GetByPair(Pairing.PAIRING_GREENSKIN_DWARVES, 1)));
            Assert.IsFalse(RacialPairHelper.Equals(manager.GetActivePairing(), RacialPairManager.GetByPair(Pairing.PAIRING_ELVES_DARKELVES, 1)));

        }

        [TestMethod]
        public void SetActivePair_Deactivates()
        {

            manager.SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 1));

            Assert.IsFalse(RacialPairHelper.Equals(manager.GetActivePairing(), RacialPairManager.GetByPair(Pairing.PAIRING_GREENSKIN_DWARVES, 1)));
            Assert.IsFalse(RacialPairHelper.Equals(manager.GetActivePairing(), RacialPairManager.GetByPair(Pairing.PAIRING_GREENSKIN_DWARVES, 1)));
        }

        [TestMethod]
        public void SetActivePair_Activates()
        {
            manager.SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 1));
            Assert.IsTrue(RacialPairHelper.Equals(manager.GetActivePairing(), RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 1)));

            manager.SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_GREENSKIN_DWARVES, 1));
            Assert.IsTrue(RacialPairHelper.Equals(manager.GetActivePairing(), RacialPairManager.GetByPair(Pairing.PAIRING_GREENSKIN_DWARVES, 1)));

            manager.SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 1));
            Assert.IsTrue(RacialPairHelper.Equals(manager.GetActivePairing(), RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 1)));

        }

        [TestMethod]
        public void GetNextTier_ReturnsCorrectValues()
        {
            Assert.IsTrue(manager.GetNextTier(0) == 1);
            Assert.IsTrue(manager.GetNextTier(1) == 1);
            Assert.IsTrue(manager.GetNextTier(2) == 1);
            Assert.IsTrue(manager.GetNextTier(3) == 1);
            Assert.IsTrue(manager.GetNextTier(4) == 1);
            Assert.IsTrue(manager.GetNextTier(99) == 1);
        }



        [TestMethod]
        public void AdvancePairing_NoActivePairing()
        {

            var newPairing = manager.AdvancePairing();
            Assert.IsNotNull(newPairing);
            Assert.IsFalse(RacialPairHelper.Equals(newPairing, RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 1)));
        }


        [TestMethod]
        public void AdvancePairing()
        {
            // Chaos -> Elf

            manager.SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 1));

            var newPairing1 = manager.AdvancePairing();
            Assert.IsNotNull(newPairing1);
            Assert.IsFalse(RacialPairHelper.Equals(newPairing1, RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 1)));
            Assert.IsTrue(RacialPairHelper.Equals(newPairing1, RacialPairManager.GetByPair(Pairing.PAIRING_ELVES_DARKELVES, 1)));


            // Elf -> Greenskin
            manager.SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_ELVES_DARKELVES, 1));

            var newPairing2 = manager.AdvancePairing();
            Assert.IsNotNull(newPairing2);
            Assert.IsTrue(RacialPairHelper.Equals(newPairing2, RacialPairManager.GetByPair(Pairing.PAIRING_GREENSKIN_DWARVES, 1)));
            
            // Greenskin -> Chaos
            manager.SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_GREENSKIN_DWARVES, 1));

            var newPairing3 = manager.AdvancePairing();
            Assert.IsNotNull(newPairing3);
            Assert.IsTrue(RacialPairHelper.Equals(newPairing3, RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 1)));


            var newPairing4 = manager.AdvancePairing();
            Assert.IsNotNull(newPairing4);
            Assert.IsTrue(RacialPairHelper.Equals(newPairing4, RacialPairManager.GetByPair(Pairing.PAIRING_ELVES_DARKELVES, 1)));

            var newPairing5 = manager.AdvancePairing();
            Assert.IsNotNull(newPairing5);
            Assert.IsTrue(RacialPairHelper.Equals(newPairing5, RacialPairManager.GetByPair(Pairing.PAIRING_GREENSKIN_DWARVES, 1)));

            var newPairing6 = manager.AdvancePairing();
            Assert.IsNotNull(newPairing6);
            Assert.IsTrue(RacialPairHelper.Equals(newPairing6, RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 1)));


        }


    }
}
