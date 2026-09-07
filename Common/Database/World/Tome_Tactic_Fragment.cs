using System;

using FrameWork;

namespace Common
{
    /// <summary>
    /// Binds a bestiary Tome entry to the tactic line whose fragment counter it advances.
    ///
    /// Seeded from the 1.4.8 client's interface/interfacecore/tome/bestiary/species.csv. Each
    /// species carries ten (reward type, reward id) slots; a slot of type 6 is
    /// TOME_REWARD_ABILITY_COUNTER (GameData.Tome) and its reward id is the line's AcId. Slot n
    /// of a species is the Tome entry at tok_bestiary.Kill1 + n - 1.
    ///
    /// These are the "pentagon with a sword" entries the client marks in the bestiary.
    /// </summary>
    [DataTable(PreCache = true, TableName = "tome_tactic_fragments", DatabaseName = "World")]
    [Serializable]
    public class Tome_Tactic_Fragment : DataObject
    {
        /// <summary>The bestiary tok_infos.Entry whose unlock grants this fragment.</summary>
        [PrimaryKey]
        public ushort TokEntry { get; set; }

        /// <summary>Tactic line counter this fragment advances, 330-338.</summary>
        [DataElement(AllowDbNull = false)]
        public ushort AcId { get; set; }
    }
}
