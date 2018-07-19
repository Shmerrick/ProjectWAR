using System;
using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "BattleFront_status", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class BattleFrontStatus : DataObject
    {
        private int _openZoneIndex;
        private int _activeRegionOrZone;
        private int _controlingRealm;

        public BattleFrontStatus()
        {
            
        }

        public BattleFrontStatus(int regionId)
        {
            RegionId = regionId;
            OpenZoneIndex = 1;
        }

        [PrimaryKey(AutoIncrement = true)]
        public int RegionId { get; set; }

        [DataElement]
        public int OpenZoneIndex
        {
            get { return _openZoneIndex; }
            set { _openZoneIndex = value; Dirty = true; }
        }

        [DataElement]
        public int ActiveRegionOrZone
        {
            get { return _activeRegionOrZone; }
            set { _activeRegionOrZone = value; Dirty = true; }
        }

        [DataElement]
        public int ControlingRealm
        {
            get { return _controlingRealm; }
            set { _controlingRealm = value; Dirty = true; }
        }
    }
}
