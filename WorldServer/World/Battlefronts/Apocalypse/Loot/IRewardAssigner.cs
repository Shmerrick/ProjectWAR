using System.Collections.Generic;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public interface IRewardAssigner
    {
        void AssignLootToPlayer(List<uint> eligiblePlayers);
    }
}