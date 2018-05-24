using Common;
using FrameWork;
using System.Collections.Generic;

namespace WorldServer.Services.World
{
    [Service]
    public class ChapterService : ServiceBase
    {

        public static Dictionary<uint, Chapter_Info> _Chapters;

        [LoadingFunction(true)]
        public static void LoadChapter_Infos()
        {
            Log.Debug("WorldMgr", "Loading Chapter_Infos...");

            _Chapters = Database.MapAllObjects<uint, Chapter_Info>("Entry");

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
            List<Chapter_Info> Chapters = new List<Chapter_Info>();

            foreach (Chapter_Info chapter in _Chapters.Values)
                if (chapter.InfluenceEntry == InfluenceEntry)
                    return chapter;
            return null;
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
