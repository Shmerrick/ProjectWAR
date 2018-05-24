using Common;
using FrameWork;
using System.Collections.Generic;

namespace WorldServer.Services.World
{
    [Service(typeof(ItemService))]
    public class VendorService : ServiceBase
    {
        public static int MAX_ITEM_PAGE = 3;

        public static Dictionary<ushort, List<Vendor_items>> _newVendors = new Dictionary<ushort, List<Vendor_items>>();

        public static List<Vendor_items> GetVendorItems(ushort id)
        {
            if (!_newVendors.ContainsKey(id))
            {
                Log.Debug("WorldMgr", "Loading Vendors of " + id + " ...");

                IList<Vendor_items> IVendors = Database.SelectObjects<Vendor_items>("VendorId=" + id);
                List<Vendor_items> Vendors = new List<Vendor_items>();
                Vendors.AddRange(IVendors);

                _newVendors.Add(id, Vendors);
                // commenting out this next line as it does not seem to be used... 
                //  Item_Info Req;
                foreach (Vendor_items Info in Vendors.ToArray())
                {
                    if ((Info.Info = ItemService.GetItem_Info(Info.ItemId)) == null)
                    {
                        Vendors.Remove(Info);
                        continue;
                    }
                }

                Log.Debug("LoadCreatureVendors", "Loaded " + Vendors.Count + " Vendors of " + id);
            }

            return _newVendors[id];
        }

        /*
        public static Dictionary<uint, List<Creature_vendor>> _Vendors = new Dictionary<uint, List<Creature_vendor>>();

        public static List<Creature_vendor> GetVendorItems(uint Entry)
        {
            if (!_Vendors.ContainsKey(Entry))
            {
                Log.Debug("WorldMgr", "Loading Vendors of " + Entry + " ...");

                IList<Creature_vendor> IVendors = Database.SelectObjects<Creature_vendor>("Entry=" + Entry);
                List<Creature_vendor> Vendors = new List<Creature_vendor>();
                Vendors.AddRange(IVendors);

                _Vendors.Add(Entry, Vendors);

                foreach (Creature_vendor Info in Vendors.ToArray())
                {
                    if ((Info.Info = ItemService.GetItem_Info(Info.ItemId)) == null)
                    {
                        Vendors.Remove(Info);
                        continue;
                    }
                }

                Log.Debug("LoadCreatureVendors", "Loaded " + Vendors.Count + " Vendors of " + Entry);
            }

            return _Vendors[Entry];
        }
        */
    }
}
