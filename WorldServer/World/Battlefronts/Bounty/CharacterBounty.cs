using System;

namespace WorldServer.World.Battlefronts.Bounty
{
    public class CharacterBounty
    {
        private static readonly DateTime EpochDateTime = new DateTime(1970, 1, 1);

        public int LastDeath { get; set; }
        public int CharacterLevel { get; set; }
        public int RenownLevel { get; set; }
        // The base bounty level of this character.
        public int BaseBountyValue { get; private set; }
        // The contribution bounty level of this character (added as the character performs actions to gain contribution). Lock contribution is derived from the ContributionManager.
        public int ContributedBountyValue { get; set; }

        public CharacterBounty()
        {
            
        }

        public CharacterBounty(Player player)
        {
            CharacterLevel = player.Level;
            RenownLevel = player.RenownRank;
            BaseBountyValue = player.BaseBountyValue;
            ContributedBountyValue = 0;
        }
        
        public override string ToString()
        {
            return $"Last Death {LastDeath} Character Level {CharacterLevel}/{RenownLevel}, BaseBounty {BaseBountyValue} Contributed {ContributedBountyValue}";
        }
    }
}