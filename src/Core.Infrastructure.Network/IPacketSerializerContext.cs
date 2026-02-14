using System.Buffers;

namespace Core.Infrastructure.Network
{
    /// <summary>
    /// Interface for packet serializer contexts that provide optimized serialization
    /// </summary>
    public interface IPacketSerializerContext
    {
        /// <summary>
        /// Attempts to deserialize a packet using generated code
        /// </summary>
        /// <param name="type">The type to deserialize</param>
        /// <param name="buffer">The buffer containing the serialized data</param>
        /// <param name="result">The deserialized object if successful</param>
        /// <returns>True if the type was handled by this context, false otherwise</returns>
        bool TryDeserialize(Type type, ReadOnlySpan<byte> buffer, out object? result);

        /// <summary>
        /// Attempts to serialize a packet using generated code
        /// </summary>
        /// <param name="value">The value to serialize</param>
        /// <param name="writer">The buffer writer to write to</param>
        /// <returns>True if the type was handled by this context, false otherwise</returns>
        bool TrySerialize(object value, IBufferWriter<byte> writer);
    }
}
