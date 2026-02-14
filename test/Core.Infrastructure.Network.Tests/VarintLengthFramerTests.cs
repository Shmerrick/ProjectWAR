using Shouldly;

namespace Core.Infrastructure.Network.Tests;

public class VarintLengthFramerTests
{
    private readonly VarintLengthFramer _framer = new();

    [Fact]
    public void TryExtractPacket_SinglePacketWithPayload_ExtractsCorrectly()
    {
        // GIVEN: A buffer with a varint length header (0x03) followed by opcode and 3 payload bytes
        var data = new byte[] { 0x03, 0xAA, 0x01, 0x02, 0x03 };
        var buffer = new ReadOnlyMemory<byte>(data);

        // WHEN: Extracting a packet from the buffer
        _framer.TryExtractPacket(ref buffer, out var packet).ShouldBeTrue();

        // THEN: The packet is extracted with opcode and payload bytes intact
        packet.Length.ShouldBe(4);
        packet.Span[0].ShouldBe((byte)0xAA);
        packet.Span[1].ShouldBe((byte)0x01);
        packet.Span[2].ShouldBe((byte)0x02);
        packet.Span[3].ShouldBe((byte)0x03);
        buffer.Length.ShouldBe(0);
    }

    [Fact]
    public void TryExtractPacket_ZeroLengthPayload_ExtractsOpcodeOnly()
    {
        // GIVEN: A buffer with varint length 0 indicating no payload, followed by opcode
        var data = new byte[] { 0x00, 0xFF };
        var buffer = new ReadOnlyMemory<byte>(data);

        // WHEN: Extracting a packet
        _framer.TryExtractPacket(ref buffer, out var packet).ShouldBeTrue();

        // THEN: Only the opcode byte is extracted with no payload
        packet.Length.ShouldBe(1);
        packet.Span[0].ShouldBe((byte)0xFF);
        buffer.Length.ShouldBe(0);
    }

    [Fact]
    public void TryExtractPacket_EmptyBuffer_ReturnsFalse()
    {
        // GIVEN: An empty buffer with no data
        var buffer = ReadOnlyMemory<byte>.Empty;

        // WHEN: Attempting to extract a packet
        // THEN: Extraction fails and returns false with empty packet
        _framer.TryExtractPacket(ref buffer, out var packet).ShouldBeFalse();
        packet.Length.ShouldBe(0);
    }

    [Fact]
    public void TryExtractPacket_SingleByte_ReturnsFalse()
    {
        // GIVEN: A buffer with only a varint length header (0x05) but no opcode or payload
        var buffer = new ReadOnlyMemory<byte>(new byte[] { 0x05 });

        // WHEN: Attempting to extract a packet
        // THEN: Extraction fails because opcode is missing and buffer remains unchanged
        _framer.TryExtractPacket(ref buffer, out _).ShouldBeFalse();
        buffer.Length.ShouldBe(1);
    }

    [Fact]
    public void TryExtractPacket_IncompletePayload_ReturnsFalseAndRestoresBuffer()
    {
        // GIVEN: A buffer declaring 5 bytes payload but only 4 bytes available total
        var data = new byte[] { 0x05, 0x01, 0xAA, 0xBB };
        var buffer = new ReadOnlyMemory<byte>(data);

        // WHEN: Attempting to extract a packet
        // THEN: Extraction fails due to incomplete payload and buffer remains unchanged
        _framer.TryExtractPacket(ref buffer, out _).ShouldBeFalse();
        buffer.Length.ShouldBe(4);
    }

    [Fact]
    public void TryExtractPacket_MultiByteVarint_ExtractsCorrectly()
    {
        // GIVEN: A buffer with a 2-byte varint (0xC8 0x01 = 200) encoding 200 bytes payload length
        var payloadSize = 200;
        var payload = new byte[payloadSize];
        for (var i = 0; i < payloadSize; i++) payload[i] = (byte)(i & 0xFF);

        var data = new byte[2 + 1 + payloadSize];
        data[0] = 0xC8;
        data[1] = 0x01;
        data[2] = 0x42;
        Array.Copy(payload, 0, data, 3, payloadSize);
        var buffer = new ReadOnlyMemory<byte>(data);

        // WHEN: Extracting a packet
        _framer.TryExtractPacket(ref buffer, out var packet).ShouldBeTrue();

        // THEN: The complete 201-byte packet (opcode + 200 payload) is extracted
        packet.Length.ShouldBe(1 + payloadSize);
        packet.Span[0].ShouldBe((byte)0x42);
        buffer.Length.ShouldBe(0);
    }

    [Fact]
    public void TryExtractPacket_MultiplePacketsInBuffer_ExtractsOneAtATime()
    {
        // GIVEN: A buffer containing two complete packets back-to-back
        var data = new byte[] { 0x01, 0xAA, 0x11, 0x02, 0xBB, 0x22, 0x33 };
        var buffer = new ReadOnlyMemory<byte>(data);

        // WHEN: Extracting the first packet
        _framer.TryExtractPacket(ref buffer, out var packet1).ShouldBeTrue();

        // THEN: Only the first packet is extracted, buffer advances to second packet
        packet1.Length.ShouldBe(2);
        packet1.Span[0].ShouldBe((byte)0xAA);
        packet1.Span[1].ShouldBe((byte)0x11);

        // WHEN: Extracting the second packet
        _framer.TryExtractPacket(ref buffer, out var packet2).ShouldBeTrue();

        // THEN: The second packet is extracted and buffer is fully consumed
        packet2.Length.ShouldBe(3);
        packet2.Span[0].ShouldBe((byte)0xBB);
        buffer.Length.ShouldBe(0);
    }

    [Fact]
    public void TryExtractPacket_ZeroLengthMissingOpcode_ReturnsFalse()
    {
        // GIVEN: A buffer with varint length 0 but missing the required opcode byte
        var buffer = new ReadOnlyMemory<byte>(new byte[] { 0x00 });

        // WHEN: Attempting to extract a packet
        // THEN: Extraction fails because opcode byte is required even with zero payload
        _framer.TryExtractPacket(ref buffer, out _).ShouldBeFalse();
    }

    [Fact]
    public void TryExtractPacket_NegativeVarintOverflow_ReturnsFalse()
    {
        // GIVEN: A buffer with a varint that decodes to a negative or overflow value
        var data = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x7F, 0x01, 0x00 };
        var buffer = new ReadOnlyMemory<byte>(data);

        // WHEN: Attempting to extract a packet
        // THEN: Extraction fails due to invalid varint encoding
        _framer.TryExtractPacket(ref buffer, out _).ShouldBeFalse();
    }

    [Fact]
    public void ExtractOpcode_ReturnsFirstByte()
    {
        // GIVEN: A packet with opcode 0x42 as the first byte
        var packet = new byte[] { 0x42, 0x01, 0x02 };

        // WHEN: Extracting the opcode
        var opcode = _framer.ExtractOpcode(packet, out var payloadOffset);

        // THEN: The first byte is returned as opcode and offset points to payload
        opcode.ShouldBe((byte)0x42);
        payloadOffset.ShouldBe(1);
    }

    [Fact]
    public void ExtractOpcode_MinMaxOpcodes()
    {
        // GIVEN: Packets with minimum (0x00) and maximum (0xFF) opcode values
        // WHEN: Extracting opcodes from each packet
        // THEN: Both boundary values are correctly extracted
        _framer.ExtractOpcode(new byte[] { 0x00 }, out _).ShouldBe((byte)0x00);
        _framer.ExtractOpcode(new byte[] { 0xFF }, out _).ShouldBe((byte)0xFF);
    }

    [Fact]
    public void CreatePacket_RoundTrips_WithTryExtractPacket()
    {
        // GIVEN: A payload object with a ushort value
        var serializer = new BinaryPacketSerializer();
        var original = new SimplePayload { Value = 0x1234 };

        // WHEN: Creating a packet and then extracting it back
        var packetBytes = _framer.CreatePacket((byte)0x10, original, serializer);
        var buffer = packetBytes;

        _framer.TryExtractPacket(ref buffer, out var extracted).ShouldBeTrue();

        // THEN: The extracted packet contains the original data with correct opcode
        var opcode = _framer.ExtractOpcode(extracted.Span, out var offset);
        opcode.ShouldBe((byte)0x10);

        var payload = extracted[offset..];
        var deserialized = serializer.Deserialize<SimplePayload>(payload.Span);
        deserialized.Value.ShouldBe((ushort)0x1234);
    }

    [Fact]
    public void CreatePacket_EmptyPayload_ProducesValidPacket()
    {
        // GIVEN: An empty payload object with no fields
        var serializer = new BinaryPacketSerializer();
        var original = new EmptyPayload();

        // WHEN: Creating and extracting a packet with empty payload
        var packetBytes = _framer.CreatePacket((byte)0x01, original, serializer);
        var buffer = packetBytes;

        _framer.TryExtractPacket(ref buffer, out var extracted).ShouldBeTrue();

        // THEN: A valid packet is created with only the opcode
        var opcode = _framer.ExtractOpcode(extracted.Span, out _);
        opcode.ShouldBe((byte)0x01);
    }

    public class SimplePayload
    {
        public ushort Value { get; set; }
    }

    public class EmptyPayload { }
}
