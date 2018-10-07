using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using WorldServer.World.Battlefronts.Bounty;

namespace WorldServer.Test
{
    [TestClass]
    public class RewardTests
    {

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
                fakeBountyManager,
                fakeContributionManager,
                fakeImpactMatrixManager,
                fakeStaticWrapper, new List<RewardPlayerKill>
                {
                    sampleRenownBandReward
                });



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
                fakeBountyManager,
                fakeContributionManager,
                fakeImpactMatrixManager,
                fakeStaticWrapper, new List<RewardPlayerKill>
                {
                    sampleRenownBandReward
                });

            var insigniaRewards = rm.GetInsigniaRewards(fakeImpacts, sampleRenownBandReward, 5500, 100);
            Assert.IsTrue(insigniaRewards.Count == 0);

            var insigniaRewards2 = rm.GetInsigniaRewards(fakeImpacts, sampleRenownBandReward, 5500, 1);
            Assert.IsTrue(insigniaRewards2.Count == 2);
            insigniaRewards2.TryGetValue(999, out var insigniaValue);
            Assert.IsTrue(insigniaValue == 208431);
            insigniaRewards2.TryGetValue(1000, out var insigniaValue2);
            Assert.IsTrue(insigniaValue2 == 208431);

            // Less than 90% chance (500/5000)*100
            var insigniaRewards3 = rm.GetInsigniaRewards(fakeImpacts, sampleRenownBandReward, 5500, 8);
            Assert.IsTrue(insigniaRewards3.Count == 2);
            insigniaRewards3.TryGetValue(999, out var insigniaValue3);
            Assert.IsTrue(insigniaValue3 == 208431);
            insigniaRewards3.TryGetValue(1000, out var insigniaValue4);
            Assert.IsTrue(insigniaValue4 == 208431);

            // More than 90% chance (500/5000)
            var insigniaRewards4 = rm.GetInsigniaRewards(fakeImpacts, sampleRenownBandReward, 5500, 28);
            Assert.IsTrue(insigniaRewards4.Count == 1);
            insigniaRewards4.TryGetValue(999, out var insigniaValue6);
            Assert.IsTrue(insigniaValue6 == 0);
            insigniaRewards4.TryGetValue(1000, out var insigniaValue7);
            Assert.IsTrue(insigniaValue7 == 208431);

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
                fakeBountyManager,
                fakeContributionManager,
                fakeImpactMatrixManager,
                fakeStaticWrapper, new List<RewardPlayerKill>
                {
                    sampleRenownBandReward
                });

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
