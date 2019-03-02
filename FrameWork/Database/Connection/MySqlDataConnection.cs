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

using MySql.Data.MySqlClient;
using System.Data.Common;
using FrameWork.Database.Connection;

namespace FrameWork
{
    /// <summary>
    /// Handles MySQL saving and loading.
    /// </summary>
    public class MySqlDataConnection:DataConnection
    {
        public override bool IsSQLConnection
        {
            get
            {
               return true;
            }
        }

        // Construction d'une connexion , Type(Mysql,ODBC,etc..) + paramètre de la connexion
        public MySqlDataConnection(string connString, string schemaName):base(ConnectionType.DATABASE_MYSQL, connString, schemaName)
        {
        }

        protected override void InitTypeMapping()
        {
            TypeDescStrings[typeof(char)] = "SMALLINT UNSIGNED";
            TypeDescStrings[typeof(DateTime)] = "DATETIME";
            TypeDescStrings[typeof(sbyte)] = "TINYINT";
            TypeDescStrings[typeof(short)] = "SMALLINT";
            TypeDescStrings[typeof(int)] = "INT";
            TypeDescStrings[typeof(long)] = "BIGINT";
            TypeDescStrings[typeof(byte)] = "TINYINT UNSIGNED";
            TypeDescStrings[typeof(bool)] = "TINYINT UNSIGNED";
            TypeDescStrings[typeof(ushort)] = "SMALLINT UNSIGNED";
            TypeDescStrings[typeof(uint)] = "INT UNSIGNED";
            TypeDescStrings[typeof(ulong)] = "BIGINT UNSIGNED";
            TypeDescStrings[typeof(float)] = "FLOAT";
            TypeDescStrings[typeof(double)] = "DOUBLE";
        }

        // Azarael 11/07/2016 - Library maintains a connection pool behind the scenes, so this is not required
        public override DbConnection GetConnection()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            long start1 = Environment.TickCount;
            MySqlConnection conn = new MySqlConnection(_connString);
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
            if (ConnectionType == ConnectionType.DATABASE_MYSQL)
            {
                Log.Debug("DataConnection", "SQL: " + sqlcommand);

                int affected = 0;
                bool repeat;
                do
                {
                    MySqlConnection conn = (MySqlConnection)GetConnection();
                    using (MySqlCommand cmd = new MySqlCommand(sqlcommand, conn))
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

            Log.Notice("DataConnection", "SQL NonQuery's not supported.");

            return 0;
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
            if (ConnectionType == ConnectionType.DATABASE_MYSQL)
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

                        using (MySqlCommand cmd = new MySqlCommand(sqlcommand, (MySqlConnection)conn) { CommandTimeout = 180 })
                        {
                            MySqlDataReader reader = cmd.ExecuteReader();
                            callback(reader);
                            reader.Close();

                            Log.Debug("DataConnection", $"SQL Select ({isolation}) exec time {Environment.TickCount - start}ms");

                            if (Environment.TickCount - start > 500)
                                Log.Notice($"Exec time { Environment.TickCount - start} ms", sqlcommand);
                        }

                        conn.Close();

                        repeat = false;
                    }
                    catch (Exception e)
                    {
                        conn?.Close();

                        if (!HandleException(e))
                        {
                            Log.Error("DataConnection", $"ExecuteSelect: \"{sqlcommand}\"\n{e}");
                            return;
                        }

                        repeat = true;
                    }
                } while (repeat);

                return;
            }

            Log.Notice("DataConnection", "SQL Selects not supported");
        }

        // Exécute un scale sur la DB
        public override object ExecuteScalar(string sqlcommand)
        {
            if (ConnectionType == ConnectionType.DATABASE_MYSQL)
            {
                Log.Debug("DataConnection", "SQL: " + sqlcommand);

                object obj = null;
                bool repeat = false;
                MySqlConnection conn = null;
                do
                {
                    try
                    {
                        conn = (MySqlConnection)GetConnection();
                        long start = Environment.TickCount;
                        using (var cmd = new MySqlCommand(sqlcommand, conn))
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
                        conn?.Close();

                        if (!HandleException(e))
                        {
                            Log.Error("DataConnection", "ExecuteSelect: \"" + sqlcommand + "\"\n" + e);
                            throw;
                        }

                        repeat = true;
                    }
                } while (repeat);

                return obj;
            }

            Log.Notice("DataConnection", "SQL Scalar not supported");

            return null;
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
                case ConnectionType.DATABASE_MYSQL:
                    return "yyyy-MM-dd HH:mm:ss";
            }

            return "yyyy-MM-dd HH:mm:ss";
        }

    }
}