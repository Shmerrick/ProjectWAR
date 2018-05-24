using System;
using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "pquest_loot", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class PQuest_Loot : DataObject
    {
        [PrimaryKey]
        public uint ItemID { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint Career { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte Chapter { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte Bag { get; set; }

        [PrimaryKey]
        public byte PQTier { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte PQType { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint PQEntry { get; set; }
    }
}
