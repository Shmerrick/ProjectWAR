namespace LauncherServer.Dtos;

public class CheckVersionResponse
{
    public byte Result { get; set; }
    public string? MessageOrMythLoginServiceConfig { get; set; }
}