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
using WorldServer.World.Battlefronts.Bounty;
using WorldServer.World.Objects;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public class KeepLockRewardDistributor
    {
        private static readonly Logger RewardLogger = LogManager.GetLogger("RewardLogger");
        public IRandomGenerator RandomGenerator { get; }
        public List<RVRKeepLockReward> KeepLockRewards { get; set; }

        public KeepLockRewardDistributor(IRandomGenerator randomGenerator, List<RVRKeepLockReward> keepLockRewards)
        {
            RandomGenerator = randomGenerator;
            KeepLockRewards = keepLockRewards;
        }

        public void DistributeNonBagAwards(Player player, byte playerRenownBand, double modifier)
        {
            if (player == null)
                return;

            var lockReward = KeepLockRewards.SingleOrDefault(x => x.RRBand == playerRenownBand);

            if (lockReward == null)
            {
                RewardLogger.Warn($"Could not find renownBand for player : {player.Name}.");
                return;
            }

            var xp = (uint)lockReward.XP * modifier;
            var rr = (uint)lockReward.Renown * modifier;
            var money = (uint)lockReward.Money * modifier;
            var influence = (ushort)lockReward.Influence * modifier;

            ushort crestCount = 0;

            if ((ushort) lockReward.ItemCount > 0)
                crestCount = (ushort) (((ushort) lockReward.ItemCount) + 1);

            player.AddXp((uint)(xp), false, false);
            player.AddRenown((uint)rr, false, RewardType.None, "");
            player.AddMoney((uint)money);

			if (player.CurrentArea != null)
			{
				ushort influenceId = 0;
				if (player.Realm == Realms.REALMS_REALM_ORDER)
					influenceId = (ushort)player.CurrentArea.OrderInfluenceId;
				else
					influenceId = (ushort)player.CurrentArea.DestroInfluenceId;
				player.AddInfluence(influenceId, (ushort)influence);
			}

            if ((ushort) crestCount > 0)
            {
                player.ItmInterface.CreateItem((uint) lockReward.ItemId, (ushort) crestCount);
                player.SendClientMessage($"You have been awarded {(ushort) crestCount} war crests.");
            }

            RewardLogger.Info($"RR {rr} Money {money} INF {influence} Crests {(uint)lockReward.ItemId} ({(ushort)crestCount}) to {player.Name} {player.CharacterId}. Modifier = {modifier}");
        }

        
        /// <summary>
        /// Builds the bag containing the players items.
        /// </summary>
        /// <param name="lootBag"></param>
        /// <param name="player"></param>
        /// <param name="playerRenownBand"></param>
        /// <returns></returns>
        public KeyValuePair<Item_Info, List<Talisman>> BuildChestLootBag(LootBagTypeDefinition lootBag, Player player)
        {
            var lootRewardDescription = string.Empty;
            // Get the bag item id
            var lootBagItemId = Convert.ToInt32(LootBagTypeDefinition.GetDescription(lootBag.BagRarity));
            // Get the bag item object
            var lootBagItem = ItemService.GetItem_Info((uint)lootBagItemId);
            var lootBagContents = new List<Talisman>
            {
                new Talisman(lootBag.ItemId, (byte) lootBag.ItemCount, 0, 0)
            };

            // RewardLogger.Info($"Distributing reward of {lootBagItem.Name}, containing {lootBag.ItemId} ({lootBag.ItemCount}) to {player.Name}. Result = {result}");
            
            return new KeyValuePair<Item_Info, List<Talisman>>(lootBagItem, lootBagContents);

        }

        /// <summary>
        /// Builds the bag containing the players items.
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<Item_Info, List<Talisman>> BuildChestLootBag(LootBagRarity rarity, uint itemId, Player player)
        {
            var lootRewardDescription = string.Empty;
            // Get the bag item id
            var lootBagItemId = Convert.ToInt32(LootBagTypeDefinition.GetDescription(rarity));
            // Get the bag item object
            var lootBagItem = ItemService.GetItem_Info(itemId);
            var lootBagContents = new List<Talisman>
            {
                new Talisman(itemId, (byte) 1, 0, 0)
            };

            return new KeyValuePair<Item_Info, List<Talisman>>(lootBagItem, lootBagContents);
        }
    }
}
