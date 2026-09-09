# Database fidelity: measured gaps against the client, and the order to close them

Goal: make `war_world` the most faithful reconstruction of live 1.4.8 that the evidence supports.

This is a measurement, not a proposal. Every number below was counted on 2026-09-08 against the
local Release database, the extracted client at `C:\Users\Admin\Downloads\myps`, the 1,027 packet
captures, and the Londo dump. Regenerate any of them with `ClientDataMatrix crosswalk items` or the
queries named in each section.

## The rule that governs all of it

`item_infos` and the ability tables are an **amalgamation of several contributors' private
databases** — WarEmu's public base, a released Return of Reckoning database, Londo's
Mythic-connected data, others (see `CROSS_REPO.md`). There is no single upstream authority for a
row. So a gap is only closed when a source says what belongs there, in this order:

| Tier | Source | Standing |
|---|---|---|
| 1 | The 1.4.8 client — `data/bin/*.bin`, `data/gamedata/*.csv`, `data/strings/` | **Arbiter.** Overwrite us. |
| 2 | The 1,027 live packet captures | **Arbiter** for anything the client does not hold (item names, stats sent on the wire). |
| 3 | Londo / other emulator dumps | **Corroboration.** Finds candidates; never settles one alone. |
| 4 | Our own database | The thing being corrected. |

**Art existing is not evidence that an item existed.** WAR shipped art for cut content, per-career
armour variants that were never itemised, and dev scratch. Creating 2,051 items because 2,051
models exist would manufacture exactly the fiction this project is trying to remove. Every addition
below is gated on a tier 1–3 source naming the thing.

---

## Gap 1 — items that no longer point at art, and art no item points at

Counted with `ClientDataMatrix crosswalk items` and the `tmp_obj` join.

| | Count |
|---|---:|
| `objects.csv` art rows | 9,905 |
| Art referenced by at least one item | 4,956 |
| **Art with no item at all** | **4,949** |
| — of those, `WORLD OBJ` scenery (never items) | 1,734 |
| — blank art names | 319 |
| — `NPC_*` (NPC-only art) | 514 |
| — `Test*` / `CCTest*` / `placeholder` / `not-used` | 332 |
| **— plausible item art with no item** | **2,051** (2,012 with a resolvable icon) |

Of those 2,051, **199 are backed by a Londo row carrying a name, type, slot and the matching
ModelID**, and neither that id nor that name exists anywhere in our table. Sampled, they are real
content: `Silver Battlepak` and `Cream Battle Boar` (rank-40 mounts), `Earthy Two-Headed Hound`,
`Dark Steel Iron Hawk`, `Black Jester`, `Charcoal Tzeentch Familiar` (vanity pets),
`Conqueror's Insignia` and `Doomflayer Insignia` (scenario currency), `Case of Captain's Elixir of
Rejuvenation` (potions). A handful are obvious dev rows (`NPC_SK_Catcher_01**TEST**`, `Dagger 1`)
and must be excluded by hand, not by pattern.

The remaining ~1,850 have art and nothing else. **Leave them.** Without a name from a capture or a
dump there is nothing to add but invention.

**Verdict: actionable now, bounded, tier 3 evidence.** Cross-check each of the 199 against the
captures before writing, and write only those that survive.

## Gap 2 — items the crosswalk already flags

From `docs/data-matrix/crosswalk/item-crosswalk.md`, 88,727 items examined:

| Severity | Kind | Rows |
|---|---|---:|
| Violation | ModelId not in `objects.csv` | 1 |
| Hole | art has no icon | 436 |
| Hole | ModelId missing (0) | 50 |
| Suspect | placeholder name (`unk1`–`unk32`) | 41 |
| Suspect | name begins lowercase | 26 |

99.5% of items resolve to an icon, so item *art* is in good shape. The 41 placeholders are the
interesting ones: `unk25` was resolved to `Talisman of the Prodigal Fighter` from a capture in
migration 86, so the rest are likely recoverable the same way.

**Verdict: actionable now, tier 2 evidence, small.**

## Gap 3 — ability damage, and abilities generally

This is the big one and the reason for the caution above.

| | Count |
|---|---:|
| Server abilities (`mythic_src_abilities`) | 8,416 |
| Client abilities (`mythic_bin_ability`) | 29,006 |
| Client ability names (`abilitynames.txt`) | 29,001 |
| Our damage/heal rows | 1,434 |
| Our abilities **with** a damage row | 1,151 |
| Our abilities with **no** damage row | 7,265 |
| Client component rows carrying `Values` | 18,524 |
| `mythic_bin_abilitycomponentlink` rows | **0** |

The client holds the real numbers. `mythic_bin_abilitycomponentbin.Values` is eight comma-separated
integers per component (`10,0,0,0,0,0,0,0`) with a parallel `Multipliers` row, and those are exactly
the values the client's own tooltips render through `{COM_2_VAL0_SPIRITDAMAGE}` and friends in
`abilitydesc.txt`. Our `mythic_src_ability_damage_heals` carries `MinDamage`, `MaxDamage`,
`DamageVariance`, `StatDamageScale` and so on — a *different shape*, populated by persons unknown.

Two things stand between here and a comparison:

1. **The link table is empty.** `mythic_bin_abilitycomponentlink` has 0 rows, and Londo's
   `AbilityComponentXComponent` is empty too. Components attach through data embedded in the ability
   row — ClientDataMatrix already parses this (the GUI reports 18,526 BIN components loaded against
   11,736 BIN abilities), so the link exists in code but has never been written back to a table.
2. **Operation semantics are only partly decoded.** Which `Val` slot is damage depends on the
   component's operation, and that is the unfinished `Operation Schemas` / `Unknown Triage`
   workstream in the matrix.

**Verdict: tractable but real work, and the highest-value gap.** Sequence: export the
ability→component links the matrix already resolves into a real table; map operation → which Val is
damage, for the handful of operations that cover most damaging abilities; then compare against
`mythic_src_ability_damage_heals` and produce a crosswalk exactly like the item one — report first,
migrate second.

Do **not** bulk-overwrite damage from a partially decoded schema. A wrong `Val` slot silently
rebalances the whole game, and unlike a wrong item name nobody will see it in a tooltip.

## Gap 4 — the 20,590 client abilities with no server row

29,006 client abilities against 8,416 of ours. Most of the difference is not missing content:
the client's ability space includes monster abilities, NPC-only actions, per-rank variants and
unused rows. Establishing which of the 20,590 *should* exist server-side is its own investigation
and is not a data-entry job.

**Verdict: investigate before acting. Not a hole to fill blindly.**

## Order of work

1. **Gap 2's 41 placeholder names** — smallest, tier 2, proves the capture→migration loop again.
2. **Gap 1's 199 items** — bounded, each one checked against captures before writing.
3. **Gap 3 step one**: write the ability→component links the matrix already resolves into a table,
   so the data stops living only inside a GUI session.
4. **Gap 3 step two**: an ability crosswalk report, same three-way split as the item one
   (violation / hole / suspect), covering damage, cast time, cooldown and range.
5. Only then, migrations against what that report shows.

Steps 1 and 2 are days of careful checking. Step 3 onward is the real restoration, and it is where
"our damage has been changed from how the game is supposed to be" actually gets answered — with a
report that names every ability whose numbers disagree with the client, before anything is written.
