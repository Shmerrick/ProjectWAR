using FrameWork;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GameData;
using WorldServer.World.Battlefronts.Keeps;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    /// <summary>
    /// Class to manage the deployment aspects of Siege for a given realm within a campaign.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class SiegeManager
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public List<SiegeTracker> SiegeTracking { get; set; }
        public List<Siege> DeployedSieges { get; set; }

        public SiegeManager()
        {
            SiegeTracking = new List<SiegeTracker>
            {
                new SiegeTracker {CurrentNumberSiege = 0, MaxNumberSiege = 1, Type = SiegeType.RAM, Realm = Realms.REALMS_REALM_DESTRUCTION},
                new SiegeTracker {CurrentNumberSiege = 0, MaxNumberSiege = 1, Type = SiegeType.RAM, Realm = Realms.REALMS_REALM_ORDER},
                new SiegeTracker {CurrentNumberSiege = 0, MaxNumberSiege = 2, Type = SiegeType.OIL, Realm = Realms.REALMS_REALM_DESTRUCTION},
                new SiegeTracker {CurrentNumberSiege = 0, MaxNumberSiege = 2, Type = SiegeType.OIL, Realm = Realms.REALMS_REALM_ORDER},
                new SiegeTracker {CurrentNumberSiege = 0, MaxNumberSiege = 2, Type = SiegeType.DIRECT, Realm = Realms.REALMS_REALM_DESTRUCTION},
                new SiegeTracker {CurrentNumberSiege = 0, MaxNumberSiege = 2, Type = SiegeType.DIRECT, Realm = Realms.REALMS_REALM_ORDER},
                new SiegeTracker {CurrentNumberSiege = 0, MaxNumberSiege = 2, Type = SiegeType.GTAOE, Realm = Realms.REALMS_REALM_DESTRUCTION},
                new SiegeTracker {CurrentNumberSiege = 0, MaxNumberSiege = 2, Type = SiegeType.GTAOE, Realm = Realms.REALMS_REALM_ORDER},
                new SiegeTracker {CurrentNumberSiege = 0, MaxNumberSiege = 2, Type = SiegeType.SNIPER, Realm = Realms.REALMS_REALM_DESTRUCTION},
                new SiegeTracker {CurrentNumberSiege = 0, MaxNumberSiege = 2, Type = SiegeType.SNIPER, Realm = Realms.REALMS_REALM_ORDER}
            };

            DeployedSieges = new List<Siege>();
        }

        public override string ToString()
        {
            var s = String.Empty;
            foreach (var track in SiegeTracking)
            {
                s += $"{track.Type} : {track.CurrentNumberSiege}/{track.MaxNumberSiege} ({track.Realm})";
            }

            return s;
        }

        public void Add(Siege siege, Realms realm)
        {
            DeployedSieges.Add(siege);
            var siegeType = SiegeTracking.SingleOrDefault(x => x.Type == siege.SiegeInterface.Type && x.Realm == realm);
            siegeType?.Increment();
        }

        public void Remove(Siege siege, Realms realm)
        {
            DeployedSieges.Remove(siege);
            var siegeType = SiegeTracking.SingleOrDefault(x => x.Type == siege.SiegeInterface.Type && x.Realm == realm);
            siegeType?.Decrement();
        }

        public void DestroyAllSiege()
        {
            _logger.Debug("Destroying all siege.");
            foreach (var deployedSiege in DeployedSieges)
            {
                deployedSiege.SiegeInterface.DeathTime = DateTime.Now.Ticks;
            }
        }

        /// <summary>
        /// Determine whether we can deploy the given siege type.
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="comparitor"></param>
        /// <param name="typeToDeploy"></param>
        /// <returns></returns>
        public DeploymentReason CanDeploySiege(Player caster, ILocationComparitor comparitor, SiegeType typeToDeploy)
        {
            if (comparitor.InRange(caster))
            {
                if (SiegeTypeCanDeploy(typeToDeploy, caster.Realm))
                    return DeploymentReason.Success;
                else
                {
                    return DeploymentReason.MaximumCount;
                }
            }
            return DeploymentReason.Range;
        }

        /// <summary>
        /// Determine whether the counts mean we can deploy this siege. 
        /// </summary>
        /// <param name="typeToDeploy"></param>
        /// <returns></returns>
        private bool SiegeTypeCanDeploy(SiegeType typeToDeploy, Realms realm)
        {
            var okToDeploy = SiegeTracking.SingleOrDefault(x => x.Type == typeToDeploy && x.Realm == realm);
            if (okToDeploy != null)
                return okToDeploy.CanDeploy();
            else
            {
                return false;
            }
        }

       public int GetNumberByType(SiegeType siegeType, Realms realm)
        {
            var tracking = SiegeTracking.SingleOrDefault(x => x.Type == siegeType && x.Realm == realm);
            if (tracking != null)
                return tracking.CurrentNumberSiege;
            else
            {
                return 0;
            }
        }
    }
}
