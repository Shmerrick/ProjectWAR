using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;

namespace WarZoneLib
{
    class Timer
    {
        [DllImport("kernel32.dll", SetLastError=true)]
        private static extern bool QueryPerformanceCounter(out long lpPerfCounter);

        [DllImport("kernel32.dll", SetLastError=true)]
        private static extern bool QueryPerformanceFrequency(out long lpPerfFrequency);

        public static long Value
        {
            get
            {
                long result;
                return QueryPerformanceCounter(out result) ? result : 0;
            }
        }

        public static long Frequency
        {
            get
            {
                long result;
                return QueryPerformanceFrequency(out result) ? result : 0;
            }
        }
    }
}
