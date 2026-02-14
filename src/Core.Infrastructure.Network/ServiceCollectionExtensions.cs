using System.Net;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Infrastructure.Network;

public static class ServiceCollectionExtensions
{
    public static IServerNetworkingBuilder AddServerNetworking(this IServiceCollection services, IPEndPoint endPoint)
    {
        services
            .AddSingleton<ClientConnectionFactory>()
            .AddSingleton<NetworkManager>(p => new NetworkManager(endPoint, p.GetRequiredService<ClientConnectionFactory>()))
            .AddHostedService(p => p.GetRequiredService<NetworkManager>());

        return new ServerNetworkingBuilder(services);
    }
}