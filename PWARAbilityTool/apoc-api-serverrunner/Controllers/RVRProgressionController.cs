using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Dapper;

namespace PWARAbilityTool.Controllers
{
    
    [Route("api/rvrprogression")]
    public class RVRProgressionController : ApocApiController
    {

        [HttpGet]
        public IHttpActionResult GetAll()
        {
            Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress}");

            try
            {
                var token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    var query = String.Empty;
                    query = $" SELECT * from war_world.rvr_progression ";
                    Logger.Debug(query);

                    var progressionList = DbConnection.Query<RVRProgression>(query).ToList();

                    return Ok(progressionList);
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
