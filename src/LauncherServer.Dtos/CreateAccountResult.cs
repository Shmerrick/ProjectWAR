namespace LauncherServer.Dtos;

public enum CreateAccountResult
{
    ACCOUNT_NAME_BUSY = 0x00,
    ACCOUNT_NAME_SUCCESS = 0x01,
    ACCOUNT_BANNED = 0x02
}