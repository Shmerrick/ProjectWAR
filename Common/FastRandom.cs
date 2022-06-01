using System;
using System.Text;

namespace Common
{
    public struct SFastRandom
    {
        private long _seed;

        public SFastRandom(long seed)
        {
            _seed = seed;
        }

        public static int FastAbs(int i)
        {
            return (i >= 0) ? i : -i;
        }

        public static float FastAbs(float d)
        {
            return (d >= 0) ? d : -d;
        }

        public static double FastAbs(double d)
        {
            return (d >= 0) ? d : -d;
        }

        public bool RandomBoolean()
        {
            return RandomLong() > 0;
        }

        public string RandomCharacterString(int length)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < length / 2; i++)
            {
                sb.Append((char)('a' + FastAbs(RandomDouble()) * 26d));
                sb.Append((char)('A' + FastAbs(RandomDouble()) * 26d));
            }

            return sb.ToString();
        }

        public double RandomDouble()
        {
            return RandomLong() / (long.MaxValue - 1d);
        }

        public float RandomFloat()
        {
            return RandomLong() / (long.MaxValue - 1f);
        }

        public int RandomInt()
        {
            return (int)RandomLong();
        }

        public int RandomInt(int range)
        {
            return (int)RandomLong() % range;
        }

        public int RandomIntAbs()
        {
            return FastAbs(RandomInt());
        }

        public int RandomIntAbs(int range)
        {
            return FastAbs(RandomInt() % range);
        }

        public float RandomPosFloat()
        {
            return 0.5f * (RandomFloat() + 1.0f);
        }

        public double StandNormalDistrDouble()
        {
            double q = double.MaxValue;
            double u1 = 0;
            double u2;

            while (q >= 1d || q == 0)
            {
                u1 = RandomDouble();
                u2 = RandomDouble();

                q = Math.Pow(u1, 2) + Math.Pow(u2, 2);
            }

            double p = Math.Sqrt((-2d * (Math.Log(q))) / q);
            return u1 * p;
        }

        private long RandomLong()
        {
            _seed ^= (_seed << 21);
            _seed ^= (_seed >> 35) & 0xFF;
            _seed ^= (_seed << 4);
            return _seed;
        }
    }

    public class FastRandom
    {
        private long _seed;

        public FastRandom(long seed)
        {
            _seed = seed;
        }

        public static int FastAbs(int i)
        {
            return (i >= 0) ? i : -i;
        }

        public static float FastAbs(float d)
        {
            return (d >= 0) ? d : -d;
        }

        public static double FastAbs(double d)
        {
            return (d >= 0) ? d : -d;
        }

        public bool RandomBoolean()
        {
            return RandomLong() > 0;
        }

        public string RandomCharacterString(int length)
        {
            StringBuilder s = new StringBuilder();

            for (int i = 0; i < length / 2; i++)
            {
                s.Append((char)('a' + FastAbs(RandomDouble()) * 26d));
                s.Append((char)('A' + FastAbs(RandomDouble()) * 26d));
            }

            return s.ToString();
        }

        public double RandomDouble()
        {
            return RandomLong() / (long.MaxValue - 1d);
        }

        public float RandomFloat()
        {
            return RandomLong() / (long.MaxValue - 1f);
        }

        public int RandomInt()
        {
            return (int)RandomLong();
        }

        public int RandomInt(int range)
        {
            return (int)RandomLong() % range;
        }

        public int RandomIntAbs()
        {
            return FastAbs(RandomInt());
        }

        public int RandomIntAbs(int range)
        {
            return FastAbs(RandomInt() % range);
        }

        public float RandomPosFloat()
        {
            return 0.5f * (RandomFloat() + 1.0f);
        }

        public double StandNormalDistrDouble()
        {
            double q = double.MaxValue;
            double u1 = 0;
            double u2;

            while (q >= 1d || q == 0)
            {
                u1 = RandomDouble();
                u2 = RandomDouble();

                q = Math.Pow(u1, 2) + Math.Pow(u2, 2);
            }

            double p = Math.Sqrt((-2d * (Math.Log(q))) / q);
            return u1 * p;
        }

        private long RandomLong()
        {
            _seed ^= (_seed << 21);
            _seed ^= (_seed >> 35) & 0xFF;
            _seed ^= (_seed << 4);
            return _seed;
        }
    }
}