using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldServer.World.Battlefronts.Bounty
{

    public class ContributionManager : IContributionManager
    {
        public const int MAX_CONTRIBUTION = 110;

        // Holds the characterId, the last death time since epoch, and the amount of bounty
        public ConcurrentDictionary<uint, uint> ContributionDictionary { get; set; }

        public ContributionManager()
        {
            ContributionDictionary = new ConcurrentDictionary<uint, uint>();
        }

        public uint UpdateContribution(uint targetCharacterId, uint contribution)
        {
            return this.ContributionDictionary.AddOrUpdate(
                targetCharacterId,
                contribution,
                (key, existingContibution) =>
                {
                    var newContribution = existingContibution + contribution;
                    if (newContribution > MAX_CONTRIBUTION)
                    {
                        return MAX_CONTRIBUTION;
                    }
                    return newContribution;
                });
        }

        public uint GetContribution(uint targetCharacterId)
        {
            if (this.ContributionDictionary.ContainsKey(targetCharacterId))
                return this.ContributionDictionary[targetCharacterId];
            else
                throw new BountyException($"Could not locate target character {targetCharacterId} for contribution");

        }

        /// <summary>
        /// Add Character to Contribution Dictionary
        /// </summary>
        /// <param name="characterId"></param>
        /// <param name="characterLevel"></param>
        /// <param name="renownLevel"></param>
        public void AddCharacter(uint characterId, uint contribution)
        {
            UpdateContribution(characterId, contribution);
        }

        /// <summary>
        /// Remove Character from Contribution Dict
        /// </summary>
        /// <param name="characterId"></param>
        public void RemoveCharacter(uint characterId)
        {
            uint contribution;
            this.ContributionDictionary.TryRemove(characterId, out contribution);
        }
    }
}
