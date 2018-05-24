using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharacterUtility
{
   public  class ItemInfo
    {
        public int Entry { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Type { get; set; }
        public int Career { get; set; }
        public string Stats { get; set; }

        public List<ItemInfoStats> StatsList { get; set; }

        public ItemInfo()
        {
            StatsList = new List<ItemInfoStats>();
        }
    }

    public class ItemInfoStats
    {
        public int Type { get; set; }
        public int Value { get; set; }
        public string Description { get; set; }

        public ItemInfoStats(string stat, IEnumerable<ItemBonus> itemBonusList)
        {
            var temp = stat.Split(':');
            Type = Convert.ToInt32(temp[0]);
            Value = Convert.ToInt32(temp[1]);
            Description = itemBonusList.Single(x => x.Entry == Convert.ToInt32(Type)).BonusName;
        }
    }
}
