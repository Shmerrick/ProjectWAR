using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using GameData;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using FrameWork;
using WorldServer.Managers;
using WorldServer.World.Battlefronts.Apocalypse.Loot;
using WorldServer.World.Battlefronts.Bounty;
using WorldServer.World.Objects;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public static class PlayerUtil
    {
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


        public static Tuple<ConcurrentDictionary<Player, int>, ConcurrentDictionary<Player, int>, ConcurrentDictionary<Player, int>> 
            SplitPlayerEligibility(
                IEnumerable<KeyValuePair<uint, int>> allContributingPlayers, Realms lockingRealm, ContributionManager contributionManager)
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
                        // Update the Honor Points of the Contributing Players
                        player.Info.HonorPoints += (ushort) contributingPlayer.Value;
                        CharMgr.Database.SaveObject(player.Info);

                        if (player.Realm == lockingRealm)
                            winningRealmPlayers.TryAdd(player, contributingPlayer.Value);
                        else
                        {
                            losingRealmPlayers.TryAdd(player, contributingPlayer.Value);
                        }

                        allEligiblePlayerDictionary.TryAdd(player, contributingPlayer.Value);

                        // Get the contribution list for this player
                        var contributionDictionary =
                            contributionManager.GetContributionStageDictionary(contributingPlayer.Key);
                        // Record the contribution types and values for the player for analytics
                        PlayerContributionManager.RecordContributionAnalytics(player, contributionDictionary);

                    }
                }

            }
        }
    }
}
