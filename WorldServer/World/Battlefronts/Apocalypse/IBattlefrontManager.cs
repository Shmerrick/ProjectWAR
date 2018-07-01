using System.Collections.Generic;
using Common.Database.World.Battlefront;
using GameData;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public interface IBattleFrontManager
    {
        RVRProgression ResetBattleFrontProgression();
        RVRProgression GetBattleFrontByName(string name);
        RVRProgression GetBattleFrontByBattleFrontId(int id);
        

        string ActiveBattleFrontName { get; set; }
        RVRProgression ActiveBattleFront { get; set; }
        void AuditBattleFronts(int tier);
        void LockBattleFrontsAllRegions(int tier);

        RVRProgression AdvanceBattleFront(Realms lockingRealm);
        RVRProgression OpenActiveBattlefront();
        RVRProgression LockActiveBattleFront(Realms realm);

        List<ApocBattleFrontStatus> GetBattleFrontStatusList();
        bool IsBattleFrontLocked(int battleFrontId);

        ApocBattleFrontStatus GetBattleFrontStatus(int battleFrontId);
        void LockBattleFrontStatus(int battleFrontId, Realms lockingRealm, VictoryPointProgress vpp);
    }
}