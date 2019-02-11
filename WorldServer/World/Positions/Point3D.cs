using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldServer
{
    public class Point3D : Point2D, IPoint3D
    {
        /// <summary>
        /// The Z coord of this point
        /// </summary>
        protected int m_z;

        /// <summary>
        /// Constructs a new 3D point object
        /// </summary>
        public Point3D()
            : base(0, 0)
        {
        }

        /// <summary>
        /// Constructs a new 3D point object
        /// </summary>
        /// <param name="x">The X coord</param>
        /// <param name="y">The Y coord</param>
        /// <param name="z">The Z coord</param>
        public Point3D(int x, int y, int z)
            : base(x, y)
        {
            m_z = z;
        }

        /// <summary>
        /// Constructs a new 3D point object
        /// </summary>
        /// <param name="point">2D point</param>
        /// <param name="z">Z coord</param>
        public Point3D(IPoint2D point, int z)
            : this(point.X, point.Y, z)
        {
        }

        /// <summary>
        /// Constructs a new 3D point object
        /// </summary>
        /// <param name="point">3D point</param>
        public Point3D(IPoint3D point)
            : this(point.X, point.Y, point.Z)
        {
        }

        #region IPoint3D Members

        /// <summary>
        /// Z coord of this point
        /// </summary>
        public virtual int Z
        {
            get { return m_z; }
            set { m_z = value; }
        }

        public override void Clear()
        {
            base.Clear();
            Z = 0;
        }

        #endregion

        public void SetCoordsFrom(Point3D point)
        {
            X = point.X;
            Y = point.Y;
            Z = point.Z;
        }

        /// <summary>
        /// Creates the string representation of this point
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", m_x, m_y, m_z);
        }

        /// <summary>
        /// Get the distance to a point, in feet.
        /// </summary>
        /// <remarks>
        /// If you don't actually need the distance value, it is faster
        /// to use IsWithinRadius (since it avoids the square root calculation)
        /// </remarks>
        /// <param name="point">Target point</param>
        /// <returns>Distance to point</returns>
        public virtual int GetDistanceTo(IPoint3D point)
        {
            if (point == null)
                return 900;

            double dx = X - point.X;
            double dy = Y - point.Y;
            double dz = Z - point.Z;
            double range = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            range = range / UNITS_TO_FEET;
            return (int)(range);
        }

        /// <summary>
        /// Get the distance to a point
        /// </summary>
        /// <remarks>
        /// If you don't actually need the distance value, it is faster
        /// to use IsWithinRadius (since it avoids the square root calculation)
        /// </remarks>
        /// <param name="point">Target point</param>
        /// <returns>Distance to point</returns>
        public virtual int GetDistanceTo(float x, float y, float z)
        {
            double dx = X - x;
            double dy = Y - y;
            double dz = Z - z;
            double range = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            range = range / UNITS_TO_FEET;
            return (int)range;
        }

        /// <summary>
        /// Determine if another point is within a given radius
        /// </summary>
        /// <param name="point">Target point</param>
        /// <param name="radius">Radius</param>
        /// <returns>True if the point is within the radius, otherwise false</returns>
        public virtual bool IsWithinRadiusUnits(IPoint3D point, int radius)
        {
            if (radius > ushort.MaxValue)
                return GetDistance(point) <= radius;

            double dx = X - point.X;
            double dy = Y - point.Y;
            double dz = Z - point.Z;
            double distSquare = dx * dx + dy * dy + dz * dz;

            return distSquare <= radius * radius;
        }

        public virtual bool IsWithinRadiusFeet(IPoint3D point, int radius)
        {
            radius *= UNITS_TO_FEET;

            if (radius > ushort.MaxValue)
                return GetDistance(point) <= radius;

            double dx = X - point.X;
            double dy = Y - point.Y;
            double dz = Z - point.Z;
            double distSquare = dx * dx + dy * dy + dz * dz;

            return distSquare <= radius * radius;
        }
    }
}
