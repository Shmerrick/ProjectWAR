using WorldServer.World.Objects;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public interface ILocationComparitor
    {
        bool InRange(Player player);
    }
}