using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldServer.Services.World;

namespace WorldServer.World.Objects
{
    public class RVRArea
    {
        private Dictionary<ushort, List<ushort>> ZoneAreas { get; set; }

        public Dictionary<ushort, List<ushort>> GetZoneRVRAreas()
        {
            if (ZoneAreas == null)
                ZoneAreas = new Dictionary<ushort, List<ushort>>();
            else
            {
                return ZoneAreas;
            }
            
            foreach (var rvrAreaPolygon in RVRProgressionService._RVRAreaPolygons)
            {
                ZoneAreas.Add(rvrAreaPolygon.ZoneId, rvrAreaPolygon.PolygonPlanarCoordinates.Split(',').Select(ushort.Parse).ToList());
            }
            return ZoneAreas;
        }

        public static bool InPoly(List<ushort> poly, ushort x, ushort y)
        {
            int i, j;
            int nvert = poly.Count / 2;
            bool c = false;
            for (i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                ushort px = poly[(i * 2)];
                ushort py = poly[(i * 2) + 1];

                if (((py > y) != (poly[(j * 2) + 1] > y)) &&
                 (x < (poly[(j * 2)] - px) * (y - py) / (poly[(j * 2) + 1] - py) + px))
                    c = !c;
            }
            return c;
        }

        public static bool IsPlayerInRvR(Player player, Dictionary<ushort, List<ushort>> zoneRVRAreas)
        {
            return zoneRVRAreas.ContainsKey(player.Zone.ZoneId) && InPoly(zoneRVRAreas[player.Zone.ZoneId], (ushort)player.X, (ushort)player.Y);
        }

    }
}
