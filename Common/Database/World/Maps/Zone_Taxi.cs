using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "zone_taxis", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Zone_Taxi : DataObject
    {
        [PrimaryKey]
        public ushort ZoneID { get; set; }

        [PrimaryKey]
        public byte RealmID { get; set; }

        [DataElement()]
        public uint WorldX { get; set; }

        [DataElement()]
        public uint WorldY { get; set; }

        [DataElement()]
        public ushort WorldZ { get; set; }

        [DataElement()]
        public ushort WorldO { get; set; }

        [DataElement()]
        public byte Tier { get; set; }

        [DataElement()]
        public bool Enable { get; set; }



        public Zone_Info Info;
    }
}
