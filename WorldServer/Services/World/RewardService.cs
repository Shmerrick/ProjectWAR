using Common;
using FrameWork;
using GameData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WorldServer.World.Battlefronts;
using WorldServer.World.Battlefronts.NewDawn.Rewards;

namespace WorldServer.Services.World
{
    [Service]
    public class RewardService : ServiceBase
    {

        public static List<RenownBandRVRObjectiveTick> _RewardObjectiveTicks;
        public static List<RenownBandRVRZoneLock> _RewardZoneLocks;
        public static List<RenownBandReward> _RewardBandRewards;

        [LoadingFunction(true)]
        public static void LoadRenownBandRewards()
        {
            Log.Debug("WorldMgr", "Loading RVRObjectiveTicks...");
            _RewardObjectiveTicks = Database.SelectAllObjects<RenownBandRVRObjectiveTick>() as List<RenownBandRVRObjectiveTick>;
            Log.Success("LoadRenownBandRewards", "Loaded " + _RewardObjectiveTicks.Count + " rvr_reward_objective_tick");

            Log.Debug("WorldMgr", "Loading RVRZoneLocks...");
            _RewardZoneLocks = Database.SelectAllObjects<RenownBandRVRZoneLock>() as List<RenownBandRVRZoneLock>;
            Log.Success("LoadRenownBandRewards", "Loaded " + _RewardZoneLocks.Count + " rvr_reward_zone_lock");

            Log.Debug("WorldMgr", "Loading Renown Band Rewards...");
            _RewardBandRewards = Database.SelectAllObjects<RenownBandReward>() as List<RenownBandReward>;
            Log.Success("LoadRenownBandRewards", "Loaded " + _RewardBandRewards.Count + " renown_band_reward");
        }

        public static RenownBandReward GetRenownBandReward(int renownBand)
        {
            return _RewardBandRewards[renownBand];
        }

    }
}

 