using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarZoneLib
{
    public class ZoneInfo
    {
        public ZoneInfo()
        {
            Enabled = true;
        }

        public uint ID;
        public uint OffsetX;
        public uint OffsetY;
        public CollisionInfo Collision;
        public String Name;
        public bool Enabled;
        public TerrainInfo Terrain;
        public RegionInfo Region;
    }
}
