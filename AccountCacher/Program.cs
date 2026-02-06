using FrameWork;
using System;
using AccountCacher.Services;
using Common;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace AccountCacher;

internal class Program
{
    public static void Main(string[] args)
    {
        try
        {
            Log.Info("", "-------------------- Account Cacher  -------------------");

            // Loading all configs files
            // ConfigMgr.LoadConfigs();
            // var configuration = ConfigMgr.GetConfig<AccountConfig>();
            var configuration = new AccountConfig
            {
                IConfiguredTheFile = true,
                AccountDB = new DatabaseInfo
                {
                    Server = "127.0.0.1",
                    Port = "3306",
                    Database = "war_accounts",
                    Username = "root",
                    Password = "admin",
                    Custom = "Treat Tiny As Boolean=False",
                    MultipleActiveResultSets = false,
                    ConnectionType = ConnectionType.DATABASE_MYSQL
                },
                EnableCache = true,
                MaxCacheSize = 10000
            };

            // Loading log level from file
            if (!Log.InitLog(configuration.LogLevel, "AccountCacher"))
                ConsoleMgr.WaitAndExit(2000);

            var builder = WebHost.CreateDefaultBuilder()
                .ConfigureKestrel(opts =>
                    opts.ListenLocalhost(6800, o => { o.UseHttps(); }))
                .ConfigureServices(services => ConfigureServices(services, configuration))
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints => { endpoints.MapGrpcService<AccountMgrService>(); });
                    var accountMgrService = app.ApplicationServices.GetService<AccountMgrService>();
                    accountMgrService.InitializeCache(configuration.EnableCache, configuration.MaxCacheSize);
                    accountMgrService.LoadRealms();
                    accountMgrService.LoadPending();
                });

            var host = builder.Build();
            host.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        Console.WriteLine("Done");
        ConsoleMgr.Start();
    }
    
    private static void ConfigureServices(IServiceCollection services, AccountConfig config)
    {
        var acc = new Account();
        services.AddSingleton(
            DBManager.Start(config.AccountDB.Total(), config.AccountDB.ConnectionType, "Accounts",
                config.AccountDB.Database));
        
        services.AddGrpc();
        services.AddSingleton<AccountMgrService>();
    }
}