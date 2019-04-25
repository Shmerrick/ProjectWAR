using System;
using GameData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldServer.World.Battlefronts.Apocalypse;

namespace WorldServer.Test
{
    [TestClass]
    public class CalculateObjectiveRewardScaleTest
    {
        [TestMethod]
        public void EqualPlayerNumbers()
        {
            var ndBO = new BattlefieldObjective();

            var objectiveMultiplierDest = ndBO.RewardManager.CalculateObjectiveRewardScale(Realms.REALMS_REALM_DESTRUCTION, 10, 10);

            Assert.IsTrue(Math.Abs(objectiveMultiplierDest - .69321472f) < 0.1);

            var objectiveMultiplierNeut = ndBO.RewardManager.CalculateObjectiveRewardScale(Realms.REALMS_REALM_NEUTRAL, 10, 10);

            Assert.IsTrue(objectiveMultiplierNeut == 0);

            var objectiveMultiplierOrder = ndBO.RewardManager.CalculateObjectiveRewardScale(Realms.REALMS_REALM_ORDER, 10, 10);

            Assert.IsTrue(Math.Abs(objectiveMultiplierOrder - .69321472f) < 0.1);

            var objectiveMultiplierSomethingElse = ndBO.RewardManager.CalculateObjectiveRewardScale(Realms.REALMS_REALM_HOSTILE, 10, 10);

            Assert.IsTrue(objectiveMultiplierSomethingElse == 0);
        }

        [TestMethod]
        public void SmallDestroDefendingBO()
        {
            var ndBO = new BattlefieldObjective();

            var objectiveMultiplierDest = ndBO.RewardManager.CalculateObjectiveRewardScale(Realms.REALMS_REALM_DESTRUCTION, 50, 5);

            Assert.IsTrue(objectiveMultiplierDest == 1.0f);
        }

        [TestMethod]
        public void SmallOrderDefendingBO()
        {
            var ndBO = new BattlefieldObjective();

            var objectiveMultiplierOrder = ndBO.RewardManager.CalculateObjectiveRewardScale(Realms.REALMS_REALM_ORDER, 5, 50);

            Assert.IsTrue(objectiveMultiplierOrder == 1f);
        }

        [TestMethod]
        public void SmallDestroAttackingBO()
        {
            var ndBO = new BattlefieldObjective();

            var objectiveMultiplierDest = ndBO.RewardManager.CalculateObjectiveRewardScale(Realms.REALMS_REALM_DESTRUCTION, 5, 50);

            Assert.IsTrue(Math.Abs(objectiveMultiplierDest - .4054651f) < 0.1);
        }

        [TestMethod]
        public void SmallOrderAttackingBO()
        {
            var ndBO = new BattlefieldObjective();

            var objectiveMultiplierOrder = ndBO.RewardManager.CalculateObjectiveRewardScale(Realms.REALMS_REALM_ORDER, 50, 5);

            Assert.IsTrue(Math.Abs(objectiveMultiplierOrder - .4054651f) < 0.1);
        }

    }
}
