using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Database.World.Characters;
using WorldServer.Managers;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Apocalypse;

namespace WorldServer.World.Objects
{
    public class RenownLevelVendorItem
    {
        public List<Vendor_items> items = new List<Vendor_items>();

        public RenownLevelVendorItem(int renown, int level)
        {
            var item = new Vendor_items
            {
                Info = ItemService.GetItem_Info(2),
                ItemId = 2,
                Price = (uint)(renown * 100 + level),
                VendorId = 0
            };

            items.Add(item);
        }
    }

    public class RealmCaptainVendorItem
    {
        public List<Vendor_items> items = new List<Vendor_items>();

        public RealmCaptainVendorItem(Player player)
        {
            if (RealmCaptainManager.IsPlayerRealmCaptain(player.CharacterId))
            {
                items = GetRealmCaptainItems(player);
            }
        }


        private List<Vendor_items> GetRealmCaptainItems(Player player)
        {
            var realmCaptainItems = new Dictionary<int, string>
            {
                {1298378490, "+10% to all Stats"}, {1298378491, "+5% RR/INF"}, {1298378492, "+5% Critical chance"}
            };

            //, {1298380000, "Ally Shield"} -- removed until I can get it working.

            foreach (var realmCaptainBuff in realmCaptainItems)
            {

                var item = new Vendor_items
                {
                    Info = ItemService.GetItem_Info((uint)realmCaptainBuff.Key),
                    ItemId = (uint)realmCaptainBuff.Key,
                    Price = 0,
                    VendorId = 0
                };
                items.Add(item);

            }
            return items;
        }

        public bool IsValidItemForPlayer(Player player, HonorReward honorReward)
        {
            if (honorReward.Realm == 0 || honorReward.Realm == (int)player.Realm)
            {
                if (honorReward.Class == 0 || honorReward.Class == player.Info.CareerLine)
                {
                    // Ensure the player doesn't have more than max count of these items.
                    if (!player.GetCountOfPlayerItems(honorReward.ItemId, honorReward.MaxCount))
                    {
                        if (HonorItemCooldown(honorReward.ItemId, player) <
                            FrameWork.TCPManager.GetTimeStamp() || honorReward.Cooldown == 0)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        // What is the time (seconds) that this item will be re-purchasble.

        private long HonorItemCooldown(int honorRewardItemId, Player player)
        {
            var cooldown = player.Info.HonorCooldowns?.SingleOrDefault(x => x.ItemId == honorRewardItemId);
            if (cooldown == null)
                return 0;
            else
                return cooldown.Cooldown;
        }
    }
}
