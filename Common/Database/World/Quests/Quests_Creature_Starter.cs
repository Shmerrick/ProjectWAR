using FrameWork;
using System;

namespace Common
{
    // Fixed value of a character
    [DataTable(PreCache = false, TableName = "quests", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Quest_Creature_Starter : DataObject
    {
        [PrimaryKey]
        public ushort Entry { get; set; }

        [DataElement()]
        public uint StartCreatureId { get; set; }
    }
}