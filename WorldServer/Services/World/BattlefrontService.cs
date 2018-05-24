using Common;
using Common.Database.World.Battlefront;
using FrameWork;
using GameData;
using System.Collections.Generic;
using System.Linq;

namespace WorldServer.Services.World
{
    [Service]
    public class BattlefrontService : ServiceBase
    {
        [LoadingFunction(true)]
        public static void LoadBattlefront()
        {
            BattlefrontStatus = Database.SelectAllObjects<BattlefrontStatus>().ToDictionary(g => g.RegionId);

            LoadBattlefrontGuards();
            LoadResourceSpawns();

            LoadBattlefrontObjectives();
            LoadBattlefrontObjects();

            LoadKeepInfos();
        }

        #region Objectives
        public static Dictionary<uint, List<Battlefront_Objective>> _BattlefrontObjectives = new Dictionary<uint, List<Battlefront_Objective>>();

        private static void LoadBattlefrontObjectives()
        {
            _BattlefrontObjectives = new Dictionary<uint, List<Battlefront_Objective>>();

            Log.Debug("WorldMgr", "Loading Battlefront_Objectives...");

            IList<Battlefront_Objective> Objectives = Database.SelectAllObjects<Battlefront_Objective>();

            int Count = 0;
            foreach (Battlefront_Objective Obj in Objectives)
            {
                if (!_BattlefrontObjectives.ContainsKey(Obj.RegionId))
                    _BattlefrontObjectives.Add(Obj.RegionId, new List<Battlefront_Objective>());

                _BattlefrontObjectives[Obj.RegionId].Add(Obj);

                if (_BattlefrontGuards.ContainsKey(Obj.Entry))
                    Obj.Guards = _BattlefrontGuards[Obj.Entry];

                ++Count;
            }

            Log.Success("WorldMgr", "Loaded " + Count + " Battlefront Objectives");
        }

        public static List<Battlefront_Objective> GetBattlefrontObjectives(uint RegionId)
        {
            if (_BattlefrontObjectives.ContainsKey(RegionId))
            {
                return _BattlefrontObjectives[RegionId];
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

            Log.Debug("WorldMgr", "Loading Battlefront_Objectives...");

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
        public static Dictionary<int, List<Battlefront_Guard>> _BattlefrontGuards = new Dictionary<int, List<Battlefront_Guard>>();
        public static void LoadBattlefrontGuards()
        {
            _BattlefrontGuards = new Dictionary<int, List<Battlefront_Guard>>();

            Log.Debug("WorldMgr", "Loading Battlefront_Guards...");

            IList<Battlefront_Guard> Guards = Database.SelectAllObjects<Battlefront_Guard>();

            int Count = 0;
            foreach (Battlefront_Guard Guard in Guards)
            {
                if (!_BattlefrontGuards.ContainsKey(Guard.ObjectiveId))
                    _BattlefrontGuards.Add(Guard.ObjectiveId, new List<Battlefront_Guard>());

                _BattlefrontGuards[Guard.ObjectiveId].Add(Guard);
                ++Count;
            }

            Log.Success("WorldMgr", "Loaded " + Count + " Battlefront Guards");
        }
        #endregion

        #region Resources
        public static Dictionary<int, List<BattlefrontResourceSpawn>> ResourceSpawns = new Dictionary<int, List<BattlefrontResourceSpawn>>();

        private static void LoadResourceSpawns()
        {
            ResourceSpawns = new Dictionary<int, List<BattlefrontResourceSpawn>>();

            Log.Debug("WorldMgr", "Loading Resource Spawns...");

            IList<BattlefrontResourceSpawn> resSpawns = Database.SelectAllObjects<BattlefrontResourceSpawn>();

            int count = 0;

            foreach (BattlefrontResourceSpawn res in resSpawns)
            {
                if (!ResourceSpawns.ContainsKey(res.Entry))
                    ResourceSpawns.Add(res.Entry, new List<BattlefrontResourceSpawn>());

                ResourceSpawns[res.Entry].Add(res);
                ++count;
            }

            Log.Success("WorldMgr", "Loaded " + count + " resource points.");
        }

        public static List<BattlefrontResourceSpawn> GetResourceSpawns(int objectiveId)
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

        #region Generic battlefront objects
        /// <summary>
        /// Arrays of warcamp entrances (0/1 for order/destro) indexed by zone id
        /// This must be changed to private later
        /// </summary>
        public static Dictionary<ushort, Point3D[]> _warcampEntrances;

        /// <summary>Arrays of portals to warcamp indexed by zone id and objective ID</summary>
        private static Dictionary<ushort, Dictionary<int, BattlefrontObject>> _portalsToWarcamp;
        /// <summary>Arrays of portals to warcamp indexed by zone id, realm (0/1 for order/destro) and objective ID</summary>
        private static Dictionary<ushort, Dictionary<int, BattlefrontObject>[]> _portalsToObjective;

        /// <summary>
        /// Loads battlefront_objects table.
        /// </summary>
        /// <remarks>Public for gm commands</remarks>
        public static void LoadBattlefrontObjects()
        {
            _warcampEntrances = new Dictionary<ushort, Point3D[]>();
            _portalsToWarcamp = new Dictionary<ushort, Dictionary<int, BattlefrontObject>>();
            _portalsToObjective = new Dictionary<ushort, Dictionary<int, BattlefrontObject>[]>();

            Log.Debug("WorldMgr", "Loading battlefront objects...");

            IList<BattlefrontObject> objects = Database.SelectAllObjects<BattlefrontObject>();

            int count = 0;

            foreach (BattlefrontObject res in objects)
            {
                switch (res.Type)
                {
                    case (ushort)BattlefrontObjectType.WARCAMP_ENTRANCE:
                        // Entrances to warcamp necessary to compute objective rewards and spawn farm check
                        if (!_warcampEntrances.ContainsKey(res.ZoneId))
                            _warcampEntrances.Add(res.ZoneId, new Point3D[2]);

                        _warcampEntrances[res.ZoneId][res.Realm - 1] = new Point3D(res.X, res.Y, res.Z);
                        break;

                    case (ushort)BattlefrontObjectType.WARCAMP_PORTAL:
                        // Objective -> warcamp portals
                        if (!_portalsToWarcamp.ContainsKey(res.ZoneId))
                            _portalsToWarcamp.Add(res.ZoneId, new Dictionary<int, BattlefrontObject>());

                        _portalsToWarcamp[res.ZoneId].Add(res.ObjectiveID, res);
                        break;

                    case (ushort)BattlefrontObjectType.OBJECTIVE_PORTAL:
                        // Warcamp -> objective portals
                        if (!_portalsToObjective.ContainsKey(res.ZoneId))
                        {
                            _portalsToObjective.Add(res.ZoneId, new Dictionary<int, BattlefrontObject>[] {
                                    new Dictionary<int, BattlefrontObject>(),
                                    new Dictionary<int, BattlefrontObject>(),
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

            Log.Success("WorldMgr", "Loaded " + count + " battlefront objects.");
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
        public static BattlefrontObject GetPortalToWarcamp(ushort zoneId, int objectiveId)
        {
            if (_portalsToWarcamp.ContainsKey(zoneId))
            {
                Dictionary<int, BattlefrontObject> zoneObjects = _portalsToWarcamp[zoneId];
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
        public static BattlefrontObject GetPortalToObjective(ushort zoneId, int objectiveId, Realms realm)
        {
            if (_portalsToObjective.ContainsKey(zoneId))
            {
                Dictionary<int, BattlefrontObject>[] zoneObjects = _portalsToObjective[zoneId];
                if (zoneObjects[(int)realm - 1].ContainsKey(objectiveId))
                    return _portalsToObjective[zoneId][(int)realm - 1][objectiveId];
            }
            return null;
        }
        #endregion

        #region Status - Updated at runtime
        public static Dictionary<int, BattlefrontStatus> BattlefrontStatus;
        public static BattlefrontStatus GetStatusFor(int regionId)
        {
            lock (BattlefrontStatus)
            {
                if (BattlefrontStatus.ContainsKey(regionId))
                    return BattlefrontStatus[regionId];

                BattlefrontStatus.Add(regionId, new BattlefrontStatus(regionId));
            }

            Database.AddObject(BattlefrontStatus[regionId]);

            return BattlefrontStatus[regionId];
        }
        #endregion

        /* no idea what it is for
        public static void CheckBattlefrontPositions()
        {
            IList<Battlefront_Objective> Objectives = Database.SelectAllObjects<Battlefront_Objective>();

            foreach (Battlefront_Objective Obj in Objectives)
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
    }
}
