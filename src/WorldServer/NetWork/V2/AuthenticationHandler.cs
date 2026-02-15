using Core.Infrastructure.Network;
using Microsoft.Extensions.Logging;
using WorldServer.NetWork.V2.Dtos;

namespace WorldServer.NetWork.V2;

/// <summary>
/// Handles authentication-related packets from the game client.
/// This is the modernized equivalent of the legacy AuthentificationHandlers class.
/// </summary>
public class AuthenticationHandler : IPacketHandler
{
    private readonly ILogger<AuthenticationHandler> _logger;

    public AuthenticationHandler(ILogger<AuthenticationHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles the F_ENCRYPTKEY packet (opcode 0x5C).
    /// The client sends its encryption capabilities and a 256-byte key.
    /// If cipher == 0: respond with F_RECEIVE_ENCRYPTKEY indicating no encryption.
    /// If cipher == 1: install RC4 encryption on the connection (not yet implemented).
    /// </summary>
    [Rpc((byte)Opcodes.F_ENCRYPTKEY)]
    public void F_ENCRYPTKEY(EncryptKeyRequest request, IConnectionContext context)
    {
        var version = $"{request.Major}.{request.Minor}.{request.Revision}";
        _logger.LogInformation(
            "Received F_ENCRYPTKEY from {RemoteAddress} — cipher={Cipher}, version={Version}, keyLength={KeyLength}",
            context.RemoteAddress, request.Cipher, version, request.Key.Length);

        if (request.Cipher == 0)
        {
            context.SendResponse((byte)Opcodes.F_RECEIVE_ENCRYPTKEY, new EncryptKeyResponse { Status = 1 });
        }
        else if (request.Cipher == 1)
        {
            // TODO: Install RC4 encryption on the connection.
            // The old code did: cclient.AddCrypt("RC4Crypto", new CryptKey(key), new CryptKey(key));
            // In the new architecture, this would likely be done via an IByteTransformer
            // that supports per-connection key installation.
            _logger.LogWarning("RC4 encryption requested but not yet implemented in V2 networking");
        }
    }
}
