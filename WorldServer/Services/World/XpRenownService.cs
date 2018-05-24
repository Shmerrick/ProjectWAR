using Common;
using FrameWork;
using System.Collections.Generic;

namespace WorldServer.Services.World
{
    [Service]
    public class XpRenownService : ServiceBase
    {
        private static Dictionary<byte, Xp_Info> _xpInfos;

        [LoadingFunction(true)]
        public static void LoadXp_Info()
        {
            Log.Debug("WorldMgr", "Loading Xp_Infos...");

            _xpInfos = Database.MapAllObjects<byte, Xp_Info>("Level");

            Log.Success("LoadXp_Info", "Loaded " + _xpInfos.Count + " Xp_Infos");
        }

        public static Xp_Info GetXp_Info(byte Level)
        {
            Xp_Info info;
            _xpInfos.TryGetValue(Level, out info);
            return info;
        }

        private static Dictionary<byte, Renown_Info> _renownInfos;

        [LoadingFunction(true)]
        public static void LoadRenown_Info()
        {
            Log.Debug("WorldMgr", "Loading Renown_Info...");

            _renownInfos = new Dictionary<byte, Renown_Info>();
            foreach (Renown_Info Info in Database.SelectAllObjects<Renown_Info>())
                _renownInfos.Add(Info.Level, Info);

            Log.Success("LoadRenown_Info", "Loaded " + _renownInfos.Count + " Renown_Info");
        }

        public static Renown_Info GetRenown_Info(byte level)
        {
            Renown_Info info;
            _renownInfos.TryGetValue(level, out info);
            return info;
        }
    }
}
