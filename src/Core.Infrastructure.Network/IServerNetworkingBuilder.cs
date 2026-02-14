namespace Core.Infrastructure.Network;

public interface IServerNetworkingBuilder
{
    IServerNetworkingBuilder WithPacketFramer<T>() where T : class, IPacketFramer;
    IServerNetworkingBuilder WithPacketFramer(IPacketFramer packetFramer);
    IServerNetworkingBuilder WithPacketSerializerFactory<T>() where T : class, IPacketSerializerFactory;
    IServerNetworkingBuilder WithPacketSerializerFactory(IPacketSerializerFactory packetSerializerFactory);
    IServerNetworkingBuilder WithPacketDispatcher<T>() where T : class, IPacketDispatcher;
    IServerNetworkingBuilder WithPacketDispatcher(IPacketDispatcher packetDispatcher);
    IServerNetworkingBuilder AddHandler<THandler>() where THandler : class;
}