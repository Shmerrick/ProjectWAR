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

        /// <summary>
        /// Long stage title shown in the client's public-quest tracker header, e.g.
        /// "Destroy Siphoning Contraptions". F_OBJECTIVE_INFO carries this and the short
        /// <see cref="StageName"/> label ("Stage I") as two separate strings. Empty falls back
        /// to StageName, which is how every row that predates the column behaves.
        /// </summary>
        [DataElement(Varchar = 255, AllowDbNull = false)]
        public string StageTitle { get; set; }

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

        /// <summary>
        /// Suppresses the stage countdown and the stage fail timer entirely. Time = 0 cannot
        /// express this: 2748 of the 2766 objective rows leave Time at zero and rely on
        /// <c>PublicQuest.TIME_EACH_STAGE</c> as their timeout, so zero means "unset", not
        /// "none". The Thanquol's Incursion captures send a stage total and remaining of zero
        /// for all five numbered stages, which this reproduces without disturbing the default
        /// every other public quest depends on.
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public byte NoStageTimer { get; set; }

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

        /// <summary>
        /// Ephemeral objective id the live 1.4.8 server sent in F_OBJECTIVE_INFO and
        /// F_OBJECTIVE_UPDATE. It is not the creature or gameobject entry: the Gunbad captures
        /// send 870/871 for public quest 181, whose ObjectId column holds creature 15106.
        /// Zero keeps the legacy behaviour of sending ObjectId.
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public uint ClientObjectiveId { get; set; }

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
