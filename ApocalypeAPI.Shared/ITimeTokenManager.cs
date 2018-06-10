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
    }
}