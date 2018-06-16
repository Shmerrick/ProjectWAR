namespace WorldServer.World.Battlefronts.Bounty
{
    public interface IBountyManager
    {
        CharacterBounty UpdateCharacterBounty(uint targetCharacterId, CharacterBounty characterBounty);
        void AddCharacter(uint characterId, int characterLevel, int renownLevel);
        void RemoveCharacter(uint characterId);
        CharacterBounty GetBounty(uint targetCharacterId);
    }
}