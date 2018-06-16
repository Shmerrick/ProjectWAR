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
        public ConcurrentDictionary<uint, List<PlayerImpact>> ImpactMatrix { get; set; }
        public const int IMPACT_EXPIRY_TIME = 60;

        public ImpactMatrixManager()
        {
            ImpactMatrix = new ConcurrentDictionary<uint, List<PlayerImpact>>();
        }

        public PlayerImpact UpdateMatrix(uint targetCharacterId, PlayerImpact playerImpact)
        {

            if (this.ImpactMatrix.ContainsKey(targetCharacterId))
            {
                var targetPlayerImpactList = ImpactMatrix[targetCharacterId];
                // Look for player in playerimpact
                foreach (var impact in targetPlayerImpactList)
                {
                    if (impact.CharacterId == playerImpact.CharacterId)
                    {
                        // Found the attacker, updating.
                        return  impact.SetImpact(
                            impact.ImpactValue + playerImpact.ImpactValue,
                            playerImpact.ExpiryTimestamp + IMPACT_EXPIRY_TIME,
                            playerImpact.ModificationValue,
                            playerImpact.CharacterId);
                    }
                }
                // Couldnt find the player in playerimpact, add.
                targetPlayerImpactList.Add(playerImpact);
            }
            else
            {
                // No dictionary entry
                this.ImpactMatrix.TryAdd(targetCharacterId, new List<PlayerImpact> {playerImpact});
                return playerImpact;
            }
            return null;
        }

        public int ExpireImpacts(int expiryTime)
        {
            int removedCount = 0;
            List<PlayerImpact> removedPlayerImpactList;

            foreach (KeyValuePair<uint, List<PlayerImpact>> entry in this.ImpactMatrix)
            {
                foreach (var playerImpact in entry.Value)
                {
                    if (playerImpact.ExpiryTimestamp < expiryTime)
                    {
                        if (this.ImpactMatrix.TryRemove(entry.Key, out removedPlayerImpactList))
                            removedCount++;
                    }
                }
            }
            return removedCount;
        }

        /// <summary>
        /// Target character has been healed to full, clear their impactList
        /// </summary>
        /// <param name="targetCharacterId"></param>
        public void FullHeal(uint targetCharacterId)
        {
            if (this.ImpactMatrix.ContainsKey(targetCharacterId))
            {
                ImpactMatrix[targetCharacterId].Clear();
            }
        }

    }


}
