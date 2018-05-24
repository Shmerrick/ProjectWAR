using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarZoneLib
{
    public class CollisionInfo
    {
        public Vector3[] Vertices;
        public TriangleInfo[] Triangles;
        public Dictionary<uint, FixtureInfo> Fixtures = new Dictionary<uint, FixtureInfo>();
        public BSPNodeInfo BSP;
    }


}
