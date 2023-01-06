using FrameWork;
using System;

namespace Common.Database.World.Battlefront
{
    [DataTable(PreCache = false, TableName = "bounty_contribution_definition", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class ContributionDefinition : DataObject
    {
        [PrimaryKey]
        public int ContributionId { get; set; }

        [DataElement(AllowDbNull = false)]
        public string ContributionDescription { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte ContributionValue { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte MaxContributionCount { get; set; }
    }
}