namespace LauncherServer.Dtos;

public enum LoginResult
{
    Success = 0,
    InvalidCredentials = 1,
    AccountBanned = 2,
    NotActive = 3,
    PatcherNotAllowed = 4,
}