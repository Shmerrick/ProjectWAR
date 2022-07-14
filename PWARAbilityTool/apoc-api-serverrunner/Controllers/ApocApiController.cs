using System.Web.Http;
using MySql.Data.MySqlClient;
using NLog;

namespace PWARAbilityTool.Controllers
{
    public class ApocApiController : ApiController
    {
        public MySqlConnection DbConnection { get; set; }
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ApocApiController()
        {
            DbConnection = new MySqlConnection(System.Configuration.ConfigurationManager.AppSettings["db-connection-string"]);
        }
    }
}
