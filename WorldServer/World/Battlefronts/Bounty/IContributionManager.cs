using System.Collections.Concurrent;

namespace WorldServer.World.Battlefronts.Bounty
{
    public interface IContributionManager
    {
        uint UpdateContribution(uint targetCharacterId, uint contribution);
        uint GetContribution(uint targetCharacterId);
    }
}