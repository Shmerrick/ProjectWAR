using Common;
using FrameWork;
using System.Collections.Generic;

namespace WorldServer.Services.World
{
    [Service]
    public class QuestService : ServiceBase
    {
        public static Dictionary<ushort, Quest> _Quests;

        [LoadingFunction(true)]
        public static void LoadQuests()
        {
            _Quests = Database.MapAllObjects<ushort, Quest>("Entry", 5000);

            Log.Success("LoadQuests", "Loaded " + _Quests.Count + " Quests");
        }
        public static Quest GetQuest(ushort QuestID)
        {
            Quest Q;
            _Quests.TryGetValue(QuestID, out Q);
            return Q;
        }

        public static Dictionary<int, Quest_Objectives> _Objectives;

        [LoadingFunction(true)]
        public static void LoadQuestsObjectives()
        {
            _Objectives = Database.MapAllObjects<int, Quest_Objectives>("Guid");

            Log.Success("LoadQuestsObjectives", "Loaded " + _Objectives.Count + " Quests Objectives");
        }

        public static List<Quest_Map> _QuestMaps;

        [LoadingFunction(true)]
        public static void LoadQuestsMaps()
        {
            _QuestMaps = Database.SelectAllObjects<Quest_Map>() as List<Quest_Map>;

            Log.Success("LoadQuestsMaps", "Loaded " + _QuestMaps.Count + " Quests Maps");
        }

        public static Quest_Objectives GetQuestObjective(int Guid)
        {
            Quest_Objectives Obj;
            _Objectives.TryGetValue(Guid, out Obj);
            return Obj;
        }

        public static Dictionary<uint, List<Quest>> _CreatureStarter;

        public static void LoadQuestCreatureStarter()
        {
            _CreatureStarter = new Dictionary<uint, List<Quest>>();

            IList<Quest_Creature_Starter> Starters = Database.SelectAllObjects<Quest_Creature_Starter>();

            if (Starters != null)
            {
                Quest Q;
                foreach (Quest_Creature_Starter Start in Starters)
                {
                    if (!_CreatureStarter.ContainsKey(Start.CreatureID))
                        _CreatureStarter.Add(Start.CreatureID, new List<Quest>());

                    Q = GetQuest(Start.Entry);

                    if (Q != null)
                        _CreatureStarter[Start.CreatureID].Add(Q);
                }
            }

            Log.Success("LoadCreatureQuests", "Loaded " + _CreatureStarter.Count + " Quests Creature Starter");
        }

        public static List<Quest> GetStartQuests(uint CreatureID)
        {
            List<Quest> Quests;
            _CreatureStarter.TryGetValue(CreatureID, out Quests);
            return Quests;
        }

        public static Dictionary<uint, List<Quest>> _CreatureFinisher;

        public static void LoadQuestCreatureFinisher()
        {
            _CreatureFinisher = new Dictionary<uint, List<Quest>>();

            IList<Quest_Creature_Finisher> Finishers = Database.SelectAllObjects<Quest_Creature_Finisher>();

            if (Finishers != null)
            {
                Quest Q;
                foreach (Quest_Creature_Finisher Finisher in Finishers)
                {
                    if (!_CreatureFinisher.ContainsKey(Finisher.CreatureID))
                        _CreatureFinisher.Add(Finisher.CreatureID, new List<Quest>());

                    Q = GetQuest(Finisher.Entry);

                    if (Q != null)
                        _CreatureFinisher[Finisher.CreatureID].Add(Q);
                }
            }

            Log.Success("LoadCreatureQuests", "Loaded " + _CreatureFinisher.Count + " Quests Creature Finisher");
        }

        public static List<Quest> GetFinishersQuests(uint CreatureID)
        {
            List<Quest> Quests;
            _CreatureFinisher.TryGetValue(CreatureID, out Quests);
            return Quests;
        }
        public static uint GetQuestCreatureFinisher(ushort QuestId)
        {
            foreach (KeyValuePair<uint, List<Quest>> Kp in _CreatureFinisher)
            {
                foreach (Quest Q in Kp.Value)
                    if (Q.Entry == QuestId)
                        return Kp.Key;
            }

            return 0;
        }

        public static bool HasQuestToFinish(uint CreatureID, ushort QuestID)
        {
            List<Quest> Quests;
            if (_CreatureFinisher.TryGetValue(CreatureID, out Quests))
            {
                foreach (Quest Q in Quests)
                    if (Q.Entry == QuestID)
                        return true;
            }

            return false;
        }

    }
}
