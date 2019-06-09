using System.Collections.Generic;
using System.Linq;
using SystemData;
using ByteOperations;
using Common;
using FrameWork;
using GameData;
using NLog;
using WorldServer.Managers;
using WorldServer.NetWork.Handler;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Apocalypse.Loot;
using WorldServer.World.Objects;
using Item = WorldServer.World.Objects.Item;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Interfaces
{
    public enum MailInteractType
    {
        CloseBox,
        SendMail,
        OpenMail,
        ReturnMail,
        DeleteMail,
        ChangeReadMarker,
        Unk,
        TakeItem,
        TakeAll
    }

    public class MailInterface : BaseInterface
    {
        private readonly Object _lockObject = new Object();
        private static readonly Logger _logger = LogManager.GetLogger("MailLogger");

        private static uint MAIL_EXPIRE_UNREAD = 28*24*60*60; // 28 Days
        private static uint MAIL_EXPIRE_READ = 3*24*60*60; // 28 Days

        private readonly List<Character_mail> _mails = new List<Character_mail>();
        private uint _nextSend;
        private uint MAIL_PRICE = 30;

        public void Load(IList<Character_mail> mails)
        {
            if (mails != null)
                _mails.AddRange(mails);

            CheckMailExpired();

            base.Load();
        }

        public override void Save()
        {
            foreach (Character_mail mail in _mails)
                CharMgr.Database.SaveObject(mail);

            ((Player)_Owner).Info.Mails = _mails.ToList();
        }

        public void SendPacketMail(PacketIn packet)
        {
            Player plr = GetPlayer();
            if (plr == null)
                return;

            if (_nextSend >= TCPManager.GetTimeStamp())
            {
                SendResult(MailResult.TEXT_MAIL_RESULT6);
                return;
            }

            // Recipient read
            packet.Skip(1);
            byte nameSize = packet.GetUint8();
            string name = packet.GetString(nameSize);

            

            // Subject (client is limited to send 30 chars but its probably a ushort anyway)
            ushort subjectSize = ByteSwap.Swap(packet.GetUint16());
            string subject = packet.GetString(subjectSize);

            // Message
            ushort messageSize = ByteSwap.Swap(packet.GetUint16());
            string message = packet.GetString(messageSize);

            // Money
            uint money = ByteSwap.Swap(packet.GetUint32());

            // COD?
            byte cr = packet.GetUint8();

            // Item
            byte itemsToSendCount = packet.GetUint8();

            var isBlackMarket = ((name.ToLower() == "black market") || (name.ToLower() == "blackmarket"));

            List<ushort> itemSlots = new List<ushort>();
            var itemList = new List<Item>();

            for (byte i = 0; i < itemsToSendCount; ++i)
            {
                ushort itemSlot = ByteSwap.Swap(packet.GetUint16());
                packet.Skip(2);

                Item item = plr.ItmInterface.GetItemInSlot(itemSlot);
                if (item == null || item.Info == null)
                {
                    SendResult(MailResult.TEXT_MAIL_RESULT16);
                    return;
                }

                // Allow black market items to be sent in mail
                if (!isBlackMarket)
                {
                    if (item.BoundtoPlayer || item.Info.Bind == 1)
                    {
                        SendResult(MailResult.TEXT_MAIL_RESULT9);
                        return;
                    }
                }

                itemSlots.Add(itemSlot);
                itemList.Add(item);
            }

            if (isBlackMarket)
            {
                _logger.Debug($"Sending mail to the BLACK MARKET. Number items {itemsToSendCount}");

                // Ensure that what is being sent is a warlord item
                if (itemsToSendCount == 0)
                {
                    SendResult(MailResult.TEXT_MAIL_RESULT9);
                    return;
                }

                var blackMarketManager = new BlackMarketManager();

                // Sending multiple items.
                foreach (var item in itemList)
                {
                    if (blackMarketManager.IsItemOnBlackMarket(item.Info.Entry))
                    {
                        _logger.Info($"Sending {item.Info.Name} ({item.Info.Entry}) to BlackMarket");
                        blackMarketManager.SendBlackMarketReward(plr, item.Info.Entry);
                        plr.SendClientMessage($"Trusting to your luck, you have sent {string.Join(",", itemList.Select(x => x.Info.Name))} to the Black Market, hoping for just recompense.");
                        _logger.Debug($"Email Sent.");
                        plr.ItmInterface.RemoveItems(item.Info.Entry, 1);
                        _logger.Info($"Removed {item.Info.Name} ({item.Info.Entry}) from {plr.Name}");
                        plr.SendClientMessage($"A suspicious looking package has arrived in your mail.", ChatLogFilters.CHATLOGFILTERS_LOOT);
                    }
                    else
                    {
                        _logger.Debug($"{MailResult.TEXT_MAIL_RESULT9}");
                        return;
                    }
                }
                
                SendResult(MailResult.TEXT_MAIL_RESULT4);   
            }
            else
            {
                Character receiver = CharMgr.GetCharacter(Player.AsCharacterName(name), false);

                if (receiver == null || receiver.Realm != (byte)plr.Realm)
                {
                    SendResult(MailResult.TEXT_MAIL_RESULT7);
                    return;
                }
                if (receiver.Name == plr.Name) // You cannot mail yourself
                {
                    plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_PLAYER_CANT_MAIL_YOURSELF);
                    return;
                }
                if ((cr == 0 && !plr.HasMoney(money)) || !plr.RemoveMoney((cr == 0 ? money : 0) + MAIL_PRICE))
                {
                    SendResult(MailResult.TEXT_MAIL_RESULT8);
                    return;
                }

                SendMail(receiver, subject, message, money, cr == 1, itemSlots);
                
                _logger.Info($"Sending mail {subject} to {receiver.Name} from {plr.Name}. Money={money}, Item Count={itemSlots.Count}");
                foreach (var itemSlot in itemSlots)
                {
                    _logger.Info($"Item : {itemSlot}");
                }
            }
            

           
        }

        public void SendMail(Character receiver, string subject, string message, uint money, bool cashOnDelivery, List<ushort> itemSlots = null)
        {
            Player sender = (Player)_Owner;

            Character_mail cMail = new Character_mail
            {
                Guid = CharMgr.GenerateMailGuid(),
                CharacterId = receiver.CharacterId,
                CharacterIdSender = sender.CharacterId,
                SenderName = sender.Info.Name,
                ReceiverName = receiver.Name,
                SendDate = (uint)TCPManager.GetTimeStamp(),
                Title = subject,
                Content = message,
                Money = money,
                Cr = cashOnDelivery,
                Opened = false
            };

            if (itemSlots != null)
            {
                foreach (ushort itmslot in itemSlots)
                {
                    Item itm = sender.ItmInterface.GetItemInSlot(itmslot);
                    // This should never happen, double check.
                    if (itm != null && itm.Info != null)
                    {
                        cMail.Items.Add(new MailItem(itm.Info.Entry, itm.GetTalismans(), itm.GetPrimaryDye(), itm.GetSecondaryDye(), itm.Count));
                        sender.ItmInterface.DeleteItem(itmslot, itm.Count);
                        itm.Owner = null;
                    }
                }
            }

            SendResult(MailResult.TEXT_MAIL_RESULT4);

            CharMgr.AddMail(cMail);

            _nextSend = (uint)TCPManager.GetTimeStamp() + 5;
        }

        public void AddMail(Character_mail mail)
        {
            
            _mails.Add(mail);
            SendMailCount();
        }

        public void RemoveMail(Character_mail mail)
        {
            _mails.Remove(mail);
            SendResult(MailResult.TEXT_MAIL_RESULT12);
        }

        /// <summary>
        /// Returns an expired mail to its sender.
        /// </summary>
        private static MailResult ReturnMail(Character_mail mail)
        {
            // Can't return auction mail.
            if (mail.AuctionType != 0)
                return MailResult.TEXT_MAIL_RESULT11;

            Character receiver = CharMgr.GetCharacter(mail.SenderName, false);

            // No one to return mail to.
            if (receiver == null)              
                return MailResult.TEXT_MAIL_RESULT11;

            // If mail is COD, remove the COD requirement and remove the money.
            if (mail.Cr)
            {
                mail.Cr = false;
                mail.Money = 0;
            }
            
            CharMgr.DeleteMail(mail);

            Character_mail returnMail = new Character_mail
            {
                // Sender -> Reciever, Reciever -> Sender
                Guid = CharMgr.GenerateMailGuid(),
                CharacterId = mail.CharacterIdSender,
                CharacterIdSender = mail.CharacterId,
                SenderName = mail.ReceiverName,
                ReceiverName = mail.SenderName,
                Content = "Your mail expired and has been returned to you.",
                ReadDate = 0,
                SendDate = (uint) TCPManager.GetTimeStamp(),
                Opened = false
            };

            CharMgr.AddMail(returnMail);

            return MailResult.TEXT_MAIL_UNK;
        }

        public static long TimeToExpire(Character_mail mail)
        {
            long sentTime = TCPManager.GetTimeStamp() - mail.SendDate;
            long readTime = TCPManager.GetTimeStamp() - mail.ReadDate;

            return mail.ReadDate == 0 ? MAIL_EXPIRE_UNREAD - sentTime : MAIL_EXPIRE_READ - readTime;
        }

        /// <summary>
        /// Determines whether any of the currently stored mails have reached their expiration date.
        /// </summary>
        public void CheckMailExpired()
        {
            List<Character_mail> toDelete = _mails.Where(mail => TimeToExpire(mail) <= 0).ToList();

            foreach (Character_mail mail in toDelete)
                MailExpired(mail);
        }

        /// <summary>
        /// Removes or returns an expired mail.
        /// </summary>
        private static void MailExpired(Character_mail mail)
        {
            if (mail.AuctionType != 0 || (mail.Money == 0 && mail.Items.Count == 0))
            {
                CharMgr.DeleteMail(mail);
                return;
            }

            Character receiver = CharMgr.GetCharacter(mail.SenderName, false);

            if (receiver != null)
                ReturnMail(mail);
            else
                CharMgr.DeleteMail(mail);
        }

        public void MailInteract(MailInteractType type, uint guid, PacketIn packet)
        {
            lock (_lockObject)
            {
                Character_mail mail = _mails.FirstOrDefault(match => match.Guid == guid);

                if (mail == null)
                    return;

                switch (type)
                {
                    case MailInteractType.OpenMail:
                        if (!mail.Opened)
                        {
                            if (mail.ReadDate == 0)
                            {
                                mail.ReadDate = (uint)TCPManager.GetTimeStamp();
                            }
                            mail.Opened = true;
                            SendMailCount();
                            SendMailBox();
                        }
                        SendMail(mail);
                        break;
                    case MailInteractType.ReturnMail:
                        SendResult(ReturnMail(mail));
                        SendMailCount();
                        SendMailBox();
                        break;
                    case MailInteractType.DeleteMail:
                        CharMgr.DeleteMail(mail);
                        SendMailCount();
                        SendMailBox();
                        break;
                    case MailInteractType.ChangeReadMarker:
                        packet.Skip(4);
                        mail.Opened = packet.GetUint8() == 1;
                        SendMailCount();
                        SendMailBox();
                        break;
                    case MailInteractType.TakeItem:
                        if (mail.Cr && mail.Money > 0)
                        {
                            if (!_Owner.GetPlayer().RemoveMoney(mail.Money))
                            {
                                _Owner.GetPlayer().SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_AUCTION_NOT_ENOUGH_MONEY);
                                return;
                            }
                            MailHandlers.SendCOD(mail.SenderName, _Owner.GetPlayer(), mail.Money);
                            mail.Money = 0;
                        }
                        packet.Skip(4);
                        byte itemnum = packet.GetUint8();
                        if (mail.Items.Count < itemnum + 1)
                        {
                            return;
                        }
                        MailItem item = mail.Items.ElementAt(itemnum);
                        ushort freeSlot = _Owner.GetPlayer().ItmInterface.GetFreeInventorySlot(ItemService.GetItem_Info(item.id), false);
                        if (freeSlot == 0)
                        {
                            _Owner.GetPlayer().SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_OVERAGE_CANT_TAKE_ATTACHMENTS);
                            return;
                        }
                        _Owner.GetPlayer().ItmInterface.CreateItem(ItemService.GetItem_Info(item.id), item.count, item.talisman, item.primary_dye, item.secondary_dye, false);
                        mail.Items.Remove(item);
                        mail.Dirty = true;
                        CharMgr.Database.SaveObject(mail);

                        SendMailUpdate(mail);
                        SendMail(mail);
                        break;
                    case MailInteractType.TakeAll:
                        if (mail.Money > 0)
                        {
                            if (mail.Cr)
                            {
                                if (!_Owner.GetPlayer().RemoveMoney(mail.Money))
                                {
                                    _Owner.GetPlayer().SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_AUCTION_NOT_ENOUGH_MONEY);
                                    return;
                                }
                                MailHandlers.SendCOD(mail.SenderName, _Owner.GetPlayer(), mail.Money);
                            }
                            else
                                _Owner.GetPlayer().AddMoney(mail.Money);

                            mail.Money = 0;
                        }
                        // Take as many items as you can before inventory is full
                        List<MailItem> toRemove = new List<MailItem>();

                        foreach (MailItem curritem in mail.Items)
                        {
                            ushort slot = _Owner.GetPlayer().ItmInterface.GetFreeInventorySlot(ItemService.GetItem_Info(curritem.id));
                            if (slot == 0)
                            {
                                _Owner.GetPlayer().SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_OVERAGE_CANT_TAKE_ATTACHMENTS);
                                break;
                            }
                            _Owner.GetPlayer().ItmInterface.CreateItem(ItemService.GetItem_Info(curritem.id), curritem.count, curritem.talisman, curritem.primary_dye, curritem.secondary_dye, false);
                            toRemove.Add(curritem);
                        }

                        foreach (MailItem remove in toRemove)
                            mail.Items.Remove(remove);

                        mail.Dirty = true;

                        CharMgr.Database.SaveObject(mail);
                        SendMailUpdate(mail);
                        SendMail(mail);
                        break;
                }
            }
        }

        #region Packets

        public void BuildPreMail(PacketOut Out, Character_mail mail)
        {
            if (mail == null)
                return;

            Out.WriteUInt32(0);
            Out.WriteUInt32((uint)mail.Guid);
            Out.WriteUInt16((ushort)(mail.Opened ? 1 : 0));
            Out.WriteByte((byte)(mail.AuctionType == 0 ? 100 : 0)); // Icon ID

            uint sentTime = (uint)(TCPManager.GetTimeStamp() - mail.SendDate);
            uint readTime = (uint)(TCPManager.GetTimeStamp() - mail.ReadDate);
            Out.WriteUInt32(0xFFFFFFFF - sentTime); // LastUpdatedTime Sent
            Out.WriteUInt32(0xFFFFFFFF - readTime); // LastUpdatedTime Read (not displayed afaik)
            Out.WriteUInt32((uint)TimeToExpire(mail)); // Seconds left (expire after 28/3 days)

            if (mail.AuctionType == 0)
            {
                Out.WriteByte(0); // 1 = localized name

                Out.WriteByte(0);
                Out.WriteStringToZero(mail.SenderName);

                Out.WriteByte(0);
                Out.WriteStringToZero(mail.ReceiverName);

                Out.WriteByte(0);
                Out.WriteStringToZero(mail.Title);
            }
            else
            {
                ushort titleLocal = 0;
                if (mail.AuctionType == 1) // Refund (outbid) 
                    titleLocal = (ushort)Localized_text.TEXT_AUCTION_MAIL_SUBJECT_REFUND;
                else if (mail.AuctionType == 2) // Complete
                    titleLocal = (ushort)Localized_text.TEXT_AUCTION_MAIL_SUBJECT_COMPLETE;
                else if (mail.AuctionType == 3) // Cancelled
                    titleLocal = (ushort)Localized_text.TEXT_AUCTION_MAIL_SUBJECT_CANCELLED;
                else if (mail.AuctionType == 4) // Expired
                    titleLocal = (ushort)Localized_text.TEXT_AUCTION_MAIL_SUBJECT_EXPIRED;
                else if (mail.AuctionType == 5) // Won
                    titleLocal = (ushort)Localized_text.TEXT_AUCTION_MAIL_SUBJECT_WON;
                else
                    titleLocal = (ushort)Localized_text.TEXT_AUCTION_MAIL_SUBJECT_OTHER;

                Out.WriteByte(0x0D);
                Out.WriteUInt16(0);
                Out.WriteUInt16((ushort)Localized_text.TEXT_AUCTION_MAIL_SENDER_NAME);
                Out.Fill(0, 5);
                Out.WriteByte(1);
                Out.Fill(0, 3);
                Out.WriteUInt16(titleLocal);
                Out.WriteUInt32(0);
            }

            if (mail.Cr && mail.Money > 0)
            {
                Out.WriteUInt32(0xFFFFFFFF);
                Out.WriteUInt32(uint.MaxValue - mail.Money + 1);
            }
            else
            {
                Out.WriteUInt32(0);
                Out.WriteUInt32(mail.Money);
            }

            Out.WriteUInt16((ushort)mail.Items.Count);
            if (mail.Items.Count > 0)
                Out.WriteByte(0);
            if (mail.Items.Count > 8)
                Out.WriteByte(0);

            foreach (MailItem item in mail.Items)
            {
                if(ItemService.GetItem_Info(item.id) != null)
                    Out.WriteUInt32(ItemService.GetItem_Info(item.id).ModelId);
            }
        }

        public void SendMailCount()
        {
            if (GetPlayer() == null)
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_MAIL);
            Out.WriteByte(0x9);
            Out.WriteByte((byte)MailboxType.MAILBOXTYPE_PLAYER);

            ushort counts = 0;

            foreach (Character_mail mail in _mails)
                if (!mail.Opened && mail.AuctionType == 0)
                    counts++;

            Out.WriteUInt16(counts);
            GetPlayer().SendPacket(Out);

            ushort auctionCounts = 0;

            foreach (Character_mail mail in _mails)
                if (!mail.Opened && mail.AuctionType != 0)
                    auctionCounts++;

            if (auctionCounts > 0)
            {
                PacketOut auction = new PacketOut((byte)Opcodes.F_MAIL);
                auction.WriteByte(0x9);
                auction.WriteByte((byte)MailboxType.MAILBOXTYPE_AUCTION);
                auction.WriteUInt16(auctionCounts);
                GetPlayer().SendPacket(auction);
            }
        }

        public void SendMailBox()
        {
            if (GetPlayer() == null)
                return;

            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_MAIL);
                Out.WriteUInt16(0);
                Out.WriteByte(1);
                GetPlayer().SendPacket(Out);
            }


            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_MAIL);
                Out.WriteUInt32(0x0E000000);
                Out.WriteUInt32(0x001E0AD7);
                Out.WriteUInt16(0xA33C);
                GetPlayer().SendPacket(Out);
            }


            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_MAIL);
                Out.WriteByte(0x0A);
                Out.WriteByte((byte)MailboxType.MAILBOXTYPE_PLAYER);
                Out.WriteUInt16((ushort)_mails.Count(info => info.AuctionType == 0));
                foreach (Character_mail mail in _mails)
                    if (mail.AuctionType == 0)
                        BuildPreMail(Out, mail);
                Out.WriteUInt16((ushort)_mails.Count);

                GetPlayer().SendPacket(Out);
            }

            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_MAIL);
                Out.WriteByte(0x0A);
                Out.WriteByte((byte)MailboxType.MAILBOXTYPE_AUCTION);
                Out.WriteUInt16((ushort)_mails.Count(info => info.AuctionType != 0));
                foreach (Character_mail mail in _mails)
                    if (mail.AuctionType != 0)
                        BuildPreMail(Out, mail);
                Out.WriteUInt16((ushort)_mails.Count);

                GetPlayer().SendPacket(Out);
            }
        }

        public void SendMailUpdate(Character_mail mail)
        {
            if (mail == null)
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_MAIL);
            Out.WriteByte(0x0B);
            Out.WriteByte(0);
            BuildPreMail(Out, mail);
            GetPlayer().SendPacket(Out);
        }

        public void SendMail(Character_mail mail)
        {
            if (mail == null)
            {
                Log.Error("MailInterface", "Attempted to send NULL mail!");
                return;
            }


            if (mail.Content == null)
            {
                Log.Error("MailInterface", "Mail sent with NULL content!");
                return;
            }

            PacketOut Out = new PacketOut((byte)Opcodes.F_MAIL);
            Out.WriteByte(0x0D);

            if (mail.AuctionType == 0)
                Out.WriteByte(0);
            else
                Out.WriteByte(1);

            BuildPreMail(Out, mail);

            if (mail.AuctionType == 0)
            {
                Out.WriteUInt16((ushort)(mail.Content.Length + 1));
                Out.WriteStringBytes(mail.Content);
                Out.WriteByte(0);
            }
            else
            {
                ushort contentLocal = 0;
                if (mail.AuctionType == 1) // Refund (outbid) 
                    contentLocal = (ushort)Localized_text.TEXT_AUCTION_MAIL_BODY_REFUND_ITEM_X;
                else if (mail.AuctionType == 2) // Complete
                    contentLocal = (ushort)Localized_text.TEXT_AUCTION_MAIL_BODY_COMPLETE_ITEM_X;
                else if (mail.AuctionType == 3) // Cancelled
                    contentLocal = (ushort)Localized_text.TEXT_AUCTION_MAIL_BODY_CANCELLED;
                else if (mail.AuctionType == 4) // Expired
                    contentLocal = (ushort)Localized_text.TEXT_AUCTION_MAIL_BODY_EXPIRED;
                else if (mail.AuctionType == 5) // Won
                    contentLocal = (ushort)Localized_text.TEXT_AUCTION_MAIL_BODY_WON_ITEM_X;
                else
                    contentLocal = (ushort)Localized_text.TEXT_AUCTION_MAIL_BODY_OTHER;

                Out.WriteUInt16(0);
                Out.WriteUInt16(contentLocal);
                Out.WriteUInt16(0);
                Out.WriteByte(0);

                if (mail.Content.Length == 0)
                    Out.WriteByte(0);
                else
                {
                    Out.WriteByte(1);
                    Out.WriteByte(1);

                    Out.WriteByte(0);
                    Out.WritePascalString(mail.Content);
                }
            }

            Out.WriteByte((byte)mail.Items.Count);
            foreach (MailItem item in mail.Items)
            {
                Item.BuildItem(ref Out, null, null, item, 0, item.count);
            }
            GetPlayer().SendPacket(Out);
        }

        public void SendResult(MailResult result)
        {
            _logger.Debug($"{result}");

            if (result == MailResult.TEXT_MAIL_UNK)
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_MAIL);
            Out.WriteByte(0x0F);
            Out.WriteUInt16((ushort)result);
            GetPlayer().SendPacket(Out);
        }
        #endregion
    }
}
