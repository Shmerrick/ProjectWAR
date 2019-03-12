using System.Collections.Generic;
using Common.Database.World.Battlefront;
using GameData;
using WorldServer.World.Battlefronts.Bounty;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public interface IBattleFrontManager
    {
        RVRProgression GetActiveBattleFrontFromProgression();
        RVRProgression GetBattleFrontByName(string name);
        RVRProgression GetBattleFrontByBattleFrontId(int id);
        
        string ActiveBattleFrontName { get; set; }
        RVRProgression ActiveBattleFront { get; set; }
        void AuditBattleFronts(int tier);
        void LockBattleFrontsAllRegions(int tier, bool forceDefaultRealm = false);

        RVRProgression AdvanceBattleFront(Realms lockingRealm);
		RVRProgression OpenActiveBattlefront();
        RVRProgression LockActiveBattleFront(Realms realm, int forceNumberOfBags = 0);

        List<BattleFrontStatus> GetBattleFrontStatusList();
        bool IsBattleFrontLocked(int battleFrontId);

        BattleFrontStatus GetBattleFrontStatus(int battleFrontId);
        void LockBattleFrontStatus(int battleFrontId, Realms lockingRealm, VictoryPointProgress vpp);
        BattleFrontStatus GetRegionBattleFrontStatus(int regionId);

        Campaign GetActiveCampaign();

        BattleFrontStatus GetActiveBattleFrontStatus(int battleFrontId);

        void Update(long tick);
        ImpactMatrixManager ImpactMatrixManagerInstance { get; set; }
        BountyManager BountyManagerInstance { get; set; }

        List<RVRProgression> BattleFrontProgressions { get; }
        void UpdateRVRPRogression(Realms lockingRealm, RVRProgression oldProg, RVRProgression newProg);
    }
}