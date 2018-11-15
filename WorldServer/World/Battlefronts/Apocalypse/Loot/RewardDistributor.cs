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

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public class RewardDistributor
    {
        private static readonly Logger RewardLogger = LogManager.GetLogger("RewardLogger");
        public List<RVRZoneLockReward> ZoneLockRewards { get; private set; }


        public IRandomGenerator RandomGenerator { get; }

        public RewardDistributor(List<RVRZoneLockReward> zoneLockRewards, IRandomGenerator randomGenerator)
        {
            ZoneLockRewards = zoneLockRewards;
            RandomGenerator = randomGenerator;
        }

        public void DistributeNonBagAwards(Player player, byte playerRenownBand, double modifier)
        {
            if (player == null)
                return;

            var lockReward = ZoneLockRewards.SingleOrDefault(x => x.RRBand == playerRenownBand);

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
                crestCount = (ushort) (((ushort) lockReward.ItemCount * modifier) + 1);

            player.AddXp((uint)(xp * modifier), false, false);
            player.AddRenown((uint)rr, false, RewardType.ZoneKeepCapture, "");
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

            RewardLogger.Info($"RR {rr} Money {money} INF {influence} Crests {(uint)lockReward.ItemId} ({(ushort)crestCount}) to {player.Name}. Modifier = {modifier}");
        }

        

        public string DistributeWinningRealm(LootBagTypeDefinition lootBag, Player player, byte playerRenownBand)
        {
            //// Combine the lootBag reward
            //var lockReward = ZoneLockRewards.SingleOrDefault(x => x.RRBand == playerRenownBand);

            //if (lockReward == null)
            //{
            //    RewardLogger.Warn($"Could not find renownBand for player : {player.Name}.");
            //    return "";
            //}

            var lootRewardDescription = string.Empty;
            // Get the bag item id
            var lootBagItemId = Convert.ToInt32(LootBagTypeDefinition.GetDescription(lootBag.BagRarity));
            // Get the bag item object
            var lootBagItem = ItemService.GetItem_Info((uint)lootBagItemId);
            var internalBagContainer = new List<Talisman>
            {
                new Talisman(lootBag.ItemId, (byte) lootBag.ItemCount, 0, 0)
            };

            // Create a 'talisman' from the reward Item
            var result = player.ItmInterface.CreateItem(lootBagItem, 1, internalBagContainer, 0, 0, false, 0, false);

            RewardLogger.Info($"Distributing reward of {lootBagItem.Name}, containing {lootBag.ItemId} ({lootBag.ItemCount}) to {player.Name}. Result = {result}");
            
            //if ((lockReward.ItemCount != 0) && (lockReward.ItemId != 0))
            //{
            //    //crests etc
            //    var lockItem = ItemService.GetItem_Info((uint)lockReward.ItemId);
            //    if (lockItem != null)
            //    {
            //        lootRewardDescription += $"You have been awarded {lockReward.ItemCount} {lockItem.Name}. ";
            //    }
            //}

            lootRewardDescription += $"For your efforts you have also won {lootBag.FormattedString()}! ";

            return lootRewardDescription;
        }
    }
}
