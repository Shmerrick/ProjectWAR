using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using FrameWork;

namespace LauncherServer.Server.Handler
{
    public class LauncherPackets : IPacketHandler
    {
        [PacketHandler(PacketHandlerType.TCP, Opcodes.CL_CREATE, 0, "OnCreate")]
        public static void CL_CREATE(BaseClient client, PacketIn packet)
        {
            Client cclient = (Client)client;

            string username = packet.GetString();
            string password = packet.GetString();
            string email = packet.GetString();
            byte langID = (byte)packet.ReadByte();
            Log.Debug("CL_CREATE", $"CL_CREATE Create Request : {username} {password} {email} lang: {langID}");

            var result = 0x02; // CreteAccountResult.ACCOUNT_BANNED;

            PacketOut Out = new PacketOut(Opcodes.LCR_CREATE);

            string ip = client.GetIp().Split(':')[0];

            // Check Ip Ban
            if (!Core.AcctMgr.IsIpBanned(new IsIpBannedRequest { IpAddress = ip }).IsBanned)
            {
                Log.Debug("CL_CREATE", "Create Account Request : " + username + " " + result);
                
                var createAccountRequest = new CreateAccountRequest
                {
                    Username = username,
                    Password = password,
                    Email = email,
                    LanguageId = Convert.ToUInt32(langID),
                    IpAddress = ip
                };
            
                if (Core.AcctMgr.CreateAccount(createAccountRequest).Created)
                {
                    result = 0x01; // CreteAccountResult.ACCOUNT_NAME_SUCCESS;
                    Log.Debug("CL_CREATE", "Create Account Request SUCCESS");
                }
                else
                {
                    Log.Debug("CL_CREATE", "Create Account Request BUSY");
                    result = 0x00; // CreteAccountResult.ACCOUNT_NAME_BUSY;
                }
                Out.WriteByte((byte)result);
            }
            else
            {
                Out.WriteByte((byte)result); // Banned
            }
            Log.Debug("CL_CREATE", $"Writing response to Client {Out} ");
            cclient.SendPacketNoBlock(Out);
        }

        [PacketHandler(PacketHandlerType.TCP, Opcodes.CL_START, 0, "OnStart")]
        public static void CL_START(BaseClient client, PacketIn packet)
        {
            var cclient = (Client)client;

            var username = packet.GetString();
            var password = packet.GetString();
            
            var Out = new PacketOut(Opcodes.LCR_START);
            
            var authResult = Core.AcctMgr.AuthenticateUser(new AuthenticateUserRequest
            {
                Username = username,
                Password = password
            });
            
            Out.WriteByte((byte)authResult.Result);

            if (authResult.Result == LoginResult.Success)
            {
                Log.Debug("CL_START", "Sending token to client : " + username + " token : " + authResult.Token);
                Out.WriteString(authResult.Token);
            }
            
            cclient.SendPacketNoBlock(Out);

#if !DEBUG
            if (result == LoginResult.LOGIN_SUCCESS && Core.Config.SeverOnConnect)
                cclient.Disconnect("Transaction complete");
#endif
        }

        [PacketHandler(PacketHandlerType.TCP, Opcodes.CL_CHECK, 0, "OnCheck")]
        public static void CL_CHECK(BaseClient client, PacketIn packet)
        {
            Client cclient = (Client)client;
            uint version = packet.GetUint32();

            Log.Debug("CL_CHECK", "Launcher Version : " + version);

            PacketOut Out = new PacketOut(Opcodes.LCR_CHECK);

            if (version != Core.Version)
            {
                Out.WriteByte((byte)CheckResult.LAUNCHER_VERSION); // Version incorrect + message
                Out.WriteString(Core.Message);
                client.SendPacketNoBlock(Out);

                cclient.Disconnect("Incorrect game version");
                return;
            }

            byte options = packet.GetUint8();
            ulong len = 0;

            if ((options & 1) == 1)
            {
                Log.Debug("CHECK", "Has mythic file info");
                len = packet.GetUint64();

                if ((long)len != Core.Info.Length)
                {
                    Out.WriteByte((byte)CheckResult.LAUNCHER_FILE);
                    Out.WriteString(Core.StrInfo);
                    cclient.SendPacketNoBlock(Out);
                    return;
                }
            }

            if ((options & 2) == 2)
            {
                Dictionary<string, object> computerProfile = readProfile(ref packet);

                Log.Debug("CHECK", "Has system info");
            }

            Out.WriteByte((byte)CheckResult.LAUNCHER_OK);
            cclient.SendPacketNoBlock(Out);
        }

        public static Dictionary<string, object> readProfile(ref PacketIn packet)
        {
            Dictionary<string, object> computerProfile = new Dictionary<string, object>();
            int count = packet.GetUint8();
            for (int i = 0; i < count; i++)
            {
                string key = packet.GetString();

                switch (packet.GetUint8())
                {
                    case 0: // string
                        {
                            computerProfile[key] = packet.GetString();
                        }
                        break;

                    case 1: // list
                        {
                            computerProfile[key] = readProfile(ref packet);
                        }
                        break;
                }
            }

            return computerProfile;
        }

        [PacketHandler(PacketHandlerType.TCP, Opcodes.CL_INFO, 0, "OnInfo")]
        public static void CL_INFO(BaseClient client, PacketIn packet)
        {
            Client cclient = client as Client;

            if (cclient.LastInfoRequest == 0 || cclient.LastInfoRequest <= TCPManager.GetTimeStampMS() + 10000)
            {
                cclient.LastInfoRequest = TCPManager.GetTimeStampMS();

                var realmsResponse = Core.AcctMgr.ListRealms(new ListRealmsRequest());

                PacketOut Out = new PacketOut(Opcodes.LCR_INFO);
                Out.WriteByte((byte)realmsResponse.Realms.Count);
                foreach (var Rm in realmsResponse.Realms)
                {
                    Out.WriteByte(Convert.ToByte(true)); // Out.WriteByte(Convert.ToByte(Rm.Info != null));
                    Out.WriteString(Rm.Name);
                    Out.WriteUInt32(Rm.OnlinePlayers);
                    Out.WriteUInt32(Rm.OrderCount);
                    Out.WriteUInt32(Rm.DestructionCount);
                }

                cclient.SendPacketNoBlock(Out);
            }
        }

        //Client is sending their patcher version. Server will compare to patcher version currently in database.
        //if invalid client will request latest patcher and restart
        [PacketHandler(PacketHandlerType.TCP, Opcodes.CL_VERSION, 0, "OnVersion")]
        public static void CL_VERSION(BaseClient client, PacketIn packet)
        {
            Client cclient = client as Client;
            var p = (uint)packet.GetInt32();
            cclient.OnValidateVersion(p);
        }

        [PacketHandler(PacketHandlerType.TCP, Opcodes.CL_LOGIN, 0, "OnLogin")]
        public static void CL_LOGIN(BaseClient client, PacketIn packet)
        {
            Client cclient = client as Client;

            var username = packet.GetString16();
            var hashLen = packet.GetUint16();
            var hash = new byte[hashLen];
            packet.Read(hash, 0, hashLen);
            var installID = packet.GetString();

            cclient.Login(username, hash, installID);
        }

        //client requesting list of required files/myps
        [PacketHandler(PacketHandlerType.TCP, Opcodes.CL_REQUEST_MANIFEST_LIST, 0, "CL_REQUEST_MANIFEST_LIST")]
        public static void CL_REQUEST_MANIFEST_LIST(BaseClient client, PacketIn packet)
        {
            Client cclient = client as Client;
            cclient.OnRequestManifestList();
        }

        //client is requesting asset hashes in specified myps
        [PacketHandler(PacketHandlerType.TCP, Opcodes.CL_REQUEST_MANIFEST, 0, "CL_REQUEST_MANIFEST")]
        public static void CL_REQUEST_MANIFEST(BaseClient client, PacketIn packet)
        {
            Client cclient = client as Client;
            List<Archive> archiveList = new List<Archive>();
            var count = packet.GetUint8();

            for (int i = 0; i < count; i++)
                archiveList.Add((Archive)packet.GetUint8());

            cclient.OnRequestManifest(archiveList);
        }

        //client is requesting file part
        [PacketHandler(PacketHandlerType.TCP, Opcodes.CL_DATA_REQUEST_PARTS, 0, "CL_DATA_REQUEST_PARTS")]
        public static void CL_DATA_REQUEST_PARTS(BaseClient client, PacketIn packet)
        {
            Client cclient = client as Client;
            var offset = packet.GetInt64(); //offset into file
            var size = packet.GetInt32(); //size from offset
            var sequence = packet.GetInt32(); //request sequence, sent back to client with reply
            cclient.OnFilePartRequest(offset, size, sequence);
        }

        //client requesting asset or file
        [PacketHandler(PacketHandlerType.TCP, Opcodes.CL_REQUEST_ASSET, 0, "CL_REQUEST_ASSET")]
        public static void CL_REQUEST_ASSET(BaseClient client, PacketIn packet)
        {
            Client cclient = client as Client;
            var archive = (Archive)packet.GetInt32();
            var hash = (ulong)packet.GetInt64();

            /*if (archive == Archive.ADDON)
                cclient.ReqestAddonFile((uint)hash);
            else*/
            if (archive == Archive.NONE)
                cclient.RequestFile((uint)hash);
            else
                cclient.RequestAsset(archive, hash);
        }

        //client is ready for more data
        [PacketHandler(PacketHandlerType.TCP, Opcodes.CL_DATA_READY, 0, "CL_DATA_READY")]
        public static void CL_DATA_READY(BaseClient client, PacketIn packet)
        {
            Client cclient = client as Client;
            cclient.OnReadyForData();
        }

        //client setting patcher notes, will be rebroadcast to all conencted clients
        [PacketHandler(PacketHandlerType.TCP, Opcodes.CL_SET_PATCH_NOTES, 0, "CL_SET_PATCH_NOTES")]
        public static void CL_SET_PATCH_NOTES(BaseClient client, PacketIn packet)
        {
            Client cclient = client as Client;
            string notes = packet.GetString();
            cclient.OnSetPatchNotes(notes);
        }

        //client sending their patch log
        [PacketHandler(PacketHandlerType.TCP, Opcodes.CL_PATCHER_LOG, 0, "CL_PATCHER_LOG")]
        public static void CL_PATCHER_LOG(BaseClient client, PacketIn packet)
        {
            Client cclient = client as Client;

            var compressedSize = packet.GetInt32();
            var uncompressedSize = packet.GetInt32();

            byte[] compressedData = packet.Read(compressedSize);
            byte[] uncompressedData = new byte[uncompressedSize];

            MemoryStream ms = new MemoryStream(compressedData);
            ms.Position += 2; //skip zlib header
            using (DeflateStream decompressionStream = new DeflateStream(ms, CompressionMode.Decompress, true))
                decompressionStream.Read(uncompressedData, 0, uncompressedSize);

            string log = System.Text.ASCIIEncoding.ASCII.GetString(uncompressedData);

            cclient.OnPatcherLog(log);
        }

        //client sending email code
        [PacketHandler(PacketHandlerType.TCP, Opcodes.CL_EMAIL_REGISTRATION, 0, "CL_EMAIL_REGISTRATION")]
        public static void CL_EMAIL_REGISTRATION(BaseClient client, PacketIn packet)
        {
            Client cclient = (Client)client;

            string username = packet.GetString().ToLower();
            string code = packet.GetString();
            PacketOut Out = new PacketOut(Opcodes.LCR_EMAIL_RESPONCE);
            // Out.WriteInt32(Core.AcctMgr.CheckCode(username, code));
            Log.Debug("CL_EMAIL_REGISTRATION", $"Writing response to Client {Out} ");
            cclient.SendPacketNoBlock(Out);
        }
    }
}