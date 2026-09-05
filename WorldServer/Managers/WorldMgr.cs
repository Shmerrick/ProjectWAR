using Common;
using FrameWork;
using GameData;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using SystemData;
using WorldServer.NetWork.Handler;
using WorldServer.Services.World;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Battlefronts.Apocalypse.Loot;
using WorldServer.World.Battlefronts.Keeps;
using WorldServer.World.Interfaces;
using WorldServer.World.Map;
using WorldServer.World.Objects;
using WorldServer.World.Objects.Instances;
using WorldServer.World.Positions;
using WorldServer.World.Scenarios;
using WorldServer.World.Scripting;
using WorldServer.World.WorldSettings;
using BattleFrontConstants = WorldServer.World.Battlefronts.Apocalypse.BattleFrontConstants;
using Item = WorldServer.World.Objects.Item;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.Managers
{
    [Service(
        typeof(AnnounceService),
        typeof(BattleFrontService),
        typeof(BountyService),
        typeof(CellSpawnService),
        typeof(ChapterService),
        typeof(CreatureService),
        typeof(DyeService),
        typeof(GameObjectService),
        typeof(GuildService),
        typeof(ItemService),
        typeof(PQuestService),
        typeof(QuestService),
        typeof(RallyPointService),
        typeof(RVRProgressionService),
        typeof(RewardService),
        typeof(ScenarioService),
        typeof(TokService),
        typeof(VendorService),
        typeof(WaypointService),
        typeof(XpRenownService),
        typeof(ZoneService))]
    public static class WorldMgr
    {
        public static IObjectDatabase Database;
        private static Thread _worldThread;
        private static Thread _groupThread;
        private static bool _running = true;
        public static long StartingPairing;
        // DEV - Development mode, PRD - Production Mode.
        public static string ServerMode;

        public static UpperTierCampaignManager UpperTierCampaignManager;
        public static LowerTierCampaignManager LowerTierCampaignManager;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public static RVRArea RVRArea = new RVRArea();


        //Log.Success("StartingPairing: ", StartingPairing.ToString());

        #region Region

        public static List<RegionMgr> _Regions = new List<RegionMgr>();
        private static ReaderWriterLockSlim RegionsRWLock = new ReaderWriterLockSlim();


        public static RegionMgr GetRegion(ushort RegionId, bool Create, string name = "")
        {
            RegionsRWLock.EnterReadLock();
            RegionMgr Mgr = _Regions.Find(region => region != null && region.RegionId == RegionId);
            RegionsRWLock.ExitReadLock();

            if (Mgr == null && Create)
            {
                Mgr = new RegionMgr(RegionId, ZoneService.GetZoneRegion(RegionId), name, new BattlefrontCommunications());
                RegionsRWLock.EnterWriteLock();
                _Regions.Add(Mgr);
                RegionsRWLock.ExitWriteLock();
                Mgr.StartUpdateThread();
            }

            return Mgr;
        }

        public static void Stop()
        {
            Log.Success("WorldMgr", "Stop");
            foreach (RegionMgr Mgr in _Regions)
                Mgr.Stop();

            ScenarioMgr.Stop();
            _running = false;

        }


        #endregion

        #region Zones

        public static SpawnPoint GetZoneRespawn(ushort zoneId, byte realm, Player player)
        {
            SpawnPoint zoneFallback = ResolveSafeZoneFallback(zoneId, realm);

            if (player == null)
            {
                if (zoneFallback != null)
                    return zoneFallback;

                return GetRealmSafeFallback(realm, $"zone {zoneId}");
            }

            if (player.CurrentArea != null)
            {
                // If player is in a Public Quest
                if (player.QtsInterface.PublicQuest != null)
                {
                    var pqRespawns = ZoneService.GetZoneRespawns(zoneId);
                    if (pqRespawns != null)
                    {
                        foreach (var res in pqRespawns)
                            if (res.Realm == 0 &&
                                res.ZoneID == zoneId &&
                                res.RespawnID == player.QtsInterface.PublicQuest.Info.RespawnID)
                                return new SpawnPoint(res);
                    }
                }

                // Scenario respawn - random if > 1
                if (player.ScnInterface.Scenario != null)
                {
                    List<Zone_Respawn> respawns = ZoneService.GetZoneRespawns(zoneId);
                    List<Zone_Respawn> options = new List<Zone_Respawn>();
                    if (respawns != null)
                    {
                        foreach (Zone_Respawn res in respawns)
                        {
                            if (res.Realm != realm)
                                continue;

                            options.Add(res);
                        }
                    }

                    if (options.Count > 0)
                        return new SpawnPoint(options.Count == 1 ? options[0] : options[StaticRandom.Instance.Next(options.Count)]);
                }

                // Keep respawn.
                if (player.CurrentKeep != null)
                {
                    _logger.Debug($"Player {player.Name} is attached to keep {player.CurrentKeep.Name} - using Keep respawn");
                    SpawnPoint keepRespawn = player.CurrentKeep.GetSpawnPoint(player);
                    if (keepRespawn != null)
                        return keepRespawn;
                }

                ushort respawnId = realm == 1
                    ? player.CurrentArea.OrderRespawnId
                    : player.CurrentArea.DestroRespawnId;

                if (respawnId > 0)
                {
                    // PVE respawn
                    Zone_Respawn areaRespawn = ZoneService.GetZoneRespawn(respawnId);
                    if (areaRespawn != null)
                    {
                        _logger.Debug($"Player {player.Name} Area:{player.CurrentArea.AreaId} PVE respawn {respawnId}");
                        return new SpawnPoint(areaRespawn);
                    }

                    _logger.Warn($"Respawning player {player.Name} from missing respawnId={respawnId} area {player.CurrentArea.AreaId}");
                }
                else
                {
                    _logger.Warn($"Respawning player {player.Name} from respawnId=0 area {player.CurrentArea.AreaId}");
                }
            }
            else
            {
                // World Spawns
                List<Zone_Respawn> respawns = ZoneService.GetZoneRespawns(zoneId);
                float lastDistance = float.MaxValue;
                SpawnPoint nearestRespawn = null;

                if (respawns != null)
                {
                    foreach (Zone_Respawn res in respawns)
                    {
                        if (res.Realm != realm)
                            continue;

                        var pos = new Point3D(res.PinX, res.PinY, res.PinZ);
                        float distance = pos.GetDistance(player);

                        if (distance < lastDistance)
                        {
                            lastDistance = distance;
                            nearestRespawn = new SpawnPoint(res);
                        }
                    }
                }

                if (nearestRespawn != null)
                {
                    _logger.Debug($"Player {player.Name} Zone {zoneId} World respawn");
                    return nearestRespawn;
                }

                _logger.Warn($"Respawning player {player.Name} from NULL area");
            }

            if (zoneFallback != null)
            {
                _logger.Warn($"Respawning player {player.Name} via safe fallback {zoneFallback}");
                return zoneFallback;
            }

            return GetRealmSafeFallback(realm, $"zone {zoneId}");
        }

        public static Zone_jump GetDungeonLoginDestination(ushort dungeonZoneId, byte realm)
        {
            if (realm != (byte)Realms.REALMS_REALM_ORDER && realm != (byte)Realms.REALMS_REALM_DESTRUCTION)
                return null;

            if (InstanceService._InstanceInfo.TryGetValue(dungeonZoneId, out Instance_Info instance))
            {
                uint exitId = realm == (byte)Realms.REALMS_REALM_ORDER
                    ? instance.OrderExitZoneJumpID : instance.DestrExitZoneJumpID;
                Zone_jump exit = ZoneService.GetZoneJump(exitId);
                if (IsSafeDungeonLoginDestination(exit))
                    return exit;
            }

            // User-requested recovery policy: missing/unusable dungeon exit -> own capital.
            // Reuse configured realm respawns; never synthesize an exit or resolve jump 0.
            ushort capitalZoneId = realm == (byte)Realms.REALMS_REALM_DESTRUCTION ? (ushort)161 : (ushort)162;
            Zone_Respawn respawn = ZoneService.GetZoneRespawn(capitalZoneId, realm);
            if (respawn == null || respawn.Realm != realm)
                return null;

            SpawnPoint capital = new SpawnPoint(respawn);
            var destination = new Zone_jump
            {
                ZoneID = capital.ZoneId, WorldX = (uint)capital.X, WorldY = (uint)capital.Y,
                WorldZ = (ushort)capital.Z, WorldO = respawn.WorldO
            };
            return destination.ZoneID == capitalZoneId && IsSafeDungeonLoginDestination(destination) ? destination : null;
        }

        private static bool IsSafeDungeonLoginDestination(Zone_jump destination)
        {
            if (destination == null || destination.Type != 0)
                return false;

            Zone_Info zone = ZoneService.GetZone_Info(destination.ZoneID);
            return zone != null && zone.Type == 0 && destination.WorldZ > 0 &&
                IsWorldPositionInsideZone(zone, (int)destination.WorldX, (int)destination.WorldY);
        }

        public static bool NormalizePlayerWorldPosition(Player player, out string reason)
        {
            if (player?.Info?.Value == null)
            {
                reason = null;
                return false;
            }

            return NormalizeCharacterWorldPosition(player.Info.Value, (byte)player.Realm, player.Name, out reason);
        }

        public static bool NormalizeCharacterWorldPosition(Character_value value, byte realm, string subjectName, out string reason)
        {
            reason = null;
            if (value == null)
                return false;

            string label = string.IsNullOrWhiteSpace(subjectName)
                ? $"character {value.CharacterId}"
                : subjectName;

            Zone_Info zone = ZoneService.GetZone_Info(value.ZoneId);
            if (zone == null)
            {
                reason = $"unknown zone {value.ZoneId}";
                return ApplySafeLocation(value, realm, label, ResolveSafeZoneFallback(0, realm), reason);
            }

            if (value.RegionId != zone.Region)
            {
                value.RegionId = zone.Region;
                reason = $"corrected region to {zone.Region}";
            }

            if (zone.Type != 0)
                return reason != null;

            if (!IsWorldPositionInsideZone(zone, value.WorldX, value.WorldY))
            {
                string localPinReason;
                if (TryConvertLocalPinCoordinatesToWorld(value, zone, out localPinReason))
                {
                    reason = CombineReasons(reason, localPinReason);
                    return true;
                }

                reason = CombineReasons(reason, $"invalid coordinates {value.WorldX},{value.WorldY} for zone {zone.ZoneId}");
                return ApplySafeLocation(value, realm, label, ResolveSafeZoneFallback(zone.ZoneId, realm), reason);
            }

            ushort pinX;
            ushort pinY;
            if (!TryGetPinFromWorld(zone, value.WorldX, value.WorldY, out pinX, out pinY))
            {
                reason = CombineReasons(reason, $"unable to derive pins from {value.WorldX},{value.WorldY} in zone {zone.ZoneId}");
                return ApplySafeLocation(value, realm, label, ResolveSafeZoneFallback(zone.ZoneId, realm), reason);
            }

            int terrainZ = ClientFileMgr.GetHeight(zone.ZoneId, pinX, pinY);
            if (terrainZ >= 0 && (value.WorldZ <= 0 || Math.Abs(value.WorldZ - terrainZ) > 12000))
            {
                value.WorldZ = terrainZ;
                reason = CombineReasons(reason, $"snapped Z to terrain {terrainZ}");

                return true;
            }

            return reason != null;
        }

        private static bool ApplySafeLocation(Character_value value, byte realm, string subjectName, SpawnPoint fallback, string reason)
        {
            SpawnPoint safeFallback = fallback ?? GetRealmSafeFallback(realm, reason);
            Zone_Info zone = ZoneService.GetZone_Info(safeFallback.ZoneId);

            value.WorldX = safeFallback.X;
            value.WorldY = safeFallback.Y;
            value.WorldZ = safeFallback.Z;
            value.WorldO = 0;
            value.ZoneId = safeFallback.ZoneId;
            if (zone != null)
                value.RegionId = zone.Region;

            _logger.Warn($"Recovered {subjectName} to {safeFallback} because {reason}");
            return true;
        }

        private static bool TryConvertLocalPinCoordinatesToWorld(Character_value value, Zone_Info zone, out string reason)
        {
            reason = null;

            if (zone == null || value.WorldX < 0 || value.WorldX > UInt16.MaxValue || value.WorldY < 0 || value.WorldY > UInt16.MaxValue)
                return false;

            ushort pinX = (ushort)value.WorldX;
            ushort pinY = (ushort)value.WorldY;
            ushort pinZ = value.WorldZ <= 0
                ? (ushort)0
                : (ushort)Math.Min(value.WorldZ, UInt16.MaxValue);

            Point3D world = ZoneService.GetWorldPosition(zone, pinX, pinY, pinZ);
            if (!IsWorldPositionInsideZone(zone, world.X, world.Y))
                return false;

            value.WorldX = world.X;
            value.WorldY = world.Y;

            int terrainZ = ClientFileMgr.GetHeight(zone.ZoneId, pinX, pinY);
            if (terrainZ >= 0 && (value.WorldZ <= 0 || Math.Abs(value.WorldZ - terrainZ) > 12000))
                value.WorldZ = terrainZ;

            reason = $"converted local pins {pinX},{pinY} to world {world.X},{world.Y}";
            if (terrainZ >= 0 && value.WorldZ == terrainZ)
                reason += $" z={terrainZ}";

            return true;
        }

        private static string CombineReasons(string existingReason, string nextReason)
        {
            if (string.IsNullOrWhiteSpace(existingReason))
                return nextReason;

            if (string.IsNullOrWhiteSpace(nextReason))
                return existingReason;

            return existingReason + "; " + nextReason;
        }

        private static SpawnPoint ResolveSafeZoneFallback(ushort zoneId, byte realm)
        {
            SpawnPoint fallback = TryGetZoneRespawnFallback(zoneId, realm);
            if (fallback != null)
                return fallback;

            fallback = TryGetZoneTaxiFallback(zoneId, realm);
            if (fallback != null)
                return fallback;

            fallback = TryGetRallyPointFallback(zoneId);
            if (fallback != null)
                return fallback;

            fallback = TryGetChapterFallback(zoneId);
            if (fallback != null)
                return fallback;

            fallback = TryGetZoneJumpFallback(zoneId);
            if (fallback != null)
                return fallback;

            fallback = TryGetZoneCenterFallback(zoneId);
            if (fallback != null)
                return fallback;

            return zoneId == 0 ? null : GetRealmSafeFallback(realm, $"zone {zoneId}");
        }

        private static SpawnPoint TryGetZoneRespawnFallback(ushort zoneId, byte realm)
        {
            if (zoneId == 0)
                return null;

            Zone_Respawn respawn = ZoneService.GetZoneRespawn(zoneId, realm);
            return respawn != null ? new SpawnPoint(respawn) : null;
        }

        private static SpawnPoint TryGetZoneTaxiFallback(ushort zoneId, byte realm)
        {
            if (zoneId == 0)
                return null;

            Zone_Taxi taxi = ZoneService.GetZoneTaxi(zoneId, realm);
            if (IsValidTaxi(taxi))
                return new SpawnPoint(zoneId, (int)taxi.WorldX, (int)taxi.WorldY, taxi.WorldZ);

            for (byte candidateRealm = (byte)Realms.REALMS_REALM_ORDER; candidateRealm <= (byte)Realms.REALMS_REALM_DESTRUCTION; ++candidateRealm)
            {
                taxi = ZoneService.GetZoneTaxi(zoneId, candidateRealm);
                if (IsValidTaxi(taxi))
                    return new SpawnPoint(zoneId, (int)taxi.WorldX, (int)taxi.WorldY, taxi.WorldZ);
            }

            return null;
        }

        private static bool IsValidTaxi(Zone_Taxi taxi)
        {
            return taxi != null && taxi.Enable && taxi.WorldX > 0 && taxi.WorldY > 0;
        }

        private static SpawnPoint TryGetRallyPointFallback(ushort zoneId)
        {
            if (zoneId == 0 || RallyPointService.RallyPoints == null)
                return null;

            foreach (RallyPoint rallyPoint in RallyPointService.RallyPoints)
            {
                if (rallyPoint.ZoneID != zoneId || rallyPoint.WorldX == 0 || rallyPoint.WorldY == 0)
                    continue;

                return new SpawnPoint(zoneId, (int)rallyPoint.WorldX, (int)rallyPoint.WorldY, rallyPoint.WorldZ);
            }

            return null;
        }

        private static SpawnPoint TryGetChapterFallback(ushort zoneId)
        {
            if (zoneId == 0)
                return null;

            Zone_Info zone = ZoneService.GetZone_Info(zoneId);
            if (zone == null)
                return null;

            List<Chapter_Info> chapters = ChapterService.GetChapters(zoneId);
            if (chapters == null)
                return null;

            foreach (Chapter_Info chapter in chapters)
            {
                int height = ClientFileMgr.GetHeight(zone.ZoneId, chapter.PinX, chapter.PinY);
                if (height < 0)
                    continue;

                Point3D world = ZoneService.GetWorldPosition(zone, chapter.PinX, chapter.PinY, (ushort)height);
                if (world.X <= 0 || world.Y <= 0 || world.Z < 0)
                    continue;

                return new SpawnPoint(zoneId, world.X, world.Y, world.Z);
            }

            return null;
        }

        private static SpawnPoint TryGetZoneJumpFallback(ushort zoneId)
        {
            if (zoneId == 0 || ZoneService.Zone_Jumps == null)
                return null;

            SpawnPoint internalFallback = null;
            foreach (Zone_jump jump in ZoneService.Zone_Jumps.Values)
            {
                if (jump.ZoneID != zoneId || !jump.Enabled || jump.WorldX == 0 || jump.WorldY == 0)
                    continue;

                GameObject_spawn sourceSpawn = null;
                bool hasSourceSpawn = GameObjectService.GameObjectSpawns != null && GameObjectService.GameObjectSpawns.TryGetValue(jump.Entry, out sourceSpawn);
                if (hasSourceSpawn && sourceSpawn.ZoneId != zoneId)
                    return new SpawnPoint(zoneId, (int)jump.WorldX, (int)jump.WorldY, jump.WorldZ);

                if (internalFallback == null)
                    internalFallback = new SpawnPoint(zoneId, (int)jump.WorldX, (int)jump.WorldY, jump.WorldZ);
            }

            return internalFallback;
        }

        private static SpawnPoint TryGetZoneCenterFallback(ushort zoneId)
        {
            if (zoneId == 0)
                return null;

            Zone_Info zone = ZoneService.GetZone_Info(zoneId);
            if (zone == null)
                return null;

            ushort[,] pins =
            {
                { 32768, 32768 },
                { 28672, 32768 },
                { 36864, 32768 },
                { 32768, 28672 },
                { 32768, 36864 },
                { 28672, 28672 },
                { 36864, 28672 },
                { 28672, 36864 },
                { 36864, 36864 }
            };

            for (int index = 0; index < pins.GetLength(0); ++index)
            {
                ushort pinX = pins[index, 0];
                ushort pinY = pins[index, 1];
                int height = ClientFileMgr.GetHeight(zone.ZoneId, pinX, pinY);
                if (height < 0)
                    continue;

                Point3D world = ZoneService.GetWorldPosition(zone, pinX, pinY, (ushort)height);
                if (world.X <= 0 || world.Y <= 0 || world.Z < 0)
                    continue;

                return new SpawnPoint(zoneId, world.X, world.Y, world.Z);
            }

            return null;
        }

        private static SpawnPoint GetRealmSafeFallback(byte realm, string reason)
        {
            ushort fallbackZoneId = realm == (byte)Realms.REALMS_REALM_DESTRUCTION ? (ushort)161 : (ushort)162;

            SpawnPoint fallback = TryGetZoneRespawnFallback(fallbackZoneId, realm);
            if (fallback == null)
                fallback = TryGetZoneTaxiFallback(fallbackZoneId, realm);

            if (fallback == null)
                fallback = TryGetZoneJumpFallback(fallbackZoneId);

            if (fallback == null)
            {
                fallback = realm == (byte)Realms.REALMS_REALM_DESTRUCTION
                    ? new SpawnPoint(161, 442630, 127892, 17396)
                    : new SpawnPoint(162, 116235, 147790, 14121);
            }

            _logger.Warn($"Using realm fallback {fallback} because {reason}");
            return fallback;
        }

        private static bool IsWorldPositionInsideZone(Zone_Info zone, int worldX, int worldY)
        {
            if (zone == null || worldX <= 0 || worldY <= 0)
                return false;

            int offX = worldX >> 12;
            int offY = worldY >> 12;
            return offX >= zone.OffX && offX < zone.OffX + RegionMgr.MaxCells &&
                   offY >= zone.OffY && offY < zone.OffY + RegionMgr.MaxCells;
        }

        private static bool TryGetPinFromWorld(Zone_Info zone, int worldX, int worldY, out ushort pinX, out ushort pinY)
        {
            pinX = 0;
            pinY = 0;

            if (zone == null || !IsWorldPositionInsideZone(zone, worldX, worldY))
                return false;

            int localX = worldX - (zone.OffX << 12);
            int localY = worldY - (zone.OffY << 12);
            if (localX < 0 || localX > UInt16.MaxValue || localY < 0 || localY > UInt16.MaxValue)
                return false;

            pinX = (ushort)localX;
            pinY = (ushort)localY;
            return true;
        }

        public static List<Zone_Taxi> GetTaxis(Player Plr)
        {
            List<Zone_Taxi> L = new List<Zone_Taxi>();

            foreach (KeyValuePair<ushort, Zone_Taxi[]> Kp in ZoneService._Zone_Taxi)
            {
                Zone_Taxi[] taxis = Kp.Value;
                Zone_Taxi taxi = taxis[(byte)Plr.Realm];

                if (taxi == null || taxi.WorldX == 0)
                    continue;

                if (taxi.Info == null)
                    taxi.Info = ZoneService.GetZone_Info(taxi.ZoneID);

                if (taxi.Info == null)
                    continue;

                if (!taxi.Enable)
                    continue;

                // The Land of the Dead destination is always listed. Whether it can be used is
                // carried by the availability byte in the flight packet, which is what the
                // client's disabled-button-plus-requirements-tooltip design expects; see
                // LotdService.IsTaxiAvailable. F_FLIGHT re-checks before moving anyone.

                // LOTD visibility is controlled by LotdService; the generic tier token unlocks
                // would otherwise hide the taxi again even after a realm wins access.
                if (taxi.Tier > 0 && taxi.ZoneID != LotdService.LotdZoneId)
                {
                    switch (taxi.Tier)
                    {
                        case 2:
                            if (!(Plr.TokInterface.HasTok(11) || Plr.TokInterface.HasTok(44) || Plr.TokInterface.HasTok(75) || Plr.TokInterface.HasTok(140) || Plr.TokInterface.HasTok(171) || Plr.TokInterface.HasTok(107)))
                                continue;
                            break;
                        case 3:
                            if (!(Plr.TokInterface.HasTok(12) || Plr.TokInterface.HasTok(50) || Plr.TokInterface.HasTok(81) || Plr.TokInterface.HasTok(108) || Plr.TokInterface.HasTok(146) || Plr.TokInterface.HasTok(177)))
                                continue;
                            break;
                        case 4:
                            if (!(Plr.TokInterface.HasTok(18) || Plr.TokInterface.HasTok(55) || Plr.TokInterface.HasTok(86) || Plr.TokInterface.HasTok(114) || Plr.TokInterface.HasTok(182) || Plr.TokInterface.HasTok(151)))
                                continue;
                            break;
                    }
                }

                L.Add(taxi);
            }

            return L;
        }



        #endregion

        #region Xp / Renown

        public static uint GenerateXPCount(Player plr, Unit victim)
        {
            uint KLvl = plr.AdjustedLevel;
            uint VLvl = victim.AdjustedLevel;

            if (KLvl > VLvl + 8)
                    return 0;

            uint XP = VLvl * 100;

            if (victim is Creature)
            {
                switch (victim.Rank)
                {
                    case 1:
                        XP *= 4; break;
                    case 2:
                        if (plr.WorldGroup != null)
                            XP *= 12;
                        break;
                }
            }

            if (KLvl > VLvl)
                XP -= (uint)((XP / (float)100) * (KLvl - VLvl + 1)) * 5;

            if (Program.Config.XpRate > 0)
                XP *= (uint)Program.Config.XpRate;

            return XP;
        }

        public static void GenerateXP(Player killer, Unit victim, float bonusMod)
        {
            _logger.Trace($"Killer : {killer.Name} Victim : {victim.Name} Bonus : {bonusMod}");
            if (killer == null) return;

            if (killer.Level != killer.EffectiveLevel)
                bonusMod = 0.0f;

            if (killer.PriorityGroup == null)
            {

                killer.AddXp((uint)(GenerateXPCount(killer, victim) * bonusMod), true, true);
            }
            else
            {
                _logger.Trace($"Priority Group : {killer.Name} Victim : {victim.Name} Bonus : {bonusMod}");
                killer.PriorityGroup.AddXpFromKill(killer, victim, bonusMod);
            }
        }

        public static uint GenerateRenownCount(Player killer, Player victim)
        {
            if (killer == null || victim == null || killer == victim)
                return 0;

            uint renownPoints = (uint)(7.31f * (victim.AdjustedRenown + victim.AdjustedLevel));

            if (killer.AdjustedLevel > 15 && killer.AdjustedLevel < 31)
                renownPoints = (uint)(renownPoints * 1.0f);

            return renownPoints;
        }

        #endregion

        #region items


        public static void SendDynamicVendorItems(Player plr, List<Vendor_items> items)
        {
            if (plr == null)
                return;


            byte Page = 0;
            int Count = items.Count;
            while (Count > 0)
            {
                byte ToSend = (byte)Math.Min(Count, VendorService.MAX_ITEM_PAGE);
                if (ToSend <= Count)
                    Count -= ToSend;
                else
                    Count = 0;

                WorldMgr.SendVendorPage(plr, ref items, ToSend, Page);

                ++Page;
            }
            plr.ItmInterface.SendBuyBack();
        }

        public static void SendVendor(Player Plr, ushort id)
        {
            if (Plr == null)
                return;

            //guildrank check
            List<Vendor_items> Itemsprecheck = VendorService.GetVendorItems(id).ToList();
            List<Vendor_items> Items = new List<Vendor_items>();

            foreach (Vendor_items vi in Itemsprecheck)
            {
                if (vi.ReqGuildlvl > 0 && Plr.GldInterface.IsInGuild() && vi.ReqGuildlvl > Plr.GldInterface.Guild.Info.Level)
                    continue;
                Items.Add(vi);
            }


            byte Page = 0;
            int Count = Items.Count;
            while (Count > 0)
            {
                byte ToSend = (byte)Math.Min(Count, VendorService.MAX_ITEM_PAGE);
                if (ToSend <= Count)
                    Count -= ToSend;
                else
                    Count = 0;

                SendVendorPage(Plr, ref Items, ToSend, Page);

                ++Page;
            }

            Plr.ItmInterface.SendBuyBack();
        }
        public static void SendVendorPage(Player Plr, ref List<Vendor_items> items, byte Count, byte Page)
        {
            Count = (byte)Math.Min(Count, items.Count);

            PacketOut Out = new PacketOut((byte)Opcodes.F_INIT_STORE, 256);
            Out.WriteByte(3);
            Out.WriteByte(0);
            Out.WriteByte(Page);
            Out.WriteByte(Count);
            Out.WriteByte((byte)(Page > 0 ? 0 : 1));
            Out.WriteByte(1);
            Out.WriteByte(0);

            if (Page == 0)
                Out.WriteByte(0);

            for (byte i = 0; i < Count; ++i)
            {
                Out.WriteByte(i);
                Out.WriteByte(1);
                Out.WriteUInt32(items[i].Price);
                Item.BuildItem(ref Out, null, items[i].Info, null, 0, 1);

                if (Plr != null && Plr.ItmInterface != null && items[i].Info != null && items[i].Info.ItemSet != 0)
                    Plr.ItmInterface.SendItemSetInfoToPlayer(Plr, items[i].Info.ItemSet);

                if ((byte)items[i].ItemsReq.Count > 0)
                {
                    Out.WriteByte(1);
                    foreach (KeyValuePair<uint, ushort> Kp in items[i].ItemsReq)
                    {
                        Item_Info item = ItemService.GetItem_Info(Kp.Key);
                        Out.WriteUInt32(Kp.Key);
                        Out.WriteUInt16((ushort)item.ModelId);
                        Out.WritePascalString(item.Name);
                        Out.WriteUInt16(Kp.Value);
                    }

                }
                if ((byte)items[i].ItemsReq.Count == 1)
                    Out.Fill(0, 18);
                else if ((byte)items[i].ItemsReq.Count == 2)
                    Out.Fill(0, 9);
                else
                    Out.Fill(0, 1);

            }

            Out.WriteByte(0);
            Plr.SendPacket(Out);

            items.RemoveRange(0, Count);
        }

        public static void BuyItemVendor(Player Plr, InteractMenu Menu, ushort id)
        {
            int Num = (Menu.Page * VendorService.MAX_ITEM_PAGE) + Menu.Num;
            ushort Count = Menu.Packet.GetUint16();
            if (Count == 0)
                Count = 1;

            //guildrank check
            List<Vendor_items> Itemsprecheck = VendorService.GetVendorItems(id).ToList();
            List<Vendor_items> Vendors = new List<Vendor_items>();

            foreach (Vendor_items vi in Itemsprecheck)
            {
                if (vi.ReqGuildlvl > 0 && Plr.GldInterface.IsInGuild() && vi.ReqGuildlvl > Plr.GldInterface.Guild.Info.Level)
                    continue;
                Vendors.Add(vi);
            }

            if (Vendors.Count <= Num)
                return;

            if (!Plr.HasMoney((Vendors[Num].Price) * Count))
            {
                Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_MERCHANT_INSUFFICIENT_MONEY_TO_BUY);
                return;
            }

            foreach (KeyValuePair<uint, ushort> Kp in Vendors[Num].ItemsReq)
            {
                if (!Plr.ItmInterface.HasItemCountInInventory(Kp.Key, (ushort)(Kp.Value * Count)))
                {
                    Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_MERCHANT_FAIL_PURCHASE_REQUIREMENT);
                    return;
                }
            }

            if (Vendors[Num].ReqTokUnlock > 0 && !Plr.TokInterface.HasTok(Vendors[Num].ReqTokUnlock))
                return;

            ItemResult result = Plr.ItmInterface.CreateItem(Vendors[Num].Info, Count);
            if (result == ItemResult.RESULT_OK)
            {
                Plr.RemoveMoney(Vendors[Num].Price * Count);
                foreach (KeyValuePair<uint, ushort> Kp in Vendors[Num].ItemsReq)
                    Plr.ItmInterface.RemoveItems(Kp.Key, (ushort)(Kp.Value * Count));
            }
            else if (result == ItemResult.RESULT_MAX_BAG)
            {
                Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_MERCHANT_INSUFFICIENT_SPACE_TO_BUY);
            }
            else if (result == ItemResult.RESULT_ITEMID_INVALID)
            {

            }
        }

        public static void BuyItemRealmCaptainDynamicVendor(Player plr, InteractMenu Menu, List<Vendor_items> items)
        {
            int Num = (Menu.Page * VendorService.MAX_ITEM_PAGE) + Menu.Num;
            ushort Count = Menu.Packet.GetUint16();
            if (Count == 0)
                Count = 1;

            if (items.Count <= Num)
                return;

            if (RealmCaptainManager.IsPlayerRealmCaptain(plr.CharacterId))
            {
                RealmCaptainManager.ApplyRealmCaptainBuff(plr, items[Num].Info.SpellId);
            }
        }

        private static void BuffAssigned(NewBuff buff)
        {
            var newBuff = buff;
        }

        #endregion

        #region Quests
        // TODO move that to QuestService
        public static void GenerateObjective(Quest_Objectives Obj, Quest Q)
        {
            switch ((Objective_Type)Obj.ObjType)
            {
                case Objective_Type.QUEST_KILL_PLAYERS:
                    {
                        if (Obj.Description.Length < 1)
                            Obj.Description = "Enemy Players";
                    }
                    break;

                case Objective_Type.QUEST_SPEAK_TO:
                    {
                        uint ObjID = 0;
                        uint.TryParse(Obj.ObjID, out ObjID);

                        if (ObjID != 0)
                            Obj.Creature = CreatureService.GetCreatureProto(ObjID);

                        if (Obj.Creature == null)
                        {
                            Obj.Description = "Invalid NPC - " + Obj.Entry + ",ObjId=" + Obj.ObjID;
                        }
                        else
                        {
                            if (Obj.Description == null || Obj.Description.Length <= Obj.Creature.Name.Length)
                                Obj.Description = "Speak to " + Obj.Creature.Name;
                        }

                    }
                    break;

                case Objective_Type.QUEST_USE_GO:
                    {
                        uint ObjID = 0;
                        uint.TryParse(Obj.ObjID, out ObjID);

                        if (ObjID != 0)
                            Obj.GameObject = GameObjectService.GetGameObjectProto(ObjID);

                        if (Obj.GameObject == null)
                        {
                            Obj.Description = "Invalid GameObject - QuestID " + Obj.Entry + ",ObjId=" + Obj.ObjID;
                        }
                        else
                        {
                            if (Obj.Description == null || Obj.Description.Length <= Obj.GameObject.Name.Length)
                                Obj.Description = "Find " + Obj.GameObject.Name;
                        }

                    }
                    break;

                case Objective_Type.QUEST_KILL_MOB:
                    {
                        uint ObjID = 0;
                        uint.TryParse(Obj.ObjID, out ObjID);

                        if (ObjID != 0)
                            Obj.Creature = CreatureService.GetCreatureProto(ObjID);

                        if (Obj.Creature == null)
                        {
                            Obj.Description = "Invalid Creature - QuestID " + Obj.Entry + ",ObjId=" + Obj.ObjID;
                        }
                        else
                        {
                            if (Obj.Description == null || Obj.Description.Length <= Obj.Creature.Name.Length)
                                Obj.Description = "Kill " + Obj.Creature.Name;
                        }

                    }
                    break;

                case Objective_Type.QUEST_KILL_GO:
                    {
                        uint ObjID = 0;
                        uint.TryParse(Obj.ObjID, out ObjID);

                        if (ObjID != 0)
                            Obj.GameObject = GameObjectService.GetGameObjectProto(ObjID);

                        if (Obj.GameObject == null)
                        {
                            Obj.Description = "Invalid GameObject - QuestID " + Obj.Entry + ",ObjId=" + Obj.ObjID;
                        }
                        else
                        {
                            if (Obj.Description == null || Obj.Description.Length <= Obj.GameObject.Name.Length)
                                Obj.Description = "Destroy " + Obj.GameObject.Name;
                        }

                    }
                    break;

                case Objective_Type.QUEST_USE_ITEM:
                case Objective_Type.QUEST_GET_ITEM:
                    {
                        uint ObjID = 0;
                        uint.TryParse(Obj.ObjID, out ObjID);

                        if (ObjID != 0)
                        {
                            Obj.Item = ItemService.GetItem_Info(ObjID);
                            if (Obj.Item == null)
                            {
                                int a = Obj.Quest.Particular.IndexOf("kill the ", StringComparison.OrdinalIgnoreCase);
                                if (a >= 0)
                                {
                                    string[] RestWords = Obj.Quest.Particular.Substring(a + 9).Split(' ');
                                    string Name = RestWords[0] + " " + RestWords[1];
                                    Creature_proto Proto = CreatureService.GetCreatureProtoByName(Name) ?? CreatureService.GetCreatureProtoByName(RestWords[0]);
                                    if (Proto != null)
                                    {
                                        Obj.Item = new Item_Info();
                                        Obj.Item.Entry = ObjID;
                                        Obj.Item.Name = Obj.Description;
                                        Obj.Item.MaxStack = 20;
                                        Obj.Item.ModelId = 531;
                                        ItemService._Item_Info.Add(Obj.Item.Entry, Obj.Item);

                                        Log.Info("WorldMgr", "Creating Quest(" + Obj.Entry + ") Item : " + Obj.Item.Entry + ",  " + Obj.Item.Name + "| Adding Loot to : " + Proto.Name);
                                        /*Creature_loot loot = new Creature_loot();
                                        loot.Entry = Proto.Entry;
                                        loot.ItemId = Obj.Item.Entry;
                                        loot.Info = Obj.Item;
                                        loot.Pct = 0;
                                        GetCreatureSpecificLootFor(Proto.Entry).Add(loot);*/
                                    }
                                }
                            }
                        }
                    }
                    break;

                case Objective_Type.QUEST_WIN_SCENARIO:
                    {
                        ushort ObjID = 0;
                        ushort.TryParse(Obj.ObjID, out ObjID);

                        if (ObjID != 0)
                            Obj.Scenario = ScenarioService.GetScenario_Info(ObjID);

                        if (Obj.Scenario == null)
                            Obj.Description = "Invalid Scenario - QuestID=" + Obj.Entry + ", ObjId=" + Obj.ObjID;
                        else
                            if (Obj.Description == null || Obj.Description.Length <= Obj.Scenario.Name.Length)
                            Obj.Description = "Win " + Obj.Scenario.Name;
                    }
                    break;

                case Objective_Type.QUEST_CAPTURE_BO:
                    {
                        ushort ObjID = 0;
                        ushort.TryParse(Obj.ObjID, out ObjID);

                        if (ObjID != 0)
                        {
                            foreach (List<BattleFront_Objective> boList in BattleFrontService._BattleFrontObjectives.Values)
                            {
                                foreach (BattleFront_Objective bo in boList)
                                {
                                    if (bo.Entry == ObjID)
                                    {
                                        Obj.BattleFrontObjective = bo;
                                        break;
                                    }
                                }

                                if (Obj.BattleFrontObjective != null)
                                    break;
                            }
                        }

                        if (Obj.BattleFrontObjective == null)
                            Obj.Description = "Invalid Battlefield Objective - QuestID=" + Obj.Entry + ", ObjId=" + Obj.ObjID;
                        else
                            if (Obj.Description == null || Obj.Description.Length <= Obj.BattleFrontObjective.Name.Length)
                            Obj.Description = "Capture " + Obj.Scenario.Name;
                    }
                    break;

                case Objective_Type.QUEST_CAPTURE_KEEP:
                    {
                        ushort ObjID = 0;
                        ushort.TryParse(Obj.ObjID, out ObjID);

                        if (ObjID != 0)
                        {
                            foreach (List<Keep_Info> keepList in BattleFrontService._KeepInfos.Values)
                            {
                                foreach (Keep_Info keep in keepList)
                                {
                                    if (keep.KeepId == ObjID)
                                    {
                                        Obj.Keep = keep;
                                        break;
                                    }
                                }

                                if (Obj.Keep != null)
                                    break;
                            }
                        }

                        if (Obj.Keep == null)
                            Obj.Description = "Invalid Keep - QuestID=" + Obj.Entry + ", ObjId=" + Obj.ObjID;
                        else
                            if (Obj.Description == null || Obj.Description.Length <= Obj.Keep.Name.Length)
                            Obj.Description = "Capture " + Obj.Keep.Name;
                    }
                    break;
            }
        }
        #endregion

        #region Relation

        [LoadingFunction(false)]
        public static void LoadRelation()
        {
            Log.Success("LoadRelation", "Loading Relations");

            foreach (Item_Info info in ItemService._Item_Info.Values)
            {
                if (info.Career != 0)
                {
                    foreach (KeyValuePair<byte, CharacterInfo> Kp in CharMgr.CharacterInfos)
                    {
                        if ((info.Career & (1 << (Kp.Value.CareerLine - 1))) == 0)
                            continue;

                        info.Realm = Kp.Value.Realm;
                        break;

                    }
                }

                else if (info.Race > 0)
                {
                    if (((Constants.RaceMaskDwarf + Constants.RaceMaskHighElf + Constants.RaceMaskEmpire) & info.Race) > 0)
                        info.Realm = 1;
                    else info.Realm = 2;
                }
            }

            LoadChapters();
            LoadPublicQuests();
            LoadQuestsRelation();
            LoadScripts(false);

            foreach (List<Keep_Info> keepInfos in BattleFrontService._KeepInfos.Values)
                foreach (Keep_Info keepInfo in keepInfos)
                    if (PQuestService._PQuests.ContainsKey(keepInfo.PQuestId))
                        keepInfo.PQuest = PQuestService._PQuests[keepInfo.PQuestId];

            CharMgr.Database.ExecuteNonQuery($"UPDATE `{CharMgr.Database.GetSchemaName()}`.characters_value SET Online=0;");

            // Preload T4 regions
            Log.Info("Regions", "Preloading pairing regions...");
            // Tier 1
            //GetRegion(1, true, Constants.RegionName[1]); // dw/gs
            //GetRegion(3, true, Constants.RegionName[3]); // he/de
            //GetRegion(8, true, Constants.RegionName[8]); // em/ch

            // Tier 2
            //GetRegion(12, true, Constants.RegionName[12]); // dw/gs
            //GetRegion(15, true, Constants.RegionName[15]); // he/de
            //GetRegion(14, true, Constants.RegionName[14]); // em/ch

            // Tier 3
            //GetRegion(10, true, Constants.RegionName[10]); // dw/gs
            //GetRegion(16, true, Constants.RegionName[16]); // he/de
            //GetRegion(6, true, Constants.RegionName[6]); // em/ch

            // Tier 4
            //GetRegion(2, true, Constants.RegionName[2]); // dw/gs
            //GetRegion(4, true, Constants.RegionName[4]);  // he/de
            //GetRegion(11, true, Constants.RegionName[11]); // em/ch

            // removed for now, as this will also trigger an attempt to load BOs for the region.
            //GetRegion(9, true, Constants.RegionName[9]); // lotd
            Log.Success("Regions", "Preloaded pairing regions.");
        }

        public static void LoadChapters()
        {
            Log.Success("LoadChapters", "Loading Zone from Chapters");

            long InvalidChapters = 0;

            Zone_Info Zone = null;
            Chapter_Info Info;
            foreach (KeyValuePair<uint, Chapter_Info> Kp in ChapterService._Chapters)
            {
                Info = Kp.Value;
                Zone = ZoneService.GetZone_Info(Info.ZoneId);

                if (Zone == null || (Info.PinX <= 0 && Info.PinY <= 0))
                {
                    _logger.Warn("LoadChapters Chapter (" + Info.Entry + ")[" + Info.Name + "] Invalid");
                    ++InvalidChapters;
                }

                if (Info.T1Rewards == null)
                    Info.T1Rewards = new List<Chapter_Reward>();
                if (Info.T2Rewards == null)
                    Info.T2Rewards = new List<Chapter_Reward>();
                if (Info.T3Rewards == null)
                    Info.T3Rewards = new List<Chapter_Reward>();

                List<Chapter_Reward> Rewards;
                if (ChapterService._Chapters_Reward.TryGetValue(Info.Entry, out Rewards))
                {
                    foreach (Chapter_Reward CW in Rewards)
                    {
                        if (Info.Tier1InfluenceCount == CW.InfluenceCount)
                        {
                            Info.T1Rewards.Add(CW);
                        }
                        else if (Info.Tier2InfluenceCount == CW.InfluenceCount)
                        {
                            Info.T2Rewards.Add(CW);
                        }
                        else if (Info.Tier3InfluenceCount == CW.InfluenceCount)
                        {
                            Info.T3Rewards.Add(CW);
                        }
                    }
                }


                foreach (Chapter_Reward Reward in Info.T1Rewards.ToArray())
                {
                    Reward.Item = ItemService.GetItem_Info(Reward.ItemId);
                    Reward.Chapter = Info;

                    if (Reward.Item == null)
                        Info.T1Rewards.Remove(Reward);
                }

                foreach (Chapter_Reward Reward in Info.T2Rewards.ToArray())
                {
                    Reward.Item = ItemService.GetItem_Info(Reward.ItemId);
                    Reward.Chapter = Info;

                    if (Reward.Item == null)
                        Info.T2Rewards.Remove(Reward);
                }
                foreach (Chapter_Reward Reward in Info.T3Rewards.ToArray())
                {
                    Reward.Item = ItemService.GetItem_Info(Reward.ItemId);
                    Reward.Chapter = Info;

                    if (Reward.Item == null)
                        Info.T3Rewards.Remove(Reward);
                }

                CellSpawnService.GetRegionCell(Zone.Region, (ushort)((float)(Info.PinX / 4096) + Zone.OffX), (ushort)((float)(Info.PinY / 4096) + Zone.OffY)).AddChapter(Info);
            }



            if (InvalidChapters > 0)
                _logger.Warn("LoadChapters", "[" + InvalidChapters + "] Invalid Chapter(s)");
        }
        public static void LoadPublicQuests()
        {
            HashSet<uint> keepPQuestIds = new HashSet<uint>();
            foreach (List<Keep_Info> keepInfos in BattleFrontService._KeepInfos.Values)
            {
                foreach (Keep_Info keepInfo in keepInfos)
                {
                    if (keepInfo.PQuestId != 0)
                        keepPQuestIds.Add(keepInfo.PQuestId);
                }
            }

            int keepOwnedPQuestCount = 0;

            foreach (KeyValuePair<uint, PQuest_Info> pair in PQuestService._PQuests)
            {
                PQuest_Info info = pair.Value;
                Zone_Info zone = ZoneService.GetZone_Info(info.ZoneId);
                if (zone == null)
                    continue;

                if (!PQuestService._PQuest_Objectives.TryGetValue(info.Entry, out info.Objectives))
                    info.Objectives = new List<PQuest_Objective>();
                else
                {
                    foreach (PQuest_Objective objective in info.Objectives)
                    {
                        objective.Quest = info;
                        PQuestService.GeneratePQuestObjective(objective, objective.Quest);

                        if (!PQuestService._PQuest_Spawns.TryGetValue(objective.Guid, out objective.Spawns))
                            objective.Spawns = new List<PQuest_Spawn>();
                    }
                }

                if (keepPQuestIds.Contains(pair.Key))
                {
                    keepOwnedPQuestCount++;
                    continue;
                }

                Log.Info("LoadPublicQuests", "Loaded public quest " + info.Entry + " to region " + zone.Region + " cell at X: " + ((float)(info.PinX / 4096) + zone.OffX) + " " + (float)(info.PinY / 4096) + zone.OffY);
                CellSpawnService.GetRegionCell(zone.Region, (ushort)((float)(info.PinX / 4096) + zone.OffX), (ushort)((float)(info.PinY / 4096) + zone.OffY)).AddPQuest(info);
            }

            if (keepOwnedPQuestCount > 0)
                Log.Info("LoadPublicQuests", $"Prepared {keepOwnedPQuestCount} keep-owned PQ metadata records; excluded them from normal cell spawning.");
        }
        public static void LoadQuestsRelation()
        {
            QuestService.LoadQuestCreatureStarter();
            QuestService.LoadQuestCreatureFinisher();

            foreach (KeyValuePair<uint, Creature_proto> Kp in CreatureService.CreatureProtos)
            {
                Kp.Value.StartingQuests = QuestService.GetStartQuests(Kp.Key);
                Kp.Value.FinishingQuests = QuestService.GetFinishersQuests(Kp.Key);
            }

            Quest quest;

            int MaxGuid = 0;
            foreach (KeyValuePair<int, Quest_Objectives> Kp in QuestService._Objectives)
            {
                if (Kp.Value.Guid >= MaxGuid)
                    MaxGuid = Kp.Value.Guid;
            }

            foreach (KeyValuePair<int, Quest_Objectives> Kp in QuestService._Objectives)
            {
                quest = Kp.Value.Quest = QuestService.GetQuest(Kp.Value.Entry);
                if (quest == null)
                    continue;

                quest.Objectives.Add(Kp.Value);
            }

            foreach (Quest_Map Q in QuestService._QuestMaps)
            {
                quest = QuestService.GetQuest(Q.Entry);
                if (quest == null)
                    continue;

                quest.Maps.Add(Q);
            }

            foreach (KeyValuePair<ushort, Quest> Kp in QuestService._Quests)
            {
                quest = Kp.Value;

                if (quest.Objectives.Count == 0)
                {
                    uint Finisher = QuestService.GetQuestCreatureFinisher(quest.Entry);
                    if (Finisher != 0)
                    {
                        Quest_Objectives NewObj = new Quest_Objectives();
                        NewObj.Guid = ++MaxGuid;
                        NewObj.Entry = quest.Entry;
                        NewObj.ObjType = (uint)Objective_Type.QUEST_SPEAK_TO;
                        NewObj.ObjID = Finisher.ToString();
                        NewObj.ObjCount = 1;
                        NewObj.Quest = quest;

                        quest.Objectives.Add(NewObj);
                        QuestService._Objectives.Add(NewObj.Guid, NewObj);

                        Log.Debug("WorldMgr", "Creating Objective for quest with no objectives: " + Kp.Value.Entry + " " + Kp.Value.Name);
                    }
                }
            }

            foreach (KeyValuePair<int, Quest_Objectives> Kp in QuestService._Objectives)
            {
                if (Kp.Value.Quest == null)
                    continue;
                GenerateObjective(Kp.Value, Kp.Value.Quest);
            }

            string sItemID, sCount;
            uint ItemID, Count;
            Item_Info Info;
            foreach (KeyValuePair<ushort, Quest> Kp in QuestService._Quests)
            {
                if (Kp.Value.Choice.Length <= 0)
                    continue;

                // [5154,12],[128,1]
                string[] Rewards = Kp.Value.Choice.Split('[');
                foreach (string Reward in Rewards)
                {
                    if (Reward.Length <= 0)
                        continue;

                    sItemID = Reward.Substring(0, Reward.IndexOf(','));
                    sCount = Reward.Substring(sItemID.Length + 1, Reward.IndexOf(']') - sItemID.Length - 1);

                    ItemID = uint.Parse(sItemID);
                    Count = uint.Parse(sCount);

                    Info = ItemService.GetItem_Info(ItemID);
                    if (Info == null)
                        continue;

                    if (!Kp.Value.Rewards.ContainsKey(Info))
                        Kp.Value.Rewards.Add(Info, Count);
                    else
                        Kp.Value.Rewards[Info] += Count;
                }
            }
        }


        #endregion

        #region Scripts

        public static Dictionary<string, Type> LocalScripts = new Dictionary<string, Type>();
        public static Dictionary<string, AGeneralScript> GlobalScripts = new Dictionary<string, AGeneralScript>();
        public static Dictionary<uint, Type> CreatureScripts = new Dictionary<uint, Type>();
        public static Dictionary<uint, Type> GameObjectScripts = new Dictionary<uint, Type>();
        public static ScriptsInterface GeneralScripts;

        public static void LoadScripts(bool Reload)
        {
            GeneralScripts = new ScriptsInterface();
            GeneralScripts.ClearScripts();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types = null;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types;
                }
                catch (Exception)
                {
                    continue;
                }

                if (types == null)
                    continue;

                foreach (Type type in types)
                {
                    if (type == null || type.IsClass != true)
                        continue;

                    if (!type.IsSubclassOf(typeof(AGeneralScript)))
                        continue;

                    foreach (GeneralScriptAttribute at in type.GetCustomAttributes(typeof(GeneralScriptAttribute), true))
                    {
                        if (!string.IsNullOrEmpty(at.ScriptName))
                            at.ScriptName = at.ScriptName.ToLower();

                        Log.Success("Scripting", "Registering Script :" + at.ScriptName);

                        if (at.GlobalScript)
                        {
                            AGeneralScript Script = Activator.CreateInstance(type) as AGeneralScript;
                            Script.ScriptName = at.ScriptName;
                            GeneralScripts.RemoveScript(Script.ScriptName);
                            GeneralScripts.AddScript(Script);
                            GlobalScripts[at.ScriptName] = Script;
                        }
                        else
                        {
                            if (at.CreatureEntry != 0)
                            {
                                Log.Success("Scripts", "Registering Creature Script :" + at.CreatureEntry);

                                if (!CreatureScripts.ContainsKey(at.CreatureEntry))
                                {
                                    CreatureScripts[at.CreatureEntry] = type;
                                }
                                else
                                {
                                    CreatureScripts[at.CreatureEntry] = type;
                                }
                            }
                            else if (at.GameObjectEntry != 0)
                            {
                                Log.Success("Scripts", "Registering GameObject Script :" + at.GameObjectEntry);

                                if (!GameObjectScripts.ContainsKey(at.GameObjectEntry))
                                {
                                    GameObjectScripts[at.GameObjectEntry] = type;
                                }
                                else
                                {
                                    GameObjectScripts[at.GameObjectEntry] = type;
                                }
                            }
                            else if (!string.IsNullOrEmpty(at.ScriptName))
                            {
                                Log.Success("Scripts", "Registering Name Script :" + at.ScriptName);

                                if (!LocalScripts.ContainsKey(at.ScriptName))
                                {
                                    LocalScripts[at.ScriptName] = type;
                                }
                                else
                                {
                                    LocalScripts[at.ScriptName] = type;
                                }
                            }
                        }
                    }
                }
            }

            Log.Success("Scripting", "Loaded  : " + (GeneralScripts.Scripts.Count + LocalScripts.Count) + " Scripts");

            if (Reload)
            {
                if (Program.Server != null)
                    Program.Server.LoadPacketHandler();
            }
        }

        public static AGeneralScript GetScript(Object Obj, string ScriptName)
        {
            if (!string.IsNullOrEmpty(ScriptName))
            {
                ScriptName = ScriptName.ToLower();

                if (GlobalScripts.ContainsKey(ScriptName))
                    return GlobalScripts[ScriptName];
                if (LocalScripts.ContainsKey(ScriptName))
                {
                    AGeneralScript Script = Activator.CreateInstance(LocalScripts[ScriptName]) as AGeneralScript;
                    Script.ScriptName = ScriptName;
                    return Script;
                }
            }
            else
            {
                if (Obj.IsCreature() && CreatureScripts.ContainsKey(Obj.GetCreature().Spawn.Entry))
                {
                    AGeneralScript Script = Activator.CreateInstance(CreatureScripts[Obj.GetCreature().Spawn.Entry]) as AGeneralScript;
                    Script.ScriptName = Obj.GetCreature().Spawn.Entry.ToString();
                    return Script;
                }

                if (Obj.IsGameObject() && GameObjectScripts.ContainsKey(Obj.GetGameObject().Spawn.Entry))
                {
                    AGeneralScript Script = Activator.CreateInstance(GameObjectScripts[Obj.GetGameObject().Spawn.Entry]) as AGeneralScript;
                    Script.ScriptName = Obj.GetGameObject().Spawn.Entry.ToString();
                    return Script;
                }
            }

            return null;
        }

        public static void UpdateScripts(long Tick)
        {
            GeneralScripts.Update(Tick);
        }

        #endregion

        #region Scenarios
        public static ScenarioMgr ScenarioMgr;

        public static InstanceMgr InstanceMgr;

        [LoadingFunction(true)]
        public static void StartScenarioMgr()
        {
            ScenarioMgr = new ScenarioMgr(ScenarioService.ActiveScenarios);
        }

        [LoadingFunction(true)]
        public static void StartInstanceMgr()
        {
            InstanceMgr = new InstanceMgr();
        }

        #endregion

        #region Settings

        public static WorldSettingsMgr WorldSettingsMgr;

        [LoadingFunction(true)]
        public static void StartWorldSettingsMgr()
        {
            WorldSettingsMgr = new WorldSettingsMgr();
        }

        #endregion



        #region Campaign

        public static void WorldUpdateStart()
        {
            Log.Debug("WorldMgr", "Starting World Monitor...");

            _worldThread = new Thread(WorldUpdate);
            _worldThread.Start();
        }

        public static void GroupUpdateStart()
        {
            Log.Debug("WorldMgr", "Starting Group Updater...");

            _groupThread = new Thread(GroupUpdate);
            _groupThread.Start();
        }


        public static Dictionary<int, int> GetZonesFightLevel()
        {
            var level = new Dictionary<int, int>();
            foreach (var region in WorldMgr._Regions.Where(e => e.Campaign != null).ToList())
            {
                foreach (var zone in region.GetZones())
                {
                    var hotspots = zone.GetHotSpots();
                    if (hotspots.Count > 0)
                        level[zone.ZoneId] = hotspots.Where(e => e.Item2 >= ZoneMgr.LOW_FIGHT).Max(e => e.Item2);
                }
            }
            return level;
        }

        /// <summary>
        /// Show swords on world map if zone has people fighting it
        /// </summary>
        public static void SendZoneFightLevel(Player player = null)
        {
            var fightLevel = GetZonesFightLevel();

            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_HOT_SPOT);
            Out.WriteByte((byte)fightLevel.Count);
            Out.WriteByte(2); //world hotspots
            Out.WriteByte(0);

            //fight level
            uint none = 0x00000000;
            uint small = 0x01000000;
            uint large = 0x01020000;
            uint huge = 0x01020100;

            foreach (var zoneId in fightLevel.Keys)
            {
                Out.WriteByte((byte)zoneId);

                if (fightLevel[zoneId] >= ZoneMgr.LARGE_FIGHT)
                    Out.WriteUInt32(huge);
                else if (fightLevel[zoneId] > ZoneMgr.MEDIUM_FIGHT)
                    Out.WriteUInt32(large);
                else if (fightLevel[zoneId] > ZoneMgr.LOW_FIGHT)
                    Out.WriteUInt32(small);
                else
                    Out.WriteUInt32(none);
            }

            if (player != null)
                player.SendPacket(Out);
            else
            {
                lock (Player._Players)
                {
                    foreach (Player pPlr in Player._Players)
                    {
                        if (pPlr == null || pPlr.IsDisposed || !pPlr.IsInWorld())
                            continue;

                        pPlr.SendCopy(Out);
                    }
                }
            }

            foreach (var region in WorldMgr._Regions.Where(e => e.Campaign != null).ToList())
            {
                foreach (var zone in region.GetZones())
                {
                    zone.SendHotSpots(player);
                }
            }
        }


        private static void WorldUpdate()
        {
            while (_running)
            {
                // This loop owns a bare thread with no caller to catch for it, so anything escaping here
                // terminates the server. It walks every region's zone list, which other threads grow as
                // players enter new zones, and that is precisely how a zone transition used to take the
                // process down. Losing one 15s pass is survivable; losing the server is not.
                try
                {
                    if (ZoneService._Zone_Info != null)
                    {
                        LotdService.Update();
                        SendZoneFightLevel();

                        foreach (var region in WorldMgr._Regions.Where(e => e.Campaign != null).ToList())
                        {
                            foreach (var zone in region.GetZones())
                            {
                                zone.DecayHotspots();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error("WorldMgr", "World update pass failed: " + e);
                }

                Thread.Sleep(15000);
            }

        }


        private static void GroupUpdate()
        {
            while (_running)
            {
                List<Group> _groups = new List<Group>();
                lock (Group.WorldGroups)
                {
                    foreach (Group g in Group.WorldGroups)
                    {
                        try
                        {
                            _groups.Add(g);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
                List<KeyValuePair<uint, GroupAction>> _worldActions = new List<KeyValuePair<uint, GroupAction>>();
                lock (Group._pendingGroupActions)
                {
                    foreach (KeyValuePair<uint, GroupAction> kp in Group._pendingGroupActions)
                    {
                        _worldActions.Add(kp);
                    }
                    Group._pendingGroupActions.Clear();
                }

                foreach (Group g in _groups)
                {
                    try
                    {
                        foreach (KeyValuePair<uint, GroupAction> grpAction in _worldActions)
                        {
                            if (g.GroupId == grpAction.Key)
                                g.EnqueuePendingGroupAction(grpAction.Value);
                        }

                        g.Update(TCPManager.GetTimeStampMS());
                    }
                    catch (Exception e)
                    {
                        Log.Error("Caught exception", "Exception thrown: " + e);
                        continue;
                    }
                }

                _worldActions.Clear();
                _groups.Clear();

                Thread.Sleep(250);
            }

        }



        #endregion

        #region Keep registry, to remove it's static bullshit
        public static Dictionary<uint, BattleFrontKeep> _Keeps = new Dictionary<uint, BattleFrontKeep>();

        public static void SendKeepStatus(Player Plr)
        {
            foreach (List<Keep_Info> list in BattleFrontService._KeepInfos.Values)
            {
                foreach (Keep_Info KeepInfo in list)
                {
                    if (_Keeps.ContainsKey(KeepInfo.KeepId))
                    {
                        _Keeps[KeepInfo.KeepId].KeepCommunications.SendKeepStatus(Plr, _Keeps[KeepInfo.KeepId]);
                    }
                    else
                    {
                        PacketOut Out = new PacketOut((byte)Opcodes.F_KEEP_STATUS, 26);
                        Out.WriteByte(KeepInfo.KeepId);
                        Out.WriteByte(1); // status
                        Out.WriteByte(0); // ?
                        Out.WriteByte(KeepInfo.Realm);
                        Out.WriteByte(KeepInfo.DoorCount);
                        Out.WriteByte(0); // Rank
                        Out.WriteByte(100); // Door health
                        Out.WriteByte(0); // Next rank %
                        Out.Fill(0, 18);
                        Plr.SendPacket(Out);
                    }
                }
            }
        }
        #endregion

        #region Logging
        [LoadingFunction(true)]
        public static void ResetPacketLogSettings()
        {
            //turn off user specific packet logging when server restarts. This is because devs/gm forget to turn it off and log file grows > 20GB
            Log.Debug("WorldMgr", "Resetting user packet log settings...");
            Database.ExecuteNonQuery($"UPDATE `{Program.AcctMgr.GetAccountSchemaName()}`.accounts SET PacketLog=0");
        }
        #endregion

        #region Other


        #endregion

        public static void AttachCampaignsToRegions()
        {
            // Ensure all campaign-relevant regions exist before attaching campaigns.
            // Regions 1,3,8 (T1) and 2,4,11 (T4) are not preloaded at startup, so
            // ResolveActiveRegion() would otherwise lazy-load them after attachment
            // runs, leaving them without a campaign and triggering a warn+reattach
            // on every boot.
            ushort[] campaignRegionIds = { 1, 3, 8, 2, 4, 11 };
            foreach (var id in campaignRegionIds)
            {
                if (id < Constants.RegionName.Length)
                    GetRegion(id, true, Constants.RegionName[id]);
                else
                    GetRegion(id, true);
            }

            foreach (var regionMgr in _Regions)
            {
                if (regionMgr?.Campaign != null)
                    continue;

                var objectiveList = LoadObjectives(regionMgr);
                switch (regionMgr.RegionId)
                {
                    case 1: // t1 dw/gs
                        regionMgr.Campaign = new Campaign(regionMgr, objectiveList, new HashSet<Player>(), WorldMgr.LowerTierCampaignManager, new BattlefrontCommunications());
                        break;
                    case 3: // t1 he/de
                        regionMgr.Campaign = new Campaign(regionMgr, objectiveList, new HashSet<Player>(), WorldMgr.LowerTierCampaignManager, new BattlefrontCommunications());
                        break;
                    case 8: // t1 em/ch
                        regionMgr.Campaign = new Campaign(regionMgr, objectiveList, new HashSet<Player>(), WorldMgr.LowerTierCampaignManager, new BattlefrontCommunications());
                        break;
                    // Tier 4
                    case 11:
                        regionMgr.Campaign = new Campaign(regionMgr, objectiveList, new HashSet<Player>(), WorldMgr.UpperTierCampaignManager, new BattlefrontCommunications());
                        break;
                    case 2:
                        regionMgr.Campaign = new Campaign(regionMgr, objectiveList, new HashSet<Player>(), WorldMgr.UpperTierCampaignManager, new BattlefrontCommunications());
                        break;
                    case 4:
                        regionMgr.Campaign = new Campaign(regionMgr, objectiveList, new HashSet<Player>(), WorldMgr.UpperTierCampaignManager, new BattlefrontCommunications());
                        break;

                    default:
                        break;
                }
                regionMgr.StartUpdateThread();
            }
        }

        public static List<BattlefieldObjective> LoadObjectives(RegionMgr regionMgr)
        {
            List<BattleFront_Objective> objectives = BattleFrontService.GetBattleFrontObjectives(regionMgr.RegionId);
            if (objectives == null)
            {
                _logger.Warn($"Region = {regionMgr.RegionId} has no objectives");
                return null;
            }
            var resultList = new List<BattlefieldObjective>();
            _logger.Debug($"Region = {regionMgr.RegionId} ObjectiveCount = {objectives.Count}");
            foreach (BattleFront_Objective obj in objectives.Where(x => x.KeepSpawn == false))
            {
                BattlefieldObjective flag = new BattlefieldObjective(regionMgr, obj);
                resultList.Add(flag);
            }

            return resultList;
        }

        /// <summary>
        /// Inform the server of the change in the RVR Progression across all regions.
        /// </summary>
        public static void UpdateRegionCaptureStatus(LowerTierCampaignManager lowerTierCampaignManager, UpperTierCampaignManager upperTierCampaignManager)
        {
            if ((lowerTierCampaignManager == null) || (upperTierCampaignManager == null))
                return;
            _logger.Trace("F_CAMPAIGN_STATUS1");
            PacketOut Out = new PacketOut((byte)Opcodes.F_CAMPAIGN_STATUS, 159);
            Out.WriteHexStringBytes("0005006700CB00"); // 7


            // Dwarfs vs Greenskins T1
            Out.WriteByte((byte)lowerTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER1_EKRUND).LockStatus);
            Out.WriteByte((byte)lowerTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER1_EKRUND).OrderVictoryPointPercentage);  // % Order lock
            Out.WriteByte((byte)lowerTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER1_EKRUND).DestructionVictoryPointPercentage);    // % Dest lock
            // Dwarfs vs Greenskins T2
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(12, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);  // % Order lock
            Out.WriteByte(0);    // % Dest lock
            // Dwarfs vs Greenskins T3
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(10, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);  // % Order lock
            Out.WriteByte(0);    // % Dest lock
            // Dwarfs vs Greenskins T4
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(2, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);  // % Order lock
            Out.WriteByte(0);    // % Dest lock
            // Empire vs Chaos T1
            Out.WriteByte((byte)lowerTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER1_NORDLAND).LockStatus);
            Out.WriteByte((byte)lowerTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER1_NORDLAND).OrderVictoryPointPercentage);  // % Order lock
            Out.WriteByte((byte)lowerTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER1_NORDLAND).DestructionVictoryPointPercentage);    // % Dest lock
            // Empire vs Chaos T2
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(14, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);  // % Order lock
            Out.WriteByte(0);    // % Dest lock
            // Empire vs Chaos T3
            // BuildCaptureStatus(Out, WorldMgr.GetRegion(6, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(45);  // % Order lock
            Out.WriteByte(55);    // % Dest lock
            // Empire vs Chaos T4
            // BuildCaptureStatus(Out, WorldMgr.GetRegion(11, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(40);  // % Order lock
            Out.WriteByte(60);    // % Dest lock
            // High Elves vs Dark Elves T1
            Out.WriteByte((byte)lowerTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER1_CHRACE).LockStatus);
            Out.WriteByte((byte)lowerTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER1_CHRACE).OrderVictoryPointPercentage);  // % Order lock
            Out.WriteByte((byte)lowerTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER1_CHRACE).DestructionVictoryPointPercentage);    // % Dest lock
            // High Elves vs Dark Elves T2
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(15, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);  // % Order lock
            Out.WriteByte(0);    // % Dest lock
            // High Elves vs Dark Elves T3
            // BuildCaptureStatus(Out, WorldMgr.GetRegion(16, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);  // % Order lock
            Out.WriteByte(0);    // % Dest lock
            // High Elves vs Dark Elves T4
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(4, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);  // % Order lock
            Out.WriteByte(0);    // % Dest lock

            Out.Fill(0, 83);

            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER4_STONEWATCH).LockStatus);  //  Dwarf Fort
            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER4_KADRIN_VALLEY).LockStatus);  // (ZONE_STATUS_ORDER_LOCKED/ZONE_STATUS_DESTRO_LOCKED)
            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER4_THUNDER_MOUNTAIN).LockStatus);
            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER4_BLACK_CRAG).LockStatus);
            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER4_BUTCHERS_PASS).LockStatus);   // greenskin Fort

            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER4_REIKWALD).LockStatus);// Empire Fort
            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER4_REIKLAND).LockStatus);
            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER4_PRAAG).LockStatus);
            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER4_CHAOS_WASTES).LockStatus);
            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER4_THE_MAW).LockStatus);  // Chaos Fort

            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER4_SHINING_WAY).LockStatus);   //elf fortress
            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER4_EATAINE).LockStatus);  // (ZONE_STATUS_ORDER_LOCKED/ZONE_STATUS_DESTRO_LOCKED)
            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER4_DRAGONWAKE).LockStatus);
            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER4_CALEDOR).LockStatus);
            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER4_FELL_LANDING).LockStatus);   //Dark elf Fortress

            Out.WriteByte(0); // Order underdog rating
            Out.WriteByte(0); // Destruction underdog rating


            /*Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(0);

            Out.WriteByte(00);

            Out.Fill(0, 4);*/

            //For debugging purposes
            var lockStr = upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER4_BLACK_CRAG).LockStatus.ToString();
            lockStr += upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER4_THUNDER_MOUNTAIN).LockStatus.ToString();
            lockStr += upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER4_KADRIN_VALLEY).LockStatus.ToString();
            lockStr += upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER4_CHAOS_WASTES).LockStatus.ToString();
            lockStr += upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER4_PRAAG).LockStatus.ToString();
            lockStr += upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER4_REIKLAND).LockStatus.ToString();
            lockStr += upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER4_CALEDOR).LockStatus.ToString();
            lockStr += upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER4_DRAGONWAKE).LockStatus.ToString();
            lockStr += upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER4_EATAINE).LockStatus.ToString();

            byte[] buffer = Out.ToArray();
            _logger.Trace("WorldMgr : " + lockStr);

            lock (Player._Players)
            {
                foreach (Player player in Player._Players)
                {
                    if (player == null || player.IsDisposed || !player.IsInWorld())
                        continue;

                    PacketOut playerCampaignStatus = new PacketOut(0, 159) { Position = 0 };
                    playerCampaignStatus.Write(buffer, 0, buffer.Length);

                    if (player.Region?.Campaign != null)
                    {
                        playerCampaignStatus.WriteByte((byte)player.Region.Campaign.VictoryPointProgress.OrderVictoryPointPercentage);
                        playerCampaignStatus.WriteByte((byte)player.Region.Campaign.VictoryPointProgress.DestructionVictoryPointPercentage);
                    }
                    else
                    {
                        playerCampaignStatus.Fill(0, 9);
                    }
                    playerCampaignStatus.Fill(0, 4);

                    player.SendPacket(playerCampaignStatus);
                }
            }
        }

        public static void BuyItemBlackMarketVendor(Player plr, InteractMenu menu, List<Vendor_items> items)
        {
            int Num = (menu.Page * VendorService.MAX_ITEM_PAGE) + menu.Num;
            ushort Count = menu.Packet.GetUint16();
            if (Count == 0)
                Count = 1;

            if (items.Count <= Num)
                return;

            ItemResult result = plr.ItmInterface.CreateItem(items[Num].Info, (ushort)1);
            if (result == ItemResult.RESULT_OK)
            {
                plr.RemoveMoney(items[Num].Price * Count);
                foreach (KeyValuePair<uint, ushort> Kp in items[Num].ItemsReq)
                    plr.ItmInterface.RemoveItems(Kp.Key, (ushort)(Kp.Value * Count));

                items.Remove(items[Num]);

            }
            else if (result == ItemResult.RESULT_MAX_BAG)
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_MERCHANT_INSUFFICIENT_SPACE_TO_BUY);
            }
            else if (result == ItemResult.RESULT_ITEMID_INVALID)
            {

            }

        }
    }
}
