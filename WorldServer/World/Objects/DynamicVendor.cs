using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using WorldServer.Services.World;

namespace WorldServer.World.Objects
{
    public class RenownLevelVendorItem
    {
        public List<Vendor_items> items = new List<Vendor_items>();

        public RenownLevelVendorItem(int renown, int level)
        {
            var item = new Vendor_items
            {
                Info = ItemService.GetItem_Info(2), ItemId = 2, Price = (uint) (renown * 100 + level), VendorId = 0
            };

            items.Add(item);
        }
    }
}
