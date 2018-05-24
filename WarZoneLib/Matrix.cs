using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace WarZoneLib
{
    public struct Matrix
    {
        public float M00, M01, M02, M03,
                     M10, M11, M12, M13,
                     M20, M21, M22, M23,
                     M30, M31, M32, M33;

        public Matrix(
            float m00, float m01, float m02, float m03,
            float m10, float m11, float m12, float m13,
            float m20, float m21, float m22, float m23,
            float m30, float m31, float m32, float m33)
        {
            M00 = m00;
            M01 = m01;
            M02 = m02;
            M03 = m03;

            M10 = m10;
            M11 = m11;
            M12 = m12;
            M13 = m13;

            M20 = m20;
            M21 = m21;
            M22 = m22;
            M23 = m23;

            M30 = m30;
            M31 = m31;
            M32 = m32;
            M33 = m33;
        }

        public static Matrix operator*(Matrix p, Matrix q)
        {
            Matrix r = new Matrix();

            r.M00 = (p.M00 * q.M00) + (p.M01 * q.M10) + (p.M02 * q.M20) + (p.M03 * q.M30);
            r.M01 = (p.M00 * q.M01) + (p.M01 * q.M11) + (p.M02 * q.M21) + (p.M03 * q.M31);
            r.M02 = (p.M00 * q.M02) + (p.M01 * q.M12) + (p.M02 * q.M22) + (p.M03 * q.M32);
            r.M03 = (p.M00 * q.M03) + (p.M01 * q.M13) + (p.M02 * q.M23) + (p.M03 * q.M33);

            r.M10 = (p.M10 * q.M00) + (p.M11 * q.M10) + (p.M12 * q.M20) + (p.M13 * q.M30);
            r.M11 = (p.M10 * q.M01) + (p.M11 * q.M11) + (p.M12 * q.M21) + (p.M13 * q.M31);
            r.M12 = (p.M10 * q.M02) + (p.M11 * q.M12) + (p.M12 * q.M22) + (p.M13 * q.M32);
            r.M13 = (p.M10 * q.M03) + (p.M11 * q.M13) + (p.M12 * q.M23) + (p.M13 * q.M33);

            r.M20 = (p.M20 * q.M00) + (p.M21 * q.M10) + (p.M22 * q.M20) + (p.M23 * q.M30);
            r.M21 = (p.M20 * q.M01) + (p.M21 * q.M11) + (p.M22 * q.M21) + (p.M23 * q.M31);
            r.M22 = (p.M20 * q.M02) + (p.M21 * q.M12) + (p.M22 * q.M22) + (p.M23 * q.M32);
            r.M23 = (p.M20 * q.M03) + (p.M21 * q.M13) + (p.M22 * q.M23) + (p.M23 * q.M33);

            r.M30 = (p.M30 * q.M00) + (p.M31 * q.M10) + (p.M32 * q.M20) + (p.M33 * q.M30);
            r.M31 = (p.M30 * q.M01) + (p.M31 * q.M11) + (p.M32 * q.M21) + (p.M33 * q.M31);
            r.M32 = (p.M30 * q.M02) + (p.M31 * q.M12) + (p.M32 * q.M22) + (p.M33 * q.M32);
            r.M33 = (p.M30 * q.M03) + (p.M31 * q.M13) + (p.M32 * q.M23) + (p.M33 * q.M33);

            return r;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static Matrix Identity
        {
            get
            {
                return new Matrix(
                    1, 0, 0, 0,
                    0, 1, 0, 0,
                    0, 0, 1, 0,
                    0, 0, 0, 1);
            }
        }

        public static Matrix Scaling(float x, float y, float z)
        {
            return new Matrix(
                x, 0, 0, 0,
                0, y, 0, 0,
                0, 0, z, 0,
                0, 0, 0, 1);
        }

        public static Matrix Translation(float x, float y, float z)
        {
            return new Matrix(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                x, y, z, 1);
        }

        public static Matrix RotationZ(float radians)
        {
            return RotationAxis(new Vector3(0,0,1), radians);
        }

        public static Matrix RotationAxis(Vector3 axis, float radians)
        {
            float c = (float) Math.Cos(radians);
            float s = (float) Math.Sin(radians);
            float t = 1.0f - c;
            float x = axis.X;
            float y = axis.Y;
            float z = axis.Z;

            Matrix r = new Matrix();

            r.M00 = t * x * x + c;
            r.M01 = t * x * y + z * s;
            r.M02 = t * x * z - y * s;
            r.M03 = 0;
            
            r.M10 = t * x * y - z * s;
            r.M11 = t * y * y + c;
            r.M12 = t * y * z + x * s;
            r.M13 = 0;

            r.M20 = t * x * z + y * s;
            r.M21 = t * y * z - x * s;
            r.M22 = t * z * z + c;
            r.M23 = 0;

            r.M30 = 0;
            r.M31 = 0;
            r.M32 = 0;
            r.M33 = 1;

            return r;
        }

        public Matrix Transpose()
        {
            return new Matrix(
                M00, M10, M20, M30,
                M01, M11, M21, M31,
                M02, M12, M22, M32,
                M03, M13, M23, M33);
        }

        public Vector3 TransformPoint(Vector3 p)
        {
            return new Vector3(
                (p.X * M00) + (p.Y * M10) + (p.Z * M20) + M30,
                (p.X * M01) + (p.Y * M11) + (p.Z * M21) + M31,
                (p.X * M02) + (p.Y * M12) + (p.Z * M22) + M32);
        }
    }
}
