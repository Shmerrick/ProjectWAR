namespace Core.Infrastructure.Network
{
    /// <summary>
    /// Factory for creating packet serializer instances.
    /// Allows per-client serializer instances or cached/pooled instances for thread-safety.
    /// </summary>
    public interface IPacketSerializerFactory
    {
        /// <summary>
        /// Creates or retrieves a packet serializer instance.
        /// </summary>
        /// <returns>An IPacketSerializer instance.</returns>
        IPacketSerializer Create();
    }
}
