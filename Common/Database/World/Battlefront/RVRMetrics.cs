using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameWork;

namespace Common.Database.World.Battlefront
{

    [DataTable(PreCache = false, TableName = "rvr_metrics", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class RVRMetrics : DataObject
    {
        [PrimaryKey(AutoIncrement = true)]
        public int MetricId { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Tier { get; set; }

        [DataElement(AllowDbNull = false)]
        public int BattlefrontId { get; set; }

        [DataElement(AllowDbNull = false)]
        public int OrderVictoryPoints { get; set; }

        [DataElement(AllowDbNull = false)]
        public int DestructionVictoryPoints { get; set; }

        [DataElement(AllowDbNull = false)]
        public string BattlefrontName { get; set; }

        [DataElement(AllowDbNull = false)]
        public int OrderPlayersInLake { get; set; }

        [DataElement(AllowDbNull = false)]
        public int DestructionPlayersInLake { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Locked { get; set; }

        [DataElement(AllowDbNull = false)]
        public DateTime Timestamp { get; set; }

        [DataElement(AllowDbNull = false)]
        public string GroupId { get; set; }

        [DataElement(AllowDbNull = false)]
        public int TotalPlayerCountInRegion { get; set; }

        [DataElement(AllowDbNull = false)]
        public int TotalOrderPlayerCountInRegion { get; set; }

        [DataElement(AllowDbNull = false)]
        public int TotalDestPlayerCountInRegion { get; set; }

        [DataElement(AllowDbNull = false)]
        public int TotalPlayerCount { get; set; }

        [DataElement(AllowDbNull = false)]
        public int TotalFlaggedPlayerCount { get; set; }

        
    }
}
