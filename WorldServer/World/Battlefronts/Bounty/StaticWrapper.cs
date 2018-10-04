using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Database.World.Battlefront;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.NewDawn.Rewards;

namespace WorldServer.World.Battlefronts.Bounty
{
    public interface IStaticWrapper
    {
        RewardPlayerKill GetRenownBandReward(int renownBand);
        ContributionDefinition GetDefinition(byte value);
    }

    public class StaticWrapper : IStaticWrapper
    {
        public RewardPlayerKill GetRenownBandReward(int renownBand)
        {
            return RewardService.GetPlayerKillReward(renownBand);
        }

        public ContributionDefinition GetDefinition(byte value)
        {
            return BountyService.GetDefinition(value);
        }
    }
}
