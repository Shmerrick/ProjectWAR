using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "chapter_rewards", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Chapter_Reward : DataObject
    {
        [PrimaryKey]
        public uint Entry { get; set; }

        [DataElement]
        public byte Realm { get; set; }

        [PrimaryKey]
        public uint ItemId { get; set; }

        [DataElement]
        public uint InfluenceCount { get; set; }

        public Chapter_Info Chapter;
        public Item_Info Item;
    }
}
