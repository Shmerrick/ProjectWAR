using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarZoneLib
{
    public enum ChunkType
    {
        Undefined = 0,
        Zone      = 1,
        NIF       = 2,
        Fixture   = 3,
        Terrain   = 4,
        Collision = 5,
        BSP       = 6,
        Region    = 7,
        Count
    }
}
