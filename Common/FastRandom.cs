using System;
using System.Text;

namespace Common
{
    internal struct FastRandomImpl
    {
        private long _seed;

        public FastRandomImpl(long seed)
        {
            _seed = seed;
        }

        private long RandomLong()
        {
            _seed ^= (_seed << 21);
            _seed ^= (_seed >> 35) & 0xFF;
            _seed ^= (_seed << 4);
            return _seed;
        }

        public int RandomInt()
        {
            return (int)RandomLong();
        }

        public int RandomInt(int range)
        {
            if (range <= 0)
                throw new ArgumentOutOfRangeException(nameof(range), "Range must be positive");
            return (int)Math.Abs(RandomLong() % range);
        }

        public int RandomIntAbs()
        {
            return (int)Math.Abs((long)RandomInt());
        }

        public int RandomIntAbs(int range)
        {
            if (range <= 0)
                throw new ArgumentOutOfRangeException(nameof(range), "Range must be positive");
            return (int)Math.Abs((long)(RandomInt() % range));
        }

        public double RandomDouble()
        {
            return RandomLong() / (long.MaxValue - 1d);
        }

        public float RandomFloat()
        {
            return RandomLong() / (long.MaxValue - 1f);
        }

        public float RandomPosFloat()
        {
            return 0.5f * (RandomFloat() + 1.0f);
        }

        public bool RandomBoolean()
        {
            return RandomLong() > 0;
        }

        public string RandomCharacterString(int length)
        {
            StringBuilder s = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                bool upper = RandomBoolean();
                int letterIndex = RandomIntAbs(26);
                char c = (char)((upper ? 'A' : 'a') + letterIndex);
                s.Append(c);
            }

            string result = s.ToString();
            if (result.Length != length)
                throw new InvalidOperationException("Generated string length does not match requested length");
            return result;
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

            double p = Math.Sqrt((-2d * Math.Log(q)) / q);
            return u1 * p;
        }

        public static int FastAbs(int i)
        {
            return i == int.MinValue ? int.MaxValue : Math.Abs(i);
        }

        public static float FastAbs(float d)
        {
            return d >= 0 ? d : -d;
        }

        public static double FastAbs(double d)
        {
            return d >= 0 ? d : -d;
        }
    }

    public class FastRandom
    {
        private FastRandomImpl _impl;

        public FastRandom(long seed)
        {
            _impl = new FastRandomImpl(seed);
        }

        public int randomInt() => _impl.RandomInt();

        public int randomInt(int range) => _impl.RandomInt(range);

        public int randomIntAbs() => _impl.RandomIntAbs();

        public int randomIntAbs(int range) => _impl.RandomIntAbs(range);

        public double randomDouble() => _impl.RandomDouble();

        public float randomFloat() => _impl.RandomFloat();

        public float randomPosFloat() => _impl.RandomPosFloat();

        public bool randomBoolean() => _impl.RandomBoolean();

        public string randomCharacterString(int length) => _impl.RandomCharacterString(length);

        public double standNormalDistrDouble() => _impl.StandNormalDistrDouble();

        public static int fastAbs(int i) => FastRandomImpl.FastAbs(i);

        public static float fastAbs(float d) => FastRandomImpl.FastAbs(d);

        public static double fastAbs(double d) => FastRandomImpl.FastAbs(d);
    }

    public struct SFastRandom
    {
        private FastRandomImpl _impl;

        public SFastRandom(long seed)
        {
            _impl = new FastRandomImpl(seed);
        }

        public int randomInt() => _impl.RandomInt();

        public int randomInt(int range) => _impl.RandomInt(range);

        public int randomIntAbs() => _impl.RandomIntAbs();

        public int randomIntAbs(int range) => _impl.RandomIntAbs(range);

        public double randomDouble() => _impl.RandomDouble();

        public float randomFloat() => _impl.RandomFloat();

        public float randomPosFloat() => _impl.RandomPosFloat();

        public bool randomBoolean() => _impl.RandomBoolean();

        public string randomCharacterString(int length) => _impl.RandomCharacterString(length);

        public double standNormalDistrDouble() => _impl.StandNormalDistrDouble();

        public static int fastAbs(int i) => FastRandomImpl.FastAbs(i);

        public static float fastAbs(float d) => FastRandomImpl.FastAbs(d);

        public static double fastAbs(double d) => FastRandomImpl.FastAbs(d);
    }
}
