using System;
using System.Collections.Generic;
using System.Configuration;
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
    [Route("api/Character")]
    public class CharacterController : ControllerBase
    {
        private readonly IDbConnectionService _db;
        private MySqlConnection dbConnection { get; set; }
        public ITimeTokenManager TokenManager { get; }

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public CharacterController(IDbConnectionService db, ITimeTokenManager tokenManager)
        {
            _db = db;
            TokenManager = tokenManager;
            dbConnection = new MySqlConnection(db.GetConnectionString());
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                _logger.Debug($"calling getall");
                var token = Request.Headers["auth-token"];
                if (TokenManager.IsValidToken(token))
                {


                    return Ok(dbConnection.Query<Character>($"select c.CharacterId, c.Name as Name, cv.Level as CharacterLevel, cv.RenownRank as RenownLevel, c.Career, c.Realm, cv.ZoneId, zi.Name as ZoneName " +
                                                         $"from war_characters.characters c, war_characters.characters_value cv, war_world.zone_infos zi " +
                                                         $"where cv.CharacterId = c.CharacterId " +
                                                         $"and zi.ZoneId=cv.ZoneId ").ToList());
                }
                else
                {
                    return Unauthorized();
                }
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
            var item = dbConnection.Query<Character>($"select c.CharacterId, c.Name as Name, cv.Level as CharacterLevel, cv.RenownRank as RenownLevel, c.Career, c.Realm, cv.ZoneId, zi.Name as ZoneName " +
                                                     $"from war_characters.characters c, war_characters.characters_value cv, war_world.zone_infos zi " +
                                                     $"where cv.CharacterId = c.CharacterId " +
                                                     $"and zi.ZoneId=cv.ZoneId " +
                                                     $"and c.CharacterId = {id}");
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }
    }
}