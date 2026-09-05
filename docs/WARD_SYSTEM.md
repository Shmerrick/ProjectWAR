# Ward System (1.4.8 Target)

This document records the target behavior for ProjectWAR's restoration of the final official 1.4.8 ward system. Creature rank and creature ward are independent: a normal, champion, hero, or lord may separately have no ward or any ward tier.

## Player Ward Progression

Wards are permanent character unlocks recorded through the Tome of Knowledge. Each tier has five fragments: boots, gloves, shoulders, helm, and chest. A fragment may be earned by any client-defined task for that slot, including equipping qualifying armor or completing its alternative PvE/RvR objective. Current equipment must not be used to recalculate or revoke earned fragments.

### Fragment task encoding

Every fragment and every task that awards it is an ordinary Tome unlock in section 5. The client's `interface/interfacecore/tome/unlockmapping.csv` and the server's `tok_infos` use the same encoding, so the relationship needs no separate table:

- `Index` is the sigil tier: 1 Lesser, 2 Greater, 3 Superior, 4 Excelsior, 5 Supreme.
- `Flag` is `fragment * 10 + task`, where fragment 1-5 is boots, gloves, shoulders, helm, chest.
- Task `0` is the fragment award itself (entries 7600-7624). Tasks `1`-`6` are the alternative ways to earn it; completing any one awards the fragment.

`TokService.BuildWardTaskLookup` derives task to fragment from those columns at load, and `TokInterface.AddTok` awards the fragment whenever a task unlocks. Granting the task rather than the fragment directly is what ticks the task's radio button on the Tome sigil page.

The armour tasks are:

| Task | Lesser | Greater | Superior | Excelsior | Supreme |
|:---|:---|:---|:---|:---|:---|
| 1 | Annihilator | Conqueror | Invader | Warlord | Sovereign |
| 2 | *next tier ward* | *next tier ward* | *next tier ward* | *next tier ward* | Doomflayer |
| 3 | Bloodlord | Sentinel | Darkpromise | — | Warpforged |

Task 4 is a boss or PQ objective and task 5 an RvR objective. Both are now counter-driven — see *Counter implementation* below for which are hooked. Confirmed in game on the Greater Ward page, first fragment (2026-09-04):

| Task | Text shown by the client |
|:---|:---|
| 1 | Equip Conqueror Boots |
| 2 | Acquire First Fragment of the Superior Ward |
| 3 | Equip Sentinel Boots |
| 4 | Kill Warlock Peenk and/or Korthuk the Raging 12 Times (0/12) |
| 5 | Kill 225 RR 45+ Players (0/225) |

Tasks 1-3 are confirmed working in game on 2026-09-04: equipping ward armour awards the task and
its fragment on login, and the sigil wedges light. Two defects had to be cleared first.

`TokUnlock3` reached the server only after `Database/21_sync_ward_fragment_tasks_to_mythic_items.sql`.
`ItemService.LoadItem_Info` reads `mythic_src_item_infos` under the shipped
`UseMythicActionCoverageTables = true`, while scripts `01`, `05` and `20` had written the mapping to
`item_infos` alone — so every item carried `TokUnlock3 = 0` in memory and the equip branch never
fired, silently (BUG-033). **A migration touching item columns must update both tables.**

Separately, `AddTok` returned early on an already-held entry before reaching the fragment cascade,
so a character holding the task from before the cascade existed could never receive its fragment,
and the login backfill could not repair it because it grants through the same method. Fixed
alongside, with `BackfillWardFragments` covering tasks completed by routes that leave nothing
equipped (BUG-032).

Task 2 is the cross-tier cascade: acquiring fragment N of tier T+1 completes task 2 of fragment N
at tier T and awards that fragment, repeating down to Lesser. `TokService` maps each fragment to
the task 2 below it (20 cascades: four tiers by five fragments) and `AddTok` follows the chain.
Termination is structural — each hop drops exactly one sigil tier, so the chain is at most four
deep, and an already-held entry returns immediately. Supreme's own task 2 (7670-7674) is the
Doomflayer equip task rather than a higher ward, and is never a cascade target.

Confirmed in game 2026-09-04: three different Supreme set pieces each cascaded down, and a fragment
showed two independent sources ticked at once (`Equip Annihilator Shoulders` and `Acquire Third
Fragment of the Greater Ward`), which is the intended any-one-of behaviour.

### Counter implementation

All 32 client bindings are seeded into `ward_fragment_tasks` by
`Database/25_ward_fragment_task_counters.sql`, resolved to their `tok_infos` entry by
`Index = sigil` and `Flag = fragment * 10 + task`. Progress is stored per character in
`characters_action_counters`, keyed `(CharacterId, AcId)`.

`TokInterface.IncrementWardTaskCounter(acId)` advances a counter, pushes
`F_ACTION_COUNTER_UPDATE`, and awards the task's Tome entry at the threshold — which then awards
the fragment and cascades down the tiers exactly as an equip route does. Counters are pushed to
the client on login so the fragment pages show real progress. `.ward counters`, `.ward add` and
`.ward complete` inspect and drive them without grinding.

Counters **704** (Greater helm), **705** (Superior helm) and **709** (Superior chest) could not
bind until migration 27 repaired their comma-split task rows. All 32 counters now have nonzero
`TokEntry` bindings, verified against the Release database on 2026-09-05 by
`tools/validation/Get-WorldDataHealth.ps1`. See *Counter data repairs* below.

Hooked so far:

| Counters | Source | Status |
|---|---|---|
| 700, 702, 703, 706, 707, 715, 717, 718, 719 | Named boss kills, via `ward_task_creatures` | Working |
| 716 | Completing any Bastion Stair public quest | Working |
| 721, 726, 731 | Killing enemy players of RR 35+/45+/55+ | Working |
| 701, 720 | Seraphine / Ssyridian Morbidae; "Any Lost Vale Mini Boss" | **No target** — names unresolved |
| 708, 710, 722-725, 727-730, 732-735 | Keeps, zones, scenarios, city, fortresses, lair bosses | **Not hooked yet** |

`ward_task_creatures` maps a counter to every creature that advances it, so an "and/or" task
counts either. Six names in those tasks resolve to no `creature_protos` row — Warlock Peenk,
Necromancer Malcidious, Seraphine, Ssyridian Morbidae, Twin Lectors, and the Lost Vale mini-boss
set — and are deliberately absent rather than pointed at a guess.

Because a keep-capture counter is not yet hooked, capturing a Tier 4 keep awards no progress
toward `Capture 10 Tier 4 Empire vs Chaos Keeps` — expected, not a regression.

### Task 4-6 counter binding

`tok_infos` carries no threshold or counter reference — the "12 Times" in a task name is display
text only. The client supplies both, in `interface/interfacecore/tome/sigils/fragment_tasks.csv`:

```
fragment id, sigil entry id, task num, AcId, AcId Max
6,2,4,717,12      Greater, fragment 1, task 4 -> action counter 717, threshold 12
6,2,5,726,225     Greater, fragment 1, task 5 -> action counter 726, threshold 225
```

`AcId` is an action counter id and `AcId Max` the completion threshold, matching the `(0/12)` and
`(0/225)` the client renders for those two tasks. The server already speaks this protocol:
`TokInterface.SendActionCounterUpdate` emits `F_ACTION_COUNTER_UPDATE(subtype, count)`, used today
for bestiary kill counters. Ward AcIds occupy 700-735 and do not collide with bestiary ids.

`fragments.csv` maps fragment id to (sigil entry, fragment index) and gives each fragment's task
count, so a row resolves to a `tok_infos` entry as `Index = sigil entry` and
`Flag = fragment index * 10 + task num`.

So the server's remaining work per task is narrow: increment the counter, push the update, and
award the task entry at the threshold — the existing `AddTok` cascade then awards the fragment.
There are 32 such counters across tiers 1-3; Excelsior and Supreme define no task 4-6 rows at all.

Progress cannot be stored in `characters_toks.Count`: `HasTok` treats the presence of a row as
completion, so partial progress there would mark the task done. It cannot go in
`characters_toks_kills` unfiltered either, because `SendBestiary` writes every row of that table
into the bestiary packet behind a count prefix. That the first three tick correctly while 4 and 5 sit at 0 is independent confirmation of the `Flag = fragment * 10 + task` encoding above. `item_infos.TokUnlock3` carries the equip task entry, restored by `Database/20_restore_ward_fragment_equip_tasks.sql`, which supersedes the fragment entries written by `05_restore_invader_superior_ward_unlocks.sql`. That script also restores ten section 5 rows (7670-7674 Doomflayer, 7695-7699 Warpforged) that the world dump held as empty placeholders.

The five original tiers, from lowest to highest, are Lesser, Greater, Superior, Excelsior, and Supreme. Higher completed wards cumulatively satisfy lower ward tiers.

| Tier | Fragment unlocks | Completed sigil ability |
|:---|:---|---:|
| Lesser | 7600-7604 | 12975 |
| Greater | 7605-7609 | 12976 |
| Superior | 7610-7614 | 12977 |
| Excelsior | 7615-7619 | 12978 |
| Supreme | 7620-7624 | 12979 |

## 1.4.8 Combat Scalars

Ward scaling applies only when fighting a creature carrying the relevant ward. It does not apply to ordinary PvE, ordinary RvR, or keeps.

| Required-tier fragments owned | Incoming damage | Outgoing damage |
|---:|---:|---:|
| 0 | 300% | 40% |
| 1 | 260% | 55% |
| 2 | 220% | 70% |
| 3 | 180% | 85% |
| 4 | 140% | 100% |
| 5 | 100% | 115% |

The scalar is based on fragments satisfying the creature's ward tier. Creature rank continues to affect ordinary NPC statistics independently. The extracted sigil abilities evaluate a character's own ward counter, so ProjectWAR applies the scalar per player. A player's pet inherits its owner's ward progress. No 1.4.8 evidence found supports scanning a party and using its least-warded member.

The client omits explicit components for the neutral points (100% incoming at five fragments and 100% outgoing at four fragments); those cases use the normal unmodified damage value. Full ward retains the explicit 115% outgoing bonus.

## Historical Behavior

WAR changed wards during its lifetime. Earlier descriptions used armor-piece bonuses and, during at least one revision, removed the outgoing-damage penalty so primarily the tank needed wards. The later Tome/sigil design made fragments permanent, added alternative acquisition tasks, made tiers cumulative, and used the scalar table above. ProjectWAR targets that later 1.4.8 behavior; older reports remain useful for explaining conflicting documentation but are not the implementation target.

## Creature Ward Representation

The ward tier is stored on each concrete world, instance, boss, or public-quest spawn. Values 0 through 5 map to no ward, Lesser, Greater, Superior, Excelsior, and Supreme. Prototypes are reused at different levels, ranks, locations, and ward tiers, so `creature_protos` cannot authoritatively own this state.

Ward must not be encoded in `F_CREATE_MONSTER` offset 35. Although an old WAR-RE-Toolkit serializer labels that field `Monster.Wards`, the extracted 1.4.8 client table `monsterdifficultymask.csv` defines values `1001` through `1004` as one through four difficulty skulls. In-client testing independently confirmed that sending `1002` displays two skulls. ProjectWAR therefore serializes the prototype's unchanged `Unk2` value as the difficulty mask and keeps the spawn ward separate for combat.

The green triangle remembered from older target frames is a second UI mechanism. `targetunitframe.lua` reads `TargetInfo:UnitSigilEntryId`, then maps values 1 through 5 through `sigil_entries.csv`. Official Bilerot `F_SET_TARGET` packets contain only the six-byte target/player/type payload. No supported server packet path for that indicator exists in 1.4.8 (see below), so ProjectWAR does not falsify the difficulty mask to force an icon.

This was settled on 2026-09-03 by a full static analysis of the 1.4.8 client, recorded in the
private WAR-RE-Toolkit repository. Two earlier attempts using pattern scans produced unsupported
negatives and were discarded; the scans both desynchronised and targeted the wrong offsets.

The conclusion that matters here: the 16-bit field the target frame reads for the sigil is written
only by the client's own constructor and clear-target routine, both storing zero. Nothing in the
1.4.8 client ever sets it, so **no server packet can light the indicator on a stock client** -- and
the Return of Reckoning client is identical in this respect, so it does not drive it either. The
field addresses, the analysis method and the component that restores the display are kept in
WAR-RE-Toolkit and deliberately not reproduced here.

The remaining idea, applying the creature-side ward marker so the tier shows in the target's effect list, was then tested and **disproved**. Abilities 12958-12967 are named exactly `Lesser Ward` through `Supreme Ward` and are marked `Specline = NPC`, but they are name-only shells:

- Their `EffectID` of `3366` is not specific to them. Nineteen abilities share it, and `buff_infos` entry 3366 is `Soul Infusion`, an unrelated 15-second Blessing.
- `buff_infos`, `ability_commands` and `buff_commands` hold no rows for 12958-12967.
- The WAR-RE-Toolkit ability dumps carry the same name and description (`Grants Lesser Ward`) with no component or expression rows.

There is no buff behind those abilities to apply, and authoring one would be invention.

### Restoring the indicator

The sigil field is inert in the stock client, so no server packet alone can light the icon.
ProjectWAR sends the tier as `F_WARD_INFO` (`0xDF`), an opcode the stock client discards, and
`Creature.SendWardInfo` emits it after every `F_CREATE_MONSTER` -- tier 0 included, because Oid
reuse would otherwise leave a stale tier on an unwarded creature. Sending it is harmless to any
client, so the server half is safe to run unconditionally.

Rendering the tier additionally requires a client that reads it. That component and its
supporting analysis are maintained privately in the WAR-RE-Toolkit repository and are not part of
this repository; it is distributed to players through the launcher's patch manifest rather than
through source control. Nothing in ProjectWAR depends on it -- an ordinary client simply shows no
sigil, exactly as it does today.

Confirmed in game on 2026-09-04: Bilerot Burrow (`Ward = 1`) shows the Lesser Ward sigil on all
trash and bosses and The Lost Vale (`Ward = 2`) the Greater Ward sigil, both immediately on
target, with the Tome click-through opening the matching sigil page -- so the tier travels from
the spawn row to the icon as real per-zone data.

The client also defines these ward mechanic abilities:

| Tier | Marker ability | Applied effect ability |
|:---|---:|---:|
| Lesser | 12958 | 12959 |
| Greater | 12960 | 12961 |
| Superior | 12962 | 12963 |
| Excelsior | 12964 | 12965 |
| Supreme | 12966 | 12967 |

Abilities 12958-12967 establish the ward counters/effects, while completed sigil abilities 12975-12979 contain the final scalar table. They are client-mechanic evidence, not the creature assignment source. ProjectWAR reads the effective tier from the concrete spawn, caches player fragment totals and cumulative completions when the Tome interface loads or awards an unlock, and applies the scalar to direct, proc, periodic, auto-attack, off-hand, and raw damage. The combat path performs only fixed-size array and field lookups.

## Assignment Coverage

`Database/07_restore_known_creature_ward_tiers.sql` historically attempted prototype assignments from spawn-packet values. `Database/08_move_creature_wards_to_spawns.sql` reverses those unsafe assignments and adds explicit `Ward` columns to `creature_spawns`, `instance_creature_spawns`, `instance_boss_spawns`, and `pquest_spawns`.

New spawn columns default to no ward. Populate them only when evidence identifies the concrete encounter or location; do not infer a tier merely from rank, level, or name. `Database/18_restore_endgame_dungeon_ward_tiers.sql` assigns Lesser Ward to the Destruction city dungeons (Bloodwrought Enclave and Bilerot Burrow) and Greater Ward to The Lost Vale. `Instance_Info.WardsNeeded` remains unverified and is not used as a creature tier.

The official packet corpus remains useful for reconstructing spawn identity, position, rank, and difficulty. It is not a creature-ward assignment source because the 1.4.8 spawn field previously treated as ward is the independent difficulty mask.

## Evidence

- Extracted client: `interface/interfacecore/tome/sigils/{sigil_entries,fragments,fragment_tasks}.csv`
- Extracted client: `interface/interfacecore/tome/unlockmapping.csv` and localized sigil caches
- Extracted client: `data/bin/abilityexport.bin`, `abilitycomponentexport.bin`, and localized ability names for abilities 12958-12979
- Extracted client: `data/gamedata/monsterdifficultymask.csv` and `interface/interfacecore/source/targetunitframe.lua`
- WAR-RE-Toolkit: IDA/Ghidra client analysis, packet models, and official Bilerot packet logs
- Steelbrand, *Definitive Guide to Armor Sets in Warhammer Online* (2010), accepted as the 1.4.8 scalar reference

## Counter data repairs

Three task rows were unusable until 2026-09-05. `tok_infos` 7708, 7713 and 7714 each carry a comma
inside their name and the CSV import behind the world dump split on it, shifting every following
column: the broken `Index` held the real `Section` and the broken `Flag` the real `Index`, with the
true `Flag` lost. They are exactly the three counters that could not bind — AcIds 704, 705 and 709
— so they were never missing rows, only corrupted ones, and a player could drive such a counter to
its threshold and watch the task stay unticked. `Database/27_fix_comma_split_ward_tasks.sql`
restores them; all 32 counters now bind.
