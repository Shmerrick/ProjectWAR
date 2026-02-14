using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Core.Infrastructure.Network;

/// <summary>
/// Manages the TCP transport for a single client connection.
/// Handles I/O loops, packet framing, and dispatches packets to handlers via the generated dispatcher.
/// Implements <see cref="IConnectionContext"/> so handlers can interact with the connection.
/// </summary>
internal sealed class ClientConnection : IConnectionContext, IDisposable
{
    private readonly TcpClient _tcpClient;
    private readonly NetworkStream _stream;
    private readonly byte[] _receiveBuffer;
    private readonly Channel<PacketEnvelope> _receiveQueue;
    private readonly Channel<ReadOnlyMemory<byte>> _sendQueue;
    private readonly IPacketFramer _framer;
    private readonly IPacketSerializer _serializer;
    private readonly IPacketDispatcher _dispatcher;
    private readonly IServiceScope _connectionScope;
    private readonly ILogger<ClientConnection> _logger;
    private readonly IByteTransformer? _byteTransformer;
    private readonly int _errorThreshold;
    private readonly ConcurrentDictionary<string, object> _items = new();

    private Task? _receiveTask;
    private Task? _processTask;
    private Task? _sendTask;
    private CancellationTokenSource? _clientCancellation;
    private int _errorCount;
    private bool _disposed;

    /// <summary>
    /// Raised when the connection disconnects.
    /// </summary>
    public event Action<DisconnectReason>? Disconnected;

    /// <summary>
    /// Gets the current number of handler errors.
    /// </summary>
    public int ErrorCount => _errorCount;

    // IConnectionContext
    public string? RemoteAddress
    {
        get
        {
            try { return ((IPEndPoint?)_tcpClient.Client.RemoteEndPoint)?.ToString(); }
            catch { return null; }
        }
    }

    public IDictionary<string, object> Items => _items;

    public ClientConnection(
        TcpClient tcpClient,
        IPacketFramer framer,
        IPacketSerializer serializer,
        IPacketDispatcher dispatcher,
        IServiceScope connectionScope,
        ILogger<ClientConnection> logger,
        IByteTransformer? byteTransformer = null,
        int receiveBufferSize = 65536,
        int errorThreshold = 3)
    {
        _tcpClient = tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));
        _stream = tcpClient.GetStream();
        _framer = framer ?? throw new ArgumentNullException(nameof(framer));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _connectionScope = connectionScope ?? throw new ArgumentNullException(nameof(connectionScope));
        _logger = logger;
        _byteTransformer = byteTransformer;
        _receiveBuffer = new byte[receiveBufferSize];
        _receiveQueue = Channel.CreateUnbounded<PacketEnvelope>();
        _sendQueue = Channel.CreateUnbounded<ReadOnlyMemory<byte>>();
        _errorThreshold = errorThreshold;
    }

    /// <summary>
    /// Starts the receive, process, and send loops.
    /// </summary>
    public void Start(CancellationToken cancellationToken)
    {
        _clientCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _receiveTask = ReceiveLoopAsync(_clientCancellation.Token);
        _processTask = ProcessLoopAsync(_clientCancellation.Token);
        _sendTask = SendLoopAsync(_clientCancellation.Token);
    }

    public void SendResponse<T>(byte opcode, T response)
    {
        var packet = _framer.CreatePacket(opcode, response, _serializer);
        _sendQueue.Writer.TryWrite(packet);
    }

    public void Disconnect(DisconnectReason reason)
    {
        if (_disposed) return;
        Disconnected?.Invoke(reason);
        Dispose();
    }

    public void OnDispatchError(byte opcode, Exception exception)
    {
        _logger.LogError(exception, "Handler error for opcode 0x{Opcode:X2}", opcode);
        var errors = Interlocked.Increment(ref _errorCount);
        if (errors >= _errorThreshold)
            Disconnect(DisconnectReason.TooManyErrors);
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            var bufferOffset = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                var bytesRead = await _stream.ReadAsync(
                        _receiveBuffer.AsMemory(bufferOffset, _receiveBuffer.Length - bufferOffset),
                        cancellationToken)
                    .ConfigureAwait(false);

                if (bytesRead == 0)
                {
                    Disconnect(DisconnectReason.ClientDisconnected);
                    return;
                }

                bufferOffset += bytesRead;

                // Apply byte transformation if configured
                if (_byteTransformer != null)
                {
                    var dataSpan = new ReadOnlySpan<byte>(_receiveBuffer, 0, bufferOffset);
                    var transformBuffer = ArrayPool<byte>.Shared.Rent(bufferOffset * 2);
                    try
                    {
                        var transformedLength = _byteTransformer.Transform(dataSpan, transformBuffer);

                        // Validate transformed output length before copying back into the receive buffer
                        if (transformedLength < 0 ||
                            transformedLength > _receiveBuffer.Length ||
                            transformedLength > transformBuffer.Length)
                        {
                            Disconnect(DisconnectReason.BufferOverrun);
                            return;
                        }
                        Buffer.BlockCopy(transformBuffer, 0, _receiveBuffer, 0, transformedLength);
                        bufferOffset = transformedLength;
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(transformBuffer);
                    }
                }

                // Extract and queue packets
                bufferOffset = await ExtractAndQueuePacketsAsync(bufferOffset, cancellationToken)
                    .ConfigureAwait(false);

                if (bufferOffset >= _receiveBuffer.Length)
                {
                    Disconnect(DisconnectReason.BufferOverrun);
                    return;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown via cancellation token - exit cleanly
            Disconnect(DisconnectReason.ServerShutdown);
        }
        catch (SocketException)
        {
            Disconnect(DisconnectReason.SocketError);
        }
        catch (Exception)
        {
            Disconnect(DisconnectReason.SocketError);
        }
    }

    private async Task<int> ExtractAndQueuePacketsAsync(int bufferLength, CancellationToken cancellationToken)
    {
        var buffer = new ReadOnlyMemory<byte>(_receiveBuffer, 0, bufferLength);
        _logger.LogDebug("Buffer hex: {Buffer}", Convert.ToHexString(buffer.Span));

        while (_framer.TryExtractPacket(ref buffer, out var packetData))
        {
            var opcode = _framer.ExtractOpcode(packetData.Span, out var payloadOffset);
            var payloadSlice = packetData[payloadOffset..];
            _logger.LogInformation("Received packet with opcode 0x{Opcode:X2} and payload size {PayloadLength} bytes", opcode, payloadSlice.Length);

            // Copy payload — the slice points into _receiveBuffer which may be overwritten
            var payloadCopy = payloadSlice.ToArray();

            await _receiveQueue.Writer.WriteAsync(new PacketEnvelope(opcode, payloadCopy), cancellationToken)
                .ConfigureAwait(false);
        }

        // Compact buffer
        var remaining = buffer.Length;
        var totalConsumed = bufferLength - remaining;
        if (remaining > 0 && totalConsumed > 0)
        {
            Buffer.BlockCopy(_receiveBuffer, totalConsumed, _receiveBuffer, 0, remaining);
        }

        return remaining;
    }

    private async Task ProcessLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var envelope in _receiveQueue.Reader.ReadAllAsync(cancellationToken))
            {
                try
                {
                    _dispatcher.Dispatch(
                        envelope.Opcode,
                        envelope.Payload,
                        _connectionScope.ServiceProvider,
                        _serializer,
                        this);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogError(ex, "Deserialization error for opcode 0x{Opcode:X2}", envelope.Opcode);
                    Disconnect(DisconnectReason.MalformedPacket);
                    return;
                }
                catch (Exception ex)
                {
                    OnDispatchError(envelope.Opcode, ex);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Cancellation requested
        }
    }

    private async Task SendLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var data in _sendQueue.Reader.ReadAllAsync(cancellationToken))
            {
                _logger.LogDebug("Sending packet hex: {SendBuffer}", Convert.ToHexString(data.Span));
                try
                {
                    await _stream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
                    await _stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (IOException)
                {
                    Disconnect(DisconnectReason.SocketError);
                    return;
                }
                catch (SocketException)
                {
                    Disconnect(DisconnectReason.SocketError);
                    return;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Cancellation requested
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _receiveQueue.Writer.TryComplete();
        _sendQueue.Writer.TryComplete();
        _clientCancellation?.Cancel();

        try
        {
            Task.WaitAll(
                new[] { _receiveTask!, _processTask!, _sendTask! }.Where(t => t != null).ToArray(),
                TimeSpan.FromSeconds(5));
        }
        catch { /* Ignore wait errors */ }

        try { _stream?.Close(); } catch { }
        try { _tcpClient?.Close(); } catch { }

        _clientCancellation?.Dispose();

        try { _connectionScope.Dispose(); } catch { }
    }
}
