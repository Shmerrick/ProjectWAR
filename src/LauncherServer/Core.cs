using System;
using System.IO;
using System.Net;
using System.Net.Http;
using FrameWork;
using FrameWork.Misc;
using FrameWork.NetWork.V4;
using Grpc.Net.Client;
using LauncherServer.Config;
using LauncherServer.Dtos;
using LauncherServer.Server;

namespace LauncherServer
{
    internal class Core
    {
        public static LauncherConfig Config;
        // public static TCPServer Server;
        public static NetworkManager NetworkManager;

        public static int Version => 1;

        public static string Message => "hello";
        public static FileInfo Info;
        public static string StrInfo;

        public static AccountMgr.AccountMgrClient AcctMgr;

        [STAThread]
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(onError);

            Log.Info("", "------------------- Launcher Server -------------------", ConsoleColor.DarkRed);

            // Loading all configs files
            // ConfigMgr.LoadConfigs();
            // Config = ConfigMgr.GetConfig<LauncherConfig>();

            // Loading log level from file
            if (!Log.InitLog(new LogInfo { Info = true, Error = true }, "LauncherServer"))
                ConsoleMgr.WaitAndExit(2000);

            // ServerState previousState = Config.ServerState;
            // Config.ServerState = ServerState.PATCH;
            Config = new LauncherConfig()
            {
                IConfiguredTheFile = true,
                LauncherServerPort = 8000,
                ServerState = ServerState.CLOSED,
                TempFilesPath = "TempFilesDirectory"
            };

            LoaderMgr.Start();

            // Config.ServerState = previousState;

            Info = new FileInfo("Configs/mythloginserviceconfig.xml");
            if (!Info.Exists)
            {
                Log.Error("Configs/mythloginserviceconfig.xml", "Config file missing !");
                ConsoleMgr.WaitAndExit(5000);
            }

            StrInfo = Info.OpenText().ReadToEnd();
            Log.Info("mythloginserviceconfig.xml", StrInfo);
            
            NetworkManager = new NetworkManager();
            var serializerContext = new LauncherSerializerContext();
            NetworkManager.Start(IPEndPoint.Parse("127.0.0.1:8000"), s => new LauncherClient(s, new BinaryPacketSerializerFactory(serializerContext)));
            
            
            AcctMgr = new AccountMgr.AccountMgrClient(GrpcChannel.ForAddress($"https://127.0.0.1:6800",
                new GrpcChannelOptions
                {
                    HttpHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                    }
                }));

            ConsoleMgr.Start();
        }

        private static void onError(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error("OnError", e.ExceptionObject.ToString());
            CrashGuard.GenerateCrashReport(e);
        }
    }
}