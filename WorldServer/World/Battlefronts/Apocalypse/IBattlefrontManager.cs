using Common.Database.World.Battlefront;
using GameData;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public interface IBattleFrontManager
    {
        RVRProgression ResetBattleFrontProgression();
        RVRProgression GetBattleFrontByName(string name);
        RVRProgression GetBattleFrontByBattleFrontId(int id);
        RVRProgression AdvanceBattleFront(Realms lockingRealm);

        string ActiveBattleFrontName { get; set; }
        RVRProgression ActiveBattleFront { get; set; }
        void AuditBattleFronts(int tier);
        void LockBattleFronts(int tier);
        void OpenActiveBattlefront();
    }
}