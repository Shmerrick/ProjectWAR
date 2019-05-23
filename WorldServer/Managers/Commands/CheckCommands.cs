using System;
using System.Collections.Generic;
using System.Linq;
using static WorldServer.Managers.Commands.GMUtils;
using System.Text;
using Common.Database.World.Battlefront;
using GameData;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Battlefronts.Keeps;
using WorldServer.World.Objects;

namespace WorldServer.Managers.Commands
{
    /// <summary>Debugging commands under .check</summary>
    internal class CheckCommands
    {
        /// <summary>
        /// Check the child records for all keeps
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool CheckKeeps(Player plr, ref List<string> values)
        {
            foreach (var regionMgr in WorldMgr._Regions)
            {
                var campaign = regionMgr.Campaign;
                if (campaign == null)
                    continue;
                foreach (var battleFrontKeep in campaign?.Keeps)
                {
                    var result = $"Checking {battleFrontKeep.Info.Name} ({battleFrontKeep.Realm}/{battleFrontKeep.KeepStatus})";
                    var numberCreatures = battleFrontKeep.Creatures.Count;
                    var numberSpawnPoints = battleFrontKeep.SpawnPoints.Count;
                    var doors = battleFrontKeep.Doors.Count;
                    var lord = battleFrontKeep.KeepLord;

                    result += $"Keep Creatures:{numberCreatures}.";
                    result += $"Keep Spawn Points:{numberSpawnPoints}.";
                    result += $"Keep Doors:{doors}.";

                    if ((doors < 4) || (numberCreatures < 10) || (numberSpawnPoints < 4))
                    result += " ** WARNING **";

                    if (lord != null)
                    {
                        if (lord.Creature != null)
                        {
                            result += $"Keep Lord {lord.Creature.Name}";
                        }
                        else
                        {
                            result += $"Keep Lord NULL!";
                        }
                    }
                    else
                        result += $"Keep Lord NULL!";

                    plr.SendClientMessage(result);
                }
                
            }
            return true;
        }

        /// <summary>
        /// Check the child records for all keeps
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool CheckCaptain(Player plr, ref List<string> values)
        {
            var destructionRealmCaptain = plr.ActiveBattleFrontStatus.DestructionRealmCaptain;
            var orderRealmCaptain = plr.ActiveBattleFrontStatus.OrderRealmCaptain;

            if (destructionRealmCaptain != null)
                plr.SendClientMessage($"Destruction Captain {destructionRealmCaptain.Name}");
            else
            {
                plr.SendClientMessage($"No Destruction Captain");
            }
            if (orderRealmCaptain != null)
                plr.SendClientMessage($"Order Captain {orderRealmCaptain.Name}");
            else
            {
                plr.SendClientMessage($"No Order Captain");
            }

            return true;
        }
        
        /// <summary>
        /// Returns the closest keep and distance to Oil spawn pt
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool CheckOil(Player plr, ref List<string> values)
        {

            BattleFrontKeep keep = plr.Region.Campaign.GetClosestFriendlyKeep(plr.WorldPosition, plr.Realm);
            plr.SendClientMessage($"Closest Keep : {keep.Info.Name}");


            foreach (var h in keep.HardPoints)
            {
                if (!plr.PointWithinRadiusFeet(h, 10))
                    plr.SendClientMessage($"Player too far from hardpoint {h.ToString()}");
            }

            return true;
        }

        /// <summary>
        /// Check how many groups exist on the server.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool CheckGroups(Player plr, ref List<string> values)
        {
            plr.SendClientMessage(Group.WorldGroups.Count + " groups on the server:");

            lock (Group.WorldGroups)
            {
                foreach (Group group in Group.WorldGroups)
                {
                    Player ldr = group.Leader;

                    if (ldr == null)
                        plr.SendClientMessage("Leaderless group");
                    else plr.SendClientMessage("Group led by " + ldr.Name);
                }
            }

            return true;
        }

        /// <summary>
        /// Check how many objects exist in the current region.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool CheckObjects(Player plr, ref List<string> values)
        {
            plr.Region?.CountObjects(plr);

            return true;
        }

        public static bool GetPlayerContribution(Player plr, ref List<string> values)
        {
            var activeBattleFrontId = WorldMgr.UpperTierCampaignManager.ActiveBattleFront.BattleFrontId;
            var activeBattleFrontStatus = WorldMgr.UpperTierCampaignManager.GetBattleFrontStatus(activeBattleFrontId);

            var target = (Player)plr.CbtInterface.GetCurrentTarget();
            if (target != null)
            {
                var playerContribution = activeBattleFrontStatus.ContributionManagerInstance.GetContribution(target.CharacterId);

                if (playerContribution == null)
                {
                    plr.SendClientMessage("Player has no contribution");
                }
                else
                {
                    foreach (var contribution in playerContribution)
                    {
                        plr.SendClientMessage(contribution.ToString());
                    }
                    var stageDictionary = activeBattleFrontStatus.ContributionManagerInstance.GetContributionStageList(target.CharacterId);

                    var sumContribution = 0;

                    foreach (var contributionStage in stageDictionary)
                    {
                        sumContribution += contributionStage.Value.ContributionStageSum;
                        plr.SendClientMessage(contributionStage.ToString());
                    }

                    plr.SendClientMessage($"Total = {sumContribution}");
                }
            }
            return true;
        }

        public static bool GetBagBonus(Player plr, ref List<string> values)
        {
            var target = (Player)plr.CbtInterface.GetCurrentTarget();
            if (target != null)
            {
                var bagBonus = CharMgr.Database.SelectObject<RVRPlayerBagBonus>("CharacterId = " + target.CharacterId);

                if (bagBonus == null)
                {
                    plr.SendClientMessage("Player has no bag bonus");
                }
                else
                {
                    plr.SendClientMessage($"Gold {bagBonus.GoldBag}");
                    plr.SendClientMessage($"Purple {bagBonus.PurpleBag}");
                    plr.SendClientMessage($"Blue {bagBonus.BlueBag}");
                    plr.SendClientMessage($"Green {bagBonus.GreenBag}");
                    plr.SendClientMessage($"White {bagBonus.WhiteBag}");
                    plr.SendClientMessage($"Updated {bagBonus.Timestamp.ToShortDateString()}");
                }
            }
            return true;
        }

        public static bool GetBattleFrontContribution(Player plr, ref List<string> values)
        {
            var activeBattleFrontId = WorldMgr.UpperTierCampaignManager.ActiveBattleFront.BattleFrontId;
            var activeBattleFrontStatus = WorldMgr.UpperTierCampaignManager.GetBattleFrontStatus(activeBattleFrontId);

            var eligiblePlayers = activeBattleFrontStatus.ContributionManagerInstance.GetEligiblePlayers(0);

            if (eligiblePlayers.Count() == 0)
                plr.SendClientMessage("No eligible players");

            foreach (var eligiblePlayer in eligiblePlayers)
            {
                var player = Player._Players.SingleOrDefault(x => x.CharacterId == eligiblePlayer.Key);
                if (player != null)
                {
                    plr.SendClientMessage($"{player.Name}({eligiblePlayer.Key}):{eligiblePlayer.Value}");
                }
            }
            return true;
        }

        public static bool GetPlayerBounty(Player plr, ref List<string> values)
        {
            var activeBattleFrontId = WorldMgr.UpperTierCampaignManager.ActiveBattleFront.BattleFrontId;
            var activeBattleFrontStatus = WorldMgr.UpperTierCampaignManager.GetBattleFrontStatus(activeBattleFrontId);

            var target = (Player)plr.CbtInterface.GetCurrentTarget();
            if (target != null)
            {
                var playerBounty = plr.BountyManagerInstance.GetBounty(target.CharacterId);
                plr.SendClientMessage(playerBounty.ToString());
            }

            return true;
        }

        public static bool GetPlayerImpactMatrix(Player plr, ref List<string> values)
        {
            var activeBattleFrontId = WorldMgr.UpperTierCampaignManager.ActiveBattleFront.BattleFrontId;
            var activeBattleFrontStatus = WorldMgr.UpperTierCampaignManager.GetBattleFrontStatus(activeBattleFrontId);

            var target = (Player)plr.CbtInterface.GetCurrentTarget();
            if (target != null)
            {
                var killImpacts = plr.ImpactMatrixManager.GetKillImpacts(target.CharacterId);
                if ((killImpacts == null) || (killImpacts.Count == 0))
                {
                    plr.SendClientMessage($"{target.Name} has no impacts");
                }
                else
                {
                    foreach (var impact in killImpacts)
                    {
                        plr.SendClientMessage($"{target.Name} {impact.ToString()}");
                    }
                }
            }
            return true;
        }


        public static bool GetServerPopulation(Player plr, ref List<string> values)
        {
            lock (Player._Players)
            {
                plr.SendClientMessage($"Server Population ");
                plr.SendClientMessage($"Online players : {Player._Players.Count} ");
                plr.SendClientMessage($"Order : {Player._Players.Count(x => x.Realm == Realms.REALMS_REALM_ORDER && !x.IsDisposed && x.IsInWorld() && x != null)} ");
                plr.SendClientMessage($"Destro : {Player._Players.Count(x => x.Realm == Realms.REALMS_REALM_DESTRUCTION && !x.IsDisposed && x.IsInWorld() && x != null)}");

                plr.SendClientMessage("------------------------------------");
                var message = String.Empty;

                foreach (var regionMgr in WorldMgr._Regions)
                {
                    if (regionMgr.Players.Count > 0)
                    {
                        message += $"Region {regionMgr.RegionId} : " +
                                   $"Total : {Player._Players.Count(x => !x.IsDisposed && x.IsInWorld() && x != null && x.Region.RegionId == regionMgr.RegionId)} " +
                                   $"Order : {Player._Players.Count(x => x.Realm == Realms.REALMS_REALM_ORDER && !x.IsDisposed && x.IsInWorld() && x != null && x.Region.RegionId == regionMgr.RegionId)} " +
                                   $"Dest : {Player._Players.Count(x => x.Realm == Realms.REALMS_REALM_DESTRUCTION && !x.IsDisposed && x.IsInWorld() && x != null && x.Region.RegionId == regionMgr.RegionId)} ";
                    }
                }
                plr.SendClientMessage(message);
            }
            return true;
        }

        public static bool GetRewardEligibility(Player plr, ref List<string> values)
        {
            var activeBattleFrontId = WorldMgr.UpperTierCampaignManager.ActiveBattleFront.BattleFrontId;
            var activeBattleFrontStatus = WorldMgr.UpperTierCampaignManager.GetBattleFrontStatus(activeBattleFrontId);

            var players = activeBattleFrontStatus.ContributionManagerInstance.GetEligiblePlayers(0);

            plr.SendClientMessage($"Eligible players ({players.Count()}):");

            foreach (var player in players)
            {
                var playerObject = Player.GetPlayer(player.Key);

                if (playerObject.Realm == Realms.REALMS_REALM_DESTRUCTION)
                    plr.SendClientMessage($"{playerObject.Name} (D) Contrib:{player.Value}");
                else
                {
                    plr.SendClientMessage($"{playerObject.Name} (O) Contrib:{player.Value}");
                }
            }

            return true;
        }



        /// <summary>
        /// Finds all players currently in range.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool CheckPlayersInRange(Player plr, ref List<string> values)
        {
            StringBuilder str = new StringBuilder(256);
            int curOnLine = 0;

            lock (plr.PlayersInRange)
            {
                foreach (Player player in plr.PlayersInRange)
                {
                    if (curOnLine != 0)
                        str.Append(", ");
                    str.Append(player.Name);
                    str.Append(" (");
                    str.Append(player.Zone.Info.Name);
                    str.Append(")");

                    ++curOnLine;

                    if (curOnLine == 5)
                    {
                        plr.SendClientMessage(str.ToString());
                        curOnLine = 0;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Find the closest respawn point for the specified realm.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool FindClosestRespawn(Player plr, ref List<string> values)
        {
            byte realm = (byte)GetInt(ref values);

            plr.SendClientMessage("Closest respawn for " + (realm == 1 ? "Order" : "Destruction") + " is " +
                             WorldMgr.GetZoneRespawn(plr.Zone.ZoneId, realm, plr).ToString());

            return true;
        }

        /// <summary>
        /// Toggles logging outgoing packet volume.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool LogPackets(Player plr, ref List<string> values)
        {
            if (plr.Region == null)
                return false;

            plr.Region.TogglePacketLogging();

            plr.SendClientMessage(plr.Region.LogPacketVolume ? "Logging outgoing packet volume." : "No longer logging outgoing packet volume.");

            return true;
        }

        /// <summary>
        /// Displays the volume of outgoing packets over the defined period.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ReadPackets(Player plr, ref List<string> values)
        {
            plr.Region.SendPacketVolumeInfo(plr);

            return true;
        }

        /// <summary>
        /// Starts/Stops line of sight monitoring for selected target.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool StartStopLosMonitor(Player plr, ref List<string> values)
        {
            var target = plr.CbtInterface.GetCurrentTarget();

            if (target != null)
            {
                plr.EvtInterface.AddEvent(() =>
                {
                    if (plr.LOSHit(target))
                        plr.SendClientMessage("LOS=YES " + DateTime.Now.Second);
                    else
                        plr.SendClientMessage("LOS=NO" + DateTime.Now.Second);
                }, 1000, 30);

            }
            return true;
        }
    }
}
