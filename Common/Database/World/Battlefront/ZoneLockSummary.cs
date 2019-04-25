using System;
using FrameWork;

namespace Common.Database.World.Battlefront
{
    // Fixed value of a character 
    [DataTable(PreCache = false, TableName = "rvr_zone_lock_summary", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class ZoneLockSummary : DataObject
    {
        [PrimaryKey]
        public long LockId { get; set; }

        [DataElement(AllowDbNull = false), PrimaryKey]
        public int RegionId { get; set; }
        
        [DataElement(AllowDbNull = false)]
        public DateTime Timestamp { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Tier { get; set; }

        [DataElement(AllowDbNull = false)]
        public int LockingRealm { get; set; }
        
        [DataElement(AllowDbNull = false)]
        public string Description { get; set; }


		[DataElement(AllowDbNull = false)]
		public int OrderVP { get; set; }

		[DataElement(AllowDbNull = false)]
		public int DestroVP { get; set; }

        [DataElement(AllowDbNull = false)]
        public int EligiblePlayersAtLock { get; set; }
        
        [DataElement(AllowDbNull = false)]
        public int TotalPlayersAtLock { get; set; }

    }
}
