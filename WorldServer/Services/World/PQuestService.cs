using Common;
using FrameWork;
using System.Collections.Generic;

namespace WorldServer.Services.World
{
    [Service(typeof(CreatureService), typeof(GameObjectService), typeof(ItemService))]
    public class PQuestService : ServiceBase
    {
        public static Dictionary<uint, PQuest_Info> _PQuests;

        [LoadingFunction(true)]
        public static void LoadPQuest_Info()
        {
            _PQuests = Database.MapAllObjects<uint, PQuest_Info>("Entry", "PinX != 0 AND PinY != 0");

            Log.Success("WorldMgr", "Loaded " + _PQuests.Count + " Public Quests Info");
        }

        public static Dictionary<uint, List<PQuest_Objective>> _PQuest_Objectives;

        [LoadingFunction(true)]
        public static void LoadPQuest_Objective()
        {
            _PQuest_Objectives = new Dictionary<uint, List<PQuest_Objective>>();

            IList<PQuest_Objective> PObjectives = Database.SelectObjects<PQuest_Objective>("Type != 0");

            foreach (PQuest_Objective Obj in PObjectives)
            {
                List<PQuest_Objective> Objs;
                if (!_PQuest_Objectives.TryGetValue(Obj.Entry, out Objs))
                {
                    Objs = new List<PQuest_Objective>();
                    _PQuest_Objectives.Add(Obj.Entry, Objs);
                }

                Objs.Add(Obj);
            }

            Log.Success("WorldMgr", "Loaded " + PObjectives.Count + " Public Quest Objectives");
        }

        public static Dictionary<uint, List<PQuest_Spawn>> _PQuest_Spawns;

        [LoadingFunction(true)]
        public static void LoadPQuest_Creatures()
        {
            _PQuest_Spawns = new Dictionary<uint, List<PQuest_Spawn>>();

            IList<PQuest_Spawn> PQSpawns = Database.SelectAllObjects<PQuest_Spawn>();

            foreach (PQuest_Spawn Obj in PQSpawns)
            {
                List<PQuest_Spawn> Objs;
                if (!_PQuest_Spawns.TryGetValue(Obj.Objective, out Objs))
                {
                    Objs = new List<PQuest_Spawn>();
                    _PQuest_Spawns.Add(Obj.Objective, Objs);
                }

                Objs.Add(Obj);
            }

            Log.Success("WorldMgr", "Loaded " + PQSpawns.Count + " Public Quest Spawns");
        }

        public static void GeneratePQuestObjective(PQuest_Objective Obj, PQuest_Info Q)
        {
            switch ((Objective_Type)Obj.Type)
            {
                case Objective_Type.QUEST_KILL_PLAYERS:
                    {
                        if (Obj.Description.Length < 1)
                            Obj.Description = "Enemy Players";
                    }
                    break;

                case Objective_Type.QUEST_SPEAK_TO:
                    goto case Objective_Type.QUEST_KILL_MOB;
                case Objective_Type.QUEST_PROTECT_UNIT:
                    goto case Objective_Type.QUEST_KILL_MOB;
                case Objective_Type.QUEST_KILL_MOB:
                    {
                        uint ObjID = 0;
                        uint.TryParse(Obj.ObjectId, out ObjID);

                        if (ObjID != 0)
                            Obj.Creature = CreatureService.GetCreatureProto(ObjID);

                        if (Obj.Description.Length < 1 && Obj.Creature != null)
                            Obj.Description = Obj.Creature.Name;
                    }
                    break;
                case Objective_Type.QUEST_KILL_GO:
                    {
                        uint ObjID = 0;
                        uint.TryParse(Obj.ObjectId, out ObjID);

                        if (ObjID != 0)
                            Obj.GameObject = GameObjectService.GetGameObjectProto(ObjID);

                        if (Obj.Description.Length < 1 && Obj.GameObject != null)
                            Obj.Description = "Destroy " + Obj.Creature.Name;
                    }
                    break;
                case Objective_Type.QUEST_USE_GO:
                    {
                        uint ObjID = 0;
                        uint.TryParse(Obj.ObjectId, out ObjID);

                        if (ObjID != 0)
                            Obj.GameObject = GameObjectService.GetGameObjectProto(ObjID);

                        if (Obj.Description.Length < 1 && Obj.GameObject != null)
                            Obj.Description = "Use " + Obj.GameObject.Name;
                    }
                    break;
                case Objective_Type.QUEST_GET_ITEM:
                    {
                        uint ObjID = 0;
                        uint.TryParse(Obj.ObjectId, out ObjID);

                        if (ObjID != 0)
                            Obj.Item = ItemService.GetItem_Info(ObjID);
                    }
                    break;
            }
        }

        public static List<PQuest_Loot> _PQLoot;

        [LoadingFunction(true)]
        public static void LoadPQ_Loot()
        {
            Log.Debug("WorldMgr", "PQLoot ...");
            _PQLoot = Database.SelectAllObjects<PQuest_Loot>() as List<PQuest_Loot>;
            Log.Success("PQLoot", "Loaded " + _PQLoot.Count + " Items");

            LoadPQ_Loot_Crafting();
        }

        public static List<PQuest_Loot_Crafting> _PQLoot_Crafting;

        [LoadingFunction(true)]
        public static void LoadPQ_Loot_Crafting()
        {
            Log.Debug("WorldMgr", "PQLootCrafting ...");
            _PQLoot_Crafting = Database.SelectAllObjects<PQuest_Loot_Crafting>() as List<PQuest_Loot_Crafting>;
            if (_PQLoot != null)
                Log.Success("PQLootCrafting", "Loaded " + _PQLoot.Count + " Items");
        }
    }
}
