using WorldServer.World.Objects;
using WorldServer.World.Positions;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class FriendlyKeepLocationComparitor : ILocationComparitor
    {
        public int KeepComparisonRange = 300;
        public int HardPointComparisonRange = 50;

        public bool InRange(Player player)
        {
            if (player.CurrentKeep != null)
            {
                // Get the keeps in this zone.
                var keepsInZone = player.Region.Campaign.GetZoneKeeps((ushort)player.ZoneId);
                // if the player is of the same realm as the keep.
                foreach (var battleFrontKeep in keepsInZone)
                {
                    if (player.PointWithinRadiusFeet(
                        new Point3D(
                            battleFrontKeep.Info.PQuest.GoldChestWorldX,
                            battleFrontKeep.Info.PQuest.GoldChestWorldY,
                            battleFrontKeep.Info.PQuest.GoldChestWorldZ),
                        KeepComparisonRange))
                    {
                        if (player.Realm == battleFrontKeep.Realm)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            return false;
        }
    }
}