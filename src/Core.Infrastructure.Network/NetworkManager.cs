using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;

namespace Core.Infrastructure.Network;

/// <summary>
/// Manages TCP server endpoints and client connections.
/// Handles connection acceptance, DI scope creation, and client lifecycle management.
/// </summary>
public sealed class NetworkManager : IHostedService, IDisposable
{
    private readonly ClientConnectionFactory _connectionFactory;
    private readonly ConcurrentDictionary<int, ClientConnection> _connections = new();
    private readonly TcpListener _listener;
    private bool _isStarted;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _acceptTask;
    private int _nextConnectionId;
    private bool _disposed;

    /// <summary>
    /// Gets or sets the maximum number of concurrent connections.
    /// </summary>
    public int MaxConnections { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the receive buffer size for each connection.
    /// </summary>
    public int ReceiveBufferSize { get; set; } = 65536;

    /// <summary>
    /// Gets or sets the error threshold before a connection is disconnected.
    /// </summary>
    public int ErrorThreshold { get; set; } = 3;

    /// <summary>
    /// Gets the current number of connected clients.
    /// </summary>
    public int ClientCount => _connections.Count;

    /// <summary>
    /// Raised when a new client connects.
    /// </summary>
    public event Action<IConnectionContext>? ClientConnected;

    /// <summary>
    /// Raised when a client disconnects.
    /// </summary>
    public event Action<IConnectionContext, DisconnectReason>? ClientDisconnected;

    internal NetworkManager(IPEndPoint endpoint, ClientConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
        _listener = new TcpListener(endpoint);
    }

    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var tcpClient = await _listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);

                if (_connections.Count >= MaxConnections)
                {
                    try { tcpClient.Client.Shutdown(SocketShutdown.Both); tcpClient.Close(); }
                    catch { /* Ignore close errors */ }
                    continue;
                }

                var connection = _connectionFactory.Create(tcpClient, ReceiveBufferSize, ErrorThreshold);

                var connectionId = Interlocked.Increment(ref _nextConnectionId);

                if (_connections.TryAdd(connectionId, connection))
                {
                    connection.Disconnected += reason => OnConnectionDisconnected(connectionId, connection, reason);
                    connection.Start(cancellationToken);
                    ClientConnected?.Invoke(connection);
                }
                else
                {
                    connection.Dispose();
                }
            }
            catch (SocketException)
            {
                if (!cancellationToken.IsCancellationRequested)
                    break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
        }
    }

    private void OnConnectionDisconnected(int connectionId, ClientConnection connection, DisconnectReason reason)
    {
        if (_connections.TryRemove(connectionId, out _))
        {
            ClientDisconnected?.Invoke(connection, reason);
        }
    }

    /// <summary>
    /// Disconnects a specific connection.
    /// </summary>
    public void DisconnectClient(IConnectionContext connection)
    {
        if (connection == null) throw new ArgumentNullException(nameof(connection));
        connection.Disconnect(DisconnectReason.ServerShutdown);
    }

    /// <summary>
    /// Stops the server and disconnects all clients.
    /// </summary>
    public void Stop()
    {
        if (_cancellationTokenSource == null)
            return;

        _cancellationTokenSource.Cancel();

        try { _listener.Stop(); } catch { }
        try { _acceptTask?.Wait(TimeSpan.FromSeconds(5)); } catch { }

        Parallel.ForEach(_connections.Values, connection =>
        {
            try { connection.Dispose(); } catch { }
        });

        _connections.Clear();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
        _cancellationTokenSource?.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_isStarted)
            throw new InvalidOperationException("NetworkManager is already started.");

        _cancellationTokenSource = new CancellationTokenSource();
        _listener.Start(100);
        _acceptTask = AcceptLoopAsync(_cancellationTokenSource.Token);
        _isStarted = true;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Stop();
        return Task.CompletedTask;
    }
}
