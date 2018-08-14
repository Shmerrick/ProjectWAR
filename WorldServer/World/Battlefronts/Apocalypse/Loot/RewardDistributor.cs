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

        public void Distribute(LootBagTypeDefinition lootBag, Player player)
        {
            // Combine the lootBag reward
            var lockReward = ZoneLockRewards.Single(x => x.RRBand == lootBag.RenownBand);

            var randomMultiplier = RandomGenerator.Generate(25);
            player.AddXp((uint)(lockReward.XP * (1 + (randomMultiplier / 100))), false, false);
            player.AddRenown((uint)(lockReward.Renown * (1 + (randomMultiplier / 100))), false, RewardType.ZoneKeepCapture, "Zone Capture");

            player.ItmInterface.CreateItem((uint)lockReward.ItemId, (ushort)lockReward.ItemCount);

            var lootBagItemId = Convert.ToInt32(LootBagTypeDefinition.GetDescription(lootBag.BagRarity));
            var lootBagItem = ItemService.GetItem_Info((uint)lootBagItemId);
            var tl = new List<Talisman> { new Talisman(lootBag.ItemId, 1, 0, 0) };

            var result = player.ItmInterface.CreateItem(lootBagItem, 1, tl, 0, 0, false, 0, false);
            _logger.Info($"Distributing reward to {player.Name}. Result = {result}");

        }
    }
}
