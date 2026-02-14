using System.Buffers;
using System.Buffers.Binary;
using System.Text;
using Shouldly;

namespace Core.Infrastructure.Network.Tests;

public class BinaryPacketSerializerTests
{
    private static readonly Encoding WireEncoding = Encoding.GetEncoding("iso-8859-1");
    private readonly BinaryPacketSerializer _serializer = new();

    [Fact]
    public void RoundTrip_ByteProperty()
    {
        // GIVEN: A packet with a byte property set to 0xAB
        var original = new BytePacket { Value = 0xAB };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<BytePacket>(original);

        // THEN: The byte value is preserved correctly
        result.Value.ShouldBe((byte)0xAB);
    }

    [Fact]
    public void RoundTrip_SByteProperty()
    {
        // GIVEN: A packet with a signed byte property set to -42
        var original = new SBytePacket { Value = -42 };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<SBytePacket>(original);

        // THEN: The signed byte value is preserved correctly
        result.Value.ShouldBe((sbyte)-42);
    }

    [Fact]
    public void RoundTrip_Int16Property()
    {
        // GIVEN: A packet with a 16-bit signed integer set to -12345
        var original = new Int16Packet { Value = -12345 };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<Int16Packet>(original);

        // THEN: The Int16 value is preserved correctly
        result.Value.ShouldBe((short)-12345);
    }

    [Fact]
    public void RoundTrip_UInt16Property()
    {
        // GIVEN: A packet with a 16-bit unsigned integer set to 0xABCD
        var original = new UInt16Packet { Value = 0xABCD };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<UInt16Packet>(original);

        // THEN: The UInt16 value is preserved correctly
        result.Value.ShouldBe((ushort)0xABCD);
    }

    [Fact]
    public void RoundTrip_Int32Property()
    {
        // GIVEN: A packet with a 32-bit signed integer set to -123456789
        var original = new Int32Packet { Value = -123456789 };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<Int32Packet>(original);

        // THEN: The Int32 value is preserved correctly
        result.Value.ShouldBe(-123456789);
    }

    [Fact]
    public void RoundTrip_UInt32Property()
    {
        // GIVEN: A packet with a 32-bit unsigned integer set to 0xDEADBEEF
        var original = new UInt32Packet { Value = 0xDEADBEEF };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<UInt32Packet>(original);

        // THEN: The UInt32 value is preserved correctly
        result.Value.ShouldBe(0xDEADBEEF);
    }

    [Fact]
    public void RoundTrip_Int64Property()
    {
        // GIVEN: A packet with a 64-bit signed integer set to long.MinValue
        var original = new Int64Packet { Value = long.MinValue };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<Int64Packet>(original);

        // THEN: The Int64 value is preserved correctly
        result.Value.ShouldBe(long.MinValue);
    }

    [Fact]
    public void RoundTrip_UInt64Property()
    {
        // GIVEN: A packet with a 64-bit unsigned integer set to ulong.MaxValue
        var original = new UInt64Packet { Value = ulong.MaxValue };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<UInt64Packet>(original);

        // THEN: The UInt64 value is preserved correctly
        result.Value.ShouldBe(ulong.MaxValue);
    }

    [Fact]
    public void RoundTrip_DoubleProperty()
    {
        // GIVEN: A packet with a double-precision floating point value of Pi
        var original = new DoublePacket { Value = 3.141592653589793 };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<DoublePacket>(original);

        // THEN: The double value is preserved with full precision
        result.Value.ShouldBe(3.141592653589793);
    }

    [Fact]
    public void RoundTrip_BoolProperty_True()
    {
        // GIVEN: A packet with a boolean property set to true
        var original = new BoolPacket { Value = true };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<BoolPacket>(original);

        // THEN: The boolean true value is preserved correctly
        result.Value.ShouldBeTrue();
    }

    [Fact]
    public void RoundTrip_BoolProperty_False()
    {
        // GIVEN: A packet with a boolean property set to false
        var original = new BoolPacket { Value = false };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<BoolPacket>(original);

        // THEN: The boolean false value is preserved correctly
        result.Value.ShouldBeFalse();
    }

    [Fact]
    public void RoundTrip_FloatProperty()
    {
        // GIVEN: A packet with a single-precision float set to 1.5
        var original = new FloatPacket { Value = 1.5f };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<FloatPacket>(original);

        // THEN: The float value is preserved correctly
        result.Value.ShouldBe(1.5f);
    }

    [Fact]
    public void RoundTrip_FloatProperty_NegativeValue()
    {
        // GIVEN: A packet with a negative float value of -3.14
        var original = new FloatPacket { Value = -3.14f };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<FloatPacket>(original);

        // THEN: The negative float value is preserved correctly
        result.Value.ShouldBe(-3.14f);
    }

    [Fact]
    public void Serialize_Float_IsBigEndian()
    {
        // GIVEN: A packet with a float value of 1.0
        var original = new FloatPacket { Value = 1.0f };
        var writer = new ArrayBufferWriter<byte>();

        // WHEN: Serializing the packet
        _serializer.Serialize(writer, original);

        // THEN: The float is written in big-endian byte order (0x3F800000)
        writer.WrittenSpan[0].ShouldBe((byte)0x3F);
        writer.WrittenSpan[1].ShouldBe((byte)0x80);
        writer.WrittenSpan[2].ShouldBe((byte)0x00);
        writer.WrittenSpan[3].ShouldBe((byte)0x00);
    }

    [Fact]
    public void RoundTrip_StringProperty()
    {
        // GIVEN: A packet with a string property set to "Hello World"
        var original = new StringPacket { Name = "Hello World" };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<StringPacket>(original);

        // THEN: The string value is preserved correctly
        result.Name.ShouldBe("Hello World");
    }

    [Fact]
    public void RoundTrip_EmptyString()
    {
        // GIVEN: A packet with an empty string property
        var original = new StringPacket { Name = "" };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<StringPacket>(original);

        // THEN: The empty string is preserved correctly
        result.Name.ShouldBe("");
    }

    [Fact]
    public void Serialize_NullString_IsSkipped()
    {
        // GIVEN: A packet with a null string property
        var original = new StringPacket { Name = null! };
        var writer = new ArrayBufferWriter<byte>();

        // WHEN: Serializing the packet
        _serializer.Serialize(writer, original);

        // THEN: The null string is skipped and no bytes are written
        writer.WrittenCount.ShouldBe(0);
    }

    [Fact]
    public void RoundTrip_EnumProperty()
    {
        // GIVEN: A packet with an enum property set to TestStatus.Active
        var original = new EnumPacket { Status = TestStatus.Active };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<EnumPacket>(original);

        // THEN: The enum value is preserved correctly
        result.Status.ShouldBe(TestStatus.Active);
    }

    [Fact]
    public void RoundTrip_EnumProperty_ZeroValue()
    {
        // GIVEN: A packet with an enum property set to TestStatus.None (zero value)
        var original = new EnumPacket { Status = TestStatus.None };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<EnumPacket>(original);

        // THEN: The zero-value enum is preserved correctly
        result.Status.ShouldBe(TestStatus.None);
    }

    [Fact]
    public void Enum_SerializedAsSingleByte()
    {
        // GIVEN: A packet with an enum property set to TestStatus.Active
        var original = new EnumPacket { Status = TestStatus.Active };
        var writer = new ArrayBufferWriter<byte>();

        // WHEN: Serializing the packet
        _serializer.Serialize(writer, original);

        // THEN: The enum is serialized as a single byte with its underlying value
        writer.WrittenCount.ShouldBe(1);
        writer.WrittenSpan[0].ShouldBe((byte)TestStatus.Active);
    }

    [Fact]
    public void RoundTrip_ByteArray()
    {
        // GIVEN: A packet with a byte array containing three elements
        var original = new ByteArrayPacket { Data = new byte[] { 0x01, 0x02, 0x03 } };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<ByteArrayPacket>(original);

        // THEN: The byte array is preserved correctly
        result.Data.ShouldBe(new byte[] { 0x01, 0x02, 0x03 });
    }

    [Fact]
    public void RoundTrip_EmptyByteArray()
    {
        // GIVEN: A packet with an empty byte array
        var original = new ByteArrayPacket { Data = Array.Empty<byte>() };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<ByteArrayPacket>(original);

        // THEN: The empty array is preserved correctly
        result.Data.ShouldBeEmpty();
    }

    [Fact]
    public void RoundTrip_TypedArray()
    {
        // GIVEN: A packet with a ushort array containing three elements
        var original = new UInt16ArrayPacket { Items = new ushort[] { 100, 200, 300 } };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<UInt16ArrayPacket>(original);

        // THEN: The ushort array is preserved correctly
        result.Items.ShouldBe(new ushort[] { 100, 200, 300 });
    }

    [Fact]
    public void RoundTrip_EmptyTypedArray()
    {
        // GIVEN: A packet with an empty ushort array
        var original = new UInt16ArrayPacket { Items = Array.Empty<ushort>() };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<UInt16ArrayPacket>(original);

        // THEN: The empty typed array is preserved correctly
        result.Items.ShouldBeEmpty();
    }

    [Fact]
    public void RoundTrip_ListProperty()
    {
        // GIVEN: A packet with a List<byte> containing three elements
        var original = new ListPacket { Values = new List<byte> { 10, 20, 30 } };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<ListPacket>(original);

        // THEN: The list contents are preserved correctly
        result.Values.ShouldBe(new List<byte> { 10, 20, 30 });
    }

    [Fact]
    public void RoundTrip_EmptyList()
    {
        // GIVEN: A packet with an empty List<byte>
        var original = new ListPacket { Values = new List<byte>() };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<ListPacket>(original);

        // THEN: The empty list is preserved correctly
        result.Values.ShouldBeEmpty();
    }

    [Fact]
    public void RoundTrip_MultipleProperties_PreservesOrder()
    {
        // GIVEN: A packet with multiple properties of different types
        var original = new MultiPropertyPacket
        {
            Id = 0xAB,
            Count = 0x1234,
            Name = "test"
        };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<MultiPropertyPacket>(original);

        // THEN: All properties are preserved with correct values in order
        result.Id.ShouldBe((byte)0xAB);
        result.Count.ShouldBe((ushort)0x1234);
        result.Name.ShouldBe("test");
    }

    [Fact]
    public void RoundTrip_NullableProperty_WithValue()
    {
        // GIVEN: A packet with a nullable int property set to 42
        var original = new NullablePacket { MaybeId = 42 };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<NullablePacket>(original);

        // THEN: The nullable value is preserved correctly
        result.MaybeId.ShouldBe(42);
    }

    [Fact]
    public void Serialize_NullableProperty_NullValue_SkipsProperty()
    {
        // GIVEN: A packet with a nullable int property set to null
        var original = new NullablePacket { MaybeId = null };
        var writer = new ArrayBufferWriter<byte>();

        // WHEN: Serializing the packet
        _serializer.Serialize(writer, original);

        // THEN: The null property is skipped and no bytes are written
        writer.WrittenCount.ShouldBe(0);
    }

    [Fact]
    public void Deserialize_NullableProperty_AtEndOfBuffer_SetsNull()
    {
        // GIVEN: An empty buffer with no data for nullable property
        var empty = ReadOnlySpan<byte>.Empty;

        // WHEN: Deserializing the packet
        var result = _serializer.Deserialize<NullablePacket>(empty);

        // THEN: The nullable property is set to null
        result.MaybeId.ShouldBeNull();
    }

    [Fact]
    public void Deserialize_TrailingNullableString_AtEndOfBuffer_SetsNull()
    {
        // GIVEN: A buffer with only the Id byte, no data for trailing nullable string
        var data = new byte[] { 0x42 };

        // WHEN: Deserializing the packet
        var result = _serializer.Deserialize<TrailingNullablePacket>(data);

        // THEN: The Id is read and the trailing nullable string is set to null
        result.Id.ShouldBe((byte)0x42);
        result.OptionalName.ShouldBeNull();
    }

    [Fact]
    public void RoundTrip_PacketLengthAttribute_TwoByte()
    {
        // GIVEN: A packet with byte array using 2-byte length prefix attribute
        var original = new TwoByteArrayLengthPacket { Data = new byte[] { 1, 2, 3, 4, 5 } };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<TwoByteArrayLengthPacket>(original);

        // THEN: The array is correctly serialized with 2-byte length and preserved
        result.Data.ShouldBe(new byte[] { 1, 2, 3, 4, 5 });
    }

    [Fact]
    public void RoundTrip_PacketLengthAttribute_FourByte()
    {
        // GIVEN: A packet with byte array using 4-byte length prefix attribute
        var original = new FourByteArrayLengthPacket { Data = new byte[] { 10, 20, 30 } };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<FourByteArrayLengthPacket>(original);

        // THEN: The array is correctly serialized with 4-byte length and preserved
        result.Data.ShouldBe(new byte[] { 10, 20, 30 });
    }

    [Fact]
    public void Deserialize_ValueType_ThrowsInvalidOperation()
    {
        // GIVEN: A byte buffer and an attempt to deserialize to a primitive value type
        var data = new byte[] { 0x01, 0x02, 0x03, 0x04 };

        // WHEN: Attempting to deserialize to int (a value type)
        // THEN: An InvalidOperationException is thrown
        Should.Throw<InvalidOperationException>(() =>
        {
            _serializer.Deserialize<int>(data);
        });
    }

    [Fact]
    public void Serialize_ValueType_ThrowsInvalidOperation()
    {
        // GIVEN: A primitive value type (int) and an attempt to serialize it
        var writer = new ArrayBufferWriter<byte>();

        // WHEN: Attempting to serialize an int value type
        // THEN: An InvalidOperationException is thrown
        Should.Throw<InvalidOperationException>(() =>
        {
            _serializer.Serialize(writer, 42);
        });
    }

    [Fact]
    public void Serialize_Int16_IsBigEndian()
    {
        // GIVEN: A packet with Int16 value 0x0102
        var original = new Int16Packet { Value = 0x0102 };
        var writer = new ArrayBufferWriter<byte>();

        // WHEN: Serializing the packet
        _serializer.Serialize(writer, original);

        // THEN: The Int16 is written in big-endian byte order (0x01 0x02)
        writer.WrittenSpan[0].ShouldBe((byte)0x01);
        writer.WrittenSpan[1].ShouldBe((byte)0x02);
    }

    [Fact]
    public void Serialize_Int32_IsBigEndian()
    {
        // GIVEN: A packet with Int32 value 0x01020304
        var original = new Int32Packet { Value = 0x01020304 };
        var writer = new ArrayBufferWriter<byte>();

        // WHEN: Serializing the packet
        _serializer.Serialize(writer, original);

        // THEN: The Int32 is written in big-endian byte order (0x01 0x02 0x03 0x04)
        writer.WrittenSpan[0].ShouldBe((byte)0x01);
        writer.WrittenSpan[1].ShouldBe((byte)0x02);
        writer.WrittenSpan[2].ShouldBe((byte)0x03);
        writer.WrittenSpan[3].ShouldBe((byte)0x04);
    }

    [Fact]
    public void Serialize_String_HasBigEndianLengthPrefix()
    {
        // GIVEN: A packet with a string property "AB" (2 characters)
        var original = new StringPacket { Name = "AB" };
        var writer = new ArrayBufferWriter<byte>();

        // WHEN: Serializing the packet
        _serializer.Serialize(writer, original);

        // THEN: The string is prefixed with 4-byte big-endian length (2) followed by string bytes
        writer.WrittenCount.ShouldBe(6);
        BinaryPrimitives.ReadUInt32BigEndian(writer.WrittenSpan[..4]).ShouldBe(2u);
        writer.WrittenSpan[4].ShouldBe((byte)'A');
        writer.WrittenSpan[5].ShouldBe((byte)'B');
    }

    [Fact]
    public void Deserialize_UnsupportedPropertyType_Throws()
    {
        // GIVEN: A packet with an unsupported property type (Dictionary)
        var writer = new ArrayBufferWriter<byte>();
        var original = new UnsupportedTypePacket();

        // WHEN: Attempting to serialize the packet
        // THEN: A NotSupportedException is thrown for the unsupported type
        Should.Throw<NotSupportedException>(() =>
        {
            _serializer.Serialize(writer, original);
        });
    }

    [Fact]
    public void Deserialize_EmptyPayload_EmptyObject()
    {
        // GIVEN: An empty buffer and a packet type with no properties
        // WHEN: Deserializing the empty buffer
        var result = _serializer.Deserialize<EmptyPacket>(ReadOnlySpan<byte>.Empty);

        // THEN: A valid empty packet object is created
        result.ShouldNotBeNull();
    }

    [Fact]
    public void RoundTrip_ReadOnlyProperties_AreSkipped()
    {
        // GIVEN: A packet with both writable and read-only properties
        var original = new ReadOnlyPropertyPacket { Writable = 0x42 };

        // WHEN: Serializing and deserializing the packet
        var result = RoundTrip<ReadOnlyPropertyPacket>(original);

        // THEN: Writable property is preserved, read-only property is skipped and has default value
        result.Writable.ShouldBe((byte)0x42);
        result.ReadOnly.ShouldBe(0);
    }

    [Fact]
    public void Factory_CreateReturnsSameInstance()
    {
        // GIVEN: A BinaryPacketSerializerFactory instance
        var factory = new BinaryPacketSerializerFactory();

        // WHEN: Calling Create() multiple times
        var a = factory.Create();
        var b = factory.Create();

        // THEN: The factory returns the same singleton instance
        a.ShouldBeSameAs(b);
    }

    private T RoundTrip<T>(T original)
    {
        var writer = new ArrayBufferWriter<byte>();
        _serializer.Serialize(writer, original);
        return _serializer.Deserialize<T>(writer.WrittenSpan);
    }

    public class BytePacket { public byte Value { get; set; } }
    public class SBytePacket { public sbyte Value { get; set; } }
    public class Int16Packet { public short Value { get; set; } }
    public class UInt16Packet { public ushort Value { get; set; } }
    public class Int32Packet { public int Value { get; set; } }
    public class UInt32Packet { public uint Value { get; set; } }
    public class Int64Packet { public long Value { get; set; } }
    public class UInt64Packet { public ulong Value { get; set; } }
    public class FloatPacket { public float Value { get; set; } }
    public class DoublePacket { public double Value { get; set; } }
    public class BoolPacket { public bool Value { get; set; } }
    public class StringPacket { public string Name { get; set; } = "";  }
    public class EnumPacket { public TestStatus Status { get; set; } }
    public class ByteArrayPacket { [PacketLength(4)] public byte[] Data { get; set; } = Array.Empty<byte>(); }
    public class UInt16ArrayPacket { public ushort[] Items { get; set; } = Array.Empty<ushort>(); }
    public class ListPacket { public List<byte> Values { get; set; } = new(); }
    public class MultiPropertyPacket { public byte Id { get; set; } public ushort Count { get; set; } public string Name { get; set; } = ""; }
    public class NullablePacket { public int? MaybeId { get; set; } }
    public class TrailingNullablePacket { public byte Id { get; set; } public string? OptionalName { get; set; } }
    public class EmptyPacket { }
    public class ReadOnlyPropertyPacket { public byte Writable { get; set; } public int ReadOnly => 0; }

    public class TwoByteArrayLengthPacket
    {
        [PacketLength(2)]
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }

    public class FourByteArrayLengthPacket
    {
        [PacketLength(4)]
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }

    public class UnsupportedTypePacket
    {
        public Dictionary<string, string> Map { get; set; } = new();
    }

    public enum TestStatus : byte
    {
        None = 0,
        Active = 1,
        Inactive = 2
    }
}
