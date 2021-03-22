using FrameWork;
using System;

namespace Common//new
{
    [DataTable(PreCache = false, TableName = "ability_line", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    class AbilityLine : DataObject
    {
        [PrimaryKey]
        public int ID { get; set; }

        [DataElement]
        public string  Name { get; set; }

        [DataElement]
        public ushort Disabled { get; set; }
    }
}
