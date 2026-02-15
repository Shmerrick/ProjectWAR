using System;
using System.Buffers;
using System.Buffers.Binary;
using Core.Infrastructure.Network;

namespace WorldServer.NetWork.V2;

/// <summary>
/// Packet framer for the WAR game server protocol.
///
/// Incoming wire format (client → server):
/// <code>
/// [uint16 BE] packetSize
/// [uint16]    SequenceID  ┐
/// [uint16]    SessionID   │
/// [uint16]    Unk1        ├ 8-byte header (encrypted when RC4 is active)
/// [uint8]     Unk2        │
/// [uint8]     Opcode      ┘
/// [packetSize + 2 bytes]  Payload (encrypted when RC4 is active)
/// </code>
///
/// Outgoing wire format (server → client):
/// <code>
/// [uint16 BE] payloadSize (excludes size prefix and opcode)
/// [uint8]     Opcode
/// [N bytes]   Payload
/// </code>
/// </summary>
public sealed class GameServerFramer : IPacketFramer
{
    private const int SizePrefix = sizeof(ushort);
    private const int HeaderSize = 8; // SequenceID(2) + SessionID(2) + Unk1(2) + Unk2(1) + Opcode(1)
    private const int OpcodeSize = sizeof(byte);
    private const int OpcodeOffsetInHeader = 7; // Opcode is the last byte of the 8-byte header
    private const int PayloadSizeAdjustment = 2; // payload length = packetSize + 2

    /// <inheritdoc />
    public bool TryExtractPacket(ref ReadOnlyMemory<byte> buffer, out ReadOnlyMemory<byte> packet)
    {
        packet = default;

        if (buffer.Length < SizePrefix)
            return false;

        var packetSize = BinaryPrimitives.ReadUInt16BigEndian(buffer.Span[..SizePrefix]);
        var payloadLength = packetSize + PayloadSizeAdjustment;
        var totalLength = SizePrefix + HeaderSize + payloadLength;

        if (buffer.Length < totalLength)
            return false;

        // Return [8-byte header][payload] — the size prefix is consumed/stripped.
        // ExtractOpcode knows the opcode sits at offset 7 within this slice.
        packet = buffer.Slice(SizePrefix, HeaderSize + payloadLength);
        buffer = buffer[totalLength..];
        return true;
    }

    /// <inheritdoc />
    public byte ExtractOpcode(ReadOnlySpan<byte> packet, out int payloadOffset)
    {
        // The opcode is the last byte of the 8-byte header.
        // The payload begins immediately after the header.
        payloadOffset = HeaderSize;
        return packet[OpcodeOffsetInHeader];
    }

    /// <inheritdoc />
    public ReadOnlyMemory<byte> CreatePacket<T>(byte opcode, T payload, IPacketSerializer serializer)
    {
        var payloadWriter = new ArrayBufferWriter<byte>();
        serializer.Serialize(payloadWriter, payload);
        var payloadSize = payloadWriter.WrittenCount;

        // Outgoing: [uint16 BE payloadSize][opcode][payload]
        var packet = new byte[SizePrefix + OpcodeSize + payloadSize];
        BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(0, SizePrefix), (ushort)payloadSize);
        packet[SizePrefix] = opcode;
        payloadWriter.WrittenSpan.CopyTo(packet.AsSpan(SizePrefix + OpcodeSize));

        return packet;
    }
}
