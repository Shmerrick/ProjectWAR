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
        private readonly Object _lockObject = new Object();
        // Holds the characterId, and the list of contributions the player has added in the current battlefront.
        public ConcurrentDictionary<uint, List<PlayerContribution>> ContributionDictionary { get; set; }
        // Reference contribution factors
        public List<ContributionFactor> ContributionFactors { get; }

        public ContributionManager(ConcurrentDictionary<uint, List<PlayerContribution>> contributionDictionary, List<ContributionFactor> contributionFactors)
        {
            ContributionDictionary = contributionDictionary;
            ContributionFactors = contributionFactors;
        }

        public List<PlayerContribution> UpdateContribution(uint targetCharacterId, byte contibutionId)
        {

            lock (_lockObject)
            {
                var item = GetContribution(targetCharacterId);

                //filteredResults.AddOrUpdate(unfilteredResult.Key, new List<int> { number }, (k, v) => v.Add(number));
               return this.ContributionDictionary.AddOrUpdate(targetCharacterId,
                    new List<PlayerContribution>
                    {
                        new PlayerContribution
                        {
                            ContributionId = contibutionId,
                            Timestamp = FrameWork.TCPManager.GetTimeStamp()
                        }
                    }, (k, v) =>
                    {
                        v.Add(new PlayerContribution
                        {
                            ContributionId = contibutionId,
                            Timestamp = FrameWork.TCPManager.GetTimeStamp()
                        });
                        return v;
                    });

                //if (item == null)
                //{
                //    var newPlayerContributionList = new List<PlayerContribution>()>;
                //    newPlayerContributionList.Add(new PlayerContribution
                //    {
                //        ContributionId = contibutionId,
                //        Timestamp = FrameWork.TCPManager.GetTimeStamp()
                //    });
                //}

                //    item.Add(new PlayerContribution
                //    {
                //        ContributionId = contibutionId,
                //        Timestamp = FrameWork.TCPManager.GetTimeStamp()
                //    });

                //return this.ContributionDictionary.TryAdd(targetCharacterId, item);
            }

            //return this.ContributionDictionary.AddOrUpdate(
            //    targetCharacterId,
            //    item,
            //    (key, existingContibution) =>
            //    {
            //        var newContribution = existingContibution + contribution;
            //        if (newContribution > MAX_CONTRIBUTION)
            //        {
            //            return MAX_CONTRIBUTION;
            //        }
            //        return newContribution;
            //    });
        }

        public List<PlayerContribution> GetContribution(uint targetCharacterId)
        {
            List<PlayerContribution> contributionList;

            this.ContributionDictionary.TryGetValue(targetCharacterId, out contributionList);

            return contributionList;
        }

        public short GetContributionValue(uint targetCharacterId)
        {
            List<PlayerContribution> contributionList;

            this.ContributionDictionary.TryGetValue(targetCharacterId, out contributionList);

            short contributionValue = 0;

            if (contributionList == null)
                return contributionValue;

            if (ContributionFactors == null)
                return contributionValue;

            foreach (var playerContribution in contributionList)
            {
                foreach (var contributionFactor in ContributionFactors)
                {
                    if (contributionFactor.ContributionId == playerContribution.ContributionId)
                    {
                        contributionValue += contributionFactor.ContributionValue;
                    }
                }
            }

            return contributionValue;
        }

        /// <summary>
        /// Add Character to Contribution Dictionary
        /// </summary>
        public void AddCharacter(uint characterId)
        {
            UpdateContribution(characterId, 0);
        }

        /// <summary>
        /// Remove Character from Contribution Dict
        /// </summary>
        /// <param name="characterId"></param>
        public bool RemoveCharacter(uint characterId)
        {
            List<PlayerContribution> contribution;
            return this.ContributionDictionary.TryRemove(characterId, out contribution);
        }

        public void Clear()
        {
            this.ContributionDictionary.Clear();
        }
    }
}
