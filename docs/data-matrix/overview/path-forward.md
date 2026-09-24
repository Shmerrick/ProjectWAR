# ClientDataMatrix: Path Forward

Current review: [September 24 repository audit](../../handoffs/2026-09-24-repository-audit.md).
The historical "Unknown=0" classifier result below is not proof that every runtime
operation or upgrade join is understood. Direct client BINs outrank imported blank
fields; the Skaven controls are a verified example. Use the current crosswalk and
the dated generated summaries when choosing the next implementation task.

Last updated: 2026-03-28 (Londos DB + WorldServer + live war_world DB searched; op=43 confirmed; ops 29/30/32/40/41/47/51 row counts and patterns fully documented)

Component field decode is complete (Unknown=0, Structural=0). Requirement semantics fully decoded. This document covers all remaining open work, ordered by impact and tractability.

---

## ~~Area 1: Requirement Semantics~~ — RESOLVED (commit df9fff32)

All 558 requirement rows are fully decoded. Schema confirmed from decompiled client `AbilityExport.cs`:
- Val1 = `AbilitySourceType` (Self/Cast/Apply/Watch/EventReq/Result/Immunity)
- Val2 = `AbilityOperation` (89-value enum — requirement check type)
- Val3 = `AbilityCondition` (None/Equal/LessThanEqual/GreaterThanEqual/LessThan/GreaterThan/FriendlyTarget)
- Val4 = `AbilityLogicOperator` (None/And/Or/Unk10/Unk11/Unk12)
- Val5–Val9 = operation-specific threshold, child RequirementId ref, aux params, binary flag

---

## ~~Area 2: DAMAGE Value[1]~~ — RESOLVED (commit c193be3e)

Confirmed from decompiled server-side `DAMAGE.cs`:
- `Value[1]` = **`MaxCounter`** — counter cap / tick limit (zero = no limit)
- `FlagsRaw` = **`DamageFlag`** enum (NONE=0, UNMITIGATABLE=1)

---

## Area 3: SERVER_COMMAND — Field Mapping Clarification Needed (High)

### What is unresolved

`SERVER_COMMAND (op=36)` Values[0] and Values[1] are not yet analyzed in `TryBuildServerCommandStructuralInference`. The current handler covers Value[2-5], FlagsRaw, Value08, Value15. The Londos DB shows Values[0] = command code and Values[1] = primary argument. Our BIN FlagsRaw (8 distinct values: **1, 2, 4, 5, 6, 16, 18, 175**) is confirmed as a behavioral bit-field, NOT a command code — these are CC/mode bits. The command code lives in Values[0] (also unanalyzed in our BIN).

The Londos DB reveals **72+ distinct command codes** and their argument patterns. The most frequent commands:

| Command code | Count | Values[1] pattern | Values[2] | Interpretation (Inferred) |
|---|---|---|---|---|
| 32 | 310 | 30000–60000 IDs | 0 | Award/grant item or ability by ID |
| 50 | 101 | 30000–50000 IDs | 0 | Similar to 32 |
| 327 | 90 | 1 (constant) | 1 | Toggle/enable state (sub-type 1) |
| 328 | 90 | 1 (constant) | 0 | Toggle/disable state (sub-type 1) |
| 332 | 32 | career/mastery IDs | 5, 5 | Career mastery or specialization unlock |
| 53 | 64 | 87000+ IDs | 0/1/2/3 enum | ID grant with variant selector |
| 173 | 26 | 0 | large IDs | Context reversed — Values[2] = ability ID |
| 23 | 26 | small IDs | 0 | Small-enum command arg |

### Sources searched (2026-03-28)

| Source | Path searched | Result |
|---|---|---|
| RE toolkit AbilityEnums.cs | `D:\Repos\Shmerrick\WAR-RE-Toolkit\docs\research\abilities-bin-file-exporter\AbilityEnums.cs` | `ComponentOperationType` enum names op=36 as `SERVER_COMMAND` but has no Value field names |
| RE toolkit ComponentOP.cs | `D:\Repos\Shmerrick\WAR-RE-Toolkit\apps\admintool\MYPLib\MYPLib\ComponentOP.cs` | Alternative op names for 12/13/15/22/23/25/28; nothing for op=36 Value fields |
| WorldServer MythicAbilityGraphTranslator.cs | `WorldServer/World/Abilities/MythicAbilityGraphTranslator.cs` | Component dispatch switch implements only ops 1, 3, 8, 22, 23. No case for op=36 |
| **Londos DB War_AbilityComponentBin.sql** | `D:\Repos\Shmerrick\WAR-RE-Toolkit\data\database-tables\Londos Server v2\War_AbilityComponentBin.sql` | **1065 op=36 rows; Flags=0 on all rows; Values[0] = command code (72+ distinct); Values[1] = primary arg; Values[2]/[3] = secondary args; confirms polymorphic structure keyed on Values[0]** |

### Remaining approach

**Step 1 — Add Values[0] and Values[1] handlers to `TryBuildServerCommandStructuralInference`.**
These fields have no handler yet. Londos confirms: Values[0] = command code (72+ distinct: 32, 50, 327, 332, etc.), Values[1] = primary argument (ability/item/zone ID or small enum). Add Inferred-confidence handlers for both fields. Note that our BIN Values[2] is currently labeled "primary command argument" — update it to "secondary argument" once Values[0] and [1] are handled.

**Step 2 — Per-command inference.**
Extend `TryBuildServerCommandStructuralInference` to emit per-command-code named inferences keyed on Values[0] (e.g., "command=32: grant/award ID via Values[1]", "command=332: career mastery unlock, Values[1]=career ID, Values[2/3]=5"). The Londos command code table in this document provides the full distribution.

**Step 3 — Identify specific command semantics.**
Work through the 72 command codes. Many can be inferred by correlating Values[1] with known ability/item ID ranges and by examining the ability descriptions of abilities that use each command.

---

## Area 4: Coverage Gaps — 12,664 abilities below Mapped (High)

> **Superseded 2026-09-13 (BUG-165).** Every count in this section came from joining `abilities.csv`
> on ability id, and that sheet is keyed by effect id. With the join corrected the gap is **3,171**
> (Partial 1,962, StringsOnly 1,209) and Mapped rose from 1,984 to 8,657. The "csv + effect-text"
> bucket below — "Has abilities.csv but no BIN row" — was csv rows landing on unrelated ability ids.
> Run `report remaining` for the current patterns; the text below is kept as history.

### Findings from investigation (commit fbb2a83d)

The string files (`abilitynames.txt`, `abilitydesc.txt`) contain 29,001 sequential indexed entries, most of which are blank placeholders. These were inflating the gap count. Fixed by adding a new `BlankSlot` coverage status — fires when an ability's only source data is blank string-table entries. BlankSlot abilities (13,963) are excluded from coverage gap counts as irrecoverable empty placeholder IDs.

True coverage gap is now **12,664** abilities:

| Status | Count | Description |
|---|---|---|
| `Partial` | 11,635 | Have BIN or CSV but missing some pieces |
| `StringsOnly` | 1,029 | Have actual named strings but no BIN/CSV/effect/component |

Top gap patterns (post-BlankSlot fix):
| Pattern | Count | Missing |
|---|---|---|
| bin + effect-text + components | ~2,137 | BIN row but broken effect/component chain |
| csv + effect-text | ~7,133 | Has abilities.csv but no BIN row |
| csv + effect-text + effect-row + components | ~1,633 | Has CSV but no BIN/effect chain |

Extraction is complete — `C:\Users\Admin\Downloads\myps` has all expected files (abilityexport.bin with 11,736 rows, abilitycomponentexport.bin with 18,526 rows, effects.csv with 4,445 rows).

### Remaining approach

**Partial/bin-missing (2,137 abilities with `bin + effect-text + components`):**
These have a BIN row but no component linkage. The effect chain from `EffectId → effects.csv → ComponentId → abilitycomponentexport.bin` is broken. Investigation needed: check if the `EffectId` in `abilityexport.bin` for these abilities links to an effects.csv row that lacks a component chain.

**Partial/csv-missing (~7,133 abilities with `csv + effect-text`):**
Have a BIN row but no abilities.csv row and no effect text. These may be NPC/monster abilities that aren't in the player-facing CSV. Not urgent — the BIN data is the authoritative source.

**StringsOnly (1,029 abilities):**
Have actual non-blank names/descriptions in string files but no BIN/CSV/effect/component. These are likely removed or renamed abilities from a different game version. Low recovery probability.

---

## ~~Area 5: Conflict Hotspots — 2,897 EffectId Conflicts~~ — RESOLVED (commit df9fff32)

> **Note 2026-09-13 (BUG-165).** Two of these three groups were never disagreements between client
> files: `AbilityIdMirrorEffectId` and `MountOverlayEffectId` came from joining `abilities.csv` on
> ability id, and disappeared with that join. The ledger now holds 471 conflicts, all about effects.

All three EffectId conflict groups (AbilityIdMirrorEffectId=2,546, ZeroVsEffectIdGap=289, MountOverlayEffectId=65) already had resolution rules with `CanonicalValue` set. The remaining-work catalog was not filtering resolved conflicts. Fixed by adding `string.IsNullOrWhiteSpace(row.CanonicalValue)` to the `BuildConflictArea` filter and `HighSignalConflictCount` — dropping high-signal conflicts from 2,897 to 0.

Remaining string-mismatch conflicts (AbilityName, AbilityDescription, EffectName) are low-priority noise; all have triage score ≤ 110.

---

## Area 6: Unknown Operation Names (ops 29, 30, 32, 40, 41, 43, 47, 51)

### What is unresolved

Eight operation codes have no confirmed name. Their fields are fully decoded structurally, but the semantic purpose of the operation itself is unknown. Londos DB investigation (2026-03-28) provided contextual evidence for several ops. Op=43 now has a confirmed description. Op=40, 41 have strong contextual inference. Op=51, 47, 29 have partial context. Ops 30 and 32 remain most ambiguous.

| Op | Records | Key field patterns | Londos context + live DB (2026-03-28) |
|---|---|---|---|
| 29 | 12 | Value[0] = ID refs (66001–66300 range) or small enum (0,10); Value[1] = 11 or 40; FlagsRaw = CC bits | 12 rows total. Main cluster: Witch Hunter Accusations chain — Values=[AccusationAbilityID 66001–66300, 11, 10]. Secondary: comp 14230 vals=[0,40], comps 17901/17935–17937 vals=[9979,40]. Outliers: comp 14556 vals=[192555,0], comp 24010 vals=[208470,0,7]. Value[0] = 66xxx = Witch Hunter accusation target ability ID; small values (0, 9979) may index into an accusation tier table. Cannot confirm op name without server code. |
| 30 | 1 | Value[0] = 18 (single record) | **Only 1 row total**: comp 708, HEAD of "Crack Shot" (AbilityID=1536). Crack Shot is a Witch Hunter ability: "deals damage and disarms target." Value[0]=18 is unresolved (could be disarm effect code or weapon skill). No Londos description on component. Cannot confirm op name — only 1 data point. |
| 32 | 15 | Value[0] = high-range IDs (5424–10020); Value[1] = 0, 65, or 100 | 15 rows. comp 2131 is HEAD of "Eye of Sheerian" (AbilityID=8606, WP group armor+resist aura). Values=[5424,65], [5432,65], [7735,0], [6854,0], [9998,100], [8394–8399, 0]. Large ID in Value[0] likely an AoE aura/group-stat reference; Value[1]=65 or 100 may be scaling. Likely a GROUP_STAT or AURA_APPLY op. Cannot confirm name. |
| 40 | 2 | Values=[0] or [1] — binary | **2 rows only**: comp 14201 vals=[0] (state OFF), comp 14209 vals=[1] (state ON). In standard-bearing chain adjacent to SIEGE_CARRY (op=28) and "die with standard" EVENT_LISTENER (op=13). Binary toggle for carrier state. |
| 41 | 4 | Value[0] = 14; Value[1/2/3] = sequential IDs (187701–187706) | **4 rows**: comps 14214, 14215, 14583, 14584. Values=[14, 187701–187703] and [14, 187704–187706]. Same 187701–187706 ID range as op=42 (RECOVER_STANDARD). Note: AbilityID=14214 "Night of Murder" event ability has EffectID=2927 (different from comp 14214). These comps are in the standard-bearing chain after head comp. |
| 43 | 2 | Value[0] = 21 or 27 | **Confirmed**: "Can autoattack while moving." (Londos comp 3431 desc). HEAD of Cleave (AbilityID=5104 via Londos). Live DB: `MoveAndShoot` buff command on "Skirmish Stance" (9094) and "Spiked Squig" (1844). Emulator `BuffEffectInvoker.MoveAndShoot()` sets `CombatInterface_Player.MoveAndShoot = true/false`. |
| 47 | 8 | Value[0] = 0 or 1; Value[1] = 10, 20, 30, or 40 | **8 rows, exactly 4 pairs**: (0,10),(1,10),(0,20),(1,20),(0,30),(1,30),(0,40),(1,40). AbilityBin IDs 10733–10740 are "BotS - Tier 1/2 Glyph/Stone/Rod" (Bite of the Skaven event abilities) with EffectIDs 3266/3958; op=47 comps are in their chains. Value[0]=0/1 = visible/invisible toggle or on/off state; Value[1]=10/20/30/40 = tier/type code (Tier1=10, Tier2=20, Tier3=30, Tier4=40). |
| 51 | 38 | Value[0] = ability/buff reference ID; Value[2] = small enum (0–4); Value[3] = ordinal | **38 rows**. comp 1597 = HEAD of "Searing Vitality" (DoK tactic, AbilityID=8205), vals=[17666,0,0,0]. Comps 27992–27993: desc="You are a coward!", vals=[17643,0,0,13] and [17643,0,0,14]. Comps 26661–26668: vals=[20760–20767, 0, 0–3, 39–42] (battlefront ordinals). Comps 26900–26923: vals=[30030–30069, 0, 1–4, 49–91]. Comps 26941–26943: vals=[31018–31020, 0, 0, 89–91] (fortress ordinals). AbilityID 20760="TEST Skaven - Grey Seer", 20761="Streaked Blast". Value[0] = ability/buff reference; Value[3] = ordinal/rank index. |

### Sources searched (2026-03-28, sessions 1 and 2)

| Source | Path searched | Result |
|---|---|---|
| RE toolkit AbilityEnums.cs | `D:\Repos\Shmerrick\WAR-RE-Toolkit\docs\research\abilities-bin-file-exporter\AbilityEnums.cs` | `ComponentOperationType` enum — **does not contain** ops 29, 30, 32, 40, 41, 43, 47, 51. Also contains `AbilityOperation` (requirement check types) with `DamageType=29`, `SiegeControl=40`, `TargetBack=51` — **these are requirement Val2 check-type codes, NOT component operation codes.** |
| RE toolkit ComponentOP.cs | `D:\Repos\Shmerrick\WAR-RE-Toolkit\apps\admintool\MYPLib\MYPLib\ComponentOP.cs` | Client-side `ComponentOP` enum from decompiled MYPLib.dll. Contains DISABLE=12, PROC=13, BUFF_PARAM=15, BONUS_TYPE=22, LINKED_ABILITY=23, CAREER_RESOURCE=25, SIEGE_CARRY=28. **Does not contain** ops 29, 30, 32, 40, 41, 43, 47, 51. Absence from this enum confirms these ops are **server-side only** — the client does not process them. |
| WorldServer MythicAbilityGraphTranslator.cs | `WorldServer/World/Abilities/MythicAbilityGraphTranslator.cs` | Component dispatch switch: only cases 1, 3, 8, 22, 23. No implementation for any of the 8 unknown ops |
| RE toolkit broader search | `D:\Repos\Shmerrick\WAR-RE-Toolkit\apps\` and `docs\research\` | No additional op-name source found beyond AbilityEnums.cs and ComponentOP.cs. AbilityEngine.cs uses `ComponentOperationType` without adding new values |
| **Londos DB War_AbilityComponentBin.sql** | `D:\Repos\Shmerrick\WAR-RE-Toolkit\data\database-tables\Londos Server v2\War_AbilityComponentBin.sql` | **18,524 component rows (bulk INSERT, 74 lines); op=47 exactly 8 rows; op=51 exactly 38 rows; op=29 exactly 12 rows; op=30 exactly 1 row; op=32 exactly 15 rows; op=40 exactly 2 rows; op=41 exactly 4 rows. Op=43 comp 3431 desc="Can autoattack while moving." Op=51 comp 27992/27993 desc="You are a coward!". All values documented above. No op code enum in this file.** |
| **Londos DB War_AbilityBin.sql** | `D:\Repos\Shmerrick\WAR-RE-Toolkit\data\database-tables\Londos Server v2\War_AbilityBin.sql` | **59-column table; EffectID at column [13]. Confirms: Crack Shot (1536)→comp 708 (op=30); Searing Vitality (8205)→comp 1597 (op=51); Eye of Sheerian (8606)→comp 2131 (op=32); Night of Murder (14214)→EffectID=2927 (NOT comp 14214). BotS abilities 10733–10740 have EffectIDs 3266/3958 (op=46 CATAPULT), not 10733–10740 — op=47 comps are in their chains.** |
| **Live war_world DB buff_commands** | `MySQL 127.0.0.1:3306 war_world, buff_commands table` | **`MoveAndShoot` CommandName on "Skirmish Stance" (Entry=9094) and "Spiked Squig" (Entry=1844) — confirmed emulator translation of op=43. `ObjectEffectState` CommandName on Squig Armor (6), Stealth (12), Chicken (14), BotS transforms (28–30,38) etc.** |
| **WorldServer BuffEffectInvoker.cs** | `WorldServer/World/Abilities/Buffs/BuffEffectInvoker.cs` | `MoveAndShoot()` method (line 3145): sets `CombatInterface_Player.MoveAndShoot = true/false` — op=43 emulator implementation. `ObjectEffectState()` (line 3080): calls `OSInterface.AddEffect(cmd.PrimaryValue)` for visual transforms. `HoldTheLine()` (line 2792): increments HTLStacks. |
| **Common/Database/GameData.cs** | `Common/Database/GameData.cs` line 979 | `ObjectEffectState` enum: BERSERK=1, WAAAGH=2, CARRYING_BANNER=3, KNOCKED_DOWN=4, SCALE_UP=7, MOVEMENT_SPEED=8, TAUNTED=9, DETAUNTED=10, CARRYING_FLAG=11, STEALTH=12, CHAOS_CHICKEN=13, ORDER_CHICKEN=14, etc. `RealmCaptainManager.cs` uses CARRYING_BANNER to mark standard bearers. |

### Key Londos DB chain context (2026-03-28)

**Op=40 / Op=41 — Standard mechanic:**
```
Component 14199 op=8  "Mounted speed +10%"
Component 14200 op=15  blank BUFF_PARAM
Component 14201 op=40  vals=[0]          ← state OFF
Component 14202 op=15  blank
...
Component 14209 op=40  vals=[1]          ← state ON
...
Component 14212 op=13  "If you die with a standard in-hand, it will be destroyed!"
Component 14213 op=23  vals=[99999]      LINKED_ABILITY (sentinel = any)
Component 14214 op=41  vals=[14,187701,187702,187703]   ← same ID range as op=42 RECOVER_STANDARD
Component 14215 op=41  vals=[14,187704,187705,187706]
Component 14220 op=28  vals=[14508]      SIEGE_CARRY standard ID
```

**Op=43 — Autoattack while moving (confirmed):**
```
Component 3431 op=43 vals=[27] desc="Can autoattack while moving."  HEAD of Cleave (AbilityID=5104)
Component 9497 op=43 vals=[21] desc=""
Emulator: MoveAndShoot buff cmd (BuffEffectInvoker.cs:3145) on Skirmish Stance (9094), Spiked Squig (1844)
```

**Op=47 — Bite of the Skaven event (4-pair state, 8 rows total):**
```
Component 10733 op=47 vals=[0,10]  ← Invis-off, Code10
Component 10734 op=47 vals=[1,10]  ← On,   Code10
Component 10735 op=47 vals=[0,20]  ← Invis-off, Code20
Component 10736 op=47 vals=[1,20]  ← On,   Code20
Component 10737 op=47 vals=[0,30]  ← Invis-off, Code30
Component 10738 op=47 vals=[1,30]  ← On,   Code30
Component 10739 op=47 vals=[0,40]  ← Invis-off, Code40
Component 10740 op=47 vals=[1,40]  ← On,   Code40
AbilityBin IDs 10733–10741: "BotS - Tier 1/2 Glyph/Stone/Rod" (EffectID=3266 or 3958, op=46 CATAPULT head)
Op=47 comps are in the ability chain (not the HEAD); they set a state code when the BotS transform activates/deactivates.
```

**Op=51 — Progression ordinal / ability-linked rank (38 rows):**
```
Component 1597  op=51 vals=[17666,0,0,0]   desc=""  HEAD of "Searing Vitality" (DoK tactic, AbilityID=8205)
Component 27992 op=51 vals=[17643,0,0,13]  desc="You are a coward!"
Component 27993 op=51 vals=[17643,0,0,14]  desc="You are a coward!"
Components 26661–26668: vals=[20760–20767, 0, 0–3, 39–42]  (battlefront ordinals, AbilityID 20760="TEST Skaven Grey Seer")
Components 26900–26923: vals=[30030–30069, 0, 1–4, 49–91]  (higher battlefront ordinals)
Components 26941–26943: vals=[31018–31020, 0, 0, 89–91]    (fortress ordinals)
Value[0]=ability reference, Value[2]=sub-index (0–4), Value[3]=rank/ordinal
```

### Remaining approach

**Op=43 — DONE: name confirmed, emulator translation confirmed (MoveAndShoot).** Only remaining step is adding MOVEMENT_AUTOATTACK to the ComponentOperationType enum if desired. Schema catalog already has Inferred-confidence description.

**Op=40/41 — Names not found; source exhausted.** All available data sources (Londos DB, RE toolkit, live DB, emulator) have been searched. Both ops are confirmed in the standard-bearing component chain. op=40 = binary state toggle (CARRIER_STATE?), op=41 = standard take/place with 187701–187706 IDs. No further naming progress possible from available sources.

**Op=47 — Context identified (BotS event), name not found.** Eight rows forming a 4-pair toggle pattern in Bite of the Skaven event ability chains. Value[0]=0/1 toggle, Value[1]=10/20/30/40 tier/type code. No further naming progress possible.

**Op=51 — Pattern documented, name not found.** 38 rows: ability-reference + ordinal pattern. Associated with DoK tactic "Searing Vitality", "coward" debuff (ordinals 13/14), battlefront/fortress ordinals (39–91). Could be RANK_LINKED_ABILITY or TACTIC_TIER_REFERENCE. No further naming progress possible from available sources.

**Op=29 — Pattern documented, name not found.** 12 rows. Main cluster: Witch Hunter accusation-target IDs (66001–66300). Secondary cluster: vals=[0/9979, 40]. Emulator's `DamageWhileMoving` buff command appears on "Sudden Accusation" (8096) but also on non-WH abilities — uncertain mapping. No confirmed op name.

**Op=30 — Only 1 row; HEAD of Crack Shot (disarm).** Insufficient data points to name. Value[0]=18 unresolved.

**Op=32 — 15 rows; HEAD of Eye of Sheerian (group aura).** Insufficient data to name. Likely GROUP_STAT_CHANGE or similar. No further naming progress possible.

**All ops: If decompiled WAR server code (WarServer.dll) becomes available**, check `WarServer.Game.Ability.Ext.Components` namespace (referenced in AbilityEngine.cs) for component type implementations — this would give authoritative names for all server-only ops.

---

## ~~Area 7: Identity Domain — CareerName.EntryId~~ — RESOLVED (commit 2a4c1bd7)

`careernames_m.txt` has **120 entries (IDs 12–131) = 5 display-context groups × 24 careers**. Each career name repeats 5 times with different IDs — one per UI display context. The duplicates are structurally expected, not a CareerId collision risk.

Canonical CareerId mapping: **`careerlines_m.txt` IDs 0–24** (25 entries, unique, confirmed by `abilityexport.bin` CareerLine field). `CareerName.EntryId` is a string-context table, not an identity domain.

Implementation: Added `DuplicatesAreExpected` to `IdentityDomainRecord`; updated catalog and filter. Identity-domain risks: 1 → 0.

---

## Summary Priority Table

| Area | Status | Priority | Blocker For | Entry Point |
|---|---|---|---|---|
| ~~Requirement semantics (558 rows)~~ | **RESOLVED** | — | — | — |
| ~~DAMAGE Value[1] / FlagsRaw~~ | **RESOLVED** | — | — | — |
| ~~EffectId conflicts (2,897)~~ | **RESOLVED** | — | — | — |
| Coverage gaps (12,664 abilities) | Open | **High** | Ability reports below Mapped status | Investigate broken effect chains in Partial bucket |
| SERVER_COMMAND field mapping | Open | **High** | Full SERVER_COMMAND semantic decode | Reconcile FlagsRaw vs Values[0]; Londos shows Values[0]=command code |
| Unknown op names (8 ops) | Open | **Medium** | Named operation dispatch in emulator | Op=43 confirmed MOVEMENT_AUTOATTACK; ops 40/41/51 have strong contextual inference |
| ~~CareerName identity domain~~ | **RESOLVED** | — | — | — |
