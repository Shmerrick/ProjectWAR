using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FrameWork;
using GameData;
using NLog;
using WorldServer.Managers.Commands;
using WorldServer.Services.World;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Abilities.Objects;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using WorldServer.World.Positions;
using Group = WorldServer.World.Objects.Group;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Abilities
{
    public class AbilityProcessor
    {
        private readonly Unit _caster;
        private readonly AbilityInterface _abInterface;
        private readonly AbilityEffectInvoker _abEffectInvoker;
        private readonly NewChannelHandler _channelHandler;

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly List<byte> _setbacks = new List<byte>();

        private bool _shouldCheckRange;

        private long _castStartTime;
        private byte _castSequence, _pendingCastSequence, _pendingItemCooldownGroup, _itemCooldownGroup;

        private ushort _failureCode;

        private AbilityInfo _pendingInfo;
        public AbilityInfo AbInfo { get; private set; }

        public bool HasInfo()
        {
            return AbInfo != null;
        }

        public bool IsChannelling => _channelHandler.HasInfo();

        #region Initialization

        public AbilityProcessor(Unit caster, AbilityInterface parent)
        {
            _caster = caster;
            _abInterface = parent;
            _abEffectInvoker = new AbilityEffectInvoker(caster);
            _channelHandler = new NewChannelHandler(this, caster);
        }

        public bool StartAbility(AbilityInfo abInfo, byte castSequence, byte cooldownGroup, bool foeVisible, bool allyVisible, bool moving)
        {
            _pendingInfo = abInfo;
            _pendingItemCooldownGroup = cooldownGroup;
            _pendingCastSequence = castSequence;

            //Cut in to manage toggle abilities here.
            if (_pendingInfo.ConstantInfo.ToggleEntry > 0)
            {
                NewBuff toToggle = _caster.BuffInterface.GetBuff(_pendingInfo.ConstantInfo.ToggleEntry, _caster);

                if (toToggle != null)
                {
                    toToggle.BuffHasExpired = true;
                    CancelPendingCast();
                    _abInterface.SetCooldown(abInfo.ConstantInfo.CooldownEntry != 0 ? abInfo.ConstantInfo.CooldownEntry : abInfo.Entry, abInfo.Cooldown * 1000);
                    return true;
                }
            }

            if (!GetTarget(abInfo, _pendingInfo.Instigator, foeVisible, allyVisible) && _pendingInfo.Range > 0)
            {
                CancelPendingCast();
                return false;
            }

            byte result = ModifyInitials();

            if (!_pendingInfo.CanCastWhileMoving && _pendingInfo.CastTime > 0 && _pendingInfo.ConstantInfo.ChannelID == 0 && moving)
            {
                CancelPendingCast();
                return false;
            }

            if (result == 1 || !AllowStartCast())
            {
                CancelPendingCast();
                return false;
            }
                
            if (result == 2)
            {
                _abInterface.SetGlobalCooldown();
                CancelPendingCast();
                return false;
            }

            if (result == 3)
            {
                _abInterface.SetCooldown(_pendingInfo.ConstantInfo.CooldownEntry != 0 ? _pendingInfo.ConstantInfo.CooldownEntry : _pendingInfo.Entry, _pendingInfo.Cooldown * 1000);
                CancelPendingCast();
                return false;
            }

            if (!_caster.HasActionPoints(_pendingInfo.ApCost))
            {
                CancelPendingCast();
                return false;
            }
            //fix so that WE/WH cant use all their 3 openers at the same time, this in conjunction with whats in AbilityInterface
            if (_caster is Player)
            {
                if ((_caster as Player).StealthLevel > 0 && (abInfo.Entry == 9406 || abInfo.Entry == 9401 || abInfo.Entry == 9411 || abInfo.Entry == 8091 || abInfo.Entry == 8096 || abInfo.Entry == 8098))
                {
                    (_caster as Player).Uncloak();
                }
            }

            // Cast success. Cancel existing ability if any

            if (AbInfo != null)
                CancelCast(0);

            // Ether Dance / T'ree Hit Combo hold the resource state - need to check if they were broken using an attack of the same level
            if (_pendingInfo.SpecialCost > 0)
            {
                Player plr = _caster as Player;

                if (plr != null)
                {
                    if (_pendingInfo.SpecialCost > 0)
                    {
                        if (!plr.CrrInterface.HasResource((byte) _pendingInfo.SpecialCost))
                        {
                            CancelPendingCast();
                            return false;
                        }
                    }
                }
            }

            AbInfo = _pendingInfo;
            _pendingInfo = null;

            _pendingItemCooldownGroup = 0;
            _pendingCastSequence = 0;

            if (AbInfo.Level == 0)
                AbInfo.Level = _abInterface.GetMasteryLevelFor(AbInfo.ConstantInfo.MasteryTree);

            _castSequence = castSequence;
            _itemCooldownGroup = cooldownGroup;

            _castStartTime = TCPManager.GetTimeStampMS();

            _caster.BuffInterface.NotifyAbilityStarted(AbInfo);

            SendStart();

            //Secondary system for VFX
            if (abInfo.VFXTarget != null && abInfo.abilityID != 0 && _caster is Player)
            {
                string temp = "";

                if (abInfo.VFXTarget.Contains("Self"))
                    temp = (_caster as Player).Name;

                else if (abInfo.VFXTarget.Contains("Hostile") && (_caster as Player).CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ENEMY) != null)
                    temp = (_caster as Player).CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ENEMY).Name;

                else if (abInfo.VFXTarget.Contains("Friendly") && (_caster as Player).CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ALLY) != null)
                    temp = (_caster as Player).CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ALLY).Name;

                //Pets are handled differently, dont ask me why /Natherul
                else if (abInfo.VFXTarget.Contains("Pet") && (_caster as Player).CrrInterface.GetTargetOfInterest() != null)
                    temp = (_caster as Player).CrrInterface.GetTargetOfInterest().Name;

                temp += " " + abInfo.abilityID.ToString();

                if (abInfo.effectID2 != 0)
                    temp += " " + abInfo.effectID2.ToString();

                if (abInfo.Time != 0 && (!abInfo.VFXTarget.Contains("Pet") || (abInfo.VFXTarget.Contains("Hostile") && (_caster as Player).CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ENEMY).IsPlayer()) || (abInfo.VFXTarget.Contains("Friendly") && (_caster as Player).CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ALLY).IsPlayer())))
                    temp += " " + abInfo.Time.ToString() + " 0";

                if (abInfo.VFXTarget.Contains("aoe"))
                    temp += " " + abInfo.VFXTarget.Remove(0, abInfo.VFXTarget.IndexOf(' ') + 1);


                List<string> paramValue = temp.Split(' ').ToList();
                if (Regex.IsMatch(paramValue[0], @"^[a-zA-Z]+$"))
                    BaseCommands.PlayAbility((_caster as Player), ref paramValue);
            }

            if (!AbInfo.ConstantInfo.IgnoreGlobalCooldown)
                _abInterface.SetGlobalCooldown();

            if (AbInfo.ConstantInfo.ChannelID != 0)
            {
                // Channeled Morale 4s grant immunity and cannot be interrupted
                if (AbInfo.SpecialCost == -4)
                {
                    _caster.AddCrowdControlImmunity((int)CrowdControlTypes.Unstoppable);
                    _caster.IsImmovable = true;
                }

                _channelHandler.Initialize(AbInfo, castSequence);
            }

            // Instants
            else if (AbInfo.CastTime == 0)
                Cast();

            // Casts need to check range 60% of the way in
            else
                _shouldCheckRange = true;

            if (_setbacks.Count > 0)
            {
                lock (_setbacks)
                    _setbacks.Clear();
            }

            return true;
        }

        public bool StartAbilityAtPos(AbilityInfo abInfo, byte castSequence, Point3D worldPos, ushort zoneID)
        {
            _pendingInfo = abInfo;
            _pendingInfo.TargetPosition = worldPos;
            _pendingItemCooldownGroup = 0;
            _pendingCastSequence = castSequence;

            byte result = ModifyInitials();

            if (result == 1 || !AllowStartCastAtPos() || !_caster.PointWithinRadiusFeet(_pendingInfo.TargetPosition, (int)(_caster.BaseRadius + _pendingInfo.Range)))
            {
                CancelPendingCast();
                return false;
            }

            if (result == 2)
            {
                _abInterface.SetGlobalCooldown();
                CancelPendingCast();
                return false;
            }

            if (result == 3)
            {
                _abInterface.SetCooldown(_pendingInfo.ConstantInfo.CooldownEntry != 0 ? _pendingInfo.ConstantInfo.CooldownEntry : _pendingInfo.Entry, _pendingInfo.Cooldown * 1000);
                CancelPendingCast();
                return false;
            }

            // Cast success. Cancel existing ability if any

            if (AbInfo != null)
                CancelCast(0);

            AbInfo = _pendingInfo;
            _pendingInfo = null;
            _itemCooldownGroup = 0;
            _castSequence = castSequence;

            _castStartTime = TCPManager.GetTimeStampMS();

            if (AbInfo.Level == 0)
                AbInfo.Level = _abInterface.GetMasteryLevelFor(AbInfo.ConstantInfo.MasteryTree);

            if (AbInfo.ConstantInfo.Origin != AbilityOrigin.AO_ITEM)
                _caster.BuffInterface.NotifyAbilityStarted(AbInfo);

            SendStart();

            if (!AbInfo.ConstantInfo.IgnoreGlobalCooldown)
                _abInterface.SetGlobalCooldown();

            if (AbInfo.ConstantInfo.ChannelID != 0)
            {
                // Channeled Morale 4s grant immunity and cannot be interrupted
                if (AbInfo.SpecialCost == -4)
                {
                    _caster.AddCrowdControlImmunity((int)CrowdControlTypes.Unstoppable);
                    _caster.IsImmovable = true;
                }

                GroundTarget gt = new GroundTarget(_caster, AbInfo.TargetPosition, GameObjectService.GetGameObjectProto(23));
                _caster.Region.AddObject(gt, _caster.Zone.ZoneId);

                gt.SetExpiry(TCPManager.GetTimeStampMS() + 20000);

                AbInfo.Target = gt;

                _channelHandler.Initialize(AbInfo, castSequence);
            }
            else
                CastAtPosition();

            return false;
        }

        private bool AllowStartCast()
        {
            if (_caster.BlockedByCC(_pendingInfo.ConstantInfo))
                return false;

            if (_abInterface.IsOnCooldown(_pendingInfo))
                return false;

            if (_pendingItemCooldownGroup > 0 && !_abInterface.CanCastItemGroupCooldown(_pendingItemCooldownGroup))
                return false;

            Player plr = _caster as Player;

            if (plr != null)
            {
                if (plr.StealthLevel > 0 && _pendingInfo.ConstantInfo.StealthInteraction == AbilityStealthType.Block)
                    return false;
            
                if (_pendingInfo.SpecialCost != 0)
                {
                    if (_pendingInfo.SpecialCost > 0)
                    {
                        if (!plr.CrrInterface.HasResource((byte) _pendingInfo.SpecialCost))
                            return false;
                    }

                    else if (plr.MoraleLevel < -_pendingInfo.SpecialCost || !_abInterface.IsValidMorale(_pendingInfo.Entry, (byte) (-_pendingInfo.SpecialCost)))
                        return false;
                }
            }

            if (_pendingInfo.Target != null)
            {
                if (_pendingInfo.Target.IsDead ^ _pendingInfo.ConstantInfo.AffectsDead)
                    return false;

                if (_pendingInfo.Target != _caster)
                {
                    uint abRange = (uint)_pendingInfo.Target.GetAbilityRangeTo(_caster);
                    uint maxRange = _pendingInfo.Range;

                    if (_caster.IsMoving && _pendingInfo.Target.IsMoving)
                        maxRange = maxRange + 5;

                    if (abRange > maxRange || abRange < _pendingInfo.MinRange)
                        return false;
                }
            }

            if (_caster.ItmInterface != null)
            {
                var weaponOk = _caster.ItmInterface.WeaponCheck(_pendingInfo.ConstantInfo.WeaponNeeded);
                if (weaponOk == AbilityResult.ABILITYRESULT_OK)
                    return true;
                else
                {
                    _logger.Warn($"Weaponcheck failed for {_caster.Name} {_pendingInfo.Name}");
                    return false;
                }
            }

            return true;
        }

        private bool AllowStartCastAtPos()
        {
            if (_caster.BlockedByCC(_pendingInfo.ConstantInfo))
                return false;

            if (!_caster.HasActionPoints(_pendingInfo.ApCost))
                return false;

            if (_abInterface.IsOnCooldown(_pendingInfo))
                return false;

            if (_pendingInfo.SpecialCost != 0)
            {
                Player plr = _caster as Player;

                if (plr == null)
                {
                    Log.Error("NewAbility", "Ability with SpecialCost casted by non-Player!");
                    return false;
                }
                if (_pendingInfo.SpecialCost > 0 && !plr.CrrInterface.HasResource((byte)_pendingInfo.SpecialCost))
                    return false;

                if (plr.MoraleLevel < -_pendingInfo.SpecialCost)
                    return false;
            }

            return true;
        }

        private void SendStart()
        {
            PacketOut Out;

            Player plr = _caster as Player;

            if (plr != null)
            {
                //For abilities with a cast time, we send the timer first to ensure the client knows it's modified
                if (AbInfo.ConstantInfo.Origin != AbilityOrigin.AO_ITEM && AbInfo.CastTime > 0)
                {
                    Out = new PacketOut((byte)Opcodes.F_SET_ABILITY_TIMER, 12);
                    Out.WriteUInt16(1);
                    Out.WriteByte(1);
                    Out.WriteByte(0x1); // initial timer
                    Out.WriteUInt16(0);
                    Out.WriteUInt16(AbInfo.CastTime);
                    Out.WriteUInt16(AbInfo.Entry);
                    Out.WriteByte(_castSequence);
                    Out.WriteByte(0);

                    plr.SendPacket(Out);
                }
            }

            Out = new PacketOut((byte)Opcodes.F_USE_ABILITY, 20);

            Out.WriteUInt16(0);
            Out.WriteUInt16(AbInfo.Entry);
            Out.WriteUInt16(_caster.Oid);
            Out.WriteUInt16(AbInfo.ConstantInfo.EffectID);

            if (AbInfo.Target != null)
                Out.WriteUInt16(AbInfo.Target.Oid);
            else if (AbInfo.Range == 0)
                Out.WriteUInt16(_caster.Oid);
            else Out.WriteUInt16(0);

            Out.WriteByte(1);
            Out.WriteByte((byte)AbInfo.ConstantInfo.Origin);

            Out.WriteUInt32(AbInfo.ConstantInfo.ChannelID == 0 ? AbInfo.CastTime : (uint)0);

            Out.WriteByte(_castSequence);
            Out.WriteUInt16(0);
            Out.WriteByte(0);
            _caster.DispatchPacket(Out, true);
        }

        #endregion

        #region Modification
        /// <summary>
        /// Identifies target of an ability before it is casted and checks its validity (pvp flag, visibility...).
        /// </summary>
        /// <param name="abInfo">Ability that is about to be casted</param>
        /// <param name="instigator">Instigator of the ability</param>
        /// <param name="foeVisible">True if current targeted foe is visible</param>
        /// <param name="allyVisible">True if current targeted ally is visible</param>
        /// <returns>True if target is valid for the ability</returns>
        private bool GetTarget(AbilityInfo abInfo, Unit instigator, bool foeVisible, bool allyVisible)
        {
            if (_pendingInfo.Range == 0 || _pendingInfo.CommandInfo == null)
            {
                _pendingInfo.Target = _caster;
                return true;
            }

            if (_pendingInfo.TargetType == CommandTargetTypes.SiegeCannon)
            {
                _pendingInfo.Target = ((Creature) _caster).SiegeInterface.BuildTargetList(instigator);
                return true;
            }

            CommandTargetTypes selectType = (CommandTargetTypes) ((int) _pendingInfo.TargetType & 7);

            switch (selectType)
            {
                case CommandTargetTypes.Caster:
                    _pendingInfo.Target = _caster;
                    break;
                case CommandTargetTypes.Ally:
                    if (!allyVisible)
                        return false;
                    _pendingInfo.Target = _caster.CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ALLY);
                    if (_pendingInfo.Target == _caster || !CombatInterface.IsFriend(_caster, _pendingInfo.Target) || (_pendingInfo.Target is Creature && !(_pendingInfo.Target is Pet)))
                        return false;
                    if (_pendingInfo.TargetType.HasFlag(CommandTargetTypes.Groupmates))
                    {
                        Group myGroup = ((Player)_caster).PriorityGroup;
                        if (myGroup == null || !myGroup.HasMember(_pendingInfo.Target))
                            return false;
                    }
                    break;

                case CommandTargetTypes.AllyOrSelf:
                    if (!allyVisible)
                        return false;
                    _pendingInfo.Target = _caster.CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ALLY) ?? _caster;

                    if (!CombatInterface.IsFriend(_caster, _pendingInfo.Target) ||
                        (_pendingInfo.Target is Creature && !(_pendingInfo.Target is Pet)))
                    {
                        if ((_pendingInfo.Target is Boss) && (_caster is Creature))
                        {
                            // Allow healing on a boss -- careful to not allow lord heal from player
                        }
                        else
                        {
                            return false;
                        }
                    }
                   
                    if (_pendingInfo.Target != _caster)
                    {
                        if (_pendingInfo.TargetType.HasFlag(CommandTargetTypes.Groupmates))
                        {
                            Group myGroup = ((Player)_caster).PriorityGroup;
                            if (myGroup == null || !myGroup.HasMember(_pendingInfo.Target))
                                return false;
                        }
                    }
                    break;

                case CommandTargetTypes.AllyOrCareerTarget:
                    if (!allyVisible)
                        return false;
                    _pendingInfo.Target = _caster.CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ALLY);
                  
                    if (!CombatInterface.IsFriend(_caster, _pendingInfo.Target) || _pendingInfo.Target != null && (_pendingInfo.Target is Creature && !(_pendingInfo.Target is Pet)))
                        return false;
                    if (_pendingInfo.Target != null && _pendingInfo.Target != _caster)
                    {
                        if (_pendingInfo.TargetType.HasFlag(CommandTargetTypes.Groupmates))
                        {
                            Group myGroup = ((Player)_caster).PriorityGroup;
                            if (myGroup == null || !myGroup.HasMember(_pendingInfo.Target))
                                return false;
                        }
                    }
                    else
                    {
                        Player petCareerPlr = null;
                        if (_caster is Player)
                            petCareerPlr = _caster as Player;
                        if (petCareerPlr != null)
                            _pendingInfo.Target = petCareerPlr.CrrInterface.GetTargetOfInterest();

                        if(_pendingInfo.Target == null || _pendingInfo.Target == _caster)
                        {
                            _pendingInfo.Target = null;
                            return false;
                        }
                    }

                    break;

                case CommandTargetTypes.Enemy:
                    if (!foeVisible)
                        return false;
                    _pendingInfo.Target = _caster.CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ENEMY);
                    if (!CombatInterface.CanAttack(_caster, _pendingInfo.Target))
                        _pendingInfo.Target = null;
                    else
                    {
                        if (!_caster.CbtInterface.IsPvp)
                        {
                            Player plrCaster = _caster as Player;
                            if (plrCaster != null && _pendingInfo.Target.CbtInterface.IsPvp)
                                ((CombatInterface_Player)plrCaster.CbtInterface).EnablePvp();
                        }
                    }

                    Player plrTarget = _pendingInfo.Target as Player;
                    
                    if (plrTarget != null && plrTarget.Palisade != null && (plrTarget.Palisade.IsObjectInFront(_caster, 180) ^ plrTarget.Palisade.IsObjectInFront(plrTarget, 180)))
                        _pendingInfo.Target = plrTarget.Palisade;

                    //8410 - Terrible Embrace ; 9057 - Wings of Heaven ; 9178 - Fetch! ; 9186 - Pounce
                    if (_pendingInfo.Target != null
                        && (abInfo.Entry == 8410 || abInfo.Entry == 9057 || abInfo.Entry == 9178 || abInfo.Entry == 9186)
                        && Math.Abs(_caster.Z - _pendingInfo.Target.Z) > 300)
                    {
                        _caster.AbtInterface.SetCooldown(abInfo.Entry, -1);
                        return false;
                    }

                    break;

                case CommandTargetTypes.CareerTarget: // Target of Interest (oath friend/dark protector/pet if player, master if pet)
                    var player = _caster as Player;
                    if (player != null)
                        _pendingInfo.Target = player.CrrInterface.GetTargetOfInterest();
                    else
                    {
                        var pet = _caster as Pet;
                        if (pet != null)
                            _pendingInfo.Target = pet.Owner;
                        else
                        {
                            Log.Error("NewAbility", "Ability " + _pendingInfo.Entry + " with targettype 5 has no target!");
                            _pendingInfo.Target = null;
                        }
                    }
                    break;

                default:
                    Log.Error("NewAbility", "Ability " + _pendingInfo.Entry + " with TargetType zero in 3 LSBs!");
                    _pendingInfo.Target = _caster;
                    break;
            }

            if (_pendingInfo.TargetType.HasFlag(CommandTargetTypes.Groupmates) && _pendingInfo.Target != _caster)
            {
                Group myGroup = ((Player)_caster).PriorityGroup;
                if (myGroup == null || !myGroup.HasMember(_pendingInfo.Target))
                    return false;
            }

            return _pendingInfo.Target != null;
        }

        /// <summary>Performs the initial modifications to the volatile ability properties.</summary>
        private byte ModifyInitials()
        {
            if (!_pendingInfo.ConstantInfo.IgnoreOwnModifiers)
            {
                List<AbilityModifier> myModifiers = AbilityMgr.GetAbilityPreCastModifiers(_pendingInfo.Entry);
                if (myModifiers != null)
                {
                    foreach (AbilityModifier modifier in myModifiers)
                    {
                        byte result = modifier.ModifyAbility(_caster, _pendingInfo);

                        if (result > 0)
                            return result;
                    }
                }
            }

            // Morales and items should not run through general handlers
            if (_pendingInfo.SpecialCost < 0 || _pendingInfo.ConstantInfo.Origin == AbilityOrigin.AO_ITEM)
                return 0;

            if (_caster is Player)
                ((Player) _caster).TacInterface.ModifyInitials(_pendingInfo);
            _caster.StsInterface.ModifyAbilityVolatiles(_pendingInfo);
            return 0;
        }

        /// <summary>Performs the final modifications to the ability commands.</summary>
        private bool ModifyFinals()
        {
            if (!AbInfo.ConstantInfo.IgnoreOwnModifiers)
            {
                List<AbilityModifier> myModifiers = AbilityMgr.GetAbilityModifiers(AbInfo.Entry);
                if (myModifiers != null)
                {
                    foreach (var modifier in myModifiers)
                    {
                        var result = modifier.ModifyAbility(_caster, AbInfo);

                        if (result > 0)
                        {
                            if (result > 1)
                            {
                                _abInterface.SetCooldown(AbInfo.ConstantInfo.CooldownEntry != 0 ? AbInfo.ConstantInfo.CooldownEntry : AbInfo.Entry, AbInfo.Cooldown*1000);
                                _abInterface.SetGlobalCooldown();
                            }

                            return false;
                        }
                    }
                }
            }

            // Morales and items should not run through general handlers
            if (AbInfo.SpecialCost < 0 || AbInfo.ConstantInfo.Origin == AbilityOrigin.AO_ITEM)
                return true;

            var player = _caster as Player;
            player?.TacInterface.ModifyFinals(AbInfo);
            return true;
        }

        #endregion

        #region Tick

        public void Update(long tick)
        {
            _abEffectInvoker?.Update(tick);

            if (AbInfo == null)
                return;
            if (_failureCode != 0)
            {
                CancelCast(_failureCode);
                return;
            }
            if (_channelHandler.HasInfo())
            {
                _channelHandler.Update(tick);
                return;
            }
            if (_shouldCheckRange && tick >= _castStartTime + (long)(AbInfo.CastTime*0.6f))
            {
                _shouldCheckRange = false;

                uint abRange = (uint)AbInfo.Target.GetAbilityRangeTo(_caster);

                if (abRange > AbInfo.Range)
                {
                    CancelCast((byte) AbilityResult.ABILITYRESULT_OUTOFRANGE);
                    return;
                }

                if (abRange < AbInfo.MinRange)
                {
                    CancelCast((byte) AbilityResult.ABILITYRESULT_TOOCLOSE);
                    return;
                }

            }
            if (tick >= _castStartTime + AbInfo.CastTime)
                Cast();
            else
            {
                if (AbInfo.CastTime == 0 || _channelHandler.HasInfo())
                    return;
                if (_setbacks.Count > 0)
                    TrySetback();
            }
        }

        public void CheckBlockedByCC()
        {
            if (AbInfo != null && _caster.BlockedByCC(AbInfo.ConstantInfo))
                _failureCode = 1;
        }
        #endregion

        #region Channelling

        public void NotifyChannelEnded()
        {
            SendCompleted();
            Clear();
        }

        #endregion

        #region Setback

        public void AddSetback(byte chance)
        {
            if (AbInfo == null || _channelHandler.HasInfo() || AbInfo.CastTime == 0)
                return;
            lock (_setbacks)
                _setbacks.Add(chance);
        }

        private void TrySetback()
        {
            List<byte> locSetbacks = new List<byte>();

            Player plr = _caster as Player;

            if (plr == null)
                return;

            lock (_setbacks)
            {
                locSetbacks.AddRange(_setbacks);
                _setbacks.Clear();
            }

            if (AbInfo.ConstantInfo.Fragile == 2) // breaks immediately
            {
                CancelCast(1);
                return;
            }

            byte setbackChance = (byte)((AbInfo.ConstantInfo.Fragile == 1 ? 50 : 25) * _caster.StsInterface.GetStatPercentageModifier(Stats.SetbackChance));

            if (setbackChance == 0)
                return;

            ushort setbackAmount = 0;

            foreach (byte amount in locSetbacks)
            {
                if (amount <= setbackChance)
                    setbackAmount += 250;
            }

            if (setbackAmount == 0)
                return;

            _castStartTime = Math.Min(TCPManager.GetTimeStampMS(), _castStartTime + setbackAmount);

            PacketOut Out = new PacketOut((byte)Opcodes.F_SET_ABILITY_TIMER, 12);
            Out.WriteUInt16(1);
            Out.WriteByte(1);
            Out.WriteByte(3); // setback event
            Out.WriteUInt16(0);
            Out.WriteUInt16((ushort)(AbInfo.CastTime - (TCPManager.GetTimeStampMS() - _castStartTime)));
            Out.WriteUInt16(AbInfo.Entry);
            Out.WriteByte(_castSequence);
            Out.WriteByte(0);

            plr.SendPacket(Out);
        }
        #endregion

        #region Cancellation

        private void Clear()
        {
            if (AbInfo.ConstantInfo.ChannelID != 0 && AbInfo.SpecialCost == -4)
            {
                _caster.RemoveCrowdControlImmunity((int)CrowdControlTypes.Unstoppable);
                _caster.IsImmovable = false;
            }

            AbInfo = null;
            _castStartTime = 0;

            _inCastCompletion = false;

            _failureCode = 0;

            _shouldCheckRange = false;
        }

        public void CheckMoveInterrupt()
        {
            if (!AbInfo.CanCastWhileMoving)
                CancelCast((ushort)AbilityResult.ABILITYRESULT_INTERRUPTED);
        }

        public void NotifyCancelled(ushort failCode)
        {
            _failureCode = failCode;
        }

        public void CancelPendingCast()
        {
            // Failed attempt to break a channel
            if (AbInfo != null)
                return;

            if (_pendingInfo.ConstantInfo.ChannelID != 0)
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_SET_ABILITY_TIMER, 12);
                Out.WriteUInt16(_pendingInfo.Entry);
                Out.Fill(0, 4);
                Out.WriteUInt16(0);
                Out.Fill(0, 4);
                if (_caster is Pet)
                    ((Pet) _caster).Owner.SendPacket(Out);
                else if (_caster is Player)
                    ((Player) _caster).SendPacket(Out);
            }

            if (_pendingInfo.SpecialCost < 0)
            {
                Player plr = _caster as Player;

                if (plr != null)
                {
                    PacketOut Out = new PacketOut((byte)Opcodes.F_SET_ABILITY_TIMER, 12);
                    Out.WriteUInt16(0);
                    Out.WriteUInt16(0x200); // Morale timer
                    Out.WriteUInt32(0); // cooldown
                    Out.WriteUInt32(0);
                    plr.SendPacket(Out);
                }
            }

            AbInfo = _pendingInfo;
            _castSequence = _pendingCastSequence;

            CancelCast(0);

            AbInfo = null;
            _castSequence = 0;

            _pendingInfo = null;
            _pendingItemCooldownGroup = 0;
            _pendingCastSequence = 0;
        }

        public void Stop()
        {
            if (AbInfo != null)
                CancelCast(0);

            _abEffectInvoker.ClearPending();
        }

        /// <summary>
        /// <para>Cancels the currently casting ability.</para>
        /// <para>If FailCode is specified, will display a message on the client.</para>
        /// <para>If FailCode is omitted, will reset the client's GCD and ability-specific cooldown.</para>
        /// </summary>
        /// <param name="failCode"></param>
        public void CancelCast(ushort failCode, bool force = true)
        {
            if (_inCastCompletion)
                return;
            if (_channelHandler.HasInfo())
            {
                // Cannot interrupt morale channels
                if (AbInfo.SpecialCost == -4 && !force)
                    return;

                _channelHandler.Interrupt();
            }

            else
            {
                if (failCode == (ushort) AbilityResult.ABILITYRESULT_NOTVISIBLECLIENT && (TCPManager.GetTimeStampMS() - _castStartTime) > AbInfo.CastTime*0.75f)
                    return;

                // Interruptions on spells with a cast time reset the GCD
                if (AbInfo.CastTime > 0 && _abInterface.Cooldowns.ContainsKey(0))
                    _abInterface.Cooldowns.Remove(0);
            }

            if (AbInfo == null)
            {
                Log.Error("Ability System", "Cancellation on NULL ABILITY - Caster is " + _caster.Name);
                return;
            }

            PacketOut Out = new PacketOut((byte)Opcodes.F_USE_ABILITY, 20);

            Out.WriteUInt16(0);
            Out.WriteUInt16(AbInfo.Entry);
            Out.WriteUInt16(_caster.Oid);
            Out.WriteUInt16(AbInfo.ConstantInfo.EffectID);
            Out.WriteUInt16(AbInfo.Target?.Oid ?? 0);
            Out.WriteByte(0);
            Out.WriteByte(1);
            Out.WriteUInt16(failCode);
            // If a value is sent here, the GCD and cooldown will both reset.
            // TODO: Experiment with positive values.
            if (failCode == 0)
                Out.WriteInt16((short)-(TCPManager.GetTimeStampMS() - _castStartTime));
            else Out.WriteInt16(0);
            Out.WriteByte(_castSequence);
            Out.WriteUInt16(0);
            Out.WriteByte(0);

            _caster.DispatchPacket(Out, force);

            // This packet hides the cast bar when a spell is cast, so I guess it signals the end of the ability.
            // Seems to be sent on forced interrupts and on instant cast ability completion.
            if (force)
            {
                Out = new PacketOut((byte) Opcodes.F_UPDATE_STATE, 10);

                Out.WriteUInt16(_caster.Oid);
                Out.WriteByte((byte) StateOpcode.CastCompletion);
                Out.WriteUInt16(0);
                Out.WriteByte(0);
                Out.WriteUInt16(AbInfo.Entry);
                Out.WriteByte(0);
                Out.WriteByte(0);

                (_caster as Player)?.SendPacket(Out);
            }

            Clear();
        }
        #endregion

        #region Cast

        private bool AllowCast()
        {
            AbilityResult myResult = AbilityResult.ABILITYRESULT_OK;

            if (AbInfo.Target != null)
            {
                if (AbInfo.Target.IsDead && !AbInfo.ConstantInfo.AffectsDead)
                    myResult = AbilityResult.ABILITYRESULT_ILLEGALTARGET_DEAD;
                else if (!AbInfo.Target.IsDead && AbInfo.ConstantInfo.AffectsDead)
                    myResult = AbilityResult.ABILITYRESULT_ILLEGALTARGET_NOT_DEAD_ALLY;

                else if (AbInfo.Target != _caster)
                {
                    if (AbInfo.ConstantInfo.CastAngle != 0 && _caster.IsMoving && !_caster.IsObjectInFront(AbInfo.Target, AbInfo.ConstantInfo.CastAngle))
                        myResult = AbilityResult.ABILITYRESULT_OUT_OF_ARC;

                    // Explicit LOS check for move-cast abilities
                    if (AbInfo.CastTime > 0 && AbInfo.CanCastWhileMoving && AbInfo.Target != null && !_caster.LOSHit(AbInfo.Target))
                        myResult = AbilityResult.ABILITYRESULT_NOT_VISIBLE;
                }
            }

            if (myResult != AbilityResult.ABILITYRESULT_OK)
            {
                CancelCast((ushort)myResult);
                return false;
            }

            if (myResult == AbilityResult.ABILITYRESULT_OK && _caster.ItmInterface != null)
                myResult = _caster.ItmInterface.WeaponCheck(AbInfo.ConstantInfo.WeaponNeeded);

            if (AbInfo.ApCost > 0 && _caster is Player && !_caster.ConsumeActionPoints(AbInfo.ApCost))
                myResult = AbilityResult.ABILITYRESULT_AP;

            if (myResult != AbilityResult.ABILITYRESULT_OK)
            {
                CancelCast((ushort)myResult);
                return false;
            }

            AbilityMgr.GetCommandsFor(_caster, AbInfo);

            return true;
        }

        private bool _inCastCompletion;

        private void Cast()
        {

            if (!AllowCast())
                return;
            
            if (ModifyFinals())
            {
                _inCastCompletion = true;

                ushort abEffectDelay = 0;

                // Work out hit delay, if any
                if (AbInfo.ConstantInfo.EffectDelay != 0)
                {
                    if (AbInfo.ConstantInfo.EffectDelay < 0)
                        abEffectDelay = (ushort)-AbInfo.ConstantInfo.EffectDelay;
                    else
                        abEffectDelay = AbInfo.ConstantInfo.GetDelayFor((ushort)AbInfo.Target.GetDistanceToWorldPoint(_caster.WorldPosition));

                    AbInfo.InvocationTimestamp = TCPManager.GetTimeStampMS() + abEffectDelay + 150;
                }
                
                _abEffectInvoker.StartEffects(AbInfo);

                ApplyCooldown();

                if (AbInfo.ConstantInfo.EffectDelay > 0)
                    SendProjectileFlightTime(abEffectDelay);//abRange * 12);

                SendCompleted();
            }
            Clear();
        }

        // Sends a packet to the client which will set the projectile range and cause the client to reply with an F_PLAYER_INFO,
        // which gives some LOS information.
        private void SendProjectileFlightTime(ushort flightTimeMs)
        {
            flightTimeMs = (ushort) (flightTimeMs*AbInfo.FlightTimeMod);

            PacketOut Out = new PacketOut((byte)Opcodes.F_USE_ABILITY, 20);

            Out.WriteUInt16(0);
            Out.WriteUInt16(AbInfo.Entry);
            Out.WriteUInt16(_caster.Oid);
            Out.WriteUInt16(AbInfo.ConstantInfo.EffectID);
            Out.WriteUInt16(AbInfo.Target.Oid);
            Out.WriteByte(6); // Range/LOS
            Out.WriteByte(1);
            Out.WriteByte(0);
            Out.WriteByte(0); //result
            Out.WriteUInt16(flightTimeMs); // Travel distance in engine units
            Out.WriteByte(_castSequence);
            Out.WriteUInt16(0);
            Out.WriteByte(0);
            _abInterface._Owner.DispatchPacket(Out, true);
        }

        private void SendCompleted()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_USE_ABILITY, 20);

            Out.WriteUInt16(0);
            Out.WriteUInt16(AbInfo.Entry);
            Out.WriteUInt16(_caster.Oid);
            Out.WriteUInt16(AbInfo.ConstantInfo.EffectID);

            if (AbInfo.Target != null)
                Out.WriteUInt16(AbInfo.Target.Oid);
            else if (AbInfo.Range == 0)
                Out.WriteUInt16(_caster.Oid);
            else Out.WriteUInt16(0);

            Out.WriteByte(2);
            Out.WriteByte((byte)AbInfo.ConstantInfo.Origin);
            Out.WriteByte(0);
            Out.WriteByte(0); //result
            Out.WriteInt16((short)-(TCPManager.GetTimeStampMS() - _castStartTime));
            Out.WriteByte(_castSequence);
            Out.WriteUInt16(0); //time
            Out.WriteByte(0);
            _abInterface._Owner.DispatchPacket(Out, true);

            if (AbInfo.CastTime == 0)
            //if (AbInfo.ConstantInfo.ChannelID == 0)
                _caster.SendUpdateState((byte)StateOpcode.CastCompletion, 0);
        }

        private void ApplyCooldown()
        {
            if (AbInfo.ConstantInfo.ToggleEntry == 0)
            {
                if (AbInfo.ConstantInfo.Origin == AbilityOrigin.AO_ITEM)
                {
                    if (_itemCooldownGroup > 0)
                        _abInterface.SetItemGroupCooldown(_itemCooldownGroup, AbInfo.Cooldown);
                    else
                        _abInterface.SetItemCooldown(AbInfo.Entry, AbInfo.Cooldown);
                }

                else _abInterface.SetCooldown(AbInfo.ConstantInfo.CooldownEntry != 0 ? AbInfo.ConstantInfo.CooldownEntry : AbInfo.Entry, AbInfo.Cooldown * 1000);
            }

            // Morale cooldown if applicable
            if (AbInfo.SpecialCost < 0)
            {
                Player plr = _caster as Player;

                if (plr != null)
                {
                    plr.ResetMorale();

                    PacketOut Out = new PacketOut((byte)Opcodes.F_SET_ABILITY_TIMER, 12);
                    Out.WriteUInt16(0);
                    Out.WriteUInt16(0x200);
                    Out.WriteUInt32((uint)(AbInfo.Cooldown * 1000));
                    Out.WriteUInt32(0);
                    plr.SendPacket(Out);
                }
            }
        }

        #endregion

        #region Cast At Pos

        private bool AllowCastAtPos()
        {
            if (AbInfo.ApCost > 0 && !_caster.ConsumeActionPoints(AbInfo.ApCost))
            {
                CancelCast(0);
                return false;
            }

            AbilityMgr.GetCommandsFor(_caster, AbInfo);

            return true;
        }

        private void CastAtPosition()
        {
            uint abRange = (uint)_caster.GetDistanceTo(AbInfo.TargetPosition);

            if (!AllowCastAtPos())
                return;

            if (AbInfo.ConstantInfo.EffectDelay != 0)
            {
                ushort abEffectDelay;

                if (AbInfo.ConstantInfo.EffectDelay < 0)
                    abEffectDelay = (ushort)-AbInfo.ConstantInfo.EffectDelay;
                else
                    abEffectDelay = AbInfo.ConstantInfo.GetDelayFor((ushort)abRange);
                AbInfo.InvocationTimestamp = TCPManager.GetTimeStampMS() + abEffectDelay;
            }

            _abEffectInvoker.StartEffects(AbInfo);

            if (_itemCooldownGroup > 0)
                _abInterface.SetItemGroupCooldown(_itemCooldownGroup, AbInfo.Cooldown);
            else _abInterface.SetCooldown(AbInfo.ConstantInfo.CooldownEntry != 0 ? AbInfo.ConstantInfo.CooldownEntry : AbInfo.Entry, AbInfo.Cooldown * 1000);

            if (AbInfo.SpecialCost < 0)
            {
                Player plr = _caster as Player;

                if (plr != null)
                {
                    plr.ResetMorale();

                    PacketOut Out = new PacketOut((byte)Opcodes.F_SET_ABILITY_TIMER, 12);
                    Out.WriteUInt16(0);
                    Out.WriteUInt16(0x200);
                    Out.WriteUInt32(60000);
                    Out.WriteUInt32(0);
                    plr.SendPacket(Out);
                }
            }
            //SendProjectileSpeed();
            SendCompleted();

            if (AbInfo.ConstantInfo.Origin != AbilityOrigin.AO_ITEM)
                _caster.BuffInterface.NotifyAbilityCasted(AbInfo);

            Clear();
        }

        #endregion
    }
}
