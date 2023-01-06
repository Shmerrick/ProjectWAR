using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using apoc_api.common.Reference;
using Dapper;

namespace PWARAbilityTool.Controllers
{
    [Route("api/Vendor")]
    public class VendorController : ApocApiController
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
                    var vendorList = DbConnection.Query<Vendor>($"SELECT Entry, Name, VendorId, Title from war_world.creature_protos where VendorId <> 0 order by Name ").ToList();
                    foreach (var vendor in vendorList)
                    {
                        vendor.Title = Enum.GetName(typeof(CreatureTitle), Convert.ToInt32(vendor.Title));
                    }
                    return Ok(vendorList);
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