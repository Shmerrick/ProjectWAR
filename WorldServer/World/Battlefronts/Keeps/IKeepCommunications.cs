using WorldServer.World.Objects;

namespace WorldServer.World.Battlefronts.Keeps
{
    public interface IKeepCommunications
    {
        void SendKeepStatus(Player plr, BattleFrontKeep keep);
    }
}