using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Database.World.Characters;
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
                Info = ItemService.GetItem_Info(2),
                ItemId = 2,
                Price = (uint)(renown * 100 + level),
                VendorId = 0
            };

            items.Add(item);
        }
    }

    public class HonorVendorItem
    {
        public List<Vendor_items> items = new List<Vendor_items>();

        public HonorVendorItem(Player player)
        {
            switch (player.Info.HonorRank)
            {
                case 1:
                    {
                        items = GetHonorRankItems(player, 1);
                        break;
                    }
                case 2:
                    {
                        items = GetHonorRankItems(player, 2);
                        break;
                    }
                case 3:
                    {
                        items = GetHonorRankItems(player, 3);
                        break;
                    }
            }
        }


        private List<Vendor_items> GetHonorRankItems(Player player, int rank)
        {
            var rankItems = HonorService.HonorRewards.Where(x => x.HonorRank == rank);

            foreach (var honorReward in rankItems)
            {
                if (IsValidItemForPlayer(player, honorReward))
                {
                    var item = new Vendor_items
                    {
                        Info = ItemService.GetItem_Info((uint)honorReward.ItemId),
                        ItemId = (uint)honorReward.ItemId,
                        Price = 0,
                        VendorId = 0
                    };
                    items.Add(item);
                }
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

    public class RealmCaptainVendorItem
    {
        public List<Vendor_items> items = new List<Vendor_items>();

        public RealmCaptainVendorItem(Player player)
        {
            // TODO.
        }

    }
}
