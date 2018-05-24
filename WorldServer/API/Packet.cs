using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldServer.API
{
    public class Packet
    {
        public int _size;
        protected int _offset;
        protected byte[] _data;
        private byte[] _tmpDecKey = new byte[256];

        public int Size
        {
            get { return _size; }
        }

        public byte[] Data
        {
            get { return _data; }
        }


        public int Offset
        {
            get { return _offset; }
            set
            {
                _offset = value;
            }
        }
        public Packet()
        {
            _data = new byte[0xFFFF];
        }

        public Packet(int size)
        {
            _data = new byte[size];
        }

        public string ReadString(int size)
        {
            if (_offset + size > _data.Length)
                size = _data.Length - _offset;
            string result = System.Text.ASCIIEncoding.ASCII.GetString(_data, _offset, size);
            int cstrIndex = result.IndexOf('\0');
            if (cstrIndex > 0)
                result = result.Remove(cstrIndex);

            _offset += size;
            return result;
        }
        public void Skip(int size)
        {
            _offset += size;
        }

        public byte ReadByte()
        {
            return _data[_offset++];
        }

        public bool Read(byte[] destBuffer, int size)
        {
            //TODO: Check buffer size
            if (size + _offset > _data.Length)
                return false;

            System.Buffer.BlockCopy(_data, _offset, destBuffer, 0, size);
            _offset += size;
            return true;
        }


        public ushort ReadUInt16()
        {
            ushort result = (ushort)(_data[_offset] << 8 | _data[_offset + 1]);
            _offset += 2;
            return result;
        }

        public ushort ReadUInt16R()
        {
            ushort result = (ushort)(_data[_offset + 1] << 8 | _data[_offset]);
            _offset += 2;
            return result;
        }


        public uint ReadUInt32()
        {
            uint result = (uint)(_data[_offset] << 24 | _data[_offset + 1] << 16 | _data[_offset + 2] << 8 | _data[_offset + 3]);
            _offset += 4;
            return result;
        }


        public int ReadInt32()
        {
            int result = (int)(_data[_offset] << 24 | _data[_offset + 1] << 16 | _data[_offset + 2] << 8 | _data[_offset + 3]);
            _offset += 4;
            return result;
        }

        public uint ReadUInt32R()
        {
            uint result = (uint)(_data[_offset + 3] << 24 | _data[_offset + 2] << 16 | _data[_offset + 1] << 8 | _data[_offset]);
            _offset += 4;
            return result;
        }

        public ulong ReadUInt64()
        {
            ulong result = (ulong)((ulong)_data[_offset] << 56
                | (ulong)_data[_offset + 1] << 48
                | (ulong)_data[_offset + 2] << 40
                | (ulong)_data[_offset + 3] << 32
                | (ulong)_data[_offset + 4] << 24
                | (ulong)_data[_offset + 5] << 16
                | (ulong)_data[_offset + 6] << 8
                | (ulong)_data[_offset + 7]);
            _offset += 8;
            return result;
        }

        public ulong ReadUInt64R()
        {
            ulong result = (ulong)((ulong)_data[_offset + 7] << 56
                | (ulong)_data[_offset + 6] << 48
                | (ulong)_data[_offset + 5] << 40
                | (ulong)_data[_offset + 4] << 32
                | (ulong)_data[_offset + 3] << 24
                | (ulong)_data[_offset + 2] << 16
                | (ulong)_data[_offset + 1] << 8
                | (ulong)_data[_offset]);
            _offset += 8;
            return result;
        }
        //public string ReadString(int size)
        //{
        //    return System.Text.ASCIIEncoding.ASCII.GetString(_data, _offset, size);
        //}
        public void WriteByte(byte value)
        {
            _data[_offset++] = value;
        }

        public void WriteByteArray(byte[] data, int size)
        {
            if (size + _offset > _data.Length)
            {
                Console.WriteLine("Buffer short by " + ((size + _offset) - _data.Length));
                var newArr = new byte[size + _offset + 100];
                System.Buffer.BlockCopy(_data, 0, newArr, 0, _data.Length);
                _data = newArr;
            }
            System.Buffer.BlockCopy(data, 0, _data, _offset, size);
            _offset += size;
        }
        public void WriteByteArray(byte[] data, int offset, int size)
        {
            System.Buffer.BlockCopy(data, offset, _data, _offset, size);
            _offset += size;
        }
        public void WriteUInt16(ushort value)
        {
            _data[_offset] = (byte)(value >> 8);
            _data[_offset + 1] = (byte)(value);
            _offset += 2;
        }
        public void WriteUInt16R(ushort value)
        {
            _data[_offset] = (byte)(value & 0xFF);
            _data[_offset + 1] = (byte)(value >> 8);
            _offset += 2;
        }

        public void WriteUInt32(uint value)
        {
            _data[_offset] = (byte)(value >> 24);
            _data[_offset + 1] = (byte)(value >> 16);
            _data[_offset + 2] = (byte)(value >> 8);
            _data[_offset + 3] = (byte)(value);
            _offset += 4;
        }

        public void WriteInt32(int value)
        {
            _data[_offset] = (byte)(value >> 24);
            _data[_offset + 1] = (byte)(value >> 16);
            _data[_offset + 2] = (byte)(value >> 8);
            _data[_offset + 3] = (byte)(value);
            _offset += 4;
        }

        public void WriteUInt32R(uint value)
        {
            _data[_offset + 3] = (byte)(value >> 24);
            _data[_offset + 2] = (byte)(value >> 16);
            _data[_offset + 1] = (byte)(value >> 8);
            _data[_offset] = (byte)(value);
            _offset += 4;
        }

        public void WriteFloat(float value)
        {
            var bytes = BitConverter.GetBytes(value);
            _data[_offset + 3] = bytes[0];
            _data[_offset + 2] = bytes[1];
            _data[_offset + 1] = bytes[2];
            _data[_offset] = bytes[3];
            _offset += 4;
        }

        public void WriteUInt24R(uint value)
        {
            _data[_offset + 2] = (byte)(value >> 16);
            _data[_offset + 1] = (byte)(value >> 8);
            _data[_offset] = (byte)(value);
            _offset += 3;
        }

        public void WriteUInt64(ulong value)
        {
            _data[_offset] = (byte)(value >> 56);
            _data[_offset + 1] = (byte)(value >> 48);
            _data[_offset + 2] = (byte)(value >> 40);
            _data[_offset + 3] = (byte)(value >> 32);
            _data[_offset + 4] = (byte)(value >> 24);
            _data[_offset + 5] = (byte)(value >> 16);
            _data[_offset + 6] = (byte)(value >> 8);
            _data[_offset + 7] = (byte)(value);
            _offset += 8;
        }

        public void WriteUInt64R(ulong value)
        {
            _data[_offset + 7] = (byte)(value >> 56);
            _data[_offset + 6] = (byte)(value >> 48);
            _data[_offset + 5] = (byte)(value >> 40);
            _data[_offset + 4] = (byte)(value >> 32);
            _data[_offset + 3] = (byte)(value >> 24);
            _data[_offset + 2] = (byte)(value >> 16);
            _data[_offset + 1] = (byte)(value >> 8);
            _data[_offset] = (byte)(value);
            _offset += 8;
        }

        public string ReadPascalString()
        {
            byte length = ReadByte();
            string msg = "";
            if (length > 0)
            {
                msg = ReadString(length);
            }
            return msg;
        }
        public string ReadPascalString32()
        {
            uint length = ReadUInt32();
            string msg = "";
            if (length > 0)
            {
                msg = ReadString((int)length);
            }
            return msg;
        }

        public byte[] ReadByteArray()
        {
            uint length = ReadUInt32();

            if (length > 0)
            {
                byte[] data = new byte[length];
                Read(data, (int)length);
                return data;
            }
            return null;
        }

        public byte[] ReadByteArray(int length)
        {
            if (length > 0)
            {
                byte[] data = new byte[length];
                Read(data, (int)length);
                return data;
            }
            return null;
        }

        public void WriteByteArray(byte[] data)
        {
            if (data != null)
            {
                WriteUInt32((uint)data.Length);
                WriteByteArray(data, (int)data.Length);
            }
            else
            {
                WriteUInt32(0);
            }
        }

        public void WritePascalString(string str)
        {
            if (str == null || str.Length <= 0)
            {
                WriteByte(0);
                return;
            }

            WriteByte((byte)str.Length);
            System.Buffer.BlockCopy(Encoding.ASCII.GetBytes(str), 0, _data, _offset, str.Length);
            _offset += str.Length;
        }

        public void WritePascalString32(string str)
        {
            if (str == null || str.Length <= 0)
            {
                WriteUInt32(0);
                return;
            }

            WriteUInt32((uint)str.Length);
            System.Buffer.BlockCopy(Encoding.ASCII.GetBytes(str), 0, _data, _offset, str.Length);
            _offset += str.Length;
        }

        public void Fill(byte val, int length)
        {
            for (int i = 0; i < length; i++)
                _data[i + _offset] = val;
            _offset += length;
        }

        public void FillString(string str, int length)
        {
            if (str == null || str.Length <= 0)
            {
                WriteByte(0);
                return;
            }
            Array.Clear(_data, _offset, length);
            System.Buffer.BlockCopy(Encoding.ASCII.GetBytes(str), 0, _data, _offset, str.Length);

            _offset += length;
        }
        public void WriteString(string str)
        {

            System.Buffer.BlockCopy(Encoding.ASCII.GetBytes(str), 0, _data, _offset, str.Length);

            _offset += str.Length;
        }

        public void WriteCString(string str)
        {

            if (str == null)
            {
                WriteByte(0);
            }
            else
            {
                var data = Encoding.GetEncoding("iso-8859-1").GetBytes(str);
                WriteByteArray(data, data.Length);
                WriteByte(0);
            }




        }
        public void WriteString(string str, int maxLength)
        {
            if (str == null || maxLength <= 0)
            {
                //     WriteByte(0);
                return;
            }
            if (maxLength > str.Length)
                maxLength = str.Length;
            //  Array.Clear(_data, _offset, length);
            System.Buffer.BlockCopy(Encoding.ASCII.GetBytes(str), 0, _data, _offset, maxLength);

            _offset += maxLength;
        }

        public void WriteVarUInt32(uint val)
        {
            while (val > 0)
            {
                WriteByte((byte)((val & 0x7F) ^ ((val > 0x7F) ? 0x80 : 0x00)));
                val = val >> 7;
            }
        }

        public void WriteZigZag(int val)
        {
            byte sign = (byte)((val < 0) ? 1 : 0);
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

    }
}
