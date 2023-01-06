using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;

namespace PWARAbilityTool.Controllers
{
    public class IconController : ApocApiController
    {

        

        // GET: api/icon/5
        public IHttpActionResult Get(int id)
        {
            Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress} {id}");
            try
            {
                var token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    var icon = Startup.iconService.GetIcon(id);
                    if (icon == null)
                        return NotFound();
                    else
                    {
                        return Ok(icon);
                    }
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
