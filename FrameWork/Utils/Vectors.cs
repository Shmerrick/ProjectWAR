using System;

namespace FrameWork
{
    [Serializable]
    public class Vector2
    {
        public float X;
        public float Y;

        public Vector2()
        {
        }

        public Vector2(Vector2 baseVec)
        {
            X = baseVec.X;
            Y = baseVec.Y;
        }

        public Vector2(float X, float Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public static float DotProduct2D(Vector2 v1, Vector2 v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }

        public static Vector2 HeadingToVector(ushort heading, ushort unitDistance)
        {
            double angle = heading * (360.0 / 4096.0) * (Math.PI / 180.0); // Heading to Radian

            var vec = new Vector2((float)(-Math.Sin(angle) * unitDistance * 12), (float)(Math.Cos(angle) * unitDistance * 12));

            return vec;
        }

        public bool IsNullVector()
        {
            return Math.Abs(X) < 0.001f && Math.Abs(Y) < 0.001f;
        }

        public virtual float Magnitude => (float)Math.Sqrt(X*X + Y*Y);

        public float CosineOfAngleWithUp()
        {
            return Y/Magnitude;
        }

        public void Add(Vector2 b)
        {
            X += b.X;
            Y += b.Y;
        }

        public virtual void Multiply(float b)
        {
            X *= b;
            Y *= b;
        }

        public override string ToString()
        {
            return $"X: {X} Y: {Y}";
        }

        public virtual void Normalize()
        {
            float magnitude = Magnitude;

            X /= magnitude;
            Y /= magnitude;
        }
    }

    [Serializable]
    public class Vector3 : Vector2
    {
        public float Z;

        public Vector3()
        {
        }

        public Vector3(float X, float Y, float Z)
            : base(X, Y)
        {
            this.Z = Z;
        }

        public Vector3(Vector3 orig)
        {
            X = orig.X;
            Y = orig.Y;
            Z = orig.Z;
        }

        public override float Magnitude => (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        public float MagnitudeSquare => X*X + Y*Y + Z*Z;

        public static float DotProduct3D(Vector3 v1, Vector3 v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        public override void Multiply(float b)
        {
            X *= b;
            Y *= b;
            Z *= b;
        }

        public override void Normalize()
        {
            float magnitude = Magnitude;

            X /= magnitude;
            Y /= magnitude;
            Z /= magnitude;
        }

        public override string ToString()
        {
            return $"X: {X} Y: {Y} Z: {Z}";
        }
    }

    [Serializable]
    public class Quaternion : Vector3
    {
        public float W;

        public Quaternion(float X, float Y, float Z, float W)
            : base(X, Y, Z)
        {
            this.W = W;
        }

    }
}
