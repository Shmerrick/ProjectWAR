using FrameWork;
using System;

namespace Common //new
{
    [DataTable(PreCache = false, TableName = "ability_line", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    class AbilityLine : DataObject
    {
        [PrimaryKey]
        public long ID { get; set; }

        [DataElement]
        public string  Name { get; set; }

        [DataElement]
        public byte Disabled { get; set; }
    }
}
