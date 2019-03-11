using FrameWork;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
                new SiegeTracker {CurrentNumberSiege = 0, MaxNumberSiege = 1, Type = SiegeType.RAM},
                new SiegeTracker {CurrentNumberSiege = 0, MaxNumberSiege = 2, Type = SiegeType.OIL},
                new SiegeTracker {CurrentNumberSiege = 0, MaxNumberSiege = 2, Type = SiegeType.DIRECT},
                new SiegeTracker {CurrentNumberSiege = 0, MaxNumberSiege = 2, Type = SiegeType.GTAOE},
                new SiegeTracker {CurrentNumberSiege = 0, MaxNumberSiege = 2, Type = SiegeType.SNIPER}
            };

            DeployedSieges = new List<Siege>();
        }

        public override string ToString()
        {
            var s = String.Empty;
            foreach (var track in SiegeTracking)
            {
                s += $"{track.Type} : {track.CurrentNumberSiege}/{track.MaxNumberSiege}. ";
            }

            return s;
        }

        public void Add(Siege siege)
        {
            DeployedSieges.Add(siege);
            var siegeType = SiegeTracking.SingleOrDefault(x => x.Type == siege.SiegeInterface.Type);
            siegeType?.Increment();
        }

        public void Remove(Siege siege)
        {
            DeployedSieges.Remove(siege);
            var siegeType = SiegeTracking.SingleOrDefault(x => x.Type == siege.SiegeInterface.Type);
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
                if (SiegeTypeCanDeploy(typeToDeploy))
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
        private bool SiegeTypeCanDeploy(SiegeType typeToDeploy)
        {
            var okToDeploy = SiegeTracking.SingleOrDefault(x => x.Type == typeToDeploy);
            if (okToDeploy != null)
                return okToDeploy.CanDeploy();
            else
            {
                return false;
            }
        }

        public int GetNumberRamsDeployed()
        {
            var ramTracking = SiegeTracking.SingleOrDefault(x => x.Type == SiegeType.RAM);
            if (ramTracking != null)
                return ramTracking.CurrentNumberSiege;
            else
            {
                return 0;
            }
        }

        public int GetNumberByType(SiegeType siegeType)
        {
            var tracking = SiegeTracking.SingleOrDefault(x => x.Type == siegeType);
            if (tracking != null)
                return tracking.CurrentNumberSiege;
            else
            {
                return 0;
            }
        }
    }
}
