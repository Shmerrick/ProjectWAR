using System;

namespace WorldServer.World.Battlefronts.Bounty
{
    public class CharacterBounty
    {
        private static readonly DateTime EpochDateTime = new DateTime(1970, 1, 1);

        public int EffectiveLevel { get; set; }
        public int LastDeath { get; set; }
        public int CharacterLevel { get; set; }
        public int RenownLevel { get; set; }

        public CharacterBounty SetBounty(int effectiveLevel, int lastDeath, int characterLevel, int renownLevel)
        {
            EffectiveLevel = effectiveLevel;
            LastDeath = lastDeath;
            CharacterLevel = characterLevel;
            RenownLevel = renownLevel;

            return this;
        }
        
        public CharacterBounty AddCharacter(uint characterId, int characterLevel, int renownLevel)
        {
            var characterBounty = new CharacterBounty
            {
                EffectiveLevel = (2 * characterLevel + renownLevel),
                CharacterLevel = characterLevel,
                RenownLevel = renownLevel,
                LastDeath = 0
            };
            return characterBounty;
        }

        public override string ToString()
        {
            return $"EffectiveLevel {EffectiveLevel}, Last Death {LastDeath} Character Level {CharacterLevel}/{RenownLevel}";
        }
    }
}