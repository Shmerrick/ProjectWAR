using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace PatcherServer
{
    public static class TaskExtensions
    {
        public static void RunInBackground(this Task task)
        {
            task.ContinueWith(t => { }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }

    public class Utils
    {
        public static uint Adler32(uint adler, byte[] bytes, UInt64 length)
        {
            const uint a32mod = 65521;
            uint s1 = (uint)(adler & 0xFFFF), s2 = (uint)(adler >> 16);
            for (UInt64 i = 0; i < length; i++)
            {
                byte b = bytes[i];
                s1 = (s1 + b) % a32mod;
                s2 = (s2 + s1) % a32mod;
            }
            return unchecked((uint)((s2 << 16) + s1));
        }

        public static uint Adler32(Stream stream, long size, int blockSize = 0xFFFFF, uint adler = 0)
        {
            var pos = stream.Position;
            long remain = size;
            long readSize = blockSize;
            byte[] block = new byte[blockSize];
            var a = new Adler32();
            while (remain > 0)
            {
                if (stream.Position + blockSize > stream.Length)
                    readSize = stream.Length - stream.Position;

                stream.Read(block, 0, (int)readSize);
                adler = (uint)a.adler32(adler, block, 0, (int)readSize);
                //   adler = Adler32(adler, block, (int)readSize);
                remain -= readSize;
            }
            stream.Position = pos;
            return adler;
        }

        public static object FromString(string data, Type type)
        {
            if (type == typeof(bool))
                return (data == "1" || data.ToUpper() == "TRUE" || data.ToUpper() == "ON") ? true : false;

            if (type == typeof(byte))
                return byte.Parse(data);

            if (type == typeof(short))
                return short.Parse(data);
            if (type == typeof(int))
                return int.Parse(data);
            if (type == typeof(long))
                return long.Parse(data);

            if (type == typeof(ushort))
                return ushort.Parse(data);
            if (type == typeof(uint))
                return uint.Parse(data);
            if (type == typeof(ulong))
                return ulong.Parse(data);

            if (type == typeof(DateTime))
                return DateTime.Parse(data);
            if (type == typeof(float))
                return float.Parse(data);
            if (type == typeof(double))
                return double.Parse(data);

            return data;
        }


        public static Delegate CreateDelegate(MethodInfo method, object target)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            if (method.IsGenericMethod)
            {
                throw new ArgumentException("The provided method must not be generic.", "method");
            }

            return method.CreateDelegate(Expression.GetDelegateType(
                (from parameter in method.GetParameters() select parameter.ParameterType)
                .Concat(new[] { method.ReturnType })
                .ToArray()), target);
        }
    }

    sealed class Adler32
    {

        // largest prime smaller than 65536
        private const int BASE = 65521;
        // NMAX is the largest n such that 255n(n+1)/2 + (n+1)(BASE-1) <= 2^32-1
        private const int NMAX = 5552;

        internal long adler32(long adler, byte[] buf, int index, int len)
        {
            if (buf == null)
            {
                return 1L;
            }

            long s1 = adler & 0xffff;
            long s2 = (adler >> 16) & 0xffff;
            int k;

            while (len > 0)
            {
                k = len < NMAX ? len : NMAX;
                len -= k;
                while (k >= 16)
                {
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    k -= 16;
                }
                if (k != 0)
                {
                    do
                    {
                        s1 += (buf[index++] & 0xff); s2 += s1;
                    }
                    while (--k != 0);
                }
                s1 %= BASE;
                s2 %= BASE;
            }
            return (s2 << 16) | s1;
        }

    }


}
