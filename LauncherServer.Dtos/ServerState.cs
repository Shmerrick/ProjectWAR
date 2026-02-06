namespace LauncherServer.Dtos;

public enum ServerState
{
    CLOSED = 0,
    OPEN = 1, //0 = closed, specify reason in byte 32-64
    CORE = 2, //allow only core testers and up to access server
    STAFF = 3, //allow staff and up to access server
    DEV = 4, //allow only highest GM level to access server
    PATCH = 5, //closed status reason
}