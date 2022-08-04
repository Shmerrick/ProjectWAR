using MySql.Data.MySqlClient;

namespace PWARAbilityTool
{
    public interface IDatabaseConnection
    {
        MySqlConnection GetConnection();
    }
}
