using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.NewDawn.Rewards;

namespace WorldServer.World.Battlefronts.Bounty
{
    public interface IStaticWrapper
    {
        RenownBandReward GetRenownBandReward(int renownBand);
    }

    public class StaticWrapper : IStaticWrapper
    {
        public RenownBandReward GetRenownBandReward(int renownBand)
        {
            return RewardService.GetRenownBandReward(renownBand);
        }
    }
}
