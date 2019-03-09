using System;
using System.IO;
using AuthenticationServer.Config;
using AuthenticationServer.Server;
using Common;
using FrameWork;

namespace AuthenticationServer
{
    class Program
    {
        public static RpcClient Client;
        public static LauncherConfig Config;
        public static TCPServer Server;

        public static int Version
        {
            get
            {
                return Config.Version;
            }
        }
        
        public static string Message
        {
            get
            {
                return Config.Message;
            }
        }

        public static FileInfo Info;
        public static string StrInfo;

        public static AccountMgr AcctMgr
        {
            get
            {
                return Client.GetServerObject<AccountMgr>();
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(onError);

            Log.Info("", "------------------- Launcher Server -------------------", ConsoleColor.DarkRed);

            // Loading all configs files
            ConfigMgr.LoadConfigs();
            Config = ConfigMgr.GetConfig<LauncherConfig>();

            // Loading log level from file
            if (!Log.InitLog(Config.LogLevel, "LauncherServer"))
                ConsoleMgr.WaitAndExit(2000);

            ServerState previousState = Config.ServerState;
            Config.ServerState = ServerState.PATCH;

            LoaderMgr.Start();
            Client = new RpcClient("LauncherServer", Config.RpcInfo.RpcLocalIp, 1);

            Config.ServerState = previousState;

            if (!Client.Start(Config.RpcInfo.RpcServerIp, Config.RpcInfo.RpcServerPort))
                ConsoleMgr.WaitAndExit(2000);

            Info = new FileInfo("Configs/mythloginserviceconfig.xml");
            if (!Info.Exists)
            {
                Log.Error("Configs/mythloginserviceconfig.xml", "Config file missing !");
                ConsoleMgr.WaitAndExit(5000);
            }

            StrInfo = Info.OpenText().ReadToEnd();

            if (!TCPManager.Listen<TCPServer>(Config.LauncherServerPort, "LauncherServer"))
                ConsoleMgr.WaitAndExit(2000);

            Server = TCPManager.GetTcp<TCPServer>("LauncherServer");

            ConsoleMgr.Start();
        }

        static void onError(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error("OnError", e.ExceptionObject.ToString());
        }
    }
}
