using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarZoneLib
{
    public class RegionInfo
    {
        public uint ID;
        public Dictionary<uint, ZoneInfo> Zones = new Dictionary<uint, ZoneInfo>();
    }
}
