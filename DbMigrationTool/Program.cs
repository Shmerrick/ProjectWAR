using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using NLog;

namespace DbMigrationTool
{
    class Program
    {
        /*
         * Scripts need to be marked as Embedded Resources
         */

        private static Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        static int Main(string[] args)
        {
            try
            {
                var connectionString = System.Configuration.ConfigurationManager.AppSettings["Evolve.ConnectionString"];
                _logger.Debug(connectionString);
                var cnx = new MySqlConnection(connectionString);
                cnx.Open();
                var evolve = new Evolve.Evolve(cnx, msg => _logger.Info(msg))
                {
                    Locations = new List<string> { System.Configuration.ConfigurationManager.AppSettings["Evolve.Locations"] },
                    IsEraseDisabled = true
                };
                evolve.Migrate();
                // All ok
                return 0;
            }
            catch (Exception ex)
            {
                _logger.Fatal($@"Database migration failed. {ex.Message}", ex.Message);
                return 1;
            }
        }
    }
}
