using System.Collections.Generic;
using Common;
using FrameWork;
using WorldServer.Managers;
using WorldServer.Services.World;
using WorldServer.World.Objects;

namespace WorldServer.World.Interfaces
{
    /// <summary>
    /// A player's Kill Collector progress: accrues qualifying kills and pays them
    /// out when the player talks to the matching collector.
    /// </summary>
    /// <remarks>
    /// Kills accrue regardless of whether the player has ever met the collector,
    /// which is what makes the reward retroactive. The cap bounds the payout, not
    /// the counter, so passing the cap simply stops earning rather than losing
    /// progress.
    ///
    /// This is deliberately separate from TokInterface's Bestiary counter. That
    /// one aggregates by creature subtype for the Tome display; collector credit
    /// is an exact per-creature whitelist and the two do not agree.
    /// </remarks>
    public class KillCollectorInterface : BaseInterface
    {
        private readonly Dictionary<uint, Character_kill_collector> _progress =
            new Dictionary<uint, Character_kill_collector>();

        public override bool Load()
        {
            Player player = GetPlayer();
            if (player == null || player.Info == null)
                return false;

            IList<Character_kill_collector> rows =
                CharMgr.Database.SelectObjects<Character_kill_collector>(
                    "CharacterId=" + player.CharacterId);

            if (rows != null)
            {
                foreach (Character_kill_collector row in rows)
                {
                    if (!_progress.ContainsKey(row.CollectorEntry))
                        _progress.Add(row.CollectorEntry, row);
                }
            }

            return base.Load();
        }

        /// <summary>
        /// Credit a creature kill to every collector that accepts it.
        /// </summary>
        /// <remarks>
        /// Hot path: one dictionary lookup, and for the overwhelming majority of
        /// creatures the returned list is empty and nothing else runs.
        /// </remarks>
        public void CreditKill(uint creatureEntry)
        {
            if (!Loaded)
                return;

            List<uint> collectors = KillCollectorService.GetCollectorsForCreature(creatureEntry);
            if (collectors.Count == 0)
                return;

            for (int i = 0; i < collectors.Count; ++i)
                Accrue(collectors[i]);
        }

        private void Accrue(uint collectorEntry)
        {
            Kill_Collector_Definition def = KillCollectorService.GetDefinition(collectorEntry);
            if (def == null)
                return;

            Player player = GetPlayer();
            if (player == null)
                return;

            Character_kill_collector row;
            if (_progress.TryGetValue(collectorEntry, out row))
            {
                // Once the payout is exhausted the counter cannot affect anything,
                // so stop writing to the database on every subsequent kill.
                if (row.ClaimedKills >= def.KillCap && row.AccumulatedKills >= def.KillCap)
                    return;

                uint before = GetUnclaimed(collectorEntry);

                ++row.AccumulatedKills;
                row.Dirty = true;
                CharMgr.Database.SaveObject(row);

                // Only on the 0 -> 1 transition, so the surrounding-object scan in
                // UpdateQuestGiverAround runs once per collector rather than once
                // per kill.
                if (before == 0 && GetUnclaimed(collectorEntry) > 0)
                    player.QtsInterface.UpdateQuestGiverAround();
            }
            else
            {
                row = new Character_kill_collector
                {
                    CharacterId = player.CharacterId,
                    CollectorEntry = collectorEntry,
                    AccumulatedKills = 1,
                    ClaimedKills = 0,
                    RewardClaimed = 0
                };
                _progress.Add(collectorEntry, row);
                CharMgr.Database.AddObject(row);

                // First ever kill for this collector: 0 -> 1, same one-shot refresh.
                player.QtsInterface.UpdateQuestGiverAround();
            }
        }

        /// <summary>Qualifying kills that have accrued but not yet been paid out.</summary>
        public uint GetUnclaimed(uint collectorEntry)
        {
            Kill_Collector_Definition def = KillCollectorService.GetDefinition(collectorEntry);
            if (def == null)
                return 0;

            Character_kill_collector row;
            if (!_progress.TryGetValue(collectorEntry, out row))
                return 0;

            uint payable = row.AccumulatedKills < def.KillCap ? row.AccumulatedKills : def.KillCap;
            return payable > row.ClaimedKills ? payable - row.ClaimedKills : 0;
        }

        /// <summary>
        /// Pay out everything unclaimed at this collector. Returns the kills paid
        /// for, or 0 if there was nothing owed.
        /// </summary>
        public uint Claim(uint collectorEntry)
        {
            uint unclaimed = GetUnclaimed(collectorEntry);
            if (unclaimed == 0)
                return 0;

            Kill_Collector_Definition def = KillCollectorService.GetDefinition(collectorEntry);
            Player player = GetPlayer();
            if (def == null || player == null)
                return 0;

            Character_kill_collector row = _progress[collectorEntry];

            // Advance the claim before granting, so a failure part-way through
            // cannot pay the same kills twice.
            row.ClaimedKills += unclaimed;
            row.Dirty = true;
            CharMgr.Database.SaveObject(row);

            player.AddXp(unclaimed * def.Xp, false, false);

            if (row.ClaimedKills >= def.KillCap && row.RewardClaimed == 0)
            {
                row.RewardClaimed = 1;
                row.Dirty = true;
                CharMgr.Database.SaveObject(row);

                if (def.CompletionTokEntry != 0)
                    player.TokInterface.AddTok(def.CompletionTokEntry);
            }

            return unclaimed;
        }

        /// <summary>True once this collector can pay out nothing further.</summary>
        public bool IsMaxed(uint collectorEntry)
        {
            Kill_Collector_Definition def = KillCollectorService.GetDefinition(collectorEntry);
            if (def == null)
                return false;

            Character_kill_collector row;
            if (!_progress.TryGetValue(collectorEntry, out row))
                return false;

            return row.ClaimedKills >= def.KillCap;
        }
    }
}
