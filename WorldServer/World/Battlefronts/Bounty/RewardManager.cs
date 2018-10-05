using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using FrameWork;
using GameData;
using NLog;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.NewDawn.Rewards;

namespace WorldServer.World.Battlefronts.Bounty
{
    public class RewardManager
    {
        private static readonly Logger RewardLogger = LogManager.GetLogger("RewardLogger");

        public const float BOUNTY_BASE_RP_MODIFIER = 2.5f;
        public const float BOUNTY_BASE_XP_MODIFIER = 5.5f;
        public const float BOUNTY_BASE_INF_MODIFIER = 0.4f;
        public const float BOUNTY_BASE_MONEY_MODIFIER = 10f;

        public IBountyManager BountyManager { get; }
        public IContributionManager ContributionManager { get; }
        public IImpactMatrixManager ImpactMatrixManager { get; }
        public IStaticWrapper StaticWrapper { get; }
        public List<RewardPlayerKill> PlayerKillRewardBand { get; }

        public RewardManager(IBountyManager bountyManager, IContributionManager contributionManager, IImpactMatrixManager impactMatrixManager, IStaticWrapper staticWrapper, List<RewardPlayerKill>  playerKillRewardBand)
        {
            BountyManager = bountyManager;
            ContributionManager = contributionManager;
            ImpactMatrixManager = impactMatrixManager;
            StaticWrapper = staticWrapper;
            PlayerKillRewardBand = playerKillRewardBand;
        }

        public virtual int GetInsigniaItemId(int renownBand)
        {
            return (int)PlayerKillRewardBand.Single(x => x.RenownBand == renownBand).CrestId;
        }

        public virtual int GetInsigniaItemCount(int renownBand)
        {
            return (int)PlayerKillRewardBand.Single(x => x.RenownBand == renownBand).CrestCount;
        }

        public virtual double GetRenownBandMoneyBase(int renownBand)
        {
            return StaticWrapper.GetRenownBandReward(renownBand).Money;
        }

        /// <summary>
        /// Calculate the base reward for all impacters upon the target.
        /// </summary>
        /// <returns>List of impacter characterId's and their reward.</returns>
        public ConcurrentDictionary<uint, Reward> GenerateBaseRewardForKill(uint targetCharacterId, int randomNumber, float aaoBonus)
        {
            var characterBounty = BountyManager.GetBounty(targetCharacterId);
            var contributionValue = ContributionManager.GetContributionValue(targetCharacterId);
            var impacts = ImpactMatrixManager.GetKillImpacts(targetCharacterId);
            var totalImpact = ImpactMatrixManager.GetTotalImpact(targetCharacterId);

            RewardLogger.Info($"Calculating Reward for impacting {targetCharacterId} (the target)");
            RewardLogger.Debug($"Target Character Bounty : {characterBounty.ToString()}");
            RewardLogger.Debug($"Target Character Contribution : {contributionValue}");
            RewardLogger.Debug($"Impacts upon Target Character : {String.Join(",", impacts)}");

     
            if (totalImpact == 0)
                throw new BountyException("Total Impact == 0");

            var rewardDictionary = new ConcurrentDictionary<uint, Reward>();

            // return the bounty of the killed player (the base bounty + their contribution)
            var modifiedBountyValue = CalculateModifiedBountyValue(characterBounty, contributionValue);
            RewardLogger.Debug($"Target Modified Bounty Value : {modifiedBountyValue}");

            foreach (var playerImpact in impacts)
            {
                var impactFraction = CalculateImpactFraction(playerImpact.ImpactValue, totalImpact);

                int insigniaCount = 0;
                int insigniaItemId = 0;

                // Select type of insignia (should it be based on the victims RR??)
                var renownBand = Reward.DetermineRenownBand(characterBounty.RenownLevel);
                var playerKillReward = PlayerKillRewardBand.Single(x => x.RenownBand == renownBand);

                // Chance of insignia drop is the impact Fraction.
                if (randomNumber < (impactFraction * 100))
                {
                    insigniaCount = StaticRandom.Instance.Next(1, playerKillReward.CrestCount);
                    insigniaItemId = playerKillReward.CrestId;
                }
                var reward = new Reward
                {
                    Description = $"Player {playerImpact.CharacterId} Kills {targetCharacterId} ",
                    BaseInf = (int) (BOUNTY_BASE_INF_MODIFIER * modifiedBountyValue * impactFraction * playerImpact.ModificationValue * aaoBonus) + playerKillReward.BaseInf,
                    BaseXP = (int) (BOUNTY_BASE_XP_MODIFIER * modifiedBountyValue * impactFraction * playerImpact.ModificationValue * aaoBonus) + playerKillReward.BaseXP,
                    BaseRP = (int) (BOUNTY_BASE_RP_MODIFIER * modifiedBountyValue * impactFraction * playerImpact.ModificationValue * aaoBonus) + playerKillReward.BaseRP,
                    InsigniaCount = insigniaCount,
                    InsigniaItemId = insigniaItemId,
                    BaseMoney = (int) (playerKillReward.Money * impactFraction),
                    RenownBand = renownBand

                };
                rewardDictionary.TryAdd(playerImpact.CharacterId, reward);
                RewardLogger.Debug($"Reward : {reward.ToString()} applied to {playerImpact.CharacterId}");
            }

            return rewardDictionary;
        }

        private float CalculateImpactFraction(float impactValue, float totalImpact)
        {
            return impactValue / totalImpact;
        }

        private float CalculateModifiedBountyValue(CharacterBounty characterBounty, short contributionValue)
        {
            return characterBounty.BaseBountyValue + contributionValue;
        }

        public void DistributePlayerKillRewards(Player victim, Player killer, float aaoBonus, ushort influenceId, Dictionary<uint, Player> playerDictionary)
        {
            // TODO : Ensure the player is actually in the active battlefront.
            // List of players involved in the kill
            var playersToReward = GenerateBaseRewardForKill(
                victim.CharacterId,
                StaticRandom.Instance.Next(1, 100),
                aaoBonus);

            foreach (var playerReward in playersToReward)
            {
                // reward key is the characterId
                if (playerDictionary.ContainsKey(playerReward.Key))
                {
                    var playerToBeRewarded = playerDictionary[playerReward.Key];

                    // Assign non-contrib/insignia rewards to player involved in the death of the victim
                    if (playerToBeRewarded.PriorityGroup != null)
                    {
                        DistributeGroupSharedRewards(playerToBeRewarded.PriorityGroup.Members, playerReward.Value, influenceId, $"Assisting {killer.Name} in killing {victim.Name}");
                    }
                    else  // No group
                    {
                        DistributeBaseRewardsForPlayerKill(playerReward.Value, playerToBeRewarded, 1, influenceId, $"Deathblow to {victim.Name}");
                    }

                    // If this player is the killer (ie Deathblow), give them a different contribution.
                    if (playerReward.Key == killer.CharacterId)
                    {
                        // Give the insignia and contrib rewards to the killer.
                        DistributeDeathBlowRewardsForPlayerKill(playerReward.Value, killer, victim.Name);
                    }
                    else // An assist
                    {
                        DistributKillAssistRewardsForPlayerKill(playerReward.Value, playerToBeRewarded, victim.Name);
                    }
                }

            }

        }

        /// <summary>
        /// The killer is the player that performed the deathblow. 
        /// </summary>
        /// <param name="reward"></param>
        /// <param name="killer"></param>
        private void DistributeDeathBlowRewardsForPlayerKill(Reward reward, Player killer, string victimName)
        {

            RewardLogger.Info($"Death Blow rewards given to {killer.Name} ({killer.CharacterId})");
            DistributeInsigniaRewardForPlayerKill(reward, killer, victimName);

            killer.UpdatePlayerBountyEvent((byte)ContributionDefinitions.PLAYER_KILL_DEATHBLOW);

            if (killer.AAOBonus > 0)
            {
                killer.SendClientMessage($"CONTRIB:Giving DB rewards under AAO to {killer.Name}");
                // Add contribution for this kill under AAO to the killer.
                killer.UpdatePlayerBountyEvent((byte)ContributionDefinitions.PLAYER_KILL_DEATHBLOW_UNDER_AAO);
            }

            // If the deathblow comes while the target is near a BO
            if (killer.CurrentObjectiveFlag != null)
            {
                killer.SendClientMessage($"CONTRIB:Giving PLAYER_KILL_ON_BO to {killer.Name}");

                killer.UpdatePlayerBountyEvent((byte)ContributionDefinitions.PLAYER_KILL_ON_BO);
            }
        }


       

        /// <summary>
        /// Distribute rewards for a group involved in a player kill
        /// </summary>
        /// <param name="groupMembers"></param>
        /// <param name="reward"></param>
        /// <param name="influenceId"></param>
        private void DistributeGroupSharedRewards(List<Player> groupMembers, Reward reward, ushort influenceId, string description)
        {
            float shares = groupMembers.Count;

            foreach (var groupMember in groupMembers)
            {
                groupMember.SendClientMessage($"CONTRIB:Giving assist rewards to {groupMember.Name}");
                RewardLogger.Info($"Group Member {groupMember.Name} ({groupMember.CharacterId}) group assist reward");

                DistributeBaseRewardsForPlayerKill(reward, groupMember, (1f / shares), influenceId, description);

                groupMember.UpdatePlayerBountyEvent((byte)ContributionDefinitions.PARTY_KILL_ASSIST);
            }
        }

        private void DistributeInsigniaRewardForPlayerKill(Reward reward, Player killer, string victimName)
        {
            if (reward.InsigniaCount > 0)
            {
                var insigniaName = ItemService.GetItem_Info((uint)reward.InsigniaItemId).Name;
                killer.ItmInterface.CreateItem(ItemService.GetItem_Info((uint)reward.InsigniaItemId), (ushort)reward.InsigniaCount);
                killer.SendClientMessage($"You have been awarded {reward.InsigniaCount} {insigniaName} for killing {victimName}");
                RewardLogger.Debug($"{killer.Name} has been awarded {reward.InsigniaCount} {insigniaName} for killing {victimName}");
            }
        }

        private void DistributKillAssistRewardsForPlayerKill(Reward reward, Player killer, string victimName)
        {
            if (reward.InsigniaCount > 0)
            {
                // 15% chance of an assist giving a WC
                if (StaticRandom.Instance.Next(100) < 15)
                {
                    var insigniaName = ItemService.GetItem_Info((uint)reward.InsigniaItemId).Name;
                    killer.ItmInterface.CreateItem(ItemService.GetItem_Info((uint)reward.InsigniaItemId), (ushort)reward.InsigniaCount);
                    killer.SendClientMessage($"You have been awarded {reward.InsigniaCount} {insigniaName} for assisting in killing {victimName}");
                    RewardLogger.Debug($"{killer.Name} has been awarded {reward.InsigniaCount} {insigniaName} for assisting in killing {victimName}");
                }
            }


            if (killer.AAOBonus > 0)
            {
                killer.SendClientMessage($"CONTRIB:Giving assist rewards under AAO to {killer.Name}");
                // Add contribution for this kill under AAO to the killer.
                killer.UpdatePlayerBountyEvent((byte)ContributionDefinitions.PLAYER_KILL_ASSIST_UNDER_AAO);
            }

            // If the deathblow comes while the target is near a BO
            if (killer.CurrentObjectiveFlag != null)
            {
                killer.SendClientMessage($"CONTRIB:Giving PLAYER_KILL_ON_BO to {killer.Name}");
                killer.UpdatePlayerBountyEvent((byte)ContributionDefinitions.PLAYER_KILL_ON_BO);
            }
        }

        private void DistributeBaseRewardsForPlayerKill(Reward reward, Player player, float shareRewardScale, ushort influenceId, string description)
        {
            player.AddXp((uint)((uint)reward.BaseXP * shareRewardScale), true, true);
            player.AddRenown((uint)((uint)reward.BaseRP * shareRewardScale), true, RewardType.Kill, description);
            player.AddInfluence((ushort)influenceId, (ushort)((ushort)reward.BaseInf * shareRewardScale));
            player.AddMoney((uint)((uint)reward.BaseMoney * shareRewardScale));

            RewardLogger.Debug($"PlayerKill Base Reward Share Scale : {shareRewardScale} " +
                               $"XP : {reward.BaseXP}*{shareRewardScale}={reward.BaseXP * shareRewardScale} " +
                               $"RP : {reward.BaseRP}*{shareRewardScale}={reward.BaseRP * shareRewardScale} " +
                               $"INF : {reward.BaseInf}*{shareRewardScale}={reward.BaseInf * shareRewardScale} ");
        }

    }

    public class Reward
    {
        public int RenownBand { get; set; }
        public int InsigniaItemId { get; set; }
        public int BaseMoney { get; set; }
        public int InsigniaCount { get; set; }
        public int BaseRP { get; set; }
        public int BaseXP { get; set; }
        public int BaseInf { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return $"Reward {RenownBand}. {InsigniaCount}x{InsigniaItemId}, {BaseMoney}, {BaseRP}, {BaseXP}, {BaseInf} for {Description}";
        }

        public static int DetermineRenownBand(int playerRenownLevel)
        {
            // Add extra bounds. 
            if (playerRenownLevel == 0)
                return 10;
            if (playerRenownLevel >= 100)
                return 100;

            if (playerRenownLevel % 10 == 0) return playerRenownLevel;
            return (10 - playerRenownLevel % 10) + playerRenownLevel;
        }
    }

    
}
