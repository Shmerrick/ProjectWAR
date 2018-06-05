using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using ApocalypseAPI.Models;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace ApocalypseAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class CharacterController : ControllerBase
    {
        private readonly IDbConnectionService _db;
        private MySqlConnection dbConnection { get; set; }

        public CharacterController(IDbConnectionService db)
        {
            _db = db;
            dbConnection = new MySqlConnection(db.GetConnectionString());
        }

        [HttpGet]
        public List<Character> GetAll()
        {
            return dbConnection.Query<Character>("select CharacterId, Name from war_characters.characters").ToList();
        }

        [HttpGet("{id}", Name = "GetById")]
        public IActionResult GetById(long id)
        {
            var item = dbConnection.Query<Character>($"select CharacterId, Name from war_characters.characters where CharacterId = {id}");
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }
    }
}