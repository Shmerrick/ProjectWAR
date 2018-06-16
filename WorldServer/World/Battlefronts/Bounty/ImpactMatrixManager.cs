using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldServer.World.Battlefronts.Bounty
{
    public class ImpactMatrixManager
    {
        public ConcurrentDictionary<uint, PlayerImpact> ImpactMatrix { get; set; }
        public const int IMPACT_EXPIRY_TIME = 60;

        public ImpactMatrixManager()
        {
            ImpactMatrix = new ConcurrentDictionary<uint, PlayerImpact>();
        }

        public void AddOrUpdate(Player target, PlayerImpact playerImpact)
        {
            this.ImpactMatrix.AddOrUpdate(target.CharacterId, playerImpact, (key, existingImpact) =>
            {
                var newPlayerImpact = new PlayerImpact();
                newPlayerImpact.SetImpact(
                    existingImpact.ImpactValue + playerImpact.ImpactValue, 
                    FrameWork.TCPManager.GetTimeStamp() + IMPACT_EXPIRY_TIME, 
                    playerImpact.ModificationValue, 
                    playerImpact.player);
                return newPlayerImpact;

            });
        }
    }


}
