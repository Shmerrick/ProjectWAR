using System.Collections.Generic;
using apoc_api.common.Reference;

namespace PWARAbilityTool
{
    public interface IEnrichService
    {
        bool EnrichItemWithStatistics(Item item, List<ItemBonus> itemBonusList);
        void EnrichItemWithClass(Item item, List<Career> classList);
        void EnrichItemWithRace(Item item, List<Race> raceList);
        void EnrichItemWithIcon(Item item);

    }
}
