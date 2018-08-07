using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using FrameWork;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class LootBagBuilder : ILootBagBuilder
    {
        public string EventGeneratorName { get; set; }
        public string EventName { get; set; }
        public string EventDescription { get; set; }
        public ITCPManager TcpManager { get; }
        public IAccountManager AccountManager { get; set; }

        public LootBagBuilder(string eventGeneratorName, string eventName, string eventDescription, ITCPManager tcpManager, IAccountManager accountManager)
        {
            EventGeneratorName = eventGeneratorName;
            EventName = eventName;
            EventDescription = eventDescription;
            TcpManager = tcpManager;
            AccountManager = accountManager;
        }

        public List<uint> SelectLootBagWinners(List<uint> eligiblePlayers)
        {


        }

        /// <summary>
        /// Given a list of winning players assign availableLootItems to them, remaining players receive the defaultReward
        /// Return a dictionary of players - lootItems
        /// </summary>
        /// <param name="winningPlayers"></param>
        /// <param name="availableLootItems"></param>
        /// <returns></returns>
        public Dictionary<uint, uint> AssignLootToWinners(List<uint> winningPlayers, List<uint> availableLootItems, uint defaultReward)
        {
        }


        /// <summary>
        /// For each item of assigned loot, create a mail item.
        /// </summary>
        /// <param name="assignedLoot"></param>
        /// <returns></returns>
        public List<MailItem> GenerateMailItems(Dictionary<uint, uint> assignedLootDictionary)
        {
            foreach (var assignedLoot in assignedLootDictionary)
            {
                
                Character_mail mail = new Character_mail
                {
                    Guid = CharMgr.GenerateMailGuid(),
                    CharacterId = assignedLoot.Key,
                    SenderName = EventGeneratorName,
                    ReceiverName = AccountManager.GetCharacterName(assignedLootKey),
                    SendDate = (uint)TcpManager.GetTimeStamp(),
                    Title = EventName,
                    Content = EventDescription,
                    Money = 0,
                    Opened = false
                };

                var mailItem = new MailItem(assignedLoot.Value);
                mail.Items.Add(mailItem);
                CharMgr.AddMail(mail);

            }
        }

    }
}
