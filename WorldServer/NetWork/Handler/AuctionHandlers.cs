using System.Collections.Generic;
using SystemData;
using Common;
using FrameWork;
using WorldServer.World.Auction;
using WorldServer.World.Objects;

namespace WorldServer.NetWork.Handler
{
    public class AuctionHandlers : IPacketHandler
    {
        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_AUCTION_SEARCH_QUERY, "onAuctionSearchQuery")]
        public static void F_AUCTION_SEARCH_QUERY(BaseClient client, PacketIn packet)
        {
            GameClient cclient = (GameClient) client;

            if (!cclient.IsPlaying() || !cclient.Plr.IsInWorld())
                return;

            Player plr = cclient.Plr;
            packet.Skip(3);
            byte NumOfSearches = packet.GetUint8(); // Why?
            packet.Skip(12);
            // Item Level
            byte MinLevel = packet.GetUint8();
            byte MaxLevel = packet.GetUint8();
            byte Rarity = packet.GetUint8();
            byte career = packet.GetUint8();
            packet.Skip(6);

            byte NumTypes = packet.GetUint8();
            List<byte> Types = new List<byte>();
            for (byte i = 0; i < NumTypes; i++)
                Types.Add(packet.GetUint8());

            byte NumSlots = packet.GetUint8();
            List<byte> Slots = new List<byte>();
            for (byte i = 0; i < NumSlots; i++)
                Slots.Add(packet.GetUint8());

            bool IsStatistic = packet.GetUint8() == 1;
            byte Stat = 0;
            if (IsStatistic)
                Stat = packet.GetUint8();

            string SearchTerm = packet.GetPascalString();
            string Character = packet.GetPascalString().Replace("^M", string.Empty).Replace("^F", string.Empty);

            AuctionHouse.SendAuctions(plr, SearchTerm, Character, MinLevel, MaxLevel, Rarity, career, Types, Slots, Stat);

            cclient.IsPlaying();
        }
        
        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_AUCTION_POST_ITEM, "onAuctionPostItem")]
        public static void F_AUCTION_POST_ITEM(BaseClient client, PacketIn packet)
        {
            GameClient cclient = (GameClient) client;

            if (!cclient.IsPlaying() || !cclient.Plr.IsInWorld())
                return;

            Player plr = cclient.Plr;

            if(AuctionHouse.PlayerAuctionCount(plr.CharacterId) >= AuctionHouse.MAX_AUCTIONS_PER_CHAR)
            {
                plr.SendMessage("You have reached the maximum of " + AuctionHouse.MAX_AUCTIONS_PER_CHAR+" auctions.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            ushort slot = packet.GetUint16();
            packet.Skip(2);
            uint price = packet.GetUint32();

            if(plr.ItmInterface.GetItemInSlot(slot) == null)
                return;

            if (plr.ItmInterface.GetItemInSlot(slot).BoundtoPlayer || plr.ItmInterface.GetItemInSlot(slot).Info.Bind == 1)
            {
                plr.SendLocalizeString(plr.ItmInterface.GetItemInSlot(slot).Info.Name, ChatLogFilters.CHATLOGFILTERS_USER_ERROR, GameData.Localized_text.TEXT_AUCTION_ITEM_IS_BOUND);
                return;
            }

            Auction auction = new Auction
            {
                Item = plr.ItmInterface.GetItemInSlot(slot).Info,
                ItemId = plr.ItmInterface.GetItemInSlot(slot).Info.Entry,
                _Talismans = plr.ItmInterface.GetItemInSlot(slot).GetTalismans(),
                PrimaryDye = plr.ItmInterface.GetItemInSlot(slot).GetPrimaryDye(),
                SecondaryDye = plr.ItmInterface.GetItemInSlot(slot).GetSecondaryDye(),
                Count = plr.ItmInterface.GetItemInSlot(slot).Count,
                Seller = plr.Info,
                SellerId = plr.CharacterId,
                SellPrice = price,
                StartTime = (uint) TCPManager.GetTimeStamp(),
                Realm = plr.Info.Realm,
                AuctionId = (uint) AuctionHouse.GenerateAuctionGUID()
            };

            AuctionHouse.AddAuction(auction);
            plr.ItmInterface.DeleteItem(slot);
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_AUCTION_BID_ITEM, "onAuctionBidItem")]
        public static void F_AUCTION_BID_ITEM(BaseClient client, PacketIn packet)
        {
            GameClient cclient = (GameClient) client;

            if (!cclient.IsPlaying() || !cclient.Plr.IsInWorld())
                return;

            uint price = packet.GetUint32();
            packet.GetUint32(); // unk

            // item id
            // 10 bytes which are sent along with the item list, might be a very long number however lets take the first 8
            ulong Id = packet.GetUint64();
            packet.Skip(2); // last byte ranges from 01 - 03? a flag?

            packet.GetUint32(); // unk

            AuctionHouse.BuyOutAuction(cclient.Plr, Id, price);
        }
    }
}