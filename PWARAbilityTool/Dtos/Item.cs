using System.Collections.Generic;

namespace PWARAbilityTool
{
    public partial class Item
    {
        public int Entry { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public int Race { get; set; }
        public int ModelId { get; set; }
        public int SlotId { get; set; }
        public int Rarity { get; set; }
        public int Career { get; set; }
        public int Armor { get; set; }
        public int Dps { get; set; }
        public int MinRank { get; set; }
        public int MinRenown { get; set; }
        public int TwoHanded { get; set; }
        public int Salvageable { get; set; }
        public string Statistics { get; set; }
        public string RaceName { get; set; }
        public string ClassName { get; set; }
        public int ObjectLevel { get; set; }

        public List<ItemInfoStats> StatsList { get; set; }
        public Icon Icon { get; set; }

        public Item()
        {
            StatsList = new List<ItemInfoStats>();
            Icon = new Icon();
        }

    }
}
