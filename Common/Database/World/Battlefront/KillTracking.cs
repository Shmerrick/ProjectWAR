using FrameWork;
using System;

namespace Common.Database.World.BattleFront
{
    /// <summary>
    /// Generic object located in world whitch role depends on its type.
    /// </summary>
    [DataTable(PreCache = false, TableName = "Kill_Tracker", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class KillTracker : DataObject
    {
        /// <summary>Object unique identifier.</summary>
        [PrimaryKey(AutoIncrement = true)]
        public int TrackingId { get; set; }

        /// <summary>Region id, strictly positive.</summary>
        [DataElement(AllowDbNull = false)]
        public ushort RegionId { get; set; }

        /// <summary>Zone id, strictly positive.</summary>
        [DataElement(AllowDbNull = false)]
        public ushort ZoneId { get; set; }

        /// <summary>CharacterId of the Killer</summary>
        [DataElement(AllowDbNull = false)]
        public ushort KillerCharacterId { get; set; }

        /// <summary>Id of the victim's guild</summary>
        [DataElement(AllowDbNull = false)]
        public int VictimGuildId { get; set; }

        /// <summary>Id of the killer's guild</summary>
        [DataElement(AllowDbNull = false)]
        public int KillerGuildId { get; set; }

        /// <summary>CharacterId of the victim</summary>
        [DataElement(AllowDbNull = false)]
        public int VictimCharacterId { get; set; }

        /// <summary>AccountId of the killer</summary>
        [DataElement(AllowDbNull = false)]
        public int KillerAccountId { get; set; }

        /// <summary>AccountId of the target</summary>
        [DataElement(AllowDbNull = false)]
        public int VictimAccountId { get; set; }

        /// <summary>when the event occurred</summary>
        [DataElement(AllowDbNull = false)]
        public long Timestamp { get; set; }

    }
}
