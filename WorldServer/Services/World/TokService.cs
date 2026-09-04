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

            Log.Success("LoadTok_Infos", "Mapped " + taskFragments.Count + " ward fragment tasks");
        }

        /// <summary>
        /// Returns the ward fragment awarded by completing this task, or false when the entry
        /// is not a ward fragment task.
        /// </summary>
        public static bool TryGetWardFragmentForTask(ushort taskEntry, out ushort fragmentEntry)
        {
            return _wardTaskFragments.TryGetValue(taskEntry, out fragmentEntry);
        }

        public static Tok_Bestiary GetTokBestiary(ushort subTypeId)
        {
            Tok_Bestiary bestiary;
            _ToksBestiary.TryGetValue(subTypeId, out bestiary);
            return bestiary;
        }

    }
}
