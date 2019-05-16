using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Common.Database.World.Battlefront;
using GameData;
using NLog;
using WorldServer.World.Objects;

namespace WorldServer.World.Battlefronts.Bounty
{
    public interface IContributionManager
    {
        // Add a new contribution factor to the player's list of contributions.
        List<PlayerContribution> UpdateContribution(uint targetCharacterId, byte contibutionId);
        // Return the contribution list items
        List<PlayerContribution> GetContribution(uint targetCharacterId);
        // Return the value of the contribution for reward calculations.
        short GetContributionValue(uint targetCharacterId);

        bool RemoveCharacter(uint characterId);

        void Clear();
        void AddCharacter(uint characterId);
        IEnumerable<KeyValuePair<uint, int>> GetEligiblePlayers(int numberOfBags);

        ConcurrentDictionary<short, ContributionStage> GetContributionStageDictionary(
            List<PlayerContribution> contributionList,
            List<ContributionDefinition> contributionFactors);

        ConcurrentDictionary<short, ContributionStage> GetContributionStageDictionary(uint targetCharacterId);

        Tuple<ConcurrentDictionary<Player, int>, ConcurrentDictionary<Player, int>, ConcurrentDictionary<Player, int>>
            DetermineEligiblePlayers(ILogger logger, Realms lockingRealm);

        int GetMaximumContribution();
    }
}