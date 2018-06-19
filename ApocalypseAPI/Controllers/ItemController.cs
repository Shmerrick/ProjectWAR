using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApocalypseAPI.Common;
using ApocalypseAPI.Models;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using NLog;

namespace ApocalypseAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/Item")]
    public class ItemController : Controller
    {
        public IReferenceManager ReferenceManager { get; set; }
        private readonly IDbConnectionService _db;
        private MySqlConnection DbConnection { get; set; }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ItemController(IDbConnectionService db, IReferenceManager referenceManager)
        {
            ReferenceManager = referenceManager;
            _db = db;
            DbConnection = new MySqlConnection(db.GetConnectionString());
        }

        [HttpGet]
        public List<Item> GetAll(int careerLine, int minRank = 0, int minRenown = 0, string nameFilter = "")
        {
            Logger.Debug($"calling getall");

            try
            {
                string nameFilterExpression = String.Empty;
                if (!String.IsNullOrEmpty(nameFilter))
                    nameFilterExpression = $" and Name like '%{nameFilter}%' ";


                var slots = string.Join(',', Enum.GetValues(typeof(EquipmentSlotEnum)).Cast<int>());
                var query = $" SELECT *, Stats as Statistics from war_world.Item_Infos ii " +
                            $" where SlotId in ({slots}) " +
                            $" and (Stats not like '0:0%') " +
                            $" and (Stats  <> '') " +
                            $" and (Stats IS NOT NULL) " +
                            $" and Career = {careerLine} " +
                            $" and MinRank >= {minRank} " +
                            $" and MinRenown >= {minRenown} " +
                            $" {nameFilterExpression} " +
                            $" order by MinRank, Name";

                Logger.Debug(query);

                var itemList =  DbConnection.Query<Item>(query).ToList();

                foreach (var itemInfo in itemList)
                {
                    var individualStatItems = itemInfo.Statistics.Split(';');
                    foreach (var item in individualStatItems)
                    {
                        if ((item == "0:0") || (item == "") || (item.StartsWith("0:0")))
                            continue;
                        try
                        {
                            itemInfo.StatsList.Add(new ItemInfoStats(item, ReferenceManager.GetItemBonusList()));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                        
                    }
                }
                return itemList;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }

        }

        [HttpGet("{id}")]
        public IActionResult GetById(long id)
        {
            Logger.Debug($"calling get by id {id}");
            try
            {
                var item = DbConnection.Query<Item>(
                    $"SELECT sets.Entry, sets.Name, sets.ClassId, c.ClassName, sets.ItemFullDescription FROM war_world.item_sets sets, war_world.classes c " +
                    $"where c.ClassId = sets.ClassId " +
                    $" and sets.Entry = {id}; ").ToList();

                if (item == null)
                {
                    return NotFound();
                }
                return Ok(item);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }
        }
    }
}
