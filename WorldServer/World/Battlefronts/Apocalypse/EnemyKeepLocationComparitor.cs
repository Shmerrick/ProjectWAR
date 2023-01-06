using System.Linq;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using WorldServer.World.Positions;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class EnemyKeepLocationComparitor : ILocationComparitor
    {
        public SiegeType SiegeTypeValue { get; set; }
        public int KeepComparisonRange = 100;

        public EnemyKeepLocationComparitor(SiegeType siegeTypeValue)
        {
            SiegeTypeValue = siegeTypeValue;
        }

        public bool InRange(Player player)
        {
            // Get the keeps in this zone.
            var keepsInZone = player.Region.Campaign.GetZoneKeeps((ushort)player.ZoneId);

            // If one of the keeps belongs to the enemy and you are within a certain distance from it...
            if (keepsInZone != null)
            {
                foreach (var battleFrontKeep in keepsInZone)
                {
                    if (battleFrontKeep.Realm != player.Realm)
                    {
                        // Keeps can have multiple siege spawn points. If the player is within range of any, allow spawn of siege.
                        var keepSpawnPoints = battleFrontKeep.SpawnPoints.Where(x => x.SiegeType == (int)SiegeTypeValue && x.KeepId == battleFrontKeep.Info.KeepId);
                        foreach (var spawnPoint in keepSpawnPoints)
                        {
                            if (player.PointWithinRadiusFeet(
                                new Point3D(
                                    spawnPoint.X,
                                    spawnPoint.Y,
                                    spawnPoint.Z),
                                KeepComparisonRange))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}