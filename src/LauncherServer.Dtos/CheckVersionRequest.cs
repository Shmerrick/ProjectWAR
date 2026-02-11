namespace LauncherServer.Dtos;

public class CheckVersionRequest
{
    public uint Version { get; set; }
    public byte Options { get; set; }
    public ulong MythLoginServiceConfigLength { get; set; }
}