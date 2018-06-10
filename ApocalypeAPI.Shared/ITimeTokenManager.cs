using System.Collections.Generic;

namespace ApocalypseAPI.Shared
{
    public interface ITimeTokenManager
    {
        List<TimeToken> TokenList { get; set; }

        TimeToken AddToken(TimeToken newToken);
        TimeToken GetTokenByUserName(string userName);
        void RemoveToken(string userName);
        bool IsValidToken(string encodedTokenString);

        bool WellFormedToken(string encodedTokenString);
        TimeToken GetTokenByTokenString(string newToken);
        string EncodeEncryptToken(string plainToken);
        string DecodeDecryptToken(string eeToken);
        string CreateToken(string userName, string plainPassword);
    }
}