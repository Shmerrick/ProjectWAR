using System.Collections.Concurrent;
using System.Collections.Generic;
using GameData;
using NLog;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Bounty;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class BattleFrontStatus
    {
        private static readonly Logger BattlefrontLogger = LogManager.GetLogger("BattlefrontLogger");

        public int BattleFrontId { get; set; }
        public Realms LockingRealm { get; set; }
        public VictoryPointProgress FinalVictoryPoint { get; set; }
        public int OpenTimeStamp { get; set; }
        public int LockTimeStamp { get; set; }
        public bool Locked { get; set; }
        public int RegionId { get; set; }
        public string Description { get; set; }
        public ContributionManager ContributionManagerInstance { get; set; }
        public ImpactMatrixManager ImpactMatrixManagerInstance { get; set; }

        public RewardManager RewardManagerInstance { get; set; }
        
        public HashSet<uint> KillContributionSet { get; set; }
        public List<Player> RealmCaptains { get; set; }

        public BattleFrontStatus()
        {
            ContributionManagerInstance = new ContributionManager(
                new ConcurrentDictionary<uint, List<PlayerContribution>>(), 
                BountyService._ContributionDefinitions);
            
            RewardManagerInstance = new RewardManager(ContributionManagerInstance, new StaticWrapper(), RewardService._RewardPlayerKills, ImpactMatrixManagerInstance);
            RealmCaptains = new List<Player>();
        }

        public float DestructionVictoryPointPercentage
        {
            get { return FinalVictoryPoint.DestructionVictoryPointPercentage; }
        }

        public float OrderVictoryPointPercentage
        {
            get { return FinalVictoryPoint.OrderVictoryPointPercentage; }
        }

        /// <summary>
        /// Get the lock status of this battlefront (for use in communicating with the client)
        /// </summary>
        public int LockStatus
        {
            get
            {
                if (Locked)
                {
                    if (LockingRealm == Realms.REALMS_REALM_DESTRUCTION)
                        return BattleFrontConstants.ZONE_STATUS_DESTRO_LOCKED;
                    if (LockingRealm == Realms.REALMS_REALM_ORDER)
                        return BattleFrontConstants.ZONE_STATUS_ORDER_LOCKED;
                }
                return BattleFrontConstants.ZONE_STATUS_CONTESTED;
            }
        }

        

        public void AddKillContribution(Player player)
        {
            KillContributionSet.Add(player.CharacterId);
        }


        public void RemoveAsRealmCaptain(Player realmCaptain)
        {
            BattlefrontLogger.Info($"Removing player {realmCaptain.Name} as Captain");
            this.RealmCaptains.Remove(realmCaptain);
        }

        public void SetAsRealmCaptain(Player realmCaptain)
        {
            BattlefrontLogger.Info($"Adding player {realmCaptain.Name} as Captain");
            this.RealmCaptains.Add(realmCaptain);

        }
    }
}