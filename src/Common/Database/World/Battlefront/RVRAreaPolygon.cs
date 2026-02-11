using FrameWork;
using System;

namespace Common.Database.World.Battlefront
{
    [DataTable(PreCache = false, TableName = "Rvr_Area_Polygon", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class RVRAreaPolygon : DataObject
    {
        private ushort _BattleFrontId;
        private ushort _ZoneId;
        private string _ZoneName;
        
        [PrimaryKey]
        public ushort BattleFrontId
        {
            get { return _BattleFrontId; }
            set { _BattleFrontId = value; Dirty = true; }
        }

        [PrimaryKey]
        public ushort ZoneId
        {
            get { return _ZoneId; }
            set { _ZoneId = value; Dirty = true; }
        }

        [DataElement(Varchar = 255)]
        public string ZoneName
        {
            get { return _ZoneName; }
            set { _ZoneName = value; Dirty = true; }
        }

        [DataElement()]
        public string PolygonPlanarCoordinates { get; set; }
    }
}