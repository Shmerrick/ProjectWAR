namespace WorldServer.Managers;

public static class AccountMgrClientExtensions
{
    public static string GetAccountSchemaName(this AccountMgr.AccountMgrClient client) => "war_accounts";
}