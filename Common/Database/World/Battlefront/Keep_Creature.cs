using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "keep_creatures", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Keep_Creature : DataObject
    {
        [DataElement(AllowDbNull = false)]
        public int KeepId { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort ZoneId { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint OrderId { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint DestroId { get; set; }

        [PrimaryKey]
        public int X { get; set; }

        [PrimaryKey]
        public int Y { get; set; }

        [PrimaryKey]
        public int Z { get; set; }

        [DataElement(AllowDbNull = false)]
        public int O { get; set; }

        [DataElement(AllowDbNull = false)]
        public bool KeepLord { get; set; }
    }
}
