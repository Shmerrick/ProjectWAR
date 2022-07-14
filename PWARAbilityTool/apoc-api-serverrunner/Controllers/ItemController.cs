using apoc_api.common.Managers;
using Dapper;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;

namespace PWARAbilityTool.Controllers
{

    
    public class ItemController : ApocApiController
    {
        public IReferenceManager ReferenceManager { get; set; }
        public IEnrichService EnrichService { get; set; }


        public ItemController()
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
                }
                return Ok(itemList);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }

        }


        [HttpGet]
        public IHttpActionResult GetAll(string nameFilter, int rarity)
        {
            Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress} {nameFilter} {rarity}");

            try
            {
                string nameFilterExpression = String.Empty;
                string rarityFilter = String.Empty;
                if (!String.IsNullOrEmpty(nameFilter))
                    nameFilterExpression = $" where Name like '%{nameFilter}%' ";

                if (rarity > 0)
                    rarityFilter = $" and Rarity={rarity}";

                var query = $" SELECT *, Stats as Statistics from war_world.Item_Infos ii " +
                            $" {nameFilterExpression} " +
                            $" {rarityFilter} " +
                            $" order by MinRank, Name ";

                Logger.Debug(query);

                var itemList = DbConnection.Query<Item>(query).ToList();

                foreach (var itemInfo in itemList)
                {
                    EnrichService.EnrichItemWithStatistics(itemInfo, ReferenceManager.GetItemBonusList());
                    EnrichService.EnrichItemWithClass(itemInfo, ReferenceManager.GetClassList());
                    EnrichService.EnrichItemWithRace(itemInfo, ReferenceManager.GetRaceList());

                }
                return Ok(itemList);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }

        }

        [HttpGet]
        
        public IHttpActionResult GetAll(string nameFilter)
        {
            Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress} {nameFilter}");

            try
            {
                string nameFilterExpression = String.Empty;
                string rarityFilter = String.Empty;
                if (!String.IsNullOrEmpty(nameFilter))
                    nameFilterExpression = $" where Name like '%{nameFilter}%' ";

                var query = $" SELECT *, Stats as Statistics from war_world.Item_Infos ii " +
                            $" {nameFilterExpression} " +
                            $" order by MinRank, Name " +
                            $" limit 50";

                Logger.Debug(query);

                var itemList = DbConnection.Query<Item>(query).ToList();

                foreach (var itemInfo in itemList)
                {
                    EnrichService.EnrichItemWithStatistics(itemInfo, ReferenceManager.GetItemBonusList());
                    EnrichService.EnrichItemWithClass(itemInfo, ReferenceManager.GetClassList());
                    EnrichService.EnrichItemWithRace(itemInfo, ReferenceManager.GetRaceList());

                }
                return Ok(itemList);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }

        }

        [Route("{id}")]
        public IHttpActionResult GetById(long id)
        {
            Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress} {id}");
            try
            {
                var item = DbConnection.Query<Item>(
                    $"SELECT *, Stats as Statistics from war_world.Item_Infos ii " +
                    $" where ii.Entry = {id}; ").ToList();



                if (item == null)
                {
                    return NotFound();
                }
                else
                {
                    if (item.Count() > 1)
                    {
                        Logger.Debug($"calling get by id {id} returned multiple rows");
                        return InternalServerError(new Exception($"calling get by id {id} returned multiple rows"));
                    }

                    var singleItem = item[0];

                    if (!EnrichService.EnrichItemWithStatistics(singleItem, ReferenceManager.GetItemBonusList()))
                        return InternalServerError(new Exception("Could not enrich with statistics"));

                    EnrichService.EnrichItemWithClass(singleItem, ReferenceManager.GetClassList());
                }
                return Ok(item);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }


    }
}
