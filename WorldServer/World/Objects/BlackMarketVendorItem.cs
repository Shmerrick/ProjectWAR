using Common;
using System.Collections.Generic;
using System.Linq;
using WorldServer.Services.World;

namespace WorldServer.World.Objects
{
    public class BlackMarketVendorItem
    {
        public List<Vendor_items> BlackMarketVendorItems = new List<Vendor_items>();

        public BlackMarketVendorItem(Player player)
        {
            BlackMarketVendorItems = GetBlackMarketItems(player);
        }


        public List<Vendor_items> GetBlackMarketItems(Player player)
        {
            var items = ItemService._BlackMarket_Items.Where(x => x.RealmId == (int) player.Realm).ToList();
            var resultList = new List<Vendor_items>();
            var playerItems = player.ItmInterface.Items;

            foreach (var blackMarketItem in items)
            {
                if (player.ItmInterface.HasItemCountInInventory((uint) blackMarketItem.ItemId, 1))
                {

                    // Create new Item to trade the blackmarket item for
                    var tradingItem = ItemService.GetItem_Info((uint) 1298378521);
                    tradingItem.Description = "Black Market";

                    var item = new Vendor_items
                    {
                        Info = tradingItem,
                        ItemId = tradingItem.Entry,
                        Price = 0,
                        //VendorId = 0, ReqItems = $"(1,208470)"
                        VendorId = 0, ReqItems = $"(1,{blackMarketItem.ItemId})"
                    };
                    resultList.Add(item);
                }
            }

            return resultList;
        }
    }
}