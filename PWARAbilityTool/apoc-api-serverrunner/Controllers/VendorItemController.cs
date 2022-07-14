using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Dapper;

namespace PWARAbilityTool.Controllers
{
    
    [Route("api/Vendor/Item")]
    public class VendorItemController : ApocApiController
    {
    
        ///api/Vendor/Item/48
        public IHttpActionResult GetByVendorId(long vendorId)
        {
            Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Vendor {vendorId}");
            try
            {
                var token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    var vendorItemList = DbConnection.Query<VendorItem>(
                        $" SELECT vendorid, itemid, price, reqitems, name, rarity, stats, objectlevel, minrenown, minrank " +
                        $" FROM war_world.vendor_items vi, war_world.item_infos ii where vendorid = {vendorId} and ii.entry = vi.itemid " +
                        $" ").ToList();

                    foreach (var vendorItem in vendorItemList)
                    {
                        var noBrackets = vendorItem.ReqItems.Replace("(", "");
                        noBrackets = noBrackets.Replace(")", "");

                        if (noBrackets == "")
                            continue;

                        vendorItem.CrestCount = Convert.ToInt32(noBrackets.Split(',')[0]);
                        vendorItem.CrestType = Convert.ToInt32(noBrackets.Split(',')[1]);
                    }

                    if (vendorItemList == null)
                    {
                        return NotFound();
                    }
                    return Ok(vendorItemList);
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
