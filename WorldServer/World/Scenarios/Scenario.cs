using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SystemData;
using Common;
using Common.Database.World.Battlefront;
using FrameWork;
using GameData;
using NLog;
using WorldServer.Scenarios.Objects;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Apocalypse;

namespace WorldServer.Scenarios
{
    public class ScenarioScoreboard
    {
        public Player MyPlayer;
        public uint SoloKills;
        public uint Kills;
        public uint DeathBlows;
        public uint Deaths;
        public uint Damage;
        public uint GuardDamage;
        public uint Healing;
        public uint Renown, EndRenown;
        public uint Xp, EndXP;

        public ScenarioScoreboard(Player plr)
        {
            MyPlayer = plr;
            MyPlayer.EvtInterface.AddEventNotify(EventName.OnDie, OnDie);
            MyPlayer.EvtInterface.AddEventNotify(EventName.OnDealDamage, OnDealDamage);
            MyPlayer.EvtInterface.AddEventNotify(EventName.OnDealHeal, OnDealHeal);
            MyPlayer.EvtInterface.AddEventNotify(EventName.OnKill, OnKill);
            MyPlayer.EvtInterface.AddEventNotify(EventName.OnAddRenown, OnAddRenown);
            MyPlayer.EvtInterface.AddEventNotify(EventName.OnAddXP, OnAddXp);
        }

        public void SendScore(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_SCENARIO_PLAYER_INFO, 48);
            Out.WriteByte(0);
            Out.WriteUInt16(0);
            Out.WriteByte(1);
            Out.WriteUInt32(MyPlayer.CharacterId);
            Out.WriteByte((byte)MyPlayer.Realm);
            Out.WriteByte(MyPlayer.Level);
            Out.WriteUInt16(0);
            Out.WriteByte(MyPlayer.Info.Career);
            Out.WritePascalString(MyPlayer.GenderedName);
            plr.SendPacket(Out);
        }

        public bool OnDie(Object obj, object args)
        {
            Deaths++;
            return false;
        }

        public bool OnDealDamage(Object obj, object args)
        {
            Damage += (uint)args;
            return false;
        }

        public bool OnDealHeal(Object obj, object args)
        {
            Healing += (uint)args;
            return false;
        }

        public bool OnKill(Object obj, object args)
        {
            Kills++;

            if (obj == MyPlayer)
                DeathBlows++;

            if (MyPlayer.ScenarioGroup == null)
                SoloKills++;

            return false;
        }

        public bool OnAddRenown(Object obj, object args)
        {
            Renown += (uint)args;
            return false;
        }

        public bool OnAddXp(Object obj, object args)
        {
            Xp += (uint)args;
            return false;
        }

        public void ClearEventNotifies()
        {
            MyPlayer.EvtInterface.RemoveEventNotify(EventName.OnDie, OnDie);
            MyPlayer.EvtInterface.RemoveEventNotify(EventName.OnDealDamage, OnDealDamage);
            MyPlayer.EvtInterface.RemoveEventNotify(EventName.OnDealHeal, OnDealHeal);
            MyPlayer.EvtInterface.RemoveEventNotify(EventName.OnKill, OnKill);
            MyPlayer.EvtInterface.RemoveEventNotify(EventName.OnAddRenown, OnAddRenown);
            MyPlayer.EvtInterface.RemoveEventNotify(EventName.OnAddXP, OnAddXp);
        }
    }

    public enum EScenarioQueueAction
    {
        // Entry from ScenarioMgr
        AddPendingPlayer,
        AddPendingGroup,
        // Pop acceptance or decline from client
        AddPlayer,
        RemovePendingPlayer,
        // Removal of existing player from scenario
        RemovePlayer,
        NotifyPlayerLeft
    }

    public class ScenarioQueueAction
    {
        public EScenarioQueueAction Action;
        public object MyObject;

        public ScenarioQueueAction(EScenarioQueueAction action, object obj)
        {
            Action = action;
            MyObject = obj;
        }
    }

    public abstract class Scenario
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly uint[] _emblemIds = { 208470, 208470, 208470, 208470 };
        public Scenario_Info Info { get; }

        public int Tier;
        protected EventInterface EvtInterface;
        protected RegionMgr Region;

        protected Point3D[] RespawnLocations = new Point3D[2];
        protected ushort[] RespawnHeadings = new ushort[2];

        protected Scenario(Scenario_Info info, int tier)
        {
            EvtInterface = new EventInterface();
            EvtInterface.AddEvent(SendScoreboardUpdate, 10000, 0);
            EvtInterface.AddEvent(CheckQueue, 1000, 0);
            EvtInterface.AddEvent(UpdatePreBegin, 1000, 0);

            StartTime = TCPManager.GetTimeStampMS();

            Info = info;
            Tier = tier;

            for (byte i = 0; i < 2; ++i)
            {
                Zone_Respawn respawn = WorldMgr.GetZoneRespawn(Info.MapId, (byte)(i + 1), null);
                if (respawn == null)
                    throw new Exception("Scenario " + Info.Name + " is missing a respawn!");
                RespawnLocations[i] = ZoneService.GetWorldPosition(ZoneService.GetZone_Info(Info.MapId), respawn.PinX, respawn.PinY, respawn.PinZ);
                RespawnHeadings[i] = respawn.WorldO;
            }

            Region = new RegionMgr(Info.MapId, ZoneService.GetZoneRegion(Info.MapId), info.Name, new ApocCommunications()) { Scenario = this };

            /* create groups */
            for (int i = 0; i < 2; ++i)
            {
                _scGroupsHandlers[i] = new ScenarioGroupsHandler();

                _pendingPlayers[i] = new Dictionary<Player, long>();
                Players[i] = new List<Player>();
            }
        }

        public virtual void Interact(GameObject obj, Player plr, InteractMenu menu)
        {
        }

        #region Progression

        private byte _countdownStage;

        public long StartTime { get; protected set; }
        public long EndTime { get; protected set; }
        public bool HasStarted { get; protected set; }
        public bool HasEnded { get; protected set; }
        public bool IsClosed { get; protected set; }

        private bool ShouldEnd => (TCPManager.GetTimeStampMS() - StartTime) / 1000 > 900;

        /// <summary>
        /// Displays countdown messages to clients, and finally starts the game.
        /// </summary>
        public void EventCountdown()
        {
            switch (_countdownStage)
            {
                case 1:
                    ++_countdownStage;
                    StartTime = TCPManager.GetTimeStampMS();
                    SendToAll(WorldMgr.ScenarioMgr.BuildScenarioInfo(this));
                    SendChatMessage("One minute until the battle for " + Info.Name + " begins!");
                    break;
                case 2:
                    ++_countdownStage;
                    SendChatMessage("Thirty seconds until the battle for " + Info.Name + " begins!");
                    break;
                case 3:
                    SendChatMessage("The battle for " + Info.Name + " has begun!");
                    StartTime = TCPManager.GetTimeStampMS();
                    EvtInterface.RemoveEvent(EventCountdown);
                    Start();
                    SendToAll(WorldMgr.ScenarioMgr.BuildScenarioInfo(this));
                    break;
            }
        }

        public void Start()
        {
            HasStarted = true;
            EvtInterface.RemoveEvent(UpdatePreBegin);
            EvtInterface.AddEvent(UpdateScenario, 1000, 0);
            EvtInterface.AddEvent(BeginScoreUp, 60000 * 14, 1);
            EvtInterface.AddEvent(CheckPopulation, 60000, 0);

            if (MapTrackedObjects.Count > 0)
            {
                for (int i = 0; i < 2; ++i)
                {
                    foreach (Player player in Players[i])
                        foreach (HoldObject flag in MapTrackedObjects)
                            SendFlagObjectState(player, flag);
                }
            }
            OnStart();
        }

        /// <summary>
        /// Called every second if the scenario has not yet begun.
        /// </summary>
        protected virtual void UpdatePreBegin()
        {
            for (int i = 0; i < 2; i++)
            {
                foreach (Player plr in Players[i])
                {
                    if (plr == null || plr.IsDisposed || !plr.IsInWorld())
                        continue;

                    // Teleport player back if hes tried to run
                    int realmIndex = (int)plr.Realm - 1;

                    if (plr.Region == Region && plr.Get2DDistanceToWorldPoint(RespawnLocations[realmIndex]) > 60)
                        plr.IntraRegionTeleport((uint)RespawnLocations[realmIndex].X,
                            (uint)RespawnLocations[realmIndex].Y, (ushort)RespawnLocations[realmIndex].Z,
                            RespawnHeadings[realmIndex]);
                }
            }
        }

        /// <summary>
        /// Updates the EventInterface and the scenario warbands.
        /// </summary>
        public void Update(long tick)
        {
            if (_scenarioQueueActions.Count > 0)
                ProcessScenarioQueueActions();

            EvtInterface?.Update(tick);

            lock (_scGroupsHandlers[0])
            {
                _scGroupsHandlers[0].Update(tick);
            }
            lock (_scGroupsHandlers[1])
            {
                _scGroupsHandlers[1].Update(tick);
            }
        }

        /// <summary>
        /// Called every second if the scenario has begun but has not yet ended.
        /// </summary>
        protected virtual void UpdateScenario()
        {
            if (_shutdownLevel > 1 || (ShouldEnd && HasStarted && !HasEnded))
                End();

            else if (_dominatingRealm > 0)
                GivePoints((int)_dominatingRealm, 5);
        }

        public int GetRespawnDelay()
        {
            if (HasEnded || !HasStarted)
                return 5;

            return Math.Max(2, (int)(30 - ((TCPManager.GetTimeStampMS() - StartTime) * 0.001f) % 30));
        }

        /// <summary>
        /// Initiates the scenario's closing, dissolves the warbands and hands out any rewards.
        /// </summary>
        public void End()
        {
            if (HasEnded)
            {
                Log.Error(Info.Name, "Multiple calls to End()!");
                return;
            }

            HasEnded = true;
            EndTime = TCPManager.GetTimeStampMS();

            uint maxScore = Math.Max(Score[0], Score[1]);

            if (maxScore > 0)
                CharMgr.Database.AddObject(new ScenarioDurationRecord { ScenarioId = Info.ScenarioId, Tier = (byte)Tier, DurationSeconds = (uint)((EndTime - StartTime) * 0.001f), StartTime = StartTime });

            SendToAll(WorldMgr.ScenarioMgr.BuildScenarioInfo(this));

            EvtInterface.RemoveEvent(UpdateScenario);
            EvtInterface.RemoveEvent(BeginScoreUp);
            EvtInterface.RemoveEvent(ScoreUp);
            EvtInterface.RemoveEvent(CheckPopulation);

            EvtInterface.AddEvent(Close, 120000, 1);

            for (int i = 0; i < 2; ++i)
                _scGroupsHandlers[i].NotifyScenarioClosed();

            Rewards();

            foreach (HoldObject flag in MapTrackedObjects)
            {
                flag.ResetTo(EHeldState.Inactive);
            }
        }

        /// <summary>
        /// Kicks any remaining players out of the scenario and stops the region.
        /// </summary>
        public void Close()
        {
            for (int i = 0; i < 2; ++i)
            {
                while (Players[i].Count > 0)
                    RemovePlayer(Players[i].First(), true);

                while (_pendingPlayers[i].Count > 0)
                    RemovePendingPlayer(_pendingPlayers[i].First().Key);

                // Send any other player in the region to the start point
                foreach (Player player in Region.Players)
                {
                    if (!Players[0].Contains(player) && !Players[1].Contains(player))
                    {
                        CharacterInfo info = CharMgr.GetCharacterInfo(player.Info.Career);
                        TeleportInOut(player, null, info.ZoneId, (uint)info.WorldX, (uint)info.WorldY, (ushort)info.WorldZ, (ushort)info.WorldO);
                    }
                }
            }

            OnClose();

            EvtInterface.AddEvent(StopRegion, 20000, 1);
        }

        public void StopRegion()
        {
            Region.Stop();
            IsClosed = true;
        }

        #endregion

        #region Scoring and Rewards

        public uint[] Score { get; } = new uint[2];
        private readonly int[] _realmScoreProgress = new int[2];
        public Dictionary<Player, ScenarioScoreboard> PlayerScoreboard = new Dictionary<Player, ScenarioScoreboard>();

        public virtual bool OnPlayerKilled(Object pkilled, object instigator)
        {
            Player killer = instigator as Player;

            if (killer == null)
                return false;

            ++_totalKills[(int)killer.Realm - 1];

            if (Info.KillPointScore == 0)
                return false;

            Player killed = (Player)pkilled;

            if (killer.Realm != killed.Realm)
            {
                byte pointValue = Info.KillPointScore;

                if (MapTrackedObjects.Count > 0)
                    foreach (HoldObject obj in MapTrackedObjects)
                        if (obj.Holder == killer || obj.Holder == killed)
                            pointValue *= 2;

                GivePoints(killer.Realm == Realms.REALMS_REALM_DESTRUCTION ? 2 : 1, pointValue, true);
            }

            if (Info.DeferKills)
                CheckDomination(killer.Realm, killed);

            return false;
        }

        #region Delayed Reward

        /*
        protected internal class DeferredReward
        {
            public uint XP;
            public uint Renown;

            public DeferredReward(uint xp, uint renown)
            {
                XP = xp;
                Renown = renown;
            }
        }
        
        private readonly Dictionary<Player, DeferredReward> _deferredReward = new Dictionary<Player, DeferredReward>(); */

        public virtual bool DeferKillReward(Player killer, uint xp, uint renown)
        {
            return Info.DeferKills && !HasEnded && killer.Region == Region;
        }

        #endregion

        private int _lootIssued;

        public void GivePoints(int team, uint points, bool fromKill = false)
        {
            if (HasEnded)
                return;

            int teamIndex = team - 1;

            Score[teamIndex] += points;

            if (Score[teamIndex] > 500)
                Score[teamIndex] = 500;

            PacketOut Out = new PacketOut((byte)Opcodes.F_SCENARIO_POINT_UPDATE, 8);
            Out.WriteByte((byte)team);
            Out.WriteByte(0);
            Out.WriteUInt16((ushort)Score[teamIndex]);
            Out.Fill(0, 4);
            SendToAll(Out);

            if (Info.DeferKills)
            {
                // Check for spawncamping.
                if (!fromKill)
                    CheckDomination((Realms)team, null);

                // If scenario uses deferred mechanics, loot is issued based upon points scored.
                _realmScoreProgress[teamIndex] += (int)points;

                if (_realmScoreProgress[teamIndex] >= 150)
                {
                    _realmScoreProgress[teamIndex] -= 150;
                    ++_lootIssued;

                    if (_lootIssued > 5)
                        Log.Error("Scenario", "Attempted to drop too much loot. Times: " + _lootIssued);
                    else
                    {
                        ZoneMgr curZone = Region.GetZoneMgr(Info.MapId);

                        foreach (Player plr in Players[teamIndex])
                        {
                            LootContainer ctr = LootsMgr.GetScenarioLoot(plr, Tier, curZone);

                            if (ctr == null)
                                continue;

                            plr.PriorityGroup?.GroupLoot(plr, ctr);
                            ctr.TakeAll(plr, true);
                        }
                    }
                }
            }

            if (Score[team - 1] >= 500)
                End();
        }

        private int _rampFactor = 4;

        public void BeginScoreUp()
        {
            ScoreUp();
            EvtInterface.AddEvent(ScoreUp, 15000, 3);
        }

        /// <summary>
        /// Tends the winning team's score towards 500 over the last minute of the scenario.
        /// </summary>
        public void ScoreUp()
        {
            int winningTeam, losingTeam;

            if (Score[0] > Score[1])
            {
                winningTeam = 0;
                losingTeam = 1;
            }

            else if (Score[1] > Score[0])
            {
                winningTeam = 1;
                losingTeam = 0;
            }

            else
            {
                winningTeam = StaticRandom.Instance.Next(0, 1);
                losingTeam = 1 - winningTeam;
            }

            if (Score[winningTeam] == 0)
            {
                GivePoints(losingTeam + 1, (uint)(500 * (1f / _rampFactor)));
                GivePoints(winningTeam + 1, (uint)(500 * (1f / _rampFactor)));
            }

            else
            {
                float scaleFactor = (500 - Score[winningTeam]) * (1f / _rampFactor) / Score[winningTeam];

                GivePoints(losingTeam + 1, (uint)(Score[losingTeam] * scaleFactor));
                GivePoints(winningTeam + 1, (uint)(Score[winningTeam] * scaleFactor));
            }

            --_rampFactor;
        }

        private bool _rewardsDealt;

        public void Rewards()
        {
            if (_rewardsDealt)
            {
                Log.Error("Scenario", "Multiple calls to Rewards()");
                return;
            }

            var contributionDefinition = new ContributionDefinition();

            _rewardsDealt = true;

            uint[] endingXp = new uint[2];
            uint[] endingRenown = new uint[2];
            byte[] emblemCount = new byte[2];

            for (int i = 0; i < 2; ++i)
            {
                endingXp[i] = (uint)(Score[i] * 10 * Tier * Info.RewardScaler);
                endingRenown[i] = (uint)(Score[i] * 0.25f * Tier * Info.RewardScaler);
                emblemCount[i] = 3;
            }

            byte winningTeam = 2;

            if (Score[0] != Score[1])
            {
                winningTeam = (Score[0] > Score[1] ? (byte)0 : (byte)1);

                endingRenown[winningTeam] *= 2;
                ++emblemCount[winningTeam];

                if (Score[winningTeam] == 500)
                    endingRenown[winningTeam] += 100;
            }

            if (Info.DeferKills)
                AddEstimatedKillRewards();

            Item_Info desiredItem = ItemService.GetItem_Info(_emblemIds[Tier - 1]);

            // Winner rewards for stomping are halved
            if (_dominatingRealm != Realms.REALMS_REALM_NEUTRAL)
            {

                endingXp[(int)_dominatingRealm - 1] /= 2;
                endingRenown[(int)_dominatingRealm - 1] /= 2;
                emblemCount[(int)_dominatingRealm - 1] /= 2;
            }

            foreach (Player plr in PlayerScoreboard.Keys)
            {
                if (plr == null)
                    continue;

                byte realmIndex = (byte)(plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 1 : 0);

                plr.AddXp(endingXp[realmIndex], false, false);
                plr.AddRenown(endingRenown[realmIndex], false, RewardType.ScenarioWin);

                PlayerScoreboard[plr].EndXP = endingXp[realmIndex];
                PlayerScoreboard[plr].EndRenown = endingRenown[realmIndex];

                // Add Contribution
                WorldMgr.UpperTierCampaignManager.GetActiveCampaign().GetActiveBattleFrontStatus().ContributionManagerInstance.UpdateContribution(plr.CharacterId, (byte)ContributionDefinitions.PLAY_SCENARIO);
                contributionDefinition = BountyService.GetDefinition((byte)ContributionDefinitions.PLAY_SCENARIO);
                WorldMgr.UpperTierCampaignManager.GetActiveCampaign().GetActiveBattleFrontStatus().BountyManagerInstance.AddCharacterBounty(plr.CharacterId, contributionDefinition.ContributionValue);

                if (realmIndex == winningTeam)
                {
                    // Lower reward for domination
                    if (_dominatingRealm != Realms.REALMS_REALM_NEUTRAL)
                    {
                        plr.ItmInterface.CreateItem(desiredItem, 3);
                        plr.SendLocalizeString(new[] { desiredItem.Name, "3" }, ChatLogFilters.CHATLOGFILTERS_LOOT,
                            Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
                    }
                    else
                    {
                        plr.ItmInterface.CreateItem(desiredItem, 6);
                        plr.SendLocalizeString(new[] { desiredItem.Name, "6" }, ChatLogFilters.CHATLOGFILTERS_LOOT,
                            Localized_text.TEXT_YOU_RECEIVE_ITEM_X);

                        // Add Contribution
                        WorldMgr.UpperTierCampaignManager.GetActiveCampaign().GetActiveBattleFrontStatus().ContributionManagerInstance.UpdateContribution(plr.CharacterId, (byte)ContributionDefinitions.WIN_SCENARIO);
                        contributionDefinition = BountyService.GetDefinition((byte)ContributionDefinitions.WIN_SCENARIO);
                        WorldMgr.UpperTierCampaignManager.GetActiveCampaign().GetActiveBattleFrontStatus().BountyManagerInstance.AddCharacterBounty(plr.CharacterId, contributionDefinition.ContributionValue);

                    }

                    plr.QtsInterface.HandleEvent(Objective_Type.QUEST_WIN_SCENARIO, Info.ScenarioId, 1);
                }
                else
                {
                    plr.ItmInterface.CreateItem(desiredItem, 4);
                    plr.SendLocalizeString(new[] { desiredItem.Name, "4" }, ChatLogFilters.CHATLOGFILTERS_LOOT,
                        Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
                }

            }

            try
            {
                // Destro win
                if (winningTeam == 1)
                {
                    _logger.Debug($"Scenario {Info.Name} won by Destruction. {Score[1]} to {Score[0]}");
                    _logger.Debug($"Suggest {Score[1] / 10} additional VP to winner,  {Score[0] / 20} to loser.");
                    new ApocCommunications().Broadcast("Destruction has defeated Order in a critical battle! Their forces come closer to victory.", Tier);
                    WorldMgr.UpperTierCampaignManager.GetActiveCampaign().VictoryPointProgress.DestructionVictoryPoints += (Score[1] / 10);
                    WorldMgr.UpperTierCampaignManager.GetActiveCampaign().VictoryPointProgress.OrderVictoryPoints += (Score[0] / 20);
                }
                if (winningTeam == 0)
                {
                    _logger.Debug($"Scenario {Info.Name} won by Order. {Score[0]} to {Score[1]}");
                    _logger.Debug($"Suggest {Score[0] / 10} additional VP to winner,  {Score[1] / 20} to loser.");
                    new ApocCommunications().Broadcast("Order has defeated Destruction in a critical battle! Their forces come closer to victory.", Tier);
                    WorldMgr.UpperTierCampaignManager.GetActiveCampaign().VictoryPointProgress.OrderVictoryPoints += (Score[1] / 10);
                    WorldMgr.UpperTierCampaignManager.GetActiveCampaign().VictoryPointProgress.DestructionVictoryPoints += (Score[0] / 20);

                }

            }
            catch (Exception e)
            {
                _logger.Error($"Scenario reporting exception {e.Message}");
            }



            SendScoreboardUpdate();
        }

        private void AddEstimatedKillRewards()
        {
            // For all players on each team:
            // Determine appropriate renown count from killing each member of the enemy team twice.
            // Multiply by time and scoring factor.
            foreach (Player player in Players[0])
            {
                float pointScaleFactor = (float)Score[0] / 500;

                /*
                // No kills, no rewards
                if (PlayerScoreboard[player].Kills == 0 || _totalKills[0] == 0)
                    continue;

                // If this player has too few kill contributions,
                if (_totalKills[0]/PlayerScoreboard[player].Kills > 4)
                    pointScaleFactor *= PlayerScoreboard[player].Kills/(float) _totalKills[0];
                */

                uint curXp = 0;
                uint curRenown = 0;

                foreach (Player foe in Players[1])
                {
                    curXp += WorldMgr.GenerateXPCount(player, foe);
                    curRenown += WorldMgr.GenerateRenownCount(player, foe);
                }

                curXp = (uint)(curXp * 2f * 0.16f * pointScaleFactor * Info.RewardScaler);
                curRenown = (uint)(curRenown * 2f * 0.16f * pointScaleFactor * Info.RewardScaler);

                player.SendClientMessage($"Adding {curXp} XP from estimated kills.", ChatLogFilters.CHATLOGFILTERS_EXP);
                player.SendClientMessage($"Adding {curRenown} renown from estimated kills.", ChatLogFilters.CHATLOGFILTERS_RENOWN);

                player.AddXp(curXp, false, true);
                player.AddRenown(curRenown, false, RewardType.ScenarioWin);
            }

            foreach (Player player in Players[1])
            {
                float pointScaleFactor = (float)Score[1] / 500;

                /*
                // No kills, no rewards
                if (PlayerScoreboard[player].Kills == 0 || _totalKills[1] == 0)
                    continue;

                // If this player has too few kill contributions, reduce rewards
                if (_totalKills[1] / PlayerScoreboard[player].Kills > 4)
                    pointScaleFactor *= PlayerScoreboard[player].Kills / (float)_totalKills[1];
                */

                uint curXp = 0;
                uint curRenown = 0;

                foreach (Player foe in Players[0])
                {
                    curXp += WorldMgr.GenerateXPCount(player, foe);
                    curRenown += WorldMgr.GenerateRenownCount(player, foe);
                }

                curXp = (uint)(curXp * 2f * 0.16f * pointScaleFactor * Info.RewardScaler);
                curRenown = (uint)(curRenown * 2f * 0.16f * pointScaleFactor * Info.RewardScaler);

                player.SendClientMessage($"Adding {curXp} XP from estimated kills.", ChatLogFilters.CHATLOGFILTERS_EXP);
                player.SendClientMessage($"Adding {curRenown} renown from estimated kills.", ChatLogFilters.CHATLOGFILTERS_RENOWN);

                player.AddXp(curXp, false, true);
                player.AddRenown(curRenown, false, RewardType.ScenarioWin);
            }
        }

        public bool PreventKillReward()
        {
            return Info.DeferKills && !HasEnded;
        }

        public bool SoloBlock(Player killer, bool announce = true)
        {
            if (killer.ScenarioGroup == null || (killer.ScenarioGroup.Members.Count == 1 && PartialGroupExistsFor(killer)))
            {
                if (announce)
                    killer.SendClientMessage("Because you are choosing to play solo, you received no XP, Renown or drops from this player.");
                return true;
            }

            return false;
        }

        #endregion

        #region Queue and Player Management

        public readonly List<Player>[] Players = new List<Player>[2];
        private readonly Dictionary<Player, long>[] _pendingPlayers = new Dictionary<Player, long>[2];

        #region Queue Processing

        public void CheckQueue()
        {
            long tick = TCPManager.GetTimeStampMS();
            for (int i = 0; i < 2; ++i)
            {
                List<Player> toRemove = new List<Player>();
                foreach (Player plr in _pendingPlayers[i].Keys)
                {
                    if (tick - _pendingPlayers[i][plr] > 60000)
                    {
                        toRemove.Add(plr);
                    }
                }

                foreach (var player in _pendingPlayers[i].Keys.Intersect(toRemove).ToList())
                {
                    _pendingPlayers[i].Remove(player);
                    player.ScnInterface.ClearPendingScenario();
                    ScenarioMgr.SendScenarioStatus(player, ScenarioUpdateType.Leave, Info);
                    if (!HasEnded && player.DisconnectType == Player.EDisconnectType.Unclean)
                    {
                        ScenarioMgr.UpdateQuitter(player);
                        player.SendClientMessage("Your invitation to " + Info.Name + " has elapsed, and you have been given the Quitter! debuff.");
                    }
                }
            }
        }

        private readonly List<ScenarioQueueAction> _scenarioQueueActions = new List<ScenarioQueueAction>();
        private readonly List<ScenarioQueueAction> _currentActionList = new List<ScenarioQueueAction>();

        public void EnqueueScenarioAction(ScenarioQueueAction action)
        {
            lock (_scenarioQueueActions)
                _scenarioQueueActions.Add(action);
        }

        private void ProcessScenarioQueueActions()
        {
            lock (_scenarioQueueActions)
            {
                _currentActionList.AddRange(_scenarioQueueActions);
                _scenarioQueueActions.Clear();
            }

            foreach (ScenarioQueueAction action in _currentActionList)
            {
                switch (action.Action)
                {
                    case EScenarioQueueAction.AddPendingPlayer:
                        AddPendingPlayer((Player)action.MyObject);
                        break;
                    case EScenarioQueueAction.AddPendingGroup:
                        AddPendingGroup((List<Player>)action.MyObject);
                        break;
                    case EScenarioQueueAction.AddPlayer:
                        AddPlayer((Player)action.MyObject);
                        break;
                    case EScenarioQueueAction.RemovePendingPlayer:
                        RemovePendingPlayer((Player)action.MyObject);
                        break;
                    case EScenarioQueueAction.RemovePlayer:
                        RemovePlayer((Player)action.MyObject, true);
                        break;
                    case EScenarioQueueAction.NotifyPlayerLeft:
                        RemovePlayer((Player)action.MyObject, false);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            _currentActionList.Clear();
        }

        #endregion

        #region Player Entry / Exit
        public virtual void GmCommand(Player plr, ref List<string> values)
        {
        }
        private readonly ScenarioGroupsHandler[] _scGroupsHandlers = new ScenarioGroupsHandler[2];
        public Vector2[] BalanceVectors { get; } = { new Vector2(), new Vector2() };
        public bool IsPickup;

        private readonly int[] _activePlayers = new int[2];

        public void AddPendingPlayer(Player plr)
        {
            byte team = (byte)(plr.Realm - 1);

            if (_pendingPlayers[team].Keys.Contains(plr)) // give me more time
                _pendingPlayers[team][plr] = TCPManager.GetTimeStampMS();
            else
            {
                _pendingPlayers[team].Add(plr, TCPManager.GetTimeStampMS());
                plr.ScnInterface.PendingScenario = this;

                // Notify scenario available
                ScenarioMgr.SendScenarioStatus(plr, ScenarioUpdateType.Pop, Info);

                ScenarioMgr.AddToBalanceVector(BalanceVectors[(byte)plr.Realm - 1], plr);
            }
        }

        public void AddPendingGroup(List<Player> players)
        {
            foreach (Player player in players)
                AddPendingPlayer(player);
        }

        public void IncrementPlayers(Player player)
        {
            Interlocked.Increment(ref _activePlayers[(int)(player.Realm - 1)]);
        }

        public void DecrementPlayers(Player player)
        {
            Interlocked.Decrement(ref _activePlayers[(int)(player.Realm - 1)]);
        }

        public void ReserveGroupSlot(Player plr, int team)
        {
            /*
            //find group with open slots
            var parties = _scGroupsHandlers[plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 1 : 0].GetParties();

            foreach (var party in parties)
            {
                if (!party.IsFull)
                {
                    party.ReserveSlot(plr);
                    break;
                }
            }

            SendReservedSlots(plr.Realm);
            */
        }

        public void AddPlayerToGroup(Player plr, byte subGroup)
        {
            _scGroupsHandlers[plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 1 : 0].AddMemberToGroup(plr, subGroup);
            SendReservedSlots(plr.Realm);
        }

        public void RemovePlayerFromGroup(Player plr)
        {
            _scGroupsHandlers[plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 1 : 0].RemoveMemberFromGroup(plr);
            SendReservedSlots(plr.Realm);
        }

        public void SendReservedSlots(Realms realm)
        {
            /*
            var parties = _scGroupsHandlers[realm == Realms.REALMS_REALM_DESTRUCTION ? 1 : 0].GetParties();

            List<Tuple<byte, byte>> reserved = new List<Tuple<byte, byte>>();

            for (int partyIndex = 0; partyIndex < parties.Count; partyIndex++)
                foreach (var slot in parties[partyIndex].ReservedSlots)
                    reserved.Add(new Tuple<byte, byte>((byte) partyIndex, (byte) slot));

            PacketOut Out = new PacketOut((byte) Opcodes.F_SCENARIO_PLAYER_INFO);
            Out.WriteByte(4); // slot reservation
            Out.WriteUInt16(0); // Unk
            Out.WriteByte((byte) reserved.Count); // Count

            foreach (var slot in reserved)
            {
                Out.WriteByte((byte) (slot.Item1 + 1));
                Out.WriteByte(slot.Item2);
            }

            Out.WriteByte(0);

            SendToTeam(Out, realm == Realms.REALMS_REALM_DESTRUCTION ? 1 : 0);
            */
        }

        public void RemovePendingPlayer(Player plr)
        {
            int realmIndex = plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 1 : 0;

            if (!_pendingPlayers[realmIndex].Remove(plr))
                return;

            DecrementPlayers(plr);

            plr.ScnInterface.ClearPendingScenario();

            // Cancel Reservation

            if (!HasEnded)
            {
                ScenarioMgr.RemoveFromBalanceVector(BalanceVectors[(byte)plr.Realm - 1], plr);

                if (plr.DisconnectType == Player.EDisconnectType.Unclean)
                {
                    plr.SendClientMessage("For having declined entry to " + Info.Name + ", you have been given the Quitter! debuff.");
                    ScenarioMgr.UpdateQuitter(plr);
                }
            }

            // Update Reserved Slots
            SendReservedSlots(plr.Realm);
            // send null scenario
        }

        public void AddPlayer(Player plr)
        {
            if (HasEnded)
            {
                plr.ScnInterface.ClearPendingScenario();
                plr.SendClientMessage("Unfortunately, the scenario you attempted to join has already ended.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            if (plr.ScnInterface.PendingScenario != this)
            {
                plr.ScnInterface.ClearPendingScenario();
                plr.SendClientMessage("You're not pending entry to this scenario.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            int realmIndex = plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 1 : 0;

            _pendingPlayers[realmIndex].Remove(plr);

            Players[realmIndex].Add(plr);

            plr.ScnInterface.Scenario = this;
            plr.ScnInterface.ClearPendingScenario();

            // Reset to nearest spawn point if taking SC in ORvR area while in presence of BO or keep
            if (plr.CurrentArea != null && plr.CurrentArea.IsRvR && plr.Zone != null && (plr.CurrentKeep != null || plr.CurrentObjectiveFlag != null))
            {
                Zone_Respawn warcampRespawn = WorldMgr.GetZoneRespawn(plr.Zone.ZoneId, (byte)plr.Realm, plr);

                Point3D world = ZoneService.GetWorldPosition(ZoneService.GetZone_Info((ushort)warcampRespawn.ZoneID), warcampRespawn.PinX, warcampRespawn.PinY, warcampRespawn.PinZ);

                plr.ScnInterface.ScenarioEntryWorldX = world.X;
                plr.ScnInterface.ScenarioEntryWorldZ = world.Z;
                plr.ScnInterface.ScenarioEntryWorldY = world.Y;
                plr.ScnInterface.ScenarioEntryWorldO = warcampRespawn.WorldO;
                plr.ScnInterface.ScenarioEntryZoneId = (ushort)warcampRespawn.ZoneID;
            }

            else
            {
                plr.ScnInterface.ScenarioEntryZoneId = plr._Value.ZoneId;
                plr.ScnInterface.ScenarioEntryWorldX = plr._Value.WorldX;
                plr.ScnInterface.ScenarioEntryWorldY = plr._Value.WorldY;
                plr.ScnInterface.ScenarioEntryWorldZ = plr._Value.WorldZ;
                plr.ScnInterface.ScenarioEntryWorldO = plr._Value.WorldO;
            }

            if (_countdownStage == 0)
            {
                _countdownStage = 1;
                EventCountdown();
                EvtInterface.AddEvent(EventCountdown, 30000, 0);
            }

            TeleportInOut(plr, Region, Info.MapId,
                (uint)RespawnLocations[realmIndex].X, (uint)RespawnLocations[realmIndex].Y, (ushort)RespawnLocations[realmIndex].Z, RespawnHeadings[realmIndex]);

            if (plr.ChickenDebuff != null)
            {
                plr.ChickenDebuff.BuffHasExpired = true;
                plr.ChickenDebuff = null;
            }

            if (plr.StsInterface.BolsterLevel > 0)
                plr.RemoveBolster();

            plr.EvtInterface.AddEventNotify(EventName.OnDie, OnPlayerKilled);
        }

        public void OnPlayerPushed(Player plr)
        {
            plr.SendPacket(WorldMgr.ScenarioMgr.BuildScenarioInfo(this));
            SendObjectiveStates(plr);

            foreach (KeyValuePair<Player, ScenarioScoreboard> plrScore in PlayerScoreboard)
                plrScore.Value.SendScore(plr);

            if (!PlayerScoreboard.ContainsKey(plr))
            {
                ScenarioScoreboard score = new ScenarioScoreboard(plr);
                PlayerScoreboard.Add(plr, score);
            }

            foreach (KeyValuePair<Player, ScenarioScoreboard> plrScore in PlayerScoreboard)
                PlayerScoreboard[plr].SendScore(plrScore.Key);

            plr.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_SCENARIO_CHAT_INSTRUCTIONS);


            _scGroupsHandlers[plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 1 : 0].AddScenarioMember(plr);

            SendReservedSlots(plr.Realm);

            ((CombatInterface_Player)plr.CbtInterface).EnablePvp();
        }

        public virtual void RemovePlayer(Player plr, bool teleport)
        {
            if (plr.ScnInterface.Scenario != this)
                return;


            plr.RemoveBolster();

            DecrementPlayers(plr);

            Players[plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 1 : 0].Remove(plr);

            _scGroupsHandlers[plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 1 : 0].RemoveScenarioMember(plr);
            plr.ScnInterface.Scenario = null;

            plr.EvtInterface.RemoveEventNotify(EventName.OnDie, OnPlayerKilled);

            if (!HasEnded)
            {
                ScenarioMgr.RemoveFromBalanceVector(BalanceVectors[(byte)plr.Realm - 1], plr);
            }

            if (PlayerScoreboard.Keys.Contains(plr))
            {
                PlayerScoreboard[plr].ClearEventNotifies();
                PlayerScoreboard.Remove(plr);
            }

            if (!plr.PendingDisposal && plr.Client != null)
            {
                if (!HasEnded)
                {
                    ScenarioMgr.UpdateQuitter(plr);
                    if (!teleport)
                        plr.SendClientMessage("For having left " + Info.Name + " before it has ended, you have been given the Quitter! debuff.");
                    else
                        plr.SendClientMessage("For using an exit portal or refusing the invitation to " + Info.Name + ", you have been given the Quitter! debuff.");
                }

                if (teleport)
                    TeleportInOut(plr, null, (ushort)plr.ScnInterface.ScenarioEntryZoneId,
                        (uint)plr.ScnInterface.ScenarioEntryWorldX, (uint)plr.ScnInterface.ScenarioEntryWorldY, (ushort)plr.ScnInterface.ScenarioEntryWorldZ, (ushort)plr.ScnInterface.ScenarioEntryWorldO);
            }
            else
            {
                if (!HasEnded && plr.DisconnectType == Player.EDisconnectType.Unclean)
                    ScenarioMgr.UpdateQuitter(plr);

                plr.PendingDisposal = true;
            }

            if (!HasEnded && _scGroupsHandlers[0].IsEmpty && _scGroupsHandlers[1].IsEmpty)
                End();
        }

        public void NotifyPopExpired(Player plr)
        {
            _pendingPlayers[plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 1 : 0].Remove(plr);
            plr.ScnInterface.ClearPendingScenario();
        }

        public bool IsFull() => !(HasFreeSlots(0) || HasFreeSlots(1));

        public bool CanAcceptPair() => HasFreeSlots(0) && HasFreeSlots(1);

        public int GetTotalTeamCount(byte team) => _activePlayers[team];

        public bool HasFreeSlots(byte team) => (GetTotalTeamCount(team) < Info.MaxPlayers) && (GetTotalTeamCount(team) - GetTotalTeamCount((byte)(1 - team)) <= 0);

        public bool PartialGroupExistsFor(Player player)
        {
            return _scGroupsHandlers[(int)player.Realm - 1].PartialGroupAvailable(player.ScenarioGroup);
        }

        /// <summary>Internal utility method teleporting given player in or out of the scenario</summary>
        /// <remarks>Clears pet and cooldowns</remarks>
        private void TeleportInOut(Player plr, RegionMgr region, ushort zoneID, uint worldX, uint worldY, ushort worldZ, ushort worldO)
        {
            Pet pet = plr.CrrInterface.GetTargetOfInterest() as Pet;
            if (pet != null)
                pet.Destroy();
            plr.AbtInterface.ResetCooldowns();

            if (region != null)
                plr.Teleport(region, zoneID, worldX, worldY, worldZ, worldO);
            else
                plr.Teleport(zoneID, worldX, worldY, worldZ, worldO);
        }

        #endregion

        #region Population checking

        private int _shutdownLevel;

        public void CheckPopulation()
        {
            float popFactor;

            if (Players[0].Count == 0 || Players[1].Count == 0)
                popFactor = 2f;

            else
            {
                popFactor = Players[0].Count / (float)Players[1].Count;

                if (popFactor < 1)
                    popFactor = 1 / popFactor;
            }

            if (popFactor > 1.4f)
            {
                ++_shutdownLevel;

                if (_shutdownLevel == 1)
                {
                    foreach (Player player in Players[0])
                    {
                        player.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR_HIGH_PRIORITY, Localized_text.TEXT_SCENARIO_SHUTDOWN_IMBALANCED);
                        player.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_SCENARIO_SHUTDOWN_IMBALANCED);
                    }
                    foreach (Player player in Players[1])
                    {
                        player.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR_HIGH_PRIORITY, Localized_text.TEXT_SCENARIO_SHUTDOWN_IMBALANCED);
                        player.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_SCENARIO_SHUTDOWN_IMBALANCED);
                    }
                }
            }

            else
                _shutdownLevel = 0;
        }

        #endregion

        #endregion

        #region Event

        public virtual void OnStart()
        {
        }

        public virtual void OnClose()
        {
        }

        public void OnGuardHit(Player attacker, uint damageCount, Player tank)
        {
            if (PlayerScoreboard.ContainsKey(attacker))
            {
                PlayerScoreboard[attacker].Damage += damageCount;
                PlayerScoreboard[tank].GuardDamage += damageCount;
            }
        }

        public virtual void SendObjectiveStates(Player plr)
        {
        }

        #endregion

        #region Packets

        public void SendScoreboardUpdate()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_SCENARIO_PLAYER_INFO);
            Out.WriteByte(1); // Scoreboard
            Out.WriteUInt16(0); // Unk
            Out.WriteByte((byte)PlayerScoreboard.Count); // Count

            foreach (ScenarioScoreboard score in PlayerScoreboard.Values)
            {
                Out.WriteUInt32(score.MyPlayer.CharacterId);
                Out.WriteUInt32(score.SoloKills);
                Out.WriteUInt32(score.Kills);
                Out.WriteUInt32(score.DeathBlows);
                Out.WriteUInt32(score.Deaths);
                Out.WriteUInt32(score.Damage);
                Out.WriteUInt32(score.Healing);
                Out.WriteUInt32(score.Renown);
                Out.WriteUInt32(score.EndRenown); // Unk
                Out.WriteUInt32(score.Xp);
                Out.WriteUInt32(score.EndXP);
            }

            SendToAll(Out);
        }

        public void SendScenarioStatus(Player plr)
        {
        }

        private void SendToTeam(PacketOut Out, int team)
        {
            foreach (Player plr in Players[team])
            {
                plr.SendCopy(Out);
            }
        }

        public void SayToTeam(Player sender, string text)
        {
            if (sender.ShouldThrottle())
                return;

            foreach (Player plr in Players[sender.Realm == Realms.REALMS_REALM_ORDER ? 0 : 1])
            {
                if (!plr.BlocksChatFrom(sender))
                    plr.SendMessage(sender.Oid, sender.ChatName, text, ChatLogFilters.CHATLOGFILTERS_SCENARIO);
            }
        }

        public void SendToAll(PacketOut Out)
        {
            for (int i = 0; i < 2; ++i)
            {
                foreach (Player plr in Players[i])
                {
                    if (plr.Region == Region)
                        plr.SendCopy(Out);
                }
            }
        }

        public void SendChatMessage(string message)
        {
            for (int i = 0; i < 2; ++i)
            {
                foreach (Player plr in Players[i])
                {
                    plr.SendLocalizeString(message, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.CHAT_TAG_DEFAULT);
                }
            }
        }

        public void PlaySoundToAll(ushort sound)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAY_SOUND, 14);
            Out.WriteByte(0);
            Out.WriteUInt16(sound);
            Out.Fill(0, 10);
            SendToAll(Out);
        }

        public void PlaySoundToTeam(ushort sound, int team)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAY_SOUND, 14);
            Out.WriteByte(0);
            Out.WriteUInt16(sound);
            Out.Fill(0, 10);
            SendToTeam(Out, team);
        }

        #endregion

        #region Domination / Farm check

        private readonly int[] _totalKills = new int[2];
        private readonly List<float>[] _killRelativeDist = { new List<float>(), new List<float>() };
        private readonly float[] _spawncampingFactor = new float[2];

        private Realms _dominatingRealm;

        private void CheckDomination(Realms realm, Player killed)
        {
            int thisRealm = (int)realm - 1;
            int foeRealm = 1 - thisRealm;

            if (killed != null)
            {
                // Check how close this kill was to the enemy spawn.
                int killedDistToSpawn = Math.Max(1, killed.WorldPosition.GetDistanceTo(RespawnLocations[thisRealm]) - 100);
                int interSpawnDist = Math.Max(1, RespawnLocations[thisRealm].GetDistanceTo(RespawnLocations[foeRealm]) - 200);

                _killRelativeDist[thisRealm].Add(killedDistToSpawn / (float)interSpawnDist);

                // A factor determining how close, on average, kills are to the enemy's spawnpoint
                _spawncampingFactor[thisRealm] = _killRelativeDist[thisRealm].Average();
            }

            // Attempt to gain domination
            if (_dominatingRealm == Realms.REALMS_REALM_NEUTRAL)
            {
                if (_totalKills[thisRealm] >= 15 && Score[thisRealm] >= 50 && Score[thisRealm] / Math.Max(1, Score[foeRealm]) >= 3 && ((_totalKills[thisRealm] / Math.Max(1, _totalKills[foeRealm]) >= 2 && _spawncampingFactor[thisRealm] > 0.6f) || _spawncampingFactor[thisRealm] >= 0.75f))
                {
                    _dominatingRealm = realm;
                    TickDomination();
                    EvtInterface.AddEvent(TickDomination, 15000, 0);
                    for (int i = 0; i < 2; ++i)
                    {
                        foreach (Player player in Players[i])
                            player.SendClientMessage($"{(realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction")} is dominating the scenario!", ChatLogFilters.CHATLOGFILTERS_C_WHITE);
                    }
                }
            }
            // Attempt to break enemy's domination
            else if (_dominatingRealm != realm)
            {
                if (Score[foeRealm] / Score[thisRealm] < 3 || (_totalKills[foeRealm] / Math.Max(1, _totalKills[thisRealm]) < 2))
                {
                    _dominatingRealm = Realms.REALMS_REALM_NEUTRAL;
                    EvtInterface.RemoveEvent(TickDomination);
                    for (int i = 0; i < 2; ++i)
                    {
                        foreach (Player player in Players[i])
                            player.SendClientMessage($"{(_dominatingRealm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction")}'s domination has been broken.", ChatLogFilters.CHATLOGFILTERS_C_WHITE);
                    }
                }
            }
        }

        /// <summary>
        /// Invokes Heroic Defender repeatedly on all players of the dominated team as long as the domination is held.
        /// </summary>
        public void TickDomination()
        {
            foreach (Player player in Players[1 - ((int)_dominatingRealm - 1)])
                player.BuffInterface.QueueBuff(new BuffQueueInfo(player, player.Level, AbilityMgr.GetBuffInfo(24589)));
        }

        public void ForceDomination(int destrealm)
        {
            Realms realm = (Realms)destrealm;

            if (_dominatingRealm == realm)
                return;

            _dominatingRealm = realm;

            if (realm == Realms.REALMS_REALM_NEUTRAL)
            {
                for (int i = 0; i < 2; ++i)
                {
                    foreach (Player player in Players[i])
                        player.SendClientMessage($"{(_dominatingRealm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction")}'s domination has been broken.", ChatLogFilters.CHATLOGFILTERS_C_WHITE);
                }
                EvtInterface.RemoveEvent(TickDomination);
            }

            else
            {
                for (int i = 0; i < 2; ++i)
                {
                    foreach (Player player in Players[i])
                        player.SendClientMessage($"{(realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction")} is dominating the scenario!", ChatLogFilters.CHATLOGFILTERS_C_WHITE);
                }
                TickDomination();
                EvtInterface.AddEvent(TickDomination, 15000, 0);
            }
        }

        public void CheckDominationStatus(Player checking)
        {
            if (Score[0] / Score[1] >= 3)
                checking.SendClientMessage("Score domination: Order");
            else if (Score[1] / Score[0] >= 3)
                checking.SendClientMessage("Score domination: Destruction");
            else
                checking.SendClientMessage("Score domination: None");

            if (_totalKills[0] / Math.Max(1, _totalKills[1]) >= 2)
                checking.SendClientMessage($"Kill domination: Order ({_totalKills[0]} / {_totalKills[1]})");
            else if (_totalKills[1] / Math.Max(1, _totalKills[0]) >= 2)
                checking.SendClientMessage($"Kill domination: Destruction ({_totalKills[1]} / {_totalKills[0]})");
            else
                checking.SendClientMessage("Kill domination: None");

            checking.SendClientMessage("Order total kills " + _totalKills[0] + ", spawncamping factor: " + _spawncampingFactor[0]);
            checking.SendClientMessage("Destruction total kills " + _totalKills[1] + ", spawncamping factor: " + _spawncampingFactor[1]);
        }


        public void GetScenarioScore(Player player)
        {
            player.SendClientMessage($"Kills:{PlayerScoreboard[player].Kills} " +
                                     $"Guard Damage:{PlayerScoreboard[player].GuardDamage} " +
                                     $"Solo Kills:{PlayerScoreboard[player].SoloKills} "+
                                     $"Healing:{PlayerScoreboard[player].Healing} ");
        }

        #endregion

        #region Scenario Objects

        protected List<HoldObject> MapTrackedObjects = new List<HoldObject>();

        protected void LoadScenarioObject(Scenario_Object obj)
        {
            switch (obj.Type)
            {
                case "Pityball":
                    HoldObject essence = new HoldObject(obj.Identifier, obj.ObjectiveName, new Point3D(obj.WorldPosX, obj.WorldPosY, obj.PosZ), 14052, 30000, ObjectPickedUp, ObjectDropped, ObjectReset, null, 1491, 1491);
                    Region.AddObject(essence, Info.MapId);
                    AddTrackedObject(essence);
                    break;
                case "Murderball":
                    HoldObject murderball = new HoldObject(obj.Identifier, obj.ObjectiveName, new Point3D(obj.WorldPosX, obj.WorldPosY, obj.PosZ), 14053, 30000, ObjectPickedUp, ObjectDropped, ObjectReset, null, 3463, 3463);
                    Region.AddObject(murderball, Info.MapId);
                    AddTrackedObject(murderball);
                    murderball.SetActive(0);
                    break;
            }
        }

        #endregion

        #region Map tracking for carried objectives

        public void AddTrackedObject(HoldObject ball)
        {
            MapTrackedObjects.Add(ball);

            if (MapTrackedObjects.Count == 1)
                EvtInterface.AddEvent(UpdateFlagLocations, 500, 0);
        }

        public void ObjectPickedUp(HoldObject ball, Player pickedBy)
        {
            for (int i = 0; i < 2; ++i)
                foreach (Player plr in Players[i])
                {
                    plr.SendLocalizeString(new[] { pickedBy.GenderedName, pickedBy.Realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction", ball.name }, ChatLogFilters.CHATLOGFILTERS_C_WHITE, Localized_text.TEXT_FLAG_PICKUP);
                    SendFlagObjectState(plr, ball);
                }
        }

        public void ObjectDropped(HoldObject ball)
        {
            for (int i = 0; i < 2; ++i)
                foreach (Player plr in Players[i])
                {
                    plr.SendLocalizeString(ball.name, ChatLogFilters.CHATLOGFILTERS_C_WHITE, Localized_text.TEXT_FLAG_DROP);
                    SendFlagObjectState(plr, ball);
                }
        }

        public void ObjectReset(HoldObject ball)
        {
            for (int i = 0; i < 2; ++i)
                foreach (Player plr in Players[i])
                {
                    plr.SendClientMessage($"The {ball.name} has been reset.", ChatLogFilters.CHATLOGFILTERS_C_WHITE);
                    SendFlagObjectState(plr, ball);
                }
        }

        /// <summary>
        /// Updates the UI blip for any pickup objects that are currently being transported by a player.
        /// </summary>
        public void UpdateFlagLocations()
        {
            for (int i = 0; i < 2; ++i)
            {
                foreach (HoldObject ball in MapTrackedObjects)
                {
                    if (ball.HeldState != EHeldState.Carried)
                        continue;

                    foreach (Player plr in Players[i])
                        SendFlagObjectLocation(plr, ball);
                }
            }
        }

        /// <summary>
        /// Sends the current state of a pickup object to the given player.
        /// </summary>
        protected void SendFlagObjectState(Player plr, HoldObject ball)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_FLAG_OBJECT_STATE, 32);
            Out.WriteUInt32(ball.Identifier);
            Out.WriteByte((byte)ball.HeldState);
            Out.WriteByte(ball.ObjectType);

            switch (ball.HeldState)
            {
                case EHeldState.Carried:
                    Out.WriteUInt32(ball.Holder.CharacterId);
                    Out.WritePascalString(ball.Holder.GenderedName);
                    break;
                case EHeldState.Ground:
                    Out.WriteUInt32(ball.Holder?.CharacterId ?? 0);
                    break;
                default:
                    Out.Fill(0, 2);
                    break;
            }

            plr.SendPacket(Out);

            SendFlagObjectLocation(plr, ball);
        }

        /// <summary>
        /// Sends the current location of a pickup object to the given player for UI display.
        /// </summary>
        protected void SendFlagObjectLocation(Player plr, HoldObject ball)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_FLAG_OBJECT_LOCATION, 24);
            Out.WriteUInt32(ball.Identifier);

            if (ball.HeldState == EHeldState.Carried && ball.Holder != null)
            {
                Out.WriteByte((byte)ball.Holder.Realm);
                Out.Fill(0, 3);
                Out.WriteUInt32((uint)ball.Holder.WorldPosition.X);
                Out.WriteUInt32((uint)ball.Holder.WorldPosition.Y);
                Out.WriteUInt32((uint)ball.Holder.Z);
            }

            else
            {
                Out.WriteByte(0);
                Out.Fill(0, 3);
                Out.WriteUInt32((uint)ball.WorldPosition.X);
                Out.WriteUInt32((uint)ball.WorldPosition.Y);
                Out.WriteUInt32((uint)ball.WorldPosition.Z);
            }

            plr.SendPacket(Out);
        }

        #endregion

        public virtual bool TooCloseToSpawn(Player plr)
        {
            return plr.Get2DDistanceToWorldPoint(RespawnLocations[plr.Realm == Realms.REALMS_REALM_ORDER ? 0 : 1]) < 100;
        }
    }
}
