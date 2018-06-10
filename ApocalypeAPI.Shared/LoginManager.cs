using MySql.Data.MySqlClient;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace ApocalypseAPI.Shared
{
    public class LoginManager : ILoginManager
    {
        private readonly IDbConnectionService _db;
        private MySqlConnection dbConnection { get; set; }
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();


        public LoginManager(IDbConnectionService db)
        {
            _db = db;
            dbConnection = new MySqlConnection(db.GetConnectionString());
        }


        /// <summary>
        /// Calls the DB and returns the accountId if the login is successful.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool CanLogin(string userName, string password, string authKey)
        {
            try
            {
                _logger.Debug($"Attempting to login {userName}{password}");
                // Generates the password as per the client Launcher
                var cryptPassword = Cryptography.ConvertSHA256(userName + ":" + password);
                _logger.Trace($"Crypt password {cryptPassword}");
                var accountId = dbConnection.ExecuteScalar<int>($"SELECT AccountId from war_accounts.accounts where CryptPassword = {cryptPassword} and Username = {userName}");
                _logger.Trace($"AccountId {accountId}");
                if (accountId > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception e)
            {
                _logger.Error($"Exception for {userName} {e.Message}");
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Is the Auth Key valid (number and divisible by 7)
        /// </summary>
        /// <param name="authKey"></param>
        /// <returns></returns>
        public bool ValidAuthKey(string authKey)
        {
            int number;
            bool result = Int32.TryParse(authKey, out number);
            if (!result)
            {
                return false;
            }
            else
            {
                if ((number % 7) == 0)
                    return true;
                else
                    return false;
            }
        }
    }
}
