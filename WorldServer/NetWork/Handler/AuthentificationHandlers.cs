using Common;
using FrameWork;
using WorldServer.Managers;
using WorldServer.World.Objects;

namespace WorldServer.NetWork.Handler
{
    public class AuthentificationHandlers : IPacketHandler
    {
        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_CONNECT, 0, "onConnect")]
        public static void F_CONNECT(BaseClient client, PacketIn packet)
        {
            Log.Success("F_CONNECT", "Entering F_CONNECT " + client.Id.ToString() + " " + packet.Opcode.ToString() );
            GameClient cclient = (GameClient) client;

            packet.Skip(8);
            uint Tag = packet.GetUint32();
            string Token = packet.GetString(80);
            packet.Skip(21);
            string Username = packet.GetString(23);

            // TODO
            AuthResult Result = Program.AcctMgr.CheckToken(Username, Token);
#if DEBUG
            Result = AuthResult.AUTH_SUCCESS;
#endif

            if (Result == AuthResult.AUTH_ACCT_SUSPENDED)
            {
                Log.Error("F_CONNECT", "Banned Account =" + Username);
                cclient.Disconnect("Banned account");
            }
            else if (Result != AuthResult.AUTH_SUCCESS)
            {
                Log.Error("F_CONNECT", "Invalid Token =" + Username + " " + Result);

                // Kick people who spam the god damn button for 5 minutes straight before they clock on.
                PacketOut Out = new PacketOut((byte)Opcodes.F_PLAYER_QUIT, 4);
                Out.WriteHexStringBytes("01000000");
                cclient.SendPacket(Out);

                cclient.Disconnect("Invalid token");
            }
            else
            {
                cclient._Account = Program.AcctMgr.GetAccount(Username);
                if (cclient._Account == null)
                {
                    Log.Error("F_CONNECT", "Invalid Account =" + Username);
                    cclient.Disconnect("Invalid account");
                }
                else
                {
                    Log.Success("F_CONNECT", "MeId=" + cclient.Id);
                    Log.Success("F_CONNECT", "User connection : " + Username);

                    GameClient Other = ((TCPServer) cclient.Server).GetClientByAccount(cclient, cclient._Account.AccountId);
                    if (Other != null)
                        Other.Disconnect("Failed to get GameClient for account");

                    // Check if ip is banned. (they may have been just banned so launcher server wouldnt have picked it up)
                    if(!Program.AcctMgr.CheckIp(cclient.GetIp().Split(':')[0]))
                    {
                        Log.Error("F_CONNECT", "Banned IP =" + Username);
                        cclient.Disconnect("Banned by IP");
                    }

                    // Load characters before connection instead of later on
                    CharMgr.LoadCharacters(cclient._Account.AccountId);

                    {
                        cclient.PacketLog = cclient._Account.PacketLog;

                        PacketOut Out = new PacketOut((byte)Opcodes.S_CONNECTED, 48);
                        Out.WriteUInt32(0);
                        Out.WriteUInt32(Tag);
                        Out.WriteByte(Program.Rm.RealmId);
                        Out.WriteByte(0);
                        Out.WriteByte(0);
                        Out.WriteByte(0);
                        Out.WriteByte(0); // TRANSFER_FLAG (1 - Low population server..free transfers...)
                        Out.WritePascalString(Username);
                        Out.WritePascalString(Program.Rm.Name);
                        Out.WriteByte(0);
                        Out.WriteUInt16(0);
                        cclient.SendPacket(Out);
                    }
                }
            }
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_PING, "onPing")]
        public static void F_PING(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            uint Timestamp = packet.GetUint32();

            PacketOut Out = new PacketOut((byte)Opcodes.S_PONG, 20);
            Out.WriteUInt32(Timestamp);
            Out.WriteUInt64((ulong)TCPManager.GetTimeStamp());
            Out.WriteUInt32((uint)(cclient.SequenceID+1));
            Out.WriteUInt32(0);
            cclient.SendPacket(Out);

            Player plr = cclient.Plr;

            if (plr != null)
                plr.LastKeepAliveTime = TCPManager.GetTimeStampMS();
        }

        public struct sEncrypt
        {
            public byte cipher, application, major, minor, revision, unk1;
        };

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_ENCRYPTKEY, "onEncryptKey")]
        public static void F_ENCRYPTKEY(BaseClient client, PacketIn packet)
        {
            GameClient cclient = (GameClient) client;

            sEncrypt result = BaseClient.ByteToType<sEncrypt>(packet);

            string version = result.major + "." + result.minor + "." + result.revision;

            Log.Debug("F_ENCRYPTKEY", "Version = " + version);
            // Should be a connection to the remote client.
            //Log.Debug("F_ENCRYPTKEY", "Client Ip = " + cclient.);

            if (result.cipher == 0)
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_RECEIVE_ENCRYPTKEY, 1);
                Out.WriteByte(1);
                cclient.SendPacket(Out);
            }
            else if (result.cipher == 1)
            {
                byte[] encryptKey = new byte[256];
                packet.Read(encryptKey, 0, encryptKey.Length);
                cclient.AddCrypt("RC4Crypto", new CryptKey(encryptKey), new CryptKey(encryptKey));
            }
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_DISCONNECT, 0,"onDisconnect")]
        public static void F_DISCONNECT(BaseClient client, PacketIn packet)
        {
            GameClient cclient = (GameClient) client;

            if (cclient._Account == null)
                return;

            TCPServer server = (TCPServer) client.Server;
            server.GetClientByAccount(cclient, cclient._Account.AccountId)?.Disconnect("F_DISCONNECT"); ;
        }
    }
}
