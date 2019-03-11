using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Database.World.Battlefront;
using WorldServer.Services.World;

namespace WorldServer.World.Battlefronts.Bounty
{
    public interface IStaticWrapper
    {
        RewardPlayerKill GetRenownBandReward(int renownBand);
    }

    public class StaticWrapper : IStaticWrapper
    {
        public RewardPlayerKill GetRenownBandReward(int renownBand)
        {
            return RewardService.GetPlayerKillReward(renownBand);
        }

    }
}
