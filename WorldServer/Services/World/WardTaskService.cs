using System.Collections.Generic;

using Common;
using FrameWork;

namespace WorldServer.Services.World
{
    /// <summary>
    /// Ward fragment task counters: which action counter measures which task, and the count at
    /// which it completes. See Common.Ward_Fragment_Task and docs/WARD_SYSTEM.md.
    /// </summary>
    [Service]
    public class WardTaskService : ServiceBase
    {
        private static Dictionary<ushort, Ward_Fragment_Task> _wardTasks = new Dictionary<ushort, Ward_Fragment_Task>();

        [LoadingFunction(true)]
        public static void LoadWardFragmentTasks()
        {
            Log.Debug("WorldMgr", "Loading Ward_Fragment_Tasks...");

            Dictionary<ushort, Ward_Fragment_Task> wardTasks = new Dictionary<ushort, Ward_Fragment_Task>();

            IList<Ward_Fragment_Task> tasks = Database.SelectAllObjects<Ward_Fragment_Task>();

            if (tasks != null)
            {
                foreach (Ward_Fragment_Task task in tasks)
                {
                    if (task == null || wardTasks.ContainsKey(task.AcId))
                        continue;

                    if (task.Threshold == 0)
                    {
                        Log.Error("LoadWardFragmentTasks", "Ward task counter " + task.AcId + " has a zero threshold and would complete immediately; skipped.");
                        continue;
                    }

                    wardTasks.Add(task.AcId, task);
                }
            }

            _wardTasks = wardTasks;

            int unresolved = 0;
            foreach (KeyValuePair<ushort, Ward_Fragment_Task> kv in wardTasks)
                if (kv.Value.TokEntry == 0)
                    ++unresolved;

            Log.Success("LoadWardFragmentTasks", "Loaded " + wardTasks.Count + " ward fragment task counters (" + unresolved + " with no Tome entry to award)");
        }

        /// <summary>
        /// Returns the ward task measured by this action counter, or false when the counter is
        /// not a ward task counter.
        /// </summary>
        public static bool TryGetWardTask(ushort acId, out Ward_Fragment_Task task)
        {
            return _wardTasks.TryGetValue(acId, out task);
        }

        /// <summary>Every ward task counter, for the login send and GM inspection.</summary>
        public static IEnumerable<Ward_Fragment_Task> GetWardTasks()
        {
            return _wardTasks.Values;
        }

        /// <summary>Creature entry -> the counters its death advances.</summary>
        private static Dictionary<uint, List<ushort>> _creatureCounters = new Dictionary<uint, List<ushort>>();

        [LoadingFunction(true)]
        public static void LoadWardTaskCreatures()
        {
            Log.Debug("WorldMgr", "Loading Ward_Task_Creatures...");

            Dictionary<uint, List<ushort>> creatureCounters = new Dictionary<uint, List<ushort>>();

            IList<Ward_Task_Creature> mappings = Database.SelectAllObjects<Ward_Task_Creature>();

            if (mappings != null)
            {
                foreach (Ward_Task_Creature mapping in mappings)
                {
                    if (mapping == null)
                        continue;

                    List<ushort> counters;
                    if (!creatureCounters.TryGetValue(mapping.CreatureEntry, out counters))
                    {
                        counters = new List<ushort>();
                        creatureCounters.Add(mapping.CreatureEntry, counters);
                    }

                    if (!counters.Contains(mapping.AcId))
                        counters.Add(mapping.AcId);
                }
            }

            _creatureCounters = creatureCounters;

            Log.Success("LoadWardTaskCreatures", "Loaded ward task targets for " + creatureCounters.Count + " creatures");
        }

        /// <summary>
        /// The ward task counters this creature's death advances, or false when it is not a ward
        /// task target. A single dictionary lookup, so it is cheap enough for the death path.
        /// </summary>
        public static bool TryGetCountersForCreature(uint creatureEntry, out List<ushort> counters)
        {
            return _creatureCounters.TryGetValue(creatureEntry, out counters);
        }
    }
}
