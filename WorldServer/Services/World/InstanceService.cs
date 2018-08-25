using Common;
using FrameWork;
using System.Collections.Generic;

namespace WorldServer.Services.World
{
    [Service(typeof(CreatureService), typeof(GameObjectService), typeof(ItemService))]
    public class InstanceService : ServiceBase
    {
        public static Dictionary<uint, List<Instance_Spawn>> _InstanceSpawns;
        public static Dictionary<uint, List<Instance_Boss_Spawn>> _InstanceBossSpawns;
        public static Dictionary<uint, Instance_Info> _InstanceInfo;
        public static Dictionary<uint, List<Instance_Encounter>> _InstanceEncounter;
        public static Dictionary<string, Instance_Lockouts> _InstanceLockouts;

        [LoadingFunction(true)]
        public static void LoadInstance_Creatures()
        {
            _InstanceSpawns = new Dictionary<uint, List<Instance_Spawn>>();

            IList<Instance_Spawn> InstanceSpawns = Database.SelectAllObjects<Instance_Spawn>();

            foreach (Instance_Spawn Obj in InstanceSpawns)
            {
                List<Instance_Spawn> Objs;
                if (!_InstanceSpawns.TryGetValue(Obj.ZoneID, out Objs))
                {
                    Objs = new List<Instance_Spawn>();
                    _InstanceSpawns.Add(Obj.ZoneID, Objs);
                }

                Objs.Add(Obj);
            }
            Log.Success("WorldMgr", "Loaded " + _InstanceSpawns.Count + "Instance_Spawn");
        }

        [LoadingFunction(true)]
        public static void LoadInstance_Boss_Creatures()
        {
            _InstanceBossSpawns = new Dictionary<uint, List<Instance_Boss_Spawn>>();

            IList<Instance_Boss_Spawn> InstanceSpawns = Database.SelectAllObjects<Instance_Boss_Spawn>();

            foreach (Instance_Boss_Spawn Obj in InstanceSpawns)
            {
                List<Instance_Boss_Spawn> Objs;
                if (!_InstanceBossSpawns.TryGetValue(Obj.InstanceID, out Objs))
                {
                    Objs = new List<Instance_Boss_Spawn>();
                    _InstanceBossSpawns.Add(Obj.InstanceID, Objs);
                }

                Objs.Add(Obj);
            }
            Log.Success("WorldMgr", "Loaded " + _InstanceBossSpawns.Count + "Instance_Boss_Spawn");
        }

        [LoadingFunction(true)]
        public static void LoadInstance_Info()
        {
            _InstanceInfo = new Dictionary<uint, Instance_Info>();

            IList<Instance_Info> InstanceInfo = Database.SelectAllObjects<Instance_Info>();

            foreach(Instance_Info II in InstanceInfo)
            {
                _InstanceInfo.Add(II.ZoneID, II);

            }
            Log.Success("WorldMgr", "Loaded " + _InstanceInfo.Count + "Instance_Info");
        }

        [LoadingFunction(true)]
        public static void LoadInstance_Lockouts()
        {
            _InstanceLockouts = new Dictionary<string, Instance_Lockouts>();

            IList<Instance_Lockouts> InstanceSpawns = Database.SelectAllObjects<Instance_Lockouts>();

            foreach (Instance_Lockouts Obj in InstanceSpawns)
            {
                _InstanceLockouts.Add(Obj.InstanceID, Obj);
            }
            Log.Success("WorldMgr", "Loaded " + _InstanceLockouts.Count + "Instance_Lockouts");
        }

        [LoadingFunction(true)]
        public static void LoadInstance_Encounter()
        {
            _InstanceEncounter = new Dictionary<uint, List<Instance_Encounter>>();

            IList<Instance_Encounter> InstanceEncounter = Database.SelectAllObjects<Instance_Encounter>();

            foreach (Instance_Encounter Obj in InstanceEncounter)
            {
                List<Instance_Encounter> Objs;
                if (!_InstanceEncounter.TryGetValue(Obj.InstanceID, out Objs))
                {
                    Objs = new List<Instance_Encounter>();
                    _InstanceEncounter.Add(Obj.InstanceID, Objs);
                }

                Objs.Add(Obj);
            }
            Log.Success("WorldMgr", "Loaded " + _InstanceEncounter.Count + "Instance_Encounters");
        }

        public static Instance_Encounter GetInstanceEncounter(uint instanceID,uint BossID)
        {
            List<Instance_Encounter> bosses;
            _InstanceEncounter.TryGetValue(instanceID, out bosses);
            foreach(Instance_Encounter IE in bosses)
            {
                if (BossID == IE.BossID)
                    return IE;
            }
            return null;
        }
    }
}
