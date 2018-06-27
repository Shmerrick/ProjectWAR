//#define BattleFront_DEBUG

#if !DEBUG && BattleFront_DEBUG
#error BattleFront DEBUG ENABLED IN RELEASE BUILD
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using Common;
using FrameWork;
using GameData;
using NLog;
using WorldServer.World.BattleFronts;
using WorldServer.Scenarios.Objects;
using WorldServer.World.BattleFronts.Objectives;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Apocalypse;

namespace WorldServer
{
    public class BfFlagGuardCreature : Creature
    {
        
        RvRFlagGuard _flagGrd;

        public BfFlagGuardCreature(Creature_spawn spawn, RvRFlagGuard flagGrd) : base (spawn)
        {
            _flagGrd = flagGrd;
        }

        public override void RezUnit()
        {
            _flagGrd.Creature = new BfFlagGuardCreature(Spawn, _flagGrd);
            Region.AddObject(_flagGrd.Creature, Spawn.ZoneId);
            Destroy();
        }

        protected override void HandleDeathRewards(Player killer)
        {
            
        }
    }

    public class RvRFlagGuard
    {
        uint _orderId;
        uint _destroId;
        int _x;
        int _y;
        int _z;
        int _o;
        ushort _zoneId;
        RegionMgr _region;
        public BfFlagGuardCreature Creature;

        public RvRFlagGuard(RegionMgr region, ushort zoneId, uint orderId, uint destroId, int x, int y, int z, int o)
        {
            _region = region;
            _zoneId = zoneId;
            _orderId = orderId;
            _destroId = destroId;
            _x = x;
            _y = y;
            _z = z;
            _o = o;
        }

        /// <summary>
        /// <para>Despawns the current guard, if any, and spawns one of the supplied team.</para>
        /// <para>If team is 0, no guard will be spawned.</para>
        /// </summary>
        public void SpawnGuard(int team)
        {
            Creature?.Destroy();

            if (team != 0)
            {
                Creature_proto proto = CreatureService.GetCreatureProto(team == 1 ? _orderId : _destroId);
                if (proto == null)
                {
                    Log.Error("FlagGuard", "No FlagGuard Proto");
                    return;
                }

                Creature_spawn spawn = new Creature_spawn();
                spawn.BuildFromProto(proto);
                spawn.WorldO = _o;
                spawn.WorldY = _y;
                spawn.WorldZ = _z;
                spawn.WorldX = _x;
                spawn.ZoneId = _zoneId;

                Creature = new BfFlagGuardCreature(spawn, this);
                _region.AddObject(Creature, spawn.ZoneId);
            }
        }
    }

    [Flags]
    public enum ObjectiveFlags
    {
        Open = 0,
        Hidden = 1,
        Contested = 4,
        Locked = 8,
        ResourceInteraction = 16,


        ZoneLocked = 9
    };

    public class BattleFrontFlag : BattleFrontObjective, IBattleFrontFlag
    {
        private static Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public int ID;
        public string ObjectiveName { get; private set; }

        private int _x, _y, _z, _o, _tokdiscovery, _tokunlocked;

        public Realms _owningRealm;
        private Realms _assaultingRealm;

        public ObjectiveFlags FlagState { get; private set; } = ObjectiveFlags.Open;

        public long _stateTransitionTimer;
        private long _nextTick;
        readonly byte _tier;

        public ushort ZoneId { get; set; }

        public List<RvRFlagGuard> Guards = new List<RvRFlagGuard>();

        // This is used to modify the timer of BO Lock - default from Aza system was is below, 
        // TIMER_MODIFIER should be set to 1.0f, currently we are cutting it by half, change it
        // back to 1.0f to restore default value
        private const float TIMER_MODIFIER = 0.5f;

        private const int MIN_CONTESTED_TIME = (int)(1*60*1000 * TIMER_MODIFIER);
        private const int MAX_CONTESTED_TIME = (int)(6 *60*1000 * TIMER_MODIFIER);
        private const int MAX_LOCK_TIME = (int)(12 *60*1000 * TIMER_MODIFIER);
        private const int RECLAIM_LOCK_TIME = (int)(5 *60*1000 * TIMER_MODIFIER);
        private const float MAX_LOCK_REDUCTION = 0.75f;

        private static bool _allowLockTimer = true;

        public BattleFrontFlag(int id, string name, ushort zoneId, int x, int y, int z, int o, int tokdiscovery, int tokunlocked, byte tier)
        {
            ID = id;
            ObjectiveName = name;
            ZoneId = zoneId;
            _x = x;
            _y = y;
            _z = z;
            _o = o;
            _tokdiscovery = tokdiscovery;
            _tokunlocked = tokunlocked;

            _tier = tier;
            _supplySpawns = BattleFrontService.GetResourceSpawns(ID);

            CaptureDuration = 10;

            _logger.Debug($"Id={id} Name={name} ZoneId={ZoneId} XYZO={x}{y}{z}{o} Tier={tier}");
        }

        public override void OnLoad()
        {
            // Objective position
            Z = _z;
            X = Zone.CalculPin((uint)(_x), true);
            Y = Zone.CalculPin((uint)(_y), false);
            base.OnLoad();

            Heading = (ushort)_o;
            WorldPosition.X = _x;
            WorldPosition.Y = _y;
            WorldPosition.Z = _z;

            SetOffset((ushort)(_x >> 12), (ushort)(_y >> 12));

            IsActive = true;

            if (_supplySpawns != null && _supplySpawns.Count > 0)
                LoadResources();
        }

        public override void Update(long tick)
        {
            if (PendingDisposal)
            {
                Dispose();
                return;
            }

            EvtInterface.Update(tick);

            if (tick < _nextTick)
                return;

            _nextTick = tick + 500;

            if (_stateTransitionTimer == 0 || tick < _stateTransitionTimer)
                return;

            switch (FlagState)
            {
                case ObjectiveFlags.Contested:
                    Realms oldRealm = _owningRealm;
                    ChangeOwnership(_assaultingRealm);
                    _assaultingRealm = Realms.REALMS_REALM_NEUTRAL;

                    if (_allowLockTimer)
                    {
                        FlagState = ObjectiveFlags.Locked;
                        _stateTransitionTimer = tick + (long) (RECLAIM_LOCK_TIME*GetReclaimLockScaler(_owningRealm)*Region.Bttlfront.GetLockPopulationScaler(oldRealm));
                    }

                    else
                    {
                        FlagState = ObjectiveFlags.Open;
                        _stateTransitionTimer = 0;

                        foreach (RvRFlagGuard guard in Guards)
                            guard.SpawnGuard((byte)_owningRealm);
                    }

                    GrantCaptureRewards(_owningRealm);

                    foreach (Player plr in PlayersInRange)
                    {
                        SendMeTo(plr);
                        SendFlagInfo(plr);
                    }
                    SendFlagState(null, true);

                    _logger.Debug($"ContestedFlag. NewState={FlagState}");

                    break;

                case ObjectiveFlags.Locked:
                    FlagState = ObjectiveFlags.Open;
                    _stateTransitionTimer = 0;
                    foreach (RvRFlagGuard guard in Guards)
                        guard.SpawnGuard((byte)_owningRealm);

                    foreach (Player plr in PlayersInRange)
                    {
                        SendMeTo(plr);
                        SendFlagInfo(plr);
                    }

                    SendFlagState(null, false);
                    break;

            }
        }

        #region Internal accessor

        private string GetStateText(Realms realm)
        {
            switch (FlagState)
            {
                case ObjectiveFlags.ZoneLocked:
                    return "LOCKED";
                case ObjectiveFlags.Locked:
                    return "SECURED";
                case ObjectiveFlags.Contested:
                    return realm == _owningRealm ? "CLAIM" : "ASSAULT";
                case ObjectiveFlags.Open:
                    return realm == _owningRealm ? "GENERATING" : "OPEN";
            }

            return "UNKNOWN";
        }

        private byte GetStateFlags()
        {
            if (FlagState == ObjectiveFlags.ZoneLocked)
                return (byte)ObjectiveFlags.Locked;

            byte flagState = (byte) FlagState;

            if (GeneratingSupplies)
                flagState += (byte) ObjectiveFlags.ResourceInteraction;

            return flagState;
        }

        private static string GetRealmString(Realms realm)
        {
            return realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction";
        }

        /// <summary>Returns _assaultingRealm and not owning realm if not neutral !</summary>
        /// <returns></returns>
        private Realms GetOwningRealm()
        {
            return _assaultingRealm != Realms.REALMS_REALM_NEUTRAL ? _assaultingRealm : _owningRealm;
        }

        public Realms OwningRealm { get { return _owningRealm; } }

        #endregion

        #region Range

        private short _playersInRange;

        public override void AddInRange(Object obj)
        {
            Player plr = obj as Player;
            if (plr != null)
            {
                SendFlagInfo(plr);
                //NEW DAWN
                //plr.CurrentObjectiveFlag = this;
                ++_playersInRange;

                if (_tokdiscovery > 0)
                    plr.TokInterface.AddTok((ushort)this._tokdiscovery);
                if (_tokunlocked > 0)
                    plr.TokInterface.AddTok((ushort)this._tokunlocked);
            }

            base.AddInRange(obj);
        }

        public override void RemoveInRange(Object obj)
        {
            Player plr = obj as Player;
            if (plr != null)
            {
                SendFlagLeft(plr);
                // NEWDAWN
                //if (plr.CurrentObjectiveFlag == this)
                //    plr.CurrentObjectiveFlag = null;
                --_playersInRange;
            }

            base.RemoveInRange(obj);
        }

        #endregion

        #region Reward Management

        const int DEFENSE_TICK_INTERVAL_SECONDS = 300;

        public uint AccumulatedKills;

        private readonly Dictionary<uint, XpRenown> _delayedRewards = new Dictionary<uint, XpRenown>();

        public bool FlagActive()
        {
            return FlagState != ObjectiveFlags.Locked && FlagState != ObjectiveFlags.ZoneLocked && _owningRealm != Realms.REALMS_REALM_NEUTRAL;
        }

        public bool CheckKillValid(Player player)
        {
            if (FlagActive() && _playersInRange > 4 && Get2DDistanceToObject(player) < 200)
            {
                if (player.Realm != _owningRealm)
                    AccumulatedKills++;
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Stores kill-based delayed rewards for the quadrant represented by this battlefield objective.
        /// </summary>
        public void AddDelayedRewardsFrom(Player killer, Player killed, uint xpShare, uint renownShare)
        {
            if (xpShare == 0 && renownShare == 0)
                return;

            XpRenown curEntry;

            uint renownReward = (uint) (renownShare * killer.GetKillRewardScaler(killed));

            #if BattleFront_DEBUG
            player.SendClientMessage($"{ObjectiveName} storing {xpShare} XP and {renownReward} renown");
            #endif
            _delayedRewards.TryGetValue(killer.CharacterId, out curEntry);

            if (curEntry == null)
            {
                curEntry = new XpRenown(xpShare, renownReward, 0, 0, TCPManager.GetTimeStamp() + DEFENSE_TICK_INTERVAL_SECONDS);
                if (killer.Realm == _owningRealm)
                    curEntry.LastUpdatedTime = TCPManager.GetTimeStamp() + DEFENSE_TICK_INTERVAL_SECONDS;

                _delayedRewards.Add(killer.CharacterId, curEntry);
            }

            else
            {
                curEntry.XP += xpShare;
                curEntry.Renown += renownReward;
            }

            if (killer.Realm != _owningRealm)
                curEntry.LastUpdatedTime = TCPManager.GetTimeStamp();
        }

        /// <summary>
        /// Grants a reward to each player in the region based upon their kill contribution, if any, to this attack over the last 30 minutes.
        /// </summary>
        public void RewardAttack(Realms capturingRealm)
        {
            long curTimeSeconds = TCPManager.GetTimeStamp();

            float attackBias = ((BattleFront)Region.Bttlfront).GetAttackBias(capturingRealm);

            Item_Info medallionInfo = null;

            if (_tier > 1)
                medallionInfo = ItemService.GetItem_Info((uint) (208399 + (_tier - 1)));

            foreach (Player plr in Region.Players)
            {
                if (plr.Realm != capturingRealm)
                    continue;

                XpRenown curEntry;

                _delayedRewards.TryGetValue(plr.CharacterId, out curEntry);

                if (curEntry == null)
                    continue;

                float scaleFactor = attackBias * (1f - Math.Min(1f, (curTimeSeconds - curEntry.LastUpdatedTime)/1800f));

                if (curEntry.XP > 0)
                {
                    #if BattleFront_DEBUG
                    plr.SendClientMessage($"{ObjectiveName} distributing {curEntry.XP} XP and {curEntry.Renown} renown with scaler {scaleFactor}");
                    #endif

                    plr.SendClientMessage($"You've received a reward for your contribution to the assault on {ObjectiveName}.", ChatLogFilters.CHATLOGFILTERS_RVR);
                    plr.AddXp((uint)(curEntry.XP * scaleFactor), true, false);
                    plr.AddRenown((uint)(curEntry.Renown * scaleFactor), true, RewardType.ObjectiveCapture, ObjectiveName);
                    plr.AddInfluence(curEntry.InfluenceId, (ushort)(curEntry.Influence * scaleFactor));

                    if (medallionInfo != null)
                    {
                        ushort medallionCount = Math.Max((ushort) 1, (ushort) (curEntry.Renown/(300*_tier)));

                        if (plr.ItmInterface.CreateItem(medallionInfo, medallionCount) == ItemResult.RESULT_OK)
                            plr.SendLocalizeString(new[] { medallionInfo.Name, medallionCount.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
                    }


                    Region.Bttlfront.AddContribution(plr, (uint)(curEntry.Renown * scaleFactor));

                    curEntry.XP = 0;
                    curEntry.Renown = 0;
                    curEntry.Influence = 0;
                }

                else
                    plr.SendClientMessage($"You somehow have an attacking contribution logged on {ObjectiveName} which has already been paid out.", ChatLogFilters.CHATLOGFILTERS_RVR);
            }

            // Both attacking and defending rewards should be cleared when an objective is taken.
            _delayedRewards.Clear();
        }

        /// <summary>
        /// Grants a reward to each player in the region based on their kill contribution, if any, to this defense over the last 10 minutes.
        /// </summary>
        public void TickDefense(long curTimeSeconds)
        {
            if (_owningRealm == Realms.REALMS_REALM_NEUTRAL)
                return;

            // Defense rewards scale the more objectives are held.
            float defenseBias;
            if (Constants.DoomsdaySwitch == 2)
                defenseBias = ((ProximityBattleFront)Region.Bttlfront).GetDefenseBias(_owningRealm);
            else
                defenseBias = ((BattleFront)Region.Bttlfront).GetDefenseBias(_owningRealm);

            Item_Info medallionInfo = null;

            if (_tier > 1)
                medallionInfo = ItemService.GetItem_Info((uint)(208399 + (_tier - 1)));

            try
            {
                foreach (Player plr in Region.Players)
                {
                    if (plr.Realm != _owningRealm || !plr.Loaded)
                        continue;

                    if (plr._Value == null)
                    {
                        Log.Error("TickDefense", "Player "+plr.Name+" with no char values");
                        continue;
                    }

                    // Base reward for being stationed within this area

                    XpRenown curEntry;

                    _delayedRewards.TryGetValue(plr.CharacterId, out curEntry);

                    // Not a contributor.
                    if (curEntry == null)
                        continue;

                    // Not ready to distribute rewards.
                    if (curTimeSeconds < curEntry.LastUpdatedTime)
                    {
                        #if BattleFront_DEBUG
                        plr.SendClientMessage($"TickDefense: next tick is in {curEntry.LastUpdatedTime - curTimeSeconds}s.");
                        #endif
                        continue;
                    }

                    // No XP earned here between this defense tick and the last one.
                    if (curEntry.XP == 0)
                    {
                        _delayedRewards.Remove(plr.CharacterId);
                        continue;
                    }

                    #if BattleFront_DEBUG
                    plr.SendClientMessage($"{ObjectiveName} distributing {curEntry.XP} XP and {curEntry.Renown} with bias scale {defenseBias}");
                    #endif

                    plr.SendClientMessage($"You've received a reward for your contribution to the holding of {ObjectiveName}.", ChatLogFilters.CHATLOGFILTERS_RVR);
                    plr.AddXp((uint)(curEntry.XP * defenseBias), true, false);
                    plr.AddRenown((uint)(curEntry.Renown * defenseBias), true, RewardType.ObjectiveDefense, ObjectiveName);
                    plr.AddInfluence(curEntry.InfluenceId, (ushort)(curEntry.Influence * defenseBias));

                    Region.Bttlfront.AddContribution(plr, curEntry.Renown);

                    if (medallionInfo != null)
                    {
                        ushort medallionCount = Math.Max((ushort)1, (ushort)(curEntry.Renown / (450 * _tier)));

                        if (plr.ItmInterface.CreateItem(medallionInfo, medallionCount) == ItemResult.RESULT_OK)
                            plr.SendLocalizeString(new[] { medallionInfo.Name, medallionCount.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
                    }

                    // Delayed rewards for defense are removed over time, to preserve the tick interval for that player
                    curEntry.XP = 0;
                    curEntry.Renown = 0;
                    curEntry.Influence = 0;

                    curEntry.LastUpdatedTime = curTimeSeconds + DEFENSE_TICK_INTERVAL_SECONDS;
                }
            }
            catch (Exception e)
            {
                Log.Error("TickDefense", e.ToString());
            }
        }

        /// <summary>
        /// Grants rewards for taking this battlefield objective from the enemy.
        /// </summary>
        /// <param name="capturingRealm"></param>
        public void GrantCaptureRewards(Realms capturingRealm)
        {
            foreach (Player plr in PlayersInRange)
                plr.QtsInterface.HandleEvent(Objective_Type.QUEST_CAPTURE_BO, (uint) ID, 1);

            Item_Info lowerMedallionInfo = null;
            Item_Info medallionInfo = ItemService.GetItem_Info((uint)(208399 + _tier));

            if (_tier > 1)
                lowerMedallionInfo = ItemService.GetItem_Info((uint) (208399 + (_tier - 1)));

            uint influenceId = 0;
            float scaleMult = Region.Bttlfront.GetObjectiveRewardScaler(capturingRealm, PlayersInRange.Count(p => p.Realm == capturingRealm));

            int curTime = TCPManager.GetTimeStamp();

            // Because of the Field of Glory buff, the XP value here is doubled.
            // The base reward in T4 is therefore 3000 XP.
            // Population scale factors can up this to 9000 if the region is full of people and then raise or lower it depending on population balance.
            uint baseXp = (uint)(1500 * _tier * scaleMult); // 750 
            uint baseRp = (uint) (75 * _tier * scaleMult);
            ushort baseInf = (ushort) (30 * _tier * scaleMult);

            // Every 10 kills on the BO awards a medallion of the tier below.
            int lowerMedallionCount = (int)(scaleMult + AccumulatedKills * 0.1f);
            int medallionCount = lowerMedallionCount/5;
            if (lowerMedallionCount >= 5)
                lowerMedallionCount %= 5;

            foreach (Player plr in PlayersInRange)
            {
                if (plr.Realm == capturingRealm && plr.ValidInTier(_tier, true))
                {
                    if (influenceId == 0)
                        influenceId = plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? plr.CurrentArea.DestroInfluenceId : plr.CurrentArea.OrderInfluenceId;

                    #if BattleFront_DEBUG
                    plr.SendClientMessage($"{ObjectiveName} capture reward: {750 * _tier} XP and {75 * _tier} renown with scale mult {scaleMult}");
                    #endif

                    plr.AddXp(baseXp, false, false);
                    plr.AddRenown(baseRp, false, RewardType.ObjectiveCapture, ObjectiveName);
                    plr.AddInfluence((ushort)influenceId, baseInf);

                    if (lowerMedallionInfo != null && lowerMedallionCount > 0 && plr.ItmInterface.CreateItem(lowerMedallionInfo, (ushort)lowerMedallionCount) == ItemResult.RESULT_OK)
                        plr.SendLocalizeString(new[] { lowerMedallionInfo.Name, lowerMedallionCount.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);

                    if (medallionCount > 0 && plr.ItmInterface.CreateItem(medallionInfo, (ushort)medallionCount) == ItemResult.RESULT_OK)
                        plr.SendLocalizeString(new[] { medallionInfo.Name, medallionCount.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);

                    Region.Bttlfront.AddContribution(plr, (uint)(75 * _tier));
                }
            }

            RewardAttack(capturingRealm);

            AccumulatedKills = 0;
        }

        /// <summary>
        /// Grants rewards upon a zone lock.
        /// </summary>
        public void GrantKeepCaptureRewards()
        {
            foreach (Player plr in PlayersInRange)
            {
                if (plr.Realm == _owningRealm && plr.ValidInTier(_tier, true))
                {
                    if (AccumulatedKills < 3)
                    {
                        plr.SendLocalizeString("Defending this objective was a noble duty. You have received a small reward for your service.", ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
                        plr.ItmInterface.CreateItem((uint)208399 + _tier, 1);
                    }
                    else if (AccumulatedKills <= 6)
                    {
                        plr.SendLocalizeString("Your defense of this objective has been noteworthy! You have received a moderate reward.", ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
                        plr.ItmInterface.CreateItem((uint)208399 + _tier, 2);
                    }
                    else if (AccumulatedKills > 6)
                    {
                        plr.SendLocalizeString("Your defense of this objective has been heroic! You have received a respectable reward.", ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
                        plr.ItmInterface.CreateItem((uint)208399 + _tier, 3);
                    }
                }
            }

            AccumulatedKills = 0;
        }

        #endregion

        #region Interaction

        private bool InteractableFor(Player plr)
        {
            _logger.Debug($"FlagState={FlagState}");
            switch (FlagState)
            {
                case ObjectiveFlags.ZoneLocked:
                case ObjectiveFlags.Locked:
                    return false;
                case ObjectiveFlags.Open:
                    return plr.Realm != _owningRealm;
                case ObjectiveFlags.Contested:
                    return plr.Realm == _owningRealm;
                default:
                    return false;
            }
        }

        public override void SendInteract(Player player, InteractMenu menu)
        {
            if (_owningRealm == player.Realm && _assaultingRealm == Realms.REALMS_REALM_NEUTRAL)
            {
                player.SendClientMessage("Your realm already owns this flag.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            if (_owningRealm != player.Realm && _assaultingRealm == player.Realm)
            {
                player.SendClientMessage("Your realm is already assaulting this flag.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            if (!player.CbtInterface.IsPvp)
            {
                player.SendClientMessage("You must be flagged to cap.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            if (player.StealthLevel > 0)
            {
                player.SendClientMessage("You can't interact with objects while in stealth.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            foreach (var guard in Guards)
            {
                if (guard.Creature != null && !guard.Creature.IsDead && guard.Creature.Realm == _owningRealm && GetDistanceTo(guard.Creature) < 100)
                {
                    player.SendClientMessage("Can't capture while a guard ("+guard.Creature.Name+") is still alive.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    return;
                }
            }

            BeginInteraction(player);
        }

        public override void NotifyInteractionComplete(NewBuff b)
        {
            switch (FlagState)
            {
                // Interaction invalid.
                case ObjectiveFlags.Locked:
                case ObjectiveFlags.ZoneLocked:
                    CapturingPlayer = null;
                    return;

                // Owning realm reclaims contested flag.
                case ObjectiveFlags.Contested:

                    if (_allowLockTimer)
                    {
                        FlagState = ObjectiveFlags.Locked;
                        _stateTransitionTimer = TCPManager.GetTimeStampMS() + (long) (MAX_LOCK_TIME*GetLockScaler(_owningRealm)*Region.Bttlfront.GetLockPopulationScaler(_assaultingRealm));
                    }
                    else
                    {
                        FlagState = ObjectiveFlags.Open;
                        _stateTransitionTimer = 0;
                    }

                    _assaultingRealm = Realms.REALMS_REALM_NEUTRAL;
                    break;

                // Taking empty flag.
                case ObjectiveFlags.Open:
                    if (_owningRealm != Realms.REALMS_REALM_NEUTRAL)
                    {
                        FlagState = ObjectiveFlags.Contested;
                        _assaultingRealm = CapturingPlayer.Realm;
                        _stateTransitionTimer = TCPManager.GetTimeStampMS() + (long) Math.Min(MAX_CONTESTED_TIME, MIN_CONTESTED_TIME  * GetContestedScaler(_assaultingRealm) * Region.Bttlfront.GetLockPopulationScaler(_assaultingRealm));
                    }

                    else
                        ChangeOwnership(CapturingPlayer.Realm);

                    break;
            }

            foreach (RvRFlagGuard guard in Guards)
                guard.SpawnGuard(FlagState == ObjectiveFlags.Open ? (byte)CapturingPlayer.Realm : 0);

            GrantCaptureRewards(CapturingPlayer.Realm);

            CapturingPlayer = null;

            foreach (Player plr in PlayersInRange)
            {
                SendMeTo(plr);
                SendFlagInfo(plr);
            }

            SendFlagState(null, true);

            if (_tier == 1)
                new ApocCommunications().SendCampaignStatus(null, null, Realms.REALMS_REALM_NEUTRAL);
        }

        private void ChangeOwnership(Realms newRealm)
        {
            if (_owningRealm == newRealm)
                return;

            if (newRealm == Realms.REALMS_REALM_NEUTRAL)
            {
                _owningRealm = newRealm;
                _assaultingRealm = Realms.REALMS_REALM_NEUTRAL;
            }

            else
            {
                Realms oldRealm = _owningRealm;
                _owningRealm = newRealm;

                if (!Region.Bttlfront.NoSupplies)
                {
                    if (_supplies.HeldState == EHeldState.Inactive)
                        StartSupplyRespawnTimer(SupplyEvent.OwnershipChanged);

                    else _supplies.SetRealmAssociation(newRealm);
                }

                if (Constants.DoomsdaySwitch == 2)
                    ((ProximityBattleFront)Region.Bttlfront).ObjectiveCaptured(oldRealm, newRealm, ZoneId);
                else
                    ((BattleFront)Region.Bttlfront).ObjectiveCaptured(oldRealm, newRealm, ZoneId);
            }
        }

        #endregion

        #region Supply Generation


        private int _supplyRespawnTimeMs = (int)(140000 * TIMER_MODIFIER);
        private const int SUPPLY_CLIENT_TIMER_MS = (int)(135000 * TIMER_MODIFIER);

        private readonly List<BattleFrontResourceSpawn> _supplySpawns;

        private ResourceBox _supplies;
        private int _generationTimerEnd;
        private bool GeneratingSupplies => _generationTimerEnd > 0;

        /// <summary>
        /// Sets up the supply box.
        /// </summary>
        private void LoadResources()
        {
            // codeword p0tat0 - Disabled for DoomsDay
            /*BattleFrontResourceSpawn destSpawn = _supplySpawns[StaticRandom.Instance.Next(_supplySpawns.Count)];

            Point3D homePos = ZoneService.GetWorldPosition(Zone.Info, (ushort)destSpawn.X, (ushort)destSpawn.Y, (ushort)destSpawn.Z);
            _supplies = new ResourceBox(
                (uint) ID, "Supplies", homePos, (ushort) GameBuffs.ResourceCarrier, (int)(120000 * TIMER_MODIFIER), SuppliesPickedUp, null, SuppliesReset, ResBuffAssigned,
                GameObjectService.GetGameObjectProto(429).DisplayID, GameObjectService.GetGameObjectProto(429).DisplayID)
            {
                Objective = this,
                ColorMatchesRealm = true,
                PreventsRide = false
            };

            Region.AddObject(_supplies, Zone.ZoneId);*/
        }

        /// <summary>
        /// Assigns the visual effects to a player carrying supplies.
        /// </summary>
        /// <param name="b"></param>
        public void ResBuffAssigned(NewBuff b)
        {
            HoldObjectBuff hB = (HoldObjectBuff)b;

            switch (hB.Target.Realm)
            {
                case Realms.REALMS_REALM_ORDER:
                    hB.FlagEffect = FLAG_EFFECTS.Blue;
                    break;
                case Realms.REALMS_REALM_DESTRUCTION:
                    hB.FlagEffect = FLAG_EFFECTS.Red;
                    break;
                default:
                    hB.FlagEffect = FLAG_EFFECTS.Mball1;
                    break;
            }
        }

        public void SuppliesPickedUp(HoldObject holdObject, Player player)
        {
            player.SendClientMessage("You have picked up the supplies!", player.Realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE);

            if (_generationTimerEnd > 0)
            {
                _generationTimerEnd = 0;
                holdObject.SetRealmAssociation(0);
                SendFlagState(null, false, false);

                foreach (Player plr in Region.Players)
                {
                    if (plr.CbtInterface.IsPvp)
                        plr.SendClientMessage($"{player.Name} is transporting supplies from {ObjectiveName}!", player.Realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE);
                }
            }

            else
            {
                lock (player.PlayersInRange)
                    foreach (Player plr in player.PlayersInRange)
                        plr.SendClientMessage($"{player.Name} is now carrying the supplies!", player.Realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE);
            }

        }

        /// <summary>
        /// Begins respawning of the supplies after a reset.
        /// </summary>
        public void SuppliesReset(HoldObject holdObject)
        {
            if (Region.Bttlfront.NoSupplies)
                return;

            if (holdObject.HeldState == EHeldState.Home)
            {
                _generationTimerEnd = TCPManager.GetTimeStamp();
                return;
            }

            if (FlagState == ObjectiveFlags.Locked || FlagState == ObjectiveFlags.Open || FlagState == ObjectiveFlags.Contested)
                StartSupplyRespawnTimer(SupplyEvent.Reset);
        }

        /// <summary>
        /// Begins the process of making the supplies available at the Battlefield Objective.
        /// </summary>
        public void StartSupplyRespawnTimer(SupplyEvent supplyEvent)
        {
            if (Region.Bttlfront.NoSupplies)
                return;

            if (GeneratingSupplies)
            {
                _generationTimerEnd = 0;
                SendFlagState(null, false, false);
            }

            //Region.Bttlfront.SendPairingBroadcast("Resource timer for "+ObjectiveName+" started.", Realms.REALMS_REALM_ORDER);

            switch (supplyEvent)
            {
                case SupplyEvent.Reset:
                    EvtInterface.AddEvent(SendClientSupplyTimer, _supplyRespawnTimeMs - SUPPLY_CLIENT_TIMER_MS, 1);
                    EvtInterface.AddEvent(RespawnSupplies, _supplyRespawnTimeMs, 1);
                    break;

                case SupplyEvent.OwnershipChanged:
                    SendClientSupplyTimer();
                    EvtInterface.AddEvent(RespawnSupplies, SUPPLY_CLIENT_TIMER_MS, 1);
                    break;

                case SupplyEvent.ZoneActiveStatusChanged:
                    int respawnTime = (int)(StaticRandom.Instance.Next(8, 10)*60*1000 * TIMER_MODIFIER);
                    EvtInterface.AddEvent(SendClientSupplyTimer, respawnTime - SUPPLY_CLIENT_TIMER_MS, 1);
                    EvtInterface.AddEvent(RespawnSupplies, respawnTime, 1);
                    break;
            }
        }

        /// <summary>
        /// Sends the outline around the Battlefield Objective flag to the clients.
        /// </summary>
        public void SendClientSupplyTimer()
        {
            if (Region.Bttlfront.NoSupplies)
                return;

            //Region.Bttlfront.SendPairingBroadcast("Client timer for " + ObjectiveName + " sent.", Realms.REALMS_REALM_ORDER);

            _generationTimerEnd = TCPManager.GetTimeStamp() + 135;
            SendFlagState(null, false, false);
        }

        /// <summary>
        /// Renders the supplies active for capturing.
        /// </summary>
        public void RespawnSupplies()
        {
            if (Region.Bttlfront.NoSupplies)
                return;

            if (_supplies == null)
            {
                Log.Error(ObjectiveName+" in "+Zone.Info.Name+" with BattleFront supply block status "+Region.Bttlfront.NoSupplies, "Supplies are null!");
                return;
            }
            //Region.Bttlfront.SendPairingBroadcast("Respawning resource for " + ObjectiveName + ".", Realms.REALMS_REALM_ORDER);
            _supplies.SetActive(_owningRealm);
        }

        /// <summary>
        /// Prevents the supplies from respawning and hides any existing supplies.
        /// </summary>
        public void BlockSupplySpawn()
        {
            EvtInterface.RemoveEvent(SendClientSupplyTimer);
            EvtInterface.RemoveEvent(RespawnSupplies);

            if (GeneratingSupplies)
            {
                _generationTimerEnd = 0;
                SendFlagState(null, false, false);
            }

            if (_supplies == null)
            {
                Log.Error("BattleFront", "NO SUPPLIES AT " + ObjectiveName + " WITH ID " + ID);
                return;
            }

            if (_supplies.HeldState != EHeldState.Inactive)
                _supplies.ResetTo(EHeldState.Inactive);

            _supplies.SetRealmAssociation(Realms.REALMS_REALM_NEUTRAL);
        }

        #endregion

        #region Senders

        public override void SendMeTo(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CREATE_STATIC, 64);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(0);

            Out.WriteUInt16((ushort)_o);
            Out.WriteUInt16((ushort)_z);
            Out.WriteUInt32((uint)_x);
            Out.WriteUInt32((uint)_y);

            int displayId;

            switch (_owningRealm)
            {
                case Realms.REALMS_REALM_NEUTRAL:
                    displayId = 3442;
                    break;
                case Realms.REALMS_REALM_ORDER:
                    displayId = 3443;
                    break;
                case Realms.REALMS_REALM_DESTRUCTION:
                    displayId = 3438;
                    break;
                default:
                    displayId = 3442;
                    break;
            }

            Out.WriteUInt16((ushort)displayId);

            Out.WriteUInt16(0x1E);
            Out.WriteUInt16(0);
            Out.WriteUInt16(0);
            Out.WriteByte(0);

            if (FlagState != ObjectiveFlags.Locked && FlagState != ObjectiveFlags.ZoneLocked && InteractableFor(plr))
                Out.WriteUInt16(4);
            else
                Out.WriteUInt16(0);

            Out.WriteByte(0);

            Out.WriteUInt16(0);
            Out.WriteByte(0);
            Out.WriteByte(100);

            Out.WriteUInt16(0);
            Out.WriteUInt16(0);
            Out.WriteUInt32(0);

            Out.WritePascalString("Battlefield Objective");
            Out.WriteByte(0);

            plr.SendPacket(Out);

            base.SendMeTo(plr);
        }

        public void SendFakeState(Player plr, int unk, int announceType, int status)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_STATE, 16);
            Out.WriteUInt32((uint)ID);

            if ((status & 16) > 0)
            {
                Out.WriteByte(0xFF);
                Out.WriteByte(0xFF);
                Out.WriteByte(0xFF);
                Out.WriteByte(0xFF);
                Out.WriteByte(0);
                Out.WriteByte(135); // Unk6 - progress towards next resource release, lower is greater
                Out.WriteUInt16((ushort) unk); // Unk7 - vertical offset for drawing overlay - Unk6 may not exceed
            }

            else if ((status & 4) > 0)
            {
                Out.WriteByte(0);
                Out.WriteByte(0);
                Out.WriteByte(0);
                Out.WriteByte((byte)unk);
                Out.WriteByte(0xFF);
                Out.WriteByte(0xFF);
                Out.WriteUInt16(0);
            }

            else
            {
                Out.Fill(0xFF, 6);
                Out.WriteUInt16(0);
            }
            Out.WriteByte((byte)_owningRealm);
            Out.WriteByte((byte)announceType);
            Out.WriteByte((byte)status); // Bitfield. 16 - Resource generation state 8 - Locked 4 - Burning 1 - Hidden 0 - Open

            plr.SendPacket(Out);
        }

        public void SendFlagState(Player plr, bool announce, bool update = true)
        {
            if (!Loaded)
                return;

            byte flagState = GetStateFlags();

            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_STATE, 16);
            Out.WriteUInt32((uint)ID);

            if (FlagState == ObjectiveFlags.Contested)
            {
                Out.Fill(0, 2);
                Out.WriteUInt16((ushort)Math.Max(1, (_stateTransitionTimer - TCPManager.GetTimeStampMS()) / 1000));
                Out.Fill(0xFF, 2);
                Out.WriteUInt16(0);
            }
            else if (GeneratingSupplies)
            {
                Out.Fill(0xFF, 4);
                Out.WriteByte(0);
                Out.WriteByte((byte)Math.Max(0, _generationTimerEnd - TCPManager.GetTimeStamp())); // Unk6 - time till next resource release
                Out.WriteUInt16(135); // Unk7 - vertical offset for drawing overlay - Unk6 may not exceed
            }
            else
            {
                Out.Fill(0xFF, 6);
                Out.WriteUInt16(0);
            }

            Out.WriteByte((byte)_owningRealm);
            Out.WriteByte(update ? (byte)1 : (byte)0);
            Out.WriteByte(flagState);
            Out.WriteByte(0);

            if (!announce)
            {
                if (plr != null)
                    plr.SendPacket(Out);
                else foreach (Player player in Region.Players)
                    player.SendPacket(Out);
                return;
            }

            string message = null;
            ChatLogFilters largeFilter = GetOwningRealm() == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE;
            PacketOut snd = null;

            if ((FlagState == ObjectiveFlags.Contested || FlagState == ObjectiveFlags.Locked) && _owningRealm != Realms.REALMS_REALM_NEUTRAL)
            {
                switch (FlagState)
                {
                    case ObjectiveFlags.Contested:
                        message = $"{ObjectiveName} is under assault by the forces of {GetRealmString(_assaultingRealm)}!";
                        break;
                    case ObjectiveFlags.Locked:
                        message = $"The forces of {GetRealmString(_owningRealm)} have taken {ObjectiveName}!";
                        snd = new PacketOut((byte)Opcodes.F_PLAY_SOUND);
                        snd.WriteByte(0);
                        snd.WriteUInt16(_owningRealm == Realms.REALMS_REALM_ORDER ? (ushort)0x0C : (ushort)0x332);
                        snd.Fill(0, 10);
                        break;
                }
            }

            if (plr != null)
                plr.SendPacket(Out);
            else
            {
                foreach (Player player in Region.Players)
                {
                    player.SendPacket(Out);

                    if (string.IsNullOrEmpty(message) || !player.CbtInterface.IsPvp)
                        continue;

                    // Notify RvR flagged players of activity
                    //player.SendLocalizeString(message, ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
                    //player.SendLocalizeString(message, largeFilter, Localized_text.CHAT_TAG_DEFAULT);
                    if (snd != null)
                        player.SendPacket(snd);
                }
            }

            // This is for State of the Realms addon
            UpdateStateOfTheRealmBO();
        }

        public void UpdateStateOfTheRealmBO()
        {
            if (this != null)
            {
                long now = TCPManager.GetTimeStampMS();
                long timer = (_stateTransitionTimer - TCPManager.GetTimeStampMS()) / 1000;

                string stateOfTheRealmMessage = "SoR_T" + _tier + "_BO_Update:" + ZoneId + ":" + ID + ":" + (int)_owningRealm + ":" + (int)FlagState;

                if (timer > 0)
                    stateOfTheRealmMessage = stateOfTheRealmMessage + ":" + timer;
                else
                    stateOfTheRealmMessage = stateOfTheRealmMessage + ":0";

                if (_tier == 4)
                {
                    BattleFrontStatus BattleFrontStatus = BattleFrontService.GetStatusFor(Region.RegionId);
                    if (BattleFrontStatus != null)
                        stateOfTheRealmMessage = stateOfTheRealmMessage + ":" + BattleFrontStatus.OpenZoneIndex;
                    else
                        stateOfTheRealmMessage = stateOfTheRealmMessage + ":-1";
                }

                if (stateOfTheRealmMessage != "")
                {
                    foreach (Player player in Player._Players.ToList())
                    {
                        if (player != null && player.SoREnabled)
                            player.SendLocalizeString(stateOfTheRealmMessage, ChatLogFilters.CHATLOGFILTERS_CHANNEL_9, Localized_text.CHAT_TAG_DEFAULT);
                    }
                }
            }
        }

        public void SendFlagInfo(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_INFO, 32);
            Out.WriteUInt32((uint)ID);
            Out.WriteByte(0);
            Out.WriteByte((byte)_owningRealm);
            Out.WriteByte(1);
            Out.WriteUInt16(0);
            Out.WritePascalString(ObjectiveName);
            Out.WriteByte(2);
            Out.WriteUInt32(0x0000348F);
            Out.WriteByte((byte)_assaultingRealm);

            // Expansion for objective goal
            Out.WriteByte(0);

            Out.WriteUInt16(0xFF00);
            Out.WritePascalString(GetStateText(plr.Realm));
            Out.WriteByte(0);

            ushort transitionTimer = (_stateTransitionTimer == 0 ? (ushort)0 : (ushort)((_stateTransitionTimer - TCPManager.GetTimeStampMS()) / 1000));

            switch (FlagState)
            {
                case ObjectiveFlags.ZoneLocked:
                    if (_owningRealm == Realms.REALMS_REALM_NEUTRAL)
                        Out.WritePascalString("This Battlefield Objective will be capturable soon!");
                    else
                        Out.WritePascalString($"This area has been captured by {(_owningRealm == Realms.REALMS_REALM_DESTRUCTION ? "Destruction" : "Order")}. The battle wages on elsewhere!");
                    break;


                case ObjectiveFlags.Locked:
                    Out.WritePascalString($"This Battlefield Objective has recently been captured and is generating resources securely for {(_owningRealm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction")}. Once the timer elapses, this Battlefield Objective will become vulnerable to assault!");
                    break;


                case ObjectiveFlags.Contested:
                    if (plr.Realm != _owningRealm)
                        Out.WritePascalString($"This Battlefield Objective is being assaulted by {(_owningRealm == Realms.REALMS_REALM_DESTRUCTION ? "Order" : "Destruction")}. Ensure the timer elapses to claim it for your Realm!");
                    else
                        Out.WritePascalString($"This Battlefield Objective is being assaulted by {(_owningRealm == Realms.REALMS_REALM_DESTRUCTION ? "Order" : "Destruction")}. Interact with the flag to reclaim this Battlefield Objective for {(plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? "Destruction" : "Order")}!");
                    
                    break;

                case ObjectiveFlags.Open:
                    if (_owningRealm == Realms.REALMS_REALM_NEUTRAL)
                        Out.WritePascalString($"Interact with the flag to claim this Battlefield Objective for {(plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? "Destruction" : "Order")}!");
                    else if (plr.Realm == _owningRealm)
                        Out.WritePascalString($"This Battlefield Objective is generating resources for {(_owningRealm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction")}. Defend the flag from enemy assault!");
                    else
                        Out.WritePascalString($"This Battlefield Objective is generating resources for {(_owningRealm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction")}. Interact with the flag to claim this Battlefield Objective for {(_owningRealm == Realms.REALMS_REALM_ORDER ? "Destruction" : "Order")}!");
                    break;
            }

            Out.WriteUInt32(transitionTimer);
            Out.WriteUInt32(transitionTimer);
            Out.Fill(0, 4);
            Out.WriteByte(0x71);
            Out.WriteByte(1);
            Out.Fill(0, 3);

            plr.SendPacket(Out);
        }
        public void SendFlagLeft(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_UPDATE, 8);

            Out.WriteUInt32((uint)ID);
            Out.WriteUInt32(0);
            plr.SendPacket(Out);
        }

        #endregion

        #region Zone Locking

        /// <summary>
        /// Prevents this objective from being captured.
        /// </summary>
        public void LockObjective(Realms lockingRealm, bool announce)
        {
            _owningRealm = lockingRealm;
            _assaultingRealm = Realms.REALMS_REALM_NEUTRAL;

            FlagState = ObjectiveFlags.ZoneLocked;
            _stateTransitionTimer = 0;
            AccumulatedKills = 0;

            foreach (RvRFlagGuard guard in Guards)
                guard.SpawnGuard(0);

            if (!announce)
                return;
            foreach (Player plr in PlayersInRange)
            {
                SendMeTo(plr);
                SendFlagInfo(plr);
            }

            SendFlagState(null, false);

            _delayedRewards.Clear();
        }

        /// <summary>
        /// Sets the realm as neutral, and prevents this objective from being captured for a time.
        /// </summary>
        public void OpenObjective(Realms desiredRealm, long timer)
        {
            ChangeOwnership(desiredRealm);

            FlagState = ObjectiveFlags.Locked;
            _stateTransitionTimer = TCPManager.GetTimeStampMS() + timer;

            foreach (Player plr in PlayersInRange)
            {
                SendMeTo(plr);
                SendFlagInfo(plr);
            }

            SendFlagState(null, false);
        }

        /// <summary>
        /// Allows this objective to be captured if it was previously locked.
        /// </summary>
        public void UnlockObjective()
        {
            FlagState = ObjectiveFlags.Open;

            foreach (RvRFlagGuard guard in Guards)
                guard.SpawnGuard(0);

            ChangeOwnership(Realms.REALMS_REALM_NEUTRAL);

            _stateTransitionTimer = 0;
            AccumulatedKills = 0;

            foreach (Player plr in PlayersInRange)
            {
                SendMeTo(plr);
                SendFlagInfo(plr);
            }

            SendFlagState(null, false);
        }

        #endregion

        #region Quadrant history

        /// <summary>
        /// A history of factor of population within this BO's quadrant.
        /// </summary>
        private readonly float[][] _controlRatioHistory = {new float[5], new float[5]};
        /// <summary>
        /// A history of factor of population within this BO's close control radius.
        /// </summary>
        private readonly float[][] _closeControlRatioHistory = { new float[5], new float[5] };
        /// <summary>
        /// A factor derived from the population distribution within this quadrant to determine the lock timer's duration.
        /// </summary>
        private readonly float[] _adjustedAverageControlRatios = new float[2];
        /// <summary>
        /// The highest control factor for a realm on this objective within the last 5 minutes. Used to detect movement of zergs.
        /// </summary>
        private readonly float[] _controlHighs = new float[2];
        private readonly int[] _quadrantPop = new int[2];
        private readonly int[] _quadrantClosePop = new int[2];

        private int _curIndex;
        const int MAX_HISTORY_INDEX = 5;

        public void AdvancePopHistory(int orderCount, int destroCount)
        {
            _controlRatioHistory[0][_curIndex] = orderCount == 0 ? 0 : (float) _quadrantPop[0]/orderCount;
            _controlRatioHistory[1][_curIndex] = destroCount == 0 ? 0 : (float) _quadrantPop[1]/destroCount;

            _closeControlRatioHistory[0][_curIndex] = orderCount == 0 ? 0 : (float) _quadrantClosePop[0]/orderCount;
            _closeControlRatioHistory[1][_curIndex] = destroCount == 0 ? 0 : (float) _quadrantClosePop[1]/destroCount;

            for (int i = 0; i < 2; ++i)
            {
                _adjustedAverageControlRatios[i] = 0f;
                _controlHighs[i] = 0f;

                for (int j = 0; j < MAX_HISTORY_INDEX; ++j)
                {
                    float curCtrl = _controlRatioHistory[i][j];

                    if (curCtrl <= 0.15f)
                        _adjustedAverageControlRatios[i] += curCtrl*1.6f;
                    else if (curCtrl <= 0.30f)
                        _adjustedAverageControlRatios[i] += 0.25f;
                    else if (curCtrl <= 0.55f)
                        _adjustedAverageControlRatios[i] += 0.25f - Math.Max(0, (curCtrl - 0.30f));

                    if (_closeControlRatioHistory[i][j] > _controlHighs[i])
                        _controlHighs[i] = curCtrl;
                }
            }

            ++_curIndex;

            if (_curIndex == MAX_HISTORY_INDEX)
                _curIndex = 0;

            #if QUADRANT_DEBUG
            if (!Region.Bttlfront.NoSupplies)
                Log.Info(ObjectiveName+"'s quadrant", $"Updated pop history - quadrant population: {_quadrantPop[0]}, {_quadrantPop[1]} - total pop: {orderCount}, {destroCount} - adjusted control factors: {_adjustedAverageControlRatios[0]}, {_adjustedAverageControlRatios[1]}");
            #endif

            _quadrantPop[0] = 0;
            _quadrantPop[1] = 0;

            _quadrantClosePop[0] = 0;
            _quadrantClosePop[1] = 0;
        }

        private float _pendingLockScaler;

        public float GetContestedScaler(Realms attackingRealm)
        {
            int alliedRealm = (int)attackingRealm - 1;
            int enemyRealm = 1 - alliedRealm;

            // Check enemy control factor for this area.
            // Higher factors indicate acceptable control levels, increasing the contested timer.
            float lockFactor = 1 + 2 * _adjustedAverageControlRatios[enemyRealm];

            // Get area control immediately, as zerg wave may have entered location.
            float alliedControlHigh = Math.Max(Region.Bttlfront.GetControlHighFor(this, attackingRealm), _controlHighs[alliedRealm]);

            // If allied control maximum for this area is very high, and higher than enemy control factor,
            // then the contested timer scales up.
            if (alliedControlHigh > 0.2f && alliedControlHigh > _controlHighs[enemyRealm])
                lockFactor *= alliedControlHigh / Math.Max(0.2f, _controlHighs[enemyRealm]);

            _pendingLockScaler = GetLockScaler(attackingRealm);

            return Math.Min(lockFactor, 6);
        }

        public float GetLockScaler(Realms realm)
        {
            if (_pendingLockScaler > 0f)
            {
                float lockScale = _pendingLockScaler;
                _pendingLockScaler = 0f;
                return lockScale;
            }

            int alliedRealm = (int) realm - 1;
            int enemyRealm = 1 - alliedRealm;

            // Check enemy control factor for this area.
            // Higher factors indicate acceptable control levels, reducing the lock timer.
            float lockFactor = 1 - MAX_LOCK_REDUCTION *_adjustedAverageControlRatios[enemyRealm];

            #if QUADRANT_DEBUG
            Log.Info(ObjectiveName, "Base lock factor: "+lockFactor);
            #endif

            // If allied control maximum for this area is very high, and higher than enemy control factor,
            // then lock timer is scaled down.
            if (_controlHighs[alliedRealm] > 0.2f && _controlHighs[alliedRealm] > _controlHighs[enemyRealm])
                lockFactor *= Math.Max(0.2f, _controlHighs[enemyRealm])/_controlHighs[alliedRealm];

            #if QUADRANT_DEBUG
            Log.Info(ObjectiveName, "After relative control highs adjustment: " + lockFactor);
            #endif

            return Math.Max(lockFactor, 0.16f);
        }

        public float GetReclaimLockScaler(Realms realm)
        {
            _pendingLockScaler = 0f;

            int alliedRealm = (int)realm - 1;
            int enemyRealm = 1 - alliedRealm;

            // Check enemy control factor for this area.
            // Higher factors indicate acceptable control levels, reducing the lock timer.
            float lockFactor = 1f;

            // If allied control maximum for this area is very high, and higher than enemy control factor,
            // then lock timer is scaled down.
            if (_controlHighs[alliedRealm] > 0.2f && _controlHighs[alliedRealm] > _controlHighs[enemyRealm])
                lockFactor *= Math.Max(0.2f, _controlHighs[enemyRealm]) / _controlHighs[alliedRealm];

            #if QUADRANT_DEBUG
            Log.Info(ObjectiveName, "After relative control highs adjustment: " + lockFactor);
            #endif

            return Math.Max(lockFactor, 0.16f);
        }

        const int OBJECTIVE_CLOSE_RADIUS = 600;

        public void AddPlayerInQuadrant(Player player)
        {
            ++_quadrantPop[(int) player.Realm - 1];

            if (player.IsWithinRadiusFeet(this, OBJECTIVE_CLOSE_RADIUS))
                ++_quadrantClosePop[(int) player.Realm - 1];
        }

        #endregion

        public void SendDiagnostic(Player plr)
        {
            plr.SendClientMessage($"[{ObjectiveName}]", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            plr.SendClientMessage($"{Enum.GetName(typeof(ObjectiveFlags), FlagState)} and held by {(_owningRealm == Realms.REALMS_REALM_NEUTRAL ? "no realm" : (_owningRealm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction"))}");
            if (_assaultingRealm != Realms.REALMS_REALM_NEUTRAL)
            {
                plr.SendClientMessage($"Assaulting realm: {(_assaultingRealm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction")}");
                if (FlagState != ObjectiveFlags.Contested)
                {
                    plr.SendClientMessage("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                    plr.SendClientMessage("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                    plr.SendClientMessage("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                    plr.SendClientMessage("BUGGED FLAG STATE!");
                    plr.SendClientMessage("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                    plr.SendClientMessage("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                    plr.SendClientMessage("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                }
            }

            if (_stateTransitionTimer > 0)
                plr.SendClientMessage($"To next state change: {(_stateTransitionTimer - TCPManager.GetTimeStampMS()) / 1000} seconds.");

            if (GeneratingSupplies)
                plr.SendClientMessage($"Generation timer end: {_generationTimerEnd - TCPManager.GetTimeStamp()} seconds.");
            plr.SendClientMessage($"Control highs: Order {_controlHighs[0].ToString("P")}, Destruction {_controlHighs[1].ToString("P")}");
            plr.SendClientMessage($"Adjusted average control: Order {_adjustedAverageControlRatios[0].ToString("P")}, Destruction {_adjustedAverageControlRatios[1].ToString("P")}");
            plr.SendClientMessage($"Order control history: {_controlRatioHistory[0][0].ToString("P")}, {_controlRatioHistory[0][1].ToString("P")}, {_controlRatioHistory[0][2].ToString("P")}, {_controlRatioHistory[0][3].ToString("P")}, {_controlRatioHistory[0][4].ToString("P")}");
            plr.SendClientMessage($"Destruction control history: {_controlRatioHistory[1][0].ToString("P")}, {_controlRatioHistory[1][1].ToString("P")}, {_controlRatioHistory[1][2].ToString("P")}, {_controlRatioHistory[1][3].ToString("P")}, {_controlRatioHistory[1][4].ToString("P")}");
        }
    }
}