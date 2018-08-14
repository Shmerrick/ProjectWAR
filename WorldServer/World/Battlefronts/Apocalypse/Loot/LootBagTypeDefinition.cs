using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public class LootBagTypeDefinition
    {
        public byte LootBagNumber { get; set; }
        public uint Assignee { get; set; }
        public LootBagRarity BagRarity { get; set; }
        public uint ItemId { get; set; }
        public uint ItemCount { get; set; }
        public byte RenownBand { get; set; }

        public LootBagTypeDefinition()
        {
            ItemId = 0;
            ItemCount = 0;
        }

        public List<LootBagTypeDefinition> BuildLootBagTypeDefinitions(byte numberLootBags)
        {
            var result = new List<LootBagTypeDefinition>();

            var numberGoldBags = ((uint)Math.Floor((numberLootBags / 10.0)));
            var numberPurpledBags = ((uint)Math.Floor((numberLootBags / 10.0)));
            var numberBlueBags = ((uint)Math.Floor((numberLootBags / 3.0)));
            var numberGreenBags = ((uint)Math.Floor((numberLootBags / 2.0)));

            byte lootBagNumber = 0;

            for (byte i = 0; i < numberGoldBags; i++)
            {
                result.Add(new LootBagTypeDefinition { LootBagNumber= lootBagNumber++, Assignee = 0, BagRarity = LootBagRarity.Gold });
            }
            for (byte i = 0; i < numberPurpledBags; i++)
            {
                result.Add(new LootBagTypeDefinition { LootBagNumber = lootBagNumber++, Assignee = 0, BagRarity = LootBagRarity.Purple });
            }
            for (byte i = 0; i < numberBlueBags; i++)
            {
                result.Add(new LootBagTypeDefinition { LootBagNumber = lootBagNumber++, Assignee = 0, BagRarity = LootBagRarity.Blue });
            }
            for (byte i = 0; i < numberGreenBags; i++)
            {
                result.Add(new LootBagTypeDefinition { LootBagNumber = lootBagNumber++, Assignee = 0, BagRarity = LootBagRarity.Green });
            }

            // No bags or low number (< 2) requested
            if ((numberLootBags == 1) || (numberLootBags == 2))
            {
                result.Add(new LootBagTypeDefinition { LootBagNumber = lootBagNumber++, Assignee = 0, BagRarity = LootBagRarity.Green });
            }
            return result;
        }
    }
}
