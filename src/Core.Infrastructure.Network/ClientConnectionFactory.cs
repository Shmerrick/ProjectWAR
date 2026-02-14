using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Core.Infrastructure.Network;

internal class ClientConnectionFactory
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IPacketFramer _framer;
    private readonly IPacketSerializerFactory _serializerFactory;
    private readonly IPacketDispatcher _dispatcher;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IByteTransformer? _byteTransformer;
    
    public ClientConnectionFactory(
        IServiceScopeFactory scopeFactory,
        IPacketFramer framer,
        IPacketSerializerFactory serializerFactory,
        IPacketDispatcher dispatcher,
        ILoggerFactory loggerFactory,
        IByteTransformer? byteTransformer = null)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _framer = framer ?? throw new ArgumentNullException(nameof(framer));
        _serializerFactory = serializerFactory ?? throw new ArgumentNullException(nameof(serializerFactory));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _loggerFactory = loggerFactory;
        _byteTransformer = byteTransformer;
    }
    
    public ClientConnection Create(TcpClient tcpClient, int receiveBufferSize, int errorThreshold)
    {
        // Create a connection-scoped DI scope
        var connectionScope = _scopeFactory.CreateScope();

        // Create a per-connection serializer instance
        var serializer = _serializerFactory.Create();

        // Create the connection (owns the scope lifetime)
        var connection = new ClientConnection(
            tcpClient, _framer, serializer, _dispatcher,
            connectionScope, _loggerFactory.CreateLogger<ClientConnection>(), _byteTransformer,
            receiveBufferSize, errorThreshold);

        return connection;
    }
}