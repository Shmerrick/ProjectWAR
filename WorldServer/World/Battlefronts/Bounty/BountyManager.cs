using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace WorldServer.World.Battlefronts.Bounty
{
    public class BountyManager : IBountyManager
    {
        private static readonly Logger BountyLogger = LogManager.GetLogger("BountyLogger");
        // Holds the characterId, the last death time since epoch, and the amount of bounty
        public ConcurrentDictionary<uint, CharacterBounty> BountyDictionary { get; set; }

        public BountyManager()
        {
            BountyDictionary = new ConcurrentDictionary<uint, CharacterBounty>();
        }

        public CharacterBounty UpdateCharacterBounty(uint targetCharacterId, CharacterBounty characterBounty)
        {
            return this.BountyDictionary.AddOrUpdate(
                targetCharacterId, 
                characterBounty, 
                (key, existingCharacterBounty) => characterBounty);
        }

     

        /// <summary>
        /// Add Character to Bounty Dictionary
        /// </summary>
        /// <param name="characterId"></param>
        /// <param name="characterLevel"></param>
        /// <param name="renownLevel"></param>
        public void AddCharacter(uint characterId, int characterLevel, int renownLevel)
        {
            var characterBounty = new CharacterBounty().AddCharacter(characterId, characterLevel, renownLevel);
            UpdateCharacterBounty(characterId, characterBounty);
        }

        /// <summary>
        /// Remove Character from Bountry Dictionary
        /// </summary>
        /// <param name="characterId"></param>
        public void RemoveCharacter(uint characterId)
        {
            var characterBounty = new CharacterBounty();
            this.BountyDictionary.TryRemove(characterId, out characterBounty);
        }

        public CharacterBounty GetBounty(uint targetCharacterId)
        {
            if (this.BountyDictionary.ContainsKey(targetCharacterId))
                return this.BountyDictionary[targetCharacterId];
            else
                throw new BountyException($"Could not locate target character {targetCharacterId} for bounty calculation");
        }
    }
}
