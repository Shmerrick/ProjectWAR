using System.Buffers;
using System.Buffers.Binary;
using Core.Infrastructure.Network;
using Shouldly;
using WorldServer.NetWork.V2;

namespace WorldServer.Tests;

public class GameServerFramerTests
{
    private readonly GameServerFramer _framer = new();

    #region Helpers

    /// <summary>
    /// Builds a raw incoming wire-format packet.
    /// Format: [uint16 BE packetSize][8-byte header][payload of packetSize+2 bytes]
    /// </summary>
    private static byte[] BuildIncomingPacket(
        byte opcode,
        byte[] payload,
        ushort sequenceId = 0,
        ushort sessionId = 0,
        ushort unk1 = 0,
        byte unk2 = 0)
    {
        // packetSize = payload.Length - 2 (the protocol defines payload length = packetSize + 2)
        var packetSize = (ushort)(payload.Length - 2);
        var totalLength = 2 + 8 + payload.Length; // sizePrefix + header + payload
        var buffer = new byte[totalLength];

        var offset = 0;

        // Size prefix (uint16 BE)
        BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(offset, 2), packetSize);
        offset += 2;

        // 8-byte header
        BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(offset, 2), sequenceId);
        offset += 2;
        BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(offset, 2), sessionId);
        offset += 2;
        BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(offset, 2), unk1);
        offset += 2;
        buffer[offset++] = unk2;
        buffer[offset++] = opcode;

        // Payload
        payload.CopyTo(buffer.AsSpan(offset));

        return buffer;
    }

    /// <summary>
    /// Builds a raw incoming packet where the payload size is exactly the minimum (2 bytes).
    /// This means packetSize = 0.
    /// </summary>
    private static byte[] BuildMinimalIncomingPacket(byte opcode, byte payloadByte1, byte payloadByte2)
    {
        return BuildIncomingPacket(opcode, [payloadByte1, payloadByte2]);
    }

    #endregion

    #region TryExtractPacket

    [Fact]
    public void TryExtractPacket_CompletePacket_ReturnsTrue()
    {
        // F_ENCRYPTKEY-like: 6 bytes struct + 256 bytes key = 262 bytes payload
        var payload = new byte[262];
        payload[0] = 0x01; // cipher
        var rawPacket = BuildIncomingPacket(0x5C, payload);
        var buffer = new ReadOnlyMemory<byte>(rawPacket);

        var result = _framer.TryExtractPacket(ref buffer, out var packet);

        result.ShouldBeTrue();
        // Packet should be [8-byte header][262-byte payload] = 270 bytes
        packet.Length.ShouldBe(8 + 262);
        // Buffer should be fully consumed
        buffer.Length.ShouldBe(0);
    }

    [Fact]
    public void TryExtractPacket_InsufficientSizePrefix_ReturnsFalse()
    {
        var buffer = new ReadOnlyMemory<byte>([0x01]); // Only 1 byte, need at least 2

        var result = _framer.TryExtractPacket(ref buffer, out _);

        result.ShouldBeFalse();
        buffer.Length.ShouldBe(1); // Buffer unchanged
    }

    [Fact]
    public void TryExtractPacket_IncompletePacket_ReturnsFalse()
    {
        var payload = new byte[10];
        var rawPacket = BuildIncomingPacket(0x5C, payload);
        // Truncate the packet
        var truncated = rawPacket[..^5];
        var buffer = new ReadOnlyMemory<byte>(truncated);

        var result = _framer.TryExtractPacket(ref buffer, out _);

        result.ShouldBeFalse();
        buffer.Length.ShouldBe(truncated.Length); // Buffer unchanged
    }

    [Fact]
    public void TryExtractPacket_MinimalPayload_ReturnsTrue()
    {
        // packetSize = 0, payload = 2 bytes (the minimum)
        var rawPacket = BuildMinimalIncomingPacket(0x0B, 0xAA, 0xBB);
        var buffer = new ReadOnlyMemory<byte>(rawPacket);

        var result = _framer.TryExtractPacket(ref buffer, out var packet);

        result.ShouldBeTrue();
        packet.Length.ShouldBe(8 + 2); // header + 2-byte payload
        buffer.Length.ShouldBe(0);
    }

    [Fact]
    public void TryExtractPacket_MultiplePackets_ExtractsBothSequentially()
    {
        var payload1 = new byte[4]; // 4-byte payload
        var payload2 = new byte[6]; // 6-byte payload
        var raw1 = BuildIncomingPacket(0x0B, payload1); // F_PING
        var raw2 = BuildIncomingPacket(0x5C, payload2); // F_ENCRYPTKEY

        var combined = new byte[raw1.Length + raw2.Length];
        raw1.CopyTo(combined, 0);
        raw2.CopyTo(combined, raw1.Length);
        var buffer = new ReadOnlyMemory<byte>(combined);

        // Extract first packet
        var result1 = _framer.TryExtractPacket(ref buffer, out var packet1);
        result1.ShouldBeTrue();
        packet1.Length.ShouldBe(8 + 4);

        // Extract second packet
        var result2 = _framer.TryExtractPacket(ref buffer, out var packet2);
        result2.ShouldBeTrue();
        packet2.Length.ShouldBe(8 + 6);

        // Buffer should be fully consumed
        buffer.Length.ShouldBe(0);
    }

    [Fact]
    public void TryExtractPacket_EmptyBuffer_ReturnsFalse()
    {
        var buffer = ReadOnlyMemory<byte>.Empty;

        var result = _framer.TryExtractPacket(ref buffer, out _);

        result.ShouldBeFalse();
    }

    #endregion

    #region ExtractOpcode

    [Fact]
    public void ExtractOpcode_ReturnsOpcodeAtPosition7()
    {
        // Build a packet and extract it
        var payload = new byte[4];
        var rawPacket = BuildIncomingPacket(0x5C, payload);
        var buffer = new ReadOnlyMemory<byte>(rawPacket);
        _framer.TryExtractPacket(ref buffer, out var packet);

        var opcode = _framer.ExtractOpcode(packet.Span, out var payloadOffset);

        opcode.ShouldBe((byte)0x5C);
        payloadOffset.ShouldBe(8); // Payload starts after 8-byte header
    }

    [Fact]
    public void ExtractOpcode_PayloadStartsAtCorrectOffset()
    {
        // Create payload with known content
        var payload = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
        var rawPacket = BuildIncomingPacket(0x0B, payload);
        var buffer = new ReadOnlyMemory<byte>(rawPacket);
        _framer.TryExtractPacket(ref buffer, out var packet);

        _framer.ExtractOpcode(packet.Span, out var payloadOffset);
        var extractedPayload = packet[payloadOffset..];

        extractedPayload.ToArray().ShouldBe(payload);
    }

    [Fact]
    public void ExtractOpcode_PreservesHeaderFields()
    {
        // Verify the extracted packet contains the full header
        var payload = new byte[2];
        var rawPacket = BuildIncomingPacket(0x5C, payload, sequenceId: 0x1234, sessionId: 0x5678);
        var buffer = new ReadOnlyMemory<byte>(rawPacket);
        _framer.TryExtractPacket(ref buffer, out var packet);

        // SequenceID at offset 0-1
        BinaryPrimitives.ReadUInt16BigEndian(packet.Span[..2]).ShouldBe((ushort)0x1234);
        // SessionID at offset 2-3
        BinaryPrimitives.ReadUInt16BigEndian(packet.Span[2..4]).ShouldBe((ushort)0x5678);
    }

    #endregion

    #region CreatePacket

    [Fact]
    public void CreatePacket_ProducesCorrectOutgoingFormat()
    {
        var serializer = new GameServerSerializer();

        var packet = _framer.CreatePacket(
            0x8A,
            new NetWork.V2.Dtos.EncryptKeyResponse { Status = 1 },
            serializer);

        var bytes = packet.ToArray();
        // [uint16 BE payloadSize=1][opcode=0x8A][payload=0x01]
        bytes.Length.ShouldBe(4); // 2 + 1 + 1
        BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(0, 2)).ShouldBe((ushort)1);
        bytes[2].ShouldBe((byte)0x8A);
        bytes[3].ShouldBe((byte)0x01);
    }

    [Fact]
    public void CreatePacket_EmptyPayload_ProducesHeaderOnly()
    {
        // A serializer that writes nothing
        var serializer = new EmptySerializer();

        var packet = _framer.CreatePacket<object>(0x42, new object(), serializer);

        var bytes = packet.ToArray();
        // [uint16 BE payloadSize=0][opcode=0x42]
        bytes.Length.ShouldBe(3);
        BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(0, 2)).ShouldBe((ushort)0);
        bytes[2].ShouldBe((byte)0x42);
    }

    #endregion

    #region Round-trip

    [Fact]
    public void RoundTrip_ExtractThenCreate_PayloadMatchesOriginal()
    {
        // Simulate receiving F_ENCRYPTKEY and responding with F_RECEIVE_ENCRYPTKEY
        var inPayload = new byte[262]; // 6 struct bytes + 256 key bytes
        inPayload[0] = 0x00; // cipher = 0 (no encryption)
        inPayload[1] = 0x01; // application
        inPayload[2] = 0x01; // major
        inPayload[3] = 0x04; // minor
        inPayload[4] = 0x08; // revision
        // Fill key with pattern
        for (var i = 6; i < 262; i++)
            inPayload[i] = (byte)(i & 0xFF);

        var rawIncoming = BuildIncomingPacket(0x5C, inPayload);
        var buffer = new ReadOnlyMemory<byte>(rawIncoming);

        // Extract incoming
        _framer.TryExtractPacket(ref buffer, out var packet).ShouldBeTrue();
        var opcode = _framer.ExtractOpcode(packet.Span, out var payloadOffset);
        opcode.ShouldBe((byte)0x5C);

        var extractedPayload = packet[payloadOffset..];
        extractedPayload.ToArray().ShouldBe(inPayload);

        // Create outgoing response
        var serializer = new GameServerSerializer();
        var outgoing = _framer.CreatePacket(
            0x8A,
            new NetWork.V2.Dtos.EncryptKeyResponse { Status = 1 },
            serializer);

        var outBytes = outgoing.ToArray();
        outBytes.Length.ShouldBe(4); // 2 (size) + 1 (opcode) + 1 (status)
    }

    #endregion

    /// <summary>
    /// Test helper: serializer that writes nothing.
    /// </summary>
    private class EmptySerializer : IPacketSerializer
    {
        public T Deserialize<T>(ReadOnlySpan<byte> payload) => throw new NotImplementedException();
        public void Serialize<T>(IBufferWriter<byte> writer, T message) { /* no-op */ }
    }
}
