using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldServer.World.Battlefronts.Bounty
{

    public class ImpactMatrixManager : IImpactMatrixManager
    {
        public ConcurrentDictionary<uint, List<PlayerImpact>> ImpactMatrix { get; set; }
        // Number of seconds until the impact is removed from the ImpactMatrix.
        public const int IMPACT_EXPIRY_TIME = 60;
        // Maximum number of PlayerImpacts to return when calculating a kill
        public const int MAX_REWARD_IMPACT_COUNT = 20;
        // Minimum impact value for the record to be stored in the Impact Matrix
        public const int MIN_IMPACT_VALIDITY = 50;


        public ImpactMatrixManager()
        {
            ImpactMatrix = new ConcurrentDictionary<uint, List<PlayerImpact>>();
        }

      
        /// <summary>
        /// Update the Impact matrix with a PlayerImpact object. (Create new if required)
        /// </summary>
        /// <param name="targetCharacterId"></param>
        /// <param name="playerImpact"></param>
        /// <returns></returns>
        public PlayerImpact UpdateMatrix(uint targetCharacterId, PlayerImpact playerImpact)
        {
            // Only add if minimum value passed.
            if (playerImpact.ImpactValue < MIN_IMPACT_VALIDITY)
                return playerImpact;

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

        /// <summary>
        /// Scan over the Impact matrix and remove any PlayerImpact objects that have expired.
        /// </summary>
        /// <param name="expiryTime"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Once killed, return the list of PlayerImpacts for reward calculation
        /// </summary>
        /// <param name="targetCharacterId"></param>
        /// <returns></returns>
        public List<PlayerImpact> GetKillImpacts(uint targetCharacterId)
        {
            if (this.ImpactMatrix.ContainsKey(targetCharacterId))
            {
                return ImpactMatrix[targetCharacterId].Take(MAX_REWARD_IMPACT_COUNT).OrderBy(o => o.ImpactValue).ToList();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///  Returns the sum of the total impact for this target.
        /// </summary>
        /// <param name="targetCharacterId"></param>
        /// <returns></returns>
        public int GetTotalImpact(uint targetCharacterId)
        {
            return GetKillImpacts(targetCharacterId).Sum(x => x.ImpactValue);
        }


    }


}
