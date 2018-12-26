using Common;
using Common.Database.World.BattleFront;
using FrameWork;
using GameData;
using System.Collections.Generic;
using System.Linq;

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

        /* no idea what it is for
        public static void CheckBattleFrontPositions()
        {
            IList<BattleFront_Objective> Objectives = Database.SelectAllObjects<BattleFront_Objective>();

            foreach (BattleFront_Objective Obj in Objectives)
            {
                // Buggy Pin coords - calculate!
                if (Obj.X <= 65535)
                {
                    Zone_Info info = ZoneService.GetZone_Info(Obj.ZoneId);

                    if (info != null)
                    {
                        int x = Obj.X > 32768 ? Obj.X - 32768 : Obj.X;
                        int y = Obj.Y > 32768 ? Obj.Y - 32768 : Obj.Y;
                        Point3D WorldPosition = new Point3D(0, 0, 0);


                        WorldPosition.X = (int)ZoneMgr.CalcOffset(info, (ushort)Obj.X, true) + (x & 0x00000FFF);
                        WorldPosition.Y = (int)ZoneMgr.CalcOffset(info, (ushort)Obj.Y, false) + (y & 0x00000FFF);
                        WorldPosition.Z = Obj.Z;

                        Log.Notice(Obj.Name, "Region ID: " + Obj.RegionId + " Zone ID: " + Obj.ZoneId);
                        Log.Notice("Pin Position", "X: " + Obj.X + " Y: " + Obj.Y + " Z: " + Obj.Z);
                        Log.Notice("World Position", "X: " + WorldPosition.X + " Y: " + WorldPosition.Y + " Z: " + WorldPosition.Z);
                        Log.Notice("=", "==================================================================================================");
                    }

                }
            }
        }
        */
        public static List<Keep_Info> GetZoneKeeps(int regionId, int zoneId)
        {
            var keepList = new List<Keep_Info>();
            foreach (var keyValuePair in _KeepInfos.Where(x=>x.Key == regionId))
            {
                foreach (var keep in keyValuePair.Value)
                {
                    if (keep.ZoneId == zoneId)
                    {
                        keepList.Add(keep);
                    }
                }
            }
            return keepList;
        }


        public static List<BattleFront_Objective> GetZoneBattlefrontObjectives(int regionId, int zoneId)
        {
            var boList = new List<BattleFront_Objective>();
            foreach (var keyValuePair in _BattleFrontObjectives.Where(x => x.Key == regionId))
            {
                foreach (var bo in keyValuePair.Value)
                {
                    if (bo.ZoneId == zoneId)
                    {
                        boList.Add(bo);
                    }
                }
            }
            return boList;
        }
    }
}
