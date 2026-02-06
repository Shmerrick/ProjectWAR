namespace LauncherServer.Dtos;

public class GetVersionResponse
{
    // [Reversed]
    public uint VersionHash { get; set; }
    public ServerState ServerState { get; set; }
    public string InstalId { get; set; }
}
