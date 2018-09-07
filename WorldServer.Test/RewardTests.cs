using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Bounty;
using WorldServer.World.Battlefronts.NewDawn.Rewards;

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

            var sampleRenownBandReward = new RenownBandReward
            {
                CrestCount = 1,
                CrestId = 208431,
                Money = 1500
            };
            // Bounty for the character being killed.
            var charBounty = new CharacterBounty
            {
                EffectiveLevel = 60,
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

            A.CallTo(() => fakeBountyManager.GetBounty(123)).Returns(charBounty);
            A.CallTo(() => fakeContributionManager.GetContributionValue(123)).Returns<short>(100);
            A.CallTo(() => fakeImpactMatrixManager.GetKillImpacts(123)).Returns(fakeImpacts);
            A.CallTo(() => fakeImpactMatrixManager.GetTotalImpact(123)).Returns(3350);

            A.CallTo(() => fakeStaticWrapper.GetRenownBandReward(30)).Returns(sampleRenownBandReward);

            var rm = new RewardManager(
                fakeBountyManager,
                fakeContributionManager,
                fakeImpactMatrixManager,
                fakeStaticWrapper, new List<RenownBandReward>
                {
                    sampleRenownBandReward
                });

            // Pass in random number to ensure we effect as expected. StaticRandom.Instance.Next(1, 100)
            var result = rm.GenerateBaseReward(123, 99);

            Assert.IsTrue(result.Count == 3);
            Assert.IsTrue(result[999].RenownBand == 30);
            Assert.IsTrue(result[999].BaseRP == 152);
            Assert.IsTrue(result[999].BaseXP == 334);
            Assert.IsTrue(result[999].BaseInf == 24);
            Assert.IsTrue(result[999].BaseMoney == 671);
        }


        [TestMethod, Ignore]
        public void Replicate_Excel_TestCaseCEKillsTom()
        {
            var fakeBountyManager = A.Fake<IBountyManager>();
            var fakeContributionManager = A.Fake<IContributionManager>();
            var fakeImpactMatrixManager = A.Fake<IImpactMatrixManager>();
            var fakeRewardManager = A.Fake<RewardManager>();
            var fakeStaticWrapper = A.Fake<IStaticWrapper>();

            var sampleRenownBandReward = new RenownBandReward
            {
                CrestCount = 1,
                CrestId = 208431,
                Money = 1500
            };
            // Bounty for the character being killed.
            var charBounty = new CharacterBounty
            {
                EffectiveLevel = 3,
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

            A.CallTo(() => fakeBountyManager.GetBounty(123)).Returns(charBounty);
            A.CallTo(() => fakeContributionManager.GetContributionValue(123)).Returns<short>(10);
            A.CallTo(() => fakeImpactMatrixManager.GetKillImpacts(123)).Returns(fakeImpacts);
            A.CallTo(() => fakeImpactMatrixManager.GetTotalImpact(123)).Returns(3350);

            A.CallTo(() => fakeStaticWrapper.GetRenownBandReward(1)).Returns(sampleRenownBandReward);

            var rm = new RewardManager(
                fakeBountyManager,
                fakeContributionManager,
                fakeImpactMatrixManager,
                fakeStaticWrapper, new List<RenownBandReward>
                {
                    sampleRenownBandReward
                });

            // Pass in random number to ensure we effect as expected. StaticRandom.Instance.Next(1, 100)
            var result = rm.GenerateBaseReward(123, 99);

            Assert.IsTrue(result.Count == 2);
            Assert.IsTrue(result[999].RenownBand == 1);
            Assert.IsTrue(result[999].BaseRP == 11);
            Assert.IsTrue(result[999].BaseXP == 334);
            Assert.IsTrue(result[999].BaseInf == 24);
            Assert.IsTrue(result[999].BaseMoney == 671);

            Assert.IsTrue(result[1000].RenownBand == 1);
            Assert.IsTrue(result[1000].BaseRP == 32);
            Assert.IsTrue(result[1000].BaseXP == 334);
            Assert.IsTrue(result[1000].BaseInf == 24);
            Assert.IsTrue(result[1000].BaseMoney == 671);
        }

    }


}
