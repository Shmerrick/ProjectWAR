using Common;
using FrameWork;
using GameData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WorldServer.World.Battlefronts;
using WorldServer.World.Battlefronts.Bounty;

namespace WorldServer.Services.World
{
    [Service]
    public class RewardService : ServiceBase
    {

        public static List<RenownBandRVRObjectiveTick> _RewardObjectiveTicks;
        public static List<RenownBandRVRZoneLock> _RewardZoneLocks;
        public static List<RewardPlayerKill> _RewardPlayerKills;
        public static List<PlayerRVRGearDrop> _PlayerRVRGearDrops;

        [LoadingFunction(true)]
        public static void LoadRenownBandRewards()
        {
            Log.Debug("WorldMgr", "Loading RVRObjectiveTicks...");
            _RewardObjectiveTicks = Database.SelectAllObjects<RenownBandRVRObjectiveTick>() as List<RenownBandRVRObjectiveTick>;
            Log.Success("LoadRenownBandRewards", "Loaded " + _RewardObjectiveTicks.Count + " rvr_reward_objective_tick");

            Log.Debug("WorldMgr", "Loading RVRZoneLocks...");
            _RewardZoneLocks = Database.SelectAllObjects<RenownBandRVRZoneLock>() as List<RenownBandRVRZoneLock>;
            Log.Success("LoadRenownBandRewards", "Loaded " + _RewardZoneLocks.Count + " rvr_reward_zone_lock");

            Log.Debug("WorldMgr", "Loading RVR Player Kill rewards...");
            _RewardPlayerKills = Database.SelectAllObjects<RewardPlayerKill>() as List<RewardPlayerKill>;
            Log.Success("LoadRVRPlayerKillRewards", "Loaded " + _RewardPlayerKills.Count + " rvr_reward_player_kill");

            Log.Debug("WorldMgr", "Loading Player Gear Drops...");
            _PlayerRVRGearDrops = Database.SelectAllObjects<PlayerRVRGearDrop>() as List<PlayerRVRGearDrop>;
            Log.Success("PlayerRVRGearDrops", "Loaded " + _PlayerRVRGearDrops.Count + " rvr_player_gear_drop");

        }

        public static RewardPlayerKill GetPlayerKillReward(int renownBand)
        {
            return _RewardPlayerKills.Single(x=>x.RenownBand == renownBand);
        }

    }
}

 