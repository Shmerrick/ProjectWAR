using System;
using System.Linq;
using Core.Infrastructure.Network;
using LauncherServer.Config;
using LauncherServer.Dtos;
using Microsoft.Extensions.Logging;

namespace LauncherServer.Server;

public class LauncherClient : IPacketHandler
{
    private readonly AccountMgr.AccountMgrClient _accountMgrClient;
    private readonly MythLoginServiceConfigManager _loginServiceConfigManager;
    private readonly LauncherConfig _config;
    private readonly ILogger<LauncherClient> _logger;

    public LauncherClient(
        AccountMgr.AccountMgrClient accountMgrClient,
        MythLoginServiceConfigManager loginServiceConfigManager,
        LauncherConfig config,
        ILogger<LauncherClient> logger)
    {
        _accountMgrClient = accountMgrClient;
        _loginServiceConfigManager = loginServiceConfigManager;
        _config = config;
        _logger = logger;
    }

    [Rpc(Opcodes.CL_CHECK, Opcodes.LCR_CHECK)]
    public CheckVersionResponse CL_CHECK(CheckVersionRequest packet)
    {
        _logger.LogDebug("Launcher Version : {Version}", packet.Version);

        if (packet.Version != _config.Version)
        {
            return new CheckVersionResponse
            {
                Result = (byte)CheckResult.LAUNCHER_VERSION,
                MessageOrMythLoginServiceConfig = _config.Message
            };
        }

        if ((packet.Options & 1) == 1)
        {
            _logger.LogDebug("Has mythic file info");
            if (packet.MythLoginServiceConfigLength != (ulong)_loginServiceConfigManager.Content.Length)
            {
                return new CheckVersionResponse
                {
                    Result = (byte)CheckResult.LAUNCHER_FILE,
                    MessageOrMythLoginServiceConfig = _config.Message
                };
            }
        }

        if ((packet.Options & 2) == 2)
        {
            _logger.LogDebug("Has system info");
        }

        return new CheckVersionResponse { Result = (byte)CheckResult.LAUNCHER_OK };
    }

    [Rpc(Opcodes.CL_CREATE, Opcodes.LCR_CREATE)]
    public Dtos.CreateAccountResponse CL_CREATE(Dtos.CreateAccountRequest request, IConnectionContext context)
    {
        var result = CreateAccountResult.ACCOUNT_BANNED;
        var ip = context.RemoteAddress?.Split(":")[0];

        if (!_accountMgrClient.IsIpBanned(new IsIpBannedRequest { IpAddress = ip }).IsBanned)
        {
            var createAccountRequest = new CreateAccountRequest()
            {
                Username = request.Username,
                Password = request.Password,
                Email = request.Email ?? "",
                LanguageId = Convert.ToUInt32(request.LangID),
                IpAddress = ip
            };

            if (_accountMgrClient.CreateAccount(createAccountRequest).Created)
                result = CreateAccountResult.ACCOUNT_NAME_SUCCESS;
            else
                result = CreateAccountResult.ACCOUNT_NAME_BUSY;
        }

        return new Dtos.CreateAccountResponse { Status = result };
    }

    [Rpc(Opcodes.CL_START, Opcodes.LCR_START)]
    public StartResponse CL_START(StartRequest startRequest)
    {
        var authResult = _accountMgrClient.AuthenticateUser(new AuthenticateUserRequest
        {
            Username = startRequest.Username,
            Password = startRequest.PasswordHash
        });

        var response = new StartResponse
        {
            Result = authResult.Result switch
            {
                LoginResult.Success => Dtos.LoginResult.Success,
                LoginResult.InvalidCredentials => Dtos.LoginResult.InvalidCredentials,
                LoginResult.AccountBanned => Dtos.LoginResult.AccountBanned,
                LoginResult.NotActive => Dtos.LoginResult.NotActive,
                LoginResult.PatcherNotAllowed => Dtos.LoginResult.PatcherNotAllowed,
                _ => throw new InvalidOperationException()
            }
        };

        if (authResult.Result == LoginResult.Success)
        {
            _logger.LogDebug("Sending token to client : {Username}", startRequest.Username);
            response.AuthToken = authResult.Token;
        }

        return response;
    }

    [Rpc(Opcodes.CL_INFO, Opcodes.LCR_INFO)]
    public GetInfoResponse CL_INFO(GetInfoRequest request)
    {
        var realmsResponse = _accountMgrClient.ListRealms(new ListRealmsRequest());

        return new GetInfoResponse
        {
            RealmInfo = realmsResponse.Realms.Select(x =>
                new Dtos.RealmInfo
                {
                    Name = x.Name,
                    OnlinePlayers = x.OnlinePlayers,
                    OrderCount = x.OrderCount,
                    DestructionCount = x.DestructionCount
                }
            ).ToList()
        };
    }

    [Rpc(Opcodes.CL_VERSION, Opcodes.LCR_VERSION)]
    public GetVersionResponse CL_VERSION(GetVersionRequest request)
    {
        var g = Guid.NewGuid();
        return new GetVersionResponse
        {
            VersionHash = PatchMgr.VersionHash,
            ServerState = _config.ServerState,
            InstalId = g.ToString()
        };
    }
}
