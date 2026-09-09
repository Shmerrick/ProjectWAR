# Londo's dump: what we use, and what we have never touched

Audited 2026-09-08. Short version: **we have used one of his 68 tables.**

The repository owner's assessment, recorded because it changes how this data should be weighed:
Londo's layer is the best-developed of the contributions that make up `war_world`, ahead of WarEmu's
public base and the released Return of Reckoning database, owing to his connection with the Mythic
developers. `docs/CROSS_REPO.md` previously ranked it tier 3, "corroboration, never settles one
alone", which under-rates it. The dump's *form* is still a reconstruction — `Unk5`–`Unk24`
placeholder columns, a MySQL 8.0.13 header from five years after shutdown — but its *content* is
the strongest non-capture source available.

## The tables

`D:\Repos\Shmerrick\WAR-RE-Toolkit\data\database-tables\Londos Server v2`, 68 `.sql` files:

```
Ability  PatcherAsset  AssetHash  AbilityBin  ObjectCSV  AbilityComponentBin  ItemStatistic
Item  AbilityExpression  MonsterItem  Package  EffectAnim  Monster  EffectDef  AbilityXGroup
EffectVfx  Effect  EffectNif  ItemCSV  EffectList  Door  AbilityUpgradeEntry  PackageName
StaticObject  AbilityXLabel  MonsterStatistic  ItemSetBonus  Player  ItemCraft  Category
AbilityLine  ItemSet  ZoneSpawnPoint  Zone  ItemAbility  BonusType  ItemBuff  Career
AbilityLabel  Region  PlayerItem  PlayerPackage  EffectProj  ServerInfo  AbilityUpgradeBin
PatcherFile  CareerStatistic  MonsterActiveEffect  CareerType  CareerLine
AbilityComponentXComponent  AbilityControl  PlayerItemTalisman  MonsterEffect  AbilityLineNames
abilitylinetobufftype  Environment  Account  LocalizedString  Objective  LocalizedStringGroup
Scenario  AccountConfig  Race  Archtype  AbilityRequirmentBin  routines  sysdiagrams
```

**Imported: `Item` only**, and only during this session, to source migration 87.

## The code already has a Londo path, and it has never fired

`AbilityMgr.BuildMythicUpgradeLevelScalars` falls back to `LondoAbilityUpgradeBinRow` /
`LondoAbilityUpgradeEntryRow` when the Mythic tables come up empty, and
`Common/Database/World/Ability/MythicAbilityGraphTables.cs` declares Londo-shaped entities against
plain table names — `Ability`, `AbilityBin`, `AbilityComponentBin`, `AbilityComponentXComponent`,
`AbilityExpression`, `AbilityUpgradeBin`, `AbilityUpgradeEntry`.

Every one of those tables exists in `war_world` and **every one has zero rows**. The ORM
auto-created them on registration (`CheckOrCreateTable`) and nobody ever loaded the data. The
fallback is dead code in practice.

## What is worth mining, in rough order

| Table | Size | Why |
|---|---|---|
| `Ability` | 31 MB | The largest thing in the dump, and abilities are where our worst divergences are |
| `AbilityBin` | 5.5 MB | |
| `AbilityComponentBin` | 2.0 MB | Our copy comes from the client bin; a second opinion on component values |
| `AbilityComponentXComponent` | 2.7 KB | **Empty in the dump** — the link table is missing there too |
| `AbilityUpgradeEntry` / `AbilityUpgradeBin` | | A second copy of the table whose field 1 we cannot decode. Worth diffing against the client's |
| `ItemStatistic` | 1.6 MB | 32,359 stat rows over 8,597 items — the `Stats` column content |
| `BonusType` | 6 KB | The stat enumeration, below |
| `MonsterStatistic`, `Monster`, `MonsterItem` | | Creature data |
| `ItemSet`, `ItemSetBonus`, `ItemBuff`, `ItemAbility` | | Set bonuses and item procs |
| `Door`, `StaticObject`, `ZoneSpawnPoint` | | World objects |
| `LocalizedString`, `LocalizedStringGroup` | | |

### ItemStatistic — measured, and smaller than it looks

32,359 rows across 8,597 items. Only **6,662 rows covering 1,784 items** overlap what we hold, and
of those just **72 have an empty `Stats` column**. So it is not a large gap-filler. Its real value
is as a **comparison** set for the 1,712 items where both sides carry stats — the same
violation/hole/suspect treatment the item and ability crosswalks already do.

Migration 87's header claims Londo "does not carry [stats] in a form we can trust". That was written
without opening the file and is wrong: `ItemID`, `BonusTypeID`, `Value`, `IsPercent`, `Duration` is a
perfectly usable shape. What is true is that it covers few of the items 87 added.

### BonusType — the stat enumeration, in Mythic's own words

118 named entries, and they confirm the enum CLAUDE.md already records (1 Strength … 9 Intelligence)
from an independent source. Reproduced because it names fields we would otherwise guess at:

| | | | | | |
|---:|---|---:|---|---:|---|
| 1 | STRENGTH | 2 | AGILITY | 3 | WILLPOWER |
| 4 | TOUGHNESS | 5 | WOUNDS | 6 | INITIATIVE |
| 7 | WEAPONSKILL | 8 | BALLISTIC | 9 | INTELLIGENCE |
| 10 | BLOCKSKILL | 11 | PARRYSKILL | 12 | EVADESKILL |
| 13 | DISRUPTSKILL | 14 | SPIRIT_RESIST | 15 | ELEMENTAL_RESIST |
| 16 | CORPOREAL_RESIST | 22 | INC_DAMAGE | 23 | INC_DAMAGE_PERCENT |
| 24 | OUT_DAMAGE | 25 | OUT_DAMAGE_PERCENT | 26 | ARMOR |
| 27 | VELOCITY | 28 | BLOCK | 29 | PARRY |
| 30 | EVADE | 31 | DISRUPT | 32 | AP_REGEN |
| 33 | MORALE_REGEN | 34 | COOLDOWN | 35 | BUILD_TIME |
| 36 | CRITICAL_DAMAGE | 37 | RANGE | 38 | AUTO_ATTACK_SPEED |
| 39 | RADIUS | 40 | AUTO_ATTACK_DAMAGE | 41 | AP_COST |
| 42 | CRITICAL_HIT_RATE | 43 | CRITICAL_DAMAGE_TAKEN | 44 | EFFECT_RESIST |
| 45 | EFFECT_BUFF | 46 | MIN_RANGE | 47 | DAMAGE_ABSORB |
| 48 | SETBACK_CHANCE | 49 | SETBACK_VALUE | 50 | XP_WORTH |
| 51 | RENOWN_WORTH | 52 | INFLUENCE_WORTH | 53 | MONETARY_WORTH |
| 54 | AGGRO_RADIUS | 55 | TARGET_DURATION | 56 | SPEC |
| 57 | GOLD_LOOTED | 58 | XP_RECEIVED | 59 | BUTCHERING |
| 60 | TRADE_SKILL_SCAVENGING | 61 | TRADE_SKILL_CULTIVATION | 62 | TRADE_SKILL_APOTHECARY |
| 63 | TRADE_SKILL_TALISMAN | 64 | TRADE_SKILL_SALVAGING | 65 | STEALTH |
| 66 | STEALTH_DETECTION | 67 | HATE_CAUSED | 68 | HATE_RECEIVED |
| 69 | OFFHAND_CHANCE | 70 | OFFHAND_DAMAGE | 71 | RENOWN_RECEIVED |
| 72 | INFLUENCE_RECEIVED | 73 | DISMOUNT_CHANCE | 74 | GRAVITY |
| 75 | LEVITATION_HEIGHT | 76 | CRITICAL_HIT_RATE_MELEE | 77 | CRITICAL_HIT_RATE_RANGED |
| 78 | MAGIC_CRITICAL | 79 | HEALTH_REGEN | 80 | DAMAGE_MELEE |
| 81 | DAMAGE_RANGED | 82 | MAGIC_POWER | 83 | ARMOR_PENETRATION_REDUCTION |
| 84 | CRITICAL_HIT_RATE_REDUCTION | 85 | BLOCK_STRIKETHROUGH | 86 | PARRY_STRIKETHROUGH |
| 87 | EVADE_STRIKETHROUGH | 88 | DISRUPT_STRIKETHROUGH | 89 | CRITICAL_HIT_RATE_HEALING |
| 90 | MAX_ACTION_POINTS | 91 | SPEC_1 | 92 | SPEC_2 |
| 93 | SPEC_3 | 94 | HEALING_POWER | 95 | INTERACT_TIME |
| 96 | FORTITUDE | 150 | HASTE | 151 | MOVE_SPEED |
| 152 | HEALTH | 153 | ITEM_EFFECT | 154 | AUTO_ATTACK_SPEED2 |
| 155 | DAMAGE_ABSORB_MELEE | 156 | AP_POOL | 157 | MORALE_POOL |
| 158 | AP | 159 | MORALE | 160 | DAMAGE |
| 161 | CRITICAL_DAMAGE_REDUCTION | 162 | DAMAGE_ABSORB_MAGICAL | 163 | MAX |

17–21, 97 and 98 are `UNK_*` in the dump itself — undecoded there too, not omitted here.

**`MORALE_POOL` (157), `MORALE` (159), `AP_POOL` (156) and `AP` (158) are worth noting** against the
open question of what the ability upgrade table's field 1 selects: a pool-modifier enumeration was
one hypothesis, and this is what such an enumeration looks like in Mythic's own vocabulary. The
codes do not line up with the upgrade table's observed values (1, 2, 3, 5, 4, 22, 8, 13…) in any way
that has been demonstrated, so this is a lead, not an answer.

## Caveat that still applies

Roughly 7,092 of the dump's 9,948 `Item` rows are the alphabetically-ordered block above id 100M
with fabricated sequential ids and zeroed `ModelID`/`SlotIndex`/`ItemTypeID` — a harvested name list,
not server data. Any bulk use of a Londo table has to exclude that shape or it will import
inventions. Migration 87 did, by requiring the `ModelID` to resolve in `objects.csv`.
