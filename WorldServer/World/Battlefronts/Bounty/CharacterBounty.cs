using System;

namespace WorldServer.World.Battlefronts.Bounty
{
    public class CharacterBounty
    {
        // Multiplier of effective Level to calculate bounty.
        public const float BOUNTY_MULTIPLIER = 2.5f;

        private static readonly DateTime EpochDateTime = new DateTime(1970, 1, 1);

        public float BountyAmount { get; set; }
        public int Contribution { get; set; }
        public int LastDeath { get; set; }
        public int CharacterLevel { get; set; }
        public int RenownLevel { get; set; }

        public CharacterBounty SetBounty(float bountyAmount, int contribution, int lastDeath, int characterLevel, int renownLevel)
        {
            BountyAmount = bountyAmount;
            Contribution = contribution;
            LastDeath = lastDeath;
            CharacterLevel = characterLevel;
            RenownLevel = renownLevel;

            return this;
        }
        
        public CharacterBounty AddCharacter(uint characterId, int characterLevel, int renownLevel)
        {
            var characterBounty = new CharacterBounty
            {
                BountyAmount = BOUNTY_MULTIPLIER * (2 * characterLevel + renownLevel),
                CharacterLevel = characterLevel,
                RenownLevel = renownLevel,
                LastDeath = 0,
                Contribution = 0
            };
            return characterBounty;
        }
    }
}