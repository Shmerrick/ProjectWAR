using System.Collections.Generic;
using Common;
using FrameWork;

namespace WorldServer.Services.World
{
    /// <summary>
    /// Loads Kill Collector definitions and builds the creature -> collectors
    /// reverse index used on the kill path.
    /// </summary>
    /// <remarks>
    /// The reverse index exists so that a kill costs one dictionary lookup rather
    /// than a scan of all 132 definitions. Creature death is a hot path, so per
    /// AGENTS.md rule 2 it must not contain an unbounded scan.
    /// </remarks>
    [Service]
    public class KillCollectorService : ServiceBase
    {
        private static Dictionary<uint, Kill_Collector_Definition> _definitions;

        /// <summary>Creature entry -> the collectors that accept it. Usually one.</summary>
        private static Dictionary<uint, List<uint>> _creatureToCollectors;

        private static readonly List<uint> _noCollectors = new List<uint>();

        [LoadingFunction(true)]
        public static void LoadKillCollectors()
        {
            Log.Debug("WorldMgr", "Loading Kill Collector definitions...");

            _definitions = new Dictionary<uint, Kill_Collector_Definition>();
            _creatureToCollectors = new Dictionary<uint, List<uint>>();

            IList<Kill_Collector_Definition> defs = Database.SelectAllObjects<Kill_Collector_Definition>();
            if (defs != null)
            {
                foreach (Kill_Collector_Definition def in defs)
                {
                    if (def.KillCap == 0)
                    {
                        Log.Error("KillCollectorService", "Collector " + def.CollectorEntry + " has KillCap 0, skipped");
                        continue;
                    }

                    if (_definitions.ContainsKey(def.CollectorEntry))
                    {
                        Log.Error("KillCollectorService", "Duplicate collector definition " + def.CollectorEntry + ", skipped");
                        continue;
                    }

                    _definitions.Add(def.CollectorEntry, def);
                }
            }

            int targetCount = 0;
            IList<Kill_Collector_Target> targets = Database.SelectAllObjects<Kill_Collector_Target>();
            if (targets != null)
            {
                foreach (Kill_Collector_Target target in targets)
                {
                    if (!_definitions.ContainsKey(target.CollectorEntry))
                    {
                        Log.Error("KillCollectorService", "Target for unknown collector " + target.CollectorEntry + ", skipped");
                        continue;
                    }

                    List<uint> collectors;
                    if (!_creatureToCollectors.TryGetValue(target.CreatureEntry, out collectors))
                    {
                        collectors = new List<uint>(1);
                        _creatureToCollectors.Add(target.CreatureEntry, collectors);
                    }

                    if (!collectors.Contains(target.CollectorEntry))
                    {
                        collectors.Add(target.CollectorEntry);
                        ++targetCount;
                    }
                }
            }

            int inert = _definitions.Count - CountCollectorsWithTargets();
            Log.Success("LoadKillCollectors", "Loaded " + _definitions.Count + " Kill Collectors, "
                + targetCount + " creature targets" + (inert > 0 ? " (" + inert + " with no targets)" : ""));
        }

        private static int CountCollectorsWithTargets()
        {
            HashSet<uint> seen = new HashSet<uint>();
            foreach (KeyValuePair<uint, List<uint>> pair in _creatureToCollectors)
                foreach (uint collector in pair.Value)
                    seen.Add(collector);
            return seen.Count;
        }

        /// <summary>True if this NPC is a configured Kill Collector.</summary>
        public static bool IsCollector(uint collectorEntry)
        {
            return _definitions != null && _definitions.ContainsKey(collectorEntry);
        }

        public static Kill_Collector_Definition GetDefinition(uint collectorEntry)
        {
            Kill_Collector_Definition def = null;
            if (_definitions != null)
                _definitions.TryGetValue(collectorEntry, out def);
            return def;
        }

        /// <summary>
        /// The collectors that credit a kill of this creature. Empty for the vast
        /// majority of creatures, so callers should test Count before allocating.
        /// </summary>
        public static List<uint> GetCollectorsForCreature(uint creatureEntry)
        {
            List<uint> collectors;
            if (_creatureToCollectors != null && _creatureToCollectors.TryGetValue(creatureEntry, out collectors))
                return collectors;
            return _noCollectors;
        }
    }
}
