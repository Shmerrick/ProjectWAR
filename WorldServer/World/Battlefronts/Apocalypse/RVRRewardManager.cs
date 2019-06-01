using System;
using System.Collections.Generic;
using Common;
using FrameWork;
using GameData;
using NLog;
using WorldServer.World.Objects;

namespace WorldServer.World.Battlefronts.Apocalypse
{
	public enum BORewardType
	{
		CAPTURING = 0,
		GUARDED = 1,
		CAPTURED = 2
	}

    /// <summary>
    /// Manages rewards from RVR mechanic.
    /// </summary>
    public class RVRRewardManager
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public Dictionary<uint, XpRenown> DelayedRewards = new Dictionary<uint, XpRenown>();
        public float RewardScaleMultiplier { get; set; }
        public object PlrLevel { get; private set; }

        /// <summary>
        /// Add delayed XPR rewards for kills in RVR. 
        /// </summary>
        /// <param name="killer"></param>
        /// <param name="killed"></param>
        /// <param name="xpShare"></param>
        /// <param name="renownShare"></param>
        public void AddDelayedRewardsFrom(Player killer, Player killed, uint xpShare, uint renownShare)
        {
            if (xpShare == 0 && renownShare == 0)
                return;

            XpRenown xprEntry;

            uint renownReward = (uint)(renownShare * killer.GetKillRewardScaler(killed));

#if BattleFront_DEBUG
            player.SendClientMessage($"{ObjectiveName} storing {xpShare} XP and {renownReward} renown");
#endif
            DelayedRewards.TryGetValue(killer.CharacterId, out xprEntry);

            // Character has no record of XP/RR gain.
            if (xprEntry == null)
            {
                xprEntry = new XpRenown(xpShare, renownReward, 0, 0, TCPManager.GetTimeStamp());
                DelayedRewards.Add(killer.CharacterId, xprEntry);
            }
            else
            {
                xprEntry.XP += xpShare;
                xprEntry.Renown += renownReward;
                xprEntry.LastUpdatedTime = TCPManager.GetTimeStamp();
            }
            _logger.Debug($"Delayed Rewards for Player : {killer.Name}  - {xprEntry.ToString()}");
        }

		/// <summary>
		/// Return an additional scale value based upon who is holding a BattlefieldObjective and how many players from either side are near.
		/// </summary>
		/// <param name="owningRealm"></param>
		/// <param name="nearOrderCount"></param>
		/// <param name="nearDestroCount"></param>
		/// <returns></returns>
		public float CalculateObjectiveRewardScale(Realms owningRealm, short nearOrderCount, short nearDestroCount)
        {
			float scale = 0.0f;
            if (owningRealm == Realms.REALMS_REALM_DESTRUCTION)
            {
				scale = (float)Math.Abs(Math.Log(nearOrderCount / 10f + 1f));
            }
            else
            {
                if (owningRealm == Realms.REALMS_REALM_ORDER)
                {
                    scale = (float)Math.Abs(Math.Log(nearDestroCount / 10f + 1f));
				}
            }
			if (scale > 1f)
				scale = 1f;
			return scale;
        }
		
        /// <summary>
        ///     Grants a small reward to all players in close range for defending.
        /// </summary>
        /// <remarks>Invoked in short periods of time</remarks>
        public VictoryPoint RewardCaptureTick(ISet<Player> playersWithinRange, Realms owningRealm, int tier, string objectiveName, float rewardScaleMultiplier, BORewardType boRewardType)
        {
            ushort influenceId;

            _logger.Trace($"Objective {objectiveName} has {playersWithinRange} players (realm:{owningRealm}) nearby");

			// Because of the Field of Glory buff, the XP value here is doubled.
			// The base reward in T4 is therefore 3000 XP.
			// Population scale factors can up this to 9000 if the region is full of people and then raise or lower it depending on population balance.
            var baseXp = 0;
            var baseRp = 0;
            var baseInf = 0;

            if (boRewardType == BORewardType.CAPTURING)
            {
                baseXp = Program.Config.BOCapturingRewardXp;
                baseRp = Program.Config.BOCapturingRewardRp;
                baseInf = Program.Config.BOCapturingRewardInf;
            }
            if (boRewardType == BORewardType.CAPTURED)
            {
                baseXp = Program.Config.BOCapturedRewardXp;
                baseRp = Program.Config.BOCapturedRewardRp;
                baseInf = Program.Config.BOCapturedRewardInf;
            }
            if (boRewardType == BORewardType.GUARDED)
            {
                baseXp = Program.Config.BOGuardedRewardXp;
                baseRp = Program.Config.BOGuardedRewardRp;
                baseInf = Program.Config.BOGuardedRewardInf;
            }
            
            foreach (var player in playersWithinRange)
            {
                // if the BattlefieldObjective is Neutral, allow rewards. 
                if (owningRealm != Realms.REALMS_REALM_NEUTRAL)
                {
                    if (player.Realm != owningRealm || player.IsAFK || player.IsAutoAFK)
                        continue;
                }

                
                if (player.CurrentArea != null)
                {
                    if (player.Realm == Realms.REALMS_REALM_ORDER)
                        influenceId = (ushort) player.CurrentArea.OrderInfluenceId;
                    else
                        influenceId = (ushort) player.CurrentArea.DestroInfluenceId;

                    player.AddInfluence(influenceId, Math.Max((ushort)baseInf, (ushort)1));
					
				}

                Random rnd = new Random();
                int random = rnd.Next(-25, 25);
                var xp = (uint)Math.Max((baseXp * (1 + (random / 100))), 1);
                var rr = (uint) Math.Max((baseRp * (1 + (random / 100))), 1);
                
                player.AddXp(xp, false, false);
                player.AddRenown(rr, false, RewardType.ObjectiveCapture, objectiveName);
                
                _logger.Trace($"Player:{player.Name} ScaleMult:{rewardScaleMultiplier} XP:{xp} RR:{rr}");
            }

			VictoryPoint VP = new VictoryPoint(0, 0);
			switch (boRewardType)
			{
				case BORewardType.CAPTURING: // small tick
					if (owningRealm == Realms.REALMS_REALM_ORDER)
						VP.OrderVictoryPoints += 0;
					else if (owningRealm == Realms.REALMS_REALM_DESTRUCTION)
						VP.DestructionVictoryPoints += 0;
					break;

				case BORewardType.CAPTURED: // big tick
				    if (owningRealm == Realms.REALMS_REALM_ORDER)
				    {
				        VP.OrderVictoryPoints += 50;
				        VP.DestructionVictoryPoints -= 50;
                    }
				    else if (owningRealm == Realms.REALMS_REALM_DESTRUCTION)
				    {
				        VP.DestructionVictoryPoints += 50;
				        VP.OrderVictoryPoints -= 50;
                    }

				    break;

				case BORewardType.GUARDED: // small tick
					if (owningRealm == Realms.REALMS_REALM_ORDER)
						VP.OrderVictoryPoints += 0;
					else if (owningRealm == Realms.REALMS_REALM_DESTRUCTION)
						VP.DestructionVictoryPoints += 0;
					break;

				default:
					break;
			}
			return VP;
		}
		
        
    }
}
