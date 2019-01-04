using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Remoting;
using Common.Database.World.Battlefront;
using FakeItEasy;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Bounty;
using PlayerContribution = WorldServer.World.Battlefronts.Bounty.PlayerContribution;

namespace WorldServer.Test
{
    [TestClass]
    public class ContributionTests
    {
        public List<ContributionDefinition> ContributionFactorReferenceList { get; set; }

        [TestInitialize]
        public void Init()
        {
            ContributionFactorReferenceList = new List<ContributionDefinition>
            {
                // Special contribution factor.
                new ContributionDefinition
                {
                    ContributionId = 0,
                    ContributionDescription = "Initial",
                    ContributionValue = 0,
                    MaxContributionCount = 1
                },
                new ContributionDefinition
                {
                    ContributionId = 1,
                    ContributionDescription = "PVP Kill",
                    ContributionValue = 1,
                    MaxContributionCount = 10
                },
                new ContributionDefinition
                {
                    ContributionId = 2,
                    ContributionDescription = "BattlefieldObjective Capture",
                    ContributionValue = 3,
                    MaxContributionCount = 4
                }
            };
        }

        [TestMethod]
        public void TestModificationValue()
        {

            var im = new ImpactMatrixManager();
            Assert.IsTrue(Math.Abs(im.CalculateModificationValue(40, 160) - 0.9657842847f) < 0.01);
            Assert.IsTrue(Math.Abs(im.CalculateModificationValue(40, 180) - 0.8685198516) < 0.01);
            Assert.IsTrue(Math.Abs(im.CalculateModificationValue(40, 40) - 3) < 0.01);
            Assert.IsTrue(Math.Abs(im.CalculateModificationValue(360, 40) - 9.965784285) < 0.01);
            Assert.IsTrue(Math.Abs(im.CalculateModificationValue(300, 200) - 3.965784285) < 0.01);
        }

        [TestMethod]
        public void GetContributionOnEmptyManager()
        {
            var contributionManager = new ContributionManager(new ConcurrentDictionary<uint, List<World.Battlefronts.Bounty.PlayerContribution>>(), ContributionFactorReferenceList);
            var result = contributionManager.GetContribution(0);

            Assert.IsNull(result);
            var result2 = contributionManager.GetContribution(1000);
            Assert.IsNull(result2);
        }

        [TestMethod]
        public void GetContributionValueOnEmptyManager()
        {
            var contributionManager = new ContributionManager(new ConcurrentDictionary<uint, List<World.Battlefronts.Bounty.PlayerContribution>>(), ContributionFactorReferenceList);
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

            Assert.IsFalse(contributionManager.RemoveCharacter(100));


        }


        [TestMethod]
        public void RemoveCharacterFromNotEmpty()
        {
            var contributionManager = new ContributionManager(new ConcurrentDictionary<uint, List<PlayerContribution>>(), ContributionFactorReferenceList);
            contributionManager.AddCharacter(100);
            Assert.IsTrue(contributionManager.RemoveCharacter(100));
            var result = contributionManager.GetContributionValue(100);
            Assert.IsTrue(result == 0);

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
            var contributionManager = new ContributionManager(
                new ConcurrentDictionary<uint, List<PlayerContribution>>(), 
                ContributionFactorReferenceList);

            contributionManager.UpdateContribution(100, 1);
            contributionManager.UpdateContribution(100, 2);
            contributionManager.UpdateContribution(100, 2);
            contributionManager.UpdateContribution(100, 1);

            contributionManager.RemoveCharacter(100);

            contributionManager.AddCharacter(100);

            var result = contributionManager.GetContributionValue(100);
            Assert.IsTrue(result == 0);
        }

        [TestMethod]
        public void AddMultipleCharactersThenClear()
        {
            var fakeStaticWrapper = A.Fake<IStaticWrapper>();
            A.CallTo(fakeStaticWrapper).WithReturnType<ContributionDefinition>()
                .Returns(new ContributionDefinition {ContributionValue = 1, ContributionId = 1});
            
            var contributionManager = new ContributionManager(
                new ConcurrentDictionary<uint, List<PlayerContribution>>(), 
                ContributionFactorReferenceList);

            contributionManager.UpdateContribution(100, 1);
            contributionManager.UpdateContribution(102, 2);
            contributionManager.UpdateContribution(104, 2);
            contributionManager.UpdateContribution(104, 1);

            contributionManager.RemoveCharacter(102);
            var result = contributionManager.GetContributionValue(102);
            Assert.IsTrue(result == 0);

            contributionManager.Clear();
            var result2 = contributionManager.GetContributionValue(102);
            Assert.IsTrue(result2 == 0);
            var result3 = contributionManager.GetContributionValue(104);
            Assert.IsTrue(result3 == 0);

        }

        [TestMethod, Ignore]
        public void TimingOnLargeSetUpdates()
        {
            var contributionManager = new ContributionManager(new ConcurrentDictionary<uint, List<PlayerContribution>>(), ContributionFactorReferenceList);

            contributionManager.UpdateContribution(100, 1);
            contributionManager.UpdateContribution(102, 2);
            Stopwatch sw = new Stopwatch();

            sw.Start();
            var result2 = contributionManager.GetContributionValue(102);
            Assert.IsTrue(result2 == 3);
            sw.Stop();

            contributionManager.UpdateContribution(104, 2);
            contributionManager.UpdateContribution(104, 1);
            for (uint i = 0; i < 2000; i++)
            {
                contributionManager.UpdateContribution(i + 200, 1);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 1);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 1);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 1);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 1);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 1);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
                contributionManager.UpdateContribution(i + 200, 2);
            }

            var result = contributionManager.GetContributionValue(102);
            Assert.IsTrue(result == 3);
            sw.Start();
            var result4 = contributionManager.GetContributionValue(200);
            sw.Stop();
            var elapsed = sw.ElapsedMilliseconds;

        }

        [TestMethod]
        public void NoPlayerContributionReturnsPreparedStructure()
        {
            var contributionList = new List<World.Battlefronts.Bounty.PlayerContribution>();
            var contributionManager = new ContributionManager(new ConcurrentDictionary<uint, List<World.Battlefronts.Bounty.PlayerContribution>>(), ContributionFactorReferenceList);

            var result = contributionManager.GetContributionStageDictionary(contributionList, ContributionFactorReferenceList);

            Assert.IsTrue(result.Count == ContributionFactorReferenceList.Count);
            Assert.IsTrue(result[0].ContributionStageSum == 0);
            Assert.IsTrue(result[1].ContributionStageSum == 0);
            Assert.IsTrue(result[2].ContributionStageSum == 0);

            Assert.IsTrue(result[0].ContributionStageCount == 0);
            Assert.IsTrue(result[1].ContributionStageCount == 0);
            Assert.IsTrue(result[2].ContributionStageCount == 0);

            Assert.IsTrue(result[0].ContributionStageMax == ContributionFactorReferenceList[0].MaxContributionCount);
            Assert.IsTrue(result[1].ContributionStageMax == ContributionFactorReferenceList[1].MaxContributionCount);
            Assert.IsTrue(result[2].ContributionStageMax == ContributionFactorReferenceList[2].MaxContributionCount);

        }

        [TestMethod]
        public void SimpleContributionCalculatedCorrectly()
        {
            var contributionList = new List<PlayerContribution>();
            var contributionManager = new ContributionManager(new ConcurrentDictionary<uint, List<PlayerContribution>>(), ContributionFactorReferenceList);

            contributionList.Add(new PlayerContribution {ContributionId = 1, Timestamp = 1111} );

            var response = contributionManager.GetContributionStageDictionary(contributionList, ContributionFactorReferenceList);

            Assert.IsTrue(response[1].ContributionStageCount == 1);
            Assert.IsTrue(response[0].ContributionStageCount == 0);
            Assert.IsTrue(response[2].ContributionStageCount == 0);

            Assert.IsTrue(response[1].ContributionStageSum == 1);
            Assert.IsTrue(response[0].ContributionStageSum == 0);
            Assert.IsTrue(response[2].ContributionStageSum == 0);
         
        }

        [TestMethod]
        public void ComplexContributionNoMax()
        {
            var contributionList = new List<PlayerContribution>();
            var contributionManager = new ContributionManager(new ConcurrentDictionary<uint, List<PlayerContribution>>(), ContributionFactorReferenceList);

            contributionList.Add(new PlayerContribution { ContributionId = 1, Timestamp = 1111 });
            contributionList.Add(new PlayerContribution { ContributionId = 1, Timestamp = 1111 });
            contributionList.Add(new PlayerContribution { ContributionId = 1, Timestamp = 1111 });
            contributionList.Add(new PlayerContribution { ContributionId = 1, Timestamp = 1111 });
            contributionList.Add(new PlayerContribution { ContributionId = 1, Timestamp = 1111 });
            contributionList.Add(new PlayerContribution { ContributionId = 1, Timestamp = 1111 });
            contributionList.Add(new PlayerContribution { ContributionId = 1, Timestamp = 1111 });

            contributionList.Add(new PlayerContribution { ContributionId = 2, Timestamp = 1111 });
            contributionList.Add(new PlayerContribution { ContributionId = 2, Timestamp = 1111 });
            contributionList.Add(new PlayerContribution { ContributionId = 2, Timestamp = 1111 });

            var response = contributionManager.GetContributionStageDictionary(contributionList, ContributionFactorReferenceList);

            Assert.IsTrue(response[1].ContributionStageCount == 7);
            Assert.IsTrue(response[0].ContributionStageCount == 0);
            Assert.IsTrue(response[2].ContributionStageCount == 3);

            Assert.IsTrue(response[1].ContributionStageSum == 7);
            Assert.IsTrue(response[0].ContributionStageSum == 0);
            Assert.IsTrue(response[2].ContributionStageSum == 9);

        }

        [TestMethod]
        public void ComplexContributionWithMax()
        {
            var contributionList = new List<PlayerContribution>();
            var contributionManager = new ContributionManager(new ConcurrentDictionary<uint, List<PlayerContribution>>(), ContributionFactorReferenceList);

            contributionList.Add(new PlayerContribution { ContributionId = 1, Timestamp = 1111 });
            contributionList.Add(new PlayerContribution { ContributionId = 1, Timestamp = 1111 });
            contributionList.Add(new PlayerContribution { ContributionId = 1, Timestamp = 1111 });
            contributionList.Add(new PlayerContribution { ContributionId = 1, Timestamp = 1111 });
            contributionList.Add(new PlayerContribution { ContributionId = 1, Timestamp = 1111 });
            contributionList.Add(new PlayerContribution { ContributionId = 1, Timestamp = 1111 });
            contributionList.Add(new PlayerContribution { ContributionId = 1, Timestamp = 1111 });
            contributionList.Add(new PlayerContribution { ContributionId = 1, Timestamp = 1111 });
            contributionList.Add(new PlayerContribution { ContributionId = 1, Timestamp = 1111 });
            contributionList.Add(new PlayerContribution { ContributionId = 1, Timestamp = 1111 });
            contributionList.Add(new PlayerContribution { ContributionId = 1, Timestamp = 1111 });
            contributionList.Add(new PlayerContribution { ContributionId = 1, Timestamp = 1111 });
            contributionList.Add(new PlayerContribution { ContributionId = 1, Timestamp = 1111 });

            contributionList.Add(new PlayerContribution { ContributionId = 2, Timestamp = 1111 });
            contributionList.Add(new PlayerContribution { ContributionId = 2, Timestamp = 1111 });
            contributionList.Add(new PlayerContribution { ContributionId = 2, Timestamp = 1111 });
            contributionList.Add(new PlayerContribution { ContributionId = 2, Timestamp = 1111 });
            contributionList.Add(new PlayerContribution { ContributionId = 2, Timestamp = 1111 });

            var response = contributionManager.GetContributionStageDictionary(contributionList, ContributionFactorReferenceList);

            // maxes out at 10 count
            Assert.IsTrue(response[1].ContributionStageCount == 10);
            Assert.IsTrue(response[0].ContributionStageCount == 0);
            Assert.IsTrue(response[2].ContributionStageCount == 4);

            Assert.IsTrue(response[1].ContributionStageSum == 10);
            Assert.IsTrue(response[0].ContributionStageSum == 0);
            Assert.IsTrue(response[2].ContributionStageSum ==12);

        }
    }
}