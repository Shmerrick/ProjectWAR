using FrameWork;
using System;

namespace Common//new
{
    [DataTable(PreCache = false, TableName = "ability_expression", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    class AbilityExpression : DataObject
    {
        [PrimaryKey]
        public int ID { get; set; }

        [DataElement]
        public int AbilityID { get; set; }

        [DataElement]
        public int ComponentID { get; set; }

        [DataElement]
        public int Index { get; set; }

        [DataElement]
        public int Type { get; set; }

        [DataElement]
        public int Operation { get; set; }

        [DataElement]
        public int Condition { get; set; }

        [DataElement]
        public int LogicOperator { get; set; }

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
        public int RequirmentID { get; set; }

        [DataElement]
        public ushort Disabled { get; set; }
    }
}
