namespace ApocalypseAPI.Common
{
    public interface ILoginManager
    {
        bool CanLogin(string userName, string password, string authKey);
        bool ValidAuthKey(string authKey);
    }
}