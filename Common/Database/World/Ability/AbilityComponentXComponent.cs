using FrameWork;
using System;

namespace Common//new
{
    [DataTable(PreCache = false, TableName = "ability_component_x_component", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    class AbilityComponentXComponent : DataObject
    {
        [PrimaryKey]
        public int ID { get; set; }

        [DataElement]
        public int AbilityID { get; set; }

        [DataElement]
        public int ComponentID { get; set; }

        [DataElement]
        public int Trigger { get; set; }

        [DataElement]
        public ushort VfxID { get; set; }

        [DataElement]
        public ushort Index { get; set; }

        [DataElement]
        public ushort Disabled { get; set; }
    }
}
