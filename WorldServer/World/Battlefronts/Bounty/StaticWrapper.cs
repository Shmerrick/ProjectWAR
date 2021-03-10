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