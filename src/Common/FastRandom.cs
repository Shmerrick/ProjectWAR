using System;
using System.Text;

namespace Common
{
    public class FastRandom
    {
        private long _seed;

        public FastRandom(long seed)
        {
            _seed = seed;
        }

        private long randomLong()
        {
            _seed ^= (_seed << 21);
            _seed ^= (_seed >> 35) & 0xFF;
            _seed ^= (_seed << 4);
            return _seed;
        }

        public int randomInt()
        {
            return (int)randomLong();
        }

        public int randomInt(int range)
        {
            return fastAbs((int)randomLong() % range);
        }

        public int randomIntAbs()
        {
            return fastAbs(randomInt());
        }

        public int randomIntAbs(int range)
        {
            return fastAbs(randomInt() % range);
        }

        public double randomDouble()
        {
            return randomLong() / (long.MaxValue - 1d);
        }

        public float randomFloat()
        {
            return randomLong() / (long.MaxValue - 1f);
        }

        public float randomPosFloat()
        {
            return 0.5f * (randomFloat() + 1.0f);
        }

        public bool randomBoolean()
        {
            return randomLong() > 0;
        }

        public string randomCharacterString(int length)
        {
            StringBuilder s = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                bool upper = randomBoolean();
                int letterIndex = randomIntAbs(26);
                char c = (char)((upper ? 'A' : 'a') + letterIndex);
                s.Append(c);
            }

            string result = s.ToString();
            if (result.Length != length)
                throw new InvalidOperationException("Generated string length does not match requested length");
            return result;
        }

        public double standNormalDistrDouble()
        {
            double q = double.MaxValue;
            double u1 = 0;
            double u2;

            while (q >= 1d || q == 0)
            {
                u1 = randomDouble();
                u2 = randomDouble();

                q = Math.Pow(u1, 2) + Math.Pow(u2, 2);
            }

            double p = Math.Sqrt((-2d * (Math.Log(q))) / q);
            return u1 * p;
        }

        public static int fastAbs(int i)
        {
            return (i >= 0) ? i : -i;
        }

        public static float fastAbs(float d)
        {
            return (d >= 0) ? d : -d;
        }

        public static double fastAbs(double d)
        {
            return (d >= 0) ? d : -d;
        }
    }

    public struct SFastRandom
    {
        private long _seed;

        public SFastRandom(long seed)
        {
            _seed = seed;
        }

        private long randomLong()
        {
            _seed ^= (_seed << 21);
            _seed ^= (_seed >> 35) & 0xFF;
            _seed ^= (_seed << 4);
            return _seed;
        }

        public int randomInt()
        {
            return (int)randomLong();
        }

        public int randomInt(int range)
        {
            return fastAbs((int)randomLong() % range);
        }

        public int randomIntAbs()
        {
            return fastAbs(randomInt());
        }

        public int randomIntAbs(int range)
        {
            return fastAbs(randomInt() % range);
        }

        public double randomDouble()
        {
            return randomLong() / (long.MaxValue - 1d);
        }

        public float randomFloat()
        {
            return randomLong() / (long.MaxValue - 1f);
        }

        public float randomPosFloat()
        {
            return 0.5f * (randomFloat() + 1.0f);
        }

        public bool randomBoolean()
        {
            return randomLong() > 0;
        }

        public string randomCharacterString(int length)
        {
            StringBuilder s = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                bool upper = randomBoolean();
                int letterIndex = randomIntAbs(26);
                char c = (char)((upper ? 'A' : 'a') + letterIndex);
                s.Append(c);
            }

            string result = s.ToString();
            if (result.Length != length)
                throw new InvalidOperationException("Generated string length does not match requested length");
            return result;
        }

        public double standNormalDistrDouble()
        {
            double q = double.MaxValue;
            double u1 = 0;
            double u2;

            while (q >= 1d || q == 0)
            {
                u1 = randomDouble();
                u2 = randomDouble();

                q = Math.Pow(u1, 2) + Math.Pow(u2, 2);
            }

            double p = Math.Sqrt((-2d * (Math.Log(q))) / q);
            return u1 * p;
        }

        public static int fastAbs(int i)
        {
            return (i >= 0) ? i : -i;
        }

        public static float fastAbs(float d)
        {
            return (d >= 0) ? d : -d;
        }

        public static double fastAbs(double d)
        {
            return (d >= 0) ? d : -d;
        }
    }
}
