using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameWork;

namespace Common.Database.World.Battlefront
{

    [DataTable(PreCache = false, TableName = "Rvr_Area_Polygon", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Rvr_Area_Polygon : DataObject
    {
        private ushort _LakeID;
        private ushort _ZoneInfoID;
        private string _ZoneNameID;
        private string _PolygonPlanarCoordinates;

        [PrimaryKey]
        public ushort LakeID
        {
            get { return _LakeID; }
            set { _LakeID = value; Dirty = true; }
        }

        [PrimaryKey]
        public ushort ZoneInfoID
        {
            get { return _ZoneInfoID; }
            set { _LakeID = value; Dirty = true; }
        }

        [DataElement(Varchar = 255)]
        public string ZoneNameID
        {
            get { return ZoneNameID; }
            set { ZoneNameID = value; Dirty = true; }
        }

        [DataElement()]
        public string PolygonPlanarCoordinates { get; set; }

        }
    }