using System.Buffers.Binary;
using Shouldly;

namespace Core.Infrastructure.Network.Tests;

public class BigEndianLengthFramerTests
{
    private readonly BigEndianLengthFramer _framer = new();

    [Fact]
    public void TryExtractPacket_SinglePacketWithPayload_ExtractsCorrectly()
    {
        // GIVEN: A buffer containing a single valid packet with a payload
        var serializer = new BinaryPacketSerializer();
        var payload = new TestPayload { Id = 42 };
        var created = _framer.CreatePacket((byte)0x10, payload, serializer);
        var buffer = created;

        // WHEN: Extracting a packet from the buffer
        _framer.TryExtractPacket(ref buffer, out var packet).ShouldBeTrue();

        // THEN: The packet is extracted with correct opcode and payload data
        var opcode = _framer.ExtractOpcode(packet.Span, out var offset);
        opcode.ShouldBe((byte)0x10);
        var data = serializer.Deserialize<TestPayload>(packet[offset..].Span);
        data.Id.ShouldBe((byte)42);
        buffer.Length.ShouldBe(0);
    }

    [Fact]
    public void TryExtractPacket_EmptyBuffer_ReturnsFalse()
    {
        // GIVEN: An empty buffer with no data
        var buffer = ReadOnlyMemory<byte>.Empty;

        // WHEN: Attempting to extract a packet
        // THEN: Extraction fails and returns false
        _framer.TryExtractPacket(ref buffer, out _).ShouldBeFalse();
    }

    [Fact]
    public void TryExtractPacket_IncompleteLengthHeader_ReturnsFalse()
    {
        // GIVEN: A buffer with fewer than 4 bytes (incomplete big-endian length header)
        var buffer = new ReadOnlyMemory<byte>(new byte[] { 0x00, 0x00, 0x05 });

        // WHEN: Attempting to extract a packet
        // THEN: Extraction fails and buffer remains unchanged
        _framer.TryExtractPacket(ref buffer, out _).ShouldBeFalse();
        buffer.Length.ShouldBe(3);
    }

    [Fact]
    public void TryExtractPacket_ZeroLengthFrame_IsSkipped()
    {
        // GIVEN: A buffer with a zero-length frame followed by a valid packet
        var serializer = new BinaryPacketSerializer();
        var payload = new TestPayload { Id = 99 };
        var realPacket = _framer.CreatePacket((byte)0x20, payload, serializer);
        var combined = new byte[4 + realPacket.Length];
        combined[0] = 0; combined[1] = 0; combined[2] = 0; combined[3] = 0;
        realPacket.CopyTo(combined.AsMemory(4));
        var buffer = new ReadOnlyMemory<byte>(combined);

        // WHEN: Extracting a packet
        _framer.TryExtractPacket(ref buffer, out var packet).ShouldBeTrue();

        // THEN: The zero-length frame is skipped and the valid packet is extracted
        var opcode = _framer.ExtractOpcode(packet.Span, out var offset);
        opcode.ShouldBe((byte)0x20);
    }

    [Fact]
    public void TryExtractPacket_NegativeLengthHeader_ReturnsFalse()
    {
        // GIVEN: A buffer with a negative length header value (invalid)
        var data = new byte[8];
        BinaryPrimitives.WriteInt32BigEndian(data, -1);
        data[4] = 0x01;
        data[5] = 0x02; data[6] = 0x03; data[7] = 0x04;
        var buffer = new ReadOnlyMemory<byte>(data);

        // WHEN: Attempting to extract a packet
        // THEN: Extraction fails due to invalid negative length
        _framer.TryExtractPacket(ref buffer, out _).ShouldBeFalse();
    }

    [Fact]
    public void TryExtractPacket_InsufficientDataForDeclaredLength_ReturnsFalse()
    {
        // GIVEN: A buffer where the length header declares 100 bytes but only 8 are available
        var data = new byte[8];
        BinaryPrimitives.WriteInt32BigEndian(data, 100);
        data[4] = 0x01;
        var buffer = new ReadOnlyMemory<byte>(data);

        // WHEN: Attempting to extract a packet
        // THEN: Extraction fails due to insufficient data
        _framer.TryExtractPacket(ref buffer, out _).ShouldBeFalse();
    }

    [Fact]
    public void TryExtractPacket_MultipleZeroLengthFramesBeforeRealPacket()
    {
        var serializer = new BinaryPacketSerializer();
        var payload = new TestPayload { Id = 77 };
        var realPacket = _framer.CreatePacket((byte)0x30, payload, serializer);

        // 3 zero-length frames, then real packet
        var combined = new byte[12 + realPacket.Length];
        realPacket.CopyTo(combined.AsMemory(12));

        var buffer = new ReadOnlyMemory<byte>(combined);

        _framer.TryExtractPacket(ref buffer, out var packet).ShouldBeTrue();
        var opcode = _framer.ExtractOpcode(packet.Span, out _);
        opcode.ShouldBe((byte)0x30);
    }

    [Fact]
    public void ExtractOpcode_ReturnsFirstByte()
    {
        // GIVEN: A packet with opcode 0x99 as the first byte
        var packet = new byte[] { 0x99, 0xAA, 0xBB };

        // WHEN: Extracting the opcode
        var opcode = _framer.ExtractOpcode(packet, out var offset);

        // THEN: The first byte is returned as the opcode and offset points to payload
        opcode.ShouldBe((byte)0x99);
        offset.ShouldBe(1);
    }

    [Fact]
    public void CreatePacket_RoundTrips_WithMultipleFields()
    {
        // GIVEN: A payload object with multiple fields
        var serializer = new BinaryPacketSerializer();
        var original = new MultiFieldPayload { A = 0x1234, B = 0xAB };

        // WHEN: Creating a packet and then extracting it
        var packetBytes = _framer.CreatePacket((byte)0x55, original, serializer);
        var buffer = packetBytes;
        _framer.TryExtractPacket(ref buffer, out var extracted).ShouldBeTrue();

        // THEN: The extracted packet contains the original data with correct opcode
        var opcode = _framer.ExtractOpcode(extracted.Span, out var offset);
        opcode.ShouldBe((byte)0x55);
        var deserialized = serializer.Deserialize<MultiFieldPayload>(extracted[offset..].Span);
        deserialized.A.ShouldBe((ushort)0x1234);
        deserialized.B.ShouldBe((byte)0xAB);
    }

    [Fact]
    public void CreatePacket_EmptyPayload_ProducesValidPacket()
    {
        // GIVEN: An empty payload object with no fields
        var serializer = new BinaryPacketSerializer();
        var original = new EmptyPayload();

        // WHEN: Creating a packet and extracting it
        var packetBytes = _framer.CreatePacket((byte)0x01, original, serializer);
        var buffer = packetBytes;
        _framer.TryExtractPacket(ref buffer, out var extracted).ShouldBeTrue();

        // THEN: A valid packet is created with the correct opcode
        var opcode = _framer.ExtractOpcode(extracted.Span, out _);
        opcode.ShouldBe((byte)0x01);
    }

    public class TestPayload { public byte Id { get; set; } }
    public class MultiFieldPayload { public ushort A { get; set; } public byte B { get; set; } }
    public class EmptyPayload { }
}
