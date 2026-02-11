using AccountCacher.Services;
using Common;
using FrameWork;
using Microsoft.Extensions.DependencyInjection;

namespace AccountCacher;

internal static class ServiceCollectionExtensions
{
    public static void ConfigureServices(this IServiceCollection services, AccountConfig config)
    {
        // Need to create an instance of Account to load the assembly and avoid issues DB connection with the IObjectDatabase interface
        var acc = new Account();
        services.AddSingleton(
            DBManager.Start(config.AccountDB.Total(), config.AccountDB.ConnectionType, "Accounts",
                config.AccountDB.Database));

        services.AddGrpc();

        services.AddSingleton<AccountMgrService>(sp =>
            new AccountMgrService(sp.GetRequiredService<IObjectDatabase>(), config.EnableCache, config.MaxCacheSize));
        services.AddHostedService(sp => sp.GetRequiredService<AccountMgrService>());
    }

}