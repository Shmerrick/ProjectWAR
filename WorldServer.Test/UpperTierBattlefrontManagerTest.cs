using System;
using System.Collections.Generic;
using Common;
using Common.Database.World.Battlefront;
using GameData;
using WorldServer.World.Battlefronts.Apocalypse;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldServer.World.Battlefronts.Objectives;
using WorldServer.World.Map;
using WorldServer.World.Objects;


namespace WorldServer.Test
{
    [TestClass]
    public class UpperTierBattleFrontManagerTest
    {
        public UpperTierCampaignManager manager { get; set; }
        public List<RVRProgression> SampleProgressionList { get; set; }
        public RegionMgr Region1 { get; set; }
        public RegionMgr Region3 { get; set; }
        public List<WorldServer.World.Battlefronts.Apocalypse.BattlefieldObjective> PraagBOList { get; set; }
        public List<BattlefieldObjective> ChaosWastesBOList { get; set; }
        public List<BattlefieldObjective> ThunderMountainBOList { get; set; }
        public List<BattlefieldObjective> KadrinValleyBOList { get; set; }

        public List<BattlefieldObjective> Region1BOList { get; set; }
        public List<BattlefieldObjective> Region3BOList { get; set; }
        public List<RegionMgr> RegionMgrs { get; set; }
        public IApocCommunications FakeComms { get; set; }


        [TestInitialize]
        public void Setup()
        {
            FakeComms = A.Fake<IApocCommunications>();
            RegionMgrs = new List<RegionMgr>();

            PraagBOList = new List<BattlefieldObjective>();
            ChaosWastesBOList = new List<BattlefieldObjective>();
            ThunderMountainBOList = new List<BattlefieldObjective>();
            KadrinValleyBOList = new List<BattlefieldObjective>();

            Region1BOList = new List<BattlefieldObjective>();
            Region3BOList = new List<BattlefieldObjective>();


            var R1ZoneList = new List<Zone_Info>();
            R1ZoneList.Add(new Zone_Info { ZoneId = 200, Name = "R1Zone200 PR", Pairing = 2, Tier = 4 });
            R1ZoneList.Add(new Zone_Info { ZoneId = 201, Name = "R1Zone201 CW", Pairing = 2, Tier = 4 });

            var R3ZoneList = new List<Zone_Info>();
            R3ZoneList.Add(new Zone_Info { ZoneId = 400, Name = "R3Zone400 TM", Pairing = 1, Tier = 4 });
            R3ZoneList.Add(new Zone_Info { ZoneId = 401, Name = "R3Zone401 KV", Pairing = 1, Tier = 4 });

            Region1 = new RegionMgr(1, R1ZoneList, "Region1", FakeComms);
            Region3 = new RegionMgr(3, R3ZoneList, "Region3", FakeComms);


            RegionMgrs.Add(Region1);
            RegionMgrs.Add(Region3);


            PraagBOList.Add(new BattlefieldObjective(1, "BO1", 200, 1, 4));
            PraagBOList.Add(new BattlefieldObjective(2, "BO2", 200, 1, 4));
            PraagBOList.Add(new BattlefieldObjective(3, "BO3", 200, 1, 4));
            PraagBOList.Add(new BattlefieldObjective(4, "BO4", 200, 1, 4));

            ChaosWastesBOList.Add(new BattlefieldObjective(11, "BO1", 201, 1, 4));
            ChaosWastesBOList.Add(new BattlefieldObjective(12, "BO2", 201, 1, 4));
            ChaosWastesBOList.Add(new BattlefieldObjective(13, "BO3", 201, 1, 4));
            ChaosWastesBOList.Add(new BattlefieldObjective(14, "BO4", 201, 1, 4));

            ThunderMountainBOList.Add(new BattlefieldObjective(21, "BO1", 400, 3, 4));
            ThunderMountainBOList.Add(new BattlefieldObjective(22, "BO2", 400, 3, 4));
            ThunderMountainBOList.Add(new BattlefieldObjective(23, "BO3", 400, 3, 4));
            ThunderMountainBOList.Add(new BattlefieldObjective(24, "BO4", 400, 3, 4));

            KadrinValleyBOList.Add(new BattlefieldObjective(31, "BO1", 401, 3, 4));
            KadrinValleyBOList.Add(new BattlefieldObjective(32, "BO2", 401, 3, 4));
            KadrinValleyBOList.Add(new BattlefieldObjective(33, "BO3", 401, 3, 4));
            KadrinValleyBOList.Add(new BattlefieldObjective(34, "BO4", 401, 3, 4));

            Region1BOList.AddRange(PraagBOList);
            Region1BOList.AddRange(ChaosWastesBOList);

            Region3BOList.AddRange(ThunderMountainBOList);
            Region3BOList.AddRange(KadrinValleyBOList);


            SampleProgressionList = new List<RVRProgression>();
            SampleProgressionList.Add(new RVRProgression
            {
                Tier = 4,
                ZoneId = 200,
                BattleFrontId = 1,
                Description = "Praag", // named for default pickup
                DestWinProgression = 2,
                OrderWinProgression = 3,
                PairingId = 2,
                RegionId = 1,
                ResetProgressionOnEntry = 1
            });
            SampleProgressionList.Add(new RVRProgression
            {
                Tier = 4,
                ZoneId = 201,
                BattleFrontId = 2,
                Description = "Chaos Wastes",
                DestWinProgression = 6,
                OrderWinProgression = 7,
                PairingId = 2,
                RegionId = 1,
                ResetProgressionOnEntry = 0
            });
            SampleProgressionList.Add(new RVRProgression
            {
                Tier = 4,
                ZoneId = 400,
                BattleFrontId = 6,
                Description = "Thunder Mountain",
                DestWinProgression = 7,
                OrderWinProgression = 2,
                PairingId = 1,
                RegionId = 3,
                ResetProgressionOnEntry = 0
            });
            SampleProgressionList.Add(new RVRProgression
            {
                Tier = 4,
                ZoneId = 401,
                BattleFrontId = 7,
                Description = "Kadrin Valley",
                DestWinProgression = 1,
                OrderWinProgression = 1,
                PairingId = 1,
                RegionId = 3,
                ResetProgressionOnEntry = 0
            });
            manager = new UpperTierCampaignManager(SampleProgressionList, RegionMgrs);
            // Must be run before attaching ApocBattleFronts to get an ActiveBF
            manager.GetActiveBattleFrontFromProgression();

            Region1.Campaign = new Campaign(Region1, Region1BOList, new HashSet<Player>(), manager, FakeComms);
            Region3.Campaign = new Campaign(Region3, Region3BOList, new HashSet<Player>(), manager, FakeComms);


        }

        [TestMethod]
        public void Constructor_NoPairings_CreatesError()
        {
            var manager = new UpperTierCampaignManager(null, RegionMgrs);
            Assert.IsNull(manager.ActiveBattleFront);
        }

        [TestMethod]
        public void Constructor_NoActivePairings_CreatesError()
        {
            var manager = new UpperTierCampaignManager(SampleProgressionList, RegionMgrs);
            Assert.IsNull(manager.ActiveBattleFront);
        }

        [TestMethod]
        public void ResetActivePairing()
        {
            var manager = new UpperTierCampaignManager(SampleProgressionList, RegionMgrs);
            var bf = manager.GetActiveBattleFrontFromProgression();
            Assert.IsTrue(bf.BattleFrontId == 1);
        }

        [TestMethod]
        public void ActivePairingLocated()
        {

            manager = new UpperTierCampaignManager(SampleProgressionList, RegionMgrs);
            // Must be run before attaching ApocBattleFronts to get an ActiveBF
            var bf = manager.GetActiveBattleFrontFromProgression();

            Region1.Campaign = new Campaign(Region1, Region1BOList, new HashSet<Player>(), manager, FakeComms);
            Region3.Campaign = new Campaign(Region3, Region3BOList, new HashSet<Player>(), manager, FakeComms);

            Assert.IsTrue(bf.DestWinProgression == 2);

			
			bf = manager.AdvanceBattleFront(Realms.REALMS_REALM_DESTRUCTION);
			Assert.IsTrue(bf.BattleFrontId == 2);
            Assert.IsTrue(bf.DestWinProgression == 6);
            Assert.IsTrue(bf.OrderWinProgression == 7);
            Assert.IsTrue(manager.ActiveBattleFront.BattleFrontId == 2);

            bf = manager.AdvanceBattleFront(Realms.REALMS_REALM_DESTRUCTION);
			Assert.IsTrue(bf.BattleFrontId == 6);
            Assert.IsTrue(bf.DestWinProgression == 7);
            Assert.IsTrue(bf.OrderWinProgression == 2);
            Assert.IsTrue(manager.ActiveBattleFront.BattleFrontId == 6);

            bf = manager.AdvanceBattleFront(Realms.REALMS_REALM_ORDER);
			Assert.IsTrue(bf.BattleFrontId == 2);
            Assert.IsTrue(bf.DestWinProgression == 6);
            Assert.IsTrue(bf.OrderWinProgression == 7);
            Assert.IsTrue(manager.ActiveBattleFront.BattleFrontId == 2);

            bf = manager.AdvanceBattleFront(Realms.REALMS_REALM_DESTRUCTION);
			Assert.IsTrue(bf.BattleFrontId == 6);
            Assert.IsTrue(bf.DestWinProgression == 7);
            Assert.IsTrue(bf.OrderWinProgression == 2);
            Assert.IsTrue(manager.ActiveBattleFront.BattleFrontId == 6);

            bf = manager.AdvanceBattleFront(Realms.REALMS_REALM_DESTRUCTION);
			Assert.IsTrue(bf.BattleFrontId == 7);
            Assert.IsTrue(bf.DestWinProgression == 1);
            Assert.IsTrue(bf.OrderWinProgression == 1);

            Assert.IsTrue(manager.ActiveBattleFront.BattleFrontId == 7);
            Assert.IsTrue(manager.ActiveBattleFront.DestWinProgression == 1);
            Assert.IsTrue(manager.ActiveBattleFront.OrderWinProgression == 1);
        }

        [TestMethod]
        public void OpenActiveBattleFrontSetsCorrectBOFlags()
        {
            manager = new UpperTierCampaignManager(SampleProgressionList, RegionMgrs);
            // Must be run before attaching ApocBattleFronts to get an ActiveBF
            var bf = manager.GetActiveBattleFrontFromProgression();

            Region1.Campaign = new Campaign(Region1, Region1BOList, new HashSet<Player>(), manager, FakeComms);
            Region3.Campaign = new Campaign(Region3, Region3BOList, new HashSet<Player>(), manager, FakeComms);
            // Open Praag (BF==1)
            manager.OpenActiveBattlefront();
            Assert.IsTrue(manager.ActiveBattleFront.BattleFrontId == 1); // Praag

            // Ensure that the BOs for this battlefront ONLY are unlocked.
            foreach (var bo in Region1.Campaign.Objectives)
            {
                if (bo.ZoneId == 200)
                {
                    // Locking the Region should set all BOs in the region to be zonelocked (
                    Assert.IsTrue(bo.State == StateFlags.Unsecure);
                }
                else
                {
                    Assert.IsTrue(bo.State == StateFlags.ZoneLocked);
                }
            }
        }

        [TestMethod]
        public void LockBattleFront1()
        {
            var fakeCommsEngine = A.Fake<IApocCommunications>();
            manager = new UpperTierCampaignManager(SampleProgressionList, RegionMgrs);
            // Must be run before attaching ApocBattleFronts to get an ActiveBF
            var bf = manager.GetActiveBattleFrontFromProgression();

            Region1.Campaign = new Campaign(Region1, Region1BOList, new HashSet<Player>(), manager, FakeComms);
            Region3.Campaign = new Campaign(Region3, Region3BOList, new HashSet<Player>(), manager, FakeComms);
            // Open Praag (BF==1)
            manager.OpenActiveBattlefront();
            // Locking Region1.Campaign
            Region1.Campaign.VictoryPointProgress.DestructionVictoryPoints = 5000f;
            Region1.Campaign.VictoryPointProgress.OrderVictoryPoints = 1000f;
            manager.LockActiveBattleFront(Realms.REALMS_REALM_DESTRUCTION, -1);

            // Ensure battlefront 1 is locked and to Destro
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).LockingRealm == Realms.REALMS_REALM_DESTRUCTION);
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).Locked);
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).FinalVictoryPoint.DestructionVictoryPoints == 5000f);
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).FinalVictoryPoint.OrderVictoryPoints == 1000f);

            // Ensure that the BOs for this battlefront ONLY are locked.
            foreach (var apocBattlefieldObjective in Region1.Campaign.Objectives)
            {
                // Locking a battlefront should ZoneLock the BOs in that Zone, and Open those in the next battlefront.
                if ((apocBattlefieldObjective.ZoneId == 200) || (apocBattlefieldObjective.ZoneId == 201))
                {
                    // Should be all locked.
                    Assert.IsTrue(apocBattlefieldObjective.State == StateFlags.ZoneLocked);
                }

            }
        }

        [TestMethod]
        public void LockBattleFrontAndAdvance()
        {
            var fakeCommsEngine = A.Fake<IApocCommunications>();
            manager = new UpperTierCampaignManager(SampleProgressionList, RegionMgrs);
            // Must be run before attaching ApocBattleFronts to get an ActiveBF
            var bf = manager.GetActiveBattleFrontFromProgression();

            Region1.Campaign = new Campaign(Region1, Region1BOList, new HashSet<Player>(), manager, FakeComms);
            Region3.Campaign = new Campaign(Region3, Region3BOList, new HashSet<Player>(), manager, FakeComms);
            // Open Praag (BF==1)
            manager.OpenActiveBattlefront();
            // Locking Region1.Campaign
            Region1.Campaign.VictoryPointProgress.DestructionVictoryPoints = 5000f;
            Region1.Campaign.VictoryPointProgress.OrderVictoryPoints = 1000f;
            manager.LockActiveBattleFront(Realms.REALMS_REALM_DESTRUCTION, -1);

            // Ensure battlefront 1 is locked and to Destro
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).LockingRealm == Realms.REALMS_REALM_DESTRUCTION);
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).Locked);
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).FinalVictoryPoint.DestructionVictoryPoints == 5000f);
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).FinalVictoryPoint.OrderVictoryPoints == 1000f);

			// Advance Destro
			
			var progression = manager.AdvanceBattleFront(Realms.REALMS_REALM_DESTRUCTION);
			Assert.IsTrue(progression.BattleFrontId == 2);
            Assert.IsTrue(progression.ZoneId == 201);

            manager.OpenActiveBattlefront();

            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).LockingRealm == Realms.REALMS_REALM_NEUTRAL);
            Assert.IsFalse(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).Locked);
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).FinalVictoryPoint.DestructionVictoryPoints == 0f);
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).FinalVictoryPoint.OrderVictoryPoints == 0f);

            foreach (var apocBattlefieldObjective in Region1.Campaign.Objectives)
            {
                // Locking a battlefront should ZoneLock the BOs in that Zone, and Open those in the next battlefront.
                if (apocBattlefieldObjective.ZoneId == 201)
                {
                    // Should be all locked.
                    Assert.IsTrue(apocBattlefieldObjective.State == StateFlags.Unsecure);
                }
                if (apocBattlefieldObjective.ZoneId == 200)
                {
                    // Should be all locked.
                    Assert.IsTrue(apocBattlefieldObjective.State == StateFlags.ZoneLocked);
                }
            }

            Region1.Campaign.VictoryPointProgress.DestructionVictoryPoints = 5000f;
            Region1.Campaign.VictoryPointProgress.OrderVictoryPoints = 2200f;

            // Lock Destro again
            manager.LockActiveBattleFront(Realms.REALMS_REALM_DESTRUCTION, -1);

            // Ensure battlefront 2 is locked and to Destro
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).LockingRealm == Realms.REALMS_REALM_DESTRUCTION);
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).Locked);
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).FinalVictoryPoint.DestructionVictoryPoints == 5000f);
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).FinalVictoryPoint.OrderVictoryPoints == 2200f);

            // Ensure that the BOs for this battlefront ONLY are locked.
            foreach (var apocBattlefieldObjective in Region1.Campaign.Objectives)
            {
                // Locking a battlefront should ZoneLock the BOs in that Zone, and Open those in the next battlefront.
                if ((apocBattlefieldObjective.ZoneId == 200) || (apocBattlefieldObjective.ZoneId == 201))
                {
                    // Should be all locked.
                    Assert.IsTrue(apocBattlefieldObjective.State == StateFlags.ZoneLocked);
                }
            }

            // Ensure that the BOs for this battlefront ONLY are locked.
            foreach (var apocBattlefieldObjective in Region3.Campaign.Objectives)
            {
                // Should be all locked.
                Assert.IsTrue(apocBattlefieldObjective.State == StateFlags.ZoneLocked);
            }

        }

        [TestMethod]
        public void LockAndRollRegions()
        {
            var fakeCommsEngine = A.Fake<IApocCommunications>();
            manager = new UpperTierCampaignManager(SampleProgressionList, RegionMgrs);
            // Must be run before attaching ApocBattleFronts to get an ActiveBF
            var bf = manager.GetActiveBattleFrontFromProgression();

            Region1.Campaign = new Campaign(Region1, Region1BOList, new HashSet<Player>(), manager, FakeComms);
            Region3.Campaign = new Campaign(Region3, Region3BOList, new HashSet<Player>(), manager, FakeComms);
            // Open Praag (BF==1)
            manager.OpenActiveBattlefront();
            // Locking Region1.Campaign
            Region1.Campaign.VictoryPointProgress.DestructionVictoryPoints = 5000f;
            Region1.Campaign.VictoryPointProgress.OrderVictoryPoints = 1000f;
            manager.LockActiveBattleFront(Realms.REALMS_REALM_DESTRUCTION, -1);

            // Ensure battlefront 1 is locked and to Destro
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).LockingRealm == Realms.REALMS_REALM_DESTRUCTION);
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).Locked);
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).FinalVictoryPoint.DestructionVictoryPoints == 5000f);
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).FinalVictoryPoint.OrderVictoryPoints == 1000f);
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).RegionId ==1);

			// Advance Destro
			
			var progression = manager.AdvanceBattleFront(Realms.REALMS_REALM_DESTRUCTION);
			Assert.IsTrue(progression.BattleFrontId == 2);
            Assert.IsTrue(progression.ZoneId == 201);

            manager.OpenActiveBattlefront();

            Region1.Campaign.VictoryPointProgress.DestructionVictoryPoints = 5000f;
            Region1.Campaign.VictoryPointProgress.OrderVictoryPoints = 2200f;

            var activeCampaign = manager.GetActiveCampaign();
            Assert.IsTrue(activeCampaign.ActiveCampaignName == "Chaos Wastes");
            Assert.IsTrue(activeCampaign.Region.RegionId == 1);
            Assert.IsTrue(activeCampaign.VictoryPointProgress.DestructionVictoryPoints == 5000f);
            Assert.IsTrue(activeCampaign.VictoryPointProgress.OrderVictoryPoints == 2200f);

            // Lock Destro again
            manager.LockActiveBattleFront(Realms.REALMS_REALM_DESTRUCTION, -1);
            
            // Ensure battlefront is locked and to Destro
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).LockingRealm == Realms.REALMS_REALM_DESTRUCTION);
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).Locked);

            // Advance Destro
            var progression2 = manager.AdvanceBattleFront(Realms.REALMS_REALM_DESTRUCTION);
			Assert.IsTrue(progression2.BattleFrontId == 6);
            Assert.IsTrue(progression2.ZoneId == 400);


            // Ensure that the BOs for this battlefront ONLY are locked.
            foreach (var apocBattlefieldObjective in Region1.Campaign.Objectives)
            {
                // Locking a battlefront should ZoneLock the BOs in that Zone, and Open those in the next battlefront.
                if (apocBattlefieldObjective.ZoneId == 400)
                {
                    // Should be all locked.
                    Assert.IsTrue(apocBattlefieldObjective.State == StateFlags.ZoneLocked);
                }
            }

            manager.OpenActiveBattlefront();

            // Ensure that the BOs for this battlefront ONLY are locked.
            foreach (var apocBattlefieldObjective in Region1.Campaign.Objectives)
            {
                // Locking a battlefront should ZoneLock the BOs in that Zone, and Open those in the next battlefront.
                if (apocBattlefieldObjective.ZoneId == 400)
                {
                    // Should be all locked.
                    Assert.IsTrue(apocBattlefieldObjective.State == StateFlags.Unsecure);
                }
            }

            Region3.Campaign.VictoryPointProgress.DestructionVictoryPoints = 2200f;
            Region3.Campaign.VictoryPointProgress.OrderVictoryPoints = 5000f;

            // Lock Destro again
            manager.LockActiveBattleFront(Realms.REALMS_REALM_ORDER,-1);

            // Ensure battlefront is locked and to Destro
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).LockingRealm == Realms.REALMS_REALM_ORDER);
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).Locked);

            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).FinalVictoryPoint.DestructionVictoryPoints == 2200);
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).FinalVictoryPoint.OrderVictoryPoints == 5000f);
        }

        [TestMethod]
        public void GetBattleFrontStatus()
        {
            var fakeCommsEngine = A.Fake<IApocCommunications>();
            manager = new UpperTierCampaignManager(SampleProgressionList, RegionMgrs);
            // Must be run before attaching ApocBattleFronts to get an ActiveBF
            var bf = manager.GetActiveBattleFrontFromProgression();

            Region1.Campaign = new Campaign(Region1, Region1BOList, new HashSet<Player>(), manager, FakeComms);
            Region3.Campaign = new Campaign(Region3, Region3BOList, new HashSet<Player>(), manager, FakeComms);
            // Open Praag (BF==1)
            manager.OpenActiveBattlefront();

            var status = manager.GetBattleFrontStatus(bf.BattleFrontId);

            Assert.IsTrue(status.Locked == false);
            Assert.IsTrue(status.FinalVictoryPoint.DestructionVictoryPoints == 0);
            Assert.IsTrue(status.FinalVictoryPoint.OrderVictoryPoints == 0);
            Assert.IsTrue(status.LockStatus == BattleFrontConstants.ZONE_STATUS_CONTESTED);

        }

        [TestMethod]
        public void GetCampaignStatus()
        {
            var fakeCommsEngine = A.Fake<IApocCommunications>();
            manager = new UpperTierCampaignManager(SampleProgressionList, RegionMgrs);
            // Must be run before attaching ApocBattleFronts to get an ActiveBF
            var bf = manager.GetActiveBattleFrontFromProgression();

            Region1.Campaign = new Campaign(Region1, Region1BOList, new HashSet<Player>(), manager, FakeComms);
            Region3.Campaign = new Campaign(Region3, Region3BOList, new HashSet<Player>(), manager, FakeComms);
            // Open Praag (BF==1)
            manager.OpenActiveBattlefront();

            var activeCampaign = manager.GetActiveCampaign();
            Assert.IsTrue(activeCampaign.ActiveCampaignName == "Praag");
            Assert.IsTrue(activeCampaign.Region.RegionId == 1);
            Assert.IsTrue(activeCampaign.VictoryPointProgress.DestructionVictoryPoints == 0f);
            Assert.IsTrue(activeCampaign.VictoryPointProgress.OrderVictoryPoints == 0f);

        }


        [TestMethod]
        public void RollAllRegionsBackToStart()
        {
            var fakeCommsEngine = A.Fake<IApocCommunications>();
            manager = new UpperTierCampaignManager(SampleProgressionList, RegionMgrs);
            // Must be run before attaching ApocBattleFronts to get an ActiveBF
            var bf = manager.GetActiveBattleFrontFromProgression();

            Region1.Campaign = new Campaign(Region1, Region1BOList, new HashSet<Player>(), manager, FakeComms);
            Region3.Campaign = new Campaign(Region3, Region3BOList, new HashSet<Player>(), manager, FakeComms);
            // Open Praag (BF==1)
            manager.OpenActiveBattlefront();
            // Locking Region1.Campaign
            Region1.Campaign.VictoryPointProgress.DestructionVictoryPoints = 5000f;
            Region1.Campaign.VictoryPointProgress.OrderVictoryPoints = 1000f;
            
            manager.LockActiveBattleFront(Realms.REALMS_REALM_DESTRUCTION, -1);

            // Ensure battlefront 1 is locked and to Destro
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).LockingRealm == Realms.REALMS_REALM_DESTRUCTION);
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).Locked);
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).FinalVictoryPoint.DestructionVictoryPoints == 5000f);
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).FinalVictoryPoint.OrderVictoryPoints == 1000f);
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).RegionId == 1);

			// Advance Destro
			
			var progression = manager.AdvanceBattleFront(Realms.REALMS_REALM_DESTRUCTION);
			Assert.IsTrue(progression.BattleFrontId == 2);
            Assert.IsTrue(progression.ZoneId == 201);
            manager.OpenActiveBattlefront();
            Region1.Campaign.VictoryPointProgress.DestructionVictoryPoints = 5000f;
            Region1.Campaign.VictoryPointProgress.OrderVictoryPoints = 2200f;

            var activeCampaign = manager.GetActiveCampaign();
            Assert.IsTrue(activeCampaign.ActiveCampaignName == "Chaos Wastes");
            Assert.IsTrue(activeCampaign.Region.RegionId == 1);
            Assert.IsTrue(activeCampaign.VictoryPointProgress.DestructionVictoryPoints == 5000f);
            Assert.IsTrue(activeCampaign.VictoryPointProgress.OrderVictoryPoints == 2200f);

            // Lock Destro again
            manager.LockActiveBattleFront(Realms.REALMS_REALM_DESTRUCTION,-1);

            // Ensure battlefront is locked and to Destro
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).LockingRealm == Realms.REALMS_REALM_DESTRUCTION);
            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).Locked);

            // Advance Destro
            var progression2 = manager.AdvanceBattleFront(Realms.REALMS_REALM_DESTRUCTION);
			Assert.IsTrue(progression2.BattleFrontId == 6);
            Assert.IsTrue(progression2.ZoneId == 400);

            // KV
            manager.LockActiveBattleFront(Realms.REALMS_REALM_DESTRUCTION, -1);
            var progression3 = manager.AdvanceBattleFront(Realms.REALMS_REALM_DESTRUCTION);
			manager.OpenActiveBattlefront();
            Assert.IsTrue(progression3.BattleFrontId == 7);
            Assert.IsTrue(progression3.ZoneId == 401);

            //Back to Emp
            manager.LockActiveBattleFront(Realms.REALMS_REALM_DESTRUCTION, -1);
            var progression4 = manager.AdvanceBattleFront(Realms.REALMS_REALM_DESTRUCTION);
			manager.OpenActiveBattlefront();
            Assert.IsTrue(progression4.BattleFrontId == 1);
            Assert.IsTrue(progression4.ZoneId == 200);

            // Should be all unlocked in Praag
            // Ensure that the BOs for this battlefront ONLY are locked.
            foreach (var apocBattlefieldObjective in Region1.Campaign.Objectives)
            {
                // Locking a battlefront should ZoneLock the BOs in that Zone, and Open those in the next battlefront.
                if (apocBattlefieldObjective.ZoneId == 200)
                {
                    // Should be all unsecure.
                    Assert.IsTrue(apocBattlefieldObjective.State == StateFlags.Unsecure);
                }
                else
                {
                    Assert.IsTrue(apocBattlefieldObjective.State == StateFlags.ZoneLocked);
                }
            }

            Assert.IsTrue(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).LockingRealm == Realms.REALMS_REALM_NEUTRAL);
            Assert.IsFalse(manager.GetBattleFrontStatus(manager.ActiveBattleFront.BattleFrontId).Locked);


        }

    }
}