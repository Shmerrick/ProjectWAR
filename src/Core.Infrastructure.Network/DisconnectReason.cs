namespace Core.Infrastructure.Network
{
    /// <summary>
    /// Specifies the reason a client was disconnected.
    /// </summary>
    public enum DisconnectReason
    {
        /// <summary>Client disconnected gracefully.</summary>
        ClientDisconnected,
        
        /// <summary>Server is shutting down.</summary>
        ServerShutdown,
        
        /// <summary>Socket error occurred.</summary>
        SocketError,
        
        /// <summary>Malformed or invalid packet received.</summary>
        MalformedPacket,
        
        /// <summary>Too many handler errors occurred.</summary>
        TooManyErrors,
        
        /// <summary>Receive buffer overrun (potential DoS).</summary>
        BufferOverrun,
        
        /// <summary>Connection limit reached.</summary>
        ConnectionLimitReached
    }
}
