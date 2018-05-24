/*
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */
 
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace FrameWork
{
    public class PacketIn : MemoryStream
    {
        public ulong Opcode { get; set; }
        public ulong Size { get; set; }

        public PacketIn(int size)
            : base(size)
        {
        }

        public PacketIn(byte[] buf, int start, int size)
            : base(buf, start, size)
        {
        }

        public PacketIn(byte[] buf, int start, int size, bool writeable, bool exposable)
            : base (buf, start, size, writeable, exposable)
        {
            
        }

        public byte[] Read(int size)
        {
            byte[] buf = new byte[size];
            Read(buf, 0, size);

            return buf;
        }
        public void Skip(long num)
        {
            Seek(num, SeekOrigin.Current);
        }
        public long Remain()
        {
            return Length - Position;
        }

        public virtual byte GetUint8()
        {
            return (byte)ReadByte();
        }

        public virtual ushort GetUint16()
        {
            return Marshal.ConvertToUInt16(GetUint8(), GetUint8());
        }
        public virtual ushort GetUint16R()
        {
            byte v1 = GetUint8();
            byte v2 = GetUint8();

            return Marshal.ConvertToUInt16(v2, v1);
        }

        public virtual short GetInt16()
        {
            byte[] tmp = { GetUint8() , GetUint8()  };
            return BitConverter.ToInt16(tmp, 0);
        }
        public virtual short GetInt16R()
        {
            byte v1 = GetUint8();
            byte v2 = GetUint8();

            byte[] tmp = { v2, v1 };
            return BitConverter.ToInt16(tmp, 0);
        }

        public virtual uint GetUint32()
        {
            byte v1 = GetUint8();
            byte v2 = GetUint8();
            byte v3 = GetUint8();
            byte v4 = GetUint8();

            return Marshal.ConvertToUInt32(v1, v2, v3, v4);
        }
        public virtual uint GetUint32R()
        {
            byte v1 = GetUint8();
            byte v2 = GetUint8();
            byte v3 = GetUint8();
            byte v4 = GetUint8();

            return Marshal.ConvertToUInt32(v4, v3, v2, v1);
        }

        public virtual int GetInt32()
        {
            byte[] tmp = { GetUint8(), GetUint8(), GetUint8(), GetUint8() };
            return BitConverter.ToInt32(tmp, 0);
        }
        public virtual int GetInt32R()
        {
            byte v1 = GetUint8();
            byte v2 = GetUint8();
            byte v3 = GetUint8();
            byte v4 = GetUint8();

            byte[] tmp = { v4, v3, v2, v1 };
            return BitConverter.ToInt32(tmp, 0);
        }

        public ulong GetUint64()
        {
            ulong value = (GetUint32() << 24) + (GetUint32());
            return value;
        }
        public ulong GetUint64R()
        {

            ulong value = (GetUint32()) + (GetUint32() << 24);
            return value;
        }

        public long GetInt64()
        {
            byte[] tmp = Read(8);
            return BitConverter.ToInt64(tmp, 0);
        }

        public long GetInt64R()
        {
            byte[] tmp = Read(8);
            Array.Reverse(tmp);
            return BitConverter.ToInt64(tmp, 0);
        }

        public int GetIntClamped(int min, int max)
        {
            unchecked
            {
                int eax = 0, ecx = 0, edx = 0;

                eax = max;
                eax = eax - min;
                if (eax < 0)
                    eax = min - max;

                eax++;
                edx = 0x20;
                max = eax;
                ecx = 0;

                //find out how many bits min->max fits in
                while (eax >= 1 << ecx && ecx < 32)
                    ecx++;

                edx = ecx;

                int val;
                if (edx <= 8)
                    val = ReadByte();
                else if (edx <= 16)
                    val = GetInt16();
                else if (edx <= 24)
                    throw new ArgumentException("24");
                else if (edx <= 32)
                    val = GetInt32();
                else
                    throw new ArgumentException("> 32");

                return min + val;
            }
        }

        public float GetFloat()
        {
            byte[] b = new byte[4];
            b[0] = (byte)ReadByte();
            b[1] = (byte)ReadByte();
            b[2] = (byte)ReadByte();
            b[3] = (byte)ReadByte();

            return BitConverter.ToSingle(b, 0);
        }

        private char ReadPs()
        {
            if (Length >= Position + 2)
                return BitConverter.ToChar(new byte[] { GetUint8(), GetUint8() }, 0);

            return (char)0;
        }
        public virtual string GetString()
        {
            int len = (int)GetUint32();

            var buf = new byte[len];
            Read(buf, 0, len);

            return Marshal.ConvertToString(buf);
        }

        public virtual string GetString16()
        {
            var len = GetUint16();

            var buf = new byte[len];
            Read(buf, 0, len);

            return Marshal.ConvertToString(buf);
        }
        public virtual string GetString(int maxlen)
        {
            var buf = new byte[maxlen];
            Read(buf, 0, maxlen);

            return Marshal.ConvertToString(buf);
        }
        public virtual string GetPascalString()
        {
            return GetString(GetUint8());
        }
        public virtual string GetUnicodeString()
        {
            string tmp = "";

            char tmp2 = ReadPs();
            while (tmp2 != 0x00)
            {
                tmp += tmp2;
                tmp2 = ReadPs();
            }

            return tmp;
        }
        public virtual string GetStringToZero()
        {
            string value = "";

            while (Position < Length)
            {
                char c = (char)ReadByte();
                if (c == 0)
                    break;
                value += c;

            }

            return value;
        }

        public virtual Vector2 GetVector2()
        {
            return new Vector2(GetFloat(), GetFloat());
        }
        public virtual Vector3 GetVector3()
        {
            return new Vector3(GetFloat(), GetFloat(), GetFloat());
        }
        public virtual Quaternion GetQuaternion()
        {
            return new Quaternion(GetFloat(), GetFloat(), GetFloat(), GetFloat());
        }

        public override string ToString()
        {
            return GetType().Name;
        }

        #region Mythic

        public int DecodeMythicSize()
        {
            int mSize = 0;
            int mByteCount = 0;
            int mByte = 0;

            mByte = ReadByte();
            while ((mByte & 0x80) == 0x80)
            {
                //Log.Debug("readSize", "mByte = " + mByte);
                mByte ^= 0x80;
                mSize = (mSize | (mByte << (7 * mByteCount)));

                if (Length == Capacity)
                    return 0;


                mByte = ReadByte();
                mByteCount++;
            }

            mSize = (mSize | (mByte << (7 * mByteCount)));
            return mSize;
        }

        private static readonly ThreadLocal<byte[]> ThreadLocalKey = new ThreadLocal<byte[]>(() => new byte[256]);

        public static void DecryptMythicRC4(byte[] key, byte[] packetBuffer, int offset, int length)
        {
            try
            {
                int x = 0;
                int y = 0;
                int pos;
                byte tmp;

                byte[] k = ThreadLocalKey.Value;

                Buffer.BlockCopy(key, 0, k, 0, 256);

                int midpoint = length / 2;

                for (pos = midpoint; pos < length; ++pos)
                {
                    x = (x + 1) & 255;
                    y = (y + k[x]) & 255;

                    tmp = k[x];

                    k[x] = k[y];
                    k[y] = tmp;

                    tmp = (byte)(( k[x] + k[y] ) & 255);
                    packetBuffer[pos + offset] ^= k[tmp];
                    y = (y + packetBuffer[pos + offset]) & 255;
                }

                for (pos = 0; pos < midpoint; ++pos)
                {
                    x = (x + 1) & 255;
                    y = (y + k[x]) & 255;

                    tmp = k[x];

                    k[x] = k[y];
                    k[y] = tmp;

                    tmp = (byte)((k[x] + k[y]) & 255);
                    packetBuffer[pos + offset] ^= k[tmp];
                    y = (y + packetBuffer[pos + offset]) & 255; 
                }
            }
            catch(Exception e)
            {
                Log.Error("PacketIn","DecryptMythicRC4 : Failed !" + e);
            }
        }

        #endregion

        #region GameBryo

        public int DecodeGamebryoSize()
        {
            int Size = GetUint8();

            if (Size >= 128)
                Size += ((GetUint8() - 1) * 128) + 2;

            return Size;
        }

        #endregion
    }
}
