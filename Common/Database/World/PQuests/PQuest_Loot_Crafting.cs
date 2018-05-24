using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "pquest_loot_crafting", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class PQuest_Loot_Crafting : DataObject
    {
        [PrimaryKey]
        public byte PQCraftingBag_ID { get; set; }

        [PrimaryKey]
        public uint ItemID { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte Count { get; set; }
    }
}
