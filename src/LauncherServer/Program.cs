using System;
using System.Net;
using System.Net.Http;
using Core.Infrastructure.Network;
using FrameWork;
using Grpc.Net.Client;
using LauncherServer;
using LauncherServer.Config;
using LauncherServer.Dtos;
using LauncherServer.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

try
{
    Log.Info("", "------------------- Launcher Server -------------------", ConsoleColor.DarkRed);
    if (!Log.InitLog(new LogInfo { Info = true, Error = true }, "LauncherServer")) ConsoleMgr.WaitAndExit(2000);
    var mythLoginServiceConfigManager = new MythLoginServiceConfigManager("Configs/mythloginserviceconfig.xml");
    Log.Info("mythloginserviceconfig.xml", mythLoginServiceConfigManager.Content);

    var builder = Host.CreateDefaultBuilder()
        .ConfigureServices((ctx, s) =>
        {
            s.AddSingleton(new AccountMgr.AccountMgrClient(GrpcChannel.ForAddress("https://127.0.0.1:6800",
                new GrpcChannelOptions { HttpHandler = new HttpClientHandler {
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true } })));

            var config = new LauncherConfig
            {
                IConfiguredTheFile = true, LauncherServerPort = 8000,
                ServerState = ServerState.CLOSED, TempFilesPath = "TempFilesDirectory"
            };
            s.AddSingleton(config);
            s.AddSingleton(mythLoginServiceConfigManager);
            s.AddSingleton<IPacketSerializerContext, LauncherSerializerContext>();

            s.AddServerNetworking(IPEndPoint.Parse($"127.0.0.1:{config.LauncherServerPort}"))
                .WithPacketFramer<BigEndianLengthFramer>()
                .WithPacketSerializerFactory<BinaryPacketSerializerFactory>()
                .AddDefaultPacketHandlers();
        });

    var host = builder.Build();
    await host.RunAsync();
}
catch(Exception ex)
{
    Log.Error("OnError", ex.Message);
}
