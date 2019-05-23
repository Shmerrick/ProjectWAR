using System.Collections.Concurrent;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Common.Database.World.Battlefront;
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
        public void AssignLootSortingIsCorrectToBagsContribution()
        {

            var sortedPairs = new List<KeyValuePair<uint, int>>
            {
                new KeyValuePair<uint, int>(50000, 124),
                new KeyValuePair<uint, int>(50020, 100),
                new KeyValuePair<uint, int>(12332, 78),
                new KeyValuePair<uint, int>(42223, 77),
                new KeyValuePair<uint, int>(73738, 50),
                new KeyValuePair<uint, int>(12202, 12),
                new KeyValuePair<uint, int>(12902, 4),
                new KeyValuePair<uint, int>(12123, 0),
                new KeyValuePair<uint, int>(33222, 220)
            };
            var rewardAssigner = new  RewardAssigner(StaticRandom.Instance, _logger);
            var numberOfAwards = 4;

            var x = new LootBagTypeDefinition().BuildLootBagTypeDefinitions(numberOfAwards);
            var bonusList = new List<RVRPlayerBagBonus>();
            var randomRolls = new Dictionary<uint, int>( );
            var pairingContributions= new Dictionary<uint, int> {{50000, 100}};
            var assignedLoot = rewardAssigner.AssignLootToPlayers(numberOfAwards, x, sortedPairs, bonusList, randomRolls, pairingContributions);

            Assert.IsTrue(numberOfAwards == assignedLoot.Count);
            Assert.IsTrue(assignedLoot[0].Assignee == 50000);
            Assert.IsTrue(assignedLoot[1].Assignee == 33222);
            Assert.IsTrue(assignedLoot[2].Assignee == 50020);
            Assert.IsTrue(assignedLoot[3].Assignee == 12332);
        }

        [TestMethod]
        public void AssignLootSortingIsCorrectToBagsRandom()
        {

            var sortedPairs = new List<KeyValuePair<uint, int>>
            {
                new KeyValuePair<uint, int>(50000, 124),
                new KeyValuePair<uint, int>(50020, 100),
                new KeyValuePair<uint, int>(12332, 78),
                new KeyValuePair<uint, int>(42223, 77),
                new KeyValuePair<uint, int>(73738, 50),
                new KeyValuePair<uint, int>(12202, 12),
                new KeyValuePair<uint, int>(12902, 4),
                new KeyValuePair<uint, int>(12123, 0),
                new KeyValuePair<uint, int>(33222, 220)
            };
            var rewardAssigner = new  RewardAssigner(StaticRandom.Instance, _logger);
            var numberOfAwards = 4;

            var x = new LootBagTypeDefinition().BuildLootBagTypeDefinitions(numberOfAwards);
            var bonusList = new List<RVRPlayerBagBonus>();
            var randomRolls = new Dictionary<uint, int> {{50000, 100}};
            var pairingContributions = new Dictionary<uint, int>();
            var assignedLoot = rewardAssigner.AssignLootToPlayers(numberOfAwards, x, sortedPairs, bonusList, randomRolls, pairingContributions);

            Assert.IsTrue(numberOfAwards == assignedLoot.Count);
            Assert.IsTrue(assignedLoot[0].Assignee == 50000);
            Assert.IsTrue(assignedLoot[1].Assignee == 33222);
            Assert.IsTrue(assignedLoot[2].Assignee == 50020);
            Assert.IsTrue(assignedLoot[3].Assignee == 12332);
        }

        [TestMethod]
        public void AssignLootSortingIsCorrectToBagsRandomMultiple()
        {

            var sortedPairs = new List<KeyValuePair<uint, int>>
            {
                new KeyValuePair<uint, int>(50000, 124),
                new KeyValuePair<uint, int>(50020, 100),
                new KeyValuePair<uint, int>(12332, 78),
                new KeyValuePair<uint, int>(42223, 77),
                new KeyValuePair<uint, int>(73738, 50),
                new KeyValuePair<uint, int>(12202, 12),
                new KeyValuePair<uint, int>(12902, 4),
                new KeyValuePair<uint, int>(12123, 0),
                new KeyValuePair<uint, int>(33222, 220)
            };
            var rewardAssigner = new  RewardAssigner(StaticRandom.Instance, _logger);
            var numberOfAwards = 4;

            var x = new LootBagTypeDefinition().BuildLootBagTypeDefinitions(numberOfAwards);
            var bonusList = new List<RVRPlayerBagBonus>();
            var randomRolls = new Dictionary<uint, int> {{50000, 100}, {12202, 200}};
            var pairingContributions = new Dictionary<uint, int>();
            var assignedLoot = rewardAssigner.AssignLootToPlayers(numberOfAwards, x, sortedPairs, bonusList, randomRolls, pairingContributions);

            Assert.IsTrue(numberOfAwards == assignedLoot.Count);
            Assert.IsTrue(assignedLoot[0].Assignee == 50000);
            Assert.IsTrue(assignedLoot[1].Assignee == 33222);
            Assert.IsTrue(assignedLoot[2].Assignee == 12202);
            Assert.IsTrue(assignedLoot[3].Assignee == 50020);
        }


        [TestMethod]
        public void AssignLootSortingIsCorrectToBags()
        {

            var sortedPairs = new List<KeyValuePair<uint, int>>
            {
                new KeyValuePair<uint, int>(50000, 124),
                new KeyValuePair<uint, int>(50020, 100),
                new KeyValuePair<uint, int>(12332, 78),
                new KeyValuePair<uint, int>(42223, 77),
                new KeyValuePair<uint, int>(73738, 50),
                new KeyValuePair<uint, int>(12202, 12),
                new KeyValuePair<uint, int>(12902, 4),
                new KeyValuePair<uint, int>(12123, 0),
                new KeyValuePair<uint, int>(33222, 220)
            };
            var rewardAssigner = new  RewardAssigner(StaticRandom.Instance, _logger);
            var numberOfAwards = 4;

            var x = new LootBagTypeDefinition().BuildLootBagTypeDefinitions(numberOfAwards);
            var bonusList = new List<RVRPlayerBagBonus>();
            var randomRolls = new Dictionary<uint, int>();
            var pairingContributions = new Dictionary<uint, int>();
            var assignedLoot = rewardAssigner.AssignLootToPlayers(numberOfAwards, x, sortedPairs, bonusList, randomRolls, pairingContributions);

            Assert.IsTrue(numberOfAwards == assignedLoot.Count);
            Assert.IsTrue(assignedLoot[0].Assignee == 33222);
            Assert.IsTrue(assignedLoot[1].Assignee == 50000);
            Assert.IsTrue(assignedLoot[2].Assignee == 50020);
            Assert.IsTrue(assignedLoot[3].Assignee == 12332);
        }

        
        [TestMethod]
        public void AssignLootWithBonusesGoldBagBonus()
        {

            var sortedPairs = new List<KeyValuePair<uint, int>>
            {
                new KeyValuePair<uint, int>(50000, 124),
                new KeyValuePair<uint, int>(50020, 100),
                new KeyValuePair<uint, int>(12332, 78),
                new KeyValuePair<uint, int>(42223, 77),
                new KeyValuePair<uint, int>(73738, 50),
                new KeyValuePair<uint, int>(12202, 12),
                new KeyValuePair<uint, int>(12902, 4),
                new KeyValuePair<uint, int>(12123, 0),
                new KeyValuePair<uint, int>(33222, 220)

            };

            // +100 gold bag bonus should mean 50020 wins gold. And nothing else!
            var bonusList = new List<RVRPlayerBagBonus>
            {
                new RVRPlayerBagBonus
                {
                    CharacterId = 50020,
                    GoldBag = 200,
                    BlueBag = 1,
                    PurpleBag = 0,
                    GreenBag = 0
                }
            };

            var rewardAssigner = new  RewardAssigner(StaticRandom.Instance, _logger);
            var numberOfAwards = 4;

            var x = new LootBagTypeDefinition().BuildLootBagTypeDefinitions(numberOfAwards);
            var randomRolls = new Dictionary<uint, int>();
            
            var pairingContributions = new Dictionary<uint, int>();
            var assignedLoot = rewardAssigner.AssignLootToPlayers(numberOfAwards, x, sortedPairs, bonusList, randomRolls,pairingContributions);

            Assert.IsTrue(numberOfAwards == assignedLoot.Count);
            Assert.IsTrue(assignedLoot[0].Assignee == 50020);
            Assert.IsTrue(assignedLoot[1].Assignee == 33222);
            Assert.IsTrue(assignedLoot[2].Assignee == 50000);
            Assert.IsTrue(assignedLoot[3].Assignee == 12332);
            

        }

        [TestMethod]
        public void AssignLootWithBonusesBlueBagBonus()
        {

            var sortedPairs = new List<KeyValuePair<uint, int>>
            {
                new KeyValuePair<uint, int>(50000, 124),
                new KeyValuePair<uint, int>(50020, 100),
                new KeyValuePair<uint, int>(12332, 78),
                new KeyValuePair<uint, int>(42223, 77),
                new KeyValuePair<uint, int>(73738, 50),
                new KeyValuePair<uint, int>(12202, 12),
                new KeyValuePair<uint, int>(12902, 4),
                new KeyValuePair<uint, int>(12123, 0),
                new KeyValuePair<uint, int>(33222, 220)

            };

            
            var bonusList = new List<RVRPlayerBagBonus>
            {
                new RVRPlayerBagBonus
                {
                    CharacterId = 50020,
                    GoldBag = 0,
                    BlueBag = 30,
                    PurpleBag = 0,
                    GreenBag = 0
                }
            };

            var rewardAssigner = new  RewardAssigner(StaticRandom.Instance, _logger);
            var numberOfAwards = 4;
            // Gold, Blue, Green, Green
            var x = new LootBagTypeDefinition().BuildLootBagTypeDefinitions(numberOfAwards);
            var randomRolls = new Dictionary<uint, int>();
            var pairingContributions = new Dictionary<uint, int>();
            var assignedLoot = rewardAssigner.AssignLootToPlayers(numberOfAwards, x, sortedPairs, bonusList, randomRolls,pairingContributions);

            
            Assert.IsTrue(numberOfAwards == assignedLoot.Count);
            Assert.IsTrue(assignedLoot[0].Assignee == 33222);
            Assert.IsTrue(assignedLoot[1].Assignee == 50020);
            Assert.IsTrue(assignedLoot[2].Assignee == 50000);
            Assert.IsTrue(assignedLoot[3].Assignee == 12332);
            

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
                (IImpactMatrixManager) fakeImpactMatrixManager);

            var insigniaRewards = rm.GetInsigniaRewards(100);
            Assert.IsTrue(insigniaRewards);

          
        }


    
    }


}
