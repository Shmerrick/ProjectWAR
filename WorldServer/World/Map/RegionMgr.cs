//#define NO_CREATURE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Common;
using FrameWork;
using WorldServer.World.BattleFronts;
using GameData;
using WorldServer.World.Objects.PublicQuests;
using WorldServer.Scenarios;
using WorldServer.World.BattleFronts.Objectives;
using Common.Database.World.Maps;
using NLog;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Apocalypse;
using BattleFrontConstants = WorldServer.World.Battlefronts.Apocalypse.BattleFrontConstants;

namespace WorldServer
{

    public class RegionMgr
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public static UInt16 MaxCellID = 800;
        public static UInt16 MaxCells = 16;
        public static Int32 MaxVisibilityRange = 400; // It was 400 on Age of Reckoning

        public IApocCommunications ApocCommunications { get; set; }
        public UInt16 RegionId;

        private volatile Boolean _updateLoopRunning = false;
        private volatile Boolean _updateTriggerLoopRunning = false;
        private readonly Thread _updateTriggerLoop;
        private readonly Thread _updateLoop;
        private const Int32 _updateInterval = 99; // every minute (1000ms) process operations, actions and objects.
        private static ManualResetEvent _updateSleep = new ManualResetEvent(false);

        public List<Zone_Info> ZonesInfo;
        
        public Campaign Campaign;
        public Scenario Scenario;
        public String RegionName;

        /// <summary>Races associated with the pairing, may be null</summary>
        private readonly Races[] _races;

        public RegionMgr(UInt16 regionId, List<Zone_Info> zones, String name, IApocCommunications apocCommunications)
        {
            ApocCommunications = apocCommunications;
            RegionId = regionId;
            ZonesInfo = zones;
            RegionName = name;
            
            LoadSpawns();

            switch (ZonesInfo[0].Pairing)
            {
                case (Byte)Pairing.PAIRING_GREENSKIN_DWARVES:
                    _races = new Races[] { Races.RACES_DWARF, Races.RACES_GOBLIN };
                    break;
                case (Byte)Pairing.PAIRING_EMPIRE_CHAOS:
                    _races = new Races[] { Races.RACES_EMPIRE, Races.RACES_CHAOS };
                    break;
                case (Byte)Pairing.PAIRING_ELVES_DARKELVES:
                    _races = new Races[] { Races.RACES_HIGH_ELF, Races.RACES_DARK_ELF };
                    break;
                default:
                    break;
            }

            ThreadStart updateTriggerStart = UpdateTrigger;
            _updateTriggerLoop = new Thread(updateTriggerStart);
            _updateTriggerLoop.Name = "RegionMgr.UpdateTrigger";
            _updateTriggerLoopRunning = true;
            _updateTriggerLoop.Start();

            ThreadStart updateLoopStart = UpdateLoop;
            _updateLoop = new Thread(updateLoopStart);
            _updateLoop.Name = "RegionMgr.Update";
            _updateLoopRunning = true;
            _updateLoop.Start();
        }

        /// <summary>
        /// Returns the zone entity of given identifier.
        /// </summary>
        /// <param name="zoneId">Identifier of the searched zone</param>
        /// <returns>Zone or null if does not exists</returns>
        public Zone_Info GetZone_Info(UInt16 zoneId)
        {
            foreach (Zone_Info zone in ZonesInfo)
            {
                if (zone != null && zone.ZoneId == zoneId)
                    return zone;
            }
            return null;
        }

        public Zone_Info GetZone(UInt16 offX, UInt16 offY)
        {
            return ZonesInfo.Find(zone => zone != null &&
                (zone.OffX <= offX && zone.OffX + MaxCells > offX) &&
                (zone.OffY <= offY && zone.OffY + MaxCells > offY));
        }

        public Int32 GetTier() => Scenario != null ? Scenario.Tier : ZonesInfo.Count > 0 ? ZonesInfo[0].Tier : 4;

        #region ZoneMgr

        public List<ZoneMgr> ZonesMgr = new List<ZoneMgr>();

        /// <summary>
        /// Gets the zone of given id, lazy loading it if necessary.
        /// </summary>
        /// <param name="zoneId">Id of the zone to get</param>
        /// <returns>Zone or null if zone info does not exists</returns>
        public ZoneMgr GetZoneMgr(UInt16 zoneId)
        {
            Zone_Info info = GetZone_Info(zoneId);
            if (info == null)
                return null;

            ZoneMgr mgr = null;
            foreach (ZoneMgr z in ZonesMgr)
            {
                if (z != null && z.Info.ZoneId == zoneId)
                {
                    mgr = z;
                    break;
                }
            }

            if (mgr == null)
            {
                mgr = new ZoneMgr(this, info);
                ZonesMgr.Add(mgr);
            }

            return mgr;
        }

        public UInt16 CheckZone(Object obj)
        {
            Zone_Info info = GetZone(obj.XOffset, obj.YOffset);
            if (info != null && info != obj.Zone.Info)
            {
                AddObject(obj, info.ZoneId);
            }

            CellMgr curCell = obj._Cell;
            CellMgr newCell = GetCell(obj.XOffset, obj.YOffset);

            if (newCell == null || newCell == curCell)
                return info?.ZoneId ?? 0;

            curCell?.RemoveObject(obj);
            newCell.AddObject(obj); // On l'ajoute dans le nouveau cell

            return info?.ZoneId ?? 0;
        }

        ~RegionMgr()
        {
            Stop();
        }

        public void Stop()
        {
            _updateTriggerLoopRunning = false;
            _updateLoopRunning = false;
            _updateSleep.Set();
        }

        protected virtual void UpdateTrigger()
        {
            while (_updateTriggerLoopRunning) {
                Thread.Sleep(_updateInterval);
                _updateSleep.Set();
                Thread.Yield();
                //Log.Notice(Thread.CurrentThread.Name, $"A Thread Necromancer awakens.");
            }
        }

        protected void UpdateLoop()
        {
            Int64 previousTick;
            Int64 thisTick = TCPManager.GetTimeStampMS();
            Int64 deltaTick;

            while (_updateLoopRunning) {
                Thread.Yield();

                previousTick = thisTick;
                thisTick = TCPManager.GetTimeStampMS();
                deltaTick = thisTick - previousTick;

                try
                {
                    WorldMgr.UpdateScripts(deltaTick);

                    AddNewObjects();

                    UpdateActors(deltaTick);

                    RemoveOldObjects();

                    Campaign?.Update(deltaTick);

                    Campaign?.BattleFrontManager.Update(deltaTick);
                }
                catch (Exception e) {
                    Log.Error(RegionName + $" ({RegionId})", e.Message);
                }

                _updateSleep.WaitOne();
                _updateSleep.Reset();
                //Log.Notice(Thread.CurrentThread.Name, $"Thread Necromancer hit you for {deltaTick}ms.");
            }

            DisposeActors();
        }

        /// <summary>
        /// A list of players currently within this region. Should only be accessed from within this region's thread!
        /// </summary>
        public readonly List<Player> Players = new List<Player>();

        public Int32 OrderPlayers { get; set; }

        public Int32 DestPlayers { get; set; }

        private void AddNewObjects()
        {
            try
            {
                lock (_objectsToAdd)
                {
                    foreach (ObjectAdd obj in _objectsToAdd) {
                        Thread.Yield();

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
                                    if (plr.Realm == Realms.REALMS_REALM_ORDER)
                                    {
                                        OrderPlayers++;
                                    }
                                    if (plr.Realm == Realms.REALMS_REALM_DESTRUCTION)
                                    {
                                        DestPlayers++;
                                    }
                                }
                            }
                        }

                        else
                            obj.Obj.Zone.RemoveObject(obj.Obj);

                        ZoneMgr mgr = GetZoneMgr(obj.ZoneId);
                        mgr.AddObject(obj.Obj);

                        if (obj.MustUpdateRange)
                            UpdateRange(obj.Obj);

                        if (plr != null)
                        {
                            //Bttlfront?.SendObjectives(plr);

                            Campaign?.SendObjectives(plr);
                            // ApocCommunications.SendCampaignStatus(plr, Campaign?.VictoryPointProgress, (Campaign?.VictoryPointProgress.DestructionVictoryPoints >= BattleFrontConstants.LOCK_VICTORY_POINTS) ? Realms.REALMS_REALM_DESTRUCTION : Realms.REALMS_REALM_ORDER);
                            WorldMgr.UpdateRegionCaptureStatus(WorldMgr.LowerTierCampaignManager, WorldMgr.UpperTierCampaignManager);
                        }
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
                    foreach (ObjectRemove removeInfo in _objectsToRemove) {
                        Thread.Yield();

                        removeInfo.Obj.InRegionChange = false;
                        removeInfo.Obj.ClearRange();

                        if (removeInfo.Cell != null)
                            removeInfo.Cell.RemoveObject(removeInfo.Obj);

                        if (removeInfo.Zone != null)
                            removeInfo.Zone.RemoveObject(removeInfo.Obj);

                        if (removeInfo.Oid != 0)
                        {
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

        private void UpdateActors(Int64 start)
        {
            for (Int32 i = 0; i < Objects.Length; ++i) {
                Thread.Yield();

                Object obj = Objects[i];
                if (obj == null || obj.Region != this)
                    continue;

                try
                {
                    if (!obj.Loaded)
                        obj.Load();
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
                        ((Player)obj).SendClientMessage(e.GetType().Name + " was thrown from " + e.TargetSite?.Name + ".");
                    else if (obj is IApocBattleFront)
                    {
                        try
                        {
                            foreach (Player player in Players)
                                player.SendClientMessage(e.GetType().Name + " from " + e.TargetSite?.Name + " was thrown from a Battlefield Objective in this region.");
                        }
                        catch (Exception)
                        {
                            Log.Error("RegionMgr", "Exception throw within Player exception notification");
                        }
                    }

                    else
                    {
                        obj.Say(e.GetType().Name + " was thrown from " + e.TargetSite?.Name + ". This object will be destroyed.");
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

            for (Int32 i = 0; i < Objects.Length; ++i) {
                Thread.Yield();

                Object obj = Objects[i];

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
        /// Checks whether the region matches the given race.
        /// </summary>
        /// <param name="race">Race to check</param>
        /// <returns>True if matchs, false otherwise</returns>
        public Boolean Matches(Races race) => _races != null && (_races[0] == race || _races[1] == race);

        #region Diagnostic
        public void CountObjects(Player plr)
        {
            Dictionary<String, Int32> objectCounts = new Dictionary<String, Int32>();

            foreach (Object obj in Objects)
            {
                if (obj == null)
                    continue;
                String type = obj.GetType().ToString();

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

        public void GetRangedObject(Object obj, Int32 range, RangedObjectDelegate rangeFunction)
        {
            if (!obj.IsInWorld())
                return;

            GetCells(obj.XOffset, obj.YOffset, range, cell =>
            {
                for (Int32 i = 0; i < cell.Objects.Count; ++i)
                {
                    Object distObject;
                    if ((distObject = cell.Objects[i]) == null)
                    {
                        cell.Objects.RemoveAt(i);
                        i--;
                    }
                    else if (obj.Get2DDistanceToObject(distObject) <= MaxVisibilityRange)
                        rangeFunction(distObject);
                }
            });
        }
        public static Boolean IsRange(Int32 fixe, Int32 move, Int32 range)
        {
            Int32 max = fixe + range;
            Int32 min = fixe - range;

            return move <= max && move >= min;
        }

        public void DispatchPacket(PacketOut packet, Point3D point, Int32 radius, Func<Player, Boolean> predicate = null)
        {
            foreach (var player in WorldQuery<Player>(point, radius, predicate))
            {
                player.DispatchPacket(packet, true);
            }
        }

        public List<T> WorldQuery<T>(Point3D point, Int32 radius, Func<T, Boolean> predicate = null) where T : Object
        {
            var list = new List<T>();

            Int32 aradius = radius * Point2D.UNITS_TO_FEET;
            Int32 count = 0;
            foreach (var zone in ZonesInfo.ToList())
            {
                var mapX = zone.OffX << 12;
                var mapY = zone.OffY << 12;

                if (point.X - aradius >= mapX && point.X + aradius <= mapX + 0xFFFF &&
                    point.Y - aradius >= mapY && point.Y + aradius <= mapY + 0xFFFF) //is the point on this zone?
                {
                    UInt16 offX = (UInt16)Math.Truncate((Decimal)((point.X - mapX) / 4096 + zone.OffX));
                    UInt16 offY = (UInt16)Math.Truncate((Decimal)((point.Y - mapY) / 4096 + zone.OffY));

                    for (Int32 x = offX - 1; x < offX + 1; x++) //scan all cells within radius
                        for (Int32 y = offY - 1; y < offY + 1; y++)
                        {
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

            }
            return predicate != null ? list.Where(predicate).ToList() : list;
        }

        public Boolean UpdateRange(Object curObj, Boolean forceUpdate = false)
        {
            if (!curObj.IsActive || curObj.IsDisposed)
                return false;

            if (curObj.X == 0 && curObj.Y == 0)
                return false;

            Single distance = curObj.Get2DDistanceToWorldPoint(curObj.LastRangeCheck);
            if (distance > 100 || forceUpdate)
            {
                curObj.LastRangeCheck.X = curObj.WorldPosition.X;
                curObj.LastRangeCheck.Y = curObj.WorldPosition.Y;
            }
            else
                return false;

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

            for (Int32 i = 0; i < curObj.ObjectsInRange.Count; ++i)
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

        public Boolean IsVisibleBForA(Object a, Object b)
        {
            if (a == null || b == null || a.IsDisposed || b.IsDisposed)
                return false;

            return a == b || !a.IsActive || !b.IsActive || !b.IsVisible
                ? false
                : !b.IsPlayer() || b.GetPlayer().Client != null && b.GetPlayer().Client.IsPlaying();
        }

        #endregion

        #region Oid

        public static UInt16 MaxObjects = 65000;
        public static UInt16 MaxOid = 2;
        public Object[] Objects = new Object[MaxObjects];
        public Dictionary<UInt32, PublicQuest> PublicQuests = new Dictionary<UInt32, PublicQuest>();
        private List<ObjectAdd> _objectsToAdd = new List<ObjectAdd>();
        private List<ObjectRemove> _objectsToRemove = new List<ObjectRemove>();

        public void GenerateOid(Object obj)
        {
            UInt16 oid = GetOid();
            Objects[oid] = obj;

            obj.SetOid(oid);
            obj.Loaded = false;
        }

        public UInt16 GetOid()
        {
            for (Int32 i = MaxOid; i < MaxObjects; ++i)
            {
                if (MaxOid >= MaxObjects - 1)
                {
                    MaxOid = 2;
                    i = 2;
                }

                if (Objects[i] == null)
                {
                    MaxOid = (UInt16)i;
                    return (UInt16)i;
                }
            }

            return MaxOid;
        }

        public struct ObjectAdd
        {
            public Object Obj;
            public UInt16 ZoneId;
            public Boolean MustUpdateRange;
        }

        public Boolean AddObject(Object obj, UInt16 zoneId, Boolean mustUpdateRange = false)
        {
            Zone_Info info = GetZone_Info(zoneId);
            if (info == null)
            {
                Log.Error("RegionMgr", "AddObject: Unable to add object " + obj.Name + " to invalid Zone with ID : " + zoneId);
                return false;
            }
            
            ObjectAdd add = new ObjectAdd
            {
                Obj = obj,
                ZoneId = zoneId,
                MustUpdateRange = mustUpdateRange
            };

            //obj.MovementZone = GetZoneMgr(zoneId);

            lock (_objectsToAdd)
                _objectsToAdd.Add(add);

            return true;
        }

        public struct ObjectRemove
        {
            public Object Obj;
            public UInt16 Oid;
            public ZoneMgr Zone;
            public CellMgr Cell;
        }

        public Boolean RemoveObject(Object obj)
        {
			// nothing to remove here
			if (obj == null)
				return true;

            //if (Obj.IsPlayer())
            //    Log.Success("RemoveObject", Obj.Name);

            obj.EvtInterface.Notify(EventName.OnRemoveFromWorld, obj, null);

            ObjectRemove rem = new ObjectRemove
            {
                Obj = obj,
                Oid = obj.Oid,
                Zone = obj.Zone,
                Cell = obj._Cell
            };

            lock (_objectsToRemove)
                _objectsToRemove.Add(rem);

            return false;
        }

        public Object GetObject(UInt16 oid)
        {
            if (oid < 2 || oid >= Objects.Length)
                return null;

            Object obj = Objects[oid];

            return obj == null || obj.IsDisposed ? null : obj;
        }

        public Player GetPlayer(UInt16 oid) => (GetObject(oid) as Player);
        public UInt16 GetObjects() => (UInt16)Objects.Count(obj => obj != null);

        public List<T> GetObjects<T>() where T : Object => Objects.Where(obj => obj != null && obj is T).Select(e => (T)e).ToList();

        #endregion

        #region Cells

        public CellMgr[,] Cells = new CellMgr[MaxCellID, MaxCellID];
        public delegate void GetCellDelegate(CellMgr cell);

        public CellMgr GetCell(UInt16 x, UInt16 y)
        {
            if (x >= MaxCellID) x = (UInt16)(MaxCellID - 1);
            if (y >= MaxCellID) y = (UInt16)(MaxCellID - 1);

            return Cells[x, y] ?? (Cells[x, y] = new CellMgr(this, x, y));
        }
        public void LoadCells(UInt16 x, UInt16 y, Int32 range)
        {
            GetCells(x, y, range, cell =>
            {
                cell?.Load();
            });
        }
        public void GetCells(UInt16 x, UInt16 y, Int32 range, GetCellDelegate cellFunction)
        {
            if (cellFunction == null)
                return;

            UInt16 minX = (UInt16)Math.Max(0, x - range);
            UInt16 maxX = (UInt16)Math.Min(MaxCellID - 1, x + range);

            UInt16 minY = (UInt16)Math.Max(0, y - range);
            UInt16 maxY = (UInt16)Math.Min(MaxCellID - 1, y + range);

            for (UInt16 ox = minX; ox <= maxX; ++ox)
                for (UInt16 oy = minY; oy <= maxY; ++oy)
                    cellFunction(GetCell(ox, oy));
        }

        #endregion

        #region Spawns

        private CellSpawns[,] _cellSpawns;

        public void LoadSpawns() => _cellSpawns = CellSpawnService.GetCells(RegionId);

        public CellSpawns GetCellSpawn(UInt16 x, UInt16 y)
        {
            x = (UInt16)Math.Min(MaxCellID - 1, x);
            y = (UInt16)Math.Min(MaxCellID - 1, y);

            return _cellSpawns[x, y] ?? (_cellSpawns[x, y] = new CellSpawns(RegionId, x, y));
        }
        public Creature CreateCreature(Creature_spawn spawn)
        {
#if NO_CREATURE
            return null;
#endif
            if (spawn?.Proto == null)
                return null;

            Creature crea = new Creature(spawn);
            AddObject(crea, spawn.ZoneId);
            return crea;
        }
        public GameObject CreateGameObject(GameObject_spawn spawn)
        {
            if (spawn == null || spawn.Proto == null)
                return null;

            GameObject obj = new GameObject(spawn);
            AddObject(obj, spawn.ZoneId);
            return obj;
        }
        public ChapterObject CreateChapter(Chapter_Info chapter)
        {
            ChapterObject obj = new ChapterObject(chapter);
            AddObject(obj, chapter.ZoneId);
            return obj;
        }
        public PublicQuest CreatePQuest(PQuest_Info quest)
        {
            if (PublicQuests.ContainsKey(quest.Entry))
                Log.Error("CreatePQuest", "Attempted to create public quest that was already contained: ZoneID:" + quest.ZoneId + " Entry: " + quest.Entry);
            ZoneMgr zone = GetZoneMgr(quest.ZoneId);
            PublicQuest obj = new PublicQuest(quest);
            AddObject(obj, quest.ZoneId);
            PublicQuests.Add(quest.Entry, obj);
            return obj;
        }

        #endregion

        #region Outgoing packet logging

        private readonly Dictionary<Byte, UInt32> _packetVolume = new Dictionary<Byte, UInt32>();

        public Boolean LogPacketVolume;
        private Boolean _sending;

        public void NotifyOutgoingPacket(Byte opcode, UInt32 len)
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
                LogPacketVolume = true;

            else
            {
                lock (_packetVolume)
                    _packetVolume.Clear();
                LogPacketVolume = false;
            }
        }

        public void SendPacketVolumeInfo(Player plr)
        {
            lock (_packetVolume)
            {
                _sending = true;

                plr.SendClientMessage("[Total Packet Volume]");

                foreach (KeyValuePair<Byte, UInt32> pair in _packetVolume)
                    plr.SendClientMessage((Opcodes)pair.Key + ": " + $"{pair.Value * 0.001f:0.0##}" + "KB");

                _sending = false;
            }
        }

        #endregion
    }
}
