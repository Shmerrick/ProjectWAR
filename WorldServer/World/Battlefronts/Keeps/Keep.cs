using System;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using Common;
using FrameWork;
using GameData;
using NLog;
using CreatureSubTypes = GameData.CreatureSubTypes;
using WorldServer.World.Objects.PublicQuests;
using WorldServer.Scenarios.Objects;
using WorldServer.Services.World;

namespace WorldServer.World.BattleFronts.Keeps
{
    public class Keep : BattleFrontObjective
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();


        /*  public enum KeepMessage
        {
            KEEP_OUTER_100,
            KEEP_OUTER_50,
            KEEP_OUTER_20,
            KEEP_OUTER_0,
            KEEP_SANCTUM_100,
            KEEP_SANCTUM_50,
            KEEP_SANCTUM_20,
            KEEP_SANCTUM_0,
            KEEP_LORD_100,
            KEEP_LORD_50,
            KEEP_LORD_

        }*/


        public const int KEEP_INNER_DOOR_VICTORYPOINTS = 500;
        public const int KEEP_OUTER_DOOR_VICTORYPOINTS = 250;

        public enum KeepMessage
        {
            Safe,
            Outer75,
            Outer50,
            Outer20,
            Outer0,
            Inner75,
            Inner50,
            Inner20,
            Inner0,
            Lord100,
            Lord50,
            Lord20,
            Fallen
        }

        private class Hardpoint : Point3D
        {
            public readonly SiegeType SiegeType;
            public readonly ushort Heading;
            public Siege CurrentWeapon;
            public KeepMessage SiegeRequirement = KeepMessage.Safe;

            public Hardpoint(SiegeType type, int x, int y, int z, int heading)
            {
                SiegeType = type;
                X = x;
                Y = y;
                Z = z;
                Heading = (ushort) heading;
            }
        }

        /// <summary>
        /// A list of positions where certain siege weapons may be deployed.
        /// </summary>
        private readonly List<Hardpoint> _hardpoints = new List<Hardpoint>();

		// keep safe timer variables
		private int _safeKeepTimer = 0;
		private const int TIMESPAN_SAFEKEEP = 15 * 60;

		// public variables
		public byte Tier;
        public bool InformRankOne = false;
        public int _OrderCount = 0;
        public int _DestroCount = 0;
        public Keep_Info Info;
        public Dictionary<uint, ContributionInfo> Attackers = new Dictionary<uint, ContributionInfo>();
        public Dictionary<uint, ContributionInfo> Defenders = new Dictionary<uint, ContributionInfo>();
        public Realms Realm;
        public bool Ruin = false;
        public RegionMgr Region;

		public List<KeepNpcCreature> Creatures = new List<KeepNpcCreature>();
        public List<KeepDoor> Doors = new List<KeepDoor>();
        public List<KeepDoor.KeepGameObject> KeepGOs = new List<KeepDoor.KeepGameObject>();

        public List<BattleFrontResourceSpawn> SupplyReturnPoints;

        public bool RamDeployed = false;

        public Keep(Keep_Info info, byte tier, RegionMgr region)
        {
            Info = info;
            Realm = (Realms) info.Realm;
            Tier = tier;
            Region = region;

            _hardpoints.Add(new Hardpoint(SiegeType.OIL, info.OilX, info.OilY, info.OilZ, info.OilO));
            if (info.OilOuterX > 0)
                _hardpoints.Add(new Hardpoint(SiegeType.OIL, info.OilOuterX, info.OilOuterY, info.OilOuterZ, info.OilOuterO));

            _hardpoints.Add(new Hardpoint(SiegeType.RAM, info.RamX, info.RamY, info.RamZ, info.RamO));

            if (info.RamOuterX > 0)
            {
                _hardpoints[_hardpoints.Count - 1].SiegeRequirement = KeepMessage.Outer0;
                _hardpoints.Add(new Hardpoint(SiegeType.RAM, info.RamOuterX, info.RamOuterY, info.RamOuterZ, info.RamOuterO));
            }

            //SetSupplyRequirement();

            EvtInterface.AddEvent(UpdateResources, 60000, 0);
        }

        public override void OnLoad()
        {
            Z = Info.Z;
            X = Zone.CalculPin((uint) (Info.X), true);
            Y = Zone.CalculPin((uint) (Info.Y), false);
            base.OnLoad();

            Heading = (ushort) Info.O;
            WorldPosition.X = Info.X;
            WorldPosition.Y = Info.Y;
            WorldPosition.Z = Info.Z;

            SetOffset((ushort) (Info.X >> 12), (ushort) (Info.Y >> 12));

            IsActive = true;

            foreach (KeepNpcCreature crea in Creatures)
            {
                crea.SpawnGuard(Realm);
            }

            foreach (KeepDoor door in Doors)
            {
                door.Spawn();
            }


            if (WorldMgr._Keeps.ContainsKey(Info.KeepId))
                WorldMgr._Keeps[Info.KeepId] = this;
            else
            {
                WorldMgr._Keeps.Add(Info.KeepId, this);
            }

            SupplyReturnPoints = BattleFrontService.GetResourceSpawns(Info.KeepId);

            if (SupplyReturnPoints == null)
                Log.Error("Keep", $"No resource return points for {Info.Name}");
            else
            {
                CreateSupplyDrops();
            }
        }

        #region Keep Progression

        public KeepStatus KeepStatus = KeepStatus.KEEPSTATUS_SAFE;
        public KeepMessage LastMessage;

        public void OnKeepDoorAttacked(byte number, byte pctHealth)
        {
            if (number == 2)
            {
                if (KeepStatus == KeepStatus.KEEPSTATUS_SAFE)
                {
                    UpdateKeepStatus(KeepStatus.KEEPSTATUS_OUTER_WALLS_UNDER_ATTACK);
                    SendRegionMessage(Info.Name + "'s outer door is under attack!");
                    foreach (Player plr in PlayersInRange)
                        SendKeepInfo(plr);
                    EvtInterface.AddEvent(UpdateStateOfTheRealmKeep,100,1);
                }

                if (LastMessage < KeepMessage.Outer0)
                {
                    if (pctHealth < 75 && LastMessage < KeepMessage.Outer75)
                    {
                        SendRegionMessage(Info.Name + "'s outer door is taking damage!");
                        LastMessage = KeepMessage.Outer75;
                        SendKeepStatus(null);
                        EvtInterface.AddEvent(UpdateStateOfTheRealmKeep,100,1);
                    }
                    if (pctHealth < 50 && LastMessage < KeepMessage.Outer50)
                    {
                        SendRegionMessage(Info.Name + "'s outer door begins to buckle under the assault!");
                        LastMessage = KeepMessage.Outer50;
                        SendKeepStatus(null);
                        EvtInterface.AddEvent(UpdateStateOfTheRealmKeep,100,1);
                    }
                    if (pctHealth < 20 && LastMessage < KeepMessage.Outer20)
                    {
                        SendRegionMessage(Info.Name + "'s outer door creaks and moans. It appears to be almost at the breaking point!");
                        LastMessage = KeepMessage.Outer20;
                        SendKeepStatus(null);
                        EvtInterface.AddEvent(UpdateStateOfTheRealmKeep,100,1);
                        
                    }
                }
            }
            else if (number == 1)
            {
                if ((Info.DoorCount == 1 && KeepStatus == KeepStatus.KEEPSTATUS_SAFE) || KeepStatus == KeepStatus.KEEPSTATUS_OUTER_WALLS_UNDER_ATTACK)
                {
                    UpdateKeepStatus(KeepStatus.KEEPSTATUS_INNER_SANCTUM_UNDER_ATTACK);
                    SendRegionMessage(Info.Name + "'s inner sanctum door is under attack!");
                    foreach (Player plr in PlayersInRange)
                        SendKeepInfo(plr);
                    EvtInterface.AddEvent(UpdateStateOfTheRealmKeep,100,1);
                }

                if (LastMessage < KeepMessage.Inner0)
                {
                    if (pctHealth < 75 && LastMessage < KeepMessage.Inner75)
                    {
                        SendRegionMessage(Info.Name + "'s inner sanctum door is taking damage!");
                        LastMessage = KeepMessage.Inner75;
                        SendKeepStatus(null);
                        EvtInterface.AddEvent(UpdateStateOfTheRealmKeep,100,1);
                    }
                    if (pctHealth < 50 && LastMessage < KeepMessage.Inner50)
                    {
                        SendRegionMessage(Info.Name + "'s inner sanctum door begins to buckle under the assault!");
                        LastMessage = KeepMessage.Inner50;
                        SendKeepStatus(null);
                        EvtInterface.AddEvent(UpdateStateOfTheRealmKeep,100,1);
                    }
                    if (pctHealth < 20 && LastMessage < KeepMessage.Inner20)
                    {
                        SendRegionMessage(Info.Name + "'s inner sanctum door creaks and moans. It appears to be almost at the breaking point!");
                        LastMessage = KeepMessage.Inner20;
                        SendKeepStatus(null);
                        EvtInterface.AddEvent(UpdateStateOfTheRealmKeep,100,1);

                        // This disables rewards for attackers
                        if (Constants.DoomsdaySwitch == 2)
                        {
                            if (WorldMgr.WorldSettingsMgr.GetPopRewardSwitchSetting() == 1)
                            {
                                _DestroCount = Region.Campaign._totalMaxDestro;
                                _OrderCount = Region.Campaign._totalMaxOrder;

#if !DEBUG
                                if (Info.Realm == (byte)Realms.REALMS_REALM_ORDER)
                                {
                                    if (_DestroCount > _OrderCount * 4)
                                    {
                                        Region.Campaign.DefenderPopTooSmall = true;
                                        SendRegionMessage("The forces of Destruction are attacking abandoned keep, there are no spoils of war inside!");
                                    }
                                }
                                else
                                {
                                    if (_OrderCount > _DestroCount * 4)
                                    {
                                        Region.Campaign.DefenderPopTooSmall = true;
                                        SendRegionMessage("The forces of Order are attacking abandoned keep, there are no spoils of war inside!");
                                    }
                                }
#endif
                            }
                        }
                    }
                }
            }
        }

        public void OnDoorDestroyed(byte number, Realms realm)
        {
            if (LastMessage < KeepMessage.Inner0)
            {
                switch (number)
                {
                    case 1:
                        SendRegionMessage(Info.Name + "'s inner sanctum door has been destroyed!");
                        if (Rank == 0)
                            SendRegionMessage(Info.Name + "'s inner sanctum postern is no longer defended!");
                        LastMessage = KeepMessage.Inner0;

                        foreach (Hardpoint h in _hardpoints)
                        {
                            if (h.SiegeType == SiegeType.RAM && (Tier == 2 || h.SiegeRequirement > 0))
                            {
                                h.CurrentWeapon?.Destroy();
                                RamDeployed = false;
                                break;
                            }
                        }

                        // Small reward for inner door destruction
                        foreach (Player player in PlayersInRange)
                        {
                            if (!player.Initialized)
                                continue;
                            Random rnd = new Random();
                            int random = rnd.Next(1, 25);
                            player.AddXp((uint)(1500 * (1 + (random / 100))), false, false);
                            player.AddRenown((uint)(400 * (1 + (random / 100))), false, RewardType.ObjectiveCapture, Info.Name);
                        }

                        if (realm == Realms.REALMS_REALM_DESTRUCTION)
                            this.Region.Campaign.VictoryPointProgress.DestructionVictoryPoints += KEEP_INNER_DOOR_VICTORYPOINTS;
                        else
                        {
                            this.Region.Campaign.VictoryPointProgress.OrderVictoryPoints += KEEP_INNER_DOOR_VICTORYPOINTS;
                        }

                        _logger.Debug($"Inner door destroyed for realm {Realms.REALMS_REALM_DESTRUCTION} adding {KEEP_INNER_DOOR_VICTORYPOINTS} VP");

                        break;
                    case 2:
                        SendRegionMessage(Info.Name + "'s outer door has been destroyed!");
						if (Rank == 0)
                        	SendRegionMessage($"{Info.Name}'s outer postern{(Tier == 2 ? " is " : "s are ")} no longer defended!");
                        LastMessage = KeepMessage.Outer0;


                        // Small reward for outer door destruction
                        foreach (Player player in PlayersInRange)
                        {
                            if (!player.Initialized)
                                continue;

                            Random rnd = new Random();
                            int random = rnd.Next(1, 25);

                            player.AddXp((uint) (1000 * (1+(random/100))), false, false);
                            player.AddRenown((uint) (200 * (1 + (random / 100))), false, RewardType.ObjectiveCapture, Info.Name);
                        }

                        foreach (Hardpoint h in _hardpoints)
                        {
                            if (h.SiegeType == SiegeType.RAM)
                            {
                                h.CurrentWeapon?.Destroy();
                                RamDeployed = false;
                                break;
                            }
                        }

                        _logger.Debug($"Outer door destroyed for realm {Realms.REALMS_REALM_DESTRUCTION} adding {KEEP_OUTER_DOOR_VICTORYPOINTS} VP");

                        if (realm == Realms.REALMS_REALM_DESTRUCTION)
                            this.Region.Campaign.VictoryPointProgress.DestructionVictoryPoints += KEEP_OUTER_DOOR_VICTORYPOINTS;
                        else
                        {
                            this.Region.Campaign.VictoryPointProgress.OrderVictoryPoints += KEEP_OUTER_DOOR_VICTORYPOINTS;
                        }


                        break;
                }

                SendKeepStatus(null);
            }

            EvtInterface.AddEvent(UpdateStateOfTheRealmKeep,100,1);

        }

        public void OnKeepLordAttacked(byte pctHealth)
        {
            if (KeepStatus == KeepStatus.KEEPSTATUS_INNER_SANCTUM_UNDER_ATTACK)
            {
                UpdateKeepStatus(KeepStatus.KEEPSTATUS_KEEP_LORD_UNDER_ATTACK);
                foreach (Player plr in PlayersInRange)
                    SendKeepInfo(plr);
                EvtInterface.AddEvent(UpdateStateOfTheRealmKeep,100,1);
            }

            if (pctHealth < 100 && LastMessage < KeepMessage.Lord100)
            {
                SendRegionMessage(Info.Name + "'s Keep Lord is under attack!");
                LastMessage = KeepMessage.Lord100;
                EvtInterface.AddEvent(UpdateStateOfTheRealmKeep,100,1);
            }
            else if (pctHealth < 50 && LastMessage < KeepMessage.Lord50)
            {
                SendRegionMessage(Info.Name + "'s Keep Lord is being overrun!");
                LastMessage = KeepMessage.Lord50;
                EvtInterface.AddEvent(UpdateStateOfTheRealmKeep,100,1);
            }
            else if (pctHealth < 20 && LastMessage < KeepMessage.Lord20)
            {
                SendRegionMessage(Info.Name + "'s Keep Lord is weak!");
                LastMessage = KeepMessage.Lord20;
                EvtInterface.AddEvent(UpdateStateOfTheRealmKeep,100,1);
            }
        }

        public void OnKeepLordDied()
        {
            // if (KeepStatus == KeepStatus.KEEPSTATUS_LOCKED)
            //     return;

            foreach (Hardpoint h in _hardpoints)
                h.CurrentWeapon?.Destroy();

            UpdateKeepStatus(KeepStatus.KEEPSTATUS_SEIZED);
            Ruin = true;
            
            _safeKeepTimer = TCPManager.GetTimeStamp() + 45 * 60;

            // Despawn Keep Creatures
            foreach (KeepNpcCreature crea in Creatures)
            {
                crea.SpawnGuard(0);
                // This is spawning new ProximityFlag inside keep
                // We are also moving keep rank from keep level to realm level
                if (crea.Info.KeepLord)
                    SpawnRuinFlag(crea);
            }

            Realm = ((Realm == Realms.REALMS_REALM_ORDER) ? Realms.REALMS_REALM_DESTRUCTION : Realms.REALMS_REALM_ORDER);

            foreach (Player plr in PlayersInRange)
                SendKeepInfo(plr);

            if (LastMessage < KeepMessage.Fallen)
            {
                SendRegionMessage(Info.Name + "'s Keep Lord has fallen!");
                LastMessage = KeepMessage.Fallen;
                _logger.Info($"Awarding VP for Keep Lord kill");
                if (Realm == Realms.REALMS_REALM_ORDER)
                {
                    WorldMgr.UpperTierCampaignManager.GetActiveCampaign().VictoryPointProgress.OrderVictoryPoints += 1500;
                }
                else
                {
                    WorldMgr.UpperTierCampaignManager.GetActiveCampaign().VictoryPointProgress.DestructionVictoryPoints += 1500;
                }
               
                

            }

            /*if (_playersKilledInRange >= (4*Tier))
            {
                Dictionary<uint, ContributionInfo> attackers = Region.Campaign.GetContributorsFromRealm(Realm);
                GoldChest.Create(Region, Info.PQuest, ref attackers);
            }*/

            DistributeRewards();

            Rank = 0;
            _currentResource = 0;
            _currentResourcePercent = 0;
            //SetSupplyRequirement();
            SendKeepStatus(null);

            EvtInterface.AddEvent(UpdateStateOfTheRealmKeep,100,1);
        }

        public void SpawnRuinFlag(KeepNpcCreature crea)
        {
            //var flag = new Objectives(Info.KeepId, "Ruins of " + Info.Name, (ushort)ZoneId, (uint)crea.Info.X, (uint)crea.Info.Y, (ushort)crea.Info.Z, (ushort)crea.Info.O, Region.Campaign, Region, Tier);
            // TODO - replace??? Region.Campaign.Objectives.Add(flag);
            //Region.AddObject(flag, (ushort)ZoneId);

            //flag.Ruin = true;

            //// Need to be correctly set
            //flag.SetWarcampDistanceScaler(1, 1);
        }

        public void DistributeRewards()
        {
            Log.Info("Keep", "Locking " + Zone.Info.Name);
            _logger.Info("**********************KEEP FLIP******************************");
            _logger.Info($"Distributing rewards for Keep {this.Name}");
            _logger.Info("*************************************************************");
            uint influenceId = 0;

            byte objCount = 0;

            bool battlePenalty = false;

            _logger.Debug($"Keep flip reward for BO hold");
            foreach (var flag in Region.Campaign.Objectives)
            {
                // RB   5/21/2016   Battlefield Objectives now reward defenders when a keep is captured.
                if (flag.OwningRealm == Realm)
                {
                    if (flag.ZoneId == this.ZoneId)
                    {
                        ++objCount;
                        flag.GrantKeepCaptureRewards();
                    }
                }
            }

            int totalXp = (800 * Tier) + (200 * Tier * objCount) + (_playersKilledInRange * Tier * 30); // Field of Glory, reduced
            int totalRenown = (250 * Tier) + (120 * Tier * objCount) + (_playersKilledInRange * 75);   // Ik : Increased values here.
            int totalInfluence = (40 * Tier) + (20 * Tier * objCount) + (_playersKilledInRange * Tier * 6);

            if (_playersKilledInRange < (4 * Tier))
            {
                battlePenalty = true;

                totalXp = (int) (totalXp * (0.25 + (_playersKilledInRange / 40f) * 0.75));
                totalRenown = (int) (totalRenown * (0.25 + (_playersKilledInRange / 40f) * 0.75));
                totalInfluence = (int) (totalInfluence * (0.25 + (_playersKilledInRange / 40f) * 0.75));
            }

            Log.Info("Keep", $"Lock XP : {totalXp} RP: {totalRenown}, Influence: {totalInfluence}");

            _logger.Info($"Lock XP : {totalXp} RP: {totalRenown}, Influence: {totalInfluence}");

            // Dont believe contribution is being triggered.
            // Dictionary<uint, ContributionInfo> contributors = Region.Campaign.GetContributorsFromRealm(Realm);

            //if (contributors.Count == 0)
            //{
            //    _playersKilledInRange = 0;
            //    return;
            //}

            //uint maxContribution = contributors.Values.Max(x => x.BaseContribution);

            //Log.Info("Keep", $"Contributor count : {contributors.Count} Max contribution: {maxContribution}");

            //if (maxContribution == 0)
            //{
            //    _playersKilledInRange = 0;
            //    return;
            //}

            try
            {
                var activeBattleFrontId = WorldMgr.UpperTierCampaignManager.ActiveBattleFront.BattleFrontId;
                var activeBattleFrontStatus = WorldMgr.UpperTierCampaignManager.GetActiveBattleFrontStatus(activeBattleFrontId);
                var eligiblePlayers = WorldMgr.UpperTierCampaignManager.GetEligiblePlayers(activeBattleFrontStatus);

                foreach (var characterId in eligiblePlayers)
                {
                    var player = Player.GetPlayer(characterId);

                    if (player == null)
                        continue;

                    if (!player.Initialized)
                        continue;

                    if (player.ValidInTier(Tier, true))
                        player.QtsInterface.HandleEvent(Objective_Type.QUEST_CAPTURE_KEEP, Info.KeepId, 1);


                    if (HasInRange(player))
                    {
                        SendKeepInfo(player);
                    }
                    else
                    {
                        player.SendClientMessage("The keep was taken, but you were too far away!");
                        return;
                    }

                    if (influenceId == 0)
                        influenceId = (player.Realm == Realms.REALMS_REALM_DESTRUCTION) ? player.CurrentArea.DestroInfluenceId : player.CurrentArea.OrderInfluenceId;

                    player.AddXp((uint)totalXp, false, false);
                    // New method- non scaling renown. RP not effected by AAO and similar things.
                    player.AddRenown((uint)totalRenown, false, RewardType.ZoneKeepCapture, Info.Name);
                    player.AddInfluence((ushort)influenceId, (ushort)totalInfluence);

                    if (battlePenalty)
                        player.SendClientMessage("This keep was taken with little to no resistance. The rewards have therefore been reduced.");
                    else
                        // Invader crests
                        player.ItmInterface.CreateItem((uint)(208429), (ushort)5);

                    _logger.Info($"Distributing rewards for Keep {this.Name} to {player.Name} RR:{totalRenown} INF:{totalInfluence}");
                }

                _playersKilledInRange = 0;
            }
            catch (Exception e)
            {
                _logger.Error($"Exception distributing rewards for Keep take {e.Message} {e.StackTrace}");
                throw;
            }

         

        
        }

        public void ResetSafeTimer()
        {
            if (KeepStatus != KeepStatus.KEEPSTATUS_SAFE && KeepStatus != KeepStatus.KEEPSTATUS_SEIZED)
            {
                _safeKeepTimer = TCPManager.GetTimeStamp() + TIMESPAN_SAFEKEEP;
            }
        }

        public void SafeKeep()
        {
            uint influenceId = 0;

            if (KeepStatus == KeepStatus.KEEPSTATUS_SEIZED || KeepStatus == KeepStatus.KEEPSTATUS_LOCKED || KeepStatus == KeepStatus.KEEPSTATUS_SAFE)
                return;

            foreach (Player plr in PlayersInRange)
            {
                if (Realm == plr.Realm && plr.ValidInTier(Tier, true))
                {
                    if (influenceId == 0)
                        influenceId = (plr.Realm == Realms.REALMS_REALM_DESTRUCTION) ? plr.CurrentArea.DestroInfluenceId : plr.CurrentArea.OrderInfluenceId;

					int totalXp = 2000 * Tier;
					int totalRenown = 300 * Tier;
					int totalInfluence = 100 * Tier;

					if (_playersKilledInRange < (4 * Tier))
					{
						totalXp += (int)(totalXp * (0.25 + (_playersKilledInRange / 40f) * 0.75));
						totalRenown += (int)(totalRenown * (0.25 + (_playersKilledInRange / 40f) * 0.75));
						totalInfluence += (int)(totalInfluence * (0.25 + (_playersKilledInRange / 40f) * 0.75));
					}

					plr.AddXp((uint)totalXp, false, false);
					plr.AddRenown((uint)totalRenown, false, RewardType.ObjectiveDefense, Info.Name);
					plr.AddInfluence((ushort)influenceId, (ushort)totalInfluence);
					
					plr.SendClientMessage($"You've received a reward for your contribution to the holding of {Info.Name}.", ChatLogFilters.CHATLOGFILTERS_RVR);

					Log.Info("Keep", $"Keep Defence XP : {totalXp} RP: {totalRenown}, Influence: {totalInfluence}");
				}

				SendKeepInfo(plr);
            }

            foreach (KeepNpcCreature crea in Creatures)
            {
                if (crea.Creature == null)
                    crea.SpawnGuard(Realm);
            }

            foreach (KeepDoor door in Doors)
            {
                door.Spawn();
            }

            Log.Info("SafeKeep", "Players Killed: " + _playersKilledInRange);

            if ((LastMessage >= KeepMessage.Outer0 && Tier > 2) || (Tier == 2 && LastMessage >= KeepMessage.Inner0))
            {
                /*if (StaticRandom.Instance.Next(100) < 25)
                {
                    foreach (ContributionInfo plrInfo in Defenders.Values)
                        plrInfo.RandomBonus = (ushort) StaticRandom.Instance.Next(1000);
                    GoldChest.Create(Region, Info.PQuest, ref Defenders);
                }*/
                _playersKilledInRange /= 2;
            }

            UpdateKeepStatus(KeepStatus.KEEPSTATUS_SAFE);
            LastMessage = KeepMessage.Safe;

            _OrderCount = 0;
            _DestroCount = 0;
			_playersKilledInRange = 0;
		}

        private void UpdateKeepStatus(KeepStatus newStatus)
        {
            KeepStatus = newStatus;
            SendKeepStatus(null);
        }

        /// <summary>
        /// Scales the lord depending on enemy population.
        /// </summary>
        /// <param name="enemyPlayercount">Maximum number of enemies in short history.</param>
        public void ScaleLord(int enemyPlayercount)
        {
            foreach (KeepNpcCreature crea in Creatures)
                if (crea.Creature != null && crea.Info.KeepLord)
                    crea.Creature.ScaleLord(enemyPlayercount);
        }

        public void ScaleLordVP(int vp)
        {
            foreach (KeepNpcCreature crea in Creatures)
                if (crea.Creature != null && crea.Info.KeepLord)
                    crea.Creature.ScaleLordVP(vp);
        }

        #endregion

        #region Range

        private short _playersInRange;

        public void AddPlayer(Player plr)
        {
            if (plr == null)
                return;

            plr.CurrentKeep?.RemovePlayer(plr);

            if (!Ruin)
                SendKeepInfo(plr);

            // RB   5/22/2016   Prevent underleveled players from leeching rewards.
            if (plr.Realm == Realm && Tier > 1 && plr.ValidInTier(Tier, true))
                Defenders.Add(plr.CharacterId, new ContributionInfo(plr));
            
            plr.CurrentKeep = this;
            ++_playersInRange;
        }

        public void RemovePlayer(Player plr)
        {
            if (plr == null)
                return;

            SendKeepInfoLeft(plr);
            if (plr.CurrentKeep == this)
                plr.CurrentKeep = null;
            --_playersInRange;
            Defenders.Remove(plr.CharacterId);
        }

#endregion

#region Reward Management

        private ushort _playersKilledInRange;

        public bool CheckKillValid(Player player)
        {
            int antiFarmCount = 3;

            antiFarmCount = 0;

            if (KeepStatus != KeepStatus.KEEPSTATUS_LOCKED && _playersInRange >= antiFarmCount && player.CurrentKeep == this)
            {
                ++_playersKilledInRange;

                return true;
            }

            return false;
        }

        public void ModifyLoot(LootContainer lootContainer)
        {
            if (StaticRandom.Instance.Next(100) <= 10)
                lootContainer.LootInfo.Add(new LootInfo(ItemService.GetItem_Info(208470)));
        }

#endregion

#region Resources

        //DoomsDay Change
        //public byte Rank { get; private set; }
        public byte Rank { get; set; }

        public float _currentResource;
        public int _maxResource;
        public byte _currentResourcePercent;

        public int _lastReturnSeconds;

        public readonly int[] _resourcePerRank = { 3, 5, 7, 9, 12, 14/*, 18*/ };
        public readonly int[] _resourceValueMax = { 12, 24, 48, 72, 108, 144/*, 180*/ };

        //public void SetSupplyRequirement(int realm = -1)
        //{
        //    if (realm > -1)
        //    {
        //        Region.Campaign.RealmMaxResource[realm-1] = Region.Campaign._RealmResourcePerRank[Region.Campaign.RealmRank[realm-1]] * Region.Campaign._RealmResourceValueMax[Region.Campaign.RealmRank[realm - 1]];
        //    }
        //    else
        //        _maxResource = _resourcePerRank[Rank] * _resourceValueMax[Rank];
        //}

        public GameObject ResourceReturnFlag;

        public void CreateSupplyDrops()
        {
            for (int index = 0; index < SupplyReturnPoints.Count; index++)
            {
                var returnPoint = SupplyReturnPoints[index];

                Realms displayRealm = index == 0 ? Realm : GetContestedRealm();

                GameObject_proto proto = GameObjectService.GetGameObjectProto(displayRealm == Realms.REALMS_REALM_ORDER ? (uint)100650 : (uint)100651);

                Point3D flagPos = ZoneService.GetWorldPosition(Zone.Info, (ushort) returnPoint.X, (ushort) returnPoint.Y, (ushort) returnPoint.Z);

                GameObject_spawn spawn = new GameObject_spawn
                {
                    WorldX = flagPos.X,
                    WorldY = flagPos.Y,
                    WorldZ = flagPos.Z,
                    WorldO = returnPoint.O,
                    ZoneId = Info.ZoneId
                };

                spawn.BuildFromProto(proto);

                switch (displayRealm)
                {
                    case Realms.REALMS_REALM_ORDER:
                        switch (Zone.Info.ZoneId/100)
                        {
                            // Dwarf
                            case 0:
                                spawn.DisplayID = 3238;
                                break;

                            // Empire
                            case 1:
                                spawn.DisplayID = 4753;
                                break;

                            // High Elf
                            case 2:
                                spawn.DisplayID = 4769;
                                break;
                        }
                        break;
                    case Realms.REALMS_REALM_DESTRUCTION:
                        switch (Zone.Info.ZoneId/100)
                        {
                            // Greenskin
                            case 0:
                                spawn.DisplayID = 4779;
                                break;

                            // Chaos
                            case 1:
                                spawn.DisplayID = 4782;
                                break;

                            // Dark Elf
                            case 2:
                                spawn.DisplayID = 1463;
                                break;
                        }
                        break;
                }

                ResourceReturnFlag = Region.CreateGameObject(spawn);

                ResourceReturnFlag.CaptureDuration = 3;
                ResourceReturnFlag.AssignCaptureCheck(CheckHoldingSupplies);
                //ResourceReturnFlag.AssignCaptureDelegate(SuppliesReturned);
            }
        }

        public bool CheckHoldingSupplies(Player returner)
        {
            if (returner.Realm != Realm || returner.HeldObject == null)
                return false;

            NewBuff buff = returner.BuffInterface.GetBuff((ushort) GameBuffs.ResourceCarrier, returner);

            return buff != null && !buff.BuffHasExpired;
        }

//        public void SuppliesReturned(Player returner, GameObject sender)
//        {
//            HoldObjectBuff buff = (HoldObjectBuff) returner.BuffInterface.GetBuff((ushort)GameBuffs.ResourceCarrier, returner);

//            if (buff == null || buff.BuffHasExpired)
//                return;

//            if (KeepStatus == KeepStatus.KEEPSTATUS_LOCKED)
//            {
//                returner.SendClientMessage("This keep is not active!", ChatLogFilters.CHATLOGFILTERS_RVR);
//                return;
//            }

//            foreach (Player plr in Region.Players)
//            {
//                if (plr.CbtInterface.IsPvp)
//                    plr.SendClientMessage($"{returner.Name} successfully returned the supplies!", returner.Realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE);
//            }

//            ResourceBox box = (ResourceBox) buff.HeldObject;

//            float resourceValue;
//            if (Constants.DoomsdaySwitch == 2)
//                resourceValue = ((ProximityBattleFront)Region.Campaign).GetResourceValue(returner.Realm, _resourceValueMax[Rank]);
//            else
//                resourceValue = ((Campaign)Region.Campaign).GetResourceValue(returner.Realm, _resourceValueMax[Rank]);

//            float distFactor = sender.GetDistanceToObject(box.Objective) / 2000f;

//#if DEBUG
//            //resourceValue *= 50;
//#endif
//            //resourceValue *= 50;

//            if (!sender.ObjectWithinRadiusFeet(this, 100))
//            {
//                returner.SendClientMessage("As you have returned supplies to the warcamp, they are worth 50% less.", ChatLogFilters.CHATLOGFILTERS_RVR);
//                distFactor *= 0.5f;
//            }

//            resourceValue *= distFactor;

//            Item_Info medallionInfo = ItemService.GetItem_Info((uint)(208398 + Tier));
//            ushort medallionCount = (ushort)Clamp(resourceValue/35, 1, 4);
//            uint renownCount = (uint) (resourceValue * 10 + Tier*50*GetDistanceToObject(box.Objective)/2000f);

//            returner.AddXp(renownCount * 5, false, false);
//            returner.AddRenown(renownCount, false);
//            Region.Campaign.AddContribution(returner, renownCount);

//            if (returner.ItmInterface.CreateItem(medallionInfo, medallionCount) == ItemResult.RESULT_OK)
//                returner.SendLocalizeString(new[] { medallionInfo.Name, medallionCount.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);

//            if (returner.WorldGroup != null)
//            {
//                List<Player> members = returner.WorldGroup.GetPlayersCloseTo(returner, 300);

//                foreach (Player player in members)
//                {
//                    if (player != returner && player.IsWithinRadiusFeet(returner, 300))
//                    {
//                        player.AddXp((uint)resourceValue * 50, false, false);
//                        player.AddRenown((uint)resourceValue * 10, false);
//                        Region.Campaign.AddContribution(player, (uint)resourceValue * 10);

//                        if (player.ItmInterface.CreateItem(medallionInfo, medallionCount) == ItemResult.RESULT_OK)
//                            player.SendLocalizeString(new[] { medallionInfo.Name, medallionCount.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
//                    }
//                }
//            }

//            // Reload siege weapons
//            //ReloadSiege();

//            // Intercept to check if any doors need repairing
//            foreach (KeepDoor door in Doors)
//            {
//                if (door.GameObject.PctHealth < 100 && !door.GameObject.IsDead)
//                {
//                    // One box heals 30%.
//                    float healCapability = Math.Min(door.GameObject.MaxHealth * 0.5f, distFactor * 0.5f * door.GameObject.MaxHealth);
                    
//                    uint currentDamage = door.GameObject.MaxHealth - door.GameObject.Health;

//                    float consumeFactor = Math.Min(1f, currentDamage/healCapability);

//                    door.GameObject.ReceiveHeal(returner, (uint)healCapability);

//                    Region.Campaign.Broadcast($"{returner.Name} has repaired {Info.Name}'s keep door by {(int)(healCapability / door.GameObject.MaxHealth * 100)}%!", Realm);

//                    resourceValue *= 1f - consumeFactor;

//                    // If resources are not stored in keep, keep continues to derank
//                    if (consumeFactor == 1f)
//                    {
//                        returner.SendClientMessage("You expended all of your resources to repair the keep door.", ChatLogFilters.CHATLOGFILTERS_RVR);
//                        buff.HeldObject.ResetTo(EHeldState.Inactive);
//                        SendKeepStatus(null);
//                        return;
//                    }

//                    returner.SendClientMessage("You expended part of your resources to repair the keep door.", ChatLogFilters.CHATLOGFILTERS_RVR);
//                }
//            }

//            if (Rank < 6 && _currentResource + resourceValue >= _maxResource)
//            {
//                if (CanSustainRank(Rank + 1))
//                {
//                    resourceValue -= _maxResource - _currentResource;

//                    if (Rank < 5)
//                        ++Rank;
//                    SetSupplyRequirement();
//                    _currentResource = resourceValue;
//                    Region.Campaign.Broadcast($"{Info.Name} is now Rank {Rank}!", Realm);
//                    if (Rank == 1)
//                    {
//                        if (LastMessage >= KeepMessage.Inner0)
//                            Region.Campaign.Broadcast($"{Info.Name}'s postern doors are barred once again!", Realm);
//                        else if (LastMessage >= KeepMessage.Outer0)
//                            Region.Campaign.Broadcast($"{Info.Name}'s outer postern doors are barred once again!", Realm);
//                    }
//                }

//                else
//                {
//                    _currentResource = _maxResource - 1;
//                    returner.SendClientMessage("More warriors of your realm are required to reach the next Keep rank!", ChatLogFilters.CHATLOGFILTERS_RVR);
//                }

//                // We notify everyone that this keep is Rank 1
//                if (Rank == 1)
//                {
//                    foreach (Player player in Player._Players)
//                    {
//                        if (player.Region.GetTier() > 1 && player.ValidInTier(Tier, true) && !InformRankOne && player.Region.RegionId != Region.RegionId)
//                        {
//                            player.SendLocalizeString($"{Info.Name} keep in {Zone.Info.Name} just reached Rank 1!", ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
//                        }
//                    }
//                    InformRankOne = true;
//                }
//            }

//            else
//                _currentResource += resourceValue;

//            _currentResourcePercent = (byte) (_currentResource/_maxResource*100f);

//#if DEBUG
//            if (Constants.DoomsdaySwitch == 2)
//                returner.SendClientMessage($"Resource worth {resourceValue} ({((ProximityBattleFront)Region.Campaign).GetResourceValue(returner.Realm, _resourceValueMax[Rank])} base * {GetDistanceToObject(box.Objective) / 2000f} dist factor) returned!");
//            else
//                returner.SendClientMessage($"Resource worth {resourceValue} ({((Campaign)Region.Campaign).GetResourceValue(returner.Realm, _resourceValueMax[Rank])} base * {GetDistanceToObject(box.Objective) / 2000f} dist factor) returned!");

//            returner.SendClientMessage($"Resources: {_currentResource}/{_maxResource} ({_currentResourcePercent}%)");
//#endif

//            buff.HeldObject.ResetTo(EHeldState.Inactive);
//            _lastReturnSeconds = TCPManager.GetTimeStamp();
//            EvtInterface.AddEvent(UpdateStateOfTheRealmKeep,100,1);
//            SendKeepStatus(null);
//        }

        private int _nextDoorWarnTime;
        private int _nextDegenerationWarnTime;

        //private readonly int[] _rankDecayTimer = {480, 300, 300, 240, 180, 120};
        private readonly int[] _rankDecayTimer = { 60, 60, 60, 60, 60, 40 };

        // TODO - fix Keep status.
   //     public void TickUpkeep()
   //     {
   //         // We are ticking Realm Rank in sync here
   //         Region.Campaign.TickRealmRankTimer();

   //         if (Rank == 0 && _currentResource == 0)
   //             return;

   //         int curTime = TCPManager.GetTimeStamp();

   //         // Sustain keep if resources were returned within last 10 minutes and enough players exist to support the rank
   //         ProximityBattleFront front = (ProximityBattleFront)Region.Campaign;
   //         if (_lastReturnSeconds + _rankDecayTimer[Rank] > curTime && CanSustainRank(Rank) && front.HeldObjectives[(int)Realm] > WorldMgr.WorldSettingsMgr.GetGenericSetting(9))
   //             return;

   //         if (_nextDegenerationWarnTime < curTime)
   //         {
   //             _nextDegenerationWarnTime = curTime + 5000;
   //             // codeword 0ni0n
   //             //Region.Campaign.Broadcast(CanSustainRank(Rank) ? $"{Info.Name} is not meeting its supply upkeep!" : $"Not enough warriors are present to sustain {Info.Name}'s current rank!", Realm);
   //         }

   //         // Degeneration for failing to supply keep or for failing numbers threshold.
   //         // Loss of 10% of rank per minute -> rank loss every 10 minutes
   //         float rankLoss = _maxResource * 0.1f;
   //         // Changed to 50% per minute
   //         rankLoss = _maxResource * (float)WorldMgr.WorldSettingsMgr.GetGenericSetting(8) / 10.0f;
   //         if (rankLoss > _currentResource)
   //         {
   //             // Keeps Rank 3 or less do not derank
   //             //if (Rank == 0)
   //             if (Rank < 4)
   //                 _currentResource = 0;
   //             else
   //             {
   //                 --Rank;
   //                 SetSupplyRequirement();
   //                 _currentResource = _maxResource*0.95f;
   //                 Region.Campaign.Broadcast($"{Info.Name}'s rank has fallen to {Rank}!", Realm);
   //                 if (Rank == 0)
   //                 {
   //                     if (LastMessage >= KeepMessage.Inner0)
   //                         Region.Campaign.Broadcast($"{Info.Name}'s postern doors are no longer defended!", Realm);
   //                     else if (LastMessage >= KeepMessage.Outer0)
   //                         Region.Campaign.Broadcast($"{Info.Name}'s outer postern doors are no longer defended!", Realm);
   //                 }
   //             }
   //         }

			//else
			//	_currentResource -= rankLoss;

   //         _currentResourcePercent = (byte)(_currentResource / _maxResource * 100f);

   //         EvtInterface.AddEvent(UpdateStateOfTheRealmKeep,100,1);

   //         SendKeepStatus(null);
   //     }

//        public bool CanSustainRank(int rank)
//        {
//#if DEBUG
//            return true;
//#endif
//            if (Constants.DoomsdaySwitch == 2)
//            {
//                //return true;
//            }
//            if (rank == 0)
//                return true;
//            if (rank > 5)
//                rank = 5;

//            return Region.Campaign.CanSustainRank(Realm, _resourceValueMax[rank]);
//        }

//        public bool ShouldRation()
//        {
//            return !CanSustainRank(Rank);
//        }

//        public float GetRationFactor()
//        {
//            float rationFactor = 0.25f + Rank * 0.15f;

//            if (CanSustainRank(Rank))
//                rationFactor *= 2f;

//            return Math.Min(rationFactor, 1f);
//        }

#endregion

#region Materiel

        private enum MaterielType
        {
            Barricade,
            Artillery,
            Cannon,
            Ram,
            MaxMateriel
        }

        private readonly string[] _materielMessageNames = {"palisade", "artillery piece", "cannon", "ram"};
        private readonly float[] _materielSupply = new float[4];
        private readonly List<Siege>[] _activeMateriel = {new List<Siege>(), new List<Siege>(), new List<Siege>(), new List<Siege>()};

        private readonly int[][] _materielCaps =
        {
            new[] { 0, 2, 4, 6, 8, 10 }, // barricades
            new[] { 1, 2, 3, 4, 5, 6 }, // artillery
            new[] { 0, 2, 3, 5, 6, 8 }, // cannon
            new[] { 0, 1, 1, 1, 2, 3 } // ram
        };

        private readonly int[][] _materielRegenTime =
        {
            new[] { 5, 5, 4, 3, 2, 2 }, // barricades
            new[] { 3, 3, 3, 3, 2, 1 }, // artillery
            new[] { 4, 3, 3, 3, 2, 1 }, // cannon
            new[] { 15, 15, 10, 7, 5, 3 } // ram
        };

        public void UpdateResources()
        {
            if (KeepStatus != KeepStatus.KEEPSTATUS_LOCKED)
            {
                for (int i = 0; i < (int) MaterielType.MaxMateriel; ++i)
                {
                    int prevSupply = (int) _materielSupply[i];

                    if (_materielCaps[i][Rank] == 0)
                    {
                        if (_activeMateriel[i].Count == 0 && _materielSupply[i] < 0.9f)
                        {
                            if (KeepStatus == KeepStatus.KEEPSTATUS_SAFE)
                                _materielSupply[i] = Math.Min(0.9f, _materielSupply[i] + 1f/_materielRegenTime[i][Rank]);
                            else _materielSupply[i] = Math.Min(0.9f, _materielSupply[i] + 0.1f/_materielRegenTime[i][Rank]);
                        }
                    }

                    else if (_materielSupply[i] + _activeMateriel[i].Count < _materielCaps[i][Rank])
                    {
                        if (KeepStatus == KeepStatus.KEEPSTATUS_SAFE)
                            _materielSupply[i] += 1f/_materielRegenTime[i][Rank];
                        else _materielSupply[i] += 0.1f/_materielRegenTime[i][Rank];
                    }

                    int curSupply = (int) _materielSupply[i];

                    if (i > 0 && curSupply > prevSupply)
                    {
                        string message;
                        
                        if (curSupply == 1)
                            message = $"{(i == 1 ? "An" : "A")} {_materielMessageNames[i]} is now available from {Info.Name}!";
                        else
                            message = $"{curSupply} {_materielMessageNames[i]}s are now available from {Info.Name}!";

                        //ChatLogFilters desiredFilter = Realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE;

                        foreach (Player player in Region.Players)
                        {
                            if (player.CbtInterface.IsPvp && player.Realm == Realm)
                            {
                                //player.SendClientMessage(message, desiredFilter);
                                player.SendClientMessage(message, ChatLogFilters.CHATLOGFILTERS_RVR);
                            }
                        }
                    }
                }
            }

            if (_safeKeepTimer > 0 && _safeKeepTimer < TCPManager.GetTimeStamp())
            {
                if (KeepStatus != KeepStatus.KEEPSTATUS_SEIZED)
                    TickSafety();
                // No more keep reclaiming, sorry...
                /*else if (Region.Campaign.CanReclaimKeep((Realms)Info.Realm))
                    ReclaimKeep();*/
            }

            // TODO - NEW DAWN status of Keep
            // TickUpkeep();
        }

        public void ReloadSiege()
        {
            if (_activeMateriel[(int)MaterielType.Artillery].Count > 0)
                foreach (Siege siege in _activeMateriel[(int) MaterielType.Artillery])
                    siege.AddShots((int)(Siege.MAX_SHOTS * 0.65f * GetSiegeDamageMod(SiegeType.GTAOE)));

            if (_activeMateriel[(int)MaterielType.Cannon].Count > 0)
                foreach (Siege siege in _activeMateriel[(int)MaterielType.Cannon])
                    siege.AddShots((int)(Siege.MAX_SHOTS * 0.65f * GetSiegeDamageMod(SiegeType.SNIPER)));
        }

        public void ProximityReloadSiege(int ammo)
        {
            if (_activeMateriel[(int)MaterielType.Artillery].Count > 0)
                foreach (Siege siege in _activeMateriel[(int)MaterielType.Artillery])
                    siege.AddShots(ammo);

            if (_activeMateriel[(int)MaterielType.Cannon].Count > 0)
                foreach (Siege siege in _activeMateriel[(int)MaterielType.Cannon])
                    siege.AddShots(ammo);
        }

        private void TickSafety()
        {
            int doorReplacementCost = 0;

            //Check doors. If any door is down, it needs to be regenerated before we can declare safe.
            foreach (KeepDoor door in Doors)
            {
                if (door.GameObject.IsDead)
                    doorReplacementCost += Math.Min(_resourceValueMax[Rank] * 4, (int)(_maxResource * 0.35f));
                else if (door.GameObject.PctHealth < 100)
                    doorReplacementCost += (int)(Math.Min(_resourceValueMax[Rank] * 4, (int)(_maxResource * 0.35f)) * (1f - door.GameObject.PctHealth * 0.01f));
            }

            if (doorReplacementCost > 0)
            {
                if (_currentResource < doorReplacementCost)
                {
                    if (_nextDoorWarnTime < TCPManager.GetTimeStamp())
                    {
                        _nextDoorWarnTime = TCPManager.GetTimeStamp() + 300;

                        foreach (Player player in Region.Players)
                        {
                            if (player.Realm == Realm && player.CbtInterface.IsPvp)
                                player.SendClientMessage(Info.Name + " requires supplies to repair the fallen doors!", ChatLogFilters.CHATLOGFILTERS_RVR);
                        }
                    }
                }
                else
                {
                    _currentResource -= doorReplacementCost;

                    _currentResourcePercent = (byte)(_currentResource / _maxResource * 100f);

                    foreach (KeepDoor door in Doors)
                    {
                        if (door.GameObject.IsDead)
                            door.Spawn();
                    }

                    SafeKeep();

                    _safeKeepTimer = 0;
                }
            }
            else
            {
                SafeKeep();

                _safeKeepTimer = 0;
            }
        }

        private void ReclaimKeep()
        {
            Region.Campaign.CommunicationsEngine.Broadcast($"{Info.Name} has been reclaimed by the forces of {(Info.Realm == 1 ? "Order" : "Destruction")}!", Tier);

            Realm = (Realms) Info.Realm;

            foreach (KeepNpcCreature crea in Creatures)
                crea.SpawnGuard(Realm);

            foreach (KeepDoor door in Doors)
                door.Spawn();

            UpdateKeepStatus(KeepStatus.KEEPSTATUS_SAFE);

            LastMessage = KeepMessage.Safe;
        }

#endregion

        /*public bool AttackerCanUsePostern(int posternNum)
        {
            if (Rank > 0)
                return false;

            if (posternNum == (int) KeepDoorType.OuterPostern)
                return LastMessage >= KeepMessage.Outer0;
            return LastMessage >= KeepMessage.Inner0;
        }*/

        public bool AttackerCanUsePostern(int posternNum)
        {
            if (Ruin)
                return true;

            // Keeps rank 4 or 5 have unaccessible posterns for attackers, even after main gate falls
            if (Rank > 3)
                return false;

            if (posternNum == (int) KeepDoorType.OuterPostern)
                return LastMessage >= KeepMessage.Outer0 && Rank < 4;
            return LastMessage >= KeepMessage.Inner0 && Rank < 3;
        }

#region Util

        private string GetAttackerMessage()
        {
            switch (KeepStatus)
            {
                case KeepStatus.KEEPSTATUS_INNER_SANCTUM_UNDER_ATTACK:
                    return "Destroy the Sanctum Door";
                case KeepStatus.KEEPSTATUS_KEEP_LORD_UNDER_ATTACK:
                    return "Kill the Keep Lord";
                case KeepStatus.KEEPSTATUS_SEIZED:
                    return "Hold the Battlefield Objectives";
                default:
                    return "Destroy the Outer Door";

            }
        }

        private string GetObjectiveMessage(Player plr)
        {
            switch (KeepStatus)
            {
                case KeepStatus.KEEPSTATUS_SEIZED:
                    return "Keep Has Fallen";
                case KeepStatus.KEEPSTATUS_LOCKED:
                    return "Keep Locked";
                case KeepStatus.KEEPSTATUS_SAFE:
                case KeepStatus.KEEPSTATUS_OUTER_WALLS_UNDER_ATTACK:
                    return plr.Realm == Realm ? "Defend Outer Door" : "Destroy Outer Door";
                case KeepStatus.KEEPSTATUS_INNER_SANCTUM_UNDER_ATTACK:
                    return plr.Realm == Realm ? "Defend Sanctum Door" : "Destroy Sanctum Door";
                case KeepStatus.KEEPSTATUS_KEEP_LORD_UNDER_ATTACK:
                    return plr.Realm == Realm ? "Defend Keep Lord" : "Kill Keep Lord";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string GetObjectiveDescription(Realms realm)
        {
            switch (KeepStatus)
            {
                case KeepStatus.KEEPSTATUS_SAFE:
                case KeepStatus.KEEPSTATUS_OUTER_WALLS_UNDER_ATTACK:
                    return $"The Outer Door of {Info.Name} stands strong!";
                case KeepStatus.KEEPSTATUS_INNER_SANCTUM_UNDER_ATTACK:
                    return $"The Sanctum Door of {Info.Name} stands strong!";
                case KeepStatus.KEEPSTATUS_KEEP_LORD_UNDER_ATTACK:
                    return $"The Keep Lord of {Info.Name} stands strong!";
                case KeepStatus.KEEPSTATUS_SEIZED:
                    if (realm == Realm)
                        return $"The Keep Lord of {Info.Name} has been defeated! Maintain overall control of the Battlefield Objectives to lock this zone!";
                    return $"The Keep Lord of {Info.Name} has been defeated, granting the enemy control of this keep for 45 minutes! Hold 50% of the Victory Points to reclaim this keep after the time has expired!";
                case KeepStatus.KEEPSTATUS_LOCKED:
                    return $"{Info.Name} is not attackable at the moment!";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

#endregion

#region Senders

        public void SendKeepInfo(Player plr)
        {
            PacketOut Out = new PacketOut((byte) Opcodes.F_OBJECTIVE_INFO, 32);
            Out.WriteUInt32(Info.PQuestId);
            Out.WriteByte(0);
            Out.WriteByte((byte) Realm);
            Out.WriteByte(1);
            Out.WriteUInt16(0);
            if (Ruin)
                Out.WritePascalString("Ruins of " + Info.Name);
            else
                Out.WritePascalString(Info.Name);
            Out.WriteByte(2);
            Out.WriteUInt32(0x000039F5);
            Out.WriteByte(0);

            // Expansion for objective goal
            if (plr.Realm != Realm && KeepStatus != KeepStatus.KEEPSTATUS_LOCKED)
            {
                Out.WriteUInt16(0x0100);
                Out.WriteUInt32(0x00010000);

                Out.WriteShortString(GetAttackerMessage());
            }

            else Out.WriteByte(0);

            Out.WriteUInt16(0xFF00);
            Out.WritePascalString(GetObjectiveMessage(plr));
            Out.WriteByte(0);

            Out.WritePascalString(GetObjectiveDescription(plr.Realm));

            Out.WriteUInt32(0); // timer
            Out.WriteUInt32(0); // timer
            Out.Fill(0, 4);
            Out.WriteByte(0x71);
            Out.WriteByte(3); // keep
            Out.Fill(0, 3);

            plr.SendPacket(Out);
        }

        public void SendKeepInfoLeft(Player plr)
        {
            PacketOut Out = new PacketOut((byte) Opcodes.F_OBJECTIVE_UPDATE, 8);
            Out.WriteUInt32(Info.PQuestId);
            Out.WriteUInt32(0);
            plr.SendPacket(Out);
        }

        public void SendRegionMessage(string message)
        {
            foreach (Object obj in Region.Objects)
            {
                Player plr = obj as Player;
                plr?.SendLocalizeString(message, ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
                plr?.SendLocalizeString(message, Realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE, Localized_text.CHAT_TAG_DEFAULT);
            }
        }

        public void SendKeepStatus(Player plr)
        {
            if (Region == null)
                return;

            List<KeepDoor> doors = Doors.FindAll(x => x.Info.Number != (int)KeepDoorType.None && x.Info.GameObjectId == 100 && x.GameObject.PctHealth > 0).OrderByDescending(x => x.Info.Number).ToList();

            PacketOut Out = new PacketOut((byte) Opcodes.F_KEEP_STATUS, 26);
            Out.WriteByte(Info.KeepId);

            if (Ruin)
            {
                Out.WriteByte(2); // Keep Status
                Out.WriteByte(0); // ?
                //Out.WriteByte((byte)Realm);
                Out.WriteByte((byte)Realm);
                Out.WriteByte(0); // Number of doors
                Out.WriteByte(0); // Rank
                Out.WriteByte(0); // Door Health
                Out.WriteByte(0); // Next rank %
            }
            else
            {
                Out.WriteByte(KeepStatus == KeepStatus.KEEPSTATUS_LOCKED ? (byte)1 : (byte)KeepStatus);
                Out.WriteByte(0); // ?
                Out.WriteByte((byte)Realm);
                Out.WriteByte((byte) doors.Count);
                Out.WriteByte(Rank); // Rank
                if (doors.Count > 0)
                    Out.WriteByte(doors.First().GameObject.PctHealth); // Door health
                else
                    Out.WriteByte(0);
                Out.WriteByte(_currentResourcePercent); // Next rank %
            }

            
            Out.Fill(0, 18);

            if (plr != null)
                plr.SendPacket(Out);
            else
            {
                lock (Player._Players)
                    foreach (Player player in Player._Players)
                        player.SendCopy(Out);
            }
        }

        //reconstructed packet from client dissasmbly
        //player must have role of SystemData.GuildPermissons.KEEPUPGRADE_EDIT 
        //for detailed UI logic, look at interface/default/ea_interactionwindow/source/interactionkeepupgrades.lua
        public void SendKeepUpgradesInteract(Player plr)
        {
            PacketOut Out = new PacketOut((byte) Opcodes.F_INTERACT_RESPONSE, 1000);
            Out.WriteByte(0x1E);

            Random r = new Random();

            int availibleUpgradeCount = 10;

            Out.WriteByte((byte) availibleUpgradeCount); //total upgrades
            Out.WriteUInt32(10); //current upkeep cost
            Out.WriteUInt32(4); //keep id from data/strings/english/keepnames.txt

            int col = 0;
            int row = 0;

            int keepRankCount = 5;
            int unkCount = 5;


            for (int i = 0; i < availibleUpgradeCount; i++)
            {
                Out.WriteUInt32((uint) (i + 3)); //id from data/strings/english/keepupgradenames.txt

                Out.WriteByte((byte) r.Next(10)); //status fom GameData.KeepUpgradeStatus
                Out.WriteByte((byte) r.Next(5)); //currentLevel
                Out.WriteByte((byte) r.Next(5)); //target level

                Out.WriteByte((byte) keepRankCount); //keep rank pricing count
                Out.WriteUInt32(100); //time

                for (int x = 0; x < keepRankCount; x++)
                {
                    Out.WriteByte((byte) r.Next(25)); //duration
                    Out.WriteByte(0); //min guild rank
                    Out.WriteByte((byte) r.Next(5)); //unk level?
                    Out.WriteUInt32((uint) r.Next(10000)); //gold per minute
                }

                Out.WriteByte((byte) col); //col
                Out.WriteByte((byte) row); //row

                Out.WriteByte((byte) unkCount); //count

                col++;
                if (col > 0)
                {
                    col = 0;
                    row++;
                }

                for (int a = 0; a < unkCount; a++)
                    Out.WriteByte((byte) a);
            }

            if (plr != null)
                plr.SendPacket(Out);
        }

#endregion

#region Zone Locking

        public void LockKeep(Realms lockingRealm, bool announce, bool reset)
        {
            _logger.Debug($"Locking Keep {Name} for {lockingRealm.ToString()}");

            _safeKeepTimer = 0;

            Rank = 0;
            Ruin = false;
            _currentResource = 0;
            //SetSupplyRequirement();

            // RA - Currently dont do anything with lockingRealm.

            if (reset)
            {
                Realm = (Realms) Info.Realm;
                KeepStatus = KeepStatus.KEEPSTATUS_LOCKED;
            }
            else
            {
                Realm = lockingRealm;
                KeepStatus = KeepStatus.KEEPSTATUS_LOCKED;
            }

            if (KeepStatus != KeepStatus.KEEPSTATUS_SEIZED)
            {
                foreach (KeepNpcCreature crea in Creatures)
                    crea.SpawnGuard(Realm);
            }

            foreach (KeepDoor door in Doors)
                door.Spawn();

            UpdateKeepStatus(KeepStatus.KEEPSTATUS_LOCKED);

            EvtInterface.RemoveEvent(UpdateResources);

            LastMessage = KeepMessage.Safe;
        }

        public void NotifyPairingUnlocked()
        {
            // Default was 15, changed to 10
            int _keepLockTime = (10*60*1000);

#if (DEBUG)
            _keepLockTime = (2*60*1000);
#endif

            Realms unlockedRealm = GetContestedRealm();

            if (unlockedRealm != Realm)
            {
                Realm = unlockedRealm;

                foreach (KeepNpcCreature crea in Creatures)
                    crea.SpawnGuard(Realm);

                foreach (KeepDoor door in Doors)
                    door.Spawn();
            }

            UpdateKeepStatus(KeepStatus.KEEPSTATUS_LOCKED);

            Ruin = false;

            EvtInterface.RemoveEvent(UpdateResources);

            LastMessage = KeepMessage.Safe;

            EvtInterface.AddEvent(ReopenKeep, _keepLockTime, 1);

            SendKeepStatus(null);
        }

        public Realms GetContestedRealm()
        {
            switch (Info.KeepId)
            {
                case 6:  // Kazad Dammaz
                case 15: // Wilhelm's Fist
                case 26: // Pillars of Remembrance
                    Log.Info(Info.Name, "Overriding control of this keep towards Destruction");
                    return Realms.REALMS_REALM_DESTRUCTION;
                case 9: // Ironskin Skar
                case 20: // Charon's Citadel
                case 30: // Wrath's Resolve
                    Log.Info(Info.Name, "Overriding control of this keep towards Order");
                    return Realms.REALMS_REALM_ORDER;
                default:
                    return (Realms)Info.Realm;
            }
        }

        public void ReopenKeep()
        {
            UpdateKeepStatus(KeepStatus.KEEPSTATUS_SAFE);

            InformRankOne = false;

            EvtInterface.AddEvent(UpdateResources, 60000, 0);

            foreach (KeepDoor door in Doors)
            {
                door.GameObject.SetAttackable(true);
            }

            foreach (Object obj in Region.Objects)
            {
                Player plr = obj as Player;

                if (plr == null || !plr.ValidInTier(Tier, true))
                    continue;

                plr.SendLocalizeString(Info.Name + " is now open for capture!", ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
                plr.SendLocalizeString(Info.Name + " is now open for capture!", Realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE, Localized_text.CHAT_TAG_DEFAULT);
            }
        }

#endregion

#region Siege Weapon Management

        public void SpawnOil(Player player, ushort slot)
        {
            if ((Realms) player.Info.Realm != Realm)
            {
                player.SendClientMessage("Can't deploy oil at hostile keep", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                player.SendClientMessage("You cannot deploy oil at a keep you do not own.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            if (Ruin)
            {
                player.SendClientMessage("Can't deploy oil at ruined keep", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                player.SendClientMessage("You cannot deploy oil at a keep that is ruined.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            uint entry = player.ItmInterface.GetItemInSlot(slot).Info.Entry;

            if (Constants.DoomsdaySwitch == 0)
            { 
                if ((entry == 86215 || entry == 86203) && Info.PQuest.PQTier != 2)
                    return;
                if ((entry == 86219 || entry == 86207) && Info.PQuest.PQTier != 3)
                    return;
                if ((entry == 86223 || entry == 86211) && Info.PQuest.PQTier != 4)
                    return;
            }
            // Disabling T2 and T3 oils here
            else
            {
                if (entry == 86215 || entry == 86203 || entry == 86219 || entry == 86207)
                    return;
            }

            foreach (Hardpoint h in _hardpoints)
            {
                if (h.SiegeType != SiegeType.OIL || !player.PointWithinRadiusFeet(h, 10))
                    continue;

                if (h.CurrentWeapon != null)
                {
                    player.SendClientMessage("Can't deploy oil yet", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                    player.SendClientMessage("The oil is blocked by another oil, or the previous oil was destroyed too recently.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    return;
                }

                Creature_proto proto = CreatureService.GetCreatureProto(GetOilProto(player.Realm));

                Creature_spawn spawn = null;
                if (Constants.DoomsdaySwitch == 0)
                {
                    spawn = new Creature_spawn
                    {
                        Guid = (uint)CreatureService.GenerateCreatureSpawnGUID(),
                        Level = (byte)(Info.PQuest.PQTier * 10),
                        ZoneId = Info.ZoneId,
                        WorldX = h.X,
                        WorldY = h.Y,
                        WorldZ = h.Z,
                        WorldO = h.Heading,
                    };
                }
                else
                {
                    spawn = new Creature_spawn
                    {
                        Guid = (uint)CreatureService.GenerateCreatureSpawnGUID(),
                        Level = 40,
                        ZoneId = Info.ZoneId,
                        WorldX = h.X,
                        WorldY = h.Y,
                        WorldZ = h.Z,
                        WorldO = h.Heading,
                    };
                }

                spawn.BuildFromProto(proto);

                h.CurrentWeapon = new Siege(spawn, player, this, SiegeType.OIL);
                Region.AddObject(h.CurrentWeapon, spawn.ZoneId);

                player.ItmInterface.DeleteItem(slot, 1);
                return;
            }

            player.SendClientMessage("Can't deploy oil here", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
            player.SendClientMessage("This is not a good place to deploy the oil. Move to a better location.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
        }

        public bool CanDeploySiege(Player player, int level, uint protoEntry)
        {
#if DEBUG
            return true;
#endif
            if (Constants.DoomsdaySwitch == 0)
            { 
                if (level / 10 != Tier)
                {
                    player.SendClientMessage("Invalid weapon tier", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                    player.SendClientMessage("This weapon is not of the correct tier.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    return false;
                }
            }

            if (!CheckDist(player))
            {
                player.SendClientMessage("Too close to other weapon or deploy point", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                player.SendClientMessage("This position is too close to another siege weapon or spawn.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return false;
            }

            Creature_proto siegeProto = CreatureService.GetCreatureProto(protoEntry);

            if (siegeProto == null)
                return false;

            int type;

            switch ((CreatureSubTypes)siegeProto.CreatureSubType)
            {
                case CreatureSubTypes.SIEGE_GTAOE:
                    type = (int)MaterielType.Artillery;
                    if ((int)_materielSupply[type] < 1f || _activeMateriel[type].Count >= _materielCaps[type][Rank])
                    {
                        player.SendClientMessage("No artillery available", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                        player.SendClientMessage("Your supply lines cannot support any more artillery pieces.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                        return false;
                    }
                    break;
                case CreatureSubTypes.SIEGE_SINGLE_TARGET:
                    type = (int)MaterielType.Cannon;
                    if ((int)_materielSupply[type] < 1f || _activeMateriel[type].Count >= _materielCaps[type][Rank])
                    {
                        player.SendClientMessage("No cannon available", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                        player.SendClientMessage("Your supply lines cannot support any more cannon.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                        return false;
                    }
                    break;
                case CreatureSubTypes.SIEGE_RAM:
                    type = (int) MaterielType.Ram;
                    /*if (KeepStatus != KeepStatus.KEEPSTATUS_SAFE)
                    {
                        player.SendClientMessage("Unsafe keep", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                        player.SendClientMessage("You cannot deploy a ram at a keep that is unsafe.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                        return false;
                    }*/
                    if ((int)_materielSupply[type] < 1f || _activeMateriel[type].Count >= _materielCaps[type][Rank])
                    {
                        player.SendClientMessage("No rams available", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                        player.SendClientMessage("Your supply lines cannot support any more rams.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                        return false;
                    }
#if !DEBUG
                    if (player.GldInterface.Guild == null || (Tier == 4 && player.GldInterface.Guild.Info.Level < 20))
                    {
                        player.SendClientMessage(Tier == 4 ? "Must be in guild of rank 20" : "Must be in guild", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                        player.SendClientMessage($"In order to deploy a ram, you must be in a { (Tier == 4 ? "reputable guild of rank 20 or higher." : "guild.") }", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                        return false;
                    }
#endif
                    break;
                default:
                    return true;
            }

            return true;
        }

        public void SpawnSiegeWeapon(Player player, uint protoEntry)
        {
            if (Constants.DoomsdaySwitch == 0)
                protoEntry += (uint)(4 * (Tier - 2));
            else
                protoEntry += 8;

            Creature_proto siegeProto = CreatureService.GetCreatureProto(protoEntry);

            if (siegeProto == null)
                return;

            int type;

            switch ((CreatureSubTypes)siegeProto.CreatureSubType)
            {
                case CreatureSubTypes.SIEGE_GTAOE:
                    type = (int)MaterielType.Artillery;
                    break;
                case CreatureSubTypes.SIEGE_SINGLE_TARGET:
                    type = (int)MaterielType.Cannon;
                    break;
                case CreatureSubTypes.SIEGE_RAM:
                    type = (int) MaterielType.Ram;

                    if (player.GldInterface.Guild != null)
                    {
                        string message = $"{player.Name} of {player.GldInterface.Guild.Info.Name} has deployed a ram at {Info.Name}!";
                        ChatLogFilters filter = player.Realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE;
                        foreach (Player plr in Region.Players)
                            if (plr.CbtInterface.IsPvp && plr.ValidInTier(Region.GetTier(), true) && plr.Realm == player.Realm)
                            {
                                plr.SendClientMessage(message, filter);
                                plr.SendClientMessage(message, ChatLogFilters.CHATLOGFILTERS_RVR);
                            }
                    }
                    break;
                default:
                    return;
            }

            Siege siege = Siege.SpawnSiegeWeapon(player, this, protoEntry, true);
            _activeMateriel[type].Add(siege);
            Region.AddObject(siege, Info.ZoneId);
            _materielSupply[type] -= 1f;
        }

        public void RemoveKeepSiege(Siege weapon)
        {
            foreach (Hardpoint h in _hardpoints)
            {
                if (h.CurrentWeapon == weapon)
                {
                    h.CurrentWeapon = null;
                    return;
                }
            }
        }

        public void RemoveSiege(Siege weapon)
        {
            switch ((CreatureSubTypes) weapon.Spawn.Proto.CreatureSubType)
            {
                case CreatureSubTypes.SIEGE_GTAOE:
                    _activeMateriel[(int) MaterielType.Artillery].Remove(weapon);
                    break;
                case CreatureSubTypes.SIEGE_SINGLE_TARGET:
                    _activeMateriel[(int)MaterielType.Cannon].Remove(weapon);
                    break;
                case CreatureSubTypes.SIEGE_RAM:
                    string message = $"{weapon.SiegeInterface.Creator.Name}'s ram has been destroyed!";
                    ChatLogFilters filter = Realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE;
                    foreach (Player plr in Region.Players)
                        if (plr.CbtInterface.IsPvp && plr.ValidInTier(Region.GetTier(), true) && plr.Realm == Realm)
                        {
                            plr.SendClientMessage(message, filter);
                            plr.SendClientMessage(message, ChatLogFilters.CHATLOGFILTERS_RVR);
                        }
                    _activeMateriel[(int)MaterielType.Ram].Remove(weapon);

                    foreach (Hardpoint h in _hardpoints)
                        if (weapon == h.CurrentWeapon)
                        {
                            h.CurrentWeapon = null;
                            RamDeployed = false;
                        }
                    break;
            }
        }

        public void TryAlignRam(Object owner, Player player)
        {
            Point3D hardPos = new Point3D();
            foreach (Hardpoint h in _hardpoints)
            {
                if (h.SiegeType != SiegeType.RAM)
                    continue;
                if (h.SiegeType == SiegeType.RAM && h.CurrentWeapon != null)
                { 
                    player.SendClientMessage("You cannot deploy another ram!", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    continue;
                }

                hardPos.X = h.X;
                hardPos.Y = h.Y;
                hardPos.Z = h.Z;

                if (!owner.WorldPosition.IsWithinRadiusFeet(hardPos, 25))
                    continue;

                owner.SetPosition(Zone.CalculPin((uint)hardPos.X, true), Zone.CalculPin((uint)hardPos.Y, false), (ushort)hardPos.Z, h.Heading, Zone.ZoneId);
                h.CurrentWeapon = (Siege)owner;
                RamDeployed = true;
                return;
            }
        }

        public float GetSiegeDamageMod(SiegeType type)
        {
            float siegeCap = _materielCaps[(int)(type == SiegeType.SNIPER ? MaterielType.Cannon : MaterielType.Artillery)][Rank];
            float siegeCount = Math.Max(1, _activeMateriel[(int) (type == SiegeType.SNIPER ? MaterielType.Cannon : MaterielType.Artillery)].Count);

            return Math.Min(1f, siegeCap/siegeCount);
        }

        public bool CheckDist(Player player)
        {
            if (player == null)
                return false;

            // If the keep is ruined we check if we can deploy siege at Warcamp - we beed to be 50 ft from the entrance to do that
            // This need to be redone correctly
            Realm playerRealm;
            if (Ruin)
            {
                foreach (KeyValuePair<ushort, Point3D[]> entry in BattleFrontService._warcampEntrances)
                {
                    if (player.PointWithinRadiusFeet(entry.Value[(int)player.Realm-1],50))
                    {
                        return true;
                    }
                }
                return false;
            }

            // If we are too near to chest I guess we cannot deply it...?
            else if (player.PointWithinRadiusFeet(new Point3D(Info.PQuest.GoldChestWorldX, Info.PQuest.GoldChestWorldY, Info.PQuest.GoldChestWorldZ), 50))
                return false;

            if (player.Realm == Realm)
            {
                foreach (Hardpoint h in _hardpoints)
                {
                    if (player.PointWithinRadiusFeet(h, 40))
                        return false;
                }
            }
            else
            {
                foreach (Hardpoint h in _hardpoints)
                {
                    if (player.PointWithinRadiusFeet(h, 40))
                        return false;
                }
            }
            return true;
        }

        private uint GetOilProto(Realms targetRealm)
        {
            uint baseEntry = 0;

            switch (Info.Race)
            {
                case 1: //dwarf
                case 2: //orc
                    baseEntry = 13406;
                    break;
                case 3: //human
                case 4: //chaos
                    baseEntry = 13418;
                    break;
                case 5: //he
                case 6: //de
                    baseEntry = 13430;
                    break;
            }

            if (targetRealm == Realms.REALMS_REALM_DESTRUCTION)
                baseEntry += 36;

            if (Constants.DoomsdaySwitch == 0)
                return (uint)(baseEntry + (Info.PQuest.PQTier - 2) * 4);
            else
                return (uint)(baseEntry + (4 - 2) * 4);
        }

#endregion

        public void SendDiagnostic(Player plr)
        {
            plr.SendClientMessage($"[{Info.Name}]", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            plr.SendClientMessage($"{Enum.GetName(typeof(KeepStatus), KeepStatus)} and held by {(Realm == Realms.REALMS_REALM_NEUTRAL ? "no realm" : (Realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction"))}");
            plr.SendClientMessage($"Last message sent: {Enum.GetName(typeof(KeepMessage), LastMessage)}");
            //plr.SendClientMessage($"Rank {Rank}, Ration Factor {GetRationFactor()}");

            KeepNpcCreature lord = Creatures.Find(x => x.Info.KeepLord);

            if (lord == null || lord.Creature == null)
                plr.SendClientMessage("NO LORD");
            else if (lord.Creature.IsDead)
                plr.SendClientMessage("LORD DEAD");
            else
            {
                plr.SendClientMessage($"Keep Lord: {lord.Creature.Name}");
                plr.SendClientMessage($"WorldPosition: {lord.Creature.WorldPosition}");
                plr.SendClientMessage($"Distance from spawnpoint: {lord.Creature.WorldPosition.GetDistanceTo(lord.Creature.WorldSpawnPoint)}");
                plr.SendClientMessage($"Health: {lord.Creature.PctHealth}");
                if (_safeKeepTimer > 0)
                    plr.SendClientMessage($"Keep will be safe in {(_safeKeepTimer - TCPManager.GetTimeStamp()) / 60} minutes", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                else
                    plr.SendClientMessage($"Keep is now safe", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                plr.SendClientMessage($"RamDeployed: " + RamDeployed); 
            }
            plr.SendClientMessage($"Ruin: {Ruin}");
        }

        public void AddAllSiege(List<Siege> siege)
        {
            for (int i = 0; i < 4; ++i)
            {
                if (_activeMateriel[i].Count > 0)
                    siege.AddRange(_activeMateriel[i]);
            }
        }

        public void UpdateStateOfTheRealmKeep()
        {
            string keepStatus = "";
            if (this != null && ZoneId != null)
            {
                keepStatus = "SoR_T" + this.Tier + "_Keep_Update:" + this.ZoneId + ":" + this.Info.KeepId + ":" + (int)this.Realm + ":" + this.Rank + ":" + (int)this.KeepStatus + ":" + (int)this.LastMessage;
                if (Tier == 4)
                {
                    BattleFrontStatus BattleFrontStatus = BattleFrontService.GetStatusFor(Region.RegionId);
                    if (BattleFrontStatus != null)
                        keepStatus = keepStatus + ":" + BattleFrontStatus.OpenZoneIndex;
                    else
                        keepStatus = keepStatus + ":-1";
                }
                foreach (Player plr in Player._Players.ToList())
                {
                    if (plr != null && plr.SoREnabled)
                        plr.SendLocalizeString(keepStatus, ChatLogFilters.CHATLOGFILTERS_CHANNEL_9, Localized_text.CHAT_TAG_DEFAULT);
                }
            }
        }

 
    }
}