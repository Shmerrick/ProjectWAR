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
    [Route("api/ItemSet")]
    public class ItemSetController : Controller
    {
        private readonly IDbConnectionService _db;
        private MySqlConnection DbConnection { get; set; }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ItemSetController(IDbConnectionService db)
        {
            _db = db;
            DbConnection = new MySqlConnection(db.GetConnectionString());
        }

        [HttpGet]
        public List<ItemSet> GetAll()
        {
            Logger.Debug($"calling getall");
            try
            {
                return DbConnection.Query<ItemSet>( $"SELECT sets.Entry, sets.Name, sets.ClassId, c.ClassName, sets.ItemSetFullDescription FROM war_world.item_sets sets, war_world.classes c " +
                                                    $"where c.ClassId = sets.ClassId " +
                                                    $" order by sets.Name; ").ToList();
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
                var item = DbConnection.Query<ItemSet>(
                    $"SELECT sets.Entry, sets.Name, sets.ClassId, c.ClassName, sets.ItemSetFullDescription FROM war_world.item_sets sets, war_world.classes c " +
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
