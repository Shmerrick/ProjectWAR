using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApocalypseAPI.Shared
{
    public class DbConnectionService : IDbConnectionService
    {
        private string connectionString;
        public DbConnectionService(string connString)
        {
            this.connectionString = connString;
        }

        public string GetConnectionString()
        {
            return connectionString;
        }
    }

    public interface IDbConnectionService 
    {
        string GetConnectionString();
    }
}
