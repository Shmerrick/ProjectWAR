using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using FrameWork;
using GameData;
using WorldServer.Managers.Commands;
using WorldServer.Services.World;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.CareerInterfaces;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Abilities.Objects;
using WorldServer.World.AI;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using WorldServer.World.Positions;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Abilities
{
    public class AbCmdStorageComparer : Comparer<AbilityInfo>
    {
        public override int Compare(AbilityInfo ab1, AbilityInfo ab2)
        {
            return ab1.InvocationTimestamp.CompareTo(ab2.InvocationTimestamp);
        }
    }

    /// <summary>
    /// Responsible for invoking ability commands which are not executed by buffs.
    /// </summary>
    public class AbilityEffectInvoker
    {
        const int MAX_AOE_TARGETS = 9;
        private const int TANK_GUARD_CONTRIBUTION_CHANCE = 8;


        private readonly Unit _caster;
        private Unit _instigator, _target;
        private CommandTargetTypes _principalTargetType;

        // Holds the various commands that abilities can execute.
        private delegate bool AbilityCommandDelegate(AbilityCommandInfo cmd, byte level, Unit target);
        private readonly Dictionary<string, AbilityCommandDelegate> _commandList = new Dictionary<string, AbilityCommandDelegate>();

        private delegate void AbilityPositionalCommandDelegate(AbilityCommandInfo cmd, byte level, Point3D position);
        private readonly Dictionary<string, AbilityPositionalCommandDelegate> _posCommandList = new Dictionary<string, AbilityPositionalCommandDelegate>();

        private AbilityInfo _pendingAbility;
        private long _pendingAbilityInvokeTime;

        // Total damage dealt on this ability pass (advanced lifetaps need to read it)
        private int _totalDamage;
        // Targets damaged on this ability pass (advanced lifetaps need to read it)
        private int _damagedCount;
        // Targets considered on last AoE pass
        private int _targetsFound;
        // Allies caught in the last enemy-affecting attack (used to modify power of AoE attacks)
        private float _alliesFound;

        // Holds a list of delayed effects (because of projectile travel, etc)
        private readonly SortedSet<AbilityInfo> _delayedEffects = new SortedSet<AbilityInfo>(new AbCmdStorageComparer());

        public AbilityEffectInvoker(Unit myCaster)
        {
            _caster = myCaster;

            //Damage
            _commandList.Add("DealDamage", DealDamage);
            _commandList.Add("FlankingShot", FlankingShot);
            _commandList.Add("SwellOfGloom", SwellOfGloom);
            _commandList.Add("MultipleDealDamage", MultipleDealDamage);
            _commandList.Add("BounceDamage", BounceDamage);
            _commandList.Add("Slay", Slay);
            _commandList.Add("PetTandemStrikeMain", PetTandemStrikeMain);
            _commandList.Add("PetTandemStrikeSub", PetTandemStrikeSub);
            _commandList.Add("StealLife", StealLife);
            _commandList.Add("Ram", Ram);
            _commandList.Add("GroundedEffect", GroundEffect);
            //Buffs
            _commandList.Add("InvokeBuff", InvokeBuff);
            _commandList.Add("InvokeBuffWithDuration", InvokeBuffWithDuration);
            _commandList.Add("InvokeAura", InvokeAura);
            _commandList.Add("InvokeLinkedBuff", InvokeLinkedBuff);
            _commandList.Add("InvokeBouncingBuff", InvokeBouncingBuff);
            _commandList.Add("StackBuffByNearbyFoes", StackBuffByNearbyFoes);
            _commandList.Add("InvokeGuard", InvokeGuard);
            _commandList.Add("InvokeOathFriend", InvokeOathFriend);
            _commandList.Add("InvokeOnYourGuard", InvokeOnYourGuard);
            _commandList.Add("SetPetBuff", SetPetBuff);
            //KB/Pull
            _commandList.Add("PuntEnemy", PuntEnemy);
            _commandList.Add("PuntSelf", PuntSelf);
            _commandList.Add("JumpTo", JumpTo);
            _commandList.Add("Pull", Pull);
            //Cleansing
            _commandList.Add("CleanseCC", CleanseCc);
            _commandList.Add("CleanseDebuffType", CleanseDebuffType);
            _commandList.Add("ExclusiveCleanseDebuffType", ExclusiveCleanseDebuffType);
            _commandList.Add("CleanseBuff", CleanseBuff);
            //ResourceMod
            _commandList.Add("SetCareerRes", SetCareerRes);
            _commandList.Add("ModifyCareerRes", ModifyCareerRes);
            _commandList.Add("ModifyMorale", ModifyMorale);
            _commandList.Add("ModifyAP", ModifyAp);
            _commandList.Add("ResourceToAP", ResourceToAP);
            _commandList.Add("StealAP", StealAp);
            // Utility
            _commandList.Add("Interrupt", Interrupt);
            _commandList.Add("SummonPet", SummonPet);
            _commandList.Add("SpawnMobInstance", SpawnMobInstance);
            _commandList.Add("MovePet", MovePet);
            _commandList.Add("PokeClassBuff", PokeClassBuff);
            _commandList.Add("CastPlayerEffect", CastPlayerEffect);
            _commandList.Add("CreateBuffObject", CreateBuffObject);
            _commandList.Add("PetCast", PetCast);
            _commandList.Add("TeleportToTarget", TeleportToTarget);
            _commandList.Add("PlayEffect", PlayEffect);
            _commandList.Add("PlayGroundEffect", PlayGroundEffect);
            _commandList.Add("SummonNPC", SummonNPC);
            // Item
            _commandList.Add("WarpToBindPoint", WarpToBindPoint);
            _commandList.Add("WarpToZoneJump", WarpToZoneJump);
            _commandList.Add("WarpToDungeon", WarpToDungeon);
            _commandList.Add("SummonSiegeWeapon", SummonSiegeWeapon);
            // Career
            _commandList.Add("AvengingTheDebt", AvengingTheDebt);
            _commandList.Add("BanishWeakness", BanishWeakness);
            _commandList.Add("JumpbackSnare", JumpbackSnare);
            _commandList.Add("InvokeCooldown", InvokeCooldown);

            // Positionals
            _posCommandList.Add("DamageAtPosition", DamageAtPosition);
            _posCommandList.Add("CreateLandMine", CreateLandMine);
            _posCommandList.Add("CreateBuffObject", CreateBuffObject);
            _posCommandList.Add("GroundAttack", GroundAttack);
            _posCommandList.Add("InvokeCooldown", InvokeCooldown);
        }

        #region Interface

        public void StartEffects(AbilityInfo abInfo)
        {
            if (abInfo.ConstantInfo.InvokeDelay == 0)
                InvokeEffects(abInfo);
            else
            {
                _pendingAbility = abInfo;
                _pendingAbilityInvokeTime = TCPManager.GetTimeStampMS() + abInfo.ConstantInfo.InvokeDelay;
            }
        }
        /// <summary>
        /// Receives the information about a series of ability commands to be used with respect to a target.
        /// </summary>
        private void InvokeEffects(AbilityInfo abInfo)
        {
            bool bDelayAdded = false;

            _instigator = abInfo.Instigator;

            // Direct cast.
            if (abInfo.Target != null)
            {
                _target = abInfo.Target;
                _principalTargetType = abInfo.CommandInfo[0].TargetType;

                foreach (AbilityCommandInfo cmdInfo in abInfo.CommandInfo)
                {
                    if (!_commandList.ContainsKey(cmdInfo.CommandName))
                    {
                        Log.Error("AbilityEffectInvoker", "Missing Command: " + cmdInfo.CommandName);
                        return;
                    }
                    if (cmdInfo.IsDelayedEffect)
                    {
                        if (!bDelayAdded)
                        {
                            bDelayAdded = true;
                            _delayedEffects.Add(abInfo);
                        }
                        continue;
                    }

                    InvokeCommandOnTargets(cmdInfo, abInfo, abInfo.Target);
                }
            }

            // Positional cast.
            if (abInfo.TargetPosition != null)
            {
                if (abInfo.CommandInfo[0].IsDelayedEffect)
                    _delayedEffects.Add(abInfo);

                else
                {
                    for (AbilityCommandInfo cmd = abInfo.CommandInfo[0]; cmd != null; cmd = cmd.NextCommand)
                        _posCommandList[cmd.CommandName](cmd, abInfo.Level, abInfo.TargetPosition);
                }
            }

            _totalDamage = 0;
            _damagedCount = 0;

            _caster.BuffInterface.NotifyAbilityCasted(abInfo);
        }

        /// <summary>
        /// <para>Invokes only those effects that have a delay.</para>
        /// <para>It is assumed that each ability has either a zero delay or a shared delay for its effects.</para>
        /// </summary>
        private void InvokeDelayedEffects(AbilityInfo abInfo)
        {
            _instigator = abInfo.Instigator;

            if (abInfo.Target != null)
            {
                _target = abInfo.Target;
                _principalTargetType = abInfo.CommandInfo[0].TargetType;

                List<AbilityModifier> myModifiers = AbilityMgr.GetAbilityDelayedModifiers(abInfo.Entry);

                if (myModifiers != null)
                    foreach (var modifier in myModifiers)
                        modifier.ModifyAbility(_caster, abInfo);

                foreach (AbilityCommandInfo cmdInfo in abInfo.CommandInfo)
                {
                    if (!_commandList.ContainsKey(cmdInfo.CommandName))
                        return;
                    if (!cmdInfo.IsDelayedEffect)
                        continue;

                    InvokeCommandOnTargets(cmdInfo, abInfo, _target);
                }

                _totalDamage = 0;
                _damagedCount = 0;
            }

            if (abInfo.TargetPosition == null)
                return;

            _posCommandList[abInfo.CommandInfo[0].CommandName](abInfo.CommandInfo[0], abInfo.Level, abInfo.TargetPosition);
        }

        /// <summary>
        /// Gets the targets for an ability command and invokes that command on all of them.
        /// </summary>
        private void InvokeCommandOnTargets(AbilityCommandInfo cmdInfo, AbilityInfo abInfo, Unit currentTarget)
        {
            while (true)
            {
                // Handle the correct level for appended commands here
                byte cmdLevel = cmdInfo.Entry == abInfo.Entry ? abInfo.Level : _caster.AbtInterface.GetMasteryLevelFor(AbilityMgr.GetMasteryTreeFor(cmdInfo.Entry));

                if (cmdInfo.EffectRadius > 0)
                {
                    _alliesFound = 0;
                    _targetsFound = 0;

                    if (cmdInfo.TargetType != 0)
                        currentTarget = cmdInfo.TargetType == _principalTargetType ? _target : GetTargetFor(cmdInfo.TargetType);

                    List<Unit> targets = GetTargetsFor(cmdInfo, cmdInfo.AoESource > 0 ? GetTargetFor(cmdInfo.AoESource) : currentTarget);

                    if (targets.Count == 0)
                        break;

                    _targetsFound = targets.Count;

                    //(_caster as Player)?.SendClientMessage($"Targets struck: {_targetsFound} Allied players struck: {_alliesFound}");

                    foreach (Unit target in targets)
                    {
                        if (abInfo.BoostLevel > 0 && _caster != target)
                        {
                            cmdLevel = abInfo.BoostLevel;

#if DEBUG
                            Player player = _caster as Player;
                            player?.SendClientMessage(abInfo.Name + " casting on " + target.Name + " with boosted level " + cmdLevel);
#endif
                        }

                        if ((cmdInfo.AttackingStat > 0 && _caster != target && CombatManager.CheckDefense(cmdInfo, _caster, target, true)) || !_commandList[cmdInfo.CommandName](cmdInfo, cmdLevel, target))
                            continue;

                        if (cmdInfo.NextCommand != null && cmdInfo.NextCommand.FromAllTargets)
                            InvokeCommandOnTargets(cmdInfo.NextCommand, abInfo, target);
                    }

                    if (cmdInfo.NextCommand != null && !cmdInfo.NextCommand.FromAllTargets)
                    {
                        cmdInfo = cmdInfo.NextCommand;
                        continue;
                    }
                }

                else
                {
                    if (cmdInfo.TargetType != 0)
                    {
                        currentTarget = cmdInfo.TargetType == _principalTargetType ? _target : GetTargetFor(cmdInfo.TargetType);
                    }

                    if (abInfo.BoostLevel > 0 && _caster != currentTarget)
                    {
                        cmdLevel = abInfo.BoostLevel;

#if DEBUG
                        Player player = _caster as Player;
                        player?.SendClientMessage(abInfo.Name + " casting on " + currentTarget.Name + " with boosted level " + cmdLevel);
#endif
                    }

                    if (currentTarget == null || (cmdInfo.AttackingStat > 0 && CombatManager.CheckDefense(cmdInfo, _caster, currentTarget, false)) || !_commandList[cmdInfo.CommandName](cmdInfo, cmdLevel, currentTarget))
                        return;

                    if (cmdInfo.NextCommand != null)
                    {
                        cmdInfo = cmdInfo.NextCommand;
                        continue;
                    }
                }
                break;
            }
        }

        #endregion

        #region Targeting

        private Unit GetTargetFor(CommandTargetTypes targetType)
        {
            switch (targetType)
            {
                case CommandTargetTypes.Caster: return _caster;
                case CommandTargetTypes.Ally: return _caster.CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ALLY);
                case CommandTargetTypes.AllyOrSelf:
                    Unit allyTarget = _caster.CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ALLY);
                    if (allyTarget == null || allyTarget.IsDead || !CombatInterface.IsFriend(_caster, allyTarget) || (allyTarget is Creature && !(allyTarget is Pet)))
                        return _caster;
                    return allyTarget;
                case CommandTargetTypes.AllyOrCareerTarget:
                    Unit allyPetTarget = _caster.CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ALLY);
                    if (allyPetTarget == null || allyPetTarget.IsDead || !CombatInterface.IsFriend(_caster, allyPetTarget) || (allyPetTarget is Creature && !(allyPetTarget is Pet)))
                    {
                        Player plr = _caster as Player;
                        if (plr != null && plr.CrrInterface.GetTargetOfInterest() != null)
                            return plr.CrrInterface.GetTargetOfInterest();
                        return allyPetTarget == _caster ? null : allyPetTarget;
                    }
                    return allyPetTarget == _caster ? null : allyPetTarget;
                case CommandTargetTypes.Enemy:
                    return _target;
                case CommandTargetTypes.CareerTarget:
                    Player player = _caster as Player;
                    if (player != null)
                        return player.CrrInterface.GetTargetOfInterest();
                    return (_caster as Pet)?.Owner;
                default: return _target;
            }
        }

        private readonly List<ulong> _targetRangeList = new List<ulong>(MAX_AOE_TARGETS + 1);

        /// <summary>
        /// <para>Returns a List of Units which would be affected by this command.</para> 
        /// <para>Will only ever be used if the ability can hit multiple targets (i.e. it's Group-affecting or AoE.)</para>
        /// <para>AoE abilities will never include the player specified as the Source in the returned List.</para>
        /// </summary>
        private List<Unit> GetTargetsFor(AbilityCommandInfo cmdInfo, Unit source)
        {
            int maxTargets = cmdInfo.MaxTargets > 0 ? cmdInfo.MaxTargets : MAX_AOE_TARGETS + _caster.StsInterface.GetTotalStat(Stats.EffectBuff);

            int radius = (int)(cmdInfo.EffectRadius * _caster.StsInterface.GetStatPercentageModifier(Stats.Radius));

            List<Unit> targetList = new List<Unit>(maxTargets + 1);

            if (cmdInfo.TargetType.HasFlag(CommandTargetTypes.Siege))
            {
                List<Unit> myTargetList = new List<Unit>(((Creature)_caster).SiegeInterface.CurrentTargetList);
                ((Creature)_caster).SiegeInterface.CurrentTargetList.Clear();
                return myTargetList;
            }

            if (cmdInfo.TargetType.HasFlag(CommandTargetTypes.Groupmates))
                return GetGroupTargets(cmdInfo, source);

            if (cmdInfo.TargetType == CommandTargetTypes.CareerTarget)
                return GetSporeTargets(cmdInfo, source);

            List<Object> sourceObjRanged;

            lock (source.ObjectsInRange)
                sourceObjRanged = new List<Object>(source.ObjectsInRange);

            Player plrCaster = _caster as Player;

            foreach (Object obj in sourceObjRanged)
            {
                Unit curTarget = obj as Unit;

                if (curTarget == null)
                    continue;

                // Check target is valid for ability's targeting type.
                if ((cmdInfo.TargetType == CommandTargetTypes.Enemy) ^ CombatInterface.CanAttack(_caster, curTarget))
                {
                    // If invalid target, check for ally within range of an enemy-affecting ability.
                    // This is used to determine friendly fire penalty on AoE attacks.

                    if (plrCaster != null && cmdInfo.TargetType == CommandTargetTypes.Enemy && curTarget.Realm == _caster.Realm)
                    {
                        Player plrTarget = curTarget as Player;

                        // Non-players don't count
                        if (plrTarget == null)
                            continue;

                        // Players in same group don't count
                        if (plrCaster.PriorityGroup != null && plrCaster.PriorityGroup == plrTarget.PriorityGroup)
                            continue;

                        if (cmdInfo.EffectAngle != 0)
                        {
                            if (source.CanHitWithAoE(curTarget, cmdInfo.EffectAngle, (uint)radius))
                                ++_alliesFound;
                        }

                        else if (curTarget.ObjectWithinRadiusFeet(source, radius) && source.LOSHit(curTarget))
                            ++_alliesFound;
                    }
                    continue;
                }

                if (curTarget is Player && curTarget.CbtInterface.IsPvp && _caster is Player && !_caster.CbtInterface.IsPvp)
                    continue;

                if (cmdInfo.EffectAngle != 0)
                {
                    if (source.CanHitWithAoE(curTarget, cmdInfo.EffectAngle, (uint)radius))
                    {
                        ulong dist = source.GetDistanceSquare(curTarget);

                        for (int i = 0; i < maxTargets; ++i)
                        {
                            if (i < targetList.Count && dist > _targetRangeList[i])
                                continue;

                            if (i == targetList.Count)
                            {
                                targetList.Add(curTarget);
                                _targetRangeList.Add(dist);
                            }

                            else
                            {
                                targetList.Insert(i, curTarget);
                                _targetRangeList.Insert(i, dist);
                            }

                            if (targetList.Count > maxTargets)
                            {
                                targetList.RemoveAt(maxTargets);
                                _targetRangeList.RemoveAt(maxTargets);
                            }

                            break;
                        }
                    }
                }

                else if (curTarget.ObjectWithinRadiusFeet(source, radius) && ((cmdInfo.TargetType & CommandTargetTypes.Enemy) == 0 || source.LOSHit(curTarget)))
                {
                    ulong dist = source.GetDistanceSquare(curTarget);

                    for (int i = 0; i < maxTargets; ++i)
                    {
                        if (i < targetList.Count && dist > _targetRangeList[i])
                            continue;

                        if (i == targetList.Count)
                        {
                            targetList.Add(curTarget);
                            _targetRangeList.Add(dist);
                        }

                        else
                        {
                            targetList.Insert(i, curTarget);
                            _targetRangeList.Insert(i, dist);
                        }

                        if (targetList.Count > maxTargets)
                        {
                            targetList.RemoveAt(maxTargets);
                            _targetRangeList.RemoveAt(maxTargets);
                        }

                        break;
                    }
                }
            }

            _targetRangeList.Clear();

            return targetList;
        }

        private List<Unit> GetGroupTargets(AbilityCommandInfo cmdInfo, Unit source)
        {
            List<Unit> myTargetList = new List<Unit>();

            Player plrCaster = _caster as Player;

            if (plrCaster == null)
                return myTargetList;

            Group myGroup = plrCaster.PriorityGroup;

            if (myGroup == null)
            {
                if (cmdInfo.TargetType.HasFlag(CommandTargetTypes.Caster))
                {
                    myTargetList.Add(plrCaster);
                    Pet pet = plrCaster.CrrInterface.GetTargetOfInterest() as Pet;
                    if (pet != null && pet.ObjectWithinRadiusFeet(plrCaster, cmdInfo.EffectRadius))
                        myTargetList.Add(pet);
                }
                return myTargetList;
            }

            foreach (Unit member in myGroup.GetUnitList((Player)_caster))
            {
                if (!cmdInfo.TargetType.HasFlag(CommandTargetTypes.Caster) && member == _caster)
                    continue;
                if (member.CbtInterface.IsPvp && !_caster.CbtInterface.IsPvp)
                    continue;
                if (cmdInfo.EffectAngle != 0)
                {
                    if (source.CanHitWithAoE(member, cmdInfo.EffectAngle, cmdInfo.EffectRadius))
                        myTargetList.Add(member);
                }

                else if (member.ObjectWithinRadiusFeet(source, cmdInfo.EffectRadius))
                    myTargetList.Add(member);
            }

            return myTargetList;
        }

        private List<Unit> GetSporeTargets(AbilityCommandInfo cmdInfo, Unit source)
        {
            List<Unit> myTargetList = new List<Unit>();

            Pet pet = _caster as Pet;

            if (pet == null)
                return myTargetList;

            if (pet.ObjectWithinRadiusFeet(source, cmdInfo.EffectRadius))
                myTargetList.Add(pet);

            Player petOwner = pet.Owner;

            if (petOwner == null)
                return myTargetList;

            if (petOwner.ObjectWithinRadiusFeet(source, cmdInfo.EffectRadius) && petOwner.CrrInterface.HasResource(1))
                myTargetList.Add(pet);

            return myTargetList;
        }

        #endregion

        #region Tick

        public void Update(long tick)
        {
            if (_delayedEffects.Count == 0 && _pendingAbility == null)
                return;

            if (_pendingAbility != null && _pendingAbilityInvokeTime <= TCPManager.GetTimeStampMS())
            {
                InvokeEffects(_pendingAbility);
                _pendingAbility = null;
            }

            while (_delayedEffects.Count > 0 && _delayedEffects.First().InvocationTimestamp <= TCPManager.GetTimeStampMS())
            {
                InvokeDelayedEffects(_delayedEffects.First());
                _delayedEffects.Remove(_delayedEffects.First());
            }

        }
        #endregion

        #region AbilityCommands

        #region Health/Damage

        private bool DealDamage(AbilityCommandInfo cmd, byte level, Unit target)
        {
            if (cmd.EffectRadius > 0 || cmd.FromAllTargets)
            {
                AbilityDamageInfo damageThisPass = cmd.DamageInfo.Clone(_caster);
                damageThisPass.IsAoE = true;
                if (cmd.DamageInfo.IsHeal)
                    CombatManager.HealTarget(damageThisPass, level, _instigator, target);
                else
                {
                    CombatManager.InflictDamage(damageThisPass, level, _instigator, target);
                }
                cmd.CommandResult += (short)damageThisPass.Damage;
                _totalDamage += (int)damageThisPass.Damage;
                ++_damagedCount;

                return damageThisPass.DamageEvent == 0 || damageThisPass.DamageEvent == 9;
            }

            if (cmd.DamageInfo.IsHeal)
                CombatManager.HealTarget(cmd.DamageInfo, level, _instigator, target);
            else
                CombatManager.InflictDamage(cmd.DamageInfo, level, _instigator, target);
            cmd.CommandResult += (short)(cmd.DamageInfo.Damage * cmd.DamageInfo.TransferFactor);

            if (cmd.DamageInfo.ResultFromRaw)
                _totalDamage += (short)((cmd.DamageInfo.Damage + cmd.DamageInfo.Mitigation/* + cmd.DamageInfo.Absorption*/) * cmd.DamageInfo.TransferFactor);
            else
                _totalDamage += (short)(cmd.DamageInfo.Damage * cmd.DamageInfo.TransferFactor);

            ++_damagedCount;

            return cmd.DamageInfo.DamageEvent == 0 || cmd.DamageInfo.DamageEvent == 9;
        }

        // Difficult to do with the modifier system
        private bool FlankingShot(AbilityCommandInfo cmd, byte level, Unit target)
        {
            cmd.DamageInfo.CriticalHitRate += (byte)(100 - target.PctHealth);

            CombatManager.InflictDamage(cmd.DamageInfo, level, _caster, target);
            cmd.CommandResult += (short)cmd.DamageInfo.Damage;

            return cmd.DamageInfo.DamageEvent == 0 || cmd.DamageInfo.DamageEvent == 9;
        }

        private bool SwellOfGloom(AbilityCommandInfo cmd, byte level, Unit target)
        {
            CombatManager.InflictProcDamage(cmd.DamageInfo, level, _caster, target);
            cmd.CommandResult += (short)cmd.DamageInfo.Damage;

            ((CareerInterface_BWSorc)((Player)_caster).CrrInterface).AutoBacklash = true;

            return cmd.DamageInfo.DamageEvent == 0 || cmd.DamageInfo.DamageEvent == 9;
        }

        private bool MultipleDealDamage(AbilityCommandInfo cmd, byte level, Unit target)
        {
            while (cmd.PrimaryValue > 0)
            {
                AbilityDamageInfo damageThisPass = cmd.DamageInfo.Clone(_caster);

                CombatManager.InflictDamage(damageThisPass, level, _caster, target);
                cmd.CommandResult += (short)damageThisPass.Damage;

                cmd.PrimaryValue--;
            }

            return cmd.CommandResult > 0;
        }

        private bool BounceDamage(AbilityCommandInfo cmd, byte level, Unit target)
        {
            List<Unit> targets = new List<Unit>();

            Unit lastTarget = target;

            targets.Add(target);

            for (int i = 0; i < cmd.MaxTargets - 1; ++i)
            {
                foreach (Object obj in lastTarget.ObjectsInRange)
                {
                    Unit unit = obj as Unit;

                    if (unit == null || unit.IsDead || lastTarget.GetDistanceTo(unit) > 20 || targets.Contains(unit))
                        continue;

                    if ((cmd.TargetType == CommandTargetTypes.Enemy) ^ CombatInterface.IsEnemy(_caster, unit))
                        continue;

                    targets.Add(unit);
                    break;
                }

                if (targets.Count == 0 || targets[targets.Count - 1] == lastTarget)
                    break;

                lastTarget = targets[targets.Count - 1];
            }

            foreach (Unit unit in targets)
                CombatManager.InflictDamage(cmd.DamageInfo.Clone(_caster), level, _caster, unit);

            return true;
        }

        private static bool Slay(AbilityCommandInfo cmd, byte level, Unit target)
        {
            target.ReceiveDamage(target, int.MaxValue);

            return true;
        }

        private bool PetTandemStrikeMain(AbilityCommandInfo cmd, byte level, Unit target)
        {
            CombatManager.InflictDamage(cmd.DamageInfo.Clone(_caster), level, _caster, target);
            Player plr = _caster as Player;
            if (plr != null)
            {
                Pet myPet = (Pet)plr.CrrInterface.GetTargetOfInterest();
                if (myPet != null)
                {
                    Unit myPetTarget = myPet.CbtInterface.GetCurrentTarget();
                    if (myPetTarget != null && myPet.IsInCastRange(myPetTarget, 5))
                        CombatManager.InflictDamage(cmd.DamageInfo.Clone(_caster), level, _caster, myPet.CbtInterface.GetCurrentTarget());
                }
            }
            return true;
        }

        private bool PetTandemStrikeSub(AbilityCommandInfo cmd, byte level, Unit target)
        {
            Player plr = _caster as Player;
            if (plr != null)
            {
                Pet myPet = (Pet)plr.CrrInterface.GetTargetOfInterest();
                if (myPet != null)
                {
                    Unit myPetTarget = myPet.CbtInterface.GetCurrentTarget();
                    if (myPetTarget != null && _caster.IsInCastRange(myPetTarget, (uint)cmd.PrimaryValue))
                        CombatManager.InflictDamage(cmd.DamageInfo.Clone(_caster), level, myPet, myPet.CbtInterface.GetCurrentTarget());
                    if (myPet.IsInCastRange(target, (uint)cmd.PrimaryValue))
                        CombatManager.InflictDamage(cmd.DamageInfo.Clone(_caster), level, myPet, target);
                }
            }

            return true;
        }

        private bool StealLife(AbilityCommandInfo cmd, byte level, Unit target)
        {
            ushort healValue = (ushort)(_totalDamage * (cmd.PrimaryValue / 100f));

            if (cmd.DamageInfo != null)
            {
                if (_damagedCount == 0)
                    _damagedCount = 1;

                healValue += (ushort)(cmd.DamageInfo.GetDamageForLevel(level) * _damagedCount);
            }

            if (cmd.DamageInfo != null && cmd.DamageInfo.DamageType == DamageTypes.Healing)
            {
                AbilityDamageInfo damageThisPass = cmd.DamageInfo.Clone();

                damageThisPass.Damage = healValue;
                CombatManager.LifeSteal(damageThisPass, level, _caster, target);

                return true;
            }

            int pointsHealed = CombatManager.RawLifeSteal(healValue, cmd.DamageInfo?.DisplayEntry ?? cmd.Entry, (byte)cmd.SecondaryValue, _caster, target);

            if (pointsHealed == -1)
                return false;

            cmd.CommandResult = (short)_totalDamage;

            return true;
        }

        private bool Ram(AbilityCommandInfo cmd, byte level, Unit target)
        {
            float damageFactor = level / (float)_caster.Level;
            damageFactor = (float)Math.Pow(damageFactor, 1.4);

#if DEBUG
            Log.Info("Ram", "Damage scale factor: " + damageFactor);
#endif

            target.ReceiveDamage(_caster, (uint)(target.MaxHealth * 0.035f * damageFactor));

            return true;
        }

        private bool GroundEffect(AbilityCommandInfo cmd, byte level, Unit target)
        {
            GroundTarget gt = new GroundTarget(_instigator, target.WorldPosition, GameObjectService.GetGameObjectProto(23));
            _caster.Region.AddObject(gt, _caster.Zone.ZoneId);

            BuffInfo bi = AbilityMgr.GetBuffInfo((ushort)cmd.PrimaryValue, _instigator, gt);

            if (bi == null)
            {
                gt.SetExpiry(TCPManager.GetTimeStampMS() + 1000);
                return false;
            }

            if (string.IsNullOrEmpty(bi.AuraPropagation))
            {
                if (_caster != _instigator)
                    gt.BuffInterface.QueueBuff(new BuffQueueInfo(_instigator, level, bi, LinkCaster));
                else
                    gt.BuffInterface.QueueBuff(new BuffQueueInfo(_instigator, level, bi));
            }
            else
                gt.BuffInterface.QueueBuff(new BuffQueueInfo(_instigator, level, bi, BuffEffectInvoker.CreateAura));

            if (cmd.SecondaryValue != 0)
            {
                var prms = new List<object>() { gt, cmd.SecondaryValue };
                gt.EvtInterface.AddEvent(LoopVfx, 3000, (int)(bi.Duration / 3), prms);
            }

            gt.SetExpiry(TCPManager.GetTimeStampMS() + bi.Duration * 1000 + 1000);

            return true;
        }

        private void LoopVfx(object parameters)
        {
            var Params = (List<object>)parameters;

            GroundTarget gt = Params[0] as GroundTarget;
            ushort effectId = (ushort)((int)Params[1]);
            gt.PlayEffect(effectId);
        }

        #endregion

        #region Transitions
        private bool PuntEnemy(AbilityCommandInfo cmd, byte level, Unit target)
        {
            target.ApplyKnockback(_caster, AbilityMgr.GetKnockbackInfo(cmd.Entry, cmd.PrimaryValue));

            // Give contribution for punt.
            (_caster as Player)?.UpdatePlayerBountyEvent((byte)ContributionDefinitions.PUNT_ENEMY);

            return true;
        }

        private bool PuntSelf(AbilityCommandInfo cmd, byte level, Unit target)
        {
            Player plr = _caster as Player;

            if (plr == null)
                return false;

            if (target.StsInterface.IsRooted())
                return false;

            if (target == plr)
                plr.ApplySelfKnockback(AbilityMgr.GetKnockbackInfo(cmd.Entry, cmd.PrimaryValue));
            else _caster.ApplyKnockback(target, AbilityMgr.GetKnockbackInfo(cmd.Entry, cmd.PrimaryValue));
            return true;
        }

        private bool JumpTo(AbilityCommandInfo cmd, byte level, Unit target)
        {
            if (_caster == target || !_caster.LOSHit(target))
            {
                _caster.AbtInterface.SetCooldown(cmd.Entry, -1);
                return true;
            }


            PacketOut Out = new PacketOut((byte)Opcodes.F_CATAPULT, 30);
            Out.WriteUInt16(_caster.Oid);
            Out.WriteUInt16(_caster.Zone.ZoneId);
            Out.WriteUInt16((ushort)_caster.X); // Start X
            Out.WriteUInt16((ushort)_caster.Y); // Start Y
            Out.WriteUInt16((ushort)_caster.Z); // Start Z
            Out.WriteUInt16(target.Zone.ZoneId);

            if (target.IsGameObject())
            {
                // Generate a vector that points from the GO to the caster and is 5 feet in length.
                Vector2 normalToCaster = new Vector2(_caster.X - target.X, _caster.Y - target.Y);
                normalToCaster.Normalize();
                normalToCaster.Multiply(60);

                Out.WriteUInt16((ushort)(target.X + normalToCaster.X)); // Destination X
                Out.WriteUInt16((ushort)(target.Y + normalToCaster.Y)); // Destination Y
                Out.WriteUInt16((ushort)target.Z); // Destination Z
            }

            else
            {
                Out.WriteUInt16((ushort)target.X); // Destination X
                Out.WriteUInt16((ushort)target.Y); // Destination Y
                Out.WriteUInt16((ushort)target.Z); // Destination Z
            }

            Out.WriteUInt16(0x010F);    //arc height
            //Out.WriteUInt16(0x012C); 
            //Out.WriteUInt16(0x592c);  
            Out.WriteByte(1);   //flight time in sec, can also use UInt16R or UInt32R but 1 seems to be the lowest time you can set
            Out.Fill(0, 19);
            _caster.DispatchPacket(Out, true);

            return true;
        }

        private bool Pull(AbilityCommandInfo cmd, byte level, Unit target)
        {
            if (!target.IsPlayer())
                target.ApplyKnockback(_caster, null);

            else
                ((Player)target).PulledBy(_caster, (ushort)cmd.PrimaryValue, (ushort)cmd.SecondaryValue);

            return true;
        }

        #endregion

        private static bool Interrupt(AbilityCommandInfo cmd, byte level, Unit target)
        {
            Creature crea = target as Creature;

            if (crea != null && crea.Spawn.Proto.CreatureType == (byte)GameData.CreatureTypes.SIEGE)
                return true;

            if (target.AbtInterface.IsCasting())
                target.AbtInterface.Cancel(false);

            return true;
        }

        #region Buff Management

        /// <summary>
        /// <para>Creates a buff on the target.</para>
        /// <para>Primary value is the buff's ID.</para>
        /// <para>If secondary value is not 0, it will be the proc chance of the buff.</para>
        /// </summary>
        private bool InvokeBuff(AbilityCommandInfo cmd, byte level, Unit target)
        {
            if (cmd.SecondaryValue != 0 && StaticRandom.Instance.Next(100) > cmd.SecondaryValue)
                return false;

            BuffInfo buffInfo = AbilityMgr.GetBuffInfo((ushort)cmd.PrimaryValue, _caster, target);

            if (cmd.EffectRadius > 0 || cmd.FromAllTargets)
                buffInfo.IsAoE = true;

            if (cmd.TargetType.HasFlag(CommandTargetTypes.Groupmates))
                buffInfo.IsGroupBuff = true;

            if (cmd.AoESource == CommandTargetTypes.CareerTarget && cmd.EffectRadius > 0)
            {
                Unit careerTarget = ((Player)_caster).CrrInterface.GetTargetOfInterest();

                if (careerTarget is Pet)
                {
                    target.BuffInterface.QueueBuff(new BuffQueueInfo(careerTarget, level, buffInfo));
                    return true;
                }
            }

            target.BuffInterface.QueueBuff(new BuffQueueInfo(_caster, level, buffInfo));

            return true;
        }

        private bool InvokeBuffWithDuration(AbilityCommandInfo cmd, byte level, Unit target)
        {
            BuffInfo buffInfo = AbilityMgr.GetBuffInfo((ushort)cmd.PrimaryValue, _caster, target);

            if (cmd.EffectRadius > 0 || cmd.FromAllTargets)
                buffInfo.IsAoE = true;

            if (cmd.TargetType.HasFlag(CommandTargetTypes.Groupmates))
                buffInfo.IsGroupBuff = true;

            buffInfo.Duration = (ushort)cmd.SecondaryValue;

            target.BuffInterface.QueueBuff(new BuffQueueInfo(cmd.TargetType == CommandTargetTypes.CareerTarget && cmd.EffectRadius != 0 ? ((Player)_caster).CrrInterface.GetTargetOfInterest() : _caster, level, buffInfo));

            return true;
        }

        private bool InvokeAura(AbilityCommandInfo cmd, byte level, Unit target)
        {
            BuffInfo buffInfo = AbilityMgr.GetBuffInfo((ushort)cmd.PrimaryValue, _caster, target);

            target.BuffInterface.QueueBuff(new BuffQueueInfo(_caster, level, buffInfo, BuffEffectInvoker.CreateAura));

            return true;
        }

        private bool InvokeOnYourGuard(AbilityCommandInfo cmd, byte level, Unit target)
        {
            BuffInfo buffInfo = AbilityMgr.GetBuffInfo((ushort)cmd.PrimaryValue, _caster, target);

            target.BuffInterface.QueueBuff(new BuffQueueInfo(_caster, level, buffInfo, BuffEffectInvoker.CreateOygAuraBuff));

            return true;
        }

        private bool SetPetBuff(AbilityCommandInfo cmd, byte level, Unit target)
        {
            Player plr = _caster as Player;
            Pet _pet = (Pet)plr.CrrInterface.GetTargetOfInterest();

            if (_pet != null && !_pet.IsDead)
                _pet.BuffInterface.QueueBuff(new BuffQueueInfo(plr, plr.EffectiveLevel, AbilityMgr.GetBuffInfo((ushort)cmd.PrimaryValue, plr, _pet)));

            return true;
        }

        private bool InvokeLinkedBuff(AbilityCommandInfo cmd, byte level, Unit target)
        {
            LinkedBuffInteraction lbi = new LinkedBuffInteraction((ushort)cmd.PrimaryValue, _caster, target);
            lbi.Initialize();
            return true;
        }

        private bool InvokeBouncingBuff(AbilityCommandInfo cmd, byte level, Unit target)
        {
            BuffInfo buffInfo = AbilityMgr.GetBuffInfo((ushort)cmd.PrimaryValue, _caster, target);

            target.BuffInterface.QueueBuff(new BuffQueueInfo(_caster, level, buffInfo));

            List<Player> targets = new List<Player>();

            Unit lastTarget = target;

            for (int i = 0; i < cmd.MaxTargets - 1; ++i)
            {
                foreach (Player player in lastTarget.PlayersInRange)
                {
                    if ((cmd.TargetType == CommandTargetTypes.Enemy) ^ CombatInterface.IsEnemy(_caster, player))
                        continue;

                    if (lastTarget.GetDistanceTo(player) > 30)
                        continue;

                    if (targets.Contains(player))
                        continue;

                    targets.Add(player);
                    break;
                }

                if (targets.Count == 0 || targets[targets.Count - 1] == lastTarget)
                    break;

                lastTarget = targets[targets.Count - 1];
            }

            foreach (Player plr in targets)
                plr.BuffInterface.QueueBuff(new BuffQueueInfo(_caster, level, buffInfo));

            return true;
        }

        private bool StackBuffByNearbyFoes(AbilityCommandInfo cmd, byte level, Unit target)
        {
            // Application threshold
            if (_targetsFound <= cmd.SecondaryValue)
                return false;

            BuffInfo buffInfo = AbilityMgr.GetBuffInfo((ushort)cmd.PrimaryValue, _caster, target);

            if (cmd.EffectRadius > 0 || cmd.FromAllTargets)
                buffInfo.IsAoE = true;

            buffInfo.InitialStacks = Math.Min(buffInfo.MaxStack, _targetsFound - cmd.SecondaryValue);

            target.BuffInterface.QueueBuff(new BuffQueueInfo(_caster, level, buffInfo));

            return true;
        }

        private bool InvokeGuard(AbilityCommandInfo cmd, byte level, Unit target)
        {
            LinkedBuffInteraction lbi = new LinkedBuffInteraction((ushort)cmd.PrimaryValue, _caster, target, BuffEffectInvoker.CreateGuardBuff);
            lbi.Initialize();
            // Give contribution for guarding.

            if (StaticRandom.Instance.Next(100) < TANK_GUARD_CONTRIBUTION_CHANCE)
            {
                (_caster as Player)?.UpdatePlayerBountyEvent((byte)ContributionDefinitions.TANK_GUARD);
            }
            return true;
        }

        private bool InvokeOathFriend(AbilityCommandInfo cmd, byte level, Unit target)
        {
            Player plr = _caster as Player;

            if (plr == null)
                return false;

            if (plr.Info.CareerLine == 1)
            {
                LinkedBuffInteraction lbi = new LinkedBuffInteraction((ushort)cmd.PrimaryValue, _caster, target, BuffEffectInvoker.CreateOathFriendBuff);
                lbi.Initialize();
            }
            else
            {
                LinkedBuffInteraction lbi = new LinkedBuffInteraction((ushort)cmd.PrimaryValue, _caster, target, BuffEffectInvoker.CreateDarkProtectorBuff);
                lbi.Initialize();
            }
            return true;
        }

        private static bool CleanseCc(AbilityCommandInfo cmd, byte level, Unit target)
        {
            // use of 11435 Torch of Lileath out of ZoneId=260 (Lost Vale)
            if (cmd.Entry == 11435 && target.ZoneId != 260)
                return false;

            return target.BuffInterface.CleanseCC((byte)cmd.PrimaryValue);
        }

        private static bool CleanseDebuffType(AbilityCommandInfo cmd, byte level, Unit target)
        {
            cmd.CommandResult = target.BuffInterface.CleanseDebuffType((byte)cmd.PrimaryValue, (byte)cmd.SecondaryValue);

            return cmd.CommandResult > 0;
        }

        private bool ExclusiveCleanseDebuffType(AbilityCommandInfo cmd, byte level, Unit target)
        {
            // Prevent DoK double cleanse
            if (target == _target)
                return false;

            cmd.CommandResult = target.BuffInterface.CleanseDebuffType((byte)cmd.PrimaryValue, (byte)cmd.SecondaryValue);

            return cmd.CommandResult > 0;
        }

        private bool CleanseBuff(AbilityCommandInfo cmd, byte level, Unit target)
        {
            NewBuff buff = target.BuffInterface.GetBuff((ushort)cmd.PrimaryValue, _caster);

            if (buff != null)
                buff.BuffHasExpired = true;

            return true;
        }

        #endregion

        #region Resource Management

        private bool SetCareerRes(AbilityCommandInfo cmd, byte level, Unit target)
        {
            Player plr = _caster as Player;

            plr?.CrrInterface.SetResource((byte)cmd.PrimaryValue, false);
            return true;
        }

        private bool ModifyCareerRes(AbilityCommandInfo cmd, byte level, Unit target)
        {
            Player plr = _caster as Player;

            if (plr != null)
            {
                if ((cmd.Entry == 1906 || cmd.Entry == 9244) && !plr.CrrInterface.ExperimentalMode)
                    return true;
                if (cmd.PrimaryValue == 255)
                    plr.CrrInterface.AddResource((byte)Math.Abs(cmd.LastCommand.PrimaryValue), cmd.SecondaryValue == 1);
                else if (cmd.PrimaryValue < 0)
                    plr.CrrInterface.ConsumeResource((byte)-cmd.PrimaryValue, cmd.SecondaryValue == 1);
                else
                    plr.CrrInterface.AddResource((byte)cmd.PrimaryValue, cmd.SecondaryValue == 1);
            }
            return true;
        }

        private static bool ModifyMorale(AbilityCommandInfo cmd, byte level, Unit target)
        {
            Player plrTarget = target as Player;

            if (plrTarget == null)
                return true;


            if (cmd.PrimaryValue < 0)
                plrTarget.ConsumeMorale(-cmd.PrimaryValue);
            else
                plrTarget.AddMorale(cmd.PrimaryValue);

            return true;
        }

        private static bool ModifyAp(AbilityCommandInfo cmd, byte level, Unit target)
        {
            int value = cmd.SecondaryValue == 0 ? cmd.PrimaryValue : Point2D.Lerp(cmd.PrimaryValue, cmd.SecondaryValue, level - 1 / 39f);

            Player plrTarget = target as Player;

            if (plrTarget != null)
                cmd.CommandResult += (short)plrTarget.ModifyActionPoints(value);
            else cmd.CommandResult += (short)value;
            return true;
        }

        private bool ResourceToAP(AbilityCommandInfo cmd, byte level, Unit target)
        {
            Player plrCaster = (Player)_caster;
            plrCaster.ModifyActionPoints(plrCaster.CrrInterface.CareerResource);

            return true;
        }

        private bool StealAp(AbilityCommandInfo cmd, byte level, Unit target)
        {
            if (target is Player plrTarget)
            {
                if (plrTarget == null)
                    return false;

                if (cmd.PrimaryValue != 0)
                {
                    int deltaAp = plrTarget.ModifyActionPoints((short)-cmd.PrimaryValue);
                    if (_caster is Player _plrCaster)
                        (_plrCaster).ModifyActionPoints(-deltaAp);
                }

                else
                    if (_caster is Player _plrCaster)
                    (_plrCaster).ModifyActionPoints(Math.Abs(cmd.LastCommand.CommandResult));
            }

            return true;
        }

        #endregion

        #region Pet

        private bool SummonPet(AbilityCommandInfo cmd, byte level, Unit target)
        {
            Player plr = _caster as Player;
            if (plr == null || plr.CrrInterface == null)
                return false;
            IPetCareerInterface petInterface = plr.CrrInterface as IPetCareerInterface;

            if (petInterface == null)
                return false;

            petInterface.SummonPet(cmd.Entry);
            return true;
        }

        private bool SpawnMobInstance(AbilityCommandInfo cmd, byte level, Unit target)
        {
            Player plr = _caster as Player;
            if (plr == null || plr.CrrInterface == null)
                return false;

            ushort facing = 2093;

            var X = plr.WorldPosition.X;
            var Y = plr.WorldPosition.Y;
            var Z = plr.WorldPosition.Z;


            Creature_spawn spawn = new Creature_spawn { Guid = (uint)CreatureService.GenerateCreatureSpawnGUID() };
            spawn.BuildFromProto(CreatureService.GetCreatureProto(1000155));

            spawn.WorldO = facing;
            spawn.WorldX = X + StaticRandom.Instance.Next(500);
            spawn.WorldY = Y + StaticRandom.Instance.Next(500);
            spawn.WorldZ = Z;
            spawn.ZoneId = (ushort)plr.ZoneId;
            spawn.Level = 42;

            Creature c = plr.Region.CreateCreature(spawn);
            c.PlayersInRange = plr.PlayersInRange;
            c.AiInterface.SetBrain(new AggressiveBrain(c));


            return true;
        }

        private bool MovePet(AbilityCommandInfo cmd, byte level, Unit target)
        {
            Player plr = _caster as Player;
            if (plr == null || plr.CrrInterface == null)
                return false;

            if (plr.CrrInterface.GetTargetOfInterest() != null)
                plr.CrrInterface.GetTargetOfInterest().MvtInterface.Teleport(_caster.WorldPosition);

            return true;
        }

        private bool PetCast(AbilityCommandInfo cmd, byte level, Unit target)
        {
            var player = _caster as Player;
            if (player == null)
                return true;
            var pet = player.CrrInterface.GetTargetOfInterest();
            pet?.AbtInterface.StartCast(pet, (ushort)cmd.PrimaryValue, 0, 0, level);

            return true;
        }

        private bool TeleportToTarget(AbilityCommandInfo cmd, byte level, Unit target)
        {

            PacketOut Out = new PacketOut((byte)Opcodes.F_CATAPULT, 30);
            Out.WriteUInt16(_caster.Oid);
            Out.WriteUInt16(_caster.Zone.ZoneId);
            Out.WriteUInt16((ushort)_caster.X); // Start X
            Out.WriteUInt16((ushort)_caster.Y); // Start Y
            Out.WriteUInt16((ushort)_caster.Z); // Start Z
            Out.WriteUInt16(target.Zone.ZoneId);

            // Generate a vector that points from the GO to the caster and is 5 feet in length.
            Vector2 normalToCaster = new Vector2(_caster.X - target.X, _caster.Y - target.Y);
            normalToCaster.Normalize();
            normalToCaster.Multiply(60);

            Out.WriteUInt16((ushort)(target.X + (uint)normalToCaster.X)); // Destination X
            Out.WriteUInt16((ushort)(target.Y + (uint)normalToCaster.Y)); // Destination Y
            Out.WriteUInt16((ushort)target.Z); // Destination Z
            Out.WriteUInt16(0x0100);    //arc height
            Out.WriteUInt32R(1);    //animation time, cant seem to get it faster then 1 sec, will come back to this later.
            Out.Fill(0, 19);
            _caster.DispatchPacket(Out, true);

            return true;
        }
        /// <summary>
        /// so abilities can call specific effects similar to how .playeffect works
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="level"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool PlayEffect(AbilityCommandInfo cmd, byte level, Unit target)
        {
            //need to find a way to toggle it off again if its a persistant VFX.
            target.PlayEffect((ushort)cmd.PrimaryValue);
            return true;
        }
        /// <summary>
        /// to play a effect from a location, similar to the command .playeffect but instead of on a person, place it on the floor at the target location
        /// still need a way to turn the effect off. Initially I had a groundtarget that disappeared after a while aking to groundattack but I could not apply effect to it.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="level"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool PlayGroundEffect(AbilityCommandInfo cmd, byte level, Unit target)
        {
            //still need a way to turn the effect off. Initially I had a groundtarget that disappeared after a while aking to groundattack but I could not apply effect to it.
            target.PlayEffect((ushort)cmd.PrimaryValue, target.WorldPosition);


            return true;
        }

        /// <summary>
        /// Summon an aggressive NPC
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="level"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool SummonNPC(AbilityCommandInfo cmd, byte level, Unit target)
        {
            target = target == null ? _caster : _caster.CbtInterface.GetCurrentTarget();

            if (target is Player)
            {
                ushort facing = 2093;

                var X = target.WorldPosition.X;
                var Y = target.WorldPosition.Y;
                var Z = target.WorldPosition.Z;

                for (int i = 0; i < cmd.SecondaryValue; i++)
                {
                    var spawn = new Creature_spawn {Guid = (uint) CreatureService.GenerateCreatureSpawnGUID()};
                    var proto = CreatureService.GetCreatureProto((uint) cmd.PrimaryValue);
                    spawn.BuildFromProto(proto);

                    spawn.WorldO = facing;
                    spawn.WorldX = X + StaticRandom.Instance.Next(500);
                    spawn.WorldY = Y + StaticRandom.Instance.Next(500);
                    spawn.WorldZ = Z;
                    spawn.ZoneId = (ushort) target.ZoneId;

                    var player = _caster as Player;

                    var creature = target.Region.CreateCreature(spawn);
                    creature.AiInterface.SetBrain(new AggressiveBrain(creature));
                    creature.CbtInterface.SetTarget(_caster.CbtInterface.GetCurrentTarget().Oid,
                        TargetTypes.TARGETTYPES_TARGET_NONE);

                    creature.EvtInterface.AddEvent(creature.Destroy, 30000, 1);
                }

            }

            return true;
        }

        #endregion

        private bool CastPlayerEffect(AbilityCommandInfo cmd, byte level, Unit target)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);
            Out.WriteUInt16(_caster.Oid);
            Out.WriteUInt16(target.Oid);
            Out.WriteUInt16((ushort)cmd.PrimaryValue);
            Out.WriteByte((byte)cmd.SecondaryValue);
            Out.WriteByte(0);
            Out.WriteByte(5);
            Out.WriteByte(0);

            Player plr = _caster as Player;

            plr?.DispatchPacket(Out, true);
            return true;
        }

        private bool CreateBuffObject(AbilityCommandInfo cmd, byte level, Unit target)
        {
            BuffInfo bi = AbilityMgr.GetBuffInfo((ushort)cmd.PrimaryValue);

            if (bi == null)
                return false;

            BuffHostObject bho = new BuffHostObject((Player)_caster, target.WorldPosition, CreatureService.GetCreatureProto((uint)cmd.SecondaryValue), TCPManager.GetTimeStampMS() + (bi.Duration + 3) * 1000);
            _caster.Region.AddObject(bho, _caster.Zone.ZoneId);

            bho.BuffInterface.QueueBuff(new BuffQueueInfo(_caster, level, bi));

            return true;
        }

        // Career-Linked

        private bool PokeClassBuff(AbilityCommandInfo cmd, byte level, Unit target)
        {
            _caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.Manual, null, target);
            return true;
        }

        private bool AvengingTheDebt(AbilityCommandInfo cmd, byte level, Unit target)
        {
            if (!cmd.LastCommand.DamageInfo.WasLethalDamage)
                return false;

            CombatManager.HealTarget(cmd.DamageInfo.Clone(), level, _caster, _caster,100);

            Unit oathFriend = ((Player)_caster).CrrInterface.GetTargetOfInterest();

            if (oathFriend != null)
                CombatManager.HealTarget(cmd.DamageInfo, level, _caster, oathFriend,100);

            return true;
        }

        private bool BanishWeakness(AbilityCommandInfo cmd, byte level, Unit target)
        {
            Player plrTarget = target as Player;

            plrTarget?.ModifyActionPoints((short)(cmd.PrimaryValue * cmd.LastCommand.CommandResult));

            cmd.DamageInfo.DamageBonus += cmd.LastCommand.CommandResult - 1;

            CombatManager.HealTarget(cmd.DamageInfo, level, _caster, target);

            return true;
        }

        private bool JumpbackSnare(AbilityCommandInfo cmd, byte level, Unit target)
        {
            List<Unit> myTargetList = new List<Unit>();
            byte count = 0;
            List<Object> sourceObjRanged;

            lock (_caster.ObjectsInRange)
                sourceObjRanged = new List<Object>(_caster.ObjectsInRange);

            foreach (Object obj in sourceObjRanged)
            {
                Unit curTarget = obj as Unit;

                if (curTarget == null || curTarget == _caster || !CombatInterface.CanAttack(_caster, curTarget))
                    continue;

                if (curTarget is Player && curTarget.CbtInterface.IsPvp && _caster is Player && !_caster.CbtInterface.IsPvp)
                    continue;

                if (curTarget.ObjectWithinRadiusFeet(_caster, cmd.PrimaryValue) && _caster.LOSHit(curTarget))
                {
                    myTargetList.Add(curTarget);
                    ++count;
                }

                if (count == MAX_AOE_TARGETS)
                    break;
            }

            if (count == 0)
                return false;

            count = 0;

            foreach (Unit unit in myTargetList)
            {
                if (unit.ImmuneToCC((byte)CrowdControlTypes.Snare, _caster, cmd.Entry))
                    continue;

                ++count;

                unit.BuffInterface.QueueBuff(new BuffQueueInfo(_caster, level, AbilityMgr.GetBuffInfo(cmd.Entry, _caster, unit)));
            }

            return count > 0;
        }

        private bool InvokeCooldown(AbilityCommandInfo cmd, byte level, Unit target)
        {
            _caster.AbtInterface.SetCooldown((ushort)cmd.PrimaryValue, AbilityMgr.GetCooldownFor((ushort)cmd.PrimaryValue) * 1000);
            return true;
        }

        #endregion

        #region ItemCommands

        private bool WarpToBindPoint(AbilityCommandInfo cmd, byte level, Unit target)
        {
            Player plr = _caster as Player;

            RallyPoint rallyPoint = RallyPointService.GetRallyPoint(plr._Value.RallyPoint);

            if (rallyPoint != null)
                plr.Teleport(rallyPoint.ZoneID, rallyPoint.WorldX, rallyPoint.WorldY, rallyPoint.WorldZ, rallyPoint.WorldO);

            return true;
        }

        private bool WarpToZoneJump(AbilityCommandInfo cmd, byte level, Unit target)
        {
            Player plr = (Player)_caster;

            Zone_jump zoneJump = ZoneService.GetZoneJump((uint)cmd.PrimaryValue);

            if (zoneJump != null)
                plr.Teleport(zoneJump.ZoneID, zoneJump.WorldX, zoneJump.WorldY, zoneJump.WorldZ, zoneJump.WorldO);

            return true;
        }

        private bool WarpToDungeon(AbilityCommandInfo cmd, byte level, Unit target)
        {
            Player plr = (Player)_caster;
            Zone_jump zoneJump = null;

            if (plr.Realm == Realms.REALMS_REALM_ORDER)
            {
                zoneJump = ZoneService.GetZoneJump((uint)cmd.PrimaryValue);
            }
            else
            {
                zoneJump = ZoneService.GetZoneJump((uint)cmd.SecondaryValue);
            }

            if (zoneJump != null)
                plr.Teleport(zoneJump.ZoneID, zoneJump.WorldX, zoneJump.WorldY, zoneJump.WorldZ, zoneJump.WorldO);

            return true;
        }

        private bool SummonSiegeWeapon(AbilityCommandInfo cmd, byte level, Unit target)
        {
            Player player = (Player)_caster;

            var siege = Siege.SpawnSiegeWeapon(player, (ushort) player.ZoneId, (uint)cmd.PrimaryValue, true);
            player.Region.AddObject(siege, (ushort)player.ZoneId);
            player.Region.Campaign.SiegeManager.Add(siege, player.Realm);

            //if (player.CurrentKeep == null)
            //{
            //    var siege = Siege.SpawnSiegeWeapon(player, this, (uint) cmd.PrimaryValue, true);
            //    Region.AddObject(siege, Info.ZoneId);
            //}
            //else
            //    player.CurrentKeep.SpawnSiegeWeapon(player, (uint) cmd.PrimaryValue);
            return true;
        }

        #endregion

        #region Position Commands

        private void DamageAtPosition(AbilityCommandInfo cmdInfo, byte level, Point3D position)
        {
            int count = 0;

            List<Unit> myTargetList = new List<Unit>();

            foreach (Object obj in _caster.Region.Objects)
            {
                if (!(obj is Unit))
                    continue;

                var curTarget = obj.GetUnit();

                if (!CombatInterface.CanAttack(_caster, curTarget))
                    continue;

                if (curTarget is Player && curTarget.CbtInterface.IsPvp && _caster is Player && !_caster.CbtInterface.IsPvp)
                    continue;

                if (curTarget.Get2DDistanceToWorldPoint(position) <= cmdInfo.EffectRadius)
                {
                    myTargetList.Add(curTarget);
                    count++;
                }

                if (count == 9)
                    break;
            }

            if (myTargetList.Count == 0)
                return;

            foreach (Unit target in myTargetList)
                CombatManager.InflictDamage(cmdInfo.DamageInfo, level, _caster, target);
        }

        private void CreateLandMine(AbilityCommandInfo cmd, byte level, Point3D position)
        {
            LandMine l = new LandMine((Player)_caster, position, CreatureService.GetCreatureProto(_caster.Realm == Realms.REALMS_REALM_ORDER ? 5623 : (uint)33036));
            _caster.Region.AddObject(l, _caster.Zone.ZoneId);
            l.BuffInterface.QueueBuff(new BuffQueueInfo(_caster, _caster.AbtInterface.GetMasteryLevelFor(3), AbilityMgr.GetBuffInfo(cmd.Entry, _caster, l)));
        }

        private void CreateBuffObject(AbilityCommandInfo cmd, byte level, Point3D position)
        {
            BuffInfo bi = AbilityMgr.GetBuffInfo((ushort)cmd.PrimaryValue);

            if (bi == null)
                return;

            BuffHostObject bho = new BuffHostObject(_caster, position, CreatureService.GetCreatureProto((uint)cmd.SecondaryValue), TCPManager.GetTimeStampMS() + (bi.Duration + 3) * 1000);
            _caster.Region.AddObject(bho, _caster.Zone.ZoneId);

            bi = AbilityMgr.GetBuffInfo((ushort)cmd.PrimaryValue, _caster, bho);

            bho.BuffInterface.QueueBuff(new BuffQueueInfo(_caster, level, bi));
        }

        private void GroundAttack(AbilityCommandInfo cmd, byte level, Point3D position)
        {
            GroundTarget gt = new GroundTarget(_instigator, position, GameObjectService.GetGameObjectProto(23));
            _caster.Region.AddObject(gt, _caster.Zone.ZoneId);

            BuffInfo bi = AbilityMgr.GetBuffInfo((ushort)cmd.PrimaryValue, _instigator, gt);

            if (bi == null)
            {
                gt.SetExpiry(TCPManager.GetTimeStampMS() + 1000);
                return;
            }

            if (string.IsNullOrEmpty(bi.AuraPropagation))
            {
                if (_caster != _instigator)
                    gt.BuffInterface.QueueBuff(new BuffQueueInfo(_instigator, level, bi, LinkCaster));
                else
                    gt.BuffInterface.QueueBuff(new BuffQueueInfo(_instigator, level, bi));
            }
            else
                gt.BuffInterface.QueueBuff(new BuffQueueInfo(_instigator, level, bi, BuffEffectInvoker.CreateAura));
            gt.SetExpiry(TCPManager.GetTimeStampMS() + bi.Duration * 1000 + 1000);
        }

        private void InvokeCooldown(AbilityCommandInfo cmd, byte level, Point3D position)
        {
            InvokeCooldown(cmd, level, null);
        }

        #endregion

        public void ClearPending()
        {
            _delayedEffects.Clear();
        }

        public void LinkCaster(NewBuff b)
        {
            if (b == null)
                return;

            b.OptionalObject = _caster;
        }
    }
}