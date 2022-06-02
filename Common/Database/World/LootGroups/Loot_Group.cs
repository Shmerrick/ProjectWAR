using FrameWork;
using System;
using System.Collections.Generic;

namespace Common
{
    [DataTable(PreCache = false, TableName = "loot_groups", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Loot_Group : DataObject
    {
        [PrimaryKey]
        public uint Entry { get; set; }

        [DataElement]
        public string Name { get; set; }

        [DataElement]
        public byte DropEvent { get; set; }

        [DataElement]
        public uint CreatureID { get; set; }

        [DataElement]
        public ushort CreatureSubType { get; set; }

        [DataElement]
        public float DropChance { get; set; }

        [DataElement]
        public byte DropCount { get; set; }

        [DataElement]
        public bool ReqGroupUsable { get; set; }

        [DataElement]
        public ushort ReqActiveQuest { get; set; }

        [DataElement]
        public ushort SpecificZone { get; set; }

        public List<Loot_Group_Item> LootGroupItems;
    }
}