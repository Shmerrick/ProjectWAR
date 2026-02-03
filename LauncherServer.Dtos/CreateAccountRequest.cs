namespace LauncherServer.Dtos;

public class CreateAccountRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public string? Email { get; set; }
    public byte? LangID { get; set; }
}