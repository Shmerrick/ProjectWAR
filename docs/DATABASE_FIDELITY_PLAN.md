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
| 3 | **Londo's dump** — the best-developed contributed layer, per the repository owner, from his connection with the Mythic developers | Strong. Settles a question when no capture covers it; still yields to one that does. See `docs/LONDO_DATA_AUDIT.md`. |
| 3b | WarEmu / Return of Reckoning layers | Weaker corroboration. |
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

### The COM token is a complete address, and that is the whole unlock

An earlier revision of this document claimed two blockers — an empty link table, and undecoded
operation semantics needed to work out which `Val` slot holds damage. **Both were wrong**, and the
correction matters enough to record how it was established.

A token is `[ABIL_<id>_]COM_<componentIndex>_VAL<slot>_<meaning>`. It names the component **by its
index in that ability's own ordered component list**, the value **by slot**, and the meaning in the
suffix (`DAMAGE`, `TOD_DAMAGE`, `SPIRITDAMAGE`, `DURA_SECONDS`). No operation table is required to
find the damage: the ability's own description string says where it is. The optional `ABIL_<id>`
prefix addresses *another* ability's components — `abilitydesc.txt` row 9 carries
`{ABIL_3881_COM_0_VAL0_TOD_DAMAGE}`.

Worked, end to end, on ability 7 "Spine Fling" — *"dealing `{COM_1_VAL0_DAMAGE}` every second for
`{COM_0_DURA_SECONDS}`"*:

| | |
|---|---|
| Ordered components (`abilityexport.bin`, `Components` column) | `3301, 2` |
| `COM_0` → component 3301 | Duration 3000, Interval 1000 → **"every second for 3 seconds"** |
| `COM_1` → component 2 | `Values` = `15, 0, 0, 0, 0, 0, 0, 0` → Val0 = **15** |
| `mythic_src_ability_damage_heals` MinDamage | **15** |

**The trap that makes this look broken.** The ability report's `- Related component IDs:` line is a
**sorted** set, not the ability's order. For ability 7 it prints `2, 3301`, which makes `COM_0` look
like component 2 and yields the wrong damage for every token above index 0. The ordered list is the
`Components` column of the `abilityexport.bin` row — `3301, 2`. Ability 1 happens to agree under
both readings, so validating on one ability proves nothing; validate on one whose ordered and sorted
lists differ.

**Verdict: buildable now, and it is the highest-value gap.** Sequence: parse the COM tokens out of
`abilitydesc.txt`, resolve each against the ability's *ordered* component list, and produce an
ability crosswalk in the same violation/hole/suspect shape as the item one — every ability whose
damage, duration, interval or radius disagrees with the client, named, with both numbers side by
side. Report first, migrate second.

Do **not** bulk-overwrite from the report. A wrong `Val` slot silently rebalances the whole game,
and unlike a wrong item name nobody will see it in a tooltip.

## Gap 4 — the 20,590 client abilities with no server row

29,006 client abilities against 8,416 of ours. Most of the difference is not missing content:
the client's ability space includes monster abilities, NPC-only actions, per-rank variants and
unused rows. Establishing which of the 20,590 *should* exist server-side is its own investigation
and is not a data-entry job.

**Verdict: investigate before acting. Not a hole to fill blindly.**

## Order of work

1. **Gap 2's 41 placeholder names** — smallest, tier 2, proves the capture→migration loop again.
2. **Gap 1's 199 items** — bounded, each one checked against captures before writing.
3. **Gap 3**: parse the COM tokens and build the ability crosswalk. No preliminary decode is needed —
   the ordered component list is already parsed into the `Components` column of the ability row.
4. Widen it beyond damage: cast time, cooldown, range, duration, interval and radius are all
   addressable by the same tokens and the same component rows.
5. Only then, migrations against what that report shows.

Steps 1 and 2 are days of careful checking. Step 3 onward is the real restoration, and it is where
"our damage has been changed from how the game is supposed to be" actually gets answered — with a
report that names every ability whose numbers disagree with the client, before anything is written.

---

## Correction: matching `MinDamage` is not the same as matching the tooltip

Added 2026-09-08 after the ability crosswalk landed, because the crosswalk answers a narrower
question than it appears to.

**The tooltip is a contract with the player.** It is rendered from client files and says what it
says regardless of the server. If it reads 500 and the hit lands for 400, the server is wrong by
definition — there is no "our value is also defensible" here.

`MinDamage` is not the tooltip number. It is a base the server scales
(`WorldServer/World/Abilities/Components/AbilityDamageInfo.cs`):

```
MaxDamage == 0:  damage = ((level - 1) * LevelScalingFactor * MinDamage) + MinDamage
MaxDamage  > 0:  damage = MinDamage + (MaxDamage - MinDamage) * ((level - 1) / 39)
```

### The Min/Max question, answered

The second branch is effectively dead. `MaxDamage` is NULL on **1,433 of 1,434 rows** and the one
exception equals `MinDamage`; `DamageVariance` is NULL on 1,401, so its random spread is a no-op as
well. The data is single-valued, which is the same shape the client uses — one value plus a
multiplier. **The Min/Max columns are vestigial, not actively wrong.** Whatever was done about them
previously, the data does not carry a min/max range today.

### What is actually wrong

At rank 40 the live branch gives `7.5 × MinDamage`, so everything rests on `LevelScalingFactor`,
which defaults to `0.16667f`.

That constant is not invented. `mythic_bin_abilityupgradeentry` packs a 32-bit float across `V1`
(low half) and `V2` (high half): `43713/15914` decodes to exactly **0.166667**, `0/16256` to 1.0,
`0/16384` to 2.0. But it covers **25 of that table's 2,760 rows**. The table also holds:

| Scalar | Rows |
|---:|---:|
| 1.0 | 60 |
| 0.166667 | 25 |
| 0.8147 | 16 |
| 0.7346 | 16 |
| 0.6544 | 16 |
| 0.5742 | 16 |
| 0.5344 | 16 |
| 0.4941 | 12 |
| 0.2052 | 12 |

The client scales each ability by its own factor. The emulator falls back to one of them for
everything it cannot resolve, and that is the most plausible mechanism behind a tooltip and a hit
disagreeing. **It is invisible to `crosswalk abilities`**, which compares the unscaled base: an
ability can agree there and still land for the wrong amount in play.

### How the scalars are applied today

The decode itself is already correct — `AbilityMgr.TryDecodeUpgradeRowScalar` does exactly the
`(V2 << 16) | V1` float unpack described above. The problem is everything after it, and the
server's own startup log states the outcome:

```
upgrades=70  ability_entries=211  applied_rows=121  applied_entries=86  unresolved_rows=3940
```

**121 damage rows get a per-ability scalar; 3,940 keep the hardcoded default.** About 3%.

Three causes worth chasing, in order:

1. **Only 70 of 138 upgrade bins yield a scalar at all.** `TryExtractUpgradeLevelScalar` discards
   anything outside `(0, 2]`, and the same `V1`/`V2` pair legitimately decodes to 5000, 3000, 1000
   and 2000 in other rows. The column means different things depending on `Index`, and that is
   undecoded — so the filter is throwing away rows it cannot interpret rather than rows that are
   wrong.
2. **A qualifying bin's 20 per-`Index` entries collapse into one number** — whichever fractional
   candidate is encountered first. That is order-dependent, and 20 entries per bin looks like
   per-level or per-tier data that should not collapse to a single scalar at all.
3. **Bins map to abilities by `EffectID`, then `Entry`** (`BuildAbilityLevelScalars`). That key
   choice is unverified.

Decode the `Index` semantics first; causes 1 and 2 both dissolve if the 20 entries turn out to be a
per-level curve. Tracked as BUG-151.

---

## Progress

**Gap 2 (placeholder names) — closed as not actionable.** The 41 `unk*` placeholders and 26
lowercase-starting names were cross-checked against both remaining sources and neither can resolve
them: no capture covers any of them (`unk25` was the only one that ever did, fixed in migration 86),
and Londo carries no differing name for any. They stay as they are, correctly — there is nothing to
write but invention.

**Gap 1 (missing items) — 95 of 199 written, migration 87.** Of the 199 Londo-named candidates, 99
are also present in the live packet captures; on those the two sources agree on 99 of 99 ModelIds
and 97 of 99 names. 95 were inserted after dropping two dev rows (`NPC_SK_Catcher_01**TEST**`,
`Dagger 1`) and two duplicate ids that the temporary table's primary key caught. Both item tables
went 88,727 → 88,822, all 95 resolve to an icon, and the crosswalk's finding count is unchanged at
554 — nothing new was broken.

One mapping was recovered on the way: **Londo's `DPS` column is the combined wire field**, not DPS.
Armour rows carry a value there with Speed 0, which is the shape our own `Item.BuildItem` writes as
`info.Dps > 0 ? info.Dps : info.Armor`. Splitting on `Speed > 0` is what stopped a chest piece being
imported with 1188 DPS.

The remaining 104 have Londo only. Left out deliberately.
