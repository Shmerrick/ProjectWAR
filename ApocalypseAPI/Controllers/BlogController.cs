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
    [Route("api/Blog")]
    public class BlogController : Controller
    {
        private readonly IDbConnectionService _db;
        private MySqlConnection DbConnection { get; set; }
        public ITimeTokenManager TokenManager { get; }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public BlogController(IDbConnectionService db, ITimeTokenManager tokenManager)
        {
            _db = db;
            TokenManager = tokenManager;
            DbConnection = new MySqlConnection(db.GetConnectionString());
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                Logger.Debug($"calling getall");
                var token = Request.Headers["auth-token"];
                if (TokenManager.IsValidToken(token))
                {
                    /*
                     * CREATE TABLE `war_tools`.`blogs` (
  `BlogId` INT NOT NULL AUTO_INCREMENT,
  `BlogTimestamp` DATETIME(6) NULL,
  `BlogText` MEDIUMTEXT NULL,
  `BlogUrl` VARCHAR(200) NULL,
  PRIMARY KEY (`BlogId`));
  */

                    return Ok(DbConnection.Query<Blog>($"select top 10 BlogId, BlogTimestamp, BlogText, BlogUrl " +
                                                         $"from war_tools.blogs order by BlogId desc ").ToList());
                }
                else
                {
                    return Unauthorized();
                }
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
                var token = Request.Headers["auth-token"];
                if (TokenManager.IsValidToken(token))
                {
                    var item = DbConnection.Query<Blog>($"select BlogId, BlogTimestamp, BlogText, BlogUrl " +
                                                        $"from war_tools.blogs where BlogId = {id}");
                    if (item == null)
                    {
                        return NotFound();
                    }
                    return Ok(item);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }
        }
    }
}