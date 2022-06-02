using FrameWork;
using System;

namespace Common
{
    [DataTable(PreCache = false, TableName = "tok_bestary", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Tok_Bestary : DataObject
    {
        [PrimaryKey]
        public ushort Creature_Sub_Type { get; set; }

        [DataElement]
        public ushort Bestary_ID { get; set; }

        [DataElement(Varchar = 50)]
        public string Kill1 { get; set; }

        [DataElement(Varchar = 50)]
        public string Kill25 { get; set; }

        [DataElement(Varchar = 50)]
        public string Kill100 { get; set; }

        [DataElement(Varchar = 50)]
        public string Kill1000 { get; set; }

        [DataElement(Varchar = 50)]
        public string Kill10000 { get; set; }

        [DataElement(Varchar = 50)]
        public string Kill100000 { get; set; }
    }
}