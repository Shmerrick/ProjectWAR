using Common;
using Common.Database.World.BattleFront;
using FrameWork;
using GameData;
using System.Collections.Generic;
using System.Linq;
using Common.Database.World.Battlefront;
using WorldServer.World.Positions;

namespace WorldServer.Services.World
{
    [Service]
    public class BattleFrontService : ServiceBase
    {
        [LoadingFunction(true)]
        public static void LoadBattleFront()
        {
            BattleFrontStatus = Database.SelectAllObjects<BattleFrontStatus>().ToDictionary(g => g.RegionId);

            LoadBattleFrontGuards();
            LoadResourceSpawns();

            LoadBattleFrontObjectives();
            LoadBattleFrontObjects();

            LoadKeepInfos();
            LoadPlayerKeepSpawnPoints();
        }

        #region Objectives
        public static Dictionary<uint, List<BattleFront_Objective>> _BattleFrontObjectives = new Dictionary<uint, List<BattleFront_Objective>>();

        private static void LoadBattleFrontObjectives()
        {
            _BattleFrontObjectives = new Dictionary<uint, List<BattleFront_Objective>>();

            Log.Debug("WorldMgr", "Loading BattleFront_Objectives...");

            IList<BattleFront_Objective> Objectives = Database.SelectAllObjects<BattleFront_Objective>();

            int Count = 0;
            foreach (BattleFront_Objective Obj in Objectives)
            {
                if (!_BattleFrontObjectives.ContainsKey(Obj.RegionId))
                    _BattleFrontObjectives.Add(Obj.RegionId, new List<BattleFront_Objective>());

                _BattleFrontObjectives[Obj.RegionId].Add(Obj);

                if (_BattleFrontGuards.ContainsKey(Obj.Entry))
                    Obj.Guards = _BattleFrontGuards[Obj.Entry];

                ++Count;
            }

            Log.Success("WorldMgr", "Loaded " + Count + " Campaign Objectives");
        }

        public static List<BattleFront_Objective> GetBattleFrontObjectives(uint RegionId)
        {
            if (_BattleFrontObjectives.ContainsKey(RegionId))
            {
                return _BattleFrontObjectives[RegionId];
            }

            return null;
        }
        #endregion

        #region Keeps
        public static Dictionary<uint, List<Keep_Info>> _KeepInfos = new Dictionary<uint, List<Keep_Info>>();

        public static void LoadKeepInfos()
        {
            LoadKeepCreatures();
            LoadKeepDoors();
            LoadKeepSiegeSpawnPoints();

            _KeepInfos = new Dictionary<uint, List<Keep_Info>>();

            Log.Debug("WorldMgr", "Loading BattleFront_Objectives...");

            IList<Keep_Info> keepInfos = Database.SelectAllObjects<Keep_Info>();

            int Count = 0;
            foreach (Keep_Info keepInfo in keepInfos)
            {
                if (!_KeepInfos.ContainsKey(keepInfo.RegionId))
                    _KeepInfos.Add(keepInfo.RegionId, new List<Keep_Info>());

                _KeepInfos[keepInfo.RegionId].Add(keepInfo);

                if (_KeepCreatures.ContainsKey(keepInfo.KeepId))
                    keepInfo.Creatures = _KeepCreatures[keepInfo.KeepId];

                if (_KeepDoors.ContainsKey(keepInfo.KeepId))
                    keepInfo.Doors = _KeepDoors[keepInfo.KeepId];

                if (_KeepSiegeSpawnPoints.ContainsKey(keepInfo.KeepId))
                    keepInfo.KeepSiegeSpawnPoints = _KeepSiegeSpawnPoints[keepInfo.KeepId];

                ++Count;
            }

            Log.Success("WorldMgr", "Loaded " + Count + " Keep Infos");
        }

        public static Dictionary<int, List<Keep_Creature>> _KeepCreatures = new Dictionary<int, List<Keep_Creature>>();
        public static void LoadKeepCreatures()
        {
            _KeepCreatures = new Dictionary<int, List<Keep_Creature>>();

            Log.Debug("WorldMgr", "Loading Keep_Creatures...");

            IList<Keep_Creature> Creatures = Database.SelectAllObjects<Keep_Creature>();

            int Count = 0;
            foreach (Keep_Creature Creature in Creatures)
            {
                if (!_KeepCreatures.ContainsKey(Creature.KeepId))
                    _KeepCreatures.Add(Creature.KeepId, new List<Keep_Creature>());

                _KeepCreatures[Creature.KeepId].Add(Creature);
                ++Count;
            }

            Log.Success("WorldMgr", "Loaded " + Count + " Keep Creatures");
        }


        public static Dictionary<int, List<Keep_Door>> _KeepDoors = new Dictionary<int, List<Keep_Door>>();
        public static void LoadKeepDoors()
        {
            _KeepDoors = new Dictionary<int, List<Keep_Door>>();

            Log.Debug("WorldMgr", "Loading Keep_Doors...");

            IList<Keep_Door> Doors = Database.SelectAllObjects<Keep_Door>();

            int Count = 0;
            foreach (Keep_Door Door in Doors)
            {
                if (!_KeepDoors.ContainsKey(Door.KeepId))
                    _KeepDoors.Add(Door.KeepId, new List<Keep_Door>());

                _KeepDoors[Door.KeepId].Add(Door);
                ++Count;
            }

            Log.Success("WorldMgr", "Loaded " + Count + " Keep Doors");
        }


        public static Dictionary<int, List<KeepSiegeSpawnPoints>> _KeepSiegeSpawnPoints = new Dictionary<int, List<KeepSiegeSpawnPoints>>();
        public static void LoadKeepSiegeSpawnPoints()
        {
            _KeepSiegeSpawnPoints = new Dictionary<int, List<KeepSiegeSpawnPoints>>();

            Log.Debug("WorldMgr", "Loading KeepSiegeSpawnPoints...");

            IList<KeepSiegeSpawnPoints> points = Database.SelectAllObjects<KeepSiegeSpawnPoints>();

            int Count = 0;
            foreach (KeepSiegeSpawnPoints point in points)
            {
                if (!_KeepSiegeSpawnPoints.ContainsKey(point.KeepId))
                    _KeepSiegeSpawnPoints.Add(point.KeepId, new List<KeepSiegeSpawnPoints>());

                _KeepSiegeSpawnPoints[point.KeepId].Add(point);
                ++Count;
            }

            Log.Success("WorldMgr", "Loaded " + Count + " KeepSiegeSpawnPoints");
        }


        public static Dictionary<int, PlayerKeepSpawn> _PlayerKeepSpawnPoints = new Dictionary<int, PlayerKeepSpawn>();
        public static void LoadPlayerKeepSpawnPoints()
        {
            _PlayerKeepSpawnPoints = new Dictionary<int, PlayerKeepSpawn>();

            Log.Debug("WorldMgr", "Loading PlayerKeepSpawn...");

            var spawns = Database.SelectAllObjects<PlayerKeepSpawn>();

            int count = 0;
            foreach (var spawn in spawns)
            {
                if (!_PlayerKeepSpawnPoints.ContainsKey(spawn.KeepId))
                    _PlayerKeepSpawnPoints.Add(spawn.KeepId, new PlayerKeepSpawn());

                _PlayerKeepSpawnPoints[spawn.KeepId] = spawn;
                ++count;
            }

            Log.Success("WorldMgr", "Loaded " + count + " Player Keep Spawn Points");
        }

        public static List<Keep_Info> GetKeepInfos(uint RegionId)
        {
            if (_KeepInfos.ContainsKey(RegionId))
            {
                return _KeepInfos[RegionId];
            }

            return null;
        }
        #endregion

        #region Guards
        public static Dictionary<int, List<BattleFront_Guard>> _BattleFrontGuards = new Dictionary<int, List<BattleFront_Guard>>();
        public static void LoadBattleFrontGuards()
        {
            _BattleFrontGuards = new Dictionary<int, List<BattleFront_Guard>>();

            Log.Debug("WorldMgr", "Loading BattleFront_Guards...");

            IList<BattleFront_Guard> Guards = Database.SelectAllObjects<BattleFront_Guard>();

            int Count = 0;
            foreach (BattleFront_Guard Guard in Guards)
            {
                if (!_BattleFrontGuards.ContainsKey(Guard.ObjectiveId))
                    _BattleFrontGuards.Add(Guard.ObjectiveId, new List<BattleFront_Guard>());

                _BattleFrontGuards[Guard.ObjectiveId].Add(Guard);
                ++Count;
            }

            Log.Success("WorldMgr", "Loaded " + Count + " Campaign Guards");
        }
        #endregion

        #region Resources
        public static Dictionary<int, List<BattleFrontResourceSpawn>> ResourceSpawns = new Dictionary<int, List<BattleFrontResourceSpawn>>();

        private static void LoadResourceSpawns()
        {
            ResourceSpawns = new Dictionary<int, List<BattleFrontResourceSpawn>>();

            Log.Debug("WorldMgr", "Loading Resource Spawns...");

            IList<BattleFrontResourceSpawn> resSpawns = Database.SelectAllObjects<BattleFrontResourceSpawn>();

            int count = 0;

            foreach (BattleFrontResourceSpawn res in resSpawns)
            {
                if (!ResourceSpawns.ContainsKey(res.Entry))
                    ResourceSpawns.Add(res.Entry, new List<BattleFrontResourceSpawn>());

                ResourceSpawns[res.Entry].Add(res);
                ++count;
            }

            Log.Success("WorldMgr", "Loaded " + count + " resource points.");
        }

        public static List<BattleFrontResourceSpawn> GetResourceSpawns(int objectiveId)
        {
            return ResourceSpawns.ContainsKey(objectiveId) ? ResourceSpawns[objectiveId] : null;
        }
        #endregion

        #region RvRObjects
        public static List<RvRObjectInfo> RvRObjects;

        [LoadingFunction(true)]
        public static void LoadRvRObjects()
        {
            Log.Debug("WorldMgr", "Loading Creature_Protos...");

            RvRObjects = Database.SelectAllObjects<RvRObjectInfo>().ToList();

            Log.Success("LoadRvRObjects", "Loaded " + RvRObjects.Count + " RvR Objects");
        }

        public static RvRObjectInfo GetRvRObjectInfo(int index)
        {
            if (index < RvRObjects.Count)
                return RvRObjects[index];
            return null;
        }

        #endregion

        #region Generic Campaign objects
        /// <summary>
        /// Arrays of warcamp entrances (0/1 for order/destro) indexed by zone id
        /// This must be changed to private later
        /// </summary>
        public static Dictionary<ushort, Point3D[]> _warcampEntrances;

        /// <summary>Arrays of portals to warcamp indexed by zone id and objective ID</summary>
        private static Dictionary<ushort, Dictionary<int, BattleFrontObject>> _portalsToWarcamp;
        /// <summary>Arrays of portals to warcamp indexed by zone id, realm (0/1 for order/destro) and objective ID</summary>
        private static Dictionary<ushort, Dictionary<int, BattleFrontObject>[]> _portalsToObjective;

        /// <summary>
        /// Loads BattleFront_objects table.
        /// </summary>
        /// <remarks>Public for gm commands</remarks>
        public static void LoadBattleFrontObjects()
        {
            _warcampEntrances = new Dictionary<ushort, Point3D[]>();
            _portalsToWarcamp = new Dictionary<ushort, Dictionary<int, BattleFrontObject>>();
            _portalsToObjective = new Dictionary<ushort, Dictionary<int, BattleFrontObject>[]>();

            Log.Debug("WorldMgr", "Loading Campaign objects...");

            IList<BattleFrontObject> objects = Database.SelectAllObjects<BattleFrontObject>();

            int count = 0;

            foreach (BattleFrontObject res in objects)
            {
                switch (res.Type)
                {
                    case (ushort)BattleFrontObjectType.WARCAMP_ENTRANCE:
                        // Entrances to warcamp necessary to compute objective rewards and spawn farm check
                        if (!_warcampEntrances.ContainsKey(res.ZoneId))
                            _warcampEntrances.Add(res.ZoneId, new Point3D[2]);

                        _warcampEntrances[res.ZoneId][res.Realm - 1] = new Point3D(res.X, res.Y, res.Z);
                        break;

                    case (ushort)BattleFrontObjectType.WARCAMP_PORTAL:
                        // Objective -> warcamp portals
                        if (!_portalsToWarcamp.ContainsKey(res.ZoneId))
                            _portalsToWarcamp.Add(res.ZoneId, new Dictionary<int, BattleFrontObject>());

                        _portalsToWarcamp[res.ZoneId].Add(res.ObjectiveID, res);
                        break;

                    case (ushort)BattleFrontObjectType.OBJECTIVE_PORTAL:
                        // Warcamp -> objective portals
                        if (!_portalsToObjective.ContainsKey(res.ZoneId))
                        {
                            _portalsToObjective.Add(res.ZoneId, new Dictionary<int, BattleFrontObject>[] {
                                    new Dictionary<int, BattleFrontObject>(),
                                    new Dictionary<int, BattleFrontObject>(),
                                });
                        }

                        _portalsToObjective[res.ZoneId][res.Realm - 1].Add(res.ObjectiveID, res);
                        break;

                    default:
                        Log.Error("WorldMgr", "Unkown type for object : " + res.Type.ToString());
                        break;
                }
                ++count;
            }

            Log.Success("WorldMgr", "Loaded " + count + " Campaign objects.");
        }

        /// <summary>
        /// Gets the warcamp entrance in a zone for given realm.
        /// </summary>
        /// <param name="zoneId">Zone identifer</param>
        /// <param name="realm">Order/destro</param>
        /// <returns>Warcamp entrance coordinate or null if does not exists or is not parameterized
        /// (given zone's inner coordinates)</returns>
        public static Point3D GetWarcampEntrance(ushort zoneId, Realms realm)
        {
            if (_warcampEntrances.ContainsKey(zoneId))
                return _warcampEntrances[zoneId][(int)realm - 1];
            return null;
        }

        /// <summary>
        /// Gets the portal to warcamp for the given battlefield objective.
        /// </summary>
        /// <param name="zoneId">Zone identifer</param>
        /// <param name="objectiveId">Objective identifier</param>
        /// <returns>Portal, null if not found</returns>
        public static BattleFrontObject GetPortalToWarcamp(ushort zoneId, int objectiveId)
        {
            if (_portalsToWarcamp.ContainsKey(zoneId))
            {
                Dictionary<int, BattleFrontObject> zoneObjects = _portalsToWarcamp[zoneId];
                if (zoneObjects.ContainsKey(objectiveId))
                    return zoneObjects[objectiveId];
            }
            return null;
        }

        /// <summary>
        /// Gets the portal to the given battlefield objective for given realm.
        /// </summary>
        /// <param name="zoneId">Zone identifer</param>
        /// <param name="objectiveId">Objective identifier</param>
        /// <param name="realm">From order/destro warcamp</param>
        /// <returns>List of portals indexed by objective ID, null if not found</returns>
        public static BattleFrontObject GetPortalToObjective(ushort zoneId, int objectiveId, Realms realm)
        {
            if (_portalsToObjective.ContainsKey(zoneId))
            {
                Dictionary<int, BattleFrontObject>[] zoneObjects = _portalsToObjective[zoneId];
                if (zoneObjects[(int)realm - 1].ContainsKey(objectiveId))
                    return _portalsToObjective[zoneId][(int)realm - 1][objectiveId];
            }
            return null;
        }
        #endregion

        #region Status - Updated at runtime
        public static Dictionary<int, BattleFrontStatus> BattleFrontStatus;
        public static BattleFrontStatus GetStatusFor(int regionId)
        {
            lock (BattleFrontStatus)
            {
                if (BattleFrontStatus.ContainsKey(regionId))
                    return BattleFrontStatus[regionId];

                BattleFrontStatus.Add(regionId, new BattleFrontStatus(regionId));
            }

            Database.AddObject(BattleFrontStatus[regionId]);

            return BattleFrontStatus[regionId];
        }
        #endregion

       
        public static List<Keep_Info> GetZoneKeeps(int regionId, int zoneId)
        {
            return (from keyValuePair in _KeepInfos.Where(x => x.Key == regionId)
                    from keep in keyValuePair.Value
                    where keep.ZoneId == zoneId
                    select keep).ToList();
        }


        public static List<BattleFront_Objective> GetZoneBattlefrontObjectives(int regionId, int zoneId)
        {
            return (from keyValuePair in _BattleFrontObjectives.Where(x => x.Key == regionId)
                    from bo 
                    in keyValuePair.Value
                    where bo.ZoneId == zoneId
                    select bo).ToList();
        }

        public static void SetCampaignBuff(int buffId, int battleFrontId)
        {
        }
    }
}
