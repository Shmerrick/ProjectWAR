using Common;
using FrameWork;
using System;

namespace AccountCacher
{
    internal class Core
    {
        public static AccountMgr AcctMgr;
        public static AccountConfig Config;
        public static RpcServer Server;

        [STAThread]
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(onError);

            Log.Info("", "-------------------- Account Cacher  -------------------", ConsoleColor.DarkRed);

            // Loading all configs files
            ConfigMgr.LoadConfigs();
            Config = ConfigMgr.GetConfig<AccountConfig>();

            // Loading log level from file
            if (!Log.InitLog(Config.LogLevel, "AccountCacher"))
                ConsoleMgr.WaitAndExit(2000);

            AccountMgr.Database = DBManager.Start(Config.AccountDB.Total(), Config.AccountDB.ConnectionType, "Accounts", Config.AccountDB.Database);
            if (AccountMgr.Database == null)
                ConsoleMgr.WaitAndExit(2000);

            Server = new RpcServer(Config.RpcInfo.RpcClientStartingPort, 1);
            if (!Server.Start(Config.RpcInfo.RpcIp, Config.RpcInfo.RpcPort))
                ConsoleMgr.WaitAndExit(2000);

            AcctMgr = Server.GetLocalObject<AccountMgr>();
            AcctMgr.LoadRealms();

            ConsoleMgr.Start();
        }

        private static void onError(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error("OnError", e.ExceptionObject.ToString());
        }
    }
}