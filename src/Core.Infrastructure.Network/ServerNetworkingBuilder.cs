using Microsoft.Extensions.DependencyInjection;

namespace Core.Infrastructure.Network;

internal class ServerNetworkingBuilder : IServerNetworkingBuilder
{
    private readonly IServiceCollection _services;

    internal ServerNetworkingBuilder(IServiceCollection services)
    {
        _services = services;
    }
    
    public IServerNetworkingBuilder WithPacketFramer<T>() where T : class, IPacketFramer
    {
        _services.AddSingleton<IPacketFramer, T>();
        return this;
    }

    public IServerNetworkingBuilder WithPacketFramer(IPacketFramer packetFramer)
    {
        _services.AddSingleton(packetFramer);
        return this;
    }

    public IServerNetworkingBuilder WithPacketSerializerFactory<T>() where T : class, IPacketSerializerFactory
    {
        _services.AddSingleton<IPacketSerializerFactory, T>();
        return this;
    }

    public IServerNetworkingBuilder WithPacketSerializerFactory(IPacketSerializerFactory packetSerializerFactory)
    {
        _services.AddSingleton(packetSerializerFactory);
        return this;
    }

    public IServerNetworkingBuilder WithPacketDispatcher<T>() where T : class, IPacketDispatcher
    {
        _services.AddSingleton<IPacketDispatcher, T>();
        return this;
    }

    public IServerNetworkingBuilder WithPacketDispatcher(IPacketDispatcher packetDispatcher)
    {
        _services.AddSingleton(packetDispatcher);
        return this;
    }

    public IServerNetworkingBuilder AddHandler<THandler>() where THandler : class
    {
        _services.AddScoped<THandler>();
        return this;
    }
}
