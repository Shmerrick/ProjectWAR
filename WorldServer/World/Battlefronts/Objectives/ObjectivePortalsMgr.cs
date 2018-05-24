using Common;
using Common.Database.World.Battlefront;
using FrameWork;
using GameData;
using System.Collections.Generic;
using WorldServer.Services.World;

namespace WorldServer.World.Battlefronts.Objectives
{
    /// <summary>
    /// Object responsible of managing spawn of portals bound to a battlefield objective.
    /// </summary>
    internal class ObjectivePortalsMgr
    {
        private readonly RegionMgr _region;
        private readonly ProximityFlag _objective;

        /// <summary>Database objects providing data for portal from objective to warcamps</summary>
        private BattlefrontObject _objectivePortalData;
        /// <summary>Database objects providing data for portals from warcamps to objective</summary>
        private BattlefrontObject _orderPortalData, _destroPortalData;

        /// <summary>Portal from objective to warcamps, available when locked</summary>
        private PortalToWarcamp _objectivePortal;
        /// <summary>Portals in warcamps to objective, available when unlocked</summary>
        private PortalToObjective _destroPortal, _orderPortal;
        
        /// <summary>Builds a new portal manager.</summary>
        /// <param name="objective">Objective bound to this object</param>
        /// <param name="region">Objective's region (objective.Region is null)</param>
        internal ObjectivePortalsMgr(ProximityFlag objective, RegionMgr region)
        {
            _objective = objective;
            _region = region;
            Load();
        }

        /// <summary>(Re)loads portals data.</summary>
        internal bool Load()
        {
            ushort _zoneId = _objective.ZoneId;
            int objectiveId = _objective.ID;
            _objectivePortalData = BattlefrontService.GetPortalToWarcamp(_zoneId, objectiveId);
            _orderPortalData = GetPortalToObjective(Realms.REALMS_REALM_ORDER);
            _destroPortalData = GetPortalToObjective(Realms.REALMS_REALM_DESTRUCTION);
            
            if (_objectivePortalData != null && _orderPortalData != null && _destroPortalData != null)
                return true;
#if DEBUG
            if (_region.GetTier() < 5)
            {
                List<string> missingInfo = new List<string>();
                if (_objectivePortalData == null) missingInfo.Add("objective portal");
                if (_orderPortalData == null) missingInfo.Add("order portal");
                if (_destroPortalData == null) missingInfo.Add("destro portal");
                Log.Error("ObjectivePortalsMgr", $"Missing portal data for objective {_objective.Name} ({_objective.ID}) : " + string.Join(", ", missingInfo));
                BattlefrontService.LoadBattlefrontObjects();
            }
#endif


            return false;
        }

        /// <summary>
        /// Searches for the portal from warcamp to the managed objective.
        /// </summary>
        /// <param name="realm">Realm of warcamp to seach</param>
        /// <returns>Portal of null if missing info</returns>
        private BattlefrontObject GetPortalToObjective(Realms realm)
        {
            ushort zoneId = _objective.ZoneId;
            int objectiveId = _objective.ID;
            BattlefrontObject portalData = BattlefrontService.GetPortalToObjective(zoneId, objectiveId, realm);

            if (portalData != null) // A portal exists in same zone
                return portalData;

            // Obtherwise, search in other zones of same region
            foreach (Zone_Info zone in _region.ZonesInfo)
            {
                if (zone.ZoneId == zoneId)
                    continue;
                portalData = BattlefrontService.GetPortalToObjective(zone.ZoneId, objectiveId, realm);
                if (portalData != null)
                    return portalData;
            }

            return null;
        }
        
        /// <summary>
        /// Invoked when the objective is locked, spawning the portal to warcamp.
        /// </summary>
        internal void ObjectiveLocked()
        {
            if (_objectivePortal != null)
                return;

            if (!Load()) // Useful when database has been updated at runtime
                return;

            if (_objectivePortal == null && _region.GetTier() < 2)
            {
                _objectivePortal = new PortalToWarcamp(_objectivePortalData, _orderPortalData, _destroPortalData);
                _region.AddObject(_objectivePortal, _objectivePortalData.ZoneId);
            }

            if (_orderPortal != null)
            {
                _orderPortal.Dispose();
                _destroPortal.Dispose();
                _orderPortal = null;
                _destroPortal = null;
            }
        }

        /// <summary>
        /// Invoked when the objective is locked, initializing the portals in warcamps.
        /// They will be spawned depending on flag state.
        /// </summary>
        internal void ObjectiveUnlocked()
        {
            if (!Load()) // Useful when database has been updated at runtime
                return;

            if (_objectivePortal != null)
            {
                _region.RemoveObject(_objectivePortal);
                _objectivePortal = null;
            }

            if (_orderPortal == null)
            {
                _orderPortal = new PortalToObjective(_orderPortalData, _objectivePortalData, _objective.Name);
                _destroPortal = new PortalToObjective(_destroPortalData, _objectivePortalData, _objective.Name);
            }
            int i = 0;
        }

        /// <summary>
        /// Updates spawn of warcamp portals depending on objective's current state.
        /// </summary>
        internal void UpdateWarcampPortals()
        {
            if (_orderPortal == null || _destroPortal == null)
                return;

            Realms secureRealm = _objective.GetSecureRealm();
            bool threatening = _objective.HasThreateningPlayer;

            bool needOrder = !threatening && secureRealm == Realms.REALMS_REALM_ORDER;
            bool needDestro = !threatening && secureRealm == Realms.REALMS_REALM_DESTRUCTION;

#if false
            needOrder = needDestro = true;
#endif
            
            UpdatePortalSpawn(_orderPortal, _orderPortalData, needOrder);
            UpdatePortalSpawn(_destroPortal, _destroPortalData, needDestro);
        }

        /// <summary>
        /// Spawns or despawns given portal.
        /// </summary>
        /// <param name="portal">Portal to spawn or despawn</param>
        /// <param name="portalData">Data bound to the portal</param>
        /// <param name="spawn">True to spawn the portal, false to despawn it</param>
        private void UpdatePortalSpawn(PortalToObjective portal, BattlefrontObject portalData, bool spawn)
        {
            if (portal.IsInWorld() != spawn)
            {
                if (spawn)
                {
                    PortalToObjective clone = new PortalToObjective(portal);
                    if (portalData.Realm == (ushort)Realms.REALMS_REALM_ORDER)
                        _orderPortal = clone;
                    else
                        _destroPortal = clone;
                    _region.AddObject(clone, portalData.ZoneId);
                }
                else
                    _region.RemoveObject(portal);
            }
        }
    }
}
