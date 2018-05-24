using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    // Fixed value of a character
    [DataTable(PreCache = false, TableName = "quests", DatabaseName = "World")]
    [Serializable]
    public class Quest : DataObject
    {
        [PrimaryKey]
        public ushort Entry { get; set; }

        [DataElement(Varchar=255,AllowDbNull=false)]
        public string Name { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte Type { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte MinLevel { get; set; }

        [DataElement(AllowDbNull = false)]
        public string Description { get; set; }

        [DataElement(AllowDbNull = false)]
        public string OnCompletionQuest { get; set; }

        [DataElement(AllowDbNull = false)]
        public string ProgressText { get; set; }

        [DataElement(AllowDbNull = false)]
        public string Particular { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint Xp { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint Gold { get; set; }

        [DataElement(AllowDbNull = false)]
        public string Given { get; set; }

        [DataElement(AllowDbNull = false)]
        public string Choice { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte ChoiceCount { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort PrevQuest { get; set; }

        [DataElement(AllowDbNull = false)]
        public bool Repeatable { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte MaxLevel { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte MinRenown { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte MaxRenown { get; set; }

        [DataElement(AllowDbNull = false)]
        public bool Active { get; set; }

        [DataElement(AllowDbNull = false)]
        public bool Shareable { get; set; }

        public List<Quest_Objectives> Objectives = new List<Quest_Objectives>();
        public List<Quest_Map> Maps = new List<Quest_Map>();
        public Dictionary<Item_Info, uint> Rewards = new Dictionary<Item_Info, uint>();
    }
}
