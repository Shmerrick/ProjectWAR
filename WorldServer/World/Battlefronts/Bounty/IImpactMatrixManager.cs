using System.Collections.Concurrent;
using System.Collections.Generic;

namespace WorldServer.World.Battlefronts.Bounty
{
    public interface IImpactMatrixManager
    {
        ConcurrentDictionary<uint, List<PlayerImpact>> ImpactMatrix { get; set; }

        int ExpireImpacts(int expiryTime);
        void FullHeal(uint targetCharacterId);
        List<PlayerImpact> GetKillImpacts(uint targetCharacterId);
        int GetTotalImpact(uint targetCharacterId);
        PlayerImpact UpdateMatrix(uint targetCharacterId, PlayerImpact playerImpact);
        void ClearImpacts(uint targetCharacterId);
        float CalculateModificationValue(float targetBaseBounty, float killerBaseBounty);
    }
}