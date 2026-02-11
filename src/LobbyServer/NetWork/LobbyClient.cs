using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using FrameWork;
using FrameWork.NetWork.V4;
using Google.Protobuf;
using IMessage = Google.Protobuf.IMessage;

namespace LobbyServer.NetWork;

public partial class LobbyClient : Client
{
    private const int PROTOCOL_OPCODE_SIZE = sizeof(byte);
    
    private readonly AccountMgr.AccountMgrClient _accountMgrClient;
    
    public LobbyClient(
        TcpClient tcpClient,
        IPacketSerializerFactory serializerFactory,
        AccountMgr.AccountMgrClient accountMgrClient,
        IByteTransformer byteTransformer = null,
        int receiveBufferSize = 65536,
        int errorThreshold = 3)
        : base(tcpClient, serializerFactory, byteTransformer, receiveBufferSize, errorThreshold)
    {
        _accountMgrClient = accountMgrClient;
    }


    protected override bool TryExtractPacket(ref ReadOnlyMemory<byte> buffer, out ReadOnlyMemory<byte> packet)
    {
        Log.Info("TryExtractPacket", "Attempting to extract packet from buffer, length: " + buffer.Length);
        packet = default;
        
        while (buffer.Length > 1)
        {
            // Snapshot buffer position so we can restore it if there isn't enough data yet
            var savedBuffer = buffer;
            
            var packetLength = ReadVariableLengthSize(ref buffer);
            
            if (packetLength == 0)
            {
                // Zero-length payload: extract just the opcode byte
                if (buffer.Length < PROTOCOL_OPCODE_SIZE)
                {
                    buffer = savedBuffer;
                    return false;
                }
                
                packet = buffer[..PROTOCOL_OPCODE_SIZE];
                buffer = buffer[PROTOCOL_OPCODE_SIZE..];
                return true;
            }
            
            // Validate packet length and check if we have the full packet
            if (packetLength < 0 || buffer.Length < PROTOCOL_OPCODE_SIZE + packetLength)
            {
                // Not enough data yet — restore buffer to before the varint
                buffer = savedBuffer;
                return false;
            }
            
            // Extract packet data (without length header)
            packet = buffer[..(packetLength + PROTOCOL_OPCODE_SIZE)];

            // Advance buffer past this packet
            buffer = buffer[(packetLength + PROTOCOL_OPCODE_SIZE)..];
            return true;
        }
        
        return false;
    }
    
    private static int ReadVariableLengthSize(ref ReadOnlyMemory<byte> buffer)
    {
        var size = 0;
        var byteCount = 0;

        while (buffer.Length > 0)
        {
            var mByte = buffer.Span[0];
            buffer = buffer[1..];

            size |= (mByte & 0x7F) << (7 * byteCount);
            byteCount++;

            if ((mByte & 0x80) == 0)
                return size;
        }

        return 0; // Buffer exhausted before decoding completed
    }

    protected override byte ExtractOpcode(ReadOnlySpan<byte> packet, out int payloadOffset)
    {
        payloadOffset = PROTOCOL_OPCODE_SIZE;
        return packet[0];
    }

    protected override ReadOnlyMemory<byte> CreatePacket<T>(byte opcode, T payload)
    {
        if (payload is not IMessage message)
            throw new ArgumentException($"Payload must implement IMessage, got {typeof(T).Name}", nameof(payload));

        var payloadSize = message.CalculateSize();
        var varintSize = ComputeVarintSize(payloadSize);

        // Single allocation: [varint size][opcode][protobuf payload]
        var packet = new byte[varintSize + PROTOCOL_OPCODE_SIZE + payloadSize];
        var span = packet.AsSpan();

        WriteVariableLengthSize(span, payloadSize);
        span[varintSize] = opcode;
        message.WriteTo(span.Slice(varintSize + PROTOCOL_OPCODE_SIZE, payloadSize));

        return packet;
    }

    private static int ComputeVarintSize(int value)
    {
        var size = 1;
        while (value > 0x7F) { value >>= 7; size++; }
        return size;
    }

    private static void WriteVariableLengthSize(Span<byte> buffer, int size)
    {
        var index = 0;
        while (size > 0x7F)
        {
            buffer[index++] = (byte)(size | 0x80);
            size >>= 7;
        }
        buffer[index++] = (byte)size;
    }
    
    [Rpc((int)Opcodes.CMSG_VerifyProtocolReq, (int)Opcodes.SMSG_VerifyProtocolReply)]
    public VerifyProtocolReply CMSG_VerifyProtocolReq()
    {
        Log.Info("CMSG_VerifyProtocolReq", "Received VerifyProtocolReq from " + GetRemoteAddress());
        byte[] IV_HASH1 = [0x01, 0x53, 0x21, 0x4d, 0x4a, 0x04, 0x27, 0xb7, 0xb4, 0x59, 0x0f, 0x3e, 0xa7, 0x9d, 0x29, 0xe9];
        byte[] IV_HASH2 = [0x49, 0x18, 0xa1, 0x2a, 0x64, 0xe1, 0xda, 0xbd, 0x84, 0xd9, 0xf4, 0x8a, 0x8b, 0x3c, 0x27, 0x20];

        return new VerifyProtocolReply
        {
            ResultCode = ResultCode.ResSuccess,
            Iv1 = ByteString.CopyFrom(IV_HASH1),
            Iv2 = ByteString.CopyFrom(IV_HASH2)
        };
    }
    
    [Rpc((byte)Opcodes.CMSG_AuthSessionTokenReq, (byte)Opcodes.SMSG_AuthSessionTokenReply)]
    public AuthSessionTokenReply CMSG_AuthSessionTokenReq(AuthSessionTokenReq request)
    {
        Log.Info("CMSG_AuthSessionTokenReq", "Received AuthSessionTokenReq from " + GetRemoteAddress());
        
        return new AuthSessionTokenReply
        {
            ResultCode = ResultCode.ResSuccess
        };
    }
    
    [Rpc((int)Opcodes.CMSG_GetAcctPropListReq, (int)Opcodes.SMSG_GetAcctPropListReply)]
    public GetAcctPropListReply CMSG_GetAcctPropListReq()
    {
        Log.Info("CMSG_GetAcctPropListReq", "Received GetAcctPropListReply");
        return new GetAcctPropListReply
        {
            ResultCode = ResultCode.ResSuccess
        };
    }
    
    [Rpc((byte)Opcodes.CMSG_MetricEventNotify)]
    public void CMSG_MetricEventNotify()
    {
        Log.Info("CMSG_MetricEventNotify", "Received MetricEventNotify");
        //do nothing
    }
    
    [Rpc((int)Opcodes.CMSG_GetClusterListReq, (byte)Opcodes.SMSG_GetClusterListReply)]
    public async Task<GetClusterListReply> CMSG_GetClusterListReq()
    {
        var clustersListResponse = await _accountMgrClient.GetClusterListAsync(new GetClusterListRequest());

        var response = new GetClusterListReply { ResultCode = ResultCode.ResSuccess };
        response.ClusterList.AddRange(clustersListResponse.Clusters);
        
        return response;
    }
    
    [Rpc((byte)Opcodes.CMSG_GetCharSummaryListReq, (byte)Opcodes.SMSG_GetCharSummaryListReply)]
    public GetCharSummaryListReply CMSG_GetCharSummaryListReq()
    {
        Log.Info("CMSG_GetCharSummaryListReq", "Received GetCharSummaryListReq");
        return new GetCharSummaryListReply
        {
            ResultCode = ResultCode.ResSuccess,
        };
    }
}