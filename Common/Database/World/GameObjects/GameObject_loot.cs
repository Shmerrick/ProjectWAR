using FrameWork;
using System;

namespace Common
{
    [DataTable(PreCache = false, TableName = "gameobject_loots", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class GameObject_loot : DataObject
    {
        [PrimaryKey]
        public uint Entry { get; set; }

        [PrimaryKey]
        public uint ItemId { get; set; }

        [DataElement()]
        public float Pct { get; set; }

        public Item_Info Info;
    }
}