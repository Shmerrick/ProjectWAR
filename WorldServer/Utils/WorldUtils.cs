using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldServer.World.Positions;

namespace WorldServer
{
    public static class WorldUtils
    {
        public static Point2D CalculatePoint(Random random, int radius, int originX, int originY)
        {
            double angle = random.NextDouble() * Math.PI * 2.0d;
            double pointRadius = random.NextDouble() * (double)radius;
            double x = originX + pointRadius * Math.Cos(angle);
            double y = originY + pointRadius * Math.Sin(angle);
           // Console.WriteLine($"Angle: {angle}, radius: {pointRadius}");
            return new Point2D((int)x, (int)y);
        }
    }
}
