using Appccelerate.StateMachine;
using Common;
using Common.Database.World.Battlefront;
using FrameWork;
using GameData;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using SystemData;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Battlefronts.Keeps;
using WorldServer.World.BattleFronts.Objectives;
using CreatureSubTypes = GameData.CreatureSubTypes;

namespace WorldServer.World.BattleFronts.Keeps
{
    public abstract class BattleFrontObjective : Object
    {
    }

    public class BattleFrontKeep : BattleFrontObjective
    {
        public const byte INNER_DOOR = 1;
        public const byte OUTER_DOOR = 2;
        public const byte HEALTH_BOUNDARY_DEFENCE_TICK_RESTART = 50;
        public const byte DEFENCE_LOCK_COUNT = 4;

        public uint HEAVY_EMPIRE_OIL = 86211;
        public uint HEAVY_CHAOS_OIL = 86223;
        public uint HEAVY_GREENSKIN_OIL = 13450;

        public uint HEAVY_DWARF_OIL = 13414;
        public uint HEAVY_DARKELF_OIL = 13474;
        public uint HEAVY_HIGHELF_OIL = 13438;

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly Logger ProgressionLogger = LogManager.GetLogger("RVRProgressionLogger");

        // List of positions where siege weapons may be deployed.
        public List<Hardpoint> HardPoints = new List<Hardpoint>();

        public List<KeepSiegeSpawnPoints> SpawnPoints = new List<KeepSiegeSpawnPoints>();
        public GuildClaimObjective GuildFlag { get; set; }

        #region timers
        public KeepTimer SeizedTimer;
        public KeepTimer LordKilledTimer;
        public KeepTimer DefenceTickTimer;
        public KeepTimer BackToSafeTimer;

        public const int DoorRepairTimerLength = 5 * 60;
        public const int SeizedTimerLength = 1 * 60;
        public const int LordKilledTimerLength = 1 * 15;
        public const int DefenceTickTimerLength = 5 * 60;
        public const int BackToSafeTimerLength = 5 * 60;
        #endregion

        public List<KeepNpcCreature> Creatures = new List<KeepNpcCreature>();
        public List<KeepDoor> Doors = new List<KeepDoor>();
        public ConcurrentDictionary<uint, KeepTimer> DoorRepairTimers = new ConcurrentDictionary<uint, KeepTimer>();
        public Keep_Info Info;
        public KeepStatus KeepStatus = KeepStatus.KEEPSTATUS_SAFE;

        public byte Rank = 0;
        public bool RamDeployed;
        public Realms Realm;
        public RegionMgr Region;

        public bool InnerPosternCanBeUsed { get; set; }
        public bool OuterPosternCanBeUsed { get; set; }

        public IKeepCommunications KeepCommunications { get; private set; }

        public HashSet<Player> PlayersCloseToLord { get; set; }
        public bool Fortress { get; set; }
        public byte FortDefenceCounter { get; set; }

        public byte Tier;
        public int PlayersKilledInRange { get; set; }
        public Realms PendingRealm { get; set; }
        public Realms AttackingRealm
        {
            get
            {
                if (Realm == Realms.REALMS_REALM_DESTRUCTION)
                    return Realms.REALMS_REALM_ORDER;
                else
                {
                    return Realms.REALMS_REALM_DESTRUCTION;
                }
            }
        }
        public Guild OwningGuild { get; set; }
        public PassiveStateMachine<SM.ProcessState, SM.Command> fsm { get; set; }
        public KeepNpcCreature KeepLord => Creatures?.Find(x => x.Info.KeepLord);

        public BattleFrontKeep(Keep_Info info, byte tier, RegionMgr region, IKeepCommunications comms, bool isFortress)
        {
            Info = info;
            Realm = (Realms)info.Realm;
            Tier = tier;
            Region = region;
            KeepCommunications = comms;
            Zone = region.GetZoneMgr(info.ZoneId);

            if (Zone == null)
            {
                _logger.Warn($"Zone for {Info.Name} is null!");
            }

            SpawnPoints = Info.KeepSiegeSpawnPoints;
            PlayersInRange = new HashSet<Player>();

            if (SpawnPoints != null)
            {
                _logger.Debug($"Loading SpawnPoints for {Info.Name}");
                foreach (var keepSiegeSpawnPointse in SpawnPoints)
                {
                    switch (keepSiegeSpawnPointse.SiegeType)
                    {
                        case (int)SiegeType.OIL:
                            HardPoints.Add(
                                new Hardpoint(
                                    SiegeType.OIL,
                                    keepSiegeSpawnPointse.X,
                                    keepSiegeSpawnPointse.Y,
                                    keepSiegeSpawnPointse.Z,
                                    keepSiegeSpawnPointse.O));
                            break;
                        case (int)SiegeType.RAM:
                            HardPoints.Add(
                                new Hardpoint(
                                    SiegeType.RAM,
                                    keepSiegeSpawnPointse.X,
                                    keepSiegeSpawnPointse.Y,
                                    keepSiegeSpawnPointse.Z,
                                    keepSiegeSpawnPointse.O));
                            break;
                    }
                }
            }

            PlayersKilledInRange = 0;
            FortDefenceCounter = 0;

            fsm = new SM(this).fsm;

            InnerPosternCanBeUsed = false;
            OuterPosternCanBeUsed = false;

            PlayersCloseToLord = new HashSet<Player>();

            SeizedTimer = new KeepTimer($"GuildClaim Keep {Info.Name} Timer", 0, SeizedTimerLength);
            LordKilledTimer = new KeepTimer($"Lord Killed {Info.Name} Timer", 0, LordKilledTimerLength);
            DefenceTickTimer = new KeepTimer($"Defence Tick {Info.Name} Timer", 0, DefenceTickTimerLength);
            BackToSafeTimer = new KeepTimer($"Back to Safe {Info.Name} Keep Timer", 0, BackToSafeTimerLength);

            Fortress = isFortress;
        }

        public override void OnLoad()
        {
            Z = Info.Z;
            X = Zone.CalculPin((uint)Info.X, true);
            Y = Zone.CalculPin((uint)Info.Y, false);

            base.OnLoad();

            Heading = (ushort)Info.O;
            WorldPosition.X = Info.X;
            WorldPosition.Y = Info.Y;
            WorldPosition.Z = Info.Z;

            SetOffset((ushort)(Info.X >> 12), (ushort)(Info.Y >> 12));

            IsActive = true;

            foreach (var crea in Creatures)
                if (!crea.Info.IsPatrol)
                    crea.SpawnGuard(Realm);

            foreach (var door in Doors)
            {
                door.Spawn();
            }

            // Create the guild claim objective flag.
            var guildClaimObjective = BattleFrontService.GetBattleFrontObjectives(this.Region.RegionId).SingleOrDefault(x => x.Entry == Info.GuildClaimObjectiveId);
            if (guildClaimObjective != null)
            {
                GuildFlag = new GuildClaimObjective(this.Region, guildClaimObjective);
                Region.AddObject(GuildFlag, this.ZoneId.Value);
            }
            else
            {
                _logger.Error($"Could not find Guild Claim Objective for {this.Info.Name}");
            }

            if (WorldMgr._Keeps.ContainsKey(Info.KeepId))
                WorldMgr._Keeps[Info.KeepId] = this;
            else
                WorldMgr._Keeps.Add(Info.KeepId, this);
        }

        /// <summary>
        /// Each time this ticks, add one to the FortDefenceCounter. Once it's == 4 (60 mins), Lock the fort in favour of the defender.
        /// </summary>
        public void CountdownFortDefenceTimer()
        {
            if (IsFortress())
            {
                FortDefenceCounter++;
                if (FortDefenceCounter >= DEFENCE_LOCK_COUNT)
                {
                    //if (this.Info.Realm == (int) Realms.REALMS_REALM_ORDER)
                    //    SendRegionMessage("Weakened by the long crusade, the forces of Chaos have been thrown back from Reikwald. The Empire prepares their army for a counterattack.");
                    //if (this.Info.Realm == (int)Realms.REALMS_REALM_DESTRUCTION)
                    //    SendRegionMessage($"The Dark gods have blessed the fortress defenders." +
                    //                      $"Chaos will spread across the Old World with renewed strength.");

                    // Lock the keep for the defending realm
                    OnLockZone((Realms)Info.Realm);
                    WorldMgr.UpperTierCampaignManager.GetActiveCampaign().ExecuteBattleFrontLock((Realms)Info.Realm);
                    FortDefenceCounter = 0;
                }
                else
                {
                    // Inform players on the defending side of the remaining time.
                    SendRegionMessage(
                        $"You have {(DEFENCE_LOCK_COUNT - FortDefenceCounter - 1) * 15} minutes remaining to capture the fortress.",
                        Info.Realm);
                }

            }
        }


        public void SetGuildOwner(Guild guild)
        {
            OwningGuild = guild;
            SendRegionMessage($"{guild.Info.Name} has taken {Info.Name} as their own!");
        }


        /// <summary>
        /// Check the various timers and determine whether to fire any events
        /// </summary>
        public void CheckTimers()
        {
            foreach (var doorRepairTimer in DoorRepairTimers)
            {
                if (doorRepairTimer.Value.IsExpired())
                {
                    var doorIdRepaired = doorRepairTimer.Key;
                    DoorRepairTimers[doorRepairTimer.Key].Reset();
                    OnDoorRepaired(doorIdRepaired);

                    return;
                }
            }

            if (SeizedTimer.IsExpired())
            {
                OnGuildClaimTimerEnd();
                return;
            }
            if (LordKilledTimer.IsExpired())
            {
                OnLordKilledTimerEnd();
                return;
            }
            if (DefenceTickTimer.IsExpired())
            {
                OnDefenceTickTimerEnd();
                return;
            }
            if (BackToSafeTimer.IsExpired())
            {
                OnBackToSafeTimerEnd();
                return;
            }
        }

       

        public void SetKeepSeized()
        {
            SeizedTimer.Start();

            KeepStatus = KeepStatus.KEEPSTATUS_SEIZED;
            KeepCommunications.SendKeepStatus(null, this);

        }

        public void SetLordKilled()
        {
            _logger.Debug($"{Info.Name} : Lord Killed");

            foreach (var h in HardPoints)
                h.CurrentWeapon?.Destroy();

            // Flip realm on Lord Kill
            PendingRealm = Realm == Realms.REALMS_REALM_ORDER ? Realms.REALMS_REALM_DESTRUCTION : Realms.REALMS_REALM_ORDER; ;

            _logger.Info($"Updating VP for Lord Kill. Pending Realm = {PendingRealm}");
            WorldMgr.UpperTierCampaignManager.GetActiveCampaign().VictoryPointProgress.UpdateStatus(WorldMgr.UpperTierCampaignManager.GetActiveCampaign());

            // Find the lord in the Keep creatures.
            var lord = Creatures.SingleOrDefault(x => x.Info.KeepLord == true);
            if (lord == null)
            {
                _logger.Info($"Lord is NULL {Info.Name}");
                return;
            }

            var activeBattleFrontId = WorldMgr.UpperTierCampaignManager.ActiveBattleFront.BattleFrontId;
            var activeBattleFrontStatus = WorldMgr.UpperTierCampaignManager.GetBattleFrontStatus(activeBattleFrontId);

            _logger.Info($"Updating Contribution for Lord Kill. Pending Realm = {PendingRealm}");
            foreach (var plr in lord.Creature.PlayersInRange)
            {
                // Add contribution for being in range
                activeBattleFrontStatus.ContributionManagerInstance.UpdateContribution(plr.CharacterId, (byte)ContributionDefinitions.KILL_KEEP_LORD);
                var contributionDefinition = new BountyService().GetDefinition((byte)ContributionDefinitions.KILL_KEEP_LORD);
                plr.BountyManagerInstance.AddCharacterBounty(plr.CharacterId, contributionDefinition.ContributionValue);

                if (plr.PriorityGroup?.GetLeader() == plr)
                {
                    activeBattleFrontStatus.ContributionManagerInstance.UpdateContribution(plr.CharacterId, (byte)ContributionDefinitions.GROUP_LEADER_KILL_KEEP_LORD);
                    contributionDefinition = new BountyService().GetDefinition((byte)ContributionDefinitions.GROUP_LEADER_KILL_KEEP_LORD);
                    plr.BountyManagerInstance.AddCharacterBounty(plr.CharacterId, contributionDefinition.ContributionValue);
                }
            }

            // Players with contribution to be rewarded.
            var eligiblePlayers = activeBattleFrontStatus.ContributionManagerInstance.GetEligiblePlayers(0);

            KeepRewardManager.KeepLordKill(this, lord.Creature.PlayersInRange, eligiblePlayers);

            // Despawn Keep Creatures
            foreach (var crea in Creatures)
                crea.DespawnGuard();

            SendRegionMessage(Info.Name + "'s Keep Lord has fallen! Hold the keep and await reinforcements.");

            KeepStatus = KeepStatus.KEEPSTATUS_KEEP_LORD_UNDER_ATTACK;
            KeepCommunications.SendKeepStatus(null, this);

            EvtInterface.AddEvent(UpdateStateOfTheRealmKeep, 100, 1);

            PlayersKilledInRange = 0;

            ResetAllStateTimers();

            LordKilledTimer.Start();

            this.GuildFlag.State = StateFlags.Unsecure;

        }

        private void ResetAllStateTimers()
        {
            _logger.Debug($"{Info.Name} Reset All Timers ");
            foreach (var doorRepairTimer in DoorRepairTimers)
            {
                DoorRepairTimers[doorRepairTimer.Key].Reset();
            }

            SeizedTimer.Reset();
            LordKilledTimer.Reset();
            DefenceTickTimer.Reset();
            BackToSafeTimer.Reset();
        }

        public void SetDoorRepaired(uint doorId)
        {

            foreach (var keepDoor in Doors)
            {
                if (keepDoor.GameObject.DoorId == doorId)
                {
                    keepDoor.Spawn();
                    keepDoor.GameObject.MaxHealth = 100;
                    if (keepDoor.Info.Number == INNER_DOOR)
                        InnerPosternCanBeUsed = false;
                    if (keepDoor.Info.Number == OUTER_DOOR)
                        OuterPosternCanBeUsed = false;

                }
            }
        }

        public void SetInnerDoorDown(uint doorId)
        {
            SendRegionMessage(Info.Name + "'s inner sanctum door has been destroyed!");
            _logger.Debug($"{Info.Name} : Inner door destroyed by realm {AttackingRealm}. Door Id : {doorId}");

            var door = Doors.Single(x => x.GameObject.DoorId == doorId);

            KeepRewardManager.InnerDoorReward(door,
                AttackingRealm,
                Info.Name,
                Region.Campaign.GetActiveBattleFrontStatus().ContributionManagerInstance);

            // Remove any placed rams. 
            foreach (var h in HardPoints)
            {
                if (h.SiegeType == SiegeType.RAM)
                {
                    h.CurrentWeapon?.Destroy();
                    RamDeployed = false;
                    break;
                }
            }
            DoorRepairTimers[doorId] = new KeepTimer($"Door {doorId} Repair Timer", 0, DoorRepairTimerLength);
            DoorRepairTimers[doorId].Start();

            KeepStatus = KeepStatus.KEEPSTATUS_KEEP_LORD_UNDER_ATTACK;

            KeepCommunications.SendKeepStatus(null, this);

        }

        public void SetOuterDoorDown(uint doorId)
        {
            SendRegionMessage(Info.Name + "'s outer door has been destroyed!");
            _logger.Debug($"{Info.Name} : Outer door destroyed by realm {AttackingRealm}. Door Id : {doorId}");

            var door = Doors.Single(x => x.GameObject.DoorId == doorId);

            KeepRewardManager.OuterDoorReward(door,
                AttackingRealm,
                Info.Name,
                Region.Campaign.GetActiveBattleFrontStatus().ContributionManagerInstance);

            // Remove any placed rams. 
            foreach (var h in HardPoints)
            {
                if (h.SiegeType == SiegeType.RAM)
                {
                    h.CurrentWeapon?.Destroy();
                    RamDeployed = false;
                    break;
                }
            }

            DoorRepairTimers[doorId] = new KeepTimer($"Door {doorId} Repair Timer", 0, DoorRepairTimerLength);
            DoorRepairTimers[doorId].Start();

            KeepStatus = KeepStatus.KEEPSTATUS_INNER_SANCTUM_UNDER_ATTACK;
            KeepCommunications.SendKeepStatus(null, this);

        }

        public bool AllDoorsRepaired()
        {
            // If all doors are alive - door status is ok
            foreach (var keepDoor in Doors)
            {
                if (keepDoor.GameObject.IsDead)
                    return false;
            }

            _logger.Info($"{Info.Name} : All doors repaired");

            // Start the defence tick timer once the outer door has been repaired.
            DefenceTickTimer.Start();

            return true;
        }




        /// <summary>
        /// Set the keep safe
        /// </summary>
        public void SetDefenceTick()
        {
            ProgressionLogger.Info($"Defence Tick for {Info.Name}");

            if (KeepLord == null)
                _logger.Warn($"{Info.Name} : Lord is null");
            if (KeepLord.Creature == null)
                _logger.Warn($"{Info.Name} : KeepLord.Creature is null");

            _logger.Debug($"Defence Tick for {Info.Name} - {KeepLord?.Creature?.PlayersInRange} players in range.");


            KeepRewardManager.DefenceTickReward(this, KeepLord?.Creature?.PlayersInRange, Info.Name, Region.Campaign.GetActiveBattleFrontStatus().ContributionManagerInstance);

            foreach (var plr in KeepLord?.Creature?.PlayersInRange)
            {
                SendKeepInfo(plr);
            }
            KeepCommunications.SendKeepStatus(null, this);

            ProgressionLogger.Info($"Resetting DefenceTickTimer {Info.Name}");

        }

        /// <summary>
        /// Set the keep safe
        /// </summary>
        public void SetLordWounded()
        {
            ProgressionLogger.Info($"Lord Wounded in {Info.Name}");
            SendRegionMessage($"{KeepLord.Creature.Name} has been wounded!");

            InnerPosternCanBeUsed = true;

            KeepStatus = KeepStatus.KEEPSTATUS_KEEP_LORD_UNDER_ATTACK;
        }


        /// <summary>
        /// Set the keep safe
        /// </summary>
        public void SetKeepSafe()
        {
            ProgressionLogger.Info($"Setting Keep Safe {Info.Name}. Pending Realm = {PendingRealm}");

            Realm = PendingRealm;
            // Must be set before the doors are spawned.
            KeepStatus = KeepStatus.KEEPSTATUS_SAFE;

            foreach (var door in Doors)
            {
                door.Spawn();
            }

            foreach (var crea in Creatures)
                crea.DespawnGuard();

            foreach (var crea in Creatures)
                if (!crea.Info.IsPatrol)
                    crea.SpawnGuard(Realm);


            // Remove all siege
            RemoveAllAttackingKeepSiege();

            PlayersKilledInRange /= 2;
            // Update all players within 200 range - update the map.
            foreach (var plr in GetInRange<Player>(200))
            {
                SendKeepInfo(plr);
            }
            // Send all players an update upper right.
            KeepCommunications.SendKeepStatus(null, this);

            WorldMgr.UpperTierCampaignManager.GetActiveCampaign().VictoryPointProgress.UpdateStatus(WorldMgr.UpperTierCampaignManager.GetActiveCampaign());

            InnerPosternCanBeUsed = false;
            OuterPosternCanBeUsed = false;

            ProgressionLogger.Trace($"Setting Door Timers {Info.Name}. Pending Realm = {PendingRealm}");

            foreach (var door in Doors)
            {
                if (!DoorRepairTimers.ContainsKey(door.GameObject.DoorId))
                {
                    DoorRepairTimers.TryAdd(door.GameObject.DoorId, new KeepTimer($"Door {door.GameObject.DoorId} Repair Timer", 0, DoorRepairTimerLength));
                }

            }

        }

        /// <summary>
        /// Set the keep locked
        /// </summary>
        /// <param name="realm"></param>
        public void SetKeepLocked()
        {
            ProgressionLogger.Info($"Setting Keep Locked {Info.Name} locking to {PendingRealm}");

            Realm = PendingRealm;

            foreach (var door in Doors)
            {
                door.Spawn();
                door.GameObject.SetAttackable(false);
            }
            foreach (var crea in Creatures)
                if (!crea.Info.IsPatrol)
                    crea.DespawnGuard();

            KeepStatus = KeepStatus.KEEPSTATUS_LOCKED;

            InnerPosternCanBeUsed = false;
            OuterPosternCanBeUsed = false;

            // Update all players within 200 range - update the map.
            foreach (var plr in GetInRange<Player>(200))
            {
                SendKeepInfo(plr);
            }
            // Send all players an update upper right.
            KeepCommunications.SendKeepStatus(null, this);

            // Remove all siege
            RemoveAllKeepSiege();

            // Remove any persisted values for this keep.
            RVRProgressionService.RemoveBattleFrontKeepStatus(Info.KeepId);
            // Stop the state machine
            //  this.fsm.Stop();
        }


        public bool AttackerCanUsePostern(int posternNum)
        {
            if (posternNum == (int)KeepDoorType.OuterPostern)
                return OuterPosternCanBeUsed;
            if (posternNum == (int)KeepDoorType.InnerPostern)
                return InnerPosternCanBeUsed;

            return false;
        }


        public void SendDiagnostic(Player plr)
        {
            plr.SendClientMessage($"[{Info.Name}]", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            plr.SendClientMessage(
                $"{Enum.GetName(typeof(KeepStatus), KeepStatus)} and held by {(Realm == Realms.REALMS_REALM_NEUTRAL ? "no realm" : (Realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction"))}");

            var lord = Creatures.Find(x => x.Info.KeepLord);

            if (lord == null || lord.Creature == null)
            {
                plr.SendClientMessage("NO LORD");
            }
            else if (lord.Creature.IsDead)
            {
                plr.SendClientMessage("LORD DEAD");
            }
            else
            {
                plr.SendClientMessage($"Keep Lord: {lord.Creature.Name}");
                plr.SendClientMessage($"WorldPosition: {lord.Creature.WorldPosition}");
                plr.SendClientMessage($"Distance from spawnpoint: {lord.Creature.WorldPosition.GetDistanceTo(lord.Creature.WorldSpawnPoint)}");
                plr.SendClientMessage($"Health: {lord.Creature.PctHealth}");
                plr.SendClientMessage($"RamDeployed: " + RamDeployed);
                plr.SendClientMessage($"GuildClaim: " + this.OwningGuild?.Info.Name);
            }
        }

        public void AddAllSiege(List<Siege> siege)
        {
            for (var i = 0; i < 4; ++i)
                if (_activeMateriel[i].Count > 0)
                    siege.AddRange(_activeMateriel[i]);
        }

        public void UpdateStateOfTheRealmKeep()
        {
            var keepStatus = "";
            if (this != null && ZoneId != null)
            {
                keepStatus = "SoR_T" + Tier + "_Keep_Update:" + ZoneId + ":" + Info.KeepId + ":" + (int)Realm + ":" + Rank + ":" + (int)KeepStatus + ":" + (int)0;
                if (Tier == 4)
                {
                    var BattleFrontStatus = BattleFrontService.GetStatusFor(Region.RegionId);
                    if (BattleFrontStatus != null)
                        keepStatus = keepStatus + ":" + BattleFrontStatus.OpenZoneIndex;
                    else
                        keepStatus = keepStatus + ":-1";
                }
                foreach (var plr in Player._Players.ToList())
                    if (plr != null && plr.SoREnabled)
                        plr.SendLocalizeString(keepStatus, ChatLogFilters.CHATLOGFILTERS_CHANNEL_9, Localized_text.CHAT_TAG_DEFAULT);
            }
        }


        public bool IsRamDeployed()
        {
            return _activeMateriel[(int)MaterielType.Ram].Count > 0;
        }

        #region State Event Handlers

        public void OpenBattleFront()
        {
            // When the battlefront opens, set the default realm for the keep
            PendingRealm = (Realms)Info.Realm;

            // Detect if there is a save state for this Keep. If so, load it. 
            var status = RVRProgressionService.GetBattleFrontKeepStatus(Info.KeepId);
            if (status != null)
            {
                ProgressionLogger.Debug($"Existing BattlefrontKeepStatus located. Loading.. {status.Status}");
                if (!fsm.IsRunning)
                    fsm.Initialize((SM.ProcessState)status.Status);

            }
            else
            {
                // Take us to SAFE
                if (!fsm.IsRunning)
                {
                    fsm.Initialize(SM.ProcessState.Initial);
                    fsm.Fire(SM.Command.OnOpenBattleFront);

                    if (this.GuildFlag != null)
                    {
                        this.GuildFlag.Keep = this;
                        this.GuildFlag.State = StateFlags.Unsecure;
                    }
                }
            }
            ProgressionLogger.Debug($"Starting Keep {Info.Name} FSM...");

            if (!fsm.IsRunning)
                fsm.Start();

        }

        public void OnDoorRepaired(uint doorId)
        {
            _logger.Debug($"Door has been repaired {doorId}");

            SetDoorRepaired(doorId);
            fsm.Fire(SM.Command.OnDoorRepaired, doorId);

        }

        public void OnGuildClaimTimerEnd()
        {
            SeizedTimer.Reset();
            fsm.Fire(SM.Command.OnGuildClaimTimerEnd);
        }

        public void OnOuterDoorDown(uint doorId)
        {
            fsm.Fire(SM.Command.OnOuterDoorDown, doorId);
        }

        public void OnInnerDoorDown(uint doorId)
        {
            fsm.Fire(SM.Command.OnInnerDoorDown, doorId);
        }

        public void OnLordKilled()
        {
            fsm.Fire(SM.Command.OnLordKilled);
        }

        public void OnLockZone(Realms lockingRealm)
        {
            PendingRealm = lockingRealm;
            fsm.Fire(SM.Command.OnLockZone);
        }

        public void OnLordKilledTimerEnd()
        {
            LordKilledTimer.Reset();
            fsm.Fire(SM.Command.OnLordKilledTimerEnd);
        }

        public void OnDefenceTickTimerEnd()
        {
            DefenceTickTimer.Reset();
            SetDefenceTick();
        }

        public void OnBackToSafeTimerEnd()
        {
            BackToSafeTimer.Reset();
            fsm.Fire(SM.Command.OnBackToSafeTimerEnd);
        }

        public void OnGuildClaimInteracted(uint guildId)
        {
            fsm.Fire(SM.Command.OnGuildClaimInteracted, guildId);
        }

        public void OnLordWounded()
        {
            fsm.Fire(SM.Command.OnLordWounded);
        }

        #endregion

        #region Update AAO multiplier

        private static readonly object _lockObjUpdateCurrentAAO = new object();

        /// <summary>
        ///     updates mechanics according to aao multiplier
        /// </summary>
        /// <param name="aaoMultiplier">AAO multiplier, -20 if order has 400 aao, +20 if destro has 400 aao</param>
        public void UpdateCurrentAAO(int aaoMultiplier)
        {
            lock (_lockObjUpdateCurrentAAO)
            {
                if (KeepStatus == KeepStatus.KEEPSTATUS_LOCKED || KeepStatus == KeepStatus.KEEPSTATUS_SEIZED)
                    return;

                // calc patrol size (max 5 patrols, including mid)
                var size = 1;
                if (Realm == Realms.REALMS_REALM_ORDER && aaoMultiplier < 0 // keep is order and aao is on destro
                    || Realm == Realms.REALMS_REALM_DESTRUCTION && aaoMultiplier > 0) // keep is destro and aao is on order
                    size = (int)Math.Round(Math.Abs(aaoMultiplier) / 2.5); // 20 / 2.5 = 8 -> 8 is max guard size

               
            }
        }

        #endregion

        #region Keep Progression


        public void OnKeepDoorAttacked(byte number, byte pctHealth, uint doorId)
        {
            // If enough damage is done to the door (not just 'tagged')
            if (pctHealth < HEALTH_BOUNDARY_DEFENCE_TICK_RESTART)
            {
                DefenceTickTimer.Start();

                // if the door attacked is an inner door, reset any active outer door timers.
                var doorUnderAttack = Doors.SingleOrDefault(x => x.GameObject.DoorId == doorId);
                if (doorUnderAttack != null)
                {
                    // Is the door an inner main?
                    if (doorUnderAttack.Info.Number == (int)KeepDoorType.InnerMain)
                    {
                        // Find active timers for outer doors and reset them.
                        foreach (var doorRepairTimer in DoorRepairTimers)
                        {
                            if (doorRepairTimer.Value.Value != 0)
                            {
                                // outer doors.
                                var outerDoors = Doors.Where(x => x.Info.Number == (int)KeepDoorType.OuterMain);
                                foreach (var outerDoor in outerDoors)
                                {
                                    DoorRepairTimers[outerDoor.GameObject.DoorId].Reset();
                                }
                            }
                        }
                    }
                }
                KeepStatus = KeepStatus.KEEPSTATUS_OUTER_WALLS_UNDER_ATTACK;
            }

            switch (number)
            {
                case INNER_DOOR:
                    OnInnerDoorAttacked(pctHealth, doorId);
                    break;
                case OUTER_DOOR:
                    OnOuterDoorAttacked(pctHealth, doorId);
                    break;
            }
        }

        public void OnKeepNpcAttacked(byte pctHealth)
        {
            // If NPC has been killed
            if (pctHealth == 0)
                DefenceTickTimer.Start();

            ProgressionLogger.Debug($"{Info.Name} : Keep NPC Attacked");
        }

        public void OnOuterDoorAttacked(byte pctHealth, uint doorId)
        {
            ProgressionLogger.Debug($" {Info.Name} : Outer Door ({doorId}) Attacked");
            SendRegionMessage($"{Info.Name}'s outer door is under attack!");

            EvtInterface.AddEvent(UpdateStateOfTheRealmKeep, 100, 1);
        }

        public void OnInnerDoorAttacked(byte pctHealth, uint doorId)
        {
            ProgressionLogger.Debug($" {Info.Name} : Inner Door ({doorId}) Attacked");
            SendRegionMessage($"{Info.Name}'s inner door is under attack!");

            OuterPosternCanBeUsed = true;

            EvtInterface.AddEvent(UpdateStateOfTheRealmKeep, 100, 1);
        }


        public void OnDoorDestroyed(byte number, Realms realm, uint doorId)
        {
            switch (number)
            {
                case INNER_DOOR:
                    OnInnerDoorDown(doorId);
                    break;
                case OUTER_DOOR:
                    OnOuterDoorDown(doorId);
                    break;
            }
        }


        public void OnKeepLordAttacked(byte pctHealth)
        {
            DefenceTickTimer.Start();
            ProgressionLogger.Debug($"Keep Lord attacked {pctHealth}");

        }



        /// <summary>
        ///     Scales the lord depending on enemy population.
        /// </summary>
        /// <param name="enemyPlayercount">Maximum number of enemies in short history.</param>
        public void ScaleLord(int enemyPlayercount)
        {
            foreach (var crea in Creatures)
                if (crea.Creature != null && crea.Info.KeepLord)
                    crea.Creature.ScaleLord(enemyPlayercount);
        }

        #endregion

        #region Range

        public HashSet<Player> PlayersInRange;

        public void AddPlayer(Player plr)
        {


            if (plr == null)
                return;
            SendKeepInfoLeft(plr);
            SendKeepInfo(plr);
            KeepCommunications.SendKeepStatus(plr, this);
            plr.CurrentKeep = this;
            PlayersInRange.Add(plr);
        }

        public void RemovePlayer(Player plr)
        {
            if (plr == null)
                return;

            SendKeepInfoLeft(plr);
            plr.CurrentKeep = null;
            PlayersInRange.Remove(plr);
        }

        /// <summary>
        /// Get players that are close and members of a given realm.
        /// </summary>
        /// <param name="capturingRealm"></param>
        /// <returns></returns>
        private List<Player> GetClosePlayers(Realms capturingRealm)
        {
            var applicablePlayerList = PlayersInRange.Where(x => x.Realm == capturingRealm).ToList();

            return GetClosePlayers(applicablePlayerList);
        }

        private List<Player> GetClosePlayers(List<Player> playerList)
        {
            var closeHeight = 70 / 2 * UNITS_TO_FEET;
            var closePlayers = new List<Player>();

            foreach (var player in playerList)
            {
                if (player.IsDead || player.StealthLevel != 0 || !player.CbtInterface.IsPvp || player.IsInvulnerable)
                    continue;

                var distance = GetDistanceToObject(player);
                var heightDiff = Math.Abs(Z - player.Z);
                if (distance < 200 && heightDiff < closeHeight)
                {
                    closePlayers.Add(player);
                    SendMeTo(player);
                }
            }
            return closePlayers;
        }

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

        private readonly string[] _materielMessageNames = { "palisade", "artillery piece", "cannon", "ram" };
        private readonly float[] _materielSupply = new float[4];
        private readonly List<Siege>[] _activeMateriel = { new List<Siege>(), new List<Siege>(), new List<Siege>(), new List<Siege>() };

        private readonly int[][] _materielCaps =
        {
            new[] {0, 2, 4, 6, 8, 10}, // barricades
            new[] {2, 2, 3, 4, 5, 6}, // artillery
            new[] {2, 2, 3, 5, 6, 8}, // cannon
            new[] {1, 1, 1, 1, 2, 3} // ram
        };

        private readonly int[][] _materielRegenTime =
        {
            new[] {5, 5, 4, 3, 2, 2}, // barricades
            new[] {3, 3, 3, 3, 2, 1}, // artillery
            new[] {4, 3, 3, 3, 2, 1}, // cannon
            new[] {15, 15, 10, 7, 5, 3} // ram
        };

        ///// <summary>
        /////     When the lock timer ends, reset the keep to safe. The lord should spawn with guards.
        ///// </summary>
        //public void CheckLockTimer()
        //{
        //    if (KeepStatus == KeepStatus.KEEPSTATUS_SEIZED)
        //        if (_lockKeepTimer > 0 && _lockKeepTimer < TCPManager.GetTimeStamp())
        //        {
        //            RewardLogger.Info($"Locking Keep - setting SAFE");

        //            foreach (var crea in Creatures)
        //                if (crea.Creature == null && !crea.Info.IsPatrol)
        //                    crea.SpawnGuard(Realm);
        //                else if (crea.Info.IsPatrol)
        //                    crea.DespawnGuard();

        //            foreach (var door in Doors)
        //                door.Spawn();

        //            UpdateKeepStatus(KeepStatus.KEEPSTATUS_SAFE);
        //            LastMessage = KeepMessage.Safe;

        //            _OrderCount = 0;
        //            _DestroCount = 0;
        //            _playersKilledInRange = 0;
        //        }
        //}

        public void UpdateResources()
        {
            if (KeepStatus != KeepStatus.KEEPSTATUS_LOCKED)
            {
                for (var i = 0; i < (int)MaterielType.MaxMateriel; ++i)
                {
                    var prevSupply = (int)_materielSupply[i];

                    if (_materielCaps[i][Rank] == 0)
                    {
                        if (_activeMateriel[i].Count == 0 && _materielSupply[i] < 0.9f)
                            if (KeepStatus == KeepStatus.KEEPSTATUS_SAFE)
                                _materielSupply[i] = Math.Min(0.9f, _materielSupply[i] + 1f / _materielRegenTime[i][Rank]);
                            else _materielSupply[i] = Math.Min(0.9f, _materielSupply[i] + 0.1f / _materielRegenTime[i][Rank]);
                    }

                    else if (_materielSupply[i] + _activeMateriel[i].Count < _materielCaps[i][Rank])
                    {
                        if (KeepStatus == KeepStatus.KEEPSTATUS_SAFE)
                            _materielSupply[i] += 1f / _materielRegenTime[i][Rank];
                        else _materielSupply[i] += 0.1f / _materielRegenTime[i][Rank];
                    }

                    var curSupply = (int)_materielSupply[i];

                    if (i > 0 && curSupply > prevSupply)
                    {
                        string message;

                        if (curSupply == 1)
                            message = $"{(i == 1 ? "An" : "A")} {_materielMessageNames[i]} is now available from {Info.Name}!";
                        else
                            message = $"{curSupply} {_materielMessageNames[i]}s are now available from {Info.Name}!";

                        //ChatLogFilters desiredFilter = Realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE;

                        foreach (var player in Region.Players)
                            if (player.CbtInterface.IsPvp && player.Realm == Realm)
                                player.SendClientMessage(message, ChatLogFilters.CHATLOGFILTERS_RVR);
                    }
                }
            }
        }

        public void ProximityReloadSiege(int ammo)
        {
            if (_activeMateriel[(int)MaterielType.Artillery].Count > 0)
                foreach (var siege in _activeMateriel[(int)MaterielType.Artillery])
                    siege.AddShots(ammo);

            if (_activeMateriel[(int)MaterielType.Cannon].Count > 0)
                foreach (var siege in _activeMateriel[(int)MaterielType.Cannon])
                    siege.AddShots(ammo);
        }



        #endregion

        #region Util

        private string GetAttackerMessage()
        {
            switch (KeepStatus)
            {
                case KeepStatus.KEEPSTATUS_INNER_SANCTUM_UNDER_ATTACK:
                    return "Destroy the Sanctum Door";
                case KeepStatus.KEEPSTATUS_KEEP_LORD_UNDER_ATTACK:
                    return "Kill the Lord";
                case KeepStatus.KEEPSTATUS_SEIZED:
                    return "Hold your ground until reinforcements arrive";
                case KeepStatus.KEEPSTATUS_SAFE:
                case KeepStatus.KEEPSTATUS_OUTER_WALLS_UNDER_ATTACK:
                    return "Destroy the Outer Door";
                case KeepStatus.KEEPSTATUS_LOCKED:
                    return "Locked";
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
                    return
                        $"The Keep Lord of {Info.Name} has been defeated, granting the enemy control of this keep for 45 minutes! Hold 50% of the Victory Points to reclaim this keep after the time has expired!";
                case KeepStatus.KEEPSTATUS_LOCKED:
                    return $"{Info.Name} is not attackable at the moment!";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region Senders

        // Updates objective text and 
        public void SendKeepInfo(Player plr)
        {
            var Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_INFO, 32);
            Out.WriteUInt32(Info.PQuestId);
            Out.WriteByte(0);
            Out.WriteByte((byte)Realm);
            Out.WriteByte(1);
            Out.WriteUInt16(0);

            if (this.OwningGuild != null)
                Out.WritePascalString($"{Info.Name} ({this.OwningGuild.Info.Name})");
            Out.WriteByte(2);
            Out.WriteUInt32(0x000039F5);
            Out.WriteByte(0);

            // if player is not the owner and the keep is not locked - send attacker message.
            if (plr.Realm != Realm && KeepStatus != KeepStatus.KEEPSTATUS_LOCKED)
            {
                Out.WriteUInt16(0x0100);
                Out.WriteUInt32(0x00010000);

                Out.WriteShortString(GetAttackerMessage());
            }
            else
            {
                Out.WriteByte(0);
            }

            Out.WriteUInt16(0xFF00);
            Out.WritePascalString(GetObjectiveMessage(plr));
            Out.WriteByte(0);

            Out.WritePascalString(GetObjectiveDescription(plr.Realm));

            // Zeroes previously
            Out.WriteUInt32(0); // timer
            Out.WriteUInt32(0); // timer
            Out.Fill(0, 4);

            Out.WriteByte(0x71);
            Out.WriteByte(3); // keep
            Out.Fill(0, 3);

            plr.SendPacket(Out);

            _logger.Debug($"F_OBJECTIVE_INFO {Info.Name} Quest Id : {Info.PQuestId} Status : {KeepStatus} {GetAttackerMessage()}");
        }

        public void SendKeepInfoLeft(Player plr)
        {
            var Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_UPDATE, 8);
            Out.WriteUInt32(Info.PQuestId);
            Out.WriteUInt32(0);
            plr.SendPacket(Out);
        }

        public void SendRegionMessage(string message, int realmFilter = 0)
        {
            foreach (var obj in Region.Objects)
            {
                var plr = obj as Player;
                if (realmFilter != 0)
                {
                    if (plr?.Realm == (Realms)realmFilter)
                    {
                        plr?.SendLocalizeString(message, ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
                        plr?.SendLocalizeString(message,
                            Realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE,
                            Localized_text.CHAT_TAG_DEFAULT);
                    }
                }
            }
        }



        //reconstructed packet from client dissasmbly
        //player must have role of SystemData.GuildPermissons.KEEPUPGRADE_EDIT 
        //for detailed UI logic, look at interface/default/ea_interactionwindow/source/interactionkeepupgrades.lua
        public void SendKeepUpgradesInteract(Player plr)
        {
            var Out = new PacketOut((byte)Opcodes.F_INTERACT_RESPONSE, 1000);
            Out.WriteByte(0x1E);

            var r = new Random();

            var availibleUpgradeCount = 10;

            Out.WriteByte((byte)availibleUpgradeCount); //total upgrades
            Out.WriteUInt32(10); //current upkeep cost
            Out.WriteUInt32(4); //keep id from data/strings/english/keepnames.txt

            var col = 0;
            var row = 0;

            var keepRankCount = 5;
            var unkCount = 5;


            for (var i = 0; i < availibleUpgradeCount; i++)
            {
                Out.WriteUInt32((uint)(i + 3)); //id from data/strings/english/keepupgradenames.txt

                Out.WriteByte((byte)r.Next(10)); //status fom GameData.KeepUpgradeStatus
                Out.WriteByte((byte)r.Next(5)); //currentLevel
                Out.WriteByte((byte)r.Next(5)); //target level

                Out.WriteByte((byte)keepRankCount); //keep rank pricing count
                Out.WriteUInt32(100); //time

                for (var x = 0; x < keepRankCount; x++)
                {
                    Out.WriteByte((byte)r.Next(25)); //duration
                    Out.WriteByte(0); //min guild rank
                    Out.WriteByte((byte)r.Next(5)); //unk level?
                    Out.WriteUInt32((uint)r.Next(10000)); //gold per minute
                }

                Out.WriteByte((byte)col); //col
                Out.WriteByte((byte)row); //row

                Out.WriteByte((byte)unkCount); //count

                col++;
                if (col > 0)
                {
                    col = 0;
                    row++;
                }

                for (var a = 0; a < unkCount; a++)
                    Out.WriteByte((byte)a);
            }

            if (plr != null)
                plr.SendPacket(Out);
        }

        #endregion

        #region Siege Weapon Management

        public void SpawnOil(Player player, ushort slot)
        {
            if ((Realms)player.Info.Realm != Realm)
            {
                player.SendClientMessage("Can't deploy oil at hostile keep", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                player.SendClientMessage("You cannot deploy oil at a keep you do not own.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }


            var entry = player.ItmInterface.GetItemInSlot(slot).Info.Entry;

            // Disabling T2 and T3 oils here
            if (entry == 86215 || entry == 86203 || entry == 86219 || entry == 86207)
                return;

            foreach (var h in HardPoints)
            {
                if (h.SiegeType != SiegeType.OIL || !player.PointWithinRadiusFeet(h, 10))
                    continue;

                if (h.CurrentWeapon != null)
                {
                    player.SendClientMessage("Can't deploy oil yet", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                    player.SendClientMessage("The oil is blocked by another oil, or the previous oil was destroyed too recently.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    return;
                }

                var proto = CreatureService.GetCreatureProto(GetOilProto(player.Realm));

                Creature_spawn spawn = null;

                spawn = new Creature_spawn
                {
                    Guid = (uint)CreatureService.GenerateCreatureSpawnGUID(),
                    Level = 40,
                    ZoneId = Info.ZoneId,
                    WorldX = h.X,
                    WorldY = h.Y,
                    WorldZ = h.Z,
                    WorldO = h.Heading
                };

                spawn.BuildFromProto(proto);

                h.CurrentWeapon = new Siege(spawn, player, SiegeType.OIL);
                Region.AddObject(h.CurrentWeapon, spawn.ZoneId);

                player.ItmInterface.DeleteItem(slot, 1);
                return;
            }

            player.SendClientMessage("Can't deploy oil here", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
            player.SendClientMessage("This is not a good place to deploy the oil. Move to a better location.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
        }

        public bool CanDeploySiege(Player player, int level, uint protoEntry)
        {
            if (!CheckDist(player))
            {
                player.SendClientMessage("Too close to other weapon or deploy point", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                player.SendClientMessage("This position is too close to another siege weapon or spawn.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return false;
            }

            var siegeProto = CreatureService.GetCreatureProto(protoEntry);

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
                    type = (int)MaterielType.Ram;

                    // If the number of spawned siege items > cap per keep level, dont allow.
                    if (_activeMateriel[type].Count >= _materielCaps[type][Rank])
                    {
                        player.SendClientMessage("No rams available", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                        player.SendClientMessage("Your supply lines cannot support any more rams.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                        return false;
                    }
                    break;
                default:
                    return true;
            }

            return true;
        }


        public void RemoveKeepSiege(Siege weapon)
        {
            foreach (var h in HardPoints)
                if (h.CurrentWeapon == weapon)
                {
                    h.CurrentWeapon = null;
                    return;
                }
        }

        public void RemoveAllKeepSiege()
        {
            foreach (var h in HardPoints)
                h.CurrentWeapon = null;
            return;
        }

        public void RemoveAllAttackingKeepSiege()
        {
            foreach (var h in HardPoints)
            {
                if (h.CurrentWeapon?.Realm != Realm)
                    h.CurrentWeapon = null;
            }
        }

        public void TryAlignRam(Object owner, Player player)
        {
            var hardPos = new Point3D();
            foreach (var h in HardPoints)
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

        public bool CheckDist(Player player)
        {
            if (player == null)
                return false;
            // If we are too near to chest I guess we cannot deply it...?
            if (player.PointWithinRadiusFeet(new Point3D(Info.PQuest.GoldChestWorldX, Info.PQuest.GoldChestWorldY, Info.PQuest.GoldChestWorldZ), 50))
                return false;

            if (player.Realm == Realm)
            {
                foreach (Hardpoint h in HardPoints)
                {
                    if (player.PointWithinRadiusFeet(h, 40))
                        return false;
                }
            }
            else
            {
                foreach (Hardpoint h in HardPoints)
                {
                    if (player.PointWithinRadiusFeet(h, 40))
                        return false;
                }
            }
            return true;
        }

        private uint GetOilProto(Realms targetRealm)
        {
            // Dwarf or Orc
            if (Info.Race == 1 || Info.Race == 2)
            {
                if (targetRealm == Realms.REALMS_REALM_DESTRUCTION)
                    return HEAVY_GREENSKIN_OIL;
                if (targetRealm == Realms.REALMS_REALM_ORDER)
                    return HEAVY_DWARF_OIL;

            }
            // Empire or Chaos (Human). Depending on which realm owns the keep, return the correct type of oil.
            if (Info.Race == 3 || Info.Race == 4)
            {
                if (targetRealm == Realms.REALMS_REALM_DESTRUCTION)
                    return HEAVY_CHAOS_OIL;
                if (targetRealm == Realms.REALMS_REALM_ORDER)
                    return HEAVY_EMPIRE_OIL;

            }
            // High Elf or Dark Elf
            if (Info.Race == 5 || Info.Race == 6)
            {
                if (targetRealm == Realms.REALMS_REALM_DESTRUCTION)
                    return HEAVY_DARKELF_OIL;
                if (targetRealm == Realms.REALMS_REALM_ORDER)
                    return HEAVY_HIGHELF_OIL;

            }
            return 0;

        }
        
        #endregion

        public void OnKeepSiegeAttacked(byte pctHealth)
        {
            ProgressionLogger.Debug($"Keep Siege Attacked {pctHealth}");
        }


        /// <summary>
        /// Record players that were within range of the Keep Lord. 
        /// </summary>
        public void CheckKeepPlayersInvolved()
        {
            var lord = Creatures.SingleOrDefault(x => x.Info.KeepLord == true);
            if (lord == null)
                _logger.Error($"Lord is null!!");
            else
            {
                var playersCloseToLord = lord.Creature?.PlayersInRange.Where(x => x.Realm == PendingRealm).ToList();
                if (playersCloseToLord != null)
                {
                    foreach (var player in playersCloseToLord)
                    {
                        PlayersCloseToLord.Add(player);
                    }
                }

                var y = lord.Creature?.GetPlayersInRange(400, true);
                if (y != null)
                {
                    foreach (var plr in y)
                    {
                        _logger.Trace($"Players close to Lord Y : {plr.Name}");
                    }
                }
            }

            foreach (var plr in PlayersCloseToLord)
            {
                _logger.Trace($"Players close to Lord : {plr.Name}");
            }
        }


        /// <summary>
        /// Force Zone lock for the attacker
        /// </summary>
        public void ForceLockZone()
        {
            _logger.Info($"Attempt to Force Lock Zone from {Info.Name}");
            if (IsFortress())
            {
                OnLockZone(Realm);
                WorldMgr.UpperTierCampaignManager.GetActiveCampaign().ExecuteBattleFrontLock(Realm);
                WorldMgr.UpperTierCampaignManager.GetActiveCampaign().StartRegionLock();
                FortDefenceCounter = 0;
                _logger.Info($"Zone Force Locked from {Info.Name}");
            }
            else
            {
                _logger.Warn($"Attempt to Force Lock Zone for non-fortress {Info.Name}");
            }
        }

        public bool IsFortress()
        {
            return Fortress;
        }

        public bool IsNotFortress()
        {
            return !Fortress;
        }


        /// <summary>
        /// Keep guild claim flag has been interacted with and a guild has claimed this keep.
        /// </summary>
        /// <param name="guildId"></param>
        public void SetGuildClaimed(uint guildId)
        {
            // Flag is secure (cant be interacted with)
            this.GuildFlag.State = StateFlags.Secure;
            SetGuildOwner(Guild.GetGuild(guildId));
        }
    }



}