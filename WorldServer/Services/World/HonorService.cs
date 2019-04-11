using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Database.World.Battlefront;
using Common.Database.World.Characters;
using FrameWork;
using GameData;
using WorldServer.World.Objects;

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
