using System.Buffers;

namespace Core.Infrastructure.Network;

/// <summary>
/// Packet framer using variable-length (varint) size prefix.
/// Wire format: [varint payload-size][opcode byte][payload bytes]
/// </summary>
public sealed class VarintLengthFramer : IPacketFramer
{
    private const int OpcodeSize = sizeof(byte);

    public bool TryExtractPacket(ref ReadOnlyMemory<byte> buffer, out ReadOnlyMemory<byte> packet)
    {
        packet = default;

        while (buffer.Length > 1)
        {
            // Snapshot so we can restore if there isn't enough data yet
            var saved = buffer;
            var packetLength = ReadVarint(ref buffer);

            if (packetLength == 0)
            {
                // Zero-length payload: extract just the opcode byte
                if (buffer.Length < OpcodeSize)
                {
                    buffer = saved;
                    return false;
                }

                packet = buffer[..OpcodeSize];
                buffer = buffer[OpcodeSize..];
                return true;
            }

            if (packetLength < 0 || buffer.Length < OpcodeSize + packetLength)
            {
                buffer = saved;
                return false;
            }

            packet = buffer[..(packetLength + OpcodeSize)];
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
        // Serialize payload to determine size
        var payloadWriter = new ArrayBufferWriter<byte>();
        serializer.Serialize(payloadWriter, payload);
        var payloadSize = payloadWriter.WrittenCount;

        var varintSize = ComputeVarintSize(payloadSize);
        var packet = new byte[varintSize + OpcodeSize + payloadSize];
        var span = packet.AsSpan();

        WriteVarint(span, payloadSize);
        span[varintSize] = opcode;
        payloadWriter.WrittenSpan.CopyTo(span.Slice(varintSize + OpcodeSize));

        return packet;
    }

    private static int ReadVarint(ref ReadOnlyMemory<byte> buffer)
    {
        var size = 0;
        var byteCount = 0;

        while (buffer.Length > 0)
        {
            var b = buffer.Span[0];
            buffer = buffer[1..];

            size |= (b & 0x7F) << (7 * byteCount);
            byteCount++;

            if ((b & 0x80) == 0)
                return size;
        }

        return 0;
    }

    private static int ComputeVarintSize(int value)
    {
        var size = 1;
        while (value > 0x7F) { value >>= 7; size++; }
        return size;
    }

    private static void WriteVarint(Span<byte> buffer, int size)
    {
        var index = 0;
        while (size > 0x7F)
        {
            buffer[index++] = (byte)(size | 0x80);
            size >>= 7;
        }
        buffer[index] = (byte)size;
    }
}
