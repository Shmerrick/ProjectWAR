using System.Collections.Generic;
using GameData;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class BattleFrontStatus
    {
        public int BattleFrontId { get; set; }
        public Realms LockingRealm { get; set; }
        public VictoryPointProgress FinalVictoryPoint { get; set; }
        public int OpenTimeStamp { get; set; }
        public int LockTimeStamp { get; set; }
        public bool Locked { get; set; }
        public int RegionId { get; set; }
        public string Description { get; set; }
        public ContributionManager ContributionManagerInstance { get; set; }
        public BountyManager BountyManagerInstance { get; set; }
        public RewardManager RewardManagerInstance { get; set; }
        public ImpactMatrixManager ImpactMatrixManagerInstance { get; set; }
        public HashSet<uint> KillContributionSet { get; set; }

        public BattleFrontStatus()
        {
            KillContributionSet = new HashSet<uint>();
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

        public BattleFrontStatus()
        {
            ContributionManagerInstance = new ContributionManager(new ConcurrentDictionary<uint, List<PlayerContribution>>(), BountyService._ContributionDefinitions);
            BountyManagerInstance = new BountyManager();
            ImpactMatrixManagerInstance = new ImpactMatrixManager();
            RewardManagerInstance = new RewardManager(BountyManagerInstance, ContributionManagerInstance, ImpactMatrixManagerInstance, new StaticWrapper(), RewardService._RewardBandRewards);
        }
    }
}