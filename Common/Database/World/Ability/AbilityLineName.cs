using FrameWork;
using System;

namespace Common//new
{
    [DataTable(PreCache = false, TableName = "ability_line_name", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    class AbilityLineName : DataObject
    {
        [PrimaryKey]
        public long ID { get; set; }

        [DataElement]
        public string Name { get; set; }
    }
}
