using System;
using System.Buffers;
using System.Reflection;
using System.Text;
using FastGenericNew;

namespace FrameWork.NetWork.V4
{
    /// <summary>
    /// Binary packet serializer that uses reflection to serialize/deserialize structs.
    /// Processes properties in the order they are declared.
    /// Can optionally use a source-generated context for improved performance.
    /// </summary>
    public class BinaryPacketSerializer : IPacketSerializer
    {
        private static readonly Encoding Encoding = Encoding.GetEncoding("iso-8859-1");
        private readonly IPacketSerializerContext? _context;

        /// <summary>
        /// Creates a new BinaryPacketSerializer
        /// </summary>
        /// <param name="context">Optional source-generated context for optimized serialization</param>
        public BinaryPacketSerializer(IPacketSerializerContext? context = null)
        {
            _context = context;
        }

        /// <summary>
        /// Deserializes a packet payload into a strongly-typed struct.
        /// Properties are read in declaration order.
        /// </summary>
        public T Deserialize<T>(ReadOnlySpan<byte> payload)
        {
            // Try context first
            if (_context != null && _context.TryDeserialize(typeof(T), payload, out var result))
                return (T)result!;

            // Fall back to reflection
            var type = typeof(T);
            
            if (type.IsValueType)
                throw new InvalidOperationException($"Type {type.Name} must be a reference type");

            var instance = FastNew.CreateInstance<T>();
            var reader = new SpanReader(payload);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var ctx = new NullabilityInfoContext();

            foreach (var property in properties)
            {
                if (!property.CanWrite)
                    continue;
                
                if (ctx.Create(property).WriteState is NullabilityState.Nullable && reader.IsAtEnd())
                {
                    property.SetValue(instance, null);
                    continue;
                }

                var value = ReadProperty(ref reader, property.PropertyType);
                property.SetValue(instance, value);
            }

            return instance;
        }

        /// <summary>
        /// Serializes a struct into a buffer writer.
        /// Properties are written in declaration order.
        /// </summary>
        public void Serialize<T>(IBufferWriter<byte> writer, T message)
        {
            // Try context first
            if (_context != null && _context.TrySerialize(message!, writer))
                return;

            // Fall back to reflection
            var type = typeof(T);
            
            if (type.IsValueType)
                throw new InvalidOperationException($"Type {type.Name} must be reference type");

            var spanWriter = new SpanWriter(writer);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (!property.CanRead)
                    continue;

                var value = property.GetValue(message);
                
                // Skip nullable properties that are null
                bool isNullable = Nullable.GetUnderlyingType(property.PropertyType) != null || !property.PropertyType.IsValueType;
                if (isNullable && value == null)
                    continue;

                WriteProperty(ref spanWriter, property.PropertyType, value);
            }
        }

        private object ReadProperty(ref SpanReader reader, Type propertyType)
        {
            // Handle nullable types
            var underlyingType = Nullable.GetUnderlyingType(propertyType);
            if (underlyingType != null)
            {
                propertyType = underlyingType;
            }

            if (propertyType == typeof(byte))
                return reader.ReadByte();
            else if (propertyType == typeof(sbyte))
                return reader.ReadSByte();
            else if (propertyType == typeof(short))
                return reader.ReadInt16();
            else if (propertyType == typeof(ushort))
                return reader.ReadUInt16();
            else if (propertyType == typeof(int))
                return reader.ReadInt32();
            else if (propertyType == typeof(uint))
                return reader.ReadUInt32();
            else if (propertyType == typeof(long))
                return reader.ReadInt64();
            else if (propertyType == typeof(ulong))
                return reader.ReadUInt64();
            else if (propertyType == typeof(float))
                return reader.ReadFloat();
            else if (propertyType == typeof(double))
                return reader.ReadDouble();
            else if (propertyType == typeof(bool))
                return reader.ReadByte() != 0;
            else if (propertyType.IsEnum)
                return Enum.ToObject(propertyType,reader.ReadByte());
            else if (propertyType == typeof(string))
                return reader.ReadString();
            else if (propertyType.IsArray)
            {
                var elementType = propertyType.GetElementType();
                if (elementType == typeof(byte))
                    return reader.ReadByteArray();
                else
                    throw new NotSupportedException($"Array type {elementType.Name}[] is not supported");
            }
            else
                throw new NotSupportedException($"Property type {propertyType.Name} is not supported");
        }

        private void WriteProperty(ref SpanWriter writer, Type propertyType, object value)
        {
            // Handle nullable types
            var underlyingType = Nullable.GetUnderlyingType(propertyType);
            if (underlyingType != null)
            {
                if (value == null)
                {
                    // Write default value for null
                    value = Activator.CreateInstance(underlyingType);
                }
                propertyType = underlyingType;
            }

            if (propertyType == typeof(byte))
                writer.WriteByte((byte)value);
            else if (propertyType == typeof(sbyte))
                writer.WriteSByte((sbyte)value);
            else if (propertyType == typeof(short))
                writer.WriteInt16((short)value);
            else if (propertyType == typeof(ushort))
                writer.WriteUInt16((ushort)value);
            else if (propertyType == typeof(int))
                writer.WriteInt32((int)value);
            else if (propertyType == typeof(uint))
                writer.WriteUInt32((uint)value);
            else if (propertyType == typeof(long))
                writer.WriteInt64((long)value);
            else if (propertyType == typeof(ulong))
                writer.WriteUInt64((ulong)value);
            else if (propertyType == typeof(float))
                writer.WriteFloat((float)value);
            else if (propertyType == typeof(double))
                writer.WriteDouble((double)value);
            else if (propertyType == typeof(bool))
                writer.WriteByte((byte)((bool)value ? 1 : 0));
            else if (propertyType.IsEnum)
                writer.WriteByte(Convert.ToByte(value));
            else if (propertyType == typeof(string))
                writer.WriteString((string)value ?? string.Empty);
            else if (propertyType.IsArray)
            {
                var elementType = propertyType.GetElementType();
                if (elementType == typeof(byte))
                    writer.WriteByteArray((byte[])value);
                else
                    throw new NotSupportedException($"Array type {elementType.Name}[] is not supported");
            }
            else
                throw new NotSupportedException($"Property type {propertyType.Name} is not supported");
        }

        /// <summary>
        /// Helper for reading from a ReadOnlySpan<byte> with position tracking.
        /// </summary>
        public ref struct SpanReader
        {
            private ReadOnlySpan<byte> _span;
            private int _position;

            public SpanReader(ReadOnlySpan<byte> span)
            {
                _span = span;
                _position = 0;
            }

            public bool IsAtEnd()
            {
                return _position >= _span.Length;
            }

            public byte ReadByte()
            {
                if (_position >= _span.Length)
                    throw new InvalidOperationException("Attempted to read past end of buffer");
                return _span[_position++];
            }

            public sbyte ReadSByte()
            {
                return (sbyte)ReadByte();
            }

            public ushort ReadUInt16()
            {
                // Big-endian (network byte order)
                byte b1 = ReadByte();
                byte b2 = ReadByte();
                return (ushort)((b1 << 8) | b2);
            }

            public short ReadInt16()
            {
                return (short)ReadUInt16();
            }

            public uint ReadUInt32()
            {
                // Big-endian (network byte order)
                byte b1 = ReadByte();
                byte b2 = ReadByte();
                byte b3 = ReadByte();
                byte b4 = ReadByte();
                return (uint)((b1 << 24) | (b2 << 16) | (b3 << 8) | b4);
            }

            public int ReadInt32()
            {
                return (int)ReadUInt32();
            }

            public ulong ReadUInt64()
            {
                // Big-endian (network byte order)
                uint high = ReadUInt32();
                uint low = ReadUInt32();
                return ((ulong)high << 32) | low;
            }

            public long ReadInt64()
            {
                return (long)ReadUInt64();
            }

            public float ReadFloat()
            {
                Span<byte> bytes = stackalloc byte[4];
                bytes[3] = ReadByte();
                bytes[2] = ReadByte();
                bytes[1] = ReadByte();
                bytes[0] = ReadByte();
                return BitConverter.ToSingle(bytes);
            }

            public double ReadDouble()
            {
                Span<byte> bytes = stackalloc byte[8];
                bytes[7] = ReadByte();
                bytes[6] = ReadByte();
                bytes[5] = ReadByte();
                bytes[4] = ReadByte();
                bytes[3] = ReadByte();
                bytes[2] = ReadByte();
                bytes[1] = ReadByte();
                bytes[0] = ReadByte();
                return BitConverter.ToDouble(bytes);
            }

            public string ReadString()
            {
                // String format: [Length:4][Bytes:N]
                uint length = ReadUInt32();
                if (length == 0)
                    return string.Empty;

                if (_position + length > _span.Length)
                    throw new InvalidOperationException("String length exceeds buffer size");

                var stringBytes = _span.Slice(_position, (int)length);
                _position += (int)length;
                return Encoding.GetString(stringBytes);
            }

            public byte[] ReadByteArray()
            {
                // Array format: [Length:4][Bytes:N]
                uint length = ReadUInt32();
                if (length == 0)
                    return Array.Empty<byte>();

                if (_position + length > _span.Length)
                    throw new InvalidOperationException("Array length exceeds buffer size");

                var array = new byte[length];
                _span.Slice(_position, (int)length).CopyTo(array);
                _position += (int)length;
                return array;
            }
        }

        /// <summary>
        /// Helper for writing to an IBufferWriter<byte>.
        /// </summary>
        public ref struct SpanWriter
        {
            private IBufferWriter<byte> _writer;

            public SpanWriter(IBufferWriter<byte> writer)
            {
                _writer = writer;
            }

            public void WriteByte(byte value)
            {
                var span = _writer.GetSpan(1);
                span[0] = value;
                _writer.Advance(1);
            }

            public void WriteSByte(sbyte value)
            {
                WriteByte((byte)value);
            }

            public void WriteUInt16(ushort value)
            {
                // Big-endian (network byte order)
                var span = _writer.GetSpan(2);
                span[0] = (byte)(value >> 8);
                span[1] = (byte)(value & 0xFF);
                _writer.Advance(2);
            }

            public void WriteInt16(short value)
            {
                WriteUInt16((ushort)value);
            }

            public void WriteUInt32(uint value)
            {
                // Big-endian (network byte order)
                var span = _writer.GetSpan(4);
                span[0] = (byte)(value >> 24);
                span[1] = (byte)((value >> 16) & 0xFF);
                span[2] = (byte)((value >> 8) & 0xFF);
                span[3] = (byte)(value & 0xFF);
                _writer.Advance(4);
            }

            public void WriteInt32(int value)
            {
                WriteUInt32((uint)value);
            }

            public void WriteUInt64(ulong value)
            {
                // Big-endian (network byte order)
                var span = _writer.GetSpan(8);
                span[0] = (byte)(value >> 56);
                span[1] = (byte)((value >> 48) & 0xFF);
                span[2] = (byte)((value >> 40) & 0xFF);
                span[3] = (byte)((value >> 32) & 0xFF);
                span[4] = (byte)((value >> 24) & 0xFF);
                span[5] = (byte)((value >> 16) & 0xFF);
                span[6] = (byte)((value >> 8) & 0xFF);
                span[7] = (byte)(value & 0xFF);
                _writer.Advance(8);
            }

            public void WriteInt64(long value)
            {
                WriteUInt64((ulong)value);
            }

            public void WriteFloat(float value)
            {
                Span<byte> bytes = stackalloc byte[4];
                BitConverter.TryWriteBytes(bytes, value);
                var span = _writer.GetSpan(4);
                span[0] = bytes[3];
                span[1] = bytes[2];
                span[2] = bytes[1];
                span[3] = bytes[0];
                _writer.Advance(4);
            }

            public void WriteDouble(double value)
            {
                Span<byte> bytes = stackalloc byte[8];
                BitConverter.TryWriteBytes(bytes, value);
                var span = _writer.GetSpan(8);
                span[0] = bytes[7];
                span[1] = bytes[6];
                span[2] = bytes[5];
                span[3] = bytes[4];
                span[4] = bytes[3];
                span[5] = bytes[2];
                span[6] = bytes[1];
                span[7] = bytes[0];
                _writer.Advance(8);
            }

            public void WriteString(string value)
            {
                // String format: [Length:4][Bytes:N]
                if (string.IsNullOrEmpty(value))
                {
                    WriteUInt32(0);
                    return;
                }

                var bytes = Encoding.GetBytes(value);
                WriteUInt32((uint)bytes.Length);

                var span = _writer.GetSpan(bytes.Length);
                bytes.CopyTo(span);
                _writer.Advance(bytes.Length);
            }

            public void WriteByteArray(byte[] value)
            {
                // Array format: [Length:4][Bytes:N]
                if (value == null || value.Length == 0)
                {
                    WriteUInt32(0);
                    return;
                }

                WriteUInt32((uint)value.Length);

                var span = _writer.GetSpan(value.Length);
                value.CopyTo(span);
                _writer.Advance(value.Length);
            }
        }
    }

    /// <summary>
    /// Factory for creating BinaryPacketSerializer instances.
    /// </summary>
    public class BinaryPacketSerializerFactory : IPacketSerializerFactory
    {
        private readonly BinaryPacketSerializer _sharedInstance;

        /// <summary>
        /// Creates a new BinaryPacketSerializerFactory
        /// </summary>
        /// <param name="context">Optional source-generated context for optimized serialization</param>
        public BinaryPacketSerializerFactory(IPacketSerializerContext? context = null)
        {
            _sharedInstance = new BinaryPacketSerializer(context);
        }

        /// <summary>
        /// Creates or returns a cached serializer instance.
        /// BinaryPacketSerializer is thread-safe and can be reused.
        /// </summary>
        public IPacketSerializer Create()
        {
            return _sharedInstance;
        }
    }
}
