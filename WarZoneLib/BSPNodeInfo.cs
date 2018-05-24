using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarZoneLib
{
    public class BSPNodeInfo
    {
        public Plane P;
        public BSPNodeInfo Back, Front;
        public int[] Triangles;

        public bool IsLeaf
        {
            get
            {
                return Back == null;
            }
        }
    }
}
