using System.Collections.Generic;
using FrameWork;

namespace AuthenticationServer.Server
{
    public class TCPServer : TCPManager
    {
        public bool AllowPatching = true;

        protected override BaseClient GetNewClient()
        {
            Client client = new Client(this);

            return client;
        }

        public void DispatchPatcket(PacketOut packet)
        {
            foreach (var c in GetClients<Client>())
            {
                c.SendTCPRaw(packet);
            }
        }

        public static List<string> LoadAddons()
        {
            return null;
            /*
            List<Addon> addons = new List<Addon>();

            List<aModuleFile> addon_info = new List<aModuleFile>();

            // Recurse into subdirectories of this directory.
            string dir = @"F:\Code\ReturnOfReckoning\Client\Addons\";
            string[] subdirectoryEntries = Directory.GetDirectories(dir);
            foreach (string subdirectory in subdirectoryEntries)
            {
                string name = subdirectory.Substring(dir.Length);

                aModuleFile Obj = null;
                System.Xml.Serialization.XmlSerializer Xml = new System.Xml.Serialization.XmlSerializer(typeof(aModuleFile));
                FileStream fds = new FileStream(subdirectory + "\\" + name + ".mod", FileMode.OpenOrCreate);
                Obj = Xml.Deserialize(fds) as aModuleFile;
                fds.Close();

                Log.Success("ADDON", Obj.UiMod.Name + " Author: " + Obj.UiMod.Author.Name);

               // Program.AcctMgr.UpdateAddon(Obj.UiMod.Name, Obj.UiMod.Version);

                DirectoryInfo d = new DirectoryInfo(subdirectory);
                var files = d.GetFiles("*", SearchOption.AllDirectories).Where(e => !e.FullName.Contains(".git")).ToArray();

                foreach (var file in files)
                {
                    uint adler = 0;
                    long size = 0;
                    string path = "";
                    ulong hash = 0;
                    using (var fs = File.OpenRead(file.FullName))
                    {
                        adler = Utils.Adler32(fs, fs.Length);
                        size = fs.Length;
                        path = (file.FullName.Remove(0, d.FullName.Length + 1));
                        hash = (ulong)HashWAR(name);
                    }


                   // Program.AcctMgr.UpdateAddonFile(Obj.UiMod.Name, path, adler, size);
                }
            }

            return addons;
            */
        }
    }
}
