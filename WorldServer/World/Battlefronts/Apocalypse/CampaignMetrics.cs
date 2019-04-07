using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Database.World.Battlefront;
using NLog;
using WorldServer.Managers;
using WorldServer.World.Map;
using WorldServer.World.Objects;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public static class CampaignMetrics
    {
        public static void RecordMetrics(Logger logger, int tier, RegionMgr region, IBattleFrontManager battleFrontManager)
        {
            try
            {
                var groupId = Guid.NewGuid().ToString();

                logger.Trace($"There are {battleFrontManager.GetBattleFrontStatusList().Count} battlefront statuses ({battleFrontManager.GetType().ToString()}).");
                foreach (var status in battleFrontManager.GetBattleFrontStatusList())
                {
                    lock (status)
                    {
                        if (status.RegionId == region.RegionId)
                        {
                            logger.Trace($"Recording metrics for BF Status : ({status.BattleFrontId}) {status.Description}");
                            if (!status.Locked)
                            {
                                var metrics = new RVRMetrics
                                {
                                    BattlefrontId = status.BattleFrontId,
                                    BattlefrontName = status.Description,
                                    DestructionVictoryPoints = (int)battleFrontManager.ActiveBattleFront.DestroVP,
                                    OrderVictoryPoints = (int)battleFrontManager.ActiveBattleFront.OrderVP,
                                    Locked = status.LockStatus,
                                    OrderPlayersInLake = PlayerUtil.GetTotalOrderPVPPlayerCountInZone(battleFrontManager.ActiveBattleFront.ZoneId),
                                    DestructionPlayersInLake = PlayerUtil.GetTotalDestPVPPlayerCountInZone(battleFrontManager.ActiveBattleFront.ZoneId),
                                    Tier = tier,
                                    Timestamp = DateTime.UtcNow,
                                    GroupId = groupId,
                                    TotalPlayerCountInRegion = PlayerUtil.GetTotalPVPPlayerCountInRegion(status.RegionId),
                                    TotalDestPlayerCountInRegion = PlayerUtil.GetTotalDestPVPPlayerCountInRegion(status.RegionId),
                                    TotalOrderPlayerCountInRegion = PlayerUtil.GetTotalOrderPVPPlayerCountInRegion(status.RegionId),
                                    TotalPlayerCount = Player._Players.Count(x => !x.IsDisposed && x.IsInWorld() && x != null),
                                    TotalFlaggedPlayerCount = Player._Players.Count(x => !x.IsDisposed && x.IsInWorld() && x != null && x.CbtInterface.IsPvp)
                                };
                                WorldMgr.Database.AddObject(metrics);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"Could not write rvr metrics..continuing. {e.Message} {e.StackTrace}");
            }

        }

        
    }
}
