using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameWork;
using GameData;
using NLog;
using WorldServer.World.Battlefronts.Keeps;

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
                deployedSiege.SiegeInterface.DeathTime = TCPManager.GetTimeStamp();
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
    }

    public enum DeploymentReason
    {
        Success,
        Range,
        MaximumCount
    }

    public interface ILocationComparitor
    {
        bool InRange(Player player);
    }

    public class SiegeMerchantLocationComparitor : ILocationComparitor
    {
        public int ComparisonRange = 100;

        public bool InRange(Player player)
        {
            var creaturesInRange = player.GetInRange<Creature>(ComparisonRange);
            foreach (var creature in creaturesInRange)
            {
                if (creature.IsSiegeMerchant() && creature.Realm == player.Realm)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class FriendlyKeepLocationComparitor : ILocationComparitor
    {
        public int KeepComparisonRange = 50;
        public int HardPointComparisonRange = 40;

        public bool InRange(Player player)
        {
            if (player.CurrentKeep != null)
            {
                if (player.CurrentKeep.Realm == player.Realm)
                {
                    if (player.PointWithinRadiusFeet(
                        new Point3D(
                            player.CurrentKeep.Info.PQuest.GoldChestWorldX,
                            player.CurrentKeep.Info.PQuest.GoldChestWorldY,
                            player.CurrentKeep.Info.PQuest.GoldChestWorldZ),
                        KeepComparisonRange))
                    {
                        foreach (Hardpoint h in player.CurrentKeep.HardPoints)
                        {
                            if (player.PointWithinRadiusFeet(h, HardPointComparisonRange))
                                return false;
                        }
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
    }

    public class EnemyKeepLocationComparitor : ILocationComparitor
    {
        public int KeepComparisonRange = 200;

        public bool InRange(Player player)
        {
            // Get the keeps in this zone.
            var keepsInZone = player.Region.Campaign.GetZoneKeeps(player.ZoneId);

            // If one of the keeps belongs to the enemy and you are within a certain distance from it...
            if (keepsInZone != null)
            {
                foreach (var battleFrontKeep in keepsInZone)
                {
                    if (battleFrontKeep.Realm != player.Realm)
                    {
                        if (player.PointWithinRadiusFeet(
                            new Point3D(
                                battleFrontKeep.Info.PQuest.GoldChestWorldX,
                                battleFrontKeep.Info.PQuest.GoldChestWorldY,
                                battleFrontKeep.Info.PQuest.GoldChestWorldZ),
                            KeepComparisonRange))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }


    public class SiegeTracker
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public byte MaxNumberSiege { get; set; }
        public byte CurrentNumberSiege { get; set; }
        public SiegeType Type { get; set; }

        public override string ToString()
        {
            return $"{Type} {CurrentNumberSiege}/{MaxNumberSiege}";
        }

        public bool CanDeploy()
        {
            _logger.Debug($"{this.ToString()}");
            return CurrentNumberSiege < MaxNumberSiege;
        }

        public void Increment()
        {
            CurrentNumberSiege++;
            if (CurrentNumberSiege > MaxNumberSiege)
                _logger.Warn($"Number of Siege now exceeds maximum!");
                
        }
        public void Decrement()
        {
            CurrentNumberSiege--;
            if (CurrentNumberSiege < 0 )
                _logger.Warn($"Number of Siege now less than zero!");

        }

    }
}
