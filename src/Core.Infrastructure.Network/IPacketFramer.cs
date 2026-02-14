namespace Core.Infrastructure.Network;

/// <summary>
/// Defines the wire-level packet framing protocol (length headers, opcode extraction, packet creation).
/// Each protocol (varint-prefixed, big-endian int32, etc.) implements this interface.
/// Implementations should be stateless and thread-safe.
/// </summary>
public interface IPacketFramer
{
    /// <summary>
    /// Attempts to extract a complete packet from the receive buffer.
    /// On success, <paramref name="buffer"/> is advanced past the consumed data.
    /// </summary>
    /// <param name="buffer">The accumulated receive buffer (advanced on success).</param>
    /// <param name="packet">The extracted packet bytes, if successful.</param>
    /// <returns>True if a packet was extracted; false if more data is needed.</returns>
    bool TryExtractPacket(ref ReadOnlyMemory<byte> buffer, out ReadOnlyMemory<byte> packet);

    /// <summary>
    /// Extracts the opcode from a complete packet.
    /// </summary>
    /// <param name="packet">The complete packet bytes.</param>
    /// <param name="payloadOffset">The offset where the payload begins after the opcode.</param>
    /// <returns>The packet opcode.</returns>
    byte ExtractOpcode(ReadOnlySpan<byte> packet, out int payloadOffset);

    /// <summary>
    /// Creates a wire-format packet containing the opcode and serialized payload.
    /// </summary>
    /// <typeparam name="T">The payload type.</typeparam>
    /// <param name="opcode">The packet opcode.</param>
    /// <param name="payload">The payload object to serialize.</param>
    /// <param name="serializer">The serializer to encode the payload.</param>
    /// <returns>The complete packet bytes ready for transmission.</returns>
    ReadOnlyMemory<byte> CreatePacket<T>(byte opcode, T payload, IPacketSerializer serializer);
}
