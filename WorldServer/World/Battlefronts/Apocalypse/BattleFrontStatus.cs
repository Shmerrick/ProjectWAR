using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameWork;
using GameData;
using WorldServer.World.Battlefronts.Apocalypse;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class ApocBattleFrontStatus
    {
        public int BattleFrontId { get; set; }
        public Realms LockingRealm { get; set; }
        public VictoryPointProgress FinalVictoryPoint { get; set; }
        public int OpenTimeStamp { get; set; }
        public int LockTimeStamp { get; set; }
        public bool Locked { get; set; }
        public int RegionId { get; set; }

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

        
    }
}

