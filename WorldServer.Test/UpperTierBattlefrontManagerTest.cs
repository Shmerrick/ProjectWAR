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
    public class UpperTierBattlefrontManagerTest
    {
        public UpperTierBattlefrontManager manager { get; set; }
        public UpperTierRacialPairManager RacialPairManager { get; set; }

        [TestInitialize]
        public void Setup()
        {
            manager  = new UpperTierBattlefrontManager();
            RacialPairManager = new UpperTierRacialPairManager();
        }

        [TestMethod]
        public void Constructor_NoPairings_CreatesError()
        {
            Assert.IsNull(manager.GetActivePairing());
        }

        [TestMethod]
        public void Constructor_NoActivePairings_CreatesError()
        {
            
            Assert.IsNull(manager.GetActivePairing());
        }

        [TestMethod]
        public void ResetActivePairing()
        {
            
            manager.SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_ELVES_DARKELVES, 2));
            manager.ResetActivePairings();

            Assert.IsNull(manager.GetActivePairing());
        }

        [TestMethod]
        public void ActivePairingLocated()
        {

            manager.SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 2));
            Assert.IsNotNull(manager.GetActivePairing());
            Assert.IsTrue(manager.GetActivePairing().Tier == 2);
            Assert.IsTrue(manager.GetActivePairing().Pairing == GameData.Pairing.PAIRING_EMPIRE_CHAOS);
            manager.ResetActivePairings();
            manager.SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 3));
            Assert.IsNotNull(manager.GetActivePairing());
            Assert.IsTrue(manager.GetActivePairing().Tier == 3);
            Assert.IsTrue(manager.GetActivePairing().Pairing == GameData.Pairing.PAIRING_EMPIRE_CHAOS);
            manager.ResetActivePairings();
            manager.SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 4));
            Assert.IsNotNull(manager.GetActivePairing());
            Assert.IsTrue(manager.GetActivePairing().Tier == 4);
            Assert.IsTrue(manager.GetActivePairing().Pairing == GameData.Pairing.PAIRING_EMPIRE_CHAOS);
            manager.ResetActivePairings();

            manager.SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_GREENSKIN_DWARVES, 2));
            Assert.IsNotNull(manager.GetActivePairing());
            Assert.IsTrue(manager.GetActivePairing().Tier == 2);
            Assert.IsTrue(manager.GetActivePairing().Pairing == GameData.Pairing.PAIRING_GREENSKIN_DWARVES);

        }

        [TestMethod]
        public void SetActivePair_Deactivates()
        {

            manager.SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_ELVES_DARKELVES, 3));
            Assert.IsNotNull(manager.GetActivePairing());
            Assert.IsTrue(manager.GetActivePairing().Tier == 3);

            manager.SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_ELVES_DARKELVES, 3));

            Assert.IsFalse(manager.GetActivePairing().Pairing == GameData.Pairing.PAIRING_GREENSKIN_DWARVES);
            Assert.IsFalse(manager.GetActivePairing().Tier == 2);
            Assert.IsFalse(manager.GetActivePairing().Pairing == GameData.Pairing.PAIRING_EMPIRE_CHAOS);
        }

        [TestMethod]
        public void SetActivePair_Activates()
        {
            
            
            manager.SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_ELVES_DARKELVES, 3));
            Assert.IsTrue(RacialPairHelper.Equals(manager.GetActivePairing(), RacialPairManager.GetByPair(Pairing.PAIRING_ELVES_DARKELVES, 3)));

            manager.SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_ELVES_DARKELVES, 4));
            Assert.IsTrue(RacialPairHelper.Equals(manager.GetActivePairing(), RacialPairManager.GetByPair(Pairing.PAIRING_ELVES_DARKELVES, 4)));

            manager.SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 2));
            Assert.IsTrue(RacialPairHelper.Equals(manager.GetActivePairing(), RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 2)));

        }

        [TestMethod]
        public void GetNextTier_ReturnsCorrectValues()
        {
            Assert.IsTrue(manager.GetNextTier(0) == 2);
            Assert.IsTrue(manager.GetNextTier(1) == 1);
            Assert.IsTrue(manager.GetNextTier(2) == 3);
            Assert.IsTrue(manager.GetNextTier(3) == 4);
            Assert.IsTrue(manager.GetNextTier(4) == 5);
            Assert.IsTrue(manager.GetNextTier(99) == 2);
            Assert.IsTrue(manager.GetNextTier(5) == 2);
        }
        

        [TestMethod]
        public void AdvancePairing_NoPairings()
        {
            var newPairing = manager.AdvancePairing();
            Assert.IsTrue(RacialPairHelper.Equals(newPairing, RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 2)));

        }

        [TestMethod]
        public void AdvancePairing_NoActivePairing()
        {
            
            var newPairing = manager.AdvancePairing();
            Assert.IsNotNull(newPairing);
            Assert.IsTrue(RacialPairHelper.Equals(newPairing, RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 2)));
        }


        [TestMethod]
        public void AdvancePairing_T2_to_T3()
        {
            
            manager.SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 2));

            var newPairing = manager.AdvancePairing();
            Assert.IsNotNull(newPairing);
            Assert.IsTrue(RacialPairHelper.Equals(newPairing, RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 3)));
        }

        [TestMethod]
        public void AdvancePairing_T3_to_T4()
        {
            
            manager.SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 3));

            var newPairing = manager.AdvancePairing();

            Assert.IsNotNull(newPairing);
            Assert.IsTrue(RacialPairHelper.Equals(newPairing, RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 4)));
        }

        [TestMethod]
        public void AdvancePairing_T4_to_T5()
        {
            manager.SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 4));
            var newPairing = manager.AdvancePairing();

            Assert.IsNotNull(newPairing);
            Assert.IsTrue(newPairing.Tier == 5);
            Assert.IsTrue(manager.GetActivePairing().Pairing == GameData.Pairing.PAIRING_LAND_OF_THE_DEAD);
            Assert.IsTrue(manager.GetActivePairing().Tier == 5);

            manager.SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_ELVES_DARKELVES, 4));
            var newPairing1 = manager.AdvancePairing();

            Assert.IsNotNull(newPairing1);
            Assert.IsTrue(newPairing1.Tier == 5);
            Assert.IsTrue(manager.GetActivePairing().Pairing == GameData.Pairing.PAIRING_LAND_OF_THE_DEAD);
            Assert.IsTrue(manager.GetActivePairing().Tier == 5);

            manager.SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_GREENSKIN_DWARVES, 4));
            var newPairing2 = manager.AdvancePairing();

            Assert.IsNotNull(newPairing2);
            Assert.IsTrue(newPairing2.Tier == 5);
            Assert.IsTrue(manager.GetActivePairing().Pairing == GameData.Pairing.PAIRING_LAND_OF_THE_DEAD);
            Assert.IsTrue(manager.GetActivePairing().Tier == 5);
        }

        [TestMethod]
        public void AdvancePairing_T5_to_T2()
        {
            manager.SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_LAND_OF_THE_DEAD, 5));
            Assert.IsTrue(manager.GetActivePairing().Pairing == GameData.Pairing.PAIRING_LAND_OF_THE_DEAD);
            Assert.IsTrue(manager.GetActivePairing().Tier == 5);

            var newPairing = manager.AdvancePairing();

            Assert.IsNotNull(newPairing);
            Assert.IsTrue(newPairing.Tier == 2);

        }

        [TestMethod]
        public void SetInitialPairActive_ReturnsT2Emp()
        {
            
            manager.SetInitialPairActive();
            Assert.IsTrue(RacialPairHelper.Equals(manager.GetActivePairing(), RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 2)));
        }
    }
}
