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
            var byInfluence = new Dictionary<uint, Chapter_Info>();
            foreach (Chapter_Info chapter in _Chapters.Values)
            {
                if (chapter.InfluenceEntry == 0)
                    continue;

                if (byInfluence.TryGetValue(chapter.InfluenceEntry, out Chapter_Info existing))
                {
                    Log.Notice("Chapter_Info", "Shared influence " + chapter.InfluenceEntry + " on chapters " +
                        existing.Entry + " and " + chapter.Entry + "; retaining the first chapter.");
                    continue;
                }
                byInfluence.Add(chapter.InfluenceEntry, chapter);
            }
            _chaptersByInfluence = byInfluence;

            Log.Success("LoadChapter_Infos", "Loaded " + _Chapters.Count + " Chapter_Infos");
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
            Log.Notice("Zone_Area", "Influence reference validation: " + invalidAreas + " area rows with missing tracks.");
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
