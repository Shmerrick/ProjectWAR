
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameWork
{
    [Serializable]
    public class Color
    {
        public float R = 0.f;
        public float G = 0.f;
        public float B = 0.f;
        public float A = 0.f;

        public override string ToString()
        {
            return R + ":" + G + ":" + B + ":" + A;
        }
    }
}
