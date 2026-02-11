using FrameWork;
using System;
using AccountCacher;
using AccountCacher.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

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

    var builder = Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(builder =>
        {
            builder.ConfigureKestrel(opts =>
                    opts.ListenLocalhost(6800, o => { o.UseHttps(); }))
                .ConfigureServices(s => s.ConfigureServices(configuration))
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints => { endpoints.MapGrpcService<AccountMgrService>(); });
                });
        });

    var host = builder.Build();
    await host.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
