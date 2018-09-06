using PatcherServer.Config;
using PatcherServer.Database;
using PatcherServer.Framework;
using System;
using System.Threading;

namespace PatcherServer
{
    class Program
    {
        public static PatcherConfig Config = new PatcherConfig();

        static void Main(string[] args)
        {
            Config = ConfigMgr.LoadConfig<PatcherConfig>();

            if (Config == null)
            {
                Console.WriteLine("Invalid config file");
                Console.Read();
                return;
            }

            var con = new MySql.Data.MySqlClient.MySqlConnection(Config.PatcherDatabase.Total());

            try
            {
                var server = new Patcher(Config, new PatcherDB(con));

                server.RunHttpServer(Config.PatcherServerAddress, Config.PatcherServerPort, "Patcher", new CancellationTokenSource()).Wait();

                Console.Read();
            }
            catch (Exception ee)
            {
                Console.WriteLine("Unhandled exception: " + ee.ToString());
            }

        }
    }
}
