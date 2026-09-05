//#define NO_CREATURE

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Common;
using SystemData;
using Common.Database.World.Maps;
using FrameWork;
using GameData;
using NLog;
using WorldServer.Managers;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Battlefronts.Bounty;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using WorldServer.World.Objects.PublicQuests;
using WorldServer.World.Positions;
using WorldServer.World.Scenarios;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Map
{
    public class RegionMgr
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public static int REGION_UPDATE_INTERVAL = 50; // 50 ms between each update
        public static ushort MaxCellID = 800;
        public static ushort MaxCells = 16;
        public static int MaxVisibilityRange = 400; // It was 400 on Age of Reckoning

        /// <summary>Races associated with the pairing, may be null</summary>
        private readonly Races[] _races;

        private long _lastRegionUpdate = TCPManager.GetTimeStampMS();
        private bool _running = true;
        public BountyManager BountyManager;

        public Campaign Campaign;
        public ContributionManager ContributionManager;
        public ImpactMatrixManager ImpactMatrix;
        public List<Creature> RegionCreatures;
        public ushort RegionId;
        public string RegionName;
        public RewardManager RewardManager;
        public Scenario Scenario;
        public List<Zone_Info> ZonesInfo;

        public RegionMgr(ushort regionId, List<Zone_Info> zones, string name, IBattlefrontCommunications battlefrontCommunications)
        {
            BattlefrontCommunications = battlefrontCommunications;
            RegionId = regionId;
            ZonesInfo = zones?.Where(zone => zone != null).ToList() ?? new List<Zone_Info>();
            RegionName = name;

            if (ZonesInfo.Count == 0)
                throw new InvalidOperationException($"Region {RegionId} ({RegionName}) could not be created because no zone metadata was found.");

            LoadSpawns();

            BountyManager = new BountyManager();

            try
            {
                switch (ZonesInfo[0].Pairing)
                {
                    case (byte) Pairing.PAIRING_GREENSKIN_DWARVES:
                        _races = new[] {Races.RACES_DWARF, Races.RACES_GOBLIN};
                        break;
                    case (byte) Pairing.PAIRING_EMPIRE_CHAOS:
                        _races = new[] {Races.RACES_EMPIRE, Races.RACES_CHAOS};
                        break;
                    case (byte) Pairing.PAIRING_ELVES_DARKELVES:
                        _races = new[] {Races.RACES_HIGH_ELF, Races.RACES_DARK_ELF};
                        break;
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Zone - Pairing {e.Message} {e.StackTrace}");
                throw;
            }

            RegionCreatures = GetObjects<Creature>().ToList();
        }

        private Thread _updater;
        public void StartUpdateThread(bool isBackground = false)
        {
            if (_updater != null)
                return;

            _updater = new Thread(Update) { IsBackground = isBackground };
            _updater.Start();
        }

        public IBattlefrontCommunications BattlefrontCommunications { get; set; }


        public void Stop()
        {
            try
            {
                Log.Debug("RegionMgr", "[" + RegionId + "] Stop");
                _running = false;

                foreach (var zone in GetZones())
                    zone.Stop();
            }
            catch (Exception e)
            {
                Log.Error("Region " + RegionId + " Stop", e.ToString());
            }
        }

        /// <summary>
        ///     Returns the zone entity of given identifier.
        /// </summary>
        /// <param name="zoneId">Identifier of the searched zone</param>
        /// <returns>Zone or null if does not exists</returns>
        public Zone_Info GetZone_Info(ushort zoneId)
        {
            foreach (var zone in ZonesInfo)
                if (zone != null && zone.ZoneId == zoneId)
                    return zone;
            return null;
        }

        public Zone_Info GetZone(ushort offX, ushort offY)
        {
            return ZonesInfo.Find(zone =>
                zone != null && zone.OffX <= offX && zone.OffX + MaxCells > offX && zone.OffY <= offY &&
                zone.OffY + MaxCells > offY);
        }

        public int GetTier()
        {
            if (Scenario != null)
                return Scenario.Tier;
            if (ZonesInfo.Count > 0)
                return ZonesInfo[0].Tier;
            return 4;
        }

        #region ZoneMgr

        private readonly List<ZoneMgr> _zonesMgr = new List<ZoneMgr>();
        private readonly object _zonesLock = new object();

        /// <summary>
        ///     Snapshot of the zones currently live in this region, safe to enumerate from any thread.
        ///     Zones are created on demand by <see cref="GetZoneMgr"/>, which runs on player zone-change
        ///     and object-add paths while other threads walk this list.
        /// </summary>
        public List<ZoneMgr> GetZones()
        {
            lock (_zonesLock)
                return new List<ZoneMgr>(_zonesMgr);
        }

        /// <summary>Caller must hold <see cref="_zonesLock"/>.</summary>
        private ZoneMgr FindZoneMgrUnsafe(ushort zoneId)
        {
            foreach (var z in _zonesMgr)
                if (z != null && z.Info.ZoneId == zoneId)
                    return z;

            return null;
        }

        /// <summary>
        ///     Gets the zone of given id, lazy loading it if necessary.
        /// </summary>
        /// <param name="zoneId">Id of the zone to get</param>
        /// <returns>Zone or null if zone info does not exists</returns>
        public ZoneMgr GetZoneMgr(ushort zoneId)
        {
            var info = GetZone_Info(zoneId);
            if (info == null)
                return null;

            lock (_zonesLock)
            {
                ZoneMgr existing = FindZoneMgrUnsafe(zoneId);
                if (existing != null)
                    return existing;
            }

            // Built outside the lock: the ZoneMgr constructor faults in the zone's collision data, which can
            // take hundreds of milliseconds and must not stall every other thread touching this region.
            var created = new ZoneMgr(this, info);

            lock (_zonesLock)
            {
                // Another thread may have created the same zone while we were building ours. Theirs wins, so
                // a zone is never represented by two managers and objects cannot be split across them.
                ZoneMgr existing = FindZoneMgrUnsafe(zoneId);
                if (existing != null)
                    return existing;

                _zonesMgr.Add(created);
                return created;
            }
        }

        public ushort CheckZone(Object obj)
        {
            var info = GetZone(obj.XOffset, obj.YOffset);

            // Object.Zone is documented as nullable and is genuinely null while an object is between zones.
            // Dereferencing it here threw on the offset-boundary crossing that a zone transition performs,
            // and because SetOffset runs on the movement path rather than the region thread, that exception
            // was not contained by the region tick's handler. A null zone simply means "not placed yet",
            // which is already the condition for adding the object to the resolved zone.
            if (info != null && info != obj.Zone?.Info) AddObject(obj, info.ZoneId);

            var curCell = obj._Cell;
            var newCell = GetCell(obj.XOffset, obj.YOffset);

            if (newCell == null || newCell == curCell)
                return info?.ZoneId ?? 0;

            curCell?.RemoveObject(obj);
            newCell.AddObject(obj); // On l'ajoute dans le nouveau cell

            return info?.ZoneId ?? 0;
        }

        public void Update()
        {
            while (_running)
            {
                var stampMs = TCPManager.GetTimeStampMS();

                //if (stampMs - _lastRegionUpdate > 50000)
                //    Log.Error("RegionMgr", "[" + RegionId + "] - Region inter-update period too long - took " + (stampMs - _lastRegionUpdate) + " ms.");

                //else if (stampMs - _lastRegionUpdate > 25000)
                //    Log.Notice("RegionMgr", "[" + RegionId + "] - Region inter-update period too long - took " + (stampMs - _lastRegionUpdate) + " ms.");

                try
                {
                    WorldMgr.UpdateScripts(stampMs);

                    AddNewObjects();

                    RemoveOldObjects();

                    UpdateActors(stampMs);

                    Campaign?.Update(stampMs);

                    Campaign?.BattleFrontManager?.ImpactMatrixManagerInstance?.Update(stampMs);

                    ScenarioMgr.ImpactMatrixManagerInstance?.Update(stampMs);
                }

                catch (Exception e)
                {
                    Log.Error("Error", e.ToString());
                }

                var elapsed = TCPManager.GetTimeStampMS() - stampMs;

                _lastRegionUpdate = TCPManager.GetTimeStampMS();

                // If we updated the region in less time than the REGION_UPDATE_INTERVAL sleep until the interval has expired.
                if (elapsed < REGION_UPDATE_INTERVAL) Thread.Sleep((int) (REGION_UPDATE_INTERVAL - elapsed));
            }

            DisposeActors();
        }


        /// <summary>
        ///     Membership snapshot, safe to enumerate from any thread. Player state still belongs
        ///     to its region thread. A new snapshot is published only when membership changes.
        /// </summary>
        public ReadOnlyCollection<Player> Players => _playerSnapshot;

        private readonly List<Player> _players = new List<Player>();
        private volatile ReadOnlyCollection<Player> _playerSnapshot = Array.AsReadOnly(Array.Empty<Player>());

        public int OrderPlayers { get; private set; }
        public int DestPlayers { get; private set; }

        // Membership writes run only on the region thread; readers never see the mutable list.
        private void AddPlayer(Player player)
        {
            if (_players.Contains(player))
                return;

            _players.Add(player);
            if (player.Realm == Realms.REALMS_REALM_ORDER) OrderPlayers++;
            if (player.Realm == Realms.REALMS_REALM_DESTRUCTION) DestPlayers++;
            _playerSnapshot = Array.AsReadOnly(_players.ToArray());
        }

        private void RemovePlayer(Player player)
        {
            if (!_players.Remove(player))
                return;

            if (player.Realm == Realms.REALMS_REALM_ORDER) OrderPlayers--;
            if (player.Realm == Realms.REALMS_REALM_DESTRUCTION) DestPlayers--;
            _playerSnapshot = Array.AsReadOnly(_players.ToArray());
        }

        private void AddNewObjects()
        {
            try
            {
                lock (_objectsToAdd)
                {
                    foreach (var obj in _objectsToAdd)
                    {
                        var plr = obj.Obj as Player;

                        if (obj.Obj.Region != this)
                        {
                            if (obj.Obj.Region != null)
                                obj.Obj.Region.RemoveObject(obj.Obj);

                            GenerateOid(obj.Obj);
                            _activeObjects.Add(obj.Obj);

                            if (plr != null)
                            {
                                plr.InRegionChange = true;
                                AddPlayer(plr);
                            }
                        }

                        else
                        {
                            obj.Obj.Zone.RemoveObject(obj.Obj);
                        }

                        var mgr = GetZoneMgr(obj.ZoneId);
                        mgr.AddObject(obj.Obj);

                        if (obj.MustUpdateRange)
                            UpdateRange(obj.Obj);
                    }

                    _objectsToAdd.Clear();
                }
            }
            catch (Exception e)
            {
                Log.Error("AddNewObjects", e.ToString());
            }
        }

        private void RemoveOldObjects()
        {
            try
            {
                lock (_objectsToRemove)
                {
                    foreach (var removeInfo in _objectsToRemove)
                    {
                        removeInfo.Obj.InRegionChange = false;
                        removeInfo.Obj.ClearRange();

                        if (removeInfo.Cell != null)
                            removeInfo.Cell.RemoveObject(removeInfo.Obj);

                        if (removeInfo.Zone != null)
                            removeInfo.Zone.RemoveObject(removeInfo.Obj);

                        if (removeInfo.Oid != 0)
                        {
                            _activeObjects.Remove(removeInfo.Obj);
                            if (Objects[removeInfo.Oid] != null && Objects[removeInfo.Oid] == removeInfo.Obj)
                            {
                                Objects[removeInfo.Oid] = null;

                                /*
                                    Player Oid could previously be zeroed after another region had set it.
                                    If this were to happen, NPC mounts in the zone would display
                                    the player's name, title and guild, and the player's mount would not display.
                                */
                                if (removeInfo.Obj.Oid == removeInfo.Oid && removeInfo.Zone == removeInfo.Obj.Zone)
                                    removeInfo.Obj.ZeroOid(removeInfo.Oid);
                            }
                        }

                        if (removeInfo.Obj is Player player)
                            RemovePlayer(player);
                    }

                    _objectsToRemove.Clear();
                }
            }
            catch (Exception e)
            {
                Log.Error("RemoveOldObjects", e.ToString());
            }
        }

        private void UpdateActors(long start)
        {
            foreach (var obj in _activeObjects)
            {
                if (obj == null || obj.Region != this)
                    continue;

                try
                {
                    if (!obj.Loaded)
                    {
                        obj.Load();
                    }
                    else
                    {
                        if (obj.IsDisposed)
                            RemoveObject(obj);
                        else
                            obj.Update(start);
                    }
                }
                catch (Exception e)
                {
                    Log.Error("EXCEPTION: " + obj.Name + " in Region " + RegionId, e.ToString());

                    if (obj is Player)
                    {
                        ((Player) obj).SendClientMessage(e.GetType().Name + " was thrown from " + e.TargetSite?.Name +
                                                         ".");
                    }
                    else if (obj is IBattlefront)
                    {
                        try
                        {
                            foreach (var player in Players)
                                player.SendClientMessage(e.GetType().Name + " from " + e.TargetSite?.Name +
                                                         " was thrown from a Battlefield Objective in this region.");
                        }
                        catch (Exception)
                        {
                            Log.Error("RegionMgr", "Exception throw within Player exception notification");
                        }
                    }

                    else
                    {
                        obj.Say(e.GetType().Name + " was thrown from " + e.TargetSite?.Name +
                                ". This object will be destroyed.");
                        Log.Error("Unhandled Exception", obj.Name + " has been removed from the region.");
                        obj.Dispose();
                        RemoveObject(obj);
                    }
                }
            }
        }

        private void DisposeActors()
        {
            RemoveOldObjects();

            foreach (var obj in _activeObjects)
            {
                if (obj == null || obj.Region != this)
                    continue;

                try
                {
                    if (!obj.IsDisposed)
                        obj.Dispose();
                }
                catch (Exception e)
                {
                    Log.Error("Zone Disposal", e.ToString());
                }
            }
        }

        /// <summary>
        ///     Checks whether the region matches the given race.
        /// </summary>
        /// <param name="race">Race to check</param>
        /// <returns>True if matchs, false otherwise</returns>
        public bool Matches(Races race)
        {
            return _races != null && (_races[0] == race || _races[1] == race);
        }

        #region Diagnostic

        public void CountObjects(Player plr)
        {
            var objectCounts = new Dictionary<string, int>();

            foreach (var obj in _activeObjects)
            {
                if (obj == null)
                    continue;
                var type = obj.GetType().ToString();

                if (objectCounts.ContainsKey(type))
                    objectCounts[type]++;
                else objectCounts.Add(type, 1);
            }

            plr.SendClientMessage("Object count for current region:");
            foreach (var entry in objectCounts)
                plr.SendClientMessage(entry.Key + " " + entry.Value);
        }

        #endregion

        #endregion

        #region Ranged

        public delegate void RangedObjectDelegate(Object obj);

        public void GetRangedObject(Object obj, int range, RangedObjectDelegate rangeFunction)
        {
            if (!obj.IsInWorld())
                return;

            GetCells(obj.XOffset, obj.YOffset, range, cell =>
            {
                for (var i = 0; i < cell.Objects.Count; ++i)
                {
                    Object distObject;
                    if ((distObject = cell.Objects[i]) == null)
                    {
                        cell.Objects.RemoveAt(i);
                        i--;
                    }
                    else if (obj.Get2DDistanceToObject(distObject) <= MaxVisibilityRange)
                    {
                        rangeFunction(distObject);
                    }
                }
            });
        }

        public static bool IsRange(int fixe, int move, int range)
        {
            var max = fixe + range;
            var min = fixe - range;

            if (move > max || move < min)
                return false;

            return true;
        }

        public void DispatchPacket(PacketOut packet, Point3D point, int radius, Func<Player, bool> predicate = null)
        {
            foreach (var player in WorldQuery(point, radius, predicate)) player.DispatchPacket(packet, true);
        }

        public List<T> WorldQuery<T>(Point3D point, int radius, Func<T, bool> predicate = null) where T : Object
        {
            var list = new List<T>();

            var aradius = radius * Point2D.UNITS_TO_FEET;
            var count = 0;
            foreach (var zone in ZonesInfo.ToList())
            {
                var mapX = zone.OffX << 12;
                var mapY = zone.OffY << 12;

                if (point.X - aradius >= mapX && point.X + aradius <= mapX + 0xFFFF &&
                    point.Y - aradius >= mapY && point.Y + aradius <= mapY + 0xFFFF) //is the point on this zone?
                {
                    var offX = (ushort) Math.Truncate((decimal) ((point.X - mapX) / 4096 + zone.OffX));
                    var offY = (ushort) Math.Truncate((decimal) ((point.Y - mapY) / 4096 + zone.OffY));

                    for (var x = offX - 1; x < offX + 1; x++) //scan all cells within radius
                    for (var y = offY - 1; y < offY + 1; y++)
                        if (x >= 0 && x <= MaxCellID && y >= 0 && y <= MaxCellID)
                            if (Cells[x, y] != null)
                                foreach (var obj in Cells[x, y].Objects.ToList())
                                {
                                    count++;
                                    if (obj is T && obj.PointWithinRadiusFeet(point, radius) && !list.Contains(obj))
                                        list.Add((T) obj);
                                }
                }
            }

            if (predicate != null)
                return list.Where(predicate).ToList();
            return list;
        }


        public bool UpdateRange(Object curObj, bool forceUpdate = false)
        {
            if (!curObj.IsActive || curObj.IsDisposed)
                return false;

            if (curObj.X == 0 && curObj.Y == 0)
                return false;

            float distance = curObj.Get2DDistanceToWorldPoint(curObj.LastRangeCheck);
            if (distance > 100 || forceUpdate)
            {
                curObj.LastRangeCheck.X = curObj.WorldPosition.X;
                curObj.LastRangeCheck.Y = curObj.WorldPosition.Y;
            }
            else
            {
                return false;
            }

            curObj.OnRangeUpdate();

            GetRangedObject(curObj, 1, distObj =>
            {
                if (distObj == null)
                    return;

                if (IsVisibleBForA(curObj, distObj) && !curObj.HasInRange(distObj))
                {
                    curObj.AddInRange(distObj);
                    distObj.AddInRange(curObj);

                    if (curObj.IsPlayer())
                        distObj.SendMeTo(curObj.GetPlayer());

                    if (distObj.IsPlayer())
                        curObj.SendMeTo(distObj.GetPlayer());
                }
            });

            Object dist;

            for (var i = 0; i < curObj.ObjectsInRange.Count; ++i)
            {
                if ((dist = curObj.ObjectsInRange[i]) == null)
                    continue;

                if (dist.Get2DDistanceToObject(curObj) > MaxVisibilityRange || !IsVisibleBForA(curObj, dist))
                {
                    curObj.RemoveInRange(dist);
                    dist.RemoveInRange(curObj);
                    i--;
                }
            }

            return true;
        }

        public bool IsVisibleBForA(Object a, Object b)
        {
            if (a == null || b == null || a.IsDisposed || b.IsDisposed)
                return false;

            if (a == b || !a.IsActive || !b.IsActive || !b.IsVisible)
                return false;

            if (b.IsPlayer() && (b.GetPlayer().Client == null || !b.GetPlayer().Client.IsPlaying()))
                return false;

            return true;
        }

        #endregion

        #region Oid

        public static ushort MaxObjects = 65000;
        public static ushort MaxOid = 2;
        public Object[] Objects = new Object[MaxObjects];
        private readonly HashSet<Object> _activeObjects = new HashSet<Object>();
        public Dictionary<uint, PublicQuest> PublicQuests = new Dictionary<uint, PublicQuest>();
        private readonly List<ObjectAdd> _objectsToAdd = new List<ObjectAdd>();
        private readonly List<ObjectRemove> _objectsToRemove = new List<ObjectRemove>();

        public void GenerateOid(Object obj)
        {
            var oid = GetOid();
            Objects[oid] = obj;

            obj.SetOid(oid);
            obj.Loaded = false;
        }

        public ushort GetOid()
        {
            for (int i = MaxOid; i < MaxObjects; ++i)
            {
                if (MaxOid >= MaxObjects - 1)
                {
                    MaxOid = 2;
                    i = 2;
                }

                if (Objects[i] == null)
                {
                    MaxOid = (ushort) i;
                    return (ushort) i;
                }
            }

            return MaxOid;
        }

        public struct ObjectAdd
        {
            public Object Obj;
            public ushort ZoneId;
            public bool MustUpdateRange;
        }

        public bool AddObject(Object obj, ushort zoneId, bool mustUpdateRange = false)
        {
            var info = GetZone_Info(zoneId);
            if (info == null)
            {
                Log.Error("RegionMgr",
                    "AddObject: Unable to add object " + obj.Name + " to invalid Zone with ID : " + zoneId);
                return false;
            }

            var add = new ObjectAdd
            {
                Obj = obj,
                ZoneId = zoneId,
                MustUpdateRange = mustUpdateRange
            };

            //obj.MovementZone = GetZoneMgr(zoneId);

            lock (_objectsToAdd)
            {
                _objectsToAdd.Add(add);
            }

            return true;
        }

        public struct ObjectRemove
        {
            public Object Obj;
            public ushort Oid;
            public ZoneMgr Zone;
            public CellMgr Cell;
        }

        public bool RemoveObject(Object obj)
        {
            // nothing to remove here
            if (obj == null)
                return true;

            //if (Obj.IsPlayer())
            //    Log.Success("RemoveObject", Obj.Name);

            obj.EvtInterface.Notify(EventName.OnRemoveFromWorld, obj, null);

            var rem = new ObjectRemove
            {
                Obj = obj,
                Oid = obj.Oid,
                Zone = obj.Zone,
                Cell = obj._Cell
            };

            lock (_objectsToRemove)
            {
                _objectsToRemove.Add(rem);
            }

            return false;
        }

        public Object GetObject(ushort oid)
        {
            if (oid < 2 || oid >= Objects.Length)
                return null;

            var obj = Objects[oid];

            if (obj == null || obj.IsDisposed)
                return null;

            return obj;
        }

        public Player GetPlayer(ushort oid)
        {
            return GetObject(oid) as Player;
        }

        public ushort GetObjects()
        {
            return (ushort) _activeObjects.Count;
        }

        public List<T> GetObjects<T>() where T : Object
        {
            return _activeObjects.OfType<T>().ToList();
        }

        #endregion

        #region Cells

        public CellMgr[,] Cells = new CellMgr[MaxCellID, MaxCellID];

        public delegate void GetCellDelegate(CellMgr cell);

        public CellMgr GetCell(ushort x, ushort y)
        {
            if (x >= MaxCellID) x = (ushort) (MaxCellID - 1);
            if (y >= MaxCellID) y = (ushort) (MaxCellID - 1);

            return Cells[x, y] ?? (Cells[x, y] = new CellMgr(this, x, y));
        }

        public void LoadCells(ushort x, ushort y, int range)
        {
            GetCells(x, y, range, cell => { cell?.Load(); });
        }

        public void GetCells(ushort x, ushort y, int range, GetCellDelegate cellFunction)
        {
            if (cellFunction == null)
                return;

            var minX = (ushort) Math.Max(0, x - range);
            var maxX = (ushort) Math.Min(MaxCellID - 1, x + range);

            var minY = (ushort) Math.Max(0, y - range);
            var maxY = (ushort) Math.Min(MaxCellID - 1, y + range);

            for (var ox = minX; ox <= maxX; ++ox)
            for (var oy = minY; oy <= maxY; ++oy)
                cellFunction(GetCell(ox, oy));
        }

        #endregion

        #region Spawns

        private CellSpawns[,] _cellSpawns;

        public void LoadSpawns()
        {
            _cellSpawns = CellSpawnService.GetCells(RegionId);
        }

        public CellSpawns GetCellSpawn(ushort x, ushort y)
        {
            x = (ushort) Math.Min(MaxCellID - 1, x);
            y = (ushort) Math.Min(MaxCellID - 1, y);

            return _cellSpawns[x, y] ?? (_cellSpawns[x, y] = new CellSpawns(RegionId, x, y));
        }

        public Creature CreateCreature(Creature_spawn spawn)
        {
#if NO_CREATURE
            return null;
#endif
            if (spawn?.Proto == null)
                return null;

            var crea = new Creature(spawn);
            AddObject(crea, spawn.ZoneId);
            return crea;
        }

        public Boss CreateBoss(Creature_spawn spawn, uint bossId)
        {
            if (spawn?.Proto == null)
                return null;

            var boss = new Boss(spawn, bossId);
            AddObject(boss, spawn.ZoneId);
            return boss;
        }

        public AdvancedCreature CreateAdvancedCreature(Creature_spawn spawn)
        {
#if NO_CREATURE
            return null;
#endif
            if (spawn?.Proto == null)
                return null;

            var crea = new AdvancedCreature(spawn);
            AddObject(crea, spawn.ZoneId);
            return crea;
        }

        public GameObject CreateGameObject(GameObject_spawn spawn)
        {
            if (spawn == null || spawn.Proto == null)
                return null;

            var obj = new GameObject(spawn);
            AddObject(obj, spawn.ZoneId);
            return obj;
        }

        public LootChest CreateLootChest(GameObject_spawn spawn)
        {
            if (spawn == null || spawn.Proto == null)
                return null;

            var obj = new LootChest(spawn);
            AddObject(obj, spawn.ZoneId);
            return obj;
        }

        public ChapterObject CreateChapter(Chapter_Info chapter)
        {
            var obj = new ChapterObject(chapter);
            AddObject(obj, chapter.ZoneId);
            return obj;
        }

        public PublicQuest CreatePQuest(PQuest_Info quest)
        {
            // Detecting the duplicate is not enough: falling through adds a second live
            // PublicQuest to the region and only then throws on the dictionary key, leaving an
            // orphan that keeps ticking and can award its own stage rewards and reward chest.
            // Return the existing quest instead.
            PublicQuest existing;
            if (PublicQuests.TryGetValue(quest.Entry, out existing))
            {
                Log.Error("CreatePQuest",
                    "Public quest " + quest.Entry + " in zone " + quest.ZoneId +
                    " is already live in region " + RegionId + "; reusing it instead of creating a duplicate.");
                return existing;
            }

            var zone = GetZoneMgr(quest.ZoneId);
            var obj = new PublicQuest(quest);
            AddObject(obj, quest.ZoneId);
            PublicQuests.Add(quest.Entry, obj);
            return obj;
        }

        #endregion

        #region Outgoing packet logging

        private readonly Dictionary<byte, uint> _packetVolume = new Dictionary<byte, uint>();

        public bool LogPacketVolume;
        private bool _sending;

        public void NotifyOutgoingPacket(byte opcode, uint len)
        {
            lock (_packetVolume)
            {
                if (_sending)
                    return;
                if (_packetVolume.ContainsKey(opcode))
                    _packetVolume[opcode] += len;
                else
                    _packetVolume.Add(opcode, len);
            }
        }

        public void TogglePacketLogging()
        {
            if (!LogPacketVolume)
            {
                LogPacketVolume = true;
            }

            else
            {
                lock (_packetVolume)
                {
                    _packetVolume.Clear();
                }

                LogPacketVolume = false;
            }
        }

        public void SendPacketVolumeInfo(Player plr)
        {
            lock (_packetVolume)
            {
                _sending = true;

                plr.SendClientMessage("[Total Packet Volume]");

                foreach (var pair in _packetVolume)
                    plr.SendClientMessage((Opcodes) pair.Key + ": " + $"{pair.Value * 0.001f:0.0##}" + "KB");

                _sending = false;
            }
        }

        #endregion
    }
}
