using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Dapper;

namespace PWARAbilityTool.Controllers
{
    
    [Route("api/rvrmetrics")]
    public class RVRMetricsController : ApocApiController
    {


        [HttpGet]
        public IHttpActionResult GetAll(int dayCount)
        {
            Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress} >= {dayCount} days");
            
            try
            {
                var token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    var query = String.Empty;
                    if (dayCount==0)
                    {
                        query = $"SELECT DATE_FORMAT(timestamp, '%Y-%m-%d %H:00:00') AS Hour," +
                                $" MAX(TotalPlayerCount) AS Logins" +
                                $" FROM war_world.rvr_metrics " +
                                $" WHERE  tier = 4 " +
                                $" GROUP BY DATE_FORMAT(timestamp, '%Y-%m-%d %H:00:00') " +
                                $" ORDER BY timestamp DESC "+
                                $" limit 10";

                    }
                    else
                    {
                        query = $"SELECT DATE_FORMAT(timestamp, '%Y-%m-%d %H:00:00') AS Hour," +
                                $" MAX(TotalPlayerCount) AS Logins" +
                                $" FROM war_world.rvr_metrics " +
                                $" WHERE timestamp >= DATE_SUB(NOW(), INTERVAL {dayCount} DAY) " +
                                $" AND tier = 4 " +
                                $" GROUP BY DATE_FORMAT(timestamp, '%Y-%m-%d %H:00:00') " +
                                $" ORDER BY timestamp DESC ";
                    }

                    Logger.Debug(query);

                    var metricsList = DbConnection.Query<HourlyPopulation>(query).ToList();

                    return Ok(metricsList);
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
