using System;
using System.Buffers;
using Core.Infrastructure.Network;
using Google.Protobuf;

namespace LobbyServer.NetWork;

public class ProtobufPacketSerializer : IPacketSerializer
{
    public T Deserialize<T>(ReadOnlySpan<byte> payload)
    {
        if (typeof(T) == typeof(VerifyProtocolReq))
            return (T)(object)VerifyProtocolReq.Parser.ParseFrom(payload);
        if (typeof(T) == typeof(AuthSessionTokenReq))
            return (T)(object)AuthSessionTokenReq.Parser.ParseFrom(payload);
        
        throw new InvalidOperationException();
    }

    public void Serialize<T>(IBufferWriter<byte> writer, T message)
    {
        if (message is not IMessage protobufMessage)
            throw new InvalidOperationException("Unable to serialize message of type " + typeof(T).FullName + " - not a Protobuf message.");

        protobufMessage.WriteTo(writer);
    }
    
    /// <summary>
    /// Factory for creating BinaryPacketSerializer instances.
    /// </summary>
    public class Factory : IPacketSerializerFactory
    {
        private readonly ProtobufPacketSerializer _sharedInstance;

        /// <summary>
        /// Creates a new BinaryPacketSerializerFactory
        /// </summary>
        public Factory()
        {
            _sharedInstance = new ProtobufPacketSerializer();
        }

        /// <summary>
        /// Creates or returns a cached serializer instance.
        /// BinaryPacketSerializer is thread-safe and can be reused.
        /// </summary>
        public IPacketSerializer Create()
        {
            return _sharedInstance;
        }
    }
}