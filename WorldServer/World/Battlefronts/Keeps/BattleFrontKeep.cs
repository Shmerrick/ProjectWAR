using Appccelerate.StateMachine;
using Common;
using Common.Database.World.Battlefront;
using FrameWork;
using GameData;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Battlefronts.Keeps;
using CreatureSubTypes = GameData.CreatureSubTypes;

namespace WorldServer.World.BattleFronts.Keeps
{
    public class BattleFrontKeep : BattleFrontObjective
    {
        public const byte INNER_DOOR = 1;
        public const byte OUTER_DOOR = 2;


        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly Logger ProgressionLogger = LogManager.GetLogger("RVRProgressionLogger");

        // List of positions where siege weapons may be deployed.
        private readonly List<Hardpoint> _hardpoints = new List<Hardpoint>();

        #region timers
        public int OuterDownTimer;
        public int InnerDownTimer;
        public int SeizedTimer;
        public int LordKilledTimer;
        public int DefenceTickTimer;
        public int BackToSafeTimer;

        public const int OuterDownTimerLength = 3 * 60;
        public const int InnerDownTimerLength =3 * 60;
        public const int SeizedTimerLength = 2 * 60;
        public const int LordKilledTimerLength = 2 * 60;
        public const int DefenceTickTimerLength = 5 * 60;
        public const int BackToSafeTimerLength = 5 * 60;
        #endregion

        public List<KeepNpcCreature> Creatures = new List<KeepNpcCreature>();
        public List<KeepDoor> Doors = new List<KeepDoor>();
        public Keep_Info Info;
        public List<KeepDoor.KeepGameObject> KeepGOs = new List<KeepDoor.KeepGameObject>();
        //public KeepStateMachine.Process p = new KeepStateMachine.Process();
        public bool RamDeployed;
        public Realms Realm;
        public RegionMgr Region;

        public IKeepCommunications KeepCommunications { get; private set; }

        public byte Tier;
        public int PlayersKilledInRange { get; set; }
        public Realms PendingRealm { get; set; }
        public Guild OwningGuild { get; set; }
        public PassiveStateMachine<SM.ProcessState, SM.Command> fsm { get; set; }

        //public Dictionary<KeepStateMachine.ProcessState, Action> actions = new Dictionary<KeepStateMachine.ProcessState, Action>();
        public KeepNpcCreature KeepLord => Creatures?.Find(x => x.Info.KeepLord);


        public BattleFrontKeep(Keep_Info info, byte tier, RegionMgr region, IKeepCommunications comms)
        {
            Info = info;
            Realm = (Realms)info.Realm;
            Tier = tier;
            Region = region;
            KeepCommunications = comms;
            this.Zone = region.GetZoneMgr(info.ZoneId);

            _hardpoints.Add(new Hardpoint(SiegeType.OIL, info.OilX, info.OilY, info.OilZ, info.OilO));
            if (info.OilOuterX > 0)
                _hardpoints.Add(new Hardpoint(SiegeType.OIL, info.OilOuterX, info.OilOuterY, info.OilOuterZ, info.OilOuterO));

            _hardpoints.Add(new Hardpoint(SiegeType.RAM, info.RamX, info.RamY, info.RamZ, info.RamO));

            if (info.RamOuterX > 0)
            {
                _hardpoints[_hardpoints.Count - 1].SiegeRequirement = KeepMessage.Outer0;
                _hardpoints.Add(new Hardpoint(SiegeType.RAM, info.RamOuterX, info.RamOuterY, info.RamOuterZ, info.RamOuterO));
            }

            EvtInterface.AddEvent(UpdateResources, 60000, 0);
            EvtInterface.AddEvent(CheckTimers, 10000, 0);
            PlayersKilledInRange = 0;
            PlayersInRangeOnTake = new HashSet<uint>();

            fsm = new SM(this).fsm;
        }

       
        public void SetGuildOwner(Guild guild)
        {
            OwningGuild = guild;
            SendRegionMessage($"{guild.Info.Name} has taken {Info.Name} as their own!");
        }



        /// <summary>
        /// Check the various timers and determine whether to fire any events
        /// </summary>
        private void CheckTimers()
        {
            var currentTime = TCPManager.GetTimeStamp();

            if (OuterDownTimer > 0 && OuterDownTimer <= currentTime)
                OnOuterDownTimerEnd();
            if (InnerDownTimer > 0 && InnerDownTimer <= currentTime)
                OnInnerDownTimerEnd();
            if (SeizedTimer > 0 && SeizedTimer <= currentTime)
                OnSeizedTimerEnd();
            if (LordKilledTimer > 0 && LordKilledTimer <= currentTime)
                OnLordKilledTimerEnd();
            if (DefenceTickTimer > 0 && DefenceTickTimer <= currentTime)
                OnDefenceTickTimerEnd();
            if (BackToSafeTimer > 0 && BackToSafeTimer <= currentTime)
                OnBackToSafeTimerEnd();
        }

        public HashSet<uint> PlayersInRangeOnTake { get; set; }

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
                door.Spawn();


            if (WorldMgr._Keeps.ContainsKey(Info.KeepId))
                WorldMgr._Keeps[Info.KeepId] = this;
            else
                WorldMgr._Keeps.Add(Info.KeepId, this);
        }

        public void SetSeized()
        {
            _logger.Debug($"Set Seized Timer");
            SeizedTimer = TCPManager.GetTimeStamp() + SeizedTimerLength;

            KeepStatus = KeepStatus.KEEPSTATUS_SEIZED;
        }

        public void SetLordKilled()
        {
            _logger.Debug($"{Info.Name} : Lord Killed");
            var contributionDefinition = new ContributionDefinition();

            foreach (var h in _hardpoints)
                h.CurrentWeapon?.Destroy();

            // Despawn Keep Creatures
            foreach (var crea in Creatures)
                crea.DespawnGuard();


            // Flip realm on Lord Kill
            PendingRealm = Realm == Realms.REALMS_REALM_ORDER ? Realms.REALMS_REALM_DESTRUCTION : Realms.REALMS_REALM_ORDER; ;

            _logger.Info($"Updating VP for Lord Kill. Pending Realm = {PendingRealm}");
            WorldMgr.UpperTierCampaignManager.GetActiveCampaign().VictoryPointProgress.UpdateStatus(WorldMgr.UpperTierCampaignManager.GetActiveCampaign());

            foreach (var plr in PlayersInRange)
            {
                SendKeepInfo(plr);
            }

            var activeBattleFrontId = WorldMgr.UpperTierCampaignManager.ActiveBattleFront.BattleFrontId;
            var activeBattleFrontStatus = WorldMgr.UpperTierCampaignManager.GetBattleFrontStatus(activeBattleFrontId);
            var eligiblePlayers = activeBattleFrontStatus.ContributionManagerInstance.GetEligiblePlayers(0);

            _logger.Info($"Updating Contribution for Lord Kill. Pending Realm = {PendingRealm}");
            KeepRewardManager.KeepLordKill(this, PlayersInRange, Info.Name, activeBattleFrontStatus.ContributionManagerInstance, eligiblePlayers);

            SendRegionMessage(Info.Name + "'s Keep Lord has fallen!");
            LastMessage = KeepMessage.Fallen;

            KeepCommunications.SendKeepStatus(null, this);
            EvtInterface.AddEvent(UpdateStateOfTheRealmKeep, 100, 1);

            PlayersKilledInRange = 0;
            LordKilledTimer = TCPManager.GetTimeStamp() + LordKilledTimerLength;
        }


        public void SetInnerDoorDown()
        {
            SendRegionMessage(Info.Name + "'s inner sanctum  door has been destroyed!");
            _logger.Debug($"{Info.Name} : Inner door destroyed for realm {Realms.REALMS_REALM_DESTRUCTION}");

            // TODO : What is this for?
            //LastMessage = KeepMessage.Inner0;
            KeepRewardManager.InnerDoorReward(PlayersInRange, Info.Name, Region.Campaign.GetActiveBattleFrontStatus().ContributionManagerInstance);
            // Remove any placed rams. TODO - correct?
            foreach (var h in _hardpoints)
            {
                if (h.SiegeType == SiegeType.RAM)
                {
                    h.CurrentWeapon?.Destroy();
                    RamDeployed = false;
                    break;
                }
            }
            InnerDownTimer = TCPManager.GetTimeStamp() + InnerDownTimerLength;

            foreach (var plr in PlayersInRange)
            {
                SendKeepInfo(plr);
            }
            KeepCommunications.SendKeepStatus(null, this);
            KeepStatus = KeepStatus.KEEPSTATUS_INNER_SANCTUM_UNDER_ATTACK;
        }

        public void SetOuterDoorDown()
        {
            SendRegionMessage(Info.Name + "'s outer door has been destroyed!");
            _logger.Debug($"{Info.Name} : Outer door destroyed for realm {Realms.REALMS_REALM_DESTRUCTION}");

            // TODO : What is this for?
            //LastMessage = KeepMessage.Outer0;
            KeepRewardManager.OuterDoorReward(PlayersInRange, Info.Name, Region.Campaign.GetActiveBattleFrontStatus().ContributionManagerInstance);
            // Remove any placed rams. TODO - correct?
            foreach (var h in _hardpoints)
            {
                if (h.SiegeType == SiegeType.RAM)
                {
                    h.CurrentWeapon?.Destroy();
                    RamDeployed = false;
                    break;
                }
            }

            OuterDownTimer = TCPManager.GetTimeStamp() + OuterDownTimerLength;

            KeepCommunications.SendKeepStatus(null, this);
            foreach (var plr in PlayersInRange)
            {
                SendKeepInfo(plr);
            }
            KeepStatus = KeepStatus.KEEPSTATUS_OUTER_WALLS_UNDER_ATTACK;

        }

        public bool BothDoorsRepaired()
        {
            // If both doors are alive - door status is ok
            foreach (var keepDoor in Doors)
            {
                if (keepDoor.GameObject.IsDead)
                    return false;
            }

            return true;
        }


        public void SetInnerDoorRepaired()
        {
            _logger.Debug($"Inner Door Repaired");

            Doors.Single(x => x.Info.Number == INNER_DOOR).Spawn();
            Doors.Single(x => x.Info.Number == INNER_DOOR).GameObject.MaxHealth = 100;



            //var state = p.MoveNext(KeepStateMachine.Command.AllDoorsRepaired);
            //ExecuteAction(state);

        }

        public void SetOuterDoorRepaired()
        {
            _logger.Debug($"Outer Door Repaired");

            Doors.Single(x => x.Info.Number == OUTER_DOOR).Spawn();

            //var state = p.MoveNext(KeepStateMachine.Command.AllDoorsRepaired);
            //ExecuteAction(state);

        }

        /// <summary>
        /// Set the keep safe
        /// </summary>
        public void SetDefenceTick()
        {
            ProgressionLogger.Info($"Defence Tick for {Info.Name}");
            KeepRewardManager.DefenceTickReward(this, PlayersInRange, Info.Name, Region.Campaign.GetActiveBattleFrontStatus().ContributionManagerInstance);

            // Send client message to players in range.
            foreach (var player in PlayersInRange)
            {
                SendKeepInfo(player);
            }

            DefenceTickTimer = 0;
        }

        /// <summary>
        /// Set the keep safe
        /// </summary>
        public void SetLordWounded()
        {
            ProgressionLogger.Info($"Lord Wounded in {Info.Name}");
            SendRegionMessage($"{KeepLord.Creature.Name} has been wounded!");

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

            PlayersKilledInRange /= 2;

            foreach (var plr in PlayersInRange)
            {
                SendKeepInfo(plr);
            }
            KeepCommunications.SendKeepStatus(null, this);

            WorldMgr.UpperTierCampaignManager.GetActiveCampaign().VictoryPointProgress.UpdateStatus(WorldMgr.UpperTierCampaignManager.GetActiveCampaign());


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

            foreach (var plr in PlayersInRange)
            {
                SendKeepInfo(plr);
            }
            KeepCommunications.SendKeepStatus(null, this);

            KeepStatus = KeepStatus.KEEPSTATUS_LOCKED;

            // Remove any persisted values for this keep.
            RVRProgressionService.RemoveBattleFrontKeepStatus(Info.KeepId);
        }


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
            // Keeps rank 4 or 5 have unaccessible posterns for attackers, even after main gate falls
            if (Rank > 3)
                return false;

            if (posternNum == (int)KeepDoorType.OuterPostern)
                return LastMessage >= KeepMessage.Outer0 && Rank < 4;
            return LastMessage >= KeepMessage.Inner0 && Rank < 3;
        }

        public void SendDiagnostic(Player plr)
        {
            plr.SendClientMessage($"[{Info.Name}]", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            plr.SendClientMessage(
                $"{Enum.GetName(typeof(KeepStatus), KeepStatus)} and held by {(Realm == Realms.REALMS_REALM_NEUTRAL ? "no realm" : (Realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction"))}");
            plr.SendClientMessage($"Last message sent: {Enum.GetName(typeof(KeepMessage), LastMessage)}");
            //plr.SendClientMessage($"Rank {Rank}, Ration Factor {GetRationFactor()}");

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
                //if (_safeKeepTimer > 0)
                //    plr.SendClientMessage($"Keep will be safe in {(_safeKeepTimer - TCPManager.GetTimeStamp()) / 60} minutes", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                //else
                //    plr.SendClientMessage($"Keep is now safe", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                plr.SendClientMessage($"RamDeployed: " + RamDeployed);
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
                keepStatus = "SoR_T" + Tier + "_Keep_Update:" + ZoneId + ":" + Info.KeepId + ":" + (int)Realm + ":" + Rank + ":" + (int)KeepStatus + ":" + (int)LastMessage;
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
                // Take us to whatever this was..
                fsm.Initialize((SM.ProcessState)status.Status);

            }
            else
            {
                // Take us to SAFE
                fsm.Initialize(SM.ProcessState.Initial);
                fsm.Fire(SM.Command.OnOpenBattleFront);
            }
            ProgressionLogger.Debug($"Starting Keep {Info.Name} FSM...");

            fsm.Start();

        }

        public void OnOuterDownTimerEnd()
        {
            _logger.Debug($"Outer door has reset");

            OuterDownTimer = 0;

            Doors.Single(x => x.Info.Number == OUTER_DOOR).Spawn();

            //// If both doors are alive - door status is ok
            //lock (Doors)
            //{
            //    _logger.Debug($"Checking Outer door");
            //    var doorList = Doors.Where(x => x.Info.Number == INNER_DOOR || x.Info.Number == OUTER_DOOR);

            //    foreach (var keepDoor in doorList)
            //    {
            //        if (keepDoor.GameObject.IsDead)
            //            return;
            //    }
            //}
            //var state = p.MoveNext(KeepStateMachine.Command.OnOuterDownTimerEnd);
            //ExecuteAction(state);
            fsm.Fire(SM.Command.OnOuterDownTimerEnd);
        }

        public void OnInnerDownTimerEnd()
        {
            _logger.Debug($"Inner door has reset");

            InnerDownTimer = 0;

            Doors.Single(x => x.Info.Number == INNER_DOOR).Spawn();

            //// If both doors are alive - door status is ok
            //lock (Doors)
            //{
            //    _logger.Debug($"Checking Inner door");
            //    var doorList = Doors.Where(x => x.Info.Number == INNER_DOOR || x.Info.Number == OUTER_DOOR);

            //    foreach (var keepDoor in doorList)
            //    {
            //        if (keepDoor.GameObject.IsDead)
            //            return;
            //    }
            //}
            //var state = p.MoveNext(KeepStateMachine.Command.OnInnerDownTimerEnd);
            //ExecuteAction(state);
            fsm.Fire(SM.Command.OnInnerDownTimerEnd);

        }


        //private void ExecuteAction(KeepStateMachine.ProcessState state)
        //{
        //    _logger.Debug($"Executing Action {actions[state]} for {state}");
        //    SendRegionMessage($"Executing Action {actions[state]} for {state}");
        //    actions[state].Invoke();
        //}

        public void OnSeizedTimerEnd()
        {
            SeizedTimer = 0;

            //var state = p.MoveNext(KeepStateMachine.Command.OnSeizedTimerEnd);
            //ExecuteAction(state);
           fsm.Fire(SM.Command.OnSeizedTimerEnd);
        }

        public void OnOuterDoorDown()
        {
            //var state = p.MoveNext(KeepStateMachine.Command.OnOuterDoorDown);
            //ExecuteAction(state);
            fsm.Fire(SM.Command.OnOuterDoorDown);
        }

        public void OnInnerDoorDown()
        {
            //var state = p.MoveNext(KeepStateMachine.Command.OnInnerDoorDown);
            //ExecuteAction(state);
            fsm.Fire(SM.Command.OnInnerDoorDown);
        }

        public void OnLordKilled()
        {
            //var state = p.MoveNext(KeepStateMachine.Command.OnLordKilled);
            //ExecuteAction(state);
            fsm.Fire(SM.Command.OnLordKilled);
        }

        public void OnLockZone(Realms lockingRealm)
        {
            PendingRealm = lockingRealm;
            //var state = p.MoveNext(KeepStateMachine.Command.OnLockZone);
            //ExecuteAction(state);

            fsm.Fire(SM.Command.OnLockZone);
        }

        public void OnLordKilledTimerEnd()
        {
            LordKilledTimer = 0;

            //var state = p.MoveNext(KeepStateMachine.Command.OnLordKilledTimerEnd);
            //ExecuteAction(state);
            fsm.Fire(SM.Command.OnLordKilledTimerEnd);
        }

        public void OnDefenceTickTimerEnd()
        {
            DefenceTickTimer = 0;

            //var state = p.MoveNext(KeepStateMachine.Command.OnDefenceTickTimerEnd);
            //ExecuteAction(state);
            fsm.Fire(SM.Command.OnDefenceTickTimerEnd);
        }

        public void OnBackToSafeTimerEnd()
        {
            BackToSafeTimer = 0;

            //var state = p.MoveNext(KeepStateMachine.Command.OnBackToSafeTimerEnd);
            //ExecuteAction(state);
            fsm.Fire(SM.Command.OnBackToSafeTimerEnd);
        }

        public void OnLordWounded()
        {
            //var state = p.MoveNext(KeepStateMachine.Command.OnLordWounded);
            //ExecuteAction(state);
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

                var patrols = Creatures.Select(x => x).Where(x => x.Info.IsPatrol && x.Creature != null).ToList();
                if (patrols.Count > size) // remove overflow patrols
                {
                    var toRemove = patrols.GetRange(size, patrols.Count - size);

                    foreach (var crea in toRemove)
                        crea.DespawnGuard();
                    for (var i = 0; i < toRemove.Count; i++)
                        Creatures.Remove(toRemove[i]);
                }
                else if (patrols.Count < size) // add new patrols
                {
                    for (var i = 0; i < size - patrols.Count; i++)
                    {
                        var captain = Info.Creatures.Select(x => x).Where(x => x.IsPatrol).FirstOrDefault();
                        if (captain != null)
                        {
                            var allUsedCreatures = Creatures.Select(y => y).Where(y => y.Creature != null).Select(x => x.Info).ToList();
                            if (allUsedCreatures.Contains(captain))
                            {
                                var add = captain.CreateDeepCopy();
                                Creatures.Add(new KeepNpcCreature(Region, add, this));
                            }
                            else
                            {
                                Creatures.Add(new KeepNpcCreature(Region, captain, this));
                            }
                        }
                    }
                }

                // spawn all not yet spawned patrols
                foreach (var patrol in Creatures.Select(x => x).Where(x => x.Info.IsPatrol))
                    if (patrol.Creature == null)
                    {
                        var list = Creatures.Select(x => x).Where(x => x.Info.IsPatrol && x.Creature != null).ToList();
                        list.Sort();
                        var curr = list.FirstOrDefault();
                        if (curr != null)
                            patrol.SpawnGuardNear(Realm, curr);
                        else
                            patrol.SpawnGuard(Realm);
                    }
            }
        }

        #endregion

        #region Keep Progression

        public KeepStatus KeepStatus = KeepStatus.KEEPSTATUS_SAFE;
        public KeepMessage LastMessage;

        public void OnKeepDoorAttacked(byte number, byte pctHealth)
        {
            // Reset the defence tick timer
            if (DefenceTickTimer > 0)
            {
                ProgressionLogger.Debug($"{Info.Name} : Defence Timer reset");
                DefenceTickTimer = TCPManager.GetTimeStamp() + DefenceTickTimerLength;
            }

            switch (number)
            {
                case INNER_DOOR:
                    OnInnerDoorAttacked(pctHealth);
                    break;
                case OUTER_DOOR:
                    OnOuterDoorAttacked(pctHealth);
                    break;
            }
        }

        public void OnKeepNpcAttacked(byte pctHealth)
        {
            // Reset the defence tick timer
            if (DefenceTickTimer > 0)
            {
                ProgressionLogger.Debug($"{Info.Name} : Defence Timer reset");
                DefenceTickTimer = TCPManager.GetTimeStamp() + DefenceTickTimerLength;
            }

            ProgressionLogger.Debug($"{Info.Name} : Keep NPC Attacked");
        }

        public void OnOuterDoorAttacked(byte pctHealth)
        {
            ProgressionLogger.Debug($" {Info.Name} : Outer Door Attacked ");
            SendRegionMessage(Info.Name + "'s outer door is under attack!");
            EvtInterface.AddEvent(UpdateStateOfTheRealmKeep, 100, 1);

            foreach (var plr in PlayersInRange)
            {
                SendKeepInfo(plr);
            }
        }

        public void OnInnerDoorAttacked(byte pctHealth)
        {
            ProgressionLogger.Debug($" {Info.Name} : Inner Door Attacked");
            SendRegionMessage(Info.Name + "'s inner door is under attack!");
            EvtInterface.AddEvent(UpdateStateOfTheRealmKeep, 100, 1);

            foreach (var plr in PlayersInRange)
            {
                SendKeepInfo(plr);
            }
        }

        //        public void OnKeepDoorAttacked(byte number, byte pctHealth)
        //        {
        //            if (number == 2)
        //            {
        //                if (KeepStatus == KeepStatus.KEEPSTATUS_SAFE)
        //                {
        //                    UpdateKeepStatus(KeepStatus.KEEPSTATUS_OUTER_WALLS_UNDER_ATTACK);
        //                    SendRegionMessage(Info.Name + "'s outer door is under attack!");
        //                    foreach (var plr in PlayersInRange)
        //                    {
        //                        SendKeepInfo(plr);
        //                        PlayersInRangeOnTake.Add(plr.CharacterId);
        //                    }
        //                    EvtInterface.AddEvent(UpdateStateOfTheRealmKeep, 100, 1);
        //                }

        //                if (LastMessage < KeepMessage.Outer0)
        //                {
        //                    if (pctHealth < 75 && LastMessage < KeepMessage.Outer75)
        //                    {
        //                        SendRegionMessage(Info.Name + "'s outer door is taking damage!");
        //                        LastMessage = KeepMessage.Outer75;
        //                        KeepCommunications.SendKeepStatus(null, this);
        //                        EvtInterface.AddEvent(UpdateStateOfTheRealmKeep, 100, 1);
        //                    }
        //                    if (pctHealth < 50 && LastMessage < KeepMessage.Outer50)
        //                    {
        //                        SendRegionMessage(Info.Name + "'s outer door begins to buckle under the assault!");
        //                        LastMessage = KeepMessage.Outer50;
        //                        KeepCommunications.SendKeepStatus(null, this);
        //                        EvtInterface.AddEvent(UpdateStateOfTheRealmKeep, 100, 1);
        //                    }
        //                    if (pctHealth < 20 && LastMessage < KeepMessage.Outer20)
        //                    {
        //                        SendRegionMessage(Info.Name + "'s outer door creaks and moans. It appears to be almost at the breaking point!");
        //                        LastMessage = KeepMessage.Outer20;
        //                        KeepCommunications.SendKeepStatus(null, this);
        //                        EvtInterface.AddEvent(UpdateStateOfTheRealmKeep, 100, 1);
        //                    }
        //                }
        //            }
        //            else if (number == 1)
        //            {
        //                if (Info.DoorCount == 1 && KeepStatus == KeepStatus.KEEPSTATUS_SAFE || KeepStatus == KeepStatus.KEEPSTATUS_OUTER_WALLS_UNDER_ATTACK)
        //                {
        //                    UpdateKeepStatus(KeepStatus.KEEPSTATUS_INNER_SANCTUM_UNDER_ATTACK);
        //                    SendRegionMessage(Info.Name + "'s inner sanctum door is under attack!");
        //                    foreach (var plr in PlayersInRange)
        //                    {
        //                        SendKeepInfo(plr);

        //                        PlayersInRangeOnTake.Add(plr.CharacterId);
        //                    }
        //                    EvtInterface.AddEvent(UpdateStateOfTheRealmKeep, 100, 1);
        //                }

        //                if (LastMessage < KeepMessage.Inner0)
        //                {
        //                    if (pctHealth < 75 && LastMessage < KeepMessage.Inner75)
        //                    {
        //                        SendRegionMessage(Info.Name + "'s inner sanctum door is taking damage!");
        //                        LastMessage = KeepMessage.Inner75;
        //                        KeepCommunications.SendKeepStatus(null, this);
        //                        EvtInterface.AddEvent(UpdateStateOfTheRealmKeep, 100, 1);
        //                    }
        //                    if (pctHealth < 50 && LastMessage < KeepMessage.Inner50)
        //                    {
        //                        SendRegionMessage(Info.Name + "'s inner sanctum door begins to buckle under the assault!");
        //                        LastMessage = KeepMessage.Inner50;
        //                        KeepCommunications.SendKeepStatus(null, this);
        //                        EvtInterface.AddEvent(UpdateStateOfTheRealmKeep, 100, 1);
        //                    }
        //                    if (pctHealth < 20 && LastMessage < KeepMessage.Inner20)
        //                    {
        //                        SendRegionMessage(Info.Name + "'s inner sanctum door creaks and moans. It appears to be almost at the breaking point!");
        //                        LastMessage = KeepMessage.Inner20;
        //                        KeepCommunications.SendKeepStatus(null, this);
        //                        EvtInterface.AddEvent(UpdateStateOfTheRealmKeep, 100, 1);

        //                        // This disables rewards for attackers
        //                        if (Constants.DoomsdaySwitch == 2)
        //                            if (WorldMgr.WorldSettingsMgr.GetPopRewardSwitchSetting() == 1)
        //                            {
        ////                                _DestroCount = Region.Campaign._totalMaxDestro;
        ////                                _OrderCount = Region.Campaign._totalMaxOrder;

        ////#if !DEBUG
        ////                                if (Info.Realm == (byte)Realms.REALMS_REALM_ORDER)
        ////                                {
        ////                                    if (_DestroCount > _OrderCount * 4)
        ////                                    {
        ////                                        Region.Campaign.DefenderPopTooSmall = true;
        ////                                        SendRegionMessage("The forces of Destruction are attacking abandoned keep, there are no spoils of war inside!");
        ////                                    }
        ////                                }
        ////                                else
        ////                                {
        ////                                    if (_OrderCount > _DestroCount * 4)
        ////                                    {
        ////                                        Region.Campaign.DefenderPopTooSmall = true;
        ////                                        SendRegionMessage("The forces of Order are attacking abandoned keep, there are no spoils of war inside!");
        ////                                    }
        ////                                }
        ////#endif
        //                            }
        //                    }
        //                }
        //            }
        //        }

        public void OnDoorDestroyed(byte number, Realms realm)
        {
            switch (number)
            {
                case INNER_DOOR:
                    OnInnerDoorDown();
                    break;
                case OUTER_DOOR:
                    OnOuterDoorDown();
                    break;
            }
        }

        //public void OnDoorDestroyed(byte number, Realms realm)
        //{
        //    if (LastMessage < KeepMessage.Inner0)
        //    {
        //        switch (number)
        //        {
        //            case 1:
        //                SendRegionMessage(Info.Name + "'s inner sanctum door has been destroyed!");
        //                if (Rank == 0)
        //                    SendRegionMessage(Info.Name + "'s inner sanctum postern is no longer defended!");
        //                LastMessage = KeepMessage.Inner0;

        //                foreach (var h in _hardpoints)
        //                    if (h.SiegeType == SiegeType.RAM && (Tier == 2 || h.SiegeRequirement > 0))
        //                    {
        //                        h.CurrentWeapon?.Destroy();
        //                        RamDeployed = false;
        //                        break;
        //                    }

        //                // Small reward for inner door destruction
        //                foreach (var player in PlayersInRange)
        //                {
        //                    if (!player.Initialized)
        //                        continue;
        //                    var rnd = new Random();
        //                    var random = rnd.Next(1, 25);
        //                    player.AddXp((uint)(1500 * (1 + random / 100)), false, false);
        //                    player.AddRenown((uint)(400 * (1 + random / 100)), false, RewardType.ObjectiveCapture, Info.Name);

        //                    // Add contribution
        //                    Region.Campaign.GetActiveBattleFrontStatus().ContributionManagerInstance
        //                        .UpdateContribution(player.CharacterId, (byte)ContributionDefinitions.DESTROY_INNER_DOOR);
        //                    var contributionDefinition = new BountyService().GetDefinition((byte)ContributionDefinitions.DESTROY_INNER_DOOR);
        //                    player.BountyManagerInstance.AddCharacterBounty(player.CharacterId, contributionDefinition.ContributionValue);
        //                }

        //                if (realm == Realms.REALMS_REALM_DESTRUCTION)
        //                    Region.Campaign.VictoryPointProgress.DestructionVictoryPoints += KEEP_INNER_DOOR_VICTORYPOINTS;
        //                else
        //                    Region.Campaign.VictoryPointProgress.OrderVictoryPoints += KEEP_INNER_DOOR_VICTORYPOINTS;

        //                _logger.Debug($"Inner door destroyed for realm {Realms.REALMS_REALM_DESTRUCTION} adding {KEEP_INNER_DOOR_VICTORYPOINTS} VP");

        //                break;
        //            case 2:
        //                SendRegionMessage(Info.Name + "'s outer door has been destroyed!");
        //                if (Rank == 0)
        //                    SendRegionMessage($"{Info.Name}'s outer postern{(Tier == 2 ? " is " : "s are ")} no longer defended!");
        //                LastMessage = KeepMessage.Outer0;


        //                // Small reward for outer door destruction
        //                foreach (var player in PlayersInRange)
        //                {
        //                    if (!player.Initialized)
        //                        continue;

        //                    var rnd = new Random();
        //                    var random = rnd.Next(1, 25);

        //                    player.AddXp((uint)(1000 * (1 + random / 100)), false, false);
        //                    player.AddRenown((uint)(200 * (1 + random / 100)), false, RewardType.ObjectiveCapture, Info.Name);

        //                    // Add contribution
        //                    Region.Campaign.GetActiveBattleFrontStatus().ContributionManagerInstance
        //                        .UpdateContribution(player.CharacterId, (byte)ContributionDefinitions.DESTROY_OUTER_DOOR);
        //                    var contributionDefinition = new BountyService().GetDefinition((byte)ContributionDefinitions.DESTROY_OUTER_DOOR);
        //                    player.BountyManagerInstance.AddCharacterBounty(player.CharacterId, contributionDefinition.ContributionValue);
        //                }

        //                foreach (var h in _hardpoints)
        //                    if (h.SiegeType == SiegeType.RAM)
        //                    {
        //                        h.CurrentWeapon?.Destroy();
        //                        RamDeployed = false;
        //                        break;
        //                    }

        //                _logger.Debug($"Outer door destroyed for realm {Realms.REALMS_REALM_DESTRUCTION} adding {KEEP_OUTER_DOOR_VICTORYPOINTS} VP");

        //                if (realm == Realms.REALMS_REALM_DESTRUCTION)
        //                    Region.Campaign.VictoryPointProgress.DestructionVictoryPoints += KEEP_OUTER_DOOR_VICTORYPOINTS;
        //                else
        //                    Region.Campaign.VictoryPointProgress.OrderVictoryPoints += KEEP_OUTER_DOOR_VICTORYPOINTS;


        //                break;
        //        }

        //        KeepCommunications.SendKeepStatus(null, this);
        //    }

        //    EvtInterface.AddEvent(UpdateStateOfTheRealmKeep, 100, 1);
        //}

        public void OnKeepLordAttacked(byte pctHealth)
        {
            // Reset the defence tick timer
            if (DefenceTickTimer > 0)
                DefenceTickTimer = TCPManager.GetTimeStamp() + DefenceTickTimerLength;

            ProgressionLogger.Debug($"Keep Lord attacked {pctHealth}");

            //if (KeepStatus == KeepStatus.KEEPSTATUS_INNER_SANCTUM_UNDER_ATTACK)
            //{
            //    UpdateKeepStatus(KeepStatus.KEEPSTATUS_KEEP_LORD_UNDER_ATTACK);
            //    foreach (var plr in PlayersInRange)
            //    {
            //        SendKeepInfo(plr);
            //        PlayersInRangeOnTake.Add(plr.CharacterId);
            //    }
            //    EvtInterface.AddEvent(UpdateStateOfTheRealmKeep, 100, 1);
            //}

            //if (pctHealth < 100 && LastMessage < KeepMessage.Lord100)
            //{
            //    SendRegionMessage(Info.Name + "'s Keep Lord is under attack!");
            //    LastMessage = KeepMessage.Lord100;
            //    EvtInterface.AddEvent(UpdateStateOfTheRealmKeep, 100, 1);
            //}
            //else if (pctHealth < 50 && LastMessage < KeepMessage.Lord50)
            //{
            //    SendRegionMessage(Info.Name + "'s Keep Lord is being overrun!");
            //    LastMessage = KeepMessage.Lord50;
            //    EvtInterface.AddEvent(UpdateStateOfTheRealmKeep, 100, 1);
            //}
            //else if (pctHealth < 20 && LastMessage < KeepMessage.Lord20)
            //{
            //    SendRegionMessage(Info.Name + "'s Keep Lord is weak!");
            //    LastMessage = KeepMessage.Lord20;
            //    EvtInterface.AddEvent(UpdateStateOfTheRealmKeep, 100, 1);
            //}
        }


        ///// <summary>
        /////     Provide a defence tick
        ///// </summary>
        //public void SafeKeep()
        //{
        //    uint influenceId = 0;

        //    if (KeepStatus == KeepStatus.KEEPSTATUS_SEIZED || KeepStatus == KeepStatus.KEEPSTATUS_LOCKED || KeepStatus == KeepStatus.KEEPSTATUS_SAFE)
        //        return;

        //    foreach (var plr in PlayersInRange)
        //    {
        //        if (Realm == plr.Realm && plr.ValidInTier(Tier, true))
        //        {
        //            if (influenceId == 0)
        //                influenceId = plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? plr.CurrentArea.DestroInfluenceId : plr.CurrentArea.OrderInfluenceId;

        //            var totalXp = 2000 * Tier;
        //            var totalRenown = 300 * Tier;
        //            var totalInfluence = 100 * Tier;

        //            if (_playersKilledInRange < 4 * Tier)
        //            {
        //                totalXp += (int)(totalXp * (0.25 + _playersKilledInRange / 40f * 0.75));
        //                totalRenown += (int)(totalRenown * (0.25 + _playersKilledInRange / 40f * 0.75));
        //                totalInfluence += (int)(totalInfluence * (0.25 + _playersKilledInRange / 40f * 0.75));
        //            }

        //            plr.AddXp((uint)totalXp, false, false);
        //            plr.AddRenown((uint)totalRenown, false, RewardType.ObjectiveDefense, Info.Name);
        //            plr.AddInfluence((ushort)influenceId, (ushort)totalInfluence);

        //            plr.SendClientMessage($"You've received a reward for your contribution to the holding of {Info.Name}.", ChatLogFilters.CHATLOGFILTERS_RVR);

        //            // Add Contribution for Keep Defence Tick
        //            plr.UpdatePlayerBountyEvent((byte)ContributionDefinitions.KEEP_DEFENCE_TICK);

        //            Log.Info("Keep", $"Keep Defence XP : {totalXp} RP: {totalRenown}, Influence: {totalInfluence}");
        //        }

        //        SendKeepInfo(plr);
        //    }

        //    foreach (var crea in Creatures)
        //        if (crea.Creature == null && !crea.Info.IsPatrol)
        //            crea.SpawnGuard(Realm);
        //        else if (crea.Info.IsPatrol)
        //            crea.DespawnGuard();

        //    foreach (var door in Doors)
        //        door.Spawn();

        //    Log.Info("SafeKeep", "Players Killed: " + _playersKilledInRange);

        //    if (LastMessage >= KeepMessage.Outer0 && Tier > 2 || Tier == 2 && LastMessage >= KeepMessage.Inner0)
        //        _playersKilledInRange /= 2;

        //    UpdateKeepStatus(KeepStatus.KEEPSTATUS_SAFE);
        //    LastMessage = KeepMessage.Safe;

        //    _OrderCount = 0;
        //    _DestroCount = 0;
        //    _playersKilledInRange = 0;
        //}

        private void UpdateKeepStatus(KeepStatus newStatus)
        {
            ProgressionLogger.Debug($"Updating Keep Status : {newStatus}");
            KeepStatus = newStatus;
            KeepCommunications.SendKeepStatus(null, this);
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



        //public void ScaleLordVP(int vp)
        //{
        //    foreach (KeepNpcCreature crea in Creatures)
        //        if (crea.Creature != null && crea.Info.KeepLord)
        //            crea.Creature.ScaleLordVP(vp);
        //}

        #endregion

        #region Range

        private short _playersInRange;

        public void AddPlayer(Player plr)
        {
            if (plr == null)
                return;

            SendKeepInfo(plr);
        }

        public void RemovePlayer(Player plr)
        {
            if (plr == null)
                return;

            SendKeepInfoLeft(plr);
        }

        #endregion

        #region Reward Management

        private ushort _playersKilledInRange;

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

        public readonly int[] _resourcePerRank = { 3, 5, 7, 9, 12, 14 /*, 18*/};
        public readonly int[] _resourceValueMax = { 12, 24, 48, 72, 108, 144 /*, 180*/};

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

        //public void CreateSupplyDrops()
        //{
        //    for (int index = 0; index < SupplyReturnPoints.Count; index++)
        //    {
        //        var returnPoint = SupplyReturnPoints[index];

        //        Realms displayRealm = index == 0 ? Realm : GetContestedRealm();

        //        GameObject_proto proto = GameObjectService.GetGameObjectProto(displayRealm == Realms.REALMS_REALM_ORDER ? (uint)100650 : (uint)100651);

        //        Point3D flagPos = ZoneService.GetWorldPosition(Zone.Info, (ushort) returnPoint.X, (ushort) returnPoint.Y, (ushort) returnPoint.Z);

        //        GameObject_spawn spawn = new GameObject_spawn
        //        {
        //            WorldX = flagPos.X,
        //            WorldY = flagPos.Y,
        //            WorldZ = flagPos.Z,
        //            WorldO = returnPoint.O,
        //            ZoneId = Info.ZoneId
        //        };

        //        spawn.BuildFromProto(proto);

        //        switch (displayRealm)
        //        {
        //            case Realms.REALMS_REALM_ORDER:
        //                switch (Zone.Info.ZoneId/100)
        //                {
        //                    // Dwarf
        //                    case 0:
        //                        spawn.DisplayID = 3238;
        //                        break;

        //                    // Empire
        //                    case 1:
        //                        spawn.DisplayID = 4753;
        //                        break;

        //                    // High Elf
        //                    case 2:
        //                        spawn.DisplayID = 4769;
        //                        break;
        //                }
        //                break;
        //            case Realms.REALMS_REALM_DESTRUCTION:
        //                switch (Zone.Info.ZoneId/100)
        //                {
        //                    // Greenskin
        //                    case 0:
        //                        spawn.DisplayID = 4779;
        //                        break;

        //                    // Chaos
        //                    case 1:
        //                        spawn.DisplayID = 4782;
        //                        break;

        //                    // Dark Elf
        //                    case 2:
        //                        spawn.DisplayID = 1463;
        //                        break;
        //                }
        //                break;
        //        }

        //        ResourceReturnFlag = Region.CreateGameObject(spawn);

        //        ResourceReturnFlag.CaptureDuration = 3;
        //        ResourceReturnFlag.AssignCaptureCheck(CheckHoldingSupplies);
        //        //ResourceReturnFlag.AssignCaptureDelegate(SuppliesReturned);
        //    }
        //}

        public bool CheckHoldingSupplies(Player returner)
        {
            if (returner.Realm != Realm || returner.HeldObject == null)
                return false;

            var buff = returner.BuffInterface.GetBuff((ushort)GameBuffs.ResourceCarrier, returner);

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
        //                        KeepCommunications.SendKeepStatus(null, this);
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
        //            KeepCommunications.SendKeepStatus(null, this);
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

        //         KeepCommunications.SendKeepStatus(null, this);
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
            //if (_safeKeepTimer > 0 && _safeKeepTimer < TCPManager.GetTimeStamp())
            //    if (KeepStatus != KeepStatus.KEEPSTATUS_SEIZED)
            //        TickSafety();
        }

        public void ReloadSiege()
        {
            if (_activeMateriel[(int)MaterielType.Artillery].Count > 0)
                foreach (var siege in _activeMateriel[(int)MaterielType.Artillery])
                    siege.AddShots((int)(Siege.MAX_SHOTS * 0.65f * GetSiegeDamageMod(SiegeType.GTAOE)));

            if (_activeMateriel[(int)MaterielType.Cannon].Count > 0)
                foreach (var siege in _activeMateriel[(int)MaterielType.Cannon])
                    siege.AddShots((int)(Siege.MAX_SHOTS * 0.65f * GetSiegeDamageMod(SiegeType.SNIPER)));
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

        //private void TickSafety()
        //{
        //    var doorReplacementCost = 0;

        //    //Check doors. If any door is down, it needs to be regenerated before we can declare safe.
        //    foreach (var door in Doors)
        //        if (door.GameObject.IsDead)
        //            doorReplacementCost += Math.Min(_resourceValueMax[Rank] * 4, (int)(_maxResource * 0.35f));
        //        else if (door.GameObject.PctHealth < 100)
        //            doorReplacementCost += (int)(Math.Min(_resourceValueMax[Rank] * 4, (int)(_maxResource * 0.35f)) * (1f - door.GameObject.PctHealth * 0.01f));

        //    if (doorReplacementCost > 0)
        //    {
        //        if (_currentResource < doorReplacementCost)
        //        {
        //            if (_nextDoorWarnTime < TCPManager.GetTimeStamp())
        //            {
        //                _nextDoorWarnTime = TCPManager.GetTimeStamp() + 300;

        //                foreach (var player in Region.Players)
        //                    if (player.Realm == Realm && player.CbtInterface.IsPvp)
        //                        player.SendClientMessage(Info.Name + " requires supplies to repair the fallen doors!", ChatLogFilters.CHATLOGFILTERS_RVR);
        //            }
        //        }
        //        else
        //        {
        //            _currentResource -= doorReplacementCost;

        //            _currentResourcePercent = (byte)(_currentResource / _maxResource * 100f);

        //            foreach (var door in Doors)
        //                if (door.GameObject.IsDead)
        //                    door.Spawn();

        //            SafeKeep();

        //            _safeKeepTimer = 0;
        //        }
        //    }
        //    else
        //    {
        //        SafeKeep();

        //        _safeKeepTimer = 0;
        //    }
        //}

        //private void ReclaimKeep()
        //{
        //    Region.Campaign.CommunicationsEngine.Broadcast($"{Info.Name} has been reclaimed by the forces of {(Info.Realm == 1 ? "Order" : "Destruction")}!", Tier);

        //    Realm = (Realms)Info.Realm;

        //    foreach (var crea in Creatures)
        //        if (!crea.Info.IsPatrol)
        //            crea.SpawnGuard(Realm);

        //    foreach (var door in Doors)
        //        door.Spawn();

        //    UpdateKeepStatus(KeepStatus.KEEPSTATUS_SAFE);

        //    LastMessage = KeepMessage.Safe;
        //}

        #endregion

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

        public void SendKeepInfo(Player plr)
        {
            var Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_INFO, 32);
            Out.WriteUInt32(Info.PQuestId);
            Out.WriteByte(0);
            Out.WriteByte((byte)Realm);
            Out.WriteByte(1);
            Out.WriteUInt16(0);
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

            else
            {
                Out.WriteByte(0);
            }

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
            var Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_UPDATE, 8);
            Out.WriteUInt32(Info.PQuestId);
            Out.WriteUInt32(0);
            plr.SendPacket(Out);
        }

        public void SendRegionMessage(string message)
        {
            foreach (var obj in Region.Objects)
            {
                var plr = obj as Player;
                plr?.SendLocalizeString(message, ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
                plr?.SendLocalizeString(message,
                    Realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE,
                    Localized_text.CHAT_TAG_DEFAULT);
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

        #region Zone Locking

        ///// <summary>
        /////     Lock the keep. Keep can no longer be retaken until the Campaign is Initialised.
        ///// </summary>
        ///// <param name="lockingRealm"></param>
        ///// <param name="announce"></param>
        ///// <param name="reset">Reset the owner to the original keep owner for the campaign</param>
        //public void LockKeep(Realms lockingRealm, bool announce, bool reset)
        //{
        //    _logger.Debug($"Locking Keep {Info.Name} for {lockingRealm.ToString()} -- keep can no longer be retaken");

        //    _safeKeepTimer = 0;

        //    Rank = 0;
        //    _currentResource = 0;
        //    if (reset)
        //        Realm = (Realms)Info.Realm;
        //    else
        //        Realm = lockingRealm;

        //    // Despawning the lord means the keep cannot be retaken -> and hence cannot be seized -> reward.
        //    foreach (var crea in Creatures)
        //        crea.DespawnGuard();

        //    foreach (var door in Doors)
        //        door.Spawn();

        //    UpdateKeepStatus(KeepStatus.KEEPSTATUS_LOCKED);

        //    EvtInterface.RemoveEvent(UpdateResources);

        //    LastMessage = KeepMessage.Safe;
        //}

        /// <summary>
        ///     The campaign (pairing) for this keep has just unlocked. Set the intial owner according to the zone.
        /// </summary>
        //public void NotifyPairingUnlocked()
        //{
        //    Realm = (Realms)Info.Realm;

        //    UpdateKeepStatus(KeepStatus.KEEPSTATUS_LOCKED);

        //    EvtInterface.RemoveEvent(UpdateResources);

        //    LastMessage = KeepMessage.Safe;

        //    KeepCommunications.SendKeepStatus(null, this);
        //}

        //public Realms GetInitialOwner()
        //{
        //    switch (Info.KeepId)
        //    {
        //        case 6:  // Kazad Dammaz
        //        case 15: // Wilhelm's Fist
        //        case 26: // Pillars of Remembrance
        //            Log.Info(Info.Name, "Overriding control of this keep towards Destruction");
        //            return Realms.REALMS_REALM_DESTRUCTION;
        //        case 9: // Ironskin Skar
        //        case 20: // Charon's Citadel
        //        case 30: // Wrath's Resolve
        //            Log.Info(Info.Name, "Overriding control of this keep towards Order");
        //            return Realms.REALMS_REALM_ORDER;
        //        default:
        //            return (Realms)Info.Realm;
        //    }
        //}

        //public void ReopenKeep()
        //{
        //    UpdateKeepStatus(KeepStatus.KEEPSTATUS_SAFE);

        //    InformRankOne = false;

        //    EvtInterface.AddEvent(UpdateResources, 60000, 0);

        //    foreach (KeepDoor door in Doors)
        //    {
        //        door.GameObject.SetAttackable(true);
        //    }

        //    foreach (Object obj in Region.Objects)
        //    {
        //        Player plr = obj as Player;

        //        if (plr == null || !plr.ValidInTier(Tier, true))
        //            continue;

        //        plr.SendLocalizeString(Info.Name + " is now open for capture!", ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
        //        plr.SendLocalizeString(Info.Name + " is now open for capture!", Realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE, Localized_text.CHAT_TAG_DEFAULT);
        //    }
        //}

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

            foreach (var h in _hardpoints)
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
                if (Constants.DoomsdaySwitch == 0)
                    spawn = new Creature_spawn
                    {
                        Guid = (uint)CreatureService.GenerateCreatureSpawnGUID(),
                        Level = (byte)(Info.PQuest.PQTier * 10),
                        ZoneId = Info.ZoneId,
                        WorldX = h.X,
                        WorldY = h.Y,
                        WorldZ = h.Z,
                        WorldO = h.Heading
                    };
                else
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
            if (Constants.DoomsdaySwitch == 0)
                if (level / 10 != Tier)
                {
                    player.SendClientMessage("Invalid weapon tier", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                    player.SendClientMessage("This weapon is not of the correct tier.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    return false;
                }

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
                    //foreach (Hardpoint h in _hardpoints)
                    //{
                    //    if (h.SiegeType == SiegeType.OIL)
                    //    {
                    //        player.SendClientMessage("Keep under attack", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                    //        player.SendClientMessage("You cannot deploy a ram at a keep that is defending itself.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    //        return false;
                    //    }
                    //}


                    //if (KeepStatus != KeepStatus.KEEPSTATUS_SAFE)
                    //{
                    //    player.SendClientMessage("Unsafe keep", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                    //    player.SendClientMessage("You cannot deploy a ram at a keep that is unsafe.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    //    return false;
                    //}

                    // If the number of spawned siege items > cap per keep level, dont allow.
                    if (_activeMateriel[type].Count >= _materielCaps[type][Rank])
                    {
                        player.SendClientMessage("No rams available", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                        player.SendClientMessage("Your supply lines cannot support any more rams.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                        return false;
                    }

                    //if (player.GldInterface.Guild == null || (Tier == 4 && player.GldInterface.Guild.Info.Level < 20))
                    //{
                    //    player.SendClientMessage(Tier == 4 ? "Must be in guild of rank 20" : "Must be in guild", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                    //    player.SendClientMessage($"In order to deploy a ram, you must be in a { (Tier == 4 ? "reputable guild of rank 20 or higher." : "guild.") }", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    //    return false;
                    //}
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

            var siegeProto = CreatureService.GetCreatureProto(protoEntry);

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
                    type = (int)MaterielType.Ram;

                    if (player.GldInterface.Guild != null)
                    {
                        var message = $"{player.Name} of {player.GldInterface.Guild.Info.Name} has deployed a ram at {Info.Name}!";
                        var filter = player.Realm == Realms.REALMS_REALM_ORDER
                            ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE
                            : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE;
                        foreach (var plr in Region.Players)
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

            var siege = Siege.SpawnSiegeWeapon(player, this, protoEntry, true);
            _activeMateriel[type].Add(siege);
            Region.AddObject(siege, Info.ZoneId);
            _materielSupply[type] -= 1f;
        }

        public void RemoveKeepSiege(Siege weapon)
        {
            foreach (var h in _hardpoints)
                if (h.CurrentWeapon == weapon)
                {
                    h.CurrentWeapon = null;
                    return;
                }
        }

        public void RemoveSiege(Siege weapon)
        {
            switch ((CreatureSubTypes)weapon.Spawn.Proto.CreatureSubType)
            {
                case CreatureSubTypes.SIEGE_GTAOE:
                    _activeMateriel[(int)MaterielType.Artillery].Remove(weapon);
                    break;
                case CreatureSubTypes.SIEGE_SINGLE_TARGET:
                    _activeMateriel[(int)MaterielType.Cannon].Remove(weapon);
                    break;
                case CreatureSubTypes.SIEGE_RAM:
                    var message = $"{weapon.SiegeInterface.Creator.Name}'s ram has been destroyed!";
                    var filter = Realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE;
                    foreach (var plr in Region.Players)
                        if (plr.CbtInterface.IsPvp && plr.ValidInTier(Region.GetTier(), true) && plr.Realm == Realm)
                        {
                            plr.SendClientMessage(message, filter);
                            plr.SendClientMessage(message, ChatLogFilters.CHATLOGFILTERS_RVR);
                        }
                    _activeMateriel[(int)MaterielType.Ram].Remove(weapon);

                    foreach (var h in _hardpoints)
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
            var hardPos = new Point3D();
            foreach (var h in _hardpoints)
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
            float siegeCount = Math.Max(1, _activeMateriel[(int)(type == SiegeType.SNIPER ? MaterielType.Cannon : MaterielType.Artillery)].Count);

            return Math.Min(1f, siegeCap / siegeCount);
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
            return baseEntry + (4 - 2) * 4;
        }

        #endregion

        public void OnKeepSiegeAttacked(byte pctHealth)
        {
            ProgressionLogger.Debug($"Keep Siege Attacked {pctHealth}");
        }

    }



}