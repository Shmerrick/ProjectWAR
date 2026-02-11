using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace FrameWork.NetWork.V4
{
    /// <summary>
    /// Manages TCP server endpoints and client connections.
    /// Handles connection acceptance and client lifecycle management.
    /// </summary>
    public sealed class NetworkManager<TClient> : IHostedService, IDisposable where TClient : Client
    {
        private readonly IClientFactory<TClient> _clientFactory;
        private readonly ConcurrentDictionary<int, Client> _clients = new();
        private readonly TcpListener _listenerClient;
        private bool _isStarted = false;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _acceptTask;
        private int _nextClientId;
        private int _maxConnections = 5000;
        private bool _disposed;

        /// <summary>
        /// Gets or sets the maximum number of concurrent connections.
        /// </summary>
        public int MaxConnections
        {
            get => _maxConnections;
            set => _maxConnections = value;
        }

        /// <summary>
        /// Gets the current number of connected clients.
        /// </summary>
        public int ClientCount => _clients.Count;

        /// <summary>
        /// Raised when a new client connects.
        /// </summary>
        public event Action<Client> ClientConnected;

        /// <summary>
        /// Raised when a client disconnects.
        /// </summary>
        public event Action<Client, DisconnectReason> ClientDisconnected;

        public NetworkManager(IPEndPoint endpoint, IClientFactory<TClient> clientFactory)
        {
            _listenerClient = new TcpListener(endpoint);
            _clientFactory = clientFactory;
        }

        private async Task AcceptLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var tcpClient = await _listenerClient.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);

                    // Check connection limit
                    if (_clients.Count >= _maxConnections)
                    {
                        try
                        {
                            tcpClient.Client.Shutdown(SocketShutdown.Both);
                            tcpClient.Close();
                        }
                        catch { /* Ignore close errors */ }
                        continue;
                    }

                    // Wrap socket in TcpClient and create client instance
                    var client = _clientFactory.Create(tcpClient);
                    var clientId = Interlocked.Increment(ref _nextClientId);
                    
                    if (_clients.TryAdd(clientId, client))
                    {
                        // Set up client disconnection handling
                        client.Disconnected += (reason) => OnClientDisconnected(clientId, client, reason);
                        
                        // Start the client
                        client.Start(cancellationToken);
                        
                        // Notify connection
                        ClientConnected?.Invoke(client);
                    }
                    else
                    {
                        // Failed to add to dictionary (shouldn't happen)
                        client.Dispose();
                    }
                }
                catch (SocketException)
                {
                    // Socket closed or error during accept
                    if (!cancellationToken.IsCancellationRequested)
                        break;
                }
                catch (ObjectDisposedException)
                {
                    // Socket was disposed
                    break;
                }
            }
        }

        private void OnClientDisconnected(int clientId, Client client, DisconnectReason reason)
        {
            if (_clients.TryRemove(clientId, out _))
            {
                ClientDisconnected?.Invoke(client, reason);
            }
        }

        /// <summary>
        /// Stops the server and disconnects all clients.
        /// </summary>
        public void Stop()
        {
            if (_cancellationTokenSource == null)
                return;

            // Signal cancellation
            _cancellationTokenSource.Cancel();

            // Close listener socket
            try
            {
                _listenerClient?.Stop();
            }
            catch { /* Ignore close errors */ }

            // Wait for accept loop to finish
            try
            {
                _acceptTask?.Wait(TimeSpan.FromSeconds(5));
            }
            catch { /* Ignore wait errors */ }

            // Disconnect all clients in parallel
            Parallel.ForEach(_clients.Values, client =>
            {
                try
                {
                    client.Dispose();
                }
                catch { /* Ignore disposal errors */ }
            });

            _clients.Clear();
        }

        /// <summary>
        /// Disconnects a specific client.
        /// </summary>
        /// <param name="client">The client to disconnect.</param>
        public void DisconnectClient(Client client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            client.Dispose();
        }

        /// <summary>
        /// Disposes the NetworkManager and releases all resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            Stop();
            _cancellationTokenSource?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_isStarted)
                throw new InvalidOperationException("NetworkManager is already started.");

            _cancellationTokenSource = new CancellationTokenSource();
            _listenerClient.Start(100);

            _acceptTask = AcceptLoopAsync(_cancellationTokenSource.Token);
            _isStarted = true;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }
    }
}
