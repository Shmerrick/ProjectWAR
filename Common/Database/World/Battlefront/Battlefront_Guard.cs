using System;
using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "BattleFront_guards", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class BattleFront_Guard : DataObject
    {
        [DataElement(AllowDbNull = false)]
        public int ObjectiveId { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort ZoneId { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint OrderId { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint DestroId { get; set; }

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
