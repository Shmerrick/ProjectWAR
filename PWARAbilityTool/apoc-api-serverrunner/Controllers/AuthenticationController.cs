using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using apoc_api.common;
using apoc_api.common.Managers;
using apoc_api.security;

namespace PWARAbilityTool.Controllers
{
    public class AuthenticationController : ApocApiController
    {
        public LoginManager LoginManager { get; private set; }

        public AuthenticationController()
        {
            LoginManager = new LoginManager();
        }

        /// <summary>
        /// Returns an encoded, encrypted token for a login attempt (user/password).
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="encryptedToken">Encoded + AES</param>
        /// <returns></returns>
        [HttpGet]
        //[Route("api/Authentication?userName={userName}?encryptedToken={encryptedToken}")]
        [Route("api/Authentication/{userName}/{encryptedToken}")]
        public IHttpActionResult Login(string userName, string encryptedToken)
        {
            try
            {
                Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress} User:{userName} EEToken:{encryptedToken}");

                if (!Startup.tokenManager.WellFormedToken(encryptedToken))
                {
                    Logger.Debug($"Unauthorised (not well formed token) Login Attempt {userName}{encryptedToken}");
                    return Unauthorized();
                }

                // authKey is sent by the client in Request headers.
                var authKey = Request.Headers.GetValues("auth-key").FirstOrDefault();
                if (!LoginManager.ValidAuthKey(authKey))
                    return Unauthorized();

                // Get the plain password.
                var plainToken = Startup.tokenManager.DecodeDecryptToken(encryptedToken);
                var plainPassword = plainToken.Split('|')[2];
                // Create a token string (plain)
                var plainTokenString = Startup.tokenManager.CreateToken(userName, plainPassword);

                var accountId = LoginManager.CanLogin(userName.ToLower(), plainPassword.ToLower(), authKey);
                if (accountId > 0)
                {
                    Logger.Debug($"Adding Token {userName}{encryptedToken}");
                    Startup.tokenManager.AddToken(new TimeToken(userName, encryptedToken, DateTime.Now));
                    return Ok(encryptedToken);
                }
                else
                {
                    Logger.Debug($"Unauthorised Login Attempt {userName}{encryptedToken}");
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return InternalServerError(e);
            }
        }

        ///// <summary>
        ///// Returns an encoded, encrypted token for a login attempt (user/password).
        ///// </summary>
        ///// <param name="userName"></param>
        ///// <param name="password">plain text password</param>
        ///// <returns></returns>
        //[HttpGet]
        //[Route("authenticate")]
        //public IHttpActionResult Authenticate(string userName, string password)
        //{
        //    try
        //    {
        //        Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress} User:{userName} Pwd:{password}");

        //        var authenticationEngine = new Authenticator
        //        {
        //            BaseUrl = System.Configuration.ConfigurationManager.AppSettings["base-url"],
        //            AuthenticationPath = System.Configuration.ConfigurationManager.AppSettings["authentication-path"],
        //            HeartbeatPath = System.Configuration.ConfigurationManager.AppSettings["heartbeat-path"]
        //        };
        //        if (authenticationEngine.CanConnect())
        //        {
        //            var authenticationToken = authenticationEngine.Connect(userName, password);
        //            if (authenticationToken == "")
        //            {
        //                Logger.Debug($"Login attempt - Unauthorised");
        //                return Unauthorized();
        //            }
        //            else
        //            {
        //                Logger.Debug($"Login attempt - OK. Token = {authenticationToken}");
        //                Startup.tokenManager.AddToken(authenticationToken);
        //                // Extract the AccountId from the response
        //                var accountId = authenticationToken.Substring(authenticationToken.LastIndexOf("||") + 2);

        //                return Ok(accountId);
        //            }
        //        }
        //        else
        //        {
        //            Logger.Debug($"Login attempt - Cannot Connect");
        //            return InternalServerError(new Exception($"Login attempt - Cannot Connect"));
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        Logger.Debug($"Login attempt exception. {e.Message} {e.StackTrace}");
        //        return InternalServerError(e);
        //    }
        //}
    }
}