using System;
using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "battlefront_resource_spawns", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class BattlefrontResourceSpawn : DataObject
    {
        [PrimaryKey(AutoIncrement = true)]
        public int Guid { get; set; }

        [DataElement]
        public int Entry { get; set; }

        [DataElement(AllowDbNull = false)]
        public int X { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Y { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Z { get; set; }

        [DataElement(AllowDbNull = false)]
        public int O { get; set; }
    }
}
