using FrameWork;
using GameData;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public interface IApocCommunications
    {
        void SendFlagLeft(Player plr, int id);
        void BuildCaptureStatus(PacketOut Out, RegionMgr region, Realms realm);
        void BuildBattleFrontStatus(PacketOut Out, RegionMgr region);
        void SendCampaignStatus(Player plr, VictoryPointProgress vpp, Realms realm);
        void Broadcast(string message, int tier);
        void Broadcast(string message, Realms realm, RegionMgr region, int tier);
    }
}