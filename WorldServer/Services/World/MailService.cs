using System;
using SystemData;
using Common;
using FrameWork;
using WorldServer.Managers;

namespace WorldServer.Services.World
{
    [Service]
    public class MailService : ServiceBase
    {
        public static bool MailItem(uint characterId, uint itemId, ushort count)
        {
            var character = CharMgr.GetCharacter(characterId, true);
            if (character == null) return false;
            var characterName = character?.Name;
            
            Character_mail mail = new Character_mail
            {
                Guid = CharMgr.GenerateMailGuid(),
                CharacterId = characterId, //CharacterId
                SenderName = "Ikthaleon",
                ReceiverName = characterName,
                SendDate = (uint)TCPManager.GetTimeStamp(),
                Title = "",
                Content = "",
                Money = 0,
                Opened = false,
                CharacterIdSender = 283
            };

            MailItem item = new MailItem(itemId, count);
            if (item != null)
            {
                mail.Items.Add(item);
                CharMgr.AddMail(mail);
            }

            return true;
        }
    }
}
