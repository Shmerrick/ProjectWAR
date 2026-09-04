using System;

using FrameWork;

namespace Common
{
    /// <summary>
    /// Binds a ward fragment task to the action counter that measures it.
    ///
    /// Seeded from the 1.4.8 client's interface/interfacecore/tome/sigils/fragment_tasks.csv,
    /// which is the only source for the counter id and its completion threshold: tok_infos holds
    /// neither, and a task's "12 Times" exists only inside its display name.
    ///
    /// TokEntry is 0 for the three client-defined tasks that have no tok_infos row to award
    /// (AcIds 704, 705 and 709). Their counters still advance and display; they simply cannot
    /// complete until those rows are restored.
    /// </summary>
    [DataTable(PreCache = true, TableName = "ward_fragment_tasks", DatabaseName = "World")]
    [Serializable]
    public class Ward_Fragment_Task : DataObject
    {
        [PrimaryKey]
        public ushort AcId { get; set; }

        /// <summary>Sigil tier: 1 Lesser to 5 Supreme.</summary>
        [DataElement(AllowDbNull = false)]
        public byte SigilEntry { get; set; }

        /// <summary>Fragment within the tier: 1 boots, 2 gloves, 3 shoulders, 4 helm, 5 chest.</summary>
        [DataElement(AllowDbNull = false)]
        public byte FragmentIndex { get; set; }

        /// <summary>Task number within the fragment, 4 to 6 for counter-driven tasks.</summary>
        [DataElement(AllowDbNull = false)]
        public byte TaskNum { get; set; }

        /// <summary>The tok_infos entry completing this counter awards, or 0 when none exists.</summary>
        [DataElement(AllowDbNull = false)]
        public ushort TokEntry { get; set; }

        /// <summary>Count at which the task completes.</summary>
        [DataElement(AllowDbNull = false)]
        public uint Threshold { get; set; }
    }
}
