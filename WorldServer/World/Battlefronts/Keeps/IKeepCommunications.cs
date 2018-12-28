using FrameWork;
using GameData;
using WorldServer.World.BattleFronts.Keeps;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public interface IKeepCommunications
    {
        void SendKeepStatus(Player plr, BattleFrontKeep keep);
    }
}