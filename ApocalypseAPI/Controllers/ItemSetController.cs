using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApocalypseAPI.Models;
using ApocalypseAPI.Shared;
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
        private MySqlConnection dbConnection { get; set; }
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public ItemSetController(IDbConnectionService db)
        {
            _db = db;
            dbConnection = new MySqlConnection(db.GetConnectionString());
        }

        [HttpGet]
        public List<ItemSet> GetAll()
        {
            try
            {
                _logger.Debug($"calling getall");
                return dbConnection.Query<ItemSet>( $"SELECT sets.Entry, sets.Name, sets.ClassId, c.ClassName, sets.ItemSetFullDescription FROM war_world.item_sets sets, war_world.classes c " +
                                                    $"where c.ClassId = sets.ClassId " +
                                                    $" order by sets.Name; ").ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        [HttpGet("{id}")]
        public IActionResult GetById(long id)
        {
            _logger.Debug($"calling get by id {id}");
            var item =  dbConnection.Query<ItemSet>($"SELECT sets.Entry, sets.Name, sets.ClassId, c.ClassName, sets.ItemSetFullDescription FROM war_world.item_sets sets, war_world.classes c " +
                                     $"where c.ClassId = sets.ClassId " +
                                     $" and sets.Entry = {id}; ").ToList();

            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }
    }
}
