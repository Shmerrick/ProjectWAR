namespace Core.Infrastructure.Network;

/// <summary>
/// Marker interface for server-side RPC packet handlers.
/// Implement this interface and add methods decorated with <see cref="RpcAttribute"/> to handle incoming packets.
/// Dependencies can be injected via the constructor (connection-scoped) or
/// via <see cref="FromServicesAttribute"/> on method parameters (packet-scoped).
/// </summary>
public interface IPacketHandler
{
}
