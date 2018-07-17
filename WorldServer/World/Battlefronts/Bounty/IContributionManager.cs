using System.Collections.Concurrent;
using System.Collections.Generic;

namespace WorldServer.World.Battlefronts.Bounty
{
    public interface IContributionManager
    {
        uint UpdateContribution(uint targetCharacterId, uint contribution);
        List<PlayerContribution> GetContribution(uint targetCharacterId);

        void Clear();
    }
}