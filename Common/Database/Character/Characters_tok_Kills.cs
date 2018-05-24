using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    // Fixed value of a character
    [DataTable(PreCache = false, TableName = "characters_toks_kills", DatabaseName = "Characters", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Character_tok_kills : DataObject
    {
        [PrimaryKey]
        public uint CharacterId { get; set; }

        [PrimaryKey]
        public ushort NPCEntry { get; set; }

        [DataElement()]
        public uint Count { get; set; }
    }
}
