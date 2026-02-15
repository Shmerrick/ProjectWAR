using System.Buffers;
using Shouldly;
using WorldServer.NetWork.V2;
using WorldServer.NetWork.V2.Dtos;

namespace WorldServer.Tests;

public class GameServerSerializerTests
{
    private readonly GameServerSerializer _serializer = new();

    #region EncryptKeyRequest Deserialization

    [Fact]
    public void Deserialize_EncryptKeyRequest_ReadsStructFields()
    {
        // 6-byte struct: cipher=0, app=1, major=1, minor=4, revision=8, unk1=0
        // + 256-byte key
        var payload = new byte[262];
        payload[0] = 0x00; // cipher
        payload[1] = 0x01; // application
        payload[2] = 0x01; // major
        payload[3] = 0x04; // minor
        payload[4] = 0x08; // revision
        payload[5] = 0x00; // unk1
        for (var i = 6; i < 262; i++)
            payload[i] = (byte)(i & 0xFF);

        var result = _serializer.Deserialize<EncryptKeyRequest>(payload);

        result.Cipher.ShouldBe((byte)0x00);
        result.Application.ShouldBe((byte)0x01);
        result.Major.ShouldBe((byte)0x01);
        result.Minor.ShouldBe((byte)0x04);
        result.Revision.ShouldBe((byte)0x08);
        result.Unk1.ShouldBe((byte)0x00);
        result.Key.Length.ShouldBe(256);
        result.Key[0].ShouldBe((byte)0x06); // i=6 & 0xFF
    }

    [Fact]
    public void Deserialize_EncryptKeyRequest_CipherOne_ReadsKey()
    {
        var payload = new byte[262];
        payload[0] = 0x01; // cipher = RC4
        // Fill 256-byte key with 0xAA
        for (var i = 6; i < 262; i++)
            payload[i] = 0xAA;

        var result = _serializer.Deserialize<EncryptKeyRequest>(payload);

        result.Cipher.ShouldBe((byte)0x01);
        result.Key.Length.ShouldBe(256);
        result.Key.ShouldAllBe(b => b == 0xAA);
    }

    [Fact]
    public void Deserialize_EncryptKeyRequest_MinimalPayload_EmptyKey()
    {
        // Only the 6 struct bytes, no key data
        var payload = new byte[] { 0x00, 0x01, 0x01, 0x04, 0x08, 0x00 };

        var result = _serializer.Deserialize<EncryptKeyRequest>(payload);

        result.Key.Length.ShouldBe(0);
    }

    [Fact]
    public void Deserialize_EncryptKeyRequest_PayloadTooShort_Throws()
    {
        var payload = new byte[] { 0x00, 0x01, 0x02 }; // Only 3 bytes, need at least 6

        Should.Throw<InvalidOperationException>(() =>
            _serializer.Deserialize<EncryptKeyRequest>(payload));
    }

    [Fact]
    public void Deserialize_UnsupportedType_Throws()
    {
        Should.Throw<InvalidOperationException>(() =>
            _serializer.Deserialize<string>([]));
    }

    #endregion

    #region EncryptKeyResponse Serialization

    [Fact]
    public void Serialize_EncryptKeyResponse_WritesSingleByte()
    {
        var writer = new ArrayBufferWriter<byte>();
        var response = new EncryptKeyResponse { Status = 1 };

        _serializer.Serialize(writer, response);

        writer.WrittenCount.ShouldBe(1);
        writer.WrittenSpan[0].ShouldBe((byte)1);
    }

    [Fact]
    public void Serialize_EncryptKeyResponse_StatusZero()
    {
        var writer = new ArrayBufferWriter<byte>();
        var response = new EncryptKeyResponse { Status = 0 };

        _serializer.Serialize(writer, response);

        writer.WrittenCount.ShouldBe(1);
        writer.WrittenSpan[0].ShouldBe((byte)0);
    }

    [Fact]
    public void Serialize_UnsupportedType_Throws()
    {
        var writer = new ArrayBufferWriter<byte>();

        Should.Throw<InvalidOperationException>(() =>
            _serializer.Serialize(writer, "unsupported"));
    }

    #endregion

    #region Factory

    [Fact]
    public void Factory_ReturnsSameInstance()
    {
        var factory = new GameServerSerializer.Factory();

        var a = factory.Create();
        var b = factory.Create();

        a.ShouldBeSameAs(b);
    }

    #endregion
}
