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
    [Route("api/Class")]
    public class ClassController : Controller
    {
        private readonly IDbConnectionService _db;
        private MySqlConnection DbConnection { get; set; }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ClassController(IDbConnectionService db)
        {
            _db = db;
            DbConnection = new MySqlConnection(db.GetConnectionString());
        }

        [HttpGet]
        public List<Class> GetAll()
        {
            Logger.Debug($"calling getall");
            try
            {
                return DbConnection.Query<Class>( $"SELECT ClassId, ClassName from war_world.Classes ").ToList();
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }

        }

       
    }
}
