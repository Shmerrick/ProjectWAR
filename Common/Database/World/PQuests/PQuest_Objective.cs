using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "pquest_objectives", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class PQuest_Objective : DataObject
    {
        [PrimaryKey(AutoIncrement = true)]
        public uint Guid { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint Entry { get; set; }

        [DataElement(Varchar = 255, AllowDbNull = false)]
        public string StageName { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte Type { get; set; }

        [DataElement(Varchar = 255, AllowDbNull = false)]
        public string Objective { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort Count { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort Time { get; set; }

        [DataElement(AllowDbNull = false)]
        public string Description { get; set; }

        [DataElement(Varchar = 255, AllowDbNull = false)]
#pragma warning disable CS0108 // Член скрывает унаследованный член: отсутствует новое ключевое слово
        public string ObjectId { get; set; }
#pragma warning restore CS0108 // Член скрывает унаследованный член: отсутствует новое ключевое слово

        [DataElement(Varchar = 255, AllowDbNull = true)]
        public string ObjectId2 { get; set; }

        [DataElement(Varchar = 255, AllowDbNull = true)]
        public string ObjectId3 { get; set; }

        [DataElement(Varchar = 255, AllowDbNull = true)]
        public string ObjectId4 { get; set; }

        [DataElement(Varchar = 255, AllowDbNull = true)]
        public string ObjectId5 { get; set; }

        [DataElement(Varchar = 255, AllowDbNull = true)]
        public string ObjectId6 { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint TokCompleted { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte NoRespawn { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint SoundId { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint SoundDelay { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint SoundIteration { get; set; }

        public PQuest_Info Quest;

        public Item_Info Item;
        public Creature_proto Creature;
        public GameObject_proto GameObject = null;

        public List<PQuest_Spawn> Spawns = new List<PQuest_Spawn>();
    }
}
