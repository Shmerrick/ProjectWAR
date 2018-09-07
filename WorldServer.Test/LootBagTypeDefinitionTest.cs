using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldServer.World.Battlefronts.Apocalypse.Loot;

namespace WorldServer.Test
{
    [TestClass]
    public class LootBagTypeDefinitionTest
    {
        [TestMethod]
        public void BuildLootBagTypeDefinitionsCountsCorrect()
        {
            var def = new LootBagTypeDefinition();
            Assert.IsTrue(def.BuildLootBagTypeDefinitions(0).Count == 0);
            Assert.IsTrue(def.BuildLootBagTypeDefinitions(1).Count == 1);
            Assert.IsTrue(def.BuildLootBagTypeDefinitions(2).Count == 2);
            Assert.IsTrue(def.BuildLootBagTypeDefinitions(3).Count == 2);
            Assert.IsTrue(def.BuildLootBagTypeDefinitions(4).Count == 3);
            Assert.IsTrue(def.BuildLootBagTypeDefinitions(6).Count == 5);

            Assert.IsTrue(def.BuildLootBagTypeDefinitions(12).Count == 12);
            Assert.IsTrue(def.BuildLootBagTypeDefinitions(10).Count == 10);
            Assert.IsTrue(def.BuildLootBagTypeDefinitions(20).Count == 20);
            Assert.IsTrue(def.BuildLootBagTypeDefinitions(30).Count == 31);
            Assert.IsTrue(def.BuildLootBagTypeDefinitions(40).Count == 41);
            Assert.IsTrue(def.BuildLootBagTypeDefinitions(50).Count == 51);
        }

        [TestMethod]
        public void BuildLootBagTypeDefinitionsRarityCorrect()
        {
            var def = new LootBagTypeDefinition();

            var bagList2 = def.BuildLootBagTypeDefinitions(1);
            Assert.IsTrue(bagList2[0].Assignee == 0);
            Assert.IsTrue(bagList2[0].LootBagNumber == 0);
            Assert.IsTrue(bagList2[0].BagRarity == LootBagRarity.Green);

            var bagList3 = def.BuildLootBagTypeDefinitions(2);
            Assert.IsTrue(bagList3[0].Assignee == 0);
            Assert.IsTrue(bagList3[0].LootBagNumber == 0);
            Assert.IsTrue(bagList3[0].BagRarity == LootBagRarity.Green);

            Assert.IsTrue(bagList3[1].Assignee == 0);
            Assert.IsTrue(bagList3[1].LootBagNumber == 1);
            Assert.IsTrue(bagList3[1].BagRarity == LootBagRarity.Green);

            var bagList4 = def.BuildLootBagTypeDefinitions(3);

            Assert.IsTrue(bagList4[0].Assignee == 0);
            Assert.IsTrue(bagList4[0].LootBagNumber == 0);
            Assert.IsTrue(bagList4[0].BagRarity == LootBagRarity.Blue);

            Assert.IsTrue(bagList4[1].Assignee == 0);
            Assert.IsTrue(bagList4[1].LootBagNumber == 1);
            Assert.IsTrue(bagList4[1].BagRarity == LootBagRarity.Green);

            var bagList5 = def.BuildLootBagTypeDefinitions(4);

            Assert.IsTrue(bagList5[0].Assignee == 0);
            Assert.IsTrue(bagList5[0].LootBagNumber == 0);
            Assert.IsTrue(bagList5[0].BagRarity == LootBagRarity.Blue);

            Assert.IsTrue(bagList5[1].Assignee == 0);
            Assert.IsTrue(bagList5[1].LootBagNumber == 1);
            Assert.IsTrue(bagList5[1].BagRarity == LootBagRarity.Green);

            Assert.IsTrue(bagList5[2].Assignee == 0);
            Assert.IsTrue(bagList5[2].LootBagNumber == 2);
            Assert.IsTrue(bagList5[2].BagRarity == LootBagRarity.Green);

            
            var bagList7 = def.BuildLootBagTypeDefinitions(12);
            Assert.IsTrue(bagList7[0].Assignee == 0);
            Assert.IsTrue(bagList7[0].LootBagNumber == 0);
            Assert.IsTrue(bagList7[0].BagRarity == LootBagRarity.Gold);

            Assert.IsTrue(bagList7[1].Assignee == 0);
            Assert.IsTrue(bagList7[1].LootBagNumber == 1);
            Assert.IsTrue(bagList7[1].BagRarity == LootBagRarity.Purple);

            Assert.IsTrue(bagList7[2].Assignee == 0);
            Assert.IsTrue(bagList7[2].LootBagNumber == 2);
            Assert.IsTrue(bagList7[2].BagRarity == LootBagRarity.Blue);

            Assert.IsTrue(bagList7[3].Assignee == 0);
            Assert.IsTrue(bagList7[3].LootBagNumber == 3);
            Assert.IsTrue(bagList7[3].BagRarity == LootBagRarity.Blue);

            Assert.IsTrue(bagList7[4].Assignee == 0);
            Assert.IsTrue(bagList7[4].LootBagNumber == 4);
            Assert.IsTrue(bagList7[4].BagRarity == LootBagRarity.Blue);

        }
    }
}
