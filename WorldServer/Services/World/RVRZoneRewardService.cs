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
    public class RVRZoneRewardService : ServiceBase
    {
        public static List<RVRZoneReward> RVRZoneRewards;
        
        [LoadingFunction(true)]
        public static void LoadRVRRewards()
        {
            Log.Debug("WorldMgr", "Loading RVR Zone Rewards...");
            RVRZoneRewards = Database.SelectAllObjects<RVRZoneReward>() as List<RVRZoneReward>;
            if (RVRZoneRewards != null) Log.Success("RVRZoneReward", "Loaded " + RVRZoneRewards.Count + " RVRZoneReward");
        }

    }
}
