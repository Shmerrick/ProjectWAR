using System;
using System.Buffers;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace FrameWork.NetWork.V4;

/// <summary>
/// Abstract base class for TCP clients.
/// Manages socket I/O, packet framing, and RPC dispatch.
/// </summary>
public abstract class Client : IDisposable
{
    private readonly TcpClient _tcpClient;
    private readonly NetworkStream _stream;
    private readonly byte[] _receiveBuffer;
    private readonly Channel<PacketEnvelope> _receiveQueue;
    private readonly Channel<ReadOnlyMemory<byte>> _sendQueue;
    private readonly IByteTransformer _byteTransformer;
    private readonly int _errorThreshold;
    private Task _receiveTask;
    private Task _processTask;
    private Task _sendTask;
    private CancellationTokenSource _clientCancellation;
    private int _errorCount;
    private bool _disposed;

    /// <summary>
    /// Gets the packet serializer for this client.
    /// </summary>
    protected IPacketSerializer Serializer { get; }

    /// <summary>
    /// Gets the current number of handler errors.
    /// </summary>
    public int ErrorCount => _errorCount;

    /// <summary>
    /// Raised when the client disconnects.
    /// </summary>
    internal event Action<DisconnectReason> Disconnected;

    /// <summary>
    /// Creates a new client instance.
    /// </summary>
    /// <param name="tcpClient">The connected TcpClient.</param>
    /// <param name="serializerFactory">Factory to create the packet serializer.</param>
    /// <param name="byteTransformer">Optional byte transformer for encryption/decryption.</param>
    /// <param name="receiveBufferSize">Size of the receive ring buffer in bytes.</param>
    /// <param name="errorThreshold">Number of handler errors before disconnection.</param>
    protected Client(
        TcpClient tcpClient,
        IPacketSerializerFactory serializerFactory,
        IByteTransformer byteTransformer = null,
        int receiveBufferSize = 65536,
        int errorThreshold = 3)
    {
        _tcpClient = tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));
        if (serializerFactory == null)
            throw new ArgumentNullException(nameof(serializerFactory));

        _stream = _tcpClient.GetStream();
        Serializer = serializerFactory.Create();
        _byteTransformer = byteTransformer;
        _receiveBuffer = new byte[receiveBufferSize];
        _receiveQueue = Channel.CreateUnbounded<PacketEnvelope>();
        _sendQueue = Channel.CreateUnbounded<ReadOnlyMemory<byte>>();
        _errorThreshold = errorThreshold;
    }

    /// <summary>
    /// Starts the client's receive, process, and send loops.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for graceful shutdown.</param>
    internal void Start(CancellationToken cancellationToken)
    {
        _clientCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _receiveTask = ReceiveLoopAsync(_clientCancellation.Token);
        _processTask = ProcessLoopAsync(_clientCancellation.Token);
        _sendTask = SendLoopAsync(_clientCancellation.Token);
    }

    /// <summary>
    /// Attempts to extract a complete packet from the receive buffer.
    /// </summary>
    /// <param name="buffer">The accumulated receive buffer.</param>
    /// <param name="packet">The extracted packet, if successful.</param>
    /// <returns>True if a packet was extracted; false if more data is needed.</returns>
    protected abstract bool TryExtractPacket(ref ReadOnlyMemory<byte> buffer, out ReadOnlyMemory<byte> packet);

    /// <summary>
    /// Extracts the opcode from a packet and returns the payload offset.
    /// </summary>
    /// <param name="packet">The complete packet bytes.</param>
    /// <param name="payloadOffset">The offset where the payload begins.</param>
    /// <returns>The packet opcode.</returns>
    protected abstract byte ExtractOpcode(ReadOnlySpan<byte> packet, out int payloadOffset);
    
    protected abstract ReadOnlyMemory<byte> CreatePacket<T>(byte opcode, T payload);

    public string? GetRemoteAddress()
    {
        return ((IPEndPoint)_tcpClient.Client.RemoteEndPoint)?.ToString();
    }

    /// <summary>
    /// Processes a packet with the given opcode and payload.
    /// This method is overridden by the source generator in derived classes.
    /// </summary>
    /// <param name="opcode">The packet opcode.</param>
    /// <param name="payload">The packet payload.</param>
    protected virtual void ProcessPacket(byte opcode, ReadOnlySpan<byte> payload)
    {
        OnUnknownOpcode(opcode);
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            int bufferOffset = 0; // Tracks valid data in _receiveBuffer

            while (!cancellationToken.IsCancellationRequested)
            {
                // Read from network stream into buffer after existing data
                int bytesRead = await _stream.ReadAsync(_receiveBuffer, bufferOffset, _receiveBuffer.Length - bufferOffset, cancellationToken)
                    .ConfigureAwait(false);

                if (bytesRead == 0)
                {
                    // Client disconnected gracefully
                    Disconnect(DisconnectReason.ClientDisconnected);
                    return;
                }

                // Update buffer tracking
                bufferOffset += bytesRead;

                // Apply byte transformation if configured
                ReadOnlySpan<byte> dataSpan = new ReadOnlySpan<byte>(_receiveBuffer, 0, bufferOffset);
                if (_byteTransformer != null)
                {
                    byte[] transformBuffer = ArrayPool<byte>.Shared.Rent(bufferOffset * 2);
                    try
                    {
                        int transformedLength = _byteTransformer.Transform(dataSpan, transformBuffer);
                        // Copy transformed data back to receive buffer
                        Buffer.BlockCopy(transformBuffer, 0, _receiveBuffer, 0, transformedLength);
                        bufferOffset = transformedLength;
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(transformBuffer);
                    }
                }

                // Extract and queue packets from buffer
                bufferOffset = await ExtractAndQueuePacketsAsync(bufferOffset, cancellationToken).ConfigureAwait(false);

                // Check for buffer overrun
                if (bufferOffset >= _receiveBuffer.Length)
                {
                    Disconnect(DisconnectReason.BufferOverrun);
                    return;
                }
            }
        }
        catch (SocketException)
        {
            Disconnect(DisconnectReason.SocketError);
        }
        catch (Exception)
        {
            Disconnect(DisconnectReason.SocketError);
        }
        finally
        {
            // _receiveQueue.Writer.Complete();
        }
    }

    private async Task<int> ExtractAndQueuePacketsAsync(int bufferLength, CancellationToken cancellationToken)
    {
        var buffer = new ReadOnlyMemory<byte>(_receiveBuffer, 0, bufferLength);

        while (TryExtractPacket(ref buffer, out var packetData))
        {
            // int bytesConsumed = packetData.Length;
            // totalBytesConsumed += bytesConsumed;

            // Extract opcode and payload
            byte opcode = ExtractOpcode(packetData.Span, out var payloadOffset);
            ReadOnlyMemory<byte> payload = packetData.Slice(payloadOffset);

            // Queue packet for processing
            await _receiveQueue.Writer.WriteAsync(new PacketEnvelope(opcode, payload), cancellationToken)
                .ConfigureAwait(false);
        }

        // Compact buffer: move remaining data to start
        var remaining = (int)buffer.Length;
        var totalBytesConsumed = bufferLength - remaining;
        if (remaining > 0 && totalBytesConsumed > 0)
        {
            Buffer.BlockCopy(_receiveBuffer, totalBytesConsumed, _receiveBuffer, 0, remaining);
        }

        return remaining;
    }

    private async Task ProcessLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (PacketEnvelope envelope in _receiveQueue.Reader.ReadAllAsync(cancellationToken))
            {
                try
                {
                    ProcessPacket(envelope.Opcode, envelope.Payload.Span);
                }
                catch (InvalidOperationException ex)
                {
                    // Deserialization error - malformed packet
                    OnDeserializationError(envelope.Opcode, ex);
                    Disconnect(DisconnectReason.MalformedPacket);
                    return;
                }
                catch (Exception ex)
                {
                    // Handler error
                    OnHandlerError(envelope.Opcode, ex);

                    int errors = Interlocked.Increment(ref _errorCount);
                    if (errors >= _errorThreshold)
                    {
                        Disconnect(DisconnectReason.TooManyErrors);
                        return;
                    }
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
            await foreach (ReadOnlyMemory<byte> data in _sendQueue.Reader.ReadAllAsync(cancellationToken))
            {
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

    /// <summary>
    /// Sends a packet to the client.
    /// </summary>
    /// <param name="opcode">The packet opcode.</param>
    /// <param name="writer">Action to write packet payload.</param>
    protected void SendPacket(byte opcode, Action<IBufferWriter<byte>> writer)
    {
        var bufferWriter = new ArrayBufferWriter<byte>();
        bufferWriter.Write(new[] { opcode });
        writer?.Invoke(bufferWriter);
            
        if (!_sendQueue.Writer.TryWrite(bufferWriter.WrittenMemory))
        {
            // Send queue closed, ignore
        }
    }

    /// <summary>
    /// Sends a serialized response packet.
    /// </summary>
    /// <typeparam name="T">The response type.</typeparam>
    /// <param name="opcode">The response opcode.</param>
    /// <param name="response">The response object.</param>
    protected void SendResponse<T>(byte opcode, T response)
    {
        var packet = CreatePacket(opcode, response);
            
        if (!_sendQueue.Writer.TryWrite(packet))
        {
            // Send queue closed, ignore
        }
    }

    /// <summary>
    /// Called when a deserialization error occurs.
    /// </summary>
    /// <param name="opcode">The opcode that failed to deserialize.</param>
    /// <param name="exception">The exception that occurred.</param>
    protected virtual void OnDeserializationError(byte opcode, Exception exception)
    {
        Console.WriteLine($"[Client] Deserialization error for opcode 0x{opcode:X2}: {exception.Message}");
    }

    /// <summary>
    /// Called when a handler error occurs.
    /// </summary>
    /// <param name="opcode">The opcode whose handler threw an exception.</param>
    /// <param name="exception">The exception that occurred.</param>
    protected virtual void OnHandlerError(byte opcode, Exception exception)
    {
        Console.WriteLine($"[Client] Handler error for opcode 0x{opcode:X2}: {exception}");
    }

    /// <summary>
    /// Called when an unknown opcode is received.
    /// </summary>
    /// <param name="opcode">The unknown opcode.</param>
    protected virtual void OnUnknownOpcode(byte opcode)
    {
        Console.WriteLine($"[Client] Unknown opcode received: 0x{opcode:X2}");
    }

    /// <summary>
    /// Disconnects the client with the specified reason.
    /// </summary>
    /// <param name="reason">The reason for disconnection.</param>
    protected void Disconnect(DisconnectReason reason)
    {
        if (_disposed)
            return;

        Disconnected?.Invoke(reason);
        Dispose();
    }

    /// <summary>
    /// Disposes the client and releases all resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        // Complete channels
        _receiveQueue.Writer.TryComplete();
        _sendQueue.Writer.TryComplete();

        // Cancel operations
        _clientCancellation?.Cancel();

        // Wait for tasks with timeout
        try
        {
            Task.WaitAll(new[] { _receiveTask, _processTask, _sendTask }, TimeSpan.FromSeconds(5));
        }
        catch { /* Ignore wait errors */ }

        // Close connection
        try
        {
            _stream?.Close();
        }
        catch { /* Ignore close errors */ }

        try
        {
            _tcpClient?.Close();
        }
        catch { /* Ignore close errors */ }

        // Dispose resources
        _clientCancellation?.Dispose();
    }
}