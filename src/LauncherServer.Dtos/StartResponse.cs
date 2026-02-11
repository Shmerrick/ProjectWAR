namespace LauncherServer.Dtos;

public class StartResponse
{
    public LoginResult Result { get; set; }
    public string? AuthToken { get; set; }
}