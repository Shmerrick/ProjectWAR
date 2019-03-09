
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Common;
using FrameWork;

namespace LobbyServer
{
    class Program
    {
        public static LobbyConfigs Config;

        public static RpcClient Client;
        public static TCPServer Server;

        public static AccountMgr AcctMgr => Client.GetServerObject<AccountMgr>();

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(onError);

            Log.Info("", "-------------------- Lobby Server ---------------------", ConsoleColor.DarkRed);

            // Loading all configs files
            ConfigMgr.LoadConfigs();
            Config = ConfigMgr.GetConfig<LobbyConfigs>();

            // Loading log level from file
            if (!Log.InitLog(Config.LogLevel, "LobbyServer"))
                ConsoleMgr.WaitAndExit(2000);

            Client = new RpcClient("LobbyServer", Config.RpcInfo.RpcLocalIp, 1);
            if (!Client.Start(Config.RpcInfo.RpcServerIp, Config.RpcInfo.RpcServerPort))
                ConsoleMgr.WaitAndExit(2000);

            if (!TCPManager.Listen<TCPServer>(Config.ClientPort, "LobbyServer"))
                ConsoleMgr.WaitAndExit(2000);

            Server = TCPManager.GetTcp<TCPServer>("LobbyServer");

            

            Log.Debug($"LobbyServer", $"RpcClient on Local Ip {Config.RpcInfo.RpcLocalIp}");
            Log.Debug($"LobbyServer", $"RpcClient Connect (Start) to {Config.RpcInfo.RpcServerIp}:{ Config.RpcInfo.RpcServerPort}");
            Log.Debug($"LobbyServer", $"TcpServer on Port {Config.ClientPort}");

            ConsoleMgr.Start();
        }

        static void onError(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error("onError", e.ExceptionObject.ToString());
        }
    }
}
