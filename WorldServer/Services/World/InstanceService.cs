using Common;
using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using WorldServer.World.Objects;
using WorldServer.World.Objects.Instances;

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
        public static Dictionary<string, Instances_Statistics> _InstanceStatistics;

        #region loading methods

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

            IList<Instance_Lockouts> InstanceLockouts = Database.SelectAllObjects<Instance_Lockouts>();

            foreach (Instance_Lockouts Obj in InstanceLockouts)
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

        [LoadingFunction(true)]
        public static void LoadInstances_Statistics()
        {
            _InstanceStatistics = new Dictionary<string, Instances_Statistics>();

            IList<Instances_Statistics> InstanceStatistics = Database.SelectAllObjects<Instances_Statistics>();

            foreach (Instances_Statistics Obj in InstanceStatistics)
            {
                //_InstanceStatistics.Add(Obj.InstanceID, Obj);
                Obj.Dirty = true;
                Database.DeleteObject(Obj);
            }
            Database.ForceSave();

            Log.Success("WorldMgr", "Loaded " + _InstanceStatistics.Count + "Instances_Statistics");
        }

        #endregion

        #region access methods
        
        private static Instances_Statistics AddNewInstanceStatisticsEntry(string instanceID)
        {
            Instances_Statistics stat = new Instances_Statistics()
            {
                InstanceID = instanceID,
                lockouts_InstanceID = string.Empty,
                playerIDs = string.Empty,
                ttkPerBoss = string.Empty,
                deathCountPerBoss = string.Empty,
                attemptsPerBoss = string.Empty
            };
            _InstanceStatistics.Add(instanceID, stat);

            stat.Dirty = true;
            Database.AddObject(stat);
            return stat;
        }

        public static void SaveLockoutInstanceID(string instanceID, Instance_Lockouts lockout)
        {
            if (lockout == null)
                return;

            // instanceID:      260:123456;

            if (!_InstanceStatistics.TryGetValue(instanceID, out Instances_Statistics stat))
                stat = AddNewInstanceStatisticsEntry(instanceID);

            stat.lockouts_InstanceID = lockout.InstanceID;

            stat.Dirty = true;
            Database.SaveObject(stat);
            Database.ForceSave();
        }

        public static void SavePlayerIDs(string instanceID, List<Player> plrs)
        {
            if (string.IsNullOrEmpty(instanceID) || plrs == null || plrs.Count == 0)
                return;

            // instanceID:      260:123456;
            // playerIDs:       123;456;

            if (!_InstanceStatistics.TryGetValue(instanceID, out Instances_Statistics stat))
                stat = AddNewInstanceStatisticsEntry(instanceID);

            string newStr = string.Empty;
            foreach (Player plr in plrs)
            {
                newStr += plr.CharacterId.ToString() + ":" + plr.Name + ";";
            }
            stat.playerIDs = newStr;
            
            stat.Dirty = true;
            Database.SaveObject(stat);
            Database.ForceSave();
        }

        public static void SaveTtkPerBoss(string instanceID, InstanceBossSpawn boss, TimeSpan time)
        {
            if (boss == null || time == null)
                return;

            // instanceID:      260:123456;
            // ttkPerBoss:      330:123;331:456;

            if (!_InstanceStatistics.TryGetValue(instanceID, out Instances_Statistics stat))
                stat = AddNewInstanceStatisticsEntry(instanceID);
            
            string[] split = stat.ttkPerBoss.Split(';');
            int idx = -1;
            foreach (var s in split)
            {
                if (s.Split(':')[0].Equals(boss.BossId.ToString()))
                {
                    idx = split.ToList().IndexOf(s);
                    break;
                }
            }

            if (idx == -1) // nothing found
            {
                stat.ttkPerBoss += boss.BossId + ":" + Math.Round(time.TotalSeconds, 0) + ";";
            }
            else
            {
                string[] spl = split[idx].Split(':');
                try
                {
                    string newStr = boss.BossId + ":" + Math.Round(time.TotalSeconds, 0);
                    stat.ttkPerBoss = stat.ttkPerBoss.Replace(split[idx], newStr);
                }
                catch (Exception e)
                {
                    Log.Error(e.GetType().ToString(), e.Message + "\r\n" + e.StackTrace);
                    return;
                }
            }

            stat.Dirty = true;
            Database.SaveObject(stat);
            Database.ForceSave();
        }

        public static void SaveDeathCountPerBoss(string instanceID, InstanceBossSpawn boss, int deaths)
        {
            if (boss == null)
                return;

            // instanceID:      260:123456;
            // deathCountPerBoss: 330:2;331:1;

            if (!_InstanceStatistics.TryGetValue(instanceID, out Instances_Statistics stat))
                stat = AddNewInstanceStatisticsEntry(instanceID);

            string[] split = stat.deathCountPerBoss.Split(';');
            int idx = -1;
            foreach (var s in split)
            {
                if (s.Split(':')[0].Equals(boss.BossId.ToString()))
                {
                    idx = split.ToList().IndexOf(s);
                    break;
                }
            }

            if (idx == -1) // nothing found
            {
                stat.deathCountPerBoss += boss.BossId + ":" + deaths + ";";
            }
            else
            {
                string[] spl = split[idx].Split(':');
                try
                {
                    string newStr = boss.BossId + ":" + (int.Parse(spl[1]) + deaths).ToString();
                    stat.deathCountPerBoss = stat.deathCountPerBoss.Replace(split[idx], newStr);
                }
                catch (Exception e)
                {
                    Log.Error(e.GetType().ToString(), e.Message + "\r\n" + e.StackTrace);
                    return;
                }
            }

            stat.Dirty = true;
            Database.SaveObject(stat);
            Database.ForceSave();
        }

        public static void SaveAttemptsPerBoss(string instanceID, InstanceBossSpawn boss, int attempts)
        {
            if (boss == null)
                return;

            // instanceID:      260:123456;
            // attemptsPerBoss: 330:2;331:1;

            if (!_InstanceStatistics.TryGetValue(instanceID, out Instances_Statistics stat))
                stat = AddNewInstanceStatisticsEntry(instanceID);

            string[] split = stat.attemptsPerBoss.Split(';');
            int idx = -1;
            foreach (var s in split)
            {
                if (s.Split(':')[0].Equals(boss.BossId.ToString()))
                {
                    idx = split.ToList().IndexOf(s);
                    break;
                }
            }

            if (idx == -1) // nothing found
            {
                stat.attemptsPerBoss += boss.BossId + ":" + attempts + ";";
            }
            else
            {
                string[] spl = split[idx].Split(':');
                try
                {
                    string newStr = boss.BossId + ":" + (int.Parse(spl[1]) + attempts).ToString();
                    stat.attemptsPerBoss = stat.attemptsPerBoss.Replace(split[idx], newStr);
                }
                catch (Exception e)
                {
                    Log.Error(e.GetType().ToString(), e.Message + "\r\n" + e.StackTrace);
                    return;
                }
            }

            stat.Dirty = true;
            Database.SaveObject(stat);
            Database.ForceSave();
        }

        public static Instance_Encounter GetInstanceEncounter(uint instanceID,uint bossId)
        {
            _InstanceEncounter.TryGetValue(instanceID, out List<Instance_Encounter> bosses);
            foreach (Instance_Encounter IE in bosses)
            {
                if (bossId == IE.bossId)
                    return IE;
            }
            return null;
        }

        public static void ClearLockouts(Player plr)
        {
            if (plr._Value.GetAllLockouts().Count == 0 || plr.Zone == null)
                return;

            _InstanceInfo.TryGetValue(plr.Zone.ZoneId, out Instance_Info Info);

            if (Info == null)
                return;

            plr._Value.ClearLockouts((int)Info.LockoutTimer);
            Database.SaveObject(plr._Value);
        }

        #endregion
    }
}
