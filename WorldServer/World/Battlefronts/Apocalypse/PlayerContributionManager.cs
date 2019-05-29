using Common.Database.World.Battlefront;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WorldServer.Managers;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Bounty;
using WorldServer.World.Objects;
using PlayerContribution = WorldServer.World.Battlefronts.Bounty.PlayerContribution;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public static class PlayerContributionManager
    {
        public static void RecordContributionAnalytics(Player player, ConcurrentDictionary<short, ContributionStage> playerContributionList)
        {

            var analyticsRecord = new ContributionAnalytics
            {
                Timestamp = DateTime.Now,
                CharacterId = player.CharacterId,
                RenownRank = player.RenownRank,
                Name = player.Name,
                Class = player.Info.CareerLine,
                AccountId = player.Info.AccountId,
                ZoneId = (int)player.ZoneId.GetValueOrDefault(),
            };

            var totalValue = 0;
            foreach (var playerContribution in playerContributionList)
            {
                var contributionDetails = new ContributionAnalyticsDetails
                {
                    CharacterId = player.CharacterId,
                    Timestamp = DateTime.UtcNow,
                    ContributionId = playerContribution.Key,
                    ContributionSum = playerContribution.Value.ContributionStageSum
                };
                totalValue += playerContribution.Value.ContributionStageSum;

                WorldMgr.Database.AddObject(contributionDetails);
            }

            analyticsRecord.ContributionValue = totalValue;
            WorldMgr.Database.AddObject(analyticsRecord);
        }

        public static void SavePlayerContribution(int battleFrontId, ContributionManager contributionManagerInstance)
        {

            WorldMgr.Database.ExecuteNonQuery($"DELETE FROM rvr_player_contribution Where BattleFrontId={battleFrontId};");

            foreach (var contribution in contributionManagerInstance.ContributionDictionary)
            {
                var characterId = contribution.Key;
                var contributionSerialised = JsonConvert.SerializeObject(contribution.Value);
                var recordToWrite = new Common.Database.World.Battlefront.PlayerContribution
                {
                    CharacterId = characterId,
                    BattleFrontId = battleFrontId,
                    ContributionSerialised = contributionSerialised,
                    Timestamp = DateTime.Now
                };

                WorldMgr.Database.AddObject(recordToWrite);
            }

        }

        public static ContributionManager LoadPlayerContribution(int battleFrontId)
        {
            var contributionDictionary = new ConcurrentDictionary<uint, List<PlayerContribution>>();
            var contributions =
                WorldMgr.Database.SelectObjects<Common.Database.World.Battlefront.PlayerContribution>(
                    $"BattleFrontId = {battleFrontId.ToString()}");

            foreach (var contribution in contributions)
            {
                contributionDictionary.TryAdd(contribution.CharacterId,
                    JsonConvert.DeserializeObject<List<PlayerContribution>>(contribution.ContributionSerialised));
            }

            return new ContributionManager(
                contributionDictionary,
                BountyService._ContributionDefinitions);
        }
    }
}
