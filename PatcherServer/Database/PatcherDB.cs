using Dapper.Contrib.Extensions;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace PatcherServer.Database
{
    public class PatcherDB
    {
        private IDbConnection _connection;

        public PatcherDB(IDbConnection connection)
        {
            _connection = connection;
        }

        public Task<IEnumerable<Patcher_File>> GetFiles()
        {
            return _connection.GetAllAsync<Patcher_File>();
        }

        public Task Add(Patcher_File file)
        {
            return _connection.InsertAsync(file);
        }

        public Task ClearFiles()
        {
            return _connection.DeleteAllAsync<Patcher_File>();
        }

        public Task Save(Patcher_File file)
        {
            return _connection.UpdateAsync(file);
        }
    }
}
