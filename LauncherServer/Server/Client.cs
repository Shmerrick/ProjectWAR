using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Common;
using Common.Database.Account;
using FrameWork;

namespace AuthenticationServer.Server
{
    public class Client : BaseClient
    {
        public long LastInfoRequest = 0;
        private Account _account;
        private string _sessionToken;
        Queue<FileUploadInfo> _uploadQueue = new Queue<FileUploadInfo>();
        private bool _uploading = false;
        public static int UPLOAD_SIZE = 0xFFFFA;

        PatcherFile _uploadFile;


        public Client(TCPManager srv)
            : base(srv)
        {

        }

        public override void OnConnect()
        {
            Log.Info("Connection", GetIp());
        }

        public override void OnDisconnect(string reason)
        {
            Log.Info("Disconnection", GetIp() + " (" + reason + ")");
        }


        public override void SendTCPRaw(PacketOut packet)
        {
            base.SendTCPRaw(packet);

        }
        protected override void OnReceive(byte[] packetBuffer)
        {
            lock (this)
            {
                PacketIn pack = new PacketIn(packetBuffer, 0, packetBuffer.Length);
                pack.Size = pack.GetUint32();
                pack.Opcode = pack.GetUint8();

                if (!Enum.IsDefined(typeof(Opcodes), (byte)pack.Opcode))
                {
                    Log.Error("OnReceive", "Opcode invalid : " + pack.Opcode);
                    return;
                }

                Server.HandlePacket(this, pack);
            }
        }

        public void Login(string username, byte[] hash, string installID)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("x2"));

            var password = sb.ToString();

            string ip = GetIp().Split(':')[0];
            LoginResult result = LoginResult.LOGIN_BANNED;
            PacketOut Out = new PacketOut((byte)Opcodes.LCR_LOGIN);

            if (Program.AcctMgr.CheckIp(ip))
            {
                int accountId;

                result = Program.AcctMgr.CheckAccount(username, password, ip, out accountId);

                var account = Program.AcctMgr.GetAccount(accountId);
                if (account == null)
                {
                    result = LoginResult.LOGIN_INVALID_USERNAME_PASSWORD;
                }
                else
                {
                    if (account.CoreLevel == 0)
                    {
                        if (account.GmLevel == 0)
                            result = LoginResult.LOGIN_PATCHER_NOT_ALLOWED;
                    }
                }
                Out.WriteByte((byte)result);
                Out.WriteUInt32((uint)Program.Config.ServerState);


                if (result == LoginResult.LOGIN_SUCCESS)
                {
                    string token = Program.AcctMgr.GenerateToken(username);
                    Out.WriteString(token);

                    OnLogin(account, token, installID);
                    Out.WriteUInt32((uint)account.GmLevel);
                }
                else
                {
                    Out.WriteString("");
                    Out.WriteUInt32((uint)0);
                }
            }
            else
            {
                Out.WriteByte((byte)result); // Banned
                Out.WriteString("");
                Out.WriteUInt32((uint)0);
                Socket.Close();
            }

            SendTCPRaw(Out);
        }

        public void OnLogin(Account account, string token, string installID)
        {
            _account = account;
            _sessionToken = token;

            Program.AcctMgr.UpdateAccountBio(account.AccountId, ((IPEndPoint)_socket.RemoteEndPoint).Address.ToString(), installID);
            PacketOut Out = new PacketOut((byte)Opcodes.LCR_PATCH_NOTES);
            Out.WriteString(Program.Config.PatchNotes);

            SendTCPRaw(Out);
        }

        public void OnRequestManifestList()
        {
            if (!VerifyLoggedIn())
                return;

            if (Program.Config.ServerState == ServerState.PATCH)
                return;

            var list = PatchMgr._Patch_Assets;


            var files = PatchMgr._Patch_Files.ToList();

            PacketOut Out = new PacketOut((byte)Opcodes.LCR_REQUEST_MANIFEST_LIST);
            Out.WriteInt32(list.Count);

            foreach (var myp in list)
            {
                Patch_MYP pMYP = myp.Key;
                List<PatchAsset> pAssets = myp.Value;

                uint hashhash = 0;

                var hashes = pAssets.ToList();
                foreach (var asset in hashes)
                {
                    var newcrc = BitConverter.GetBytes(asset.CRC32);
                    hashhash = Utils.Adler32(hashhash, newcrc, 4);
                    //Console.WriteLine("CRC32: " + asset.CRC32 + " HH:" + hashhash);
                }


                Out.WriteByte((byte)pMYP.Id);
                Out.WriteUInt32(pMYP.AssetCount);
                Out.WriteUInt64(pMYP.ExtractedSize);
                Out.WriteUInt32(hashhash);
            }

            Out.WriteInt32(files.Count);

            foreach (var file in files)
            {

                Out.WriteString(file.Name);
                Out.WriteUInt32(file.CRC32);
                Out.WriteUInt64((ulong)file.Size);
            }
            SendTCPRaw(Out);
        }

        public void OnRequestManifest(List<Archive> archiveList)
        {
            CSV csv = new CSV();

            if (archiveList.Count == 0)
            {
                var list = PatchMgr._Patch_Assets;

                foreach (var myp in list)
                {
                    List<PatchAsset> pAssets = myp.Value;
                    foreach (var asset in pAssets)
                    {
                        csv.NewRow();
                        csv.WriteCol(0, ((int)asset.ArchiveId).ToString());
                        csv.WriteCol(1, asset.Hash.ToString());
                        csv.WriteCol(2, "");
                        csv.WriteCol(3, asset.CRC32.ToString());
                        csv.WriteCol(5, asset.Size.ToString());
                        csv.WriteCol(6, asset.MetaDataSize.ToString());
                        csv.WriteCol(7, String.IsNullOrEmpty(asset.File) ? 0 : 1);
                    }
                }

                
            }
            else
                foreach (var archive in archiveList)
                {
                    var pAssets = PatchMgr._Patch_Assets.Where( m => m.Key.Id == (int)archive).First().Value;

                    foreach (var asset in pAssets)
                    {
                        csv.NewRow();
                        csv.WriteCol(0, ((int)archive).ToString());
                        csv.WriteCol(1, asset.Hash.ToString());
                        csv.WriteCol(2, "");
                        csv.WriteCol(3, asset.CRC32.ToString());
                        csv.WriteCol(5, asset.Size.ToString());
                        csv.WriteCol(6, asset.MetaDataSize.ToString());
                        csv.WriteCol(7, String.IsNullOrEmpty(asset.File) ? 0 : 1);
                    }
                }

            PatcherFile p = new PatcherFile();
            var data = System.Text.ASCIIEncoding.ASCII.GetBytes(csv.ToText());
            QueueFileUpload(FileType.MANIFEST_SET, "MANIFEST", data, FileCompressionMode.WHOLE);
            ProcessFileUploadQueue();
        }


        private void QueueFileUpload(FileType type, string destination, byte[] data, FileCompressionMode compress)
        {
            var info = new FileUploadInfo()
            {
                Destination = destination,
                Data = data,
                FileType = type,
                Compress = compress
            };

            lock (_uploadQueue)
                _uploadQueue.Enqueue(info);
        }

        private void QueueFileUpload(PatchAsset asset, FileCompressionMode compress)
        {
            var info = new FileUploadInfo()
            {
                Asset = asset,
                Compress = compress
            };

            lock (_uploadQueue)
                _uploadQueue.Enqueue(info);
        }

        private void QueueFileUpload(PatchFile asset, FileCompressionMode compress)
        {
            var info = new FileUploadInfo()
            {
                File = asset,
                Compress = compress,
                LocalFile = Path.Combine(Program.Config.PatcherFilesPath, asset.Name)
            };
            lock (_uploadQueue)
                _uploadQueue.Enqueue(info);
        }

        private void ProcessFileUploadQueue()
        {
            FileUploadInfo upload = null;
            if (_uploading)
                return;

            lock (_uploadQueue)
            {
                if (_uploadQueue.Count > 0)
                    upload = _uploadQueue.Dequeue();
            }

            if (upload != null)
            {
                _uploading = true;
                var p = new PatcherFile();

                if (upload.Asset != null) // Asset
                {
                    var content = File.ReadAllBytes(Path.Combine(Program.Config.PatcherFilesPath, upload.Asset.File));
                    p.CreateUpload(upload.Asset, content, FileCompressionMode.WHOLE, FileType.MYP_ASSET);
                }
                else if (upload.Data != null) // Manifest
                    p.CreateUpload(upload.Destination, upload.Data, upload.Compress, upload.FileType);
                else
                    p.CreateUpload(upload.LocalFile, upload.File.Name, upload.Compress, FileType.GENERIC, upload.File.CRC32);

                SendUploadInfo(p);
            }
        }

        public void SendUploadInfo(PatcherFile file)
        {
            _uploadFile = file;

            // Send out notification of file transfer
            PacketOut Out = new PacketOut((byte)Opcodes.LCR_DATA_START);
            Out.WriteUInt32((uint)_uploadFile.ArchiveID);
            Out.WriteUInt64(_uploadFile.FilenameHash);
            Out.WriteUInt32((uint)_uploadFile.FileHash);
            Out.WriteUInt64((ulong)_uploadFile.CompressedSize);
            Out.WriteUInt64((ulong)_uploadFile.FileSize);
            Out.WriteInt32((int)_uploadFile.Compress);
            Out.WriteInt32((int)_uploadFile.Type);
            Out.WriteInt32((int)_uploadFile.OldCrc);
            if (_uploadFile.Filename != null)
            {
                Out.WriteUInt16((ushort)_uploadFile.Filename.Length);
                Out.Write(System.Text.ASCIIEncoding.ASCII.GetBytes(_uploadFile.Filename));
            }
            else
                Out.WriteUInt16(0);

            SendTCPRaw(Out);

        }

        public void OnFilePartRequest(long offset, int size, int sequence)
        {
            if (_uploadFile != null)
            {
                byte[] data = new byte[UPLOAD_SIZE];
                long read_size = _uploadFile.Read(data, (long)offset, size);
                if (read_size > 0)
                {
                    PacketOut Out = new PacketOut((byte)Opcodes.LCR_DATA_PART);
                    Out.WriteInt64(offset);
                    Out.WriteInt32((int)read_size);
                    Out.WriteInt32((int)sequence);
                    Out.Write(data, 0, data.Length);
                    SendTCPRaw(Out);
                }
                if (offset + size >= (long)_uploadFile.CompressedSize)
                {
                    _uploadFile.Close();
                    _uploadFile = null;
                    _uploading = false;
                    ProcessFileUploadQueue();
                }
            }
        }
        public void RequestAsset(Archive archive, ulong hash)
        {
            // Verify that server can give all requested assets
            var foundAsset = PatchMgr._Patch_Assets.Where(m => m.Key.Id == (int)archive).First().Value.Where(a => a.Hash == hash).First();

            // If asset is not found in database, of server does not have local file
            if (foundAsset == null || string.IsNullOrEmpty(foundAsset.File) || !File.Exists(Path.Combine(Program.Config.PatcherFilesPath, foundAsset.File)))
            {
                PacketOut Out = new PacketOut((byte)Opcodes.LCR_DATA_NOT_FOUND);
                Out.WriteUInt32((uint)archive);
                Out.WriteUInt64(hash);
                SendTCPRaw(Out);
                return;
            }

            QueueFileUpload(foundAsset, FileCompressionMode.WHOLE);
            ProcessFileUploadQueue();
        }

        public void RequestFile(uint hash)
        {
            //verify that server can give all requested assets
            var foundAsset = PatchMgr._Patch_Files.Where(f => f.CRC32 == hash).First();

            //if asset is not found in database, of server does not have local file
            if (foundAsset == null || string.IsNullOrEmpty(foundAsset.Name) || !File.Exists(Path.Combine(Program.Config.PatcherFilesPath, foundAsset.Name)))
            {
                PacketOut Out = new PacketOut((byte)Opcodes.LCR_DATA_NOT_FOUND);
                Out.WriteUInt32((uint)Archive.NONE);
                Out.WriteUInt64(hash);
                SendTCPRaw(Out);
                return;
            }

            QueueFileUpload(foundAsset, FileCompressionMode.WHOLE);
            ProcessFileUploadQueue();
        }

        public void RequestAddonFile(string Addon, uint hash)
        {
            return;
        }

        public void OnSetPatchNotes(string notes)
        {
            if (!VerifyGMLevel(EGmLevel.AllStaff))
                return;

            Program.Config.PatchNotes = notes;
            ConfigMgr.SaveConfig(Program.Config);

            PacketOut Out = new PacketOut((byte)Opcodes.LCR_PATCH_NOTES);
            Out.WriteString(notes);

            ((TCPServer)Server).DispatchPatcket(Out);
        }

        public bool VerifyGMLevel(params EGmLevel[] flags)
        {
            if (_account != null)
            {
                foreach (var flag in flags)
                {
                    if (_account != null && Utils.HasFlag(_account.GmLevel, (int)flag))
                        return true;
                }

            }
            else
                Close(); //client has not sent login packet

            SendError(LauncherErrorCode.INSUFFICIENT_GM_LEVEL, "Insufficent GM Level.");
            return false;
        }

        public void SendError(LauncherErrorCode errorCode, string msg)
        {
            //send out notification of file transfer
            PacketOut Out = new PacketOut((byte)Opcodes.LCR_ERROR);
            Out.WriteInt32((int)errorCode);
            Out.WriteString(msg);
            SendTCPRaw(Out);
        }

        public void OnReadyForData()
        {
            ProcessFileUploadQueue();
        }

        public void OnValidateVersion(UInt32 hash)
        {
            Guid g = Guid.NewGuid(); //create installID

            PacketOut Out = new PacketOut((byte)Opcodes.LCR_VERSION);
            Out.WriteUInt32R((uint)PatchMgr.VersionHash);
            Out.WriteUInt32((uint)Program.Config.ServerState);
            Out.WriteString(g.ToString());
            SendTCPRaw(Out);

        }

        public void OnPatcherLog(string log)
        {
            if (_account == null)
            {
                Close();
                return;
            }
            Program.AcctMgr.UpdateClientPatcherLog(_account.AccountId, log);
        }

        private bool VerifyLoggedIn()
        {
            if (_account != null)
                return true;
            Close();
            return false;
        }
        public void Close()
        {
            _socket.Close();
        }


    }
}
