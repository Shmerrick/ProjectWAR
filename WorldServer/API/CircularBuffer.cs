using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace WorldServer.API
{
    public class CircularBuffer
    {
        private int _capacity;
        private int _level;
        private int _offset;
        private byte[] _data;

        private byte[] _tmpEncKey = new byte[256];
        private byte[] _tmpDecKey = new byte[256];

        public int Capacity
        {
            get { return _capacity; }
        }

        public int Level
        {
            get { return _level; }
            set { _level = value; }
        }

        public int Offset
        {
            get { return _offset; }
            set
            {
                if (value < _level)
                    _offset = value;
            }
        }

        public byte[] Data
        {
            get { return _data; }
        }

        public CircularBuffer(int capacity)
        {
            _capacity = capacity;
            _data = new byte[capacity];
        }

        public CircularBuffer(CircularBuffer buffer)
        {
            _data = new byte[buffer._data.Length];
            _capacity = buffer._capacity;
            _level = buffer._level;
            _offset = buffer._offset;
            System.Buffer.BlockCopy(buffer._data, 0, _data, 0, _data.Length);
        }

        public void Clear()
        {
            _offset = 0;
            _level = 0;
        }

        public bool Write(byte[] data, int size)
        {
            if (_level + size > _capacity)
                return false;

            int pos = (_offset + _level) % _capacity;

            if (pos + size <= _capacity)
            {
                System.Buffer.BlockCopy(data, 0, _data, pos, size);
            }
            else
            {
                int s0 = _capacity - pos;
                int s1 = size - s0;

                System.Buffer.BlockCopy(data, 0, _data, pos, s0);
                System.Buffer.BlockCopy(data, s0, _data, 0, s1);
            }

            _level += size;
            return true;
        }

        public bool Read(CircularBuffer buf, int size)
        {
            if (_level < size)
                return false;
            if (buf._level + size > buf._capacity)
                return false;

            for (int i = 0; i < size; i++)
            {
                int si = (_offset + i) % _capacity;
                int di = (buf._offset + buf._level + i) % buf._capacity;
                buf._data[di] = _data[size];
            }

            _offset = (_offset + size) % _capacity;
            _level -= size;
            buf._level += size;

            return true;
        }

        public bool Peek(byte[] data, int size)
        {
            if (size > _level)
                return false;

            if (_offset + size <= _capacity)
            {
                System.Buffer.BlockCopy(_data, _offset, data, 0, size);
            }
            else
            {
                int s0 = _capacity - _offset;
                int s1 = size - s0;

                System.Buffer.BlockCopy(_data, _offset, data, 0, s0);
                System.Buffer.BlockCopy(_data, s0, data, s0, s1);
            }

            return true;
        }

        public bool Read(byte[] data, int size)
        {
            if (size > _level)
                return false;

            if (_offset + size <= _capacity)
            {
                System.Buffer.BlockCopy(_data, _offset, data, 0, size);
            }
            else
            {
                int s0 = _capacity - _offset;
                int s1 = size - s0;

                System.Buffer.BlockCopy(_data, _offset, data, 0, s0);
                System.Buffer.BlockCopy(_data, s0, data, s0, s1);
            }


            _offset = (_offset + size) % _capacity;
            _level -= size;

            return true;
        }

        public byte this[int index]
        {
            get
            {
                return _data[(_offset + index) % _capacity];
            }
        }

        public void Seek(int offset)
        {
            _offset = offset;
        }

        public bool Discard(int size)
        {
            if (size > _level)
                return false;

            _offset = (_offset + size) % _capacity;
            _level -= size;

            return true;
        }

        public byte ReadByte()
        {
            byte result = 0;

            if (_level < 1)
                return result;

            for (int i = 0; i < 1; i++)
            {
                int si = (_offset + i) % _capacity;
                result = _data[si];
            }

            _offset = (_offset + 1) % _capacity;
            _level -= 1;

            return result;
        }

        public ushort ReadInt16()
        {
            ushort result = 0;

            if (_level < 2)
                return result;

            result = (ushort)(_data[_offset % _capacity] << 8 | _data[(_offset + 1) % _capacity]);

            _offset = (_offset + 2) % _capacity;
            _level -= 2;

            return result;
        }

        public ushort ReadInt16R()
        {
            ushort result = 0;

            if (_level < 2)
                return result;

            result = (ushort)(_data[(_offset + 1) % _capacity] << 8 | _data[_offset % _capacity]);

            _offset = (_offset + 2) % _capacity;
            _level -= 2;

            return result;
        }

        public UInt32 ReadInt32()
        {
            UInt32 result = 0;

            if (_level < 4)
                return result;

            result = (UInt32)(_data[_offset % _capacity] << 24 | _data[(_offset + 1) % _capacity] << 16 | _data[(_offset + 2) % _capacity] << 8 | _data[(_offset + 3) % _capacity]);

            _offset = (_offset + 4) % _capacity;
            _level -= 4;

            return result;
        }

        public UInt32 ReadInt32R()
        {
            UInt32 result = 0;

            if (_level < 4)
                return result;

            result = (UInt32)(_data[(_offset + 3) % _capacity] << 24 | _data[(_offset + 2) % _capacity] << 16 | _data[(_offset + 1) % _capacity] << 8 | _data[_offset % _capacity]);

            _offset = (_offset + 4) % _capacity;
            _level -= 4;

            return result;
        }


        public bool WriteByte(byte data)
        {
            if (_level + 1 > _capacity)
                return false;

            int pos = (_offset + _level) % _capacity;

            if (pos < _capacity)
            {
                _data[pos] = data;
            }

            _level += 1;
            return true;
        }

        public bool WriteInt16(ushort data)
        {
            if (_level + 2 > _capacity)
                return false;

            int pos = (_offset + _level) % _capacity;

            if (pos + 2 <= _capacity)
            {
                _data[(_offset) % _capacity] = (byte)(data >> 8);
                _data[(_offset + 1) % _capacity] = (byte)(data & 0xFF);
            }

            _level += 2;
            return true;
        }


        public bool WriteInt16R(ushort data)
        {
            if (_level + 2 > _capacity)
                return false;

            int pos = (_offset + _level) % _capacity;

            if (pos + 2 <= _capacity)
            {
                _data[(_offset + 1) % _capacity] = (byte)(data >> 8);
                _data[(_offset) % _capacity] = (byte)(data & 0xFF);
            }

            _level += 2;
            return true;
        }


        public bool WriteInt32(UInt32 data)
        {
            if (_level + 4 > _capacity)
                return false;

            int pos = (_offset + _level) % _capacity;

            if (pos + 4 <= _capacity)
            {
                _data[(_offset) % _capacity] = (byte)(data >> 24);
                _data[(_offset + 1) % _capacity] = (byte)(data >> 16);
                _data[(_offset + 2) % _capacity] = (byte)(data >> 8);
                _data[(_offset + 3) % _capacity] = (byte)(data & 0xFF);
            }

            _level += 4;
            return true;
        }

        public bool WriteInt32R(UInt32 data)
        {
            if (_level + 4 > _capacity)
                return false;

            int pos = (_offset + _level) % _capacity;

            if (pos + 4 <= _capacity)
            {
                _data[(_offset) % _capacity] = (byte)(data & 0xFF);
                _data[(_offset + 1) % _capacity] = (byte)(data >> 8);
                _data[(_offset + 2) % _capacity] = (byte)(data >> 16);
                _data[(_offset + 3) % _capacity] = (byte)(data >> 24);
            }

            _level += 4;
            return true;
        }

        public bool WriteInt64(UInt64 data)
        {
            if (_level + 8 > _capacity)
                return false;

            int pos = (_offset + _level) % _capacity;

            if (pos + 8 <= _capacity)
            {
                _data[(_offset) % _capacity] = (byte)(data >> 56);
                _data[(_offset + 1) % _capacity] = (byte)(data >> 48);
                _data[(_offset + 2) % _capacity] = (byte)(data >> 40);
                _data[(_offset + 3) % _capacity] = (byte)(data >> 32);
                _data[(_offset + 4) % _capacity] = (byte)(data >> 24);
                _data[(_offset + 5) % _capacity] = (byte)(data >> 16);
                _data[(_offset + 6) % _capacity] = (byte)(data >> 8);
                _data[(_offset + 7) % _capacity] = (byte)(data & 0xFF);
            }

            _level += 8;
            return true;
        }

        public bool WriteInt64R(UInt64 data)
        {
            if (_level + 8 > _capacity)
                return false;

            int pos = (_offset + _level) % _capacity;

            if (pos + 8 <= _capacity)
            {
                _data[(_offset + 7) % _capacity] = (byte)(data >> 56);
                _data[(_offset + 6) % _capacity] = (byte)(data >> 48);
                _data[(_offset + 5) % _capacity] = (byte)(data >> 40);
                _data[(_offset + 4) % _capacity] = (byte)(data >> 32);
                _data[(_offset + 3) % _capacity] = (byte)(data >> 24);
                _data[(_offset + 2) % _capacity] = (byte)(data >> 16);
                _data[(_offset + 1) % _capacity] = (byte)(data >> 8);
                _data[(_offset) % _capacity] = (byte)(data & 0xFF);
            }

            _level += 8;
            return true;
        }

        public UInt64 ReadInt64()
        {
            UInt64 result = 0;

            if (_level < 8)
                return result;

            result = (UInt64)(
               (UInt64)_data[_offset % _capacity] << 56 |
               (UInt64)_data[(_offset + 1) % _capacity] << 48 |
               (UInt64)_data[(_offset + 2) % _capacity] << 40 |
               (UInt64)_data[(_offset + 3) % _capacity] << 32 |
               (UInt64)_data[(_offset + 4) % _capacity] << 24 |
               (UInt64)_data[(_offset + 5) % _capacity] << 16 |
               (UInt64)_data[(_offset + 6) % _capacity] << 8 |
               (UInt64)_data[(_offset + 7) % _capacity]);

            _offset = (_offset + 8) % _capacity;
            _level -= 8;

            return result;
        }

        public UInt64 ReadInt64R()
        {
            UInt64 result = 0;

            if (_level < 8)
                return result;

            result = (UInt64)(
                (UInt64)_data[(_offset + 7) % _capacity] << 56 |
                (UInt64)_data[(_offset + 6) % _capacity] << 48 |
                (UInt64)_data[(_offset + 5) % _capacity] << 40 |
                (UInt64)_data[(_offset + 4) % _capacity] << 32 |
                (UInt64)_data[(_offset + 3) % _capacity] << 24 |
                (UInt64)_data[(_offset + 2) % _capacity] << 16 |
                (UInt64)_data[(_offset + 1) % _capacity] << 8 |
                (UInt64)_data[(_offset) % _capacity]);

            _offset = (_offset + 8) % _capacity;
            _level -= 8;

            return result;
        }

        public void Encrypt(byte[] key, int offset, int size)
        {
            //TODO:check buffer size
            System.Buffer.BlockCopy(key, 0, _tmpEncKey, 0, 256);
            int x, y, midpoint, pos;
            byte tmp = 0;
            x = y = 0;
            pos = 0;

            midpoint = size / 2;
            for (pos = midpoint; pos < size; pos++)
            {
                x = (x + 1) & 255;
                y = (y + _tmpEncKey[x]) & 255;
                tmp = _tmpEncKey[x];
                _tmpEncKey[x] = _tmpEncKey[y];
                _tmpEncKey[y] = tmp;

                tmp = (byte)((_tmpEncKey[x] + _tmpEncKey[y]) & 255);
                y = (y + _data[pos + offset]) & 255;
                _data[pos + offset] ^= _tmpEncKey[tmp];
            }

            for (pos = 0; pos < midpoint; pos++)
            {
                x = (x + 1) & 255;
                y = (y + _tmpEncKey[x]) & 255;

                tmp = _tmpEncKey[x];

                _tmpEncKey[x] = _tmpEncKey[y];
                _tmpEncKey[y] = tmp;

                tmp = (byte)((_tmpEncKey[x] + _tmpEncKey[y]) & 255);
                y = (y + _data[pos + offset]) & 255;
                _data[pos + offset] ^= _tmpEncKey[tmp];
            }
        }

        public void Decrypt(byte[] key, int offset, int size)
        {
            System.Buffer.BlockCopy(key, 0, _tmpDecKey, 0, 256);
            int x, y, midpoint, pos;
            byte tmp = 0;
            x = 0;
            y = 0;
            pos = 0;

            midpoint = size / 2;
            int circPos = 0;

            for (pos = midpoint; pos < size; pos++)
            {
                circPos = (pos + offset) % _capacity;
                x = (x + 1) & 255;
                y = (y + _tmpDecKey[x]) & 255;

                tmp = _tmpDecKey[x];

                _tmpDecKey[x] = _tmpDecKey[y];
                _tmpDecKey[y] = tmp;

                tmp = (byte)((_tmpDecKey[x] + _tmpDecKey[y]) & 255);
                _data[circPos] ^= _tmpDecKey[tmp];
                y = (y + _data[circPos]) & 255;
            }

            for (pos = 0; pos < midpoint; pos++)
            {
                circPos = (pos + offset) % _capacity;
                x = (x + 1) & 255;
                y = (y + _tmpDecKey[x]) & 255;

                tmp = _tmpDecKey[x];

                _tmpDecKey[x] = _tmpDecKey[y];
                _tmpDecKey[y] = tmp;

                tmp = (byte)((_tmpDecKey[x] + _tmpDecKey[y]) & 255);
                _data[circPos] ^= _tmpDecKey[tmp];
                y = (y + _data[circPos]) & 255;
            }
        }
    }
}
