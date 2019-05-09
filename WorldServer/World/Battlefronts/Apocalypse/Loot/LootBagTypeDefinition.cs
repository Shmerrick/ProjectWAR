using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WorldServer.Managers;

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

        public override string ToString()
        {
            return $"Bag#:{LootBagNumber} Assignee:{Assignee} Rarity:{BagRarity} ItemId:{ItemId} ItemCount:{ItemCount} RenownBand:{RenownBand}";
        }

        public bool IsValid()
        {
            return Assignee != 0 && ItemId != 0 && ItemCount != 0;
        }

        public static string GetDescription(Enum value)
        {
            return
                value
                    .GetType()
                    .GetMember(value.ToString())
                    .FirstOrDefault()
                    ?.GetCustomAttribute<DescriptionAttribute>()
                    ?.Description;
        }

        public List<LootBagTypeDefinition> BuildLootBagTypeDefinitions(int numberLootBags)
        {
            var result = new List<LootBagTypeDefinition>();

            var numberGoldBags = ((uint)Math.Floor((numberLootBags / 10.0)));
            var numberPurpleBags = ((uint)Math.Floor((numberLootBags / 10.0)));
            var numberBlueBags = ((uint)Math.Floor((numberLootBags / 3.0)));
            var numberGreenBags = ((uint)Math.Floor((numberLootBags / 2.0)));

            byte lootBagNumber = 0;

            if (numberLootBags > 2)
            {
                numberGoldBags++;
            }

            for (byte i = 0; i < numberGoldBags; i++)
            {
                result.Add(new LootBagTypeDefinition { LootBagNumber= lootBagNumber++, Assignee = 0, BagRarity = LootBagRarity.Gold });
            }
            for (byte i = 0; i < numberPurpleBags; i++)
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

        public string FormattedString()
        {
            var bagTypeName = Enum.GetName(typeof(LootBagRarity), BagRarity);
            return $"a {bagTypeName} bag";
        }
    }
}
