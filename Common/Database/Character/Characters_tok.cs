using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    // Valeur Fixe d'un character
    [DataTable(PreCache = false, TableName = "characters_toks", DatabaseName = "Characters", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Character_tok : DataObject
    {
        [PrimaryKey]
        public uint CharacterId { get; set; }

        [PrimaryKey]
        public ushort TokEntry { get; set; }

        [DataElement()]
        public uint Count { get; set; }
    }
}
