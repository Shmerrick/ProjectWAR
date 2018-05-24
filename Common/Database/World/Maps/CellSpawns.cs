using System.Collections.Generic;

namespace Common.Database.World.Maps
{
    /// <summary>
    /// Entity computed on server startup providing spawning elements in a zone cell.
    /// </summary>
    public class CellSpawns
    {
        ushort _x, _y, _regionId;
        public List<Creature_spawn> CreatureSpawns = new List<Creature_spawn>();
        public List<GameObject_spawn> GameObjectSpawns = new List<GameObject_spawn>();
        public List<Chapter_Info> ChapterSpawns = new List<Chapter_Info>();
        public List<PQuest_Info> PublicQuests = new List<PQuest_Info>();

        public CellSpawns(ushort regionId, ushort x, ushort y)
        {
            _regionId = regionId;
            _x = x;
            _y = y;
        }

        public void AddSpawn(Creature_spawn spawn)
        {
            CreatureSpawns.Add(spawn);
        }

        public void AddSpawn(GameObject_spawn spawn)
        {
            GameObjectSpawns.Add(spawn);
        }

        public void AddChapter(Chapter_Info chapter)
        {
            chapter.OffX = _x;
            chapter.OffY = _y;

            ChapterSpawns.Add(chapter);
        }

        public void AddPQuest(PQuest_Info pQuest)
        {
            pQuest.OffX = _x;
            pQuest.OffY = _y;

            PublicQuests.Add(pQuest);
        }
    }
}
