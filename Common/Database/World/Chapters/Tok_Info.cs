using FrameWork;
using System;

namespace Common
{
    [DataTable(PreCache = false, TableName = "tok_infos", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Tok_Info : DataObject
    {
        [PrimaryKey]
        public ushort Entry { get; set; }

        [DataElement(Varchar = 255)]
        public string Name { get; set; }

        [DataElement()]
        public uint Xp { get; set; }

        [DataElement()]
        public uint Section { get; set; }

        [DataElement()]
        public uint Index { get; set; }

        [DataElement()]
        public uint Flag { get; set; }

        [DataElement(Varchar = 255)]
        public string EventName { get; set; }

        [DataElement()]
        public uint Rewards { get; set; }

        [DataElement()]
        public byte Realm { get; set; }
    }
}