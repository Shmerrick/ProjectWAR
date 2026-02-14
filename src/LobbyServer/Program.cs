using FrameWork;
using System;
using System.Net;
using System.Net.Http;
using Core.Infrastructure.Network;
using Grpc.Net.Client;
using LobbyServer;
using LobbyServer.NetWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

try
{
    Log.Info("", "-------------------- Lobby Server ---------------------", ConsoleColor.DarkRed);
    var config = new LobbyConfigs()
    {
        IConfiguredTheFile = true, ClientPort = 8048, ClientVersion = "1.4.8",
        SeverOnFinish = true, LogLevel = new LogInfo { Info = true, Error = true, Debug = true }
    };
    if (!Log.InitLog(config.LogLevel, "LobbyServer")) ConsoleMgr.WaitAndExit(2000);

    var builder = Host.CreateDefaultBuilder()
        .ConfigureServices((ctx, s) =>
        {
            s.AddSingleton(new AccountMgr.AccountMgrClient(GrpcChannel.ForAddress("https://127.0.0.1:6800",
                new GrpcChannelOptions { HttpHandler = new HttpClientHandler {
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true } })));
            
            s.AddServerNetworking(IPEndPoint.Parse($"127.0.0.1:{config.ClientPort}"))
                .WithPacketFramer<VarintLengthFramer>()
                .WithPacketSerializerFactory<ProtobufPacketSerializer.Factory>()
                .AddDefaultPacketHandlers();
        });

    var host = builder.Build();
    await host.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
