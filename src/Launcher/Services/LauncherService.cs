using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Core.Infrastructure.Network;
using Launcher.NetWork;
using NLog;

namespace Launcher.Services;

/// <summary>
/// Manages the launcher client connection lifecycle with lazy initialization,
/// thread-safe connection management, and centralized error handling.
/// Exposes the strongly-typed LauncherProxy API.
/// </summary>
public class LauncherService : IDisposable
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    private readonly string _host;
    private readonly int _port;
    private readonly IPacketSerializerFactory _serializerFactory;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    
    private LauncherProxy _proxy;
    private bool _disposed;

    /// <summary>
    /// Raised when the connection is lost.
    /// </summary>
    public event Action<DisconnectReason> Disconnected;

    /// <summary>
    /// Gets whether the client is currently connected.
    /// </summary>
    public bool IsConnected => _proxy != null && !_disposed;

    public LauncherService(string host, int port, IPacketSerializerFactory serializerFactory)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _port = port;
        _serializerFactory = serializerFactory ?? throw new ArgumentNullException(nameof(serializerFactory));
    }

    /// <summary>
    /// Ensures a connection exists and returns the LauncherProxy for direct API access.
    /// This method is thread-safe and will only establish one connection even if called concurrently.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Connected LauncherProxy instance</returns>
    /// <exception cref="InvalidOperationException">If connection cannot be established</exception>
    public async Task<LauncherProxy> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        // Fast path: already connected
        if (IsConnected)
            return _proxy;

        await _connectionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Double-check after acquiring lock
            if (IsConnected)
                return _proxy;

            Logger.Info($"Establishing connection to {_host}:{_port}");

            var tcpClient = new TcpClient();
            try
            {
                await tcpClient.ConnectAsync(_host, _port, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to connect to {_host}:{_port}");
                tcpClient.Dispose();
                throw new InvalidOperationException($"Could not connect to launcher server at {_host}:{_port}", ex);
            }

            _proxy = new LauncherProxy(tcpClient, _serializerFactory);
            _proxy.Disconnected += OnProxyDisconnected;
            _proxy.Start(cancellationToken);

            Logger.Info($"Successfully connected to {_host}:{_port}");
            return _proxy;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <summary>
    /// Executes an operation using the LauncherProxy, ensuring connection first.
    /// Provides centralized error handling and automatic connection management.
    /// </summary>
    /// <typeparam name="TResult">The return type of the operation</typeparam>
    /// <param name="operation">The operation to execute with the proxy</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<TResult> ExecuteAsync<TResult>(
        Func<LauncherProxy, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));

        var proxy = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        return await operation(proxy).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an operation using the LauncherProxy, ensuring connection first.
    /// Provides centralized error handling and automatic connection management.
    /// </summary>
    /// <param name="operation">The operation to execute with the proxy</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task ExecuteAsync(
        Func<LauncherProxy, Task> operation,
        CancellationToken cancellationToken = default)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));

        var proxy = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await operation(proxy).ConfigureAwait(false);
    }

    /// <summary>
    /// Disconnects the client if connected.
    /// </summary>
    public void Disconnect()
    {
        if (_proxy != null)
        {
            Logger.Info("Disconnecting from launcher server");
            _proxy.Disconnected -= OnProxyDisconnected;
            _proxy.Dispose();
            _proxy = null;
        }
    }

    private void OnProxyDisconnected(DisconnectReason reason)
    {
        Logger.Warn($"Disconnected from launcher server: {reason}");
        
        // Clean up the proxy reference
        if (_proxy != null)
        {
            _proxy.Disconnected -= OnProxyDisconnected;
            _proxy = null;
        }

        // Notify subscribers
        Disconnected?.Invoke(reason);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        
        Disconnect();
        _connectionLock?.Dispose();
    }
}
