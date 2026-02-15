namespace WorldServer.NetWork.V2.Dtos;

/// <summary>
/// Response DTO for the F_RECEIVE_ENCRYPTKEY packet (opcode 0x8A).
/// Sent by the server to acknowledge the encryption handshake.
/// </summary>
public class EncryptKeyResponse
{
    /// <summary>Encryption status. 1 = no encryption (cipher was 0).</summary>
    public byte Status { get; set; }
}
