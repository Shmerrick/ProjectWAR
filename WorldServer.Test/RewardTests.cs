using System.Collections.Concurrent;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FrameWork;
using NLog;
using WorldServer.World.Battlefronts.Apocalypse.Loot;
using WorldServer.World.Battlefronts.Bounty;

namespace WorldServer.Test
{
    [TestClass]
    public class RewardTests
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        [TestMethod]
        public void CheckSortOrderBagDefinitions()
        {
            var x = new LootBagTypeDefinition().BuildLootBagTypeDefinitions(20);
            Assert.IsTrue(x[0].BagRarity == LootBagRarity.Gold);
            Assert.IsTrue(x[1].BagRarity == LootBagRarity.Gold);
            Assert.IsTrue(x[x.Count-1].BagRarity == LootBagRarity.Green);

        }

        [TestMethod]
        public void AssignLootSortingIsCorrectToBags()
        {

            var sortedPairs = new List<KeyValuePair<uint, int>>
            {
                new KeyValuePair<uint, int>(128, 124),
                new KeyValuePair<uint, int>(123, 100),
                new KeyValuePair<uint, int>(127, 78),
                new KeyValuePair<uint, int>(126, 77),
                new KeyValuePair<uint, int>(124, 50),
                new KeyValuePair<uint, int>(129, 12),
                new KeyValuePair<uint, int>(130, 4),
                new KeyValuePair<uint, int>(125, 0)

            };


            var rewardAssigner = new  RewardAssigner(StaticRandom.Instance, _logger);
            var numberOfAwards = rewardAssigner.DetermineNumberOfAwards(sortedPairs.Count);
            var x = new LootBagTypeDefinition().BuildLootBagTypeDefinitions(numberOfAwards);

            var assignedLoot = rewardAssigner.AssignLootToPlayers(numberOfAwards, x, sortedPairs);

            Assert.IsTrue(assignedLoot[0].Assignee == 128);
            Assert.IsTrue(assignedLoot[1].Assignee == 123);
            Assert.IsTrue(assignedLoot[2]?.Assignee == 127);
            Assert.IsTrue(assignedLoot[3]?.Assignee == 126);
            Assert.IsTrue(assignedLoot[4]?.Assignee == 124);
            Assert.IsTrue(assignedLoot[5]?.Assignee == 129);
            Assert.IsTrue(assignedLoot[6]?.Assignee == 130);
            //Assert.IsTrue(assignedLoot[7]?.Assignee == 125);

        }


        [TestMethod]
        public void DropCandidateIsRandom()
        {
            var fakeBountyManager = A.Fake<IBountyManager>();
            var fakeContributionManager = A.Fake<IContributionManager>();
            var fakeImpactMatrixManager = A.Fake<IImpactMatrixManager>();
            var fakeRewardManager = A.Fake<RewardManager>();
            var fakeStaticWrapper = A.Fake<IStaticWrapper>();


            var rm = new RewardManager(
                fakeContributionManager,
                fakeStaticWrapper, new List<RewardPlayerKill>(),
                fakeImpactMatrixManager);

            var impactFractions = new ConcurrentDictionary<uint, float>();

            impactFractions.TryAdd(100, 0.05f);
            impactFractions.TryAdd(101, 0.10f);
            impactFractions.TryAdd(102, 0.15f);
            impactFractions.TryAdd(103, 0.50f);
            impactFractions.TryAdd(104, 0.20f);

            var x = rm.GetPlayerRVRDropCandidate(impactFractions,0);

            Assert.IsTrue(x==102);

            var y = rm.GetPlayerRVRDropCandidate(impactFractions, 1);
            Assert.IsTrue(y == 103);

            var z = rm.GetPlayerRVRDropCandidate(impactFractions, 0);
            Assert.IsTrue(z != 0);


        }



        [TestMethod]
        public void Replicate_Excel_TestCase1()
        {
            var fakeBountyManager = A.Fake<IBountyManager>();
            var fakeContributionManager = A.Fake<IContributionManager>();
            var fakeImpactMatrixManager = A.Fake<IImpactMatrixManager>();
            var fakeRewardManager = A.Fake<RewardManager>();
            var fakeStaticWrapper = A.Fake<IStaticWrapper>();

            var sampleRenownBandReward = new RewardPlayerKill
            {
                CrestCount = 1,
                CrestId = 208431,
                Money = 1500,
                RenownBand = 30,
                BaseRP = 300,
                BaseInf = 100,
                BaseXP = 3000
            };
            // Bounty for the character being killed.
            var charBounty = new CharacterBounty
            {
                CharacterLevel = 15,
                RenownLevel = 30,
                LastDeath = 0
            };
            // Impacts upon the target character
            var fakeImpacts = new List<PlayerImpact>
            {
                new PlayerImpact {CharacterId = 999, ExpiryTimestamp = 0, ImpactValue = 1500, ModificationValue = 0.85f},
                new PlayerImpact {CharacterId = 1000, ExpiryTimestamp = 0, ImpactValue = 1250, ModificationValue = 2.32f},
                new PlayerImpact {CharacterId = 1001, ExpiryTimestamp = 0, ImpactValue = 600, ModificationValue = 2.69f}
            };

            A.CallTo(() => fakeBountyManager.GetBounty(123, true)).Returns(charBounty);
            A.CallTo(() => fakeContributionManager.GetContributionValue(123)).Returns<short>(100);
            A.CallTo(() => fakeImpactMatrixManager.GetKillImpacts(123)).Returns(fakeImpacts);
            A.CallTo(() => fakeImpactMatrixManager.GetTotalImpact(123)).Returns(3350);

            A.CallTo(() => fakeStaticWrapper.GetRenownBandReward(30)).Returns(sampleRenownBandReward);

            var rm = new RewardManager(
                fakeContributionManager,
                fakeStaticWrapper, new List<RewardPlayerKill>
                {
                    sampleRenownBandReward
                },
                (ImpactMatrixManager) fakeImpactMatrixManager);



            // Pass in random number to ensure we effect as expected. StaticRandom.Instance.Next(1, 100)
            //var result = rm.GenerateBaseRewardForKill(123, 99, new Dictionary<uint, Player>())};

            //Assert.IsTrue(result.Count == 3);
            //Assert.IsTrue(result[999].RenownBand == 30);
            //Assert.IsTrue(result[999].BaseRP == 152);
            //Assert.IsTrue(result[999].BaseXP == 334);
            //Assert.IsTrue(result[999].BaseInf == 24);
            //Assert.IsTrue(result[999].BaseMoney == 671);
        }

        [TestMethod]
        public void NoInsigniasIfNoRewardBand()
        {
            var fakeBountyManager = A.Fake<IBountyManager>();
            var fakeContributionManager = A.Fake<IContributionManager>();
            var fakeImpactMatrixManager = A.Fake<IImpactMatrixManager>();
            var fakeRewardManager = A.Fake<RewardManager>();
            var fakeStaticWrapper = A.Fake<IStaticWrapper>();

            var sampleRenownBandReward = new RewardPlayerKill
            {
                CrestCount = 1,
                CrestId = 208431,
                Money = 1500
            };
            // Bounty for the character being killed.
            var charBounty = new CharacterBounty
            {
                CharacterLevel = 1,
                RenownLevel = 1,
                LastDeath = 0
            };
            // Impacts upon the target character
            var fakeImpacts = new List<PlayerImpact>
            {
                new PlayerImpact {CharacterId = 999, ExpiryTimestamp = 0, ImpactValue = 500, ModificationValue = 0.35f},
                new PlayerImpact {CharacterId = 1000, ExpiryTimestamp = 0, ImpactValue = 5000, ModificationValue = 0.10f}

            };

            A.CallTo(() => fakeBountyManager.GetBounty(123, true)).Returns(charBounty);
            A.CallTo(() => fakeContributionManager.GetContributionValue(123)).Returns<short>(10);
            A.CallTo(() => fakeImpactMatrixManager.GetKillImpacts(123)).Returns(fakeImpacts);
            A.CallTo(() => fakeImpactMatrixManager.GetTotalImpact(123)).Returns(3350);

            A.CallTo(() => fakeStaticWrapper.GetRenownBandReward(1)).Returns(sampleRenownBandReward);

            var rm = new RewardManager(
                fakeContributionManager,
                fakeStaticWrapper, new List<RewardPlayerKill>
                {
                    sampleRenownBandReward
                },
                (ImpactMatrixManager) fakeImpactMatrixManager);

            var insigniaRewards = rm.GetInsigniaRewards(100);
            Assert.IsTrue(insigniaRewards);

          
        }


        [TestMethod, Ignore]
        public void Replicate_Excel_TestCaseCEKillsTom()
        {
            var fakeBountyManager = A.Fake<IBountyManager>();
            var fakeContributionManager = A.Fake<IContributionManager>();
            var fakeImpactMatrixManager = A.Fake<IImpactMatrixManager>();
            var fakeRewardManager = A.Fake<RewardManager>();
            var fakeStaticWrapper = A.Fake<IStaticWrapper>();

            var sampleRenownBandReward = new RewardPlayerKill
            {
                CrestCount = 1,
                CrestId = 208431,
                Money = 1500
            };
            // Bounty for the character being killed.
            var charBounty = new CharacterBounty
            {
                CharacterLevel = 1,
                RenownLevel = 1,
                LastDeath = 0
            };
            // Impacts upon the target character
            var fakeImpacts = new List<PlayerImpact>
            {
                new PlayerImpact {CharacterId = 999, ExpiryTimestamp = 0, ImpactValue = 500, ModificationValue = 0.35f},
                new PlayerImpact {CharacterId = 1000, ExpiryTimestamp = 0, ImpactValue = 5000, ModificationValue = 0.10f}

            };

            A.CallTo(() => fakeBountyManager.GetBounty(123, true)).Returns(charBounty);
            A.CallTo(() => fakeContributionManager.GetContributionValue(123)).Returns<short>(10);
            A.CallTo(() => fakeImpactMatrixManager.GetKillImpacts(123)).Returns(fakeImpacts);
            A.CallTo(() => fakeImpactMatrixManager.GetTotalImpact(123)).Returns(3350);

            A.CallTo(() => fakeStaticWrapper.GetRenownBandReward(1)).Returns(sampleRenownBandReward);

            var rm = new RewardManager(
                fakeContributionManager,
                fakeStaticWrapper, new List<RewardPlayerKill>
                {
                    sampleRenownBandReward
                },
                (ImpactMatrixManager)fakeImpactMatrixManager);

            // Pass in random number to ensure we effect as expected. StaticRandom.Instance.Next(1, 100)
            //var result = rm.GenerateBaseRewardForKill(123, 99, new Dictionary<uint, Player>());

            //Assert.IsTrue(result.Count == 2);
            //Assert.IsTrue(result[999].RenownBand == 1);
            //Assert.IsTrue(result[999].BaseRP == 11);
            //Assert.IsTrue(result[999].BaseXP == 334);
            //Assert.IsTrue(result[999].BaseInf == 24);
            //Assert.IsTrue(result[999].BaseMoney == 671);

            //Assert.IsTrue(result[1000].RenownBand == 1);
            //Assert.IsTrue(result[1000].BaseRP == 32);
            //Assert.IsTrue(result[1000].BaseXP == 334);
            //Assert.IsTrue(result[1000].BaseInf == 24);
            //Assert.IsTrue(result[1000].BaseMoney == 671);
        }

    }


}
