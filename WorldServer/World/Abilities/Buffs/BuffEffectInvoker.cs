using Common;
using FrameWork;
using GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using WorldServer.Managers;
using WorldServer.Services.World;
using WorldServer.World.Abilities.Buffs.SpecialBuffs;
using WorldServer.World.Abilities.CareerInterfaces;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Battlefronts.Keeps;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using WorldServer.World.Positions;
using WorldServer.World.Scenarios.Objects;
using Item = WorldServer.World.Objects.Item;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Abilities.Buffs
{
    /// <summary>
    /// Holds and invokes buff commands.
    /// </summary>
    [Service(typeof(WorldMgr))]
    public static class BuffEffectInvoker
    {
        const byte MAX_AOE_TARGETS = 9;
        private const int HOLD_THE_LINE_CONTRIBUTION_CHANCE = 8;

        private const byte BUFF_START = 1;
        private const byte BUFF_TICK = 2;
        private const byte BUFF_END = 4;
        private const byte BUFF_REMOVE = 5;

        private delegate bool BuffCommandDelegate(NewBuff hostBuff, BuffCommandInfo cmd, Unit target);
        private delegate void BuffAbilityUseCommandDelegate(NewBuff hostBuff, BuffCommandInfo cmd, AbilityInfo abInfo);

        private delegate bool BuffEventCheckDelegate(NewBuff hostBuff, AbilityDamageInfo damageInfo, uint value, Unit eventInstigator);

        private delegate bool BuffEventCommandDelegate(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator);
        private delegate void PetEventCommandDelegate(NewBuff hostBuff, BuffCommandInfo cmd, Pet myPet);
        private delegate void ResourceEventCommandDelegate(NewBuff hostBuff, BuffCommandInfo cmd, byte oldVal, ref byte change);
        private delegate void ItemEventCommandDelegate(NewBuff hostBuff, BuffCommandInfo cmd, Item_Info itemInfo);

        private static readonly Dictionary<string, BuffEventCheckDelegate> _eventChecks = new Dictionary<string, BuffEventCheckDelegate>();

        private static readonly Dictionary<string, BuffCommandDelegate> _commandList = new Dictionary<string, BuffCommandDelegate>();
        private static readonly Dictionary<string, BuffAbilityUseCommandDelegate> _abUseCommandList = new Dictionary<string, BuffAbilityUseCommandDelegate>();

        private static readonly Dictionary<string, BuffEventCommandDelegate> _damageEventCommandList = new Dictionary<string, BuffEventCommandDelegate>();
        private static readonly Dictionary<string, PetEventCommandDelegate> _petEventCommandList = new Dictionary<string, PetEventCommandDelegate>();
        private static readonly Dictionary<string, ResourceEventCommandDelegate> _resourceEventCommandList = new Dictionary<string, ResourceEventCommandDelegate>(); // consider List
        private static readonly Dictionary<string, ItemEventCommandDelegate> _itemEventCommandList = new Dictionary<string, ItemEventCommandDelegate>(); // same

        [LoadingFunction(false)]
        public static void LoadBuffCommands()
        {
            _eventChecks.Clear();
            _commandList.Clear();
            _abUseCommandList.Clear();
            _damageEventCommandList.Clear();
            _petEventCommandList.Clear();
            _resourceEventCommandList.Clear();
            _itemEventCommandList.Clear();

            #region Event Checks

            _eventChecks.Add("DamageIsCritical", DamageIsCritical);
            _eventChecks.Add("MutatedCritical", MutatedCritical);
            _eventChecks.Add("StealthBreak", StealthBreak);
            _eventChecks.Add("IsSingleTargetDamage", IsSingleTargetDamage);
            _eventChecks.Add("DamageIsFromStat", DamageIsFromStat);
            _eventChecks.Add("DamageIsMeleeAbility", DamageIsMeleeAbility);
            _eventChecks.Add("DamageIsInTree", DamageIsInTree);
            _eventChecks.Add("DamageIsFromID", DamageIsFromID);
            _eventChecks.Add("DamageIsCriticalFromTree", DamageIsCriticalFromTree);
            _eventChecks.Add("GiftOfKhaine", GiftOfKhaine);
            _eventChecks.Add("TargetHPBelow", TargetHPBelow);
            _eventChecks.Add("InstigatorHPBelow", InstigatorHPBelow);
            _eventChecks.Add("TargetNotCaster", TargetNotCaster);
            _eventChecks.Add("IsFlanking", IsFlanking);
            _eventChecks.Add("IsBehind", IsBehind);
            _eventChecks.Add("IsFlankingWithAbility", IsFlankingWithAbility);
            _eventChecks.Add("TOIWithinRange", TOIWithinRange);
            _eventChecks.Add("AllowDismount", AllowDismount);
            _eventChecks.Add("DamageThrottle", DamageThrottle);
            _eventChecks.Add("IsSingleTargetCriticalDamage", IsSingleTargetCriticalDamage);
            _eventChecks.Add("DamageIsFromMagicType", DamageIsFromMagicType);
            _eventChecks.Add("CheckDefenseFlags", CheckDefenseFlags);
            _eventChecks.Add("CheckDefenseFlagsNotSelf", CheckDefenseFlagsNotSelf);
            _eventChecks.Add("InstigatorNotSelf", InstigatorNotSelf);
            _eventChecks.Add("MissingBuff", MissingBuff);
            #endregion

            #region General Commands

            #region Damage
            _commandList.Add("DamageOverTime", DamageOverTime);
            _commandList.Add("IllegalZone", IllegalZone);
            _commandList.Add("InstantTickDoT", InstantTickDoT);
            _commandList.Add("RampDamageOverTime", RampDamageOverTime);
            _commandList.Add("DealChannelDamage", DealDamage);
            _commandList.Add("DealProcDamage", DealProcDamage);
            _commandList.Add("HealHPBelow", HealHPBelow);
            _commandList.Add("DamageWhileMoving", DamageWhileMoving);
            _commandList.Add("Slay", Slay);
            _commandList.Add("Resurrection", Resurrection);
            _commandList.Add("Shield", Shield);
            _commandList.Add("StealLife", StealLife);
            _commandList.Add("NapalmGrenade", NapalmGrenade);
            _commandList.Add("BrimstoneBauble", BrimstoneBauble);
            _commandList.Add("ArtilleryStrike", ArtilleryStrike);
            #endregion

            #region Buffs
            _commandList.Add("InvokeBuff", InvokeBuff);
            _commandList.Add("InvokeChildBuff", InvokeChildBuff);
            _commandList.Add("InvokeBuffOnPet", InvokeBuffOnPet);
            _commandList.Add("InvokeAuraOnPet", InvokeAuraOnPet);
            _commandList.Add("ReapplyBuff", ReapplyBuff);
            _commandList.Add("ReapplyClassBuff", ReapplyClassBuff);
            _commandList.Add("ReapplyGroupBuff", ReapplyGroupBuff);
            _commandList.Add("CleanseDebuffType", CleanseDebuffType);
            _commandList.Add("RemoveBuff", RemoveBuff);
            #endregion

            #region Resource Management
            _commandList.Add("SetCareerRes", SetCareerRes);
            _commandList.Add("ModifyCareerRes", ModifyCareerRes);
            _commandList.Add("SetAP", SetAp);
            _commandList.Add("ModifyAP", ModifyAp);
            _commandList.Add("StealAP", StealAp);
            _commandList.Add("ModifyMorale", ModifyMorale);
            _commandList.Add("Panic", Panic);

            #endregion

            #region Stats
            _commandList.Add("ModifyStat", ModifyStat);
            _commandList.Add("ModifyStatNoStack", ModifyStatNoStack);
            _commandList.Add("ModifyStatByNearbyFoes", ModifyStatByNearbyFoes);
            _commandList.Add("ModifyStatByNearbyAllies", ModifyStatByNearbyAllies);
            _commandList.Add("ModifyStatByResourceLevel", ModifyStatByResourceLevel);
            _commandList.Add("ModifyPercentageStatByNearbyAllies", ModifyPercentageStatByNearbyAllies);
            _commandList.Add("ModifyPercentageStatByResourceLevel", ModifyPercentageStatByResourceLevel);
            _commandList.Add("ModifyStatIfHasResource", ModifyStatIfHasResource);
            _commandList.Add("AddCasterStat", AddCasterStat);
            _commandList.Add("ModifyPercentageStat", ModifyPercentageStat);
            _commandList.Add("ModifyPercentageStatNoStack", ModifyPercentageStatNoStack);
            _commandList.Add("ModifyStatRealmwise", ModifyStatRealmwise);
            _commandList.Add("GiftItemStatTo", GiftItemStatTo);
            _commandList.Add("AddItemStatMultiplier", AddItemStatMultiplier);
            #endregion

            #region Crowd Control
            _commandList.Add("ModifySpeed", ModifySpeed);
            _commandList.Add("Root", Root);
            _commandList.Add("Grapple", Grapple);
            _commandList.Add("ApplyCC", ApplyCc);
            _commandList.Add("SetImmovable", SetImmovable);
            _commandList.Add("WindsKnockback", WindsKnockback);
            _commandList.Add("Pull", Pull);
            _commandList.Add("RiftPull", RiftPull);
            _commandList.Add("CCGuard", CCGuard);
            #endregion

            #region Utility
            _commandList.Add("GrantTempAbility", GrantTempAbility);
            _commandList.Add("DetauntDamage", DetauntDamage);
            _commandList.Add("AddBuffLine", AddBuffParameter);
            _commandList.Add("RegisterModifiers", RegisterModifiers);
            _commandList.Add("DetauntWard", DetauntWard);
            _commandList.Add("CastPlayerEffect", CastPlayerEffect);
            _commandList.Add("ActivationEffect", ActivationEffect);
            _commandList.Add("Bolster", Bolster);
            _commandList.Add("InvokeCooldown", InvokeCooldown);
            _commandList.Add("Chickenize", Chickenize);
            _commandList.Add("Hotswap", Hotswap);
            _commandList.Add("ResourceHandler", ResourceHandler);
            _commandList.Add("GfxMod", GfxMod);
            _commandList.Add("BlockScenarioQueue", BlockScenarioQueue);
            #endregion

            #region Archetype Specific
            _commandList.Add("HoldTheLine", HoldTheLine);
            _commandList.Add("TauntDamage", TauntMob);
            _commandList.Add("Challenge", TauntMob);
            _commandList.Add("MoveAndShoot", MoveAndShoot);
            #endregion

            #region Class Specific
            _commandList.Add("BerserkerRegen", BerserkerRegen);
            _commandList.Add("GitToTheChoppa", GitToTheChoppa);
            _commandList.Add("ObjectEffectState", ObjectEffectState);
            _commandList.Add("PokeClassBuff", PokeClassBuff);
            _commandList.Add("BounceBuff", BounceBuff);
            _commandList.Add("RestoreAPOnDeath", RestoreAPOnDeath);
            _commandList.Add("RefreshIfMoving", RefreshIfMoving);
            #endregion

            #region NPCs
            _commandList.Add("SpawnCreature", SpawnCreature);
            #endregion

            #region Pets
            _commandList.Add("ManagePetAbilities", ManagePetAbilities);
            _commandList.Add("ResetAttackTimer", ResetAttackTimer);
            _commandList.Add("MasterPetModifyStat", MasterPetModifyStat);
            _commandList.Add("PetModifyStat", PetModifyStat);
            _commandList.Add("PetModifySpeed", PetModifySpeed);
            _commandList.Add("MasterModifyPercentageStatReverse", MasterModifyPercentageStatReverse);
            _commandList.Add("TickMoraleIfHasPet", TickMoraleIfHasPet);
            _commandList.Add("DismissPet", DismissPet);
            _commandList.Add("SummonVanityPet", SummonVanityPet);
            #endregion

            #region Items
            _commandList.Add("Mount", Mount);
            _commandList.Add("Backpack", Backpack);
            _commandList.Add("GreatweaponMastery", GreatweaponMastery);
            _commandList.Add("OppressingBlows", OppressingBlows);
            _commandList.Add("ModifyStatIfHasShield", ModifyStatIfHasShield);
            _commandList.Add("ModifyPercentageStatIfHasShield", ModifyPercentageStatIfHasShield);
            _commandList.Add("ItemSwap", ItemSwap);
            #endregion

            #endregion

            _abUseCommandList.Add("DamageByAbilityType", DamageByAbilityType);
            _abUseCommandList.Add("ModifyAP", AbEventModifyAp);
            _abUseCommandList.Add("InvokeBuffOnResourceAttack", InvokeBuffOnResourceAttack);
            _abUseCommandList.Add("InvokeBuff", InvokeBuff);
            _abUseCommandList.Add("DealDamageOnFinisherUse", DealDamageOnFinisherUse);

            #region Damage Event Commands

            #region Damage
            _damageEventCommandList.Add("DealDamage", DealDamage);
            _damageEventCommandList.Add("DealProcDamage", DealProcDamage);
            _damageEventCommandList.Add("HealHPBelow", HealHPBelow);
            _damageEventCommandList.Add("StealLife", EventStealLife);
            _damageEventCommandList.Add("StealLifeFromDamageInfo", EventStealLifeFromDamageInfo);
            _damageEventCommandList.Add("Shield", EventShield);
            _damageEventCommandList.Add("Resurrection", EventResurrection);
            _damageEventCommandList.Add("ReduceDamage", ReduceDamage);
            _damageEventCommandList.Add("DamageByCareerResource", DamageByCareerResource);
            _damageEventCommandList.Add("SelfRez", SelfRez);
            #endregion

            #region Buffs
            _damageEventCommandList.Add("InvokeBuff", InvokeBuff);
            _damageEventCommandList.Add("InvokeBuffIfResAttack", InvokeBuffIfResAttack);
            _damageEventCommandList.Add("RemoveBuff", RemoveBuff);
            #endregion

            #region Resource
            _damageEventCommandList.Add("ModifyAP", EventModifyAp);
            _damageEventCommandList.Add("ModifyCareerRes", EventModifyCareerRes);
            _damageEventCommandList.Add("ModifyCareerResOver", BuffOverrideModifyCareerRes);
            _damageEventCommandList.Add("SetCareerRes", EventSetCareerRes);
            _damageEventCommandList.Add("ModifyMorale", EventModifyMorale);
            #endregion

            #region Archetype
            _damageEventCommandList.Add("TauntDamage", TauntDamage);
            _damageEventCommandList.Add("Challenge", ChallengeDamage);
            _damageEventCommandList.Add("TauntRemoveStack", TauntRemoveStack);
            #endregion

            #region Utility
            _damageEventCommandList.Add("DetauntDamage", EventDetauntDamage);
            _damageEventCommandList.Add("RemoveStack", RemoveStack);
            _damageEventCommandList.Add("CastPlayerEffect", EventCastPlayerEffect);
            _damageEventCommandList.Add("PuntEnemy", PuntEnemy);
            _damageEventCommandList.Add("SoftRefresh", SoftRefresh);
            _damageEventCommandList.Add("ResourceHandler", ResourceHandler);
            #endregion

            #region Class Specific
            _damageEventCommandList.Add("SpreadBuff", SpreadBuff);
            _damageEventCommandList.Add("DaGreenest", DaGreenest);
            _damageEventCommandList.Add("IncreaseCritDamageByHPLost", IncreaseCritDamageByHPLost);
            _damageEventCommandList.Add("AutoLife", AutoLife);
            _damageEventCommandList.Add("TOISplitDamage", ToiSplitDamage);
            _damageEventCommandList.Add("AddCriticalChance", AddCriticalChance);
            _damageEventCommandList.Add("AddDamageBonus", AddDamageBonus);
            _damageEventCommandList.Add("DealBacklashAoE", DealBacklashAoE);
            _damageEventCommandList.Add("Slay", EventSlay);
            _damageEventCommandList.Add("PassItOn", PassItOn);
            _damageEventCommandList.Add("AMShamanResOnCrit", AmShamanResOnCrit);
            _damageEventCommandList.Add("CleanseDebuffType", CleanseDebuffType);
            _damageEventCommandList.Add("TransferDamage", TransferDamage);
            _damageEventCommandList.Add("BroadSwings", BroadSwings);
            #endregion

            #endregion

            #region Pet Event Commands
            _petEventCommandList.Add("MasterPetModifyStat", MasterPetModifyStat);
            _petEventCommandList.Add("PetModifyStat", PetModifyStat);
            _petEventCommandList.Add("PetModifySpeed", PetModifySpeed);
            _petEventCommandList.Add("MasterModifyPercentageStatReverse", MasterModifyPercentageStatReverse);
            _petEventCommandList.Add("InvokeBuffOnPet", InvokeBuffOnPet);
            _petEventCommandList.Add("InvokeAuraOnPet", InvokeAuraOnPet);
            _petEventCommandList.Add("MasterInvokeBuffOnPetDie", MasterInvokeBuffOnPetDie);
            _petEventCommandList.Add("MasterModifyMoraleOnPetDie", MasterModifyMoraleOnPetDie);
            #endregion

            #region Resource Event Commands
            _resourceEventCommandList.Add("ModifyCareerRes", ResEventModifyCareerRes);
            _resourceEventCommandList.Add("ModifyStatByResourceLevel", ResEventModifyStatByResourceLevel);
            _resourceEventCommandList.Add("ModifyPercentageStatByResourceLevel", ResEventModifyPercentageStatByResourceLevel);
            _resourceEventCommandList.Add("ModifyStatIfHasResource", ResEventModifyStatIfHasResource);
            _resourceEventCommandList.Add("ViolentImpacts", ViolentImpacts);
            #endregion

            _itemEventCommandList.Add("GreatweaponMastery", EventGreatweaponMastery);
            _itemEventCommandList.Add("OppressingBlows", EventOppressingBlows);
            _itemEventCommandList.Add("ModifyStatIfHasShield", EventModifyStatIfHasShield);
            _itemEventCommandList.Add("ModifyPercentageStatIfHasShield", EventModifyPercentageStatIfHasShield);
        }

        #region Interface

        /// <summary>
        /// Gets the targets for an ability command and invokes that command on all of them.
        /// </summary>
        public static void InvokeCommand(NewBuff hostBuff, BuffCommandInfo cmd, Unit lastTarget)
        {
            uint iterations = 0;

            while (true)
            {
                ++iterations;

                if (iterations > 50)
                {
                    Log.Error("BuffEffectInvoker.InvokeCommand", "Potential infinite recursion detected on " + cmd.Entry);
                    return;
                }

                if (!_commandList.ContainsKey(cmd.CommandName))
                {
                    Log.Error("BuffEffectInvoker", "Command not found for entry " + hostBuff.Entry + ": " + cmd.CommandName);
                    return;
                }

                if (cmd.EffectRadius > 0 && cmd.AoESource > 0)
                {
                    List<Unit> aoeTargets = GetTargetsFor(hostBuff, cmd, cmd.AoESource > 0 ? GetTargetFor(hostBuff, cmd.AoESource) : hostBuff.Target);

                    foreach (Unit aoeTarget in aoeTargets)
                    {
                        if (!_commandList[cmd.CommandName](hostBuff, cmd, aoeTarget))
                            continue;

                        if (cmd.NextCommand != null && cmd.NextCommand.TargetType == 0)
                            InvokeAoEChain(hostBuff, cmd.NextCommand, aoeTarget);
                    }

                    while (cmd != null)
                    {
                        cmd = cmd.NextCommand;

                        // Made our way out of a possible AoE chain, continue executing.
                        if (cmd != null && cmd.TargetType > 0)
                            break;
                    }

                    if (cmd != null)
                        continue;
                }
                else
                {
                    var target = cmd.TargetType == 0 ? lastTarget : GetTargetFor(hostBuff, cmd.TargetType);

                    if (cmd.TargetType == CommandTargetTypes.NPCAlly)
                    {
                        var possibleTargets = hostBuff.Caster.GetInRange<Creature>(cmd.EffectRadius).Where(x => x.Entry == cmd.SecondaryValue).ToList();
                        var rand = StaticRandom.Instance.Next(possibleTargets.Count);
                        target = possibleTargets[rand];
                        
                    }

                    if (target == null || !_commandList[cmd.CommandName](hostBuff, cmd, target))
                        return;

                    if (cmd.NextCommand != null)
                    {
                        cmd = cmd.NextCommand;
                        lastTarget = target;
                        continue;
                    }
                }

                break;
            }
        }

        public static void InvokeAoEChain(NewBuff hostBuff, BuffCommandInfo cmd, Unit chainTarget)
        {
            uint iterations = 0;

            while (cmd != null && cmd.TargetType == 0)
            {
                ++iterations;

                if (iterations > 50)
                {
                    Log.Error("BuffEffectInvoker.InvokeAoEChain", "Potential infinite recursion detected on " + cmd.Entry);
                    return;
                }

                if (!_commandList[cmd.CommandName](hostBuff, cmd, chainTarget))
                    return;

                cmd = cmd.NextCommand;
            }
        }

        public static void InvokeDamageAoEChain(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit chainTarget, Unit eventInstigator)
        {
            uint iterations = 0;

            while (cmd != null && cmd.TargetType == 0)
            {
                ++iterations;

                if (iterations > 50)
                {
                    Log.Error("BuffEffectInvoker.InvokeDamageAoEChain", "Potential infinite recursion detected on " + cmd.Entry);
                    return;
                }

                if (!_damageEventCommandList[cmd.CommandName](hostBuff, cmd, damageInfo, chainTarget, eventInstigator))
                    return;

                cmd = cmd.NextCommand;
            }
        }

        public static void InvokeAbilityUseCommand(NewBuff hostBuff, BuffCommandInfo cmd, AbilityInfo abInfo)
        {
            _abUseCommandList[cmd.CommandName](hostBuff, cmd, abInfo);
        }

        public static void InvokePetCommand(NewBuff hostBuff, BuffCommandInfo cmd, Pet myPet)
        {
            uint iterations = 0;
            do
            {
                ++iterations;

                if (iterations > 50)
                {
                    Log.Error("BuffEffectInvoker.InvokePetCommand", "Potential infinite recursion detected on " + cmd.Entry);
                    return;
                }
                _petEventCommandList[cmd.CommandName](hostBuff, cmd, myPet);
                cmd = cmd.NextCommand;
            } while (cmd != null);
        }

        public static void InvokeResourceCommand(NewBuff hostBuff, BuffCommandInfo cmd, byte oldVal, ref byte change)
        {
            uint iterations = 0;
            do
            {
                ++iterations;

                if (iterations > 50)
                {
                    Log.Error("BuffEffectInvoker.InvokeResourceCommand", "Potential infinite recursion detected on " + cmd.Entry);
                    return;
                }

                _resourceEventCommandList[cmd.CommandName](hostBuff, cmd, oldVal, ref change);
                cmd = cmd.NextCommand;
            } while (cmd != null);
        }

        public static void InvokeItemCommand(NewBuff hostBuff, BuffCommandInfo cmd, Item_Info itmInfo)
        {
            uint iterations = 0;
            do
            {
                ++iterations;

                if (iterations > 50)
                {
                    Log.Error("BuffEffectInvoker.InvokeItemCommand", "Potential infinite recursion detected on " + cmd.Entry);
                    return;
                }

                _itemEventCommandList[cmd.CommandName](hostBuff, cmd, itmInfo);
                cmd = cmd.NextCommand;
            } while (cmd != null);
        }

        public static void InvokeDamageEventCommand(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit lastTarget, Unit eventInstigator)
        {
            uint iterations = 0;

            while (true)
            {
                ++iterations;

                if (iterations > 50)
                {
                    Log.Error("BuffEffectInvoker.InvokeDamageEventCommand", "Potential infinite recursion detected on " + cmd.Entry);
                    return;
                }

                if (!_damageEventCommandList.ContainsKey(cmd.CommandName))
                {
                    Log.Error("BuffEffectInvoker", "Event command not found for entry " + hostBuff.Entry + ": " + cmd.CommandName);
                    return;
                }

                if (cmd.EffectRadius > 0)
                {
                    List<Unit> aoeTargets = cmd.AoESource == CommandTargetTypes.EventInstigator ?
                        GetTargetsFor(hostBuff, cmd, eventInstigator)
                        : GetTargetsFor(hostBuff, cmd, cmd.AoESource > 0 ? GetTargetFor(hostBuff, cmd.AoESource) : hostBuff.Target);

                    foreach (Unit aoeTarget in aoeTargets)
                    {
                        if (!_damageEventCommandList[cmd.CommandName](hostBuff, cmd, damageInfo, aoeTarget, eventInstigator))
                            continue;

                        if (cmd.NextCommand != null && cmd.NextCommand.TargetType == 0)
                            InvokeDamageAoEChain(hostBuff, cmd.NextCommand, damageInfo, aoeTarget, eventInstigator);
                    }

                    while (cmd != null)
                    {
                        cmd = cmd.NextCommand;

                        // Made our way out of a possible AoE chain, continue executing.
                        if (cmd != null && cmd.TargetType > 0)
                            break;
                    }

                    if (cmd != null)
                        continue;
                }
                else
                {
                    Unit target = cmd.TargetType == 0 ? lastTarget : GetTargetFor(hostBuff, cmd.TargetType);

                    if (target == null || !_damageEventCommandList[cmd.CommandName](hostBuff, cmd, damageInfo, target, eventInstigator))
                        return;

                    if (cmd.NextCommand != null)
                    {
                        cmd = cmd.NextCommand;
                        lastTarget = target;
                        continue;
                    }
                }
                break;
            }
        }

        public static bool PerformCheck(NewBuff hostBuff, AbilityDamageInfo damageInfo, BuffCommandInfo cmd, Unit eventInstigator)
        {
            return _eventChecks[cmd.EventCheck](hostBuff, damageInfo, cmd.EventCheckParam, eventInstigator);
        }

        #endregion

        #region Targeting

        private static Unit GetTargetFor(NewBuff hostBuff, CommandTargetTypes targetType)
        {
            switch (targetType)
            {
                case CommandTargetTypes.Enemy:
                    return hostBuff.Caster.CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ENEMY);
                case CommandTargetTypes.CareerTarget:
                    return ((Player)hostBuff.Caster).CrrInterface.GetTargetOfInterest();
                case CommandTargetTypes.Host: return hostBuff.Target;
                case CommandTargetTypes.AllyOrSelf:
                    Unit allyTarget = hostBuff.Caster.CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ALLY);
                    if (allyTarget == null || allyTarget.IsDead || !CombatInterface.IsFriend(hostBuff.Caster, allyTarget) || (allyTarget is Creature && !(allyTarget is Pet)))
                        return hostBuff.Caster;
                    return allyTarget;
                case CommandTargetTypes.Ally: return hostBuff.Caster.CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ALLY);
                case CommandTargetTypes.Caster: return hostBuff.Caster;
                case CommandTargetTypes.AllyOrCareerTarget:
                    Unit allyPetTarget = hostBuff.Caster.CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ALLY);
                    if (allyPetTarget == null || allyPetTarget.IsDead || !CombatInterface.IsFriend(hostBuff.Caster, allyPetTarget) || (allyPetTarget is Creature && !(allyPetTarget is Pet)))
                    {
                        Player plr = hostBuff.Caster as Player;
                        if (plr != null && plr.CrrInterface.GetTargetOfInterest() != null)
                            return plr.CrrInterface.GetTargetOfInterest();
                        return allyPetTarget == hostBuff.Caster ? null : allyPetTarget;
                    }
                    return allyPetTarget == hostBuff.Caster ? null : allyPetTarget;
                default: return hostBuff.Target;
            }
        }

        /// <summary>
        /// Gets the targets within the command's radius and cone of the person who has this buff.
        /// </summary>
        private static List<Unit> GetTargetsFor(NewBuff hostBuff, BuffCommandInfo myCommand, Unit source)
        {
            List<Unit> myTargetList = new List<Unit>();

            #region Group

            if (myCommand.TargetType.HasFlag(CommandTargetTypes.Groupmates))
            {
                try
                {
                    if (myCommand.TargetType.HasFlag(CommandTargetTypes.Caster))
                        myTargetList.Add(hostBuff.Caster);
                    Group myGroup = hostBuff.Caster.GetPlayer().PriorityGroup;
                    if (myGroup == null)
                        return myTargetList;

                    foreach (Unit member in myGroup.GetUnitList((Player)hostBuff.Caster))
                    {
                        if (member == hostBuff.Caster)
                            continue;
                        if (myCommand.EffectAngle != 0)
                        {
                            if (source.CanHitWithAoE(member, myCommand.EffectAngle, myCommand.EffectRadius))
                                myTargetList.Add(member);
                        }

                        else if (member.ObjectWithinRadiusFeet(source, myCommand.EffectRadius))
                            myTargetList.Add(member);
                    }
                }
                catch
                {
                    Log.Error("BuffEffectInvoker", "Group member iteration error!");
                    myTargetList = new List<Unit>();
                }
            }

            #endregion

            else
            {
                int maxTargets = MAX_AOE_TARGETS;

                if (myCommand.MaxTargets > 0)
                    maxTargets = myCommand.MaxTargets;

                List<ulong> myRangeList = new List<ulong>(maxTargets);

                List<Object> sourceObjRanged = new List<Object>();

                int alliesFound = 0;

                lock (source.ObjectsInRange)
                    sourceObjRanged.AddRange(source.ObjectsInRange);

                Player plrCaster = hostBuff.Caster as Player;

                foreach (Object obj in sourceObjRanged)
                {
                    var curTarget = obj as Unit;

                    if (curTarget == null)
                        continue;

                    if (myCommand.TargetType.HasFlag(CommandTargetTypes.Enemy))
                    {
                        if (!CombatInterface.CanAttack(hostBuff.Caster, curTarget))
                        {
                            // If invalid target, check for ally within range of an enemy-affecting ability.
                            // This is used to determine friendly fire penalty on AoE attacks.
                            if (plrCaster != null && curTarget.Realm == hostBuff.Caster.Realm)
                            {
                                Player plrTarget = curTarget as Player;

                                // Non-players don't count
                                if (plrTarget == null)
                                    continue;

                                // Players in same group don't count
                                if (plrCaster.PriorityGroup != null && plrCaster.PriorityGroup == plrTarget.PriorityGroup)
                                    continue;

                                if (myCommand.EffectAngle != 0)
                                {
                                    if (source.CanHitWithAoE(curTarget, myCommand.EffectAngle, myCommand.EffectRadius))
                                        ++alliesFound;
                                }

                                else if (curTarget.ObjectWithinRadiusFeet(source, myCommand.EffectRadius) && source.LOSHit(curTarget))
                                    ++alliesFound;
                            }

                            continue;
                        }
                    }
                    else
                    {
                        if (curTarget.Realm != hostBuff.Caster.Realm || !(obj is Player))
                            continue;
                    }

                    if (myCommand.EffectAngle != 0)
                    {
                        if (myCommand.EffectAngle > 0)
                        {
                            if (source.CanHitWithAoE(curTarget, myCommand.EffectAngle, myCommand.EffectRadius))
                            {
                                ulong dist = source.GetDistanceSquare(curTarget);

                                for (int i = 0; i < maxTargets; ++i)
                                {
                                    if (i < myTargetList.Count && dist > myRangeList[i])
                                        continue;

                                    if (i == myTargetList.Count)
                                    {
                                        myTargetList.Add(curTarget);
                                        myRangeList.Add(dist);
                                    }

                                    else
                                    {
                                        myTargetList.Insert(i, curTarget);
                                        myRangeList.Insert(i, dist);
                                    }

                                    if (myTargetList.Count > maxTargets)
                                    {
                                        myTargetList.RemoveAt(maxTargets);
                                        myRangeList.RemoveAt(maxTargets);
                                    }

                                    break;
                                }
                            }
                        }
                        else if (!source.IsObjectInFront(curTarget, -myCommand.EffectAngle, (uint)(myCommand.EffectRadius - 10)) && source.LOSHit(curTarget))
                        {
                            ulong dist = source.GetDistanceSquare(curTarget);

                            for (int i = 0; i < maxTargets; ++i)
                            {
                                if (i < myTargetList.Count && dist > myRangeList[i])
                                    continue;

                                if (i == myTargetList.Count)
                                {
                                    myTargetList.Add(curTarget);
                                    myRangeList.Add(dist);
                                }

                                else
                                {
                                    myTargetList.Insert(i, curTarget);
                                    myRangeList.Insert(i, dist);
                                }

                                if (myTargetList.Count > maxTargets)
                                {
                                    myTargetList.RemoveAt(maxTargets);
                                    myRangeList.RemoveAt(maxTargets);
                                }

                                break;
                            }
                        }
                    }

                    else if (curTarget.ObjectWithinRadiusFeet(source, myCommand.EffectRadius) && source.LOSHit(curTarget))
                    {
                        ulong dist = source.GetDistanceSquare(curTarget);

                        for (int i = 0; i < maxTargets; ++i)
                        {
                            if (i < myTargetList.Count && dist > myRangeList[i])
                                continue;

                            if (i == myTargetList.Count)
                            {
                                myTargetList.Add(curTarget);
                                myRangeList.Add(dist);
                            }

                            else
                            {
                                myTargetList.Insert(i, curTarget);
                                myRangeList.Insert(i, dist);
                            }

                            if (myTargetList.Count > maxTargets)
                            {
                                myTargetList.RemoveAt(maxTargets);
                                myRangeList.RemoveAt(maxTargets);
                            }

                            break;
                        }
                    }
                }


                hostBuff.AoEMod = 1f - (alliesFound * 0.2f);
                if (myTargetList.Count > 6)
                    hostBuff.AoEMod += (myTargetList.Count - 6) * 0.3f;

                hostBuff.AoEMod = Point2D.Clamp(hostBuff.AoEMod, 0.2f, 5f);
            }

            return myTargetList;
        }

        #endregion

        #region Commands

        #region Damage
        private static bool DealDamage(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            if (hostBuff.BuffState == BUFF_REMOVE && (hostBuff.WasManuallyRemoved || cmd.SecondaryValue == 0))
                return false;

            if (hostBuff.LastPassTime < TCPManager.GetTimeStampMS() - 100)
            {
                hostBuff.LastPassTime = TCPManager.GetTimeStampMS();
                cmd.CommandResult = 0;
            }

            AbilityDamageInfo damageThisPass = cmd.DamageInfo.Clone(hostBuff.Caster);

            if (cmd.EffectRadius > 0 || hostBuff.WasCastFromAoE)
                damageThisPass.IsAoE = true;

            if (cmd.DamageInfo.IsHeal)
                CombatManager.HealTarget(damageThisPass, hostBuff.BuffLevel, hostBuff.Caster, target);
            else
            {
                /*
                if (cmd.EffectRadius > 0)
                {
                    if (hostBuff.AoEMod < 1f)
                        damageThisPass.DamageReduction = hostBuff.AoEMod;
                    else if (damageThisPass.StatUsed == 1)
                        damageThisPass.DamageBonus = hostBuff.AoEMod;
                }
                */
                CombatManager.InflictDamage(damageThisPass, hostBuff.BuffLevel, hostBuff.Caster, target);
            }

            if ((cmd.EffectRadius == 0 || cmd.CommandResult == 0) && hostBuff.BuffState == BUFF_START)
                hostBuff.AddBuffParameter(cmd.BuffLine, (int)(damageThisPass.Damage + damageThisPass.Mitigation));

            if (damageThisPass.ResultFromRaw)
                cmd.CommandResult += (short)((damageThisPass.Damage + damageThisPass.Mitigation/* + damageThisPass.Absorption*/) * damageThisPass.TransferFactor);
            else
                cmd.CommandResult += (short)(damageThisPass.Damage * damageThisPass.TransferFactor);

            return damageThisPass.DamageEvent == 0 || damageThisPass.DamageEvent == 9;
        }

        private static bool DealProcDamage(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            if (hostBuff.BuffState == BUFF_REMOVE && hostBuff.WasManuallyRemoved)
                return false;

            if (cmd.EventChance > 0 && StaticRandom.Instance.Next(100) > cmd.EventChance)
                return false;

            AbilityDamageInfo damageThisPass = cmd.DamageInfo.Clone(hostBuff.Caster);

            if (cmd.EffectRadius > 0 || hostBuff.WasCastFromAoE)
                damageThisPass.IsAoE = true;

            // Oil
            if (hostBuff.OptionalObject != null)
            {
                damageThisPass.SubDamageType = SubDamageTypes.Oil;
                damageThisPass.ContributoryFactor = 0.1f;
            }

            if (cmd.DamageInfo.IsHeal)
                CombatManager.ProcHealTarget(damageThisPass, hostBuff.BuffLevel, hostBuff.Caster, target);
            else CombatManager.InflictProcDamage(damageThisPass, hostBuff.BuffLevel, hostBuff.Caster, target);


            if ((cmd.EffectRadius == 0 || cmd.CommandResult == 0) && hostBuff.BuffState == BUFF_START)
                hostBuff.AddBuffParameter(cmd.BuffLine, (int)(damageThisPass.Damage + damageThisPass.Mitigation));

            cmd.CommandResult = (short)damageThisPass.Damage;

            return true;
        }

        private static bool HealHPBelow(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            AbilityDamageInfo damageThisPass = cmd.DamageInfo.Clone();

            if (hostBuff.Target.PctHealth <= cmd.EventCheckParam)
            {
                CombatManager.HealTarget(damageThisPass, hostBuff.BuffLevel, hostBuff.Caster, target);
                hostBuff.RemoveStack();
            }

            hostBuff.AddBuffParameter(cmd.BuffLine, 1);

            return true;
        }

        private static bool DamageOverTime(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:

                    if (cmd.EffectRadius > 0 || hostBuff.WasCastFromAoE)
                        cmd.DamageInfo.IsAoE = true;

                    if (cmd.DamageInfo.IsHeal)
                        CombatManager.SetHealAmount(cmd.DamageInfo, hostBuff.BuffLevel, hostBuff.Caster, target);
                    else
                        CombatManager.SetDamageAmount(cmd.DamageInfo, hostBuff.BuffLevel, hostBuff.Caster, target);

                    cmd.DamageInfo.PrecalcDamage *= hostBuff.StackLevel;
                    cmd.DamageInfo.Mitigation *= hostBuff.StackLevel;

                    cmd.CommandResult = (short)(cmd.DamageInfo.PrecalcDamage / hostBuff.BuffIntervals);

                    hostBuff.AddBuffParameter(cmd.BuffLine, cmd.CommandResult);
                    break;

                case BUFF_TICK:
                    if (cmd.DamageInfo.IsHeal)
                        CombatManager.SetHealAmount(cmd.DamageInfo, hostBuff.BuffLevel, hostBuff.Caster, target);
                    else
                        CombatManager.SetDamageAmount(cmd.DamageInfo, hostBuff.BuffLevel, hostBuff.Caster, target);

                    cmd.DamageInfo.PrecalcDamage *= hostBuff.StackLevel;
                    cmd.DamageInfo.Mitigation *= hostBuff.StackLevel;

                    cmd.DamageInfo.DamageEvent = 0;

                    if (cmd.DamageInfo.IsHeal)
                        CombatManager.PrecalculatedHealTarget(cmd.DamageInfo, hostBuff.Caster, target, hostBuff.BuffIntervals, false);
                    else
                        CombatManager.InflictPrecalculatedDamage(cmd.DamageInfo, hostBuff.Caster, target, 1f / hostBuff.BuffIntervals, false);

                    break;

                case BUFF_END:
                    if (cmd.DamageInfo.IsHeal)
                    {
                        cmd.DamageInfo.PrecalcDamage *= hostBuff.StackLevel;
                        cmd.DamageInfo.Mitigation *= hostBuff.StackLevel;

                        cmd.DamageInfo.DamageEvent = 0;
                        CombatManager.SetHealAmount(cmd.DamageInfo, hostBuff.BuffLevel, hostBuff.Caster, target);
                        if ((cmd.InvokeOn & 2) > 0)
                        {
                            CombatManager.PrecalculatedHealTarget(cmd.DamageInfo, hostBuff.Caster, target, hostBuff.BuffIntervals, true);
                            hostBuff.HasSentEnd = true;
                        }
                        else
                            CombatManager.PrecalculatedHealTarget(cmd.DamageInfo, hostBuff.Caster, target, 1, true);
                    }
                    else
                    {
                        CombatManager.SetDamageAmount(cmd.DamageInfo, hostBuff.BuffLevel, hostBuff.Caster, target);
                        cmd.DamageInfo.PrecalcDamage *= hostBuff.StackLevel;
                        cmd.DamageInfo.Mitigation *= hostBuff.StackLevel;

                        cmd.DamageInfo.DamageEvent = 0;

                        if ((cmd.InvokeOn & 2) > 0)
                        {
                            CombatManager.InflictPrecalculatedDamage(cmd.DamageInfo, hostBuff.Caster, target, 1f / hostBuff.BuffIntervals, true);
                            hostBuff.HasSentEnd = true;
                        }
                        else
                            CombatManager.InflictPrecalculatedDamage(cmd.DamageInfo, hostBuff.Caster, target, 1f, true);
                    }

                    break;

                case BUFF_REMOVE:
                    break;
            }
            return true;
        }

        private static bool IllegalZone(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {

            if (hostBuff.BuffState == BUFF_START)
            {
                hostBuff.Caster.GetPlayer().SendClientMessage("The gods are angry that you are trying to leave the world, turn back now!", ChatLogFilters.CHATLOGFILTERS_C_ORANGE_L);
            }

            if (hostBuff.BuffState == BUFF_END)
            {

                hostBuff.Caster.ReceiveDamage(hostBuff.Caster, int.MaxValue);

                PacketOut damageOut = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 24);

                damageOut.WriteUInt16(hostBuff.Caster.Oid);
                damageOut.WriteUInt16(hostBuff.Caster.Oid);
                damageOut.WriteUInt16(23584); // Terminate

                damageOut.WriteByte(0);
                damageOut.WriteByte(0); // DAMAGE EVENT
                damageOut.WriteByte(7);

                damageOut.WriteZigZag(-30000);
                damageOut.WriteByte(0);

                hostBuff.Caster.DispatchPacketUnreliable(damageOut, true, hostBuff.Caster);
            }

            return true;
        }

        private static bool InstantTickDoT(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:

                    if (cmd.EffectRadius > 0 || hostBuff.WasCastFromAoE)
                        cmd.DamageInfo.IsAoE = true;

                    CombatManager.SetDamageAmount(cmd.DamageInfo, hostBuff.BuffLevel, hostBuff.Caster, target);

                    cmd.DamageInfo.PrecalcDamage *= hostBuff.StackLevel;
                    cmd.DamageInfo.Mitigation *= hostBuff.StackLevel;

                    cmd.CommandResult = (short)(cmd.DamageInfo.PrecalcDamage / (cmd.InvokeOn == 7 ? (hostBuff.BuffIntervals + 1) : hostBuff.BuffIntervals));

                    CombatManager.InflictPrecalculatedDamage(cmd.DamageInfo, hostBuff.Caster, target, 1f / (cmd.InvokeOn == 7 ? (hostBuff.BuffIntervals + 1) : hostBuff.BuffIntervals), false);

                    hostBuff.AddBuffParameter(cmd.BuffLine, cmd.CommandResult);
                    break;

                case BUFF_TICK:
                    CombatManager.SetDamageAmount(cmd.DamageInfo, hostBuff.BuffLevel, hostBuff.Caster, target);
                    cmd.DamageInfo.PrecalcDamage *= hostBuff.StackLevel;
                    cmd.DamageInfo.Mitigation *= hostBuff.StackLevel;
                    cmd.DamageInfo.DamageEvent = 0;
                    CombatManager.InflictPrecalculatedDamage(cmd.DamageInfo, hostBuff.Caster, target, 1f / (cmd.InvokeOn == 7 ? (hostBuff.BuffIntervals + 1) : hostBuff.BuffIntervals), false);
                    break;

                case BUFF_END:
                    CombatManager.SetDamageAmount(cmd.DamageInfo, hostBuff.BuffLevel, hostBuff.Caster, target);
                    cmd.DamageInfo.PrecalcDamage *= hostBuff.StackLevel;
                    cmd.DamageInfo.Mitigation *= hostBuff.StackLevel;
                    cmd.DamageInfo.DamageEvent = 0;
                    CombatManager.InflictPrecalculatedDamage(cmd.DamageInfo, hostBuff.Caster, target, 1f / (cmd.InvokeOn == 7 ? (hostBuff.BuffIntervals + 1) : hostBuff.BuffIntervals), true);
                    hostBuff.HasSentEnd = true;
                    break;

                case BUFF_REMOVE:
                    break;
            }
            return true;
        }

        private static bool RampDamageOverTime(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    CombatManager.SetDamageAmount(cmd.DamageInfo, hostBuff.BuffLevel, hostBuff.Caster, target);

                    cmd.DamageInfo.PrecalcDamage *= hostBuff.StackLevel;
                    cmd.DamageInfo.Mitigation *= hostBuff.StackLevel;

                    cmd.CommandResult = hostBuff.BuffIntervals;

                    hostBuff.AddBuffParameter(cmd.BuffLine, cmd.CommandResult);
                    break;

                case BUFF_TICK:
                    CombatManager.SetDamageAmount(cmd.DamageInfo, hostBuff.BuffLevel, hostBuff.Caster, target);
                    cmd.DamageInfo.PrecalcDamage *= hostBuff.StackLevel;
                    cmd.DamageInfo.Mitigation *= hostBuff.StackLevel;
                    cmd.DamageInfo.DamageEvent = 0;
                    CombatManager.InflictPrecalculatedDamage(cmd.DamageInfo, hostBuff.Caster, target, (float)(System.Math.Pow(0.5d, cmd.CommandResult)), false);
                    cmd.CommandResult--;
                    break;

                case BUFF_END:
                    CombatManager.SetDamageAmount(cmd.DamageInfo, hostBuff.BuffLevel, hostBuff.Caster, target);
                    cmd.DamageInfo.PrecalcDamage *= hostBuff.StackLevel;
                    cmd.DamageInfo.Mitigation *= hostBuff.StackLevel;
                    cmd.DamageInfo.DamageEvent = 0;
                    CombatManager.InflictPrecalculatedDamage(cmd.DamageInfo, hostBuff.Caster, target, (float)(System.Math.Pow(0.5d, cmd.CommandResult)), true);
                    hostBuff.HasSentEnd = true;
                    break;

                case BUFF_REMOVE:
                    break;
            }
            return true;
        }

        private static bool DamageWhileMoving(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            if (hostBuff.BuffState == BUFF_START)
            {
                hostBuff.AddBuffParameter(cmd.BuffLine, 1);
                target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.WasAttacked, cmd.DamageInfo, hostBuff.Caster);
                return true;
            }

            if (!target.IsMoving)
                return false;

            AbilityDamageInfo damageThisPass = cmd.DamageInfo.Clone(hostBuff.Caster);

            CombatManager.InflictProcDamage(damageThisPass, hostBuff.BuffLevel, hostBuff.Caster, target);

            return true;
        }

        private static bool Slay(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            if (cmd.CommandResult == 1)
                return false;

            cmd.CommandResult = 1;
            target.ReceiveDamage(target, int.MaxValue);

            return true;
        }

        private static bool DismissPet(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            if (cmd.CommandResult == 1)
                return false;

            cmd.CommandResult = 1;
            if (!target.IsDead)
                ((Pet)target).Dismiss(hostBuff.Caster, null);

            return true;
        }

        private static bool StealLife(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            if ((hostBuff.BuffState & cmd.InvokeOn) == 0)
                return true;

            ushort toHeal;

            if (cmd.LastCommand.DamageInfo?.PrecalcDamage > 0)
                toHeal = (ushort)(cmd.LastCommand.DamageInfo.Damage * cmd.LastCommand.DamageInfo.TransferFactor * (cmd.PrimaryValue / 100f));
            else toHeal = (ushort)(cmd.LastCommand.CommandResult * (cmd.PrimaryValue / 100f));

            int pointsHealed = CombatManager.RawLifeSteal(toHeal, cmd.DamageInfo?.DisplayEntry ?? cmd.Entry, (byte)cmd.SecondaryValue, hostBuff.Caster, target);

            if (pointsHealed == -1)
                return false;

            cmd.CommandResult = cmd.LastCommand.CommandResult;

            return true;
        }

        private static bool Resurrection(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            // We are handling terror here


            Player t = hostBuff.Target as Player;
            Player c = hostBuff.Caster as Player;

            if (t.BuffInterface.GetBuff(5968, t) != null)
            {
                t.SendClientMessage("Resurrection failed - you are too terrified!", ChatLogFilters.CHATLOGFILTERS_ABILITY_ERROR);
                c.SendClientMessage("Resurrection failed - " + t.Name + " is too terrified!", ChatLogFilters.CHATLOGFILTERS_ABILITY_ERROR);
                hostBuff.Caster.AbtInterface.Cancel(true);
                return true;
            }

            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    if (!(hostBuff.Target is Player))
                    {
                        hostBuff.BuffHasExpired = true;
                        return true;
                    }
                    hostBuff.OptionalObject = new Point3D(hostBuff.Caster.WorldPosition);
                    ((Player)hostBuff.Target).SendDialog(Dialog.ResurrectionOffer, hostBuff.Caster.Oid, 60);
                    hostBuff.AddBuffParameter(cmd.BuffLine, -1);



                    break;
            }
            return true;
        }

        // Awful, rotten hack for Slayer/Choppa berserk-regen
        private static bool BerserkerRegen(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    hostBuff.AddBuffParameter(cmd.BuffLine, 1); break;
                case BUFF_TICK:
                    byte curRes = ((Player)hostBuff.Caster).CrrInterface.GetCurrentResourceLevel(0);

                    if (curRes == 0 || curRes > cmd.PrimaryValue)
                        return false;

                    if (cmd.CommandResult != curRes)
                    {
                        cmd.DamageInfo = AbilityMgr.GetExtraDamageFor(hostBuff.Entry, 0, (byte)(curRes - 1));
                        cmd.CommandResult = curRes;
                    }
                    CombatManager.ProcHealTarget(cmd.DamageInfo, hostBuff.BuffLevel, hostBuff.Caster, target);
                    break;
            }

            return true;
        }

        private static bool NapalmGrenade(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            if (hostBuff.BuffState == BUFF_REMOVE && hostBuff.WasManuallyRemoved)
                return false;

            if (hostBuff.LastPassTime < TCPManager.GetTimeStampMS() - 100)
            {
                hostBuff.LastPassTime = TCPManager.GetTimeStampMS();
                cmd.CommandResult = 0;
            }

            AbilityDamageInfo damageThisPass = cmd.DamageInfo.Clone(hostBuff.Caster);

            if (hostBuff.RemainingTimeMs < 10000)
            {
                damageThisPass.MinDamage *= 3;
                damageThisPass.MinDamage *= 3;
            }
            else if (hostBuff.RemainingTimeMs < 20000)
            {
                damageThisPass.MinDamage *= 2;
                damageThisPass.MinDamage *= 2;
            }

            damageThisPass.IsAoE = true;

            CombatManager.InflictDamage(damageThisPass, hostBuff.BuffLevel, hostBuff.Caster, target);

            if (hostBuff.BuffState == BUFF_START)
                hostBuff.AddBuffParameter(cmd.BuffLine, 1);

            cmd.CommandResult += (short)damageThisPass.Damage;

            return damageThisPass.DamageEvent == 0 || damageThisPass.DamageEvent == 9;
        }

        private static readonly int[] BrimstoneDamageMultipliers = { 3, 6, 10, 15, 20, 30, 45, 60, 90 };

        private static bool BrimstoneBauble(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            cmd.DamageInfo.PrecalcDamage = cmd.DamageInfo.GetDamageForLevel(hostBuff.BuffLevel) * BrimstoneDamageMultipliers[cmd.CommandResult / 3];

            CombatManager.InflictPrecalculatedDamage(cmd.DamageInfo, hostBuff.Caster, hostBuff.Caster, 1f, false);

            hostBuff.Caster.CbtInterface.RefreshCombatTimer();

            if (cmd.CommandResult < 24)
                ++cmd.CommandResult;

            return true;
        }

        #endregion

        #region Siege

        /// <summary>
        /// Strikes all targets within the specified command radius. 
        /// Inflicts damage to each target which is scaled by the number of additional targets present within a certain distance.
        /// </summary>
        private static bool ArtilleryStrike(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            if (hostBuff.OptionalObject is Siege || hostBuff.OptionalObject is KeepCreature)
                HandleArtillery(hostBuff, cmd, target);
            else
                HandleAnticampSiege(hostBuff, cmd, target);

            return true;
        }

        private static void HandleArtillery(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            List<Unit> myTargetList = new List<Unit>();
            int relevantPlayers = 0;

            Creature weapon = (Creature)hostBuff.OptionalObject;
            Siege siege = weapon as Siege;

            Campaign front = weapon.Region?.Campaign;

            if (front == null)
                return;

            foreach (Object obj in target.ObjectsInRange)
            {
                var curTarget = obj as Unit;

                if (curTarget == null || !CombatInterface.CanAttack(hostBuff.Caster, curTarget) || !target.WorldPosition.IsWithinRadiusFeet(curTarget.WorldPosition, 60))
                    continue;

                ++relevantPlayers;

                if (!target.CanHitWithAoE(curTarget, 360, cmd.EffectRadius))
                    continue;

                myTargetList.Add(curTarget);
            }

            relevantPlayers = Math.Min(relevantPlayers, 50);

            float artilleryScale = front.GetArtilleryDamageScale(weapon.Realm);

            int desiredStackLevel = (int)(Point2D.Clamp(relevantPlayers - 20, 0, 30) * 0.1f * artilleryScale);

            artilleryScale *= 1f + (float)Math.Pow(relevantPlayers / 30f, 3);

            foreach (Unit foe in myTargetList)
            {
                AbilityDamageInfo damageThisPass = cmd.DamageInfo.Clone();

                // Damage inflicted by an artillery weapon increases for every player within 60ft of the blast zone.
                // Maximum of 50 players, +400% damage, exponential.
                damageThisPass.MinDamage = (ushort)(damageThisPass.MinDamage * artilleryScale);
                damageThisPass.MinDamage = (ushort)(damageThisPass.MinDamage * artilleryScale);

                weapon.ModifyDamageOut(damageThisPass);

                CombatManager.InflictDamage(damageThisPass, target.Level, hostBuff.Caster, foe);

                if (desiredStackLevel == 0)
                    continue;

                BuffInfo info = AbilityMgr.GetBuffInfo((ushort)cmd.SecondaryValue);
                info.InitialStacks = desiredStackLevel;
                foe.BuffInterface.QueueBuff(new BuffQueueInfo(hostBuff.Caster, target.Level, info));
            }
        }

        private static void HandleAnticampSiege(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            List<Unit> myTargetList = new List<Unit>();
            List<byte> numStrikes = new List<byte>();

            Creature siege = (Creature)hostBuff.OptionalObject;

            foreach (Object obj in target.ObjectsInRange)
            {
                var curTarget = obj as Unit;

                if (curTarget == null || !CombatInterface.CanAttack(hostBuff.Caster, curTarget) || !target.CanHitWithAoE(curTarget, 360, cmd.EffectRadius))
                    continue;

                if (!(curTarget is Pet))
                {
                    byte curStrikes = 0;

                    for (int i = 0; i < myTargetList.Count; ++i)
                    {
                        if (!curTarget.IsInCastRange(myTargetList[i], (uint)(cmd.PrimaryValue - 10)))
                            continue;

                        ++curStrikes;
                        ++numStrikes[i];
                    }
                    myTargetList.Add(curTarget);
                    numStrikes.Add(curStrikes);
                }
            }

            for (int i = 0; i < myTargetList.Count; i++)
            {
                AbilityDamageInfo damageThisPass = cmd.DamageInfo.Clone();

                // Maximum +300% damage for hitting same person 6 times with added splash
                damageThisPass.MinDamage = (ushort)(damageThisPass.MinDamage * 3 * (1 + Math.Min(numStrikes[i], (byte)6) * 0.5f));
                damageThisPass.MinDamage = (ushort)(damageThisPass.MinDamage * 3 * (1 + Math.Min(numStrikes[i], (byte)6) * 0.5f));

                siege.ModifyDamageOut(damageThisPass);

                CombatManager.InflictDamage(damageThisPass, target.Level, hostBuff.Caster, myTargetList[i]);
            }
        }


        #endregion

        #region Resources

        private static bool SetCareerRes(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            ((Player)hostBuff.Caster).CrrInterface.SetResource((byte)cmd.PrimaryValue, false);
            return true;
        }

        // Value 2 blocks the event from being raised with buffs listening for career resource modification.
        // Value 3 allows the initial tick to grant resource value if it's not the only one.
        private static bool ModifyCareerRes(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    hostBuff.AddBuffParameter(cmd.BuffLine, cmd.PrimaryValue);
                    if (cmd.InvokeOn == BUFF_START || cmd.TertiaryValue == 1)
                        goto case BUFF_TICK;
                    break;
                case BUFF_TICK:
                case BUFF_END:
                    Player player = (Player)hostBuff.Caster;
#warning EX mode - link offhand regen to battlefield conditions
                    if (cmd.Entry > 14000 && player.CrrInterface.ExperimentalMode)
                        player.CrrInterface.AddResource((byte)cmd.PrimaryValue, cmd.SecondaryValue == 1);
                    else if (cmd.PrimaryValue > 0)
                        player.CrrInterface.AddResource((byte)cmd.PrimaryValue, cmd.SecondaryValue == 1);
                    else if (!player.CrrInterface.ConsumeResource((byte)-cmd.PrimaryValue, cmd.SecondaryValue == 1) && cmd.ConsumesStack)
                    {
                        hostBuff.RemoveStack();
                        return false;
                    }
                    break;
            }
            return true;
        }

        private static bool SetAp(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            Player plrTarget = target as Player;

            if (plrTarget == null)
                return false;

            plrTarget.ActionPoints = (ushort)cmd.PrimaryValue;

            return true;
        }

        private static bool ModifyAp(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            Player plrTarget = target as Player;

            if (plrTarget == null)
            {
                hostBuff.AddBuffParameter(cmd.BuffLine, cmd.PrimaryValue);
                return false;
            }

            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    if (cmd.InvokeOn == 1)
                        plrTarget.ModifyActionPoints((short)cmd.PrimaryValue);
                    else
                        hostBuff.AddBuffParameter(cmd.BuffLine, cmd.PrimaryValue); break;
                case BUFF_TICK:
                    goto case 4;
#warning Intended to cancel self-buffs, non-channeling, which are consuming action points on tick. This needs to be split to another command.
                case BUFF_END:
                    if (plrTarget.ModifyActionPoints((short)cmd.PrimaryValue) == 0 && cmd.PrimaryValue < 0 && hostBuff.Caster == hostBuff.Target)
                        hostBuff.RemoveStack();
                    break;
            }


            return true;
        }

        private static bool StealAp(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            if (target is Player plrTarget)
            {
                switch (hostBuff.BuffState)
                {
                    case BUFF_START:
                        hostBuff.AddBuffParameter(cmd.BuffLine, cmd.PrimaryValue);
                        break;

                    case BUFF_TICK:
                        goto case 4;

                    case BUFF_END:
                        if (plrTarget != null)
                        {
                            int deltaAp = plrTarget.ModifyActionPoints((short)-cmd.PrimaryValue);
                            if (hostBuff.Caster is Player plrCaster)
                                plrCaster.ModifyActionPoints(-deltaAp);
                        }
                        else
                            if (hostBuff.Caster is Player plrCaster)
                            plrCaster.ModifyActionPoints((short)cmd.PrimaryValue);
                        break;
                }
            }

            return true;
        }

        private static bool ModifyMorale(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            Player plrTarget = target as Player;

            if (plrTarget == null)
                return false;

            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    hostBuff.AddBuffParameter(cmd.BuffLine, cmd.PrimaryValue); break;
                case BUFF_TICK:
                    goto case 4;
                case BUFF_END:
                    if (cmd.PrimaryValue < 0)
                        plrTarget.ConsumeMorale(cmd.PrimaryValue);
                    else
                        plrTarget.AddMorale(cmd.PrimaryValue);
                    break;
            }

            return true;
        }

        private static bool Panic(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            Player plr = (Player)hostBuff.Target;

            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    plr.ChangePanicState(true);
                    break;
                case BUFF_END:
                    plr.ChangePanicState(false);
                    break;
                case BUFF_REMOVE:
                    goto case BUFF_END;
            }

            return true;
        }

        #endregion

        #region Stats

        /// <summary>
        /// <para>Modifies player stats.</para>
        /// <para>PrimaryValue is the stat to modify.</para>
        /// <para>SecondaryValue is the value to modify the stat by.</para>
        /// <para>TertiaryValue, if specified, causes the applied value to be Lerp(SecondaryValue, TertiaryValue, PlayerLevel).</para>
        /// </summary>
        private static bool ModifyStat(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    if (cmd.TertiaryValue != 0)
                        cmd.CommandResult = (short)Point2D.Lerp(cmd.SecondaryValue, cmd.TertiaryValue, (hostBuff.BuffLevel - 1) / 39.0f);
                    else cmd.CommandResult = (short)cmd.SecondaryValue;

                    cmd.CommandResult *= hostBuff.StackLevel;

                    if (cmd.CommandResult < 0)
                        target.StsInterface.AddReducedStat((Stats)cmd.PrimaryValue, (ushort)-cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    else
                        target.StsInterface.AddBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));

                    hostBuff.AddBuffParameter(cmd.BuffLine, cmd.CommandResult);
                    break;
                case BUFF_TICK:
                    Log.Error("BuffEffectInvoker", "ModifyStat from " + hostBuff.Entry + " should never tick!");
                    break;
                case BUFF_END:
                case BUFF_REMOVE:
                    if (cmd.CommandResult < 0)
                        target.StsInterface.RemoveReducedStat((Stats)cmd.PrimaryValue, (ushort)-cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    else target.StsInterface.RemoveBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    break;
            }

            return true;
        }

        private static bool ModifyStatNoStack(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    if (cmd.TertiaryValue != 0)
                        cmd.CommandResult = (short)(cmd.SecondaryValue + (cmd.TertiaryValue - cmd.SecondaryValue) * ((hostBuff.BuffLevel - 1) / 39.0f));
                    else cmd.CommandResult = (short)cmd.SecondaryValue;

                    if (cmd.CommandResult < 0)
                        target.StsInterface.AddReducedStat((Stats)cmd.PrimaryValue, (ushort)-cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    else
                        target.StsInterface.AddBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));

                    hostBuff.AddBuffParameter(cmd.BuffLine, cmd.CommandResult);
                    break;
                case BUFF_TICK:
                    Log.Error("BuffEffectInvoker", "ModifyStatNoStack from " + hostBuff.Entry + " should never tick!");
                    break;
                case BUFF_END:
                    if (cmd.CommandResult < 0)
                        target.StsInterface.RemoveReducedStat((Stats)cmd.PrimaryValue, (ushort)-cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    else target.StsInterface.RemoveBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    break;
                case BUFF_REMOVE:
                    goto case 4;
            }

            return true;
        }

        private static bool ModifyStatByResourceLevel(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    byte resLv = ((Player)hostBuff.Caster).CrrInterface.GetCurrentResourceLevel((byte)cmd.EventCheckParam);

                    if (resLv > 0)
                    {
                        if (cmd.TertiaryValue != 0)
                            cmd.CommandResult = (short)(cmd.SecondaryValue + (cmd.TertiaryValue - cmd.SecondaryValue) * ((hostBuff.BuffLevel - 1) / 39.0f));
                        else cmd.CommandResult = (short)cmd.SecondaryValue;

                        cmd.CommandResult *= resLv;

                        if (cmd.CommandResult < 0)
                            target.StsInterface.AddReducedStat((Stats)cmd.PrimaryValue, (ushort)-cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                        else
                            target.StsInterface.AddBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    }

                    for (byte i = 1; i <= ((Player)hostBuff.Caster).CrrInterface.GetResourceLevelMax((byte)cmd.EventCheckParam); ++i)
                        hostBuff.AddBuffParameter(i, cmd.SecondaryValue * i);
                    break;
                case BUFF_END:
                    if (cmd.CommandResult < 0)
                        target.StsInterface.RemoveReducedStat((Stats)cmd.PrimaryValue, (ushort)-cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    else if (cmd.CommandResult > 0)
                        target.StsInterface.RemoveBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    break;
                case BUFF_REMOVE:
                    goto case 4;
            }

            return true;
        }

        private static bool ModifyPercentageStatByResourceLevel(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    byte resLv = ((Player)hostBuff.Caster).CrrInterface.GetCurrentResourceLevel((byte)cmd.EventCheckParam);

                    if (resLv > 0)
                    {
                        cmd.CommandResult = (short)(cmd.SecondaryValue * hostBuff.StackLevel);
                        cmd.CommandResult *= resLv;

                        if (cmd.CommandResult < 0)
                            target.StsInterface.AddReducedMultiplier((Stats)cmd.PrimaryValue, (100 + cmd.CommandResult) * 0.01f, hostBuff.GetBuffClass(cmd));
                        else
                            target.StsInterface.AddBonusMultiplier((Stats)cmd.PrimaryValue, cmd.CommandResult * 0.01f, hostBuff.GetBuffClass(cmd));
                    }

                    // Thanks, Mythic
                    if (cmd.CommandResult > 0)
                        hostBuff.AddBuffParameter(cmd.BuffLine, cmd.CommandResult);
                    else
                        hostBuff.AddBuffParameter(cmd.BuffLine, 100 + cmd.CommandResult);
                    break;
                case BUFF_END:
                    if (cmd.CommandResult < 0)
                        target.StsInterface.RemoveReducedMultiplier((Stats)cmd.PrimaryValue, (100 + cmd.CommandResult) * 0.01f, hostBuff.GetBuffClass(cmd));
                    else target.StsInterface.RemoveBonusMultiplier((Stats)cmd.PrimaryValue, cmd.CommandResult * 0.01f, hostBuff.GetBuffClass(cmd));
                    break;
                case BUFF_REMOVE:
                    goto case 4;
            }

            return true;
        }

        private static bool ModifyStatIfHasResource(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    if (((Player)hostBuff.Caster).CrrInterface.CareerResource > cmd.EventCheckParam)
                    {
                        if (cmd.TertiaryValue != 0)
                            cmd.CommandResult = (short)(cmd.SecondaryValue + (cmd.TertiaryValue - cmd.SecondaryValue) * ((hostBuff.BuffLevel - 1) / 39.0f));
                        else cmd.CommandResult = (short)cmd.SecondaryValue;

                        if (cmd.CommandResult < 0)
                            target.StsInterface.AddReducedStat((Stats)cmd.PrimaryValue, (ushort)-cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                        else
                            target.StsInterface.AddBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    }

                    hostBuff.AddBuffParameter(cmd.BuffLine, cmd.CommandResult);
                    break;
                case BUFF_END:
                    if (cmd.CommandResult < 0)
                        target.StsInterface.RemoveReducedStat((Stats)cmd.PrimaryValue, (ushort)-cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    else target.StsInterface.RemoveBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));

                    cmd.CommandResult = 0;

                    break;
                case BUFF_REMOVE: goto case 4;
            }

            return true;
        }

        private static bool ModifyStatByNearbyFoes(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    if (cmd.TertiaryValue != 0)
                        cmd.CommandResult = (short)(cmd.SecondaryValue + (cmd.TertiaryValue - cmd.SecondaryValue) * ((hostBuff.BuffLevel - 1) / 39.0f));
                    else cmd.CommandResult = (short)cmd.SecondaryValue;

                    // Get foes within range
                    Unit source = hostBuff.Caster;
                    byte count = 0;

                    foreach (Object obj in source.ObjectsInRange)
                    {
                        if (!obj.IsUnit())
                            continue;

                        Unit curTarget = obj.GetUnit();

                        if (!CombatInterface.CanAttack(source, curTarget))
                            continue;

                        if (curTarget.ObjectWithinRadiusFeet(source, 20))
                            count++;

                        if (count == cmd.MaxTargets)
                            break;
                    }

                    cmd.CommandResult *= count;

                    if (cmd.CommandResult < 0)
                        target.StsInterface.AddReducedStat((Stats)cmd.PrimaryValue, (ushort)-cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    else
                        target.StsInterface.AddBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));

                    hostBuff.AddBuffParameter(cmd.BuffLine, cmd.CommandResult);
                    break;
                case BUFF_TICK:
                    Log.Error("BuffEffectInvoker", "ModifyStat should never tick!");
                    break;
                case BUFF_END:
                    if (cmd.CommandResult < 0)
                        target.StsInterface.RemoveReducedStat((Stats)cmd.PrimaryValue, (ushort)-cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    else target.StsInterface.RemoveBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    break;
                case BUFF_REMOVE:
                    goto case 4;
            }

            return true;
        }


        private static bool ModifyStatByNearbyAllies(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    if (cmd.TertiaryValue != 0)
                        cmd.CommandResult = (short)(cmd.SecondaryValue + (cmd.TertiaryValue - cmd.SecondaryValue) * ((hostBuff.BuffLevel - 1) / 39.0f));
                    else cmd.CommandResult = (short)cmd.SecondaryValue;

                    // Get foes within range
                    Unit source = hostBuff.Caster;
                    byte count = 0;

                    var alliesInRange = source.PlayersInRange.Where(x => x.Realm == source.Realm);

                    foreach (var player in alliesInRange)
                    {
                        if (source.ObjectWithinRadiusFeet(player, 100))
                            count++;
                    }

                    if (count > cmd.MaxTargets)
                        count = cmd.MaxTargets;

                    cmd.CommandResult *= count;


                    if (cmd.CommandResult < 0)
                        target.StsInterface.AddReducedStat((Stats)cmd.PrimaryValue, (ushort)-cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    else
                        target.StsInterface.AddBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));

                    hostBuff.AddBuffParameter(cmd.BuffLine, cmd.CommandResult);
                    break;
                case BUFF_TICK:
                    Log.Error("BuffEffectInvoker", "ModifyStat should never tick!");
                    break;
                case BUFF_END:
                    if (cmd.CommandResult < 0)
                        target.StsInterface.RemoveReducedStat((Stats)cmd.PrimaryValue, (ushort)-cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    else target.StsInterface.RemoveBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    break;
                case BUFF_REMOVE:
                    goto case 4;
            }

            return true;
        }

        private static bool ModifyPercentageStatByNearbyAllies(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:

                    // Get foes within range
                    Unit source = hostBuff.Caster;
                    byte count = 0;

                    var alliesInRange = source.PlayersInRange.Where(x => x.Realm == source.Realm);

                    foreach (var player in alliesInRange)
                    {
                        if (player.IsAFK)
                            continue;
                        if (player.IsDead)
                            continue;

                        if (source.ObjectWithinRadiusFeet(player, 100))
                            count++;
                    }

                    if (count > cmd.MaxTargets)
                        count = cmd.MaxTargets;


                    cmd.CommandResult = count;

                    target.StsInterface.AddReducedMultiplier((Stats)cmd.PrimaryValue, (100 + cmd.CommandResult * cmd.SecondaryValue) * 0.01f, hostBuff.GetBuffClass(cmd));

                    hostBuff.AddBuffParameter(cmd.BuffLine, cmd.CommandResult);
                    break;
                case BUFF_TICK:
                    Log.Error("BuffEffectInvoker", "ModifyStat should never tick!");
                    break;
                case BUFF_END:
                    target.StsInterface.RemoveReducedMultiplier((Stats)cmd.PrimaryValue, (100 + cmd.CommandResult * cmd.SecondaryValue) * 0.01f, hostBuff.GetBuffClass(cmd));
                    break;
                case BUFF_REMOVE:
                    goto case 4;
            }

            return true;
        }

        private static bool AddCasterStat(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    Stats sourceStat = cmd.TertiaryValue > 0 ? (Stats)cmd.TertiaryValue : (Stats)cmd.PrimaryValue;
                    cmd.CommandResult = (short)(hostBuff.Caster.StsInterface.GetCombinationItemStat(sourceStat) * cmd.SecondaryValue * hostBuff.StackLevel * 0.01f);
                    target.StsInterface.AddBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    hostBuff.AddBuffParameter(cmd.BuffLine, cmd.CommandResult);
                    break;
                case BUFF_END:
                    target.StsInterface.RemoveBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    break;
                case BUFF_REMOVE:
                    goto case 4;
            }
            return true;
        }

        private static bool ModifyPercentageStat(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    cmd.CommandResult = (short)(cmd.SecondaryValue * hostBuff.StackLevel);

                    if (cmd.CommandResult < 0)
                        target.StsInterface.AddReducedMultiplier((Stats)cmd.PrimaryValue, (100 + cmd.CommandResult) * 0.01f, hostBuff.GetBuffClass(cmd));
                    else
                        target.StsInterface.AddBonusMultiplier((Stats)cmd.PrimaryValue, cmd.CommandResult * 0.01f, hostBuff.GetBuffClass(cmd));

                    // Thanks, Mythic
                    if (cmd.CommandResult > 0 || cmd.TertiaryValue == 0)
                        hostBuff.AddBuffParameter(cmd.BuffLine, cmd.CommandResult);
                    else
                        hostBuff.AddBuffParameter(cmd.BuffLine, 100 + cmd.CommandResult);
                    break;
                case BUFF_END:
                    if (cmd.CommandResult < 0)
                        target.StsInterface.RemoveReducedMultiplier((Stats)cmd.PrimaryValue, (100 + cmd.CommandResult) * 0.01f, hostBuff.GetBuffClass(cmd));
                    else target.StsInterface.RemoveBonusMultiplier((Stats)cmd.PrimaryValue, cmd.CommandResult * 0.01f, hostBuff.GetBuffClass(cmd));
                    break;
                case BUFF_REMOVE:
                    goto case 4;
                default:
                    Log.Error("BuffEffectInvoker", "ModifyPercentageStat should never tick!");
                    break;
            }

            return true;
        }

        private static bool ModifyPercentageStatNoStack(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    cmd.CommandResult = (short)(cmd.SecondaryValue);
                    if (cmd.CommandResult < 0)
                        target.StsInterface.AddReducedMultiplier((Stats)cmd.PrimaryValue, (100 + cmd.CommandResult) * 0.01f, hostBuff.GetBuffClass(cmd));
                    else
                        target.StsInterface.AddBonusMultiplier((Stats)cmd.PrimaryValue, cmd.CommandResult * 0.01f, hostBuff.GetBuffClass(cmd));

                    // Thanks, Mythic
                    if (cmd.CommandResult > 0 || cmd.TertiaryValue == 0)
                        hostBuff.AddBuffParameter(cmd.BuffLine, cmd.CommandResult);
                    else
                        hostBuff.AddBuffParameter(cmd.BuffLine, 100 + cmd.CommandResult);
                    break;
                case BUFF_END:
                    if (cmd.CommandResult < 0)
                        target.StsInterface.RemoveReducedMultiplier((Stats)cmd.PrimaryValue, (100 + cmd.CommandResult) * 0.01f, hostBuff.GetBuffClass(cmd));
                    else target.StsInterface.RemoveBonusMultiplier((Stats)cmd.PrimaryValue, cmd.CommandResult * 0.01f, hostBuff.GetBuffClass(cmd));
                    break;
                case BUFF_REMOVE:
                    goto case 4;
                default:
                    Log.Error("BuffEffectInvoker", "ModifyPercentageStat should never tick!");
                    break;
            }

            return true;
        }

        // FIXME.
        private static bool ModifyStatRealmwise(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    if (cmd.TertiaryValue != 0)
                        cmd.CommandResult = (short)(cmd.SecondaryValue + (cmd.TertiaryValue - cmd.SecondaryValue) * ((hostBuff.BuffLevel - 1) / 39.0f));
                    else cmd.CommandResult = (short)cmd.SecondaryValue;

                    cmd.CommandResult *= hostBuff.StackLevel;

                    if (hostBuff.Caster.Realm != target.Realm)
                    {
                        target.StsInterface.AddReducedStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                        hostBuff.AddBuffParameter(cmd.BuffLine, -cmd.CommandResult);
                    }
                    else
                    {
                        target.StsInterface.AddBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                        hostBuff.AddBuffParameter((byte)(cmd.BuffLine + 1), cmd.CommandResult);
                    }


                    break;
                case BUFF_TICK:
                    Log.Error("BuffEffectInvoker", "ModifyStat should never tick!");
                    break;
                case BUFF_END:
                    if (hostBuff.Caster.Realm != target.Realm)
                        target.StsInterface.RemoveReducedStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    else target.StsInterface.RemoveBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    break;
                case BUFF_REMOVE:
                    goto case 4;
            }

            return true;
        }

        private static bool GiftItemStatTo(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    hostBuff.AddBuffParameter(cmd.BuffLine, target.StsInterface.GiftItemStatTo((Stats)cmd.PrimaryValue, (Stats)cmd.SecondaryValue)); return true;
                case BUFF_END:
                    target.StsInterface.RemoveItemStatGift((Stats)cmd.PrimaryValue); return true;
                case BUFF_REMOVE:
                    goto case 4;
                default:
                    return true;
            }
        }

        private static bool AddItemStatMultiplier(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    hostBuff.AddBuffParameter(cmd.BuffLine, target.StsInterface.SetItemStatMultiplier((Stats)cmd.PrimaryValue, cmd.SecondaryValue)); return true;
                case BUFF_END:
                    target.StsInterface.SetItemStatMultiplier((Stats)cmd.PrimaryValue, 1); return true;
                case BUFF_REMOVE:
                    goto case 4;
                default:
                    return true;
            }
        }

        private static bool ModifySpeed(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    if (target.StsInterface != null)
                    {
                        if (cmd.PrimaryValue < 0 && target.ImmuneToCC((int)CrowdControlTypes.Snare, hostBuff.Caster, hostBuff.Entry))
                        {
                            cmd.CommandResult = 1;
                            return true;
                        }

                        int speedMod = cmd.PrimaryValue;
                        if (cmd.SecondaryValue != 0)
                            speedMod = cmd.PrimaryValue + StaticRandom.Instance.Next(cmd.SecondaryValue - cmd.PrimaryValue);

                        speedMod *= hostBuff.StackLevel;

                        if (speedMod < 0)
                            hostBuff.CrowdControl = (byte)(hostBuff.CrowdControl | (int)CrowdControlTypes.Snare);

                        target.StsInterface.AddVelocityModifier(hostBuff, 1f + speedMod * 0.01f);

                        hostBuff.AddBuffParameter(cmd.BuffLine, speedMod);
                    }
                    break;
                case BUFF_TICK:
                    Log.Error("BuffEffectInvoker", "ModifySpeed from Entry " + hostBuff.Entry + " should never tick!");
                    break;
                case BUFF_END:
                case BUFF_REMOVE:
                    if (target.StsInterface != null && cmd.CommandResult == 0)
                        target.StsInterface.RemoveVelocityModifier(hostBuff);
                    break;
            }
            return true;
        }

        #endregion

        #region CC
        /// <summary>
        /// <para>Applies any type of hard CC to the target.</para>
        /// <para>PrimaryValue: The type of CC to apply.</para>
        /// <para>SecondaryValue: The Cast_Player_Effect effect ID to show knockdown/stun on the client.</para>
        /// </summary>
        private static bool ApplyCc(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    hostBuff.CrowdControl = (byte)(hostBuff.CrowdControl | Math.Min(cmd.PrimaryValue, 16));
                    if (target.StsInterface != null)
                    {
                        // Hurried Restore / Dat Makes Me Dizzy 
                        //if (hostBuff.Caster != hostBuff.Target)
                        //{
                        if (target.ImmuneToCC(hostBuff.CrowdControl, hostBuff.Caster, hostBuff.Entry))
                        {
                            cmd.CommandResult = 1;
                            return false;
                        }

                        BuffInfo immunityInfo = AbilityMgr.GetBuffInfo((ushort)GameBuffs.Unstoppable);
                        if (cmd.TertiaryValue > 0)
                            immunityInfo.Duration = (ushort)cmd.TertiaryValue;
                        if (cmd.Entry != 4998) //Crown of fire stun
                        {
                            immunityInfo.Duration = (cmd.PrimaryValue == 32 ? (ushort)30 : (ushort)(hostBuff.Duration * 10));
                            target.BuffInterface.InsertUnstoppable(new BuffQueueInfo(target, target.EffectiveLevel, immunityInfo));
                        }
                        //}

                        target.CrowdControlType = (byte)cmd.PrimaryValue;
                        target.AbtInterface.OnPlayerCCed();

                        // 
                        if ((cmd.PrimaryValue == 16) && (cmd.SecondaryValue == 1))
                        {
                            (hostBuff.Caster as Player)?.UpdatePlayerBountyEvent((byte)ContributionDefinitions.KNOCK_DOWN);
                        }

                        #region Client Effect
                        if (cmd.PrimaryValue >= 16)
                        {
                            target.StsInterface.AddVelocityModifier(hostBuff, 0);

                            target.DispatchUpdateState((byte)StateOpcode.Stunned, 1);

                            PacketOut Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);
                            Out.WriteUInt16(hostBuff.Caster.Oid);
                            Out.WriteUInt16(target.Oid);
                            Out.WriteUInt16(hostBuff.Entry);
                            Out.WriteByte((byte)cmd.SecondaryValue);
                            Out.WriteByte(0);
                            Out.WriteByte(1);
                            Out.WriteByte(0);
                            target.DispatchPacket(Out, true);
                        }
                        #endregion
                        hostBuff.AddBuffParameter(cmd.BuffLine, 0);
                    }
                    break;

                case BUFF_TICK:
                    if (cmd.CommandResult == 0)
                    {
                        target.CrowdControlType = 0;

                        #region ClientEffect
                        if (cmd.PrimaryValue >= 16)
                        {
                            target.StsInterface.RemoveVelocityModifier(hostBuff);

                            target.DispatchUpdateState((byte)StateOpcode.Stunned, 0);

                            PacketOut Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);
                            Out.WriteUInt16(hostBuff.Caster.Oid);
                            Out.WriteUInt16(target.Oid);
                            Out.WriteUInt16(hostBuff.Entry);
                            Out.WriteByte((byte)(cmd.SecondaryValue));
                            Out.WriteByte(0);
                            Out.WriteByte(0);
                            Out.WriteByte(0);
                            if (target.IsPlayer())
                                ((Player)target).DispatchPacket(Out, true);
                            else if (hostBuff.Caster.IsPlayer())
                                ((Player)hostBuff.Caster).DispatchPacket(Out, true);
                        }
                        #endregion
                    }

                    hostBuff.DeleteBuffParameter(cmd.BuffLine);
                    hostBuff.Resend();
                    break;
                case BUFF_END:
                    if (cmd.CommandResult == 0)
                    {
                        target.CrowdControlType = 0;

                        #region ClientEffect
                        if (cmd.PrimaryValue >= 16)
                        {
                            target.StsInterface.RemoveVelocityModifier(hostBuff);

                            target.DispatchUpdateState((byte)StateOpcode.Stunned, 0);

                            PacketOut Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);
                            Out.WriteUInt16(hostBuff.Caster.Oid);
                            Out.WriteUInt16(target.Oid);
                            Out.WriteUInt16(hostBuff.Entry);
                            Out.WriteByte((byte)(cmd.SecondaryValue));
                            Out.WriteByte(0);
                            Out.WriteByte(0);
                            Out.WriteByte(0);
                            if (target.IsPlayer())
                                ((Player)target).DispatchPacket(Out, true);
                            else if (hostBuff.Caster.IsPlayer())
                                ((Player)hostBuff.Caster).DispatchPacket(Out, true);
                        }
                        #endregion
                    }
                    break;
                case BUFF_REMOVE:
                    goto case 4;
            }

            return true;
        }

        private static bool Root(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    hostBuff.CrowdControl = (byte)(hostBuff.CrowdControl | (int)CrowdControlTypes.Root);
                    if (target.StsInterface != null)
                    {
                        if (cmd.PrimaryValue == 1)
                            target.StsInterface.AddVelocityModifier(hostBuff, 0);

                        else if (target.ImmuneToCC((int)CrowdControlTypes.Root, hostBuff.Caster, hostBuff.Entry) || !target.TryRoot(hostBuff))
                        {
                            cmd.CommandResult = 1;
                            return false;
                        }

                        hostBuff.AddBuffParameter(cmd.BuffLine, 0);

                        (hostBuff.Caster as Player)?.UpdatePlayerBountyEvent((byte)ContributionDefinitions.AOE_ROOT);
                    }

                    break;
                case BUFF_END:
                    if (cmd.CommandResult == 0)
                        target.StsInterface?.RemoveVelocityModifier(hostBuff);
                    break;
                case BUFF_REMOVE:
                    goto case 4;
            }

            return true;
        }

        private static bool Grapple(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    if (target.StsInterface != null)
                    {
                        hostBuff.CrowdControl = (byte)(hostBuff.CrowdControl | (int)CrowdControlTypes.Grapple);

                        if (target != hostBuff.Caster && (target.ImmuneToCC((int)CrowdControlTypes.Root, hostBuff.Caster, hostBuff.Entry) || target.IsImmovable))
                        {
                            cmd.CommandResult = 1;
                            return false;
                        }
                        target.EnterGrapple(hostBuff, target != hostBuff.Caster);
                        hostBuff.AddBuffParameter(cmd.BuffLine, 0);
                    }
                    break;
                case BUFF_END:
                    if (cmd.CommandResult == 0 && target.StsInterface != null)
                    {
                        target.StsInterface.RemoveVelocityModifier(hostBuff);
                        target.NoKnockbacks = false;
                    }
                    break;
                case BUFF_REMOVE:
                    goto case 4;
            }

            return true;
        }

        private static bool SetImmovable(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    hostBuff.AddBuffParameter(cmd.BuffLine, cmd.CommandResult);
                    if (cmd.PrimaryValue == 1)
                        target.SetImmovable(true);
                    break;
                case BUFF_END:
                    target.SetImmovable(false);
                    break;
                case BUFF_REMOVE:
                    goto case 4;
            }

            return true;
        }

        private static bool GitToTheChoppa(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            Unit source = hostBuff.Caster;
            List<Player> myPlayerList = new List<Player>();
            byte count = 0;

            foreach (Object obj in source.ObjectsInRange)
            {
                if (!obj.IsUnit())
                    continue;

                Unit curTarget = obj.GetUnit();

                if (!CombatInterface.CanAttack(hostBuff.Caster, curTarget))
                    continue;

                if (curTarget.ObjectWithinRadiusFeet(source, 30) && source.LOSHit(curTarget))
                {
                    AbilityDamageInfo damageThisPass = cmd.DamageInfo.Clone(hostBuff.Caster);
                    CombatManager.InflictDamage(damageThisPass, hostBuff.BuffLevel, source, curTarget);

                    if (damageThisPass.DamageEvent != 0 && damageThisPass.DamageEvent != 9)
                        continue;

                    Player plr = curTarget as Player;

                    if (plr != null && !plr.IsImmovable)
                        myPlayerList.Add(plr);

                    count++;
                }

                if (count == cmd.MaxTargets)
                    break;
            }

            if (myPlayerList.Count > 0)
            {
                Player pulledPlayer = myPlayerList[StaticRandom.Instance.Next(myPlayerList.Count)];

                pulledPlayer.PulledBy(hostBuff.Caster, 200, 1);
            }

            if (hostBuff.BuffState == BUFF_START)
                hostBuff.AddBuffParameter(cmd.BuffLine, 0);

            return true;
        }

        private static bool WindsKnockback(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            if (target is Player)
                ((Player)target).ApplyWindsKnockback(hostBuff.Caster, AbilityMgr.GetKnockbackInfo(cmd.Entry, cmd.PrimaryValue));

            else
                target.ApplyKnockback(hostBuff.Caster, null);

            return true;
        }

        private static bool Pull(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            if (CombatManager.CheckMagnetDefense(hostBuff.Entry, hostBuff.Caster, target, true))
                return false;
            if (!(target is Player))
                target.ApplyKnockback(hostBuff.Caster, null);
            else
                ((Player)target).PulledBy(hostBuff.Caster, (ushort)cmd.PrimaryValue, (ushort)cmd.SecondaryValue);
            return true;
        }

        private static bool RiftPull(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            if (CombatManager.CheckRiftDefense(hostBuff.Entry, hostBuff.Caster, target, true))
                return false;
            if (!(target is Player))
                target.ApplyKnockback(hostBuff.Caster, null);
            else
                ((Player)target).PulledBy(hostBuff.Caster, (ushort)cmd.PrimaryValue, (ushort)cmd.SecondaryValue);
            return true;
        }

        private static bool CCGuard(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    target.AddCrowdControlImmunity(cmd.PrimaryValue);

                    if (cmd.SecondaryValue == 1)
                        target.BuffInterface.RemoveCCFromPending(cmd.PrimaryValue); // Snare 1, Root 2

                    // THANKS MYTHIC
                    switch (hostBuff.Entry)
                    {
                        case 1445:
                        case 1757:
                            hostBuff.AddBuffParameter(1, 2);
                            hostBuff.AddBuffParameter(4, 2);
                            break;
                        case 8095:
                        case 9395:
                            hostBuff.AddBuffParameter(1, 2);
                            hostBuff.AddBuffParameter(4, 2);
                            break;
                        case 8408:
                        case 9173:
                            hostBuff.AddBuffParameter(4, 2);
                            hostBuff.AddBuffParameter(5, 2);
                            hostBuff.AddBuffParameter(6, 2);
                            break;
                        default:
                            hostBuff.AddBuffParameter(cmd.BuffLine, 2);
                            break;
                    }
                    break;
                case BUFF_END:
                    target.RemoveCrowdControlImmunity(cmd.PrimaryValue);
                    break;
                case BUFF_REMOVE:
                    goto case 4;
            }
            return true;
        }

        #endregion

        #region DamageMod

        private static bool Shield(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    cmd.CommandResult = (short)(cmd.PrimaryValue + (cmd.SecondaryValue - cmd.PrimaryValue) * (hostBuff.BuffLevel - 1) / 39);
                    hostBuff.Type = (BuffTypes)((int)hostBuff.Type | 32);
                    hostBuff.AddBuffParameter(cmd.BuffLine, cmd.CommandResult);
                    break;
                default:
                    Log.Error("BuffEffectInvoker", "Shield from entry " + hostBuff.Entry + " invoked in unsupported state!");
                    break;
            }

            return true;
        }

        private static bool DetauntDamage(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    if (hostBuff.Caster.CbtInterface.IsAttacking)
                        hostBuff.Caster.CbtInterface.IsAttacking = false;
                    hostBuff.AddBuffParameter(cmd.BuffLine, cmd.PrimaryValue);

#if DEBUG
                    //target.Detaunters.Add(hostBuff.Caster.Oid, cmd.PrimaryValue);
#endif
                    break;
                /*case BUFF_END:
                    if (target.Detaunters.ContainsKey(hostBuff.Caster.Oid))
                    { 
                        target.Detaunters.Remove(hostBuff.Caster.Oid);
                        Log.Texte("Removing...", "Removing...", ConsoleColor.Cyan);
                    }
                break;
                case BUFF_REMOVE:
                    if (target.Detaunters.ContainsKey(hostBuff.Caster.Oid))
                    {
                        target.Detaunters.Remove(hostBuff.Caster.Oid);
                        Log.Texte("Removing...", "Removing...", ConsoleColor.Cyan);
                    }
                break;*/
                default:
                    Log.Error("BuffEffectInvoker", "DetauntDamage from entry " + hostBuff.Entry + " invoked in unsupported state!");
                    break;
            }

            return true;
        }

        private static bool TauntMob(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            hostBuff.AddBuffParameter(cmd.BuffLine, cmd.PrimaryValue);
            if (!(target is Creature) || target is Pet)
                return true;
            target.AiInterface.ProcessTaunt(hostBuff.Caster, hostBuff.BuffLevel);

            return true;
        }

        #endregion

        #region Item

        
        private static bool GreatweaponMastery(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            Item myItem = hostBuff.Caster.ItmInterface.GetItemInSlot((ushort)EquipSlot.MAIN_HAND);

            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    if (myItem != null && myItem.Info.TwoHanded)
                    {
                        cmd.CommandResult = 1;
                        hostBuff.Target.StsInterface.AddBonusMultiplier(Stats.OutgoingDamagePercent, 0.1f, hostBuff.GetBuffClass(cmd));
                        hostBuff.Target.StsInterface.AddBonusStat(Stats.Parry, 5, hostBuff.GetBuffClass(cmd));
                    }

                    hostBuff.AddBuffParameter(1, 5);
                    hostBuff.AddBuffParameter(2, 10);
                    break;

                case BUFF_END:
                    if (cmd.CommandResult == 0)
                        return true;
                    hostBuff.Target.StsInterface.RemoveBonusMultiplier(Stats.OutgoingDamagePercent, 0.1f, hostBuff.GetBuffClass(cmd));
                    hostBuff.Target.StsInterface.RemoveBonusStat(Stats.Parry, 5, hostBuff.GetBuffClass(cmd));
                    break;
                case BUFF_REMOVE: goto case 4;
            }

            return true;
        }

        private static bool OppressingBlows(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            Item myItem = hostBuff.Caster.ItmInterface.GetItemInSlot((ushort)EquipSlot.MAIN_HAND);

            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    if (myItem != null && myItem.Info.TwoHanded)
                    {
                        cmd.CommandResult = 1;
                        hostBuff.Target.StsInterface.AddBonusStat(Stats.CriticalHitRate, 15, hostBuff.GetBuffClass(cmd));
                    }

                    hostBuff.AddBuffParameter(1, 15);
                    break;

                case BUFF_END:
                    if (cmd.CommandResult == 0)
                        return true;
                    hostBuff.Target.StsInterface.RemoveBonusStat(Stats.CriticalHitRate, 15, hostBuff.GetBuffClass(cmd));
                    break;
                case BUFF_REMOVE: goto case 4;
            }

            return true;
        }

        private static bool ModifyStatIfHasShield(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            Item myItem = hostBuff.Caster.ItmInterface.GetItemInSlot((ushort)EquipSlot.OFF_HAND);
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    if (myItem?.Info.Type == 5)
                    {
                        if (cmd.TertiaryValue != 0)
                            cmd.CommandResult = (short)(cmd.SecondaryValue + (cmd.TertiaryValue - cmd.SecondaryValue) * ((hostBuff.BuffLevel - 1) / 39.0f));
                        else cmd.CommandResult = (short)cmd.SecondaryValue;

                        if (cmd.CommandResult < 0)
                            target.StsInterface.AddReducedStat((Stats)cmd.PrimaryValue, (ushort)-cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                        else
                            target.StsInterface.AddBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    }

                    hostBuff.AddBuffParameter(cmd.BuffLine, cmd.CommandResult);
                    break;
                case BUFF_END:
                    if (cmd.CommandResult == 0)
                        break;
                    if (cmd.CommandResult < 0)
                        target.StsInterface.RemoveReducedStat((Stats)cmd.PrimaryValue, (ushort)-cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    else target.StsInterface.RemoveBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    break;
                case BUFF_REMOVE: goto case 4;
            }

            return true;
        }

        private static bool ModifyPercentageStatIfHasShield(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            Item myItem = hostBuff.Caster.ItmInterface.GetItemInSlot((ushort)EquipSlot.OFF_HAND);
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    if (myItem?.Info.Type == 5)
                    {
                        if (cmd.TertiaryValue != 0)
                            cmd.CommandResult = (short)(cmd.SecondaryValue + (cmd.TertiaryValue - cmd.SecondaryValue) * ((hostBuff.BuffLevel - 1) / 39.0f));
                        else cmd.CommandResult = (short)cmd.SecondaryValue;

                        if (cmd.CommandResult < 0)
                            target.StsInterface.AddReducedMultiplier((Stats)cmd.PrimaryValue, (100 + cmd.CommandResult) * 0.01f, hostBuff.GetBuffClass(cmd));
                        else
                            target.StsInterface.AddBonusMultiplier((Stats)cmd.PrimaryValue, cmd.CommandResult * 0.01f, hostBuff.GetBuffClass(cmd));
                    }

                    hostBuff.AddBuffParameter(cmd.BuffLine, cmd.CommandResult);
                    break;
                case BUFF_END:
                    if (cmd.CommandResult == 0)
                        break;
                    if (cmd.CommandResult < 0)
                        target.StsInterface.RemoveReducedMultiplier((Stats)cmd.PrimaryValue, (100 + cmd.CommandResult) * 0.01f, hostBuff.GetBuffClass(cmd));
                    else target.StsInterface.RemoveBonusMultiplier((Stats)cmd.PrimaryValue, cmd.CommandResult * 0.01f, hostBuff.GetBuffClass(cmd));
                    break;
                case BUFF_REMOVE: goto case 4;
            }

            return true;
        }

        public static bool ItemSwap(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            Player plr = target as Player;
            //need to look at this later but this is going to break everything if the player does not have a item in the slot at all.
            if (plr.ItmInterface.GetItemInSlot((ushort)cmd.SecondaryValue) == null)
                return true;

            byte showing = 0;
            uint orgAppearance = 0;
            Item item = plr.ItmInterface.GetItemInSlot((ushort)cmd.SecondaryValue);

            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    if (item.AltAppearanceEntry != 0)
                        orgAppearance = item.AltAppearanceEntry;

                    showing = plr._Value.GearShow;

                    item.AltAppearanceEntry = (uint)cmd.PrimaryValue;
                    if (cmd.SecondaryValue == (int)EquipSlot.BACK && (plr._Value.GearShow & 2) == 0)
                        plr.SetGearShowing(2, true);

                    if (cmd.SecondaryValue == (int)EquipSlot.HELM && (plr._Value.GearShow & 1) == 0)
                        plr.SetGearShowing(1, true);

                    PacketOut Out = new PacketOut((byte)Opcodes.F_GET_ITEM);
                    Out.WriteByte(1);
                    Out.Fill(0, 3);
                    Item.BuildItem(ref Out, item, null, null, (ushort)cmd.SecondaryValue, 0, plr);
                    plr.SendPacket(Out);
                    break;

                case BUFF_END:
                case BUFF_REMOVE:
                    item.AltAppearanceEntry = orgAppearance;
                    if (cmd.SecondaryValue == (int)EquipSlot.BACK && (showing & 2) == 0)
                        plr.SetGearShowing(2, false);

                    if (cmd.SecondaryValue == (int)EquipSlot.HELM && (showing & 1) == 0)
                        plr.SetGearShowing(1, false);

                    Out = new PacketOut((byte)Opcodes.F_GET_ITEM);
                    Out.WriteByte(1);
                    Out.Fill(0, 3);
                    Item.BuildItem(ref Out, item, null, null, (ushort)cmd.SecondaryValue, 0, plr);
                    plr.SendPacket(Out);
                    break;
            }
            return true;
        }

        private static bool Mount(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            Player plr = target as Player;

            if (plr == null)
                return false;

            if (!plr.CanMount)
                return false;

            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    plr.MountFromBuff(hostBuff, (ushort)cmd.PrimaryValue, (ushort)cmd.SecondaryValue, 1f + (cmd.LastCommand.PrimaryValue * 0.01f));
                    break;
                case BUFF_REMOVE:
                    // Send dismount smoke/effect
                    PacketOut Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);
                    Out.WriteUInt16(hostBuff.Caster.Oid);
                    Out.WriteUInt16(target.Oid);
                    Out.WriteUInt16(hostBuff.Entry);
                    Out.WriteByte(2);
                    Out.WriteByte(0);
                    Out.WriteByte(5);
                    Out.WriteByte(0);
                    target.DispatchPacket(Out, true);

                    plr.DismountFromBuff();
                    break;
            }

            return true;
        }

        private static bool Backpack(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            Player plr = target as Player;

            if (plr == null)
                return false;

            if (!plr.CanMount)
                return false;

            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    plr.MountBackpackFromBuff(hostBuff, (ushort)cmd.PrimaryValue, 1f + (cmd.LastCommand.PrimaryValue * 0.01f));
                    break;
                case BUFF_REMOVE:
                    // Send dismount smoke/effect
                    PacketOut Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);
                    Out.WriteUInt16(hostBuff.Caster.Oid);
                    Out.WriteUInt16(target.Oid);
                    Out.WriteUInt16(hostBuff.Entry);
                    Out.WriteByte(2);
                    Out.WriteByte(0);
                    Out.WriteByte(5);
                    Out.WriteByte(0);
                    target.DispatchPacket(Out, true);

                    plr.DismountBackpackFromBuff();
                    break;
            }

            return true;
        }

        #endregion

        #region Buff

        private static bool Chickenize(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    if (hostBuff.Caster.Realm == Realms.REALMS_REALM_ORDER)
                        target.OSInterface.AddEffect((byte)GameData.ObjectEffectState.OBJECTEFFECTSTATE_ORDER_CHICKEN);
                    else target.OSInterface.AddEffect((byte)GameData.ObjectEffectState.OBJECTEFFECTSTATE_CHAOS_CHICKEN);
                    ((Player)hostBuff.Target).IsPolymorphed = true;

                    for (byte i = 0; i <= (byte)Stats.CorporealResistance; ++i)
                    {
                        target.StsInterface.AddReducedMultiplier((Stats)i, i == (byte)Stats.Wounds ? 0.01f : 0.1f, BuffClass.Career);
                    }

                    target.StsInterface.AddReducedMultiplier(Stats.Armor, 0.1f, BuffClass.Career);
                    target.StsInterface.AddReducedMultiplier(Stats.Block, 0f, BuffClass.Career);
                    target.StsInterface.AddReducedMultiplier(Stats.Parry, 0f, BuffClass.Career);

                    target.BuffInterface.RemoveCasterBuffs();

                    Pet pet = ((Player)target).CrrInterface.GetTargetOfInterest() as Pet;

                    pet?.Dismiss(null, null);

                    for (byte i = 0; i < 10; ++i)
                        hostBuff.AddBuffParameter(i, 1);
                    break;
                case BUFF_END:
                    if (hostBuff.Caster.Realm == Realms.REALMS_REALM_ORDER)
                        target.OSInterface.RemoveEffect((byte)GameData.ObjectEffectState.OBJECTEFFECTSTATE_ORDER_CHICKEN);
                    else target.OSInterface.RemoveEffect((byte)GameData.ObjectEffectState.OBJECTEFFECTSTATE_CHAOS_CHICKEN);
                    ((Player)hostBuff.Target).IsPolymorphed = false;

                    for (byte i = 0; i <= (int)Stats.CorporealResistance; ++i)
                        target.StsInterface.RemoveReducedMultiplier((Stats)i, i == (int)Stats.Wounds ? 0.01f : 0.1f, BuffClass.Career);
                    target.StsInterface.RemoveReducedMultiplier(Stats.Armor, 0.1f, BuffClass.Career);
                    target.StsInterface.RemoveReducedMultiplier(Stats.Block, 0f, BuffClass.Career);
                    target.StsInterface.RemoveReducedMultiplier(Stats.Parry, 0f, BuffClass.Career);


                    break;
                case BUFF_REMOVE:
                    goto case 4;
            }

            return true;
        }

        private static bool HoldTheLine(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    if (hostBuff.Caster == hostBuff.Target)
                        target.StsInterface.HTLStacks += 3;
                    else ++target.StsInterface.HTLStacks;
                    hostBuff.AddBuffParameter(1, hostBuff.Caster == hostBuff.Target ? 45 : 15);

                    // Give contribution for HTL.
                    if (StaticRandom.Instance.Next(100) < HOLD_THE_LINE_CONTRIBUTION_CHANCE)
                    {
                        (hostBuff.Caster as Player)?.UpdatePlayerBountyEvent((byte)ContributionDefinitions.HOLD_THE_LINE);
                    }
                    break;
                case BUFF_END:
                case BUFF_REMOVE:
                    if (hostBuff.Caster == hostBuff.Target)
                        target.StsInterface.HTLStacks -= 3;
                    else --target.StsInterface.HTLStacks;
                    break;

            }
            return true;
        }

        private static bool InvokeBuff(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            if (cmd.SecondaryValue != 0 && StaticRandom.Instance.Next(100) > cmd.SecondaryValue)
                return false;

            // Deal with effects which apply when the buff ticks
            if (hostBuff.BuffState == BUFF_REMOVE && cmd.InvokeOn != 4)
                return false;

            BuffInfo buffInfo = AbilityMgr.GetBuffInfo((ushort)cmd.PrimaryValue, hostBuff.Caster, target);
            if (buffInfo.Entry == hostBuff.Entry)
            {
                if (buffInfo.Duration != 0)
                    buffInfo.Duration = (ushort)(hostBuff.RemainingTimeMs * 0.001f);
            }
            target.BuffInterface.QueueBuff(new BuffQueueInfo(hostBuff.Caster, hostBuff.BuffLevel, buffInfo));

            return true;
        }

        // Because of Pack Hunting being an exception to the general buff design.
        private static bool InvokeChildBuff(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            BuffInfo buffInfo = AbilityMgr.GetBuffInfo((ushort)cmd.PrimaryValue, hostBuff.Caster, target);

            target.BuffInterface.QueueBuff(new BuffQueueInfo(hostBuff.Caster, hostBuff.BuffLevel, buffInfo, hostBuff.SetLinkedBuff));

            return true;
        }

        private static bool InvokeBuffOnPet(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            Pet myPet = (((Player)hostBuff.Caster).CrrInterface.GetTargetOfInterest() as Pet);

            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    hostBuff.AddBuffParameter(cmd.BuffLine, 1);
                    myPet?.BuffInterface.QueueBuff(new BuffQueueInfo(myPet, hostBuff.BuffLevel, AbilityMgr.GetBuffInfo((ushort)cmd.PrimaryValue)));
                    break;
                case BUFF_REMOVE:
                    myPet?.BuffInterface.RemoveBuffByEntry((ushort)cmd.PrimaryValue);
                    break;
            }

            return true;
        }

        private static bool InvokeAuraOnPet(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            Pet myPet = (((Player)hostBuff.Caster).CrrInterface.GetTargetOfInterest() as Pet);

            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    hostBuff.AddBuffParameter(cmd.BuffLine, 1);
                    myPet?.BuffInterface.QueueBuff(new BuffQueueInfo(myPet, hostBuff.BuffLevel, AbilityMgr.GetBuffInfo((ushort)cmd.PrimaryValue), CreateAura));
                    break;
                case BUFF_REMOVE:
                    myPet?.BuffInterface.RemoveBuffByEntry((ushort)cmd.PrimaryValue);
                    break;
            }

            return true;
        }

        private static bool ReapplyBuff(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            if (hostBuff.Caster.BuffInterface.Stopping)
                return false;
            if (cmd.SecondaryValue == 100)
            {
                NewBuff existing = target.BuffInterface.GetBuff((ushort)cmd.PrimaryValue, target);
                if (existing != null)
                {
                    existing.BuffHasExpired = true;
                    if (existing is AuraBuff)
                        target.BuffInterface.QueueBuff(new BuffQueueInfo(hostBuff.Caster, hostBuff.BuffLevel, AbilityMgr.GetBuffInfo((ushort)cmd.PrimaryValue, hostBuff.Caster, target), CreateAura));
                    else
                        target.BuffInterface.QueueBuff(new BuffQueueInfo(hostBuff.Caster, hostBuff.BuffLevel, AbilityMgr.GetBuffInfo((ushort)cmd.PrimaryValue, hostBuff.Caster, target)));
                }
            }
            else if (StaticRandom.Instance.Next(100) < cmd.SecondaryValue)
                target.BuffInterface.QueueBuff(new BuffQueueInfo(hostBuff.Caster, hostBuff.BuffLevel, AbilityMgr.GetBuffInfo((ushort)cmd.PrimaryValue, hostBuff.Caster, target)));

            return true;
        }

        private static bool ReapplyClassBuff(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            if (hostBuff.Caster.BuffInterface.Stopping)
                return false;

            NewBuff crrBuff = target.BuffInterface.GetCareerBuff(cmd.PrimaryValue);

            if (crrBuff == null)
                return true;

            crrBuff.BuffHasExpired = true;
            target.BuffInterface.QueueBuff(new BuffQueueInfo(target, crrBuff.BuffLevel, AbilityMgr.GetBuffInfo(crrBuff.Entry, target, target)));

            return true;
        }

        private static bool ReapplyGroupBuff(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            List<Unit> members = new List<Unit> { hostBuff.Caster };

            Player player = (Player)hostBuff.Caster;

            if (player.WorldGroup != null)
                members.AddRange(player.WorldGroup.GetPlayerListCopy(player));
            if (player.ScenarioGroup != null)
                members.AddRange(player.ScenarioGroup.GetPlayerListCopy(player));

            foreach (Unit member in members)
            {
                if (member.BuffInterface.Stopping)
                    return false;

                NewBuff targetBuff = member.BuffInterface.GetBuff((ushort)cmd.PrimaryValue, hostBuff.Caster);

                if (targetBuff == null)
                    return true;

                targetBuff.BuffHasExpired = true;

                if (!hostBuff.Caster.BuffInterface.Stopping)
                    member.BuffInterface.QueueBuff(new BuffQueueInfo(hostBuff.Caster, targetBuff.BuffLevel, AbilityMgr.GetBuffInfo(targetBuff.Entry, hostBuff.Caster, member)));
            }

            return true;
        }

        private static bool CleanseDebuffType(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            if (StaticRandom.Instance.Next(100) < cmd.EventChance)
                return target.BuffInterface.CleanseDebuffType((byte)cmd.PrimaryValue, (byte)cmd.SecondaryValue) > 0;
            return false;
        }
        private static bool RemoveBuff(NewBuff hostbuff, BuffCommandInfo cmd, Unit target)
        {


            target.BuffInterface.RemoveBuffByEntry(Convert.ToUInt16(cmd.PrimaryValue));

            return true;
        }

        #endregion

        #region Misc

        private static bool RegisterModifiers(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            Player plr = target as Player;

            if (plr == null)
                return false;

            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    plr.TacInterface.RegisterGeneralBuff(hostBuff);
                    break;
                case BUFF_END:
                    plr.TacInterface.UnregisterGeneralBuff(hostBuff);
                    break;
                case BUFF_REMOVE:
                    goto case BUFF_END;
            }

            return true;
        }

        private static bool GrantTempAbility(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    hostBuff.Target.AbtInterface.GrantAbility((ushort)cmd.PrimaryValue);
                    break;
                case BUFF_END:
                case BUFF_REMOVE:
                    hostBuff.Target.AbtInterface.RemoveGrantedAbility((ushort)cmd.PrimaryValue);
                    break;
            }

            return true;
        }

        private static bool DetauntWard(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    target.BuffInterface.DetauntWard = true;
                    hostBuff.AddBuffParameter(1, cmd.BuffLine);
                    break;
                case BUFF_END:
                    target.BuffInterface.DetauntWard = false;
                    break;
                case BUFF_REMOVE: goto case 4;
            }

            return true;
        }

        private static bool AddBuffParameter(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            if (hostBuff.BuffState == BUFF_START)
                hostBuff.AddBuffParameter(cmd.BuffLine, (short)cmd.PrimaryValue);
            return true;
        }

        private static bool CastPlayerEffect(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);
            Out.WriteUInt16(hostBuff.Caster.Oid);
            Out.WriteUInt16(target.Oid);
            Out.WriteUInt16((ushort)cmd.PrimaryValue);
            Out.WriteByte((byte)cmd.SecondaryValue);
            Out.WriteByte(0);
            Out.WriteByte(5);
            Out.WriteByte(0);

            target.DispatchPacket(Out, true);
            return true;
        }

        private static bool ActivationEffect(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            if (hostBuff.Caster is Player)
            {
                Player plr = (Player)hostBuff.Caster;

                PacketOut Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);
                Out.WriteUInt16(hostBuff.Caster.Oid);
                Out.WriteUInt16(target.Oid);
                Out.WriteUInt16((ushort)cmd.PrimaryValue);
                Out.WriteByte((byte)cmd.SecondaryValue);
                Out.WriteByte(0);
                Out.WriteByte(hostBuff.BuffState == BUFF_START ? (byte)1 : (byte)0);
                Out.WriteByte(0);
                plr.DispatchPacket(Out, true);
            }

            return true;
        }

        private static bool Bolster(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START: (hostBuff.Caster as Player).ApplyBolster((byte)cmd.PrimaryValue); hostBuff.AddBuffParameter(cmd.BuffLine, cmd.PrimaryValue); break;
                case BUFF_REMOVE: (hostBuff.Caster as Player).ApplyBolster(0); break;
            }

            return true;
        }

        private static bool ObjectEffectState(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    target.OSInterface.AddEffect((byte)cmd.PrimaryValue);
                    hostBuff.AddBuffParameter(cmd.BuffLine, 1);
                    if (cmd.PrimaryValue == (byte)GameData.ObjectEffectState.OBJECTEFFECTSTATE_STEALTH && hostBuff.Caster is Player)
                    {
                        ((Player)hostBuff.Caster).Cloak(1);
                        hostBuff.Caster.CbtInterface.IsAttacking = false;
                    }
                    return true;
                case BUFF_END:
                    target.OSInterface.RemoveEffect((byte)cmd.PrimaryValue);
                    if (cmd.PrimaryValue == (byte)GameData.ObjectEffectState.OBJECTEFFECTSTATE_STEALTH && hostBuff.Caster is Player)
                        ((Player)hostBuff.Caster).Uncloak();
                    return true;
                case BUFF_REMOVE:
                    goto case 4;
                default:
                    return true;
            }
        }

        private static bool RefreshIfMoving(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            //Lets not let a refresh of a buff happen to a siege object. for now good but needs more once sieges and ram rework is complete.
            if (target is Siege)
                return true;

            //lets not let a refresh of a buff happen to a keep lord
            var lord = target as KeepCreature;
            if (lord != null && lord.returnflag().Info.KeepLord)
                return true;

            if (target.IsMoving)
            {
                // Maximum duration of refreshers (GSS) 30 seconds, scaled down to fit more conveniently within short int
                if (cmd.CommandResult < 3000)
                {
                    cmd.CommandResult += (short)((hostBuff.Duration * 1000 - hostBuff.RemainingTimeMs) * 0.1f);
                    hostBuff.SoftRefresh();
                }
            }

            return true;
        }

        private static bool InvokeCooldown(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    hostBuff.Caster.AbtInterface.SetCooldown(hostBuff.Entry, 120000, true);
                    break;
                case BUFF_END:
                case BUFF_REMOVE:
                    hostBuff.Caster.AbtInterface.SetCooldown(hostBuff.Entry, AbilityMgr.GetCooldownFor(hostBuff.Entry) * 1000);
                    break;
            }

            return true;
        }

        private static bool MoveAndShoot(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START: ((CombatInterface_Player)hostBuff.Caster.CbtInterface).MoveAndShoot = true; hostBuff.AddBuffParameter(cmd.BuffLine, 1); break;
                case BUFF_END: ((CombatInterface_Player)hostBuff.Caster.CbtInterface).MoveAndShoot = false; break;
                case BUFF_REMOVE: goto case 4;
            }

            return true;
        }

        private static bool PokeClassBuff(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            if (hostBuff.BuffState == BUFF_REMOVE)
                return false;
            hostBuff.Caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.Manual, null, target);
            return true;
        }

        private static bool BounceBuff(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            if (hostBuff.BuffState != 4)
                return false;

            BouncingBuff bBuff = hostBuff as BouncingBuff;

            if (bBuff == null || !bBuff.CanBounce())
                return false;

            foreach (var plr in hostBuff.Target.PlayersInRange)
            {
                if (plr.Realm != hostBuff.Target.Realm)
                    continue;
                if (!plr.ObjectWithinRadiusFeet(hostBuff.Target, 30))
                    continue;
                if (bBuff.previousPlayers.Contains(plr))
                    continue;
                plr.BuffInterface.QueueBuff(new BuffQueueInfo(hostBuff.Caster, hostBuff.BuffLevel, AbilityMgr.GetBuffInfo(hostBuff.Entry, hostBuff.Caster, plr), CreateBouncingBuff, bBuff.PassPreviousBounces));
                return true;
            }

            return false;
        }

        private static bool Hotswap(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    hostBuff.AddBuffParameter(1, 1);
                    break;
                case BUFF_END:
                    hostBuff.Target.BuffInterface.Hotswap(hostBuff, (ushort)cmd.PrimaryValue);
                    break;
            }

            return true;
        }

        private static bool ResourceHandler(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            Player player = (Player)hostBuff.Caster;

            if (hostBuff.BuffState == BUFF_START)
                cmd.CommandResult = 5;

            // Check out of PvE area.
            else if (player.CurrentArea == null || !player.CurrentArea.IsRvR)
            {
                --cmd.CommandResult;

                if (cmd.CommandResult > 0)
                {
                    player.SendClientMessage("You are carrying a resource out of the RvR area. Holding a resource outside of the RvR area too long will cause it to reset.", ChatLogFilters.CHATLOGFILTERS_RVR);
                    player.SendClientMessage("Must return to the RvR area when carrying a resource!", player.Realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE);
                }

                else
                {
                    HoldObjectBuff buff = (HoldObjectBuff)hostBuff;
                    buff.HeldObject.ResetTo(EHeldState.Home);
                    player.SendClientMessage("You held a resource outside of the RvR area for too long, and it has been reset.", ChatLogFilters.CHATLOGFILTERS_RVR);
                }
            }

            // Recalculate drop chance.
            cmd.EventChance = 50;

            if (player.WorldGroup != null)
            {
                if (player.BuffInterface.HasGuard())
                    cmd.EventChance -= 25;

                List<Player> nearby = player.WorldGroup.GetPlayersCloseTo(player, 50);

                cmd.EventChance -= (byte)(nearby.Count * 5);
            }

            return true;
        }

        private static bool GfxMod(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            if (hostBuff.BuffState == 1)
            {
                target.AddGfxMod((ushort)cmd.PrimaryValue, (ushort)cmd.SecondaryValue);
                if (cmd.TertiaryValue == 1)
                    target.EvtInterface.AddEvent(target.SendGfxMods, 1, 1);
                hostBuff.AddBuffParameter(cmd.BuffLine, 1);
            }
            return true;
        }

        private static bool BlockScenarioQueue(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    ((Player)hostBuff.Target).ScnInterface.BlockQueue = true;
                    hostBuff.AddBuffParameter(cmd.BuffLine, -1);
                    break;
                case BUFF_END:
                case BUFF_REMOVE:
                    ((Player)hostBuff.Target).ScnInterface.BlockQueue = false;
                    break;
            }

            return true;
        }

        #endregion

        #region NPCs
        private static bool SpawnCreature(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    break;

                case BUFF_TICK:
                    break;

                case BUFF_END:
                    for (int i = 0; i < cmd.SecondaryValue; i++)
                    {
                        Creature_proto Proto = CreatureService.GetCreatureProto((uint)cmd.PrimaryValue);

                        Creature_spawn Spawn = new Creature_spawn();
                        Spawn.Guid = (uint)CreatureService.GenerateCreatureSpawnGUID();
                        Spawn.BuildFromProto(Proto);
                        Spawn.WorldO = target.Heading;
                        Spawn.WorldX = target.WorldPosition.X;
                        Spawn.WorldY = target.WorldPosition.Y;
                        Spawn.WorldZ = target.WorldPosition.Z;
                        Spawn.ZoneId = (ushort)target.ZoneId;
                        Spawn.NoRespawn = 1;
                        Creature c = target.Region.CreateCreature(Spawn);
                        c.EvtInterface.AddEventNotify(EventName.OnDie, RemoveCreature); // We are removing spawns from world when adds die
                        c.EvtInterface.AddEvent(c.Destroy, 120 * 1000, 1); // We spawn adds just for 120 seconds
                    }
                    break;

                case BUFF_REMOVE:
                    break;
            }

            return true;
        }

        // This is used to clean up after creature that was spawned in SpawnCreature was killed - we don't want to add new spawn to the world forever
        static public bool RemoveCreature(Object npc = null, object instigator = null)
        {
            Creature c = npc as Creature;
            c.EvtInterface.AddEvent(c.Destroy, 100, 1);
            return false;
        }

        // When caster dies - just clean it up
        static public bool RemoveCreatureNow(Object npc = null, object instigator = null)
        {
            Creature c = npc as Creature;
            c.EvtInterface.AddEvent(c.Destroy, 100, 1);
            return false;
        }

        #endregion

        #region Pet

        private static bool SummonVanityPet(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            Player myPlayer = (Player)hostBuff.Target;

            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    if (!myPlayer.CbtInterface.IsPvp)
                    {
                        Creature_proto proto = CreatureService.GetCreatureProto((uint)cmd.PrimaryValue);
                        Creature_spawn spawn = new Creature_spawn();

                        if (proto == null)
                        {
                            Log.Error("SummonVanityPet", "No proto at " + cmd.PrimaryValue);
                            return true;
                        }

                        proto.MinScale = 50;
                        proto.MaxScale = 50;
                        spawn.BuildFromProto(proto);
                        spawn.WorldO = myPlayer._Value.WorldO;
                        spawn.WorldY = myPlayer._Value.WorldY;
                        spawn.WorldZ = myPlayer._Value.WorldZ;
                        spawn.WorldX = myPlayer._Value.WorldX;
                        spawn.ZoneId = myPlayer.Zone.ZoneId;
                        spawn.Icone = 18;
                        spawn.WaypointType = 0;
                        spawn.Proto.MinLevel = spawn.Proto.MaxLevel = myPlayer.EffectiveLevel;

                        Pet vanityPet = new Pet(0, spawn, myPlayer, 3, false, false);

                        vanityPet.IsVanity = true;
                        myPlayer.Region.AddObject(vanityPet, spawn.ZoneId);
                        myPlayer.Companion = vanityPet;
                    }
                    else
                    {
                        hostBuff.Caster.AbtInterface.Cancel(true);
                        myPlayer.BuffInterface.RemoveBuffByEntry(hostBuff.Entry);
                        myPlayer.SendClientMessage("You cannot summon vanity pet when you are flagged for RvR.", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                    }
                    return true;
                case BUFF_REMOVE:
                    if (myPlayer.Companion != null && myPlayer.Companion.IsVanity)
                    {
                        myPlayer.Companion.RemoveVanityPet();
                    }
                    return true;
                default:
                    return true;

            }
        }

        private static bool ManagePetAbilities(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            Pet myPet = target as Pet;

            if (myPet == null)
                return false;

            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    myPet.AddAbilities((ushort)cmd.PrimaryValue, (ushort)cmd.SecondaryValue);
                    return true;
                case BUFF_END:
                    myPet.RemoveAbilities();
                    return true;
                case BUFF_REMOVE:
                    goto case 4;
                default:
                    return false;
            }
        }

        private static bool ResetAttackTimer(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            target.CbtInterface.ResetAttackTimer();

            return true;
        }

        // Pack Synergy.
        private static bool MasterPetModifyStat(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    Pet myPet = (((Player)hostBuff.Caster).CrrInterface.GetTargetOfInterest() as Pet);

                    if (PetAlive(myPet))
                    {
                        if (cmd.TertiaryValue != 0)
                            cmd.CommandResult = (short)(cmd.SecondaryValue + (cmd.TertiaryValue - cmd.SecondaryValue) * ((hostBuff.BuffLevel - 1) / 39.0f));
                        else cmd.CommandResult = (short)cmd.SecondaryValue;

                        hostBuff.Caster.StsInterface.AddBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                        myPet.StsInterface.AddBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    }

                    hostBuff.AddBuffParameter(cmd.BuffLine, (short)(cmd.SecondaryValue + (cmd.TertiaryValue - cmd.SecondaryValue) * ((hostBuff.BuffLevel - 1) / 39.0f)));
                    break;


                case BUFF_REMOVE:
                    if (cmd.CommandResult > 0)
                    {
                        myPet = (((Player)hostBuff.Caster).CrrInterface.GetTargetOfInterest() as Pet);

                        myPet?.StsInterface.RemoveBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                        hostBuff.Caster.StsInterface.RemoveBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));

                        cmd.CommandResult = 0;
                    }

                    break;
            }

            return true;
        }

        private static bool PetModifyStat(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    Pet myPet = (((Player)hostBuff.Caster).CrrInterface.GetTargetOfInterest() as Pet);

                    if (PetAlive(myPet))
                    {
                        if (cmd.TertiaryValue != 0)
                            cmd.CommandResult = (short)(cmd.SecondaryValue + (cmd.TertiaryValue - cmd.SecondaryValue) * ((hostBuff.BuffLevel - 1) / 39.0f));
                        else cmd.CommandResult = (short)cmd.SecondaryValue;

                        if (cmd.CommandResult < 0)
                            myPet.StsInterface.AddReducedStat((Stats)cmd.PrimaryValue, (ushort)-cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                        else
                            myPet.StsInterface.AddBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    }
                    break;
                case BUFF_REMOVE:
                    myPet = (((Player)hostBuff.Caster).CrrInterface.GetTargetOfInterest() as Pet);

                    if (PetAlive(myPet))
                    {
                        if (cmd.CommandResult < 0)
                            myPet.StsInterface.RemoveReducedStat((Stats)cmd.PrimaryValue, (ushort)-cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                        else
                            myPet.StsInterface.RemoveBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    }
                    break;
            }

            hostBuff.AddBuffParameter(cmd.BuffLine, 1);

            return true;
        }

        private static bool MasterModifyPercentageStatReverse(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    Pet myPet = (((Player)hostBuff.Caster).CrrInterface.GetTargetOfInterest() as Pet);

                    if (!PetAlive(myPet))
                    {
                        cmd.CommandResult = (short)cmd.SecondaryValue;

                        hostBuff.Caster.StsInterface.AddBonusMultiplier((Stats)cmd.PrimaryValue, cmd.CommandResult * 0.01f, hostBuff.GetBuffClass(cmd));
                    }
                    break;

                case BUFF_REMOVE:
                    if (cmd.CommandResult > 0)
                    {
                        hostBuff.Caster.StsInterface.RemoveBonusMultiplier((Stats)cmd.PrimaryValue, cmd.CommandResult * 0.01f, hostBuff.GetBuffClass(cmd));

                        cmd.CommandResult = 0;
                    }

                    break;
            }

            hostBuff.AddBuffParameter(cmd.BuffLine, 1);

            return true;
        }

        private static bool RestoreAPOnDeath(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            if (hostBuff.Target.IsDead)
                hostBuff.Caster.BuffInterface.QueueBuff(new BuffQueueInfo(hostBuff.Caster, hostBuff.BuffLevel, AbilityMgr.GetBuffInfo(3071)));

            return true;
        }

        private static bool PetModifySpeed(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            switch (hostBuff.BuffState)
            {
                case BUFF_START:
                    Pet myPet = (((Player)hostBuff.Caster).CrrInterface.GetTargetOfInterest() as Pet);
                    if (PetAlive(myPet))
                        myPet.StsInterface.AddVelocityModifier(hostBuff, cmd.PrimaryValue / 100f);
                    hostBuff.AddBuffParameter(cmd.BuffLine, 1);
                    break;
                case BUFF_REMOVE:
                    myPet = (((Player)hostBuff.Caster).CrrInterface.GetTargetOfInterest() as Pet);
                    if (PetAlive(myPet))
                        myPet.StsInterface.RemoveVelocityModifier(hostBuff);
                    break;
            }

            return true;
        }

        private static bool TickMoraleIfHasPet(NewBuff hostBuff, BuffCommandInfo cmd, Unit target)
        {
            Pet myPet = ((Player)hostBuff.Caster).CrrInterface.GetTargetOfInterest() as Pet;
            if (PetAlive(myPet))
                ((Player)hostBuff.Caster).AddMorale(cmd.PrimaryValue);

            return true;
        }
        #endregion

        #endregion

        #region AbilityUseCommands

        private static void DamageByAbilityType(NewBuff hostBuff, BuffCommandInfo cmd, AbilityInfo abInfo)
        {
            if ((int)abInfo.ConstantInfo.AbilityType == cmd.PrimaryValue)
                CombatManager.InflictProcDamage(cmd.DamageInfo, hostBuff.BuffLevel, hostBuff.Caster, hostBuff.Target);
        }

        private static void AbEventModifyAp(NewBuff hostBuff, BuffCommandInfo cmd, AbilityInfo abInfo)
        {
            ModifyAp(hostBuff, cmd, hostBuff.Target);
        }

        private static void InvokeBuffOnResourceAttack(NewBuff hostBuff, BuffCommandInfo cmd, AbilityInfo abInfo)
        {
            if (abInfo.ConstantInfo.IsDamaging && abInfo.SpecialCost > 0)
                hostBuff.Caster.BuffInterface.QueueBuff(new BuffQueueInfo(hostBuff.Caster, hostBuff.BuffLevel, AbilityMgr.GetBuffInfo((ushort)cmd.PrimaryValue)));
        }

        private static void InvokeBuff(NewBuff hostBuff, BuffCommandInfo cmd, AbilityInfo abInfo)
        {
            hostBuff.Target.BuffInterface.QueueBuff(new BuffQueueInfo(hostBuff.Caster, hostBuff.BuffLevel, AbilityMgr.GetBuffInfo((ushort)cmd.PrimaryValue, hostBuff.Caster, hostBuff.Caster)));
        }

        // Honor Restored
        private static void DealDamageOnFinisherUse(NewBuff hostBuff, BuffCommandInfo cmd, AbilityInfo abInfo)
        {
            if (abInfo.SpecialCost == 25)
            {
                CombatManager.HealTarget(cmd.DamageInfo, hostBuff.BuffLevel, hostBuff.Caster, hostBuff.Target);
            }

        }

        #endregion

        #region PetEventCommands

        private static bool PetAlive(Pet myPet)
        {
            return myPet != null && !myPet.IsDead && !myPet.PendingDisposal && !myPet.IsDisposed;
        }

        private static void MasterPetModifyStat(NewBuff hostBuff, BuffCommandInfo cmd, Pet myPet)
        {
            if (PetAlive(myPet))
            {
                if (cmd.CommandResult > 0)
                {
                    ((Player)hostBuff.Caster).SendClientMessage(hostBuff.BuffName + " attempted to add its effect twice!", ChatLogFilters.CHATLOGFILTERS_SHOUT);
                    return;
                }

                if (cmd.TertiaryValue != 0)
                    cmd.CommandResult = (short)(cmd.SecondaryValue + (cmd.TertiaryValue - cmd.SecondaryValue) * ((hostBuff.BuffLevel - 1) / 39.0f));
                else cmd.CommandResult = (short)cmd.SecondaryValue;

                hostBuff.Caster.StsInterface.AddBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                myPet.StsInterface.AddBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
            }
            else
            {
                if (cmd.CommandResult > 0)
                {
                    hostBuff.Caster.StsInterface.RemoveBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                    cmd.CommandResult = 0;
                }
            }
        }

        private static void PetModifyStat(NewBuff hostBuff, BuffCommandInfo cmd, Pet myPet)
        {
            if (PetAlive(myPet))
            {
                if (cmd.TertiaryValue != 0)
                    cmd.CommandResult = (short)(cmd.SecondaryValue + (cmd.TertiaryValue - cmd.SecondaryValue) * ((hostBuff.BuffLevel - 1) / 39.0f));
                else cmd.CommandResult = (short)cmd.SecondaryValue;

                if (cmd.CommandResult < 0)
                    myPet.StsInterface.AddReducedStat((Stats)cmd.PrimaryValue, (ushort)-cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                else
                    myPet.StsInterface.AddBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
            }
        }

        private static void PetModifySpeed(NewBuff hostBuff, BuffCommandInfo cmd, Pet myPet)
        {
            if (PetAlive(myPet))
                myPet.StsInterface.AddVelocityModifier(hostBuff, cmd.PrimaryValue / 100f);
        }

        private static void MasterModifyPercentageStatReverse(NewBuff hostBuff, BuffCommandInfo cmd, Pet myPet)
        {
            if (!PetAlive(myPet))
            {
                cmd.CommandResult = (short)cmd.SecondaryValue;

                hostBuff.Caster.StsInterface.AddBonusMultiplier((Stats)cmd.PrimaryValue, cmd.CommandResult * 0.01f, hostBuff.GetBuffClass(cmd));
            }
            else
            {
                if (cmd.CommandResult > 0)
                {
                    hostBuff.Caster.StsInterface.RemoveBonusMultiplier((Stats)cmd.PrimaryValue, cmd.CommandResult * 0.01f, hostBuff.GetBuffClass(cmd));
                    cmd.CommandResult = 0;
                }
            }
        }

        private static void InvokeBuffOnPet(NewBuff hostBuff, BuffCommandInfo cmd, Pet myPet)
        {
            myPet?.BuffInterface.QueueBuff(new BuffQueueInfo(myPet, hostBuff.BuffLevel, AbilityMgr.GetBuffInfo((ushort)cmd.PrimaryValue)));
        }

        private static void InvokeAuraOnPet(NewBuff hostBuff, BuffCommandInfo cmd, Pet myPet)
        {
            myPet?.BuffInterface.QueueBuff(new BuffQueueInfo(myPet, hostBuff.BuffLevel, AbilityMgr.GetBuffInfo((ushort)cmd.PrimaryValue), CreateAura));
        }

        private static void MasterInvokeBuffOnPetDie(NewBuff hostBuff, BuffCommandInfo cmd, Pet myPet)
        {
            if (myPet != null && myPet.IsDead)
                hostBuff.Caster.BuffInterface.QueueBuff(new BuffQueueInfo(hostBuff.Caster, hostBuff.Caster.EffectiveLevel, AbilityMgr.GetBuffInfo((ushort)cmd.PrimaryValue)));
        }

        private static void MasterModifyMoraleOnPetDie(NewBuff hostBuff, BuffCommandInfo cmd, Pet myPet)
        {
            if (myPet != null && myPet.IsDead)
            {
                Player plrTarget = hostBuff.Target as Player;

                if (plrTarget == null)
                    return;

                if (cmd.PrimaryValue < 0)
                    plrTarget.ConsumeMorale(-cmd.PrimaryValue);
                else
                    plrTarget.AddMorale(cmd.PrimaryValue);
            }
        }

        #endregion

        #region ResourceEventCommands

        private static void ResEventModifyCareerRes(NewBuff hostBuff, BuffCommandInfo cmd, byte oldVal, ref byte change)
        {
            change += (byte)cmd.PrimaryValue;
        }

        private static void ResEventModifyStatByResourceLevel(NewBuff hostBuff, BuffCommandInfo cmd, byte oldVal, ref byte change)
        {
            if (oldVal == change)
                return;

            if (cmd.CommandResult < 0)
                hostBuff.Target.StsInterface.RemoveReducedStat((Stats)cmd.PrimaryValue, (ushort)-cmd.CommandResult, hostBuff.GetBuffClass(cmd));
            else if (cmd.CommandResult > 0)
                hostBuff.Target.StsInterface.RemoveBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));

            //hostBuff.Caster.Say("Removed "+cmd.CommandResult+" from stat "+cmd.PrimaryValue, SystemData.ChatLogFilters.CHATLOGFILTERS_SAY);

            cmd.CommandResult = 0;

            byte resLv = ((Player)hostBuff.Target).CrrInterface.GetLevelForResource(change, (byte)cmd.EventCheckParam);

            if (resLv > 0)
            {
                if (cmd.TertiaryValue != 0)
                    cmd.CommandResult = (short)(cmd.SecondaryValue + (cmd.TertiaryValue - cmd.SecondaryValue) * ((hostBuff.BuffLevel - 1) / 39.0f));
                else cmd.CommandResult = (short)cmd.SecondaryValue;

                cmd.CommandResult *= resLv;

                if (cmd.CommandResult < 0)
                    hostBuff.Target.StsInterface.AddReducedStat((Stats)cmd.PrimaryValue, (ushort)-cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                else
                    hostBuff.Target.StsInterface.AddBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));

                //hostBuff.Caster.Say("Added" + cmd.CommandResult + " to stat " + cmd.PrimaryValue, SystemData.ChatLogFilters.CHATLOGFILTERS_SAY);
            }
        }

        private static void ResEventModifyPercentageStatByResourceLevel(NewBuff hostBuff, BuffCommandInfo cmd, byte oldVal, ref byte change)
        {
            if (oldVal == change)
                return;

            if (cmd.CommandResult < 0)
                hostBuff.Target.StsInterface.RemoveReducedMultiplier((Stats)cmd.PrimaryValue, (100 + cmd.CommandResult) * 0.01f, hostBuff.GetBuffClass(cmd));
            else hostBuff.Target.StsInterface.RemoveBonusMultiplier((Stats)cmd.PrimaryValue, cmd.CommandResult * 0.01f, hostBuff.GetBuffClass(cmd));

            //hostBuff.Caster.Say("Removed "+cmd.CommandResult+" from stat "+cmd.PrimaryValue);

            cmd.CommandResult = 0;

            byte resLv = ((Player)hostBuff.Target).CrrInterface.GetLevelForResource(change, (byte)cmd.EventCheckParam);

            if (resLv > 0)
            {
                cmd.CommandResult = (short)(cmd.SecondaryValue * hostBuff.StackLevel);
                cmd.CommandResult *= resLv;

                if (cmd.CommandResult < 0)
                    hostBuff.Target.StsInterface.AddReducedMultiplier((Stats)cmd.PrimaryValue, (100 + cmd.CommandResult) * 0.01f, hostBuff.GetBuffClass(cmd));
                else
                    hostBuff.Target.StsInterface.AddBonusMultiplier((Stats)cmd.PrimaryValue, cmd.CommandResult * 0.01f, hostBuff.GetBuffClass(cmd));

                //hostBuff.Caster.Say("Added " + cmd.CommandResult + " to stat " + cmd.PrimaryValue);
            }
        }

        private static void ResEventModifyStatIfHasResource(NewBuff hostBuff, BuffCommandInfo cmd, byte oldVal, ref byte change)
        {
            if (oldVal == 0 && change == 0)
                return;

            if (change <= cmd.EventCheckParam)
            {
                if (cmd.CommandResult == 0)
                    return;

                if (cmd.CommandResult < 0)
                    hostBuff.Target.StsInterface.RemoveReducedStat((Stats)cmd.PrimaryValue, (ushort)-cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                else if (cmd.CommandResult > 0)
                    hostBuff.Target.StsInterface.RemoveBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                cmd.CommandResult = 0;
            }
            else
            {
                if (cmd.CommandResult != 0)
                    return;

                if (cmd.TertiaryValue != 0)
                    cmd.CommandResult = (short)(cmd.SecondaryValue + (cmd.TertiaryValue - cmd.SecondaryValue) * ((hostBuff.BuffLevel - 1) / 39.0f));
                else cmd.CommandResult = (short)cmd.SecondaryValue;

                if (cmd.CommandResult < 0)
                    hostBuff.Target.StsInterface.AddReducedStat((Stats)cmd.PrimaryValue, (ushort)-cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                else
                    hostBuff.Target.StsInterface.AddBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
            }
        }

        private static void ViolentImpacts(NewBuff hostBuff, BuffCommandInfo cmd, byte oldVal, ref byte change)
        {
            change = (byte)(change - 25);
        }

        #endregion

        #region ItemEventCommands

        // Yeah I'm lazy.
        private static void EventGreatweaponMastery(NewBuff hostBuff, BuffCommandInfo cmd, Item_Info itemInfo)
        {
            if (itemInfo.TwoHanded ^ cmd.CommandResult == 0)
                return;

            if (itemInfo.TwoHanded)
            {
                cmd.CommandResult = 1;
                hostBuff.Target.StsInterface.AddBonusMultiplier(Stats.OutgoingDamagePercent, 0.1f, hostBuff.GetBuffClass(cmd));
                hostBuff.Target.StsInterface.AddBonusStat(Stats.Parry, 5, hostBuff.GetBuffClass(cmd));
            }

            else
            {
                cmd.CommandResult = 0;
                hostBuff.Target.StsInterface.RemoveBonusMultiplier(Stats.OutgoingDamagePercent, 0.1f, hostBuff.GetBuffClass(cmd));
                hostBuff.Target.StsInterface.RemoveBonusStat(Stats.Parry, 5, hostBuff.GetBuffClass(cmd));
            }
        }
        private static void EventOppressingBlows(NewBuff hostBuff, BuffCommandInfo cmd, Item_Info itemInfo)
        {
            if (itemInfo.TwoHanded ^ cmd.CommandResult == 0)
                return;

            if (itemInfo.TwoHanded)
            {
                cmd.CommandResult = 1;
                hostBuff.Target.StsInterface.AddBonusStat(Stats.CriticalHitRate, 15, hostBuff.GetBuffClass(cmd));
            }

            else
            {
                cmd.CommandResult = 0;
                hostBuff.Target.StsInterface.RemoveBonusStat(Stats.CriticalHitRate, 15, hostBuff.GetBuffClass(cmd));
            }
        }

        private static void EventModifyStatIfHasShield(NewBuff hostBuff, BuffCommandInfo cmd, Item_Info itemInfo)
        {
            if ((itemInfo?.Type == 5) ^ cmd.CommandResult == 0)
                return;

            if (itemInfo?.Type == 5)
            {
                if (cmd.TertiaryValue != 0)
                    cmd.CommandResult = (short)(cmd.SecondaryValue + (cmd.TertiaryValue - cmd.SecondaryValue) * ((hostBuff.BuffLevel - 1) / 39.0f));
                else cmd.CommandResult = (short)cmd.SecondaryValue;

                if (cmd.CommandResult < 0)
                    hostBuff.Target.StsInterface.AddReducedStat((Stats)cmd.PrimaryValue, (ushort)-cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                else
                    hostBuff.Target.StsInterface.AddBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));
            }

            else
            {
                if (cmd.CommandResult < 0)
                    hostBuff.Target.StsInterface.RemoveReducedStat((Stats)cmd.PrimaryValue, (ushort)-cmd.CommandResult, hostBuff.GetBuffClass(cmd));
                else hostBuff.Target.StsInterface.RemoveBonusStat((Stats)cmd.PrimaryValue, (ushort)cmd.CommandResult, hostBuff.GetBuffClass(cmd));

                cmd.CommandResult = 0;
            }
        }

        private static void EventModifyPercentageStatIfHasShield(NewBuff hostBuff, BuffCommandInfo cmd, Item_Info itemInfo)
        {
            if ((itemInfo?.Type == 5) ^ cmd.CommandResult == 0)
                return;

            if (itemInfo?.Type == 5)
            {
                if (cmd.TertiaryValue != 0)
                    cmd.CommandResult = (short)(cmd.SecondaryValue + (cmd.TertiaryValue - cmd.SecondaryValue) * ((hostBuff.BuffLevel - 1) / 39.0f));
                else cmd.CommandResult = (short)cmd.SecondaryValue;

                if (cmd.CommandResult < 0)
                    hostBuff.Target.StsInterface.AddReducedMultiplier((Stats)cmd.PrimaryValue, (100 + cmd.CommandResult) * 0.01f, hostBuff.GetBuffClass(cmd));
                else
                    hostBuff.Target.StsInterface.AddBonusMultiplier((Stats)cmd.PrimaryValue, cmd.CommandResult * 0.01f, hostBuff.GetBuffClass(cmd));
            }

            else
            {
                if (cmd.CommandResult < 0)
                    hostBuff.Target.StsInterface.RemoveReducedMultiplier((Stats)cmd.PrimaryValue, (100 + cmd.CommandResult) * 0.01f, hostBuff.GetBuffClass(cmd));
                else hostBuff.Target.StsInterface.RemoveBonusMultiplier((Stats)cmd.PrimaryValue, cmd.CommandResult * 0.01f, hostBuff.GetBuffClass(cmd));

                cmd.CommandResult = 0;
            }
        }

        #endregion

        #region DamageEventCommands

        #region Damage
        private static bool DealDamage(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            if (cmd.TargetType == CommandTargetTypes.EventInstigator)
                target = eventInstigator;

            AbilityDamageInfo damageThisPass = cmd.DamageInfo.Clone(hostBuff.Caster);

            if (cmd.EffectRadius > 0)
                damageThisPass.IsAoE = true;

            if (cmd.DamageInfo.IsHeal)
                CombatManager.HealTarget(damageThisPass, hostBuff.BuffLevel, hostBuff.Caster, target);
            else CombatManager.InflictDamage(damageThisPass, hostBuff.BuffLevel, hostBuff.Caster, target);

            return damageThisPass.DamageEvent == 0 || damageThisPass.DamageEvent == 9;
        }

        private static bool DealBacklashAoE(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            AbilityDamageInfo damageThisPass = damageInfo.Clone();
            damageThisPass.PrecalcDamage = damageThisPass.Damage * 0.5f;
            CombatManager.InflictPrecalculatedDamage(damageThisPass, hostBuff.Caster, target, 1, true);

            return true;
        }

        private static bool DealProcDamage(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            // Need to clone damage here because we could potentially be hitting more than one person at once with a proc
            AbilityDamageInfo damageThisPass = cmd.DamageInfo.Clone(hostBuff.Caster);

            if (cmd.TargetType == CommandTargetTypes.EventInstigator)
                target = eventInstigator;

            if (cmd.EffectRadius > 0)
                damageThisPass.IsAoE = true;

            if (cmd.PrimaryValue == 1)
            {
                if (cmd.DamageInfo.IsHeal)
                    CombatManager.ProcHealTarget(damageThisPass, hostBuff.BuffLevel, hostBuff.Target, target);
                else CombatManager.InflictProcDamage(damageThisPass, hostBuff.BuffLevel, hostBuff.Target, target);
            }
            else
            {
                if (cmd.DamageInfo.IsHeal)
                    CombatManager.ProcHealTarget(damageThisPass, hostBuff.BuffLevel, hostBuff.Caster, target);
                else CombatManager.InflictProcDamage(damageThisPass, hostBuff.BuffLevel, hostBuff.Caster, target);
            }

            cmd.CommandResult = (short)damageThisPass.Damage;
            return true;
        }

        private static bool HealHPBelow(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            return DealDamage(hostBuff, cmd, damageInfo, target, eventInstigator);
        }

        private static bool EventStealLife(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            ushort toHeal = (ushort)(cmd.LastCommand.CommandResult * (cmd.PrimaryValue / 100f));

            int pointsHealed = CombatManager.RawLifeSteal(toHeal, cmd.DamageInfo?.DisplayEntry ?? cmd.Entry, (byte)cmd.SecondaryValue, hostBuff.Caster, target);

            if (pointsHealed == -1)
                return false;

            cmd.CommandResult = cmd.LastCommand.CommandResult;

            return true;
        }

        private static bool EventStealLifeFromDamageInfo(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            if (damageInfo == null)
                return false;

            if (cmd.DamageInfo == null)
                throw new NullReferenceException("Missing DamageInfo for StealLifeFromDamageInfo " + cmd.Entry + ", " + cmd.CommandID + ", " + cmd.CommandSequence);

            int pointsHealed = CombatManager.RawLifeSteal((ushort)(damageInfo.Damage * (cmd.PrimaryValue / 100f)), cmd.DamageInfo.DisplayEntry, (byte)cmd.SecondaryValue, hostBuff.Caster, target);

            if (pointsHealed == -1)
                return false;

            cmd.CommandResult = (short)damageInfo.Damage;

            return true;
        }

        private static bool EventShield(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            // Can't block Morale damage
            if (damageInfo.DamageType == DamageTypes.RawDamage)
                return true;

            if (cmd.CommandResult > 0)
            {
                if (damageInfo.Damage >= cmd.CommandResult)
                {
                    damageInfo.Damage -= cmd.CommandResult;
                    damageInfo.Absorption += cmd.CommandResult;
                    cmd.CommandResult = 0;
                    hostBuff.BuffHasExpired = true;
                }
                else
                {
                    damageInfo.Absorption += damageInfo.Damage;
                    cmd.CommandResult -= (short)damageInfo.Damage;
                    damageInfo.Damage = 0;
                }
            }

            return true;
        }

        private static bool EventResurrection(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            (eventInstigator as Player)?.RezUnit((Point3D)hostBuff.OptionalObject, cmd.DamageInfo.DisplayEntry, hostBuff.Caster, cmd.PrimaryValue, cmd.SecondaryValue == 1, cmd.DamageInfo);

            hostBuff.RemoveStack();

            return true;
        }

        private static bool ReduceDamage(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            float damageReductionFactor = cmd.PrimaryValue * 0.01f;

            if (damageReductionFactor == 0)
            {
                damageInfo.Damage = 1;
                damageInfo.Mitigation = 0;
                damageInfo.Absorption = 0;
            }

            else
            {
                damageInfo.Damage *= damageReductionFactor;
                damageInfo.Mitigation *= damageReductionFactor;
                damageInfo.Absorption *= damageReductionFactor;
            }

            return true;
        }

        // For Warrior Priest and DoK experimental mechanics - increase damage dealt by Path of Torture / Path of Wrath skills
        private static bool DamageByCareerResource(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            Player player = (Player)hostBuff.Target;
            damageInfo.DamageBonus += player.CrrInterface.GetCurrentResourceLevel(0) * cmd.PrimaryValue * 0.01f;
            ((CareerInterface_WPDoK)player.CrrInterface).UpdateDrain();

            return true;
        }

        // For Rune of Sanc and Mark of Remaking when player dies - sets buff duration to 10m and sends resurrection prompt
        private static bool SelfRez(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            hostBuff.UpdateDuration(600);
            hostBuff.AddBuffParameter(2, -1);
            hostBuff.SoftRefresh();
            ((Player)hostBuff.Target).SendDialog(Dialog.ResurrectionOffer, hostBuff.Caster.Oid, 600);

            return true;
        }

        #endregion

        #region Resource

        private static bool EventModifyAp(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            if (cmd.TargetType == CommandTargetTypes.EventInstigator)
                target = eventInstigator;

            target.ModifyActionPoints((short)cmd.PrimaryValue);
            return true;
        }

        private static bool EventModifyCareerRes(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            if (cmd.PrimaryValue > 0)
                ((Player)hostBuff.Caster).CrrInterface.AddResource((byte)cmd.PrimaryValue, cmd.SecondaryValue == 1);
            else if (!((Player)hostBuff.Caster).CrrInterface.ConsumeResource((byte)-cmd.PrimaryValue, cmd.SecondaryValue == 1))
            {
                hostBuff.RemoveStack();
                return false;
            }

            return true;
        }

        private static bool BuffOverrideModifyCareerRes(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            if (cmd.PrimaryValue > 0)
            {
                if (cmd.TertiaryValue != 1)
                    ((Player)hostBuff.Caster).CrrInterface.AddResourceOverride((byte)cmd.PrimaryValue, cmd.SecondaryValue == 1, false);

                else
                    ((Player)hostBuff.Caster).CrrInterface.AddResourceOverride((byte)cmd.PrimaryValue, cmd.SecondaryValue == 1, true);
            }


            else if (!((Player)hostBuff.Caster).CrrInterface.ConsumeResource((byte)-cmd.PrimaryValue, cmd.SecondaryValue == 1))
            {
                hostBuff.RemoveStack();
                return false;
            }

            return true;
        }

        private static bool EventSetCareerRes(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            SetCareerRes(hostBuff, cmd, target);

            return true;
        }

        private static bool EventModifyMorale(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            if (cmd.TargetType == CommandTargetTypes.EventInstigator)
                target = eventInstigator;
            Player plrTarget = target as Player;

            if (plrTarget == null)
                return true;

            if (cmd.PrimaryValue < 0)
                plrTarget.ConsumeMorale(-cmd.PrimaryValue);
            else
                plrTarget.AddMorale(cmd.PrimaryValue);
            return true;
        }

        #endregion

        #region Taunt/Detaunt

        private static bool TauntDamage(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            if (eventInstigator == hostBuff.Caster)
            {
                int buffClass = (int)hostBuff.GetBuffClass(cmd);
                if (damageInfo.ExclusiveBonusApplied[buffClass])
                    return false;
                damageInfo.ExclusiveBonusApplied[buffClass] = true;
                damageInfo.DamageBonus += cmd.PrimaryValue * 0.01f;
            }

            return true;
        }

        private static bool ChallengeDamage(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            if (eventInstigator != hostBuff.Caster)
            {
                if (!damageInfo.ExclusiveReductionApplied[2])
                {
                    damageInfo.DamageReduction *= cmd.PrimaryValue * 0.01f;
                    damageInfo.ExclusiveReductionApplied[2] = true;
                }
            }
            else
                hostBuff.RemoveStack();

            return true;
        }

        private static bool EventDetauntDamage(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            int buffClass = (int)hostBuff.BuffClass;
            if (damageInfo.ExclusiveReductionApplied[buffClass])
                return false;
            if (eventInstigator == hostBuff.Caster)
            {
                damageInfo.ExclusiveReductionApplied[buffClass] = true;
                damageInfo.DamageReduction *= cmd.PrimaryValue / 100f;
            }

            return true;
        }

        #endregion

        #region Stack Management

        private static bool RemoveStack(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            hostBuff.RemoveStack();

            return true;
        }

        private static bool TauntRemoveStack(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            if (eventInstigator == hostBuff.Caster)
                hostBuff.RemoveStack();

            return true;
        }

        #endregion

        #region Buff Spread

        private static bool InvokeBuff(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            if (cmd.SecondaryValue == 0)
                cmd.CommandResult = (short)cmd.PrimaryValue;
            else cmd.CommandResult = (short)(cmd.PrimaryValue + StaticRandom.Instance.Next(cmd.SecondaryValue - cmd.PrimaryValue + 1));

            if (cmd.TargetType == CommandTargetTypes.EventInstigator)
                target = eventInstigator;

            BuffInfo b = AbilityMgr.GetBuffInfo((ushort)cmd.CommandResult, hostBuff.Caster, target);

            if (damageInfo != null && damageInfo.IsAoE)
                b.IsAoE = true;

            target.BuffInterface.QueueBuff(new BuffQueueInfo(cmd.TertiaryValue == 0 ? hostBuff.Caster : hostBuff.Target, hostBuff.BuffLevel, b));

            return true;
        }


        private static bool RemoveBuff(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            if (cmd.TargetType == CommandTargetTypes.EventInstigator)
                target = eventInstigator;

            target.BuffInterface.RemoveBuffByEntry(Convert.ToUInt16(cmd.PrimaryValue));

            return true;
        }

        private static bool InvokeBuffIfResAttack(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            if (!AbilityMgr.RequiresResource(damageInfo.Entry))
                return false;

            if (cmd.TargetType == CommandTargetTypes.EventInstigator)
                eventInstigator.BuffInterface.QueueBuff(new BuffQueueInfo(hostBuff.Caster, hostBuff.BuffLevel, AbilityMgr.GetBuffInfo((ushort)cmd.PrimaryValue, hostBuff.Caster, eventInstigator)));
            else
                target.BuffInterface.QueueBuff(new BuffQueueInfo(hostBuff.Caster, hostBuff.BuffLevel, AbilityMgr.GetBuffInfo((ushort)cmd.PrimaryValue, hostBuff.Caster, target)));

            return true;
        }

        private static bool SpreadBuff(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            if (cmd.LastCommand == null)
            {
                Log.Error("SpreadBuff", "No last command to pull a buff from!");
                return false;
            }

            cmd.CommandResult = cmd.LastCommand.CommandResult;

            if (cmd.LastCommand.CommandResult == 0)
            {
                cmd.CommandResult = cmd.LastCommand.LastCommand?.CommandResult ?? 0;
                if (cmd.CommandResult == 0)
                    Log.Error("SpreadBuff", "LastCommand result was 0 - this command: " + cmd.Entry + " " + AbilityMgr.GetAbilityNameFor(cmd.Entry) + " last command:" + cmd.LastCommand.Entry + " " + AbilityMgr.GetAbilityNameFor(cmd.LastCommand.Entry) + " " + cmd.LastCommand.CommandName);
                return true;
            }

            if (cmd.TargetType == CommandTargetTypes.EventInstigator)
                eventInstigator.BuffInterface.QueueBuff(new BuffQueueInfo(hostBuff.Caster, hostBuff.BuffLevel, AbilityMgr.GetBuffInfo((ushort)cmd.CommandResult, hostBuff.Caster, eventInstigator)));
            else
                target.BuffInterface.QueueBuff(new BuffQueueInfo(hostBuff.Caster, hostBuff.BuffLevel, AbilityMgr.GetBuffInfo((ushort)cmd.CommandResult, hostBuff.Caster, target)));
            return true;
        }

        #endregion

        #region Special

        // I got nothing :/
        private static bool EventCastPlayerEffect(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 18);
            Out.WriteUInt16(hostBuff.Caster.Oid);
            Out.WriteUInt16(target.Oid);
            Out.WriteUInt16((ushort)cmd.PrimaryValue);
            Out.WriteByte((byte)cmd.SecondaryValue);
            Out.WriteByte(0);
            Out.WriteByte(5);
            Out.WriteByte(0);

            Player plr = hostBuff.Caster as Player;

            plr?.DispatchPacket(Out, true);
            return true;
        }

        private static bool PuntEnemy(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            if (cmd.TargetType == CommandTargetTypes.EventInstigator)
                target = eventInstigator;

            target.ApplyKnockback(hostBuff.Caster, AbilityMgr.GetKnockbackInfo(cmd.Entry, cmd.PrimaryValue));

            return true;
        }

        private static bool SoftRefresh(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            hostBuff.SoftRefresh();
            return true;
        }

        private static bool ResourceHandler(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            Player player = (Player)hostBuff.Caster;
            player.SendClientMessage("You have dropped the supplies!", player.Realm == Realms.REALMS_REALM_DESTRUCTION ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE);
            lock (player.PlayersInRange)
                foreach (Player plr in player.PlayersInRange)
                    plr.SendClientMessage($"{player.Name} has dropped supplies!", player.Realm == Realms.REALMS_REALM_DESTRUCTION ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE);

            hostBuff.BuffHasExpired = true;

            return true;
        }

        #region MDPS
        private static bool BroadSwings(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            if (target == eventInstigator)
                return true;
            AbilityDamageInfo damageThisPass = new AbilityDamageInfo { Entry = 629, DisplayEntry = 629, DamageType = DamageTypes.RawDamage, MinDamage = (ushort)(damageInfo.Damage) };

            CombatManager.InflictProcDamage(damageThisPass, hostBuff.BuffLevel, hostBuff.Caster, target);

            return true;
        }

        #endregion

        #region BattlefieldObjective
        private static bool DaGreenest(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            cmd.CommandResult = damageInfo.DamageType == 0 ? (short)3242 : (short)(3238 + (int)damageInfo.DamageType);

            if (cmd.TargetType == CommandTargetTypes.EventInstigator)
                eventInstigator.BuffInterface.QueueBuff(new BuffQueueInfo(hostBuff.Caster, hostBuff.BuffLevel, AbilityMgr.GetBuffInfo((ushort)cmd.CommandResult)));
            else
                target.BuffInterface.QueueBuff(new BuffQueueInfo(hostBuff.Caster, hostBuff.BuffLevel, AbilityMgr.GetBuffInfo((ushort)cmd.CommandResult)));

            return true;
        }
        #endregion

        #region WL
        private static bool AddCriticalChance(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            damageInfo.CriticalHitRate += (byte)cmd.PrimaryValue;

            return true;
        }

        private static bool AddDamageBonus(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            damageInfo.DamageBonus += cmd.PrimaryValue * 0.01f;

            return true;
        }
        #endregion

        #region Slayer

        private static bool IncreaseCritDamageByHPLost(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            damageInfo.CriticalHitDamageBonus += (100 - hostBuff.Target.PctHealth) * 0.01f;

            return true;
        }

        private static bool AutoLife(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            target.Region.AddObject(new RunicBlessingsHandler((Player)target), target.Zone.ZoneId);

            return true;
        }

        #endregion

        #region SH
        private static bool ToiSplitDamage(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            Unit myToi = (hostBuff.Caster as Player).CrrInterface.GetTargetOfInterest();

            if (myToi == null || myToi.IsDead || !hostBuff.Caster.ObjectWithinRadiusFeet(myToi, cmd.PrimaryValue))
                return false;

            AbilityDamageInfo toiDamage = new AbilityDamageInfo();

            toiDamage.Entry = hostBuff.Entry;
            toiDamage.PrecalcDamage = damageInfo.Damage * 0.5f;

            CombatManager.InflictPrecalculatedDamage(toiDamage, hostBuff.Caster, myToi, 1, true);

            damageInfo.Damage *= 0.5f;
            damageInfo.Mitigation *= 0.5f;

            return true;
        }
        #endregion

        #region Engi/Magus
        private static bool EventSlay(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            target.ReceiveDamage(target, int.MaxValue);

            return true;
        }
        #endregion

        #region AM/Shaman

        private static bool AmShamanResOnCrit(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            if (damageInfo.ResourceBuild == 0)
                return false;

            if (damageInfo.ResourceBuild == 1)
                ((Player)hostBuff.Caster).CrrInterface.AddResource(1, true);
            else ((Player)hostBuff.Caster).CrrInterface.ConsumeResource(1, true);

            return true;
        }

        private static bool CleanseDebuffType(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            if (cmd.TargetType == CommandTargetTypes.EventInstigator)
                target = eventInstigator;

            return target.BuffInterface.CleanseDebuffType((byte)cmd.PrimaryValue, (byte)cmd.SecondaryValue) > 0;
        }

        private static bool PassItOn(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            foreach (Player plr in eventInstigator.PlayersInRange)
            {
                if (plr.Realm != eventInstigator.Realm || plr.IsDead || !eventInstigator.ObjectWithinRadiusFeet(plr, 30))
                    continue;

                CombatManager.HealTarget(cmd.DamageInfo, hostBuff.BuffLevel, hostBuff.Caster, plr);

                return true;
            }

            return false;
        }

        #endregion

        #region Zealot
        private static bool TransferDamage(NewBuff hostBuff, BuffCommandInfo cmd, AbilityDamageInfo damageInfo, Unit target, Unit eventInstigator)
        {
            Unit offTarget;

            switch (cmd.TargetType)
            {
                case CommandTargetTypes.EventInstigator:
                    offTarget = eventInstigator;
                    break;
                case CommandTargetTypes.Enemy:
                    offTarget = hostBuff.Caster.CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ENEMY);
                    break;
                default:
                    offTarget = target;
                    break;
            }

            if (offTarget == null)
                return false;

            AbilityDamageInfo damageThisPass = new AbilityDamageInfo { Entry = cmd.Entry, DisplayEntry = cmd.Entry, DamageType = (DamageTypes)cmd.SecondaryValue, MinDamage = (ushort)(damageInfo.Damage * cmd.PrimaryValue * 0.01f), CastPlayerSubID = (byte)cmd.TertiaryValue };

            CombatManager.InflictProcDamage(damageThisPass, hostBuff.BuffLevel, hostBuff.Caster, offTarget);

            return true;
        }
        #endregion

        #endregion

        #endregion

        #region Event Checks

        private static bool DamageIsCritical(NewBuff hostBuff, AbilityDamageInfo damageInfo, uint value, Unit eventInstigator)
        {
            if (value > 0 && damageInfo.StatUsed != value)
                return false;
            return damageInfo.DamageEvent == 9;
        }

        private static bool StealthBreak(NewBuff hostBuff, AbilityDamageInfo damageInfo, uint value, Unit eventInstigator)
        {
            return !damageInfo.IsAoE && (hostBuff.Duration * 1000) - hostBuff.RemainingTimeMs > 2000;
        }

        private static bool IsSingleTargetDamage(NewBuff hostBuff, AbilityDamageInfo damageInfo, uint value, Unit eventInstigator)
        {
            return !damageInfo.IsAoE && (value == 0 || damageInfo.StatUsed == value);
        }

        private static bool MutatedCritical(NewBuff hostBuff, AbilityDamageInfo damageInfo, uint value, Unit eventInstigator)
        {
            return damageInfo.DamageEvent == 9 && ((Player)hostBuff.Caster).CrrInterface.HasResource(7);
        }

        private static bool TargetNotCaster(NewBuff hostBuff, AbilityDamageInfo damageInfo, uint value, Unit eventInstigator)
        {
            return eventInstigator != hostBuff.Caster;
        }

        private static bool DamageIsFromStat(NewBuff hostBuff, AbilityDamageInfo damageInfo, uint value, Unit eventInstigator)
        {
            return damageInfo.StatUsed == value;
        }

        private static bool DamageIsMeleeAbility(NewBuff hostBuff, AbilityDamageInfo damageInfo, uint value, Unit eventInstigator)
        {
            return damageInfo.Entry != 0 && damageInfo.StatUsed == 1;
        }

        private static bool TargetHPBelow(NewBuff hostBuff, AbilityDamageInfo damageInfo, uint value, Unit eventInstigator)
        {
            return !hostBuff.Target.IsDead && hostBuff.Target.PctHealth < value;
        }

        private static bool InstigatorHPBelow(NewBuff hostBuff, AbilityDamageInfo damageInfo, uint value, Unit eventInstigator)
        {
            return !eventInstigator.IsDead && eventInstigator.PctHealth < value;
        }

        private static bool CheckDefenseFlags(NewBuff hostBuff, AbilityDamageInfo damageInfo, uint flags, Unit eventInstigator)
        {
            byte count = (byte)CombatEvent.COMBATEVENT_BLOCK;

            //can´t hit me, none shall pass and oathstone should never damage yourself, so set it to false so it never procs.
            if (hostBuff.Caster == eventInstigator && (hostBuff.Entry == 1382 || hostBuff.Entry == 1692 || hostBuff.Entry == 9345))
                return false;

            while (flags != 0)
            {
                if ((flags & 1) > 0 && damageInfo.DamageEvent == count)
                    return true;
                ++count;
                flags = flags >> 1;
            }

            return false;
        }

        private static bool CheckDefenseFlagsNotSelf(NewBuff hostBuff, AbilityDamageInfo damageInfo, uint flags, Unit eventInstigator)
        {
            if (eventInstigator == hostBuff.Caster)
                return false;

            byte count = (byte)CombatEvent.COMBATEVENT_BLOCK;

            while (flags != 0)
            {
                if ((flags & 1) > 0 && damageInfo.DamageEvent == count)
                    return true;
                ++count;
                flags = flags >> 1;
            }

            return false;
        }

        private static bool InstigatorNotSelf(NewBuff hostBuff, AbilityDamageInfo damageInfo, uint flags, Unit eventInstigator)
        {
            return eventInstigator != hostBuff.Caster;
        }

        private static bool MissingBuff(NewBuff hostBuff, AbilityDamageInfo damageInfo, uint value, Unit eventInstigator)
        {

            NewBuff buff = hostBuff.Caster.BuffInterface.GetBuff((ushort)value, null);
            if (buff != null && buff.BuffHasExpired)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private static bool DamageIsInTree(NewBuff hostBuff, AbilityDamageInfo damageInfo, uint value, Unit eventInstigator)
        {
            return damageInfo.MasteryTree == value;
        }

        private static bool DamageIsFromID(NewBuff hostBuff, AbilityDamageInfo damageInfo, uint value, Unit eventInstigator)
        {
            return damageInfo.Entry == value;
        }

        private static bool DamageIsCriticalFromTree(NewBuff hostBuff, AbilityDamageInfo damageInfo, uint value, Unit eventInstigator)
        {
            return damageInfo.DamageEvent == 9 && damageInfo.MasteryTree == value;
        }

        private static bool IsSingleTargetCriticalDamage(NewBuff hostbuff, AbilityDamageInfo damageInfo, uint value, Unit eventInstigator)
        {
            return damageInfo.DamageEvent == 9 && !damageInfo.IsAoE;
        }

        private static bool DamageIsFromMagicType(NewBuff hostbuff, AbilityDamageInfo damageInfo, uint value, Unit eventInstigator)
        {
            if (damageInfo.DamageType == DamageTypes.Corporeal)
            {
                return true;
            }
            if (damageInfo.DamageType == DamageTypes.Elemental)
            {
                return true;
            }
            if (damageInfo.DamageType == DamageTypes.Spiritual)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool GiftOfKhaine(NewBuff hostBuff, AbilityDamageInfo damageInfo, uint value, Unit eventInstigator)
        {
            return damageInfo.DamageEvent == 9
                && damageInfo.MasteryTree == value
                && hostBuff.Caster.CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ALLY) != null
                && hostBuff.Caster.CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ALLY) != hostBuff.Caster;
        }

        private static bool IsFlanking(NewBuff hostBuff, AbilityDamageInfo damageInfo, uint value, Unit eventInstigator)
        {
            return !eventInstigator.IsObjectInFront(hostBuff.Target, 90);
        }

        private static bool IsBehind(NewBuff hostBuff, AbilityDamageInfo damageInfo, uint value, Unit eventInstigator)
        {
            return !eventInstigator.IsObjectInFront(hostBuff.Target, 240);
        }

        private static bool IsFlankingWithAbility(NewBuff hostBuff, AbilityDamageInfo damageInfo, uint value, Unit eventInstigator)
        {
            if (damageInfo.Entry == 0)
                return false;
            return !eventInstigator.IsObjectInFront(hostBuff.Target, 90);
        }

        private static bool TOIWithinRange(NewBuff hostBuff, AbilityDamageInfo damageInfo, uint value, Unit eventInstigator)
        {
            Unit TOI = ((Player)hostBuff.Caster).CrrInterface.GetTargetOfInterest();

            return TOI != null && hostBuff.Caster.ObjectWithinRadiusFeet(TOI, (int)value);
        }

        private static bool AllowDismount(NewBuff hostBuff, AbilityDamageInfo damageInfo, uint value, Unit eventInstigator)
        {
            return true;

            Player player = (Player)hostBuff.Caster;

            return player.CurrentSiege == null;
        }

        // Not thread safe - should we ever change to a different threading model, will need to change this.
        private static bool DamageThrottle(NewBuff hostBuff, AbilityDamageInfo damageInfo, uint value, Unit eventInstigator)
        {
            if (damageInfo.Entry == 0)
                return true; // auto attack pass

            long curTime = TCPManager.GetTimeStampMS();

            if (curTime < hostBuff.AbilityThrottleReleaseTime)
                return false;

            hostBuff.AbilityThrottleReleaseTime = curTime + 100;

            return true;
        }

        #endregion

        #region Buff Creators

        public static NewBuff CreateAura()
        {
            return new AuraBuff();
        }

        public static NewBuff CreateOygBuff()
        {
            return new OYGBuff();
        }

        public static NewBuff CreateOygAuraBuff()
        {
            return new OYGAuraBuff();
        }

        public static NewBuff CreateGuardBuff()
        {
            return new GuardBuff();
        }

        public static NewBuff CreateOathFriendBuff()
        {
            return new OathFriendBuff();
        }

        public static NewBuff CreateDarkProtectorBuff()
        {
            return new DarkProtectorBuff();
        }

        public static NewBuff CreateBouncingBuff()
        {
            return new BouncingBuff();
        }
        #endregion
    }
}
