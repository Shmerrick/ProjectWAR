using System;

using FrameWork;

namespace Common
{
    /// <summary>
    /// Progress toward one of a character's action counters.
    ///
    /// The client identifies these by an action counter id and renders them as "(current/max)" on
    /// the ward fragment task pages, receiving updates through F_ACTION_COUNTER_UPDATE. Ward task
    /// counters occupy ids 700-735; see war_world.ward_fragment_tasks.
    ///
    /// This is deliberately not stored in characters_toks, whose Count column exists but whose row
    /// presence means the unlock is complete -- partial progress there would mark the task done at
    /// the first tick. It is also kept out of characters_toks_kills, because SendBestiary writes
    /// every row of that table into the bestiary packet behind a count prefix.
    /// </summary>
    [DataTable(PreCache = false, TableName = "characters_action_counters", DatabaseName = "Characters", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Character_action_counter : DataObject
    {
        [PrimaryKey]
        public uint CharacterId { get; set; }

        [PrimaryKey]
        public ushort AcId { get; set; }

        [DataElement]
        public uint Count { get; set; }
    }
}
