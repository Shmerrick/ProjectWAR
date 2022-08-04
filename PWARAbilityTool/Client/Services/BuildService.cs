using PWARAbilityTool.Client.Models;
using PWARAbilityTool.Dtos;

namespace PWARAbilityTool.Client.Services
{
    /// <summary>
    /// this class is a service to build several models from backend to frontend model.
    /// e.g: abilitySingle to abilitysinglemodel.
    /// </summary>
    public class BuildService
    {
        #region to model
        public static AbilitySingleModel buildAbilitySingleModelFromDto(AbilitySingle ab)
        {
            AbilitySingleModel abilitySingleModel = new AbilitySingleModel();
            if (ab == null)
                return abilitySingleModel;

            abilitySingleModel.abilityID = ab.abilityID;
            abilitySingleModel.AbilityType = ab.AbilityType;
            abilitySingleModel.AffectsDead = ab.AffectsDead;
            abilitySingleModel.AIRange = ab.AIRange;
            abilitySingleModel.ApCost = ab.ApCost;
            abilitySingleModel.CareerLine = ab.CareerLine;
            abilitySingleModel.CashCost = ab.CashCost;
            abilitySingleModel.CastAngle = ab.CastAngle;
            abilitySingleModel.CastTime = ab.CastTime;
            abilitySingleModel.Category = ab.Category;
            abilitySingleModel.CDcap = ab.CDcap;
            abilitySingleModel.ChannelID = ab.ChannelID;
            abilitySingleModel.Cooldown = ab.Cooldown;
            abilitySingleModel.CooldownEntry = ab.CooldownEntry;
            abilitySingleModel.EffectDelay = ab.EffectDelay;
            abilitySingleModel.EffectID = ab.EffectID;
            abilitySingleModel.effectID2 = ab.effectID2;
            abilitySingleModel.Entry = ab.Entry;
            abilitySingleModel.Flags = ab.Flags;
            abilitySingleModel.Fragile = ab.Fragile;
            abilitySingleModel.IconId = ab.IconId;
            abilitySingleModel.IgnoreCooldownReduction = ab.IgnoreCooldownReduction;
            abilitySingleModel.IgnoreGlobalCooldown = ab.IgnoreGlobalCooldown;
            abilitySingleModel.IgnoreOwnModifiers = ab.IgnoreOwnModifiers;
            abilitySingleModel.InvokeDelay = ab.InvokeDelay;
            abilitySingleModel.MasteryTree = ab.MasteryTree;
            abilitySingleModel.MinimumRank = ab.MinimumRank;
            abilitySingleModel.MinimumRenown = ab.MinimumRenown;
            abilitySingleModel.MinRange = ab.MinRange;
            abilitySingleModel.MoveCast = ab.MoveCast;
            abilitySingleModel.Name = ab.Name;
            abilitySingleModel.PointCost = ab.PointCost;
            abilitySingleModel.Range = ab.Range;
            abilitySingleModel.SpecialCost = ab.SpecialCost;
            abilitySingleModel.Specline = ab.Specline;
            abilitySingleModel.StealthInteraction = ab.StealthInteraction;
            abilitySingleModel.Time = ab.Time;
            abilitySingleModel.ToggleEntry = ab.ToggleEntry;
            abilitySingleModel.VFXTarget = ab.VFXTarget;
            abilitySingleModel.WeaponNeeded = ab.WeaponNeeded;
            return abilitySingleModel;
        }

        public static AbilityCommandModel buildAbilityCommandModelFromDto(AbilityCommand ab)
        {
            AbilityCommandModel model = new AbilityCommandModel();
            if (ab == null)
                return model;

            model.AbilityName = ab.AbilityName;
            model.AttackingStat = ab.AttackingStat;
            model.CommandID = ab.CommandID;
            model.CommandName = ab.CommandName;
            model.CommandSequence = ab.CommandSequence;
            model.EffectAngle = ab.EffectAngle;
            model.EffectRadius = ab.EffectRadius;
            model.EffectSource = ab.EffectSource;
            model.Entry = ab.Entry;
            model.FromAllTargets = ab.FromAllTargets;
            model.isDelayedEffect = ab.isDelayedEffect;
            model.MaxTargets = ab.MaxTargets;
            model.NoAutoUse = ab.NoAutoUse;
            model.PrimaryValue = ab.PrimaryValue;
            model.SecondaryValue = ab.SecondaryValue;
            model.Target = ab.Target;
            return model;
        }

        public static AbilityDamageHealsModel buildAbilityDmgHealModelFromDto(AbilityDamageHeals ab)
        {
            AbilityDamageHealsModel model = new AbilityDamageHealsModel();

            if (ab == null)
                return model;

            model.ArmorResistPenFactor = ab.ArmorResistPenFactor;
            model.CastPlayerSubID = ab.CastPlayerSubID;
            model.CastTimeDamageMult = ab.CastTimeDamageMult;
            model.DamageType = ab.DamageType;
            model.DamageVariance = ab.DamageVariance;
            model.DisplayEntry = ab.DisplayEntry;
            model.Entry = ab.Entry;
            model.HatredScale = ab.HatredScale;
            model.HealHatredScale = ab.HealHatredScale;
            model.Index = ab.Index;
            model.MaxDamage = ab.MaxDamage;
            model.MinDamage = ab.MinDamage;
            model.Name = ab.Name;
            model.NoCrits = ab.NoCrits;
            model.OverrideDefenseEvent = ab.OverrideDefenseEvent;
            model.ParentCommandID = ab.ParentCommandID;
            model.ParentCommandSequence = ab.ParentCommandSequence;
            model.PriStatMultiplier = ab.PriStatMultiplier;
            model.RessourceBuild = ab.RessourceBuild;
            model.StatDamageScale = ab.StatDamageScale;
            model.StatUsed = ab.StatUsed;
            model.Undefendable = ab.Undefendable;
            model.WeaponDamageFrom = ab.WeaponDamageFrom;
            model.WeaponDamageScale = ab.WeaponDamageScale;

            return model;
        }

        public static AbilityKnockBackInfoModel buildAbilityKnockBackInfoModelFromDto(AbilityKnockBackInfo ab)
        {
            AbilityKnockBackInfoModel model = new AbilityKnockBackInfoModel();

            if (ab == null)
                return model;

            model.Angle = ab.Angle;
            model.Entry = ab.Entry;
            model.GravMultiplier = ab.GravMultiplier;
            model.Id = ab.Id;
            model.Power = ab.Power;
            model.RangeExtension = ab.RangeExtension;
            model.Unk = ab.Unk;
            return model;
        }

        public static AbilityModifiersModel buildAbilityModModelFromDto(AbilityModifiers ab)
        {
            AbilityModifiersModel model = new AbilityModifiersModel();

            if (ab == null)
                return model;

            model.ability_modifiers_ID = ab.ability_modifiers_ID;
            model.AffectedAbility = ab.AffectedAbility;
            model.Affecting = ab.Affecting;
            model.BuffLine = ab.BuffLine;
            model.Entry = ab.Entry;
            model.ModifierCommandName = ab.ModifierCommandName;
            model.PreOrPost = ab.PreOrPost;
            model.PrimaryValue = ab.PrimaryValue;
            model.SecondaryValue = ab.SecondaryValue;
            model.Sequence = ab.Sequence;
            model.SourceAbility = ab.SourceAbility;
            model.TargetCommandID = ab.TargetCommandID;
            model.TargetCommandSequence = ab.TargetCommandSequence;
            return model;
        }

        public static AbilityModifierChecksModel buildAbilityModChecksModelFromDto(AbilityModifierChecks ab)
        {
            AbilityModifierChecksModel model = new AbilityModifierChecksModel();

            if (ab == null)
                return model;

            model.ability_modifier_checks_ID = ab.ability_modifier_checks_ID;
            model.AffectedAbility = ab.AffectedAbility;
            model.Affecting = ab.Affecting;
            model.CommandName = ab.CommandName;
            model.Entry = ab.Entry;
            model.FailCode = ab.FailCode;
            model.ID = ab.ID;
            model.PreOrPost = ab.PreOrPost;
            model.PrimaryValue = ab.PrimaryValue;
            model.SecondaryValue = ab.SecondaryValue;
            model.Sequence = ab.Sequence;
            model.SourceAbility = ab.SourceAbility;
            return model;
        }

        public static AbilityBuffCommandsModel buildAbilityBuffCommandsModelFromDto(AbilityBuffCommands ab)
        {
            AbilityBuffCommandsModel model = new AbilityBuffCommandsModel();

            if (ab == null)
                return model;

            model.BuffClassString = ab.BuffClassString;
            model.BuffLine = ab.BuffLine;
            model.CommandID = ab.CommandID;
            model.CommandName = ab.CommandName;
            model.CommandSequence = ab.CommandSequence;
            model.ConsumesStack = ab.ConsumesStack;
            model.EffectAngle = ab.EffectAngle;
            model.EffectRadius = ab.EffectRadius;
            model.EffectSource = ab.EffectSource;
            model.Entry = ab.Entry;
            model.EventChance = ab.EventChance;
            model.EventCheck = ab.EventCheck;
            model.EventCheckParam = ab.EventCheckParam;
            model.EventIDString = ab.EventIDString;
            model.InvokeOn = ab.InvokeOn;
            model.MaxTargets = ab.MaxTargets;
            model.Name = ab.Name;
            model.NoAutoUse = ab.NoAutoUse;
            model.PrimaryValue = ab.PrimaryValue;
            model.RetriggerInterval = ab.RetriggerInterval;
            model.SecondaryValue = ab.SecondaryValue;
            model.Target = ab.Target;
            model.TertiaryValue = ab.TertiaryValue;
            return model;
        }

        public static AbilityBuffInfosModel buildAbilityBuffInfosModelFromDto(AbilityBuffInfos ab)
        {
            AbilityBuffInfosModel model = new AbilityBuffInfosModel();

            if (ab == null)
                return model;

            model.AuraPropagation = ab.AuraPropagation;
            model.BuffClassString = ab.BuffClassString;
            model.CanRefresh = ab.CanRefresh;
            model.Duration = ab.Duration;
            model.EnemyEffectID = ab.EnemyEffectID;
            model.Entry = ab.Entry;
            model.FriendlyEffectID = ab.FriendlyEffectID;
            model.Group = ab.Group;
            model.Interval = ab.Interval;
            model.LeadInDelay = ab.LeadInDelay;
            model.MaxCopies = ab.MaxCopies;
            model.MaxStack = ab.MaxStack;
            model.Name = ab.Name;
            model.PersistsOnDeath = ab.PersistsOnDeath;
            model.Silent = ab.Silent;
            model.StackLine = ab.StackLine;
            model.StacksFromCaster = ab.StacksFromCaster;
            model.TypeString = ab.TypeString;
            model.UserMaxStackAsInitial = ab.UserMaxStackAsInitial;
            return model;
        }
        #endregion

        #region to dto
        public static AbilitySingle buildAbilitySingleDtoFromModel(AbilitySingleModel model)
        {
            AbilitySingle ab = new AbilitySingle();
            if (model == null)
                return ab;

            ab.abilityID = model.abilityID;
            ab.AbilityType = model.AbilityType;
            ab.AffectsDead = model.AffectsDead;
            ab.AIRange = model.AIRange;
            ab.ApCost = model.ApCost;
            ab.CareerLine = model.CareerLine;
            ab.CashCost = model.CashCost;
            ab.CastAngle = model.CastAngle;
            ab.CastTime = model.CastTime;
            ab.Category = model.Category;
            ab.CDcap = model.CDcap;
            ab.ChannelID = model.ChannelID;
            ab.Cooldown = model.Cooldown;
            ab.CooldownEntry = model.CooldownEntry;
            ab.EffectDelay = model.EffectDelay;
            ab.EffectID = model.EffectID;
            ab.effectID2 = model.effectID2;
            ab.Entry = model.Entry;
            ab.Flags = model.Flags;
            ab.Fragile = model.Fragile;
            ab.IconId = model.IconId;
            ab.IgnoreCooldownReduction = model.IgnoreCooldownReduction;
            ab.IgnoreGlobalCooldown = model.IgnoreGlobalCooldown;
            ab.IgnoreOwnModifiers = model.IgnoreOwnModifiers;
            ab.InvokeDelay = model.InvokeDelay;
            ab.MasteryTree = model.MasteryTree;
            ab.MinimumRank = model.MinimumRank;
            ab.MinimumRenown = model.MinimumRenown;
            ab.MinRange = model.MinRange;
            ab.MoveCast = model.MoveCast;
            ab.Name = model.Name;
            ab.PointCost = model.PointCost;
            ab.Range = model.Range;
            ab.SpecialCost = model.SpecialCost;
            ab.Specline = model.Specline;
            ab.StealthInteraction = model.StealthInteraction;
            ab.Time = model.Time;
            ab.ToggleEntry = model.ToggleEntry;
            ab.VFXTarget = model.VFXTarget;
            ab.WeaponNeeded = model.WeaponNeeded;
            ab.toUpdateMembers = model.ToUpdateMembers;
            return ab;
        }

        public static AbilityCommand buildAbilityCommandDtoFromModel(AbilityCommandModel model)
        {
            AbilityCommand ab = new AbilityCommand();
            if (model == null)
                return ab;

            ab.AbilityName = model.AbilityName;
            ab.AttackingStat = model.AttackingStat;
            ab.CommandID = model.CommandID;
            ab.CommandName = model.CommandName;
            ab.CommandSequence = model.CommandSequence;
            ab.EffectAngle = model.EffectAngle;
            ab.EffectRadius = model.EffectRadius;
            ab.EffectSource = model.EffectSource;
            ab.Entry = model.Entry;
            ab.FromAllTargets = model.FromAllTargets;
            ab.isDelayedEffect = model.isDelayedEffect;
            ab.MaxTargets = model.MaxTargets;
            ab.NoAutoUse = model.NoAutoUse;
            ab.PrimaryValue = model.PrimaryValue;
            ab.SecondaryValue = model.SecondaryValue;
            ab.Target = model.Target;
            ab.toUpdateMembers = model.ToUpdateMembers;
            return ab;
        }

        public static AbilityDamageHeals buildAbilityDmgHealDtoFromModel(AbilityDamageHealsModel model)
        {
            AbilityDamageHeals ab = new AbilityDamageHeals();

            if (model == null)
                return ab;

            ab.ArmorResistPenFactor = model.ArmorResistPenFactor;
            ab.CastPlayerSubID = model.CastPlayerSubID;
            ab.CastTimeDamageMult = model.CastTimeDamageMult;
            ab.DamageType = model.DamageType;
            ab.DamageVariance = model.DamageVariance;
            ab.DisplayEntry = model.DisplayEntry;
            ab.Entry = model.Entry;
            ab.HatredScale = model.HatredScale;
            ab.HealHatredScale = model.HealHatredScale;
            ab.Index = model.Index;
            ab.MaxDamage = model.MaxDamage;
            ab.MinDamage = model.MinDamage;
            ab.Name = model.Name;
            ab.NoCrits = model.NoCrits;
            ab.OverrideDefenseEvent = model.OverrideDefenseEvent;
            ab.ParentCommandID = model.ParentCommandID;
            ab.ParentCommandSequence = model.ParentCommandSequence;
            ab.PriStatMultiplier = model.PriStatMultiplier;
            ab.RessourceBuild = model.RessourceBuild;
            ab.StatDamageScale = model.StatDamageScale;
            ab.StatUsed = model.StatUsed;
            ab.Undefendable = ab.Undefendable;
            ab.WeaponDamageFrom = model.WeaponDamageFrom;
            ab.WeaponDamageScale = model.WeaponDamageScale;
            ab.ToUpdateMembers = model.ToUpdateMembers;
            return ab;
        }

        public static AbilityKnockBackInfo buildAbilityKnockBackInfoDtoFromModel(AbilityKnockBackInfoModel model)
        {
            AbilityKnockBackInfo ab = new AbilityKnockBackInfo();

            if (model == null)
                return ab;

            ab.Angle = model.Angle;
            ab.Entry = model.Entry;
            ab.GravMultiplier = model.GravMultiplier;
            ab.Id = model.Id;
            ab.Power = model.Power;
            ab.RangeExtension = model.RangeExtension;
            ab.Unk = model.Unk;
            ab.ToUpdateMembers = model.ToUpdateMembers;
            return ab;
        }

        public static AbilityModifiers buildAbilityModDtoFromModel(AbilityModifiersModel model)
        {
            AbilityModifiers ab = new AbilityModifiers();

            if (model == null)
                return ab;

            ab.ability_modifiers_ID = model.ability_modifiers_ID;
            ab.AffectedAbility = model.AffectedAbility;
            ab.Affecting = model.Affecting;
            ab.BuffLine = model.BuffLine;
            ab.Entry = model.Entry;
            ab.ModifierCommandName = model.ModifierCommandName;
            ab.PreOrPost = model.PreOrPost;
            ab.PrimaryValue = model.PrimaryValue;
            ab.SecondaryValue = model.SecondaryValue;
            ab.Sequence = model.Sequence;
            ab.SourceAbility = model.SourceAbility;
            ab.TargetCommandID = model.TargetCommandID;
            ab.TargetCommandSequence = model.TargetCommandSequence;
            ab.toUpdateMembers = model.ToUpdateMembers;
            return ab;
        }

        public static AbilityModifierChecks buildAbilityModChecksDtoFromModel(AbilityModifierChecksModel model)
        {
            AbilityModifierChecks ab = new AbilityModifierChecks();

            if (model == null)
                return ab;

            ab.ability_modifier_checks_ID = model.ability_modifier_checks_ID;
            ab.AffectedAbility = model.AffectedAbility;
            ab.Affecting = model.Affecting;
            ab.CommandName = model.CommandName;
            ab.Entry = model.Entry;
            ab.FailCode = model.FailCode;
            ab.ID = model.ID;
            ab.PreOrPost = model.PreOrPost;
            ab.PrimaryValue = model.PrimaryValue;
            ab.SecondaryValue = model.SecondaryValue;
            ab.Sequence = model.Sequence;
            ab.SourceAbility = model.SourceAbility;
            ab.toUpdateMembers = model.ToUpdateMembers;
            return ab;
        }

        public static AbilityBuffCommands buildAbilityBuffCommandsDtoFromModel(AbilityBuffCommandsModel model)
        {
            AbilityBuffCommands ab = new AbilityBuffCommands();

            if (model == null)
                return ab;

            ab.BuffClassString = model.BuffClassString;
            ab.BuffLine = model.BuffLine;
            ab.CommandID = model.CommandID;
            ab.CommandName = model.CommandName;
            ab.CommandSequence = model.CommandSequence;
            ab.ConsumesStack = model.ConsumesStack;
            ab.EffectAngle = model.EffectAngle;
            ab.EffectRadius = model.EffectRadius;
            ab.EffectSource = model.EffectSource;
            ab.Entry = model.Entry;
            ab.EventChance = model.EventChance;
            ab.EventCheck = model.EventCheck;
            ab.EventCheckParam = model.EventCheckParam;
            ab.EventIDString = model.EventIDString;
            ab.InvokeOn = model.InvokeOn;
            ab.MaxTargets = model.MaxTargets;
            ab.Name = model.Name;
            ab.NoAutoUse = model.NoAutoUse;
            ab.PrimaryValue = model.PrimaryValue;
            ab.RetriggerInterval = model.RetriggerInterval;
            ab.SecondaryValue = model.SecondaryValue;
            ab.Target = model.Target;
            ab.TertiaryValue = model.TertiaryValue;
            ab.toUpdateMembers = model.ToUpdateMembers;
            return ab;
        }

        public static AbilityBuffInfos buildAbilityBuffInfosDtoFromModel(AbilityBuffInfosModel model)
        {
            AbilityBuffInfos ab = new AbilityBuffInfos();

            if (model == null)
                return ab;

            ab.AuraPropagation = model.AuraPropagation;
            ab.BuffClassString = model.BuffClassString;
            ab.CanRefresh = model.CanRefresh;
            ab.Duration = model.Duration;
            ab.EnemyEffectID = model.EnemyEffectID;
            ab.Entry = model.Entry;
            ab.FriendlyEffectID = model.FriendlyEffectID;
            ab.Group = model.Group;
            ab.Interval = model.Interval;
            ab.LeadInDelay = model.LeadInDelay;
            ab.MaxCopies = model.MaxCopies;
            ab.MaxStack = model.MaxStack;
            ab.Name = model.Name;
            ab.PersistsOnDeath = model.PersistsOnDeath;
            ab.Silent = model.Silent;
            ab.StackLine = model.StackLine;
            ab.StacksFromCaster = model.StacksFromCaster;
            ab.TypeString = model.TypeString;
            ab.UserMaxStackAsInitial = model.UserMaxStackAsInitial;
            ab.toUpdateMembers = model.ToUpdateMembers;
            return ab;
        }
        #endregion
    }
}
