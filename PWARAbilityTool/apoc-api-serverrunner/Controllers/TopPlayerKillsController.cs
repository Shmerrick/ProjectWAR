using Dapper;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;

namespace PWARAbilityTool.Controllers
{

    //endpoint for route to get players: TopPlayerKills
    public class TopPlayerKillsController : ApocApiController
    {
        public TopPlayerKillsController()
        {
        }

        [HttpGet]
        public IHttpActionResult GetAll()
        {
            Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress}");

            try
            {
                var query = $"select KillerCharacterId as CharacterId, count(killercharacterId) as KillCount, name as CharacterName " +
                    $"from war_world.kill_tracker kt, war_characters.characters c "+
                    $"where c.characterId = kt.KillerCharacterId "+
                    $"group by KillerCharacterId order by count(killercharacterId) desc limit 10";

                Logger.Debug(query);

                var killTrackList = DbConnection.Query<KillTracker>(query).ToList();
             
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
