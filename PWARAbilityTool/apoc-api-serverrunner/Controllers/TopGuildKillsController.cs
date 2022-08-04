using Dapper;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;

namespace PWARAbilityTool.Controllers
{

    
    public class TopGuildKillsController : ApocApiController
    {
        public TopGuildKillsController()
        {
        }

        [HttpGet]
        public IHttpActionResult GetAll()
        {
            Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress}");

            try
            {
                var query = $"select KillerGuildId as GuildId, count(KillerGuildId) as KillCount, g.name as GuildName "+
                "from war_world.kill_tracker kt,war_characters.guild_info g "+
                "where g.GuildId = kt.KillerGuildId "+
                "group by KillerGuildId order by count(KillerGuildId) desc limit 10" ;

                Logger.Debug(query);

                var killTrackList = DbConnection.Query<GuildKillTrack>(query).ToList();
             
                return Ok(killTrackList);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }

        }


       

    }
}
