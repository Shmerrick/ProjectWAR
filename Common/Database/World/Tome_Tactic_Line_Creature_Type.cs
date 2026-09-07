using System;

using FrameWork;

namespace Common
{
    /// <summary>
    /// Which creature types a tome tactic line acts against.
    ///
    /// Derived from the monster-type operands in the client's ability component data: every tome
    /// tactic component is gated by AbilityOperation 16 (MonsterType) and its operand names one of
    /// the client's monster-type codes. See migration 57 for the decoded code table and the
    /// corroborating checks.
    /// </summary>
    [DataTable(PreCache = true, TableName = "tome_tactic_line_creature_types", DatabaseName = "World")]
    [Serializable]
    public class Tome_Tactic_Line_Creature_Type : DataObject
    {
        [PrimaryKey]
        public ushort AcId { get; set; }

        /// <summary>GameData.CreatureTypes value, matching creature_protos.CreatureType.</summary>
        [PrimaryKey]
        public byte CreatureType { get; set; }
    }
}
