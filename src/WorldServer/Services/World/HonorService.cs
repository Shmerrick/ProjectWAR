using Common.Database.World.Characters;
using FrameWork;
using System.Collections.Generic;

namespace WorldServer.Services.World
{
    [Service]
    public class HonorService : ServiceBase
    {
        public static List<HonorReward> HonorRewards;

        /// <summary>
        /// List of Honor Rewards
        /// </summary>
        [LoadingFunction(true)]
        public static void LoadHonorRewards()
        {
            Log.Debug("WorldMgr", "Loading Honor Rewards...");
            HonorRewards = Database.SelectAllObjects<HonorReward>() as List<HonorReward>;
            if (HonorRewards != null) Log.Success("HonorRewards", "Loaded " + HonorRewards.Count + " HonorRewards");
        }
    }
}