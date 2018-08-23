using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Common;
using Common.Database.World.Battlefront;
using FrameWork;
using GameData;
using NLog;
using WorldServer.Services.World;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public class RewardDistributor
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public List<RVRZoneLockReward> ZoneLockRewards { get; private set; }
        public IRandomGenerator RandomGenerator { get; }

        public RewardDistributor(List<RVRZoneLockReward> zoneLockRewards, IRandomGenerator randomGenerator)
        {
            ZoneLockRewards = zoneLockRewards;
            RandomGenerator = randomGenerator;
        }

        public string Distribute(LootBagTypeDefinition lootBag, Player player, byte playerRenownBand)
        {
            // Combine the lootBag reward
            var lockReward = ZoneLockRewards.SingleOrDefault(x => x.RRBand == playerRenownBand);

          
            player.AddXp((uint) lockReward.XP, false, false);
            player.AddRenown((uint) lockReward.Renown, false, RewardType.ZoneKeepCapture, "");
            player.AddMoney((uint) lockReward.Money);
            player.ItmInterface.CreateItem((uint)lockReward.ItemId, (ushort)lockReward.ItemCount);

        
            var lootBagItemId = Convert.ToInt32(LootBagTypeDefinition.GetDescription(lootBag.BagRarity));
            var lootBagItem = ItemService.GetItem_Info((uint)lootBagItemId);
            var tl = new List<Talisman> { new Talisman(lootBag.ItemId, 1, 0, 0) };
            var result = player.ItmInterface.CreateItem(lootBagItem, 1, tl, 0, 0, false, 0, false);

            _logger.Info($"Distributing reward to {player.Name}. Result = {result}");

            var lootRewardDescription = string.Empty;
            if ((lockReward.ItemCount != 0) && (lockReward.ItemId != 0))
            {
                var lockItem = ItemService.GetItem_Info((uint) lockReward.ItemId);
                if (lockItem != null)
                {
                    lootRewardDescription += $"You have been awarded {lockReward.ItemCount} {lockItem.Name}. ";
                }
            }

            lootRewardDescription += $"For your valiant efforts you have also won {lootBag.FormattedString()}! ";
            return lootRewardDescription;
        }
    }
}
