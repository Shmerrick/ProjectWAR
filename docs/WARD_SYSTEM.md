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

Task 4 is a boss or PQ objective and task 5 an RvR objective; those remain unimplemented. The client renders both, so their exact wording is known. Confirmed in game on the Greater Ward page, first fragment (2026-09-04):

| Task | Text shown by the client |
|:---|:---|
| 1 | Equip Conqueror Boots |
| 2 | Acquire First Fragment of the Superior Ward |
| 3 | Equip Sentinel Boots |
| 4 | Kill Warlock Peenk and/or Korthuk the Raging 12 Times (0/12) |
| 5 | Kill 225 RR 45+ Players (0/225) |

Tasks 1-3 work. Task 4 needs a named-boss kill counter and task 5 a renown-ranked player kill counter, both persisted per character and per fragment. That the first three tick correctly while 4 and 5 sit at 0 is independent confirmation of the `Flag = fragment * 10 + task` encoding above. `item_infos.TokUnlock3` carries the equip task entry, restored by `Database/20_restore_ward_fragment_equip_tasks.sql`, which supersedes the fragment entries written by `05_restore_invader_superior_ward_unlocks.sql`. That script also restores ten section 5 rows (7670-7674 Doomflayer, 7695-7699 Warpforged) that the world dump held as empty placeholders.

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
