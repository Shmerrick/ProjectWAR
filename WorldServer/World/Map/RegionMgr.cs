//#define NO_CREATURE

using Common;
using Common.Database.World.Maps;
using FrameWork;
using GameData;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        private readonly Thread _updater;

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

        public RegionMgr(ushort regionId, List<Zone_Info> zones, string name, IApocCommunications apocCommunications)
        {
            ApocCommunications = apocCommunications;
            RegionId = regionId;
            ZonesInfo = zones;
            RegionName = name;

            LoadSpawns();

            BountyManager = new BountyManager();

            try
            {
                switch (ZonesInfo[0].Pairing)
                {
                    case (byte)Pairing.PAIRING_GREENSKIN_DWARVES:
                        _races = new[] { Races.RACES_DWARF, Races.RACES_GOBLIN };
                        break;

                    case (byte)Pairing.PAIRING_EMPIRE_CHAOS:
                        _races = new[] { Races.RACES_EMPIRE, Races.RACES_CHAOS };
                        break;

                    case (byte)Pairing.PAIRING_ELVES_DARKELVES:
                        _races = new[] { Races.RACES_HIGH_ELF, Races.RACES_DARK_ELF };
                        break;
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Zone - Pairing {e.Message} {e.StackTrace}");
                throw;
            }

            _updater = new Thread(Update);
            _updater.Start();

            RegionCreatures = GetObjects<Creature>().ToList();
        }

        public IApocCommunications ApocCommunications { get; set; }

        public void Stop()
        {
            try
            {
                Log.Debug("RegionMgr", "[" + RegionId + "] Stop");
                _running = false;

                foreach (var zone in ZonesMgr)
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

        public List<ZoneMgr> ZonesMgr = new List<ZoneMgr>();

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

            ZoneMgr mgr = null;
            foreach (var z in ZonesMgr)
                if (z != null && z.Info.ZoneId == zoneId)
                {
                    mgr = z;
                    break;
                }

            if (mgr == null)
            {
                mgr = new ZoneMgr(this, info);
                ZonesMgr.Add(mgr);
            }

            return mgr;
        }

        public ushort CheckZone(Object obj)
        {
            var info = GetZone(obj.XOffset, obj.YOffset);
            if (info != null && info != obj.Zone.Info) AddObject(obj, info.ZoneId);

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
                if (elapsed < REGION_UPDATE_INTERVAL) Thread.Sleep((int)(REGION_UPDATE_INTERVAL - elapsed));
            }

            DisposeActors();
        }

        /// <summary>
        ///     A list of players currently within this region. Should only be accessed from within this region's thread!
        /// </summary>
        public readonly List<Player> Players = new List<Player>();

        public int OrderPlayers { get; set; }
        public int DestPlayers { get; set; }

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

                            if (plr != null)
                            {
                                plr.InRegionChange = true;
                                if (!Players.Contains(plr))
                                {
                                    Players.Add(plr);
                                    if (plr.Realm == Realms.REALMS_REALM_ORDER) OrderPlayers++;
                                    if (plr.Realm == Realms.REALMS_REALM_DESTRUCTION) DestPlayers++;
                                }
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

                        if (removeInfo.Obj is Player)
                            Players.Remove((Player)removeInfo.Obj);
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
            for (var i = 0; i < Objects.Length; ++i)
            {
                var obj = Objects[i];
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
                        ((Player)obj).SendClientMessage(e.GetType().Name + " was thrown from " + e.TargetSite?.Name +
                                                         ".");
                    }
                    else if (obj is IApocBattleFront)
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

            for (var i = 0; i < Objects.Length; ++i)
            {
                var obj = Objects[i];

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

            foreach (var obj in Objects)
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

        #endregion Diagnostic

        #endregion ZoneMgr

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
                    var offX = (ushort)Math.Truncate((decimal)((point.X - mapX) / 4096 + zone.OffX));
                    var offY = (ushort)Math.Truncate((decimal)((point.Y - mapY) / 4096 + zone.OffY));

                    for (var x = offX - 1; x < offX + 1; x++) //scan all cells within radius
                        for (var y = offY - 1; y < offY + 1; y++)
                            if (x >= 0 && x <= MaxCellID && y >= 0 && y <= MaxCellID)
                                if (Cells[x, y] != null)
                                    foreach (var obj in Cells[x, y].Objects.ToList())
                                    {
                                        count++;
                                        if (obj is T && obj.PointWithinRadiusFeet(point, radius) && !list.Contains(obj))
                                            list.Add((T)obj);
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

        #endregion Ranged

        #region Oid

        public static ushort MaxObjects = 65000;
        public static ushort MaxOid = 2;
        public Object[] Objects = new Object[MaxObjects];
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
                    MaxOid = (ushort)i;
                    return (ushort)i;
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
            return (ushort)Objects.Count(obj => obj != null);
        }

        public List<T> GetObjects<T>() where T : Object
        {
            return Objects.Where(obj => obj != null && obj is T).Select(e => (T)e).ToList();
        }

        #endregion Oid

        #region Cells

        public CellMgr[,] Cells = new CellMgr[MaxCellID, MaxCellID];

        public delegate void GetCellDelegate(CellMgr cell);

        public CellMgr GetCell(ushort x, ushort y)
        {
            if (x >= MaxCellID) x = (ushort)(MaxCellID - 1);
            if (y >= MaxCellID) y = (ushort)(MaxCellID - 1);

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

            var minX = (ushort)Math.Max(0, x - range);
            var maxX = (ushort)Math.Min(MaxCellID - 1, x + range);

            var minY = (ushort)Math.Max(0, y - range);
            var maxY = (ushort)Math.Min(MaxCellID - 1, y + range);

            for (var ox = minX; ox <= maxX; ++ox)
                for (var oy = minY; oy <= maxY; ++oy)
                    cellFunction(GetCell(ox, oy));
        }

        #endregion Cells

        #region Spawns

        private CellSpawns[,] _cellSpawns;

        public void LoadSpawns()
        {
            _cellSpawns = CellSpawnService.GetCells(RegionId);
        }

        public CellSpawns GetCellSpawn(ushort x, ushort y)
        {
            x = (ushort)Math.Min(MaxCellID - 1, x);
            y = (ushort)Math.Min(MaxCellID - 1, y);

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
            if (PublicQuests.ContainsKey(quest.Entry))
                Log.Error("CreatePQuest",
                    "Attempted to create public quest that was already contained: ZoneID:" + quest.ZoneId + " Entry: " +
                    quest.Entry);
            var zone = GetZoneMgr(quest.ZoneId);
            var obj = new PublicQuest(quest);
            AddObject(obj, quest.ZoneId);
            PublicQuests.Add(quest.Entry, obj);
            return obj;
        }

        #endregion Spawns

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
                    plr.SendClientMessage((Opcodes)pair.Key + ": " + $"{pair.Value * 0.001f:0.0##}" + "KB");

                _sending = false;
            }
        }

        #endregion Outgoing packet logging
    }
}