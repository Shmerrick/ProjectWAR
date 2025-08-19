using Common;
using FrameWork;
using FrameWork.Misc;
using System;

namespace LobbyServer
{
    // This is the main starting point for the Lobby Server.
    // Think of the lobby as a waiting room before you get into the actual game.
    // This server handles things like logging in and choosing your character.
    internal class Core
    {
        // This holds all the settings for our lobby server.
        public static LobbyConfigs Config;

        // This is a helper that lets our lobby server talk to other servers, like the account server.
        public static RpcClient Client;
        // This is the main server that listens for players trying to connect to the lobby.
        public static TCPServer Server;

        // This is a shortcut to get to the account manager, which handles all the player accounts.
        public static AccountMgr AcctMgr => Client.GetServerObject<AccountMgr>();

        // This is where everything starts when we run the lobby server.
        private static void Main(string[] args)
        {
            // This makes sure that if something bad happens, we have a way to handle it.
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(onError);

            Log.Info("", "-------------------- Lobby Server ---------------------", ConsoleColor.DarkRed);

            // This loads all our settings from files.
            ConfigMgr.LoadConfigs();
            Config = ConfigMgr.GetConfig<LobbyConfigs>();

            // This sets up the logging system, so we can write down what's happening in the server.
            if (!Log.InitLog(Config.LogLevel, "LobbyServer"))
                ConsoleMgr.WaitAndExit(2000);

            // This connects our lobby server to another server (the account cacher) to get account information.
            Client = new RpcClient("LobbyServer", Config.RpcInfo.RpcLocalIp, 1);
            if (!Client.Start(Config.RpcInfo.RpcServerIp, Config.RpcInfo.RpcServerPort))
                ConsoleMgr.WaitAndExit(2000);

            // This starts the part of the server that listens for players trying to connect to the lobby.
            if (!TCPManager.Listen<TCPServer>(Config.ClientPort, "LobbyServer"))
                ConsoleMgr.WaitAndExit(2000);

            Server = TCPManager.GetTcp<TCPServer>("LobbyServer");

            Log.Debug($"LobbyServer", $"RpcClient on Local Ip {Config.RpcInfo.RpcLocalIp}");
            Log.Debug($"LobbyServer", $"RpcClient Connect (Start) to {Config.RpcInfo.RpcServerIp}:{Config.RpcInfo.RpcServerPort}");
            Log.Debug($"LobbyServer", $"TcpServer on Port {Config.ClientPort}");

            // This starts the console so we can type commands to the server.
            ConsoleMgr.Start();
        }

        // This is what happens when a really bad error occurs that we didn't expect.
        private static void onError(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error("onError", e.ExceptionObject.ToString());
            // We create a report about the crash to help us figure out what went wrong.
            CrashGuard.GenerateCrashReport(e);
        }
    }
}