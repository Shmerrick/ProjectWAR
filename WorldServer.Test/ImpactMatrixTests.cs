using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldServer.World.Battlefronts.Bounty;
using WorldServer.World.Objects;


namespace WorldServer.Test
{
    [TestClass]
    public class ImpactMatrixManagerTests
    {
        public Player TargetPlayer { get; set; }
        public PlayerImpact SamplePlayerImpact { get; set; }
        private static readonly DateTime EpochDateTime = new DateTime(1970, 1, 1);
        public DateTime BaseDateTime { get; set; }


        [TestInitialize]
        public void Init()
        {
            // Use a base date to compare timed events against.
            BaseDateTime = DateTime.UtcNow;

            SamplePlayerImpact = new PlayerImpact
            {
                ImpactValue = 500,
                ExpiryTimestamp = (int)(BaseDateTime - EpochDateTime).TotalSeconds,
                ModificationValue = 1.2f,
                CharacterId = 999
            };
        }

        [TestMethod]
        public void AddSingleImpactWorks()
        {
            var im = new ImpactMatrixManager();
            var returnValue = im.UpdateMatrix(123, SamplePlayerImpact);

            Assert.IsNotNull(returnValue);
            Assert.IsTrue(returnValue.ImpactValue == 500);
            Assert.IsTrue(returnValue.ModificationValue == 1.2f);
            Assert.IsTrue(returnValue.ExpiryTimestamp == SamplePlayerImpact.ExpiryTimestamp);

        }

        [TestMethod]
        public void AddSingleImpact_ReadBackWorks()
        {
            var im = new ImpactMatrixManager();
            var returnValue = im.UpdateMatrix(123, SamplePlayerImpact);

            Assert.IsNotNull(returnValue);

            var returnValue2 = im.ImpactMatrix[123];
            Assert.IsNotNull(returnValue2);
            Assert.IsTrue(returnValue2[0].ImpactValue == 500);
            Assert.IsTrue(returnValue2[0].ModificationValue == 1.2f);
            Assert.IsTrue(returnValue2[0].ExpiryTimestamp == SamplePlayerImpact.ExpiryTimestamp);

        }

        [TestMethod, ExpectedException(typeof(KeyNotFoundException))]
        public void EmptyReadThrowsException()
        {
            var im = new ImpactMatrixManager();
            var returnValue2 = im.ImpactMatrix[123];
            Assert.IsNull(returnValue2);
        }

        [TestMethod, ExpectedException(typeof(KeyNotFoundException))]
        public void NonExistantKeyThrowsException()
        {
            var im = new ImpactMatrixManager();
            var returnValue = im.UpdateMatrix(123, SamplePlayerImpact);

            Assert.IsNotNull(returnValue);
            var returnValue2 = im.ImpactMatrix[124];
            Assert.IsNotNull(returnValue2);
        }

        [TestMethod]
        public void DoubleImpactUpdatesAsExpected()
        {
            var im = new ImpactMatrixManager();
            var returnValue = im.UpdateMatrix(123, SamplePlayerImpact);

            Assert.IsNotNull(returnValue);
            Assert.IsTrue(returnValue.ImpactValue == 500);
            Assert.IsTrue(returnValue.ModificationValue == 1.2f);
            Assert.IsTrue(returnValue.ExpiryTimestamp == SamplePlayerImpact.ExpiryTimestamp);

            var returnValue2 = im.UpdateMatrix(123, SamplePlayerImpact);
            Assert.IsNotNull(returnValue2);
            Assert.IsTrue(returnValue2.ImpactValue == 1000);
            Assert.IsTrue(returnValue2.ModificationValue == 1.2f);
            Assert.IsTrue(returnValue2.ExpiryTimestamp == SamplePlayerImpact.ExpiryTimestamp);

        }

        [TestMethod]
        public void ExpireEmptyImpactMatrixDoesNotFail()
        {
            var im = new ImpactMatrixManager();
            var numberRemoved = im.ExpireImpacts((int)(BaseDateTime - EpochDateTime).TotalSeconds + 60);
            Assert.IsTrue(numberRemoved == 0);
        }

        [TestMethod]
        public void ExpireDoesNotEffectCurrentImpacts()
        {
            var im = new ImpactMatrixManager();
            var impact1 = (PlayerImpact)SamplePlayerImpact.Clone();
            impact1.ExpiryTimestamp = (int)(BaseDateTime - EpochDateTime).TotalSeconds + 60;
            var returnValue = im.UpdateMatrix(123, impact1);
            var numberRemoved = im.ExpireImpacts((int)(BaseDateTime - EpochDateTime).TotalSeconds);
            Assert.IsTrue(numberRemoved == 0);
        }

        [TestMethod]
        public void ExpireDoesEffectsExpiredImpacts()
        {
            var im = new ImpactMatrixManager();
            SamplePlayerImpact.ExpiryTimestamp = (int)(BaseDateTime - EpochDateTime).TotalSeconds - 120;
            var returnValue = im.UpdateMatrix(123, SamplePlayerImpact);
            var numberRemoved = im.ExpireImpacts((int)(BaseDateTime - EpochDateTime).TotalSeconds + 60);
            Assert.IsTrue(numberRemoved == 1);

        }
        [TestMethod, ExpectedException(typeof(KeyNotFoundException))]
        public void ExpiryRemovesImpacts()
        {
            var im = new ImpactMatrixManager();
            SamplePlayerImpact.ExpiryTimestamp = (int)(BaseDateTime - EpochDateTime).TotalSeconds - 120;
            var returnValue = im.UpdateMatrix(123, SamplePlayerImpact);
            var numberRemoved = im.ExpireImpacts((int)(BaseDateTime - EpochDateTime).TotalSeconds + 60);
            var returnValue2 = im.ImpactMatrix[123];

        }
        [TestMethod]
        public void ExpiryRemovesOnlyExpiredImpacts()
        {
            var im = new ImpactMatrixManager();

            var impact1 = (PlayerImpact)SamplePlayerImpact.Clone();
            impact1.ExpiryTimestamp = (int)(BaseDateTime - EpochDateTime).TotalSeconds - 120;
            var returnValue1 = im.UpdateMatrix(123, impact1);

            var impact2 = (PlayerImpact)SamplePlayerImpact.Clone();
            impact2.ExpiryTimestamp = (int)(BaseDateTime - EpochDateTime).TotalSeconds - 120;
            var returnValue2 = im.UpdateMatrix(124, impact2);

            // Back to "now"
            var impact3 = (PlayerImpact)SamplePlayerImpact.Clone();
            impact3.ExpiryTimestamp = (int)(BaseDateTime - EpochDateTime).TotalSeconds + 60;
            var returnValue3 = im.UpdateMatrix(125, impact3);

            var numberRemoved = im.ExpireImpacts((int)(BaseDateTime - EpochDateTime).TotalSeconds);
            Assert.IsTrue(numberRemoved == 2);
            // Expect this to work.
            var validImpact = im.ImpactMatrix[125];
            Assert.IsTrue(validImpact.Count == 1);
            Assert.IsTrue(validImpact[0].CharacterId == 999);
        }


        [TestMethod]
        public void AddMultipleCharacterImpacts()
        {
            var im = new ImpactMatrixManager();

            var impact1 = (PlayerImpact)SamplePlayerImpact.Clone();
            var returnValue1 = im.UpdateMatrix(123, impact1);

            var impact2 = (PlayerImpact)SamplePlayerImpact.Clone();
            var returnValue2 = im.UpdateMatrix(123, impact2);

            var impact3 = (PlayerImpact)SamplePlayerImpact.Clone();
            impact3.CharacterId = 998;
            impact3.ImpactValue = 1200;
            // Now have 2 impacts from 999 (additive) and one from 998
            var returnValue3 = im.UpdateMatrix(123, impact3);

            var impact4 = (PlayerImpact)SamplePlayerImpact.Clone();
            var returnValue4 = im.UpdateMatrix(123, impact4);

            var impactList = im.ImpactMatrix[123];

            foreach (var playerImpact in impactList)
            {
                if (playerImpact.CharacterId == 998)
                {
                    Assert.IsTrue(playerImpact.ImpactValue == 1200);
                }
                if (playerImpact.CharacterId == 999)
                {
                    Assert.IsTrue(playerImpact.ImpactValue == 1500);
                }
            }
        }

        [TestMethod]
        public void FullHealClearsImpactList()
        {
            var im = new ImpactMatrixManager();

            var impact1 = (PlayerImpact)SamplePlayerImpact.Clone();
            var returnValue1 = im.UpdateMatrix(123, impact1);

            var impact2 = (PlayerImpact)SamplePlayerImpact.Clone();
            var returnValue2 = im.UpdateMatrix(123, impact2);

            var impact3 = (PlayerImpact)SamplePlayerImpact.Clone();
            impact3.CharacterId = 998;
            impact3.ImpactValue = 1200;
            // Now have 2 impacts from 999 (additive) and one from 998
            var returnValue3 = im.UpdateMatrix(123, impact3);

            var impact4 = (PlayerImpact)SamplePlayerImpact.Clone();
            var returnValue4 = im.UpdateMatrix(123, impact4);

            Assert.IsTrue(im.ImpactMatrix[123].Count == 2);

            im.FullHeal(123);

            Assert.IsTrue(im.ImpactMatrix[123].Count == 0);

        }

        [TestMethod]
        public void ReturnGetKillImpacts()
        {
            var im = new ImpactMatrixManager();

            var impact1 = (PlayerImpact)SamplePlayerImpact.Clone();
            var returnValue1 = im.UpdateMatrix(123, impact1);

            var impact2 = (PlayerImpact)SamplePlayerImpact.Clone();
            var returnValue2 = im.UpdateMatrix(123, impact2);

            var impact3 = (PlayerImpact)SamplePlayerImpact.Clone();
            impact3.CharacterId = 998;
            impact3.ImpactValue = 1200;
            // Now have 2 impacts from 999 (additive) and one from 998
            var returnValue3 = im.UpdateMatrix(123, impact3);
            var impact4 = (PlayerImpact)SamplePlayerImpact.Clone();
            var returnValue4 = im.UpdateMatrix(123, impact4);

            Assert.IsTrue(im.ImpactMatrix[123].Count == 2);

            var killImpacts = im.GetKillImpacts(123);

            Assert.IsTrue(killImpacts.Count == 2);

        }

        [TestMethod]
        public void ReturnGetKillImpactsLargeList()
        {
            var im = new ImpactMatrixManager();
            // Create 20 impacts
            for (int i = 0; i < 20; i++)
            {
                var impact3 = (PlayerImpact)SamplePlayerImpact.Clone();
                impact3.CharacterId = (uint)(10000 + i);
                im.UpdateMatrix(123, impact3);
            }
            var killImpacts = im.GetKillImpacts(123);
            Assert.IsTrue(killImpacts.Count == 20);

            var impact4 = (PlayerImpact)SamplePlayerImpact.Clone();
            impact4.CharacterId = (uint)(20000);
            im.UpdateMatrix(123, impact4);

            var killImpacts2 = im.GetKillImpacts(123);
            Assert.IsTrue(killImpacts2.Count == 20);

        }

        [TestMethod, ExpectedException(typeof(KeyNotFoundException))]
        public void AddSingleImpactUnderMinimumNotAdded()
        {
            var im = new ImpactMatrixManager();
            var impact4 = (PlayerImpact)SamplePlayerImpact.Clone();
            impact4.CharacterId = (uint)(20000);
            impact4.ImpactValue = 20;
            im.UpdateMatrix(123, impact4);

            // Expect this to throw an exception as the amount was too low and not added.
            var returnValue2 = im.ImpactMatrix[123];
        }

        [TestMethod]
        public void AddLowImpactToExistingListNotAdded()
        {
            var im = new ImpactMatrixManager();
            var impact3 = (PlayerImpact)SamplePlayerImpact.Clone();
            impact3.CharacterId = (uint)(20000);
            impact3.ImpactValue = 2000;
            im.UpdateMatrix(123, impact3);

            var impact4 = (PlayerImpact)SamplePlayerImpact.Clone();
            impact4.CharacterId = (uint)(20000);
            impact4.ImpactValue = 20;
            im.UpdateMatrix(123, impact4);
            var returnValue2 = im.ImpactMatrix[123];
            Assert.IsTrue(returnValue2.Count == 1);
            Assert.IsTrue(returnValue2[0].ImpactValue == 2000);
        }
    }
}
