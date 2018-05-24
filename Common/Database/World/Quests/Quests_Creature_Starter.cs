using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    // Fixed value of a character
    [DataTable(PreCache = false, TableName = "quests_creature_starter", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Quest_Creature_Starter : DataObject
    {
        [PrimaryKey]
        public ushort Entry { get; set; }

        [DataElement()]
        public uint CreatureID { get; set; }
    }
}
