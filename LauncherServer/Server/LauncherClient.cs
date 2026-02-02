using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Net.Sockets;
using FrameWork;
using FrameWork.NetWork.V4;
using ClientV4 = FrameWork.NetWork.V4.Client;

namespace LauncherServer.Server;

public partial class LauncherClient : ClientV4
{
    private const int PROTOCOL_LENGTH_SIZE = sizeof(int);
    private const int PROTOCOL_OPCODE_SIZE = sizeof(byte);
    
    public LauncherClient(
        TcpClient tcpClient,
        IPacketSerializerFactory serializerFactory,
        IByteTransformer? byteTransformer = null)
        : base(tcpClient, serializerFactory, byteTransformer, receiveBufferSize: 65536, errorThreshold: 3)
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

    [Rpc(Opcodes.CL_CHECK, Opcodes.LCR_CHECK)]
    public CheckVersionResponse CL_CHECK(CheckVersionRequest packet)
    {
        Log.Debug("CL_CHECK", "Launcher Version : " + packet.Version);
        
        if (packet.Version != Core.Version)
        {
            return new CheckVersionResponse
            {
                Result = (byte)CheckResult.LAUNCHER_VERSION,
                MessageOrMythLoginServiceConfig = Core.Message
            };
        }
        
        if ((packet.Options & 1) == 1)
        {
            Log.Debug("CHECK", "Has mythic file info");

            if (packet.MythLoginServiceConfigLength != (ulong)Core.Info.Length)
            {
                return new CheckVersionResponse
                {
                    Result = (byte)CheckResult.LAUNCHER_FILE,
                    MessageOrMythLoginServiceConfig = Core.StrInfo
                };
            }
        }

        if ((packet.Options & 2) == 2)
        {
            // Dictionary<string, object> computerProfile = readProfile(ref packet);

            Log.Debug("CHECK", "Has system info");
        }

        return new CheckVersionResponse
        {
            Result = (byte)CheckResult.LAUNCHER_OK,
        };
    }

    [Rpc(Opcodes.CL_CREATE, Opcodes.LCR_CREATE)]
    public CreateAccountResponse CL_CREATE(CreateAccountRequest request)
    {
        var result = CreateAccountResult.ACCOUNT_BANNED;
        
        var ip = GetRemoteAddress()?.Split(":")[0];

        // Check Ip Ban
        if (!Core.AcctMgr.IsIpBanned(new IsIpBannedRequest { IpAddress = ip }).IsBanned)
        {
            var createAccountRequest = new global::CreateAccountRequest()
            {
                Username = request.Username,
                Password = request.Password,
                Email = request.Email ?? "",
                LanguageId = Convert.ToUInt32(request.LangID),
                IpAddress = ip
            };

            if (Core.AcctMgr.CreateAccount(createAccountRequest).Created)
            {
                result = CreateAccountResult.ACCOUNT_NAME_SUCCESS;
            }
            else
            {
                result = CreateAccountResult.ACCOUNT_NAME_BUSY;
            }
        }

        return new CreateAccountResponse { Status = result };
    }
    
    [Rpc(Opcodes.CL_START, Opcodes.LCR_START)]
    public StartResponse CL_START(StartRequest startRequest)
    {
        var authResult = Core.AcctMgr.AuthenticateUser(new AuthenticateUserRequest
        {
            Username = startRequest.Username,
            Password = startRequest.PasswordHash
        });

        var response = new StartResponse
        {
            Result = authResult.Result
        };

        if (authResult.Result == LoginResult.Success)
        {
            Log.Debug("CL_START", "Sending token to client : " + startRequest.Username + " token : " + authResult.Token);
            response.AuthToken = authResult.Token;
        }

        return response;
    }

    public class CheckVersionRequest
    {
        public uint Version { get; set; }
        public byte Options { get; set; }
        public ulong MythLoginServiceConfigLength { get; set; }
    }
    
    public class CheckVersionResponse
    {
        public byte Result { get; set; }
        public string? MessageOrMythLoginServiceConfig { get; set; }
    }
    
    public class CreateAccountRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public string? Email { get; set; }
        public byte? LangID { get; set; }
    }

    public enum CreateAccountResult
    {
        ACCOUNT_NAME_BUSY = 0x00,
        ACCOUNT_NAME_SUCCESS = 0x01,
        ACCOUNT_BANNED = 0x02
    }

    public class CreateAccountResponse
    {
        public CreateAccountResult Status { get; set; }
    }
    
    public class StartRequest
    {
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
    }
    
    public class StartResponse
    {
        public LoginResult Result { get; set; }
        public string? AuthToken { get; set; }
    }
}

[PacketSerializerContext(
    typeof(LauncherClient.CheckVersionRequest),
    typeof(LauncherClient.CheckVersionResponse),
    typeof(LauncherClient.CreateAccountRequest),
    typeof(LauncherClient.CreateAccountResponse),
    typeof(LauncherClient.StartRequest))]
public partial class LauncherSerializerContext
{
}