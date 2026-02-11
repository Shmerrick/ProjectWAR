namespace LauncherServer.Dtos;

public class StartRequest
{
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
}
