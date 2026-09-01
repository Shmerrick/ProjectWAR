# Ward System (1.4.8 Target)

This document records the target behavior for ProjectWAR's restoration of the final official 1.4.8 ward system. Creature rank and creature ward are independent: a normal, champion, hero, or lord may separately have no ward or any ward tier.

## Player Ward Progression

Wards are permanent character unlocks recorded through the Tome of Knowledge. Each tier has five fragments: boots, gloves, shoulders, helm, and chest. A fragment may be earned by any client-defined task for that slot, including equipping qualifying armor or completing its alternative PvE/RvR objective. Current equipment must not be used to recalculate or revoke earned fragments.

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

`F_CREATE_MONSTER` carries the creature instance's effective ward at packet offset 35. WAR-RE-Toolkit's server serializer names this field `Monster.Wards`. ProjectWAR previously copied the complete field from `creature_protos.Unk2`, but prototypes are reused at different levels, ranks, locations, and ward tiers. A prototype is therefore not an authoritative ward assignment.

Official capture families use bare values, `0xE8`-based values, and `1000`-based values. The ward tier is the low three bits:

```text
ward tier = F_CREATE_MONSTER offset 35 & 0x7
```

The prototype supplies only the upper packet flags. The concrete world, instance, boss, or public-quest spawn supplies the low ward bits. Values 0 through 5 map to no ward, Lesser, Greater, Superior, Excelsior, and Supreme. Values 6 and 7 are invalid/reserved and are normalized to no ward. Creature rank is encoded and processed separately.

Across 748,044 official spawn packets that matched exactly one ProjectWAR prototype by normalized name, model, and level, the low bits agreed with the historical prototype snapshot in 99.28% of packets. This validates the wire encoding, not prototype-level assignment: the match omitted the packet's location and cannot distinguish copies of the same prototype used with different wards.

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

The current curated world dump contains 13,754 creature prototypes, but their low `Unk2` bits are historical packet snapshots rather than safe gameplay assignments. `Database/07_restore_known_creature_ward_tiers.sql` attempted to restore 79 additional prototype values from exact name/model/level capture matches. `Database/08_move_creature_wards_to_spawns.sql` reverses those changes and adds explicit `Ward` columns to `creature_spawns`, `instance_creature_spawns`, `instance_boss_spawns`, and `pquest_spawns`.

New spawn columns default to no ward. Existing prototype low bits are ignored by runtime combat and are replaced during packet serialization, preventing a reused prototype from leaking a ward into another location. Populate a spawn ward only when evidence identifies the concrete location or spawn; do not infer it merely from rank, level, name, or general dungeon membership. `Instance_Info.WardsNeeded` remains unverified and is not used as a creature tier.

The 926,226-packet corpus remains useful for assignment work, but it must be reprocessed with zone and coordinate context. Generic identities, model/level-only matches, and observations that cannot be tied to one concrete spawn remain excluded.

## Evidence

- Extracted client: `interface/interfacecore/tome/sigils/{sigil_entries,fragments,fragment_tasks}.csv`
- Extracted client: `interface/interfacecore/tome/unlockmapping.csv` and localized sigil caches
- Extracted client: `data/bin/abilityexport.bin`, `abilitycomponentexport.bin`, and localized ability names for abilities 12958-12979
- WAR-RE-Toolkit: server packet serializer naming offset 35 `Monster.Wards`, IDA-reversed packet models, and 1,027 official packet logs (926,226 decoded creature spawns)
- Steelbrand, *Definitive Guide to Armor Sets in Warhammer Online* (2010), accepted as the 1.4.8 scalar reference
