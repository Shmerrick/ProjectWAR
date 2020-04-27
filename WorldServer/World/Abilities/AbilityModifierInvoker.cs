//#define MODIFIER_DEBUG

using System;
using System.Collections.Generic;
using SystemData;
using Common;
using FrameWork;
using GameData;
using WorldServer.Managers;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using Item = WorldServer.World.Objects.Item;

namespace WorldServer.World.Abilities
{

    /// <summary>
    /// <para>Contains a set of ability checks and the effects that should be invoked if those checks pass.</para>
    /// </summary>  
    public class AbilityModifier
    {
        private readonly List<AbilityModifierCheck> _myCheckList = new List<AbilityModifierCheck>();

        public ushort Affecting { get; }
        public ushort Source { get; }
        public AbilityModifierEffect Effect { get; private set; }

        public AbilityModifier(ushort source, ushort affecting)
        {
            Affecting = affecting;
            Source = source;
        }

        public void AddCheck(AbilityModifierCheck check)
        {
            while (_myCheckList.Count < check.Sequence)
                _myCheckList.Add(null);

            if (_myCheckList.Count == check.Sequence)
                _myCheckList.Add(check);
            else if (_myCheckList[check.Sequence] != null)
                _myCheckList[check.Sequence].AddCheck(check);
            else _myCheckList[check.Sequence] = check;
        }

        public void AddModifier(AbilityModifierEffect effect)
        {
            if (Effect == null)
                Effect = effect;
            else Effect.AddModifier(effect);
        }

        public byte ModifyAbility(Unit caster, AbilityInfo abInfo)
        {
            Tuple<bool, byte> result = AbilityModifierInvoker.PerformCheck(_myCheckList, caster, abInfo.Target, abInfo);

            if (!result.Item1)
                return result.Item2;

            if (Effect != null)
                AbilityModifierInvoker.InvokeEffect(Effect, caster, abInfo);
            return 0;
        }

        public void ModifyBuff(Unit caster, Unit target, BuffInfo buffInfo)
        {
            Tuple<bool, byte> result = AbilityModifierInvoker.PerformCheck(_myCheckList, caster, target, null);

            if (!result.Item1)
                return;

            AbilityModifierInvoker.InvokeBuffEffect(Effect, caster, buffInfo);
        }
    }

    /// <summary>
    /// Responsible for performing pre-cast modifications on abilities.
    /// </summary>
    [Service(typeof(WorldMgr))]
    public static class AbilityModifierInvoker
    {
        private delegate bool AbilityCheckDelegate(Unit caster, Unit target, AbilityInfo myInfo, AbilityModifierCheck myCheck);
        private delegate void AbilityModifierDelegate(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect);
        private delegate void BuffModifierDelegate(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect);

        private static readonly Dictionary<string, AbilityCheckDelegate> CheckList = new Dictionary<string, AbilityCheckDelegate>();
        private static readonly Dictionary<string, AbilityModifierDelegate> ModifierList = new Dictionary<string, AbilityModifierDelegate>();
        private static readonly Dictionary<string, BuffModifierDelegate> BuffModifierList = new Dictionary<string, BuffModifierDelegate>();

        [LoadingFunction(false)]
        public static void LoadModifierCommands()
        {
            CheckList.Clear();

            CheckList.Add("IsBehind", IsBehind);
            CheckList.Add("IsFlanking", IsFlanking);
            CheckList.Add("WithinJumpZ", WithinJumpZ);
            CheckList.Add("TargetIsCasting", TargetIsCasting);
            CheckList.Add("TargetHPBelow", TargetHPBelow);
            CheckList.Add("IsPrincipalTarget", IsPrincipalTarget);
            CheckList.Add("HasCriticalBackstab", HasCriticalBackstab);
            CheckList.Add("IsCCed", IsCCed);
            CheckList.Add("IsImpeded", IsImpeded);
            CheckList.Add("CanMove", CanMove);
            CheckList.Add("NotImmovable", NotImmovable);
            CheckList.Add("HasResource", HasResource);
            CheckList.Add("ForTheHagQueen", ForTheHagQueen);
            CheckList.Add("HasBuff", HasBuff);
            CheckList.Add("MissingBuff", MissingBuff);
            CheckList.Add("TargetHasBuff", TargetHasBuff);
            CheckList.Add("HasCareerBuff", HasCareerBuff);
            CheckList.Add("HasBuffOfType", HasBuffOfType);
            CheckList.Add("HasDefended", HasDefended);
            CheckList.Add("IsGrounded", IsGrounded);
            CheckList.Add("TargetDefended", TargetDefended);
            CheckList.Add("OutOfCombat", OutOfCombat);
            CheckList.Add("OutOfRvR", OutOfRvR);
            CheckList.Add("CasterTargetRelation", CasterTargetRelation);
            CheckList.Add("CasterTargetSameRealm", CasterTargetSameRealm);
            CheckList.Add("TargetWithinRange", TargetWithinRange);
            CheckList.Add("HostileWithinRange", HostileWithinRange);
            CheckList.Add("TargetWithinRangeOfPet", TargetWithinRangeOfPet);
            CheckList.Add("TOIWithinRange", TOIWithinRange);
            CheckList.Add("RemoveIfOn", RemoveIfOn);
            CheckList.Add("CheckAllowAura", CheckAllowAura);
            CheckList.Add("TargetIsPlayer", TargetIsPlayer);
            CheckList.Add("TargetIsOrganic", TargetIsOrganic);
            CheckList.Add("OffensiveDamaging", OffensiveDamaging);
            CheckList.Add("RequiresResource", RequiresResource);
            CheckList.Add("CanDeploySiege", CanDeploySiege);
            CheckList.Add("IsOffensive", IsOffensive);
            CheckList.Add("ExperimentalMode", ExperimentalMode);
            CheckList.Add("CanMount", CanMount);
            CheckList.Add("ItemInSlot", ItemInSlot);

            ModifierList.Clear();

            ModifierList.Add("MoveCast", MoveCast);
            ModifierList.Add("ModifyCastTime", ModifyCastTime);
            ModifierList.Add("ShifterCastTimeBonus", ShifterCastTimeBonus);
            ModifierList.Add("ShifterCooldownChange", ShifterCooldownChange);
            ModifierList.Add("AddCooldownMS", AddCooldownMS);
            ModifierList.Add("SetCooldown", SetCooldown);
            ModifierList.Add("MultiplyCooldown", MultiplyCooldown);
            ModifierList.Add("MultiplyCooldownGreatweapon", MultiplyCooldownGreatweapon);
            ModifierList.Add("ModifySpecialCost", ModifySpecialCost);
            ModifierList.Add("ModifyDamageBonus", ModifyDamageBonus);
            ModifierList.Add("ModifyDamageType", ModifyDamageType);
            ModifierList.Add("ModifyArmorPenFactor", ModifyArmorPenFactor);
            ModifierList.Add("ModifyCriticalHitRate", ModifyCriticalHitRate);
            ModifierList.Add("ModifyCriticalDamage", ModifyCriticalDamage);
            ModifierList.Add("ModifyDefensibility", ModifyDefensibility);
            ModifierList.Add("AddAPCost", AddAPCost);
            ModifierList.Add("SetAPCost", SetAPCost);
            ModifierList.Add("ModifyAPCostByResourceLevel", ModifyAPCostByResourceLevel);
            ModifierList.Add("MultiplyAPCost", MultiplyAPCost);
            ModifierList.Add("MultiplyRange", MultiplyRange);
            ModifierList.Add("FeedingOnPain", FeedingOnPain);

            ModifierList.Add("SwitchDamage", SwitchDamage);
            ModifierList.Add("ResourceSwitchDamage", ResourceSwitchDamage);
            ModifierList.Add("SwitchParameters", SwitchCommandParams);
            ModifierList.Add("ModifyCommandDamageBonus", ModifyCommandDamageBonus);
            ModifierList.Add("ModifyCommandCritChance", ModifyCommandCritChance);
            ModifierList.Add("SetUndefendable", SetUndefendable);
            ModifierList.Add("SetCommandRadius", SetCommandRadius);
            ModifierList.Add("DragonGunRange", DragonGunRange);
            ModifierList.Add("ModifyCommandArmorPen", ModifyCommandArmorPen);
            ModifierList.Add("ModifyCommandArmorPenScale", ModifyCommandArmorPenScale);
            ModifierList.Add("MultiplyCommandRadius", MultiplyCommandRadius);
            ModifierList.Add("ModifyAPCostByCareerResource", ModifyAPCostByCareerResource);
            ModifierList.Add("AddResourceLevelToCommandParam", AddResourceLevelToCommandParam);
            ModifierList.Add("ExclusiveCleanse", ExclusiveCleanse);
            ModifierList.Add("MultiplyCommandParamByResourceLevel", MultiplyCommandParamByResourceLevel);
            ModifierList.Add("ShifterStatChange", ShifterStatChange);
            ModifierList.Add("ShifterDamageBonus", ShifterDamageBonus);

            ModifierList.Add("AddAbilityCommand", AddAbilityCommand);
            ModifierList.Add("AddAbilityCommandWithDamage", AddAbilityCommandWithDamage);
            ModifierList.Add("AppendAbilityCommand", AppendAbilityCommand);
            ModifierList.Add("AppendAbilityCommandFromBase", AppendAbilityCommandFromBase);
            ModifierList.Add("AppendAbilityCommandWithDamage", AppendAbilityCommandWithDamage);
            ModifierList.Add("DeleteAbilityCommand", DeleteAbilityCommand);

            ModifierList.Add("ModifyMaxTargets", ModifyMaxTargets);
            ModifierList.Add("ModifyTargetType", ModifyTargetType);

            BuffModifierList.Clear();

            BuffModifierList.Add("SwitchDuration", SwitchDuration);
            BuffModifierList.Add("ModifyBuffCommandDamageBonus", ModifyBuffCommandDamageBonus);
            BuffModifierList.Add("AddResourceLevelToDuration", AddResourceLevelToDuration);
            BuffModifierList.Add("SetDurationToResourceLevel", SetDurationToResourceLevel);
            BuffModifierList.Add("SwitchDamage", SwitchBuffDamage);
            BuffModifierList.Add("ResourceSwitchDamage", ResourceBuffSwitchDamage);
            BuffModifierList.Add("ResourceIncreaseCritical", ResourceBuffIncreaseCritical);
            BuffModifierList.Add("SetStackLevel", SetStackLevel);
            BuffModifierList.Add("SetEventChance", SetEventChance);
            BuffModifierList.Add("SetAuraPropagation", SetAuraPropagation);
            BuffModifierList.Add("SwitchParameters", SwitchBuffCommandParams);
            BuffModifierList.Add("ModifyCommandArmorPenScale", ModifyBuffCommandArmorPenScale);
            BuffModifierList.Add("MultiplyBuffCommandParamsByResourceLevel", MultiplyBuffCommandParamsByResourceLevel);
            BuffModifierList.Add("ModifyStacksByCareerResource", ModifyStacksByCareerResource);

            BuffModifierList.Add("AddBuffCommand", AddBuffCommand);
            BuffModifierList.Add("AppendBuffCommand", AppendBuffCommand);
            BuffModifierList.Add("DeleteBuffCommand", DeleteBuffCommand);

            BuffModifierList.Add("ModifyDamageBonus", ModifyBuffDamageBonus);
            BuffModifierList.Add("ModifyArmorPenFactor", ModifyBuffArmorPenFactor);
            BuffModifierList.Add("ModifyCriticalHitRate", ModifyBuffCriticalHitRate);
            BuffModifierList.Add("ModifyCriticalDamage", ModifyBuffCriticalDamage);
            BuffModifierList.Add("ModifyDamageType", ModifyBuffDamageType);
            BuffModifierList.Add("ModifyDefensibility", ModifyBuffDefensibility);

            BuffModifierList.Add("ShifterBuffParamBonus", ShifterBuffParamBonus);
            BuffModifierList.Add("ShifterDamageBonus", ShifterBuffDamageBonus);
            BuffModifierList.Add("FuriousReprisalSetup", FuriousReprisalSetup);
            BuffModifierList.Add("ContractDoT", ContractDoT);
        }

        #region Interface

        public static Tuple<bool, byte> PerformCheck(List<AbilityModifierCheck> myCheckList, Unit caster, Unit target, AbilityInfo abInfo)
        {
            foreach (var check in myCheckList)
            {
                for (AbilityModifierCheck myCheck = check; ; myCheck = myCheck.nextCheck)
                {
                    if (CheckList[myCheck.CommandName](caster, target, abInfo, myCheck))
                    {
#if MODIFIER_DEBUG
                        if (abInfo != null)
                            ((Player)caster).SendClientMessage($"[X] [{myCheck.PreOrPost}][{myCheck.ID},{myCheck.Sequence}] {myCheck.CommandName} - {abInfo.Name}");
#endif
                        break;
                    }

#if MODIFIER_DEBUG
                    if (abInfo != null)
                        ((Player)caster).SendClientMessage($"[  ] [{myCheck.PreOrPost}][{myCheck.ID},{myCheck.Sequence}] {myCheck.CommandName} - {abInfo.Name}");
#endif

                    if (myCheck.nextCheck == null)
                        return new Tuple<bool, byte>(false, myCheck.FailCode);
                }
            }

            return new Tuple<bool, byte>(true, 0);
        }

        public static void InvokeEffect(AbilityModifierEffect myEffect, Unit caster, AbilityInfo abInfo)
        {
            while (myEffect != null)
            {
                if (!ModifierList.ContainsKey(myEffect.ModifierCommandName))
                {
                    var player = caster as Player;
                    Log.Error("Missing ability modifier", myEffect.ModifierCommandName);
                    player?.SendClientMessage(myEffect.Entry + " " + AbilityMgr.GetAbilityNameFor(myEffect.Entry) + ": modifier effect " + myEffect.ModifierCommandName + " not found!");
                    return;
                }
                ModifierList[myEffect.ModifierCommandName](caster, abInfo, myEffect);
                myEffect = myEffect.nextMod;
            }
        }

        public static void InvokeBuffEffect(AbilityModifierEffect myEffect, Unit caster, BuffInfo buffInfo)
        {
            while (myEffect != null)
            {
                if (!BuffModifierList.ContainsKey(myEffect.ModifierCommandName))
                {
                    var player = caster as Player;
                    Log.Error("Missing buff modifier", myEffect.ModifierCommandName);
                    player?.SendClientMessage(myEffect.Entry + " " + AbilityMgr.GetAbilityNameFor(myEffect.Entry) + ": modifier effect " + myEffect.ModifierCommandName + " not found!");
                    return;
                }
                BuffModifierList[myEffect.ModifierCommandName](caster, buffInfo, myEffect);
                myEffect = myEffect.nextMod;
            }
        }

        #endregion

        #region Checks

        #region Positions

        /// <summary>
        /// Returns whether the caster is behind the target.
        /// Primary value is the angle of the back arc.
        /// </summary>
        private static bool IsBehind(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            return !target.IsObjectInFront(caster, 360 - myCheck.PrimaryValue);
        }

        private static bool IsFlanking(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            return !target.IsObjectInFront(caster, 90);
        }

        private static bool WithinJumpZ(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            return caster.Z > target.Z || target.Z - caster.Z < myCheck.PrimaryValue * 12;
        }

        #endregion

        #region Health
        private static bool HasCriticalBackstab(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            return !target.IsObjectInFront(caster, 180) && target.PctHealth < 11;
        }

        private static bool TargetHPBelow(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            return target.PctHealth < myCheck.PrimaryValue;
        }
        #endregion

        #region Crowd Control
        private static bool IsCCed(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            return target.CrowdControlType != 0 || target.StsInterface.IsImpeded();
        }

        private static bool IsImpeded(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            return target.StsInterface.IsImpeded();
        }

        private static bool CanMove(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            /*if (caster is Player && ((Player) caster).FallGuard)
                return false;*/
            return !caster.StsInterface.IsRooted();
        }
        #endregion

        #region Resources
        private static bool HasResource(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            if (caster is Player)
            {

                Player plr = caster as Player;

                if (myCheck.SecondaryValue == 0)
                    return plr.CrrInterface.CareerResource == myCheck.PrimaryValue;
                return plr.CrrInterface.HasResourceRange(myCheck.PrimaryValue, myCheck.SecondaryValue);
            }
            return false;
        }

        private static bool RequiresResource(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            return abInfo.SpecialCost > 0;
        }

        private static bool ForTheHagQueen(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            return StaticRandom.Instance.Next(100) < ((Player)caster).CrrInterface.CareerResource * 10;
        }
        #endregion

        #region Buff Management
        private static bool HasBuff(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            NewBuff buff = caster.BuffInterface.GetBuff((ushort)myCheck.PrimaryValue, null);

            return buff != null && !buff.BuffHasExpired;
        }

        private static bool MissingBuff(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            NewBuff buff = caster.BuffInterface.GetBuff((ushort)myCheck.PrimaryValue, null);
            if (buff != null && !buff.BuffHasExpired)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        private static bool TargetHasBuff(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            NewBuff buff = target.BuffInterface.GetBuff((ushort)myCheck.PrimaryValue, null);

            return buff != null && !buff.BuffHasExpired;
        }

        private static bool HasCareerBuff(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            NewBuff buff = caster.BuffInterface.GetCareerBuff(myCheck.SecondaryValue);

            return buff != null && buff.Entry == myCheck.PrimaryValue;
        }

        private static bool HasBuffOfType(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            return target.BuffInterface.HasBuffOfType((byte)myCheck.PrimaryValue);
        }

        private static bool NotImmovable(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            if (target.IsImmovable)
                return false;

            Player plrTarget = target as Player;

            if (plrTarget == null)
                return true;

            return !plrTarget.ImmuneToCC((byte)CrowdControlTypes.Root, null, 0);
        }

        #endregion

        #region Combat

        private static bool OutOfCombat(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            return !caster.CbtInterface.IsInCombat;
        }

        private static bool OutOfRvR(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            Player plr = (Player)caster;

            return plr.ScnInterface.Scenario == null && (plr.CurrentArea == null || !plr.CurrentArea.IsRvR);
        }

        private static bool HasDefended(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            return caster.CbtInterface.HasDefended(myCheck.PrimaryValue);
        }

        private static bool IsGrounded(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            Player plr = (Player)caster;
            return plr.WasGrounded;
        }

        private static bool TargetDefended(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            return caster.CbtInterface.WasDefendedAgainst(myCheck.PrimaryValue);
        }

        private static bool TargetIsCasting(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            return target is Player && target.AbtInterface.IsCasting();
        }

        private static bool OffensiveDamaging(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            return abInfo.TargetType == CommandTargetTypes.Enemy && abInfo.ConstantInfo.IsDamaging;
        }
        #endregion

        #region Relations to Target
        private static bool CasterTargetRelation(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            if (myCheck.PrimaryValue == 0)
                return caster != target;
            return caster == target;
        }

        private static bool CasterTargetSameRealm(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            if (myCheck.PrimaryValue == 0)
                return caster.Faction == 64 || caster.Realm != target.Realm;
            return caster.Realm == target.Realm && caster.Faction != 64;
        }

        private static bool TargetWithinRange(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {

            return target != null && caster.ObjectWithinRadiusFeet(target, myCheck.PrimaryValue);
        }

        private static bool HostileWithinRange(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            Unit realTarget = caster.CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ENEMY);
            return realTarget != null && !realTarget.IsDead && caster.ObjectWithinRadiusFeet(realTarget, myCheck.PrimaryValue);
        }

        private static bool TargetWithinRangeOfPet(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            Pet myPet = ((Player)caster).CrrInterface.GetTargetOfInterest() as Pet;

            if (myPet == null)
                return false;

            return myPet.IsInCastRange(target, (uint)myCheck.PrimaryValue);
        }

        private static bool TOIWithinRange(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            Player plr = caster as Player;

            if (plr == null)
                return false;

            Unit careerTarget = plr.CrrInterface.GetTargetOfInterest();

            return careerTarget != null && !careerTarget.IsDead && (myCheck.PrimaryValue == 0 || caster.ObjectWithinRadiusFeet(careerTarget, myCheck.PrimaryValue));
        }

        private static bool TargetIsPlayer(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            if (myCheck.PrimaryValue == 0)
                return !(target is Player);
            return target is Player;
        }

        private static bool TargetIsOrganic(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            if (myCheck.PrimaryValue == 0)
                return !(target is Player || target is Creature);
            return target is Player || target is Creature;
        }

        private static bool IsPrincipalTarget(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            return target == caster.CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ENEMY);
        }
        #endregion

        #region RvR

        private static bool CanDeploySiege(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            if (!(caster is Player player))
                return false;

            if (!(caster as Player).CbtInterface.IsPvp)
                return false;

            if ((caster as Player).ZoneId !=
                WorldMgr.UpperTierCampaignManager.GetActiveBattleFrontFromProgression().ZoneId)
            {
                player.SendClientMessage("You may only deploy Siege in the active zone",
                    ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                return false;
            }

            var siegeType = Siege.GetSiegeType((uint)abInfo.CommandInfo[0].PrimaryValue);

            var nearRamSpawn = player.Region.Campaign.SiegeManager.CanDeploySiege(
                (Player)caster,
                new RamSpawnFlagComparitor(player.Realm), siegeType.Value);

            var nearMerchant = player.Region.Campaign.SiegeManager.CanDeploySiege(
                (Player)caster,
                new SiegeMerchantLocationComparitor(), siegeType.Value);

            var nearFriendlyKeep = player.Region.Campaign.SiegeManager.CanDeploySiege(
                (Player)caster,
                new FriendlyKeepLocationComparitor(), siegeType.Value);

            var nearEnemyKeep = player.Region.Campaign.SiegeManager.CanDeploySiege(
                (Player)caster,
                new EnemyKeepLocationComparitor(siegeType.Value), siegeType.Value);

            if (siegeType == SiegeType.RAM)
            {
                if (nearRamSpawn == DeploymentReason.Success || nearFriendlyKeep == DeploymentReason.Success)
                    return true;
                else
                {
                    if (nearFriendlyKeep == DeploymentReason.MaximumCount || nearRamSpawn == DeploymentReason.MaximumCount)
                    {
                        player.SendClientMessage("There are too many of this type of Siege deployed", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                        return false;
                    }
                    if (nearFriendlyKeep == DeploymentReason.Range || nearRamSpawn == DeploymentReason.Range)
                    {
                        player.SendClientMessage("Must deploy at a friendly keep or near the siege deployment flag outside of your realm’s RvR camp.", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                        return false;
                    }
                    return false;
                }
            }
            if (siegeType == SiegeType.GTAOE || siegeType == SiegeType.SNIPER || siegeType == SiegeType.DIRECT)
            {
                // Deployed near enemy keep (attacking) or in defence near friendly keep
                if (nearEnemyKeep == DeploymentReason.Success || nearFriendlyKeep == DeploymentReason.Success)
                    return true;
                else
                {
                    // If deploying defensive siege - must be near the siege merchant
                    if (nearMerchant == DeploymentReason.Success)
                    {
                        return true;
                    }
                    else
                    {
                        if (nearEnemyKeep == DeploymentReason.MaximumCount)
                        {
                            player.SendClientMessage("There are too many of this type of Siege deployed", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                            return false;
                        }
                        if (nearFriendlyKeep == DeploymentReason.Range || nearEnemyKeep == DeploymentReason.Range)
                        {
                            player.SendClientMessage("Must deploy siege at keep/fort", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                            return false;
                        }
                    }
                    return false;
                }
            }
            return false;
        }
        #endregion

        private static bool IsOffensive(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            Item offHand = caster.ItmInterface.GetItemInSlot((ushort)EquipSlot.OFF_HAND);

            if (offHand?.Info != null)
                return offHand.Info.Type != (int)ItemTypes.ITEMTYPES_CHARM && offHand.Info.Type != (int)ItemTypes.ITEMTYPES_SHIELD;

            Item mainHand = caster.ItmInterface.GetItemInSlot((ushort)EquipSlot.MAIN_HAND);

            return mainHand?.Info != null && mainHand.Info.TwoHanded;
        }

        private static bool ExperimentalMode(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            Player player = caster as Player;

            return player == null || player.CrrInterface.ExperimentalModeCheckAbility(abInfo);
        }

        private static bool CanMount(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            Player player = caster as Player;

            if (player == null || (player.HeldObject != null && player.HeldObject.PreventsRide) || !player.CanMount)
            {
                player?.SendClientMessage("You can't perform that action while carrying an object", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                return false;
            }

            // Block Squig Armor
            if (player.Info.CareerLine == 8 && player.CrrInterface.CareerResource == 1)
            {
                player.SendClientMessage("You can't perform that action while in Squig Armor", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                return false;
            }
            return true;
        }

        private static bool ItemInSlot(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck mycheck)
        {
            Player plr = caster as Player;
            if (plr.ItmInterface.GetItemInSlot((ushort)mycheck.PrimaryValue) == null)
                return false;
            else
                return true;
        }

        #endregion

        #region BlockingChecks

        private static bool RemoveIfOn(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            NewBuff B = caster.BuffInterface.GetBuff((ushort)myCheck.PrimaryValue, caster);

            if (B != null)
                B.BuffHasExpired = true;

            return B == null;
        }

        private static bool CheckAllowAura(Unit caster, Unit target, AbilityInfo abInfo, AbilityModifierCheck myCheck)
        {
            return caster.BuffInterface.CanAcceptAura();
        }

        #endregion

        #region Modifiers

        private static void MoveCast(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            if (abInfo.ConstantInfo.ChannelID == 0)
            {
                abInfo.CastTime = (ushort)(abInfo.CastTime * myEffect.PrimaryValue * 0.01f);
                abInfo.CanCastWhileMoving = true;
            }
        }

        private static void ModifyCastTime(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            if (abInfo.ConstantInfo.ChannelID != 0)
                return;

            // Zero cast time.
            if (myEffect.PrimaryValue == 0)
                abInfo.CastTime = 0;

            // Cast time multiplier.
            else if (myEffect.PrimaryValue > 0 && myEffect.PrimaryValue < 100)
                abInfo.CastTime = (ushort)(abInfo.CastTime * myEffect.PrimaryValue * 0.01f);

            else
            {
                // Subtractive cast time.
                if (myEffect.PrimaryValue < 0)
                {
                    if (abInfo.CastTime > (myEffect.PrimaryValue * -1))
                        abInfo.CastTime += (ushort)myEffect.PrimaryValue;
                    else abInfo.CastTime = 0;
                }

                // Additive cast time
                else abInfo.CastTime += (ushort)myEffect.PrimaryValue;
            }

            if (abInfo.CastTime == 0)
                abInfo.CanCastWhileMoving = true;
        }

        private static void ModifyAPCostByResourceLevel(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            Player plr = caster as Player;

            if (plr == null)
                return;
            byte myResource = plr.CrrInterface.GetCurrentResourceLevel(1);

            if (abInfo.ApCost + myEffect.PrimaryValue * myResource < 0)
                abInfo.ApCost = 0;
            else
                abInfo.ApCost = (byte)(abInfo.ApCost + myEffect.PrimaryValue * myResource);
        }

        private static void ModifySpecialCost(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            if (abInfo.SpecialCost > 0)
                abInfo.SpecialCost += (short)myEffect.PrimaryValue;
        }

        private static void AddCooldownMS(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            if (abInfo.Cooldown * 1000 < -myEffect.PrimaryValue)
                abInfo.Cooldown = 0;
            else abInfo.Cooldown += (ushort)(myEffect.PrimaryValue * 0.001f);
        }

        private static void MultiplyCooldown(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            abInfo.Cooldown = (ushort)(abInfo.Cooldown * (float)(100 + myEffect.PrimaryValue) * 0.01f);
        }

        private static void MultiplyCooldownGreatweapon(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            Item myItem = caster.ItmInterface.GetItemInSlot((ushort)EquipSlot.MAIN_HAND);

            if (myItem != null && myItem.Info.TwoHanded)
                abInfo.Cooldown = (ushort)(abInfo.Cooldown * (float)(100 + myEffect.PrimaryValue) * 0.01f);
        }

        private static void SetCooldown(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            abInfo.Cooldown = (ushort)myEffect.PrimaryValue;
        }

        private static void AddAPCost(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            abInfo.ApCost = (byte)(abInfo.ApCost + myEffect.PrimaryValue);
        }

        private static void SetAPCost(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            abInfo.ApCost = (byte)myEffect.PrimaryValue;
        }

        private static void MultiplyAPCost(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            abInfo.ApCost = (byte)(abInfo.ApCost * (100 + myEffect.PrimaryValue) * 0.01f);
        }

        private static void MultiplyRange(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            abInfo.Range = (ushort)(abInfo.Range * (100 + myEffect.PrimaryValue) * 0.01f);
        }

        private static void ModifyDamageBonus(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            foreach (var cmdinfo in abInfo.CommandInfo)
            {
                for (var cmd = cmdinfo; cmd != null; cmd = cmd.NextCommand)
                {
                    if (cmd.DamageInfo == null)
                        continue;

                    if (myEffect.PrimaryValue > 0)
                        cmd.DamageInfo.DamageBonus += myEffect.PrimaryValue * 0.01f;
                    else cmd.DamageInfo.DamageReduction *= (100 + myEffect.PrimaryValue) * 0.01f;
                }
            }
        }

        private static void ModifyArmorPenFactor(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            foreach (var cmdinfo in abInfo.CommandInfo)
            {
                for (var cmd = cmdinfo; cmd != null; cmd = cmd.NextCommand)
                {
                    if (cmd.DamageInfo == null)
                        continue;

                    cmd.DamageInfo.ArmorResistPenFactor += myEffect.PrimaryValue * 0.01f;
                }
            }
        }

        private static void ModifyDamageType(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            foreach (var cmdinfo in abInfo.CommandInfo)
            {
                for (var cmd = cmdinfo; cmd != null; cmd = cmd.NextCommand)
                {
                    if (cmd.DamageInfo == null)
                        continue;

                    if (!cmd.DamageInfo.IsHeal)
                        cmd.DamageInfo.DamageType = (DamageTypes)myEffect.PrimaryValue;
                }
            }
        }

        private static void ModifyCriticalHitRate(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            foreach (var cmdinfo in abInfo.CommandInfo)
            {
                for (var cmd = cmdinfo; cmd != null; cmd = cmd.NextCommand)
                {
                    if (cmd.DamageInfo == null)
                        continue;

                    cmd.DamageInfo.CriticalHitRate += (byte)myEffect.PrimaryValue;
                }
            }
        }

        private static void ModifyCriticalDamage(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            foreach (var cmdinfo in abInfo.CommandInfo)
            {
                for (var cmd = cmdinfo; cmd != null; cmd = cmd.NextCommand)
                {
                    if (cmd.DamageInfo == null)
                        continue;

                    cmd.DamageInfo.CriticalHitDamageBonus += myEffect.PrimaryValue * 0.01f;
                }
            }
        }

        private static void ModifyDefensibility(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            foreach (var cmdinfo in abInfo.CommandInfo)
            {
                for (var cmd = cmdinfo; cmd != null; cmd = cmd.NextCommand)
                {
                    if (cmd.DamageInfo == null)
                        continue;

                    cmd.DamageInfo.Defensibility += myEffect.PrimaryValue;
                }
            }
        }

        private static void FeedingOnPain(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            if (caster.PctHealth < 76)
                abInfo.ApCost = (byte)(abInfo.ApCost * (caster.PctHealth / 25 + 1) * 0.25f);
        }

        #endregion

        #region CommandModifiers

        private static void SwitchDamage(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            AbilityCommandInfo cmd = abInfo.CommandInfo[myEffect.TargetCommandID].GetSubcommand(myEffect.TargetCommandSequence);

            if (cmd != null)
            {
                cmd.DamageInfo.MinDamage = (ushort)myEffect.PrimaryValue;
                cmd.DamageInfo.MinDamage = (ushort)myEffect.SecondaryValue;
            }
        }

        private static void ResourceSwitchDamage(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            AbilityCommandInfo cmd = abInfo.CommandInfo[myEffect.TargetCommandID].GetSubcommand(myEffect.TargetCommandSequence);

            if (cmd != null)
                cmd.DamageInfo = AbilityMgr.GetExtraDamageFor(myEffect.Affecting, (byte)myEffect.PrimaryValue, (byte)(Math.Max(0, ((Player)caster).CrrInterface.GetCurrentResourceLevel((byte)myEffect.SecondaryValue) - 1)));
        }

        private static void ModifyCommandDamageBonus(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            AbilityCommandInfo cmd = abInfo.CommandInfo[myEffect.TargetCommandID].GetSubcommand(myEffect.TargetCommandSequence);

            if (cmd != null)
            {
                if (myEffect.PrimaryValue > 0)
                    cmd.DamageInfo.DamageBonus += myEffect.PrimaryValue * 0.01f;
                else cmd.DamageInfo.DamageReduction *= (100 + myEffect.PrimaryValue) * 0.01f;
            }
        }

        private static void ModifyCommandCritChance(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            AbilityCommandInfo cmd = abInfo.CommandInfo[myEffect.TargetCommandID].GetSubcommand(myEffect.TargetCommandSequence);

            if (cmd != null)
                cmd.DamageInfo.CriticalHitRate += (byte)myEffect.PrimaryValue;
        }

        private static void SetUndefendable(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            foreach (var cmdinfo in abInfo.CommandInfo)
            {
                for (var cmd = cmdinfo; cmd != null; cmd = cmd.NextCommand)
                {
                    if (cmd.AttackingStat > 0)
                        cmd.AttackingStat = 0;

                    if (cmd.DamageInfo == null)
                        continue;

                    cmd.DamageInfo.Undefendable = true;
                }
            }
        }

        private static void SetCommandRadius(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            AbilityCommandInfo cmd = abInfo.CommandInfo[myEffect.TargetCommandID].GetSubcommand(myEffect.TargetCommandSequence);

            if (cmd != null)
                cmd.EffectRadius = (byte)myEffect.PrimaryValue;
        }

        private static void DragonGunRange(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            byte myRes = ((Player)caster).CrrInterface.CareerResource;

            AbilityCommandInfo cmd = abInfo.CommandInfo[myEffect.TargetCommandID].GetSubcommand(myEffect.TargetCommandSequence);

            switch (myRes)
            {
                case 1:
                    cmd.EffectRadius = 30;
                    break;
                case 2:
                    cmd.EffectRadius = 35;
                    break;
                case 3:
                    cmd.EffectRadius = 40;
                    break;
                case 4:
                    cmd.EffectRadius = 44;
                    break;
                case 5:
                    cmd.EffectRadius = 49;
                    break;
            }
        }

        private static void ModifyCommandArmorPen(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            AbilityCommandInfo cmd = abInfo.CommandInfo[myEffect.TargetCommandID].GetSubcommand(myEffect.TargetCommandSequence);

            if (cmd != null)
            {
                cmd.DamageInfo.MinArmorResistPen = (ushort)myEffect.PrimaryValue;
                cmd.DamageInfo.MaxArmorResistPen = (ushort)myEffect.SecondaryValue;
            }
        }

        private static void ModifyCommandArmorPenScale(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            AbilityCommandInfo cmd = abInfo.CommandInfo[myEffect.TargetCommandID].GetSubcommand(myEffect.TargetCommandSequence);

            if (cmd != null)
            {
                if (cmd.SecondaryValue == 0)
                    cmd.DamageInfo.ArmorResistPenFactor = myEffect.PrimaryValue * 0.01f;
            }
        }

        private static void MultiplyCommandRadius(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            AbilityCommandInfo cmd = abInfo.CommandInfo[myEffect.TargetCommandID].GetSubcommand(myEffect.TargetCommandSequence);

            if (cmd != null)
                cmd.EffectRadius = (byte)(cmd.EffectRadius * (100 + (ushort)myEffect.PrimaryValue) * 0.01f);
        }

        private static void ModifyAPCostByCareerResource(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            abInfo.ApCost = (byte)(abInfo.ApCost * (1f - myEffect.PrimaryValue * 0.01f * ((Player)caster).CrrInterface.CareerResource));
        }

        private static void SwitchCommandParams(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            AbilityCommandInfo cmd = abInfo.CommandInfo[myEffect.TargetCommandID].GetSubcommand(myEffect.TargetCommandSequence);

            if (cmd != null)
            {
                cmd.PrimaryValue = myEffect.PrimaryValue;
                cmd.SecondaryValue = myEffect.SecondaryValue;
            }
        }

        private static void MultiplyCommandParamByResourceLevel(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            AbilityCommandInfo cmd = abInfo.CommandInfo[myEffect.TargetCommandID].GetSubcommand(myEffect.TargetCommandSequence);

            if (cmd != null)
                cmd.PrimaryValue *= ((Player)caster).CrrInterface.GetCurrentResourceLevel((byte)myEffect.PrimaryValue);
        }

        private static void AddResourceLevelToCommandParam(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            AbilityCommandInfo cmd = abInfo.CommandInfo[myEffect.TargetCommandID].GetSubcommand(myEffect.TargetCommandSequence);

            if (cmd != null)
                cmd.PrimaryValue += ((Player)caster).CrrInterface.GetCurrentResourceLevel((byte)myEffect.PrimaryValue);
        }

        private static void ExclusiveCleanse(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            AbilityCommandInfo cmd = abInfo.CommandInfo[myEffect.TargetCommandID].GetSubcommand(myEffect.TargetCommandSequence);

            cmd.EffectRadius = 150;
            cmd.TargetType = CommandTargetTypes.Group;
            cmd.AoESource = CommandTargetTypes.Caster;
            cmd.CommandName = "CleanseDebuffType";
        }

        #region Healer Shifter Mechanic

        /// <summary>
        /// <para>Reduces the cast time of spells empowered by the healer shifter mechanic of Archmage and Shaman.</para>
        /// <para>In experimental mode, also improves the output of those spells.</para>
        /// </summary>

        private static void ShifterCastTimeBonus(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            Player plr = caster as Player;

            if (plr == null)
                return;
            byte myResource = plr.CrrInterface.CareerResource;

            if (myResource == 0)
                return;

            // Force bonus - Tranquility casts faster
            if (myEffect.PrimaryValue == 0)
            {
                if (myResource > 5)
                    return;


                abInfo.CastTime = (ushort)(abInfo.CastTime * 0.60f);
                //abInfo.ApCost = (byte) (abInfo.ApCost*0.60f);

            }

            // Tranquility bonus - Force casts faster
            else
            {
                if (myResource < 6)
                    return;


                abInfo.Level = plr.EffectiveLevel;
                abInfo.BoostLevel = plr.EffectiveLevel; // necessary for secondary effects
                abInfo.CastTime = (ushort)(abInfo.CastTime * 0.60f);

                switch (abInfo.Entry)
                {
                    // Lifetaps cast while moving if empowered
                    case 1930: // I'll Take That!
                    case 1935: // Fury of Da Green
                    case 9257: // Balance Essence
                    case 9274: // Energy of Vaul
                        abInfo.CanCastWhileMoving = true;
                        break;

                        // Damaging abilities scale AP cost instead
                        /*
                        default:
                            abInfo.ApCost = (byte)(abInfo.ApCost * 0.60f);
                            break;
                        */
                }

            }

            if (abInfo.CastTime == 0)
                abInfo.CanCastWhileMoving = true;
        }

        /// <summary>
        /// <para>Reduces the cooldown of spells empowered by the healer shifter mechanic of Archmage and Shaman.</para>
        /// </summary>
        private static void ShifterCooldownChange(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            Player plr = caster as Player;

            if (plr == null)
                return;
            byte myResource = plr.CrrInterface.CareerResource;

            if (myResource == 0)
                return;

            // Force bonus - Tranquility cools down faster
            if (myEffect.PrimaryValue == 0)
            {
                if (myResource > 5)
                    return;

                if (plr.CrrInterface.ExperimentalMode)
                    abInfo.Cooldown = (ushort)(abInfo.Cooldown * 0.6f);
            }

            // Tranquility bonus - Force cools down faster
            else
            {
                if (myResource < 6)
                    return;

                if (plr.CrrInterface.ExperimentalMode)
                    abInfo.Cooldown = (ushort)(abInfo.Cooldown * 0.6f);
            }
        }

        /// <summary>
        /// <para>In experimental mode, improves the damage scaling and spell level of spells empowered by the healer shifter mechanic of Archmage and Shaman.</para>
        /// </summary>
        private static void ShifterStatChange(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            Player plr = caster as Player;

            if (plr == null)
                return;

            /*
            if (plr.CrrInterface.ExperimentalMode && myEffect.PrimaryValue == 1 && myEffect.PreOrPost == 1)
            {
                // Check for lifesteal abilities.
                // These abilities, have reduced damage but significantly better health stealing.
                switch (abInfo.Entry)
                {
                    case 1930: // I'll Take That!
                    case 1935: // Fury of Da Green
                    case 9257: // Balance Essence
                    case 9274: // Energy of Vaul
                        foreach (var cmdinfo in abInfo.CommandInfo)
                        {
                            for (var cmd = cmdinfo; cmd != null; cmd = cmd.NextCommand)
                            {
                                if (cmd.CommandName == "StealLife")
                                {
                                    // ITT and BE gain base healing of 250% of their base damage plus Willpower bonus
                                    if (abInfo.Entry == 1930 || abInfo.Entry == 9257)
                                    {
                                        cmd.DamageInfo.MinDamage = (ushort) (cmd.LastCommand.DamageInfo.MinDamage*2.5);
                                        cmd.DamageInfo.MinDamage = (ushort) (cmd.LastCommand.DamageInfo.MinDamage*2.5);
                                        cmd.DamageInfo.StatUsed = 3;
                                        cmd.DamageInfo.StatDamageScale = 2;
                                    }

                                    // Fury of Da Green and Energy of Vaul heal for their base damage per target struck
                                    else
                                    {
                                        cmd.DamageInfo.MinDamage = 40;
                                        cmd.DamageInfo.MinDamage = 300;
                                    }
                                }

                                if (cmd.DamageInfo == null)
                                    continue;

                                cmd.DamageInfo.Defensibility -= 20;

                                if (cmd.CommandName != "StealLife")
                                {
                                    cmd.DamageInfo.NoCrits = true;
                                    cmd.DamageInfo.StatDamageScale = 0f;
                                }
                                else if (abInfo.Entry == 1930 || abInfo.Entry == 9257)
                                    cmd.DamageInfo.DamageType = DamageTypes.Healing;
                            }
                        }
                        break;
                }
            }
            */

            byte myResource = plr.CrrInterface.CareerResource;

            if (myResource == 0)
                return;

            // Force bonus - Tranquility heal value increases
            if (myEffect.PrimaryValue == 0)
            {
                if (myResource > 5 || !plr.CrrInterface.ExperimentalMode)
                    return;

                abInfo.BoostLevel = plr.EffectiveLevel;

                // Experimental - scale AM/Shaman mechanic-empowered spells by Item Stat Total
                if (myEffect.PreOrPost == 1)
                {
                    foreach (var cmdinfo in abInfo.CommandInfo)
                    {
                        for (var cmd = cmdinfo; cmd != null; cmd = cmd.NextCommand)
                        {
                            if (cmd.DamageInfo == null)
                                continue;

                            cmd.DamageInfo.UseItemStatTotal = true;
                        }
                    }
                }
            }
            // Tranquility bonus - Force effectiveness increases
            else
            {
                if (myResource < 6 || !plr.CrrInterface.ExperimentalMode)
                    return;

                abInfo.Level = plr.EffectiveLevel;
                abInfo.BoostLevel = plr.EffectiveLevel; // necessary for secondary effects

                // Experimental - scale AM/Shaman mechanic-empowered spells by Item Stat Total
                if (myEffect.PreOrPost == 1)
                {
                    // Check for lifesteal abilities.
                    // These abilities have reduced damage but significantly better health stealing.
                    switch (abInfo.Entry)
                    {
                        case 1930: // I'll Take That!
                        case 1935: // Fury of Da Green
                        case 9257: // Balance Essence
                        case 9274: // Energy of Vaul
                            foreach (var cmdinfo in abInfo.CommandInfo)
                            {
                                for (var cmd = cmdinfo; cmd != null; cmd = cmd.NextCommand)
                                {
                                    if (cmd.CommandName == "StealLife")
                                    {
                                        // ITT and BE gain base healing of 250% of their base damage plus Willpower bonus
                                        if (abInfo.Entry == 1930 || abInfo.Entry == 9257)
                                        {
                                            cmd.DamageInfo.MinDamage = (ushort)(cmd.LastCommand.DamageInfo.MinDamage * 2.5);
                                            cmd.DamageInfo.MinDamage = (ushort)(cmd.LastCommand.DamageInfo.MinDamage * 2.5);
                                            cmd.DamageInfo.StatUsed = 3;
                                            cmd.DamageInfo.StatDamageScale = 2;
                                        }

                                        // Fury of Da Green and Energy of Vaul heal for their base damage per target struck
                                        else
                                        {
                                            cmd.DamageInfo.MinDamage = 40;
                                            cmd.DamageInfo.MinDamage = 300;
                                        }
                                    }

                                    if (cmd.DamageInfo == null)
                                        continue;

                                    cmd.DamageInfo.Defensibility -= 20;

                                    if (cmd.CommandName != "StealLife")
                                    {
                                        cmd.DamageInfo.NoCrits = true;
                                        cmd.DamageInfo.StatDamageScale = 0f;
                                    }
                                    else if (abInfo.Entry == 1930 || abInfo.Entry == 9257)
                                        cmd.DamageInfo.DamageType = DamageTypes.Healing;
                                }
                            }
                            break;
                    }

                    foreach (var cmdinfo in abInfo.CommandInfo)
                    {
                        for (var cmd = cmdinfo; cmd != null; cmd = cmd.NextCommand)
                        {
                            if (cmd.DamageInfo == null)
                                continue;

                            cmd.DamageInfo.UseItemStatTotal = true;
                        }
                    }
                }
            }
        }

        // Grants additional effectiveness based on mechanic to AM/Shaman instant casts
        private static void ShifterDamageBonus(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            Player plr = caster as Player;

            if (plr == null)
                return;
            byte myResource = plr.CrrInterface.CareerResource;

            if (myResource == 0)
                return;

            AbilityCommandInfo cmd = abInfo.CommandInfo[0];

            // Force bonus - Tranquility healing increases
            if (myEffect.PrimaryValue == 0)
            {
                if (myResource > 5)
                    return;

                // Experimental - scale AM/Shaman mechanic-empowered spells by Item Stat Total
                if (plr.CrrInterface.ExperimentalMode)
                {
                    cmd.DamageInfo.DamageBonus += 0.25f;
                    cmd.DamageInfo.UseItemStatTotal = true;

                    abInfo.BoostLevel = plr.EffectiveLevel;
                }

                else
                    cmd.DamageInfo.DamageBonus += 0.05f * myResource;


            }
            // Tranquility bonus - Force damage increases
            else
            {
                if (myResource < 6)
                    return;

                // Experimental - scale AM/Shaman mechanic-empowered spells by Item Stat Total
                if (plr.CrrInterface.ExperimentalMode)
                {
                    cmd.DamageInfo.DamageBonus += 0.25f;
                    cmd.DamageInfo.UseItemStatTotal = true;

                    abInfo.Level = plr.EffectiveLevel;
                }

                else
                    cmd.DamageInfo.DamageBonus += 0.05f * (myResource - 5);
            }
        }

        #endregion

        #region Add/Remove

        /// <summary>
        /// Adds an ability command chain to this ability's list.
        /// The ability chain to add is pulled from the ability's default list.
        /// </summary>
        private static void AddAbilityCommand(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            AbilityCommandInfo cmd = AbilityMgr.GetAbilityCommand(caster, abInfo.Entry, (byte)myEffect.PrimaryValue);

            if (cmd == null)
                return;

            abInfo.AddAbilityCommand(cmd);
        }

        private static void AddAbilityCommandWithDamage(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            AbilityCommandInfo cmd = AbilityMgr.GetAbilityCommand(caster, abInfo.Entry, (byte)myEffect.PrimaryValue);

            if (cmd == null)
                return;

            cmd.DamageInfo = abInfo.CommandInfo[0].DamageInfo.Clone();

            abInfo.AddAbilityCommand(cmd);
        }

        /// <summary>
        /// Appends an ability command to an existing chain.
        /// The ability command to use is pulled from the modifier effect's default list.
        /// </summary>
        private static void AppendAbilityCommand(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            AbilityCommandInfo cmd = AbilityMgr.GetAbilityCommand(caster, myEffect.Entry, (byte)myEffect.PrimaryValue, (byte)myEffect.SecondaryValue);

            if (cmd == null)
                return;

            if (myEffect.Affecting != myEffect.Entry)
                cmd.CommandSequence = myEffect.TargetCommandSequence;

            //cmd.Entry = abInfo.Entry;

            abInfo.AppendAbilityCommand(cmd, myEffect.TargetCommandID);
        }

        private static void AppendAbilityCommandFromBase(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            AbilityCommandInfo cmd = AbilityMgr.GetAbilityCommand(caster, myEffect.Affecting, (byte)myEffect.PrimaryValue, (byte)myEffect.SecondaryValue);

            if (cmd == null)
                return;

            abInfo.AppendAbilityCommand(cmd, myEffect.TargetCommandID);
        }

        private static void AppendAbilityCommandWithDamage(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            AbilityCommandInfo cmd = AbilityMgr.GetAbilityCommand(caster, myEffect.Entry, (byte)myEffect.PrimaryValue, (byte)myEffect.SecondaryValue);

            if (cmd == null)
                return;

            cmd.CommandSequence = myEffect.TargetCommandSequence;

            abInfo.AppendAbilityCommandWithDamage(cmd, myEffect.TargetCommandID);
        }

        private static void DeleteAbilityCommand(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            abInfo.DeleteCommand((byte)myEffect.PrimaryValue, (byte)myEffect.SecondaryValue);
        }

        #endregion

        private static void ModifyMaxTargets(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            AbilityCommandInfo cmd = abInfo.CommandInfo[myEffect.TargetCommandID].GetSubcommand(myEffect.TargetCommandSequence);

            if (cmd != null)
                cmd.MaxTargets = (byte)myEffect.PrimaryValue;
        }

        private static void ModifyTargetType(Unit caster, AbilityInfo abInfo, AbilityModifierEffect myEffect)
        {
            AbilityCommandInfo cmd = abInfo.CommandInfo[myEffect.TargetCommandID].GetSubcommand(myEffect.TargetCommandSequence);

            if (cmd != null)
            {
                cmd.TargetType = (CommandTargetTypes)myEffect.PrimaryValue;
                cmd.EffectRadius = (byte)myEffect.SecondaryValue;
            }
        }

        #endregion

        #region BuffModifiers

        private static void SwitchBuffDamage(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            BuffCommandInfo cmd = buffInfo.CommandInfo[myEffect.TargetCommandID].GetSubcommand(myEffect.TargetCommandSequence);

            if (cmd != null)
            {
                cmd.DamageInfo.MinDamage = (ushort)myEffect.PrimaryValue;
                cmd.DamageInfo.MinDamage = (ushort)myEffect.SecondaryValue;
            }
        }

        private static void ModifyBuffCommandDamageBonus(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            BuffCommandInfo cmd = buffInfo.CommandInfo[myEffect.TargetCommandID].GetSubcommand(myEffect.TargetCommandSequence);

            if (cmd != null)
            {
                if (myEffect.PrimaryValue > 0)
                    cmd.DamageInfo.DamageBonus += myEffect.PrimaryValue * 0.01f;
                else cmd.DamageInfo.DamageReduction *= (100 + myEffect.PrimaryValue) * 0.01f;
            }
        }

        #region General Mods

        private static void ModifyBuffDamageBonus(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            foreach (var cmdinfo in buffInfo.CommandInfo)
            {
                for (var cmd = cmdinfo; cmd != null; cmd = cmd.NextCommand)
                {
                    if (cmd.DamageInfo == null)
                        continue;

                    if (myEffect.PrimaryValue > 0)
                        cmd.DamageInfo.DamageBonus += myEffect.PrimaryValue * 0.01f;
                    else cmd.DamageInfo.DamageReduction *= (100 + myEffect.PrimaryValue) * 0.01f;
                }
            }
        }

        private static void ModifyBuffArmorPenFactor(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            foreach (var cmdinfo in buffInfo.CommandInfo)
            {
                for (var cmd = cmdinfo; cmd != null; cmd = cmd.NextCommand)
                {
                    if (cmd.DamageInfo == null)
                        continue;

                    cmd.DamageInfo.ArmorResistPenFactor += myEffect.PrimaryValue * 0.01f;
                }
            }
        }

        private static void ModifyBuffCriticalHitRate(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            foreach (var cmdinfo in buffInfo.CommandInfo)
            {
                for (var cmd = cmdinfo; cmd != null; cmd = cmd.NextCommand)
                {
                    if (cmd.DamageInfo == null)
                        continue;

                    cmd.DamageInfo.CriticalHitRate += (byte)myEffect.PrimaryValue;
                }
            }
        }

        private static void ModifyBuffCriticalDamage(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            foreach (var cmdinfo in buffInfo.CommandInfo)
            {
                for (var cmd = cmdinfo; cmd != null; cmd = cmd.NextCommand)
                {
                    if (cmd.DamageInfo == null)
                        continue;

                    cmd.DamageInfo.CriticalHitDamageBonus += myEffect.PrimaryValue * 0.01f;
                }
            }
        }

        private static void ModifyBuffDamageType(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            foreach (var cmdinfo in buffInfo.CommandInfo)
            {
                for (var cmd = cmdinfo; cmd != null; cmd = cmd.NextCommand)
                {
                    if (cmd.DamageInfo == null)
                        continue;

                    if (!cmd.DamageInfo.IsHeal)
                        cmd.DamageInfo.DamageType = (DamageTypes)myEffect.PrimaryValue;
                }
            }
        }

        private static void ModifyBuffDefensibility(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            foreach (var cmdinfo in buffInfo.CommandInfo)
            {
                for (var cmd = cmdinfo; cmd != null; cmd = cmd.NextCommand)
                {
                    if (cmd.DamageInfo == null)
                        continue;

                    cmd.DamageInfo.Defensibility += myEffect.PrimaryValue;
                }
            }
        }

        #endregion

        private static void ModifyBuffCommandArmorPenScale(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            BuffCommandInfo cmd = buffInfo.CommandInfo[myEffect.TargetCommandID].GetSubcommand(myEffect.TargetCommandSequence);

            if (cmd != null)
            {
                if (cmd.SecondaryValue == 0)
                    cmd.DamageInfo.ArmorResistPenFactor = myEffect.PrimaryValue * 0.01f;
            }
        }

        private static void AddResourceLevelToDuration(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            //in the case for WE we will add the combo points times secondary value to add to a duration
            if (myEffect.PrimaryValue == 1)
                buffInfo.Duration += (uint)(((Player)caster).CrrInterface.GetCurrentResourceLevel(1) * myEffect.SecondaryValue);
            else
                buffInfo.Duration += ((Player)caster).CrrInterface.GetCurrentResourceLevel((byte)myEffect.SecondaryValue);
        }

        private static void SetDurationToResourceLevel(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            buffInfo.Duration = (uint)(((Player)caster).CrrInterface.GetCurrentResourceLevel(1) * myEffect.SecondaryValue);
        }

        private static void ResourceBuffSwitchDamage(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            BuffCommandInfo cmd = buffInfo.CommandInfo[myEffect.TargetCommandID].GetSubcommand(myEffect.TargetCommandSequence);

            if (cmd != null)
                cmd.DamageInfo = AbilityMgr.GetExtraDamageFor(myEffect.Affecting, (byte)myEffect.PrimaryValue, (byte)(Math.Max(0, ((Player)caster).CrrInterface.GetCurrentResourceLevel((byte)myEffect.SecondaryValue) - 1)));
        }

        private static void ResourceBuffIncreaseCritical(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            BuffCommandInfo cmd = buffInfo.CommandInfo[myEffect.TargetCommandID].GetSubcommand(myEffect.TargetCommandSequence);

            cmd.DamageInfo.CriticalHitRate += (byte)(5 * ((Player)caster).CrrInterface.GetCurrentResourceLevel((byte)myEffect.SecondaryValue));
        }

        private static void SetEventChance(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            BuffCommandInfo cmd = buffInfo.CommandInfo[myEffect.TargetCommandID].GetSubcommand(myEffect.TargetCommandSequence);

            cmd.EventChance = (byte)myEffect.PrimaryValue;
        }

        private static void SetAuraPropagation(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            buffInfo.AuraPropagation = "Foe";
            if (myEffect.PrimaryValue == 40)
            {
                buffInfo.AuraPropagation = "Foe40";
                (caster as Player).SendClientMessage("Debug : Casting Aura for40");
            }
        }

        private static void SwitchBuffCommandParams(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            BuffCommandInfo cmd = buffInfo.CommandInfo[myEffect.TargetCommandID].GetSubcommand(myEffect.TargetCommandSequence);

            if (cmd != null)
            {
                if (myEffect.SecondaryValue == 0)
                    cmd.PrimaryValue = myEffect.PrimaryValue;
                else
                {
                    cmd.SecondaryValue = myEffect.PrimaryValue;
                    cmd.TertiaryValue = myEffect.SecondaryValue;
                }
            }
        }

        private static void SetStackLevel(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            buffInfo.InitialStacks = (byte)myEffect.PrimaryValue;
            buffInfo.MaxStack = (byte)buffInfo.InitialStacks;
        }

        private static void MultiplyBuffCommandParamsByResourceLevel(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            BuffCommandInfo cmd = buffInfo.CommandInfo[myEffect.TargetCommandID].GetSubcommand(myEffect.TargetCommandSequence);

            if (cmd != null)
            {
                if (cmd.SecondaryValue == 0)
                    cmd.PrimaryValue = (int)(cmd.PrimaryValue * ((Player)caster).CrrInterface.GetCurrentResourceLevel((byte)myEffect.PrimaryValue) * (0.01f * myEffect.SecondaryValue));

                else
                {
                    cmd.SecondaryValue = (int)(cmd.SecondaryValue * ((Player)caster).CrrInterface.GetCurrentResourceLevel((byte)myEffect.PrimaryValue) * (0.01f * myEffect.SecondaryValue));
                    cmd.TertiaryValue = (int)(cmd.TertiaryValue * ((Player)caster).CrrInterface.GetCurrentResourceLevel((byte)myEffect.PrimaryValue) * (0.01f * myEffect.SecondaryValue));
                }
            }
        }

        private static void ModifyStacksByCareerResource(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            buffInfo.InitialStacks += ((Player)caster).CrrInterface.CareerResource;
            buffInfo.MaxStack = (byte)buffInfo.InitialStacks;
        }

        private static void SwitchDuration(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            if (myEffect.SecondaryValue == 0)
                buffInfo.Duration = (ushort)myEffect.PrimaryValue;
            else
            {
                int rand = StaticRandom.Instance.Next(100);
                if (rand < 34)
                    buffInfo.Duration = (ushort)myEffect.PrimaryValue;
                else if (rand < 67)
                    buffInfo.Duration = (ushort)myEffect.SecondaryValue;
            }
        }

        #region Add/Remove

        private static void AddBuffCommand(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            BuffCommandInfo cmd = AbilityMgr.GetBuffCommand(buffInfo.Entry, (byte)myEffect.PrimaryValue);

            if (cmd == null)
                return;

            buffInfo.AddBuffCommand(cmd);
        }

        private static void AppendBuffCommand(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            BuffCommandInfo cmd = AbilityMgr.GetBuffCommand(myEffect.Entry, (byte)myEffect.PrimaryValue, (byte)myEffect.SecondaryValue);

            if (cmd == null)
                return;

            cmd.CommandSequence = myEffect.TargetCommandSequence;

            buffInfo.AppendBuffCommand(cmd, myEffect.TargetCommandID);

            cmd.Entry = buffInfo.Entry;

            if (cmd.DamageInfo != null)
                cmd.DamageInfo.Entry = buffInfo.Entry;
        }

        private static void DeleteBuffCommand(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            buffInfo.DeleteCommand((byte)myEffect.PrimaryValue, (byte)myEffect.SecondaryValue);
        }

        #endregion

        #region Healer shifter mechanic

        private static void ShifterBuffParamBonus(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            Player plr = caster as Player;

            if (plr == null)
                return;
            byte myResource = plr.CrrInterface.CareerResource;

            if (myResource == 0)
                return;

            BuffCommandInfo cmd = buffInfo.CommandInfo[0];

            // Force bonus - Tranquility damage increases
            if (myEffect.PrimaryValue == 0)
            {
                if (myResource > 5)
                    return;

                cmd.SecondaryValue += (int)(cmd.SecondaryValue * myResource * 0.05f);
                cmd.TertiaryValue += (int)(cmd.TertiaryValue * myResource * 0.05f);
            }
            // Tranquility bonus - Force damage increases
            else
            {
                if (myResource < 6)
                    return;

                cmd.SecondaryValue += (int)(cmd.SecondaryValue * (myResource - 5) * 0.05f);
                cmd.TertiaryValue += (int)(cmd.TertiaryValue * (myResource - 5) * 0.05f);
            }
        }

        private static void ShifterBuffDamageBonus(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            Player plr = caster as Player;

            if (plr == null)
                return;
            byte myResource = plr.CrrInterface.CareerResource;

            if (myResource == 0)
                return;

            BuffCommandInfo cmd = buffInfo.CommandInfo[0];

            // Force bonus - Tranquility damage increases
            if (myEffect.PrimaryValue == 0)
            {
                if (myResource > 5)
                    return;

                // Experimental - scale AM/Shaman mechanic-empowered spells by Item Stat Total
                if (plr.CrrInterface.ExperimentalMode)
                {
                    cmd.DamageInfo.DamageBonus += 0.25f;
                    cmd.DamageInfo.UseItemStatTotal = true;
                }

                else
                    cmd.DamageInfo.DamageBonus += 0.05f * myResource;
            }
            // Tranquility bonus - Force damage increases
            else
            {
                if (myResource < 6)
                    return;

                // Experimental - scale AM/Shaman mechanic-empowered spells by Item Stat Total
                if (plr.CrrInterface.ExperimentalMode)
                {
                    cmd.DamageInfo.DamageBonus += 0.25f;
                    cmd.DamageInfo.UseItemStatTotal = true;
                }

                else
                    cmd.DamageInfo.DamageBonus += 0.05f * (myResource - 5);
            }
        }

        #endregion

        private static void FuriousReprisalSetup(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            buffInfo.Duration = 5;

            // Because of this we need to change ApplyCC to end on tick instead of on end, and to trigger a resend
            buffInfo.Interval = 3000;
            buffInfo.CommandInfo[0].InvokeOn = 3;

            BuffCommandInfo cmd = AbilityMgr.GetBuffCommand(buffInfo.Entry, 1);

            if (cmd == null)
                return;

            buffInfo.AddBuffCommand(cmd);
        }

        private static void ContractDoT(Unit caster, BuffInfo buffInfo, AbilityModifierEffect myEffect)
        {
            Player player = caster as Player;

            if (player == null || !player.CrrInterface.HasResource(2))
                return;

            float scaleFactor = 1f - (myEffect.PrimaryValue * 0.01f * 0.125f * ((Player)caster).CrrInterface.GetCurrentResourceLevel(0));

            buffInfo.Duration = (ushort)(buffInfo.Duration * scaleFactor);
            buffInfo.Interval = (ushort)(buffInfo.Interval * scaleFactor);
        }

        #endregion
    }
}