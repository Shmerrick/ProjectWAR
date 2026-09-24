# Ability damage: how the client computes it, and where we diverge

Cooldown counts below are historical. Migration 11 closes all 49 fractional-value
gaps using `abilityexport.bin` record +4; [current verification and limits](handoffs/2026-09-24-cooldown-milliseconds.md).

Established 2026-09-08 by measurement against the extracted 1.4.8 client, the world database and
the packet captures. Read this before touching `mythic_src_ability_damage_heals`,
`AbilityDamageInfo`, or anything in `AbilityMgr` that mentions scaling.

**Compare the same stage of damage calculation.** The client tooltip supplies evidence
for its displayed base/scaled value; a final hit alone does not establish disagreement.
`CombatManager` applies attacker/target modifiers, toughness, armor/resistance and
critical results after `AbilityDamageInfo.GetDamageForLevel`. Control those conditions
before comparing a tooltip to a hit. This describes the emulator's pipeline, not proof
that its formulas match retail. See the [September 24 audit](handoffs/2026-09-24-repository-audit.md).

---

## 1. Reading a damage number out of the client

Tooltips live in `data/strings/english/abilitydesc.txt` with the numbers left as tokens:

```
{ [ABIL_<abilityId>_]  COM_<componentIndex>  _  <field>  _  <meaning> }
```

| Part | Meaning |
|---|---|
| `componentIndex` | indexes **that ability's own ordered component list** — not a component id |
| `field` | `VAL<n>` for the nth entry of `Values`, or a scalar: `DURA`, `FREQ`, `RADI` |
| `meaning` | presentation only — `DAMAGE`, `SPIRITDAMAGE`, `TOD_DAMAGE`, `SECONDS`, `FEET` |
| `ABIL_<id>` | optional; addresses a **different** ability's components |

**The value is `Values[slot] × Multipliers[slot] / 100`.** The multiplier is not optional. Ignoring
it left 21 abilities off by exactly 4×, 15 by 2×, 10 by 6× and 8 by 8× — a tidy integer ratio across
unrelated abilities is never balance drift, it is a missing factor.

```
627 Sever Nerve   40 x 400% = 160    our MinDamage 160
670 Mage Bolt     40 x 200% =  80    our MinDamage  80
5   KABOOM!       50 x 110% =  55    our MinDamage  55
```

**The ordered component list is `abilityexport.bin`'s `ComponentIds`, in slot order with empty
slots skipped, each looked up in `abilitycomponentexport.bin`.** The toolkit import
`mythic_bin_ability.MythicComponentData` carries the same lists with an explicit `Index`, but it is a
copy: it differs from the client on 60 of 11,692 abilities and lacks 12 the client has, so since
2026-09-13 the crosswalk reads the client and checks the import instead. Do *not* use the ability
report's `Related component IDs` line — it is a **sorted set**. For ability 7 it prints `2, 3301`
while the real order is `3301, 2`, which resolves `COM_0` to the wrong component and corrupts every
token above index 0. Ability 1 agrees under both readings, so a single-ability check proves nothing.

Worked end to end, ability 7 *Spine Fling* — *"dealing `{COM_1_VAL0_DAMAGE}` every second for
`{COM_0_DURA_SECONDS}`"*: ordered components `3301, 2`; `COM_0` → 3301, Duration 3000ms → three
seconds; `COM_1` → component 2, `Values [15,0,…]` → **15**, exactly our `MinDamage`.

Implemented in `ClientDataMatrix/Services/AbilityTokenResolver.cs`. Run
`ClientDataMatrix crosswalk abilities`, or use the **Ability Crosswalk** tab, whose *How this works*
page restates all of the above on screen.

**Measured:** 1,368 of 1,414 damage tokens resolve (96.75%) — the same count whether the component
lists come from the client or from the import. All 46 failures are accounted for. 5 tokens belong to
abilities the client lists no components for (9024, 9254, 15335, 15554). The other 41 are one
contiguous family (7717–7756 plus 15557, the blast potions) sharing a template that references
`COM_2` while the client's own records hold two components. That was first written up as a gap in
`mythic_bin_ability`; reading the client shows it has two as well, so the template names a component
the records never carried. Either way it is not a fault in the reading: a wrong reading fails
scattered across a dataset, not in one block of forty consecutive ids of the same potion line.

Of 408 comparable abilities, **393 agree (96.32%)** and 15 are candidate drift -- 381 and 27 until migration 07 took the client's value on the 12 that pair one damage row with one token. Two classes are
excluded as not comparable and listed separately: cross-reference tokens (Spine Fling 392 renders
`ABIL_7_COM_1`, the pet's damage) and reference-like values (ability 5's component 142 carries 3682
in `Values[0]`, an id the client follows rather than prints).

**The crosswalk also checks the rest of the ability row** (2026-09-13). Every `mythic_src_abilities`
row with a client record is compared column by column under measured unit rules, and each ability's
first own `DURA` and `FREQ` token against `mythic_src_buff_infos` (buff `Entry` = ability id).
*Differ* counts only rows where both sides carry a value. Measured after migrations 00-10
(2026-09-14), which took the client's value wherever the column can hold it and left the rest as a
queue — see `docs/DATABASE_FIDELITY_PLAN.md`:

| Column | Rule | Agree | Differ | Ours empty | Client empty | Not representable |
|---|---|---:|---:|---:|---:|---:|
| CastTime | client ms = ours ms | 2,422 | 0 | 0 | 0 | 0 |
| Cooldown | client ms = ours s × 1000 | 3,215 | 0 | 24 | 0 | 25 |
| Range | client = ours ft × 12 | 2,866 | 0 | 0 | 0 | 574 |
| ApCost | client = ours | 2,732 | 0 | 8 | 0 | 0 |
| Channel | client `FlagsRaw` bit 22 = ours `ChannelID` set | 5,737 | 0 | 127 | 0 | 0 |
| ChannelDuration | client first timed component `Duration` ms = ours ms | 78 | 0 | 0 | 5 | 0 |
| ChannelInterval | client `ChannelInterval` ms = ours ms | 83 | 0 | 0 | 0 | 0 |
| EffectID | client = ours | 5,864 | 0 | 0 | 0 | 0 |
| CareerLine | ours mask = 1 << (client line − 1) | 5,299 | 2 | 91 | 403 | 0 |
| Duration | `DURA` ms = buff Duration s × 1000 | 592 | 5 | 0 | 0 | 0 |
| Interval | `FREQ` ms = buff Interval ms | 27 | 0 | 0 | 0 | 0 |

A value on one side and none on the other is listed but not counted as drift, because it is often a
convention — the client binds the Squig pet abilities 6–9 to no career line while ours carry a mask.
`MinRange`, `RADI` and heal tokens are not compared: no mapping for them is proven, and the report
says why. The same report checks the toolkit import against the client — 60 component lists and 3
of 2,760 upgrade-table items differ — so anything read from `mythic_bin_*` inherits those.

---

## 2. How the server turns that into a hit

`WorldServer/World/Abilities/Components/AbilityDamageInfo.cs`:

```
MaxDamage == 0:  damage = ((level - 1) * LevelScalingFactor * MinDamage) + MinDamage
MaxDamage  > 0:  damage = MinDamage + (MaxDamage - MinDamage) * ((level - 1) / 39)
```

**The Min/Max question is settled: the second branch is dead.** `MaxDamage` is NULL on 1,433 of
1,434 rows and the one exception equals `MinDamage`; `DamageVariance` is NULL on 1,401, so its
random spread is a no-op too. The data is single-valued, matching the client's one-value-plus-
multiplier shape. Those columns are vestigial, not actively wrong.

So at rank 40 the live branch yields **7.5 × MinDamage**, and everything rests on
`LevelScalingFactor`.

---

## 3. Morale abilities are flat — fixed, BUG-152

`MoraleLevel` is the pool rank that **unlocks** an ability, not a multiplier. Using one spends the
whole pool and deals flat, unmitigated damage, so the tooltip's number is what should land.

Two facts confirm our data already holds it: of the 73 morale abilities we carry a `MinDamage` for,
**69 equal the client tooltip value exactly**; and **none of the 174 morale abilities has an upgrade
bin**, because there is nothing to scale.

That second fact was the bug. "No upgrade bin" was read as "use the default scalar" rather than
"does not scale", so every morale ability was inflated 7.5× at rank 40:

| Ability | Morale | Tooltip | Server dealt |
|---:|---|---:|---:|
| 9231 | L4 | 1000 | **7,500** |
| 1420 | L3 | 960 | 7,200 |
| 8611 | L4 | 720 | 5,400 |

Fixed by flattening morale rows to a factor of 0 **before** the upgrade tables are consulted, and by
correcting `GetDamageForLevel`, which treated `<= 0` as invalid and substituted the default — a
deliberate zero would not have survived. Needs in-client testing.

---

## 4. The upgrade table — what is known, and what is not

`mythic_bin_abilityupgradeentry` / `...upgradebin` supply `LevelScalingFactor`.

**Known**, from the client's own code (`world::AbilityUpgradeTable` constructor, Ghidra
FUN_009266db; loader FUN_0054e313):

- a record is `0x150` = 336 bytes: 20 items of 16 bytes, plus a `uint16` id at `+0x144`
- each item is **three 32-bit fields followed by four single bytes**
- so our columns pair up: `V1+V2` is field 0, `V3+V4` field 1, `V5+V6` field 2, `V7+V8` the bytes

**Known**, from the data: field 0 is a 32-bit float. `0/16256` decodes to exactly 1.0 and `0/16384`
to exactly 2.0 — random bytes do not land on those. The table holds 1.0 (60 rows), 0.166667 (25),
0.8147, 0.7346, 0.6544, 0.5742, 0.5344, 0.4941, 0.2052 in blocks of 12–16.

**Known**, from the client's evaluator (2026-09-14). The toolkit's Ghidra export of WAR.exe
(`RE_FINDINGS/world/war_ghidra_all.c`) holds `FUN_00926777(object, key, level)`, which walks the 20
items and uses one when field 1 equals a `uint32` at offset `0x2c` of the object, field 2 equals
`key`, and the level has reached the item's minimum:

| Bytes | Meaning |
|---|---|
| `V7` low | minimum level |
| `V7` high | level cap |
| `V8` low | step |
| `V8` high, bit 0 | additive flag |

`steps = (min(level, cap) − minimum) / step + 1`, and the item adds `steps × factor × base` to the
base value — or `steps × factor` when the additive flag is set. Record 1 item 0 (`V7 = 0xC802`: from
level 2, cap 200, step 1, flag clear) gives `base + (level − 1) × 0.166667 × base`, the server's
formula exactly. It also shows why one scalar per bin cannot be right: 117 of the 437 populated items
are `(1, 1, 1)`, a single flat step, and 144 carry the additive flag. What the client passes as
object and key is still unknown — the evaluator is reached through a vtable, so the export shows no
caller — which is why field 1 stays undecoded below.

**The `V` names are placeholders.** They come from an importer that read each item as eight
`uint16`s and numbered them; WAR-RE-Toolkit's exporter calls the same fields `A00A`, `A00B`, `A01A`,
`A01B`. The item bytes above are the only part decoded so far.

**Not known: what field 1 means.** Every hypothesis tested so far has failed:

- *Field 1 selects a property, 1 = damage.* Dead. Ability 41 "Bite", whose entire description is
  `Deals {COM_0_VAL0_DAMAGE} to the target.`, has a bin carrying only code 4.
- *Field 1 is the component operation.* Dead. Of 112 abilities with both a bin and component data,
  15 match fully, 24 partially, **73 not at all** — e.g. ability 4038 has code 2 against operations
  1 and 13.
- *The table is the mastery system.* Unsupported. Only 21 of 138 bins belong to abilities with a
  `Specialization`, and `Specialization` has 4 distinct values — a path index, not a rank. Mastery
  touches far more than 138 abilities.
- *Field 1 is the caster's career line.* Dead (2026-09-14). Its values run 1–24, exactly the career
  line range, but taking the record id as an ability id, 388 items belong to abilities with no career
  line and the other 10 never match.

**A lead on field 2, unproven.** It takes only 0, 1, 2, 3, 16 and 18, and the evaluator treats keys
below 4 specially, with a product over 100 in the `Values × Multipliers` shape. That fits 0–3 naming
value slots and 16 and 18 naming scalar fields such as duration and radius — the four items of ability
22 Machine Gun each add a flat 5000 with field 2 = 18 — but nothing yet ties a key to a component.

**Current behaviour, and why it is still wrong.** The server log reports
`upgrades=70, ability_entries=211, applied_rows=121, unresolved_rows=3940` — 121 damage rows get a
per-ability scalar and 3,940 keep the default, about 3%. `TryExtractUpgradeLevelScalar` now filters
to `V3 = 1` on the unproven inference that 1 means damage; that filter is defensible only as *less
arbitrary* than what it replaced, which took whichever fractional scalar came first from any field
and gave ability 4011 a damage factor of 0.005. Tracked as BUG-151, deliberately left Open.

---

## 5. The packet captures, and why they are the way out

Decoding the upgrade table is not the only route, and probably not the best one. The 1,027 capture
logs hold what the live server actually did:

| Opcode | Packets |
|---|---:|
| `F_HIT_PLAYER` 0x14 | 4,071,405 |
| `F_USE_ABILITY` 0xDA | 722,775 |
| `F_CAST_PLAYER_EFFECT` 0xB3 | 717,794 |
| `F_DO_ABILITY` 0xD5 | 204,260 |

`F_CAST_PLAYER_EFFECT` has two shapes. 315,333 are the short 13-byte cast notification; **~402,000
are the damage form**, which `CombatManager.cs:1259` writes as
`caster(2) target(2) DisplayEntry(2) subId(1) damageEvent(1) flag(1) ZigZag(-damage)
[ZigZag(mitigation)] [ZigZag(absorption)]`.

Ability id, damage **and mitigation as separate fields** — so the pre-mitigation number is
recoverable and can be compared to a tooltip without modelling armour.

**The head decodes cleanly against real captures; the tail does not yet.** Caster, target and
ability id land where expected, and one sampled packet's flag byte is the `07` our writer emits, but
others carry `0B` and `00 00 00 0B`. Our writer is a reimplementation, so establishing the live
layout is a genuine RE task and belongs in the toolkit's `RE_FINDINGS/network`.

Two caveats for whoever builds the extractor: the packet carries no level, so a caster's rank has to
come from elsewhere (most capture filenames say — `SORC40RR84`, `DOK LVL 40 RR 100`); and stats
inflate an observed hit, so it will not equal a raw component base. Neither prevents the thing that
matters — an ability running at 1.2× base instead of 7.5× is unmissable regardless.

---

## Where to look

| Question | Go to |
|---|---|
| How a token resolves | `ClientDataMatrix/Services/AbilityTokenResolver.cs` |
| Which abilities disagree | `crosswalk abilities`, or the Ability Crosswalk tab |
| The explanation, on screen | Ability Crosswalk → *How this works* |
| How the server scales | `AbilityDamageInfo.GetDamageForLevel`, `AbilityMgr.ApplyPerAbilityLevelScalars` |
| Item-side equivalent | `docs/DATABASE_FIDELITY_PLAN.md`, `crosswalk items` |
