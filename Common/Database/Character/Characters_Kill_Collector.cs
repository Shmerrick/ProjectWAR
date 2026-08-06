using System;
using FrameWork;

namespace Common
{
    /// <summary>
    /// A character's progress against one Kill Collector.
    /// </summary>
    /// <remarks>
    /// AccumulatedKills only ever grows; ClaimedKills records what has already
    /// been paid out. The unclaimed balance is
    /// <c>min(AccumulatedKills, KillCap) - ClaimedKills</c>, which is what makes
    /// progress retroactive: kills accrue whether or not the player has ever met
    /// the collector, and the cap bounds the payout rather than the counter.
    ///
    /// Property order must match the column order of `characters_kill_collector`
    /// (binding is by ordinal position, not by name).
    /// </remarks>
    [DataTable(PreCache = false, TableName = "characters_kill_collector", DatabaseName = "Characters", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Character_kill_collector : DataObject
    {
        [PrimaryKey]
        public uint CharacterId { get; set; }

        [PrimaryKey]
        public uint CollectorEntry { get; set; }

        [DataElement]
        public uint AccumulatedKills { get; set; }

        [DataElement]
        public uint ClaimedKills { get; set; }

        [DataElement]
        public byte RewardClaimed { get; set; }
    }
}
