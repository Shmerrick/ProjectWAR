using apoc_api.common.Managers;
using Dapper;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;

namespace PWARAbilityTool.Controllers
{

    
    public class ItemSearchController : ApocApiController
    {
        public IReferenceManager ReferenceManager { get; set; }
        public IEnrichService EnrichService { get; set; }


        public ItemSearchController()
        {
            EnrichService = new EnrichService();
            ReferenceManager = new ReferenceManager();
        }

        [HttpGet]
        public IHttpActionResult GetAll(int careerLine, int numberRecords = 0, int minRank = 0, int minRenown = 0, string nameFilter = "")
        {
            Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress} {careerLine} {numberRecords} {minRank} {minRenown} {nameFilter}");

            try
            {
                string nameFilterExpression = String.Empty;
                string careerFilterExpression = String.Empty;
                if (!String.IsNullOrEmpty(nameFilter))
                    nameFilterExpression = $" and Name like '%{nameFilter}%' ";

                if (numberRecords == 0)
                    numberRecords = 30;

                if (numberRecords > 30)
                    numberRecords = 30;

                if (careerLine != 0)
                    careerFilterExpression = $" and Career = {careerLine}";

                var values = Enum.GetValues(typeof(EquipmentSlotEnum)).Cast<int>().ToList();
                var slots = string.Join(",", values);
                var query = $" SELECT *, Stats as Statistics from war_world.Item_Infos ii " +
                            $" where SlotId in ({slots}) " +
                            $" and (Stats not like '0:0%') " +
                            $" and (Stats  <> '') " +
                            $" and (Stats IS NOT NULL) " +
                            $" {careerFilterExpression} " +
                            $" and MinRank >= {minRank} " +
                            $" and MinRenown >= {minRenown} " +
                            $" {nameFilterExpression} " +
                            $" order by MinRank, Name " +
                            $" limit {numberRecords}";

                Logger.Debug(query);

                var itemList = DbConnection.Query<Item>(query).ToList();

                foreach (var itemInfo in itemList)
                {
                    if (!EnrichService.EnrichItemWithStatistics(itemInfo, ReferenceManager.GetItemBonusList()))
                        return InternalServerError(new Exception("Could not enrich with statistics"));

                    EnrichService.EnrichItemWithClass(itemInfo, ReferenceManager.GetClassList());

                    EnrichService.EnrichItemWithRace(itemInfo, ReferenceManager.GetRaceList());

                    EnrichService.EnrichItemWithIcon(itemInfo);
                }
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
