using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using System.Collections.Specialized;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class LootBagBuilder : ILootBagBuilder
    {

        // Dictionary of players (RenownBand and PlayerInfo)
        public List<KeyValuePair<uint, PlayerRewardOptions>> EligiblePlayers { get; set; }

        // Dictionary of renownband, (item id, chance to drop)
        public Dictionary<uint, List<LootOption>> AvailableLootItems { get; set; }

        public IRandomGenerator RandomGenerator { get; }

        public LootBagBuilder(List<KeyValuePair<uint, PlayerRewardOptions>> eligiblePlayers, Dictionary<uint, List<LootOption>> availableLootItems, IRandomGenerator randomGenerator)
        {
            EligiblePlayers = eligiblePlayers;
            AvailableLootItems = availableLootItems;
            RandomGenerator = randomGenerator;
        }




        /// <summary>
        /// Given a list of eligible players, and a list of available loot items, select and assign the items to those players.
        /// </summary>
        /// <param name="eligiblePlayers"></param>
        /// <param name="availableLootItems"></param>
        /// <returns></returns>
        public ConcurrentDictionary<uint, uint> SelectLootBagWinners(List<KeyValuePair<uint, PlayerRewardOptions>> eligiblePlayers, Dictionary<uint, List<LootOption>> availableLootItems, bool randomisePlayers = true)
        {
            var lootItemAssigned = false;
            var assignedLootDictionary = new ConcurrentDictionary<uint, uint>();

            if (eligiblePlayers == null)
                return null;

            if (availableLootItems == null)
                return null;

            List<KeyValuePair<uint, PlayerRewardOptions>> playerList = new List<KeyValuePair<uint, PlayerRewardOptions>>();

            if (randomisePlayers)
            {
                playerList = RandomisePlayerList(eligiblePlayers);
            }
            else
            {
                playerList = eligiblePlayers;
            }

            foreach (var lootItem in availableLootItems)
            {
                foreach (var player in playerList)
                {
                    //player.key is RRBand
                    var playerCentricLootList = GetPlayerCentricLootList(player.Key, lootItem.Value);

                    if (playerCentricLootList == null)
                        continue;

                    // lootItem.Value is the chance to win the loot item.
                    if (RandomGenerator.Generate(100) <= playerCentricLootList.WinChance)
                    {
                        // player can only win one roll. 
                        if (!assignedLootDictionary.ContainsKey(player.Value.RenownBand))
                        {
                            assignedLootDictionary.TryAdd(player.Value.RenownBand, playerCentricLootList.ItemId); // player.Key is characterId
                            break;
                        }
                    }
                }
                // If not won, then bad luck!
            }

            return assignedLootDictionary;
        }

        // For a given list of loot options, find the right one for the player's renown-band
        public LootOption GetPlayerCentricLootList(uint renownBand, List<LootOption> lootOptions)
        {
            foreach (var lootOption in lootOptions)
            {
                if (lootOption.RenownBand == renownBand)
                    return lootOption;
            }
            // Could not find an item for this renown band
            return null;

        }

        //// Randomise the player list to ensure we do not bias loot rolls
        public List<KeyValuePair<uint, PlayerRewardOptions>> RandomisePlayerList(List<KeyValuePair<uint, PlayerRewardOptions>> playerList)
        {
            if (playerList == null)
                return null;

            int n = playerList.Count;

            for (int i = playerList.Count - 1; i > 1; i--)
            {
                var rnd = RandomGenerator.Generate(i + 1);

                var value = playerList[rnd];
                playerList[rnd] = playerList[i];
                playerList[i] = value;
            }

            return playerList;
        }

        ///// <summary>
        ///// For each item of assigned loot, create a mail item.
        ///// </summary>
        ///// <param name="assignedLoot"></param>
        ///// <returns></returns>
        //public List<MailItem> GenerateMailItems(Dictionary<uint, uint> assignedLootDictionary)
        //{
        //    foreach (var assignedLoot in assignedLootDictionary)
        //    {

        //        Character_mail mail = new Character_mail
        //        {
        //            Guid = CharMgr.GenerateMailGuid(),
        //            CharacterId = assignedLoot.Key,
        //            SenderName = EventGeneratorName,
        //            ReceiverName = AccountManager.GetCharacterName(assignedLootKey),
        //            SendDate = (uint)TcpManager.GetTimeStamp(),
        //            Title = EventName,
        //            Content = EventDescription,
        //            Money = 0,
        //            Opened = false
        //        };

        //        var mailItem = new MailItem(assignedLoot.Value);
        //        mail.Items.Add(mailItem);
        //        CharMgr.AddMail(mail);

        //    }
        //}

    }
}
