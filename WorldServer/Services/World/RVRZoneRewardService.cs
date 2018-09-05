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
        public static List<RVRZoneLockItemOptionReward> RVRZoneLockItemOptions;
        public static List<RVRZoneLockReward> RVRZoneRewards;

        /// <summary>
        /// List of RVR Zone Lock items that are to be considered on a zone lock
        /// </summary>
        [LoadingFunction(true)]
        public static void LoadRVRZoneLockItemOptions()
        {
            Log.Debug("WorldMgr", "Loading RVR Zone Lock Item Options...");
            RVRZoneLockItemOptions = Database.SelectAllObjects<RVRZoneLockItemOptionReward>() as List<RVRZoneLockItemOptionReward>;
            if (RVRZoneLockItemOptions != null) Log.Success("LoadRVRZoneLockItemOptions", "Loaded " + RVRZoneLockItemOptions.Count + " RVR Zone Lock Item Options");
        }

        /// <summary>
        /// List of rewards, regardless of item consideration (ie crests, RR, money, etc)
        /// </summary>
        [LoadingFunction(true)]
        public static void LoadRVRRewards()
        {
            Log.Debug("WorldMgr", "Loading RVR Zone Rewards...");
            RVRZoneRewards = Database.SelectAllObjects<RVRZoneLockReward>() as List<RVRZoneLockReward>;
            if (RVRZoneRewards != null) Log.Success("RVRZoneReward", "Loaded " + RVRZoneRewards.Count + " RVRZoneReward");
        }

    }

}
