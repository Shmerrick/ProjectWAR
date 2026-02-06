namespace LauncherServer.Dtos;

public class RealmInfo
{
    public bool Online { get; set; }
    public string Name { get; set; }
    public uint OnlinePlayers { get; set; }
    public uint OrderCount { get; set; }
    public uint DestructionCount { get; set; }
}