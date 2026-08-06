using System;
using FrameWork;

namespace Common
{
    /// <summary>
    /// One creature that counts toward one Kill Collector.
    /// </summary>
    /// <remarks>
    /// Targets are exact creature entries rather than a Bestiary subtype on
    /// purpose: 22 of the 132 collectors want creatures spanning more than one
    /// CreatureSubType, and even within one subtype only specific variants
    /// qualify. The Bestiary counter in TokInterface stays a separate concern.
    ///
    /// Property order must match the column order of `kill_collector_targets`
    /// (binding is by ordinal position, not by name).
    /// </remarks>
    [DataTable(PreCache = false, TableName = "kill_collector_targets", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Kill_Collector_Target : DataObject
    {
        [PrimaryKey]
        public uint CollectorEntry { get; set; }

        [PrimaryKey]
        public uint CreatureEntry { get; set; }
    }
}
