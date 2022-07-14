using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Dapper;

namespace PWARAbilityTool.Controllers
{
    
    
    public class ZoneLockItemOptionController : ApocApiController
    {
        

        [HttpGet]
        public IHttpActionResult GetAll(int careerLine, int rrBand)
        {
            Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Career : {careerLine} RRBand : {rrBand}");
            
            try
            {
                var query =
                    $" SELECT zlio.rewardid as RewardId, zlio.rarity as Rarity, zlio.rrband as RRBand, zlio.class as Career, zlio.itemid as ItemId, zlio.itemcount as ItemCount, zlio.canawardduplicate as CanAwardDuplicate, ii.name as ItemName, ii.stats as Stats from war_world.Item_Infos ii, war_world.rvr_zone_lock_item_option zlio " +
                    $" where zlio.itemid = ii.entry " +
                    $" and ((zlio.class={careerLine} and zlio.rrband = {rrBand} ) or (zlio.class=0 and zlio.rrband = {rrBand}) or (zlio.rrband = 0 and zlio.class = {careerLine}) or (zlio.rrband = 0 and zlio.class = 0))" + 
                    $" order by Rarity ";

                Logger.Debug(query);

                var itemList =  DbConnection.Query<ZoneLockItemOption>(query).ToList();

                return Ok(itemList);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }

        }

       
       
    }
}
