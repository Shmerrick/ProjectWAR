using FrameWork;
using System;

namespace Common
{
    // Fixed value of a character
    [DataTable(PreCache = false, TableName = "quests_creature_starter", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Quest_Creature_Starter : DataObject
    {
        [DataElement()]
        public uint CreatureID { get; set; }

        [PrimaryKey]
        public ushort Entry { get; set; }
    }
}