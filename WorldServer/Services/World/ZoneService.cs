using Common;
using FrameWork;
using GameData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Database.World.Battlefront;
using Common.Database.World.Maps;
using WorldServer.Managers;
using WorldServer.World.Positions;

namespace WorldServer.Services.World
{
    [Service]
    public class ZoneService : ServiceBase
    {

        public static List<Zone_Info> _Zone_Info;

        [LoadingFunction(true)]
        public static void LoadZone_Info()
        {
            Log.Debug("WorldMgr", "Loading Zone_Info...");

            _Zone_Info = Database.SelectAllObjects<Zone_Info>() as List<Zone_Info>;
            NormalizeZoneInfoMetadata();

            Log.Success("LoadZone_Info", "Loaded " + _Zone_Info.Count + " Zone_Info");

            if (Program.Config.RegionOcclusionFolder != null && Directory.Exists(WorldServer.Program.Config.RegionOcclusionFolder))
            {
                WorldServer.World.Map.Occlusion.InitZones(WorldServer.Program.Config.RegionOcclusionFolder);
            }
            else
                Log.Error("WorldServer", "RegionOcclusionFolder not specified/found in World.config. No server side LOS will be performed.");

            LoadZoneJumps();
            WorldMgr.WorldUpdateStart();
            WorldMgr.GroupUpdateStart();
        }

        private static void NormalizeZoneInfoMetadata()
        {
            if (_Zone_Info == null)
                return;

            foreach (Zone_Info zone in _Zone_Info)
                NormalizeZoneInfo(zone);
        }

        private static void NormalizeZoneInfo(Zone_Info zone)
        {
            if (zone == null)
                return;

            if (zone.ZoneId != LotdService.LotdZoneId || zone.Pairing == (byte)Pairing.PAIRING_LAND_OF_THE_DEAD)
                return;

            byte originalPairing = zone.Pairing;
            zone.Pairing = (byte)Pairing.PAIRING_LAND_OF_THE_DEAD;
            zone.Dirty = false;

            Log.Notice("Zone_Info", "Normalized zone " + zone.ZoneId + " pairing from " + originalPairing +
                " to " + zone.Pairing + " for Land of the Dead flight metadata.");
        }

        /// <summary>
        /// Gets a zone of given id.
        /// </summary>
        /// <param name="RegionId">Identifier of returned zone.</param>
        /// <returns>Zone or null if was not fount</returns>
        public static Zone_Info GetZone_Info(ushort ZoneId)
        {
            if (_Zone_Info == null) return null;
            return _Zone_Info.FirstOrDefault(zone => zone != null && zone.ZoneId == ZoneId);
        }
            /// <summary>
            /// Gets all zones in given region.
            /// </summary>
            /// <param name="RegionId">Region id to get zones of.</param>
            /// <returns>List on known regions, cannot be null, may be empty</returns>
            public static List<Zone_Info> GetZoneRegion(ushort RegionId)
        {
            List<Zone_Info> list = new List<Zone_Info>();
            if (_Zone_Info == null) return list;
            foreach (Zone_Info zone in _Zone_Info)
                if (zone != null && zone.Region == RegionId)
                    list.Add(zone);
            return list;
        }

        /// <summary>
        /// Gets all zones in the given region, or falls back to a single zone when the identifier is already a zone id.
        /// </summary>
        public static List<Zone_Info> GetRegionOrZone(ushort regionOrZoneId)
        {
            List<Zone_Info> zones = GetZoneRegion(regionOrZoneId);

            if (zones.Count > 0)
                return zones;

            Zone_Info zone = GetZone_Info(regionOrZoneId);

            if (zone == null)
                return zones;

            zones.Add(zone);
            return zones;
        }

        public static Zone_Info GetZoneFromOffsets(int OffsetX, int OffsetY)
        {
            if (_Zone_Info == null) return null;
            foreach (Zone_Info Info in _Zone_Info)
            {
                if (OffsetX >= Info.OffX && OffsetX < Info.OffX + 16
                    && OffsetY >= Info.OffY && OffsetY < Info.OffY + 16)
                {
                    return Info;
                }
            }

            return null;
        }

        #region Zone Portals

        public static Dictionary<uint, Zone_jump> Zone_Jumps;

        public static void LoadZoneJumps()
        {
            Log.Debug("WorldMgr", "Loading Zone_Jump...");

            Zone_Jumps = new Dictionary<uint, Zone_jump>();
            IList<Zone_jump> Jumps = Database.SelectAllObjects<Zone_jump>() as List<Zone_jump>;

            foreach (Zone_jump Jump in Jumps)
            {
                if (GetZone_Info(Jump.ZoneID) != null && !Zone_Jumps.ContainsKey(Jump.Entry))
                {
                    Zone_Jumps.Add(Jump.Entry, Jump);
                }
                else
                    Log.Error("Zone_Jump", "Invalid Jump: " + Jump.Entry + ", Zone=" + Jump.ZoneID);
            }

            Log.Success("LoadZone_Jump", "Loaded " + Zone_Jumps.Count + " Zone_Jump");
        }
        public static Zone_jump GetZoneJump(uint Entry)
        {
            // Zero means an unconfigured optional exit, not the legacy Avelorn row.
            if (Entry == 0)
                return null;

            Zone_jump jump;
            Zone_Jumps.TryGetValue(Entry, out jump);
            return jump;
        }

        public static Zone_jump GetZoneJumpByInstanceId(uint instanceID)
        {
            foreach (Zone_jump jump in Zone_Jumps.Values)
            {
                if (jump.InstanceID == instanceID)
                    return jump;
            }
            return null;
        }

        #endregion

        #region Zone Areas

        public static Dictionary<int, List<Zone_Area>> _Zone_Area;

        [LoadingFunction(true)]
        public static void LoadZone_Area()
        {
            Log.Debug("WorldMgr", "Loading Zone_Area...");

            _Zone_Area = new Dictionary<int, List<Zone_Area>>();
            IList<Zone_Area> Infos = Database.SelectAllObjects<Zone_Area>();
            foreach (Zone_Area Area in Infos)
            {
                AddZoneArea(Area);
            }

            Log.Success("LoadZone_Info", "Loaded " + Infos.Count + " Zone_Area");
        }

        public static void AddZoneArea(Zone_Area Area)
        {
            List<Zone_Area> Areas;
            if (!_Zone_Area.TryGetValue(Area.ZoneId, out Areas))
            {
                Areas = new List<Zone_Area>();
                _Zone_Area.Add(Area.ZoneId, Areas);
            }

            Areas.Add(Area);
        }
        public static List<Zone_Area> GetZoneAreas(ushort ZoneID)
        {
            List<Zone_Area> Areas;
            if (!_Zone_Area.TryGetValue(ZoneID, out Areas))
                return new List<Zone_Area>();
            return Areas;
        }

        #endregion

        #region Zone Respawns

        private static Dictionary<int, List<Zone_Respawn>> _zoneRespawnDic;
        private static List<Zone_Respawn> _zoneRespawns = new List<Zone_Respawn>();

        [LoadingFunction(true)]
        public static void LoadZone_Respawn()
        {
            Log.Debug("WorldMgr", "Loading LoadZone_Respawn...");

            _zoneRespawnDic = new Dictionary<int, List<Zone_Respawn>>();

            List<Zone_Respawn> respawns = Database.SelectAllObjects<Zone_Respawn>().OrderBy(res => res.RespawnID).ToList();

            foreach (Zone_Respawn respawn in respawns)
            {
                List<Zone_Respawn> L;
                if (!_zoneRespawnDic.TryGetValue(respawn.ZoneID, out L))
                {
                    L = new List<Zone_Respawn>();
                    _zoneRespawnDic.Add(respawn.ZoneID, L);
                }

                L.Add(respawn);
                _zoneRespawns.Add(respawn);
            }

            Log.Success("LoadZone_Info", "Loaded " + respawns.Count + " Zone_Respawn");
        }

        /// <summary>
        /// Gets the respawn info of given id.
        /// </summary>
        /// <param name="respawnId">Zone respawn id to search</param>
        /// <returns>Respawn info or null if does not exists</returns>
        public static Zone_Respawn GetZoneRespawn(ushort respawnId)
        {
            foreach (Zone_Respawn respawn in _zoneRespawns)
                if (respawn.RespawnID == respawnId)
                    return respawn;
            return null;
        }

        /// <summary>
        /// Gets the zone default respawn in given zone for given realm.
        /// </summary>
        /// <param name="zoneId">Zone to search respawn in</param>
        /// <param name="realm">Realm of returned respawn</param>
        /// <returns>Zone respawn of default one (wut ?)</returns>
        public static Zone_Respawn GetZoneRespawn(int zoneId, byte realm)
        {
            Zone_Respawn zoneFallback = null;

            foreach (Zone_Respawn res in _zoneRespawns)
            {
                if (res.ZoneID != zoneId)
                    continue;

                if (zoneFallback == null)
                    zoneFallback = res;

                if (res.Realm == realm)
                    return res;
            }

            return zoneFallback;
        }

        /// <summary>
        /// Gets the respawns info in given zone.
        /// </summary>
        /// <param name="respawnId">Zone id to search</param>
        /// <returns>Respawn info list or null if zone does not exists</returns>
        public static List<Zone_Respawn> GetZoneRespawns(ushort respawnId)
        {
            List<Zone_Respawn> respawns;
            _zoneRespawnDic.TryGetValue(respawnId, out respawns);
            return respawns;
        }

        #endregion

        #region Inter-Zone Flight
        public static Dictionary<ushort, Zone_Taxi[]> _Zone_Taxi = new Dictionary<ushort, Zone_Taxi[]>();

        [LoadingFunction(true)]
        public static void LoadZone_Taxi()
        {
            Log.Debug("LoadZone_Info", "Loading Zone_Taxis...");

            IList<Zone_Taxi> Taxis = Database.SelectAllObjects<Zone_Taxi>();
            _Zone_Taxi = new Dictionary<ushort, Zone_Taxi[]>();

            foreach (Zone_Taxi Taxi in Taxis)
            {
                Taxi.Info = GetZone_Info(Taxi.ZoneID);
                NormalizeTaxiWorldPosition(Taxi);

                Zone_Taxi[] Tax;
                if (!_Zone_Taxi.TryGetValue(Taxi.ZoneID, out Tax))
                {
                    Tax = new Zone_Taxi[(int)(Realms.REALMS_TOTAL_REALMS)];
                    _Zone_Taxi.Add(Taxi.ZoneID, Tax);
                }

                _Zone_Taxi[Taxi.ZoneID][Taxi.RealmID] = Taxi;
            }

            Log.Success("LoadZone_Info", "Loaded " + Taxis.Count + " Zone_Taxis");
        }

        private static void NormalizeTaxiWorldPosition(Zone_Taxi taxi)
        {
            if (taxi == null || taxi.WorldX == 0 || taxi.WorldY == 0)
                return;

            Zone_Info zone = taxi.Info ?? GetZone_Info(taxi.ZoneID);
            if (zone == null)
            {
                Log.Error("Zone_Taxi", "Taxi zone " + taxi.ZoneID + " realm " + taxi.RealmID + " references unknown zone.");
                return;
            }

            taxi.Info = zone;
            if (IsWorldPositionInsideZone(zone, (int)taxi.WorldX, (int)taxi.WorldY))
                return;

            if (taxi.WorldX > UInt16.MaxValue || taxi.WorldY > UInt16.MaxValue)
            {
                // Too large to be a local pin and outside the zone as world coordinates, so there is no
                // reading of this row that lands inside the zone it names. The flight handler teleports to
                // (ZoneID, WorldX, WorldY) as a unit, so offering it would drop the player outside the world
                // geometry. Hide the flight instead — the same choice already made for LOTD taxis when their
                // unlock state cannot be established. In-memory only; Zone_Taxi does not write back.
                taxi.Enable = false;

                Log.Notice("Zone_Taxi", "Taxi zone " + taxi.ZoneID + " realm " + taxi.RealmID +
                    " uses out-of-zone world coordinates " + taxi.WorldX + "," + taxi.WorldY +
                    "; flight disabled to avoid teleporting players out of bounds.");
                return;
            }

            uint originalX = taxi.WorldX;
            uint originalY = taxi.WorldY;
            Point3D world = GetWorldPosition(zone, (ushort)taxi.WorldX, (ushort)taxi.WorldY, taxi.WorldZ);
            if (!IsWorldPositionInsideZone(zone, world.X, world.Y))
            {
                // Neither reading of the row lands inside the zone; see the note above on why the flight is
                // hidden rather than left in the list.
                taxi.Enable = false;

                Log.Notice("Zone_Taxi", "Taxi zone " + taxi.ZoneID + " realm " + taxi.RealmID +
                    " has invalid coordinates " + taxi.WorldX + "," + taxi.WorldY +
                    " that could not be normalized; flight disabled.");
                return;
            }

            taxi.WorldX = (uint)world.X;
            taxi.WorldY = (uint)world.Y;
            taxi.WorldZ = (ushort)world.Z;

            Log.Notice("Zone_Taxi", "Normalized taxi zone " + taxi.ZoneID + " realm " + taxi.RealmID +
                " local pins " + originalX + "," + originalY + " to world " + taxi.WorldX + "," + taxi.WorldY + ".");
        }

        public static Zone_Taxi GetZoneTaxi(ushort ZoneId, byte Realm)
        {
            Zone_Taxi[] Taxis;
            if (_Zone_Taxi.TryGetValue(ZoneId, out Taxis))
                return Taxis[Realm];

            return null;
        }
        #endregion

        #region Utilities
        public static ushort CalculPin(Zone_Info Info, int WorldPos, bool x)
        {
            ushort Pin = 0;

            int BaseOffset = x ? Info.OffX << 12 : Info.OffY << 12;
            if (BaseOffset <= WorldPos)
                Pin = (ushort)(WorldPos - BaseOffset);

            return Pin;
        }

        /// <summary>
        /// Translates coordinates in a zone to world coordinates.
        /// Converts Pin into World Coordinates
        /// </summary>
        /// <param name="Info">Zone data</param>
        /// <param name="PinX">X coordinate in zone (as seen in map in game)</param>
        /// <param name="PinY">Y coordinate in zone (as seen in map in game)</param>
        /// <param name="PinZ">Z coordinate in zone (as seen in map in game)</param>
        /// <returns>World coordinates</returns>
        public static Point3D GetWorldPosition(Zone_Info Info, ushort PinX, ushort PinY, ushort PinZ)
        {
            int x = PinX > 32768 ? PinX - 32768 : PinX;
            int y = PinY > 32768 ? PinY - 32768 : PinY;

            Point3D worldPosition = new Point3D(0, 0, 0)
            {
                X = (int)CalcOffset(Info, PinX, true) + (x & 0x00000FFF),
                Y = (int)CalcOffset(Info, PinY, false) + (y & 0x00000FFF),
                Z = PinZ
            };

            return worldPosition;
        }

        public static uint CalcOffset(Zone_Info Info, ushort Pin, bool x)
        {
            return (uint)Math.Truncate((decimal)(Pin / 4096 + (x ? Info.OffX : Info.OffY))) << 12;
        }

        private static bool IsWorldPositionInsideZone(Zone_Info zone, int worldX, int worldY)
        {
            if (zone == null || worldX <= 0 || worldY <= 0)
                return false;

            int offX = worldX >> 12;
            int offY = worldY >> 12;
            return offX >= zone.OffX && offX < zone.OffX + WorldServer.World.Map.RegionMgr.MaxCells &&
                   offY >= zone.OffY && offY < zone.OffY + WorldServer.World.Map.RegionMgr.MaxCells;
        }
        #endregion

       
    }
}
