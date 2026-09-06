# Current creature level scaling — 2026-09-05

This describes ProjectWAR code at f6f022c3, not a verified official 1.4.8 formula set.
The user asked how a level-40 player fighting a level-40 creature differs from fighting a
level-41 or higher creature. No gameplay code or database values were changed by this review.

## Ordinary level increases

Compare the same prototype, rank and modifiers, with an unbolstered level-40 player:

- `Creature.GetStrikeDamage` is proportional to level: constants 50, 120, 300, 700 for
  normal/champion/hero/lord, replaced by nonzero prototype WeaponDPS. `ItemsInterface` uses
  one tenth of this value as the weapon-damage contribution, with integer truncation.
  Level 40 to 41 increases this component by 2.5%, not necessarily the final hit by 2.5%.
- `Creature.SetCreatureStats` loads level-indexed character base stats and adds rank/power
  bonuses. Its generated armor and resistance additions scale with level (subject to
  truncation); explicit creature stat rows and buffs also affect totals.
- `Creature.GenerateWounds` uses `70 * (level + floor(level/2))`, multiplies by the rank
  factor (1/2/8/16), divides by 10 and applies WoundsModifier before integer conversion.
  The pre-conversion base rises from 60 to 61 units between 40 and 41: about 1.67%.
  Actual health can be changed by overrides or buffs.
- `CombatManager.CheckArmorReduction` divides the target's armor by attacker effective
  level times 44 and multiplies by 0.4 before penetration and a 75% cap. Holding armor and
  penetration fixed, higher attacker level reduces the mitigation fraction. Example:
  2200 armor, no penetration, yields 50% at attacker level 40 and about 48.78% at 41.
  Resistance reduction similarly uses attacker level, with its own soft/hard caps.
- `CheckCriticalHit` uses attacker effective level and defender Initiative. Increasing
  attacker level raises the pre-truncation base chance, though integer rounding can leave
  the actual chance unchanged. The main block/parry/dodge/disrupt helpers use opposed stats
  rather than a flat per-level-gap miss penalty (`CalculateBlockRoll`, `CalculatePDDRoll`).

Player damage does not acquire a symmetric blanket penalty merely because the creature is
one level higher. Its increased defenses/toughness and other encounter settings still matter.
Ward tier and skull count do not automatically change when level changes.

## Extra NPC auto-attack bonus after a three-level gap

`WorldServer/World/Abilities/CombatManager.cs:1407-1408`, inside
`InflictAutoAttackDamage`, adds:

`max(0, creature.Level - target.EffectiveLevel - 3) * 0.4`

to DamageBonus for creatures excluding pets. `AbilityDamageInfo.DamageBonus` starts at 1;
`ApplyDamageModifiers` multiplies damage by DamageBonus and DamageReduction. Consequently,
with no other bonus/reduction, the additional level-gap factors against effective level 40 are:

| Creature level | Added bonus | Factor from this rule alone |
| --- | --- | --- |
| 40–43 | 0% | 1.0 |
| 44 | 40% | 1.4 |
| 45 | 80% | 1.8 |
| 46 | 120% | 2.2 |
| 48 | 200% | 3.0 |
| 52 | 360% | 4.6 |

This stacks with the creature's higher base stats and the independent ward calculation.
It is additive with other DamageBonus contributions, so it is not universally a separate
1.4/1.8/etc multiplier on an already-buffed final hit. The explicit rule occurs in the
auto-attack path, not the ordinary ability, proc or separate offhand paths; do not describe
it as a multiplier on all creature damage.

## Restoration evidence limits and difficulty-mode consequence

The external toolkit was searched first. Its
`RE_FINDINGS/combat/combat_formulas.md`, section 1.11, records the same gap bonus by citing
emulator CombatManager code; that is not independent retail authority. Its CF-03/CF-04
findings also flag the armor/resistance constants as requiring retail verification.
No official per-level NPC gap-bonus evidence was established in this review.

Hard Gunbad at 40–52 would encounter this steep auto-attack increase against level-40 players.
Audit this rule and encounter stats before judging Hard balance or adding Nightmare multipliers.
See `DUNGEON_DIFFICULTY.md` for the user's pending baseline choice and normal completion gate.
