using Common;
using FrameWork;
using FrameWork.Misc;
using System;

namespace AccountCacher
{
    // This is the main starting point for the Account Cacher.
    // The Account Cacher is like a librarian for player accounts.
    // It keeps all the account information ready so that other servers, like the Lobby and World servers,
    // can quickly find the information they need.
    internal class Core
    {
        // This is the account manager, which does all the work of handling accounts.
        public static AccountMgr AcctMgr;
        // This holds all the settings for our account cacher.
        public static AccountConfig Config;
        // This is the server that other servers will talk to, to get account information.
        public static RpcServer Server;

        // This is where everything starts when we run the account cacher.
        [STAThread]
        private static void Main(string[] args)
        {
            // This makes sure that if something bad happens, we have a way to handle it.
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(onError);

            Log.Info("", "-------------------- Account Cacher  -------------------", ConsoleColor.DarkRed);

            // This loads all our settings from files.
            ConfigMgr.LoadConfigs();
            Config = ConfigMgr.GetConfig<AccountConfig>();

            // This sets up the logging system, so we can write down what's happening in the server.
            if (!Log.InitLog(Config.LogLevel, "AccountCacher"))
                ConsoleMgr.WaitAndExit(2000);

            // This connects to the database that stores all the account information.
            AccountMgr.Database = DBManager.Start(Config.AccountDB.Total(), Config.AccountDB.ConnectionType, "Accounts", Config.AccountDB.Database);
            if (AccountMgr.Database == null)
                ConsoleMgr.WaitAndExit(2000);

            // This starts the server that will listen for requests from other servers (like the world and lobby servers).
            Server = new RpcServer(Config.RpcInfo.RpcClientStartingPort, 1);
            if (!Server.Start(Config.RpcInfo.RpcIp, Config.RpcInfo.RpcPort))
                ConsoleMgr.WaitAndExit(2000);

            // This gets the account manager ready to handle requests.
            AcctMgr = Server.GetLocalObject<AccountMgr>();
            // This loads all the different game worlds (realms) from the database.
            AcctMgr.LoadRealms();
            // This loads any accounts that are waiting to be processed.
            AcctMgr.LoadPending();

            // This starts the console so we can type commands to the server.
            ConsoleMgr.Start();
        }

        // This is what happens when a really bad error occurs that we didn't expect.
        private static void onError(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error("OnError", e.ExceptionObject.ToString());
            // We create a report about the crash to help us figure out what went wrong.
            CrashGuard.GenerateCrashReport(e);
        }
    }
}