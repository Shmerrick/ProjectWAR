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
        public static List<RVRRewardKeepItems> RVRRewardKeepItems;
        public static List<RVRZoneLockReward> RVRZoneRewards;
        public static List<RVRRewardFortItems> RVRRewardFortItems;

        /// <summary>
        /// List of RVR Zone Lock items that are to be considered on a zone lock
        /// </summary>
        [LoadingFunction(true)]
        public static void LoadRVRZoneLockItemOptions()
        {
            Log.Debug("WorldMgr", "Loading RVR Zone Lock Item Options...");
            RVRRewardKeepItems = Database.SelectAllObjects<RVRRewardKeepItems>() as List<RVRRewardKeepItems>;
            if (RVRRewardKeepItems != null) Log.Success("LoadRVRZoneLockItemOptions", "Loaded " + RVRRewardKeepItems.Count + " RVR Zone Lock Item Options");
        }

        /// <summary>
        /// List of RVR Zone Lock items that are to be considered on a zone lock
        /// </summary>
        [LoadingFunction(true)]
        public static void LoadFortZoneLockOptions()
        {
            Log.Debug("WorldMgr", "Loading RVR Fort Zone Lock Options...");
            RVRRewardFortItems = Database.SelectAllObjects<RVRRewardFortItems>() as List<RVRRewardFortItems>;
            if (RVRRewardFortItems != null) Log.Success("LoadFortZoneLockOptions", "Loaded " + RVRRewardFortItems.Count + " RVRRewardFortItems");
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
