using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using WorldServer.World.Objects;

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

        /// <summary>
        /// Reset the characters bounty value (typically as they have died)
        /// </summary>
        /// <param name="targetCharacterId"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool ResetCharacterBounty(uint targetCharacterId, Player player)
        {
            RemoveCharacter(targetCharacterId);
            return this.BountyDictionary.TryAdd(targetCharacterId,new CharacterBounty(player));
        }

        public bool AddCharacterBounty(uint targetCharacterId, int additionalContribution)
        {
            BountyLogger.Trace($"+++ Adding Character Bounty {targetCharacterId} value {additionalContribution}");
            var bounty = GetBounty(targetCharacterId);
            if (bounty != null)
            {
                bounty.ContributedBountyValue += additionalContribution;
                return this.BountyDictionary.TryUpdate(targetCharacterId, bounty, bounty);
            }
            else
            {
                // Something is wrong.
                return false;
            }
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

        /// <summary>
        /// Get the Bounty value for this character
        /// </summary>
        /// <param name="targetCharacterId"></param>
        /// <returns></returns>
        public CharacterBounty GetBounty(uint targetCharacterId, bool createIfNotExists = true)
        {
            if (this.BountyDictionary.ContainsKey(targetCharacterId))
                return this.BountyDictionary[targetCharacterId];
            else
            {
                // If the bounty for this player doesnt exist, create it. All players must have a bounty value.
                if (createIfNotExists)
                {
                    var player = Player.GetPlayer(targetCharacterId);
                    // player can sometimes be null - this is really unlikely here, but just incase.
                    if (player == null)
                    {
                        BountyLogger.Warn($"Player not found in BountyDictionary, tried to create, but got nothing. Character : {targetCharacterId}");
                        return new CharacterBounty();
                    }
                    if (this.BountyDictionary.TryAdd(targetCharacterId, new CharacterBounty(player)))
                    {
                        return this.BountyDictionary[targetCharacterId];
                    }
                    else
                    {
                        BountyLogger.Warn($"Player not found in BountyDictionary, tried to create, but adding failed. Character : {targetCharacterId}");
                        return new CharacterBounty();
                    }
                }
                return new CharacterBounty();
            }
        }
    }
}
