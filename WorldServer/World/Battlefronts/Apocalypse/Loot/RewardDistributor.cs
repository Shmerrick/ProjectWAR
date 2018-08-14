using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Common;
using Common.Database.World.Battlefront;
using WorldServer.Services.World;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public class RewardDistributor
    {
        public List<RVRZoneLockReward> ZoneLockRewards { get; private set; }

        public RewardDistributor(List<RVRZoneLockReward> zoneLockRewards)
        {
            ZoneLockRewards = zoneLockRewards;
        }

        public void Distribute(LootBagTypeDefinition lootBag)
        {
            // Combine the lootBag reward
            var lockReward = ZoneLockRewards.Select(x => x.RRBand == lootBag.RenownBand);

        }
    }
}
