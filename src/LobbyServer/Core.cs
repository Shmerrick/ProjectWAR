using Common;
using FrameWork;
using FrameWork.Misc;
using System;
using System.Net.Http;
using Grpc.Net.Client;

namespace LobbyServer
{
    internal class Core
    {
        public static LobbyConfigs Config;

        public static TCPServer Server;

        public static AccountMgr.AccountMgrClient AcctMgr;

        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(onError);

            Log.Info("", "-------------------- Lobby Server ---------------------", ConsoleColor.DarkRed);

            // Loading all configs files
            // ConfigMgr.LoadConfigs();
            // Config = ConfigMgr.GetConfig<LobbyConfigs>();
            
            Config = new LobbyConfigs()
            {
                IConfiguredTheFile = true,
                ClientPort = 8048,
                ClientVersion = "1.4.8",
                SeverOnFinish = true,
                LogLevel = new LogInfo { Info = true, Error = true, Debug = true }
            };

            // Loading log level from file
            if (!Log.InitLog(Config.LogLevel, "LobbyServer"))
                ConsoleMgr.WaitAndExit(2000);

            if (!TCPManager.Listen<TCPServer>(Config.ClientPort, "LobbyServer"))
                ConsoleMgr.WaitAndExit(2000);

            Server = TCPManager.GetTcp<TCPServer>("LobbyServer");
            AcctMgr = new AccountMgr.AccountMgrClient(GrpcChannel.ForAddress($"https://127.0.0.1:6800",
                new GrpcChannelOptions
                {
                    HttpHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                    }
                }));
            
            Log.Debug("LobbyServer", $"TcpServer on Port {Config.ClientPort}");

            ConsoleMgr.Start();
        }

        private static void onError(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error("onError", e.ExceptionObject.ToString());
            CrashGuard.GenerateCrashReport(e);
        }
    }
}