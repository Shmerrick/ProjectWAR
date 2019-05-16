using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Database.World.Battlefront;
using FrameWork;
using GameData;
using WorldServer.World.Objects;

namespace WorldServer.Services.World
{
    [Service]
    public class RVRZoneRewardService : ServiceBase
    {
        private static List<RVRRewardKeepItems> _RVRRewardKeepItems;
        private static List<RVRRewardFortItems> _RVRRewardFortItems;

        public static List<RVRRewardItem> RVRRewardKeepItems;
        public static List<RVRRewardItem> RVRRewardFortItems;
        public static List<RVRZoneLockReward> RVRZoneRewards;
        public static List<RVRKeepLockReward> RVRKeepRewards;

        /// <summary>
        /// List of RVR Zone Lock items that are to be considered on a zone lock
        /// </summary>
        [LoadingFunction(true)]
        public static void LoadRVRZoneLockItemOptions()
        {
            Log.Debug("WorldMgr", "Loading RVR Zone Lock Item Options...");
            _RVRRewardKeepItems = Database.SelectAllObjects<RVRRewardKeepItems>() as List<RVRRewardKeepItems>;
            if (_RVRRewardKeepItems != null) Log.Success("LoadRVRZoneLockItemOptions", "Loaded " + _RVRRewardKeepItems.Count + " RVR Zone Lock Item Options");

            if (RVRRewardKeepItems == null)
                RVRRewardKeepItems = new List<RVRRewardItem>();

            foreach (var rvrRewardKeepItem in _RVRRewardKeepItems)
            {
                RVRRewardKeepItems.Add(new RVRRewardItem(rvrRewardKeepItem));
            }
        }

        /// <summary>
        /// List of RVR Zone Lock items that are to be considered on a zone lock
        /// </summary>
        [LoadingFunction(true)]
        public static void LoadFortZoneLockOptions()
        {
            Log.Debug("WorldMgr", "Loading RVR Fort Zone Lock Options...");
            _RVRRewardFortItems = Database.SelectAllObjects<RVRRewardFortItems>() as List<RVRRewardFortItems>;
            if (_RVRRewardFortItems != null) Log.Success("LoadFortZoneLockOptions", "Loaded " + _RVRRewardFortItems.Count + " RVRRewardFortItems");

            if (RVRRewardFortItems == null)
                RVRRewardFortItems = new List<RVRRewardItem>();

            foreach (var rvrRewardKeepItem in _RVRRewardFortItems)
            {
                RVRRewardFortItems.Add(new RVRRewardItem(rvrRewardKeepItem));
            }
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

        /// <summary>
        /// List of rewards, regardless of item consideration (ie crests, RR, money, etc)
        /// </summary>
        [LoadingFunction(true)]
        public static void LoadRVRKeepRewards()
        {
            Log.Debug("WorldMgr", "Loading RVR Keep Rewards...");
            RVRKeepRewards = Database.SelectAllObjects<RVRKeepLockReward>() as List<RVRKeepLockReward>;
            if (RVRKeepRewards != null) Log.Success("RVRKeepRewards", "Loaded " + RVRKeepRewards.Count + " RVRKeepRewards");
        }

    }

}
