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
    [Route("api/Slot")]
    public class SlotController : Controller
    {
        private readonly IDbConnectionService _db;
        private MySqlConnection DbConnection { get; set; }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SlotController(IDbConnectionService db)
        {
            _db = db;
            DbConnection = new MySqlConnection(db.GetConnectionString());
        }

        [HttpGet]
        public List<KeyValuePair<int, string>> GetAll()
        {
            Logger.Debug($"calling getall");
            try
            {
                var result = new  List<KeyValuePair<int, string>>();

                var slotIds = Enum.GetValues(typeof(EquipmentSlotEnum)).Cast<int>();
                var slotNames = Enum.GetNames(typeof(EquipmentSlotEnum)).ToList();

                int i = 0;
                foreach (var slotId in slotIds)
                {
                    result.Add(new KeyValuePair<int, string>(slotId, slotNames[i]));
                    i++;
                }
                return result;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }

        }

       
    }
}
