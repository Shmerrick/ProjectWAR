using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using NLog;
using System.Collections.Generic;
using ApocalypseAPI.Shared;

namespace ApocalypseAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/Authentication")]
    public class AuthenticationController : Controller
    {
        
        private readonly IDbConnectionService _db;
        private MySqlConnection dbConnection { get; set; }
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public TimeTokenManager TokenManager { get; set; }
        public ILoginManager LoginManager { get; }

        public AuthenticationController(IDbConnectionService db, ILoginManager loginManager)
        {
            _db = db;
            LoginManager = loginManager;
            dbConnection = new MySqlConnection(db.GetConnectionString());
            TokenManager = new TimeTokenManager();
        }

        /// <summary>
        /// Returns an encoded, encrypted token for a login attempt (user/password).
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="encryptedPassword"> Encoded AES</param>
        /// <returns></returns>
        [HttpGet("")]
        public IActionResult Login(string userName, string encryptedPassword)
        {
            try
            {
                _logger.Debug($"Login attempt {userName}{encryptedPassword}");

                // authKey is sent by the client in Request headers. 
                var authKey = Request.Headers["auth-key"];
                // Get the plain password.
                var plainPassword = Cryptography.DecryptString(encryptedPassword, TokenManager.ServerEncryptionKey);
                // Create a token string (plain)
                var plainTokenString = TokenManager.CreateToken(userName, plainPassword);

                var canLogin = LoginManager.CanLogin(userName, plainPassword);
                if (canLogin)
                {
                    var encodedToken = TokenManager.EncodeEncryptToken(plainTokenString);
                    _logger.Trace($"Encoded token {encodedToken}");
                    TokenManager.AddToken(new TimeToken(userName, encodedToken, DateTime.Now));
                    return Ok(encodedToken);
                }
                else
                {
                    _logger.Debug($"Unauthorised Login Attempt {userName}{encryptedPassword}");
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

    }
}