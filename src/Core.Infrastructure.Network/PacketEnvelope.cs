namespace Core.Infrastructure.Network
{
    /// <summary>
    /// Internal struct containing a packet's opcode and payload for queuing.
    /// </summary>
    public readonly struct PacketEnvelope
    {
        /// <summary>
        /// Gets the packet opcode.
        /// </summary>
        public byte Opcode { get; }

        /// <summary>
        /// Gets the packet payload (opcode already extracted).
        /// </summary>
        public ReadOnlyMemory<byte> Payload { get; }

        /// <summary>
        /// Creates a new packet envelope.
        /// </summary>
        /// <param name="opcode">The packet opcode.</param>
        /// <param name="payload">The packet payload.</param>
        public PacketEnvelope(byte opcode, ReadOnlyMemory<byte> payload)
        {
            Opcode = opcode;
            Payload = payload;
        }
    }
}
