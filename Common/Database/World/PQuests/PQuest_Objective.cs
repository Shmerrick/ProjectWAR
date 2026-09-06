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
        public ushort StageId { get; set; }

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
        public new string ObjectId { get; set; }

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

        /// <summary>
        /// Respawn delay in seconds for the creatures this objective spawns, overriding the
        /// default timer in <see cref="PQuestCreature.SetRespawnTimer"/>. Zero keeps the default,
        /// which is ten minutes flat inside a dungeon and a level- and rank-scaled value outside
        /// one. Set it where an objective's kill target needs a faster cycle than its spawn count
        /// can otherwise sustain.
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public uint RespawnSeconds { get; set; }

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
