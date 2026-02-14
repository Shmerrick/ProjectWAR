using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Threading.Tasks;
using Core.Infrastructure.Network;
using LauncherServer.Dtos;

namespace Launcher.NetWork;

public partial class LauncherProxy : Client
{
    private const int PROTOCOL_LENGTH_SIZE = sizeof(int);
    private const int PROTOCOL_OPCODE_SIZE = sizeof(byte);

    public LauncherProxy(
        TcpClient tcpClient,
        IPacketSerializerFactory serializerFactory,
        IByteTransformer byteTransformer = null,
        int receiveBufferSize = 65536,
        int errorThreshold = 3) : base(
        tcpClient, serializerFactory, byteTransformer, receiveBufferSize, errorThreshold)
    {
    }

    protected override bool TryExtractPacket(ref ReadOnlyMemory<byte> buffer, out ReadOnlyMemory<byte> packet)
    {
        packet = default;
        
        while (buffer.Length >= PROTOCOL_LENGTH_SIZE)
        {
            // var packetLength = Marshal.ConvertToInt32(lengthBytes.ToArray());
            var packetLength = BinaryPrimitives.ReadInt32BigEndian(buffer[..PROTOCOL_LENGTH_SIZE].Span);
            
            // Handle zero padding (256-byte block boundaries)
            if (packetLength == 0)
            {
                // Skip 4 zero bytes and continue
                buffer = buffer[PROTOCOL_LENGTH_SIZE..];
                continue;
            }
            
            // Validate packet length and check if we have the full packet
            if (packetLength < 0 || buffer.Length < PROTOCOL_OPCODE_SIZE + packetLength)
                return false;
            
            // Extract packet data (without length header)
            packet = buffer.Slice(4, packetLength - PROTOCOL_LENGTH_SIZE + PROTOCOL_OPCODE_SIZE);

            // Advance buffer past this packet
            buffer = buffer[(packetLength + PROTOCOL_OPCODE_SIZE)..];
            return true;
        }
        
        return false;
    }

    protected override byte ExtractOpcode(ReadOnlySpan<byte> packet, out int payloadOffset)
    {
        payloadOffset = PROTOCOL_OPCODE_SIZE;
        return packet[0];
    }

    protected override ReadOnlyMemory<byte> CreatePacket<T>(byte opcode, T payload)
    {
        var bufferWriter = new ArrayBufferWriter<byte>();
        var lengthSpan = bufferWriter.GetSpan(4);
        bufferWriter.Advance(4);
        bufferWriter.Write(new[] { opcode });
        Serializer.Serialize(bufferWriter, payload);
        
        BinaryPrimitives.WriteInt32BigEndian(lengthSpan, bufferWriter.WrittenCount - PROTOCOL_OPCODE_SIZE);
        return bufferWriter.WrittenMemory;
    }
    
    [Rpc((byte)Opcodes.CL_CHECK, (byte)Opcodes.LCR_CHECK)]
    public partial Task<CheckVersionResponse> CL_CHECK(CheckVersionRequest request);
    
    [Rpc((byte)Opcodes.CL_CREATE, (byte)Opcodes.LCR_CREATE)]
    public partial Task<CreateAccountResponse> CL_CREATE(CreateAccountRequest request);
    
    [Rpc((byte)Opcodes.CL_START, (byte)Opcodes.LCR_START)]
    public partial Task<StartResponse> CL_START(StartRequest request);
    
    [Rpc((byte)Opcodes.CL_INFO, (byte)Opcodes.LCR_INFO)]
    public partial Task<GetInfoResponse> CL_INFO(GetInfoRequest request);
}