using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace PWARAbilityTool.Controllers
{

    [Route("api/characterprogression")]
    public class CharacterProgressionController : ApocApiController
    {


        [HttpGet]
        public IHttpActionResult GetAll(int dayCount, string requestType)
        {
            Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress} >= {dayCount} days");

            try
            {
                var result = new Dictionary<int, long>();
                var token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    var query = String.Empty;
                    query = $"SELECT * FROM war_characters.characters_value_hourly " +
                            $" WHERE timestamp >= DATE_SUB(NOW(), INTERVAL  {dayCount} DAY)";

                    if (requestType == "MONEY")
                        query += $" AND MONEY > 0 ";
                    
                    if (requestType == "RENOWN")
                        query += $" AND RENOWNRANK > 0 " ;

                    query += " ORDER BY timestamp DESC ";

                    Logger.Debug(query);

                    var metricsList = DbConnection.Query<CharacterProgression>(query).ToList();

                    var distinctCharcterList = metricsList.GroupBy(x => x.CharacterId).Select(grp => grp.First()).ToList();

                    if (requestType == "MONEY")
                    {

                        Parallel.ForEach(distinctCharcterList, character =>
                        {
                            var metricListForCharacter = metricsList.Where(x => x.CharacterId == character.CharacterId).ToList();
                            // check for no change, if so, skip
                            var difference = metricListForCharacter[0].Money - metricListForCharacter[metricListForCharacter.Count() - 1].Money;
                            if (difference != 0)
                            {
                                result.Add(character.CharacterId, difference);
                            }
                        });
                    }
                    if (requestType == "RENOWN")
                    {
                        var renownLevels = GetRenownLevels();

                        foreach (var character in distinctCharcterList)
                        {
                            if (character.Renown != 0)
                            {
                                var metricListForCharacter = metricsList.Where(x => x.CharacterId == character.CharacterId).ToList();
                                // check for no change, if so, skip
                                var difference =
                                    CalculateRenownSum(renownLevels, metricListForCharacter[0].RenownRank, metricListForCharacter[0].Renown) -
                                    CalculateRenownSum(renownLevels, metricListForCharacter[metricListForCharacter.Count() - 1].RenownRank,
                                        metricListForCharacter[metricListForCharacter.Count() - 1].Renown);
                                    
                                if (difference != 0)
                                {
                                    result.Add(character.CharacterId, difference);
                                }
                            }
                        }
                    }

                    return Ok(result);
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

        private long CalculateRenownSum(Dictionary<int, long> renownLevels, int renownRank, long renown)
        {
            return renownLevels[renownRank] + renown;
        }

        public Dictionary<int, long> GetRenownLevels()
        {
            var resultList = new Dictionary<int,long>();
            var query = String.Empty;
            query = $"SELECT * FROM war_world.renown_infos "+
                    $" ORDER BY level ";

            Logger.Debug(query);

            var result = DbConnection.Query(query).ToList();
            long sum = 0;
            foreach (var o in result)
            {
                sum = sum + o.Renown;
                resultList.Add(o.Level, sum);
            }

            return resultList;
        }
    }
}
