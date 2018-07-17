using System.Collections.Concurrent;
using System.Collections.Generic;

namespace WorldServer.World.Battlefronts.Bounty
{
    public interface IContributionManager
    {
        // Add a new contribution factor to the player's list of contributions.
        bool UpdateContribution(uint targetCharacterId, byte contibutionId);
        // Return the contribution list items
        List<PlayerContribution> GetContribution(uint targetCharacterId);
        // Return the value of the contribution for reward calculations.
        short GetContributionValue(uint targetCharacterId);

        void Clear();
    }
}