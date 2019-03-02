using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

namespace FrameWork
{
    public class DatabaseInfo
    {
        public string Server = "127.0.0.1";
        public string Port = "3306";
        public string Database = "warserver";
        public string Username = "warserver";
        public string Password = "madremia";
        public string Custom = "Treat Tiny As Boolean=False";
        public string Catalog = "";
        public string IPAddress = "";
        public bool MultipleActiveResultSets = false;

        public ConnectionType ConnectionType = ConnectionType.DATABASE_MYSQL;

        public string Total()
        {
            string Result = "";

            if (ConnectionType == ConnectionType.DATABASE_MYSQL)
            {
                Result += "Server=" + Server + ";";
                Result += "Port=" + Port + ";";
                Result += "Database=" + Database + ";";
            }
            else if (ConnectionType == ConnectionType.DATABASE_MSSQL)
            {
                if (IPAddress.Length > 0)
                {
                    Result += "Data Source=" + IPAddress + "," + Port + ";";
                    Result += "Network Library=DBMSSOCN;";
                }
                else
                    Result += "Server=" + Server + "," + Port + ";";

                Result += "Initial Catalog=" + Catalog + ";";
                if (MultipleActiveResultSets)
                    Result += "MultipleActiveResultSets=" + MultipleActiveResultSets + ";";
            }
            Result += "User Id=" + Username + ";";
            Result += "Password=" + Password + ";";
            Result += Custom;
            return Result;
        }
    }


    public class DBManager
    {
        private readonly FileInfo _file = new FileInfo("sql.conf");

        public static IObjectDatabase Start(string sqlconfig, ConnectionType Type, string databaseName, string schemaName)
        {
            Log.Debug("IObjectDatabase", databaseName + "->Start " + sqlconfig + "...");
            IObjectDatabase _database = null;

            try
            {
                _database = ObjectDatabase.GetObjectDatabase(Type, sqlconfig, schemaName);
                if (_database == null)
                    return null;

                LoadTables(_database, databaseName);

                return _database;
            }
            catch
            {
                return null;
            }
        }

        public static void LoadTables(IObjectDatabase Database, string DatabaseName)
        {
            List<string> typeNames = new List<string>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsClass != true)
                        continue;

                    try
                    {
                        DataTable[] attrib = (DataTable[])type.GetCustomAttributes(typeof(DataTable), true);
                        if (attrib.Length > 0 && attrib[0].DatabaseName == DatabaseName)
                        {
                            Database.RegisterDataObject(type);
                            typeNames.Add(type.Name);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error("DBManager", "Can not load : " + e);
                    }
                }
            }
            Log.Info("DBManager", "Registered table: " + string.Join(", ", typeNames));
        }
    }
}
