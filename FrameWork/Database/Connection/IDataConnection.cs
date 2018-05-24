using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace FrameWork.Database.Connection
{
    /// <summary>
    /// Called after a MySQL request.
    /// </summary>
    /// <param name="reader"></param>
    public delegate void QueryCallback(DbDataReader reader);

    /// <summary>
    /// Handles MySQL saving and loading.
    /// </summary>
    public interface IDataConnection
    {
        /// <summary> Retrieves connection from the pool. </summary>
        DbConnection GetConnection();
        int ExecuteNonQuery(string sqlcommand);

        bool IsSQLConnection { get; }
        /// <summary> Executes a blocking SELECT and returns the corresponding dataset. </summary>
        void ExecuteSelect(string sqlcommand, QueryCallback callback, IsolationLevel isolation);

        // Exécute un scale sur la DB
        object ExecuteScalar(string sqlcommand);

        void CheckOrCreateTable(System.Data.DataTable table);

        // Retourne le format des DateTimes
        string GetDBDateFormat();

        // Charge une table a partir de son DataSet
        void LoadDataSet(string tableName, DataSet dataSet);

        // Sauvegarde tous les changements effectué dans le dataset
        void SaveDataSet(string tableName, DataSet dataSet);

        // Affiche les erreur du dataset
        void PrintDatasetErrors(DataSet dataset);
    } 
}
