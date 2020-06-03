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
    public enum PackStruct
    {
        OpcodeAndSize = 0x01,
        SizeAndOpcode = 0x02
    };

    public class PacketOut : MemoryStream
    {

        private static readonly Encoding ISO8859_1 = Encoding.GetEncoding("iso-8859-1");

        public static int SizeLen = sizeof(uint); // 4 byte
        public static bool OpcodeInLen = false;  // Opcode on Size ?
        public static bool SizeReverse = false;   // Reversed Size ?
        public static bool OpcodeReverse = false; // Reversed Opcode ?
        public static int InterOpcodeSizeCount = 0; // Space Size 
        public static bool SizeInLen = true; // Size in Size ?
        public static PackStruct Struct = PackStruct.SizeAndOpcode;

        public int OpcodeLen = sizeof(ushort); // 2 bytes
        public ulong Opcode;

        public bool Finalized { get; private set; }

        protected PacketOut()
        {
        }

        public PacketOut(byte opcode)
            : base(sizeof(byte) + SizeLen)
        {
            Opcode = opcode;
            OpcodeLen = sizeof(byte);

            if (Struct == PackStruct.SizeAndOpcode) WriteSize();
            else if (Struct == PackStruct.OpcodeAndSize) WriteByte(opcode);

            for (int i = 0; i < InterOpcodeSizeCount; ++i)
                WriteByte(0);

            if (Struct == PackStruct.SizeAndOpcode) WriteByte(opcode);
            else if (Struct == PackStruct.OpcodeAndSize) WriteSize();
        }

        public PacketOut(byte opcode, int initialCapacity)
        : base(sizeof(byte) + SizeLen + initialCapacity)
        {
            Opcode = opcode;
            OpcodeLen = sizeof(byte);

            if (Struct == PackStruct.SizeAndOpcode) WriteSize();
            else if (Struct == PackStruct.OpcodeAndSize) WriteByte(opcode);

            for (int i = 0; i < InterOpcodeSizeCount; ++i)
                WriteByte(0);

            if (Struct == PackStruct.SizeAndOpcode) WriteByte(opcode);
            else if (Struct == PackStruct.OpcodeAndSize) WriteSize();
        }

        /*
        public PacketOut(ushort opcode)
            : base(sizeof(ushort) + SizeLen)
        {
            if (Struct == PackStruct.SizeAndOpcode) WriteSize();
            else if (Struct == PackStruct.OpcodeAndSize)
                if (!OpcodeReverse) WriteUInt16(opcode);
                else WriteUInt16R(opcode);

            for (int i = 0; i < InterOpcodeSizeCount; ++i)
                WriteByte(0);

            if (Struct == PackStruct.SizeAndOpcode) 
                if (!OpcodeReverse) WriteUInt16(opcode);
                else WriteUInt16R(opcode);
            else if (Struct == PackStruct.OpcodeAndSize) WriteSize();
        }

        public PacketOut(uint opcode)
            : base(sizeof(uint) + SizeLen)
        {
            if (Struct == PackStruct.SizeAndOpcode) WriteSize();
            else if (Struct == PackStruct.OpcodeAndSize)
                if (!OpcodeReverse) WriteUInt32(opcode);
                else WriteUInt32R(opcode);

            for (int i = 0; i < InterOpcodeSizeCount; ++i)
                WriteByte(0);

            if (Struct == PackStruct.SizeAndOpcode)
                if (!OpcodeReverse) WriteUInt32(opcode);
                else WriteUInt32R(opcode);
            else if (Struct == PackStruct.OpcodeAndSize) WriteSize();
        }

        public PacketOut(ulong opcode)
            : base(sizeof(ulong) + SizeLen)
        {
            if (Struct == PackStruct.SizeAndOpcode) WriteSize();
            else if (Struct == PackStruct.OpcodeAndSize)
                if (!OpcodeReverse) WriteUInt64(opcode);
                else WriteUInt64R(opcode);

            for (int i = 0; i < InterOpcodeSizeCount; ++i)
                WriteByte(0);

            if (Struct == PackStruct.SizeAndOpcode)
                if (!OpcodeReverse) WriteUInt64(opcode);
                else WriteUInt64R(opcode);
            else if (Struct == PackStruct.OpcodeAndSize) WriteSize();
        }
        */


        // Planning to use this in the future to pool smaller packets
        public void Reset()
        {
            /*
            Position = 0;
            Opcode = 0;
            OpcodeLen = sizeof (ushort);
            SizeInLen = true;
            InterOpcodeSizeCount = 0;
            SizeLen = sizeof (uint);
            */
        }
        

        public void WriteSize()
        {
            for (int i = 0; i < SizeLen; ++i)
                WriteByte(0);
        }

        public static string Hex(byte[] dump, int start, int len, int? current = null)
        {
            var hexDump = new StringBuilder();

            hexDump.AppendLine("|------------------------------------------------|----------------|");
            hexDump.AppendLine("|00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F |0123456789ABCDEF|");
            hexDump.AppendLine("|------------------------------------------------|----------------|");
            try
            {
                int end = start + len;
                for (int i = start; i < end; i += 16)
                {
                    StringBuilder text = new StringBuilder();
                    StringBuilder hex = new StringBuilder();


                    for (int j = 0; j < 16; j++)
                    {
                        if (j + i < end)
                        {
                            byte val = dump[j + i];

                            if (current.HasValue && current.Value == j + i)
                                hex.Append("[" + dump[j + i].ToString("X2") + "]");
                            else
                                hex.Append(dump[j + i].ToString("X2"));

                            hex.Append(" ");
                            if (val >= 32 && val < 127)
                            {
                                if (current.HasValue && current.Value == j + i)
                                    text.Append("[" + (char)val + "]");
                                else
                                    text.Append((char)val);
                            }
                            else
                            {
                                if (current.HasValue && current.Value == j + i)
                                    text.Append("[.]");
                                else
                                    text.Append(".");
                            }
                        }
                    }

                    hexDump.AppendLine("|" + hex.ToString().PadRight(48) + "|" + text.ToString().PadRight(16) + "|");
                }
            }
            catch (Exception)
            {
                // Log.Error("HexDump", e.ToString());
            }

            hexDump.Append("-------------------------------------------------------------------");
            return hexDump.ToString();
        }

        public string GetHexString()
        {
            return "[Server] packet : (0x" + ((int)Opcode).ToString("X").PadLeft(2, '0') + ") " + " Size = " 
                + Position + "\r\n" + Hex(ToArray(), 0, (int)Position) + "\r\n";
        }

        public virtual ulong WritePacketLength()
        {
            if (Struct == PackStruct.OpcodeAndSize)
                Position = OpcodeLen + InterOpcodeSizeCount;
            else if (Struct == PackStruct.SizeAndOpcode)
                Position = 0;
            else
                Position = 0;


            long size = OpcodeInLen == false ? (Length - OpcodeLen) : Length;
            if (!SizeInLen) size -= SizeLen;

            switch (SizeLen)
            {
                case sizeof(byte):
                    WriteByte((byte)size);
                    break;

                case sizeof(ushort):
                    if (!SizeReverse) WriteUInt16((ushort)size);
                    else WriteUInt16R((ushort)size);
                    break;

                case sizeof(uint):
                    if (!SizeReverse) WriteUInt32((uint)size);
                    else WriteUInt32R((uint)size);
                    break;

                case sizeof(ulong):
                    if (!SizeReverse) WriteUInt64((ulong)size);
                    else WriteUInt64R((ulong)size);
                    break;
            }

            Capacity = (int)Length;

            Finalized = true;

            return (ulong)size;
        }

        #region IPacket Members

        #endregion

        public virtual void Write(byte[] val)
        {
            Write(val, 0, val.Length);
        }

        public virtual void WriteUInt16(ushort val)
        {
            WriteByte((byte)(val >> 8));
            WriteByte((byte)(val & 0xff));
        }
        public virtual void WriteUInt16R(ushort val)
        {
            WriteByte((byte)(val & 0xff));
            WriteByte((byte)(val >> 8));
        }

        public virtual void WriteInt16(short val)
        {
            byte[] b = BitConverter.GetBytes(val);

            for (int i = b.Length; i > 0; --i)
                WriteByte(b[i-1]);
        }

        public virtual void WriteInt16R(short val)
        {
            byte[] b = BitConverter.GetBytes(val);

            for (byte i = 0; i < b.Length; ++i)
                WriteByte(i);
        }

        public virtual void WriteUInt24(uint val)
        {
            WriteByte((byte)((val >> 16) & 0xff));
            WriteByte((byte)((val & 0xffff) >> 8));
            WriteByte((byte)((val & 0xffff) & 0xff));
        }

        public virtual void WriteUInt24R(uint val)
        {
            WriteByte((byte)((val & 0xffff) & 0xff));
            WriteByte((byte)((val & 0xffff) >> 8));
            WriteByte((byte)((val >> 16) & 0xff));
        }

        public virtual void WriteUInt32(uint val)
        {
            WriteByte((byte)(val >> 24));
            WriteByte((byte)((val >> 16) & 0xff));
            WriteByte((byte)((val & 0xffff) >> 8));
            WriteByte((byte)((val & 0xffff) & 0xff));
        }
        public virtual void WriteUInt32R(uint val)
        {
            WriteByte((byte)((val & 0xffff) & 0xff));
            WriteByte((byte)((val & 0xffff) >> 8));
            WriteByte((byte)((val >> 16) & 0xff));
            WriteByte((byte)(val >> 24));
        }

        public virtual void WriteInt32(int val)
        {
            byte[] b = BitConverter.GetBytes(val);

            for (int i = 0; i < b.Length; ++i)
                WriteByte(b[i]);
        }
        public virtual void WriteInt32R(int val)
        {
            byte[] b = BitConverter.GetBytes(val);

            for (int i = b.Length; i > 0; --i)
                WriteByte(b[i - 1]);
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
        public virtual void WriteUInt64R(ulong val)
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

        public virtual void WriteInt64(long val)
        {
            byte[] b = BitConverter.GetBytes(val);

            for (int i = 0; i < b.Length; ++i)
                WriteByte(b[i]);
        }
        public virtual void WriteInt64R(long val)
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

        public virtual void WriteVarUInt(uint val)
        {
            if (val == 0)
            {
                WriteByte(0);
                return;
            }

            while (val > 0)
            {
                WriteByte((byte)((val & 0x7F) ^ ((val > 0x7F) ? 0x80 : 0x00)));
                val = val >> 7;
            }
        }

        public virtual void WriteZigZag(int val)
        {
            byte sign = (byte)(val < 0 ? 1 : 0);
            if (sign == 1)
                val++;
            val = Math.Abs(val);

            WriteByte((byte)(((val << 1) & 0x7F) ^ ((val > 0x3F) ? 0x80 : 0x00) + sign));
            val = val >> 6;

            while (val > 0)
            {
                WriteByte((byte)((val & 0x7F) ^ ((val > 0x7F) ? 0x80 : 0x00)));
                val = val >> 7;
            }
        }

        public virtual byte GetChecksum()
        {
            byte val = 0;
            byte[] buf = GetBuffer();

            for (int i = 0; i < Position - 6; ++i)
                val += buf[i + 8];

            return val;
        }

        public virtual void Fill(byte val, int num)
        {
            for (int i = 0; i < num; ++i)
                WriteByte(val);
        }

        /// <summary>
        /// Writes a concatenation of given strings with ISO-8859-1 encoding.
        /// </summary>
        /// <param name="str">String array, may be null or contain null elements</param>
        /// <remarks>
        /// This method is preffered for performance purposes over $"" or string.Concat.
        /// </remarks>
        public void WritePascalString(params string[] str)
        {
            if (str == null)
            {
                WriteByte(0);
                return;
            }
            
            int length = 0;
            for (int i = 0; i < str.Length; i++)
                length += str[i] != null ? str[i].Length : 0;
#if DEBUG
            if (ISO8859_1.GetBytes(string.Concat(str)).Length != length)
                throw new Exception(ISO8859_1.GetBytes(string.Concat(str)).Length + " should be " + length);
#endif
            WriteByte((byte)length);

            for (int i = 0; i < str.Length; i++)
            {
                string part = str[i];
                if (part != null)
                    Write(ISO8859_1.GetBytes(part), 0, part.Length);
            }
        }

        public virtual void WritePascalString(string str)
        {
            if (str == null || str.Length <= 0)
            {
                WriteByte(0);
                return;
            }

            byte[] bytes = ISO8859_1.GetBytes(str);
            WriteByte((byte)bytes.Length);
            Write(bytes, 0, bytes.Length);
        }

        public virtual void WriteVarIntString(string str)
        {
            if (str == null || str.Length <= 0)
            {
                WriteByte(0);
                return;
            }

            byte[] bytes = ISO8859_1.GetBytes(str);
            WriteVarUInt((uint)str.Length);
            Write(bytes, 0, bytes.Length);
        }

        public virtual void WriteShortString(string str)
        {
            if (str == null || str.Length <= 0)
            {
                WriteUInt16(0);
                return;
            }

            byte[] bytes = ISO8859_1.GetBytes(str);
            WriteUInt16((ushort)bytes.Length);
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
                byte[] bytes = ISO8859_1.GetBytes(str);
                WriteByte((byte)(bytes.Length + 1));
                Write(bytes, 0, bytes.Length);
            }

            WriteByte(0);
        }

        public virtual void WriteShortStringToZero(string str)
        {
            if (str == null || str.Length <= 0)
            {
                WriteUInt16(1);
            }
            else
            {
                byte[] bytes = ISO8859_1.GetBytes(str);
                WriteUInt16((byte)(bytes.Length + 1));
                Write(bytes, 0, bytes.Length);
            }

            WriteByte(0);
        }

        public void WriteCString(string str)
        {
            if (string.IsNullOrEmpty(str))
            { 
                WriteByte(0);
                return;
            }

            byte[] bytes = ISO8859_1.GetBytes(str);
            Write(bytes, 0, bytes.Length);
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

            byte[] bytes = ISO8859_1.GetBytes(str);
            Write(bytes, 0, bytes.Length);
        }
        public virtual void WriteString(string str, int maxlen)
        {
            if (str.Length <= 0)
                return;

            byte[] bytes = ISO8859_1.GetBytes(str);
            Write(bytes, 0, bytes.Length < maxlen ? bytes.Length : maxlen);
        }
        public virtual void WriteUnicodeString(string str)
        {
            byte[] data = Encoding.Unicode.GetBytes(str);//each char becomes 2 bytes

            for (int i = 0; i < data.Length; ++i)
                WriteByte(data[i]);

            WriteByte(0x00);//null terminated string
            WriteByte(0x00);//
        }
        public virtual void WriteUnicodeString(string str, int maxlen)
        {
            byte[] data = Encoding.Unicode.GetBytes(str);//each char becomes 2 bytes
            int i = 0;
            for (; i < data.Length && i < maxlen; ++i)
                WriteByte(data[i]);

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

            byte[] bytes = ISO8859_1.GetBytes(str);
            Write(bytes, 0, len > bytes.Length ? bytes.Length : len);
            Position = pos + len;
        }

        public virtual void WriteVector2(Vector2 Vector)
        {
            WriteFloat(Vector.X);
            WriteFloat(Vector.Y);
        }

        public virtual void WriteVector3(Vector3 Vector)
        {
            WriteFloat(Vector.X);
            WriteFloat(Vector.Y);
            WriteFloat(Vector.Z);
        }

        public virtual void WriteQuaternion(Quaternion Quat)
        {
            WriteFloat(Quat.X);
            WriteFloat(Quat.Y);
            WriteFloat(Quat.Z);
            WriteFloat(Quat.W);
        }

        public virtual void WriteHexStringBytes(string hexString)
        {
            int length = hexString.Length / 2;

            if ((hexString.Length % 2) == 0)
            {
                for (int i = 0; i < length; i++)
                    WriteByte(Convert.ToByte(hexString.Substring(i * 2, 2), 16));
            }
            else
            {
                WriteByte(0);
            }
        }

        public virtual void WritePacketString(string packet)
        {
            packet = packet.Replace(" ", string.Empty);

            using (StringReader Reader = new StringReader(packet))
            {
                string Line;
                while ((Line = Reader.ReadLine()) != null)
                {
                    WriteHexStringBytes(Line.Substring(1, Line.IndexOf("|", 2)-1));
                }
            }
        }

        public override string ToString()
        {
            return GetType().Name;
        }

        #region Mythic

        private static readonly ThreadLocal<byte[]> ThreadLocalKey = new ThreadLocal<byte[]>(() => new byte[256]); 

        public static void EncryptMythicRC4(byte[] key, byte[] packet, int offset, int packetLen)
        {
            try
            {
                int x = 0;
                int y = 0;

                int pos;
                byte tmp;

                int midpoint = packetLen / 2;

                byte[] k = ThreadLocalKey.Value;

                Buffer.BlockCopy(key, 0, k, 0, 256);

                for (pos = midpoint; pos < packetLen; ++pos)
                {
                    x = (x + 1) & 255;
                    y = (y + k[x]) & 255;

                    tmp = k[x];
                    k[x] = k[y];
                    k[y] = tmp;

                    tmp = (byte)((k[x] + k[y]) & 255);
                    y = (y + packet[pos+offset]) & 255;
                    packet[pos+offset] ^= k[tmp];
                }

                for (pos = 0; pos < midpoint; ++pos)
                {
                    x = (x + 1) & 255;
                    y = (y + k[x]) & 255;

                    tmp = k[x];
                    k[x] = k[y];
                    k[y] = tmp;

                    tmp = (byte)((k[x] + k[y]) & 255);
                    y = (y + packet[pos+offset]) & 255;
                    packet[pos+offset] ^= k[tmp];
                }
            }
            catch (Exception e)
            {
                Log.Error("PacketOut", "EncryptMythicRC4 : Failled !" + e);
            }
        }

        #endregion

        #region Gamebryo

        public void WriteGamebryoSize()
        {
            Position = 0;
            byte[] Total = ToArray();

            if (Total.Length - 1 < 0x80)
                WriteByte((byte)(Total.Length));
            else
            {
                int Size = Total.Length;
                int Offset = 1;

                while (Size >= (128*2))
                {
                    Size -= 128;
                    ++Offset;
                }

                WriteByte((byte)Size);
                WriteByte((byte)Offset);
            }

            Write(Total, 0, Total.Length);
        }

        #endregion
    }
}