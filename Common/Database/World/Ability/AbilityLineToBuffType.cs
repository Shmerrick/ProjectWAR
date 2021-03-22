using FrameWork;
using System;

namespace Common //new
{
    [DataTable(PreCache = false, TableName = "ability_line_to_buff_type", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    class AbilityLineToBuffType : DataObject
    {
        [DataElement]
        public int ID { get; set; }

        [DataElement]
        public string TypeName { get; set; }

        [DataElement] //fix
        public int ClientSideEnumerationValue { get; set; }

        [DataElement] //fix
        public int BuffFrameRed { get; set; }

        [DataElement] //fix
        public int BuffFrameGreen { get; set; }

        [DataElement] //fix
        public int BuffFrameBlue { get; set; }
    }
}
