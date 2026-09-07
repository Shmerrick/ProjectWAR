using System;

using FrameWork;

namespace Common
{
    /// <summary>
    /// One of the nine tome tactic lines: the action counter that accumulates its fragments, and
    /// the three tactics those fragments unlock.
    ///
    /// Seeded from the 1.4.8 client's interface/interfacecore/tome/tactics/acid_entries.csv, which
    /// is the only source for the thresholds -- they were sent by the live server in its career
    /// package data and appear nowhere in tok_infos or the ability tables. The reward columns of
    /// that file give tactic entries 1-27, which tactic_entries.csv maps to abilities 15100-15126
    /// in order, so Tactic = 15099 + entry and the matching Section 26 unlock is 6199 + entry.
    ///
    /// Confirmed in game against the client's own fragment tooltip: the Greenskin line (AcId 333)
    /// reads 2 / 3 / 5 for Outmaneuver the Dim / the Cunning / the Clever.
    /// </summary>
    [DataTable(PreCache = true, TableName = "tome_tactic_lines", DatabaseName = "World")]
    [Serializable]
    public class Tome_Tactic_Line : DataObject
    {
        /// <summary>Client action counter id carrying this line's fragment total, 330-338.</summary>
        [PrimaryKey]
        public ushort AcId { get; set; }

        /// <summary>String id in tome/tactics/tactic_ability_names.txt, 1-9.</summary>
        [DataElement(AllowDbNull = false)]
        public byte LineIndex { get; set; }

        [DataElement(AllowDbNull = false, Varchar = 32)]
        public string Name { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort Threshold1 { get; set; }

        /// <summary>abilities.Entry of the tier 1 tactic.</summary>
        [DataElement(AllowDbNull = false)]
        public ushort Tactic1 { get; set; }

        /// <summary>tok_infos.Entry (Section 26) marking the tier 1 tactic unlocked.</summary>
        [DataElement(AllowDbNull = false)]
        public ushort TokEntry1 { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort Threshold2 { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort Tactic2 { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort TokEntry2 { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort Threshold3 { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort Tactic3 { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort TokEntry3 { get; set; }

        /// <summary>Fragment count required for the given tier, or 0 when the tier is out of range.</summary>
        public ushort ThresholdForTier(int tier)
        {
            switch (tier)
            {
                case 1: return Threshold1;
                case 2: return Threshold2;
                case 3: return Threshold3;
                default: return 0;
            }
        }

        /// <summary>abilities.Entry for the given tier, or 0 when the tier is out of range.</summary>
        public ushort TacticForTier(int tier)
        {
            switch (tier)
            {
                case 1: return Tactic1;
                case 2: return Tactic2;
                case 3: return Tactic3;
                default: return 0;
            }
        }

        /// <summary>Section 26 tok entry for the given tier, or 0 when the tier is out of range.</summary>
        public ushort TokEntryForTier(int tier)
        {
            switch (tier)
            {
                case 1: return TokEntry1;
                case 2: return TokEntry2;
                case 3: return TokEntry3;
                default: return 0;
            }
        }
    }
}
