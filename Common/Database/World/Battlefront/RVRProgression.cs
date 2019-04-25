using System;
using FrameWork;

namespace Common.Database.World.Battlefront
{
    // Fixed value of a character 
    [DataTable(PreCache = false, TableName = "rvr_progression", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class RVRProgression : DataObject
    {
        [PrimaryKey]
        public int BattleFrontId { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Tier { get; set; }

        [DataElement(AllowDbNull = false)]
        public int PairingId { get; set; }

        [DataElement(AllowDbNull = false)]
        public int OrderWinProgression { get; set; }

        [DataElement(AllowDbNull = false)]
        public int DestWinProgression { get; set; }

        [DataElement(AllowDbNull = false)]
        public string Description { get; set; }

        [DataElement(AllowDbNull = false)]
        public int RegionId { get; set; }

        [DataElement(AllowDbNull = false)]
        public int ZoneId { get; set; }

        [DataElement(AllowDbNull = false)]
        public int DefaultRealmLock { get; set; }

        [DataElement(AllowDbNull = false)]
        public int ResetProgressionOnEntry { get; set; }

		[DataElement(AllowDbNull = false)]
		public int LastOwningRealm { get; set; }

		[DataElement(AllowDbNull = false)]
		public int LastOpenedZone { get; set; }

		[DataElement(AllowDbNull = false)]
		public int OrderVP { get; set; }

		[DataElement(AllowDbNull = false)]
		public int DestroVP { get; set; }

        [DataElement(AllowDbNull = false)]
        public int DestroKeepId { get; set; }

        [DataElement(AllowDbNull = false)]
        public int OrderKeepId { get; set; }
    }
}
