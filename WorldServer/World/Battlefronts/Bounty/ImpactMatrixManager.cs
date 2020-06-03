using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using WorldServer.World.Interfaces;

namespace WorldServer.World.Battlefronts.Bounty
{
    
    public class ImpactMatrixManager : IImpactMatrixManager
    {
        /// <summary>
        /// The ImpactMatrix records the list of impacts upon a character (the Key) by an attacker (represented by the Value). 
        /// </summary>
        public ConcurrentDictionary<uint, List<PlayerImpact>> ImpactMatrix { get; set; }
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        // Number of seconds until the impact is removed from the ImpactMatrix.
        public const int IMPACT_EXPIRY_TIME = 60;
        // Maximum number of PlayerImpacts to return when calculating a kill
        public const int MAX_REWARD_IMPACT_COUNT = 20;
        // Minimum impact value for the record to be stored in the Impact Matrix
        public const int MIN_IMPACT_VALIDITY = 50;
        // The multiplier in the log curve for the impact of the bounty differences. Make this higher to see more pronounced effects between RR differences
        public const float MODIFICATION_VALUE_MULTIPLIER = 3f;
        protected readonly EventInterface _EvtInterface = new EventInterface();

        public ImpactMatrixManager()
        {
            ImpactMatrix = new ConcurrentDictionary<uint, List<PlayerImpact>>();
            _EvtInterface.AddEvent(DecayImpactMatrix, 60000, 0);
        }

        private void DecayImpactMatrix()
        {
            _logger.Trace("Attempting to decay impact matrix");
            this.ExpireImpacts(FrameWork.TCPManager.GetTimeStamp());
        }

        public void Update(long tick)
        {
            _EvtInterface.Update(tick);
        }



        public string ToString(uint characterId)
        {
            var result = string.Empty;

            var killImpacts = GetKillImpacts(characterId);
            foreach (var playerImpact in killImpacts)
            {
                result += $"{playerImpact.CharacterId} {playerImpact.ImpactValue} {playerImpact.ModificationValue} {playerImpact.ExpiryTimestamp}";
            }
            return result;
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

            lock (ImpactMatrix)
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
                            return impact.SetImpact(
                                impact.ImpactValue + playerImpact.ImpactValue,
                                playerImpact.ExpiryTimestamp + IMPACT_EXPIRY_TIME,
                                playerImpact.ModificationValue,
                                playerImpact.CharacterId);

                        }
                    }
                    // Couldnt find the player in playerimpact, add.
                    targetPlayerImpactList.Add(playerImpact);
                    return playerImpact;
                }
                else
                {
                    // No dictionary entry
                    this.ImpactMatrix.TryAdd(targetCharacterId, new List<PlayerImpact> {playerImpact});
                    return playerImpact;
                }
            }
            return null;
        }

        /// <summary>
        /// Clear the impacts upon the character
        /// </summary>
        /// <param name="targetCharacterId"></param>
        public void ClearImpacts(uint targetCharacterId)
        {
            if (this.ImpactMatrix.ContainsKey(targetCharacterId))
            {
                ImpactMatrix[targetCharacterId].Clear();
            }
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
                    _logger.Trace($"Checking for decay : {playerImpact.CharacterId} {playerImpact.ImpactValue} {playerImpact.ExpiryTimestamp}");
                    if (playerImpact.ExpiryTimestamp < expiryTime)
                    {
                        if (this.ImpactMatrix.TryRemove(entry.Key, out removedPlayerImpactList))
                            removedCount++;
                    }
                }
            }
            if (removedCount != 0)
                _logger.Debug($"Removed {removedCount} impacts");
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
            var killImpacts = GetKillImpacts(targetCharacterId);
            if (killImpacts == null) 
                return 0;
            else
            {
                return killImpacts.Sum(x => x.ImpactValue);
            }
        }


        public float CalculateModificationValue(float targetBaseBounty, float killerBaseBounty)
        {
            var result =  (float)Math.Log((targetBaseBounty / (killerBaseBounty) + 1), 2) * MODIFICATION_VALUE_MULTIPLIER;

            return result;
        }

        public bool HasImpacts(uint characterId)
        {
            return this.ImpactMatrix.ContainsKey(characterId);
        }

    }   


}
