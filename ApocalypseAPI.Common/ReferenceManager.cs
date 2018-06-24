using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;
using NLog;

namespace ApocalypseAPI.Common
{
    public class ReferenceManager : IReferenceManager
    {
        private readonly IDbConnectionService _db;
        private MySqlConnection dbConnection { get; set; }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public List<ItemBonus> CachedItemBonuses { get; set; }

        public ReferenceManager(IDbConnectionService db)
        {
            _db = db;
            dbConnection = new MySqlConnection(db.GetConnectionString());
            CachedItemBonuses = new List<ItemBonus>();
        }


        public List<ItemBonus> GetItemBonusList()
        {
            try
            {
                if (!CachedItemBonuses.Any())
                {
                    dbConnection.Open();
                    CachedItemBonuses = dbConnection.Query<ItemBonus>($"select * from war_world.item_bonus").AsList();
                    return CachedItemBonuses;
                }
                else
                {
                    return CachedItemBonuses;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Logger.Error($"Exception : {e.Message}");
                return null;
            }
            finally
            {
                dbConnection.Close();
            }
        }

        
    }
}
