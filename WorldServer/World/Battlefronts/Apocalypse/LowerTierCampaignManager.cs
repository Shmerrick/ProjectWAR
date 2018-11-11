using System;
using System.Collections.Generic;
using System.Linq;
using Common.Database.World.Battlefront;
using GameData;
using NLog;
using WorldServer.World.Battlefronts.Bounty;

// ReSharper disable InconsistentNaming

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class LowerTierCampaignManager : IBattleFrontManager
	{
		private static readonly object LockObject = new object();
		public List<RegionMgr> RegionMgrs { get; }
        public List<RVRProgression> BattleFrontProgressions { get; }
        private static readonly Logger ProgressionLogger = LogManager.GetLogger("RVRProgressionLogger");
        public RVRProgression ActiveBattleFront { get; set; }
        public List<BattleFrontStatus> BattleFrontStatuses { get; set; }
	    public ImpactMatrixManager ImpactMatrixManagerInstance { get; set; }
	    public BountyManager BountyManagerInstance { get; set; }


        public LowerTierCampaignManager(List<RVRProgression> _RVRT1Progressions, List<RegionMgr> regionMgrs)
        {
            BattleFrontProgressions = _RVRT1Progressions;
            RegionMgrs = regionMgrs;
            BattleFrontStatuses = new List<BattleFrontStatus>();
            ImpactMatrixManagerInstance = new ImpactMatrixManager();
            BountyManagerInstance = new BountyManager();
            if (_RVRT1Progressions != null)
                BuildApocBattleFrontStatusList(BattleFrontProgressions);
        }

        public BattleFrontStatus GetActiveBattleFrontStatus(int battleFrontId)
        {
            return BattleFrontStatuses.Single(x => x.BattleFrontId == battleFrontId);
        }

        /// <summary>
        /// Sets up the Battlefront status list with default values. 
        /// </summary>
        /// <param name="battleFrontProgressions"></param>
        private void BuildApocBattleFrontStatusList(List<RVRProgression> battleFrontProgressions, CampaignRerollMode rerollMode = CampaignRerollMode.NONE)
		{
			lock (LockObject)
			{
				this.BattleFrontStatuses.Clear();
				foreach (var battleFrontProgression in battleFrontProgressions)
				{
					this.BattleFrontStatuses.Add(new BattleFrontStatus
					{
						BattleFrontId = battleFrontProgression.BattleFrontId,
						LockingRealm = (rerollMode == CampaignRerollMode.INIT)
							? (Realms)BattleFrontProgressions.Single(x => x.BattleFrontId == battleFrontProgression.BattleFrontId).LastOwningRealm
							: (Realms)BattleFrontProgressions.Single(x => x.BattleFrontId == battleFrontProgression.BattleFrontId).DefaultRealmLock,
						FinalVictoryPoint = new VictoryPointProgress(battleFrontProgression.OrderVP, battleFrontProgression.DestroVP),
						OpenTimeStamp = 0,
						LockTimeStamp = 0,
						Locked = true,
						RegionId = battleFrontProgression.RegionId,
						Description = battleFrontProgression.Description
					});
				}
			}
		}

        /// <summary>
        /// Return the first battlefront status for Lower tier (BF crosses regions)
        /// </summary>
        /// <param name="regionId"></param>
        /// <returns></returns>
        public BattleFrontStatus GetRegionBattleFrontStatus(int regionId)
        {
            return this.BattleFrontStatuses.FirstOrDefault(x => x.RegionId == regionId);
        }

        public Campaign GetActiveCampaign()
        {

            var activeRegionId = ActiveBattleFront.RegionId;
            foreach (var regionMgr in RegionMgrs)
            {
                if (regionMgr.RegionId == activeRegionId)
                {
                    return regionMgr.Campaign;
                }
            }
            return null;
        }

        /// <summary>
        /// Log the status of all battlefronts 
        /// </summary>
        public void AuditBattleFronts(int tier)
        {
            foreach (var regionMgr in RegionMgrs)
            {
                if (regionMgr.GetTier() == tier)
                {
                    foreach (var objective in regionMgr.Campaign.Objectives)
                    {
                        ProgressionLogger.Debug($"{regionMgr.RegionName} {objective.Name} {objective.State}");
                    }
                }
            }
        }

        /// <summary>
        /// Log the status of all battlefronts 
        /// </summary>
        public void LockBattleFrontsAllRegions(int tier, CampaignRerollMode rerollMode = CampaignRerollMode.NONE)
        {
            foreach (var regionMgr in RegionMgrs)
            {
                if (regionMgr.GetTier() == tier)
                {
                    // Find and update the status of the battlefront status (list of BFStatuses is only for this Tier)
                    foreach (var apocBattleFrontStatus in BattleFrontStatuses)
                    {
						apocBattleFrontStatus.Locked = true;
						apocBattleFrontStatus.OpenTimeStamp = FrameWork.TCPManager.GetTimeStamp();
						// Determine what the "start" realm this battlefront should be locked to.
						if (rerollMode == CampaignRerollMode.INIT)
							apocBattleFrontStatus.LockingRealm = (Realms)BattleFrontProgressions.Single(x => x.BattleFrontId == apocBattleFrontStatus.BattleFrontId).LastOwningRealm;
						else
							apocBattleFrontStatus.LockingRealm = (Realms)BattleFrontProgressions.Single(x => x.BattleFrontId == apocBattleFrontStatus.BattleFrontId).DefaultRealmLock;
						apocBattleFrontStatus.FinalVictoryPoint = new VictoryPointProgress();
						apocBattleFrontStatus.LockTimeStamp = FrameWork.TCPManager.GetTimeStamp();
					}

					if (regionMgr.Campaign == null)
						continue;

					regionMgr.Campaign.VictoryPointProgress = new VictoryPointProgress();


					foreach (var objective in regionMgr.Campaign.Objectives)
                    {
						if (rerollMode == CampaignRerollMode.INIT)
						{
							objective.LockObjective((Realms)regionMgr.Campaign.BattleFrontManager.ActiveBattleFront.LastOwningRealm, false);
							ProgressionLogger.Debug($" Locking BO to {(Realms)regionMgr.Campaign.BattleFrontManager.ActiveBattleFront.LastOwningRealm} {objective.Name} {objective.State} {objective.State}");
						}
						else
						{
							objective.LockObjective(Realms.REALMS_REALM_NEUTRAL, false);
							ProgressionLogger.Debug($" Locking BO to Neutral {objective.Name} {objective.State} {objective.State}");
						}
					}

                }
            }
        }

        public List<BattleFrontStatus> GetBattleFrontStatusList()
        {
            return this.BattleFrontStatuses;
        }

        public bool IsBattleFrontLocked(int battleFrontId)
        {
            foreach (var ApocBattleFrontStatus in BattleFrontStatuses)
            {
                if (ApocBattleFrontStatus.BattleFrontId == this.ActiveBattleFront.BattleFrontId)
                {
                    return ApocBattleFrontStatus.Locked;
                }
            }
            return false;
        }

        public BattleFrontStatus GetBattleFrontStatus(int battleFrontId)
        {
			return this.BattleFrontStatuses.Single(x => x.BattleFrontId == battleFrontId);
        }

        public void LockBattleFrontStatus(int battleFrontId, Realms lockingRealm, VictoryPointProgress vpp)
        {
            var activeStatus = BattleFrontStatuses.Single(x => x.BattleFrontId == battleFrontId);

            activeStatus.Locked = true;
            activeStatus.LockingRealm = lockingRealm;
            activeStatus.FinalVictoryPoint = vpp;
            activeStatus.LockTimeStamp = FrameWork.TCPManager.GetTimeStamp();
        }

        public RVRProgression LockActiveBattleFront(Realms realm, int forceNumberBags = 0)
        {
            var activeRegion = RegionMgrs.Single(x => x.RegionId == this.ActiveBattleFront.RegionId);
            ProgressionLogger.Info($" Locking battlefront in {activeRegion.RegionName} Zone : {this.ActiveBattleFront.ZoneId} {this.ActiveBattleFrontName}");

            LockBattleFrontStatus(this.ActiveBattleFront.BattleFrontId, realm, activeRegion.Campaign.VictoryPointProgress);

            foreach (var flag in activeRegion.Campaign.Objectives)
            {
                if (this.ActiveBattleFront.ZoneId == flag.ZoneId)
                    flag.LockObjective(realm, true);
            }

            activeRegion.Campaign.LockBattleFront(realm);

            // Use Locking Realm in the BFM, not the BF (BF applies to region)
            return this.ActiveBattleFront;
        }

		public RVRProgression OpenActiveBattlefront(CampaignRerollMode rerollMode = CampaignRerollMode.NONE)
        {
			try
			{
				// If this battlefront is the reset battlefront (ie Praag), reset all the progressions upon our return to it.
				//if (this.ActiveBattleFront.ResetProgressionOnEntry == 1)
				if (rerollMode == CampaignRerollMode.INIT || rerollMode == CampaignRerollMode.REROLL)
				{
					ProgressionLogger.Info($" Resetting Progress. Zone : {this.ActiveBattleFront.ZoneId} {this.ActiveBattleFrontName}");
					LockBattleFrontsAllRegions(1, rerollMode);
					// Reset the status list.
					BuildApocBattleFrontStatusList(this.BattleFrontProgressions, rerollMode);
					// Tells the attached players about it.
					WorldMgr.UpdateRegionCaptureStatus(this, WorldMgr.UpperTierCampaignManager);
				}
				
				var activeRegion = RegionMgrs.Single(x => x.RegionId == this.ActiveBattleFront.RegionId);
				ProgressionLogger.Info($" Opening battlefront in {activeRegion.RegionName}");
				ProgressionLogger.Info($"Resetting VP Progress {activeRegion.RegionName} BF Id : {this.ActiveBattleFront.BattleFrontId} Zone : {this.ActiveBattleFront.ZoneId} {this.ActiveBattleFrontName}");

				if (rerollMode == CampaignRerollMode.INIT)
				{
					activeRegion.Campaign.VictoryPointProgress.OrderVictoryPoints = ActiveBattleFront.OrderVP;
					activeRegion.Campaign.VictoryPointProgress.DestructionVictoryPoints = ActiveBattleFront.DestroVP;
				}
				else
					activeRegion.Campaign.VictoryPointProgress.Reset(activeRegion.Campaign);

				ProgressionLogger.Info($"Resetting BFStatus {activeRegion.RegionName} BF Id : {this.ActiveBattleFront.BattleFrontId} Zone : {this.ActiveBattleFront.ZoneId} {this.ActiveBattleFrontName}");


				// Find and update the status of the battlefront status.
				foreach (var apocBattleFrontStatus in BattleFrontStatuses)
				{
					if (apocBattleFrontStatus.BattleFrontId == this.ActiveBattleFront.BattleFrontId)
					{
						apocBattleFrontStatus.Locked = false;
						apocBattleFrontStatus.OpenTimeStamp = FrameWork.TCPManager.GetTimeStamp();
						apocBattleFrontStatus.LockingRealm = Realms.REALMS_REALM_NEUTRAL;
						if (rerollMode == CampaignRerollMode.INIT)
						{
							apocBattleFrontStatus.FinalVictoryPoint =
							new VictoryPointProgress(
								BattleFrontProgressions.Single(x => x.BattleFrontId == apocBattleFrontStatus.BattleFrontId).OrderVP,
								BattleFrontProgressions.Single(x => x.BattleFrontId == apocBattleFrontStatus.BattleFrontId).DestroVP
								);
						}
						else
						{
							apocBattleFrontStatus.FinalVictoryPoint = new VictoryPointProgress();
						}
						apocBattleFrontStatus.LockTimeStamp = 0;
					}
				}

				if (activeRegion.Campaign == null)
				{
					ProgressionLogger.Info($"activeRegion.Campaign is null");
					return this.ActiveBattleFront;
				}

				ProgressionLogger.Info($"Unlocking objectives {activeRegion.RegionName} BF Id : {this.ActiveBattleFront.BattleFrontId} Zone : {this.ActiveBattleFront.ZoneId} {this.ActiveBattleFrontName}");

				foreach (var flag in activeRegion.Campaign.Objectives)
				{
					if (this.ActiveBattleFront.RegionId == flag.RegionId)
						flag.UnlockObjective();
				}

				return this.ActiveBattleFront;
			}
			catch (Exception e)
			{
				ProgressionLogger.Error($"Exception. Zone : {this.ActiveBattleFront.ZoneId} {this.ActiveBattleFrontName} {e.Message} {e.StackTrace}");
				throw;
			}
		}

        /// <summary>
        /// Set the Active Pairing to be null. Not expected to be needed.
        /// </summary>
        public RVRProgression ResetBattleFrontProgression(CampaignRerollMode rerollMode = CampaignRerollMode.NONE)
        {
            ProgressionLogger.Info($" Resetting battlefront...");
			
			if (rerollMode == CampaignRerollMode.INIT)
			{
				var list = BattleFrontProgressions.Select(x => x).Where(x => x.LastOpenedZone == 1).ToList();
				if (list == null || list.Count == 0)
					ActiveBattleFront = GetBattleFrontByName("Norsca");
				else
					ActiveBattleFront = list.FirstOrDefault();
			}
			else
			{
				ActiveBattleFront = GetBattleFrontByName("Norsca");
			}
			
            ProgressionLogger.Info($"Active : {this.ActiveBattleFrontName}");
            return ActiveBattleFront;
        }

        public RVRProgression GetBattleFrontByName(string name)
        {
            return BattleFrontProgressions.Single(x => x.Description.Contains(name));
        }

        public RVRProgression GetBattleFrontByBattleFrontId(int id)
        {
            return BattleFrontProgressions.Single(x => x.BattleFrontId == id);
        }

        public string ActiveBattleFrontName
        {
            get { return ActiveBattleFront?.Description; }
            set { ActiveBattleFront.Description = value; }
        }

        /// <summary>
        /// </summary>
        public RVRProgression AdvanceBattleFront(Realms lockingRealm, out CampaignRerollMode rerollMode)
        {
            if (lockingRealm == Realms.REALMS_REALM_ORDER)
            {
                var newBattleFront = GetBattleFrontByBattleFrontId(ActiveBattleFront.OrderWinProgression);
                ProgressionLogger.Info($"Order Win : Advancing Battlefront from {this.ActiveBattleFrontName} to {newBattleFront.Description}");

				if (newBattleFront.ResetProgressionOnEntry == 1 && ActiveBattleFront.RegionId != newBattleFront.RegionId)
					rerollMode = CampaignRerollMode.REROLL;
				else
					rerollMode = CampaignRerollMode.NONE;

				UpdateRVRPRogression(lockingRealm, ActiveBattleFront, newBattleFront);

				return ActiveBattleFront = newBattleFront;
            }

            if (lockingRealm == Realms.REALMS_REALM_DESTRUCTION)
            {
                var newBattleFront = GetBattleFrontByBattleFrontId(ActiveBattleFront.DestWinProgression);
                ProgressionLogger.Info($"Destruction Win : Advancing Battlefront from {this.ActiveBattleFrontName} to {newBattleFront.Description}");

				if (newBattleFront.ResetProgressionOnEntry == 1 && ActiveBattleFront.RegionId != newBattleFront.RegionId)
					rerollMode = CampaignRerollMode.REROLL;
				else
					rerollMode = CampaignRerollMode.NONE;

				UpdateRVRPRogression(lockingRealm, ActiveBattleFront, newBattleFront);

				return ActiveBattleFront = newBattleFront;
            }
			rerollMode = CampaignRerollMode.REROLL;
			return ResetBattleFrontProgression(rerollMode);
        }
		
		private void UpdateRVRPRogression(Realms lockingRealm, RVRProgression oldProg, RVRProgression newProg)
		{
			oldProg.DestroVP = oldProg.OrderVP = 0;
			oldProg.LastOpenedZone = 0;
			oldProg.LastOwningRealm = (byte)lockingRealm;

			newProg.LastOwningRealm = 0;
			newProg.LastOpenedZone = 1;
		}

	    public HashSet<uint> GetEligiblePlayers(BattleFrontStatus activeBattleFrontStatus)
	    {
	        var eligiblePlayers = new HashSet<uint>();
	        ProgressionLogger.Debug($"** Kill Contribution players **");
	        foreach (var playerKillContribution in activeBattleFrontStatus.KillContributionSet)
	        {
	            eligiblePlayers.Add(playerKillContribution);
	        }
	        ProgressionLogger.Debug($"{string.Join(",", eligiblePlayers.ToArray())}");
	        ProgressionLogger.Debug($"** Objective Contribution players **");
	        foreach (var campaignObjective in GetActiveCampaign().Objectives)
	        {
	            ProgressionLogger.Debug($"** Objective Contribution for {campaignObjective.Name} **");
	            var contributionList = campaignObjective.CampaignObjectiveContributions;
	            foreach (var playerObjectiveContribution in contributionList)
	            {
	                eligiblePlayers.Add(playerObjectiveContribution.Key);
	            }
	            ProgressionLogger.Debug($"{string.Join(",", contributionList.ToArray())}");
	        }
	        ProgressionLogger.Debug($"All Eligible Players : {string.Join(",", eligiblePlayers.ToArray())}");

	        return eligiblePlayers;
	    }
    }
}
