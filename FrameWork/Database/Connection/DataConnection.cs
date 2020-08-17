using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Policy;
using System.Text;

using System.Data.Common;
using System.Text.RegularExpressions;
using FrameWork.Database.Connection;

namespace FrameWork
{
    /// <summary>
    /// Handles MySQL saving and loading.
    /// </summary>
    public abstract class DataConnection:IDataConnection
    {
        protected readonly string _connString;
        public readonly string SchemaName;

        public abstract bool IsSQLConnection { get; }

        // Construction d'une connexion , Type(Mysql,ODBC,etc..) + paramètre de la connexion
        public DataConnection(ConnectionType connType, string connString, string schemaName)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            ConnectionType = connType;
            _connString = connString;
            SchemaName = schemaName;
            InitTypeMapping();
        }
        protected abstract void InitTypeMapping();

        public ConnectionType ConnectionType { get; }

        /// <summary>Escapes invalid characters in the string.</summary>
        public virtual string SQLEscape(string s)
        {
            s = s.Replace("\\", "\\\\");
            s = s.Replace("\"", "\\\"");
            s = s.Replace("'", "\\'");
            s = s.Replace("’", "\\’");

            return s;
        }

        /// <summary>Escapes invalid characters in the string.</summary>
        public virtual string Escape(string s)
        {

            s = s.Replace("\\", "\\\\");
            s = s.Replace("\"", "\\\"");
            s = s.Replace("'", "\\'");
            s = s.Replace("’", "\\’");

            return s;
        }

        /// <summary> Retrieves a MySQL connection from the pool. </summary>
        public abstract DbConnection GetConnection();

        /// <summary> Executes a non-blocking request (INSERT, DELETE, UPDATE)</summary>
        public abstract int ExecuteNonQuery(string sqlcommand);
    

        private static bool HandleException(Exception e)
        {
            bool ret = false;
            SocketException socketException = e.InnerException?.InnerException as SocketException ?? e.InnerException as SocketException;

            if (socketException == null)
                return false;

            // Handle socket exception. Error codes:
            // http://msdn2.microsoft.com/en-us/library/ms740668.aspx
            // 10052 = Network dropped connection on reset.
            // 10053 = Software caused connection abort.
            // 10054 = Connection reset by peer.
            // 10057 = Socket is not connected.
            // 10058 = Cannot send after socket shutdown.
            switch (socketException.ErrorCode)
            {
                case 10052:
                case 10053:
                case 10054:
                case 10057:
                case 10058:
                {
                    ret = true;
                    break;
                }
            }

            Log.Notice("DataConnection", $"Socket exception: ({socketException.ErrorCode}) {socketException.Message}; repeat: {ret}");

            return ret;
        }

        /// <summary> Executes a blocking SELECT and returns the corresponding dataset. </summary>
        public abstract void ExecuteSelect(string sqlcommand, QueryCallback callback, IsolationLevel isolation);

        // Exécute un scale sur la DB
        public abstract object ExecuteScalar(string sqlcommand);

        protected Dictionary<Type, string> TypeDescStrings = new Dictionary<Type, string>();


        protected virtual void AppendTypeString(Type type, StringBuilder bldr)
        {
            bldr.Append(TypeDescStrings.ContainsKey(type) ? TypeDescStrings[type] : "TEXT");
        }

        /// <summary>
        /// Verifies that the supplied table exists in the database and that its structure therein is matching.
        /// </summary>
        public virtual void CheckOrCreateTable(System.Data.DataTable table)
        {
            // Strings describing the columns.
            List<string> columnDefs = new List<string>();
            List<string> columnsToAdd = new List<string>();
            List<string> columnsToRemove = new List<string>();
            ArrayList databaseColumns = new ArrayList();
            ArrayList databaseColumnTypes = new ArrayList();
            List<string> databasePrimaryKeys = new List<string>();

            bool createNewTable = false;

            // Check for existing table.
            try
            {
                int tableCount;

                object count = ExecuteScalar("SELECT COUNT(*) FROM information_schema.tables WHERE TABLE_SCHEMA = \"" + SchemaName + "\" AND TABLE_NAME = \"" + table.TableName+"\"");
                if (count is long)
                    tableCount = (int) (long) count;
                else tableCount = (int) count;

                if (tableCount == 0)
                {
                    createNewTable = true;
                    Log.Info("DataConnection", "Table "+SchemaName+"."+table.TableName+" is missing and will be created...");
                }

                else
                {
                    ExecuteSelect("DESCRIBE `" + table.TableName + "`", (reader) =>
                    {
                        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                        Regex removeLength = new Regex(@"\((.+)\)");
                        while (reader.Read())
                        {
                            databaseColumns.Add(reader.GetString(0).ToLower());
                            databaseColumnTypes.Add(removeLength.Replace(reader.GetString(1).ToUpperInvariant(), ""));

                            // Remove columns which no longer exist in the DataTable.
                            if (!table.Columns.Contains(reader.GetString(0).ToLower()))
                                columnsToRemove.Add(reader.GetString(0).ToLower());

                            Log.Debug("DataConnection", reader.GetString(0).ToLower());
                        }

                        Log.Debug("DataConnection", databaseColumns.Count + " in table");

                    }, IsolationLevel.DEFAULT);

                    /*
                    // Add the string primary key.
                    if (!currentTableColumns.Contains((table.TableName + "_ID").ToLower()))
                    {
                        Log.Success("WAZA", "Creating Alter Primary Key");
                        ExecuteNonQuery("ALTER TABLE `" + table.TableName + "` ADD `"+table.TableName+"_ID`  VARCHAR(36) NOT NULL;");
                        ExecuteNonQuery("ALTER TABLE `" + table.TableName + "` ADD PRIMARY KEY (`" + table.TableName + "_ID`);");
                        ExecuteNonQuery("UPDATE TABLE `" + table.TableName + "` SET " + table.TableName + "_ID=UUID();");
                    }
                    */
                }
            }

            catch (Exception e)
            {
                Log.Debug("DataConnection", "CRITICAL: Failed either to check whether the table "+SchemaName+"."+table.TableName+" exists, or to describe it: "+e);
                Environment.Exit(0);
            }

            // Load primary keys from the in-memory version of the table.
            HashSet<string> primaryKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (DataColumn pk in table.PrimaryKey)
                primaryKeys.Add(pk.ColumnName);

            long incrementSeed = 0;

            StringBuilder colBuilder = new StringBuilder(24);

            // Build information about the existing columns.
            // This will be used both to create a new table if required, 
            // and to add any columns that are missing from an existing table.
            for (int i = 0; i < table.Columns.Count; i++)
            {
                Type systype = table.Columns[i].DataType;

                colBuilder.Clear();

                colBuilder.Append("`");
                colBuilder.Append(table.Columns[i].ColumnName);
                colBuilder.Append("` ");

                //alterRemoveColumnDefs.Remove(column);

                // Append the type of the column
                if (systype == typeof(string))
                {
                    if (primaryKeys.Contains(table.Columns[i].ColumnName) ||
                        table.Columns[i].ExtendedProperties.ContainsKey("INDEX") ||
                        table.Columns[i].ExtendedProperties.ContainsKey("VARCHAR") ||
                        table.Columns[i].Unique)
                    {
                        if (table.Columns[i].ExtendedProperties.ContainsKey("VARCHAR"))
                        {
                            colBuilder.Append("VARCHAR(");
                            colBuilder.Append(table.Columns[i].ExtendedProperties["VARCHAR"]);
                            colBuilder.Append(")");
                        }
                        else
                            colBuilder.Append("VARCHAR(255)");
                    }
                    else
                        colBuilder.Append("TEXT");
                }

                else AppendTypeString(systype, colBuilder);

                // Append any modifiers
                if (!table.Columns[i].AllowDBNull)
                    colBuilder.Append(" NOT NULL");

                if (table.Columns[i].AutoIncrement)
                {
                    colBuilder.Append(" AUTO_INCREMENT");
                    incrementSeed = table.Columns[i].AutoIncrementSeed;
                }

                columnDefs.Add(colBuilder.ToString());

                // The current column exists in the datatable but not in the database, so mark it as pending creation
                if (databaseColumns.Count > 0 && !databaseColumns.Contains(table.Columns[i].ColumnName.ToLower()))
                {
                    Log.Debug("DataConnection", "Add alteration " + table.Columns[i].ColumnName.ToLower());
                    columnsToAdd.Add(colBuilder.ToString());
                }

                // Check that the DB type matches the data type
                else if(systype != typeof(string) && systype != typeof(bool) && systype != typeof(float) && TypeDescStrings.ContainsKey(systype))
                {
                    int colIndex = databaseColumns.IndexOf(table.Columns[i].ColumnName.ToLower());

                    if (colIndex == -1)
                        Log.Error("DataConnection", $"Column index out of range for {table.TableName}, {table.Columns[i].ColumnName}");

                    else if (databaseColumnTypes[colIndex].ToString() != TypeDescStrings[systype] && !table.Columns[i].AutoIncrement) //To remove errors about type mistmatch for guids
                    {
                        Log.Error($"Table {table.TableName} column {databaseColumns[colIndex]}",  $"Type mismatch ({databaseColumnTypes[colIndex]} in DB - {TypeDescStrings[systype]} in emulator)");
                    }
                }
            }

            if (createNewTable)
            {
                #region Create New Table
                string columndef = string.Join(", ", columnDefs.ToArray());

                // create primary keys
                if (table.PrimaryKey.Length > 0)
                {
                    columndef += ", PRIMARY KEY (";
                    bool first = true;
                    foreach (DataColumn pk in table.PrimaryKey)
                    {
                        if (!first)
                            columndef += ", ";
                        else
                            first = false;

                        columndef += "`" + pk.ColumnName + "`";
                    }
                    columndef += ")";
                }

                // Index Unique			
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    if (table.Columns[i].Unique && !primaryKeys.Contains(table.Columns[i].ColumnName))
                        columndef += ", UNIQUE INDEX (`" + table.Columns[i].ColumnName + "`)";
                }

                // Index
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    if (table.Columns[i].ExtendedProperties.ContainsKey("INDEX")
                        && !primaryKeys.Contains(table.Columns[i].ColumnName)
                        && !table.Columns[i].Unique)
                    {
                        columndef += ", INDEX (`" + table.Columns[i].ColumnName + "`)";
                    }
                }
                string command = "CREATE TABLE IF NOT EXISTS `" + table.TableName + "` (" + columndef + ") AUTO_INCREMENT=" + incrementSeed;

                try
                {
                    Log.Debug("DataConnection", command);

                    ExecuteNonQuery(command);
                }
                catch (Exception e)
                {
                    Log.Error("DataConnection", "Error at creating table " + table.TableName + e);
                }
                #endregion
            }

            else
            {
                bool primaryKeysDirty = false;
                try
                {
                    // Ensure that the PKs in the database match.
                    ExecuteSelect("SHOW INDEX FROM `" + table.TableName + "`",  (reader) =>
                    {
                        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                        while (reader.Read())
                        {
                            if (reader.GetString(2).Equals("PRIMARY"))
                                databasePrimaryKeys.Add(reader.GetString(4));
                        }

                    }, IsolationLevel.DEFAULT);

                    // Check for changed primary keys, so we can update them.

                    if (primaryKeys.Count != databasePrimaryKeys.Count)
                        primaryKeysDirty = true;
                    else
                    {
                        foreach (string dbpk in databasePrimaryKeys)
                        {
                            if (!primaryKeys.Contains(dbpk))
                            {
                                primaryKeysDirty = true;
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Debug("DataConnection", "Unable to check primary keys: "+ e);
                }


                if (columnsToAdd.Count > 0)
                    AddColumnsToDatabase(table.TableName, columnsToAdd);

                if (primaryKeysDirty)
                    UpdatePrimaryKeys(table.TableName, primaryKeys.ToList(), databasePrimaryKeys.Count > 0);

                if (columnsToRemove.Count > 0)
                    RemoveColumnsFromDatabase(table.TableName, columnsToRemove);
            }
        }

        private void AddColumnsToDatabase(string tableName, List<string> columnDefs)
        {
            string columndef = string.Join(", ", columnDefs.ToArray());
            string alterTable = "ALTER TABLE `" + tableName + "` ADD (" + columndef + ")";
            try
            {
                Log.Debug("DataConnection", alterTable);
                ExecuteNonQuery(alterTable);
                Log.Success(tableName, "Added columns: "+columndef);
            }
            catch (Exception e)
            {
                Log.Error("DataConnection", "Alteration table error " + tableName + e);
            }
        }

        private void RemoveColumnsFromDatabase(string tableName, List<string> columnDefs)
        {
            foreach (string column in columnDefs)
            {
                string alterTable = "ALTER TABLE `" + tableName + "` DROP COLUMN " + column + "";
                try
                {
                    Log.Debug("DataConnection", alterTable);
                    ExecuteNonQuery(alterTable);
                    Log.Success(tableName, "Removed column: " + column);
                }
                catch (Exception e)
                {
                    Log.Error("DataConnection", "Alteration table error " + tableName + e);
                }
            }
        }

        private void UpdatePrimaryKeys(string tableName, List<string> primaryKeys, bool dropFirst)
        {
            string keyDefs = '`' + string.Join("`, `", primaryKeys.ToArray()) + '`';

            string alterTable;

            if (dropFirst)
                alterTable = "ALTER TABLE `" + tableName + "` DROP KEY `PRIMARY`, ADD PRIMARY KEY (" + keyDefs + ")";
            else
                alterTable = "ALTER TABLE `" + tableName + "` ADD PRIMARY KEY (" + keyDefs + ")";

            try
            {
                Log.Debug("DataConnection", alterTable);
                ExecuteNonQuery(alterTable);
                Log.Success(tableName, "Changed primary keys to " + keyDefs);
            }
            catch (Exception e)
            {
                Log.Error("DataConnection", "Alteration table error " + tableName + e);
            }
        }

        // Retourne le format des DateTimes
        public virtual string GetDBDateFormat()
        {
            switch (ConnectionType)
            {
                case ConnectionType.DATABASE_MYSQL:
                    return "yyyy-MM-dd HH:mm:ss";
            }

            return "yyyy-MM-dd HH:mm:ss";
        }

        // Charge une table a partir de son DataSet
        public virtual void LoadDataSet(string tableName, DataSet dataSet)
        {
            dataSet.Clear();
            switch (ConnectionType)
            {
                case ConnectionType.DATABASE_MSSQL:
                    {
                        try
                        {
                            var conn = new SqlConnection(_connString);
                            var adapter = new SqlDataAdapter("SELECT * from " + tableName, conn);

                            adapter.Fill(dataSet.Tables[tableName]);
                        }
                        catch (Exception ex)
                        {
                            throw new DatabaseException("Can not load table ", ex);
                        }

                        break;
                    }
                case ConnectionType.DATABASE_ODBC:
                    {
                        try
                        {
                            var conn = new OdbcConnection(_connString);
                            var adapter = new OdbcDataAdapter("SELECT * from " + tableName, conn);

                            adapter.Fill(dataSet.Tables[tableName]);
                        }
                        catch (Exception ex)
                        {
                            throw new DatabaseException("Can not load table ", ex);
                        }

                        break;
                    }
                case ConnectionType.DATABASE_OLEDB:
                    {
                        try
                        {
                            var conn = new OleDbConnection(_connString);
                            var adapter = new OleDbDataAdapter("SELECT * from " + tableName, conn);

                            adapter.Fill(dataSet.Tables[tableName]);
                        }
                        catch (Exception ex)
                        {
                            throw new DatabaseException("Can not load table ", ex);
                        }
                        break;
                    }
            }
        }

        // Sauvegarde tous les changements effectué dans le dataset
        public virtual void SaveDataSet(string tableName, DataSet dataSet)
        {
            if (dataSet.HasChanges() == false)
                return;

            switch (ConnectionType)
            {
                case ConnectionType.DATABASE_MSSQL:
                    {
                        try
                        {
                            var conn = new SqlConnection(_connString);
                            var adapter = new SqlDataAdapter("SELECT * from " + tableName, conn);
                            var builder = new SqlCommandBuilder(adapter);

                            adapter.DeleteCommand = builder.GetDeleteCommand();
                            adapter.UpdateCommand = builder.GetUpdateCommand();
                            adapter.InsertCommand = builder.GetInsertCommand();

                            lock (dataSet) // lock dataset to prevent changes to it
                            {
                                adapter.ContinueUpdateOnError = true;
                                DataSet changes = dataSet.GetChanges();
                                adapter.Update(changes, tableName);
                                PrintDatasetErrors(changes);
                                dataSet.AcceptChanges();
                            }

                            conn.Close();
                        }
                        catch (Exception ex)
                        {
                            throw new DatabaseException("Can not save table " + tableName, ex);
                        }

                        break;
                    }
                case ConnectionType.DATABASE_ODBC:
                    {
                        try
                        {
                            var conn = new OdbcConnection(_connString);
                            var adapter = new OdbcDataAdapter("SELECT * from " + tableName, conn);
                            var builder = new OdbcCommandBuilder(adapter);

                            adapter.DeleteCommand = builder.GetDeleteCommand();
                            adapter.UpdateCommand = builder.GetUpdateCommand();
                            adapter.InsertCommand = builder.GetInsertCommand();

                            DataSet changes;
                            lock (dataSet) // lock dataset to prevent changes to it
                            {
                                adapter.ContinueUpdateOnError = true;
                                changes = dataSet.GetChanges();
                                adapter.Update(changes, tableName);
                                dataSet.AcceptChanges();
                            }

                            PrintDatasetErrors(changes);

                            conn.Close();
                        }
                        catch (Exception ex)
                        {
                            throw new DatabaseException("Can not save table ", ex);
                        }

                        break;
                    }
                case ConnectionType.DATABASE_MYSQL:
                    {
                        return;
                    }
                case ConnectionType.DATABASE_OLEDB:
                    {
                        try
                        {
                            var conn = new OleDbConnection(_connString);
                            var adapter = new OleDbDataAdapter("SELECT * from " + tableName, conn);
                            var builder = new OleDbCommandBuilder(adapter);

                            adapter.DeleteCommand = builder.GetDeleteCommand();
                            adapter.UpdateCommand = builder.GetUpdateCommand();
                            adapter.InsertCommand = builder.GetInsertCommand();

                            DataSet changes;
                            lock (dataSet) // lock dataset to prevent changes to it
                            {
                                adapter.ContinueUpdateOnError = true;
                                changes = dataSet.GetChanges();
                                adapter.Update(changes, tableName);
                                dataSet.AcceptChanges();
                            }

                            PrintDatasetErrors(changes);

                            conn.Close();
                        }
                        catch (Exception ex)
                        {
                            throw new DatabaseException("Can not save table", ex);
                        }
                        break;
                    }
            }
        }

        // Affiche les erreur du dataset
        public void PrintDatasetErrors(DataSet dataset)
        {
            if (dataset.HasErrors)
            {
                foreach (System.Data.DataTable table in dataset.Tables)
                {
                    if (table.HasErrors)
                    {
                        foreach (DataRow row in table.Rows)
                        {
                            if (row.HasErrors && row.RowState == DataRowState.Deleted)
                            {
                                Log.Error("DataConnection","Error deleting row in table " + table.TableName + ": " + row.RowError);

                                var sb = new StringBuilder();
                                foreach (DataColumn col in table.Columns)
                                {
                                    sb.Append(col.ColumnName + "=" + row[col, DataRowVersion.Original] + " ");
                                }

                                Log.Error("DataConnection", sb.ToString());
                            }
                            else if (row.HasErrors)
                            {
                                Log.Error("DataConnection", "Error updating table " + table.TableName + ": " + row.RowError + row.GetColumnsInError());

                                var sb = new StringBuilder();
                                foreach (DataColumn col in table.Columns)
                                {
                                    sb.Append(col.ColumnName + "=" + row[col] + " ");
                                }

                                Log.Error("DataConnection", sb.ToString());

                                sb = new StringBuilder("Affected columns: ");
                                foreach (DataColumn col in row.GetColumnsInError())
                                {
                                    sb.Append(col.ColumnName + " ");
                                }

                                Log.Error("DataConnection", sb.ToString());
                            }
                        }
                    }
                }
            }
        }

    }
}