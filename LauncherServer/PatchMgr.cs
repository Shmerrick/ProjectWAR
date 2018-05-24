using System.Collections.Generic;
using System.IO;
using System.Linq;
using AuthenticationServer.Server;
using Common.Database.Account;
using FrameWork;
using Opcodes = AuthenticationServer.Server.Opcodes;

namespace AuthenticationServer
{
    public class PatchFile
    {
        public string Name;
        public uint CRC32;
        public long Size;
        public long MetaDataSize;
    }

    public class PatchAsset
    {
        public string Name;
        public uint CRC32;
        public long Size;
        public long MetaDataSize;
        public string File;
        public ulong Hash;
        public int ArchiveId;
    }


    public class FileUploadInfo
    {
        public Archive ArchiveID;
        public ulong Hash;
        public string LocalFile = "";
        public PatchAsset Asset;
        public PatchFile File;
        public byte[] Data;
        public string Destination;
        public FileType FileType;
        public FileCompressionMode Compress;
    };


    class PatchMgr
    {

        public static uint VersionHash;

        #region MYP Files




        public static List<PatchFile> _Patch_Files;
        public static Dictionary<Patch_MYP, List<PatchAsset>> _Patch_Assets;
        public static readonly bool use_cache = false;

        [LoadingFunction(true)]
        public static void LoadPatch_Files()
        {
            Log.Debug("PatchMgr", "Loading Patch_Files...");

            _Patch_Files = LoadFilesFromDisk();
 
            Log.Success("LoadPatch_Files", "Loaded " + _Patch_Files.Count + " Launcher_File");
        }

        public static List<PatchFile> LoadFilesFromDisk()
        {
            List<PatchFile> patch_files = new List<PatchFile>();

            DirectoryInfo d = new DirectoryInfo(Program.Config.PatcherFilesPath);
#if DEBUG
            if (!d.Exists)
                return patch_files;
#endif

            var files = d.GetFiles("*", SearchOption.AllDirectories).Where(e => !e.FullName.Contains(".git") && !e.FullName.Contains(".MYP")).ToArray();

            foreach (var file in files)
            {
                using (var fs = File.OpenRead(file.FullName))
                {
                    PatchFile pfile = new PatchFile();
                    pfile.Name = file.FullName.Remove(0, d.FullName.Length + 1).Replace('\\', '/');
                    pfile.CRC32 = Utils.Adler32(fs, fs.Length);
                    pfile.Size = fs.Length;
                    patch_files.Add(pfile);

                    if(file.Name.ToUpper() == "WARPATCHER.EXE")
                    {
                        VersionHash = pfile.CRC32;
                    }
                }
            }

            return patch_files;
        }

        [LoadingFunction(true)]
        public static void LoadPatch_Assets()
        {
            Log.Debug("PatchMgr", "Loading Patch_Assets...");

            _Patch_Assets = LoadAssetsFromDisk();

            Log.Success("LoadPatch_Assets", "Loaded " + _Patch_Assets.Count + " MYPs");
        }

        public static Dictionary<Patch_MYP, List<PatchAsset>> LoadAssetsFromDisk()
        {
            Dictionary<Patch_MYP, List<PatchAsset>> patch_assets = new Dictionary<Patch_MYP, List<PatchAsset>>();

            DirectoryInfo d = new DirectoryInfo(Program.Config.PatcherFilesPath);
#if DEBUG
            if (!d.Exists)
                return patch_assets;
#endif

            var myps = d.GetDirectories().Where(e => e.Name.EndsWith(".MYP")).ToArray();

            foreach (var myp in myps)
            {
                Patch_MYP pMYP = new Patch_MYP();
                pMYP.Name = myp.Name.Replace(".MYP", "");
                pMYP.Id = (int)strToArchive(myp.Name);

                List<PatchAsset> assets = new List<PatchAsset>();

                DirectoryInfo m = new DirectoryInfo(myp.FullName);
                var mypFiles = m.GetFiles("*", SearchOption.AllDirectories);

                foreach (var file in mypFiles)
                {
                    using (var fs = File.OpenRead(file.FullName))
                    {
                        PatchAsset pAsset = new PatchAsset();
                        pAsset.CRC32 = Utils.Adler32(fs, fs.Length);
                        pAsset.File = (myp.Name + "\\" + file.FullName.Remove(0, myp.FullName.Length + 1));
                        pAsset.Name = file.FullName.Remove(0, myp.FullName.Length + 1).Replace('\\', '/');
                        pAsset.Size = fs.Length;
                        pAsset.Hash = (ulong)HashWAR(pAsset.Name);
                        pAsset.ArchiveId = pMYP.Id;
                        assets.Add(pAsset);
                    }
                }

                pMYP.AssetCount = (uint)assets.Count;

                patch_assets.Add(pMYP, assets);
            }

            return patch_assets;
        }
        #endregion


        public static void SetServerState(ServerState state)
        {
            Program.Config.ServerState = state;
            ConfigMgr.SaveConfig(Program.Config);

            PacketOut Out = new PacketOut((byte)Opcodes.LCR_SERVER_STATUS);
            Out.WriteUInt32((uint)state);

            Program.Server.DispatchPatcket(Out);
        }

        #region Utils

        public static Archive strToArchive(string name)
        {

            if (name == "MFT.MYP")
                return Archive.MFT;
            if (name == "ART.MYP")
                return Archive.ART;
            if (name == "ART2.MYP")
                return Archive.ART2;
            if (name == "ART3.MYP")
                return Archive.ART3;
            if (name == "AUDIO.MYP")
                return Archive.AUDIO;
            if (name == "DATA.MYP")
                return Archive.DATA;
            if (name == "WORLD.MYP")
                return Archive.WORLD;
            if (name == "INTERFACE.MYP")
                return Archive.INTERFACE;
            if (name == "VIDEO.MYP")
                return Archive.VIDEO;
            if (name == "BLOODHUNT.MYP")
                return Archive.BLOODHUNT;
            if (name == "DEV.MYP")
                return Archive.DEV;
            if (name == "VO_ENGLISH.MYP")
                return Archive.VO_ENGLISH;
            if (name == "VO_FRENCH.MYP")
                return Archive.VO_FRENCH;
            if (name == "VIDEO_FRENCH.MYP")
                return Archive.VIDEO_FRENCH;
            if (name == "VO_GERMAN.MYP")
                return Archive.VO_GERMAN;
            if (name == "VIDEO_GERMAN.MYP")
                return Archive.VIDEO_GERMAN;
            if (name == "VO_ITALIAN.MYP")
                return Archive.VO_ITALIAN;
            if (name == "VIDEO_ITALIAN.MYP")
                return Archive.VIDEO_ITALIAN;
            if (name == "VO_SPANISH.MYP")
                return Archive.VO_SPANISH;
            if (name == "VIDEO_SPANISH.MYP")
                return Archive.VIDEO_SPANISH;
            if (name == "VO_KOREAN.MYP")
                return Archive.VO_KOREAN;
            if (name == "VIDEO_KOREAN.MYP")
                return Archive.VIDEO_KOREAN;
            if (name == "VO_CHINESE.MYP")
                return Archive.VO_CHINESE;
            if (name == "VIDEO_CHINESE.MYP")
                return Archive.VIDEO_CHINESE;
            if (name == "VO_JAPANESE.MYP")
                return Archive.VO_JAPANESE;
            if (name == "VIDEO_JAPANESE.MYP")
                return Archive.VIDEO_JAPANESE;
            if (name == "VO_RUSSIAN.MYP")
                return Archive.VO_RUSSIAN;
            if (name == "VIDEO_RUSSIAN.MYP")
                return Archive.VIDEO_RUSSIAN;
            if (name == "WARTEST.MYP")
                return Archive.WARTEST;

            return Archive.NONE;
        }

        public static long HashWAR(string s)
        {
            uint ph = 0, sh = 0;
            HashWAR(s, 0xDEADBEEF, out ph, out sh);
            return ((long)ph << 32) + sh;
        }

        public static void HashWAR(string s, uint seed, out uint ph, out uint sh)
        {
            uint edx = 0, eax, esi, ebx = 0;
            uint edi, ecx;


            eax = ecx = edx = ebx = esi = edi = 0;
            ebx = edi = esi = (uint)s.Length + seed;

            int i = 0;

            for (i = 0; i + 12 < s.Length; i += 12)
            {
                edi = (uint)((s[i + 7] << 24) | (s[i + 6] << 16) | (s[i + 5] << 8) | s[i + 4]) + edi;
                esi = (uint)((s[i + 11] << 24) | (s[i + 10] << 16) | (s[i + 9] << 8) | s[i + 8]) + esi;
                edx = (uint)((s[i + 3] << 24) | (s[i + 2] << 16) | (s[i + 1] << 8) | s[i]) - esi;

                edx = (edx + ebx) ^ (esi >> 28) ^ (esi << 4);
                esi += edi;
                edi = (edi - edx) ^ (edx >> 26) ^ (edx << 6);
                edx += esi;
                esi = (esi - edi) ^ (edi >> 24) ^ (edi << 8);
                edi += edx;
                ebx = (edx - esi) ^ (esi >> 16) ^ (esi << 16);
                esi += edi;
                edi = (edi - ebx) ^ (ebx >> 13) ^ (ebx << 19);
                ebx += esi;
                esi = (esi - edi) ^ (edi >> 28) ^ (edi << 4);
                edi += ebx;
            }

            if (s.Length - i > 0)
            {
                switch (s.Length - i)
                {
                    case 12:
                        esi += (uint)s[i + 11] << 24;
                        goto case 11;
                    case 11:
                        esi += (uint)s[i + 10] << 16;
                        goto case 10;
                    case 10:
                        esi += (uint)s[i + 9] << 8;
                        goto case 9;
                    case 9:
                        esi += (uint)s[i + 8];
                        goto case 8;
                    case 8:
                        edi += (uint)s[i + 7] << 24;
                        goto case 7;
                    case 7:
                        edi += (uint)s[i + 6] << 16;
                        goto case 6;
                    case 6:
                        edi += (uint)s[i + 5] << 8;
                        goto case 5;
                    case 5:
                        edi += (uint)s[i + 4];
                        goto case 4;
                    case 4:
                        ebx += (uint)s[i + 3] << 24;
                        goto case 3;
                    case 3:
                        ebx += (uint)s[i + 2] << 16;
                        goto case 2;
                    case 2:
                        ebx += (uint)s[i + 1] << 8;
                        goto case 1;
                    case 1:
                        ebx += (uint)s[i];
                        break;
                }

                esi = (esi ^ edi) - ((edi >> 18) ^ (edi << 14));
                ecx = (esi ^ ebx) - ((esi >> 21) ^ (esi << 11));
                edi = (edi ^ ecx) - ((ecx >> 7) ^ (ecx << 25));
                esi = (esi ^ edi) - ((edi >> 16) ^ (edi << 16));
                edx = (esi ^ ecx) - ((esi >> 28) ^ (esi << 4));
                edi = (edi ^ edx) - ((edx >> 18) ^ (edx << 14));
                eax = (esi ^ edi) - ((edi >> 8) ^ (edi << 24));

                ph = edi;
                sh = eax;
                return;
            }
            ph = esi;
            sh = eax;
            return;
        }
        #endregion
    }
}
