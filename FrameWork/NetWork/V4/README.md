# V4 Networking System

A modern, high-performance TCP networking layer for C# game servers with source-generated RPC dispatch.

## Architecture Overview

The V4 networking system consists of several key components:

### Core Components

1. **NetworkManager** - Manages TCP server endpoints and client lifecycle
2. **Client** - Abstract base class for TCP clients with packet processing
3. **RpcSourceGenerator** - Compile-time code generator for RPC dispatch
4. **IPacketSerializer** - Interface for packet serialization/deserialization
5. **IByteTransformer** - Interface for encryption/decryption

### Key Features

- ✅ **Zero-reflection RPC dispatch** - Source generator creates switch statements at compile time
- ✅ **Async-capable handlers** - Support both sync and async (Task/ValueTask) handlers
- ✅ **Serial per-client processing** - Maintains packet order per client while allowing parallel processing across clients
- ✅ **Ring buffer receive** - Fixed-size buffer with overflow protection
- ✅ **Channel-based queuing** - Separate channels for receive and send with backpressure handling
- ✅ **Pluggable encryption** - Optional IByteTransformer for decryption/encryption
- ✅ **Minimal allocations** - Uses `ReadOnlySpan<byte>`, `IBufferWriter<byte>`, and `ArrayPool<byte>`
- ✅ **Compile-time opcode validation** - Detects duplicate opcodes and invalid handler signatures
- ✅ **Graceful error handling** - Disconnect on malformed packets, threshold-based error handling
- ✅ **Clean separation of concerns** - Client handles framing, NetworkManager handles connections

## Usage Example

### 1. Define Your Client

```csharp
public partial class GameClient : Client
{
    public GameClient(
        Socket socket,
        IPacketSerializerFactory serializerFactory,
        IByteTransformer byteTransformer = null)
        : base(socket, serializerFactory, byteTransformer)
    {
    }

    // Implement framing logic
    protected override bool TryExtractPacket(ref ReadOnlySequence<byte> buffer, out ReadOnlyMemory<byte> packet)
    {
        // Extract packets from TCP stream (size-prefixed, delimited, etc.)
    }

    // Implement opcode extraction
    protected override byte ExtractOpcode(ReadOnlySpan<byte> packet, out int payloadOffset)
    {
        payloadOffset = 1;
        return packet[0]; // First byte is opcode
    }

    // Define RPC handlers
    [Rpc(0x01)]
    public LoginResponse HandleLogin(LoginRequest request)
    {
        // Handle synchronous login
        return new LoginResponse { Success = true };
    }

    [Rpc(0x02)]
    public async ValueTask<InventoryResponse> HandleGetInventory(InventoryRequest request)
    {
        // Handle async inventory query
        var data = await _database.GetInventoryAsync(request.PlayerId);
        return new InventoryResponse { Items = data };
    }

    [Rpc(0x03)]
    public void HandleChat(ChatMessage message)
    {
        // Handle without response
        BroadcastToOthers(message);
    }
}
```

### 2. Implement Serializer

```csharp
public class GamePacketSerializer : IPacketSerializer
{
    public T Deserialize<T>(ReadOnlySpan<byte> payload)
    {
        // Deserialize payload into request object
    }

    public void Serialize<T>(IBufferWriter<byte> writer, T message)
    {
        // Serialize response object into buffer
    }
}

public class GamePacketSerializerFactory : IPacketSerializerFactory
{
    public IPacketSerializer Create() => new GamePacketSerializer();
}
```

### 3. Start the Server

```csharp
var manager = new NetworkManager();
manager.MaxConnections = 5000;

manager.ClientConnected += client => Console.WriteLine("Client connected");
manager.ClientDisconnected += (client, reason) => Console.WriteLine($"Client disconnected: {reason}");

var endpoint = new IPEndPoint(IPAddress.Any, 8080);
var serializerFactory = new GamePacketSerializerFactory();

manager.Start<GameClient>(endpoint, socket =>
{
    var transformer = new RC4Transformer(key); // Optional encryption
    return new GameClient(socket, serializerFactory, transformer);
});
```

## Source Generator

The `RpcSourceGenerator` automatically generates the `ProcessPacket` method at compile time:

### Generated Code Example

For the client above, it generates:

```csharp
partial class GameClient
{
    partial void ProcessPacket(byte opcode, ReadOnlySpan<byte> payload)
    {
        switch (opcode)
        {
            case 0x01:
            {
                var request = Serializer.Deserialize<LoginRequest>(payload);
                var response = HandleLogin(request);
                if (response != null)
                    SendResponse(0x01, response);
                break;
            }
            case 0x02:
            {
                var request = Serializer.Deserialize<InventoryRequest>(payload);
                _ = HandleAsync_HandleGetInventory(request);
                break;
            }
            case 0x03:
            {
                var request = Serializer.Deserialize<ChatMessage>(payload);
                HandleChat(request);
                break;
            }
            default:
                OnUnknownOpcode(opcode);
                break;
        }
    }

    private async Task HandleAsync_HandleGetInventory(InventoryRequest request)
    {
        try
        {
            var response = await HandleGetInventory(request);
            if (response != null)
                SendResponse(0x02, response);
        }
        catch (Exception ex)
        {
            OnHandlerError(0x02, ex);
        }
    }
}
```

## Packet Flow

1. **Receive** - `Client.ReceiveLoopAsync` reads from socket into ring buffer
2. **Transform** - Optional `IByteTransformer.Transform` decrypts/decompresses data
3. **Frame** - `TryExtractPacket` extracts complete packets from buffer
4. **Extract Opcode** - `ExtractOpcode` separates opcode from payload
5. **Queue** - Packet queued in `Channel<PacketEnvelope>`
6. **Process** - `ProcessLoopAsync` dequeues and dispatches via generated `ProcessPacket`
7. **Deserialize** - `IPacketSerializer.Deserialize` converts payload to request object
8. **Handle** - Generated code calls appropriate handler method
9. **Serialize** - `IPacketSerializer.Serialize` converts response to bytes
10. **Send** - Response queued in send channel, sent via `SendLoopAsync`

## Error Handling

- **Deserialization failure** → Immediate disconnect with `DisconnectReason.MalformedPacket`
- **Handler exception** → Log, increment error counter, disconnect at threshold (default 3)
- **Unknown opcode** → Log warning, continue processing
- **Buffer overrun** → Immediate disconnect with `DisconnectReason.BufferOverrun`
- **Socket error** → Disconnect with `DisconnectReason.SocketError`

## Configuration

```csharp
protected Client(
    Socket socket,
    IPacketSerializerFactory serializerFactory,
    IByteTransformer byteTransformer = null,
    int receiveBufferSize = 65536,        // Ring buffer size
    int errorThreshold = 3)                // Errors before disconnect
```

## Thread Safety

- **NetworkManager** - Thread-safe, uses `ConcurrentDictionary` for client tracking
- **Client** - Each client processes packets serially (maintains order)
- **Multiple clients** - Process packets in parallel across different clients
- **IPacketSerializer** - Must be thread-safe if shared, or use factory for per-client instances

## Performance Considerations

- Uses `ReadOnlySpan<byte>` for zero-copy packet access
- Uses `IBufferWriter<byte>` for efficient serialization
- Uses `ArrayPool<byte>` for temporary buffers in transformers
- Channel-based queues avoid lock contention
- Source-generated dispatch eliminates reflection overhead
- Ring buffer reduces allocations compared to stream-based approaches

## Migration from Legacy Code

The V4 system is in a separate namespace and does not interfere with existing `TCPManager` / `BaseClient` code. You can:

1. Implement new client types using V4
2. Run both systems side-by-side during migration
3. Test thoroughly before switching
4. Remove legacy code once migration is complete

## Examples

See [Examples](Examples/) folder for:
- `ExampleGameClient.cs` - Full client implementation with RPC handlers
- `ExamplePacketSerializer.cs` - Simple binary serialization
- `ExampleXorTransformer.cs` - Basic encryption example
- `ExampleUsage.cs` - Server startup code

## Diagnostics

The source generator provides compile-time diagnostics:

- **RPC001** - Duplicate opcode detected
- **RPC002** - Invalid handler signature (must have exactly one parameter)

## Future Enhancements (Not Implemented)

- Bounded channels with backpressure policies
- Built-in metrics (packets/sec, latency, errors)
- WebSocket/UDP transport support
- Middleware pipeline for logging, throttling, etc.
