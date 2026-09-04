using Common;
using FrameWork;
using System.Collections.Generic;

namespace WorldServer.Services.World
{
    [Service]
    public class TokService : ServiceBase
    {
        public static Dictionary<ushort, Tok_Info> _Toks;
        public static List<Tok_Info> DiscoveringToks;
        public static Dictionary<ushort, Tok_Bestiary> _ToksBestiary;

        [LoadingFunction(true)]
        public static void LoadTok_Infos()
        {
            Log.Debug("WorldMgr", "Loading LoadTok_Infos...");

            _Toks = new Dictionary<ushort, Tok_Info>();

            IList<Tok_Info> IToks = Database.SelectAllObjects<Tok_Info>();
            DiscoveringToks = new List<Tok_Info>();

            if (IToks != null)
            {
                foreach (Tok_Info Info in IToks)
                {
                    _Toks.Add(Info.Entry, Info);
                    if (Info.EventName.Contains("discovered") || Info.EventName.Contains("unlocked"))
                    {
                        DiscoveringToks.Add(Info);
                    }
                }

                BuildWardTaskLookup(IToks);
            }

            Log.Success("LoadTok_Infos", "Loaded " + _Toks.Count + " Tok_Infos");
        }

        [LoadingFunction(true)]
        public static void LoadTok_Bestiary()
        {
            Log.Debug("WorldMgr", "Loading LoadTok_Bestiary...");

            _ToksBestiary = new Dictionary<ushort, Tok_Bestiary>();



            IList<Tok_Bestiary> IToks = Database.SelectAllObjects<Tok_Bestiary>();

            if (IToks != null)
            {
                foreach (Tok_Bestiary Info in IToks)
                {
                    _ToksBestiary.Add(Info.Creature_Sub_Type, Info);
                }
            }

            Log.Success("LoadTok_Bestiary", "Loaded " + _ToksBestiary.Count + " Tok_Bestiary");
        }

        public static Tok_Info GetTok(ushort Entry)
        {
            Tok_Info tok;
            _Toks.TryGetValue(Entry, out tok);
            return tok;
        }

        /// <summary>Tome section holding the ward sigils and their fragments.</summary>
        private const uint SIGIL_SECTION = 5;

        /// <summary>Ward fragment task entry -> the fragment entry that completing it awards.</summary>
        private static Dictionary<ushort, ushort> _wardTaskFragments = new Dictionary<ushort, ushort>();

        /// <summary>Ward fragment entry -> task 2 of the same fragment on the tier below.</summary>
        private static Dictionary<ushort, ushort> _wardFragmentLowerTasks = new Dictionary<ushort, ushort>();

        /// <summary>
        /// Builds the ward task lookup from tok_infos section 5.
        ///
        /// The client's unlockmapping.csv and the server's tok_infos agree on the encoding:
        /// Index is the sigil tier (1 Lesser to 5 Supreme) and Flag is (fragment * 10 + task),
        /// where fragment 1-5 is boots, gloves, shoulders, helm, chest. Task 0 is the fragment
        /// award itself; tasks 1-6 are the alternative ways to earn it, any one of which
        /// completes the fragment.
        /// </summary>
        private static void BuildWardTaskLookup(IList<Tok_Info> toks)
        {
            Dictionary<uint, ushort> fragmentAwards = new Dictionary<uint, ushort>();

            foreach (Tok_Info info in toks)
            {
                if (info.Section != SIGIL_SECTION || info.Flag % 10 != 0)
                    continue;

                uint key = info.Index * 100 + info.Flag / 10;

                if (!fragmentAwards.ContainsKey(key))
                    fragmentAwards.Add(key, info.Entry);
            }

            Dictionary<ushort, ushort> taskFragments = new Dictionary<ushort, ushort>();

            foreach (Tok_Info info in toks)
            {
                if (info.Section != SIGIL_SECTION || info.Flag % 10 == 0)
                    continue;

                ushort fragmentEntry;
                if (!fragmentAwards.TryGetValue(info.Index * 100 + info.Flag / 10, out fragmentEntry))
                {
                    Log.Error("TokService", "Ward task " + info.Entry + " has no fragment award for sigil " + info.Index + " fragment " + info.Flag / 10 + ".");
                    continue;
                }

                if (!taskFragments.ContainsKey(info.Entry))
                    taskFragments.Add(info.Entry, fragmentEntry);
            }

            _wardTaskFragments = taskFragments;

            // Task 2 of a fragment is "acquire the same fragment of the next ward up", so
            // earning fragment N of tier T completes task 2 of fragment N at tier T-1 and
            // awards that fragment in turn. Map each fragment award to the lower tier's task 2
            // so the cascade can be followed downwards.
            //
            // Tier 1 has nothing below it and is skipped. Supreme's own task 2 (7670-7674) is
            // the Doomflayer equip task rather than a higher ward, and is never a cascade
            // target because no tier 6 fragment exists to reach it.
            Dictionary<ushort, ushort> fragmentLowerTasks = new Dictionary<ushort, ushort>();

            foreach (Tok_Info info in toks)
            {
                if (info.Section != SIGIL_SECTION || info.Flag % 10 != 0 || info.Index <= 1)
                    continue;

                uint fragmentIndex = info.Flag / 10;

                ushort lowerTaskEntry = 0;

                foreach (Tok_Info candidate in toks)
                {
                    if (candidate.Section != SIGIL_SECTION)
                        continue;

                    if (candidate.Index == info.Index - 1 && candidate.Flag == fragmentIndex * 10 + 2)
                    {
                        lowerTaskEntry = candidate.Entry;
                        break;
                    }
                }

                if (lowerTaskEntry == 0)
                {
                    Log.Error("TokService", "Ward fragment " + info.Entry + " (sigil " + info.Index + " fragment " + fragmentIndex + ") has no task 2 on the tier below; that cascade will not fire.");
                    continue;
                }

                if (!fragmentLowerTasks.ContainsKey(info.Entry))
                    fragmentLowerTasks.Add(info.Entry, lowerTaskEntry);
            }

            _wardFragmentLowerTasks = fragmentLowerTasks;

            Log.Success("LoadTok_Infos", "Mapped " + taskFragments.Count + " ward fragment tasks and " + fragmentLowerTasks.Count + " cross-tier cascades");
        }

        /// <summary>
        /// Returns the ward fragment awarded by completing this task, or false when the entry
        /// is not a ward fragment task.
        /// </summary>
        public static bool TryGetWardFragmentForTask(ushort taskEntry, out ushort fragmentEntry)
        {
            return _wardTaskFragments.TryGetValue(taskEntry, out fragmentEntry);
        }

        /// <summary>
        /// Returns task 2 of the same fragment one ward tier down, which acquiring this fragment
        /// completes. False when the entry is not a ward fragment or is already at the lowest tier.
        /// </summary>
        public static bool TryGetLowerWardTaskForFragment(ushort fragmentEntry, out ushort lowerTaskEntry)
        {
            return _wardFragmentLowerTasks.TryGetValue(fragmentEntry, out lowerTaskEntry);
        }

        public static Tok_Bestiary GetTokBestiary(ushort subTypeId)
        {
            Tok_Bestiary bestiary;
            _ToksBestiary.TryGetValue(subTypeId, out bestiary);
            return bestiary;
        }

    }
}
