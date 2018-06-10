using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApocalypseAPI.Shared
{
    public class TimeTokenManager : ITimeTokenManager
    {
        public List<TimeToken> TokenList { get; set; }
        public readonly string ServerEncryptionKey = "QCY6V0hjrKE6FMuFXS4y";
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public TimeTokenManager()
        {
            TokenList = new List<TimeToken>();
        }

        /// <summary>
        /// Does this token exist?
        /// </summary>
        /// <param name="encodedTokenString">Encoded and Encrypted Token</param>
        /// <returns></returns>
        public bool IsValidToken(string encodedTokenString)
        {
            if (WellFormedToken(encodedTokenString))
            {

                var foundToken = GetTokenByTokenString(encodedTokenString);
                if (foundToken != null)
                {
                    _logger.Debug($"Token is valid");
                    return true;
                }
                else
                {
                    _logger.Debug($"Token is NOT valid");
                    return false;
                }
            }
            else
                return false;
        }

        public bool WellFormedToken(string encodedTokenString)
        {
            // Ensure token is correctly constructed
            var plainToken = DecodeDecryptToken(encodedTokenString);
            _logger.Debug($"Plain Token {plainToken}");
            var splitToken = plainToken.Split('|');
            if (splitToken.Length != 3)
            {
                _logger.Debug($"Badly structured token.");
                return false;
            }
            return true;
        }

        public TimeToken GetTokenByTokenString(string newToken)
        {
            foreach (var token in this.TokenList)
            {
                if (token.Token == newToken)
                {
                    return token;
                }
            }
            return null;
        }

        public TimeToken GetTokenByUserName(string userName)
        {
            foreach (var token in this.TokenList)
            {
                if (token.UserName == userName)
                {
                    return token;
                }
            }
            return null;
        }

        /// <summary>
        /// Add the new token to the list, if it already exists return that existing one, if not add it
        /// </summary>
        /// <param name="newToken"></param>
        /// <returns></returns>
        public TimeToken AddToken(TimeToken newToken)
        {
            var foundToken = GetTokenByUserName(newToken.UserName);
            if (foundToken != null)
            {
                // Token exists.
                return foundToken;
            }
            else
            {
                TokenList.Add(newToken);
                return newToken;
            }
        }

        /// <summary>
        /// Remove the token for this userName from the list.
        /// </summary>
        /// <param name="newToken"></param>
        /// <returns></returns>
        public void RemoveToken(string userName)
        {
            var itemToRemove = this.TokenList.Single(r => r.UserName == userName);
            this.TokenList.Remove(itemToRemove);
        }

        public string CreateToken(string userName, string plainPassword)
        {
            var token = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "|" + userName + "|" + plainPassword;
            return token;
        }

        public string EncodeEncryptToken(string plainToken)
        {
            var encryptedToken = Shared.Cryptography.EncryptString(plainToken, ServerEncryptionKey);
            var encodedToken = Cryptography.Base64Encode(encryptedToken);
            return encodedToken;

        }

        public string DecodeDecryptToken(string eeToken)
        {
            var encryptedToken = Cryptography.Base64Decode(eeToken);
            var plainToken = Cryptography.DecryptString(encryptedToken, ServerEncryptionKey);
            return plainToken;

        }
    }
}

