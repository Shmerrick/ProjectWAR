using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameWork;

namespace Common.Database.World.Battlefront
{

    [DataTable(PreCache = false, TableName = "bounty_contribution_analytics", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class ContributionAnalytics : DataObject
    {

        [PrimaryKey(AutoIncrement = true)]
        public int AnalyticsId { get; set; }
        [DataElement(AllowDbNull = false)]
        public DateTime Timestamp { get; set; }
        [DataElement(AllowDbNull = false)]
        public uint CharacterId { get; set; }
        [DataElement(AllowDbNull = false)]
        public byte RenownRank { get; set; }
        [DataElement(AllowDbNull = false)]
        public string Name { get; set; }
        [DataElement(AllowDbNull = false)]
        public uint Class { get; set; }
        [DataElement(AllowDbNull = false)]
        public int AccountId { get; set; }
        [DataElement(AllowDbNull = false)]
        public int ZoneId { get; set; }
        [DataElement(AllowDbNull = false)]
        public int ContributionValue { get; set; }


    }

    [DataTable(PreCache = false, TableName = "bounty_contribution_analytics_details", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class ContributionAnalyticsDetails : DataObject
    {

        [PrimaryKey(AutoIncrement = true)]
        public int AnalyticsDetailId { get; set; }
        [DataElement(AllowDbNull = false)]
        public DateTime Timestamp { get; set; }
        [DataElement(AllowDbNull = false)]
        public uint CharacterId { get; set; }
        [DataElement(AllowDbNull = false)]
        public short ContributionId { get; set; }
        [DataElement(AllowDbNull = false)]
        public int ContributionSum { get; set; }

    }
}
