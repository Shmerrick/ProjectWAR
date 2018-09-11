using System.Collections.Generic;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public interface IRewardSelector
    {
        byte DetermineNumberOfAwards(int eligiblePlayers);
        List<uint> RandomisePlayerList(List<uint> nonRandomisedPlayers);
        List<uint> SelectAwardedPlayers(List<uint> randomisedPlayers, byte numberOfAwards);
    }
}