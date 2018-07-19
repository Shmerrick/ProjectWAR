using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Launcher
{
    public class PacketIn : MemoryStream
    {
        // Opcode of the packet
        public ulong Opcode = 0;
        public ulong Size = 0;

        public PacketIn(int size)
            : base(size)
        {
        }

        public PacketIn(byte[] buf, int start, int size)
            : base(buf, start, size)
        {
        }

        public virtual byte GetUint8()
        {
            return (byte)ReadByte();
        }

        // UInt16
        public virtual ushort GetUint16()
        {
            var v1 = (byte)ReadByte();
            var v2 = (byte)ReadByte();

            return Marshal.ConvertToUInt16(v1, v2);
        }

        // UInt16 Reversed
        public virtual ushort GetUint16Reversed()
        {
            var v1 = (byte)ReadByte();
            var v2 = (byte)ReadByte();

            return Marshal.ConvertToUInt16(v2, v1);
        }

        public virtual short GetInt16()
        {
            byte[] tmp = { GetUint8(), GetUint8() };
            return BitConverter.ToInt16(tmp, 0);
        }

        // UInt32
        public virtual uint GetUint32()
        {
            var v1 = (byte)ReadByte();
            var v2 = (byte)ReadByte();
            var v3 = (byte)ReadByte();
            var v4 = (byte)ReadByte();

            return Marshal.ConvertToUInt32(v1, v2, v3, v4);
        }

        public virtual int GetInt32()
        {
            byte[] tmp = { GetUint8(), GetUint8(), GetUint8(), GetUint8() };
            return BitConverter.ToInt32(tmp, 0);
        }

        // Jump bytes
        public void Skip(long num)
        {
            Seek(num, SeekOrigin.Current);
        }

        public virtual string GetString()
        {
            int len = (int)GetUint32();

            var buf = new byte[len];
            Read(buf, 0, len);

            return Marshal.ConvertToString(buf);
        }

        // Return a string
        public virtual string GetString(int maxlen)
        {
            var buf = new byte[maxlen];
            Read(buf, 0, maxlen);

            return Marshal.ConvertToString(buf);
        }

        // Read in pascal, until you find a 0
        public virtual string GetPascalString()
        {
            return GetString(GetUint8());
        }

        private char ReadPs() // Li 2 byte to build a tank
        {
            if (Length >= Position + 2)
            {
                return BitConverter.ToChar(new byte[] { GetUint8(), GetUint8() }, 0);
            }
            return (char)0;
        }

        public virtual string GetParsedString()
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

        // Int reversed
        public virtual uint GetUint32Reversed()
        {
            var v1 = (byte)ReadByte();
            var v2 = (byte)ReadByte();
            var v3 = (byte)ReadByte();
            var v4 = (byte)ReadByte();

            return Marshal.ConvertToUInt32(v4, v3, v2, v1);
        }

        public override string ToString()
        {
            return GetType().Name;
        }

        public ulong GetUint64()
        {

            ulong value = (GetUint32() << 24) + (GetUint32());
            return value;
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
    }
}

