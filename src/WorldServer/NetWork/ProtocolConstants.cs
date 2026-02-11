using System;

namespace WorldServer.NetWork
{
    /// <summary>
    /// Constants for low-level protocol values exchanged with the client.
    /// </summary>
    public static class ProtocolConstants
    {
        /// <summary>
        /// Header value for <see cref="Opcodes.F_WORLD_ENTER"/> packets.
        /// This value identifies the world entry handshake according to the
        /// official client/server protocol specification.
        /// </summary>
        public const ushort WORLD_ENTER_HEADER = 0x0608;
    }
}
