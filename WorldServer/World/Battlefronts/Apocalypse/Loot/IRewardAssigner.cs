using System.Collections.Generic;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public interface IRewardAssigner
    {
        List<LootBagTypeDefinition> AssignLootToPlayers(List<uint> eligiblePlayers, int forceNumberRewards=0);
    }
}