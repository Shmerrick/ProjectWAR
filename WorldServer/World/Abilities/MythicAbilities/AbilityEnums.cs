// Londo's Work
    public enum ComponentEventType
    {
        Applied = 1,
        Purged = 2,
        Disrupted = 3,
        Parried = 4,
        Dodged = 5,
        Blocked = 6,
        FirstTick = 7,
        LastTick = 8,
        DamageTaken = 9,
        DamageGiven = 10,
        HealTaken = 11,
        HealGiven = 12,
    }

    public enum AbilityDataType2
    {
        HEX = 1001,
        CURSE = 1002,
        CRIPPLE = 1003,
        AILMENT = 1004,
        BOLSTER = 1005,
        AUGMENTATION = 1006,
        BLESSING = 1007,
        ENCHANTMENT = 1008,
        DEBUFF = 1023,
    }

    public enum AbilityDataType
    {
        Hex = 1,
        Curse = 2,
        Cripple = 3,
        Ailment = 4,
        Bolster = 5,
        Augmentation = 6,
        Blessing = 7,
        Enchantment = 8,
        Damaging = 9,
        Healing = 10,
        Debuff = 11,
        Buff = 12,
        StatsBuff = 13,
        Offensive = 14,
        Defensive = 15,
    }

    public enum AbilityBuildInterruptType
    {
        InterruptWeapons = 47,
        InterruptMagic = 51,
        InterruptAll = 63,
        Monsters = 128,
    }

    public enum AbilityLogicOperator
    {
        None = 0,
        And = 8,
        Or = 9,
    }

    public enum AbilitySourceType
    {
        Self = 0,
        Cast = 1,
        Apply = 2,
        Watch = 3,
        EventReq = 4,
        Result = 6,
        Immunity = 7
    }

    public enum SpellFormula
    {
        Static,
        ComponentValue,
        DamagePhysical,
        DamageMagical,
        DamageRanged,
        Heal
    }

    public enum AbilityTrigger
    {
        OnEnd = 0,
        OnApply = 1,
        OnPreviousComponentApplied = 3,
        OnBuffEnded = 5,
        OnEventTriggered = 6,
        OnPreviousComponentTick = 7,
        OnPreviousComponentBuffTick= 8,
        OnPreviousComponentEndCast = 2,
        OnBuffEndedRemoved = 10
    }

    public enum AbilityOperation
    {
        Default = 0,
        FriendlyTargeted = 1,
        EnemyTargeted = 2,
        BuffGroupCounterSum = 3,
        Range = 4,
        Unk5 = 5,
        BuffGroupCount = 6,
        Unk7 = 7,
        GetGroupOnAbilityUsed = 8,
        Random = 9,
        RequirmentGroupCheck = 10,
        TargetActivatedComponent = 11,
        AbilityType = 12,
        SpellConvention = 13,
        TargetCheck = 14, //guess
        TargetIsPlayer = 15,
        MonsterType = 16,
        PreviousComponentActivated = 17,
        WoundsPercent = 18,
        TargetRace = 19,
        TargetInSameGroup = 20,
        TargetBackSide = 21,
        Unk22 = 22,
        RequirmentGroupCheck2 = 23,
        MasteryPath = 24,
        Ap = 25,
        ApPercent = 26,
        TargetRandom = 27,
        TargetIsMonster = 28,
        DamageType = 29,
        Unk30 = 30,
        InCombat = 31,
        Unk32 = 32,
        Unk33 = 33,
        TargetIsMoving = 34,
        KeepDoor = 35,
        Unk36 = 36,
        Unk37 = 37,
        TargetGreatWeapon = 38,
        Unk39 = 39,
        SiegeControl = 40,
        Unk41 = 41,
        Unk42 = 42,
        Unk43 = 43,
        EquippedInventorySlot = 44,
        Unk45 = 45,
        Unk46 = 46,
        Unk47 = 47,
        Unk48 = 48,
        GuildLevel = 49,
        Unk50 = 50,
        TargetBack = 51,
        TargetBuildTimesChanged = 52,
        Unk53 = 53,
        Unk54 = 54,
        TargetDead = 55,
        Unk56 = 56,
        Unk57 = 57,
        Unk58 = 58,
        Zone = 59,
        DoorTarget = 60,
        Unk61 = 61,
        Unk62 = 62,
        Unk63 = 63,
        Unk64 = 64,
        Unk65 = 65,
        Unk66 = 66,
        BuffAbilityIDSum = 67,
        Unk68 = 68,
        Unk69 = 69,
        ItemSlotted = 70,
        Unk71 = 71,
        Zone2 = 72,
        TimerLastTick = 73,
        AbilityLevel = 74,
        Finisher = 75,
        ScenarioCheck = 76,
        BolsterLevel = 77,
        CanMounted = 78,
        Mounted = 79,
        HasPet = 80,
        Unk81 = 81,
        LastAbilityResult = 82,
        Unk83 = 83,
        Unk84 = 84,
        Unk85 = 85,
        Unk86 = 86,
        Unk87 = 87,
        TierCheck88 = 88,
        Unk89 = 89,
    }

    public enum AbilityCondition
    {
        None = 0,
        Unk2 = 2,
        Equal = 1,
        LessThanEqual = 4,
        GreaterThanEqual = 5,
        LessThan = 6,
        GreaterThan = 7,
        FriendlyTarget = 8
    }

    public enum AbilityEvent
    {
        OnDirectDamageHit = 1,
        OnBeingHit_2 = 2,
        OnHit_3 = 3, //orig buff caster?
        OnBeingHit_4 = 4,
        OnBeingHit_Melee_5 = 5,
        OnBeingHit_All = 6,
        OnBeingDefended = 7,
        OnParry_8 = 8,
        OnBlock = 9,
        OnBeingBlocked = 10,
        OnParry_11 = 11,
        OnBeingParried = 12,
        OnBeingDodged = 14,
        OnDisrupt = 15,
        OnBeingDisrupted = 16,
        OnTargetUseAbility = 17,
        OnUseAbility = 18,
        OnCriticalHit = 19,
        OnKillingBlow = 20,
        OnEndOfCombat = 22,
        OnHeal = 23,
        OnBeingDirectHealed = 24,
        OnTargetResurrectable = 25,
        OnBeingCriticalHitDD = 26,
        OnEndCast = 27,
        OnDeath = 29,
        OnHit_4 = 31,
        OnHit_Ability = 32,
        OnDirectHeal = 34,
        OnHateGenerated = 36,
        OnPetSummoned = 37,
        OnAbilityUsedStealthed = 38,
        OnAnyDamage = 40,
        OnEnterWorld = 42,
    }

    public enum CCType
    {
        None = 0,
        Root = 1,
        Melee = 2,
        Ranged = 4,
        Magic = 8,
        Magic_1 = 16,
        AutoAttack = 32,
        AllAbilities = 64,
        DisableDefending = 128,
        Stunned = 1024,
        SquigArmor = 8192
    }

    public enum AbilityAttackType:int
    {
        NONE=0,
        MELEE=1,
        RANGED=2,
        MAGIC=3,
    }

    public enum StatChangeType
    {
        SET,
        INCREMENT,
        PERCENT
    }

    public enum ComponentOperationType
    {
        NONE = 0,
        DAMAGE = 1,
        STAT_CHANGE = 2,
        HEAL = 3,
        DAMAGE_CHANGE = 4,
        ARMOR_CHANGE = 5,
        AP_CHANGE = 6,
        HATE = 7,
        VELOCITY = 8,
        INTERRUPT = 9,
        RESSURRECT = 10,
        DISPEL_BUFF = 11,
        CC = 12,
        EVENT_LISTENER = 13,
        EFFECT_BUFF = 15,
        DEFENSIVE_STAT_CHANGE = 16,
        AP_REGEN_CHANGE = 17,
        MORALE_REGEN_CHANGE = 18,
        MORALE_CHANGE = 19,
        COOLDOWN_CHANGE = 20,
        CASTIME_CHANGE = 21,
        BONUS_TYPE_ADJUST = 22,
        KNOCKBACK = 24,
        UPDATE_COUNTER = 25,
        CATAPULT = 46,
        MONSTER_CONTROLLER = 26,
        GRANTED_ABILITY = 28,
        STEALTH = 34,
        BINDING_TELEPORT = 31,
        AUTO_ATTACK_ADJUST = 33,
        SUMMON_MOUNT = 35,
        SERVER_COMMAND = 36,
        RANK_CHANGE = 37,
        IMMUNITY = 38,
        MONSTER_FORCE_TARGET = 39,
        APPLY_ABILITY = 23,
        RECOVER_STANDARD = 42,
        DISK_UPDATE = 44,
    }

    public enum AbilityImmunityType
    {
        Damage = 1,
        Heals = 3,
        Taunt = 7,
        CC = 8,
        Root   = 12,
        Knockback = 24,
        Guard = 56,
        Knockdown = 1015,
        Snare = 1019,
        TauntChallange = 1020,
        Stuns = 1030
    }
