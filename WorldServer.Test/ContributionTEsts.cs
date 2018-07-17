using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WorldServer.World.Battlefronts.Bounty;

namespace WorldServer.Test
{
    [TestClass]
    public class ContributionTests
    {
        public List<ContributionFactor> ContributionFactorReferenceList { get; set; }

        [TestInitialize]
        public void Init()
        {
            ContributionFactorReferenceList = new List<ContributionFactor>
            {
                // Special contribution factor.
                new ContributionFactor
                {
                    ContributionId = 0,
                    ContributionDescription = "Initial",
                    ContributionValue = 0,
                    MaxContributionCount = 1
                },
                new ContributionFactor
                {
                    ContributionId = 1,
                    ContributionDescription = "PVP Kill",
                    ContributionValue = 1,
                    MaxContributionCount = 10
                },
                new ContributionFactor
                {
                    ContributionId = 2,
                    ContributionDescription = "BO Capture",
                    ContributionValue = 3,
                    MaxContributionCount = 4
                }
            };
        }

        [TestMethod]
        public void GetContributionOnEmptyManager()
        {
            var contributionManager = new ContributionManager(new ConcurrentDictionary<uint, List<PlayerContribution>>(), ContributionFactorReferenceList);
            var result = contributionManager.GetContribution(0);

            Assert.IsNull(result);
            var result2 = contributionManager.GetContribution(1000);
            Assert.IsNull(result2);
        }

        [TestMethod]
        public void GetContributionValueOnEmptyManager()
        {
            var contributionManager = new ContributionManager(new ConcurrentDictionary<uint, List<PlayerContribution>>(), ContributionFactorReferenceList);
            var result = contributionManager.GetContributionValue(0);

            Assert.IsTrue(result == 0);
        }

        [TestMethod]
        public void GetContributionValueWithNoFactorList()
        {
            var contributionManager = new ContributionManager(new ConcurrentDictionary<uint, List<PlayerContribution>>(), null);
            var result = contributionManager.GetContributionValue(0);

            Assert.IsTrue(result == 0);
        }

        [TestMethod]
        public void AddNewPlayerGetContribution()
        {
            var contributionManager = new ContributionManager(new ConcurrentDictionary<uint, List<PlayerContribution>>(), ContributionFactorReferenceList);

            contributionManager.AddCharacter(100);
            var result = contributionManager.GetContributionValue(100);

            Assert.IsTrue(result == 0);
        }

        [TestMethod]
        public void AddNewPlayerAddContribution()
        {
            var contributionManager = new ContributionManager(new ConcurrentDictionary<uint, List<PlayerContribution>>(), ContributionFactorReferenceList);

            contributionManager.AddCharacter(100);
            contributionManager.UpdateContribution(100, 1);
            var result = contributionManager.GetContributionValue(100);
            Assert.IsTrue(result == 1);

            contributionManager.UpdateContribution(100, 1);
            var result2 = contributionManager.GetContributionValue(100);
            Assert.IsTrue(result2 == 2);

            // Has a value of 3
            contributionManager.UpdateContribution(100, 2);
            var result3 = contributionManager.GetContributionValue(100);
            Assert.IsTrue(result3 == 5);

        }

        [TestMethod]
        public void RemoveCharacterFromEmpty()
        {
            var contributionManager = new ContributionManager(new ConcurrentDictionary<uint, List<PlayerContribution>>(), ContributionFactorReferenceList);

           Assert.IsFalse( contributionManager.RemoveCharacter(100));
         

        }


        [TestMethod]
        public void RemoveCharacterFromNotEmpty()
        {
            var contributionManager = new ContributionManager(new ConcurrentDictionary<uint, List<PlayerContribution>>(), ContributionFactorReferenceList);
            contributionManager.AddCharacter(100);
            Assert.IsTrue(contributionManager.RemoveCharacter(100));
            var result = contributionManager.GetContributionValue(100);
            Assert.IsTrue(result ==0);

        }


        [TestMethod]
        public void RemoveCharacterFromNotEmptyWrongId()
        {
            var contributionManager = new ContributionManager(new ConcurrentDictionary<uint, List<PlayerContribution>>(), ContributionFactorReferenceList);
            contributionManager.AddCharacter(100);
            Assert.IsFalse(contributionManager.RemoveCharacter(101));

            contributionManager.UpdateContribution(100, 1);
            var result = contributionManager.GetContributionValue(100);
            Assert.IsTrue(result == 1);

            Assert.IsTrue(contributionManager.RemoveCharacter(100));


            contributionManager.UpdateContribution(100, 1);
            var result2 = contributionManager.GetContributionValue(100);
            Assert.IsTrue(result2 == 1);
        }

        [TestMethod]
        public void UpdateCharacterNoAdd()
        {
            var contributionManager = new ContributionManager(new ConcurrentDictionary<uint, List<PlayerContribution>>(), ContributionFactorReferenceList);

            contributionManager.UpdateContribution(100, 1);
            contributionManager.UpdateContribution(100, 2);
            contributionManager.UpdateContribution(100, 2);
            contributionManager.UpdateContribution(100, 1);

            var result = contributionManager.GetContributionValue(100);
            Assert.IsTrue(result == 8);

            
        }

        [TestMethod]
        public void UpdateCharacterThenAdd()
        {
            var contributionManager = new ContributionManager(new ConcurrentDictionary<uint, List<PlayerContribution>>(), ContributionFactorReferenceList);

            contributionManager.UpdateContribution(100, 1);
            contributionManager.UpdateContribution(100, 2);
            contributionManager.UpdateContribution(100, 2);
            contributionManager.UpdateContribution(100, 1);

            contributionManager.AddCharacter(100);

            var result = contributionManager.GetContributionValue(100);
            Assert.IsTrue(result == 8);
        }

        [TestMethod]
        public void UpdateCharacterThenRemoveThenAdd()
        {
            var contributionManager = new ContributionManager(new ConcurrentDictionary<uint, List<PlayerContribution>>(), ContributionFactorReferenceList);

            contributionManager.UpdateContribution(100, 1);
            contributionManager.UpdateContribution(100, 2);
            contributionManager.UpdateContribution(100, 2);
            contributionManager.UpdateContribution(100, 1);

            contributionManager.RemoveCharacter(100);

            contributionManager.AddCharacter(100);

            var result = contributionManager.GetContributionValue(100);
            Assert.IsTrue(result ==0 );
        }
    }
}