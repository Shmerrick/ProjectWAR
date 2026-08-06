using System;
using FrameWork;

namespace Common
{
    /// <summary>
    /// One Kill Collector NPC's configuration: how many qualifying kills it pays
    /// out for, how much XP each is worth, and any one-time unlock on completion.
    /// The creatures that qualify live in <see cref="Kill_Collector_Target"/>.
    /// </summary>
    /// <remarks>
    /// Property order must match the column order of `kill_collector_definitions`.
    /// FrameWork's StaticBindSelect binds result columns to properties by ordinal
    /// position, so reordering these silently shifts every field after the change.
    /// </remarks>
    [DataTable(PreCache = false, TableName = "kill_collector_definitions", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Kill_Collector_Definition : DataObject
    {
        [PrimaryKey]
        public uint CollectorEntry { get; set; }

        /// <summary>Qualifying kills paid for. Retail is documented as 60; the
        /// seeded values are current RoR's 20-60 by chapter.</summary>
        [DataElement]
        public ushort KillCap { get; set; }

        /// <summary>XP granted per qualifying kill claimed.</summary>
        [DataElement]
        public uint Xp { get; set; }

        /// <summary>ToK entry unlocked once when the cap is reached. 0 = none.</summary>
        [DataElement]
        public ushort CompletionTokEntry { get; set; }

        /// <summary>Human-readable target ("Crazy Squigs"), for logging and hint text.</summary>
        [DataElement(Varchar = 128)]
        public string TargetLabel { get; set; }
    }
}
