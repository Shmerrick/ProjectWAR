using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "zone_jumps", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Zone_jump : DataObject
    {
        [PrimaryKey]
        public uint Entry { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort ZoneID { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint WorldX { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint WorldY { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort WorldZ { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort WorldO { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte Type { get; set; }

        [DataElement(AllowDbNull = false)]
        public bool Enabled { get; set; }

        [DataElement(AllowDbNull = true)]
        public ushort InstanceID { get; set; }
    }
}
