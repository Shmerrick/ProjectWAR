using System;

namespace WorldServer.World.Abilities.Components
{
    public enum DamageTypes : byte
    {
        Physical = 0,
        Spiritual = 1,
        Elemental = 2,
        Corporeal = 3,
        Healing = 4,
        RawHealing = 254,
        RawDamage = 255
    }

    public enum SubDamageTypes : byte
    {
        None = 0,
        Cleave = 1,
        Artillery = 2,
        Cannon = 3,
        Ram = 4,
        Oil = 5
    }

    public enum AbilityType
    {
        None = 0,
        Melee = 1,
        Ranged = 2,
        Verbal = 3,
        Effect = 255
    }

    public enum AbilityStealthType
    {
        Block,
        Break,
        Ignore
    }

    public enum BuffClass
    {
        Standard = 0,
        Morale = 1,
        Tactic = 2,
        Career = 3,
        /// <summary>Buff class intended to exclude the buff from all auto-removal algorithms (debolster...)</summary>
        Persist = 4,
        MaxBuffClasses = 5
    }

    public enum BuffTypes : byte
    {
        None = 0,
        Hex = 1,
        Curse = 2,
        Ailment = 4,
        Blessing = 8,
        Enchantment = 16
    }

    public enum EBuffState
    {
        Starting = 1,
        Running = 2,
        Refreshing = 3,
        Ended = 4,
        Removed = 5
    }

    public enum BuffGroups : byte
    {
        SelfClassBuff = 1,
        OtherClassBuff = 2,
        SelfClassSecondaryBuff = 3,
        Aura = 5,
        Vanity = 6,
        Resurrection = 7,
        Detaunt = 10,
        HealPotion = 20,
        StatPotion = 21,
        DefensePotion = 22,
        Caltrops = 23,
        SharedCooldown1 = 24,
        ItemProc = 30,
        HoldTheLine = 50,
        Guard = 51,
        OathFriend = 52
    }

    public enum BuffCombatEvents : byte
    {
        None = 0,
        AttackedTarget = 1,
        WasDefended = 2,
        DealingDamage = 3,
        DealtDamage = 4,
        DirectDamageDealt = 5,

        WasAttacked = 6,
        DefendedAgainst = 7,
        ShieldPass = 8,
        ReceivingDamage = 9,
        ReceivedDamage = 10,
        DirectDamageReceived = 11,

        DealingHeal = 12,
        ReceivingHeal = 13,

        DirectHealDealt = 14,
        DirectHealReceived = 15,

        OnKill = 16,
        OnDie = 17,
        OnResurrect = 18,

        AbilityStarted = 19,
        AbilityCasted = 20,

        PetEvent = 21,

        ResourceGained = 22,
        ResourceLost = 23,
        ResourceSet = 24,

        MainWeaponChanged = 25,
        ShieldChanged = 26,

        Manual = 27, // WH buff poke

        OnAcceptResurrection = 28,

        MaxEvents = 28
    }

    [Flags]
    public enum CommandTargetTypes : byte
    {
        Last = 0,
        Caster = 1,
        Ally = 2,
        AllyOrSelf = 3,
        Enemy = 4,
        CareerTarget = 5,
        Host = 6,
        AllyOrCareerTarget = 7,

        Groupmates = 16, // i.e. not caster
        Group = 17, // including caster
        GroupedAlly = 18,
        WithinGroup = 19,

        EventInstigator = 32,

        Siege = 64,
        SiegeCannon = 68,
        NPCAlly = 69
    };

    public enum WeaponRequirements : byte
    {
        None = 0,
        MainHand = 1,
        OffHand = 2,
        Ranged = 3,
        TwoHander = 4,
        DualWield = 5,
        Shield = 6
    }

    public enum WeaponDamageContribution : byte
    {
        None = 0,
        MainHand = 1,
        OffHand = 2,
        Ranged = 3,
        DualWield = 4,
        MainAndRanged = 5
    }

    public enum GameBuffs
    {
        Unstoppable = 402,
        Immovable = 408,
        Chicken = 14024,
        BattleFatigue = 14025,
        BolsterBase = 14312,
        FieldOfGlory = 14318,
        ResSickness = 14975,
        Quitter = 17051,
        AgainstAllOdds = 24658,
        ResourceCarrier = 24698,
        Rationing = 24796,
        Interaction = 60000
    }
}