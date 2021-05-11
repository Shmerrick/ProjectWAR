using FrameWork;
using System;

namespace Common//new
{
    [DataTable(PreCache = false, TableName = "ability_expression", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    class AbilityExpression : DataObject
    {
        [PrimaryKey]
        public long ID { get; set; }

        [DataElement]
        public long AbilityID { get; set; }

        [DataElement]
        public long ComponentID { get; set; }

        [DataElement]
        public int Index { get; set; }

        [DataElement]
        public long Type { get; set; }

        [DataElement]
        public long Operation { get; set; }

        [DataElement]
        public long Condition { get; set; }

        [DataElement]
        public long LogicOperator { get; set; }

        [DataElement]
        public int Val5 { get; set; }

        [DataElement]
        public int Val6 { get; set; }

        [DataElement]
        public int Val7 { get; set; }

        [DataElement]
        public int Val8 { get; set; }

        [DataElement]
        public int Val9 { get; set; }

        [DataElement]
        public long RequirmentID { get; set; }

        [DataElement]
        public byte Disabled { get; set; }
    }
}
