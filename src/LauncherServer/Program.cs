using System;
using System.IO;
using System.Net;
using System.Net.Http;
using FrameWork;
using FrameWork.NetWork.V4;
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

    // Loading log level from file
    if (!Log.InitLog(new LogInfo { Info = true, Error = true }, "LauncherServer"))
        ConsoleMgr.WaitAndExit(2000);

    // TODO: Rewrite loader mgr
    // LoaderMgr.Start();
    
    var mythLoginServiceConfigManager = new MythLoginServiceConfigManager("Configs/mythloginserviceconfig.xml");
    
    Log.Info("mythloginserviceconfig.xml", mythLoginServiceConfigManager.Content);
    
    var builder = Host.CreateDefaultBuilder()
        .ConfigureServices((ctx, s) =>
        {
            s.AddSingleton(new AccountMgr.AccountMgrClient(GrpcChannel.ForAddress("https://127.0.0.1:6800",
                new GrpcChannelOptions
                {
                    HttpHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                    }
                })));

            var config = new LauncherConfig
            {
                IConfiguredTheFile = true,
                LauncherServerPort = 8000,
                ServerState = ServerState.CLOSED,
                TempFilesPath = "TempFilesDirectory"
            };

            s.AddSingleton(config);
            s.AddSingleton(mythLoginServiceConfigManager);

            s.AddSingleton<LauncherSerializerContext>();
            s.AddSingleton<IPacketSerializerContext, LauncherSerializerContext>();
            s.AddSingleton<IPacketSerializerFactory, BinaryPacketSerializerFactory>();
            s.AddSingleton<IClientFactory<LauncherClient>, LauncherClientFactory>();
            s.AddSingleton(p => new NetworkManager<LauncherClient>(IPEndPoint.Parse($"127.0.0.1:{config.LauncherServerPort}"),
                p.GetRequiredService<IClientFactory<LauncherClient>>()));
            s.AddHostedService(p => p.GetRequiredService<NetworkManager<LauncherClient>>());
        });

    var host = builder.Build();
    await host.RunAsync();
}
catch(Exception ex)
{
    Log.Error("OnError", ex.Message);
}