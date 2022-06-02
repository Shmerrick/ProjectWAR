using FrameWork;
using System;

namespace Common
{
    [DataTable(PreCache = false, TableName = "quests_creature_finisher", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Quest_Creature_Finisher : DataObject
    {
        [DataElement()]
        public uint CreatureID { get; set; }

        [PrimaryKey]
        public ushort Entry { get; set; }
    }
}