using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WarZoneLib
{
    public class MYPManager
    {
        public static MYPManager Instance;

        public Dictionary<MythicPackage, MYP> Archives = new Dictionary<MythicPackage, MYP>();

        public MYPManager(string folder)
        {
            foreach (string file in Directory.GetFiles(folder, "*.myp"))
            {
               

                string name = Path.GetFileNameWithoutExtension(file).ToUpper();


                MythicPackage package = MythicPackage.ART;

                if (Enum.TryParse(name, out package))
                {
                    var stream = File.Open(file, FileMode.Open, FileAccess.Read);
                    Archives[package] = new MYP(package, stream);
                }

            }
        }

        public bool HasAsset(string assetName)
        {
            long hash = MYP.HashWAR(assetName);
            foreach (var myp in Archives.Values)
            {
                if (myp.Enteries.ContainsKey(hash))
                    return true;
            }

            return false;
        }

        public byte[] GetAsset(string assetName)
        {
            long hash = MYP.HashWAR(assetName);
            foreach (var myp in Archives.Values)
            {
                if (myp.Enteries.ContainsKey(hash))
                    return GetAsset(myp.Enteries[hash]);
            }

            return null;
        }

        public Stream GetAssetStream(string assetName)
        {
            long hash = MYP.HashWAR(assetName);
            foreach (var myp in Archives.Values)
            {
                if (myp.Enteries.ContainsKey(hash))
                    return new MemoryStream(GetAsset(myp.Enteries[hash]));
            }

            return null;
        }

        public byte[] GetAsset(MYP.MFTEntry entry)
        {
            return entry.Archive.ReadFile(entry);
        }


    }
}
