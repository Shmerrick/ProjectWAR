using Common;
using FrameWork;
using System.Collections.Generic;

namespace WorldServer.Services.World
{
    [Service]
    public class ChapterService : ServiceBase
    {

        public static Dictionary<uint, Chapter_Info> _Chapters;
        private static Dictionary<uint, Chapter_Info> _chaptersByInfluence = new Dictionary<uint, Chapter_Info>();

        [LoadingFunction(true)]
        public static void LoadChapter_Infos()
        {
            Log.Debug("WorldMgr", "Loading Chapter_Infos...");

            _Chapters = Database.MapAllObjects<uint, Chapter_Info>("Entry");
            _chaptersByInfluence = BuildInfluenceIndex(_Chapters.Values);

            Log.Success("LoadChapter_Infos", "Loaded " + _Chapters.Count + " Chapter_Infos");
        }

        internal static Dictionary<uint, Chapter_Info> BuildInfluenceIndex(IEnumerable<Chapter_Info> chapters)
        {
            var byInfluence = new Dictionary<uint, Chapter_Info>();
            foreach (Chapter_Info chapter in chapters)
            {
                if (chapter.InfluenceEntry == 0)
                    continue;

                if (byInfluence.TryGetValue(chapter.InfluenceEntry, out Chapter_Info existing))
                {
                    // Client zone001/zone007 influenceids.csv share tracks 47/56;
                    // a track is not a unique chapter or geographical area.
                    if (ConflictingThreshold(existing.Tier1InfluenceCount, chapter.Tier1InfluenceCount) ||
                        ConflictingThreshold(existing.Tier2InfluenceCount, chapter.Tier2InfluenceCount) ||
                        ConflictingThreshold(existing.Tier3InfluenceCount, chapter.Tier3InfluenceCount))
                        Log.Error("Chapter_Info", "Conflicting reward thresholds for influence " + chapter.InfluenceEntry +
                            " on chapters " + existing.Entry + " and " + chapter.Entry + ".");

                    // Select an existing reward definition, never a zero-cap map
                    // placeholder just because the database returned it first.
                    int existingQuality = InfluenceDefinitionQuality(existing);
                    int chapterQuality = InfluenceDefinitionQuality(chapter);
                    if (chapterQuality > existingQuality || (chapterQuality == existingQuality && chapter.Entry < existing.Entry))
                        byInfluence[chapter.InfluenceEntry] = chapter;
                    continue;
                }
                byInfluence.Add(chapter.InfluenceEntry, chapter);
            }
            return byInfluence;
        }

        private static bool ConflictingThreshold(uint first, uint second)
        {
            return first != 0 && second != 0 && first != second;
        }

        private static int InfluenceDefinitionQuality(Chapter_Info chapter)
        {
            bool complete = chapter.Tier1InfluenceCount > 0 &&
                chapter.Tier2InfluenceCount >= chapter.Tier1InfluenceCount &&
                chapter.Tier3InfluenceCount >= chapter.Tier2InfluenceCount;
            return (complete ? 2 : 0) + (chapter.CreatureEntry != 0 ? 1 : 0);
        }
        public static Chapter_Info GetChapter(uint Entry)
        {
            Chapter_Info Info;
            _Chapters.TryGetValue(Entry, out Info);
            return Info;
        }

        public static ushort GetChapterByNPCID(uint Entry)
        {
            foreach (Chapter_Info chapter in _Chapters.Values)
                if (chapter.CreatureEntry == Entry)
                    return (ushort)chapter.InfluenceEntry;
            return 0;
        }

        // Function is unused
        public static List<Chapter_Info> GetChapters(ushort ZoneId)
        {
            List<Chapter_Info> Chapters = new List<Chapter_Info>();

            foreach (Chapter_Info chapter in _Chapters.Values)
                if (chapter.ZoneId == ZoneId)
                    Chapters.Add(chapter);

            return Chapters;
        }

        public static Chapter_Info GetChapterEntry(ushort InfluenceEntry)
        {
            _chaptersByInfluence.TryGetValue(InfluenceEntry, out Chapter_Info chapter);
            return chapter;
        }

        // Deferred until all immediate loaders finish, so both chapter and area caches exist.
        // Report data gaps once at boot rather than allocating/logging on every influence award.
        [LoadingFunction(false)]
        public static void ValidateAreaInfluenceReferences()
        {
            int invalidAreas = 0;
            foreach (List<Zone_Area> areas in ZoneService._Zone_Area.Values)
            {
                foreach (Zone_Area area in areas)
                {
                    bool missingOrder = area.OrderInfluenceId != 0 && !_chaptersByInfluence.ContainsKey(area.OrderInfluenceId);
                    bool missingDestro = area.DestroInfluenceId != 0 && !_chaptersByInfluence.ContainsKey(area.DestroInfluenceId);
                    if (!missingOrder && !missingDestro)
                        continue;

                    invalidAreas++;
                    Log.Error("Zone_Area", "Zone " + area.ZoneId + " piece " + area.PieceId +
                        " references missing influence tracks: Order=" + area.OrderInfluenceId +
                        " (missing=" + missingOrder + "), Destruction=" + area.DestroInfluenceId +
                        " (missing=" + missingDestro + "). Influence cannot be awarded on the missing tracks (BUG-038).");
                }
            }
            if (invalidAreas != 0)
                Log.Notice("Zone_Area", "Influence reference validation: " + invalidAreas + " area rows with missing tracks.");
            else
                Log.Success("Zone_Area", "All area influence references resolve.");
        }

        public static Dictionary<uint, List<Chapter_Reward>> _Chapters_Reward;

        [LoadingFunction(true)]
        public static void LoadChapter_Rewards()
        {
            Log.Debug("WorldMgr", "Loading LoadChapter_Rewards...");

            _Chapters_Reward = new Dictionary<uint, List<Chapter_Reward>>();
            IList<Chapter_Reward> Rewards = Database.SelectAllObjects<Chapter_Reward>();

            foreach (Chapter_Reward Reward in Rewards)
            {
                if (!_Chapters_Reward.ContainsKey(Reward.Entry))
                    _Chapters_Reward.Add(Reward.Entry, new List<Chapter_Reward>());

                _Chapters_Reward[Reward.Entry].Add(Reward);
            }

            Log.Success("LoadChapter_Infos", "Loaded " + Rewards.Count + " Chapter_Rewards");
        }

        public static List<Chapter_Reward> GetChapterRewards(uint Entry)
        {
            List<Chapter_Reward> Info;
            _Chapters_Reward.TryGetValue(Entry, out Info);
            return Info;
        }

    }
}
