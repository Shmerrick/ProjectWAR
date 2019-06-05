using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemData;
using Common;
using FrameWork;
using WorldServer.Managers;
using WorldServer.Services.World;
using WorldServer.World.Objects;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public class BlackMarketManager
    {
        public bool IsItemOnBlackMarket(uint itemId)
        {
            var blackMarketItems = ItemService._BlackMarket_Items;
            foreach (var blackMarketItem in blackMarketItems)
            {
                if (blackMarketItem.ItemId == itemId)
                    return true;
            }

            return false;
        }

        public void SendBlackMarketReward(Player player, uint itemId)
        {
            var blackMarketItems = ItemService._BlackMarket_Items;
            foreach (var blackMarketItem in blackMarketItems)
            {
                if (blackMarketItem.ItemId == itemId)
                {
                    Character_mail mail = new Character_mail
                    {
                        Guid = CharMgr.GenerateMailGuid(),
                        CharacterId = Convert.ToUInt32(player.CharacterId), //CharacterId
                        SenderName = "Black Market",
                        ReceiverName = player.Name,
                        SendDate = (uint)TCPManager.GetTimeStamp(),
                        Title = "Black Market Rewards",
                        Content = "Open it and see...",
                        Money = 0,
                        Opened = false,
                        CharacterIdSender = 0  // system
                    };

                    MailItem item = new MailItem(Convert.ToUInt32(1298378521), Convert.ToUInt16(1));
                    if (item != null)
                    {
                        mail.Items.Add(item);
                        CharMgr.AddMail(mail);
                    }
                }
            }

            
        }
    }
}
