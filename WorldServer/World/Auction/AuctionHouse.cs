using System;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using Common;
using FrameWork;
using WorldServer.Managers;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Auction
{
    public class AuctionHouse
    {
        private const float AUCTION_HOUSE_TAX_MULT = 0.9f;
        #region Auction Loading

        public static ulong MAX_AUCTION_GUID = 1;
        public static byte MAX_AUCTIONS_TO_SEND = 100; // worked for 154, but may be limited by packet size
        public static byte MAX_AUCTIONS_PER_CHAR = 50; // worked for 154, but may be limited by packet size

        public static ulong GenerateAuctionGUID()
        {
            return ++MAX_AUCTION_GUID;
        }

        public static void LoadAuctions()
        {
            Log.Debug("WorldMgr", "Loading auctions...");

            Auctions = CharMgr.Database.SelectAllObjects<Common.Auction>() as List<Common.Auction>;
            int count = 0;
            if (Auctions != null)
                foreach (Common.Auction auct in Auctions)
                {
                    auct.Seller = CharMgr.GetCharacter(auct.SellerId, false);

                    if (auct.AuctionId > MAX_AUCTION_GUID)
                        MAX_AUCTION_GUID = auct.AuctionId;
                    count++;
                }


            Log.Success("LoadAuctions", "Loaded " + count + " Auctions");
        }

        #endregion

        public static List<Common.Auction> Auctions;

        public static void SendAuctions(Player plr, string searchTerm, string sellerName, byte minLevel, byte maxLevel, byte rarity, byte career, List<byte> types, List<byte> slots, byte stat)
        {
            List<Common.Auction> matching = new List<Common.Auction>();
            List<Common.Auction> toRemove = new List<Common.Auction>();

            int count = 0;
            lock (Auctions)
            {
                foreach (Common.Auction auct in Auctions)
                    if (count < MAX_AUCTIONS_TO_SEND)
                    {
                        if (auct.Item == null)
                            auct.Item = ItemService.GetItem_Info(auct.ItemId);

                        if (auct.Item == null || auct.Seller == null)
                        {
                            toRemove.Add(auct);
                            continue;
                        }

                        // Opposite realm
                        if (auct.Realm != 0 && auct.Realm != plr.Info.Realm)
                            continue;

                        // Name mismatch
                        if (searchTerm != string.Empty && auct.Item.Name.IndexOf(searchTerm, 0, StringComparison.OrdinalIgnoreCase) == -1)
                            continue;

                        // Not from requested seller
                        if (sellerName != string.Empty && (auct.Seller == null || auct.Seller.Name.IndexOf(sellerName, 0, StringComparison.OrdinalIgnoreCase) == -1))
                            continue;

                        // Level mismatch
                        if (minLevel != 0 && auct.Item.MinRank < minLevel)
                            continue;
                        if (maxLevel != 0 && auct.Item.MinRank > maxLevel)
                            continue;

                        // Attribute mismatch
                        if (rarity != 0 && auct.Item.Rarity != Math.Log(rarity, 2))
                            continue;
                        if (career != 0 && !plr.ItmInterface.CanUseItemType(auct.Item, 0, career))
                            continue;
                        if (types.Count != 0 && !types.Contains(auct.Item.Type))
                            continue;
                        if (slots.Count != 0 && !slots.Contains((byte) auct.Item.SlotId))
                            continue;
                        if (stat != 0 && !auct.Item._Stats.Keys.Contains(stat))
                            continue;

                        matching.Add(auct);
                        count++;
                    }
                    else
                        break;

                if (toRemove.Count > 0)
                    foreach (var auct in toRemove)
                    {
                        Auctions.Remove(auct);
                        CharMgr.Database.DeleteObject(auct);
                    }
            }


            PacketOut Out = new PacketOut((byte)Opcodes.F_AUCTION_SEARCH_RESULT, 7 + matching.Count * 300);
            Out.WriteHexStringBytes("0000753400");
            Out.WriteByte((byte)matching.Count);// count
            Out.WriteByte(0x01);

            foreach (Common.Auction auct in matching)
            {
                Out.WriteUInt64(auct.AuctionId); // id
                Out.WriteUInt16(1); // part of the id?
                Out.WriteUInt16(0xA752);
                Out.WriteUInt32(0); // unk
                Out.WriteUInt32(auct.SellPrice); // price
                Out.WriteByte(0x03); // ??
                Out.FillString(auct.Seller == null ? "" : auct.Seller.Name + (auct.Seller.Sex == 1 ? "^F" : "^M"), 48);
                Out.WriteHexStringBytes("880E39");

                Item.BuildItem(ref Out, null, null, new MailItem(auct.ItemId, auct._Talismans, auct.PrimaryDye, auct.SecondaryDye, auct.Count), 0, auct.Count);

                if (plr != null && plr.ItmInterface != null && auct.Item != null && auct.Item.ItemSet != 0)
                    plr.ItmInterface.SendItemSetInfoToPlayer(plr, auct.Item.ItemSet);

            }

            plr.SendPacket(Out);
        }

        public static void AddAuction(Common.Auction auction)
        {
            lock (Auctions)
                Auctions.Add(auction);

            CharMgr.Database.AddObject(auction);
        }

        public static void BuyOutAuction(Player buyer, ulong auctionId, uint price)
        {
            Common.Auction auction;

            bool cancel = false;

            lock (Auctions)
                auction = Auctions.Find(info => info.AuctionId == auctionId);

            if (auction == null)
            {
                buyer.SendLocalizeString(auctionId.ToString(), ChatLogFilters.CHATLOGFILTERS_USER_ERROR, GameData.Localized_text.TEXT_AUCTION_ITEM_NO_LONGER_EXISTS);
                return;
            }

            if (auction.SellPrice != price)
            {
                if (price == 0 && auction.SellerId == buyer.CharacterId) // cancel?
                    cancel = true;
                else
                {
                    buyer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, GameData.Localized_text.TEXT_AUCTION_UNKNOWN_ERROR);
                    return;
                }
            }
            else if (!buyer.RemoveMoney(auction.SellPrice))
            {
                buyer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, GameData.Localized_text.TEXT_AUCTION_NOT_ENOUGH_MONEY);
                return;
            }

            // Remove live auction
            lock (Auctions)
                Auctions.Remove(auction);
            auction.Dirty = true;
            CharMgr.Database.DeleteObject(auction);

            // SendMail to seller
            if (!cancel) // seller dosent need this if hes canceling
            {
                if (auction.Seller != null)
                {
                    Character_mail sellerMail = new Character_mail
                    {
                        Guid = CharMgr.GenerateMailGuid(),
                        CharacterId = auction.SellerId,
                        ReceiverName = auction.Seller.Name,
                        SendDate = (uint)TCPManager.GetTimeStamp(),
                        AuctionType = 2,
                        Content = auction.Item.Name,
                        Money = (uint)(auction.SellPrice * AUCTION_HOUSE_TAX_MULT),
                        Opened = false
                    };
                    // Complete

                    CharMgr.AddMail(sellerMail);
                }
            }

            // SendMail to buyer
            Character_mail buyerMail = new Character_mail
            {
                Guid = CharMgr.GenerateMailGuid(),
                CharacterId = buyer.CharacterId,
                ReceiverName = buyer.Name,
                SendDate = (uint) TCPManager.GetTimeStamp(),
                AuctionType = cancel ? (byte)3 : (byte)5,
                Content = auction.Item.Name,
                Money = 0,
                Opened = false
            };

            buyerMail.Items.Add(new MailItem(auction.ItemId, auction._Talismans, auction.PrimaryDye, auction.SecondaryDye, auction.Count));

            CharMgr.AddMail(buyerMail);

            // Send a list
            if (cancel)
            {
                SendAuctions(buyer, "", buyer.Name, 0, 0, 0, 0, new List<byte>(), new List<byte>(), 0);
                buyer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, GameData.Localized_text.TEXT_AUCTION_CANCEL_SUCCESSFUL);
            }
            else
            {
                SendAuctions(buyer, "", "", 0, 0, 0, 0, new List<byte>(), new List<byte>(), 0);
                buyer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, GameData.Localized_text.TEXT_AUCTION_BUYOUT_SUCCESSFUL);
            }
        }
    
        public static byte PlayerAuctionCount(uint sellerId)
        {
            byte count = 0;
            lock (Auctions)
            {
                foreach (Common.Auction auct in Auctions)
                    if (sellerId == auct.SellerId)
                        count++;
            }
            return count;
        }

        public static void CheckAuctionExpiry(object arg)
        {
            int removeCount = 0;

            long expireTimeStart = TCPManager.GetTimeStamp() - (60 * 60 * 24 * 30);

            lock (Auctions)
            {
                for (int i = 0; i < Auctions.Count; ++i)
                {
                    if (Auctions[i].StartTime >= expireTimeStart)
                        continue;

                    Common.Auction auction = Auctions[i];

                    if (auction.Item == null)
                        auction.Item = ItemService.GetItem_Info(auction.ItemId);

                    if (auction.Item != null)
                    {
                        // return item to lister
                        Character_mail expireMail = new Character_mail
                        {
                            Guid = CharMgr.GenerateMailGuid(),
                            CharacterId = auction.SellerId,
                            ReceiverName = auction.Seller.Name,
                            SendDate = (uint) TCPManager.GetTimeStamp(),
                            AuctionType = 3,
                            Content = auction.Item.Name,
                            Money = 0,
                            Opened = false
                        };

                        expireMail.Items.Add(new MailItem(auction.ItemId, auction._Talismans, auction.PrimaryDye, auction.SecondaryDye, auction.Count));

                        CharMgr.AddMail(expireMail);
                    }

                    CharMgr.Database.DeleteObject(auction);
                    Auctions.RemoveAt(i);

                    ++removeCount;

                    --i;
                }

                Log.Info("Auction House", $"Removed {removeCount} expired {(removeCount == 1 ? "auction": "auctions")}.");
            }
        }
    }
}
