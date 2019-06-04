using GameData;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using Common.Database.World.Battlefront;
using WorldServer.Managers;
using WorldServer.World.Battlefronts.Bounty;
using WorldServer.World.Objects;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public static class PlayerUtil
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static readonly float HONOR_REDUCTION_PERCENT = 0.998f;


        public static byte CalculateRenownBand(byte playerRenown)
        {
            return (byte)(Math.Round((playerRenown / 10.0)) * 10 + 10);
        }

        public static int GetTotalPVPPlayerCountInRegion(int regionId)
        {
            lock (Player._Players)
            {
                return Player._Players.Count(x => !x.IsDisposed && x.IsInWorld() && x != null && x.Region.RegionId == regionId && x.CbtInterface.IsPvp);
            }
        }

        public static int GetTotalDestPVPPlayerCountInRegion(int regionId)
        {
            lock (Player._Players)
            {
                return Player._Players.Count(x => x.Realm == Realms.REALMS_REALM_DESTRUCTION && !x.IsDisposed && x.IsInWorld() && x != null && x.Region.RegionId == regionId && x.CbtInterface.IsPvp);
            }
        }

        public static int GetTotalOrderPVPPlayerCountInRegion(int regionId)
        {
            lock (Player._Players)
            {
                return Player._Players.Count(x => x.Realm == Realms.REALMS_REALM_ORDER && !x.IsDisposed && x.IsInWorld() && x != null && x.Region.RegionId == regionId && x.CbtInterface.IsPvp);
            }
        }

        public static int GetTotalDestPVPPlayerCountInZone(int zoneID)
        {
            lock (Player._Players)
            {
                return Player._Players.Count(x => x.Realm == Realms.REALMS_REALM_DESTRUCTION && !x.IsDisposed && x.IsInWorld() && !x.IsAFK && !x.IsAutoAFK && x != null && x.ZoneId == zoneID && x.CbtInterface.IsPvp);
            }
        }

        public static int GetTotalOrderPVPPlayerCountInZone(int zoneID)
        {
            lock (Player._Players)
            {
                return Player._Players.Count(x => x.Realm == Realms.REALMS_REALM_ORDER && !x.IsDisposed && x.IsInWorld() && !x.IsAFK && !x.IsAutoAFK && x != null && x.ZoneId == zoneID && x.CbtInterface.IsPvp);
            }
        }


        public static List<Player> GetOrderPlayersInZone(int zoneId)
        {
            lock (Player._Players)
            {
                return Player._Players.Where(x => x.Realm == Realms.REALMS_REALM_ORDER && !x.IsDisposed && x.IsInWorld() && !x.IsAFK && !x.IsAutoAFK && x != null && x.ZoneId == zoneId && x.CbtInterface.IsPvp).ToList();
            }
        }

        public static List<Player> GetDestPlayersInZone(int zoneId)
        {
            lock (Player._Players)
            {
                return Player._Players.Where(x => x.Realm == Realms.REALMS_REALM_DESTRUCTION && !x.IsDisposed && x.IsInWorld() && !x.IsAFK && !x.IsAutoAFK && x != null && x.ZoneId == zoneId && x.CbtInterface.IsPvp).ToList();
            }
        }

        public static List<Player> GetAllFlaggedPlayersInZone(int zoneId)
        {
            lock (Player._Players)
            {
                return Player._Players.Where(x => !x.IsDisposed && x.IsInWorld() && !x.IsAFK && !x.IsAutoAFK && x != null && x.ZoneId == zoneId && x.CbtInterface.IsPvp).ToList();
            }
        }

        public static void UpdateHonorRankAllPlayers(bool announce)
        {
            var eligiblePlayers = CharMgr.Chars.Where(x => x.Value.HonorPoints >= 10);

            foreach (var player in eligiblePlayers)
            {
               
                try
                {
                    var currentHonorPoints = player.Value.HonorPoints;
                    // Reduce honor points by X%, unless they are < 10 - in which case make it 0
                    var newHonorPoints = 0;
                    if (currentHonorPoints < 10)
                        newHonorPoints = 0;
                    else
                        newHonorPoints = (int) (currentHonorPoints * HONOR_REDUCTION_PERCENT);

                    player.Value.HonorPoints = (ushort) newHonorPoints;
                    var oldHonorRank = player.Value.HonorRank;

                    // Recalculate Honor Rank
                    var honorLevel = new Common.HonorCalculation().GetHonorLevel((int) newHonorPoints);
                    player.Value.HonorRank = (ushort) honorLevel;
                    Logger.Trace(
                        $"Updating honor for {player.Value.Name} [{currentHonorPoints-newHonorPoints}] ({player.Value.CharacterId}) Current => New Honor Pts: {currentHonorPoints} => {player.Value.HonorPoints} ({player.Value.HonorRank}) ");
                    
                    CharMgr.Database.SaveObject(player.Value);

                    PlayerUtil.RecordHonorHistory(currentHonorPoints, (ushort) newHonorPoints, 
                        player.Value.CharacterId, player.Value.Name);

                    if (announce)
                    {
                        if (honorLevel > oldHonorRank)
                        {
                            var playerToAnnounce = Player.GetPlayer((uint) player.Value.CharacterId);
                            if (playerToAnnounce.CharacterId == player.Value.CharacterId)
                            {
                                playerToAnnounce.SendClientMessage($"You have reached Honor Rank {honorLevel}", ChatLogFilters.CHATLOGFILTERS_C_ORANGE_L);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error($"{e.Message} {e.StackTrace}");
                }
            }

        }

        /// <summary>
        /// Given contributing players and their contributions, split out the eligible, the contributing winning realm and contributing losing realm players.
        /// </summary>
        /// <param name="allContributingPlayers"></param>
        /// <param name="lockingRealm"></param>
        /// <param name="contributionManager"></param>
        /// <param name="updateHonor"></param>
        /// <param name="updateAnalytics"></param>
        /// <returns></returns>
        public static Tuple<ConcurrentDictionary<Player, int>, ConcurrentDictionary<Player, int>, ConcurrentDictionary<Player, int>>
            SegmentEligiblePlayers(
                IEnumerable<KeyValuePair<uint, int>> allContributingPlayers, Realms lockingRealm, IContributionManager contributionManager, bool updateHonor = true, bool updateAnalytics = true)
        {
            var winningRealmPlayers = new ConcurrentDictionary<Player, int>();
            var losingRealmPlayers = new ConcurrentDictionary<Player, int>();
            var allEligiblePlayerDictionary = new ConcurrentDictionary<Player, int>();


            // Partition the players by winning realm. 
            foreach (var contributingPlayer in allContributingPlayers)
            {
                var player = Player.GetPlayer(contributingPlayer.Key);
                if (player != null)
                {
                    if (updateHonor)
                    {
                        // Update the Honor Points of the Contributing Players
                        var oldHonorPoints = player.Info.HonorPoints;
                        player.Info.HonorPoints += (ushort)contributingPlayer.Value;
                        Logger.Debug($"Updating honor for {player.Info.Name} ({player.Info.CharacterId}) {oldHonorPoints} => {player.Info.HonorPoints} ({player.Info.HonorRank})");
                        CharMgr.Database.SaveObject(player.Info);


                        PlayerUtil.RecordHonorHistory(oldHonorPoints, player.Info.HonorPoints, player.CharacterId, player.Name);
                    }

                    if (player.Realm == lockingRealm)
                    {
                        winningRealmPlayers.TryAdd(player, contributingPlayer.Value);
                    }
                    else
                    {
                        losingRealmPlayers.TryAdd(player, contributingPlayer.Value);
                    }

                    allEligiblePlayerDictionary.TryAdd(player, contributingPlayer.Value);

                    if (updateAnalytics)
                    {
                        // Get the contribution list for this player
                        var contributionDictionary =
                            contributionManager.GetContributionStageDictionary(contributingPlayer.Key);
                        // Record the contribution types and values for the player for analytics
                        PlayerContributionManager.RecordContributionAnalytics(player, contributionDictionary);
                    }

                }
            }
            // Update and inform players of change in Honor Rank.
            UpdateHonorRankAllPlayers(false);

            return new Tuple<ConcurrentDictionary<Player, int>, ConcurrentDictionary<Player, int>, ConcurrentDictionary<Player, int>>(allEligiblePlayerDictionary, winningRealmPlayers, losingRealmPlayers);

        }

        private static void RecordHonorHistory(ushort oldHonorPoints, ushort infoHonorPoints, uint playerCharacterId, string playerName)
        {
            var roc = (infoHonorPoints - oldHonorPoints);
            
            var honorHistory = new HonorHistory
            {
                CharacterId = playerCharacterId,
                CharacterName = playerName,
                CurrentHonorPoints = (uint) infoHonorPoints,
                OldHonorPoints = oldHonorPoints,
                RateOfChange = roc,
                Timestamp = DateTime.UtcNow
            };
            WorldMgr.Database.AddObject(honorHistory);
        }

        public static void SendGMBroadcastMessage(List<Player> players, string message)
        {
            lock (players)
            {
                foreach (Player plr in players)
                {
                    if (plr.GmLevel > 1)
                        plr.SendClientMessage(message, ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                }
            }
        }
    }
}
