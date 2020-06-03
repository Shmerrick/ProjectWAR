//#define ABILITY_DEVELOPMENT

using System;
using System.Collections.Generic;
using SystemData;
using FrameWork;
using GameData;
using WorldServer.NetWork;
using WorldServer.World.Abilities.CareerInterfaces;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using WorldServer.World.Positions;
using Item = WorldServer.World.Objects.Item;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Abilities
{
    public class AbilityInterface : BaseInterface
    {
        #region Static
        const ushort GLOBAL_COOLDOWN = 1500;
        // The client attempts to fire abilities 75% of the way through its global cooldown. This compensates for the fact.
        const long COOLDOWN_GRACE = 400;
        public static bool PreventCasting;
        #endregion

        private Unit _unitOwner;
        private Player _playerOwner;

        private List<AbilityInfo> _abilities = new List<AbilityInfo>();

        ///<summary>The set of abilities that are actually castable by this player.</summary>
        private readonly HashSet<ushort> _abilitySet = new HashSet<ushort>();

        private readonly ushort[] _morales = new ushort[4];

        private AbilityProcessor _abilityProcessor;
        public AbilityProcessor GetAbiityProcessor() { return _abilityProcessor; }

        #region Init/Storage

        public override void SetOwner(Object owner)
        {
            _Owner = owner;
            _unitOwner = _Owner as Unit;
            _playerOwner = _Owner as Player;

            if (_Owner is Pet)
            {
                _abilityProcessor = new AbilityProcessor(_unitOwner, this);
                NPCAbilities = new List<NPCAbility>();
            }
        }

        public override bool Load()
        {
            if (_playerOwner != null)
            {
                InitializeCareerMastery();
                LoadCareerAbilities();
                RefreshBonusMasteryPoints();

                _abilityProcessor = new AbilityProcessor(_unitOwner, this);
            }

            Creature creature = _Owner as Creature;

            if (creature != null && !(_Owner is Pet))
                NPCAbilities = AbilityMgr.GetCreatureAbilities(creature.Spawn.Proto.Entry);

            _Owner.EvtInterface.AddEventNotify(EventName.OnMove, OnPlayerMoved);

            return base.Load();
        }

        private void LoadCareerAbilities()
        {
            _abilities = AbilityMgr.GetAvailableCareerAbilities(_playerOwner.Info.CareerLine, 0, _playerOwner.Level);

            foreach (AbilityInfo ab in _abilities)
                _abilitySet.Add(ab.Entry);

            List<AbilityInfo> masteryAbilities = AbilityMgr.GetMasteryAbilities(_playerOwner.Info.CareerLine);

            foreach (AbilityInfo ab in masteryAbilities)
            {
                byte entry = (byte)((ab.ConstantInfo.PointCost - 1) / 2 - 1);
                _masteryAbilities[ab.ConstantInfo.MasteryTree - 1, entry] = ab;

                if (_activeSkillsInTree[ab.ConstantInfo.MasteryTree - 1, entry] == 1)
                {
                    _abilities.Add(ab);
                    _abilitySet.Add(ab.Entry);
                }
            }
        }

        public void SendAbilityLevels()
        {
            if (!HasPlayer())
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_CHARACTER_INFO, 4 + _abilities.Count * 3);
            Out.WriteByte(1); // Action
            Out.WriteByte((byte)_abilities.Count);
            Out.WriteUInt16(0x300);

            foreach (var abInfo in _abilities/*.Where(IsValidAbility)*/) // Could cause invalid length under debolster conditions
            {
                Out.WriteUInt16(abInfo.Entry);
                Out.WriteByte(GetMasteryLevelFor(abInfo.ConstantInfo.MasteryTree));
            }

            GetPlayer().SendPacket(Out);
        }

        public void SendTest(ushort newEnt)
        {
            if (!HasPlayer())
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_CHARACTER_INFO, 4 + _abilities.Count * 3);
            Out.WriteByte(1); // Action
            Out.WriteByte((byte)(_abilities.Count + 1));
            Out.WriteUInt16(0x300);

            foreach (var abInfo in _abilities /*.Where(IsValidAbility)*/)
            {
                Out.WriteUInt16(abInfo.Entry);
                Out.WriteByte(GetMasteryLevelFor(abInfo.ConstantInfo.MasteryTree));
            }

            Out.WriteUInt16(newEnt);
            Out.WriteByte(40);

            GetPlayer().SendPacket(Out);
        }

        #endregion

        #region Events

        /// <summary>
        /// Takes old level and new level to handle .modify level
        /// </summary>
        /// <param name="oldLevel"></param>
        /// <param name="newLevel"></param>
        public void OnPlayerLeveled(byte oldLevel, byte newLevel)
        {
            List<AbilityInfo> newAbilities = AbilityMgr.GetAvailableCareerAbilities(((Player)_unitOwner).Info.CareerLine, oldLevel + 1, newLevel);

            _abilities.AddRange(newAbilities);

            foreach (AbilityInfo ab in newAbilities)
                _abilitySet.Add(ab.Entry);

            SendMasteryPointsUpdate();
            SendAbilityLevels();
        }

        public bool OnPlayerMoved(Object sender, object args)
        {
            if (_abilityProcessor != null && _abilityProcessor.HasInfo())
                _abilityProcessor.CheckMoveInterrupt();

            return false;
        }

        public void OnPlayerCCed()
        {
            _abilityProcessor?.CheckBlockedByCC();
        }

        public void OnPlayerHit()
        {
            if (_abilityProcessor != null && _abilityProcessor.HasInfo())
                _abilityProcessor.AddSetback((byte)(StaticRandom.Instance.Next(100)));
        }

        public void AssignMorale(ushort moraleEntry, byte slot)
        {
            _morales[slot - 1] = moraleEntry;
        }

        #endregion

        #region Validation

        /// <summary>Run to check whether an ability called through the DO_ABILITY packet is valid.</summary>
        public bool IsValidAbility(AbilityInfo info)
        {
            if (!HasPlayer())
                return true;

            Player plr = GetPlayer();

            // Lock out abilities the player shouldn't have access to
            if (!_abilitySet.Contains(info.Entry))
                return false;

            if (info.ConstantInfo.MinimumRank > plr.AdjustedLevel || info.ConstantInfo.MinimumRenown > plr.RenownRank)
                return false;

            if (info.CareerLine != 0 && (plr.Info.CareerFlags & info.ConstantInfo.CareerLine) == 0)
                return false;

            return true;
        }

        public bool IsValidMorale(ushort moraleEntry, byte slot)
        {
            if (_morales[slot - 1] == moraleEntry)
                return true;

            _playerOwner.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_PLAYER_MORALE_ABILITY_MUST_BE_READY);

            return false;
        }

        public bool IsValidTactic(ushort tacticEntry)
        {
            AbilityConstants constInfo = AbilityMgr.GetAbilityInfo(tacticEntry).ConstantInfo;

            if (constInfo.MinimumRank > _unitOwner.AdjustedLevel)
                return false;

            if (constInfo.MasteryTree == 0)
                return true;

            int destTree = constInfo.MasteryTree - 1;

            for (int i = 0; i < MAX_TREE_ABILITIES; ++i)
            {
                if (_masteryAbilities[destTree, i] != null && _masteryAbilities[destTree, i].Entry == tacticEntry)
                    return _activeSkillsInTree[destTree, i] == 1;
            }

            return false;
        }

        #endregion

        public bool IsCasting()
        {
            return _abilityProcessor != null && _abilityProcessor.HasInfo();
        }

        public override void Update(long tick)
        {
            _abilityProcessor?.Update(tick);

            base.Update(tick);

            if (MasteryChanged)
            {
                SendMasteryPointsUpdate();
                SendAbilityLevels();

                // Check for debolster, and notify if required
                if (_playerOwner.AdjustedLevel < _playerOwner.Level)
                    _playerOwner.CheckDebolsterValid();

                MasteryChanged = false;
            }
        }

        public override void Stop()
        {
            _abilityProcessor?.Stop();

            base.Stop();
        }

        #region Granted Abilities

        public void GrantAbility(ushort abilityEntry)
        {
            _abilities.Add(AbilityMgr.GetAbilityInfo(abilityEntry));
            _abilitySet.Add(abilityEntry);

            SendAbilityLevels();

            ResendCooldown(abilityEntry);
        }

        public void RemoveGrantedAbility(ushort abilityEntry)
        {
            _abilities.RemoveAll(x => x.Entry == abilityEntry);
            _abilitySet.Remove(abilityEntry);

            PacketOut Out = new PacketOut((byte)Opcodes.F_CHARACTER_INFO, 32);

            Out.WriteByte(0x0B);
            Out.WriteByte(1);
            Out.WriteUInt16(abilityEntry);

            ((Player)_unitOwner).SendPacket(Out);
        }

        #endregion

        #region NPC Abilities

        public List<NPCAbility> NPCAbilities { get; set; }

        /// <summary>
        /// Updates the "autouse" flag for the given ability.
        /// </summary>
        /// <param name="abilityId">Identfier of the ability to update</param>
        /// <param name="autoUse">True to set autouse, false otherwise</param>
        /// <remarks>Has no effect is ability is not set for this creature</remarks>
        public void SetAutoUse(ushort abilityId, bool autoUse)
        {
            foreach (var ab in NPCAbilities)
            {
                if (abilityId == ab.Entry)
                {
                    ab.AutoUse = autoUse;
                    break;
                }
            }
        }

        #endregion

        #region AbilityCast

        public bool StartCast(Unit instigator, ushort abilityId, byte castSequence, byte cooldownGroup = 0, byte overrideAbilityLevel = 0, bool enemyVisible = true, bool friendlyVisible = true, bool moving = false)
        {
            if (PreventCasting)
            {
                if (_Owner is Player)
                    (_Owner as Player)?.SendClientMessage("A developer has disabled all abilities.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return false;
            }

            // Allow only interruption of channeled skills of a different ID to the skill being used
            if (IsCasting() && (!_abilityProcessor.IsChannelling || _abilityProcessor.AbInfo.Entry == abilityId))
                return false;

            AbilityInfo abInfo = AbilityMgr.GetAbilityInfo(abilityId);

            if (abInfo == null || (abInfo.ConstantInfo.Origin != AbilityOrigin.AO_ITEM && !IsValidAbility(abInfo)))
                return false;

            //Fix so that WE/WH cant use all their 3 openers at the same time, this is in conjunction with whats in AbilityProcessor
            if (_Owner is Player)
            {
                if ((_Owner as Player).StealthLevel == 0 && (abilityId == 9406 || abilityId == 9401 || abilityId == 9411 || abilityId == 8091 || abilityId == 8096 || abilityId == 8098))
                {
                    return false;
                }
            }

            try
            {

                if (AbilityMgr.HasCommandsFor(abilityId) || abInfo.ConstantInfo.ChannelID != 0)
                {
                    if (_abilityProcessor == null)
                        _abilityProcessor = new AbilityProcessor(_unitOwner, this);

                    abInfo.Instigator = instigator;
                    abInfo.Level = overrideAbilityLevel;

                        return _abilityProcessor.StartAbility(abInfo, castSequence, cooldownGroup, enemyVisible, friendlyVisible, moving);
                }
                if (_Owner is Player)
                {
                    Player owner = _Owner as Player;
                    owner?.SendClientMessage(abilityId + " " + AbilityMgr.GetAbilityNameFor(abilityId) + " has no implementation.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                }
                return false;
            }

            catch (Exception e)
            {
                if (_Owner is Player)
                    (_Owner as Player)?.SendClientMessage(abilityId + " " + AbilityMgr.GetAbilityNameFor(abilityId) + " threw an unhandled " + e.GetType().Name + " from " + e.TargetSite + ".");
                Log.Error("Ability System", e.ToString());
                return false;
            }
        }

        public bool StartCastAtPos(Unit instigator, ushort abilityID, Point3D worldPos, ushort zoneId, byte castSequence)
        {
            if (PreventCasting)
            {
                if (_Owner is Player)
                {
                    Player owner = _Owner as Player;
                    owner?.SendClientMessage("A developer has disabled all abilities.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                }
                return false;
            }

            // Allow only interruption of channeled skills of a different ID to the skill being used
            if (IsCasting() && (!_abilityProcessor.IsChannelling || _abilityProcessor.AbInfo.Entry == abilityID))
                return false;

            AbilityInfo abInfo = AbilityMgr.GetAbilityInfo(abilityID);

            if (abInfo == null || (abInfo.ConstantInfo.Origin != AbilityOrigin.AO_ITEM && !IsValidAbility(abInfo)))
                return false;
            try
            {
                if (AbilityMgr.HasCommandsFor(abilityID) || abInfo.ConstantInfo.ChannelID != 0)
                {
                    if (_abilityProcessor == null)
                        _abilityProcessor = new AbilityProcessor(_unitOwner, this);

                    abInfo.Instigator = instigator;

                    if (!_abilityProcessor.HasInfo())
                        _abilityProcessor.StartAbilityAtPos(abInfo, castSequence, worldPos, zoneId);
                    return true;
                }
                if (_Owner is Player)
                {
                    var owner = _Owner as Player;
                    owner?.SendClientMessage(abilityID + " " + AbilityMgr.GetAbilityNameFor(abilityID) + " has no implementation.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                }
                return false;
            }

            catch (Exception e)
            {
                if (_Owner is Player)
                {
                    var owner = _Owner as Player;
                    owner?.SendClientMessage(abilityID + " " + AbilityMgr.GetAbilityNameFor(abilityID) + " threw an unhandled " + e.GetType().Name + " from " + e.TargetSite + ".");
                }
                return false;
            }
        }

        public void Cancel(bool force, ushort messageCode = 0)
        {
            if (_abilityProcessor != null && _abilityProcessor.HasInfo())
                _abilityProcessor.CancelCast(messageCode, force);
        }

        public void NotifyCancelled()
        {
            if (_abilityProcessor != null && _abilityProcessor.HasInfo())
                _abilityProcessor.NotifyCancelled((ushort)AbilityResult.ABILITYRESULT_INTERRUPTED);
        }

        #endregion

        #region Cooldowns

        public Dictionary<ushort, long> Cooldowns = new Dictionary<ushort, long>();

        public Dictionary<byte, long> ItemGroupCooldowns = new Dictionary<byte, long>();

        public bool IsOnCooldown(AbilityInfo abInfo)
        {
            return (!CanCastCooldown(0) && !abInfo.ConstantInfo.IgnoreGlobalCooldown) || !CanCastCooldown(abInfo.ConstantInfo.CooldownEntry != 0 ? abInfo.ConstantInfo.CooldownEntry : abInfo.Entry);
        }

        public void SetItemGroupCooldown(byte cooldownGroupId, ushort duration)
        {
            if (cooldownGroupId == byte.MaxValue)
                return;
            if (!ItemGroupCooldowns.ContainsKey(cooldownGroupId))
                ItemGroupCooldowns.Add(cooldownGroupId, duration * 1000 + TCPManager.GetTimeStampMS());
            else
                ItemGroupCooldowns[cooldownGroupId] = duration * 1000 + TCPManager.GetTimeStampMS();

            _unitOwner.ItmInterface.SendItemGroupCooldown(cooldownGroupId, duration);
        }

        public void SetItemCooldown(ushort abilityId, ushort duration, bool silent = false)
        {
            if (abilityId == ushort.MaxValue)
                return;
            if (!Cooldowns.ContainsKey(abilityId))
                Cooldowns.Add(abilityId, duration * 1000 + TCPManager.GetTimeStampMS());
            else
                Cooldowns[abilityId] = duration * 1000 + TCPManager.GetTimeStampMS();

            _unitOwner.ItmInterface.SendItemCooldown(abilityId, duration);
        }

        /// <summary>
        /// Sets and transmits the cooldown of the given ability.
        /// </summary>
        /// <param name="abilityId">Ifd of ability to reset</param>
        /// <param name="duration">Cooldown duration in milliseconds, -1 to reset</param>
        /// <param name="silent">False to transmit new value to client, true otherwise</param>
        public void SetCooldown(ushort abilityId, long duration, bool silent = false)
        {
#if DEBUG && ABILITY_DEVELOPMENT
            duration = 0;
#endif

            if (abilityId == ushort.MaxValue)
                return;
            AbilityInfo abInfo = AbilityMgr.GetAbilityInfo(abilityId);
            if (abInfo.IgnoreCooldownReduction == 1 && (abInfo.Cooldown * 1000) >= duration)
            {
                long nextTimestamp = 0;
                if (abInfo.CDcap != 0 && abInfo.CDcap * 1000 > duration)
                    nextTimestamp = (abInfo.CDcap * 1000) + TCPManager.GetTimeStampMS();

                else
                    nextTimestamp = (abInfo.Cooldown * 1000) + TCPManager.GetTimeStampMS();

                Cooldowns[abilityId] = nextTimestamp;

                if (silent)
                    return;

                PacketOut Out = new PacketOut((byte)Opcodes.F_SET_ABILITY_TIMER, 12);
                Out.WriteUInt16(abilityId);
                Out.Fill(0, 2);
                Out.WriteUInt32(((uint)abInfo.Cooldown * 1000));
                Out.Fill(0, 4);
                if (_Owner.IsPet())
                    _Owner.GetPet().Owner.SendPacket(Out);
                else
                    _playerOwner?.SendPacket(Out);
            }

            if (abInfo.IgnoreCooldownReduction != 1 || (abInfo.IgnoreCooldownReduction == 1 && (abInfo.Cooldown * 1000) < duration))
            {
                long nextTimestamp = 0;

                if (abInfo.CDcap != 0 && abInfo.CDcap * 1000 > duration)
                    nextTimestamp = (abInfo.CDcap * 1000) + TCPManager.GetTimeStampMS();

                else
                    nextTimestamp = (duration) + TCPManager.GetTimeStampMS();

                if (duration == -1 && abInfo.CDcap == 0)
                    nextTimestamp = 0;
                Cooldowns[abilityId] = nextTimestamp;

                if (silent)
                    return;

                PacketOut Out = new PacketOut((byte)Opcodes.F_SET_ABILITY_TIMER, 12);
                Out.WriteUInt16(abilityId);
                Out.Fill(0, 2);
                Out.WriteUInt32(duration != -1 ? (uint)duration : 0);
                Out.Fill(0, 4);
                if (_Owner.IsPet())
                    _Owner.GetPet().Owner.SendPacket(Out);
                else
                    _playerOwner?.SendPacket(Out);
            }

        }

        /// <summary>
        /// Resets all cooldowns of the given creature (player...).
        /// </summary>
        /// <remarks>Do not reset self rezs to prevent any abuse</remarks>
        public void ResetCooldowns()
        {
            ICollection<ushort> abilityIds = new List<ushort>(Cooldowns.Keys);
            foreach (ushort abilityId in abilityIds)
                if (abilityId != 0 && abilityId != 8567 && abilityId != 1608) // zeal & runie self rezs, 0 no idea
                    SetCooldown(abilityId, -1);
            abilityIds.Clear();
        }

        public void ResendCooldown(ushort abilityId)
        {
            uint duration = 0;

            if (Cooldowns.ContainsKey(abilityId))
                duration = (uint)(Cooldowns[abilityId] - TCPManager.GetTimeStampMS());

            PacketOut Out = new PacketOut((byte)Opcodes.F_SET_ABILITY_TIMER, 12);
            Out.WriteUInt16(abilityId);
            Out.Fill(0, 2);
            Out.WriteUInt32(duration);
            Out.Fill(0, 4);

            _playerOwner.SendPacket(Out);
        }

        public void SetGlobalCooldown()
        {
            if (!Cooldowns.ContainsKey(0))
                Cooldowns.Add(0, GLOBAL_COOLDOWN + TCPManager.GetTimeStampMS());
            else
                Cooldowns[0] = GLOBAL_COOLDOWN + TCPManager.GetTimeStampMS();
        }

        public bool CanCastCooldown(ushort abilityId)
        {
            long time;

            if (abilityId == ushort.MaxValue)
                return true;
            if (!Cooldowns.TryGetValue(abilityId, out time))
                return true;

            return time - COOLDOWN_GRACE <= TCPManager.GetTimeStampMS();
        }

        public bool CanCastItemGroupCooldown(byte groupCooldownId)
        {
            long time;

            if (!ItemGroupCooldowns.TryGetValue(groupCooldownId, out time))
                return true;

            return time - COOLDOWN_GRACE <= TCPManager.GetTimeStampMS();
        }

        public bool IsOnGlobalCooldown()
        {
            return !CanCastCooldown(0);
        }

        public void AssignItemCooldown(Item item)
        {
            long curCooldownMS;

            if (item.Info.Unk27[19] != 0)
                ItemGroupCooldowns.TryGetValue(item.Info.Unk27[19], out curCooldownMS);
            else
                Cooldowns.TryGetValue(item.Info.SpellId, out curCooldownMS);

            if (curCooldownMS == 0)
                return;

            item.CharSaveInfo.NextAllowedUseTime = curCooldownMS / 1000;
        }

        #endregion

        #region Mastery

        private const byte MAX_TREE_COUNT = 3;
        private const byte MAX_TREE_ABILITIES = 7;

        public bool MasteryChanged { get; set; }
        private readonly AbilityInfo[,] _masteryAbilities = new AbilityInfo[MAX_TREE_COUNT, MAX_TREE_ABILITIES];

        public readonly byte[] _pointsInTree = new byte[MAX_TREE_COUNT];
        private readonly byte[,] _activeSkillsInTree = new byte[MAX_TREE_COUNT, MAX_TREE_ABILITIES];

        private byte[] _bonusMasteryPoints = { 0, 0, 0 };

        // RB   4/9/2016    Need a way for ItemsInterface to update these values on demand
        /// <summary>
        /// Updates the mastery trees to reflect changes in bonus mastery points.
        /// </summary>
        public void RefreshBonusMasteryPoints()
        {
            byte[] oldBonus = (byte[])_bonusMasteryPoints.Clone();

            _bonusMasteryPoints[0] = Convert.ToByte(_playerOwner.StsInterface.GetBonusStat(Stats.Mastery1Bonus));
            _bonusMasteryPoints[1] = Convert.ToByte(_playerOwner.StsInterface.GetBonusStat(Stats.Mastery2Bonus));
            _bonusMasteryPoints[2] = Convert.ToByte(_playerOwner.StsInterface.GetBonusStat(Stats.Mastery3Bonus));

            for (int i = 0; i < MAX_TREE_COUNT; i++)
                if (oldBonus[i] > _bonusMasteryPoints[i])
                    RemoveMasteryPoints(i, (byte)(oldBonus[i] - _bonusMasteryPoints[i]));

            MasteryChanged = true;
            InitializeCareerMastery();
        }

        // RB   4/9/2016    Need to be able to remove mastery points from a tree without a full respec.
        /// <summary>
        /// Removes a specified number of mastery points from a tree, and removes invalidated abilities.
        /// </summary>
        /// <param name="masteryTree">Zero-based index of the mastery tree to be altered</param>
        /// <param name="value">The number of mastery points to remove from the tree</param>
        public void RemoveMasteryPoints(int masteryTree, byte value)
        {
            _playerOwner.BuffInterface.RemoveCasterBuffs();

            List<ushort> toRemove = new List<ushort>();

            byte temp = _pointsInTree[masteryTree];

            // RB   5/12/2016   Need to make sure we're not removing more points than are available...
            if (value > _pointsInTree[masteryTree])
                _pointsInTree[masteryTree] = 0;
            else
                _pointsInTree[masteryTree] -= value;

            for (int i = 1; i <= MAX_TREE_ABILITIES; i++)
            {
                if ((_pointsInTree[masteryTree] < ((i * 2) + 1)) && _activeSkillsInTree[masteryTree, i - 1] == 1)
                {
                    toRemove.Add(_masteryAbilities[masteryTree, i - 1].Entry);
                    _activeSkillsInTree[masteryTree, i - 1] = 0;
                }
            }

            if (toRemove.Count > 0)
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_CHARACTER_INFO, 32);

                Out.WriteByte(0x0B);
                Out.WriteByte((byte)toRemove.Count);

                foreach (ushort abilityID in toRemove)
                {
                    Out.WriteUInt16(abilityID);

                    _abilities.RemoveAll(x => x.Entry == abilityID);
                    _abilitySet.Remove(abilityID);
                }

                _playerOwner.SendPacket(Out);
            }


            SaveMastery();
            ReloadMastery();
            SendMasteryPointsUpdate();
            SendAbilityLevels();

            _playerOwner.TacInterface.ValidateTactics();
        }

        public void InitializeCareerMastery()
        {
            // RefreshBonusMasteryPoints();
            if (_playerOwner._Value.MasterySkills == null || _playerOwner._Value.MasterySkills.Length < 2)
            {
                _playerOwner._Value.MasterySkills = (0 + _bonusMasteryPoints[0]) + ";" +
                                                    (0 + _bonusMasteryPoints[1]) + ";" +
                                                    (0 + _bonusMasteryPoints[2]) + ";" +
                                                    "0,0,0,0,0,0,0;0,0,0,0,0,0,0;0,0,0,0,0,0,0"; ;
                SaveMastery();
                return;
            }

            string[] temp = _playerOwner._Value.MasterySkills.Split(';');

            // RB   4/9/2016    Apply free mastery points from gear.
            _pointsInTree[0] = (byte)(Convert.ToByte(temp[0]) + _bonusMasteryPoints[0]);
            _pointsInTree[1] = (byte)(Convert.ToByte(temp[1]) + _bonusMasteryPoints[1]);
            _pointsInTree[2] = (byte)(Convert.ToByte(temp[2]) + _bonusMasteryPoints[2]);

            for (byte i = 0; i < 3; i++)
            {
                var tempSkills = temp[i + 3].Split(',');
                for (int j = 0; j < 7; j++)
                    _activeSkillsInTree[i, j] = Convert.ToByte(tempSkills[j]);
            }
        }

        private void SaveMastery()
        {
            string masString = "";

            // RB   5/23/2016   Do not save bonus mastery points
            for (byte i = 0; i < MAX_TREE_COUNT; i++)
            {
                if ((_pointsInTree[i] - _bonusMasteryPoints[i]) < 0)
                    masString += "0;";
                else
                    masString += (_pointsInTree[i] - _bonusMasteryPoints[i]) + ";";
            }

            for (byte i = 0; i < MAX_TREE_COUNT; ++i)
                for (byte j = 0; j < MAX_TREE_ABILITIES; ++j)
                {
                    masString += _activeSkillsInTree[i, j];

                    if (j == 6)
                    {
                        if (i == 2)
                            break;
                        masString += ";";
                    }

                    else masString += ",";
                }
            _playerOwner._Value.MasterySkills = masString;

            /*if (_playerOwner.Info.CareerLine == 19)
            {
                Pet playerPet = ((CareerInterface_WhiteLion)_playerOwner.CrrInterface).myPet;
                if (playerPet != null)
                    playerPet.Dismiss(null, null);
            } else if (_playerOwner.Info.CareerLine == 8)
            {
                Pet playerPet = ((CareerInterface_SquigHerder)_playerOwner.CrrInterface).myPet;
                if (playerPet != null)
                    playerPet.Dismiss(null, null);
            } */
            if (_playerOwner.Info.CareerLine == 19 || _playerOwner.Info.CareerLine == 8)
            {
                DespawnPet(_playerOwner.Info.CareerLine);
            }
        }

        public void ReloadMastery()
        {
            int pointsSpent = GetTotalSpent();
            int pointsAvailable = GetTotalAvailable();

            if (pointsSpent > pointsAvailable)
            {
                Log.Info("Mastery System", "Points spent exceeded points available (" + pointsAvailable + "). Resetting mastery.");
                RespecializeMastery(true);
            }
            else
                pointsAvailable -= pointsSpent;

            if (pointsAvailable < 0)
                pointsAvailable = 0;

            PacketOut Out = new PacketOut((byte)Opcodes.F_CAREER_CATEGORY, 128);
            Out.WriteByte(7);
            Out.WriteByte(1);
            Out.WriteByte(0);
            Out.WriteByte((byte)(pointsSpent + pointsAvailable));
            Out.WriteByte((byte)pointsAvailable);
            Out.Fill(0, 3);
            Out.WriteUInt32((uint)(pointsSpent * 2000));   //Respec cost
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(0x75);
            Out.WriteByte(0x30);
            Out.WritePascalString(_playerOwner.Info.CareerLine + " Spec");
            Out.WriteByte(0);
            Out.WriteByte(0x18);
            Out.WriteByte(0);
            for (int i = 1; i <= 24; i++)
            {
                Out.WriteByte((byte)i);
                Out.WriteByte(0);
            }
            Out.Fill(0, 2);
            _playerOwner.SendPacket(Out);

            for (byte i = 0; i < MAX_TREE_COUNT; ++i)
            {
                Out = new PacketOut((byte)Opcodes.F_CAREER_PACKAGE_INFO, 128);
                Out.WriteByte(7);
                Out.WriteByte(1);
                Out.WriteByte(0);
                Out.WriteByte((byte)(i + 1));
                Out.Fill(0, 4);

                // RB   4/9/2016    If for whatever reason bonus mastery points push the tree above 15, cap them.
                if (_pointsInTree[i] > 15)
                    _pointsInTree[i] = 15;

                Out.WriteByte(_pointsInTree[i]);

                Out.WriteByte(2);
                Out.Fill(0, 14);
                Out.WriteByte(1);
                Out.WriteByte(1);
                Out.Fill(0, 2);
                Out.WriteByte(2);
                Out.WriteByte((byte)(0x0D + i));
                Out.WriteByte(6);
                Out.Fill(0, 5);
                Out.WriteByte(i == 2 ? (byte)0xFC : (byte)0x0F);
                Out.Fill(0, 5);
                _playerOwner.SendPacket(Out);
            }

            for (int i = 0; i < MAX_TREE_COUNT; i++)
            {
                for (int j = 0; j < MAX_TREE_ABILITIES; j++)
                {
                    Out = new PacketOut((byte)Opcodes.F_CAREER_PACKAGE_INFO, 128);
                    Out.WriteByte(7);
                    Out.WriteByte(1);
                    Out.WriteByte(0);
                    Out.WriteByte((byte)((i * 7) + j + 4));
                    Out.Fill(0, 4);
                    Out.WriteByte(_activeSkillsInTree[i, j]);  // is it selected in the mastery tree?
                    Out.WriteByte(2);
                    Out.Fill(0, 14);
                    Out.WriteByte(1);
                    Out.WriteByte(1);
                    Out.WriteByte(0);
                    Out.WriteByte(0);
                    Out.WriteByte(0x14);
                    Out.WriteByte(0x32);
                    Out.WriteByte(2);
                    Out.Fill(0, 2);
                    Out.WriteUInt16(_masteryAbilities[i, j].Entry);
                    Out.Fill(0, 18);
                    Out.WritePascalString(_masteryAbilities[i, j].Name);
                    Out.WriteByte(1);
                    Out.WriteByte(0);
                    Out.WriteByte((byte)(i + 1));    // what tree  1 2 3
                    Out.WriteByte(_masteryAbilities[i, j].ConstantInfo.PointCost);
                    Out.Fill(0, 4);
                    _playerOwner.SendPacket(Out);

#pragma warning disable CS1030 // Директива #warning
#warning FIXME. This is a skill send packet, but it sends 5 skills - 4 empties and this mastery skill at rank 0. Why is it here? - Az
                    if (_activeSkillsInTree[i, j] == 1)
#pragma warning restore CS1030 // Директива #warning
                    {
                        Out = new PacketOut((byte)Opcodes.F_CHARACTER_INFO, 48);
                        Out.WriteByte(1);
                        Out.WriteByte(5);
                        Out.WriteByte(3); // Skills
                        Out.Fill(0, 3);
                        Out.Fill(0, 10);
                        Out.WriteUInt16(_masteryAbilities[i, j].Entry);
                        Out.WriteByte(0);
                        _playerOwner.SendPacket(Out);
                    }
                }
            }
        }

        public void SendMasteryPointsUpdate()
        {
            int pointsSpent = GetTotalSpent();
            int pointsAvailable = GetTotalAvailable();

            if (pointsSpent > pointsAvailable)
            {
                Log.Info("Mastery System", "Points spent exceeded points available (" + pointsAvailable + "). Resetting mastery.");
                RespecializeMastery(true);
                return;
            }

            pointsAvailable -= pointsSpent;

            if (pointsAvailable < 0)
                pointsAvailable = 0;

            for (byte i = 0; i < MAX_TREE_COUNT; ++i)
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_CAREER_PACKAGE_UPDATE, 64);
                Out.WriteByte(7);
                Out.WriteByte(1);
                Out.WriteByte(1);
                Out.WriteByte(1);
                Out.WriteByte((byte)pointsAvailable);
                Out.Fill(0, 2);
                Out.WriteByte((byte)(i + 1));
                Out.WriteByte(_pointsInTree[i]);
                Out.Fill(0, 3);
                Out.WriteUInt32((uint)(pointsSpent * 2000));
                Out.WriteUInt32(0);

                _playerOwner.SendPacket(Out);
            }

            SendAbilityLevels();
        }

        public int GetTotalSpent()
        {
            int points = 0;

            for (byte i = 0; i < MAX_TREE_COUNT; ++i)
            {
                // Don't count bonus points as points spent.
                points += _pointsInTree[i] - _bonusMasteryPoints[i];

                for (byte j = 0; j < MAX_TREE_ABILITIES; j++)
                    points += _activeSkillsInTree[i, j];
            }

            return points;
        }

        private int GetTotalAvailable()
        {
            int pointsAvailable = 0;

            if (_unitOwner.Level > 20)
                pointsAvailable = (byte)(_unitOwner.Level - 15);
            else if (_unitOwner.Level > 10)
                pointsAvailable = (byte)((_unitOwner.Level - 9) / 2);

            if (_unitOwner.RenownRank >= 40)
            {
                if (_unitOwner.RenownRank >= 70)
                    pointsAvailable += 4;
                else
                    pointsAvailable += (byte)((_unitOwner.RenownRank - 30) / 10);
            }

            return pointsAvailable;
        }

        public byte GetMasteryLevelFor(byte masteryTree)
        {
            Pet pet = _Owner as Pet;

            if (pet?.Owner != null)
                return pet.Owner.AbtInterface.GetMasteryLevelFor(masteryTree);

            if (masteryTree == 0 || _unitOwner.EffectiveLevel < 11)
                return _unitOwner.EffectiveLevel;

            byte curLevel = _unitOwner.Level;
            if (_unitOwner.AdjustedLevel < curLevel)
                curLevel = _unitOwner.AdjustedLevel;
            
            return (byte)(10 + ((curLevel - 10) >> 1) + (_unitOwner.EffectiveLevel - curLevel) + _pointsInTree[masteryTree - 1]);
            
            //// The following correctly displays tooltip information.
            //return (byte)(curLevel + (_pointsInTree[masteryTree - 1] * 2));
        }

        #region Mastery Purchasing

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_BUY_CAREER_PACKAGE, (int)eClientState.Playing, "F_BUY_CAREER_PACKAGE")]
        public static void F_BUY_CAREER_PACKAGE(BaseClient client, PacketIn packet)
        {
            GameClient cclient = (GameClient)client;

            if (cclient.Plr == null)
                return;

            AbilityInterface abInterface = cclient.Plr.AbtInterface;

            byte value = packet.GetUint8();
            byte resource = packet.GetUint8();
            byte unk1 = packet.GetUint8();
            byte tree = packet.GetUint8();

            if (resource != 7) // renown training
            {
                cclient.Plr.RenInterface.PurchaseRenownAbility(resource, tree);
                return;
            }

            if (tree <= 3)
            {
                if (!abInterface.AddPointToTree(tree))
                    return;
            }

            else if (tree <= 24)
            {
                byte targetTree = 1;
                tree -= 3;

                while (tree > 7)
                {
                    tree -= 7;
                    targetTree++;
                }

                abInterface.ActivateSkillInTree(targetTree, tree);
            }

            else
                return;

            abInterface.SaveMastery();
            abInterface.ReloadMastery();
            abInterface.MasteryChanged = true;
        }

        public bool AddPointToTree(byte tree)
        {
            if (_pointsInTree[tree - 1] >= 15)
            {
                _playerOwner.SendClientMessage("You attempted to put more than 15 points into a mastery tree.");
                return false;
            }
            _pointsInTree[tree - 1]++;
            return true;
        }

        public void ActivateSkillInTree(byte tree, byte skill)
        {
            _activeSkillsInTree[tree - 1, skill - 1] = 1;
            _abilities.Add(_masteryAbilities[tree - 1, skill - 1]);
            _abilitySet.Add(_masteryAbilities[tree - 1, skill - 1].Entry);
        }

        #endregion

        public void RespecializeMastery(bool force)
        {
            if (!force && !_playerOwner.CrrInterface.ExperimentalMode)
            {
                if (_playerOwner.ItmInterface.HasItemCountInInventory(129841000, 1))
                {
                    _playerOwner.ItmInterface.RemoveItems(129841000, 1);
                    
                }
                else
                {
                    if (!_playerOwner.HasMoney((uint)(GetTotalSpent() * 2000)))
                        return;
                    _playerOwner.RemoveMoney((uint)(GetTotalSpent() * 2000));
                }

            }

            _playerOwner.BuffInterface.RemoveCasterBuffs();

            // Remove all existing mastery skills
            List<ushort> toRemove = new List<ushort>();

            for (byte i = 0; i < MAX_TREE_COUNT; ++i)
            {
                // Respeccing cannot remove bonus points
                _pointsInTree[i] = _bonusMasteryPoints[i];

                for (byte j = 0; j < MAX_TREE_ABILITIES; ++j)
                    if (_activeSkillsInTree[i, j] == 1)
                    {
                        toRemove.Add(_masteryAbilities[i, j].Entry);
                        _activeSkillsInTree[i, j] = 0;
                    }
            }

            if (toRemove.Count > 0)
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_CHARACTER_INFO, 32);

                Out.WriteByte(0x0B);
                Out.WriteByte((byte)toRemove.Count);

                foreach (ushort abilityID in toRemove)
                {
                    Out.WriteUInt16(abilityID);

                    _abilities.RemoveAll(x => x.Entry == abilityID);
                    _abilitySet.Remove(abilityID);
                }

                _playerOwner.SendPacket(Out);
            }


            _playerOwner._Value.MasterySkills = (0 + _bonusMasteryPoints[0]) + ";" +
                                                (0 + _bonusMasteryPoints[1]) + ";" +
                                                (0 + _bonusMasteryPoints[2]) + ";" +
                                                "0,0,0,0,0,0,0;0,0,0,0,0,0,0;0,0,0,0,0,0,0";

            ReloadMastery();
            SendMasteryPointsUpdate();
            SendAbilityLevels();

            _playerOwner.TacInterface.ValidateTactics();

            if (_playerOwner.Info.CareerLine == 19 || _playerOwner.Info.CareerLine == 8)
            {
                DespawnPet(_playerOwner.Info.CareerLine);
            }
        }

        public void DespawnPet(byte careerline)
        {
            if (careerline == 19)
            {
                Pet playerPet = ((CareerInterface_WhiteLion)_playerOwner.CrrInterface).myPet;
                if (playerPet != null)
                    playerPet.Dismiss(null, null);
            }
            else if (careerline == 8)
            {
                Pet playerPet = ((CareerInterface_SquigHerder)_playerOwner.CrrInterface).myPet;
                if (playerPet != null)
                    playerPet.Dismiss(null, null);
            }
        }
    }
    #endregion
}