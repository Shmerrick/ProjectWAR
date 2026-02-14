using System.Threading.Tasks;
using Core.Infrastructure.Network;
using Google.Protobuf;
using Microsoft.Extensions.Logging;

namespace LobbyServer.NetWork;

public class LobbyClient : IPacketHandler
{
    private readonly ILogger<LobbyClient> _logger;

    public LobbyClient(ILogger<LobbyClient> logger)
    {
        _logger = logger;
    }

    [Rpc((int)Opcodes.CMSG_VerifyProtocolReq, (int)Opcodes.SMSG_VerifyProtocolReply)]
    public VerifyProtocolReply CMSG_VerifyProtocolReq(IConnectionContext context)
    {
        _logger.LogInformation("Received VerifyProtocolReq from {RemoteAddress}", context.RemoteAddress);
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
    public AuthSessionTokenReply CMSG_AuthSessionTokenReq(AuthSessionTokenReq request, IConnectionContext context)
    {
        _logger.LogInformation("Received AuthSessionTokenReq from {RemoteAddress}", context.RemoteAddress);
        return new AuthSessionTokenReply
        {
            ResultCode = ResultCode.ResSuccess
        };
    }

    [Rpc((int)Opcodes.CMSG_GetAcctPropListReq, (int)Opcodes.SMSG_GetAcctPropListReply)]
    public GetAcctPropListReply CMSG_GetAcctPropListReq()
    {
        _logger.LogInformation("Received GetAcctPropListReply");
        return new GetAcctPropListReply
        {
            ResultCode = ResultCode.ResSuccess
        };
    }

    [Rpc((byte)Opcodes.CMSG_MetricEventNotify)]
    public void CMSG_MetricEventNotify()
    {
        _logger.LogInformation("Received MetricEventNotify");
    }

    [Rpc((int)Opcodes.CMSG_GetClusterListReq, (byte)Opcodes.SMSG_GetClusterListReply)]
    public async Task<GetClusterListReply> CMSG_GetClusterListReq(
        [FromServices] AccountMgr.AccountMgrClient accountMgrClient)
    {
        var clustersListResponse = await accountMgrClient.GetClusterListAsync(new GetClusterListRequest());

        var response = new GetClusterListReply { ResultCode = ResultCode.ResSuccess };
        response.ClusterList.AddRange(clustersListResponse.Clusters);

        return response;
    }

    [Rpc((byte)Opcodes.CMSG_GetCharSummaryListReq, (byte)Opcodes.SMSG_GetCharSummaryListReply)]
    public GetCharSummaryListReply CMSG_GetCharSummaryListReq()
    {
        _logger.LogInformation("Received GetCharSummaryListReq");
        return new GetCharSummaryListReply
        {
            ResultCode = ResultCode.ResSuccess,
        };
    }
}
