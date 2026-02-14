using System.Buffers;

namespace Core.Infrastructure.Network
{
    /// <summary>
    /// Provides serialization and deserialization of packet payloads.
    /// Implementations must be thread-safe if shared across multiple clients.
    /// </summary>
    public interface IPacketSerializer
    {
        /// <summary>
        /// Deserializes a packet payload into a strongly-typed request object.
        /// </summary>
        /// <typeparam name="T">The type of the request object.</typeparam>
        /// <param name="payload">The packet payload bytes (opcode already extracted).</param>
        /// <returns>The deserialized request object.</returns>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        T Deserialize<T>(ReadOnlySpan<byte> payload);

        /// <summary>
        /// Serializes a response object into a buffer writer.
        /// </summary>
        /// <typeparam name="T">The type of the response object.</typeparam>
        /// <param name="writer">The buffer writer to write serialized bytes to.</param>
        /// <param name="message">The response object to serialize.</param>
        void Serialize<T>(IBufferWriter<byte> writer, T message);
    }
}
