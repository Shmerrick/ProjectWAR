using System;
using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "keep_doors", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Keep_Door : DataObject
    {
        [DataElement(AllowDbNull = false)]
        public int KeepId { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte Number { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort ZoneId { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort GameObjectId { get; set; }

        [PrimaryKey]
        public uint DoorId { get; set; }

        [DataElement(AllowDbNull = false)]
        public int X { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Y { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Z { get; set; }

        [DataElement(AllowDbNull = false)]
        public int O { get; set; }

        [DataElement(AllowDbNull = false)]
        public int TeleportX1 { get; set; }

        [DataElement(AllowDbNull = false)]
        public int TeleportY1 { get; set; }

        [DataElement(AllowDbNull = false)]
        public int TeleportZ1 { get; set; }

        [DataElement(AllowDbNull = false)]
        public int TeleportO1 { get; set; }

        [DataElement(AllowDbNull = false)]
        public int TeleportX2 { get; set; }

        [DataElement(AllowDbNull = false)]
        public int TeleportY2 { get; set; }

        [DataElement(AllowDbNull = false)]
        public int TeleportZ2 { get; set; }

        [DataElement(AllowDbNull = false)]
        public int TeleportO2 { get; set; }
    }
}
