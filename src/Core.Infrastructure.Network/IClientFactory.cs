using System.Net.Sockets;

namespace Core.Infrastructure.Network;

public interface IClientFactory<out TClient>  where TClient : Client
{
    TClient Create(TcpClient tcpClient);
}