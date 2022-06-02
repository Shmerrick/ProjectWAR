using FrameWork;
using System;

namespace Common
{
    // Fixed value of a character
    [DataTable(PreCache = false, TableName = "characters_toks", DatabaseName = "Characters", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Character_tok : DataObject
    {
        [PrimaryKey]
        public uint CharacterId { get; set; }

        [DataElement()]
        public uint Count { get; set; }

        [PrimaryKey]
        public ushort TokEntry { get; set; }
    }
}