using System;

using FrameWork;

namespace Common
{
    /// <summary>
    /// A tome tactic this character has bought from a City Librarian.
    ///
    /// Every other entry in AbilityInterface's ability list is rederived at load from career and
    /// level, so nothing needed storing. A tome tactic is not: it is earned through Tome fragments
    /// and then purchased, so without this row the purchase is lost at the next login.
    ///
    /// Deliberately not reusing `character_abilities`: that table has no primary key, carries a
    /// LastCast column suggesting it was meant for cooldown persistence, and is written by nothing.
    /// </summary>
    [DataTable(PreCache = false, TableName = "characters_tome_tactics", DatabaseName = "Characters")]
    [Serializable]
    public class Characters_tome_tactic : DataObject
    {
        [PrimaryKey]
        public uint CharacterId { get; set; }

        /// <summary>abilities.Entry of the purchased tactic, 15100-15126.</summary>
        [PrimaryKey]
        public ushort TacticEntry { get; set; }
    }
}
