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
using FrameWork.Database.Connection;
using Microsoft.SqlServer.Server;

namespace FrameWork
{
    /// <summary>
    /// Handles Sql saving and loading.
    /// </summary>
    public class SqlDataConnection : DataConnection
    {
        public class Column : Attribute
        {
            public string Name { get; set; }
            public bool IsPrimaryKey { get; set; }
            public bool IsForeignKey { get; set; }
            public bool IsDbGenerated { get; set; }
            public Column(string name = null, bool isPrimaryKey = false, bool isForeignKey = false, bool isDbGenerated = true)
            {
                Name = name;
                IsPrimaryKey = isPrimaryKey;
                IsForeignKey = isForeignKey;
                IsDbGenerated = isDbGenerated;
            }
        }

        public class ColumnSchema
        {
            [DataElement]
            public string COLUMN_NAME { get; set; }
            [DataElement]
            public int ORDINAL_POSITION { get; set; }
            [DataElement]
            public string DATA_TYPE { get; set; }
            [DataElement]
            public string CHARACTER_MAX_LENGTH { get; set; }

            public override string ToString()
            {
                return COLUMN_NAME;
            }

        }

        public class TableSchema
        {
            [DataElement]
            public string TABLE_CATALOG { get; set; }
            [DataElement]
            public string TABLE_SCHEMA { get; set; }
            [DataElement]
            public string TABLE_NAME { get; set; }

            [DataElement]
            public string COLUMN_NAME { get; set; }
            [DataElement]
            public int ORDINAL_POSITION { get; set; }
            [DataElement]
            public string DATA_TYPE { get; set; }
            [DataElement]
            public string CHARACTER_MAX_LENGTH { get; set; }

            public Dictionary<string, ColumnSchema> Columns = new Dictionary<string, ColumnSchema>();

            public override string ToString()
            {
                return TABLE_NAME;
            }
        }

        public Dictionary<string, TableSchema> Schemas = new Dictionary<string, TableSchema>();

        public class CLRMap
        {
            public class Column
            {
                public string SqlName;
                public string ClrName;
                public bool ChangeType;
                public Type Type;
                public int Length;
                public bool Nullable;
                public bool IsPrimaryKey;
                public bool IsForeignKey;
                public bool IsDbGenerated;
                public override string ToString()
                {
                    return ClrName;
                }
            }

            public object Accessor;
            public TableSchema TableSchema;
            public Dictionary<string, Column> ClrToSqlFields = new Dictionary<string, Column>();
            public Dictionary<string, Column> SqlToClrFields = new Dictionary<string, Column>();
            public Column PrimaryKeyField;
            public string TableName;
        }

        private Dictionary<Type, CLRMap> _typeMap = new Dictionary<Type, CLRMap>();

        public override bool IsSQLConnection
        {
            get
            {
                return true;
            }
        }

        // Construction d'une connexion , Type(Sql,ODBC,etc..) + paramètre de la connexion
        public SqlDataConnection(string connString, string schemaName) : base(ConnectionType.DATABASE_MSSQL, connString, schemaName)
        {
            LoadSchemas();
        }

        public CLRMap GetMapping(Type type)
        {
            CLRMap map = null;
            lock (_typeMap)
            {
                if (!_typeMap.ContainsKey(type))
                {
                    _typeMap[type] = new CLRMap();
                }

                map = _typeMap[type];
            }

            if (map.Accessor == null)
            {
                map.Accessor = type;
                map.TableName = type.Name;
                var name = (DataTable)type.GetCustomAttributes(typeof(DataTable), true).FirstOrDefault();
                if (name != null && name.TableName != null)
                    map.TableName = name.TableName;

                foreach (var prop in type.GetProperties(BindingFlags.FlattenHierarchy
                                                           | BindingFlags.Public
                                                           | BindingFlags.Instance))
                {

                    string colName = prop.Name;

                  
                    var col = (DataElement)prop.GetCustomAttributes(typeof(DataElement),true).FirstOrDefault();
                    if (col != null)
                    {

                        map.ClrToSqlFields[prop.Name] = new CLRMap.Column()
                        {
                            SqlName = colName,
                            ClrName = prop.Name,
                            Type = prop.PropertyType,
                        };
                        map.SqlToClrFields[colName] = map.ClrToSqlFields[prop.Name]; 
                    }

                    var pk = (PrimaryKey)prop.GetCustomAttributes(typeof(PrimaryKey), true).FirstOrDefault();
                    if (pk != null)
                    {
                        map.ClrToSqlFields[prop.Name] = new CLRMap.Column()
                        {
                            SqlName = colName,
                            ClrName = prop.Name,
                            Type = prop.PropertyType,
                            IsPrimaryKey = true
                        };
                        map.SqlToClrFields[colName] = map.ClrToSqlFields[prop.Name];
                        map.PrimaryKeyField = map.SqlToClrFields[colName];
                    }
                }
            }

            if (map.TableSchema == null)
            {

                if (Schemas.ContainsKey(map.TableName.ToLower()))
                {
                    map.TableSchema = Schemas[map.TableName.ToLower()];

                    //remove any mapped column that does not have same type
                    foreach (var col in map.TableSchema.Columns.Values)
                    {
                        if (map.SqlToClrFields.ContainsKey(col.COLUMN_NAME))
                        {
                            SqlDbType t = SqlDbType.BigInt;
                            if (Enum.TryParse<SqlDbType>(col.DATA_TYPE, out t))
                            {
                                if (GetClrType(t) != map.ClrToSqlFields[col.COLUMN_NAME].Type)
                                {
                                    map.ClrToSqlFields[col.COLUMN_NAME].ChangeType = true;
                                }
                            }
                        }
                    }

                    //foreach (var col in map.ClrToSqlFields.Values.ToList())
                    //{
                    //    if (!map.TableSchema.Columns.ContainsKey(col.SqlName))
                    //    {
                    //        map.ClrToSqlFields.Remove(col.SqlName);
                    //    }
                    //}
                }
            }

            return map;
        }

        private object[] BuildParams(params object[] param)
        {
            if (param != null && param.Length > 0)
            {
                object[] p = new object[param.Length];
                for (int i = 0; i < param.Length; i++)
                {
                    if (param[i] != null)
                    {
                        if (param[i].GetType() == typeof(string) ||
                            param[i].GetType() == typeof(Guid) ||
                            param[i].GetType() == typeof(DateTime))
                            p[i] = "'" + param[i].ToString().Replace("'", "''") + "'";
                        else if (param[i] == null)
                            p[i] = "NULL";
                    }
                    else
                        p[i] = "NULL";
                }
                return p;
            }
            return param;

        }
        private void LoadSchemas()
        {
            Schemas.Clear();
            var table = new System.Data.DataTable();

            using (var cmd = new SqlCommand("SELECT * FROM INFORMATION_SCHEMA.COLUMNS", (SqlConnection)GetConnection()))
            {
                var da = new SqlDataAdapter(cmd);
                da.Fill(table);
            }

            foreach (DataRow row in table.Rows)
            {
                var tableName = row["TABLE_SCHEMA"].ToString().ToLower() + "." + row["TABLE_NAME"].ToString().ToLower();
                if (!Schemas.ContainsKey(tableName))
                    Schemas[tableName] = new TableSchema()
                    {
                        TABLE_CATALOG = row["TABLE_CATALOG"].ToString(),
                        TABLE_SCHEMA = row["TABLE_SCHEMA"].ToString(),
                        TABLE_NAME = row["TABLE_NAME"].ToString()
                    };

                Schemas[tableName].Columns[row["COLUMN_NAME"].ToString().ToLower()] = new ColumnSchema()
                {
                    COLUMN_NAME = row["COLUMN_NAME"].ToString(),
                    DATA_TYPE = row["DATA_TYPE"].ToString(),
                    CHARACTER_MAX_LENGTH = row["CHARACTER_MAXIMUM_LENGTH"].ToString(),
                    ORDINAL_POSITION = (int)row["ORDINAL_POSITION"]
                };
            }
        }


        public static Type GetClrType(SqlDbType sqlType)
        {
            switch (sqlType)
            {
                case SqlDbType.BigInt:
                    return typeof(long?);

                case SqlDbType.Binary:
                case SqlDbType.Image:
                case SqlDbType.Timestamp:
                case SqlDbType.VarBinary:
                    return typeof(byte[]);

                case SqlDbType.Bit:
                    return typeof(bool?);

                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.VarChar:
                case SqlDbType.Xml:
                    return typeof(string);

                case SqlDbType.DateTime:
                case SqlDbType.SmallDateTime:
                case SqlDbType.Date:
                case SqlDbType.Time:
                case SqlDbType.DateTime2:
                    return typeof(DateTime?);

                case SqlDbType.Decimal:
                case SqlDbType.Money:
                case SqlDbType.SmallMoney:
                    return typeof(decimal?);

                case SqlDbType.Float:
                    return typeof(double?);

                case SqlDbType.Int:
                    return typeof(int?);

                case SqlDbType.Real:
                    return typeof(float?);

                case SqlDbType.UniqueIdentifier:
                    return typeof(Guid?);

                case SqlDbType.SmallInt:
                    return typeof(short?);

                case SqlDbType.TinyInt:
                    return typeof(byte?);

                case SqlDbType.Variant:
                case SqlDbType.Udt:
                    return typeof(object);

                case SqlDbType.Structured:
                    return typeof(DataTable);

                case SqlDbType.DateTimeOffset:
                    return typeof(DateTimeOffset?);

                default:
                    throw new ArgumentOutOfRangeException("sqlType");
            }

        }
      

        public override DbConnection GetConnection()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            // Get connection from pool
            DbConnection conn = null;


            long start1 = Environment.TickCount;
            conn = new SqlConnection(_connString);
            conn.Open();
            if (Environment.TickCount - start1 > 1000)
            {
                Log.Notice("DataConnection", "Connection time : " + (Environment.TickCount - start1) + "ms");
            }

            Log.Debug("DataConnection", "New DB Connection");


            return conn;
        }

        /// <summary> Executes a non-blocking request (INSERT, DELETE, UPDATE)</summary>
        public override int ExecuteNonQuery(string sqlcommand)
        {

            Log.Debug("DataConnection", "SQL: " + sqlcommand);

            int affected = 0;
            bool repeat;
            do
            {
                SqlConnection conn = (SqlConnection)GetConnection();
                using (SqlCommand cmd = new SqlCommand(sqlcommand, conn))
                {
                    try
                    {
                        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                        long start = Environment.TickCount;
                        affected = cmd.ExecuteNonQuery();

                        Log.Debug("DataConnection", "SQL NonQuery exec time " + (Environment.TickCount - start) + "ms");

                        if (Environment.TickCount - start > 500)
                        {
                            Log.Notice("DataConnection", "SQL NonQuery took " + (Environment.TickCount - start) + "ms!\n" + sqlcommand + "\nTrace: " + Environment.StackTrace);
                        }


                        conn.Close();

                        repeat = false;
                    }
                    catch (Exception e)
                    {
                        conn.Close();

                        if (!HandleException(e))
                        {
                            throw;
                        }
                        repeat = true;
                    }
                }
            } while (repeat);

            return affected;
        }

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
        public override void ExecuteSelect(string sqlcommand, QueryCallback callback, IsolationLevel isolation)
        {
            Log.Debug("DataConnection", $"SQL: {sqlcommand}");

            bool repeat;

            DbConnection conn = null;
            do
            {
                try
                {
                    conn = GetConnection();

                    long start = Environment.TickCount;

                    using (SqlCommand cmd = new SqlCommand(sqlcommand, (SqlConnection)conn) { CommandTimeout = 180 })
                    {
                        SqlDataReader reader = cmd.ExecuteReader();
                        callback(reader);
                        reader.Close();

                        Log.Debug("DataConnection", $"SQL Select ({isolation}) exec time {Environment.TickCount - start}ms");

                        if (Environment.TickCount - start > 500)
                            Log.Notice("DataConnection", $"SQL Select ({isolation}) took {Environment.TickCount - start}ms!\n{sqlcommand}");
                    }

                    conn.Close();

                    repeat = false;
                }
                catch (Exception e)
                {
                    conn?.Close();

                    if (!HandleException(e))
                    {
                        Log.Error("DataConnection", $"ExecuteSelect: '{sqlcommand}'\n{e}");
                        return;
                    }

                    repeat = true;
                }
            } while (repeat);
        }



        protected override void InitTypeMapping()
        {
            TypeDescStrings[typeof(string)] = SqlDbType.NVarChar.ToString();
            TypeDescStrings[typeof(char[])] = SqlDbType.NVarChar.ToString();
            TypeDescStrings[typeof(int)] = SqlDbType.Int.ToString();
            TypeDescStrings[typeof(Int32)] = SqlDbType.Int.ToString();
            TypeDescStrings[typeof(Int16)] = SqlDbType.SmallInt.ToString();
            TypeDescStrings[typeof(Int64)] = SqlDbType.BigInt.ToString();
            TypeDescStrings[typeof(UInt32)] = SqlDbType.Int.ToString();
            TypeDescStrings[typeof(UInt16)] = SqlDbType.SmallInt.ToString();
            TypeDescStrings[typeof(UInt64)] = SqlDbType.BigInt.ToString();
            TypeDescStrings[typeof(SByte)] = SqlDbType.Int.ToString();
            TypeDescStrings[typeof(Byte[])] = SqlDbType.VarBinary.ToString();
            TypeDescStrings[typeof(Boolean)] = SqlDbType.Bit.ToString();
            TypeDescStrings[typeof(DateTime)] = SqlDbType.DateTime2.ToString();
            TypeDescStrings[typeof(DateTimeOffset)] = SqlDbType.DateTimeOffset.ToString();
            TypeDescStrings[typeof(Decimal)] = SqlDbType.Decimal.ToString();
            TypeDescStrings[typeof(Double)] = SqlDbType.Float.ToString();
            TypeDescStrings[typeof(Decimal)] = SqlDbType.Money.ToString();
            TypeDescStrings[typeof(Byte)] = SqlDbType.TinyInt.ToString();
            TypeDescStrings[typeof(TimeSpan)] = SqlDbType.Time.ToString();
        }

        public object ExecuteScalar(string query, params object[] param)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = String.Format(query, BuildParams(param));
                cmd.CommandTimeout = 400;
                cmd.Connection = (SqlConnection)GetConnection();
                cmd.CommandType = System.Data.CommandType.Text;

                var result = cmd.ExecuteScalar();
                return result;
            }
        }

        public override void CheckOrCreateTable(System.Data.DataTable table)
        {
            // Strings describing the columns.
            List<string> columnDefs = new List<string>();
            List<string> columnsToAdd = new List<string>();
            List<string> columnsToRemove = new List<string>();
            ArrayList databaseColumns = new ArrayList();
            List<string> databasePrimaryKeys = new List<string>();

            bool createNewTable = false;
            string key = SchemaName.ToLower() + "." + table.TableName.ToLower();

            if (!Schemas.ContainsKey(key))
            {
               // var schema = Schemas[SchemaName + "." + table.TableName];
                var builder = new StringBuilder();

                builder.AppendLine("CREATE TABLE [" + SchemaName.ToLower() + "].[" + table.TableName.ToLower() + "](");
                foreach (DataColumn col in table.Columns)
                {
                    builder.Append("\t[" + col.ColumnName + "] [" + TypeDescStrings[col.DataType] + "] ");

                    if (table.PrimaryKey.Length > 0 && table.PrimaryKey[0].ColumnName == col.ColumnName && col.AutoIncrement && col.AutoIncrementStep > 0)
                        builder.Append("IDENTITY(" + col.AutoIncrementSeed + "," + col.AutoIncrementStep + ") ");

                    if (col.DataType == typeof(string))
                    {
                        if (col.ExtendedProperties["VARCHAR"] != null && (int)col.ExtendedProperties["VARCHAR"] > 0)
                            builder.Append("(" + col.ExtendedProperties["VARCHAR"].ToString() + ") ");
                        else if (col.DataType == typeof(string))
                            builder.Append("(max) ");
                    }


                    if (col.AllowDBNull)
                        builder.Append("NULL ");
                    else
                        builder.Append("NOT NULL ");

                    builder.Append(",\r\n");
                }
                if (table.PrimaryKey.Length > 0)
                {
                    builder.AppendLine("CONSTRAINT [PK_" + table.TableName + "_" + table.PrimaryKey[0].ColumnName + "] PRIMARY KEY CLUSTERED ");
                    builder.AppendLine("(");
                    builder.AppendLine("\t[" + table.PrimaryKey[0].ColumnName + "] ASC");
                    builder.AppendLine(") WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]");
                }

                builder.AppendLine(") ON [PRIMARY]");
                ExecuteScalar(builder.ToString());
            }

            else
            {
                var schema = Schemas[SchemaName.ToLower() + "." + table.TableName.ToLower()];
                bool primaryKeysDirty = false;
                try
                {
                    var dbCols = schema.Columns.OrderBy(e=>e.Value.ORDINAL_POSITION).Select(e=>e.Value).ToList();
                    //rename changed columns
                    //for(int i=0; i<table.Columns.Count && i < dbCols.Count; i++)
                    //{
                    //    if (dbCols[i].COLUMN_NAME != table.Columns[i].ColumnName)
                    //    {
                    //        ExecuteScalar("EXECUTE sp_rename N'" + SchemaName + "." + table.TableName + "." + dbCols[i].COLUMN_NAME + "', N'" + table.Columns[i].ColumnName + "', 'COLUMN'");
                    //        schema.Columns.Remove(dbCols[i].COLUMN_NAME);
                    //        schema.Columns[table.Columns[i].ColumnName] = dbCols[i];
                    //        dbCols[i].COLUMN_NAME = table.Columns[i].ColumnName;
                    //    }
                    //}


                    //add new columns
                    foreach (DataColumn col in table.Columns)
                    {
                        if (!schema.Columns.ContainsKey(col.ColumnName.ToLower()))
                        {
                            var builder = new StringBuilder();

                            builder.Append("ALTER TABLE " + SchemaName + "." + table.TableName + " ADD " + col.ColumnName + " " + TypeDescStrings[col.DataType] + " ");

                            if (table.PrimaryKey.Length > 0 && table.PrimaryKey[0].ColumnName == col.ColumnName && col.AutoIncrement && col.AutoIncrementStep > 0)
                                builder.Append("IDENTITY(" + col.AutoIncrementSeed + "," + col.AutoIncrementStep + ") ");

                            if (col.DataType == typeof(string))
                            {
                                if (col.ExtendedProperties["VARCHAR"] != null && (int)col.ExtendedProperties["VARCHAR"] > 0)
                                    builder.Append("(" + col.ExtendedProperties["VARCHAR"].ToString() + ") ");
                                else if (col.DataType == typeof(string))
                                    builder.Append("(max) ");
                            }


                            if (col.AllowDBNull)
                                builder.Append("NULL ");
                            else
                                builder.Append("NOT NULL ");

                            ExecuteScalar(builder.ToString());
                        }
                    }

                   
                }
                catch (Exception e)
                {
                    Log.Debug("DataConnection", "Unable to check primary keys: " + e);
                }


                //if (columnsToAdd.Count > 0)
                //    AddColumnsToDatabase(table.TableName, columnsToAdd);

                //if (primaryKeysDirty)
                //    UpdatePrimaryKeys(table.TableName, primaryKeys.ToList(), databasePrimaryKeys.Count > 0);

                //if (columnsToRemove.Count > 0)
                //    RemoveColumnsFromDatabase(table.TableName, columnsToRemove);
            }
        }



        // Exécute un scale sur la DB
        public override object ExecuteScalar(string sqlcommand)
        {
            Log.Debug("DataConnection", "SQL: " + sqlcommand);

            object obj = null;
            bool repeat = false;
            SqlConnection conn = null;
            do
            {
                
                try
                {
                    conn = (SqlConnection)GetConnection();
                    long start = Environment.TickCount;
                    using (var cmd = new SqlCommand(sqlcommand, conn))
                    {
                        obj = cmd.ExecuteScalar();
                    }
                    conn.Close();

                    Log.Debug("DataConnection", "SQL Select exec time " + (Environment.TickCount - start) + "ms");

                    if (Environment.TickCount - start > 500)
                        Log.Notice("DataConnection", "SQL Select took " + (Environment.TickCount - start) + "ms!\n" + sqlcommand);

                    repeat = false;
                }
                catch (Exception e)
                {
                    if (conn != null)
                    {
                        conn.Close();
                    }

                    if (!HandleException(e))
                    {
                        Log.Error("DataConnection", "ExecuteSelect: '" + sqlcommand + "'\n" + e);
                        throw;
                    }

                    repeat = true;
                }
            } while (repeat);

            return obj;
        }


        private void AddColumnsToDatabase(string tableName, List<string> columnDefs)
        {
            string columndef = string.Join(", ", columnDefs.ToArray());
            string alterTable = "ALTER TABLE `" + tableName + "` ADD (" + columndef + ")";
            try
            {
                Log.Debug("DataConnection", alterTable);
                ExecuteNonQuery(alterTable);
                Log.Success(tableName, "Added columns: " + columndef);
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
        public override string GetDBDateFormat()
        {
            switch (ConnectionType)
            {
                case ConnectionType.DATABASE_MSSQL:
                    return "yyyy-MM-dd HH:mm:ss";
            }

            return "yyyy-MM-dd HH:mm:ss";
        }

    }
}