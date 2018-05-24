
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameWork
{
    public static class RandomMgr
    {
        public static Random _Random = new Random();

        public static int Next()
        {
            lock (_Random)
                return _Random.Next();
        }

        public static int Next(int MaxValue)
        {
            lock (_Random)
                return _Random.Next(MaxValue);
        }

        public static int Next(int MinValue, int MaxValue)
        {
            lock (_Random)
                return _Random.Next(MinValue, MaxValue);
        }
    }
}
