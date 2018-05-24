using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarZoneLib
{
    public struct Vector3
    {
        public float X, Y, Z;

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3 operator+(Vector3 u, Vector3 v)
        {
            return new Vector3(u.X + v.X, u.Y + v.Y, u.Z + v.Z);
        }

        public static Vector3 operator-(Vector3 u, Vector3 v)
        {
            return new Vector3(u.X - v.X, u.Y - v.Y, u.Z - v.Z);
        }

        public static Vector3 operator*(float t, Vector3 v)
        {
            return new Vector3(t * v.X, t * v.Y, t * v.Z);
        }

        public static Vector3 operator*(Vector3 v, float t)
        {
            return t * v;
        }

        public static Vector3 operator/(Vector3 v, float d)
        {
            return 1.0f / d * v;
        }

        public float Length
        {
            get { return (float) Math.Sqrt(LengthSquared); }
        }

        public float LengthSquared
        {
            get { return Dot(this, this); }
        }

        public static float Dot(Vector3 u, Vector3 v)
        {
            return u.X * v.X + u.Y * v.Y + u.Z * v.Z;
        }

        public static Vector3 Cross(Vector3 u, Vector3 v)
        {
            Vector3 r;
            r.X =   u.Y * v.Z - u.Z * v.Y;
            r.Y = -(u.X * v.Z - u.Z * v.X);
            r.Z =   u.X * v.Y - u.Y * v.X;
            return r;
        }

        public Vector3 Normalize()
        {
            return this / Length;
        }
    };
}
