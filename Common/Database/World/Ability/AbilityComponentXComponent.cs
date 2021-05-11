using FrameWork;
using System;

namespace Common//new
{
    [DataTable(PreCache = false, TableName = "ability_component_x_component", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    class AbilityComponentXComponent : DataObject
    {
        [PrimaryKey]
        public long ID { get; set; }

        [DataElement]
        public long AbilityID { get; set; }

        [DataElement]
        public long ComponentID { get; set; }

        [DataElement]
        public long Trigger { get; set; }

        [DataElement]
        public byte VfxID { get; set; }

        [DataElement]
        public byte Index { get; set; }

        [DataElement]
        public byte Disabled { get; set; }
    }
}
