using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldServer.World.Battlefronts.Bounty
{

    /// <summary>
    /// Each character (player) holds a list of contributions that they have earnt in a specific battlefront.
    /// </summary>


    //TODO : Attach this to BattlefrontStatus object. (Which we probably want to persist).
    public class ContributionManager : IContributionManager
    {
        public const int MAX_CONTRIBUTION = 110;

        // Holds the characterId, and the list of contributions the player has added in the current battlefront.
        public ConcurrentDictionary<uint, List<PlayerContribution>> ContributionDictionary { get; set; }
        // Reference contribution factors
        public List<ContributionFactor> ContributionFactors { get; }

        public ContributionManager(ConcurrentDictionary<uint, List<PlayerContribution>> contributionDictionary, List<ContributionFactor> contributionFactors)
        {
            ContributionDictionary = contributionDictionary;
            ContributionFactors = contributionFactors;
        }

        public uint UpdateContribution(uint targetCharacterId, byte contibutionId)
        {
            //var contributionFactor = ContributionFactors.SingleOrDefault(x => x.ContributionId == contibutionId);
            var item = GetContribution(targetCharacterId);
            item.Add(new PlayerContribution
            {
                ContributionId = contibutionId,
                Timestamp = FrameWork.TCPManager.GetTimeStamp()
            });

            return this.ContributionDictionary.AddOrUpdate(
                targetCharacterId,
                item,
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

        public List<PlayerContribution> GetContribution(uint targetCharacterId)
        {
            if (this.ContributionDictionary.ContainsKey(targetCharacterId))
                return this.ContributionDictionary[targetCharacterId];
            else
                throw new BountyException($"Could not locate target character {targetCharacterId} for contribution");

        }

        /// <summary>
        /// Add Character to Contribution Dictionary
        /// </summary>
        public void AddCharacter(uint characterId, byte contributionId)
        {
            UpdateContribution(characterId, contributionId);
        }

        /// <summary>
        /// Remove Character from Contribution Dict
        /// </summary>
        /// <param name="characterId"></param>
        public void RemoveCharacter(uint characterId)
        {
            List<PlayerContribution> contribution;
            this.ContributionDictionary.TryRemove(characterId, out contribution);
        }

        public void Clear()
        {
            this.ContributionDictionary.Clear();
        }
    }

    /// <summary>
    /// Structure to hold the defintion of contributions.
    /// </summary>
    public class ContributionFactor
    {
        public byte ContributionId { get; set; }
        public string ContributionDescription { get; set; }
        public byte ContributionValue { get; set; }
        public byte MaxContributionCount { get; set; }

    }

    /// <summary>
    /// The actual class to be added to the Contribution dictionary
    /// </summary>
    public class PlayerContribution
    {
        public byte ContributionId { get; set; }
        
        public long Timestamp { get; set; }
    }
}
