using System;
using System.Collections.Generic;
using System.Linq;
using apoc_api.common.Reference;

namespace PWARAbilityTool
{

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
            var bonusType = itemBonusList.SingleOrDefault(x => x.Entry == Convert.ToInt32(Type));
            if (bonusType != null)
                Description = bonusType.BonusName;
            else
                Description = "Unknown Bonus";

        }
    }
}
