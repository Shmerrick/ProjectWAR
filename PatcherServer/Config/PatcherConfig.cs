using PatcherServer.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace PatcherServer.Config
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

        public string Total()
        {
            string Result = "";

            Result += "Server=" + Server + ";";
            Result += "Port=" + Port + ";";
            Result += "Database=" + Database + ";";
            Result += "User Id=" + Username + ";";
            Result += "Password=" + Password + ";";
            Result += Custom;
            return Result;
        }
    }

    [aConfigAttributes("Configs/Patcher.xml")]
    public class PatcherConfig : aConfig
    {
        public string PatcherServerAddress = "127.0.0.1";
        public int PatcherServerPort = 8080;
        public int Version = 1;
        public string PatcherFilesPath = "PatcherFiles";
        public DatabaseInfo PatcherDatabase = new DatabaseInfo();
    }
}
