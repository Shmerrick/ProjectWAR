using Common.Database.World.Battlefront;
using FrameWork;
using System.Collections.Generic;
using System.Linq;
using Common;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Objects;

namespace WorldServer.Services.World
{
    [Service]
    public class RVRProgressionService : ServiceBase
    {
        public static List<RVRProgression> _RVRProgressions;
        public static List<RVRPairing> _RVRPairings;
        public static List<CampaignObjectiveBuff> _CampaignObjectiveBuffs;
        public static List<RVRAreaPolygon> _RVRAreaPolygons;
        

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

        [LoadingFunction(true)]
        public static void LoadCampaignObjectiveBuffs()
        {
            Log.Debug("WorldMgr", "Loading Campaign Objective Buffs...");
            _CampaignObjectiveBuffs = Database.SelectAllObjects<CampaignObjectiveBuff>() as List<CampaignObjectiveBuff>;
            Log.Success("RVRProgression", "Loaded " + _CampaignObjectiveBuffs.Count + " Campaign Objective Buffs");
        }

        [LoadingFunction(true)]
        public static void LoadRVRAreaPolygons()
        {
            Log.Debug("RVRProgression", "Loading RVR Area Polygons...");
            _RVRAreaPolygons = Database.SelectAllObjects<RVRAreaPolygon>() as List<RVRAreaPolygon>;
            Log.Success("RVRProgression", "Loaded " + _RVRAreaPolygons.Count + " RVR Area Polygons");
        }

       

        public static void SaveRVRProgression(List<RVRProgression> rvrProg)
        {
            if (rvrProg == null || rvrProg.Count <= 0)
                return;

            Log.Debug("WorldMgr", "Saving RVR progression ...");

            foreach (var item in rvrProg)
            {
                item.Dirty = true;
                item.IsValid = true;
                Database.SaveObject(item);
                item.Dirty = false;
            }

            Database.ForceSave();

            Log.Dump("RVRProgression", $"Saved RVR progression in tier {rvrProg.FirstOrDefault().Tier}");
        }

        public static void SaveBattleFrontKeepState(byte keepId, SM.ProcessState state)
        {
            var statusEntity = new BattleFrontKeepStatus {KeepId = keepId, Status = (int) state};

            Log.Debug("WorldMgr", $"Saving battlefront keep status {keepId} {(int)state}...");
            RemoveBattleFrontKeepStatus(keepId);
            Database.SaveObject(statusEntity);
            Database.ForceSave();
        }

        public static void RemoveBattleFrontKeepStatus(byte keepId)
        {
            Database.ExecuteNonQuery($"DELETE FROM battlefront_keep_status WHERE keepId={keepId}");
        }

        public static BattleFrontKeepStatus GetBattleFrontKeepStatus(byte keepId)
        {
            var status = Database.SelectObject<BattleFrontKeepStatus>($"KeepId={keepId}");
            return status;
        }



    }
}
