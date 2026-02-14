namespace Core.Infrastructure.Network
{
    /// <summary>
    /// Specifies the number of bytes used to encode the length of a collection property in a packet.
    /// Valid values are 1, 2, or 4 bytes.
    /// If not specified, defaults to 1 byte.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class PacketLengthAttribute : Attribute
    {
        /// <summary>
        /// Gets the number of bytes used to encode the collection length.
        /// </summary>
        public int ByteCount { get; }

        /// <summary>
        /// Creates a new PacketLengthAttribute.
        /// </summary>
        /// <param name="byteCount">Number of bytes for the length field (1, 2, or 4).</param>
        /// <exception cref="ArgumentException">Thrown when byteCount is not 1, 2, or 4.</exception>
        public PacketLengthAttribute(int byteCount)
        {
            if (byteCount != 1 && byteCount != 2 && byteCount != 4)
                throw new ArgumentException("ByteCount must be 1, 2, or 4", nameof(byteCount));
            
            ByteCount = byteCount;
        }
    }
}
