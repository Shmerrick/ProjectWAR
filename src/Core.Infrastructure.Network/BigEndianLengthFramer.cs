using System.Buffers;
using System.Buffers.Binary;

namespace Core.Infrastructure.Network;

/// <summary>
/// Packet framer using a 4-byte big-endian length prefix.
/// Wire format: [int32-BE (length = 4 + payload-length, excludes opcode)][opcode byte][payload bytes]
/// </summary>
public sealed class BigEndianLengthFramer : IPacketFramer
{
    private const int LengthSize = sizeof(int);
    private const int OpcodeSize = sizeof(byte);

    public bool TryExtractPacket(ref ReadOnlyMemory<byte> buffer, out ReadOnlyMemory<byte> packet)
    {
        packet = default;

        while (buffer.Length >= LengthSize)
        {
            var packetLength = BinaryPrimitives.ReadInt32BigEndian(buffer[..LengthSize].Span);

            if (packetLength == 0)
            {
                // Skip zero-length frames
                buffer = buffer[LengthSize..];
                continue;
            }

            if (packetLength < 0 || buffer.Length < OpcodeSize + packetLength)
                return false;

            packet = buffer.Slice(LengthSize, packetLength - LengthSize + OpcodeSize);
            buffer = buffer[(packetLength + OpcodeSize)..];
            return true;
        }

        return false;
    }

    public byte ExtractOpcode(ReadOnlySpan<byte> packet, out int payloadOffset)
    {
        payloadOffset = OpcodeSize;
        return packet[0];
    }

    public ReadOnlyMemory<byte> CreatePacket<T>(byte opcode, T payload, IPacketSerializer serializer)
    {
        var writer = new ArrayBufferWriter<byte>();

        // Reserve 4 bytes for length header
        var lengthSpan = writer.GetSpan(LengthSize);
        writer.Advance(LengthSize);

        // Write opcode
        writer.Write(new[] { opcode });

        // Serialize payload
        serializer.Serialize(writer, payload);

        // Write the length into the reserved header
        BinaryPrimitives.WriteInt32BigEndian(lengthSpan, writer.WrittenCount - OpcodeSize);

        return writer.WrittenMemory;
    }
}
