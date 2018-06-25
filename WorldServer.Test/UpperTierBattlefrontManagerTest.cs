using System.Collections.Generic;
using Common.Database.World.Battlefront;
using GameData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldServer.World.Battlefronts.Apocalypse;


namespace WorldServer.Test
{
    [TestClass]
    public class UpperTierBattleFrontManagerTest
    {
        public UpperTierBattleFrontManager manager { get; set; }
        public List<RVRProgression> SampleProgressionList { get; set; }

        [TestInitialize]
        public void Setup()
        {

            SampleProgressionList = new List<RVRProgression>();
            SampleProgressionList.Add(new RVRProgression
            {
                Tier = 4,
                ZoneId = 100,
                BattleFrontId = 1,
                Description = "Praag",   // named for default pickup
                DestWinProgression = 2,
                OrderWinProgression = 3,
                PairingId = 2
            });
            SampleProgressionList.Add(new RVRProgression
            {
                Tier = 4,
                ZoneId = 110,
                BattleFrontId = 2,
                Description = "BF2",
                DestWinProgression = 6,
                OrderWinProgression = 7,
                PairingId = 2
            });
            SampleProgressionList.Add(new RVRProgression
            {
                Tier = 4,
                ZoneId = 120,
                BattleFrontId = 6,
                Description = "BF3",
                DestWinProgression = 1,
                OrderWinProgression = 2,
                PairingId = 1
            });
            manager  = new UpperTierBattleFrontManager(SampleProgressionList);
        }

        [TestMethod]
        public void Constructor_NoPairings_CreatesError()
        {
            var manager = new UpperTierBattleFrontManager(null);
            Assert.IsNull(manager.ActiveBattleFront);
        }

        [TestMethod]
        public void Constructor_NoActivePairings_CreatesError()
        {
            var manager = new UpperTierBattleFrontManager(SampleProgressionList);
            Assert.IsNull(manager.ActiveBattleFront);
        }

        [TestMethod]
        public void ResetActivePairing()
        {
            var manager = new UpperTierBattleFrontManager(SampleProgressionList);
            var bf = manager.ResetBattleFrontProgression();
            Assert.IsTrue(bf.BattleFrontId == 1);
        }

        [TestMethod]
        public void ActivePairingLocated()
        {

            var manager = new UpperTierBattleFrontManager(SampleProgressionList);
            var bf = manager.ResetBattleFrontProgression();
            Assert.IsTrue(bf.DestWinProgression == 2);

            bf = manager.AdvanceBattleFront(Realms.REALMS_REALM_DESTRUCTION);
            Assert.IsTrue(bf.BattleFrontId == 2);
            Assert.IsTrue(bf.DestWinProgression == 6);
            Assert.IsTrue(bf.OrderWinProgression == 7);
            Assert.IsTrue(manager.ActiveBattleFront.BattleFrontId == 2);

            bf = manager.AdvanceBattleFront(Realms.REALMS_REALM_DESTRUCTION);
            Assert.IsTrue(bf.BattleFrontId == 6);
            Assert.IsTrue(bf.DestWinProgression == 1);
            Assert.IsTrue(bf.OrderWinProgression == 2);
            Assert.IsTrue(manager.ActiveBattleFront.BattleFrontId == 6);

            bf = manager.AdvanceBattleFront(Realms.REALMS_REALM_ORDER);
            Assert.IsTrue(bf.BattleFrontId == 2);
            Assert.IsTrue(bf.DestWinProgression == 6);
            Assert.IsTrue(bf.OrderWinProgression == 7);
            Assert.IsTrue(manager.ActiveBattleFront.BattleFrontId == 2);

            bf = manager.AdvanceBattleFront(Realms.REALMS_REALM_DESTRUCTION);
            Assert.IsTrue(bf.BattleFrontId == 6);
            Assert.IsTrue(bf.DestWinProgression == 1);
            Assert.IsTrue(bf.OrderWinProgression == 2);
            Assert.IsTrue(manager.ActiveBattleFront.BattleFrontId == 6);

            bf = manager.AdvanceBattleFront(Realms.REALMS_REALM_DESTRUCTION);
            Assert.IsTrue(bf.BattleFrontId == 1);
            Assert.IsTrue(bf.DestWinProgression == 2);
            Assert.IsTrue(bf.OrderWinProgression == 3);

            Assert.IsTrue(manager.ActiveBattleFront.BattleFrontId == 1);
            Assert.IsTrue(manager.ActiveBattleFront.DestWinProgression == 2);
            Assert.IsTrue(manager.ActiveBattleFront.OrderWinProgression == 3);
        }

     
    }
}
