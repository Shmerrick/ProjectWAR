using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Dapper;

namespace PWARAbilityTool.Controllers
{
    public class AccountController : ApocApiController
    {
        // GET: api/Account
        public IHttpActionResult GetAll()
        {
            try
            {
                Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress} ");
                var token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    return Ok(DbConnection.Query<Account>($"select AccountId, UserName, Banned, LastLogged, GmLevel from war_accounts.accounts  ").ToList());
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        // GET: api/Account/5
        public IHttpActionResult Get(int id)
        {
            Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress} {id}");
            try
            {
                var item = DbConnection.Query<Account>($"select AccountId, UserName, Banned, LastLogged, GmLevel from war_accounts.accounts where AccountId = {id}  ");
                if (item == null)
                {
                    return NotFound();
                }
                return Ok(item);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        // GET: api/Account?username=XXX
        public IHttpActionResult Get(string userName)
        {
            Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress} {userName}");
            try
            {
                var token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    var item = DbConnection.Query<Account>($"select AccountId, UserName, Banned, LastLogged, GmLevel from war_accounts.accounts where username = '{userName}'  ");
                    if (item == null)
                    {
                        return NotFound();
                    }
                    return Ok(item);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }
    }
}