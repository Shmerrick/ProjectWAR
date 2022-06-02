using FrameWork;
using System;

namespace Common
{
    [DataTable(PreCache = false, TableName = "loot_group_butchering", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Loot_Group_Butcher : DataObject
    {
        [PrimaryKey]
        public byte CreatureSubType { get; set; }

        [PrimaryKey]
        public uint ItemID { get; set; }
    }
}