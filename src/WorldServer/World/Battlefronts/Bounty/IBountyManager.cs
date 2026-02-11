using WorldServer.World.Objects;

namespace WorldServer.World.Battlefronts.Bounty
{
    public interface IBountyManager
    {
        bool ResetCharacterBounty(uint targetCharacterId, Player player);

        void RemoveCharacter(uint characterId);

        CharacterBounty GetBounty(uint targetCharacterId, bool createIfNotExists = true);
    }
}