using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldServer
{
    public interface IPoint2D
    {
        int X { get; set; }
        int Y { get; set; }

        int GetDistance(IPoint2D point);
        ushort GetHeading(IPoint2D point);

        Point2D GetPointFromHeading(ushort heading, int distance);
        Point2D GetBackPoint(ushort heading, int distance);

        void Clear();

        bool IsWithinRadiusUnits(IPoint2D point, int radius);
        bool IsWithinRadiusFeet(IPoint2D point, int radius);
    }
}

