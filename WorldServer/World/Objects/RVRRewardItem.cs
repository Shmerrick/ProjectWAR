using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Database.World.Battlefront;

namespace WorldServer.World.Objects
{
    public class RVRRewardItem
    {
        public int RewardId { get; set; }
        public int Rarity { get; set; }
        public int RRBand { get; set; }
        public int Class { get; set; }
        public int ItemId { get; set; }
        public int ItemCount { get; set; }
        public byte CanAwardDuplicate { get; set; }

        public RVRRewardItem(RVRRewardFortItems fortItem)
        {
            RewardId = fortItem.RewardId;
            Rarity = fortItem.Rarity;
            RRBand = fortItem.RRBand;
            Class = fortItem.Class;
            ItemId = fortItem.ItemId;
            ItemCount = fortItem.ItemCount;
            CanAwardDuplicate = fortItem.CanAwardDuplicate;

        }

        public RVRRewardItem(RVRRewardKeepItems keepItem)
        {
            RewardId = keepItem.RewardId;
            Rarity = keepItem.Rarity;
            RRBand = keepItem.RRBand;
            Class = keepItem.Class;
            ItemId = keepItem.ItemId;
            ItemCount = keepItem.ItemCount;
            CanAwardDuplicate = keepItem.CanAwardDuplicate;
        }

        public RVRRewardItem()
        {
            
        }
    }
}
