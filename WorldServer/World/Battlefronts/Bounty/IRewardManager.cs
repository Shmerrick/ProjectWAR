using Common;
using Common.Database.World.Battlefront;
using GameData;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WorldServer.World.Battlefronts.Apocalypse.Loot;
using WorldServer.World.Objects;

namespace WorldServer.World.Battlefronts.Bounty
{
    public interface IRewardManager
    {
        bool GetInsigniaRewards(float insigniaChance);

        ConcurrentDictionary<uint, Single> GetImpactFractionsForKill(Player victim,
            Dictionary<uint, Player> playerDictionary);

        void DistributePlayerKillRewards(Player victim, Player killer, float aaoBonus, ushort influenceId,
            Dictionary<uint, Player> playerDictionary);

        uint GetPlayerRVRDropCandidate(ConcurrentDictionary<uint, float> impactFractions,
            int forceSelectedInstance = -1);

        void RealmCaptainKill(Player victim, Player killer, ushort influenceId,
            Dictionary<uint, Player> playersByCharId);

        void PlayerKillPVPDrop(Player killer, Player victim);

        List<LootBagTypeDefinition> GenerateBagDropAssignments(ConcurrentDictionary<Player, int> realmPlayers,
            int forceNumberBags, string leadInZones, int additionalBags, int forceDropChance = 100);

        void GenerateKeepTakeLootBags(
            ILogger logger,
            ConcurrentDictionary<Player, int> allEligiblePlayers,
            ConcurrentDictionary<Player, int> winningEligiblePlayers,
            ConcurrentDictionary<Player, int> losingEligiblePlayers,
            Realms lockingRealm,
            int zoneId,
            List<RVRRewardItem> lootOptions,
            LootChest destructionLootChest,
            LootChest orderLootChest,
            Keep_Info keep, 
            int playersKilledInRange,
            int forceNumberBags = 0);

        void DistributeKeepTakeBaseRewards(ConcurrentDictionary<Player, int> eligibleLosingRealmPlayers,
            ConcurrentDictionary<Player, int> eligibleWinningRealmPlayers,
            Realms lockingRealm,
            int baselineContribution,
            float tierRewardScale,
            List<Player> allPlayersInZone,
            List<RVRKeepLockReward> rvrKeepRewards);

        void DistributeZoneFlipBaseRewards(ConcurrentDictionary<Player, int> eligibleLosingRealmPlayers,
            ConcurrentDictionary<Player, int> eligibleWinningRealmPlayers,
            Realms lockingRealm,
            int baselineContribution,
            float tierRewardScale,
            List<Player> allPlayersInZone);

        void MailItem(uint keyCharacterId, Item_Info itemToSend, int count, string senderName, string title = "",
            string content = "");
    }
}