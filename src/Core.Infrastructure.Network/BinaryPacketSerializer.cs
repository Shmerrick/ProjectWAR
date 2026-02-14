using System.Buffers;
using System.Buffers.Binary;
using System.Reflection;
using System.Text;
using FastGenericNew;

namespace Core.Infrastructure.Network
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

                var value = ReadProperty(ref reader, property.PropertyType, property);
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
                var isNullable = Nullable.GetUnderlyingType(property.PropertyType) != null || !property.PropertyType.IsValueType;
                if (isNullable && value == null)
                    continue;

                WriteProperty(ref spanWriter, property.PropertyType, value, property);
            }
        }

        private object ReadProperty(ref SpanReader reader, Type propertyType, PropertyInfo? propertyInfo = null)
        {
            // Get the length size from PacketLength attribute (default to 1 byte)
            var lengthSize = 1;
            if (propertyInfo != null)
            {
                var packetLengthAttr = propertyInfo.GetCustomAttribute<PacketLengthAttribute>();
                if (packetLengthAttr != null)
                    lengthSize = packetLengthAttr.ByteCount;
            }

            // Handle nullable types
            var underlyingType = Nullable.GetUnderlyingType(propertyType);
            if (underlyingType != null)
            {
                propertyType = underlyingType;
            }

            if (propertyType == typeof(byte))
                return reader.ReadByte();
            if (propertyType == typeof(sbyte))
                return reader.ReadSByte();
            if (propertyType == typeof(short))
                return reader.ReadInt16();
            if (propertyType == typeof(ushort))
                return reader.ReadUInt16();
            if (propertyType == typeof(int))
                return reader.ReadInt32();
            if (propertyType == typeof(uint))
                return reader.ReadUInt32();
            if (propertyType == typeof(long))
                return reader.ReadInt64();
            if (propertyType == typeof(ulong))
                return reader.ReadUInt64();
            if (propertyType == typeof(float))
                return reader.ReadFloat();
            if (propertyType == typeof(double))
                return reader.ReadDouble();
            if (propertyType == typeof(bool))
                return reader.ReadByte() != 0;
            if (propertyType.IsEnum)
                return Enum.ToObject(propertyType,reader.ReadByte());
            if (propertyType == typeof(string))
                return reader.ReadString();
            if (propertyType.IsArray)
            {
                var elementType = propertyType.GetElementType()!;
                if (elementType == typeof(byte))
                    return reader.ReadByteArray(lengthSize);
                // Use generic method for arrays
                return ReadArrayGeneric(ref reader, elementType, lengthSize);
            }

            if (propertyType.IsGenericType)
            {
                var genericTypeDef = propertyType.GetGenericTypeDefinition();
                
                // Handle List<T>, IList<T>, ICollection<T>, IEnumerable<T>
                if (genericTypeDef == typeof(System.Collections.Generic.List<>) ||
                    genericTypeDef == typeof(System.Collections.Generic.IList<>) ||
                    genericTypeDef == typeof(System.Collections.Generic.ICollection<>) ||
                    genericTypeDef == typeof(System.Collections.Generic.IEnumerable<>))
                {
                    var elementType = propertyType.GetGenericArguments()[0];
                    
                    // Read as array first
                    var array = ReadArrayGeneric(ref reader, elementType, lengthSize);
                    
                    // Convert to appropriate collection type
                    if (genericTypeDef == typeof(System.Collections.Generic.List<>))
                    {
                        var listType = typeof(System.Collections.Generic.List<>).MakeGenericType(elementType);
                        var list = Activator.CreateInstance(listType, array)!;
                        return list;
                    }

                    // For IList<T>, ICollection<T>, IEnumerable<T>, return as array
                    return array;
                }

                throw new NotSupportedException($"Generic type {propertyType.Name} is not supported");
            }

            throw new NotSupportedException($"Property type {propertyType.Name} is not supported");
        }

        private object ReadArrayGeneric(ref SpanReader reader, Type elementType, int lengthSize)
        {
            // Read the length based on lengthSize
            var length = lengthSize switch
            {
                1 => reader.ReadByte(),
                2 => reader.ReadUInt16(),
                4 => reader.ReadUInt32(),
                _ => throw new InvalidOperationException($"Invalid length size: {lengthSize}")
            };
            
            if (length == 0)
            {
                var emptyArray = Array.CreateInstance(elementType, 0);
                return emptyArray;
            }

            // Create array
            var array = Array.CreateInstance(elementType, (int)length);
            
            // Read each element by recursively calling ReadProperty
            for (var i = 0; i < length; i++)
            {
                var element = ReadProperty(ref reader, elementType);
                array.SetValue(element, i);
            }

            return array;
        }

        private void WriteProperty(ref SpanWriter writer, Type propertyType, object value, PropertyInfo? propertyInfo = null)
        {
            // Get the length size from PacketLength attribute (default to 1 byte)
            var lengthSize = 1;
            if (propertyInfo != null)
            {
                var packetLengthAttr = propertyInfo.GetCustomAttribute<PacketLengthAttribute>();
                if (packetLengthAttr != null)
                    lengthSize = packetLengthAttr.ByteCount;
            }

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
                var elementType = propertyType.GetElementType()!;
                if (elementType == typeof(byte))
                    writer.WriteByteArray((byte[])value, lengthSize);
                else
                {
                    // Use generic method for arrays
                    WriteArrayGeneric(ref writer, value, elementType, lengthSize);
                }
            }
            else if (propertyType.IsGenericType)
            {
                var genericTypeDef = propertyType.GetGenericTypeDefinition();
                
                // Handle List<T>, IList<T>, ICollection<T>, IEnumerable<T>
                if (genericTypeDef == typeof(System.Collections.Generic.List<>) ||
                    genericTypeDef == typeof(System.Collections.Generic.IList<>) ||
                    genericTypeDef == typeof(System.Collections.Generic.ICollection<>) ||
                    genericTypeDef == typeof(System.Collections.Generic.IEnumerable<>))
                {
                    var elementType = propertyType.GetGenericArguments()[0];
                    
                    // Use WriteCollection
                    WriteCollectionGeneric(ref writer, value, elementType, lengthSize);
                }
                else
                    throw new NotSupportedException($"Generic type {propertyType.Name} is not supported");
            }
            else
                throw new NotSupportedException($"Property type {propertyType.Name} is not supported");
        }

        private void WriteArrayGeneric(ref SpanWriter writer, object value, Type elementType, int lengthSize)
        {
            var array = (Array)value;
            
            // Write length based on lengthSize
            switch (lengthSize)
            {
                case 1:
                    if (array.Length > byte.MaxValue)
                        throw new InvalidOperationException($"Array length {array.Length} exceeds maximum for 1-byte length ({byte.MaxValue})");
                    writer.WriteByte((byte)array.Length);
                    break;
                case 2:
                    if (array.Length > ushort.MaxValue)
                        throw new InvalidOperationException($"Array length {array.Length} exceeds maximum for 2-byte length ({ushort.MaxValue})");
                    writer.WriteUInt16((ushort)array.Length);
                    break;
                case 4:
                    writer.WriteUInt32((uint)array.Length);
                    break;
                default:
                    throw new InvalidOperationException($"Invalid length size: {lengthSize}");
            }
            
            // Write each element by recursively calling WriteProperty
            for (var i = 0; i < array.Length; i++)
            {
                var element = array.GetValue(i);
                WriteProperty(ref writer, elementType, element!);
            }
        }

        private void WriteCollectionGeneric(ref SpanWriter writer, object value, Type elementType, int lengthSize)
        {
            // Convert to array for counting
            var enumerable = (System.Collections.IEnumerable)value;
            var list = new System.Collections.ArrayList();
            foreach (var item in enumerable)
            {
                list.Add(item);
            }
            
            // Write length based on lengthSize
            switch (lengthSize)
            {
                case 1:
                    if (list.Count > byte.MaxValue)
                        throw new InvalidOperationException($"Collection length {list.Count} exceeds maximum for 1-byte length ({byte.MaxValue})");
                    writer.WriteByte((byte)list.Count);
                    break;
                case 2:
                    if (list.Count > ushort.MaxValue)
                        throw new InvalidOperationException($"Collection length {list.Count} exceeds maximum for 2-byte length ({ushort.MaxValue})");
                    writer.WriteUInt16((ushort)list.Count);
                    break;
                case 4:
                    writer.WriteUInt32((uint)list.Count);
                    break;
                default:
                    throw new InvalidOperationException($"Invalid length size: {lengthSize}");
            }
            
            // Write each element by recursively calling WriteProperty
            foreach (var item in list)
            {
                WriteProperty(ref writer, elementType, item!);
            }
        }

        /// <summary>
        /// Helper for reading from a ReadOnlySpan<byte> with position tracking.
        /// </summary>
        public ref struct SpanReader
        {
            private readonly ReadOnlySpan<byte> _span;
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
                return _span[_position++];
            }

            public sbyte ReadSByte()
            {
                return (sbyte)ReadByte();
            }

            public ushort ReadUInt16()
            {
                // Big-endian (network byte order)
                var value = BinaryPrimitives.ReadUInt16BigEndian(_span.Slice(_position, 2));
                _position += 2;
                return value;
            }

            public short ReadInt16()
            {
                // Big-endian (network byte order)
                var value = BinaryPrimitives.ReadInt16BigEndian(_span.Slice(_position, 2));
                _position += 2;
                return value;
            }

            public uint ReadUInt32()
            {
                // Big-endian (network byte order)
                var value = BinaryPrimitives.ReadUInt32BigEndian(_span.Slice(_position, 4));
                _position += 4;
                return value;
            }

            public int ReadInt32()
            {
                var value = BinaryPrimitives.ReadInt32BigEndian(_span.Slice(_position, 4));
                _position += 4;
                return value;
            }

            public ulong ReadUInt64()
            {
                // Big-endian (network byte order)
                var value = BinaryPrimitives.ReadUInt64BigEndian(_span.Slice(_position, 8));
                _position += 8;
                return value;
            }

            public long ReadInt64()
            {
                var value = BinaryPrimitives.ReadInt64BigEndian(_span.Slice(_position, 8));
                _position += 8;
                return value;
            }

            public float ReadFloat()
            {
                var value = BinaryPrimitives.ReadSingleBigEndian(_span.Slice(_position, 4));
                _position += 4;
                return value;
            }

            public double ReadDouble()
            {
                var value = BinaryPrimitives.ReadDoubleBigEndian(_span.Slice(_position, 8));
                _position += 8;
                return value;
            }

            public string ReadString()
            {
                // String format: [Length:4][Bytes:N]
                var length = ReadUInt32();
                if (length == 0)
                    return string.Empty;

                if (_position + length > _span.Length)
                    throw new InvalidOperationException("String length exceeds buffer size");

                var stringBytes = _span.Slice(_position, (int)length);
                _position += (int)length;
                return Encoding.GetString(stringBytes);
            }

            public byte[] ReadByteArray(int lengthSize = 4)
            {
                // Array format: [Length:N][Bytes:M]
                var length = lengthSize switch
                {
                    1 => ReadByte(),
                    2 => ReadUInt16(),
                    4 => ReadUInt32(),
                    _ => throw new InvalidOperationException($"Invalid length size: {lengthSize}")
                };
                
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
        public readonly ref struct SpanWriter
        {
            private readonly IBufferWriter<byte> _writer;

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
                BinaryPrimitives.WriteUInt16BigEndian(_writer.GetSpan(2), value);
                _writer.Advance(2);
            }

            public void WriteInt16(short value)
            {
                BinaryPrimitives.WriteInt16BigEndian(_writer.GetSpan(2), value);
                _writer.Advance(2);
            }

            public void WriteUInt32(uint value)
            {
                // Big-endian (network byte order)
                BinaryPrimitives.WriteUInt32BigEndian(_writer.GetSpan(4), value);
                _writer.Advance(4);
            }

            public void WriteInt32(int value)
            {
                BinaryPrimitives.WriteInt32BigEndian(_writer.GetSpan(4), value);
                _writer.Advance(4);
            }

            public void WriteUInt64(ulong value)
            {
                // Big-endian (network byte order)
                BinaryPrimitives.WriteUInt64BigEndian(_writer.GetSpan(8), value);
                _writer.Advance(8);
            }

            public void WriteInt64(long value)
            {
                BinaryPrimitives.WriteInt64BigEndian(_writer.GetSpan(8), value);
                _writer.Advance(8);
            }

            public void WriteFloat(float value)
            {
                BinaryPrimitives.WriteSingleBigEndian(_writer.GetSpan(4), value);
                _writer.Advance(4);
            }

            public void WriteDouble(double value)
            {
                BinaryPrimitives.WriteDoubleBigEndian(_writer.GetSpan(8), value);
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

                var lengthSpan = _writer.GetSpan(4);
                _writer.Advance(4);
                var bytesLength = Encoding.GetBytes(value, _writer.GetSpan());
                _writer.Advance(bytesLength);
                BinaryPrimitives.WriteUInt32BigEndian(lengthSpan, (uint)bytesLength);
            }

            public void WriteByteArray(byte[] value, int lengthSize = 4)
            {
                // Array format: [Length:N][Bytes:M]
                if (value == null || value.Length == 0)
                {
                    switch (lengthSize)
                    {
                        case 1:
                            WriteByte(0);
                            break;
                        case 2:
                            WriteUInt16(0);
                            break;
                        case 4:
                            WriteUInt32(0);
                            break;
                        default:
                            throw new InvalidOperationException($"Invalid length size: {lengthSize}");
                    }
                    return;
                }

                // Write length based on lengthSize
                switch (lengthSize)
                {
                    case 1:
                        if (value.Length > byte.MaxValue)
                            throw new InvalidOperationException($"Array length {value.Length} exceeds maximum for 1-byte length ({byte.MaxValue})");
                        WriteByte((byte)value.Length);
                        break;
                    case 2:
                        if (value.Length > ushort.MaxValue)
                            throw new InvalidOperationException($"Array length {value.Length} exceeds maximum for 2-byte length ({ushort.MaxValue})");
                        WriteUInt16((ushort)value.Length);
                        break;
                    case 4:
                        WriteUInt32((uint)value.Length);
                        break;
                    default:
                        throw new InvalidOperationException($"Invalid length size: {lengthSize}");
                }

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
