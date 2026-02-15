namespace WorldServer.NetWork.V2.Dtos;

/// <summary>
/// Request DTO for the F_ENCRYPTKEY packet (opcode 0x5C).
/// Sent by the client during the encryption handshake.
/// </summary>
public class EncryptKeyRequest
{
    /// <summary>Encryption cipher type. 0 = no encryption, 1 = RC4.</summary>
    public byte Cipher { get; set; }

    /// <summary>Application identifier.</summary>
    public byte Application { get; set; }

    /// <summary>Protocol major version.</summary>
    public byte Major { get; set; }

    /// <summary>Protocol minor version.</summary>
    public byte Minor { get; set; }

    /// <summary>Protocol revision.</summary>
    public byte Revision { get; set; }

    /// <summary>Unknown field.</summary>
    public byte Unk1 { get; set; }

    /// <summary>Client encryption key (typically 256 bytes).</summary>
    public byte[] Key { get; set; } = [];
}
