using System;
using System.Buffers;
using Core.Infrastructure.Network;
using WorldServer.NetWork.V2.Dtos;

namespace WorldServer.NetWork.V2;

/// <summary>
/// Binary packet serializer for the WAR game server protocol.
/// Handles manual serialization/deserialization of game-specific DTOs
/// whose wire format doesn't use standard length-prefixed fields.
/// </summary>
public class GameServerSerializer : IPacketSerializer
{
    /// <inheritdoc />
    public T Deserialize<T>(ReadOnlySpan<byte> payload)
    {
        if (typeof(T) == typeof(EncryptKeyRequest))
            return (T)(object)DeserializeEncryptKeyRequest(payload);

        throw new InvalidOperationException($"GameServerSerializer does not support deserializing {typeof(T).Name}.");
    }

    /// <inheritdoc />
    public void Serialize<T>(IBufferWriter<byte> writer, T message)
    {
        switch (message)
        {
            case EncryptKeyResponse response:
                SerializeEncryptKeyResponse(writer, response);
                return;
            default:
                throw new InvalidOperationException($"GameServerSerializer does not support serializing {typeof(T).Name}.");
        }
    }

    private static EncryptKeyRequest DeserializeEncryptKeyRequest(ReadOnlySpan<byte> payload)
    {
        const int structSize = 6; // cipher + application + major + minor + revision + unk1

        if (payload.Length < structSize)
            throw new InvalidOperationException(
                $"EncryptKeyRequest payload too short: expected at least {structSize} bytes, got {payload.Length}.");

        return new EncryptKeyRequest
        {
            Cipher = payload[0],
            Application = payload[1],
            Major = payload[2],
            Minor = payload[3],
            Revision = payload[4],
            Unk1 = payload[5],
            Key = payload[structSize..].ToArray()
        };
    }

    private static void SerializeEncryptKeyResponse(IBufferWriter<byte> writer, EncryptKeyResponse response)
    {
        var span = writer.GetSpan(1);
        span[0] = response.Status;
        writer.Advance(1);
    }

    /// <summary>
    /// Factory for creating GameServerSerializer instances.
    /// </summary>
    public class Factory : IPacketSerializerFactory
    {
        private readonly GameServerSerializer _sharedInstance = new();

        /// <summary>
        /// Returns a shared serializer instance (thread-safe, stateless).
        /// </summary>
        public IPacketSerializer Create() => _sharedInstance;
    }
}
