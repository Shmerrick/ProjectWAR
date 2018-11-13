using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Database.World.Battlefront;
using GameData;
using NLog;
using WorldServer.Services.World;

namespace WorldServer.World.Battlefronts.Bounty
{

    /// <summary>
    /// Each character (player) holds a list of contributions that they have earnt in a specific battlefront.
    /// </summary>
    
    public class ContributionManager : IContributionManager
    {
        private static readonly Logger RewardLogger = LogManager.GetLogger("RewardLogger");
        private readonly Object _lockObject = new Object();
        // Holds the characterId, and the list of contributions the player has added in the current battlefront.
        public ConcurrentDictionary<uint, List<PlayerContribution>> ContributionDictionary { get; set; }

        // Reference contribution factors
        public List<ContributionDefinition> ContributionFactors { get; }

        public BountyService BountyService { get; }

        public const short MAXIMUM_CONTRIBUTION = 100;

        public ContributionManager(ConcurrentDictionary<uint, List<PlayerContribution>> contributionDictionary, List<ContributionDefinition> contributionFactors)
        {
            ContributionDictionary = contributionDictionary;
            ContributionFactors = contributionFactors;
            
        }

        /// <summary>
        /// Update the dictionary of contribution held by the server for the character. Can hold unlimited numbers
        /// of contributions, these will be culled in a later stage of the process.
        /// </summary>
        /// <param name="targetCharacterId">The character receiving the contribution</param>
        /// <param name="contributionId">Reference contribution Id</param>
        /// <returns></returns>
        public List<PlayerContribution> UpdateContribution(uint targetCharacterId, byte contributionId)
        {
            var contributionDefinition = ContributionFactors.Single(x => x.ContributionId == contributionId);

            RewardLogger.Debug($"Assigning contibution Id {contributionId} ({contributionDefinition.ContributionDescription}) value of {contributionDefinition.ContributionValue} to {targetCharacterId}");
            
            //filteredResults.AddOrUpdate(unfilteredResult.Key, new List<int> { number }, (k, v) => v.Add(number));
            return this.ContributionDictionary.AddOrUpdate(targetCharacterId,
                 new List<PlayerContribution>
                 {
                        new PlayerContribution
                        {
                            ContributionId = contributionId,
                            Timestamp = FrameWork.TCPManager.GetTimeStamp()
                        }
                 }, (k, v) =>
                 {
                     v.Add(new PlayerContribution
                     {
                         ContributionId = contributionId,
                         Timestamp = FrameWork.TCPManager.GetTimeStamp()
                     });
                     return v;
                 });
        }

        /// <summary>
        /// Get the list of playercontributions 
        /// </summary>
        /// <param name="targetCharacterId"></param>
        /// <returns></returns>
        public List<PlayerContribution> GetContribution(uint targetCharacterId)
        {
            List<PlayerContribution> contributionList;

            this.ContributionDictionary.TryGetValue(targetCharacterId, out contributionList);

            return contributionList;
        }
        /// <summary>
        /// Get the total contribution value for the character.
        /// </summary>
        /// <param name="targetCharacterId"></param>
        /// <returns></returns>
        public short GetContributionValue(uint targetCharacterId)
        {
            List<PlayerContribution> contributionList;

            this.ContributionDictionary.TryGetValue(targetCharacterId, out contributionList);

            short contributionValue = 0;

            if (contributionList == null)
                return contributionValue;

            if (ContributionFactors == null)
                return contributionValue;

            var stagedContribution = GetContributionStageDictionary(contributionList, this.ContributionFactors);

            foreach (var contributionStage in stagedContribution)
            {
                contributionValue += contributionStage.Value.ContributionStageSum;
            }

            // double check something is not right.
            if (contributionValue > short.MaxValue)
            {
                RewardLogger.Error($"ContributionManagerInstance exceeds max (over short.maxvalue) for Character {targetCharacterId}. {contributionList.Count} contribution records.");
                contributionValue = short.MaxValue;
            }

            if (contributionValue > MAXIMUM_CONTRIBUTION)
            {
                RewardLogger.Error($"ContributionManagerInstance exceeds max ({MAXIMUM_CONTRIBUTION}) for Character {targetCharacterId}. {contributionList.Count} contribution records.");
                contributionValue = MAXIMUM_CONTRIBUTION;

            }

            RewardLogger.Trace($"Returning contributionValue of {contributionValue} for {targetCharacterId} ");

            return contributionValue;
        }

        /// <summary>
        /// Get the contribution dictionary for the character.
        /// </summary>
        /// <param name="targetCharacterId"></param>
        /// <returns></returns>
        public ConcurrentDictionary<short, ContributionStage> GetContributionStageList(uint targetCharacterId)
        {
            List<PlayerContribution> contributionList;

            this.ContributionDictionary.TryGetValue(targetCharacterId, out contributionList);

            short contributionValue = 0;

            if (contributionList == null)
                return null;

            if (ContributionFactors == null)
                return null;

           return GetContributionStageDictionary(contributionList, this.ContributionFactors);
            
        }

        /// <summary>
        /// Convert list of player contributions (which are unlimited in number) into a 'staged' dictionary per reference contribution type.
        /// Limit the number of attained contributions to the maximum number defined in the reference contribution type.
        /// 
        /// </summary>
        /// <param name="contributionList"></param>
        /// <param name="contributionFactors"></param>
        /// <returns></returns>
        public ConcurrentDictionary<short, ContributionStage> GetContributionStageDictionary(List<PlayerContribution> contributionList, List<ContributionDefinition> contributionFactors)
        {
            var result = new ConcurrentDictionary<short, ContributionStage>();
            // For each reference contribution type, prepare a dictionary.
            foreach (var referenceContribution in contributionFactors)
            {
                // Dont worry about max contribution count == 0, these are effectively disabled.
                if (referenceContribution.MaxContributionCount > 0)
                {
                    result.TryAdd((short) referenceContribution.ContributionId,
                        new ContributionStage
                        {
                            Description = referenceContribution.ContributionDescription,
                            ContributionStageCount = 0,
                            ContributionStageSum = 0,
                            ContributionStageValue = 0,
                            ContributionStageMax = referenceContribution.MaxContributionCount
                        });
                }
            }

            foreach (var contributionFactor in ContributionFactors)
            {
                foreach (var playerContribution in contributionList)
                {
                    if (playerContribution.ContributionId == contributionFactor.ContributionId)
                    {
                        // If we need to add something is wrong, dont add it - return a new structure.
                        result.AddOrUpdate((short) contributionFactor.ContributionId,
                            new ContributionStage
                            {
                                Description = contributionFactor.ContributionDescription,
                                ContributionStageCount = 1,
                                ContributionStageSum = contributionFactor.ContributionValue,
                                ContributionStageMax = contributionFactor.MaxContributionCount,
                                ContributionStageValue = contributionFactor.ContributionValue
                            },
                            (k, v) =>
                        {
                            // If we have more than we need, skip.
                            if (v.ContributionStageCount + 1 <= v.ContributionStageMax)
                            {
                                v.ContributionStageCount = (short)(v.ContributionStageCount + 1);
                                v.ContributionStageSum += contributionFactor.ContributionValue;
                                v.ContributionStageValue = contributionFactor.ContributionValue;
                            }

                            return v;
                        });

                    }
                }
            }

            foreach (var contributionStage in result)
            {
                RewardLogger.Trace($"Id:{contributionStage.Key} {contributionStage.Value.ToString()}");
            }
            

            return result;

        }



        /// <summary>
        /// Add Character to ContributionManagerInstance Dictionary
        /// </summary>
        public void AddCharacter(uint characterId)
        {
            UpdateContribution(characterId, 0);
        }

        /// <summary>
        /// Remove Character from ContributionManagerInstance Dict
        /// </summary>
        /// <param name="characterId"></param>
        public bool RemoveCharacter(uint characterId)
        {
            List<PlayerContribution> contribution;
            return this.ContributionDictionary.TryRemove(characterId, out contribution);
        }

        /// <summary>
        /// Clear the contribution dictionary
        /// </summary>
        public void Clear()
        {
            this.ContributionDictionary.Clear();
        }

   
        /// <summary>
        /// Return an ordered list of eligible players based on the highest contribution
        /// </summary>
        /// <param name="numberOfBags"></param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<uint, int>> GetEligiblePlayers(int numberOfBags)
        {
            var summationDictionary = new ConcurrentDictionary<uint, int>();

            //Flatten out the characterId and contribution value.
            foreach (var dictionaryItem in ContributionDictionary)
            {
                var contributionValue = GetContributionValue(dictionaryItem.Key);
                summationDictionary.TryAdd(dictionaryItem.Key, contributionValue);
            }
            // Number of awards = 0 -> return all.
            if (numberOfBags == 0)
            {
                return summationDictionary.OrderBy(x => x.Value);
            }
            else
            {
                return summationDictionary.OrderBy(x => x.Value).Take(numberOfBags);
            }
        }

        public List<Player> GetHigestContributors(int minimumContribution, IEnumerable<Player> players)
        {
            Player destructionRealmCaptain = null;
            Player orderRealmCaptain = null;
            var returnList = new List<Player>();
            // Return ordered (contrib descending) list of Eligible Players
            var eligiblePlayers = GetEligiblePlayers(0);
            foreach (var eligiblePlayer in eligiblePlayers)
            {
                if (eligiblePlayer.Value < minimumContribution)
                    continue;
                
                // If the player is found in the list.
                var player = players.SingleOrDefault(x => x.CharacterId == eligiblePlayer.Key);
                if (player != null)
                {
                    if (player.Realm == Realms.REALMS_REALM_DESTRUCTION)
                    {
                        if (destructionRealmCaptain == null)
                        {
                            destructionRealmCaptain = player;
                            returnList.Add(destructionRealmCaptain);

                            RewardLogger.Info($"Assigning {destructionRealmCaptain.Name} as RealmCaptain");
                        }
                    }
                    else
                    {
                        if (orderRealmCaptain == null)
                        {
                            orderRealmCaptain = player;
                            returnList.Add(orderRealmCaptain);

                            RewardLogger.Info($"Assigning {orderRealmCaptain.Name} as RealmCaptain");
                        }
                    }
                }
                if ((orderRealmCaptain != null) && (destructionRealmCaptain != null))
                    return returnList;
            }
            return returnList;
        }
    }
}
