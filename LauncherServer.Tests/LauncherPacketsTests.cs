using System.Collections.Generic;
using System.Reflection;
using Common;
using FrameWork;
using LauncherServer;
using LauncherServer.Server;
using LauncherServer.Server.Handler;
using LauncherOpcodes = LauncherServer.Server.Opcodes;
using Xunit;

#nullable enable

namespace LauncherServer.Tests;

public class LauncherPacketsTests
{
    private class MockAccountMgr : AccountMgr
    {
        public bool CheckIpResult { get; set; } = true;
        public bool CreateAccountResult { get; set; } = true;
        public LoginResult CheckAccountResult { get; set; } = LoginResult.LOGIN_SUCCESS;
        public string TokenToReturn { get; set; } = "token";

        public override bool CheckIp(string ip) => CheckIpResult;

        public override bool CreateAccount(string username, string passwordHash, string email, int gmLevel, int langID, string ip = "127.0.0.1")
            => CreateAccountResult;

        public override LoginResult CheckAccount(string username, string passwordHash, string ip)
            => CheckAccountResult;

        public override LoginResult CheckAccount(string username, string passwordHash, string ip, out int accountId)
        {
            accountId = 0;
            return CheckAccountResult;
        }

        public override string GenerateToken(string username) => TokenToReturn;
    }

    private class TestTCPManager : TCPManager
    {
        public TestTCPManager()
        {
            var field = typeof(TCPManager).GetField("m_packetBufPool", BindingFlags.NonPublic | BindingFlags.Instance);
            var queue = new Queue<byte[]>();
            for (int i = 0; i < 10; i++)
                queue.Enqueue(new byte[BUF_SIZE]);
            field!.SetValue(this, queue);
        }
    }

    private class TestClient : Client
    {
        public byte[]? LastPacket { get; private set; }

        public TestClient(TCPManager srv) : base(srv) { }

        public override bool SendPacketNoBlock(PacketOut packet)
        {
            packet.WritePacketLength();
            LastPacket = packet.ToArray();
            return true;
        }
    }

    private static PacketIn BuildCreatePacket(string username, string password, string email, byte langId)
    {
        var builder = new PacketOut((byte)LauncherOpcodes.CL_CREATE);
        builder.WriteString(username);
        builder.WriteString(password);
        builder.WriteString(email);
        builder.WriteByte(langId);
        builder.WritePacketLength();
        var data = builder.ToArray();
        var packet = new PacketIn(data, 0, data.Length);
        packet.Size = packet.GetUint32();
        packet.Opcode = packet.GetUint8();
        return packet;
    }

    private static PacketIn BuildStartPacket(string username, string password)
    {
        var builder = new PacketOut((byte)LauncherOpcodes.CL_START);
        builder.WriteString(username);
        builder.WriteString(password);
        builder.WritePacketLength();
        var data = builder.ToArray();
        var packet = new PacketIn(data, 0, data.Length);
        packet.Size = packet.GetUint32();
        packet.Opcode = packet.GetUint8();
        return packet;
    }

    private static PacketIn ParseResponse(byte[] data)
    {
        var packet = new PacketIn(data, 0, data.Length);
        packet.Size = packet.GetUint32();
        packet.Opcode = packet.GetUint8();
        return packet;
    }

    private static TestClient CreateClient(MockAccountMgr mgr)
    {
        var tcp = new TestTCPManager();
        var client = new TestClient(tcp);
        typeof(BaseClient).GetField("_ip", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(client, "127.0.0.1:1234");
        Core.AcctMgr = mgr;
        return client;
    }

    [Fact]
    public void CL_CREATE_Success_ReturnsSuccess()
    {
        var acctMgr = new MockAccountMgr { CheckIpResult = true, CreateAccountResult = true };
        var client = CreateClient(acctMgr);
        var packet = BuildCreatePacket("User1", "Pass123", "user@example.com", 1);

        LauncherPackets.CL_CREATE(client, packet);

        var response = ParseResponse(client.LastPacket!);
        Assert.Equal((byte)LauncherOpcodes.LCR_CREATE, (byte)response.Opcode);
        Assert.Equal((byte)CreteAccountResult.ACCOUNT_NAME_SUCCESS, response.GetUint8());
        Core.AcctMgr = null;
    }

    [Fact]
    public void CL_CREATE_Failure_ReturnsBusy()
    {
        var acctMgr = new MockAccountMgr { CheckIpResult = true, CreateAccountResult = false };
        var client = CreateClient(acctMgr);
        var packet = BuildCreatePacket("User1", "Pass123", "user@example.com", 1);

        LauncherPackets.CL_CREATE(client, packet);

        var response = ParseResponse(client.LastPacket!);
        Assert.Equal((byte)LauncherOpcodes.LCR_CREATE, (byte)response.Opcode);
        Assert.Equal((byte)CreteAccountResult.ACCOUNT_NAME_BUSY, response.GetUint8());
        Core.AcctMgr = null;
    }

    [Fact]
    public void CL_START_Success_ReturnsToken()
    {
        var acctMgr = new MockAccountMgr { CheckIpResult = true, CheckAccountResult = LoginResult.LOGIN_SUCCESS, TokenToReturn = "tok" };
        var client = CreateClient(acctMgr);
        var packet = BuildStartPacket("User1", "Pass123");

        LauncherPackets.CL_START(client, packet);

        var response = ParseResponse(client.LastPacket!);
        Assert.Equal((byte)LauncherOpcodes.LCR_START, (byte)response.Opcode);
        Assert.Equal((byte)LoginResult.LOGIN_SUCCESS, response.GetUint8());
        Assert.Equal("tok", response.GetString());
        Core.AcctMgr = null;
    }

    [Fact]
    public void CL_START_Failure_ReturnsInvalid()
    {
        var acctMgr = new MockAccountMgr { CheckIpResult = true, CheckAccountResult = LoginResult.LOGIN_INVALID_USERNAME_PASSWORD };
        var client = CreateClient(acctMgr);
        var packet = BuildStartPacket("User1", "Pass123");

        LauncherPackets.CL_START(client, packet);

        var response = ParseResponse(client.LastPacket!);
        Assert.Equal((byte)LauncherOpcodes.LCR_START, (byte)response.Opcode);
        Assert.Equal((byte)LoginResult.LOGIN_INVALID_USERNAME_PASSWORD, response.GetUint8());
        Assert.Equal(0, response.Remain());
        Core.AcctMgr = null;
    }
}
