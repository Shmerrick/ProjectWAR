using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using apoc_api.common.Managers;
using Dapper;

namespace PWARAbilityTool.Controllers
{
    [Route("api/CharacterItem")]
    public class CharacterItemController : ApocApiController
    {
        public IReferenceManager ReferenceManager { get; set; }
        public IEnrichService EnrichService { get; set; }


        public CharacterItemController()
        {
            EnrichService = new EnrichService();
            ReferenceManager = new ReferenceManager();
        }


        public IHttpActionResult GetById(long id)
        {
            Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Character Item by Character Id {id}");
            
            try
            {
                var token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {

                    var query =
                        $"select ii.Entry, ii.Name, ii.Stats, ii.MaxStack, ii.Armor, ii.SpellId, ii.Dps, ii.Speed, ii.MinRank, ii.MinRenown, ii.ModelId " +
                        $"from war_world.item_infos ii " +
                        $"where ii.Entry in " +
                        $"(select ci.Entry from war_characters.characters c, war_characters.characters_items ci " +
                        $"where ci.characterId = c.characterId " +
                        $"and c.CharacterId = {id})";


                    var itemList = DbConnection.Query<Item>(query).ToList();

                    foreach (var itemInfo in itemList)
                    {
                        EnrichService.EnrichItemWithStatistics(itemInfo, ReferenceManager.GetItemBonusList());
                        
                        EnrichService.EnrichItemWithClass(itemInfo, ReferenceManager.GetClassList());

                        EnrichService.EnrichItemWithRace(itemInfo, ReferenceManager.GetRaceList());

                        EnrichService.EnrichItemWithIcon(itemInfo);
                    }
                    return Ok(itemList);

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