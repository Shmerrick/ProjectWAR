using System;
using System.Collections.Generic;
using System.Linq;
using apoc_api.common.Reference;

namespace PWARAbilityTool
{
    public class EnrichService : IEnrichService
    {
        public bool EnrichItemWithStatistics(Item item, List<ItemBonus> itemBonusList)
        {
            if (item.Statistics == null)
                return false;

            var individualStatItems = item.Statistics.Split(';');
            foreach (var stat in individualStatItems)
            {
                if ((stat == "0:0") || (stat == "") || (stat.StartsWith("0:0")))
                    continue;
                try
                {
                    item.StatsList.Add(new ItemInfoStats(stat, itemBonusList));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
            }
            return true;
        }

        public void EnrichItemWithClass(Item item, List<Career> classList)
        {

            if (item.Career == 0)
            {
                item.ClassName = "Unknown/Any";
                return;
            }

            var a = classList.SingleOrDefault(x => x.ClassId == item.Career);
            if (a == null)
            {
                item.ClassName = "Unknown/Any";
                return;
            }

            item.ClassName = a.ClassName;

           
        }

        public void EnrichItemWithRace(Item item, List<Race> raceList)
        {

            if (item.Race == 0)
            {
                item.RaceName = "Unknown/Any";
                return;
            }
            var a = raceList.SingleOrDefault(x => x.RaceId == item.Race);
            if (a == null)
            {
                item.RaceName = "Unknown/Any";
                return;
            }

            item.RaceName = a.RaceName;

        }

        public void EnrichItemWithIcon(Item item)
        {
            var icon = Startup.iconService.GetIcon(item.ModelId);
            {
                if (icon != null)
                    item.Icon = icon;
            }

        }
    }

    public class EnrichServiceImpl : EnrichService
    {

    }
}
