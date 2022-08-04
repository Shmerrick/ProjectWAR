using Dapper;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;

namespace PWARAbilityTool.Controllers
{
    [Route("api/Character")]
    public class CharacterController : ApocApiController
    {
        [HttpGet]
        public IHttpActionResult GetAll(int accountId)
        {
            try
            {
                Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress} AccountId {accountId}");
                var token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    if (accountId == 0)
                        return BadRequest();

                    var query =
                        $"select c.CharacterId, c.Name as Name, cv.Level as CharacterLevel, cv.RenownRank as RenownLevel, c.CareerLine, c.Realm, cv.ZoneId, zi.Name as ZoneName " +
                        $"from war_characters.characters c, war_characters.characters_value cv, war_world.zone_infos zi, war_accounts.accounts a " +
                        $"where cv.CharacterId = c.CharacterId " +
                        $"and a.AccountId = c.AccountId " +
                        $"and a.AccountId = {accountId} " +
                        $"and zi.ZoneId=cv.ZoneId ";

                    var characterList = DbConnection.Query<Character>(query).ToList();

                    var careerLineList = new ClassController(Startup.iconService).LoadCareerLines();

                    foreach (var character in characterList)
                    {
                        foreach (var @class in careerLineList)
                        {
                            if (@class.ClassId == character.CareerLine)
                            {
                                character.CareerName = @class.ClassName;
                                character.CareerIcon = @class.ClassIcon;
                            }
                        }
                    }

                    return Ok(characterList);
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

        public IHttpActionResult GetById(long id)
        {
            Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Character Id {id}");
            try
            {
                var token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    var character = DbConnection.QuerySingle<Character>(
                        $"select c.CharacterId, c.Name as Name, cv.Level as CharacterLevel, cv.RenownRank as RenownLevel, c.CareerLine, c.Realm, cv.ZoneId, zi.Name as ZoneName " +
                        $"from war_characters.characters c, war_characters.characters_value cv, war_world.zone_infos zi " +
                        $"where cv.CharacterId = c.CharacterId " +
                        $"and zi.ZoneId=cv.ZoneId " +
                        $"and c.CharacterId = {id}");

                    if (character == null)
                    {
                        return NotFound();
                    }

                    var careerLineList = new ClassController(Startup.iconService).LoadCareerLines();

                    foreach (var @class in careerLineList)
                    {
                        if (@class.ClassId == character.CareerLine)
                        {
                            character.CareerName = @class.ClassName;
                            character.CareerIcon = @class.ClassIcon;
                        }
                    }

                    return Ok(character);
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