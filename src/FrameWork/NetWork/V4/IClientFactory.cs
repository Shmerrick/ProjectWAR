using System.Net.Sockets;

namespace FrameWork.NetWork.V4;

public interface IClientFactory<out TClient>  where TClient : Client
{
    TClient Create(TcpClient tcpClient);
}