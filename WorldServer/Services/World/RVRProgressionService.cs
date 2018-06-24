using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Database.World.Battlefront;
using FrameWork;
using GameData;

namespace WorldServer.Services.World
{
    [Service]
    public class RVRProgressionService : ServiceBase
    {
        public static List<RVRProgression> _RVRProgressions;
        public static List<RVRPairing> _RVRPairings;

        [LoadingFunction(true)]
        public static void LoadRVRProgressions()
        {
            Log.Debug("WorldMgr", "Loading RVR Progression...");
            _RVRProgressions = Database.SelectAllObjects<RVRProgression>() as List<RVRProgression>;
            Log.Success("RVRProgression", "Loaded " + _RVRProgressions.Count + " RVRProgressions");
        }

        [LoadingFunction(true)]
        public static void LoadPairings()
        {
            Log.Debug("WorldMgr", "Loading RVR Pairings...");
            _RVRPairings = Database.SelectAllObjects<RVRPairing>() as List<RVRPairing>;
            Log.Success("RVRProgression", "Loaded " + _RVRProgressions.Count + " Pairings");
        }
    }
}
