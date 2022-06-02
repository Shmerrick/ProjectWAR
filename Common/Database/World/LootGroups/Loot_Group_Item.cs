using FrameWork;
using System;

namespace Common
{
    [DataTable(PreCache = false, TableName = "loot_group_items", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Loot_Group_Item : DataObject
    {
        public Item_Info Item;

        public Loot_Group Loot_Group;

        [DataElement()]
        public uint ItemID { get; set; }

        [DataElement()]
        public uint LootGroupID { get; set; }
        [DataElement()]
        public byte MaxRank { get; set; }

        [DataElement()]
        public byte MaxRenown { get; set; }

        [DataElement()]
        public byte MinRank { get; set; }
        [DataElement()]
        public byte MinRenown { get; set; }
    }
}