using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Launcher
{
    public class PacketOut : MemoryStream
    {
        public static int SizeLen = sizeof(uint); // 4 byte
        public static bool OpcodeInLen = false; // Opcode included in the size?
        public int OpcodeLen = sizeof(ushort); // 2 bytes
        public static bool reversed = false;   // Write everything in reverse?

        public ulong Opcode;

        protected PacketOut()
        {
        }

        public PacketOut(byte opcode)
            : base(sizeof(byte) + SizeLen)
        {
            WriteSize();

            Opcode = opcode;
            WriteByte(opcode);
            OpcodeLen = sizeof(byte);
        }

        public PacketOut(ushort opcode)
            : base(sizeof(ushort) + SizeLen)
        {
            WriteSize();

            Opcode = opcode;
            OpcodeLen = sizeof(ushort);

            if (!reversed) WriteUInt16(opcode);
            else WriteUInt16Reverse(opcode);
        }

        public PacketOut(uint opcode)
            : base(sizeof(uint) + SizeLen)
        {
            WriteSize();

            Opcode = opcode;
            OpcodeLen = sizeof(uint);

            if (!reversed) WriteUInt32(opcode);
            else WriteUInt32Reverse(opcode);
        }

        public PacketOut(ulong opcode)
            : base(sizeof(ulong) + SizeLen)
        {
            WriteSize();

            Opcode = opcode;
            OpcodeLen = sizeof(ulong);

            if (!reversed) WriteUInt64(opcode);
            else WriteUInt64Reverse(opcode);
        }

        public void WriteSize()
        {
            for (int i = 0; i < SizeLen; ++i)
                WriteByte(0);
        }

        #region IPacket Members

        #endregion

        public virtual void WriteUInt16(ushort val)
        {
            WriteByte((byte)(val >> 8));
            WriteByte((byte)(val & 0xff));
        }

        public virtual void WriteUInt16Reverse(ushort val)
        {
            WriteByte((byte)(val & 0xff));
            WriteByte((byte)(val >> 8));
        }

        public virtual void WriteUInt32(uint val)
        {
            WriteByte((byte)(val >> 24));
            WriteByte((byte)((val >> 16) & 0xff));
            WriteByte((byte)((val & 0xffff) >> 8));
            WriteByte((byte)((val & 0xffff) & 0xff));
        }

        public virtual void WriteUInt32Reverse(uint val)
        {
            WriteByte((byte)((val & 0xffff) & 0xff));
            WriteByte((byte)((val & 0xffff) >> 8));
            WriteByte((byte)((val >> 16) & 0xff));
            WriteByte((byte)(val >> 24));
        }

        public virtual void WriteUInt64(ulong val)
        {
            WriteByte((byte)(val >> 56));
            WriteByte((byte)((val >> 48) & 0xff));
            WriteByte((byte)((val >> 40) & 0xff));
            WriteByte((byte)((val >> 32) & 0xff));
            WriteByte((byte)((val >> 24) & 0xff));
            WriteByte((byte)((val >> 16) & 0xff));
            WriteByte((byte)((val >> 8) & 0xff));
            WriteByte((byte)(val & 0xff));
        }

        public virtual void WriteInt16(short val)
        {
            byte[] b = BitConverter.GetBytes(val);

            for (int i = b.Length; i > 0; --i)
                WriteByte(b[i - 1]);
        }

        public virtual void WriteInt32(int val)
        {
            byte[] b = BitConverter.GetBytes(val);

            for (int i = b.Length; i > 0; --i)
                WriteByte(b[i - 1]);
        }

        public virtual void WriteInt32Reverse(int val)
        {
            byte[] b = BitConverter.GetBytes(val);

            for (int i = 0; i < b.Length; ++i)
                WriteByte(b[i]);
        }

        public virtual void WriteInt64(long val)
        {
            byte[] b = BitConverter.GetBytes(val);

            for (int i = b.Length; i > 0; --i)
                WriteByte(b[i - 1]);
        }

        public virtual void WriteInt64Reverse(long val)
        {
            byte[] b = BitConverter.GetBytes(val);

            for (int i = 0; i < b.Length; ++i)
                WriteByte(b[i]);
        }

        public virtual void WriteFloat(float val)
        {
            foreach (byte b in BitConverter.GetBytes(val))
                WriteByte(b);
        }

        public virtual void WriteUInt64Reverse(ulong val)
        {
            WriteByte((byte)(val & 0xff));
            WriteByte((byte)((val >> 8) & 0xff));
            WriteByte((byte)((val >> 16) & 0xff));
            WriteByte((byte)((val >> 24) & 0xff));
            WriteByte((byte)((val >> 32) & 0xff));
            WriteByte((byte)((val >> 40) & 0xff));
            WriteByte((byte)((val >> 48) & 0xff));
            WriteByte((byte)(val >> 56));
        }

        public virtual byte GetChecksum()
        {
            byte val = 0;
            byte[] buf = GetBuffer();

            for (int i = 0; i < Position - 6; ++i)
            {
                val += buf[i + 8];
            }

            return val;
        }

        public virtual void Fill(byte val, int num)
        {
            for (int i = 0; i < num; ++i)
            {
                WriteByte(val);
            }
        }

        public virtual ulong WritePacketLength()
        {
            Position = 0;

            long size = OpcodeInLen == false ? (Length - OpcodeLen) : Length;

            if (!reversed)
            {
                switch (SizeLen)
                {
                    case sizeof(byte):
                        WriteByte((byte)(size));
                        break;

                    case sizeof(ushort):
                        WriteUInt16((ushort)(size));
                        break;

                    case sizeof(uint):
                        WriteUInt32((uint)(size));
                        break;

                    case sizeof(ulong):
                        WriteUInt64((ulong)(size));
                        break;

                }
            }
            else
            {
                switch (SizeLen)
                {
                    case sizeof(byte):
                        WriteByte((byte)(size));
                        break;

                    case sizeof(ushort):
                        WriteUInt16Reverse((ushort)(size));
                        break;

                    case sizeof(uint):
                        WriteUInt32Reverse((uint)(size));
                        break;

                    case sizeof(ulong):
                        WriteUInt64Reverse((ulong)(size));
                        break;
                }
            }
            //Capacity = (int)Length;

            return (ulong)(size);
        }

        public virtual void WritePascalString(string str)
        {
            if (str == null || str.Length <= 0)
            {
                WriteByte(0);
                return;
            }

            byte[] bytes = Encoding.ASCII.GetBytes(str);
            WriteByte((byte)bytes.Length);
            Write(bytes, 0, bytes.Length);
        }

        public virtual void WriteStringToZero(string str)
        {
            if (str == null || str.Length <= 0)
            {
                WriteByte(1);
            }
            else
            {
                byte[] bytes = Encoding.ASCII.GetBytes(str);
                WriteByte((byte)(bytes.Length + 1));
                Write(bytes, 0, bytes.Length);
            }

            WriteByte(0);
        }

        public virtual void WriteString(string str)
        {
            WriteUInt32((uint)str.Length);
            WriteStringBytes(str);
        }

        public virtual void WriteStringBytes(string str)
        {
            if (str.Length <= 0)
                return;

            byte[] bytes = Encoding.UTF8.GetBytes(str);
            Write(bytes, 0, bytes.Length);
        }

        public virtual void WriteString(string str, int maxlen)
        {
            if (str.Length <= 0)
                return;

            byte[] bytes = Encoding.UTF8.GetBytes(str);
            Write(bytes, 0, bytes.Length < maxlen ? bytes.Length : maxlen);
        }

        public virtual void WriteParsedString(string str)
        {
            byte[] data = Encoding.Unicode.GetBytes(str);//each char becomes 2 bytes

            for (int i = 0; i < data.Length; ++i)
            {
                WriteByte(data[i]);
            }
            WriteByte(0x00);//null terminated string
            WriteByte(0x00);//
        }

        public virtual void WriteParsedString(string str, int maxlen)
        {
            byte[] data = Encoding.Unicode.GetBytes(str);//each char becomes 2 bytes
            int i = 0;
            for (; i < data.Length && i < maxlen; ++i)
            {
                WriteByte(data[i]);
            }

            if (i < maxlen)
                for (; i < maxlen; ++i)
                    WriteByte(0);

            WriteByte(0x00);//null terminated string
            WriteByte(0x00);//
        }

        public virtual void FillString(string str, int len)
        {
            long pos = Position;

            Fill(0x0, len);

            if (str == null)
                return;

            Position = pos;

            if (str.Length <= 0)
            {
                Position = pos + len;
                return;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(str);
            Write(bytes, 0, len > bytes.Length ? bytes.Length : len);
            Position = pos + len;
        }

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}
