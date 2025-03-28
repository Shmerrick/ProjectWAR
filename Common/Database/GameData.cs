﻿using System;

namespace GameData
{
    public enum ObjectType
    {
        TYPEID_OBJECT = 1,
        TYPEID_GAMEOBJECT = 2,
        TYPEID_UNIT = 4,
        TYPEID_PLAYER = 8,
        TYPEID_PET = 16,
        TYPEID_ITEM = 32,
        TYPEID_CREATURE = 64
    };

    [Flags]
    public enum ItemFlags
    {
        None = 0x0,
        ColorOverride = 0x1,
        Trophy = 0x3,
        Heraldry = 0x4,
        AltAppearance = 0x20,
        SecColorExpansion = 0x40,
        PriColorExpansion = 0x80
    }

    public enum QuestCompletion
    {
        QUESTCOMPLETION_DONE = 3,
        QUESTCOMPLETION_ABANDONED = 2,
        QUESTCOMPLETION_PENDING = 1,
        QUESTCOMPLETION_OFFER = 0
    };

    public enum AppealAuthor
    {
        APPEALAUTHOR_PLAYER = 0,
        APPEALAUTHOR_CS = 1
    };

    public enum AbilityResult
    {
        ABILITYRESULT_OK = 0,
        ABILITYRESULT_FAILED = 1,
        ABILITYRESULT_INTERRUPTED = 3,
        ABILITYRESULT_BLOCK = 4,
        ABILITYRESULT_PARRY = 5,
        ABILITYRESULT_EVADE = 6,
        ABILITYRESULT_DISRUPT = 7,
        ABILITYRESULT_MISS = 8,
        ABILITYRESULT_CRITICAL = 9,
        ABILITYRESULT_IMMUNE = 10,
        ABILITYRESULT_OUTOFRANGE = 64,
        ABILITYRESULT_NOT_VISIBLE = 62,
        ABILITYRESULT_TOOCLOSE = 65,
        ABILITYRESULT_ILLEGALTARGET = 66,
        ABILITYRESULT_QUEUED = 67,
        ABILITYRESULT_DISABLED = 68,
        ABILITYRESULT_DEAD = 69,
        ABILITYRESULT_AP = 70,
        ABILITYRESULT_MORALE = 71,
        ABILITYRESULT_NOTREADY = 72,
        ABILITYRESULT_NEEDRANGED = 73,
        ABILITYRESULT_NOTVISIBLECLIENT = 74,
        ABILITYRESULT_COUNTERED = 75,
        ABILITYRESULT_TARGET_DEAD = 76,
        ABILITYRESULT_OUT_OF_ARC = 77,
        ABILITYRESULT_ILLEGALTARGET_DEAD = 78,
        ABILITYRESULT_ILLEGALTARGET_CANT_USE_SELF = 79,
        ABILITYRESULT_ILLEGALTARGET_NOT_ATTACKABLE = 80,
        ABILITYRESULT_ILLEGALTARGET_RETREATING = 81,
        ABILITYRESULT_ILLEGALTARGET_SAME_REALM = 82,
        ABILITYRESULT_YOU_NO_REALM = 83,
        ABILITYRESULT_CANT_ATTACK_RIDING = 84,
        ABILITYRESULT_ACCOUNT_CANT_ATTACK = 85,
        ABILITYRESULT_ILLEGALTARGET_NO_REALM = 86,
        ABILITYRESULT_ILLEGALTARGET_NOT_PVP_FLAGGED = 87,
        ABILITYRESULT_YOU_TEMP_CANT_ATTACK = 88,
        ABILITYRESULT_ILLEGALTARGET_TEMP_PVP_IMMUNE = 89,
        ABILITYRESULT_ILLEGALTARGET_BAD_DISTANCE = 90,
        ABILITYRESULT_ILLEGALTARGET_IN_YOUR_ALLIANCE = 91,
        ABILITYRESULT_ILLEGALTARGET_HOME_AREA = 92,
        ABILITYRESULT_ILLEGALTARGET_NOT_ENEMY = 93,
        ABILITYRESULT_ILLEGALTARGET_NOT_ALLY = 94,
        ABILITYRESULT_ILLEGALTARGET_NOT_PET = 95,
        ABILITYRESULT_NEED_MELEE_WEAPON = 96,
        ABILITYRESULT_WRONG_WEAPON_TYPE = 97,
        ABILITYRESULT_CANT_USE_STEALTH = 98,
        ABILITYRESULT_CANT_USE_MOUNTED = 99,
        ABILITYRESULT_ILLEGALTARGET_NOT_DEAD_ALLY = 106
    };

    public enum PetCommand
    {
        Stay = 1,
        Follow = 2,
        Passive = 3,
        Defensive = 4,
        Aggressive = 5,
        Attack = 6,
        Release = 7,
        AbilityCast = 8,
        Autocast = 9
    };

    public enum TargetObjectTypes
    {
        TARGETOBJECTTYPES_TARGET_OBJECT_ENEMY_NON_PLAYER = 6,
        TARGETOBJECTTYPES_TARGET_OBJECT_ALLY_PLAYER = 3,
        TARGETOBJECTTYPES_TARGET_OBJECT_ENEMY_PLAYER = 5,
        TARGETOBJECTTYPES_TARGET_OBJECT_ALLY_NON_PLAYER = 4,
        TARGETOBJECTTYPES_TARGET_OBJECT_NONE = 0,
        TARGETOBJECTTYPES_TARGET_OBJECT_STATIC_ATTACKABLE = 8,
        TARGETOBJECTTYPES_TARGET_OBJECT_STATIC = 7,
        TARGETOBJECTTYPES_TARGET_OBJECT_SELF = 1
    };

    public enum AdvanceType
    {
        ADVANCETYPE_NONE = 0,
        ADVANCETYPE_ITEM = 1,
        ADVANCETYPE_SKILL = 5,
        ADVANCETYPE_SPEC = 6,
        ADVANCETYPE_TACTIC_SLOT = 4,
        ADVANCETYPE_STAT = 3,
        ADVANCETYPE_ABILITY = 2
    };

    public enum LootRoll
    {
        LOOTROLL_PASS = 2,
        LOOTROLL_INVALID = -1,
        LOOTROLL_TIME_UNTIL_AUTO_ROLL = 60,
        LOOTROLL_NEED = 0,
        LOOTROLL_GREED = 1
    };

    public enum CityId
    {
        CITYID_EMPIRE = 3,
        CITYID_DWARF = 1,
        CITYID_GREENSKIN = 2,
        CITYID_HIGHELF = 5,
        CITYID_CHAOS = 4,
        CITYID_DARKELF = 6
    };

    public enum HotSpotSize
    {
        HOTSPOTSIZE_NONE = 0,
        HOTSPOTSIZE_LARGE = 3,
        HOTSPOTSIZE_MEDIUM = 2,
        HOTSPOTSIZE_SMALL = 1
    };

    public enum URLs
    {
        URLS_URL_ACCOUNT_MANAGEMENT = 2,
        URLS_URL_MAIN_SITE = 0,
        URLS_URL_KNOWLEDGE_BASE = 1,
        URLS_URL_ACCOUNT_UPGRADE = 3
    };

    public enum CraftingSuccessChance
    {
        CRAFTINGSUCCESSCHANCE_MEDIUM = 2,
        CRAFTINGSUCCESSCHANCE_INVALID = 0,
        CRAFTINGSUCCESSCHANCE_LOW = 1,
        CRAFTINGSUCCESSCHANCE_HIGH = 3
    };

    public enum SiegeFireMode
    {
        SIEGEFIREMODE_GOLF = 4,
        SIEGEFIREMODE_PLAYER_TARGET = 1,
        SIEGEFIREMODE_DROP = 3,
        SIEGEFIREMODE_SNIPER = 2,
        SIEGEFIREMODE_SCORCH = 5
    };

    public enum CraftingBonusRef
    {
        CRAFTINGBONUSREF_NONE = 0,
        CRAFTINGBONUSREF_TYPE = 8,
        CRAFTINGBONUSREF_DESTROY_ON_FAIL = 15,
        CRAFTINGBONUSREF_CRAFTING_FAMILY = 5
    };

    public enum SiegeObjectState
    {
        Empty = 0,
        Building = 1,
        Ready = 2,
        InUse = 3,
        Repairing = 4,
        Destroyed = 5
    };

    public enum SiegeControlType
    {
        Release = 0,
        Leader = 1,
        Helper = 2
    }

    public enum WeaponStance
    {
        Melee = 0,
        Range = 1,
        Sheathed = 2,
        Standard = 4
    }

    public enum KeepUpgradeStatus
    {
        KEEPUPGRADESTATUS_NOT_PURCHASED,
        KEEPUPGRADESTATUS_PURCHASED,
        KEEPUPGRADESTATUS_CANCELLED,
        KEEPUPGRADESTATUS_MATURED,
        KEEPUPGRADESTATUS_MATURED_NOT_FUNDED,
        KEEPUPGRADESTATUS_PURCHASED_NOT_FUNDED,
        KEEPUPGRADESTATUS_DECAYED,
        KEEPUPGRADESTATUS_NOT_ENOUGH_MONEY_TO_PURCHASE,
    }

    public enum SetBonusTypes
    {
        BONUSTYPES_SETBONUS_NONE = 0,
        BONUSTYPES_SETBONUS_MUNDANE = 1,
        BONUSTYPES_SETBONUS_MAGIC = 2,
        BONUSTYPES_SETBONUS_USE = 3,
        BONUSTYPES_SETBONUS_PROC = 4,
        BONUSTYPES_SETBONUS_CONTINUOUS = 5,
        BONUSTYPES_SETBONUS_NUM_TYPES = 12,
    }

    public enum Stats
    {
        //===================================
        // BEGIN BASE STATS
        //===================================

        None = 0,
        Strength = 1,
        Agility = 2,
        Willpower = 3,
        Toughness = 4,
        Wounds = 5,
        Initiative = 6,
        WeaponSkill = 7,
        BallisticSkill = 8,
        Intelligence = 9,

        // Defensive stats - NOT USED.
        BlockSkill = 10,

        ParrySkill = 11,
        EvadeSkill = 12,
        DisruptSkill = 13,

        // Resists
        SpiritResistance = 14,

        ElementalResistance = 15,
        CorporealResistance = 16,

        BaseStatCount = 21,

        //===================================
        // END BASE STATS
        //===================================

        // Damage
        IncomingDamage = 22,

        IncomingDamagePercent = 23, // M
        OutgoingDamage = 24,
        OutgoingDamagePercent = 25, // M

        Armor = 26,
        Velocity = 27, // M

        // THESE ARE THE ONES THAT ARE USED
        Block = 28,

        Parry = 29,
        Evade = 30,
        Disrupt = 31,

        ActionPointRegen = 32, // M
        MoraleRegen = 33, // M
        Cooldown = 34,
        BuildTime = 35, // M
        CriticalDamage = 36, // M
        Range = 37,
        AutoAttackSpeed = 38, // M
        Radius = 39,
        AutoAttackDamage = 40,
        ActionPointCost = 41, // M

        CriticalHitRate = 42,
        CriticalDamageTaken = 43,

        EffectResist = 44,
        EffectBuff = 45,
        MinimumRange = 46,
        DamageAbsorb = 47,

        // Setback
        SetbackChance = 48, // M

        SetbackValue = 49,

        // Mob stuff
        XPWorth = 50,

        RenownWorth = 51,
        InfluenceWorth = 52,
        MonetaryWorth = 53,
        AggroRadius = 54,

        TargetDuration = 55,
        Specialization = 56,

        GoldLooted = 57,
        XpReceived = 58,

        // Trade skills
        Butchering = 59,

        Scavenging = 60,
        Cultivation = 61,
        Apothecary = 62,
        TalismanMaking = 63,
        Salvaging = 64,

        // Stealth
        Stealth = 65,

        StealthDetection = 66,

        // Hate
        HateCaused = 67, // M

        HateReceived = 68, // M

        OffhandProcChance = 69, // M
        OffhandDamage = 70,

        RenownReceived = 71,
        InfluenceReceived = 72,

        DismountChance = 73,
        Gravity = 74,
        LevitationHeight = 75,

        MeleeCritRate = 76,
        RangedCritRate = 77,
        MagicCritRate = 78,

        HealthRegen = 79, // M

        MeleePower = 80,
        RangedPower = 81,
        MagicPower = 82,

        ArmorPenetrationReduction = 83,
        CriticalHitRateReduction = 84,

        // Strikethrough
        BlockStrikethrough = 85,

        ParryStrikethrough = 86,
        EvadeStrikethrough = 87,
        DisruptStrikethrough = 88,

        HealCritRate = 89,

        MaxActionPoints = 90, // RB   4/17/2016

        Mastery1Bonus = 91, // RB   4/9/2016
        Mastery2Bonus = 92, // RB   4/9/2016
        Mastery3Bonus = 93, // RB   4/9/2016
        HealingPower = 94,
        InteractTime = 95,

        //own hope they work
        OutgoingHealPercent = 100,

        SnareDuration = 101,
        KnockdownDuration = 102,
        IncomingHealPercent = 103,
        IncomingMeleeDamage = 104,
        IncomingRangedDamage = 105,
        IncomingMagicDamage = 106,

        ArmorPenetration = 107,

        MaxStatCount = 108
    };

    public static class StatsExtensions
    {
        public static Stats GetStatPowerType(Stats stat)
        {
            switch (stat)
            {
                case Stats.Strength:
                    return Stats.MeleePower;

                case Stats.BallisticSkill:
                    return Stats.RangedPower;

                case Stats.Willpower:
                    return Stats.HealingPower;

                case Stats.Intelligence:
                    return Stats.MagicPower;

                default:
                    return Stats.None;
            }
        }

        public static Stats GetStatCriticalType(Stats stat)
        {
            switch (stat)
            {
                case Stats.Strength:
                    return Stats.MeleeCritRate;

                case Stats.BallisticSkill:
                    return Stats.RangedCritRate;

                case Stats.Willpower:
                    return Stats.HealCritRate;

                case Stats.Intelligence:
                    return Stats.MagicCritRate;

                default:
                    return Stats.CriticalHitRate;
            }
        }
    }

    public enum ConType
    {
        CONTYPE_DANGEROUS = 6,
        CONTYPE_EASY = 3,
        CONTYPE_NO_LEVEL = 0,
        CONTYPE_EFFORTLESS = 2,
        CONTYPE_TRIVIAL = 1,
        CONTYPE_EQUAL = 4,
        CONTYPE_FRIENDLY = 8,
        CONTYPE_DEADLY = 7,
        CONTYPE_CHALLENGING = 5
    };

    public enum Salvaging
    {
        SALVAGING_DIFFICULT = 3,
        SALVAGING_EASY = 1,
        SALVAGING_IMPOSSIBLE = 4,
        SALVAGING_CHALLENGING = 2,
        SALVAGING_TRIVAL = 0
    };

    public enum CooldownType
    {
        COOLDOWNTYPE_GLOBAL = 0,
        COOLDOWNTYPE_MORALE = 1
    };

    public enum PQData
    {
        PQDATA_PQ_SACK_BLUE = 3,
        PQDATA_PQ_SACK_SILVER = 5,
        PQDATA_PQ_SACK_NONE = 0,
        PQDATA_PQ_SACK_UNKNOWN = 7,
        PQDATA_PQ_SACK_PURPLE = 4,
        PQDATA_PQ_SACK_GOLD = 6,
        PQDATA_PQ_SACK_GREEN = 2,
        PQDATA_PQ_SACK_WHITE = 1
    };

    public enum Races
    {
        RACES_DARK_ELF = 5,
        RACES_HIGH_ELF = 4,
        RACES_EMPIRE = 6,
        RACES_DWARF = 1,
        RACES_GOBLIN = 3,
        RACES_CHAOS = 7,
        RACES_ORC = 2
    };

    public enum SupplyEvent
    {
        Reset,
        OwnershipChanged,
        ZoneActiveStatusChanged
    }

    public enum CreatureTypes : byte
    {
        UNCLASSIFIED = 0,
        ANIMALS_BEASTS = 1,
        ANIMALS_CRITTER = 2,
        ANIMALS_INSECTS_ARACHNIDS = 3,
        ANIMALS_LIVESTOCK = 4,
        ANIMALS_REPTILES = 5,
        DAEMONS_KHORNE = 6,
        DAEMONS_NURGLE = 7,
        DAEMONS_SLAANESH = 8,
        DAEMONS_TZEENTCH = 9,
        DAEMONS_UNMARKED = 10,
        HUMANOIDS_BEASTMEN = 11,
        HUMANOIDS_DARK_ELVES = 12,
        HUMANOIDS_DWARFS = 13,
        HUMANOIDS_ELVES = 14,
        HUMANOIDS_GREENSKINS = 15,
        HUMANOIDS_HUMANS = 16,
        HUMANOIDS_OGRES = 17,
        HUMANOIDS_SKAVEN = 18,
        MONSTERS_CHAOS_BREEDS = 19,
        MONSTERS_DRAGONOIDS = 20,
        MONSTERS_GIANTS = 21,
        MAMONSTERS_GICAL_BEASTS = 22,
        MONSTERS_TROLLS = 23,
        PLANTS_FOREST_SPIRITS = 24,
        UNDEAD_CONSTRUCTS = 25,
        UNDEAD_GREATER_UNDEAD = 26,
        UNDEAD_SKELETONS = 27,
        UNDEAD_SPIRITS = 28,
        UNDEAD_WIGHTS = 29,
        UNDEAD_ZOMBIES = 30,
        ANIMALS_BIRDS = 31,
        SIEGE = 32,
    };

    public enum CreatureSubTypes : byte
    {
        UNCLASSIFIED = 0,
        ANIMALS_BEASTS_BASILISK = 1,
        ANIMALS_BEASTS_BEAR = 2,
        ANIMALS_BEASTS_BOAR = 3,
        ANIMALS_BEASTS_GIANT_BAT = 4,
        ANIMALS_BEASTS_GREAT_CAT = 5,
        ANIMALS_BEASTS_HOUND = 6,
        ANIMALS_BEASTS_RHINOX = 7,
        ANIMALS_BEASTS_WOLF = 8,
        ANIMALS_CRITTER_BAT = 9,
        ANIMALS_CRITTER_BIRD = 10,
        ANIMALS_CRITTER_CRAB = 11,
        ANIMALS_CRITTER_DEER = 12,
        ANIMALS_CRITTER_HARE = 13,
        ANIMALS_CRITTER_LIZARD = 14,
        ANIMALS_CRITTER_MAGGOT = 15,
        ANIMALS_CRITTER_RAT = 16,
        ANIMALS_CRITTER_SPIDER = 17,
        ANIMALS_INSECTS_ARACHNIDS_GIANT_SCARAB = 18,
        ANIMALS_INSECTS_ARACHNIDS_GIANT_SCORPION = 19,
        ANIMALS_INSECTS_ARACHNIDS_GIANT_SPIDER = 20,
        ANIMALS_INSECTS_ARACHNIDS_TOMB_SWARM = 21,
        ANIMALS_LIVESTOCK_CAT = 22,
        ANIMALS_LIVESTOCK_CHICKEN = 23,
        ANIMALS_LIVESTOCK_COW = 24,
        ANIMALS_LIVESTOCK_DOG = 25,
        ANIMALS_LIVESTOCK_HORSE = 26,
        ANIMALS_LIVESTOCK_PIG = 27,
        ANIMALS_LIVESTOCK_SHEEP = 28,
        ANIMALS_REPTILES_COLD_ONE = 29,
        ANIMALS_REPTILES_GIANT_LIZARD = 30,
        DAEMONS_KHORNE_BLOODBEAST = 31,
        DAEMONS_KHORNE_BLOODLETTER = 32,
        DAEMONS_KHORNE_BLOODTHIRSTER = 33,
        DAEMONS_KHORNE_FLESH_HOUND = 34,
        DAEMONS_KHORNE_JUGGERNAUT_OF_KHORNE = 35,
        DAEMONS_NURGLE_GREAT_UNCLEAN_ONE = 36,
        DAEMONS_NURGLE_NURGLING = 37,
        DAEMONS_NURGLE_PLAGUEBEARER = 38,
        DAEMONS_NURGLE_PLAGUEBEAST = 39,
        DAEMONS_NURGLE_SLIME_HOUND = 40,
        DAEMONS_SLAANESH_DAEMONETTE = 41,
        DAEMONS_SLAANESH_FIEND = 42,
        DAEMONS_SLAANESH_KEEPER_OF_SECRETS = 43,
        DAEMONS_TZEENTCH_FIREWYRM = 44,
        DAEMONS_TZEENTCH_FLAMER = 45,
        DAEMONS_TZEENTCH_HORROR = 46,
        DAEMONS_TZEENTCH_LORD_OF_CHANGE = 47,
        DAEMONS_TZEENTCH_SCREAMER = 48,
        DAEMONS_TZEENTCH_WATCHER = 49,
        DAEMONS_UNMARKED_DAEMONS_CHAOS_FURY = 50,
        DAEMONS_UNMARKED_DAEMONS_CHAOS_HOUND = 51,
        DAEMONS_UNMARKED_DAEMONS_CHAOS_SPAWN = 52,
        DAEMONS_UNMARKED_DAEMONS_DAEMON_PRINCE = 53,
        DAEMONS_UNMARKED_DAEMONS_DAEMONVINE = 54,
        DAEMONS_UNMARKED_DAEMONS_WALKER = 55,
        HUMANOIDS_BEASTMEN_BESTIGOR = 56,
        HUMANOIDS_BEASTMEN_BRAY_SHAMAN = 57,
        HUMANOIDS_BEASTMEN_DOOMBULL = 58,
        HUMANOIDS_BEASTMEN_GOR = 59,
        HUMANOIDS_BEASTMEN_UNGOR = 60,
        HUMANOIDS_DARK_ELVES_BLACK_GUARD = 61,
        HUMANOIDS_DARK_ELVES_DARK_ELF = 62,
        HUMANOIDS_DARK_ELVES_DISCIPLE_OF_KHAINE = 63,
        HUMANOIDS_DARK_ELVES_SORCERESS = 64,
        HUMANOIDS_DARK_ELVES_WITCH_ELVES = 65,
        HUMANOIDS_DWARFS_DWARF = 66,
        HUMANOIDS_DWARFS_ENGINEER = 67,
        HUMANOIDS_DWARFS_HAMMERER = 68,
        HUMANOIDS_DWARFS_IRONBREAKER = 69,
        HUMANOIDS_DWARFS_RUNEPRIEST = 70,
        HUMANOIDS_DWARFS_SLAYER = 71,
        HUMANOIDS_ELVES_ARCHMAGE = 72,
        HUMANOIDS_ELVES_HIGH_ELF = 73,
        HUMANOIDS_ELVES_SHADOW_WARRIOR = 74,
        HUMANOIDS_ELVES_SWORDMASTER = 75,
        HUMANOIDS_ELVES_WHITE_LION = 76,
        HUMANOIDS_GREENSKINS_BLACK_ORC = 77,
        HUMANOIDS_GREENSKINS_CHOPPA = 78,
        HUMANOIDS_GREENSKINS_GNOBLAR = 79,
        HUMANOIDS_GREENSKINS_GOBLIN = 80,
        HUMANOIDS_GREENSKINS_NIGHT_GOBLIN = 81,
        HUMANOIDS_GREENSKINS_ORC = 82,
        HUMANOIDS_GREENSKINS_SAVAGE_ORC = 83,
        HUMANOIDS_GREENSKINS_SHAMAN = 84,
        HUMANOIDS_GREENSKINS_SNOTLING = 85,
        HUMANOIDS_GREENSKINS_SQUIG = 86,
        HUMANOIDS_GREENSKINS_SQUIG_HERDER = 87,
        HUMANOIDS_HUMANS_BRIGHT_WIZARD = 88,
        HUMANOIDS_HUMANS_CHAOS = 89,
        HUMANOIDS_HUMANS_CHOSEN = 90,
        HUMANOIDS_HUMANS_EMPIRE = 91,
        HUMANOIDS_HUMANS_GHOUL = 92,
        HUMANOIDS_HUMANS_HUMAN = 93,
        HUMANOIDS_HUMANS_KNIGHT_OF_THE_BLAZING_SUN = 94,
        HUMANOIDS_HUMANS_MAGUS = 95,
        HUMANOIDS_HUMANS_MARAUDER = 96,
        HUMANOIDS_HUMANS_WARRIOR_PRIEST = 97,
        HUMANOIDS_HUMANS_WITCH_HUNTER = 98,
        HUMANOIDS_HUMANS_ZEALOT = 99,
        HUMANOIDS_OGRES_GORGER = 100,
        HUMANOIDS_OGRES_OGRE = 101,
        HUMANOIDS_OGRES_OGRE_BULL = 102,
        HUMANOIDS_OGRES_OGRE_TYRANT = 103,
        HUMANOIDS_OGRES_YHETEE = 104,
        HUMANOIDS_SKAVEN_RAT_OGRE = 105,
        HUMANOIDS_SKAVEN_SKAVEN = 106,
        MONSTERS_CHAOS_BREEDS_CENTIGOR = 107,
        MONSTERS_CHAOS_BREEDS_CHAOS_MUTANT = 108,
        MONSTERS_CHAOS_BREEDS_DRAGON_OGRE = 109,
        MONSTERS_CHAOS_BREEDS_FLAYERKIN = 110,
        MONSTERS_CHAOS_BREEDS_HARPY = 111,
        MONSTERS_CHAOS_BREEDS_MAGGOT = 112,
        MONSTERS_CHAOS_BREEDS_MINOTAUR = 113,
        MONSTERS_CHAOS_BREEDS_TUSKGOR = 114,
        MONSTERS_DRAGONOID_DRAGON = 115,
        MONSTERS_DRAGONOIDS_HYDRA = 116,
        MONSTERS_DRAGONOIDS_WYVERN = 117,
        MONSTERS_GIANTS_CHAOS_GIANT = 118,
        MONSTERS_GIANTS_GIANT = 119,
        MONSTERS_MAGICAL_BEASTS_COCKATRICE = 120,
        MONSTERS_MAGICAL_BEASTS_GRIFFON = 121,
        MONSTERS_MAGICAL_BEASTS_IMP = 122,
        MONSTERS_MAGICAL_BEASTS_MANTICORE = 123,
        MONSTERS_MAGICAL_BEASTS_PEGASUS = 124,
        MONSTERS_MAGICAL_BEASTS_UNICORN = 125,
        MONSTERS_TROLLS_CHAOS_TROLL = 126,
        MONSTERS_TROLLS_RIVER_TROLL = 127,
        MONSTERS_TROLLS_STONE_TROLL = 128,
        MONSTERS_TROLLS_TROLL = 129,
        PLANTS_FOREST_SPIRITS_DRYAD = 130,
        PLANTS_FOREST_SPIRITS_SPITE = 131,
        PLANTS_FOREST_SPIRITS_TREEKIN = 132,
        PLANTS_FOREST_SPIRITS_TREEMAN = 133,
        UNDEAD_CONSTRUCTS_ASP_BONE_CONSTRUCT = 134,
        UNDEAD_CONSTRUCTS_BONE_GIANT = 135,
        UNDEAD_CONSTRUCTS_CONSTRUCT = 136,
        UNDEAD_CONSTRUCTS_LIVING_ARMOR = 137,
        UNDEAD_CONSTRUCTS_SCARAB_BONE_CONSTRUCT = 138,
        UNDEAD_CONSTRUCTS_TOMB_SCORPION = 139,
        UNDEAD_CONSTRUCTS_USHABTI = 140,
        UNDEAD_CONSTRUCTS_WINGED_NIGHTMARE = 141,
        UNDEAD_GREATER_UNDEAD_LICHE = 142,
        UNDEAD_GREATER_UNDEAD_PRESERVED_DEAD = 143,
        UNDEAD_GREATER_UNDEAD_VAMPIRE = 144,
        UNDEAD_SKELETONS_CARRION = 145,
        UNDEAD_SKELETONS_SKELETON = 146,
        UNDEAD_SPIRITS_BANSHEE = 147,
        UNDEAD_SPIRITS_SPIRIT_HOST = 148,
        UNDEAD_SPIRITS_WRAITH = 149,
        UNDEAD_WIGHTS_WIGHT = 150,
        UNDEAD_ZOMBIES_ZOMBIE = 151,
        ANIMALS_BIRDS_WARHAWK = 152,
        ANIMALS_BIRDS_VULTURE = 153,
        ANIMALS_BIRDS_GREAT_EAGLE = 154,
        DWARVEN_SLAYER = 155,
        HUMANOIDS_HUMANS_BANDIT = 156,
        HUMANOIDS_HUMANS_DRAKK_CULTIST = 157,
        HUMANOIDS_HUMANS_PLAGUE_VICTIM = 158,
        SIEGE_SINGLE_TARGET = 159,
        SIEGE_GTAOE = 160,
        SIEGE_OIL = 161,
        SIEGE_RAM = 162,
    };

    public enum Tome
    {
        TOME_SECTION_LIVE_EVENT = 60,
        TOME_REWARD_TITLE = 1,
        TOME_SECTION_ZONE_MAPS = 50,
        TOME_SECTION_WAR_JOURNAL = 20,
        TOME_SECTION_GAME_FAQ = 102,
        TOME_REWARD_QUEST = 2,
        TOME_SECTION_HISTORY_AND_LORE = 3,
        TOME_REWARD_ABILITY_COUNTER = 6,
        TOME_SECTION_ACHIEVEMENTS = 30,
        TOME_REWARD_ITEM_NO_AUTOCREATE = 7,
        TOME_SECTION_BESTIARY = 1,
        TOME_SECTION_TACTICS = 26,
        TOME_SECTION_GAME_MANUAL = 100,
        TOME_SECTION_NOTEWORTHY_PERSONS = 2,
        TOME_REWARD_ABILITY = 3,
        TOME_REWARD_ITEM = 4,
        TOME_SECTION_PLAYER_TITLES = 25,
        TOME_SECTION_OLD_WORLD_ARMORY = 4,
        TOME_REWARD_XP = 0
    };

    public enum RRQDisplayType
    {
        ERRQDISPLAY_DEFAULT = 1,
        ERRQDISPLAY_TOMB_KINGS = 2,
        ERRQDISPLAY_LIVE_EVENT = 3,
    };

    public enum FortressMessage
    {
        SEIZEOWNREALM = 15,
        ALREADYCLAIMED = 7,
        NOGUILDORALLIANCEFOUND = 9,
        INVALIDSUBAREA = 11,
        NOTINALLIANCE = 10,
        NOTINPLAYERORG = 13,
        OUTOFRANGE = 6,
        UNCLAIMNOTOWNER = 14,
        REALMOWNERSHIP = 12,
        MAINFORTRESSNOTCLAIMED = 8,
    };

    public enum Sound
    {
        SOUND_APOTHECARY_FAILED = 1105,
        SOUND_CAREER_POINT_SPECIALTY = 205,
        SOUND_CAREER_POINT_CORE = 203,
        SOUND_ACTIVATE_GENERAL_ABILITY = 103,
        SOUND_RENOWN_RANK_UP = 316,
        SOUND_MINI_MAP_ZOOM_OUT = 107,
        SOUND_PREGAME_CREATE_CHAR_BUTTON = 1005,
        SOUND_RESPAWN = 216,
        SOUND_CULTIVATING_ADD_FAILED = 1113,
        SOUND_BUTTON_CLICK = 300,
        SOUND_TARGET_SELECT = 101,
        SOUND_PUBLIC_CAREER_POINTS_UPDATED = 231,
        SOUND_NEGATIVE_FEEDBACK = 302,
        SOUND_PUBLIC_TOME_UNLOCKED = 230,
        SOUND_ADVANCE_RANK = 201,
        SOUND_RELEASE_CORPSE = 108,
        SOUND_MAIN_OPEN = 118,
        SOUND_CONVERSATION_TEXT_ARRIVED = 232,
        SOUND_PUBLIC_QUEST_ADDED = 225,
        SOUND_PREGAME_BAD_PLAYER_NAME = 1004,
        SOUND_INFLUENCE_RANK_UP = 317,
        SOUND_BACKPACK_CLOSE = 113,
        SOUND_OBJECTIVE_CAPTURE = 211,
        SOUND_TARGET_DESELECT = 102,
        SOUND_PLAYER_RECEIVES_TELL = 321,
        SOUND_BACKPACK_OPEN = 112,
        SOUND_ICON_CLEAR = 305,
        SOUND_TOME_CLOSE = 110,
        SOUND_MINI_MAP_ZOOM_IN = 106,
        SOUND_CHARACTER_OPEN = 114,
        SOUND_CULTIVATING_HARVEST_CROP = 1110,
        SOUND_PREGAME_CHAR_CREATE_CONTINUE_BUTTON = 1002,
        SOUND_AUCTION_HOUSE_CREATE_AUCTION = 315,
        SOUND_ENTER_GAME = 100,
        SOUND_BETA_WARNING = 999,
        SOUND_CULTIVATING_WATER_ADDED = 1108,
        SOUND_CULTIVATING_SOIL_ADDED = 1107,
        SOUND_TOME_OPEN = 109,
        SOUND_CULTIVATING_COMPLETED = 1112,
        SOUND_PUBLIC_QUEST_FAILED = 228,
        SOUND_LOOT_MONEY = 11,
        SOUND_RVR_FLAG_OFF = 217,
        SOUND_MORALE_ABILITY_3_UNLOCK = 208,
        SOUND_LOOT_SINGLE = 106,
        SOUND_RVR_FLAG_ON = 218,
        SOUND_CHARACTER_CLOSE = 115,
        SOUND_RECEIVED_NEW_MAIL_FROM_PLAYER = 322,
        SOUND_PUBLIC_QUEST_CYCLING = 229,
        SOUND_SCENARIO_INVITE = 324,
        SOUND_APOTHECARY_DETERMINENT_ADDED = 1101,
        SOUND_WINDOW_CLOSE = 307,
        SOUND_APOTHECARY_BREW_STARTED = 1104,
        SOUND_ITEM_MOVE = 308,
        SOUND_MORALE_ABILITY_1_UNLOCK = 206,
        SOUND_SCENARIO_FANFARE_END = 998,
        SOUND_LOOT_ALL = 105,
        SOUND_CLOSE_WORLD_MAP = 314,
        SOUND_ICON_PICKUP = 303,
        SOUND_MORALE_LEVEL_DOWN = 222,
        SOUND_HELP_OPEN = 120,
        SOUND_HELP_CLOSE = 121,
        SOUND_MORALE_ABILITY_0_UNLOCK = 220,
        SOUND_ACTIVATE_MORALE_ABILITY = 104,
        SOUND_QUEST_OBJECTIVES_COMPLETED = 219,
        SOUND_APOTHECARY_ITEM_REMOVED = 1111,
        SOUND_QUEST_COMPLETED = 215,
        SOUND_APOTHECARY_RESOURCE_ADDED = 1102,
        SOUND_QUEST_ABANDONED = 214,
        SOUND_QUEST_ACCEPTED = 213,
        SOUND_OPEN_WORLD_MAP = 313,
        SOUND_CAREER_CLOSE = 117,
        SOUND_OBJECTIVE_LOSE = 212,
        SOUND_MORALE_ABILITY_5_UNLOCK = 210,
        SOUND_CAREER_POINT_SECONDARY = 204,
        SOUND_CULTIVATING_SEED_ADDED = 1106,
        SOUND_PUBLIC_QUEST_COMPLETED = 227,
        SOUND_WINDOW_OPEN = 306,
        SOUND_MONETARY_TRANSACTION = 10,
        SOUND_MORALE_ABILITY_4_UNLOCK = 209,
        SOUND_ADVANCE_TIER = 202,
        SOUND_POSITIVE_FEEDBACK = 301,
        SOUND_ACTION_FAILED = 200,
        SOUND_CULTIVATING_NUTRIENT_ADDED = 1109,
        SOUND_MORALE_LEVEL_UP = 221,
        SOUND_MAIN_CLOSE = 119,
        SOUND_PUBLIC_QUEST_UPDATED = 226,
        SOUND_ICON_DROP = 304,
        SOUND_CAREER_OPEN = 116,
        SOUND_RECEIVED_NEW_MAIL_FROM_AUCTION = 323,
        SOUND_PREGAME_CHAR_CREATE_BACK_BUTTON = 1003,
        SOUND_QUEST_OBJECTIVES_FAILED = 224,
        SOUND_APOTHECARY_ADD_FAILED = 1103,
        SOUND_APOTHECARY_CONTAINER_ADDED = 1100,
        SOUND_GROUP_PLAYER_ADDED = 233,
        SOUND_TOME_TURN_PAGE = 111,
        SOUND_PREGAME_PLAY_GAME_BUTTON = 1001,
        SOUND_GENERAL_NOTIFICATION = 310,
        SOUND_MORALE_ABILITY_2_UNLOCK = 207,
        SOUND_QUEST_OBJECTIVES_NEW = 223,
        SOUND_BUTTON_OVER = 2
    };

    public enum CraftingItemType
    {
        CRAFTINGITEMTYPE_MULTIPLIER = 4,
        CRAFTINGITEMTYPE_MAIN_INGREDIENT = 2,
        CRAFTINGITEMTYPE_STIMULANT = 18,
        CRAFTINGITEMTYPE_FIXER = 9,
        CRAFTINGITEMTYPE_PIGMENT = 8,
        CRAFTINGITEMTYPE_STABILIZER = 1,
        CRAFTINGITEMTYPE_CONTAINER = 5,
        CRAFTINGITEMTYPE_MIN_CRAFTING_ITEM_NUM = 1,
        CRAFTINGITEMTYPE_EXTENDER = 3,
        CRAFTINGITEMTYPE_MAX_CRAFTING_ITEM_NUM = 18,
        CRAFTINGITEMTYPE_QUICKSILVER = 12,
        CRAFTINGITEMTYPE_CONTAINER_ESSENCE = 7,
        CRAFTINGITEMTYPE_GOLDWEED = 10,
        CRAFTINGITEMTYPE_MAGIC_ESSENCE = 17,
        CRAFTINGITEMTYPE_CONTAINER_DYE = 6,
        CRAFTINGITEMTYPE_CURIOS = 16,
        CRAFTINGITEMTYPE_GOLD_ESSENCE = 15,
        CRAFTINGITEMTYPE_FRAGMENT = 14,
        CRAFTINGITEMTYPE_GOLDDUST = 11,
        CRAFTINGITEMTYPE_TALISMAN_CONTAINER = 13
    };

    public enum CultivationStage
    {
        CULTIVATIONSTAGE_HARVESTING = 5,
        CULTIVATIONSTAGE_EMPTY = 0,
        CULTIVATIONSTAGE_SEEDLING = 2,
        CULTIVATIONSTAGE_NUM_OF_STAGES = 6,
        CULTIVATIONSTAGE_GROWN = 4,
        CULTIVATIONSTAGE_FLOWERING = 3,
        CULTIVATIONSTAGE_GERMINATION = 1
    };

    public enum CraftingStates
    {
        CRAFTINGSTATES_FAIL = 6,
        CRAFTINGSTATES_ADDINGREDIENT = 2,
        CRAFTINGSTATES_ADDDETERMINENT = 1,
        CRAFTINGSTATES_PERFORMING = 7,
        CRAFTINGSTATES_SUCCESS_REPEAT = 5,
        CRAFTINGSTATES_SUCCESS = 4,
        CRAFTINGSTATES_ADDCONTAINER = 0,
        CRAFTINGSTATES_VALID_RECIPE = 3
    };

    public enum OpenPartyInterest
    {
        OPENPARTYINTEREST_NOT_SET = 0,
        OPENPARTYINTEREST_DUNGEON = 5,
        OPENPARTYINTEREST_PQ = 3,
        OPENPARTYINTEREST_RVR = 2,
        OPENPARTYINTEREST_SCENARIO = 4,
        OPENPARTYINTEREST_PVE = 1
    };

    public enum AssistType
    {
        ASSISTTYPE_MAIN_ASSIST = 0
    };

    public enum TargetTypes
    {
        TARGETTYPES_TARGET_NONE = 0,
        TARGETTYPES_TARGET_SELF = 1,
        TARGETTYPES_TARGET_ENEMY = 2,
        TARGETTYPES_TARGET_ALLY = 3,
        TARGETTYPES_TARGET_GROUP = 4,
        TARGETTYPES_TARGET_PET = 5,
        TARGETTYPES_TARGET_GROUND = 6
    };

    public enum SwitchTargetTypes
    {
        ENEMY_SET = 1,
        ENEMY_CEAR = 5,
        FRIENDLY_SET = 2,
        FRIENDLY_CLEAR = 6
    };

    public enum CombatEvent
    {
        COMBATEVENT_HIT = 0,
        COMBATEVENT_ABILITY_HIT = 1,
        COMBATEVENT_CRITICAL = 2,
        COMBATEVENT_BLOCK = 4,
        COMBATEVENT_PARRY = 5,
        COMBATEVENT_EVADE = 6,
        COMBATEVENT_DISRUPT = 7,
        COMBATEVENT_ABSORB = 8,
        COMBATEVENT_ABILITY_CRITICAL = 9, // 3
        COMBATEVENT_IMMUNE = 10 //9
    };

    public enum PublicQuestDifficulty
    {
        Medium = 1,
        Easy = 0,
        Hard = 2,
        VeryHard = 3,
        DifficultyCount = 4,
        Unknown = 255
    };

    public enum CultivationTypes
    {
        CULTIVATIONTYPES_NONE = 0,
        CULTIVATIONTYPES_WATERCAN = 3,
        CULTIVATIONTYPES_SPORE = 5,
        CULTIVATIONTYPES_SEED = 1,
        CULTIVATIONTYPES_NUTRIENT = 4,
        CULTIVATIONTYPES_SOIL = 2,
        CULTIVATIONTYPES_NUM_TYPES = 6
    };

    public enum Feedback
    {
        FEEDBACK_QUESTS_AND_PUBLIC_QUESTS = 9,
        FEEDBACK_TOME_OF_KNOWLEDGE = 8,
        FEEDBACK_CITIES = 7,
        FEEDBACK_GENERAL_POSITIVE = 15,
        FEEDBACK_COMBAT = 11,
        FEEDBACK_CAREER = 10,
        FEEDBACK_USER_INTERFACE = 13,
        FEEDBACK_ENUM_START = 7,
        FEEDBACK_TRADESKILL_AND_ECONOMY = 12,
        FEEDBACK_GENERAL_NEGATIVE = 14
    };

    public enum ItemLocs
    {
        ITEMLOCS_INVENTORY = 3,
        ITEMLOCS_QUEST_ITEM = 9,
        ITEMLOCS_EQUIPPED = 2,
        ITEMLOCS_INVENTORY_OVERFLOW = 20,
        ITEMLOCS_BANK = 4
    };

    public enum ObjectEffectState
    {
        OBJECTEFFECTSTATE_NONE = 0,
        OBJECTEFFECTSTATE_COMBO_STATE = 5,
        OBJECTEFFECTSTATE_EBON_KEEP = 8,
        OBJECTEFFECTSTATE_CRUSH_MUTATION = 17,
        OBJECTEFFECTSTATE_KNOCKED_DOWN = 4,
        OBJECTEFFECTSTATE_BERSERK = 1,
        OBJECTEFFECTSTATE_STEALTH = 12,
        OBJECTEFFECTSTATE_TAUNTED = 9,
        OBJECTEFFECTSTATE_SCALE_UP = 7,
        OBJECTEFFECTSTATE_DETAUNTED = 10,
        OBJECTEFFECTSTATE_ORDER_CHICKEN = 14,
        OBJECTEFFECTSTATE_BLADE_MUTATION = 16,
        OBJECTEFFECTSTATE_CLAW_MUTATION = 15,
        OBJECTEFFECTSTATE_CARRYING_BANNER = 3,
        OBJECTEFFECTSTATE_CHAOS_CHICKEN = 13,
        OBJECTEFFECTSTATE_SQUIG_ARMOR = 6,
        OBJECTEFFECTSTATE_WAAAGH = 2,
        OBJECTEFFECTSTATE_CARRYING_FLAG = 11
    };

    public enum Factions
    {
        FACTIONS_DARK_ELF = 4,
        FACTIONS_HIGH_ELF = 3,
        FACTIONS_EMPIRE = 5,
        FACTIONS_DWARF = 1,
        FACTIONS_CHAOS = 6,
        FACTIONS_GREENSKIN = 2
    };

    public enum HelpViolationCategory
    {
        HELPVIOLATIONCATEGORY_GENERAL = 0,
        HELPVIOLATIONCATEGORY_XP_RENOWN_FARMING = 3,
        HELPVIOLATIONCATEGORY_CROSS_REALMING = 7,
        HELPVIOLATIONCATEGORY_HARASSMENT = 1,
        HELPVIOLATIONCATEGORY_KILL_STEALING = 6,
        HELPVIOLATIONCATEGORY_SPEED_HACKING = 4,
        HELPVIOLATIONCATEGORY_MACROING = 5,
        HELPVIOLATIONCATEGORY_ZONE_DISRUPTION = 2
    };

    public enum TradeSkills
    {
        TRADESKILLS_NONE = 0,
        TRADESKILLS_BUTCHERING = 1,
        TRADESKILLS_SCAVENGING = 2,
        TRADESKILLS_APOTHECARY = 4,
        TRADESKILLS_NUM_TRADE_SKILLS = 6,
        TRADESKILLS_CULTIVATION = 3,
        TRADESKILLS_TALISMAN = 5,
        TRADESKILLS_SALVAGING = 6
    };

    public enum AppealStatus
    {
        APPEALSTATUS_AWAITING_PLAYER = 1,
        APPEALSTATUS_AWAITING_CS = 0,
        APPEALSTATUS_CLOSED = 2
    };

    public enum AppealTopic
    {
        APPEALTOPIC_CHARACTER_ISSUES = 6,
        APPEALTOPIC_MISSING_CHARACTER = 2,
        APPEALTOPIC_MAIL = 14,
        APPEALTOPIC_GOLD_SELLING = 18,
        APPEALTOPIC_MISSING_ITEM = 3,
        APPEALTOPIC_AUCTION_HOUSE = 15,
        APPEALTOPIC_VIOLATION_REPORT = 4,
        APPEALTOPIC_NAMING_VIOLATION = 5,
        APPEALTOPIC_TOME_OF_KNOWLEDGE = 13,
        APPEALTOPIC_PUBLIC_QUEST = 7,
        APPEALTOPIC_STUCK = 1,
        APPEALTOPIC_INTERFACE = 16,
        APPEALTOPIC_QUEST_AND_QUEST_ITEMS = 11,
        APPEALTOPIC_TRADESKILL = 17,
        APPEALTOPIC_MONSTER_ISSUE = 10,
        APPEALTOPIC_BATTLEFIELD_OBJECTIVES_AND_KEEPS = 9,
        APPEALTOPIC_SCENARIO = 8,
        APPEALTOPIC_COMBAT_OR_SKIRMISH = 12
    };

    public enum Item
    {
        ITEM_EITEMFLAG_TEMPORARY = 4,
        ITEM_EITEMFLAG_NO_MOVE = 5,
        ITEM_EITEMFLAG_BIND_ON_PICKUP = 2,
        ITEM_EITEMFLAG_DYE_ABLE = 8,
        ITEM_EITEMFLAG_DECAYED = 7,
        ITEM_NUM_ITEM_FLAGS = 16,
        ITEM_EITEMFLAG_BIND_ON_EQUIP = 3,
        ITEM_EITEMFLAG_NO_CHARGE_DELETE = 0,
        ITEM_EITEMFLAG_MAGICAL_SALVAGABLE = 9,
        ITEM_EITEMFLAG_BROKEN = 6,
        ITEM_EITEMFLAG_MUNDANE_SALVAGABLE = 10,
        ITEM_EITEMFLAG_CHAIN_USE = 1
    };

    public enum MailboxType
    {
        MAILBOXTYPE_PLAYER = 0,
        MAILBOXTYPE_AUCTION = 1
    };

    public enum SocialListType
    {
        SOCIAL_FRIEND = 1,
        SOCIAL_IGNORE = 2
    };

    public enum InteractTrainerType
    {
        None = 0,
        TradeSkill = 1,
        CareerCore = 2,
        CareerMastery = 4,
        Renown = 8,
        Tome = 16,
    };

    public enum RewardType
    {
        None = 0,
        Kill = 3,
        ScenarioWin = 7,
        Assist = 11,
        ZoneKeepCapture = 16,
        ObjectiveCapture = 28,
        ObjectiveDefense = 30
    }

    public enum BuffTargetType
    {
        BUFFTARGETTYPE_GROUP_MEMBER_END = 5,
        BUFFTARGETTYPE_SELF = 6,
        BUFFTARGETTYPE_TARGET_HOSTILE = 7,
        BUFFTARGETTYPE_TARGET_FRIENDLY = 8,
        BUFFTARGETTYPE_GROUP_MEMBER_START = 0
    };

    public enum Realms
    {
        REALMS_REALM_NEUTRAL = 0,
        REALMS_REALM_ORDER = 1,
        REALMS_REALM_DESTRUCTION = 2,
        REALMS_TOTAL_REALMS = 3,
        REALMS_REALM_HOSTILE = 4
    };

    public enum InteractType
    {
        INTERACTTYPE_BINDER = 8,
        INTERACTTYPE_INFLUENCE = 2,
        INTERACTTYPE_SCENARIO_QUEUE = 3,
        INTERACTTYPE_QUEST_GIVER = 6,
        INTERACTTYPE_LASTNAMESHOP = 15,
        INTERACTTYPE_AUCTIONEER = 13,
        INTERACTTYPE_DYEMERCHANT = 14,
        INTERACTTYPE_GUILD_VAULT = 17,
        INTERACTTYPE_HEALER = 9,
        INTERACTTYPE_GUILD_REGISTRAR = 7,
        INTERACTTYPE_STORE = 1,
        INTERACTTYPE_MAIL = 12,
        INTERACTTYPE_SIEGEWEAP = 11,
        INTERACTTYPE_REPAIR = 10,
        INTERACTTYPE_IDLE_CHAT = 5,
        INTERACTTYPE_BANKER = 16,
        INTERACTTYPE_FLIGHT_MASTER = 4,
        INTERACTTYPE_TRAINER = 0,
        INTERACTTYPE_BARBER_SURGEON = 18,
        INTERACTTYPE_TRADESKILL = 19
    };

    public enum PlayerActions
    {
        PLAYERACTIONS_NONE = 0,
        PLAYERACTIONS_FIRE_SIEGE_WEAPON = 6,
        PLAYERACTIONS_USE_ITEM = 2,
        PLAYERACTIONS_COMMAND_PET = 9,
        PLAYERACTIONS_PERFORM_CRAFTING = 8,
        PLAYERACTIONS_DO_MACRO = 4,
        PLAYERACTIONS_COMMAND_PET_DO_ABILITY = 10,
        PLAYERACTIONS_SET_TARGET = 3,
        PLAYERACTIONS_ASSIST_PLAYER = 11,
        PLAYERACTIONS_DO_CRAFTING = 7,
        PLAYERACTIONS_COMMAND_TEXT = 5,
        PLAYERACTIONS_DO_ABILITY = 1
    };

    public enum HelpType
    {
        HELPTYPE_UPDATE_APPEAL = 6,
        HELPTYPE_CREATE_APPEAL_GOLD_SELLER = 3,
        HELPTYPE_CREATE_APPEAL_VIOLATION_REPORT = 2,
        HELPTYPE_CANCEL_APPEAL = 7,
        HELPTYPE_CREATE_APPEAL_NON_VALIDATED = 0,
        HELPTYPE_CREATE_FEEDBACK = 5,
        HELPTYPE_CREATE_APPEAL_NAMING_VIOLATION = 1,
        HELPTYPE_CREATE_BUG_REPORT = 4
    };

    public enum SkillType
    {
        SKILLTYPE_NONE = 0,
        SKILLTYPE_SHOOT_ON_THE_MOVE = 27,
        SKILLTYPE_PISTOL = 15,
        SKILLTYPE_MEDIUM_ROBE = 23,
        SKILLTYPE_LANCE = 16,
        SKILLTYPE_MEDIUM_ARMOR = 19,
        SKILLTYPE_GUN = 9,
        SKILLTYPE_CHARM = 24,
        SKILLTYPE_REPEATING_CROSSBOW = 17,
        SKILLTYPE_HAMMER = 3,
        SKILLTYPE_TOTAL_SKILLS = 28,
        SKILLTYPE_ROBE = 6,
        SKILLTYPE_GREAT_WEAPONS = 22,
        SKILLTYPE_DUAL_WIELD = 21,
        SKILLTYPE_BASIC_RIDING = 25,
        SKILLTYPE_HEAVY_ARMOR = 20,
        SKILLTYPE_SPEAR = 14,
        SKILLTYPE_DAGGER = 12,
        SKILLTYPE_ADVANCED_SHIELD = 5,
        SKILLTYPE_EXPERT_SHIELD = 10,
        SKILLTYPE_CROSSBOW = 8,
        SKILLTYPE_SWORD = 1,
        SKILLTYPE_ADVANCED_RIDING = 26,
        SKILLTYPE_BASIC_SHIELD = 4,
        SKILLTYPE_LIGHT_ARMOR = 18,
        SKILLTYPE_BOW = 7,
        SKILLTYPE_AXE = 2,
        SKILLTYPE_STAFF = 11,
        SKILLTYPE_THROWN = 13
    };

    public enum Auction
    {
        AUCTION_UNKNOWN_REASON = 12,
        AUCTION_CREATE_AUCTION_DURATION_LONG = 48,
        AUCTION_RESTRICTION_GUILD_ALLIANCE_ONLY = 3,
        AUCTION_SERVER_UNAVAILABLE = 14,
        AUCTION_DURATION_INDEX_VERY_LONG = 3,
        AUCTION_CANCEL_FAIL_UNKNOWN_REASON = 12,
        AUCTION_DURATION_INDEX_MEDIUM = 1,
        AUCTION_CANCEL_FAIL_NOT_OWNER = 11,
        AUCTION_RESTRICTION_NONE = 1,
        AUCTION_CREATE_AUCTION_DURATION_VERY_LONG = 72,
        AUCTION_CANCEL_FAIL_ITEM_SOLD = 10,
        AUCTION_BID_FAIL_BAD_BUY_OUT_PRICE = 4,
        AUCTION_RESTRICTION_GUILD_ONLY = 2,
        AUCTION_NUM_OF_DURATIONS = 4,
        AUCTION_CANCEL_FAIL_BIDDER_EXISTS = 9,
        AUCTION_CANCEL_SUCCESS = 8,
        AUCTION_DURATION_INDEX_LONG = 2,
        AUCTION_BID_FAIL_UNKNOWN_REASON = 7,
        AUCTION_BID_FAIL_ITEM_SOLD = 6,
        AUCTION_BID_FAIL_BAD_BID_PRICE = 3,
        AUCTION_BUYOUT_SUCCESS = 1,
        AUCTION_CREATE_AUCTION_FAILED = 13,
        AUCTION_BID_FAIL_MISSING_ITEM = 2,
        AUCTION_BID_SUCCESS = 0,
        AUCTION_BID_FAIL_OTHER_IS_BIDDING = 5,
        AUCTION_DURATION_INDEX_SHORT = 0,
        AUCTION_CREATE_AUCTION_DURATION_MEDIUM = 24,
        AUCTION_CREATE_AUCTION_DURATION_SHORT = 8
    };

    public enum HelpField
    {
        HELPFIELD_BATTLEFIELD_OBJECTIVE_NAME = 7,
        HELPFIELD_QUEST_STEP = 11,
        HELPFIELD_CAREER = 2,
        HELPFIELD_QUEST_NAME = 10,
        HELPFIELD_DETAILS = 0,
        HELPFIELD_KEEP_NAME = 8,
        HELPFIELD_NAME_REPORTING = 4,
        HELPFIELD_TOME_ENTRY = 12,
        HELPFIELD_PRICE = 13,
        HELPFIELD_PUBLIC_QUEST_NAME = 5,
        HELPFIELD_SKILL_NAME = 14,
        HELPFIELD_MONSTER_NAME = 9,
        HELPFIELD_SCENARIO_NAME = 6,
        HELPFIELD_CATEGORY = 1,
        HELPFIELD_ITEM_NAME = 3
    };

    public enum TargetType
    {
        TARGETTYPE_FRIENDLY = 1,
        TARGETTYPE_HOSTILE = 0
    };

    public enum Inventory
    {
        INVENTORY_FIRST_AVAILABLE_EQUIPPED_SLOT = 592,
        INVENTORY_FIRST_AVAILABLE_BANK_SLOT = 484,
        INVENTORY_FIRST_AVAILABLE_INVENTORY_SLOT = 563,
        INVENTORY_FIRST_AVAILABLE_GUILD_VAULT_SLOT = 604
    };

    public enum QuestTypes
    {
        QUESTTYPES_TOME = 2,
        QUESTTYPES_EPIC = 5,
        QUESTTYPES_PLAYER_KILL = 4,
        QUESTTYPES_GROUP = 0,
        QUESTTYPES_TRAVEL = 1,
        QUESTTYPES_NUM_QUEST_TYPES = 6,
        QUESTTYPES_RVR = 3
    };

    public enum ItemTypes
    {
        ITEMTYPES_NONE = 0,
        ITEMTYPES_ACCESSORY = 35,
        ITEMTYPES_PISTOL = 15,
        ITEMTYPES_MEDIUMARMOR = 19,
        ITEMTYPES_MEDIUMROBE = 22,
        ITEMTYPES_ENHANCEMENT = 23,
        ITEMTYPES_GUN = 9,
        ITEMTYPES_CHARM = 25,
        ITEMTYPES_SALVAGING = 32,
        ITEMTYPES_SHIELD = 5,
        ITEMTYPES_HAMMER = 3,
        ITEMTYPES_ADVANCEDMOUNT = 30,
        ITEMTYPES_QUEST = 21,
        ITEMTYPES_TOTAL_TYPES = 36,
        ITEMTYPES_MARKETING = 33,
        ITEMTYPES_POTION = 31,
        ITEMTYPES_CRAFTING = 34,
        ITEMTYPES_BASICMOUNT = 29,
        ITEMTYPES_DYE = 27,
        ITEMTYPES_TROPHY = 24,
        ITEMTYPES_BOW = 7,
        ITEMTYPES_SWORD = 1,
        ITEMTYPES_HEAVYARMOR = 20,
        ITEMTYPES_SPEAR = 14,
        ITEMTYPES_ROBE = 6,
        ITEMTYPES_LIGHTARMOR = 18,
        ITEMTYPES_AXE = 2,
        ITEMTYPES_STAFF = 11,
        ITEMTYPES_DAGGER = 12
    };

    public enum TacticType
    {
        TACTICTYPE_TOME = 2,
        TACTICTYPE_CAREER = 0,
        TACTICTYPE_RENOWN = 1,
        TACTICTYPE_FIRST = 0,
        TACTICTYPE_NUM_TYPES = 2
    };

    public enum KeepStatus
    {
        KEEPSTATUS_SAFE = 1,
        KEEPSTATUS_OUTER_WALLS_UNDER_ATTACK = 2,
        KEEPSTATUS_INNER_SANCTUM_UNDER_ATTACK = 3,
        KEEPSTATUS_KEEP_LORD_UNDER_ATTACK = 4,
        KEEPSTATUS_SEIZED = 5,
        KEEPSTATUS_LOCKED = 6,
        KEEPSTATUS_RUINED = 7
    };

    public enum Cultivation
    {
        CULTIVATION_NUM_OF_PLOTS = 4
    };

    public enum BugReport
    {
        BUGREPORT_MONSTER_PATHING = 2,
        BUGREPORT_OTHER = 6,
        BUGREPORT_ITEM = 5,
        BUGREPORT_ART = 1,
        BUGREPORT_CRASH = 3,
        BUGREPORT_QUESTS_AND_PUBLIC_QUESTS = 4,
        BUGREPORT_CHARACTER = 0
    };

    public enum CareerLine
    {
        CAREERLINE_IRON_BREAKER = 1,
        CAREERLINE_SLAYER = 2,
        CAREERLINE_RUNE_PRIEST = 3,
        CAREERLINE_ENGINEER = 4,

        CAREERLINE_BLACK_ORC = 5,
        CAREERLINE_CHOPPA = 6,
        CAREERLINE_SHAMAN = 7,
        CAREERLINE_SQUIG_HERDER = 8,

        CAREERLINE_WITCH_HUNTER = 9,
        CAREERLINE_KNIGHT = 10,
        CAREERLINE_BRIGHT_WIZARD = 11,
        CAREERLINE_WARRIOR_PRIEST = 12,

        CAREERLINE_CHOSEN = 13,
        CAREERLINE_WARRIOR = 14,
        CAREERLINE_ZEALOT = 15,
        CAREERLINE_MAGUS = 16,

        CAREERLINE_SWORDMASTER = 17,
        CAREERLINE_SHADOW_WARRIOR = 18,
        CAREERLINE_WHITELION = 19,
        CAREERLINE_ARCHMAGE = 20,

        CAREERLINE_BLACKGUARD = 21,
        CAREERLINE_WITCHELF = 22,
        CAREERLINE_DISCIPLE = 23,
        CAREERLINE_SORCERER = 24,

        //pets get their own careerlines now
        CAREERLINE_WAR_LION = 25,

        CAREERLINE_SQUIG = 26,
        CAREERLINE_HORNED_SQUIG = 27,
        CAREERLINE_GAS_SQUIG = 28,
        CAREERLINE_SPIKED_SQUIG = 29,

        //NPCs are no worst and got classes too...
        CAREERLINE_NPC_TANK = 30,

        CAREERLINE_NPC_MDPS = 31,
        CAREERLINE_NPC_MAGIC_RDPS = 32,
        CAREERLINE_NPC_PHYS_RDPS = 33,
        CAREERLINE_NPC_HEALER = 34,

        // More pets here
        CAREERLINE_WAR_MANTICORE = 35
    };

    public enum LootModes
    {
        LOOTMODES_ROUND_ROBIN = 0,
        LOOTMODES_FREE_FOR_ALL = 1,
        LOOTMODES_MASTER_LOOT = 2
    };

    public enum EquipSlots
    {
        EQUIPSLOTS_RIGHT_HAND = 1,
        EQUIPSLOTS_LEFT_HAND = 2,
        EQUIPSLOTS_RANGED = 3,

        EQUIPSLOTS_BANNER = 5,

        EQUIPSLOTS_BODY = 6,
        EQUIPSLOTS_GLOVES = 7,
        EQUIPSLOTS_BOOTS = 8,
        EQUIPSLOTS_HELM = 9,
        EQUIPSLOTS_SHOULDERS = 10,

        EQUIPSLOTS_POCKET1 = 11,
        EQUIPSLOTS_POCKET2 = 12,

        EQUIPSLOTS_BACK = 13,
        EQUIPSLOTS_BELT = 14,

        EQUIPSLOTS_ACCESSORY1 = 17,
        EQUIPSLOTS_ACCESSORY2 = 18,
        EQUIPSLOTS_ACCESSORY3 = 19,
        EQUIPSLOTS_ACCESSORY4 = 20
    };

    public enum TintMasks
    {
        TINTMASKS_A = 1,
        TINTMASKS_BOTH = 3,
        TINTMASKS_NONE = 0,
        TINTMASKS_B = 2
    };

    public enum AbilityType
    {
        ABILITYTYPE_STANDARD = 1,
        ABILITYTYPE_GUILD = 7,
        ABILITYTYPE_MORALE = 2,
        ABILITYTYPE_FIRST = 1,
        ABILITYTYPE_GRANTED = 4,
        ABILITYTYPE_TACTIC = 3,
        ABILITYTYPE_PET = 6,
        ABILITYTYPE_INVALID = -1,
        ABILITYTYPE_PASSIVE = 5,
        ABILITYTYPE_NUM_TYPES = 7
    };

    public enum AbilityOrigin
    {
        AO_NONE,
        AO_STANDARD,
        AO_ITEM
    }

    public enum CraftingError
    {
        CRAFTINGERROR_NONE = 0,
        CRAFTINGERROR_BACKPACK_FULL = 3,
        CRAFTINGERROR_NOT_STABLE = 1,
        CRAFTINGERROR_ITEM_DESTROYED = 2
    };

    public enum CareerCategory
    {
        CAREERCATEGORY_CC_19 = 19,
        CAREERCATEGORY_RENOWN_STATS_B = 10,
        CAREERCATEGORY_CAREER_CATEGORY_COUNT = 20,
        CAREERCATEGORY_RACE_TACTIC = 5,
        CAREERCATEGORY_ARCHTYPE_TACTIC = 3,
        CAREERCATEGORY_CC_18 = 18,
        CAREERCATEGORY_CAREER_ABILITY = 0,
        CAREERCATEGORY_RENOWN_DEFENSIVE = 14,
        CAREERCATEGORY_RENOWN_RESISTS = 12,
        CAREERCATEGORY_TOME_CC_B = 17,
        CAREERCATEGORY_TOME_CC_A = 16,
        CAREERCATEGORY_RENOWN_OFFENSIVE = 13,
        CAREERCATEGORY_ARCHTYPE_MORALE = 4,
        CAREERCATEGORY_RENOWN_STATS_C = 11,
        CAREERCATEGORY_SPECIALIZATION = 7,
        CAREERCATEGORY_RENOWN_REALM = 15,
        CAREERCATEGORY_CAREER_MORALE = 2,
        CAREERCATEGORY_RACE_MORALE = 6,
        CAREERCATEGORY_RENOWN_STATS_A = 9,
        CAREERCATEGORY_AUTOMATIC = 8,
        CAREERCATEGORY_CAREER_TACTIC = 1
    };

    public enum PlayerSpecialization
    {
        PLAYER_SPECIALIZATION_PATH_1 = 40,
        PLAYER_SPECIALIZATION_PATH_2 = 41,
        PLAYER_SPECIALIZATION_PATH_3 = 42
    };

    public enum OpenPartyRequestType
    {
        OPENPARTYREQUESTTYPE_PARTIES = 1,
        OPENPARTYREQUESTTYPE_ALL_BRIEF = 3,
        OPENPARTYREQUESTTYPE_WARBANDS = 2,
        OPENPARTYREQUESTTYPE_ALL = 0
    };

    public enum ItemSlots
    {
        ITEMSLOTS_BANNER = 5,
        ITEMSLOTS_ACCESSORY4 = 20,
        ITEMSLOTS_LEFT_HAND = 2,
        ITEMSLOTS_RANGED = 3,
        ITEMSLOTS_BACK = 13,
        ITEMSLOTS_RIGHT_HAND = 1,
        ITEMSLOTS_BOOTS = 8,
        ITEMSLOTS_PANTS = 12,
        ITEMSLOTS_TABARD = 16,
        ITEMSLOTS_SHIRT = 11,
        ITEMSLOTS_ACCESSORY1 = 17,
        ITEMSLOTS_BODY = 6,
        ITEMSLOTS_GLOVES = 7,
        ITEMSLOTS_EITHER_HAND = 4,
        ITEMSLOTS_ACCESSORY2 = 18,
        ITEMSLOTS_BELT = 14,
        ITEMSLOTS_SHOULDERS = 10,
        ITEMSLOTS_HELM = 9,
        ITEMSLOTS_ACCESSORY3 = 19
    };

    public enum Pairing
    {
        PAIRING_EMPIRE_CHAOS = 2,
        PAIRING_GREENSKIN_DWARVES = 1,
        PAIRING_ELVES_DARKELVES = 3,
        PAIRING_LAND_OF_THE_DEAD = 4
    };

    public enum SalvagingTypes
    {
        SALVAGINGTYPES_MAGICAL = 0,
        SALVAGINGTYPES_MUNDANE = 1
    };

    public enum Localized_text
    {
        TEXT_CONNECTING_TO_SERVER,	//Connecting to server...
        TEXT_HANDSHAKING,	//Handshaking...
        TEXT_LOGGING_IN,	//Logging In....
        TEXT_LOGIN_COMPLETE,	//Login complete.
        TEXT_LOGIN_TIMED_OUT,	//Login timed out.
        TEXT_LOGIN_FAILED,	//Login failed.
        TEXT_SERVER_TIMEOUT,	//Response from the server has timed out!
        TEXT_SERVER_MOTD,	//Server Message-of-the-Day: <<1>>
        TEXT_DAMAGE_ABSORBED,	// (<<1>> absorbed)
        TEXT_DAMAGE_MITIGATED,	// (<<1>> mitigated)
        TEXT_YOU_HIT_X_FOR_Y,	//<<Co2:2>> <<3[critically hits/hits]>> <<C:4>> for <<5>> damage.
        TEXT_YOU_HEAL_X_FOR_Y,	//<<Co2:2>> <<3[critically heals/heals]>> <<C:4>> for <<5>> points.
        TEXT_X_HITS_YOU_FOR_Y,	//<<C:1>>'s <<2>> <<3[critically hits/hits]>> you for <<5>> damage.
        TEXT_X_HEALS_YOU_FOR_Y,	//<<C:1>>'s <<2>> <<3[critically heals/heals]>> you for <<5>> points.
        TEXT_YOU_HIT_YOURSELF_FOR_X,	//<<Co2:2>> <<3[critically hits/hits]>> you for <<5>> damage.
        TEXT_YOU_HEAL_YOURSELF_FOR_X,	//<<Co2:2>> <<3[critically heals/heals]>> you for <<5>> points.
        TEXT_YOU_HIT_YOUR_PET_FOR_X,	//<<Co2:2>> <<3[critically hits/hits]>> <<o2:4>> for <<5>> damage.
        TEXT_YOU_HEAL_YOUR_PET_FOR_X,	//<<Co2:2>> <<3[critically heals/heals]>> <<o2:4>> for <<5>> points.
        TEXT_YOUR_PET_HITS_X_FOR_Y,	//<<Co2:1>>'s <<2>> <<3[critically hits/hits]>> <<C:4>> for <<5>> damage.
        TEXT_YOUR_PET_HEALS_X_FOR_Y,	//<<Co2:1>>'s <<2>> <<3[critically heals/heals]>> <<C:4>> for <<5>> points.
        TEXT_X_HITS_YOUR_PET_FOR_Y,	//<<C:1>>'s <<2>> <<3[critically hits/hits]>> <<o2:4>> for <<5>> damage.
        TEXT_X_HEALS_YOUR_PET_FOR_Y,	//<<C:1>>'s <<2>> <<3[critically heals/heals]>> <<o2:4>> for <<5>> points.
        TEXT_YOUR_PET_HITS_ITSELF_FOR_X,	//<<Co2:1>>'s <<2>> <<3[critically hits/hits]>> itself for <<5>> damage.
        TEXT_YOUR_PET_HEALS_ITSELF_FOR_X,	//<<Co2:1>>'s <<2>> <<3[critically heals/heals]>> itself for <<5>> points.
        TEXT_YOUR_PET_HITS_YOU_FOR_X,	//<<Co2:1>>'s <<2>> <<3[critically hits/hits]>> you for <<5>> damage.
        TEXT_YOUR_PET_HEALS_YOU_FOR_X,	//<<Co2:1>>'s <<2>> <<3[critically heals/heals]>> you for <<5>> points.
        TEXT_X_HITS_Y_FOR_Z,	//<<C:1>>'s <<2>> <<3[critically hits/hits]>> <<C:4>> for <<5>> damage.
        TEXT_X_HEALS_Y_FOR_Z,	//<<C:1>>'s <<2>> <<3[critically heals/heals]>> <<C:4>> for <<5>> points.
        TEXT_X_FAILS_TO_HIT_Y,	//<<C:1>>'s <<2>> is <<5>> by <<4>>.
        TEXT_YOU_FAIL_TO_HIT_Y,	//<<C:4>> <<5>> <<co2:2>>.
        TEXT_YOUR_PET_FAILS_TO_HIT_Y,	//<<C:4>> <<5>> <<o2:1>>'s <<2>>.
        TEXT_X_FAILS_TO_HIT_YOU,	//<<C:p2>> <<5>> <<1>>'s <<2>>.
        TEXT_X_FAILS_TO_HIT_YOUR_PET,	//<<Co2:4>> <<5>> <<1>>'s <<2>>.
        TEXT_YOU_NOW_AFFECT_X_WITH_Y,	//<<Co2:2>> affects <<C:4>>.
        TEXT_YOU_NO_LONGER_AFFECT_X_WITH_Y,	//<<Co2:2>> fades from <<C:4>>.
        TEXT_X_NOW_AFFECTS_YOU_WITH_Y,	//<<C:1>>'s <<2>> affects you.
        TEXT_X_NO_LONGER_AFFECTS_YOU_WITH_Y,	//<<C:1>>'s <<2>> fades from you.
        TEXT_YOU_NOW_AFFECT_YOURSELF_WITH_X,	//<<Co2:2>> affects yourself.
        TEXT_YOU_NO_LONGER_AFFECT_YOURSELF_WITH_X,	//<<Co2:2>> fades.
        TEXT_YOU_NOW_AFFECT_YOUR_PET_WITH_X,	//<<Co2:2>> affects <<o2:4>>.
        TEXT_YOU_NO_LONGER_AFFECT_YOUR_PET_WITH_X,	//<<Co2:2>> fades from <<o2:4>>.
        TEXT_YOUR_PET_NOW_AFFECTS_X_WITH_Y,	//<<Co2:1>>'s <<2>> affects <<C:4>>.
        TEXT_YOUR_PET_NO_LONGER_AFFECTS_X_WITH_Y,	//<<Co2:1>>'s <<2>> fades from <<C:4>>.
        TEXT_X_NOW_AFFECTS_YOUR_PET_WITH_Y,	//<<C:1>>'s <<2>> affects <<o2:4>>.
        TEXT_X_NO_LONGER_AFFECTS_YOUR_PET_WITH_Y,	//<<C:1>>'s <<2>> fades from <<o2:4>>.
        TEXT_YOUR_PET_NOW_AFFECTS_ITSELF_WITH_X,	//<<Co2:1>>'s <<2>> affects itself.
        TEXT_YOUR_PET_NO_LONGER_AFFECTS_ITSELF_WITH_X,	//<<Co2:1>>'s <<2>> fades.
        TEXT_YOUR_PET_NOW_AFFECTS_YOU_WITH_X,	//<<Co2:1>>'s <<2>> affects you.
        TEXT_YOUR_PET_NO_LONGER_AFFECTS_YOU_WITH_X,	//<<Co2:1>>'s <<2>> fades from you.
        TEXT_X_NOW_AFFECTS_Y,	//<<C:1>>'s <<2>> affects <<C:4>>.
        TEXT_X_NO_LONGER_AFFECTS_Y,	//<<C:1>>'s <<2>> fades from <<C:4>>.
        TEXT_YOU_FALL_FOR_X,	//<<C:p2>> fall for <<5>> damage.
        TEXT_X_FALLS_FOR_Y,	//<<C:4>> falls for <<5>> damage.
        TEXT_DETAILED_COMBAT_FORMAT,	// | <<1>> | <<2>> | <<4>> | <<5>>
        TEXT_CONNECTION_LOST,	//You have stopped receiving messages from the server; you have been disconnected.
        TEXT_SERVER_DISCONNECTED,	//The server has closed your connection; you have been disconnected.
        TEXT_X_ACCEPTED,	//<<1>> accepted.
        TEXT_X_COMPLETED,	//<<1>> completed!
        TEXT_X_ABANDONED,	//<<1>> abandoned.
        TEXT_QUEST_COUNTER_UPDATED,	//<<1>> <<2>>/<<3>>
        TEXT_X_DONE,	//<<1>> done!
        TEXT_ENTERED_RVR_AREA,	//You have entered an RvR area - prepare for battle!
        TEXT_ENTERED_DISALLOWED_RVR_AREA,	//You are too powerful for this area!  Leave now or suffer the Coward’s Reward!
        TEXT_AQUIRED_CHICKEN_DEBUFF,	//A negative backlash of energy diminishes your abilities!
        TEXT_YOU_ARE_NOW_RVR_FLAGGED,	//You are now flagged for RvR!
        TEXT_YOU_ARE_NO_LONGER_RVR_FLAGGED,	//RvR status removed.
        TEXT_EXITED_RVR_AREA,	//You have exited the RvR area.
        TEXT_ENTERED_INVALID_NAME,	//You have entered a duplicate or invalid name. Please enter a new name.
        CHAT_TAG_DEFAULT,	//<<1>>
        CHAT_TAG_SAY,	//[<<1>>] says: <<X:2>>
        CHAT_TAG_TELL_RECIEVE,	//[<<1>>] tells you: <<X:2>>
        CHAT_TAG_TELL_SEND,	//You tell [<<1>>]: <<X:2>>
        CHAT_TAG_CSR,	//CSR [<<1>>] tells you: <<X:2>>
        CHAT_TAG_GROUP,	//[Party][<<1>>]: <<X:2>>
        CHAT_TAG_GUILD,	//[Guild][<<1>>]: <<X:2>>
        CHAT_TAG_GUILD_OFFICER,	//[Officer][<<1>>]: <<X:2>>
        CHAT_TAG_BROADCAST,	//[<<1>>] broadcasts: <<X:2>>
        CHAT_TAG_CHANNEL,	//[<<3>>: <<4>>][<<1>>]: <<X:2>>
        CHAT_TAG_MONSTER_SAY,	//<<1>> says: <<2>>
        CHAT_TAG_YELL,	//<<1>> shouts: <<X:2>>
        CHAT_TAG_EMOTE,	//<<1>> <<X:2>>
        CHAT_TAG_MONSTER_EMOTE,	//<<1>>
        CHAT_TAG_AFK_MESSAGE,	//[<<1>>] AFK message: <<X:2>>
        ERROR_CANNOT_CREATE_RENDERER,	//Failed to create render device -- application will now terminate.
        LABEL_YES,	//Yes
        LABEL_NO,	//No
        LABEL_OK,	//Ok
        LABEL_CANCEL,	//Cancel
        TEXT_NEW_PUBLIC_QUEST,	//New Public Quest: <<1>>
        TEXT_NEW_RVR_OBJECTIVE,	//New RvR Objective: <<1>>
        TEXT_CORE_XP_DING,	//You have gained an Ability Advance Point!
        TEXT_SPEC_XP_DING,	//You have gained a Tactic Advance Point!
        TEXT_SECN_XP_DING,	//You have gained a Morale Advance Point!
        TEXT_SPECIALIZATION_XP_DING,	//You have gained a Specialization Point!
        TEXT_RENOWN_RANK_POINT_DING,	//You have gained a Renown Rank Point!
        TEXT_TIER_DING,	//Your tier has increased to <<1>>!
        TEXT_RANK_DING,	//Your rank has increased to <<1>>!
        TEXT_RENOWN_DING,	//Your renown rank has increased to <<1>>!
        TEXT_RENOWN_TITLE_DING,	//You have gained the "<<1>>" renown title!
        TEXT_LEARN_ABILITY,	//You have learned <<1>>!
        TEXT_STATS_INCREASED,	//Your stats have increased!
        TEXT_GAIN_TACTIC_SLOT,	//You have gained an additional tactic slot!
        TEXT_YOUR_BODY,	//Respawn
        TEXT_NEAREST_WARCAMP,	//Respawn
        TEXT_PROLOGUE_X,	//Prologue: <<1>>
        TEXT_CHAPTER_X_Y,	//Chapter <<1>>: <<2>>
        TEXT_EPILOGUE_X,	//Prologue: <<1>>
        TEXT_COMBO_FINISH_A,	//LOTS BEST COMBO
        TEXT_COMBO_FINISH_B,	//BIGGEST BEST COMBO
        TEXT_COMBO_FINISH_C,	//DEAD BEST COMBO
        TEXT_YOU_HAVE_ENTERED_AREA_X,	//You have entered <<1>>.
        TEXT_YOU_HAVE_LEFT_AREA_X,	//You have left <<1>>.
        TEXT_ZONE_X,	//<<1>>
        LABEL_ABILITY_NOUN,	//ability
        LABEL_ATTACK_NOUN,	//attack
        LABEL_UNKNOWN_ENTITY,	//something^N
        TEXT_BLOCKED,	//blocked
        TEXT_PARRIED,	//parried
        TEXT_EVADED,	//dodged
        TEXT_DISRUPTED,	//disrupted
        TEXT_AUTOATTACK,	//Auto-Attack
        TEXT_STORE_NAME_SUFFIX,	//Merchant -
        TEXT_YOU_GAIN_X_EXP,	//You gain <<1>> experience<<n:2[/ $s/ $s]>>.
        TEXT_YOU_GAIN_X_RENOWN,	//You gain <<1>> renown<<n:2[/ $s/ $s]>>.
        TEXT_YOU_GAIN_X_INFL,	//You gain <<1>> influence<<n:2[/ $s/ $s]>>.
        TEXT_DEATH_SPAM_DEFAULT,	//<<C:1>> has been killed by <<2>> in <<3>>. <<4>>
        TEXT_DEATH_SPAM_ABILITY_1,	//<<C:1>> has been destroyed by <<2>>'s <<3>> in <<4>>. <<5>>
        TEXT_DEATH_SPAM_ABILITY_2,	//<<C:1>> has been annihilated by <<2>>'s <<3>> in <<4>>. <<5>>
        TEXT_DEATH_SPAM_SWORD_1,	//<<C:1>> has been slashed by <<2>>'s <<3>> in <<4>>. <<5>>
        TEXT_DEATH_SPAM_SWORD_2,	//<<C:1>> has been sliced by <<2>>'s <<3>> in <<4>>. <<5>>
        TEXT_DEATH_SPAM_AXE_1,	//<<C:1>> has been cleaved by <<2>>'s <<3>> in <<4>>. <<5>>
        TEXT_DEATH_SPAM_AXE_2,	//<<C:1>> has been mangled by <<2>>'s <<3>> in <<4>>. <<5>>
        TEXT_DEATH_SPAM_HAMMER_1,	//<<C:1>> has been pummeled by <<2>>'s <<3>> in <<4>>. <<5>>
        TEXT_DEATH_SPAM_HAMMER_2,	//<<C:1>> has been crushed by <<2>>'s <<3>> in <<4>>. <<5>>
        TEXT_DEATH_SPAM_RANGED_1,	//<<C:1>> has been shot by <<2>>'s <<3>> in <<4>>. <<5>>
        TEXT_DEATH_SPAM_RANGED_2,	//<<C:1>> has been assassinated by <<2>>'s <<3>> in <<4>>. <<5>>
        TEXT_DEATH_SPAM_DAGGER_1,	//<<C:1>> has been impaled by <<2>>'s <<3>> in <<4>>. <<5>>
        TEXT_DEATH_SPAM_DAGGER_2,	//<<C:1>> has been skewered by <<2>>'s <<3>> in <<4>>. <<5>>
        TEXT_PLAYER_FRENZY,	//<<C:1>> is on a Frenzy!
        TEXT_PLAYER_RAMPAGE,	//<<C:1>> is on a Rampage!
        TEXT_PLAYER_KILLING_SPREE,	//<<C:1>> is on a Killing Spree!
        TEXT_SCENARIO_QUEUE_TOO_LOW_LEVEL_ERROR,	//You cannot join this scenario, your rank is too low.
        TEXT_SCENARIO_QUEUE_TOO_HIGH_LEVEL_ERROR,	//You cannot join this scenario, your rank is too high.
        TEXT_SCENARIO_QUEUE_TOO_LOW_RENOWN_ERROR,	//You cannot join this scenario, your renown rank is too low.
        TEXT_SCENARIO_QUEUE_TOO_HIGH_RENOWN_ERROR,	//You cannot join this scenario, your renown rank is too high.
        TEXT_SCENARIO_QUEUE_NO_BRACKET_ERROR,	//You cannot join this scenario, there is no bracket for your current rank.
        TEXT_SCENARIO_QUEUE_BRACKET_ERROR,	//Your party cannot join this scenario together because not all players are in the same bracket.
        TEXT_ABOVE_HEAD_RENOWN_AND_GUILD_TITLES,	//<<1>> of <<2>>
        TEXT_CAMPAIGN_CAPTURED,	//<<1>> seizes control of <<2>>.
        TEXT_CAMPAIGN_LOST,	//<<1>> loses control of <<2>>.
        TEXT_CAMPAIGN_BattleFront_SHIFT,	//The WAR has moved to <<1>>.
        TEXT_CAMPAIGN_BattleFront_TAKEN,	//The forces of <<1>> have taken <<2>>.
        TEXT_CAMPAIGN_CAPITOL_WARNING,	//<<2>> has fallen to the forces of <<1>>! Prepare to defend the city!
        TEXT_CAMPAIGN_CAPITOL_VULNERABLE,	//<<2>> has fallen to the forces of <<1>>! <<3>> is vulnerable to assault!
        TEXT_CAPITOL_CHANGED_HANDS,	//<<1>> has fallen!
        LABEL_ORDER,	//Order
        LABEL_DESTRUCTION,	//Destruction
        ERROR_CAREER_PACKAGE_ALREADY_PURCHASED,	//You already have that ability.
        ERROR_CAREER_PACKAGE_DEPEND_NOT_COMPLETE,	//This ability has a prerequisite.
        ERROR_CAREER_PACKAGE_NOT_ENOUGH_POINTS,	//You don’t have enough points.
        ERROR_CAREER_PACKAGE_REQUIRES_PTS_SPENT,	//You haven't purchased the minimum required.
        ERROR_CAREER_PACKAGE_CANT_PURCHASE_MORE,	//You have exceeded your maximum allowed.
        ERROR_CAREER_PACKAGE_NOT_ENOUGH_MONEY,	//You don’t have enough money.
        ERROR_CAREER_PACKAGE_TOO_LOW_LEVEL,	//You're not high enough rank.
        ERROR_CAREER_PACKAGE_TOO_LOW_RENOWN_RANK,	//You must achieve a higher renown rank.
        TEXT_INTERACT_FAIL_TIMER_RUNNING,	//You cannot interact with that item at this time.
        TEXT_TAKING_SCREENSHOT,	//Taking a Screenshot...
        TEXT_FLIGHT_SUCCESS,	//Off you go then!
        TEXT_FLIGHT_LACK_FUNDS,	//You seem to be a little short on funds at the moment.
        TEXT_FLIGHT_REQUIREMENT,	//You do not meet the height requirement for that particular destination.
        TEXT_FLIGHT_INVALID_FLIGHT,	//That destination is unavailable at the moment.
        TEXT_GUILDNPC_PROCESS_COMPLETE,	//Guild Creation Successful.
        TEXT_GUILDNPC_BAD_NAME,	//Guild Name is already taken or banned.
        TEXT_GUILDNPC_GROUPCHANGED,	//Party was changed.
        TEXT_GUILDNPC_INVALIDREQ_FUNDS,	//You do not have enough funds to form a Guild. You must have at least <<1>> <<2>>.
        TEXT_GUILDNPC_INVALIDREQ_GUILDED,	//At least one party member is already in a Guild.
        TEXT_GUILDNPC_INVALIDREQ_NOTGROUPED,	//You must be the Leader of a Party of 6.
        TEXT_GUILDNPC_INVALIDREQ_NOTLEADER,	//You must be the Leader of a Party of 6.
        TEXT_GUILDNPC_INVALIDREQ_NUMPLAYERS,	//Your party is not full or some members are too far away from the Registrar.
        TEXT_GUILDNPC_INVALIDREQ_OUTOFRANGE,	//Some party members are out of range of the Registrar.
        TEXT_GUILDNPC_UNAPPROVED,	//A party member declined forming a Guild.
        TEXT_GUILDNPC_VALID_REQUIREMENTS,	//All prerequisites for Guild NPC Interaction have been met.
        TEXT_GUILDNPC_FORM_GUILD_DIALOG,	//Do you wish to form the Guild "<<1>>"?
        TEXT_ALLIANCE_FORM_DIALOG,	//Do you wish to form the Alliance "<<1>>"?
        CHAT_TAG_ALLIANCE,	//[Alliance][<<1>>]: <<X:2>>
        CHAT_TAG_ALLIANCE_OFFICER,	//[Alliance Officer][<<1>>]: <<X:2>>
        TEXT_LOGIN_TOKEN_MISSING,	//Unable to authenticate. Missing token.  Please relaunch the patcher.
        TEXT_LOGIN_BAD_PARAMETERS,	//Unable to authenticate. Malformed username or token.  Please relaunch the patcher.
        TEXT_LOGIN_FAILED_AUTHTIMEOUT,	//The Authorization Server is temporarily unavailable.
        TEXT_LOGIN_FAILED_INCORRECTVERSION,	//You do not have the latest version!
        TEXT_LOGIN_FAILED_RELOGINTIMER,	//Your account is being logged out!  Try back in a few minutes.
        TEXT_LOGIN_FAILED_TOOMANYPLAYERS,	//Too many players are logged in.  Please try later.
        TEXT_LOGIN_FAILED_ALREADYLOGGEDIN,	//Your account is already logged in!
        TEXT_LOGIN_FAILED_MISMATCHEDACCOUNTNAME,	//The Authorization Server is temporarily unavailable.
        TEXT_LOGIN_FAILED_INCORRECTPASSWORD,	//Your Password is Incorrect!
        TEXT_LOGIN_FAILED_ACCOUTNNAMENOTFOUND,	//No Record for User Found!
        TEXT_LOGIN_FAILED_SYSTEMERROR,	//The Authorization Server is temporarily unavailable.
        TEXT_LOGIN_FAILED_DATABASEERROR,	//Error Accessing User Account!
        TEXT_LOGIN_FAILED_NORECORDFORACOUNT,	//No Record for User Found!
        TEXT_LOGIN_FAILED_ACCOUNTSUSPENDED,	//Your account has been suspended!
        TEXT_LOGIN_FAILED_ACCOUNTSUSPENDEDBILLING,	//Your account has no access to any games!
        TEXT_LOGIN_FAILED_ACCOUNTCLOSED,	//Your account has no access to this game!
        TEXT_LOGIN_FAILED_ACCOUNTTERMINATED,	//Your account has been terminated.
        TEXT_LOGIN_FAILED_ACCOUNTINACTIVE,	//Your account is currently inactive.
        TEXT_LOGIN_FAILED_ACCOUNTLOGGEDINOTHERSERVER,	//Your account is already logged in to a different server!
        TEXT_LOGIN_FAILED_SERVERCLOSED,	//The game is currently closed.  Please try later.
        TEXT_HEAL_SUCCESS,	//You are successfully healed!
        CHAT_TAG_BATTLEGROUP,	//[Warband][<<1>>]: <<X:2>>
        TEXT_RECEIVED_INFLUENCE_REWARD,	//You've received an influence reward!
        TEXT_X_RESULTS_FOUND,	//Found <<1>> result(s).
        TEXT_PLAYER_SEARCH_RESULT_GUILDED,	//<<1>> - <<2>> <<3>> - < <<4>> > - <<5>>
        TEXT_PLAYER_SEARCH_RESULT_UNGUILDED,	//<<1>> - <<2>> <<3>> - <<4>>
        TEXT_PLAYER_SEARCH_RESULT_ANON,	//<<1>> - ANONYMOUS
        TEXT_PLAYER_SEARCH_CUT_SHORT,	//Your search returned the maximum number of results allowed. Try a more specific query.
        TEXT_GUILDNEWS_MEMBER_JOINED,	//[<<1>>] <<2>> has joined the guild.
        TEXT_GUILDNEWS_MEMBER_LEFT,	//[<<1>>] <<2>> has left the guild.
        TEXT_GUILDNEWS_MEMBER_KICKED,	//[<<1>>] <<2>> was kicked from the guild.
        TEXT_GUILDNEWS_MEMBER_LEVELED,	//[<<1>>] <<2>> has gained rank <<3>>.
        TEXT_GUILDNEWS_MEMBER_RENOWNRANKED,	//[<<1>>] <<2>> has gained renown rank <<3>>.
        TEXT_GUILDNEWS_ALLIANCE_JOINED,	//[<<1>>] <<2>> has joined the <<3>> alliance.
        TEXT_GUILDNEWS_ALLIANCE_LEFT,	//[<<1>>] <<2>> has left the <<3>> alliance.
        TEXT_GUILDNEWS_ALLIANCE_KICKED,	//[<<1>>] <<2>> has been kicked from the <<3>> alliance.
        TEXT_GUILDNEWS_GUILD_LEVELED,	//[<<1>>] <<2>> has gained guild rank <<3>>.
        TEXT_GUILDNEWS_GUILD_REWARD,	//[<<1>>] <<2>> has unlocked a new reward: <<3>>.
        TEXT_GUILDNEWS_NEW_EVENT,	//[<<1>>] A new guild event (<<2>>) was posted for <<3>>.
        TEXT_GUILDNEWS_NEW_LEADER,	//[<<1>>] <<2>> has been promoted to the role of guild leader.
        TEXT_GUILDNEWS_GUILD_CREATED,	//[<<1>>] <<2>> has been created.
        TEXT_GUILDNEWS_EVENT_REMINDER,	//[<<1>>] The <<2>> guild event will begin at <<3>>.
        TEXT_TOP_OVERALL_RENOWN,	//<<1>><BR><<2>><BR>The overall highest renown holder of all time
        TEXT_TOP10_RENOWN_OF_THE_WEEK,	//<<1>><BR><<2>><BR>The number <<3>> renown holder of the week
        TEXT_CITY_YOU_CONTRIBUTED,	//You have contributed to increasing the rank of <<1>>.
        TEXT_CITY_RATING_INCREASED,	//<<1>>'s rank has increased. <<2>>
        TEXT_CITY_RATING_DECREASED,	//<<1>>'s rank has decreased. <<2>>
        TEXT_DEATH_SELF,	//You have died.
        TEXT_DEATH_SELF_X,	//You have been slain by <<1>>!
        TEXT_DEATH_OTHER,	//<<C:1>> has died.
        TEXT_DEATH_OTHER_X,	//<<C:1>> has been slain by <<2>>!
        TEXT_DEATH_OTHER_YOU,	//You have slain <<1>>!
        TEXT_DEATH_STATIC,	//<<C:1>> has been destroyed.
        TEXT_DEATH_STATIC_X,	//<<C:1>> has been destroyed by <<2>>!
        TEXT_DEATH_STATIC_YOU,	//You have destroyed <<1>>!
        TEXT_YOU_JOIN_PARTY,	//You have joined the party. To speak with your party, please use the /p command.
        TEXT_YOU_NOW_PARTY_LEADER,	//You are now the party leader.
        TEXT_YOU_LEFT_PARTY,	//You have left the party.
        TEXT_X_LEFT_PARTY,	//<<C:1>> has left the party.
        TEXT_PARTY_IS_FULL,	//The party is full.
        TEXT_X1_OPENS_BAG_X2_AND_GETS_X3,	//<<C:1>> reaches into a <<2>> loot sack and pulls out <<3>>.
        LOOT_COLOR_NAME_UTILITY,	//grey
        LOOT_COLOR_NAME_COMMON,	//white
        LOOT_COLOR_NAME_UNCOMMON,	//green
        LOOT_COLOR_NAME_RARE,	//blue
        LOOT_COLOR_NAME_VERY_RARE,	//purple
        LOOT_COLOR_NAME_ARTIFACT,	//red
        LOOT_COLOR_NAME_SILVER,	//silver
        LOOT_COLOR_NAME_GOLD,	//gold
        TEXT_UNABLE_TO_INSPECT_PLAYER_EQUIP,	//You may not inspect this player's equipment. The player either does not allow inspections or is a member of the enemy realm.
        TEXT_UNABLE_TO_INSPECT_PLAYER_BRIGHTS,	//You may not view this player's bragging rights. The player either does not allow others to view them or is a member of the enemy realm.
        TEXT_NEED_TARGET_TO_INSPECT,	//You need a target to inspect!
        TEXT_SELECT_EXACT_REWARD,	//You must select exactly <<1>> rewards!
        TEXT_GOTO_TRAINER,	//You should  visit your nearest trainer to view your new skills.
        TEXT_YOU_HAVE_EARNED,	//You have earned
        TEXT_RANK,	//Rank <<1>>!
        TEXT_RENOUN_RANK,	//Renown Rank <<1>>!
        TEXT_RVR,	//RvR
        TEXT_ACCEPTED,	//accepted!
        TEXT_COMPLETED,	//completed!
        TEXT_DONE,	//done!
        TEXT_ABANDONED,	//abandoned!
        TEXT_ORDER_CONTROLLED,	//Order controlled
        TEXT_DESTRUCTION_CONTROLLED,	//Destruction controlled
        TEXT_CONTESTED,	//Contested
        TEXT_ENTERING,	//Entering
        TEXT_PUBLIC_QUEST_RACE_CHAPTER,	//Public Quest, <<1>> Chapter <<2>>
        TEXT_SAFE,	//Safe
        TEXT_CAPTURED,	//Captured
        TEXT_OUTPUT_FORMAT_BESTIARY,	//<<1>> [<<2>> - <<3>> - <<4>>]
        TEXT_OUTPUT_FORMAT_ACHIEVEMENTS,	//<<1>> [<<2>> - <<3>> - <<4>>]
        TEXT_OUTPUT_FORMAT_WARSTORY,	//<<1>> [<<2>> - <<3>> - <<4>>]
        TEXT_OUTPUT_FORMAT_TITLE,	//<<1>> [<<2>> - <<3>>]
        TEXT_OUTPUT_FORMAT_NOTEWORTHY,	//<<1>> [<<2>> - <<4>>]
        TEXT_OUTPUT_FORMAT_HISTORY,	//<<1>> [<<2>> - <<4>>]
        CHAT_TAG_SCENARIO,	//[Scenario][<<1>>]: <<X:2>>
        CHAT_TAG_SCENARIO_GROUP,	//[Scenario Party][<<1>>]: <<X:2>>
        TEXT_PLAYER_JOIN_PARTY,	//<<1>> has joined the party.
        TEXT_GROUP_REFERRAL_FAILED,	//Your party leader is already busy handling another referral!
        TEXT_GROUP_REFERRAL_DENIED,	//<<1>> has denied the party referral for <<2>>.
        TEXT_TRADE_SKILL_LEARNED,	//You have learned the <<1>> skill
        TEXT_TRADE_SKILL_INCREASED,	//Your <<1>> skill has increased to <<2>>
        TEXT_TRADE_SKILL_BUTCHERING,	//Butchering
        TEXT_TRADE_SKILL_SCAVENGING,	//Scavenging
        TEXT_TRADE_SKILL_CULTIVATION,	//Cultivation
        TEXT_TRADE_SKILL_APOTHECARY,	//Apothecary
        TEXT_TRADE_SKILL_TALISMAN,	//Talisman
        TEXT_TRADE_SKILL_SALVAGING,	//Salvaging
        TEXT_SCENARIO_CHAT_INSTRUCTIONS,	//Welcome! To chat with your realm during a scenario, please use the following command(s): /scenariosay or /sc
        TEXT_SCENARIO_GROUP_INVITE_WARNING,	//While in a scenario, only scenario party information will be displayed in the party window. However, all commands will still function.
        TEXT_SCENARIO_GROUP_AUTOPLACE,	//You have been automatically placed into a scenario party with your other queued party members.
        TEXT_SCENARIO_NO_RESERVATIONS,	//No reservations exist for your current party due to a lack of available consecutive slots in any scenario parties.
        TEXT_SCENARIO_RESERVATIONS_CLEARED,	//The scenario has already begun, so reservations are no longer honored for your party.
        TEXT_SCENARIO_RESERVATION_REQUIREMENTS,	//To be automatically placed into a scenario party, your party must have joined the scenario queue as a party, not as individuals.
        TEXT_SCENARIO_GROUP_CHAT_INSTRUCTIONS,	//You have successfully joined a scenario party! To chat with your party, please use the following command: /sp
        TEXT_GUILD_LACKOFFUNDS_HERALDRY,	//You do not have enough money to pay for a guild heraldry configuration!
        TEXT_GUILD_HERALDRY_EDIT_SUCCESS,	//You have successfully updated your guild's heraldry! As your guild gains access to heraldry reveal rewards, it will display pieces of your guild's heraldry on guild items.
        TEXT_GUILD_HERALDRY_NOT_REWARDED,	//Your guild has not gained access to reserve its heraldry configuration yet!
        TEXT_GUILD_HERALDRY_NO_PERMISSIONS,	//You do not have permission to configure your guild's heraldry!
        TEXT_GUILD_BANNER_CONFIGSUCCESS,	//You have successfully configured the guild's standard. It will be locked for a period of no less than 24 hours before you can change it again.
        TEXT_GUILD_BANNER_ERR_CAPTURED,	//You cannot configure a standard while it is in the enemy's hands!
        TEXT_GUILD_BANNER_ERR_NOTREWARDED,	//Your guild hasn't advanced enough to access that standard, standard tactic slot, or standard post yet.
        TEXT_GUILD_BANNER_ERR_TACTICINUSE,	//One or more of your selected tactics is already in use by another standard.
        TEXT_GUILD_BANNER_ERR_LOCKED,	//That standard was recently edited. You must wait for a period of no less than 24 hours before your guild can edit it again.
        TEXT_GUILD_BANNER_PERMSERR,	//You do not have permission to manage the guild's standards.
        TEXT_GUILD_CAL_NOTREWARDED,	//Your guild has not advanced enough to access the guild calendar.
        TEXT_GUILD_CAL_ALL_EDITERR,	//This event is owned by an alliance and cannot be edited.
        TEXT_GUILD_CAL_ATTSUCCESS,	//You have successfully set the player's attendance flag.
        TEXT_GUILD_CAL_PLAYERNOTTHERE,	//The player is not signed up to this guild calendar event.
        TEXT_GUILD_CAL_INVALIDEVENT,	//The event you're trying to access is invalid.
        TEXT_GUILD_CAL_PERMSERR,	//You do not have permission to use the guild calendar.
        TEXT_GUILD_CAL_KICKSUCCESS,	//You have successfully kicked the player from the guild calendar event.
        TEXT_GUILD_CAL_NOTSIGNEDUP,	//You are not signed up to that guild calendar event!
        TEXT_GUILD_CAL_LEAVESUCCESS,	//You have successfully left the guild calendar event.
        TEXT_GUILD_CAL_ALREADYSIGNEDUP,	//You are already signed up to that event.
        TEXT_GUILD_CAL_SIGNUPSUCCESS,	//You have successfully signed up to the guild calendar event!
        TEXT_GUILD_CAL_SIGNUPERR,	//This event is owned by an alliance guild and does not allow outsider sign-ups.
        TEXT_GUILD_CAL_REMOVESUCCESS,	//Your event was successfully removed from the guild calendar.
        TEXT_GUILD_CAL_EDITSUCCESS,	//Your event was successfully edited in the guild calendar.
        TEXT_GUILD_CAL_INVALIDDATE,	//Your calendar event had an invalid date.
        TEXT_GUILD_CAL_INVALIDTITLE,	//Your calendar event had an invalid title.
        TEXT_GUILD_CAL_UNKNOWNERROR,	//An unknown error occurred while trying to add your new guild calendar event.
        TEXT_GUILD_CAL_EVENTMAX,	//Your guild had already reached its calendar event maximum.
        TEXT_GUILD_CAL_ADDSUCCESS,	//Your new event was successfully added to the guild calendar!
        TEXT_ALLIANCE_ERR_NOALLIANCE,	//Your guild is not in an alliance!
        TEXT_ALLIANCE_ERR_POLL_VOTE,	//You do not have permission to cast a vote to alliance polls on behalf of your guild.
        TEXT_ALLIANCE_POLL_LOCKED,	//The poll you attempted to cast a vote on is locked.
        TEXT_ALLIANCE_POLL_TOOMANYVOTES,	//There are already enough votes on this poll for the results to be tallied.
        TEXT_ALLIANCE_POLL_VOTEALREADYCAST,	//Your guild has already cast its vote on this poll.
        TEXT_ALLIANCE_POLL_VOTEERRSELF,	//Your guild is the subject of the poll and may not cast a vote.
        TEXT_ALLIANCE_POLL_NOTFOUND,	//The specified poll was not found.
        TEXT_ALLIANCE_POLL_VOTESUCCESS,	//Your vote was successfully cast to the alliance poll on behalf of your guild.
        TEXT_ALLIANCE_POLL_DELETE,	//Only the guild that originally created the poll may delete it.
        TEXT_ALLIANCE_POLL_DELSUCCESS,	//The specified poll was successfully removed.
        TEXT_ALLIANCE_POLL_CREATEERR,	//You do not have permission to create new polls for your alliance.
        TEXT_ALLIANCE_POLL_INSERTERR,	//An unknown error occurred while trying to process this poll.
        TEXT_ALLIANCE_POLL_BADPERCENT,	//An inappropriate success percentage was provided for this poll.
        TEXT_ALLIANCE_POLL_INVALIDTIMER,	//An inappropriate maximum timer was provided for this poll.
        TEXT_ALLIANCE_POLL_INVALIDTYPE,	//An inappropriate poll type was provided.
        TEXT_ALLIANCE_POLL_MAXKICKPOLLS,	//Your alliance already has the maximum allowed kick polls.
        TEXT_ALLIANCE_POLL_MAXPOLLS,	//Your alliance already has the maximum allowed polls.
        TEXT_ALLIANCE_POLL_ADDSUCCESS,	//The alliance poll was successfully added.
        TEXT_ALLIANCE_RANK_ERR_DEMOPERM,	//You do not have permission to demote alliance ranks.
        TEXT_GUILD_PERM_RESETERR,	//Only the guild leader can reset the permissions for all ranks to their defaults.
        TEXT_GUILD_PERM_ERR_NOPERM,	//You do not have permission to edit guild rank permissions.
        TEXT_GUILD_RANK_ERR_NOPERM,	//You do not have permission to add or remove guild ranks.
        TEXT_GUILD_RANK_SUCCESS_NAME,	//You have successfully changed the selected rank title.
        TEXT_ALLIANCE_RANK_CHANGE_OK,	//The player's alliance rank was successfully changed.
        TEXT_ALLIANCE_RANKERR_OMAX,	//Your guild already has the maximum number of allowed alliance officers (5).
        TEXT_ALLIANCE_RANKERR_GLMAX,	//Your guild already has the maximum number of allowed alliance leaders (1).
        TEXT_ALLIANCE_RANK_ERR_PROMPERM,	//You do not have permission to promote alliance ranks.
        TEXT_ALLIANCE_FORM_ERR_LEADER,	//You must be the leader of a guild in order to form an alliance.
        TEXT_ALLIANCE_FORM_ERR_ALLIED,	//Your guild is already in an alliance!
        TEXT_ALLIANCE_FORM_ERR_TLEADER,	//The specified player is not a guild leader. You can only form an alliance with another guild leader.
        TEXT_ALLIANCE_FORM_ERR_TALLIED,	//The specified player's guild is already in an alliance.
        TEXT_GUILD_NOTES_NOPERMS,	//You do not have permission to edit that note.
        TEXT_GUILD_NOTES_NONAME,	//You must provide the name of the player whose note you want to edit even if that player is yourself.
        TEXT_ALLIANCE_KICK_ERR_PERM,	//You do not have permission to kick other guilds from the alliance.
        TEXT_ALLIANCE_KICK_ERR_NOTFOUND,	//<<1>> is not a member of your alliance.
        TEXT_SN_LFG_OFF,	//You are no longer looking for a party.
        TEXT_SN_LFG_ON,	//You are now looking for a party.
        TEXT_GUILD_BNR_ERR_GENERAL,	//An error was encountered while trying to configure the guild standard.
        TEXT_GUILD_NOT_IN_A_GUILD,	//You are not in a guild.
        TEXT_ALLIANCE_FORM_ERR_SELF,	//You cannot form an alliance by yourself!
        TEXT_ERROR_PLAYER_NOT_FOUND,	//Player <<1>> was not found!
        TEXT_ALLIANCE_NAME_UNACCEPTED,	//The specified alliance name is either a duplicate or contains unapproved language.
        TEXT_ALLIANCE_INV_ERR_ENEMY,	//You cannot invite enemy players to join your alliance.
        TEXT_ALLIANCE_FORM_ERR_NOACCESS,	//Your guild's rank is not high enough to form an alliance yet!
        TEXT_ALLIANCE_FORM_ERR_TACCESS,	//<<1>> is in a guild whose rank is not high enough to form alliances.
        TEXT_SN_IGNORED_WARNING,	//<<1>> is ignoring you.
        TEXT_ALLIANCE_FORM_BEGIN,	//You ask <<1>> to form a new alliance with your guild.
        TEXT_ALLIANCE_FORM_DECLINE_SELF,	//You have declined the formation of the alliance.
        TEXT_ALLIANCE_FORM_ERR_DECLINED,	//<<1>> declined the formation of your alliance.
        TEXT_ALLIANCE_FORM_ACCEPTED,	//<<1>> accepted the formation of your alliance.
        TEXT_ALLIANCE_FORM_SUCCESS,	//The <<1>> alliance has been successfully formed!
        TEXT_ALLIANCE_INVITE_SUCCESS,	//You have joined the <<1>> alliance!
        TEXT_ALLIANCE_INV_ERR_NOGUILD,	//<<1>> is not in a guild.
        TEXT_ALLIANCE_INV_ERR_SELF,	//You cannot invite yourself to join your own alliance.
        TEXT_ALLIANCE_INV_ERR_MAX,	//Your alliance has already met the maximum number of members allowed (10).
        TEXT_ALLIANCE_INV_ERR_PERM,	//You do not have permission to invite new guilds to join your alliance.
        TEXT_ALLIANCE_INV_ERR_NOINVITE,	//You do not have an invitation to join an alliance.
        TEXT_ALLIANCE_INVITE_DECLINED,	//<<1>> has declined your invitation.
        TEXT_GUILD_MOTD_TOOLONG,	//The guild message of the day you provided was too long.
        TEXT_GUILD_EMAIL_TOOLONG,	//The guild email you provided was too long.
        TEXT_GUILD_WEBPAGE_TOOLONG,	//The guild webpage address you provided was too long.
        TEXT_GUILD_DETAILS_TOOLONG,	//The guild details you provided were too long.
        TEXT_GUILD_RANKNAME_ERROR,	//Unable to edit the guild rank. You either have no permissions, or the rank name was not accepted.
        TEXT_SN_LFG_ALREADYGROUPED,	//You cannot look for a party while in one!
        TEXT_SN_LFG_ERR_HIDDEN,	//You cannot look for a party while hidden.
        TEXT_SN_LFM_NOGROUP,	//You cannot set recruitment settings if you are not interested in forming an open party, looking for a party, or actually in a party or warband.
        TEXT_SN_LFM_NOTLEADER,	//Only the leader can set the recruitment options.
        TEXT_SN_LFM_ERR_HIDDEN,	//Your party cannot look for recruits while the leader is hidden.
        TEXT_GUILD_INVITE_ERR_SELF,	//You cannot invite yourself to your own guild.
        TEXT_GUILD_INVITE_ERR_ENEMY,	//You cannot invite enemy players to join your guild.
        TEXT_GUILD_INVITE_ERR_GUILDED,	//Player <<1>> is already in a guild.
        TEXT_GUILD_INVITEPERM_ERROR,	//You do not have permission to invite players to join your guild.
        TEXT_GROUP_INVITE_ERR_SELF,	//You cannot invite yourself to join your own party.
        TEXT_GROUP_INVITE_ERR_ENEMY,	//You cannot invite enemy players to join your party.
        TEXT_GROUP_INVITE_ERR_GROUPED,	//<<1>> is already in a party, led by <<2>>.
        TEXT_GROUP_REFER_ERR_ENEMY,	//You cannot refer enemy players to your party's leader.
        TEXT_GROUP_REFER_ERR_SELF,	//You cannot fer yourself to your party's leader.
        TEXT_GROUP_INVITE_BEGIN,	//You invited <<1>> to join your party.
        TEXT_GUILD_INVITE_BEGIN,	//You invited <<1>> to join your guild.
        TEXT_GROUP_REFER_NOTICE_SELF,	//You have referred <<1>> to your party's leader.
        TEXT_GROUP_REFER_NOTICE_INVITEE,	//<<1>> has referred you to a party leader for an invitation.
        TEXT_GROUP_INVITE_ERR_NOINVITE,	//You do not have an invitation to join a party.
        TEXT_GUILD_INVITE_ERR_NOINVITE,	//You do not have an invitation to join a guild.
        TEXT_ERROR_INVITE_DECLINED,	//<<1>> has declined your invitation.
        TEXT_GROUP_NOT_LEADER,	//You are not a party leader.
        TEXT_GROUP_KICK_ERR_INVALIDSLOT,	//You tried to kick an invalid entry from your party!
        TEXT_ALLIANCE_INVITE_BEGIN,	//You have invited <<1>>'s guild to join your alliance.
        TEXT_ALLIANCE_NO_OFFICERCHAT,	//You cannot speak in alliance officer chat.
        TEXT_ALLIANCE_NO_ACHAT,	//You cannot speak in alliance chat.
        TEXT_GUILD_NOTES_SUCCESS,	//You have successfully edited the note.
        TEXT_GUILD_NO_OFFICERCHAT,	//You cannot speak in guild officer chat.
        TEXT_GUILD_NO_GUILDCHAT,	//You cannot speak in guild chat.
        TEXT_GUILD_RANK_ERR_NOTOGGLE,	//The rank you have chosen cannot be disabled.
        TEXT_GUILD_RANK_ERR_NOTEMPTY,	//The rank you have chosen has members in it and cannot be disabled until they are either promoted or demoted to another rank.
        TEXT_GUILD_RANK_SUCCESS_ADD,	//The rank has been successfully added.
        TEXT_GUILD_RANK_SUCCESS_REMOVE,	//The rank has been successfully removed.
        TEXT_GUILD_PERM_ERR_LEADER,	//The guild leader's permissions may not be edited.
        TEXT_GUILD_PERM_ERR_NOCHANGE,	//The permission you have tried to change cannot be edited from its default setting.
        TEXT_GUILD_PERM_SUCCESS,	//You have successfully changed the permission for this rank.
        TEXT_GUILD_PERM_RESETSUCCESS,	//You have successfully reset the permissions for all ranks to their default settings.
        TEXT_ALLIANCE_DOESNT_EXIST,	//The alliance your guild was in no longer exists.
        TEXT_GUILD_DOESNT_EXIST,	//The guild you were in no longer exists.
        TEXT_GUILD_APPROVAL_BEGIN,	//The guild approval process will now begin. Awaiting responses from party members...
        TEXT_GUILD_HERALDRY_ALREADY_EXISTS,	//That heraldry configuration is already owned by another guild on your server!
        TEXT_SLOW_DOWN_CHAT_THROTTLE,	//Slow down! Think before you say each word!
        TEXT_NO_CHAT_WHILE_DEAD,	//You cannot do that while dead!
        TEXT_PLAYER_MUTED,	//You cannot do that right now!
        TEXT_NEW_PARTY_LEADER,	//<<1>> is now the party's leader!
        TEXT_GUILD_INVITE_ERR_NOTARGET,	//You have not specified a player to invite to join your guild.
        TEXT_GROUP_NOT_IN_ONE,	//You are not currently a member of a party.
        TEXT_GROUP_ALREADY_IN_ONE,	//You are already in a party!
        TEXT_GROUP_INVITE_ERR_INVITEGONE,	//You no longer have an invitation to join a party.
        TEXT_GROUP_QSHARE_ONLYWITHGROUP,	//You may only share quests with your party members.
        TEXT_INVALID_QUEST,	//The selected quest is invalid.
        TEXT_GROUP_QSHARE_TOOFARAWAY,	//<<1>> is too far away to share quests with.
        TEXT_GROUP_QSHARE_YOU_TOOFARAWAY,	//<<1>> is trying to share a quest, but you are too far away.
        TEXT_GROUP_QSHARE_YOU_SHARE,	//You are now sharing a quest with <<1>>...
        TEXT_GROUP_QSHARE_SHARING,	//<<1>> is sharing the quest <<2>>.
        TEXT_GROUP_QSHARE_UNKNOWNERR,	//You could not share that quest with <<1>>.
        TEXT_GROUP_QSHARE_ALREADYHAS,	//<<1>> is already on that quest.
        TEXT_GROUP_QSHARE_PLAYER_INELIGIBLE,	//<<1>> is not eligible for that quest.
        TEXT_GROUP_QSHARE_YOU_INELIGIBLE,	//<<1>> is sharing the quest <<2>>, but you are ineligible.
        TEXT_GROUP_QSHARE_PLAYER_TOOMANYQUESTS,	//<<1>> has too many quests and cannot possibly take on another.
        TEXT_GROUP_QSHARE_YOU_TOOMANYQUESTS,	//<<1>> is sharing the quest <<2>>, but you have too many quests at this time.
        TEXT_GROUP_QSHARE_PLAYER_TOOMANYCAREERQUESTS,	//<<1>> has too many career quests.
        TEXT_GROUP_QSHARE_YOU_TOOMANYCAREERQUESTS,	//<<1>> is sharing the quest <<2>>, but you have too many career quests at this time.
        TEXT_YOU_WILL_LOG_OUT_IN_X_SECONDS,	//You will finish logging out in <<1>> <<1[second/seconds]>>.
        TEXT_CANCELLED_LOGOUT,	//You are no longer logging out.
        TEXT_GROUP_INVITE_ERR_NOTARGET,	//You have not specified a player to invite to join your party.
        TEXT_PLAYER_RECLAIMED_BANNER,	//You have successfully reclaimed the guild standard!
        TEXT_PLAYER_CAPTURED_BANNER,	//You have successfully captured an enemy standard!
        TEXT_BATTLEFIELD_OBJECTIVE_TAKEN,	//The forces of <<1>> have taken <<2>>!
        TEXT_MARSHALS_KILLED,	//The marshals of City have been killed, and the King is under attack!
        TEXT_PAIRING_DWARFS_AND_GREENSKINS,	//Dwarfs & Greenskins
        TEXT_PAIRING_EMPIRE_AND_CHAOS,	//Empire & Chaos
        TEXT_PAIRING_HIGHELVES_AND_DARKELVES,	//High Elves & Dark Elves
        TEXT_GUILD_RANK_INCREASED,  //Your guild has earned rank <<1>>!

        //=========================
        // APOTHECARY
        //=========================
        TEXT_CRAFT_ALREADY_HAS_CONTAINER,	//Already has container.

        TEXT_CRAFT_NOT_CRAFT_OBJ,	//Item is not a crafting object.
        TEXT_CRAFT_SKILL_TOO_LOW,	//Your <<1>> skill is too low to use this item!
        TEXT_CRAFT_ITEM_ALREADY_IN_CONTAINER,	//Item already in container.
        TEXT_CRAFT_STACK_IN_CONTAINER,	//Entire stack already in container.
        TEXT_CRAFT_MISMATCH_TYPE,	//Mis-matching container and ingredient type.
        TEXT_CRAFT_MAIN_INGREDIENT_FIRST,	//Please add main ingredient before other ingredients.
        TEXT_CRAFT_ALREADY_HAS_MAIN_INGREDIENT,	//Already has a main ingredient.
        TEXT_CRAFT_NO_ROOM_IN_BACKPACK,	//Not enough room in your backpack.
        TEXT_CRAFT_INVALID_PRODUCT,	//Invalid product.
        TEXT_CRAFT_YOU_CREATED,	//You created <<1>> <<2>>.
        TEXT_CRAFT_FAIL,	//You have failed to create a stable recipe.
        TEXT_CRAFT_NOT_CONTAINER,	//Item is not a container.
        TEXT_CRAFT_INCORRECT_CONTAINER_TYPE,	//Incorrect container type.
        TEXT_CRAFT_CONTAINER_FIRST,	//You must add a container first.
        TEXT_CRAFT_NOTHING_THERE,	//Nothing to remove.
        TEXT_CRAFT_CANT_PERFORM,	//Nothing to perform.
        TEXT_SCENARIO_SHUTDOWN_IMBALANCED,	//The scenario population is currently imbalanced.  If balance is not restored, the scenario will shut down in 60 seconds.

        //=========================
        // WARBANDS
        //=========================
        TEXT_BG_NOT_IN_GROUP,	//You must be in a party to create a warband.

        TEXT_BG_ALREADY_IN,	//You are already in a warband.
        TEXT_BG_NOT_GROUP_LEADER,	//You must be the party leader to create a warband.
        TEXT_BG_UNSPECIFIED_ERROR,	//An unspecified warband error was encountered.
        TEXT_BG_NOT_IN_BG,	//You are not in a warband.
        TEXT_BG_PLAYER_NOT_IN_BG,	//That player is not in your warband.
        TEXT_BG_NOT_ASSISTANT,	//You must be a warband assistant to do that.
        TEXT_BG_NOT_LEADER,	//You must be the warband leader to do that.
        TEXT_BG_ALREADY_ASSISTANT,	//That player is already an assistant.
        TEXT_BG_PLAYER_NOT_ASSISTANT,	//That player is not an assistant.
        TEXT_BG_LEADER_PROMOTE,	//You have been promoted to warband leader.
        TEXT_BG_ASSISTANT_PROMOTE,	//You have been promoted to warband assistant.
        TEXT_BG_ASSISTANT_DEMOTE,	//You are no longer a warband assistant.
        TEXT_BG_LEADER_DEMOTE,	//You are no longer the warband leader.
        TEXT_BG_YOU_LEFT,	//You have left the warband.
        TEXT_BG_PLAYER_ALREADY_IN_BG,	//That player is already in your warband.
        TEXT_BG_PLAYER_IN_ANOTHER_BG,	//That player is in another warband.
        TEXT_BG_PLAYER_IN_ANOTHER_GROUP,	//That player is currently in another party.
        TEXT_BG_PLAYER_ALREADY_INVITED,	//That player has already been invited to your warband.
        TEXT_BG_PLAYER_PENDING_ANOTHER,	//That player is currently considering an invite to another warband.
        TEXT_BG_YOU_WERENT_INVITED,	//You do not have a pending warband invite.
        TEXT_BG_PLAYER_COULDNT_ACCEPT,	//The player wasn't able to join your warband.
        TEXT_BG_YOU_ALREADY_IN_BG,	//You are already in a warband.
        TEXT_BG_YOU_IN_ANOTHER_GROUP,	//You must leave your party first before joining a warband.
        TEXT_BG_INVALID_INVITE,	//You weren't able to join the warband, it may have been disbanded.
        TEXT_BG_YOU_WERE_ADDED,	//You were added to the warband.
        TEXT_BG_ACCEPTED_YOUR_INVITE,	//<<1>> accepted your invite.
        TEXT_BG_COULDNT_ADD_YOU,	//You were unable to join the warband, it may be full.
        TEXT_BG_YOU_DECLINED,	//You declined the invitation to join the warband.
        TEXT_BG_DECLINED_YOUR_INVITE,	//<<1>> has declined your invitation to join the warband.
        TEXT_BG_YOUR_INVITE_CANCELLED,	//Your warband invitation has been cancelled, the warband was disbanded.
        TEXT_BG_YOUR_INVITE_TIMEDOUT,	//Your warband invitation has expired.
        TEXT_BG_PLAYER_INVITE_TIMEDOUT,	//<<1>>'s invitation to join your warband has expired.
        TEXT_BG_YOU_WERE_INVITED,	//<<1>> has invited you to join a warband.
        TEXT_BG_INVALID_MOVE_INDEX,	//The player could not be moved to that party.
        TEXT_BG_CANT_KICK_ASSISTANT,	//That player is an assistant. Only the leader may kick an assistant from the warband.
        TEXT_BG_ERR_ENEMY,	//That player is your enemy.
        TEXT_BG_ERR_PRIVATE,	//That warband is private.
        TEXT_BG_ERR_INVALID_PASSWORD,	//Invalid password.
        TEXT_BG_ERR_PLAYER_NOT_IN_BG,   //That player is not in a warband.

        //=========================
        TEXT_SCENARIO_JOIN_SOLO,	//You have joined a scenario queue for <<1>>

        TEXT_SCENARIO_JOIN_GROUP,	//Your party has joined a scenario queue for <<1>>
        TEXT_SCENARIO_JOIN_GROUP_LEADER,	//You and your party are now in a scenario queue
        TEXT_RECEIVED_ITEM_X_FROM_PLAYER_Y,	//You received <<1>> from <<2>>.
        TEXT_TRADED_ITEM_X_TO_PLAYER_Y,	//You traded <<1>> to <<2>>.
        TEXT_PLAYER_NOT_BANNER_OWNER,	//Only the owner, members of the owner's party, or guildmates can retrieve the standard!
        TEXT_MERCHANT_PURCHASED_ITEM_X_FROM_Y_FOR_Z_BRASS,	//You purchased <<1>><<3[// x$d]>> from <<2>> for <<4>>.
        TEXT_MERCHANT_SOLD_ITEM_X_TO_Y_FOR_Z_BRASS,	//You sold <<1>><<3[// x$d]>> to <<2>> for <<4>>.
        TEXT_ACTION_LEVEL_ERROR_ALREADY_HIGHER,	//Unable to set rank.  <<1>> is already at a rank higher than <<2>>.
        TEXT_ACTION_LEVEL_ERROR_CAP_EXCEEDED,	//Unable to set rank.  The maximum rank is <<1>>.
        TEXT_SALVAGING_SKILL_TOO_LOW,	//Your skill rank is too low to salvage this item.
        TEXT_SALVAGING_SKILL_UP,	//Your salvaging skill has to be improved to rank <<1>>.
        TEXT_CAPITOL_DEFENDED,	//<<1>> has been defended!
        TEXT_YOU_RECEIVE_ITEM_X,	//You receive <<a:1>> (<<2>>).
        TEXT_PLAYER_X_RECEIVES_ITEM_X,	//<<C:1>> receives <<a:2>> (<<3>>).
        TEXT_YOU_HAVE_X_UNREAD_MESSAGES_IN_YOUR_MAILBOX,	//You have <<1>> unread <<1[message/messages]>> in your inbox.
        TEXT_KILLING_SPREE_ADVANCED,	//Your killing spree has advanced to stage <<1>>. (+<<2>>%% XP).
        TEXT_KILLING_SPREE_ENDED,   //Your killing spree has ended.

        //=========================
        // APPEAL SYSTEM
        //=========================
        TEXT_COPTER_UNUSED1,	//UNUSED

        TEXT_COPTER_UNUSED2,	//UNUSED
        TEXT_COPTER_UNUSED3,	//UNUSED
        TEXT_COPTER_UNUSED4,	//UNUSED
        TEXT_COPTER_UNUSED5,	//UNUSED
        TEXT_COPTER_NAMING_APPEAL_SENT,	//Sending your TOS naming report to Customer Support.
        TEXT_COPTER_UNUSED6,	//UNUSED
        TEXT_COPTER_UNUSED7,	//UNUSED
        TEXT_COPTER_HARASSMENT_APPEAL_SENT,	//Sending your TOS harassment report to Customer Support.
        TEXT_COPTER_UNUSED8,	//UNUSED
        TEXT_COPTER_STUCK_APPEAL_SENT,	//Sending your 'stuck' appeal to Customer Support.
        TEXT_COPTER_EMERGENCY_APPEAL_SENT,	//Sending your emergency appeal to Customer Support.
        TEXT_COPTER_OTHER_APPEAL_SENT,	//Sending your appeal to Customer Support.
        TEXT_COPTER_UNUSED9,	//UNUSED
        TEXT_COPTER_UNUSED10,	//UNUSED
        TEXT_COPTER_QUEST_BUG_REPORT_SENT,	//Sending your quest bug report to Customer Support.
        TEXT_COPTER_CHARACTER_BUG_REPORT_SENT,	//Sending your character bug report to Customer Support.
        TEXT_COPTER_CRASH_BUG_REPORT_SENT,	//Sending your crash bug report to Customer Support.
        TEXT_COPTER_ART_BUG_REPORT_SENT,	//Sending your art bug report to Customer Support.
        TEXT_COPTER_ITEM_BUG_REPORT_SENT,	//Sending your item bug report to Customer Support.
        TEXT_COPTER_MONSTER_BUG_REPORT_SENT,	//Sending your monster bug report to Customer Support.
        TEXT_COPTER_PATHING_BUG_REPORT_SENT,	//Sending your pathing bug report to Customer Support.
        TEXT_COPTER_OTHER_BUG_REPORT_SENT,	//Sending your bug report to Customer Support.
        TEXT_COPTER_YOUR_REPORT_WILL_BE_REVIEWED_ASAP,	//A CSR will review your report as soon as possible.
        TEXT_COPTER_YOU_MAY_NOT_BE_CONTACTED_IN_GAME,	//Please note that you may not be contacted in-game regarding this issue.
        TEXT_COPTER_THANKS_FOR_THE_REPORT,	//Thank you for the report!
        TEXT_COPTER_UNUSED11,	//UNUSED
        TEXT_COPTER_YOU_CANCELED_YOUR_APPEAL,	//You have canceled your active appeal.
        TEXT_COPTER_UNUSED13,	//UNUSED
        TEXT_COPTER_UNUSED14,	//UNUSED
        TEXT_COPTER_NEW_NOTE_ADDED_TO_YOUR_APPEAL,	//New information is being appened to your active appeal as a note.
        TEXT_COPTER_SUPPORT_COMMAND_SPAM_WARNING,	//You have submitted a large number of reports in a short period of time. Future reports from your account will not be accepted until we've reviewed your current submissions.
        TEXT_COPTER_SUPPORT_COMMAND_ABUSE_WARNING,	//Abuse of in-game customer support and bug reporting commands could result in action taken against your account.
        TEXT_COPTER_YOUR_APPEAL_HAS_MAXIMUM_NOTES,	//Your appeal already has the maximum number of notes appended to it.
        TEXT_COPTER_YOU_HAVE_AN_ACTIVE_APPEAL,	//You have an active appeal in the CS queue.
        TEXT_COPTER_APPEAL_PRIORITY_POLICY,	//Your appeal will be addressed by the next available CSR. CSRs take appeals based on their priority. Your appeal is important, and will be handled as soon as possible. Thank you!
        TEXT_COPTER_YOU_DO_NOT_HAVE_AN_ACTIVE_APPEAL,	//You do not have an active appeal in the CS queue.
        TEXT_COPTER_UNUSED15,	//UNUSED
        TEXT_COPTER_UNUSED16,	//UNUSED
        TEXT_COPTER_UNUSED17,	//UNUSED
        TEXT_COPTER_A_CSR_WILL_CONTACT_YOU_ASAP,    //A CSR will contact you as soon as possible. Thank you!

        //=========================
        TEXT_RESPAWNING,	//Respawning...

        TEXT_CAREER_RESPEC_NOTIFICATION,	//Your career has been updated and your advancement points have been refunded.
        TEXT_LEAVE_ZONE_CONFIRMATION,	//Are you sure you want to leave? Leaving may incur penalties.
        TEXT_NAME_X_HAS_OFFERED_YOU_RES,	//<<C:1>> has offered you resurrection. Do you accept?
        TEXT_SOMEONE_OFFERED_YOU_RES,	//You have been offered resurrection. Do you accept?
        TEXT_GROUP_INVITE_CONFIRMATION,	//<<C:1>> has invited you to join a party. Do you accept?
        TEXT_GROUP_REFERRAL_CONFIRMATION,	//<<C:1>> has referred <<2>> to join the party. Do you accept?
        TEXT_GUILD_INVITE_CONFIRMATION,	//<<C:1>> has invited you to join the guild "<<2>>." Do you accept?
        TEXT_ALLIANCE_INVITE_CONFIRMATION,	//<<C:1>> has invited you to join an alliance. Do you accept?
        TEXT_WARBAND_INVITE_CONFIRMATION,	//<<C:1>> has invited you to join a warband. Do you accept?
        FORMAT_ENTITY_NAME,	//<<1>> <<2>>
        FORMAT_RACE_NAME,	//<<1>>
        FORMAT_CAREER_NAME,	//<<1>>
        FORMAT_RANK_NAME,	//<<1>>
        FORMAT_TITLE,	//<<1>>
        FORMAT_LOCATION_NAME,	//<<1>>
        TITLE_GUILD_FORMAT,	//< <<1>> >
        TEXT_GROUP_ERROR_MERGE_WITH_BG,	//A party cannot merge with a warband.
        TEXT_GROUP_ERROR_MERGE_TOO_MANY_PLAYERS,	//The parties have too many players to merge.
        ERROR_CAREER_PACKAGE_HAS_NOT_UNLOCKED_TOK_ENTRY,	//You have not unlocked the required Tome of Knowledge entry.
        ERROR_CAREER_PACKAGE_TOO_LOW_ACTION_COUNT,	//You do not meet the Action Counter requirement.
        TEXT_CURRENT_LANGUAGE_INFO,	//The current language is "<<1>>" (<<2>>).
        TEXT_UNKNOWN_LANGUAGE,	//Unknown language.
        TEXT_UNKNOWN_COMMAND,	//Unknown command.
        TEXT_UNKNOWN_COMMAND_X,	//Unknown command: <<1>>.
        TEXT_COMMAND_X,	//Command: <<1>><<2[ (operator)/]>>
        TEXT_COMMAND_USAGE,	//Usage: <<1>>
        TEXT_HELP_COMMAND_NO_COMMANDS_FOUND,	//No matching commands found. Type "<<1>>" (no parameters) for a list of all commands.
        TEXT_HELP_COMMAND_SHOWING_CLOSE_MATCHES,	//No exact match found; substring matches:
        TEXT_HELP_COMMAND_ALL_COMMANDS,	//All commands:
        TEXT_HELP_COMMAND_FOUND_X_COMMANDS,	//Found <<1>> commands.
        TEXT_CRAFT_SPECIALMOMENT,	//Special Moment.
        TEXT_CRAFT_CRITICALSUCCESS,	//Critical Success.
        TEXT_CRAFT_CRITICALFAILURE,	//Critical Failure.
        TEXT_OUTER_DOOR_X_UNDER_ATTACK_BY_Y,	//The outer doors of <<1>> are under attack. There <<2[was one enemy/were $d enemies]>> spotted in the area.
        TEXT_INNER_DOOR_X_UNDER_ATTACK_BY_Y,	//The inner doors of <<1>> are under attack. There <<2[was one enemy/were $d enemies]>> spotted in the area.
        TEXT_LORD_OF_X_UNDER_ATTACK_BY_Y,	//The lord of <<1>> is under attack. There <<2[was one enemy/were $d enemies]>> spotted in the area.
        TEXT_PLAYED_X_DAYS_Y_HOURS_Z_MINUTES,	//You have played for <<1[/$d day, /$d days, ]>> <<2[/$d hour, /$d hours, ]>> <<3[$d minute/$d minutes]>>.
        TEXT_GUILD_CLAIM_ERR_NOCLAIM,	//Your guild does not hold any claim over this war objective.
        TEXT_GUILD_CLAIM_ERR_MAXCLAIMS,	//Your guild already has the maximum allowed claims on war objectives.
        TEXT_GUILD_CLAIM_ERR_NOPERM,	//You do not have permission to claim or unclaim war objectives for your guild.
        TEXT_GUILD_CLAIM_ERR_BAD_BANNERSTATUS,	//Your status as a standard bearer does not permit you to claim a war objective at this time.
        TEXT_GUILD_UNCLAIM_KEEP_PENALTY_CONFIRMATION,	//Remotely releasing will temporarily disable the standard bearer from using standards. Continue?
        TEXT_GUILD_UNCLAIM_KEEP_NOPENALTY_CONFIRMATION,	//Are you sure you want to release your guild's claim over this war objective?
        TEXT_GUILD_KICK_ERR_MEMBER_CLAIMING,	//You cannot kick members from your guild who are claiming war objectives. You must release the war objective from your guild's ownership before kicking this member.
        TEXT_GUILD_LEAVE_ERR_MEMBER_CLAIMING,	//You cannot leave your guild while claiming a war objective on its behalf. Release your claim before leaving your guild!
        TEXT_GUILD_ASSIGN_BANNER_CARRIER_ERR,	//You must designate a guild member to assign as a standard bearer.
        TEXT_GUILD_CLAIM_ERR_SEIZETIMER,	//This war objective has not been held by your realm long enough for you to claim it just yet. Please wait a moment and try again.
        TEXT_GUILD_ADV_ERR_NOPERMS,	//You do not have permission to purchase guild tactics for your guild.
        TEXT_GUILD_ADV_ERR_NOTENOUGHPTS,	//Your guild does not have enough tactic points available to purchase this tactic.
        TEXT_GUILD_ADV_ERR_INVALIDABIL,	//The tactic you have tried to purchase for your guild is not a valid guild tactic.
        TEXT_GUILD_ADV_ERR_ALREADYPURCH,	//The tactic you are trying to purchase has already been bought by your guild.
        TEXT_GUILD_ADV_ERR_PREREQUISITE,	//The tactic you are trying to purchase has a prerequisite tactic that has not yet been purchased!
        TEXT_GUILD_ADV_TACTICSUCCESS,	//You have successfully purchased the tactic for your guild. You may now use the standard editor to place it on a guild standard!
        TEXT_GUILDNEWS_RVR_CLAIM,	//[<<1>>] The guild has claimed <<2>>!
        TEXT_GUILDNEWS_RVR_UNCLAIM,	//[<<1>>] The guild has relenquished its claim over <<2>>.
        TEXT_GUILDNEWS_RVR_CLAIM_SEIZED,	//[<<1>>] <<2>> has been seized by the enemy realm!
        TEXT_CRAFT_PIGMENT_FIRST,	//Please add pigment first.
        TEXT_CRAFT_ALREADY_HAS_PIGMENT,	//Already has a pigment.
        TEXT_INSTANCE_WITH_GROUPMATES_IS_FULL,	//The instance with your party is full. You have been sent to a different instance.
        TEXT_GROUP_YOU_WERE_INVITED,	//<<C:1>> has invited you to join a party.
        TEXT_BG_YOU_WERE_INVITED_MERGE,	//<<C:1>> has invited you to merge with <<o:n,1>> warband.
        TEXT_GROUP_YOU_WERE_INVITED_MERGE,	//<<C:1>> has invited you to merge with <<o:n,1>> party.
        TEXT_GROUP_INVITE_MERGE_CONFIRMATION,	//<<C:1>> has invited you to merge with <<o:n,1>> party.  Do you accept?
        TEXT_BG_INVITE_MERGE_CONFIRMATION,	//<<C:1>> has invited you to merge with <<o:n,1>> warband.  Do you accept?
        TEXT_GUILD_CLAIM_ERR_ALREADYCLAIMED,	//Your guild has already claimed this war objective!
        TEXT_GUILD_CLAIM_KEEP_CONFIRMATION,	//Claiming this Keep will use up your standard and will charge your Guild a base periodic upkeep cost of <<1>>. Continue?
        TEXT_GUILD_CLAIM_ERR_UNDERATTACK,	//This war objective is under attack! You cannot claim it until you deal with the enemy forces!
        TEXT_GUILD_UNCLAIM_ERR_UNDERATTACK,	//This war objective is under attack! You cannot unclaim it until you deal with the enemy forces!
        TEXT_PLAYER_BANNER_CAPTUREERR_SBALIVE,	//You must kill the bearer of the standard before capturing it!
        TEXT_PLAYER_BANNER_RECLAIMERR_SBALIVE,	//The standard bearer is still alive and defending the standard! You may not save it at this time.
        TEXT_CRAFT_GOLDWEED_FIRST,	//Please add goldweed first.
        TEXT_CRAFT_ALREADY_HAS_GOLDWEED,	//Already has a goldweed.
        TEXT_CRAFT_ALREADY_HAS_GOLDDUST,	//Already has a golddust.
        TEXT_CRAFT_ALREADY_HAS_QUICKSILVER,	//Already has a quicksilver.
        TEXT_CRAFT_CULTIVATION_HARVEST,	//You have harvested <<1>> <<2>><<1[//s]>>.
        TEXT_PLAYER_BANNER_CAPTUREERR_CHICKEN,	//You are too experienced for the area in which this standard was planted and cannot capture it!
        TEXT_PQ_EASY,	//Easy
        TEXT_PQ_MEDIUM,	//Medium
        TEXT_PQ_HARD,	//Difficult
        TEXT_PQ_VERYHARD,	//Very Difficult
        TEXT_PQ_UNKNOWN,	//Difficulty Unknown
        TEXT_INSTANCE_NEW,	//New
        TEXT_INSTANCE_AVAILABLE,	//Available
        TEXT_INSTANCE_FULL,	//Full
        TEXT_CRAFT_RECIPE_CRITSUCCESS,	//You made something better!
        TEXT_CRAFT_RECIPE_SUPERCRITSUCCESS,	//You made something awesome!
        TEXT_CRAFT_RECIPE_CRITFAILURE,	//Your creation failed.
        TEXT_INSTANCE_REQUIRES_GROUP,	//That instance requires that you are in a party.
        TEXT_INSTANCE_REQUIRES_WB,	//That instance requires that you are in a warband.
        TEXT_CRAFT_DESTROY_ON_FAIL,	//Your <<1>> was destroyed.
        TEXT_OPARTY_ERR_PLAYER_NOT_IN_OPWB,	//The player you specified is not in any public party or warband.
        TEXT_GROUP_ERR_PLAYER_NOT_IN_GROUP,	//<<C:1>> is not in a party.
        TEXT_OPARTY_ERR_ALLIANCE_ONLY,	//The public party or warband is restricted to the leader's alliance only.
        TEXT_OPARTY_ERR_GUILD_ONLY,	//The public party or warband is restricted to the leader's guild members only.
        TEXT_GROUP_ERR_ALREADY_MEMBER,	//You are already a member of that party or warband!
        TEXT_OPARTY_ERR_BANNED,	//You are banned from publicly joining that party or warband.
        TEXT_OPARTY_ERR_PASSWORD,	//The party you are trying to join has a password requirement. The password you specified is incorrect!
        TEXT_OPARTY_ERR_NOT_OPEN,	//That party is not public! You must be invited before joining it.
        TEXT_OPARTY_ERR_ENEMY,	//You may not join public parties of enemy players.
        TEXT_OPARTY_ERR_BAN_NOT_FOUND,	//That player is not part of your party or warband ban list.
        TEXT_OPARTY_BAN_REMOVED,	//You have successfully removed the player from your party or warband ban list.
        TEXT_OPARTY_ERR_BANADD_NOT_FOUND,	//The specified player was not found and cannot be placed on your party or warband ban list.
        TEXT_OPARTY_BAN_ADDED,	//You have successfully added the player to your party or warband ban list.
        TEXT_OPARTY_ALLIANCE_ONLY_ON,	//Access to join your public party or warband is now restricted only to members of your alliance.
        TEXT_OPARTY_ALLIANCE_ONLY_OFF,	//Access to join your public party or warband is no longer restricted to members of your alliance.
        TEXT_OPARTY_GUILD_ONLY_ON,	//Access to join your public party or warband is now restricted only to members of your guild.
        TEXT_OPARTY_GUILD_ONLY_OFF,	//Access to join your public party or warband is no longer restricted to members of your guild.
        TEXT_OPARTY_PASSWORD_CHANGED,	//The party's password settings have been successfully changed.
        TEXT_OPARTY_OPEN_ON,	//Your party is now flagged as public. Players may join without invitations.
        TEXT_OPARTY_OPEN_OFF,	//Your party is no longer public. Players must now be invited to join.
        TEXT_SN_OPENPARTYINTEREST_OFF,	//You are no longer interested in forming a public party.
        TEXT_SN_OPENPARTYINTEREST_ON,	//You are now flagged as interested in forming a public party. If a player selects to join you in your adventures, you will automatically be placed in a new party!
        TEXT_CRAFT_CANT_DROP_CULTIVATION,	//You can't drop cultivation when you have an active plot.
        TEXT_OPARTY_OPEN_FORMATION_SUCCESS,	//You have successfully formed a public party. Players may now join your party without an invitation.
        TEXT_OPARTY_PRIVATE_FORMATION_SUCCESS,	//You have successfully formed a private party. Players must have an invitation before joining.
        TEXT_PQ_FAILED,	//Failed
        TEXT_INSTANCE_OWNER_GROUP_TOO_MANY,	//Your party owns too many instances.
        TEXT_INSTANCE_OWNER_BG_TOO_MANY,	//Your warband owns too many instances.
        TEXT_OPARTY_RESULTS_TITLE,	//Open Party/Warband Results (<<1>> out of <<2>> maximum):
        TEXT_OPARTY_RESULT_WARBAND,	//[<<1>>] <<2>> (rank <<3>>) - Warband with <<4>> members - Distance = <<5>> seconds
        TEXT_OPARTY_RESULT_PARTY,	//[<<1>>] <<2>> (rank <<3>>) - Party with <<4>> members - Distance = <<5>> seconds
        TEXT_OPARTY_RESULTS_NONE,	//No results were found for this query!
        TEXT_OPARTY_GENERAL_ERR_PRIVATE,	//Only the leader of an open party, not currently in a warband, can change that setting!
        TEXT_OPARTY_GENERAL_ERR_OPEN,	//Only the leader of a private party, not currently in a warband, can change that setting!
        TEXT_OPARTY_GENERAL_ERR_PRIVATE_GUILDED,	//Only a guilded leader of an open party, not currently in a warband, can change that setting!
        TEXT_OPARTY_GENERAL_ERR_PRIVATE_ALLIED,	//Only a leader of an open party whose guild is in an alliance can change that setting!
        TEXT_GUILD_ADV_ERR_UPGRADED,	//One or more tactics you have attempted to slot onto this standard have been upgraded and are no longer available for use.
        TEXT_GUILD_BANNER_ERR_TACTICDUPE,	//You can only slot a guild tactic once per guild standard.
        TEXT_GUILD_HERALDRY_ALREADY_RESERVED,	//Your guild has already reserved its heraldry configuration and may not do so again.
        TEXT_CRAFT_FRAGMENT_FIRST,	//Please add fragment first.
        TEXT_CRAFT_ALREADY_HAS_FRAGMENT,	//Already has a fragment.
        TEXT_CRAFT_ALREADY_HAS_GOLDESSENCE,	//Already has a gold essence.
        TEXT_CRAFT_ALREADY_HAS_CURIO,	//Already has a curio.
        TEXT_CRAFT_ALREADY_HAS_MAGICESSENCE,	//Already has a magic essence.
        TEXT_MERCHANT_INSUFFICIENT_MONEY_TO_BUY,	//You don't have enough money to buy that.
        TEXT_MERCHANT_INSUFFICIENT_MONEY_TO_REPAIR,	//You don't have enough money to repair that.
        TEXT_MERCHANT_INSUFFICIENT_SPACE_TO_BUY,	//You don't have enough room in your backpack to buy that.
        TEXT_LOCKED_SCENARIO_REQUEUE,	//The scenario you were queued for has been locked, would you like to queue for another?
        TEXT_GUILD_PROMOTED_SB,	//You have been assigned as a standard bearer for your guild!
        TEXT_GUILDNEWS_SB_ASSIGNED,	//[<<1>>] <<2>> has been assigned to be a standard bearer!
        TEXT_GUILD_BANNER_ERR_GRPINVITE,	//You cannot invite another player with an active standard to form or join a party as you already have the maximum allowed!
        TEXT_GUILD_BANNER_ERR_OPARTYFORM,	//You cannot form an open party with that player, since the new party would have too many active standards!
        TEXT_GUILD_BANNER_ERR_PUBLICJOIN,	//You cannot join that party or warband! The maximum number of active standards allowed would be exceeded if you joined with your active standard.
        TEXT_GUILD_BANNER_ERR_WBINVITE,	//Your warband has the maximum number of active standards allowed. You may not invite another player, party, or warband if it has an active standard at this time.
        TEXT_GUILD_BANNER_WARNING_INACTIVE,	//Your active guild standard is no longer in the world. Your active standard status has been restored.
        TEXT_GUILD_BANNER_WARNING_ACTIVE,	//You still have an active guild standard somewhere in the world. You must retrieve it before activating another.
        TEXT_GUILD_RANK_ERR_NOPERM_SBEARERS,	//You do not have permission to assign members as standard bearers!
        TEXT_GUILD_RANK_ERR_SBEARER_STATUS,	//You cannot promote or demote standard bearers who have captured or claiming standards!
        TEXT_GUILD_RANK_ERR_RANKUNAVAILABLE,	//You cannot assign the guild member to that status rank, because it is not available!
        TEXT_GUILD_RANK_ERR_RANKTOOHIGH,	//You cannot assign the guild member to a status rank greater than your own!
        TEXT_GUILD_RANK_CHANGE_SUCCESS,	//You have successfully changed the status rank of the specified guild member.
        TEXT_GUILD_REMOVE_ERR_KICKCLAIM,	//You cannot kick members from your guild who are claiming a war objective. You must release the war objective's ownership before kicking this member.
        TEXT_GUILD_REMOVE_ERR_LEAVECLAIM,	//You cannot leave your guild while claiming a war objective on its behalf. Release your war objective before leaving your guild!
        TEXT_GUILD_REMOVE_ERR_MEMBERNOTFOUND,	//The specified member was not found in your guild and cannot be removed.
        TEXT_GUILD_BANNER_ERR_ABILITYLINEDUPE,	//You cannot slot more than one tactic of each type to a single banner!
        TEXT_SN_LFWARBAND_ON,	//Your party is now looking for a warband.
        TEXT_SN_LFWARBAND_OFF,	//Your party is no longer looking for a warband.
        TEXT_SN_LFWARBAND_ERR_ALREADYINWB,	//Your party is already in a warband and cannot be flagged as interested in joining one!
        TEXT_SN_LISTS_ERR_PLAYER_NOT_FOUND,	//That player could not be found!
        TEXT_SN_FRIENDSLIST_ERR_ADD_SELF,	//You cannot add yourself to your friends list!
        TEXT_SN_FRIENDSLIST_ERR_FULL,	//Your friends list is currently full. Please remove a friend to add a new one!
        TEXT_SN_FRIENDSLIST_ERR_EXISTS,	//That player already exists in your friends list!
        TEXT_SN_FRIENDSLIST_ADD_SUCCESS,	//<<C:1>> has been added to your friends list!
        TEXT_SN_IGNORELIST_ERR_ADD_SELF,	//You cannot ignore yourself!
        TEXT_SN_IGNORELIST_ERR_FULL,	//Your ignore list is currently full. Please remove an ignored player to add a new one!
        TEXT_SN_IGNORELIST_ERR_EXISTS,	//You are already ignoring that player!
        TEXT_SN_IGNORELIST_ADD_SUCCESS,	//You are now ignoring <<1>>.
        TEXT_DYEMERCHANT_ERR_PALETTE_EMPTY,	//The merchant doesn't have any dyes to sell.
        TEXT_DYEMERCHANT_ERR_NPC_NOT_DYEMERCHANT,	//This is not a dye merchant.
        TEXT_DYEMERCHANT_ERR_INSUFFICIENT_FUNDS,	//You don't have enough money to do that.
        TEXT_DYEMERCHANT_ERR_NOTHING_TO_DYE,	//No dye was needed.
        TEXT_DYEMERCHANT_ERR_ITEM_NOT_FOUND,	//The item to dye was not found.
        TEXT_GUILD_ASSIGNERR_LEADER,	//The guild's leader may not be manually assigned to another status rank without first designating a new guild leader!
        TEXT_INSTANCERESET_YOUR_GROUP,	//Your party's instances have been reset.
        TEXT_INSTANCERESET_YOUR_BATTLEGROUP,	//Your warband's instances have been reset.
        TEXT_INSTANCERESET_GROUP,	//Group <<1>>'s instance have been reset.
        TEXT_INSTANCERESET_BATTLEGROUP,	//Warband <<1>>'s instance have been reset.
        TEXT_INSTANCERESET_INVALID_GROUP,	//Group index <<1>> is not valid.
        TEXT_GM_KILL_ERR_OTHERPLAYER,	//You may not kill other players.
        TEXT_GM_RESURRECT_ERR_SPECIFY,	//You must specify a player to resurrect!
        TEXT_GM_RESURRECT_SUCCESS,	//You have resurrected <<C:1>>!
        TEXT_GM_RESURRECTED,	//You have been resurrected by <<C:1>>!
        TEXT_GM_PLAYER_X_NOT_FOUND,	//Player <<C:1>> not found!
        TEXT_GM_COMMAND_NOT_FOUND,	//Command Not Found.
        TEXT_REST_XP_ACTIVATED,	//You feel rested.
        TEXT_REST_XP_DEACTIVATED,	//You no longer feel rested.
        TEXT_DEFAULT_AFK_MESSAGE,	//I'm currently away from the keyboard.
        TEXT_YOU_ARE_NOW_AFK,	//You are now flagged as away from the keyboard.
        TEXT_YOU_ARE_NO_LONGER_AFK,	//You are no longer flagged as away from the keyboard.
        TEXT_YOUR_AFK_MESSAGE_HAS_CHANGED,	//Your AFK message has been updated.
        TEXT_CITY_INSTANCE_NOT_SELECTED,	//Your party leader must select the instance first!
        TEXT_INSTANCE_REQUEST_QUEUED,	//The instance is full. You have been placed in the wait queue.
        TEXT_INSTANCE_REQUEST_FAILED,	//The instance is not available.
        TEXT_LASTNAME_ERR_NPC_NOT_LASTNAMESHOP,	//You can't buy a last name here.
        TEXT_LASTNAME_ERR_NOT_ALLOWED_TO_SHOP,	//You must be rank <<1>> to purchase a last name.
        TEXT_LASTNAME_ERR_INSUFFICIENT_FUNDS,	//You don't have enough money to buy a last name.
        TEXT_LASTNAME_ERR_INVALID_LASTNAME,	//You requested an invalid last name.
        TEXT_DISMOUNTED,	//You have been dismounted.
        TEST_ITEM_PLAYER_LEVEL_TOO_LOW,	//You need to be a higher rank to use this item.
        TEST_ITEM_PLAYER_RENOWN_TOO_LOW,	//You need to be a higher renown rank to use this item.
        TEST_ITEM_PLAYER_INVALID_CAREER,	//Your career can't use this item.
        TEST_ITEM_PLAYER_INVALID_RACE,	//Your race can't use this item.
        TEXT_SN_ANON_ON,	//You are now anonymous.
        TEXT_SN_ANON_OFF,	//You are no longer anonymous.
        TEXT_SN_HIDE_ON,	//You are now hidden.
        TEXT_SN_HIDE_OFF,	//You are no longer hidden.
        TEXT_SN_ONLYSHOWEQUIP_ON,	//You are now allowing other players to inspect your equipment.
        TEXT_SN_ONLYSHOWEQUIP_OFF,	//You are no longer allowing other players to inspect your equipment.
        TEXT_SN_SHOWCLOAKHERALDRY_ON,	//You are now displaying your guild's heraldry on your cloak.
        TEXT_SN_SHOWCLOAKHERALDRY_OFF,	//You are no longer displaying your guild's heraldry on your cloak.
        TEXT_SN_BRAGGINGRIGHTS_ON,	//You are now allowing other players to inspect your bragging rights.
        TEXT_SN_BRAGGINGRIGHTS_OFF,	//You are no longer allowing other players to inspect your bragging rights.
        TEXT_SN_TRADESKILLS_ON,	//You are now allowing other players to view your tradeskill information.
        TEXT_SN_TRADESKILLS_OFF,	//You are no longer allowing other players to view your tradeskill information.
        TEXT_SN_LOOKINGFORGUILD_ON,	//You are now looking for a guild.
        TEXT_SN_LOOKINGFORGUILD_OFF,	//You are no longer looking for a guild.
        TEXT_SN_ALWAYSFORMPRIVATEGRP_ON,	//You will now only form private parties. To make any of your parties open, you must toggle your party preferences from the party options window.
        TEXT_SN_ALWAYSFORMPRIVATEGRP_OFF,	//You will now only form public parties. To make any of your parties private, you must toggle your party preferences from the party options window.
        TEXT_NO_HEAL_PENALTIES,	//You cannot purchase a heal, you have no heal penalties.
        TEXT_SN_OPENPARTYINTEREST_ERR,	//You cannot turn on your open party interest flag while in a party!
        TEXT_SN_SHOWCLOAKHERALDRY_ERR,	//You cannot display guild heraldry if you are not in a guild who has achieved the cloak heraldry reward!
        TEXT_GUILD_BANNER_CONFIGSUCCESS_NOTIMER,	//You have successfully configured the guild's standard.
        TEXT_ITEM_ERR_CANT_EQUIP_X,	//You can't equip <<1>>.
        TEXT_ITEM_ERR_CANT_MOVE_X,	//You could not move <<1>>.
        TEXT_CURRENCY_BRASS,	//brass
        TEXT_CURRENCY_SILVER,	//silver
        TEXT_CURRENCY_GOLD,	//gold
        TEXT_FLAG_PICKUP,	//<<1>> of <<2>> has taken the <<3>>!
        TEXT_FLAG_DROP,	//The <<1>> has been dropped!
        TEXT_FLAG_RETRIEVE,	//<<1>> of <<2>> has retrieved the <<3>>!
        TEXT_FLAG_CAPTURE,	//<<1>> of <<2>> has captured the <<3>>!
        TEXT_BOMB_CAPTURE,	//<<1>> of <<2>> has planted the <<3>>!
        TEXT_ERR_ENHANCEMENT_ALREADY_IN_USE,	//That enhancement is already in use.  Please use a different enhancement.
        TEXT_TRADE_ERR_CANT_TRADE_WHILE_DEAD,	//You can't trade while you're dead.
        TEXT_TRADE_ERR_CANT_TRADE_WITH_DEAD_PLAYERS,	//You can't trade with a dead player.
        TEXT_TRADE_ERR_CANT_TRADE_WITH_ENEMIES,	//You can't trade with your enemies.
        TEXT_TRADE_ERR_CANT_TRADE_WITH_YOURSELF,	//You can't trade with yourself.
        TEXT_TRADE_ERR_TARGET_ALREADY_TRADING,	//Your trading partner is already in another trade.
        TEXT_TRADE_ERR_TARGET_TOO_FAR,	//Your trading partner is too far away.
        TEXT_TRADE_ERR_TRADING_PARTNER_LOGGED_OUT,	//Your trading partner has logged out.
        TEXT_TRADE_ERR_BACKPACK_SPACE_TOO_LOW_YOU,	//You don't have enough room in your backpack for this trade.
        TEXT_TRADE_ERR_BACKPACK_SPACE_TOO_LOW_PARTNER,	//Your trading partner doesn't have enough room for this trade.
        TEXT_TRADE_ERR_INSUFFICIENT_MONEY,	//You don't have enough money to make that offer.
        TEXT_TRADE_ERR_OBJECT_CANT_BE_TRADED,	//That object can't be traded.
        TEXT_TRADE_ERR_OBJECT_NOT_IN_BACKPACK,	//That object isn't in your backpack.
        TEXT_TRADE_ERR_PARTNER_CHANGED_OFFER_TOO_RECENTLY,	//Your trading partner changed the trade too recently.  Please make sure you like the new offer.
        TEXT_RVR_FLAG,	//You are now flagged for RvR combat anywhere.  To turn off this flag, type /pvp and you will be unflagged after 10 minutes of non player combat.  This flag will persist even after death.
        TEXT_RVR_UNFLAG,	//You have chosen to unflag yourself for RvR combat.  If you do not engage in player combat nor enter an RvR area, you will be unflagged after 10 minutes.
        TEXT_SN_PLAYSTYLE_SUCCESS,	//You have successfully changed your playstyle social network preference.
        TEXT_SN_PLAYSTYLE_FAILURE,	//You did not provide the type of playstyle you prefer. Your social network preferences were not changed.
        TEXT_SN_PLAYTIME_SUCCESS,	//You have successfully changed your playtime social network preference.
        TEXT_SN_PLAYTIME_FAILURE,	//You did not provide the type of playtime you prefer. Your social network preferences were not changed.
        TEXT_SN_TIMEZONE_SUCCESS,	//You have successfully changed your timezone social network preference.
        TEXT_SN_TIMEZONE_FAILURE,	//You did not provide the type of timezone you prefer. Your social network preferences were not changed.
        TEXT_SN_LFGUILDINTEREST_ON,	//You have successfully turned on the specified social network interest flag.
        TEXT_SN_LFGUILDINTEREST_OFF,	//You have successfully turned off the specified social network interest flag.
        TEXT_SN_LFGUILDINTEREST_FAILURE,	//You did not provide the type of social network interest flag to toggle. Your social network preferences were not changed.
        TEXT_SN_LFGUILDATMOSPHERE_ON,	//You have successfully turned on the specified social network atmosphere preferences flag.
        TEXT_SN_LFGUILDATMOSPHERE_OFF,	//You have successfully turned off the specified social network atmosphere preferences flag.
        TEXT_SN_LFGUILDATMOSPHERE_FAILURE,	//You did not provide the type of social network atmosphere preferences flag to toggle. Your social network preferences were not changed.
        TEXT_SN_GUILD_PLAYSTYLE_FAILURE,	//You did not provide the type of guild playstyle preference. Your guild's recruiting settings were not changed.
        TEXT_SN_GUILD_ATMOSPHERE_FAILURE,	//You did not provide the type of guild atmosphere preference. Your guild's recruiting settings were not changed.
        TEXT_SN_GUILD_PLAYTIME_FAILURE,	//You did not provide the type of guild play time preference. Your guild's recruiting settings were not changed.
        TEXT_SN_GUILD_TIMEZONE_FAILURE,	//You did not provide the type of guild time zone preference. Your guild's recruiting settings were not changed.
        TEXT_SN_GUILD_RECRUITING_FAILURE,	//You did not provide the type of guild recruiting status. Your guild's recruiting settings were not changed.
        TEXT_SN_GUILD_RECRUITLEVELRANGE_RANGEFAILURE,	//The numbers provided must be between 1 and 40. Your guild's recruiting settings were not changed.
        TEXT_SN_GUILD_RECRUITLEVELRANGE_FAILURE,	//A minimum and maximum rank must be provided. Your guild's recruiting settings were not changed.
        TEXT_SN_GUILD_INTEREST_FAILURE,	//You did not provide the type of guild interest preference. Your guild's recruiting settings were not changed.
        TEXT_SN_GUILD_ARCHETYPES_FAILURE,	//You did not provide the type of guild recruiting archetype. Your guild's recruiting settings were not changed.
        TEXT_SN_GUILD_RECRUITER_ERR_NOTARGET,	//You did not provide the guild member's name. No recruiter settings were changed.
        TEXT_SN_GUILD_RECRUITER_ON,	//<<1>> is now a guild recruiter.
        TEXT_SN_GUILD_RECRUITER_OFF,	//<<1>> is no longer a guild recruiter.
        TEXT_SN_GUILD_RECRUITER_ERR_NOTFOUND,	//The specified guild member was not found.
        TEXT_SN_GUILD_RECRUITER_ERR_TOOMANY,	//Your guild has already designated the maximum number of recruiters allowed (5).
        TEXT_GUILD_ERR_MEMBERNOTFOUND,	//That person was not found in the guild roster.
        TEXT_SN_GUILD_PLAYSTYLE_SUCCESS,	//You have successfully changed the guild's preferred play style.
        TEXT_SN_GUILD_ATMOSPHERE_SUCCESS,	//You have successfully changed the guild's atmosphere settings.
        TEXT_SN_GUILD_PLAYTIME_SUCCESS,	//You have successfully changed the guild's preferred play time.
        TEXT_SN_GUILD_TIMEZONE_SUCCESS,	//You have successfully changed the guild's preferred time zone.
        TEXT_SN_GUILD_RECRUITING_SUCCESS,	//You have successfully changed the guild's recruiting status.
        TEXT_SN_GUILD_RECRUITLEVELRANGE_SUCCESS,	//You have successfully changed the guild's recruiting rank range.
        TEXT_SN_GUILD_INTEREST_ON,	//You have successfully turned on the specified guild interest flag.
        TEXT_SN_GUILD_INTEREST_OFF,	//You have successfully turned off the specified guild interest flag.
        TEXT_SN_GUILD_ARCHETYPE_ON,	//You have successfully turned on the specified archetype for recruiting.
        TEXT_SN_GUILD_ARCHETYPE_OFF,	//You are no longer recruiting the specified archetype.
        TEXT_GUILD_GENERAL_NO_PERMISSION,	//You do not have permission within your guild status rank to do that!
        TEXT_REALM_RESTRICTED_SAY,	//<<1>> says something you don't understand.
        TEXT_REALM_RESTRICTED_EMOTE,	//<<1>> makes a gesture you don't understand.
        TEXT_DUEL_OPTIONS,	///duel options are challenge/decline/cancel/surrender/accept.
        TEXT_DUEL_ERROR_NOT_CHALLENGED,	//You are not currently considering a duel.
        TEXT_DUEL_YOU_DECLINE,	//You decline the duel.
        TEXT_DUEL_X_DECLINES,	//<<1>> declines your duel.
        TEXT_DUEL_OFFER_CANCELLED,	//The offer to duel has been cancelled.
        TEXT_DUEL_YOU_ACCEPT,	//You have accepted the duel!  Begin fighting in 5 seconds!
        TEXT_DUEL_X_ACCEPTS,	//<<1>> has accepted your duel!  Begin fighting in 5 seconds!
        TEXT_DUEL_YOU_SURRENDER,	//You surrender your duel.
        TEXT_DUEL_X_SURRENDERS,	//<<1>> surrenders to you in a duel.
        TEXT_DUEL_ERROR_YOU_ALREADY_CHALLENGED,	//You have already issued a challenge for a duel. /duel cancel to remove it.
        TEXT_DUEL_ERROR_ALREADY_DUELING,	//You are already in a duel.  /duel surrender to end it.
        TEXT_DUEL_ERROR_ALREADY_BEEN_CHALLENGED,	//You have already been challanged to a duel. /duel decline to decline it.
        TEXT_DUEL_ERROR_FULL_HEALTH,	//You must be at full health to enter a duel.
        TEXT_DUEL_ERROR_TARGET_FULL_HEALTH,	//Your target must be at full health to enter a duel.
        TEXT_DUEL_ERROR_IN_COMBAT,	//You cannot enter a duel after engaging in combat recently.
        TEXT_DUEL_ERROR_RVR_ZONE,	//This is an RvR area.  You cannot duel here.
        TEXT_DUEL_ERROR_NO_TARGET,	//You need to target a player to duel.
        TEXT_DUEL_ERROR_TARGET_NOT_FRIENDLY,	//You must target an ally to duel.
        TEXT_DUEL_ERROR_TARGET_IN_COMBAT,	//Your target must disengage from combat before dueling.
        TEXT_DUEL_ERROR_TARGET_ALREADY_CHALLENGED,	//Your target has already issued a duel challenge.
        TEXT_DUEL_ERROR_TARGET_ALREADY_IN_DUEL,	//Your target is already in a duel.
        TEXT_DUEL_ERROR_TARGET_ALREADY_BEEN_CHALLENGED,	//Your target has already been issued a duel challange.
        TEXT_YOU_CHALLENGE_X_TO_DUEL,	//You have challenged <<1>> to a duel.
        TEXT_X_CHALLENGES_YOU_TO_DUEL,	//<<1>> challenges you to a duel.
        TEXT_DUEL_ERROR_TARGET_IN_RVR_ZONE,	//Your target is in an RvR area.  You cannot duel there.
        TEXT_NO_INSTANCES_AVAILABLE_SYSTEM_FULL,	//There are no instaces available for creation because the system is at capacity.  Please try again later.
        TEXT_LEAVE_CITYCONTENT_CONFIRMATION,	//You will lose your spot in the instance. Leave?
        TEXT_TELL_NAME_NOT_UNIQUE,	//The specified player name is not unique.
        TEXT_TELL_ERR_TO_SELF,	//You cannot send yourself a private chat message!
        TEXT_TELL_ERR_NOT_FOUND,	//<<1>> is not online, or is a member of the enemy realm!
        TEXT_CHATCHANNEL_DOESNT_EXIST,	//That chat channel does not exist.
        TEXT_CHATCHANNEL_NOT_IN_CHANNEL,	//You do not belong to that chat channel.
        TEXT_CHATCHANNEL_NOT_IN_ANY_CHANNELS,	//You do not belong to any chat channels.
        TEXT_CHATCHANNEL_MEMBER_OF,	//You are currently a member of the following chat channels:
        TEXT_CHATCHANNEL_CHANNEL_LIST,	//Channel: [<<1>>. - <<2>>]
        TEXT_CHATCHANNEL_ERR_NONE_TO_JOIN,	//There are currently no chat channels to join.
        TEXT_CHATCHANNEL_JOINED,	//Channel [<<1>>] - <<2>>
        TEXT_CHATCHANNEL_LEFT,	//Left Channel: [<<1>>. - <<2>>]
        TEXT_CHATCHANNEL_JOINED_CHANNEL,	//Joined Channel: [<<1>>. - <<2>>]
        TEXT_CHATCHANNEL_CHANGED_CHANNEL,	//Changed Channel: [<<1>>. - <<2>>]
        TEXT_CHATCHANNEL_ALREADY_MEMBER,	//You are already a member of that chat channel.
        TEXT_CHATCHANNEL_ERR_FULL,	//You have a full channel list and may not join another.
        TEXT_CHATCHANNEL_CREATE,	//Created Channel <<1>>
        TEXT_CHATCHANNEL_CREATE_FAILED,	//The chat channel failed to create.
        TEXT_CHATCHANNEL_ERR_NAME_EXISTS,	//A channel already exists with that name.
        TEXT_GUILD_INVITE_ERR_NO_INVITE,	//You do not currently have an invitation to join a guild.
        TEXT_GROUP_INVITE_ERR_NO_INVITE,	//You do not currently have an invitation to join a party.
        TEXT_ALLIANCE_INVITE_ERR_NO_INVITE,	//You do not currently have an invitation to join an alliance.
        TEXT_GUILD_INVITE_BEGIN_TOPLAYER,	//<<1>> has invited you to join the <<2>> guild.
        TEXT_ERROR_INVALID_AREA_ENTRY,	//You have entered an illegal area!
        TEXT_WARNING_SAFE_POINT_RESCUE,	//This area is not currently secure, and you are being transported to a safer location.
        TEXT_ROLL_ERR_MAXNUM,	//You must select a maximum number for your roll selection!
        TEXT_ROLL_ERR_MAX_GREATERTHANONE,	//You must select a maximum number greater than 1!
        TEXT_ROLL_PRIVATE_BEGIN,	//[Party] <<1>> picks a random number between <<2>> and <<3>>: <<4>>
        TEXT_ROLL_PUBLIC_BEGIN,	//<<1>> picks a random number between <<2>> and <<3>>: <<4>>
        TEXT_CURRENT_TIME,	//The current time is <<1>>:<<2>>:<<3>>.
        TEXT_BATTLEGROUP_INVALID_COMMAND,	//That is an invalid warband command.
        TEXT_TRADE_NOT_CONSIDERING_JOINING,	//You are not considering joining a trade order right now.
        TEXT_TRADE_ORDER_DECLINE,	//You decline to join the trade order.
        TEXT_LASTNAME_NOT_CONSIDERING,	//You are not currently considering a last name.
        TEXT_LASTNAME_DECLINED,	//You decline to take <<1>> as your last name.
        TEXT_NOT_IN_GROUP,	//You are not in a party.
        TEXT_JE_INST_FULL,	//The Instance Is Full
        TEXT_ALLIANCE_NO_FORMINVITE,	//You do not have a request to form an alliance.
        TEXT_ALLIANCE_INVITE_PROMPT,	//<<1>> has invited you to join an alliance.
        TEXT_ALLIANCE_MEMBER_JOINED,	//<<1>> has joined the alliance!
        TEXT_ALLIANCE_KICKED,	//Your guild was kicked from its alliance.
        TEXT_ALLIANCE_LEFT,	//Your guild has left its alliance.
        TEXT_ALLIANCE_MEMBER_KICKED,	//<<1>> has been kicked from the alliance.
        TEXT_ALLIANCE_MEMBER_LEFT,	//<<1>> has left the alliance.
        TEXT_ALLIANCE_LIST_HEADER,	//The following guilds are members of the <<1>> alliance:
        TEXT_ALLIANCE_DISBANDED,	//The <<1>> alliance has been disbanded.
        TEXT_ALLIANCE_NAME_LENGTH,	//The specified alliance name is too long.
        TEXT_ALLIANCE_INVITE_ERROR,	//The specified player is not a guild leader.
        TEXT_YOU_SELECT_NEED_FOR,	//You select Need for <<1>>.
        TEXT_YOU_SELECT_GREED_FOR,	//You select Greed for <<1>>.
        TEXT_YOU_SELECT_PASS_FOR,	//You select Pass for <<1>>.
        TEXT_SELECTS_NEED_FOR,	//<<1>> selects Need for <<2>>.
        TEXT_SELECTS_GREED_FOR,	//<<1>> selects Greed for <<2>>.
        TEXT_SELECTS_PASS_FOR,	//<<1>> selects Pass for <<2>>.
        TEXT_LOOT_RULES_SET_RR,	//Loot Rules set to Round Robin.
        TEXT_LOOT_RULES_SET_FFA,	//Loot Rules set to Free for All.
        TEXT_LOOT_RULES_SET_MASTER,	//Loot Rules set to Master Looter with permissions:
        TEXT_ADD_MASTER_LOOT_PERM,	//Master loot permissions added for <<1>>.
        TEXT_REM_MASTER_LOOT_PERM,	//Master loot permissions removed for <<1>>.
        TEXT_LOOT_THRESHOLD_SET,	//Loot threshold set to <<1>>.
        TEXT_CANT_LOOT_THAT,	//You cannot loot that.
        TEXT_NEED_ROLL_HEADER,	//<<1>> - Need Roll.
        TEXT_GREED_ROLL_HEADER,	//<<1>> - Greed Roll.
        TEXT_ALL_PASSED_HEADER,	//<<1>> - All Passed.
        TEXT_LOOT_TIE_BREAKER,	//There was a tie! Rolling for the tie breaker...
        TEXT_YOU_ROLL_NUM_BONUS,	//You roll <<1>> (<<2>> + <<3>> bonus).
        TEXT_NAME_ROLLS_NUM_BONUS,	//<<1>> rolls a <<2>> (<<3>> + <<4>> bonus).
        TEXT_YOU_ROLL_NUMBER,	//You roll <<1>>.
        TEXT_NAME_ROLLS_NUMBER,	//<<1>> rolls a <<2>>.
        TEXT_WINNER_HEADER,	//Winner - <<1>>.
        TEXT_INVALID_GIVE_TARGET,	//Invalid give target.
        TEXT_LOOT_RULES_ERROR,	//Loot Rules Unknown
        TEXT_DELIM_LIST,	//<<1>> <<2>> <<3>> <<4>> <<5>> <<6>> <<7>> <<8>> <<9>>
        TEXT_GROUP_MEMBER_LEVELED,	//<<1>> achieved a new rank!
        TEXT_GROUP_MEMBER_RENOWNLEVELED,	//<<1>> achieved a new realm rank!
        TEXT_SN_LFM_OFF,	//Your party is no longer recruiting new members.
        TEXT_SN_LFM_ON,	//Your party is looking for more members.
        TEXT_SN_LISTFRIENDS,	//Your friends list contains the following players:
        TEXT_SN_LISTIGNORES,	//Your ignore list contains the following players:
        TEXT_SN_FRIEND_LOGON,	//<<1>> has logged on.
        TEXT_SN_FRIEND_LOGOFF,	//<<1>> has logged off.
        TEXT_SN_LIST_ONLINE,	//<<1>> (Online)
        TEXT_SN_LIST_OFFLINE,	//<<1>> (Offline)
        TEXT_SN_FRIEND_REMOVE,	//You have removed <<1>> from your friends list.
        TEXT_SN_IGNORE_REMOVE,	//You have removed <<1>> from your ignore list.
        TEXT_GUILD_PLAYERKICKED,	//You were kicked from your guild!
        TEXT_GUILD_PLAYERLEFT,	//You have left your guild.
        TEXT_GUILD_MOTD,	//Guild Message of the Day: <<1>>
        TEXT_GUILD_LOGIN_KICKED,	//You are no longer in the guild.
        TEXT_GUILD_MEMBER_LOGIN,	//<<1>> has logged in.
        TEXT_GUILD_MEMBER_LOGOUT,	//<<1>> has logged out.
        TEXT_GUILD_MEMBER_JOINED,	//<<1>> has joined the guild.
        TEXT_GUILD_MEMBER_KICKED,	//<<1>> has been kicked from the guild.
        TEXT_GUILD_MEMBER_LEFT,	//<<1>> has left the guild.
        TEXT_GUILD_APPROVAL_ACCEPT,	//<<1>> approved creation of the guild.
        TEXT_GUILD_APPROVAL_DECLINE,	//<<1>> declined creation of the guild.
        TEXT_GUILD_INVITE_ERROR,	//You need to supply a name or have a valid target to invite.
        TEXT_GUILD_KICK_ERROR,	//You need to supply a name or have a valid target to kick.
        TEXT_GUILD_PROMOTE_ERROR,	//You need to supply a name or have a valid target to promote.
        TEXT_GUILD_DEMOTE_ERROR,	//You need to supply a name or have a valid target to demote.
        TEXT_GUILD_ASSIGN_ERROR,	//You need to supply a name or have a valid target to assign a new guild leader.
        TEXT_GUILD_RANKNAMELEN_ERROR,	//You must supply the ID number of the rank and a valid rank name.
        TEXT_GUILD_RANKNUM_MISSING,	//You must supply the ID number of the rank you wish to toggle the enabled flag on.
        TEXT_GUILD_PERMNUM_MISSING,	//You must suppply the ID number of the rank and the ID number of the permission you wish to toggle.
        TEXT_GUILD_PUBLICNOTE_LENGTH,	//The public note is too long.  the maximum length is 50.
        TEXT_GUILD_OFFICERNOTE_LENGTH,	//The officer note is too long.  The maximum length is 50.
        TEXT_YOU_DROWNED,	//You have drowned.
        TEXT_TS_EXCEED_MAX_TRADE_SKILLS,	//You cannot learn new skills in that category until you drop an existing skill.
        TEXT_CHANNELING_INTERRUPTED,	//Channeling Interrupted!
        TEXT_TACTIC_MUST_BE_ADDED,	//This tactic must be added to your tactics list before it can be used.
        TEXT_CANT_USE_TACTICS,	//You cannot use tactics right now!
        TEXT_ABILITY_IS_PASSIVE,	//Ability is passive.
        TEXT_PLAYER_WORTH_NO_XPRP,	//This player has been killed recently and is worth 0 renown and experience.
        TEXT_PLAYER_REDUCED_XPRP,	//This player has been killed recently and is worth reduced renown and experience.
        TEXT_ITEM_CANT_BE_USED_YET,	//This Item cannot be used yet.
        TEXT_ITEM_NO_CHARGES_LEFT,	//No charges left.
        TEXT_ITEM_CANT_BE_DYED,	//This item cannot be dyed.
        TEXT_ITEM_CANT_BE_BLEACHED,	//This item cannot be bleached.
        TEXT_ITEM_CANT_GO_THERE,	//That item cannot go there.
        TEXT_ITEM_CANT_BE_ENHANCED,	//That item cannot be enhanced.
        TEXT_CANT_REMOVE_ENHANCE,	//You cannot remove that enhancement.
        TEXT_CANT_CREATE_ENH_ITEM,	//You could not create enhanced item.
        TEXT_INSTANCE_NOT_SYNCED,	//You cannot enter this instance because your saved progress is not compatible with your group's.
        TEXT_GUILDNEWS_EVENT_CANCELLED,	//[<<1>>] A guild event (<<2>>) has been cancelled.
        TEXT_KEEP_CLAIM_ERROR_NOT_YOUR_REALM,	//You cannot claim a keep that is not owned by your realm.
        TEXT_YOU_HAVENT_RECEIVED_ANY_TELLS,	//You haven't received any private messages.
        TEXT_SN_LFM_NOTES_SET,	//You have successfully set your party or warband's recruitment notes.
        TEXT_SN_LFM_NOTES_ERR_LENGTH,	//Your note text is too long! Please shorten it and try again.
        TEXT_SN_LFM_NOTES_ERR_NOTWBLEADER,	//You are not a warband leader and may not set the warband's recruitment note.
        TEXT_SN_LFG_NOTES_SET,	//You have successfully set your party interest note.
        TEXT_INSTANCE_TIMESTAMP,	//Your progress does not match the current progress of the instance you are trying to enter.
        TEXT_TALKND_ALREADY_CUSTOM_CHAT_MEMBER,	//You're already a member of channel <<1>>
        TEXT_TALKND_ALREADY_CUSTOM_CHAT_OWNER,	//You're already the owner of private channel  <<1>>
        TEXT_TALKND_CHANNEL_COMMAND_DOESNT_EXIST,	//Channel command <<1>> doesn't exist
        TEXT_TALKND_CHANNEL_DOESNT_EXIST,	//Channel <<1>> doesn't exist
        TEXT_TALKND_CHANNEL_ISNT_CUSTOM,	//Channel <<1>> isn't a custom channel
        TEXT_TALKND_CUSTOM_ADDED_CHANNEL,	//Added channel #<<1>> (<<2>>)
        TEXT_TALKND_CUSTOM_CHANNEL_IS_DEAD,	//Channel #<<1>> is dead
        TEXT_TALKND_CUSTOM_CHANNEL_NOT_FOUND,	//Active channel #<<1>> not found
        TEXT_TALKND_CUSTOM_FAILED_TO_JOIN,	//Failed to create/join channel <<1>>
        TEXT_TALKND_CUSTOM_FAILED_TO_LEAVE,	//Failed to leave channel <<1>>
        TEXT_TALKND_CUSTOM_ID_CHANNEL,	//Channel #<<1>> (<<2>>)
        TEXT_TALKND_CUSTOM_LEFT_CHANNEL,	//Left channel #<<1>> (<<2>>)
        TEXT_TALKND_CUSTOM_MAX_CHANNELS,	//Already in maximum custom channels allowed per user
        TEXT_TALKND_CUSTOM_OTHER_REALM_OWNS,	//That channel belongs to the other realm, you can't join it
        TEXT_TALKND_CUSTOM_PRIVATE_ALREADY_BANNED,	//<<1>> is already banned from private channel <<2>>
        TEXT_TALKND_CUSTOM_PRIVATE_BANNED,	//<<1>> has been banned from private channel <<2>> by <<3>>
        TEXT_TALKND_CUSTOM_PRIVATE_BEEN_BANNED,	//You've been banned from that channel
        TEXT_TALKND_CUSTOM_PRIVATE_KICKED,	//<<1>> has been kicked from private channel <<2>> by <<3>>
        TEXT_TALKND_CUSTOM_PRIVATE_NOT_OWNER,	//You aren't the owner of private channel <<1>>
        TEXT_TALKND_CUSTOM_PRIVATE_NOW_PUBLIC,	//Channel <<1>> is now a public channel
        TEXT_TALKND_CUSTOM_PRIVATE_YOU_NOW_OWN,	//You're now the owner of private channel <<1>>
        TEXT_TALKND_CUSTOM_PUBLIC_CANT_KICK_USER,	//Channel <<1>> is public, so you are unable to kick <<2>> from it
        TEXT_TALKND_CUSTOM_PUBLIC_NO_BANS,	//Unable to ban user from public channel <<1>>
        TEXT_TALKND_CUSTOM_REALMLESS_CANT_CREATE,	//Realmless players cannot create custom channels
        TEXT_TALKND_CUSTOM_REALMLESS_CANT_JOIN,	//Realmless characters cannot join realm channels
        TEXT_TALKND_PLAYER_DOESNT_EXIST,	//Player <<1>> doesn't exist
        TEXT_AISTATE_ATTEMPTED_ABILITY,	//Attempted ability: ( <<1>> ) <<2>> < <<3>> >
        TEXT_CULTIVATION_OBJECT_DOES_NOT_EXIST,	//That object doesn't exist.
        TEXT_CULTIVATION_TOO_LOW,	//Your Cultivating skill is too low to plant that!
        TEXT_CULTIVATION_OBJECT_TOO_POWERFUL,	//That object is too powerful for you to use.
        TEXT_CULTIVATION_NOT_SEED,	//That isn't a seed.
        TEXT_CULTIVATION_CANT_ADD_NOW,	//You can't add that now.
        TEXT_CULTIVATION_NOT_TIME_TO_ADD,	//Now is not the time to add that.
        TEXT_CULTIVATION_PLOT_NOT_EMPTY,	//You can't plant seeds in this plot because it isn't empty.
        TEXT_CULTIVATION_CANT_APPLY_TO_PLOT_NOW,	//You can't apply that to the plot now.
        TEXT_CULTIVATION_CANT_ADD_MORE,	//You can't add another one of those.
        TEXT_CULTIVATION_PLOT_NOT_FINISHED_GROWING,	//Plot hasn't finished growing.
        TEXT_PLAYER_NOT_ENOUGH_INVENTORY_SPACE,	//You don't have room for that!
        TEXT_CULTIVATION_BAD_PRIMARY_CROP_DATA,	//Bad primary crop data.  Since the product couldn't be generated, it has been lost.
        TEXT_CULTIVATION_CANT_HARVEST_PRIMARY_CROP_BACKPACK_FULL,	//Primary crop could not be harvested because your backpack is full.  Please empty a slot and try again.
        TEXT_CULTIVATION_BAD_SECONDARY_CROP_DATA,	//Bad secondary crop data.  Since the product couldn't be generated, it has been lost.
        TEXT_CULTIVATION_CANT_HARVEST_SECONDARY_CROP_BACKPACK_FULL,	//Secondary crop could not be harvested because your backpack is full.  Please empty a slot and try again.
        TEXT_CULTIVATION_BAD_SEED_DATA,	//Bad seed data.  Since the seed couldn't be generated, it has been lost.
        TEXT_CULTIVATION_CANT_HARVEST_SEED_BACKPACK_FULL,	//Seed could not be harvested because your backpack is full.  Please empty a slot and try again.
        TEXT_CULTIVATION_PLOT_FLOWERING_COMPLETED,	//Cultivation plot flowering completed.
        TEXT_CULTIVATION_PLOT_FLOWERING_ADVANCED,	//Cultivation plot advanced to next phase.
        TEXT_CULTIVATION_PLAYER_LACKS_INVENTORY,	//You do not have an inventory!
        TEXT_CULTIVATION_PLOT_STILL_HAS_PLANTS,	//Plot still has plants in it, please empty it first.
        TEXT_CULTIVATION_ILLEGAL_PLOT_NUM,	//Illegal Plot Number <<1>>!
        TEXT_CULTIVATION_NOTHING_TO_PLOT,	//You have nothing that can be put in the plot!
        TEXT_INVENTORY_MAX_TROPHIES_EQUIPPED,	//Max number of trophies already equiped.
        TEXT_MONSTER_ALREADY_BUTCHERED,	//This creature has already been butchered.
        TEXT_MONSTER_ALREADY_SCAVANGED,	//This creature has already been scavanged
        TEXT_MONSTER_ONE_BUTCHER,	//Only one person may butcher a corpse at a time.
        TEXT_MONSTER_ONE_SCAVANGER,	//Only one person may scavenge a corpse at a time.
        TEXT_MONSTER_NOTHING_BUTCHERABLE,	//Nothing butcherable found in the corpse.
        TEXT_MONSTER_NOTHING_SCAVANGABLE,	//Nothing scavangable found in the corpse.
        TEXT_PLAYER_REGION_NOT_AVAILABLE,	//Region currently not available. Please try again later.
        TEXT_PLAYER_RELOCKING_TOK,	//Re-locking all TOKEntry indexes unlocked by this player.
        TEXT_PLAYER_INVALID_TROPHY_SLOT,	//Can't set trophy location, invalid trophy slot.
        TEXT_PLAYER_TROPHY_ALREADY_THERE,	//Can't set trophy location, there is already a trophy there.
        TEXT_PLAYER_NOT_VALID_TACTIC,	//Not a valid Tactic.
        TEXT_PLAYER_CANT_SELECT_TACTIC_YET,	//Tactic can't be selected yet.
        TEXT_PLAYER_CANT_REMOVET_TACTIC_YET,	//Tactic can't be removed yet.
        TEXT_PLAYER_NOT_ENOUGH_TACTIC_SLOTS,	//Not enough Tactic Slots.
        TEXT_PLAYER_CANT_CHANGE_MORALE_IN_COMBAT,	//Can't change Morale Abilities in combat.
        TEXT_PLAYER_INVALID_MORALE_SLOT,	//Invalid Morale Slot.
        TEXT_PLAYER_INVALID_MORALE_ABILITY,	//Not a valid Morale Ability.
        TEXT_PLAYER_ABILITY_DOESNT_FIT_SLOT,	//That ability doesn't go in that slot.
        TEXT_PLAYER_MORALE_ABILITY_MUST_BE_READY,	//You must ready that ability in a Morale slot.
        TEXT_PLAYER_TRADE_SKILL_ADVANCE,	//Your <<1>> skill rank has increased to <<2>>.
        TEXT_SCAVANGING_FAILED_ADD_INVENTORY,	//Failed to add item to inventory!
        TEXT_CHAT_PLAYER_NOT_IN_GAME,	//<<1>> is not in the game, or in another realm.
        TEXT_CHAT_SPECIFY_CHANNEL_JOIN,	//You need to specify a chat channel to join.<BR>Use: /chan join <channel>
        TEXT_CHAT_SPECIFY_CHANNEL_LEAVE,	//You need to specify a channel name to leave.<BR>Use: /chan leave <channel>
        TEXT_CHAT_SPECIFY_CHANNEL_SAY,	//You need to specify a channel number.<BR>Use: /chan say # Hello.
        TEXT_CHAT_SPECIFY_CHANNEL_WHO,	//You need to specify a channel number.<BR>Use: /chan who #
        TEXT_GROUP_NOT_IN_GROUP,	//You aren't currently in a group.
        TEXT_GROUP_TARGET_NOT_IN_GROUP,	//<<1>> is not in a group.
        TEXT_GROUP_TARGET_NOT_MEMBER,	//<<1>> is not a member of your group.
        TEXT_GROUP_BECOME_LEADER,	//You are now the leader of your group.
        TEXT_GROUP_NEW_LEADER,	//<<1>> is now the leader of your group.
        TEXT_STATS_PLAYER_NOT_FOUND,	//No player named '<<1>>' found (/stats help for info).
        TEXT_PLAYERORG_ALLIANCE_MEMBER_GUILD,	//<<1>>
        TEXT_PLAYERORG_GUILD_NOLONGER_EXISTS,	//The guild you were in no longer exists.
        TEXT_PLAYERORG_GUILD_MEMBER_ONLINE,	//<<1>> - Rank <<2>> - <<3>> (online)
        TEXT_PLAYERORG_GUILD_MEMBER_OFFLINE,	//<<1>> - Rank <<2>> - <<3>> (offline)
        TEXT_PLAYERORG_GUILD_INFO,	//Created: <<1>><<2[/<BR>MotD: $s/]>><<3[/<BR>Summary: $s/]>><<4[/<BR>Details: $s/]>>
        TEXT_ONE_VAULT_SUMMARY,	//<<1>>
        TEXT_POPULATION_COUNTS,	//Population Counts:<BR>  Total Population: <<1>><BR>  Order Population: <<2>><BR>  Destruction Population: <<3>>
        TEXT_REALM_MESSAGE,	//<<1>>
        TEXT_SESSION_GET_STATS,	//Statistics for <<1>> this session:<BR><<2[/These stats were reset due to a period of inactivity.<BR>/]>>Total RP: <<3>><BR>RP earned from kills: <<4>><BR>Kills that have earned RP: <<5>><BR>Deathblows:<<6>><BR>Deaths: <<7>><BR>HP healed: <<8>><BR>Resurrections performed: <<9>><BR>Online time: <<10>> hours, <<11>> mins, <<12>> seconds<BR>RP/hour: <<13>><BR>Kills per death: <<14>><BR>RP per kill: <<15>><BR>\"I Remain Standing...\": <<16>><<17[(no deaths)//]>>
        TEXT_CHAT_SEND_SAY,	//<<1>>
        TEXT_SESSION_TOP_20_EMPTY,	//Top 20 is currently empty for <<1>>.
        TEXT_SESSION_TOP_20_HEADER,	//Top 20 for <<1>>:
        TEXT_SESSION_TOP_20_INFO,	//<<1>>) <<2>> with <<3>>
        TEXT_SESSION_TOP_20,	//<<1>><<2[/<BR>$s/]>>
        TEXT_YOUR_RALLY_POINT_IS_NOW_SET_TO_X,	//Your Rally Point is now set to <<1>>.
        TEXT_CRAFTING_REQUIRES_BACKPACK_SPACE,	//<<1>> requires <<2>> empty backpack spaces.
        TEXT_CRAFTING_CANT_MOVE_ITEMS_WHILE_SALVAGING,	//Moving an item while salvaging it will disrupt salvaging.
        TEXT_YOU_SALVAGED_XQUANTITY_YITEM_FROM_ZITEM,	//You salvaged <<1>> <<2>> from <<3>>.
        TEXT_YOU_SALVAGED_XQUANTITY_YITEM_AS_SCRAP,	//You salvaged <<1>> <<2>> as scrap.
        TEXT_SALVAGING_FAILURE,	//Your salvage attempt failed.
        TEXT_SALVAGING_SUCCESS,	//Your salvage attempt succeeded.
        TEXT_SALVAGING_CRITICAL_SUCCESS,	//Your salvage attempt worked better than expected.
        TEXT_SALVAGING_SUPERCRITICAL_SUCCESS,	//Your salvage attempt worked even better than expected.
        TEXT_PET_RENAME_SUCCEEDED,	//Your pet has been renamed.
        TEXT_PET_RENAME_FAILED,	//You can't name your pet that.
        TEXT_AUCTION_OUTBID,	//You have been outbid on '<<1>>'. Current bid is <<2>>.
        TEXT_AUCTION_ITEM_SOLD,	//Your auction of '<<1>>' was sold to <<2>> for <<3>>.
        TEXT_AUCTION_ITEM_EXPIRED,	//You auction of '<<1>>' has expired.
        TEXT_NEED_TEMPLATE,	//You must select a template.
        TEXT_LASTNAME_ERR_BAD_FORMAT,	//Last name '<<1>>' is improperly formatted.  An empty name will remove your last name.  Otherwise it must be from <<2>> to <<3>> characters long, and not contain invalid characters.
        TEXT_LASTNAME_ERR_TABOO_NAME,	//Last name '<<1>>' is restricted.  Please choose a different last name.
        TEXT_LASTNAME_SUCCESS,	//You set your last name to '<<1>>'.
        TEXT_PETNAME_ERR_BAD_FORMAT,	//Pet name '<<1>>' is improperly formatted.  It must be from <<2>> to <<3>> characters, and not contain invalid characters.
        TEXT_PETNAME_ERR_TABOO_NAME,	//Pet name '<<1>>' is restricted.  Please choose a different pet name.
        TEXT_PETNAME_SUCCESS,	//You set your pet's name to '<<1>>'.
        TEXT_INSTANCE_LOCKED,	//An encounter is in progress; You can not enter this instance.
        TEXT_MONEY_STRING_1X,	//<<1>> <<2>> <<1[coins/coin/coins]>>
        TEXT_MONEY_STRING_2X,	//<<1>> <<2>> and <<3>> <<4>> coins
        TEXT_MONEY_STRING_3X,	//<<1>> <<2>>, <<3>> <<4>>, and <<5>> <<6>> coins
        TEXT_AUCTION_OUTBID_CLOSED,	//You have been outbid on '<<1>>'. The auction has closed.
        TEXT_AUCTION_NOT_ENOUGH_MONEY,	//You do not have enough money.
        TEXT_GUILD_VAULT_RANK_X_NO_PERMISSIONS,	//Your guild does not permit members with a rank of <<1>> to view the guild vault or guild coffers.
        TEXT_GUILD_VAULT_RANK_X_NO_DEPOSIT_VAULT_Y,	//Your guild does not permit members with a rank of <<1>> to deposit items in vault <<2>>.
        TEXT_GUILD_VAULT_RANK_X_NO_WITHDRAW_VAULT_Y,	//Your guild does not permit members with a rank of <<1>> to withdraw items from vault <<2>>.
        TEXT_GUILD_VAULT_SLOT_ALREADY_OCCUPIED,	//There is already an item in that slot.
        TEXT_GUILD_VAULT_SLOT_EMPTY,	//There is no item in that slot.
        TEXT_GUILD_VAULT_SLOT_LOCKED,	//Another member of your guild is currently moving that item.
        TEXT_GUILD_VAULT_RANK_X_NO_DEPOSIT_MONEY,	//Your guild does not permit members with a rank of <<1>> to deposit money in the guild coffers.
        TEXT_GUILD_VAULT_NO_ROOM_FOR_X_MONEY,	//The guild coffers are too full to accept a deposit of <<1>>.
        TEXT_GUILD_VAULT_RANK_X_NO_WITHDRAW_MONEY,	//Your guild does not permit members with a rank of <<1>> to withdraw money from the guild coffers.
        TEXT_GUILD_VAULT_OVERDRAFT_X_MONEY,	//The guild coffers do not contain enough money for a withdrawal of <<1>>.
        TEXT_GUILD_TAX_NOT_GUILDED,	//You must be in a guild to set the tax rate.
        TEXT_GUILD_TITHE_NOT_GUILDED,	//You must be in a guild to set your tithe rate.
        TEXT_GUILD_TAX_TITHE_NOT_UNLOCKED,	//Your guild does not have the Taxes & Tithes reward.
        TEXT_GUILD_RANK_NO_SET_TAX,	//Your guild does not permit members at your rank to edit the tax rate.
        TEXT_GUILD_SPECIFY_PERCENTAGE,	//You must specify a percentage between 0 and 100.
        TEXT_GUILD_CONTRIBUTED_X_TAXES,	//You contributed <<1>> to your guild for taxes and tithes.
        TEXT_AUCTION_BUYOUT_PRICE_INVALID,	//The buyout price must be higher or equal to your starting bid of <<1>>.
        TEXT_AUCTION_ITEM_NOT_TRADABLE,	//You cannot auction '<<1>>' because it is not tradable.
        TEXT_AUCTION_ITEM_IS_LOCKED,	//You cannot auction '<<1>>' because it is locked.
        TEXT_AUCTION_ITEM_IS_BOUND,	//You cannot auction '<<1>>' because it is bound to you.
        TEXT_AUCTION_ITEM_NO_LONGER_EXISTS,	//The auction '<<1>>' no longer exists.
        TEXT_AUCTION_POST_SUCCESSFUL,	//You have successfully placed your auction with a price of <<1>>. You paid <<2>> for the deposit.
        TEXT_AUCTION_BID_SUCCESSFUL,	//You have successfully placed a bid of <<1>> on '<<2>>'.
        TEXT_AUCTION_BUYOUT_SUCCESSFUL,	//You have successfully bought the item.
        TEXT_AUCTION_CANCEL_SUCCESSFUL,	//You have successfully canceled your auction.
        TEXT_AUCTION_UNKNOWN_ERROR,	//An unknown error was encountered with the auction house.
        TEXT_SALVAGING_ITEM_CANT_BE_SALVAGED,	//<<1>> can not be salvaged.
        TEXT_PLAYER_STUCK_IN_COLLISION,	//You appear to be stuck.  Use /stuck to return to your last bind location.
        TEXT_GUILD_UNASSIGN_BANNER_CARRIER_ERR,	//You must designate a guild member to unassign as a standard bearer.
        TEST_GUILD_BANNER_CARRIER_ASSIGN_PERM_ERR,	//You don't have permission to unassign standard bearers.
        TEXT_GUILD_ALREADY_BANNER_CARRIER_ERR,	//<<1>> is already a standard bearer.
        TEXT_GUILD_TOO_MANY_BANNER_CARRIERS_ERR,	//You guild is not allowed any more standard bearers.
        TEXT_BANNER_CARRIER_OFF,	//You are no longer a standard bearer.
        TEXT_GUILD_NOT_BANNER_CARRIER_ERR,	//<<1>> is not a standard bearer.
        TEST_GUILD_STANDARD_BEARER_UNASSIGN_PERM_ERR,	//You don't have permission to unassign standard bearers.
        TEXT_GUILDNEWS_SB_UNASSIGNED,	//[<<1>>] <<2>> is no longer a standard bearer.
        TEXT_AUCTION_SEARCH_RESULTS,	//You got <<1>> search result(s) back.
        TEXT_AUCTION_SERVER_NOT_AVAILABLE,	//The auction server is currently unavailable in your region.
        TEXT_AUCTION_SERVER_NOT_CONNECTED,	//The auction server is currently down. Please try again later.
        TEXT_NOT_ENOUGH_TO_REFUND,	//You don't have enough money to respec.
        TEXT_CANT_REFUND_NO_POINTS,	//You can't respec if you haven't spent any points!
        TEXT_GUILD_FULL_ERR,	//Your guild is full.
        TEXT_GUILD_VAULT_DEPOSIT_ALREADY_TRANSACTING,	//You must wait for your current Guild Vault transaction to complete before depositing that item.
        TEXT_GUILD_VAULT_WITHDRAW_ALREADY_TRANSACTING,	//You must wait for your current Guild Vault transaction to complete before withdrawing that item.
        TEXT_GUILD_VAULT_MOVE_ALREADY_TRANSACTING,	//You must wait for your current Guild Vault transaction to complete before moving that item.
        TEXT_GUILD_VAULT_NO_PERMISSIONS,	//Your guild does not permit members of you rank to view the guild vault or guild coffers.
        TEXT_GUILD_VAULT_OPEN_BEFORE_MODIFY,	//You must open the Guild Vault before you can modify it.
        TEXT_GUILD_VAULT_PLAYER_OVERDRAFT,	//You do not have enough money to make that deposit.
        TEXT_AUCTION_WON_ITEM,	//You have bought <<1>> from <<2>> for the price of <<3>>.
        TEXT_YOU_HAVE_X_UNREAD_MESSAGES_IN_YOUR_AUCTION_MAILBOX,	//You have <<1>> unread <<1[message/messages]>> in your Auction mailbox.
        TEXT_AUCTION_POST_NO_PERMISSION,	//You do not have the proper guild or alliance rewards to access the auction house.
        TEXT_TIME_CHANGED,	//The time of day suddenly changes!
        TEXT_THE_X_OPENS_THE_Y,	//The <<1>> opens the <<2>>!
        TEXT_THE_X_READS_Y,	//The <<1>> reads, "<<2>>"
        TEXT_X_SPEAKS_TO_Y,	//<<1>> speaks to <<2>>
        TEXT_X_CANNOT_GIVE_YOU_ITEM,	//<<1>> cannot currently give you an item.
        TEXT_YOU_CANNOT_BE_GIVEN_ITEM,	//You cannot currently be given an item.
        TEXT_X_DROPS_ITEM_ON_GROUND_FOR_YOU,	//<<1>> drops an item on the ground for you.
        TEXT_ITEM_CREATED_ON_GROUND_FOR_YOU,	//An item has been created on the ground for you.
        TEXT_X_DECLINED_JOIN_GUILD,	//<<1>> has declined to join the guild.
        TEXT_X_DECLINED_JOIN_GUILD_INVITE_CANCELED,	//<<1>> has declined to join the guild. All invitations are canceled.
        TEXT_X_DECLINED_JOIN_ALLIANCE,	//<<1>> has declined to join your guild's alliance.
        TEXT_X_CANCELED_INVITE_JOIN_GUILD,	//<<1>> has canceled the invitation to join the guild.
        TEXT_X_CANCELED_INVITE_JOIN_ALLIANCE,	//<<1>> has canceled the invitation to join his/her alliance.
        TEXT_X_ATTEMPTED_TO_SNOOP,	//<<1>> just attempted to snoop you.
        TEXT_NO_LONGER_SNOOPING_X,	//You are no longer snooping <<1>>.
        TEXT_BEEN_BOOTED,	//You have been booted.
        TEXT_BEEN_YANKED_TO_X,	//You have been yanked to <<1>>.
        TEXT_BEEN_SUMMONED_TO_X,	//You have been summoned to <<1>>.
        TEXT_MUTED,	//You have been muted.
        TEXT_UNMUTED,	//You are no longer muted.
        TEXT_COPTER_SENDING_NAMING_REPORT,	//Sending your TOS naming report to Customer Support.<BR>You have registered a TOS naming report against <<1>>.
        TEXT_CHATCHANNEL_X_HAS_LEFT,	//[<<1>>] has left the channel
        TEXT_CHATCHANNEL_X_HAS_JOINED,	//[<<1>>] has joined the channel
        TEXT_UNSTUCK,	//Your position will be adjusted to your last known safe point when you exit the game.
        TEXT_CANT_QUIT_YOURE_DEAD,	//You cannot quit now because you are dead.
        TEXT_FROZEN_BY_CSR,	//You have been frozen by a CSR. Leaving the game to avoid contact with a CSR can result in suspension of your account.
        TEXT_FROZEN,	//You are frozen in place.
        TEXT_UNFROZEN,	//You are no longer frozen in place.
        TEXT_PLEASE_FILL_SURVEY,	//Please fill out this survey before you quit.
        TEXT_MUST_NOT_MOVE_TO_QUIT,	//You must be stationary to quit.
        TEXT_CANT_QUIT_IN_COMBAT,	//You are currently in combat and cannot quit.
        TEXT_APPRENTICE_NO_GAINS,	//You are no longer gaining benefits from your group leader.
        TEXT_APPRENTICE_TOO_FAR,	//,	//You are too far from your group leader to gain any benefit.
        TEXT_APPRENTICE_LEADER_NOT_POWERFUL,	//Your group leader is no longer powerful enough to provide you with a benefit.
        TEXT_APPRENTICE_GAINING,	//You are now gaining combat benefits from your group leader.
        TEXT_SNOOPED_FROM,	//Locally snooped from <<1>>:<BR><<2>>
        TEXT_SLASH_USAGE_REFUND,	//Usage: /refund <actions|morale|tactics|spec>
        TEXT_RENAMED,	//You have been renamed to <<1>>.
        TEXT_REALM_SET,	//Your realm has been set to <<1>>.
        TEXT_AUCTION_MAIL_SENDER_NAME,	//Auction House
        TEXT_AUCTION_MAIL_SUBJECT_REFUND,	//Auction Refund
        TEXT_AUCTION_MAIL_SUBJECT_COMPLETE,	//Auction Complete!
        TEXT_AUCTION_MAIL_SUBJECT_CANCELLED,	//Auction Cancelled
        TEXT_AUCTION_MAIL_SUBJECT_EXPIRED,	//Auction Expired
        TEXT_AUCTION_MAIL_SUBJECT_WON,	//Auction Won!
        TEXT_AUCTION_MAIL_SUBJECT_OTHER,	//Auction Result
        TEXT_AUCTION_MAIL_BODY_REFUND_ITEM_X,	//You have been outbid in an auction for <<1>>.  A refund of your bid amount is attached to this message.
        TEXT_AUCTION_MAIL_BODY_COMPLETE_ITEM_X,	//Your auction of item <<1>> has completed successfully.  Your earnings from the sale are attached to this message.
        TEXT_AUCTION_MAIL_BODY_CANCELLED,	//Your auction has been successfully cancelled at your request.  Your item is attached to this message.
        TEXT_AUCTION_MAIL_BODY_EXPIRED,	//Your auction has expired.  Your unsold item is attached to this message.
        TEXT_AUCTION_MAIL_BODY_WON_ITEM_X,	//You have won the auction for <<1>>.  Your item is attached to this message.
        TEXT_AUCTION_MAIL_BODY_OTHER,	//This message has been sent automatically by the Auction House.
        TEXT_STANDARD_RETURN_MAIL_SENDER_NAME,	//Guild System
        TEXT_STANDARD_RETURN_MAIL_SUBJECT,	//Standard Return
        TEXT_STANDARD_RETURN_MAIL_BODY,	//Your standard has been automatically returned to you because an ally reclaimed it.
        TEXT_MARKETING_REWARD_SENDER_NAME,	//Marketing Rewards
        TEXT_RVR_LOWER_TIER_CAPTURED,	//<<1>> and <<2>> have been captured by <<3>>!~!
        TEXT_RVR_LOWER_TIER_CONTESTED,	//<<1>> and <<2>> are now contested.
        TEXT_RVR_CAMPAIGN_ZONE_CAPTURED,	//<<1>> has been captured by <<2>>!!!
        TEXT_RVR_CAMPAIGN_CONTESTED_ZONE_MOVED,	//The battle now wages on in <<1>>.
        TEXT_RVR_FORTRESS_UNLOCKED_DEFENDER,	//<<1>> is under attack, defend her with your life!
        TEXT_RVR_FORTRESS_UNLOCKED_ATTACKER,	//<<1>> is upon the gates of <<2>>, time to join the fight!
        TEXT_RVR_FORTRESS_CAPTURED,	//The fortress of <<1>> <<2[lies in ruin/has fallen/]>>!
        TEXT_RVR_CITY_FIRST_FORTRESS_FALLEN,	//The battle continues in other areas
        TEXT_RVR_CITY_SECOND_FORTRESS_FALLEN,	//<<1>> lies undefended! <<2[To arms, to arms!/Charge!/]>>
        TEXT_RVR_CITY_CONTESTED_FAIL_ATTACKER,	//<<1>> and the Realm of <<2>> have withstood. The siege is broken.
        TEXT_RVR_CITY_CONTESTED_FAIL_DEFENDER,	//<<1>> stands strong! The city is Recovering. No mercy for the straggling <<2>>.
        TEXT_RVR_CITY_CONTESTED_WIN_ATTACKER,	//<<1>> has taken <<2>>. Destroy its last remaining defenses.
        TEXT_RVR_CITY_CONTESTED_WIN_DEFENDER,	//<<1>> is in flames. The Realm of <<2>> has Captured the City.
        TEXT_COPTER_PLAYER_NOT_FOUND,	//No player named '<<1>>' was found.  Your appeal could not be submitted.
        TEXT_COPTER_MISSING_CHARACTER_APPEAL_SENT,	//Your missing character appeal has been sent to Customer Support.
        TEXT_COPTER_MISSING_ITEM_APPEAL_SENT,	//Your missing item appeal has been sent to Customer Support.
        TEXT_COPTER_CHARACTER_ISSUES_APPEAL_SENT,	//Your character appeal has been sent to Customer Support.
        TEXT_COPTER_PUBLIC_QUEST_APPEAL_SENT,	//Your Public Quest appeal has been sent to Customer Support.
        TEXT_COPTER_SCENARIO_APPEAL_SENT,	//Your Scenario appeal has been sent to Customer Support.
        TEXT_COPTER_BATTLEFIELD_OBJECTIVES_AND_KEEPS_APPEAL_SENT,	//Your Battlefield Objective or Keep appeal has been sent to Customer Support.
        TEXT_COPTER_MONSTER_APPEAL_SENT,	//Your monster appeal has been sent to Customer Support.
        TEXT_COPTER_QUEST_AND_QUEST_ITEMS_APPEAL_SENT,	//Your Quest appeal has been sent to Customer Support.
        TEXT_COPTER_COMBAT_OR_SKIRMISH_APPEAL_SENT,	//Your combat appeal has been sent to Customer Support.
        TEXT_COPTER_TOME_OF_KNOWLEDGE_APPEAL_SENT,	//Your Tome of Knowledge appeal has been sent to Customer Support.
        TEXT_COPTER_MAIL_APPEAL_SENT,	//Your in-game mail appeal has been sent to Customer Support.
        TEXT_COPTER_AUCTION_HOUSE_APPEAL_SENT,	//Your auction house appeal has been sent to Customer Support.
        TEXT_COPTER_INTERFACE_APPEAL_SENT,	//Your User Interface appeal has been sent to Customer Support.
        TEXT_COPTER_TRADESKILL_APPEAL_SENT,	//Your tradeskill appeal has been sent to Customer Support.
        TEXT_COPTER_CITY_FEEDBACK_SENT,	//Your city feedback has been sent.
        TEXT_COPTER_TOME_OF_KNOWLEDGE_FEEDBACK_SENT,	//Your Tome of Knowledge feedback has been sent.
        TEXT_COPTER_QUESTS_AND_PUBLIC_QUESTS_FEEDBACK_SENT,	//Your Quest or Public Quest feedback has been sent.
        TEXT_COPTER_CAREER_FEEDBACK_SENT,	//Your career feedback has been sent.
        TEXT_COPTER_COMBAT_FEEDBACK_SENT,	//Your combat feedback has been sent.
        TEXT_COPTER_TRADESKILL_AND_ECONOMY_FEEDBACK_SENT,	//Your trade feedback has been sent.
        TEXT_COPTER_USER_INTERFACE_FEEDBACK_SENT,	//Your User Interface feedback has been sent.
        TEXT_COPTER_GENERAL_FEEDBACK_SENT,	//Your feedback has been sent.
        TEXT_COPTER_CREATE_APPEAL_ERROR,	//An error occurred submitting your appeal.  Please try again later.
        TEXT_COPTER_CANCEL_APPEAL_ERROR,	//An error occurred canceling your appeal.  Please try again later.
        TEXT_COPTER_APPEAL_ERROR,	//An error occurred processing your feedback report.  Please try again later.
        TEXT_COPTER_PAID_ITEM_ISSUE,	//Your paid item issue appeal has been sent to Customer Support.
        TEXT_COPTER_PAID_CHAR_TRANSFER,	//Your paid character transfer appeal has been sent to Customer Support.
        TEXT_COPTER_PAID_NAME_CHANGE,	//Your paid name change appeal has been sent to Customer Support.
        TEXT_FLAGOBJECT_HOLD_ONLY_ONE,	//You can only hold one flag!
        TEXT_QUEST_TOO_MANY_CANNOT_ACCEPT,	//You cannot accept any more quests! You must finish or abandon one first.
        TEXT_LOCKOUTS_NO_ACTIVE_TIMERS,	//You have no active lockout timers.
        TEXT_LOCKOUTS_LOCKED_OUT_OF_FOLLOWING_ZONE,	//You are locked out of the following zone:
        TEXT_LOCKOUTS_LOCKED_OUT_OF_FOLLOWING_X_ZONES,	//You are locked out of the following <<1>> zones:
        TEXT_LOCKOUTS_TOTAL_TIME_X_MINUTES,	//      Total time - <<1>> minutes
        TEXT_LOCKOUTS_TIME_REMAINING_X_MINUTES,	//      Time remaining - <<1>> minutes
        TEXT_UNNAMED_ZONE,	//Unnamed Zone
        TEXT_PLAYER_QUEST_REWARD_ITEM,	//You recieve <<1>><<3[// x$d]>> as a reward for <<2>>.
        TEXT_PLAYER_MONEY_TRADED_AWAY,	//You traded <<1>> to <<C:2>>.
        TEXT_PLAYER_MONEY_TRADE_RECEIVE,	//<<C:1>> traded you <<2>>.
        TEXT_PLAYER_RECEIVE_REPAIRED_ITEM,	//You repaired <<1>> into <<2>>.
        TEXT_PLAYER_RECEIVE_MAIL_ITEM,	//You received <<1>><<2[// x$d]>> from the mail.
        TEXT_PLAYER_PURCHASED_ABILITY,	//You have purchased a new ability.
        TEXT_PLAYER_MAILED_MONEY_AWAY,	//You sent <<1>> in the mail.
        TEXT_PLAYER_MAILED_ITEM_AWAY,	//You sent <<1>><<2[// x$d]>> in the mail.
        TEXT_PLAYER_DESTROYED_ITEM,	//You destroyed <<1>><<2[// x$d]>>.
        TEXT_PLAYER_NOT_ENOUGH_INV_SPACE_QUEST_FINISH,	//You don't have enough inventory space to accept the reward and finish the quest!
        TEXT_PLAYER_PUT_ITEM_IN_BANK,	//You deposited <<1>><<2[// x$d]>> in the bank.
        TEXT_PLAYER_GOT_ITEM_FROM_BANK,	//You withdrew <<1>><<2[// x$d]>> from the bank.
        TEXT_BOOTED_FROM_SERVER_MESSAGETEXT,	//You have been removed from the server.  Please check your e-mail for further details.
        TEXT_BOOTED_FROM_SERVER_TITLEBAR,	//Removed from server
        TEXT_GUILDNEWS_RVR_CLAIM_UPKEEP_WARNING,	//[<<1>>] The guild coffers are low on funds.  The claim on <<2>> will be lost soon if money is not added to cover the periodic upkeep cost of <<3>>.
        TEXT_GUILD_RVR_CLAIM_UPKEEP_WARNING,	//The guild coffers are low on funds.  The claim on <<1>> will be lost soon if money is not added to cover the periodic upkeep cost of <<2>>.
        TEXT_GUILD_CLAIM_KEEP_CONFIRMATION_LOW_FUNDS,	//Due to the periodic upkeep cost of <<1>>, your Guild cannot hold this asset for long. Continue?
        TEXT_PETNAME_ERR_NO_PET,	//You do not have a nameable pet at your command.
        TEXT_TARGET_PLAYER_ERROR,	//<<1>> is too far away to target
        TEXT_GUILD_VAULT_ERROR_FULL,	//Vault <<1>> is already full.
        TEXT_COPTER_BUG_REPORT_RESPONSE,	//Thank you for submitting a Bug Report to our QA Department.
        TEXT_COPTER_FEEDBACK_RESPONSE,	//Thank you for submitting Feedback to our Community team.
        TEXT_COPTER_NO_IN_GAME_RESPONSE,	//This issue will not be responded to in-game.  If you require assistance from a CSR, please use the CSR Appeals feature.
        TEXT_WARBAND_NOW_PUBLIC,	//The warband is now public.  Other players are free to /join.
        TEXT_WARBAND_NOW_PRIVATE,	//The warband is now private.  You must /invite other players before they may join.
        TEXT_OVERKILL_PENALTY_APPLIED,	//You unleash terror upon your foes! (Overkill of this magnitude reduces experience gain by <<1>>%.)
        CHAT_TAG_MONSTER_WHISPER,	//<<1>> whispers to you: <<2>>
        CHAT_TAG_MONSTER_WHISPER_PARTY,	//<<1>> whispers to your party: <<2>>
        TEXT_YOU_READ_X,	//You read: <<1>>
        TEXT_SELECT_NEED_ERROR,	//You cannot Need that item.
        TEXT_CHANGE_NEED_ON_GREED,	//Need on use is now <<1[enabled/disabled]>>.
        TEXT_TIER_CHAT_CHANNEL_JOINED,	//You have joined <<1>>.
        TEXT_TIER_CHAT_CHANNEL_LEFT,	//You have left <<1>>.
        TEXT_TIER_CHAT_CHANNEL_NAME,	//<<1>> Tier:<<2>> <<3>>
        TEXT_TIER_CHAT_CHANNEL_RVR,	//Region-RvR
        TEXT_TIER_CHAT_CHANNEL_MAIN,	//Region
        TEXT_TIER_CHAT_CHANNEL_UNKNOWN,	//Unknown Chat Channel
        TEXT_PAIRING_CITY,	//City
        TEXT_NOT_IN_RVR_LAKE_TO_CHAT,	//You must enter an RvR Lake to use this channel.
        TEXT_MERCHANT_FAIL_PURCHASE_REQUIREMENT,	//You do not meet the requirements to purchase this item.
        TEXT_PLAYER_INVENTORY_INSUFFICIENT_SPACE_FOR_CULTIVATING,	//You must have 2 available backpack slots to Harvest.
        TEXT_MERCHANT_PURCHASED_ITEM_FOR_ALTCURRENCY,	//You purchased <<1>><<3[// x$d]>> from <<2>> for <<4>>.
        TEXT_MERCHANT_PURCHASED_ITEM_FOR_BRASS_AND_ALTCURRENCY,	//You purchased <<1>><<3[// x$d]>> from <<2>> for <<4>> and <<5>>.
        CHAT_TAG_GUILDNEWS,	//Guild News
        TEXT_WONT_AUTOJOIN_CHAT,	//You will no longer auto-join the <<1>> channel.
        TEXT_WILL_AUTOJOIN_CHAT,	//You will now auto-join the <<1>> channel.
        TEXT_TOO_MANY_GUILD_NOTES,	//You have exceeded the maximum number of guild notes.  To add new notes, delete old notes that are no longer used.
        TEXT_BG_YOU_HAVE_INVITED_MERGE,	//You have invited <<1>> to merge with your warband.
        TEXT_BG_YOU_HAVE_INVITED,	//You have invited <<1>> to join your warband.
        TEXT_MAIN_ASSIST_SET,	//<<1>> is now the Main Assist.
        TEXT_GUILD_VAULT_ERROR_INVENTORY_FULL,	//Withdrawal from vault <<1>> failed.  Your inventory is full.
        TEXT_GUILD_VAULT_ERROR_INVENTORY_SLOT_FULL,	//Withdrawal from vault <<1>> failed.  That slot already contains an item.
        TEXT_GUILD_XFER_DISABLED,	//Guild Transfers are disabled on this server.
        TEXT_GUILDNPC_INVALIDREQ_GUILDXFER_PENDING,	//You currently have a Guild transfer pending and are unable to create a guild at this time.
        TEXT_WARBAND_MUST_BE_PUBLIC,	//Your must be in a public warband to use this command.
        TEXT_TRADE_SKILL_UNKNOWN,	//Unknown Trade Skill
        TEXT_BACKPACK_FULL,	//Your backpack is full.
        TEXT_OVERAGE_CANT_TAKE_ATTACHMENTS,	//Cannot take attachments from mail.<BR>Inventory overflowing.<BR>Open backpack to remove overflow.
        TEXT_OVERAGE_CANT_BUY_ITEMS,	//Cannot purchase items.<BR>Inventory overflowing.<BR>Open backpack to remove overflow.
        TEXT_OVERAGE_CANT_USE_RECIPE,	//Cannot use <<1>>.<BR>Inventory overflowing.<BR>Open backpack to remove overflow.
        TEXT_OVERAGE_CANT_HARVEST,	//Cannot harvest.<BR>Inventory overflowing.<BR>Open backpack to remove overflow.
        TEXT_OVERAGE_CANT_LOOT,	//Cannot loot.<BR>Inventory overflowing.<BR>Open backpack to remove overflow.
        TEXT_OVERAGE_CANT_ROLL,	//Cannot roll on loot.<BR>Inventory overflowing.<BR>Open backpack to remove overflow.
        TEXT_OVERAGE_CANT_SALVAGE,	//Cannot salvage.<BR>Inventory overflowing.<BR>Open backpack to remove overflow.
        TEXT_OVERAGE_CANT_CRAFT,	//Cannot use <<1>>.<BR>Inventory overflowing.<BR>Open backpack to remove overflow.
        TEXT_OVERAGE_CANT_TRADE,	//Cannot trade.<BR>Inventory overflowing.<BR>Open backpack to remove overflow.
        TEXT_OVERAGE_PARTNER_CANT_TRADE,	//Your trading partner can't trade while having an overflowing inventory.
        TEXT_OVERAGE_CANT_FINISH_QUEST,	//Cannot finish <<1>>.<BR>Inventory overflowing.<BR>Open backpack to remove overflow.
        TEXT_TALKND_CONFIRM_ON,	//Confirm is now ON.
        TEXT_TALKND_CONFIRM_OFF,	//Confirm is now OFF.
        TEXT_ITEM_ENHANCING_TALISMAN_TOO_POWERFUL,	//<<1>> is too powerful to be socketed in <<2>>.
        TEXT_ITEM_ENHANCING_TOO_MANY_TALISMANS_OF_TYPE,	//There are too many talismans of the same type already socketed in <<1>> to add <<2>>.
        TEXT_RVR_TAKE_OTHER_PAIRING,	//You must also capture the <<1>> pairing or the <<2>> pairing to bring forth the Fortress Assaults!
        TEXT_GAIN_FROM_NAMED_CAPTURE,	//from capturing <<1>>
        TEXT_GAIN_FROM_BO_CAPTURE,	//from capturing a Battlefield Objective
        TEXT_GAIN_FROM_KEEP_BATTLE,	//from winning the battle for a Keep
        TEXT_GAIN_FROM_CAMPAIGN,	//from capturing a Zone
        TEXT_GAIN_FROM_DEFENDING_NAMED,	//from defending <<1>>
        TEXT_GAIN_FROM_DEFENDING_BO,	//from defending a Battlefield Objective
        TEXT_RENOWN_RANK_POINT_TOTAL,	//You have <<n:1[no renown rank points/one renown rank point/$d renown rank points]>> to spend.
        TEXT_PETNAME_CANT_CHANGE_NAME,	//You are not allowed to change your pet's name.
        TEXT_WARBAND_INVITE_ERR_GROUPED,	//<<1>> is already in a warband, led by <<2>>.
        TEXT_GUILD_VAULT_INVALID_WITHDRAWAL_AMOUNT,	//That is an invalid amount to withdraw.
        TEXT_GUILD_VAULT_INVALID_DEPOSIT_AMOUNT,	//That is an invalid amount to deposit.
        TEXT_OVERAGE_CANT_REFINE,	//You can't convert items while your inventory is overflowing.
        TEXT_ITEM_REFINE_FAILED,	//You could not refine <<1>>.
        TEXT_ITEM_REFINE_SUCCESS,	//You have successfully converted <<1>>.
        TEXT_FLASHCROWD_PORTED,	//Drawn by the presence of so many warriors, the Winds of Change suddenly howl across the battlefield, transporting you to another location!
        TEXT_FLASHCROWD_CHICKEN,	//Drawn by the presence of so many warriors, the Winds of Change suddenly howl across the battlefield, transforming you into a strange beast of Chaos!
        TEXT_FLASHCROWD_KILLED,	//Drawn by the presence of so many warriors, the Winds of Change suddenly howl across the battlefield, incinerating your body!
        TEXT_FLASHCROWD_DROP,	//Drawn by the presence of so many warriors, the Winds of Change suddenly howl across the battlefield, banishing you from the world!
        TEXT_MAIL_ITEM_MURDERNIGHT_SUBJECT,	//Getting a Head
        TEXT_MAIL_ITEM_MURDERNIGHT_BODY,	//Dear Enemy,<p><p>I didn't have time to introduce myself properly at our last meeting.  I'm afraid I was too busy killing you.  Perhaps you remember me though, I was the one smiling at you as you gasped your final breath and collapsed to the ground in agony.<p><p>It gave me no small enjoyment to see the last of your life fade from you, and so I'm writing this short note and including a memento of our time together.  I'm sure you'll recognize it.<p><p>No thanks are necessary... the pleasure was truly all mine.<p><p>Sincerely,<p><p><<1>>
        TEXT_HOTSPOT_SMALL,	//Small Hotspot
        TEXT_HOTSPOT_MEDIUM,	//Medium Hotspot
        TEXT_HOTSPOT_LARGE,	//Large Hotspot
        TEXT_HOTSPOT_EXPLANATION_SMALL,	//8 or more people are battling in this area.
        TEXT_HOTSPOT_EXPLANATION_MEDIUM,	//16 or more people are battling in this area.
        TEXT_HOTSPOT_EXPLANATION_LARGE,	//24 or more people are battling in this area.
        TEXT_GUILD_RECRUITERS_CONTACTED,	//<<1>> has requested to be contacted by a recruiter from <<2>>.
        TEXT_GUILD_NO_RECRUITERS_ONLINE,	//<<1>> has no recruiters online at the moment, try your request again later.
        TEXT_GUILD_CONTACT_RECRUITERS,	//Your request to be contacted by a recruiter from <<1>> has been sent.
        TEXT_ELLIPSIS,	//...
        TEXT_RALLY_CALL_ISSUED,	//The forces of <<1>> have issued a Rally Call!
        TEXT_PLAYER_MAIL_REFUND_ITEM,	//You received <<1>><<2[// x$d]>> as a refund for a failed outgoing mail message.
        TEXT_PLAYER_PAID_POSTAGE_FEE,	//You paid <<1>> in postage to send a mail message.
        TEXT_PLAYER_PAID_COD_FEE,	//You paid a COD fee of <<1>>.
        TEXT_PLAYER_CANT_MAIL_YOURSELF,	//You cannot send mail to yourself.
        TEXT_TRIAL_NO_TRADE,	//The trade has been cancelled due to the other party being a trial account member and is unable to add items or money to a trade with other players.
        TEXT_GAIN_FROM_KILLING_X,	//from killing <<1>>
        TEXT_BG_OLD_LEADER_ASSISTANT,	//<<1>> has been demoted to a warband assistant.
        TEXT_GUILDNPC_CANT_RENAME,	//You are not capable of renaming your guild.
        TEXT_PLAYER_BANNER_CAPTURED,	//The enemy has captured your guild standard!
        TEXT_PLAYER_BANNER_CAPTURED_NAMED,	//<<1>> has captured your guild standard!
        PLAYER_X_ACCEPTED_QUEST_X,	//<<C:1>> accepted the quest "<<2>>."
        TEXT_FRIEND_RENAMED,	//<<C:1>> has been renamed to <<C:2>>.
        TEXT_IGNORE_RENAMED,	//<<C:1>> has been renamed to <<C:2>>.
        TEXT_GROUP_OFFLINE_PROMOTE_LEADER,	//Offline members cannot lead groups.
        TEXT_BG_OFFLINE_PROMOTE_LEADER,	//Offline members cannot lead warbands.
        TEXT_KEEPUPGRADE_MATURED,	//The <<1>> keep upgrade has been completed and is now available at <<2>>.
        TEXT_KEEPUPGRADE_DECAYED,	//The <<1>> keep upgrade has been removed and is no longer available at <<2>>.
        TEXT_KEEPUPGRADE_MAINTENANCE_NO_ENOUGH_MONEY,	//The <<1>> keep upgrade at <<2>> is in danger of being lost due to lack of funds.
        TEXT_KEEPUPGRADE_MAINTENANCE_PAID,	//Maintenance fees have been deducted from the Guild Treasury for <<1>>.
        TEXT_KEEPUPGRADE_PLAYER_NOT_OWN_KEEP,	//Your guild does not own this keep.
        TEXT_KEEPUPGRADE_PLAYER_NOT_GUILD_LEADER,	//You need to be a guild leader to maintain keep upgrades.
        TEXT_KEEPUPGRADE_PERSIST_UPGRADES,	//The keep you claimed has <<1[one upgrade/$d upgrades]>> with a total upkeep cost of <<2>>. Would you like to keep these upgrades?
        TEXT_KEEPUPGRADE_GUILD_RANK_NOT_ENOUGH,	//,	//Your current guild rank is <<1>> and your guild rank needs to be <<2>> to purchase this upgrade.
        TEXT_KEEPUPGRADE_REQUIREMENT_NOT_MEET,	//The keep upgrade <<1>> cannot be purchased because its requirements have not been met.
        TEXT_KEEPUPGRADE_PURCHASED,	//The <<1>> keep upgrade has been purchased for <<2>>.
        TEXT_SCENARIO_QUEUE_TRIAL_ACCOUNT_ERROR,	//You cannot join a scenario outside of Tier 1 if you are on a trial account.
        TEXT_BANNERSTATUS_AVAILABLE_TIMED,	//You must wait until the standard release timer has passed before you can change <<1>>'s standard bearer status.
        TEXT_BANNERSTATUS_CAPTURED,	//<<1>>'s standard is currently captured. You can not change their standard bearer status.
        TEXT_BANNERSTATUS_CLAIMING,	//<<1>>'s standard is busy claiming a keep or battlefield objective. You can not change their standard bearer status.
        TEXT_FLAGOBJECT_FLAG,	//Flag
        TEXT_FLAGOBJECT_MURDERBALL,	//Murderball
        TEXT_FLAGOBJECT_BOMB,	//Bomb
        TEXT_YOU_FAIL_TO_HIT_YOURSELF,	//You <<5>> your <<2>>.
        TEXT_GAIN_FROM_ASSISTING_X,	//from assisting <<1>>
        TEXT_GUILD_VAULT_DEPOSITED_X_MONEY,	//You deposit <<1>> to the guild vault.
        TEXT_FLASHCROWD_WARNING,	//As the number of warriors in this area grows large, the Winds of Change begin to blow. Dire consequences might await those who do not move away!
        TEXT_FLASHCROWD_CATAPULT,	//Drawn by the presence of so many warriors, the Winds of Change suddenly howl across the battlefield, transporting you to another location!
        TEXT_AUCTION_ITEM_NOT_AUCTIONABLE,	//You cannot auction '<<1>>'.
        TEXT_CITY_INSTANCE_READY,	//Your city instance is now available. Proceed to the gates of the city.
        TEXT_CAMPAIGN_INSTANCE_READY,	//Your instance of Thanquol's Incursion is now available. Proceed to an entrance gate.
        TEXT_DIVINEFAVORALTAR_NO_PERMISSION_TO_FIRE_ABILITY,	//You do not have permission to fire the ability of the Divine Favor Altar.
        TEXT_ACCOUNT_PAYMENT_METHOD_MONTHLY,	//The monthly fee <<1[will/will not]>> deduct automatically.
        TEXT_ACCOUNT_PAYMENT_METHOD_TIME_DEDUCTION,	//You have chosen the time-deduction payment model. Each hour, 5 points will be deducted.
        TEXT_ACCOUNT_NOT_ENOUGH_PLAY_TIME_CREDITS,	//Your account has insufficient points to play. Please check your account at the account management website.
        TEXT_ACCOUNT_NOTICE_EXPIRATION_TIME_X_METHOD_Y,	//Your account's play time will expire at <<1>>. <<2>>
        TEXT_ACCOUNT_WARNING_EXPIRATION_TIME_X,	//Your account's play time will expire at <<1>>. To continue playing, you will need to extend your payment.
        TEXT_ACCOUNT_FINAL_WARNING_EXPIRATION_TIME_X_SECONDS,	//Your account will expire soon. After <<1>> seconds, you will be forced to log out. Please remember to deposit points on the account management website to continue playing.
        TEXT_GUILD_VAULT_NOT_UNLOCKED,	//Your guild has not yet earned the Guild Vault reward.
        TEXT_ZONE_CAPTURE_REWARD_SENDER,	//Zone Domination System
        TEXT_ZONE_CAPTURE_REWARD_SUBJECT,	//Zone Domination Guild Reward
        TEXT_ZONE_CAPTURE_REWARD_BODY_ORDER,	//An enemy zone was recently secured due in part to your guild's capture of a nearby keep. Stand tall with pride and accept this reward on behalf of Emperor Karl Franz.
        TEXT_ZONE_CAPTURE_REWARD_BODY_DESTRUCTION,	//The seizure of an enemy zone was facilitated by your Guild's capture of a neighboring keep. Tchar'zanek, in his generosity, has chosen to reward you with this gift.
        TEXT_KEEP_OUTER_DOOR_DESTROYED,	//The outer doors of <<1>> have been destroyed. There <<2[was one enemy/were $d enemies]>> spotted in the area.
        TEXT_KEEP_INNER_DOOR_DESTROYED,	//The inner doors of <<1>> have been destroyed. There <<2[was one enemy/were $d enemies]>> spotted in the area.
        TEXT_KEEP_LORD_KILLED,	//The lord of <<1>> has been killed. There <<2[was one enemy/were $d enemies]>> spotted in the area.
        TEXT_KEEP_GUARD_KILLED,	//A guard of <<1>> has been killed. There <<2[was one enemy/were $d enemies]>> spotted in the area.
        TEXT_EVERYONE_KEEP_OUTER_DOOR_DESTROYED,	//The outer doors of <<1>> have been destroyed.
        TEXT_EVERYONE_KEEP_INNER_DOOR_DESTROYED,	//The inner doors of <<1>> have been destroyed.
        TEXT_EVERYONE_KEEP_LORD_KILLED,	//The inner sanctum of <<1>> has been captured.
        TEXT_DIVINEFAVORALTAR_DONATION_ACCEPTED,	//<<1>> has accepted your donation.
        TEXT_DIVINEFAVORALTAR_ABILITY_FIRED,	//The ability of <<1>> has been fired.
        TEXT_ITEM_RECIVED_QUANTITY_X_OF_ITEM_Y,	//You receive <<1>> <<2>>.
        TEXT_ITEM_A_DECAYED_INTO_NOTHING,	//<<1>> decayed.
        TEXT_ITEM_A_DECAYED_INTO_B,	//<<1>> decayed into <<2>>.
        TEXT_ADDITIONAL_TOKEN_GRANTED_FOR_TAKING_ORVR_OBJECTIVES,	//You have received an additional <<1>> for your participation in taking open RvR objectives!
        GUILD_RANK_TITLE_LEADER,	//Guild Leader
        GUILD_RANK_TITLE_OFFICER,	//Officer
        GUILD_RANK_TITLE_MEMBER,	//Member
        GUILD_RANK_TITLE_INITIATE,	//Initiate
        GUILD_RANK_TITLE_UNUSED,	//Unused Status
        TEXT_ITEM_SOCKETING_MISMATCHED_SPECIAL_TALISMANS,	//Special talismans can only fit in special socketed items.
        TEXT_OVERAGE_CANT_UPROOT_PLANT,	//Cannot uproot plant.<BR>Inventory overflowing.<BR>Open backpack to remove overflow.
        TEXT_CULTIVATION_UPROOTED_PLANT,	//While uprooting a plant, you recovered <<1>>.
        TEXT_TOMB_KINGS_DUNGEON_ACCESS_LINE1,	//<<1>> has won access to the
        TEXT_TOMB_KINGS_DUNGEON_ACCESS_LINE2,	//<icon28300> Land of the Dead! <icon28300>
        TEXT_TOMB_KINGS_RRQ_UNPAUSED,	//The quest to access the Land of the Dead has resumed!
        TEXT_REMOVED_FROM_INSTANCE,	//You have been removed from the instance because you no longer belong to the owning group.
        TEXT_ADDITIONAL_TOKEN_GRANTED_FOR_DEFENDING_ORVR_OBJECTIVES,	//You have received an additional <<1>> for your participation in defending open RvR objectives!
        TEXT_PUBLIC_QUEST_OPT_OUT_ENABLE,	//You have opted out of the loot roll for <<1>>.
        TEXT_PUBLIC_QUEST_OPT_OUT_DISABLE,	//You are no longer opted out of <<1>>, and are once again eligible for loot.
        TEXT_PUBLIC_QUEST_OPT_OUT_APPLIED,	//You have been skipped for the loot roll in <<1>> because you opted out.
        TEXT_INSTANCE_IN_USE,	//The instance you requested is in use by another group.
        TEXT_INSTANCE_REQUEST_QUEUED_DUE_TO_GROUP,	//Your group's instance is full. You can either wait in a queue for a spot to open, or leave your group and join a different instance.
        LABEL_JOIN_QUEUE,	//Join Queue
        LABEL_LEAVE_GROUP,	//Leave Group
        TEXT_SIEGE_WEAPON_NOT_REPAIRABLE,	//This weapon can not currently be repaired.
        TEXT_GUILD_ERROR_RECRUITING_DESCRIPTION_LOCKED,	//Your recruiting description and public summary are currently locked and cannot be edited.
        TEXT_GUILD_TAX_CHANGE,	//The Guild Tax Rate is <<1>>%
        TEXT_GUILD_TITHE_CHANGE,	//Your Guild Tithe is <<1>>%
        TEXT_INFLUENCE_COMPLETE,	//Influence Complete
        TEXT_GUILD_CLAIM_KEEP_ERR_IN_CLAIMING_OTHER_KEEP,	//Your guild is currently claiming another keep.
        TEXT_GUILD_CLAIM_KEEP_ERR_ALREADY_CLAIMED_OTHER_KEEP,	//Your guild has already claimed another keep.
        TEXT_ORDER_KING_KILLED,	//Emperor Karl Franz, the Protector of the Empire, has been overcome by the forces of Destruction!
        TEXT_DESTRUCTION_KING_KILLED,	//Tchar’zanek, Champion of Tzeentch, has been overcome by the forces of Order!
        TEXT_OVERHEALING,	// (<<1>> overhealed)
        TEXT_INVENTORY_QUEST_BLOCK_FULL,	//Your quest bag is full.  Remove unneeded quest items before accepting new quests.
        TEXT_FLIGHTPATH_REFUND,	//You have been refunded the cost of your flight.
        TEXT_CHANGED_RVR_AUTOLOOT,	//Your group's RvR Auto Loot setting is now <<1[enabled/disabled]>>.
        TEXT_ITEM_NO_MOVE_BANK,	//<<1>> can not be moved into your bank.
        TEXT_X_FRIENDED_YOU,	//[<<1>> has added you as a friend. Click here to add <<1>> to your friends list.]
        TEXT_PLAYER_DISABLED_BY_ABILITY,	//You are disabled by <<1>>.
        TEXT_SUM_STONE_TARGET_NOT_FOUND,	//Target not found.
        TEXT_SUM_STONE_TARGET_TOO_HIGH,	//Target is too high rank for this summon.
        TEXT_SUM_STONE_TARGET_DECLINED,	//Your target declined your summon.
        TEXT_SUM_STONE_TARGET_ACCEPTED,	//Your target accepted your summon.
        TEXT_SUM_STONE_GROUP_NOT_FOUND,	//You are not in a group.
        TEXT_SUM_STONE_GROUP_FOUND,	//You have offered a summon to your group.
        TEXT_SUM_STONE_CASTER_NOT_FOUND,	//Summoner not found.
        TEXT_OWNED_DYNAMIC_INSTANCE_START_NEW,	//Starting new instance
        TEXT_OWNED_DYNAMIC_INSTANCE_ENTER_NEW,	//Entering instance based on group leader's progress
        TEXT_OWNED_DYNAMIC_INSTANCE_RESUME_SAVED,	//Resuming saved instance based on group leader's progress
        TEXT_OWNED_DYNAMIC_INSTANCE_ENTER_SAVED,	//Entering existing instance based on group leader's progress
        TEXT_OWNED_DYNAMIC_INSTANCE_PROGRESS_MISMATCH,	//Unable to enter, progress does not match group leader's
        TEXT_OWNED_DYNAMIC_INSTANCE_LEADER_ENTER_EXISTING_NEW,	//Entering existing instance
        TEXT_SUM_STONE_CASTER_IS_TARGET,	//You can not summon yourself.
        TEXT_GUILDNEWS_XP_FROM_ZONE_CAPTURE,	//[<<1>>] The guild has gained <<2>> experience from the capture of <<3>>.
        TEXT_REGION_RESET_NOW,	//This server region will shut down now.
        TEXT_REGION_RESET_5,	//This server region will shut down in 5 seconds.
        TEXT_REGION_RESET_15,	//This server region will shut down in 15 seconds.
        TEXT_REGION_RESET_30,	//This server region will shut down in 30 seconds.
        TEXT_REGION_RESET_60,	//This server region will shut down in one minute.
        TEXT_REGION_RESET_120,	//This server region will shut down in two minutes.
        TEXT_REGION_RESET_300,	//This server region will shut down in five minutes.
        TEXT_REGION_RESET_900,	//This server region will shut down in fifteen minutes.
        TEXT_REGION_RESET_1800,	//This server region will shut down in half an hour.
        TEXT_REGION_RESET_3600,	//This server region will shut down in one hour.
        TEXT_REGION_SHUTDOWN,	//This server region is shutting down.
        TEXT_TRADE_ERR_NO_TARGET,	//You need to target someone to trade with them.
        LABEL_MAC_KEYBOARD_KEY_CLEAR,	//Clear
        TEXT_BOLSTER_COULD_NOT_FIND_TARGET,	//Apprentice not found.
        TEXT_BOLSTER_COULD_NOT_FIND_CASTER,	//Master not found.
        TEXT_BOLSTER_TARGET_REFUSED_OFFER,	//<<1>> has declined your offer of apprenticeship.
        TEXT_BOLSTER_CAN_NOT_BOLSTER_SELF,	//You can not be your own apprentice.
        TEXT_BOLSTER_OFFER_NOT_GIVEN,	//<<1>> is not currently offering apprenticeship to <<2>>.
        TEXT_BOLSTER_MUST_BE_GROUPED,	//<<1>> and <<2>> must be grouped in order to be apprentice and master.
        TEXT_BOLSTER_ALREADY_PARTICIPATING_IN_BOLSTER,	//Apprenticeship can not be performed while either <<1>> or <<2>> are already an apprentice or master.
        TEXT_BOLSTER_TOO_HIGH_LEVEL_TO_BOLSTER_UP,	//<<1>> is too high level to apprentice up.
        TEXT_BOLSTER_TOO_LOW_LEVEL_TO_BOLSTER_DOWN,	//<<1>> is too low level to apprentice down.
        TEXT_APPRENTICE_DOWN_REQUEST_CONFIRMATION,	//<<C:1>> has asked you to be <<o:1>> apprentice. Your level will be reduced to <<2>> and you will not be able to use abilities above this level, do you accept?
        TEXT_APPRENTICE_UP_REQUEST_CONFIRMATION,	//<<C:1>> has asked you to be <<o:1>> apprentice. Your level will be increased to <<2>>, do you accept?
        CHAT_TAG_ADVICE,	//[Advice][<<1>>]: <<X:2>>
        CHAT_TAG_REALM_WAR,	//[Realm War Tier <<3>>][<<1>>]: <<X:2>>
        TEXT_REALMWAR_CHAT_NOT_CAPTAIN,	//You must be a Realm Captain for your guild to speak in this channel.
        TEXT_GUILD_REALM_CAPTAIN_ASSIGN_PERM_ERR,	//You don't have permission to assign realm captains.
        TEXT_GUILD_REALM_CAPTAIN_UNASSIGN_PERM_ERR,	//You don't have permission to unassign realm captains.
        TEXT_GUILD_REALM_CAPTAIN_OFF,	//You are no longer a realm captain.
        TEXT_GUILD_REALM_CAPTAIN_ON,	//You have been assigned as a realm captain for your guild!
        TEXT_GUILD_TOO_MANY_REALM_CAPTAINS_ERR,	//Your guild is not allowed any more realm captains.
        TEXT_GUILD_ALREADY_REALM_CAPTAIN_ERR,	//<<1>> is already a realm captain.
        TEXT_GLOBAL_REALM_CHAT_ADVICE,	//Advice
        TEXT_GLOBAL_REALM_CHAT_REALMWAR,	//Realm War Tier <<1>>
        TEXT_GLOBAL_REALM_CHAT_JOIN,	//You have joined <<1>>.
        TEXT_GLOBAL_REALM_CHAT_LEAVE,	//You have left <<1>>.
        TEXT_TALKND_CHANNEL_LOCALCLUSTER,	//System: <<1>>.
        TEXT_SCENARIO_STUCK_MOVE_TO_RESPAWN,	//Your character will be moved to the nearest respawn point in 30 seconds unless you enter combat or die.
        TEXT_SCENARIO_STUCK_DEAD_OR_IN_COMBAT,	//You cannot use /stuck while in combat or dead.
        TEXT_TALISMAN_LOAD_REFUND_SUCCESS,	//<<2>> was removed from <<1>> and refunded.
        TEXT_TALISMAN_LOAD_REFUND_FAILED,	//<<2>> was removed from <<1>>, but could not be refunded.  Please contact a CSR for help.
        TEXT_TALISMAN_LOAD_REFUND_FAILED_NO_ITEM,	//A talisman was removed from <<1>>, but it could not be refunded.  Please contact a CSR for help.
        TEXT_GLOBAL_REALM_CHAT_ERR_CANNOT_SPEAK,	//Your account is not permitted to speak in <<1>>.
        TEXT_GUILD_ASSIGN_REALM_CAPTAIN_ERR,	//You must designate a guild member to assign as a realm captain.
        TEXT_GUILD_UNASSIGN_REALM_CAPTAIN_ERR,	//You must designate a guild member to unassign as a realm captain.
        TEXT_GUILD_NOT_REALM_CAPTAIN_ERR,	//<<1>> is not a realm captain.
        TEXT_CURRENTEVENTS_INVALID_EVENT,	//That event does not exist or has expired.
        TEXT_CURRENTEVENTS_TIMER_NOT_EXPIRED,	//Your WarReport timer has not expired.
        TEXT_ADVISOR_STATUS_ON,	//You have successfully labeled yourself as an advisor.
        TEXT_ADVISOR_STATUS_OFF,	//You are no longer labeled as an advisor.
        TEXT_ADVISOR_STATUS_FAILURE,	//Your advisor status was not changed.
        TEXT_BOLSTER_OFFER_SENT,	//You've sent an offer of apprenticeship to <<1>>.
        TEXT_BOLSTER_OFFER_RECEIVED,	//You've received an offer of apprenticeship from <<1>>.
        TEXT_RVR_PAIRING_CAPTURED,	//The <<1>> pairing has been captured by <<2>>!
        TEXT_GLOBAL_NEWBIE_GUILD,	//New User Guild
        TEXT_NEWBIE_GUILD_BANNED,	//You are not allowed to join the New User Guild.
        TEXT_GAIN_FROM_NAMED_KEEP_BATTLE,	//from winning the battle for <<1>>
        TEXT_CURRENTEVENTS_TRIAL_ACCOUNT,	//Trial accounts cannot use the WarReport feature.
        TEXT_NUJ_REALMWAR_CHAT_DISABLED,	//The Realm War channel is currently disabled.
        TEXT_NUJ_ADVICE_CHAT_DISABLED,	//The Advice channel is currently disabled.
        TEXT_NUJ_ADVISOR_FLAG_DISABLED,	//The ability to flag yourself as an advisor is currently disabled.
        TEXT_YOU_JOINED_GUILD,	//You have joined the guild <<1>>.
        TEXT_YOU_LEFT_GUILD,	//You have left the guild <<1>>.
        CHAT_TAG_ADVICE_MODERATOR,	//[Advice][<<1>>] <Mod>: <<X:2>>
        CHAT_TAG_GUILD_MODERATOR,	//[Guild][<<1>>] <Mod>: <<X:2>>
        TEXT_NEWBIE_GUILD_INVITE_CONFIRMATION,	//You have been invited to join the guild "<<1>>." Do you accept?
        TEXT_ALLIANCE_FORM_ERR_NEWBIE_GUILD,	//<<1>> may not join an alliance.
        TEXT_ACCOUNT_GAME_TIME_EXPIRED,	//Your game time has been expired. You have been disconnected.
        TEXT_OVERAGE_CANT_CLAIM_EVENT_REWARDS,	//Cannot take event rewards.<BR>Inventory overflowing.<BR>Open backpack to remove overflow.
        TEXT_EVENT_REWARD_ALREADY_CLAIMED,	//You have already claimed a reward at that level for <<1>>.
        TEXT_EVENT_REWARD_NOT_ENOUGH_INFLUENCE,	//You do not have enough event influence for that reward.
        TEXT_EVENT_REWARD_NOT_AVAILABLE,	//Event influence reward unavalaible.
        TEXT_SUM_STONE_TARGET_TRIAL_ACCT,	//<<1>> has a trial account and cannot be summoned.
        TEXT_FLIGHT_MASTER_RANGE_ERR,	//You must be closer to a flight master to do this.
        TEXT_ITEM_NO_MAIL_ERR,	//<<1>> can't be sent through the in-game mail system.
        TEXT_CURRENTEVENTS_IN_COMBAT,	//You cannot use the WarReport while in combat.
        TEXT_OWNED_DYNAMIC_INSTANCE_OWNED_DIFFERENT_GROUP,	//The instance you are requesting is currently owned by <<1>>.
        TEXT_ZONE_NOT_ACCESSIBLE_TRIAL,	//Trial users are not allowed to access that zone.
        TEXT_ITEM_NO_MOVE_GUILD_VAULT,	//<<1>> can not be moved into the guild vault.
        TEXT_ACCOUNT_EXPIRATION_TIME,	//The expiration time is <<1>>.
        TEXT_ACCOUNT_AUTO_RENEW_ENABLED,	//Auto Renew is enabled and will deduct <<1>> points per month.
        TEXT_ACCOUNT_AUTO_RENEW_DISABLED,	//You currently have auto renew disabled.
        TEXT_ACCOUNT_MONTHLY_PAYMENT_EXPIRED_NO_RENEW,	//Your monthly subscription has expired. Please go to Wartown Member system to renew or re-activate your subscription.
        TEXT_ACCOUNT_MONTHLY_PAYMENT_EXPIRED_RENEW,	//Your previous monthly subscription has expired. The next monthly subscription will start soon.
        TEXT_ACCOUNT_TIME_DEDUCTION_NOTICE_RENEW_NO_TIME,	//When you log into the game, <<1>> Wartown points will be deducted from your account per hour.
        TEXT_ACCOUNT_TIME_DEDUCTION_NOTICE_RENEW_TIME_REMAINING,	//Wartown points will be deducted from your account per hour. Your current hour will expire at <<1>>.
        TEXT_LOCKED_CHEST_NEED_KEY,	//Opening <<1>> requires a key of at least tier <<2>>.
        TEXT_LOCKED_CHEST_OPENED_WITH_KEY,	//You opened <<1>> with <<2>> and received <<3>>.
        TEXT_GUILD_RESET_HERALDRY_ERR_NOMONEY,	//You need <<1>> to purchase a guild heraldry reset.
        TEXT_GUILD_RESET_HERALDRY_NOTIFICATION,	//It will cost <<1>> to reset your guild heraldry. Are you sure you want to purchase a reset?
        TEXT_GUILD_RESET_HERALDRY_SUCCESS,	//You have successfully purchased a guild heraldry reset for <<1>>. Your heraldry was set to its default state and may be changed at any time.
        TEXT_GUILD_RESET_HERALDRY_ERR_EXISTS,	//Your guild has an unused heraldry reset from a previous purchase. Please use it before purchasing another reset.
        TEXT_GUILD_RESET_HERALDRY_ERR_NONE,	//Your guild has not yet configured its heraldry. You may not reset guild heraldry until an original configuration has been created.
        TEXT_BOLSTER_CAN_NOT_BOLSTER_WHILE_CHICKEN,	//Although tasty, chickens are the master of none.
        TEXT_INVENTORY_TYPE_BACKPACK,	//backpack
        TEXT_INVENTORY_TYPE_BANK,	//bank
        TEXT_INVENTORY_CANT_EXPAND,	//Your <<1>> can not expand any more.
        TEXT_INVENTORY_EXPAND_PRICE_CHANGED,	//The cost of expanding your <<1>> has changed, and the transaction was canceled.
        TEXT_INVENTORY_EXPAND_INSUFFICIENT_FUNDS,	//You do not have enough money to buy more <<1>> space.
        TEXT_INVENTORY_EXPANDED,	//Your <<1>> has been expanded for <<2>>.
        TEXT_GUILD_RESPEC_TACTIC_ERR_NOPERM,	//You do not have permission to purchase a tactic respec for your guild.
        TEXT_GUILD_RESPEC_TACTIC_ERR_NOMONEY,	//You need <<1>> to purchase a guild tactic respec.
        TEXT_GUILD_RESPEC_TACTIC_SUCCESS,	//You have successfully purchased a guild tactic respec for <<1>>. Your guild may now choose new tactics!
        TEXT_RESPEC_NO_TOKEN_FOUND,	//You do not have the appropriate respec item to respecialize your character.
        TEXT_GUILD_VAULT_EXPANSION_NO_PERMISSIONS,	//You cannot purchase this guild vault expansion.  Members of your rank do not have view permissions for vault <<2>>.
        TEXT_GUILD_VAULT_EXPANSION_UNAVAILABLE,	//Your guild has already purchased the maximum number of vaults.
        TEXT_GUILD_VAULT_EXPANSION_MAX_CAPACITY,	//Guild vault <<1>> cannot be expanded any further.
        TEXT_GUILD_VAULT_EXPANSION_NO_MONEY,	//You do not have enough money to purchase a guild vault expansion.
        TEXT_GUILD_VAULT_EXPANSION_PRICE_CHANGED,	//The cost of expanding your guild vault has changed, and the transaction was canceled.
        TEXT_GUILDNEWS_VAULT_EXPANDED,	//[<<1>>] <<2>> has purchased additional guild vault space.
        TEXT_ZONE_RESET_SUCCESS,	//You are no longer locked out of <<1>>.
        TEXT_ZONE_RESET_FAILURE,	//The lockout reset did not succeed, because you are not currently locked out of <<1>>.
        TEXT_GUILD_RESPEC_TACTIC_FAILURE,	//Your guild does not have any purchased tactics to respec.
        TEXT_MTX_STORE_SENDER,	//Mythic Store
        TEXT_PQR_FORCED_OPT_OUT_NEW,	//You have won a prize for this objective and will now be forced to opt out of future prizes for repeated runs until your timer expires.
        TEXT_PQR_FORCED_OPT_OUT_PRIZE,	//You have won <<1>> (<<2>>) for your participation in the objective despite your forced opt out status. Congratulations!
        TEXT_PUBLIC_QUEST_FORCED_OUT_APPLIED,	//You have been forced to opt out of the loot roll for <<1>> due to a timed lockout. You will receive spoils.
        TEXT_GUILD_VAULT_EXPAND_SUCCESS,	//You have successfully purchased a guild vault expansion for <<1>>.
        TEXT_GUILD_HERALDRY_EDIT_SUCCESS_PRICE,	//You spent <<1>> reserving your guild's heraldry.
        TEXT_ITEM_CUSTOMIZE_INVALID_ITEM,	//You can not customize item appearance without a target item.
        TEXT_ITEM_CUSTOMIZE_COMBAT_OR_DEAD,	//You can not customize an item's appearance when you are in combat or dead.
        TEXT_ITEM_CUSTOMIZE_CANT_BIND,	//You can only customize an item's appearance if it binds to you.
        TEXT_ITEM_CUSTOMIZE_INVALID_TYPE,	//<<1>> is not a visible worn equipment item, so its appearance can't be customized.
        TEXT_ITEM_CUSTOMIZE_MISMATCHED_TYPE,	//<<1>> is too different from <<2>> to use its appearance.
        TEXT_AUCTION_POSTINGS_CLOSED,	//The posting of new auctions has been temporarily disabled. You may still purchase from the broker.
        TEXT_ERROR_CANT_TRADE_WITH_TRIAL,	//You cannot trade with trial accounts!
        EMOTE_COMMAND_TARGET_FRIENDLY,	//friendly
        EMOTE_COMMAND_TARGET_ENEMY,	//enemy
        TEXT_PLAYER_TEMP_IGNORED,	//The reported player will be ignored until you exit the game.
        TEXT_ERROR_NO_APPEAL_DETAILS,	//You may not submit an empty appeal. Please enter your appeal details before submitting.
        TEXT_CANT_QUIT_YOURE_CASTING,	//You may not log out while casting.
        TEXT_SERVER_UNDER_LOAD,	//The server is currently experiencing a heavy load in your area.
        TEXT_CHAT_ERR_TRIAL_USER,	//Trial accounts are not permitted to speak in <<1>>.
        TEXT_GLOBAL_REGION_CHAT,	//Region chat
        TEXT_YOU_CANT_INTERACT_WHILE_ZONING,	//You cannot interact with objects while zoning.
        TEXT_PUBLIC_QUEST_OPT_OUT_GOLD,	//You have opted out of the loot roll for gold bags in <<1>>.
        TEXT_TRADE_ERR_CANT_TRADE_WITH_PAM_PLAYERS,	//You cannot trade while a player is in another form.
        TEXT_MAX_RENOWN_RANK_ORDER,	//Karl Franz bestows a boon upon Order in recognition of <<C:1>> achieving Renown Rank 100!
        TEXT_MAX_RENOWN_RANK_DESTRUCTION,	//Tchar'zanek bestows a boon upon Destruction in recognition of <<C:1>> achieving Renown Rank 100!
        TEXT_PAM_ERR_GENERAL_PLAYER,	//You cannot do that while in another form.
        TEXT_CLAIM_AVAILABLE_REWARDS,	//You have <<1>> account entitlements available. Use the Account Entitlements window to access them.
        TEXT_CLAIM_ERROR_ALREADY_CLAIMED,	//You have already claimed this account entitlement.
        TEXT_CLAIM_ERROR_NOT_ENOUGH_SPACE,	//You do not have enough space in your inventory for the account entitlement item. This entitlement has not been claimed.
        TEXT_CLAIM_ERROR_ITEM_NOT_CREATED,	//The selected account entitlement item could not be created.
        TEXT_CLAIM_SUCCESSFUL,	//You have successfully claimed account entitlement item <<1>>.
        TEXT_GUILD_VAULT_LOCK_THROTTLE_WARNING,	//The guild vault is currently busy. Please wait a moment and try again.
        TEXT_INSTANCE_NOT_CONTESTED_OWNER,	//Your realm must have control over this region before you can create new instances.
        TEXT_FORTRESS_OUTOFRANGE,	//You are not close enough to the banner to claim this asset.
        TEXT_FORTRESS_ALREADYCLAIMED,	//This asset is already claimed!
        TEXT_FORTRESS_MAINFORTRESSNOTCLAIMED,	//You cannot claim this asset until the fortress has been claimed by a member of your alliance.
        TEXT_FORTRESS_NOGUILDORALLIANCEFOUND,	//You are not in a guild or alliance.
        TEXT_FORTRESS_NOTINALLIANCE,	//Your guild is either not in an alliance or in an alliance who does not own this fortress.
        TEXT_FORTRESS_INVALIDSUBAREA,	//This is not a valid asset to be claimed!
        TEXT_FORTRESS_REALMOWNERSHIP,	//Your realm does not currently own this asset.
        TEXT_FORTRESS_NOTINPLAYERORG,	//You are not in a guild or alliance.
        TEXT_FORTRESS_UNCLAIMNOTOWNER,	//Only the guild owning the asset can unclaim it.
        TEXT_FORTRESS_SEIZEOWNREALM,	//You cannot seize what your realm already controls.
        TEXT_ERR_JUMP_WITH_RELIC  //You cannot leave this area while carrying a relic!
    };

    public enum MailResult
    {
        TEXT_MAIL_UNK,
        TEXT_MAIL_RESULT1, //	The Mail Server is busy processing your current request.
        TEXT_MAIL_RESULT2, //	You are too far away to use that mailbox.
        TEXT_MAIL_RESULT3, //	Mail Server did not respond to the Get Headers request.
        TEXT_MAIL_RESULT4, //	Mail Sent Successfully.
        TEXT_MAIL_RESULT5, //	Send mail FAILED. Mail Server did not respond.
        TEXT_MAIL_RESULT6, //	Send mail FAILED. You must wait 5 seconds between sending messages.
        TEXT_MAIL_RESULT7, //	That recipient does not exist within your realm.
        TEXT_MAIL_RESULT8, //	You do not have enough money to send that.
        TEXT_MAIL_RESULT9, //	You may not mail that attached item.
        TEXT_MAIL_RESULT10, //	The message was returned successfully.
        TEXT_MAIL_RESULT11, //	Returning the message failed.
        TEXT_MAIL_RESULT12, //	Message was successfully deleted.
        TEXT_MAIL_RESULT13, //	Failed to delete the message.
        TEXT_MAIL_RESULT14, //	Failed to open Message. Try again later.
        TEXT_MAIL_RESULT15, //	Failed to set read/unread flag. Server Busy.
        TEXT_MAIL_RESULT16 //	Failed to take attachment.
    };

    public enum ScenarioUpdateType
    {
        List = 0,
        Queued = 1,
        Leave = 2,
        Pop = 6,
        GroupQueued = 8
    }

    [Flags]
    public enum CrowdControlTypes
    {
        Snare = 1,
        Root = 2,
        Disarm = 4,
        Silence = 8,
        Knockdown = 16,
        Stagger = 32,
        Grapple = 128,

        MoveImpedance = 3,
        Unstoppable = 28,
        AllStandardCC = 31,
        Disabled = 48,
        NoAutoAttack = 52,
        All = 255,
    };

    public enum CreatureTitle
    {
        None,
        Trainer,
        CareerTrainer,
        RenownTrainer,
        Apothecary,
        Butcher,
        Cultivator,
        Salvager,
        Scavenger,
        HedgeWizard,
        Merchant,
        ArmorMerchant,
        WeaponMerchant,
        CampMerchant,
        SiegeWeaponMerchant,
        CraftSupplyMerchant,
        RenownGearMerchant,
        RallyMaster,
        FlightMaster,
        GuildRegistrar,
        Healer,
        Postmaster,
        Banker,
        Auctioneer,
        Guard,
        WarGuard,
        Bodyguard,
        DogofWar,
        Sergeant,
        KeepLord,
        FortressLord,
        MountVendor,
        KillCollector,
        StableMaster,
        Quartermaster,
        Herald,
        NameRegistrar,
        VaultKeeper,
        Librarian,
        Realtor,
        ApprenticeRenownTrainer,
        LiveEventMaster,
        NordlandXIGuard,
        FestenplatzGuard,
        HarvestShrineGuard,
        LostLagoonGuard,
        GreystoneGuard,
        MonasteryGuard,
        CryptGuard,
        KinshelsGuard,
        VerentanesGuard,
        FeitensGuard,
        TavernGuard,
        HallenfurtGuard,
        QuarryGuard,
        ReikwatchGuard,
        RunehammerGuard,
        SchwenderhalleGuard,
        KurlovArmoryGuard,
        MartyrsSquareGuard,
        GraveyardGuard,
        OrtelvonZarisGuard,
        ShrineofTimeGuard,
        EverchosenGuard,
        ChokethornGuard,
        ThaugamondGuard,
        LookoutGuard,
        OutpostGuard,
        StonemineGuard,
        CannonBatteryGuard,
        LighthouseGuard,
        IroncladGuard,
        AlcadizaarsGuard,
        GoblinArmoryGuard,
        KaragazGuard,
        ArtilleryRangeGuard,
        FurrigsFallGuard,
        BreweryGuard,
        HardwaterGuard,
        IcehearthGuard,
        GromrilJunctionGuard,
        DolgrundsGuard,
        DoomstrikerGuard,
        GromrilKrukGuard,
        HeadwallGuard,
        KarakPalikGuard,
        MadcapGuard,
        BeastPensGuard,
        RottenpikeGuard,
        LobbaMillGuard,
        LorendythGuard,
        AltarofKhaineGuard,
        ShardofGriefGuard,
        NightflameGuard,
        SiegeCampGuard,
        ShadowSpireGuard,
        ReaverStablesGuard,
        NeedleofEllyrionGuard,
        WoodChoppazGuard,
        MaidensLandingGuard,
        SpireofTeclisGuard,
        SariDaroirGuard,
        ChillwindGuard,
        BelKorhadrisGuard,
        SanctuaryGuard,
        SiegeCamp2Guard,
        MilaithsMemoryGuard,
        MournfireGuard,
        PelgorathGuard,
        FireguardSpireGuard,
        SenlathianStandGuard,
        SarathananValeGuard,
        ConquerorShrineGuard,
        BarracksGuard,
        TomeTacticLibrarian,
        TomeTrophyLibrarian,
        BasicRenownGearMerchant,
        VeteranRenownGearMerchant,
        AdvancedRenownGearMerchant,
        EliteRenownGearMerchant,
        UpgradeMerchant,
        DoorRepairMerchant,
        StandardMerchant,
        HealingRitualist,
        LifetapRitualist,
        SiegeQuartermaster,
        RecruitMedallionQuartermaster,
        ScoutMedallionQuartermaster,
        SoldierMedallionQuartermaster,
        OfficerMedallionQuartermaster,
        RoyalQuartermaster,
        ExpeditionQuartermaster,
        Blacksmith,
        TomeAccessoryLibrarian,
        TomeTokenLibrarian,
        TalismanMerchant,
        TheForcesofOrder,
        TheForcesofDestruction,
        CrusaderQuartermaster,
        RecruitEmblemQuartermaster,
        ScoutEmblemQuartermaster,
        SoldierEmblemQuartermaster,
        OfficerEmblemQuartermaster,
        VanquisherQuartermaster,
        ApprenticeCareerTrainer,
        RecordsKeeper,
        VerySpecialDyeVendor,
        MajorTalismanMerchant,
        GreaterTalismanMerchant,
        SuperiorTalismanMerchant,
        PotentTalismanMerchant,
        SpecializedArmorsmith,
        LightMountVendor,
        HeavyMountVendor,
        SpecialtyMountWrangler,
        RenownArmorQuartermaster,
        RenownWeaponQuartermaster,
        CompanionKeeper,
        NoveltyVendor,
        TacticalAdvisor,
        BarberSurgeon,
        CommoditiesQuartermaster,
        SundriesQuartermaster,
        BlackMarketMerchant,
        General,
        FortressGeneral,
        RelicGuardian,
        WarCrestVaultKeeper
    }

    public class Constants
    {
        public const uint LastNameLevelRequirement = 20;
        public const uint LastNameChangeCost = 50000; // 5g
        public const uint LastNameCharacterLimit = 12;

        private const int NumberOfEquipSlots = 20;

        /// <summary>
        /// The factor to convert a heading value to radians
        /// </summary>
        /// <remarks>
        /// Heading to degrees = heading * (360 / 4096)
        /// Degrees to radians = degrees * (PI / 180)
        /// </remarks>
        public const double HEADING_TO_RADIAN = (360.0 / 4096.0) * (Math.PI / 180.0);

        /// <summary>
        /// The factor to convert radians to a heading value
        /// </summary>
        /// <remarks>
        /// Radians to degrees = radian * (180 / PI)
        /// Degrees to heading = degrees * (4096 / 360)
        /// </remarks>
        public const double RADIAN_TO_HEADING = (180.0 / Math.PI) * (4096.0 / 360.0);

        public const int UNITS_TO_FEET = 12;
        public const int UNITS_TO_FEET_MIN = 10;
        public const int UNITS_TO_FEET_MAX = 12;

        //as I typed them I will not erase but they are
        // 2^(i-1) where i is race number in GameData::Races
        public const int RaceMaskDwarf = 0x01;

        public const int RaceMaskOrc = 0x02;
        public const int RaceMaskGoblin = 0x04;
        public const int RaceMaskHighElf = 0x08;
        public const int RaceMaskDarkElf = 0x10;
        public const int RaceMaskEmpire = 0x20;
        public const int RaceMaskChaos = 0x40;

        // 2^(i-1) where i is career number in GameData::CareerLine
        private const int CareerMaskIronbreaker = 0x00000001;

        private const int CareerMaskSlayer = 0x00000002;
        private const int CareerMaskRunepriest = 0x00000004;
        private const int CareerMaskEngineer = 0x00000008;
        private const int CareerMaskBlackOrc = 0x00000010;
        private const int CareerMaskChoppa = 0x00000020;
        private const int CareerMaskShaman = 0x00000040;
        private const int CareerMaskSquigherder = 0x00000080;

        private const int CareerMaskWitchhunter = 0x00000100;
        private const int CareerMaskKnightoftheblazingsun = 0x00000200;
        private const int CareerMaskBrightwizard = 0x00000400;
        private const int CareerMaskWarriorpriest = 0x00000800;
        private const int CareerMaskChosen = 0x00001000;
        private const int CareerMaskMarauder = 0x00002000;
        private const int CareerMaskZealot = 0x00004000;
        private const int CareerMaskMagus = 0x00008000;

        private const int CareerMaskSwordmaster = 0x00010000;
        private const int CareerMaskShadowwarrior = 0x00020000;
        private const int CareerMaskWhitelion = 0x00040000;
        private const int CareerMaskArchmage = 0x00080000;
        private const int CareerMaskBlackguard = 0x00100000;
        private const int CareerMaskWitchelf = 0x00200000;
        private const int CareerMaskDiscipleofkhaine = 0x00400000;
        private const int CareerMaskSorcerer = 0x00800000;

        // same concept with skill mask on uint32 as 2^(i-1) where i is the skill number in GameData::SkillType

        private const int MAX_RENOWN = 100;

        // Values for default, not Dooms Day campaign
        /*public static int[] MinTierLevel = { 0, 16, 16, 31 };
        public static int[] MaxTierLevel = { 15, 30, 30, 40 };*/

#if DEBUG
        public static int DoomsdaySwitch = 2; // DoomsdaySwitch = 0 - Aza age RvR; 1 - single pairing open; 2 - 1 zone open per pairing
        public static bool DisableDebolster = true;
        public static int[] MinTierLevel = { 0, 16, 16, 40 };
        public static int[] MaxTierLevel = { 15, 39, 39, 40 };
#else
        public static int DoomsdaySwitch = 2;
        public static bool DisableDebolster = true;
        public static int[] MinTierLevel = { 0, 16, 16, 16 };
        public static int[] MaxTierLevel = { 15, 40, 40, 40 };
#endif

        public static string[] RacesName = new string[8]
        {
            "",
            "Dwarf",
            "Orc",
            "Goblin",
            "High Elf",
            "Dark Elf",
            "Empire",
            "Chaos"
        };

        public static string[] RegionName = new string[18]
        {
            "Unknown 0",
            "Dwarf/Greenskin Tier 1",
            "Dwarf/Greenskin Tier 4",
            "High Elf/Dark Elf Tier 1",
            "High Elf/Dark Elf Tier 4",
            "Pigwank",
            "Empire/Chaos Tier 3",
            "Altdorf",
            "Empire/Chaos Tier 1",
            "The Necropolis of Zandri",
            "Dwarf/Greenskin Tier 3", // 10
            "Empire/Chaos Tier 4",
            "Dwarf/Greenskin Tier 2",
            "Unknown 13",
            "Empire/Chaos Tier 2",
            "High Elf/Dark Elf Tier 2",
            "High Elf/Dark Elf Tier 3",
            "The Inevitable City",
        };

        public static string[] CareerNames = new string[]
        {
            "",
            "Ironbreaker",
            "Slayer",
            "Runepriest",
            "Engineer",
            "Black Orc",
            "Choppa",
            "Shaman",
            "Squig Herder",
            "Witch Hunter",
            "Knight",
            "Bright Wizard",
            "Warrior Priest",
            "Chosen",
            "Marauder",
            "Zealot",
            "Magus",
            "Swordmaster",
            "Shadow Warrior",
            "White Lion",
            "Archmage",
            "Blackguard",
            "Witch Elf",
            "Disciple of Khaine",
            "Sorcerer"
        };
    }

    public enum FLAG_EFFECTS //data/gamedata/flageffects.csv
    {
        Bomb = 1,
        Red = 2,
        Blue = 3,
        Mball1 = 4,
        Mball2 = 5,
        Flag6 = 6,
        Flag7 = 7,
        Flag8 = 8,
        Flag9 = 9,
        Flag10 = 10,
    }

    public enum ContributionDefinitions
    {
        BO_TAKE_BIG_TICK,
        BO_TAKE_SMALL_TICK,
        BO_TAKE_UNLOCK_TICK,
        PLAYER_KILL_DEATHBLOW,
        PLAYER_KILL_ON_BO,
        KILL_KEEP_LORD,
        DESTROY_OUTER_DOOR,
        DESTROY_INNER_DOOR,
        DESTROY_SIEGE,
        PLAYER_KILL_ASSIST,
        PLAY_SCENARIO,
        WIN_SCENARIO,
        KEEP_DEFENCE_TICK,
        PLAYER_KILL_DEATHBLOW_UNDER_AAO,
        PLAYER_KILL_ASSIST_UNDER_AAO,
        GROUP_LEADER_BO_BIG_TICK,
        GROUP_LEADER_KILL_KEEP_LORD,
        RESURRECT_PLAYER,
        OUT_OF_GROUP_HEALING,
        PARTY_KILL_ASSIST,
        REALM_CAPTAIN_KILL,
        PLAYER_KILL_ASSIST_ON_BO,
        PUNT_ENEMY,
        HOLD_THE_LINE,
        TANK_GUARD,
        GENERAL_HEALING,
        KNOCK_DOWN,
        AOE_ROOT,
        FORT_ZONE_LOCK_PRESENCE
    }
}