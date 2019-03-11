using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using Common;
using Common.Database.World.Battlefront;
using GameData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldServer.Managers.Commands;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Map;

namespace WorldServer.Test
{
    [TestClass]
    public class LowerTierBattleFrontManagerTest
    {
        public LowerTierCampaignManager manager { get; set; }
        public List<RVRProgression> SampleProgressionList { get; set; }
        public RegionMgr Region1 { get; set; }
        public RegionMgr Region3 { get; set; }
        public List<RegionMgr> RegionMgrs { get; set; }
        public IApocCommunications FakeComms { get; set; }


        [TestInitialize]
        public void Setup()
        {
            RegionMgrs = new List<RegionMgr>();


            var R1ZoneList = new List<Zone_Info>();
            R1ZoneList.Add(new Zone_Info { ZoneId = 200,Name = "R1Zone200 PR",Pairing = 2 });
            R1ZoneList.Add(new Zone_Info { ZoneId = 201,Name = "R1Zone201 CW",Pairing = 2 });

            var R3ZoneList = new List<Zone_Info>();
            R3ZoneList.Add(new Zone_Info { ZoneId = 400,Name = "R3Zone400 TM",Pairing = 1 });
            R3ZoneList.Add(new Zone_Info { ZoneId = 401,Name = "R3Zone401 KV",Pairing = 1 });

            Region1 = new RegionMgr(1,R1ZoneList,"Region1",FakeComms);
            Region3 = new RegionMgr(3,R3ZoneList,"Region3",FakeComms);

            RegionMgrs.Add(Region1);
            RegionMgrs.Add(Region3);

            SampleProgressionList = new List<RVRProgression>();
            SampleProgressionList.Add(new RVRProgression
            {
                Tier = 1,
                ZoneId = 100,
                BattleFrontId = 1,
                Description = "Norsca",  // named for default pickup
                DestWinProgression = 2,
                OrderWinProgression = 3,
                PairingId = 2
            });
            SampleProgressionList.Add(new RVRProgression
            {
                Tier = 1,
                ZoneId = 110,
                BattleFrontId = 2,
                Description = "BF2",
                DestWinProgression = 6,
                OrderWinProgression = 7,
                PairingId = 2
            });
            SampleProgressionList.Add(new RVRProgression
            {
                Tier = 1,
                ZoneId = 120,
                BattleFrontId = 6,
                Description = "BF3",
                DestWinProgression = 1,
                OrderWinProgression = 2,
                PairingId = 1
            });
            manager = new LowerTierCampaignManager(SampleProgressionList, RegionMgrs);
        }

        [TestMethod]
        public void Constructor_NoPairings_CreatesError()
        {
            var manager = new LowerTierCampaignManager(SampleProgressionList, RegionMgrs);
            Assert.IsNull(manager.ActiveBattleFront);
        }

        [TestMethod]
        public void Constructor_NoActivePairings_CreatesError()
        {
            var manager = new LowerTierCampaignManager(SampleProgressionList,RegionMgrs);
            Assert.IsNull(manager.ActiveBattleFront);
        }

        [TestMethod]
        public void ResetActivePairing()
        {
            var manager = new LowerTierCampaignManager(SampleProgressionList, RegionMgrs);
            var bf = manager.GetActiveBattleFrontFromProgression();
            Assert.IsTrue(bf.BattleFrontId == 1);
        }

        [TestMethod]
        public void ActivePairingLocated()
        {

            var manager = new LowerTierCampaignManager(SampleProgressionList, RegionMgrs);
            var bf = manager.GetActiveBattleFrontFromProgression();
            Assert.IsTrue(bf.DestWinProgression == 2);
            Assert.IsTrue(bf.BattleFrontId == 1);

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
