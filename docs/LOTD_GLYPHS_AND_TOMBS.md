# Land of the Dead — glyphs, PQs and tomb entry

Findings from 2026-09-07 against the Release database. Companion to `docs/LAND_OF_THE_DEAD.md`,
which covers the expedition race and travel and says nothing about glyphs.

**Summary: glyph acquisition is real and is recorded. Almost nothing can award a glyph, because 42
of the 46 Land of the Dead public quests have no creatures. Tomb entry checks nothing, costs
nothing and resets nothing.**

## Glyphs are Tome of Knowledge unlocks, not items

The ten glyphs are `tok_infos` entries, duplicated per realm — Destruction 7960-7969 (section 20,
index 655), Order 7970-7979 (section 20, index 657):

Reed, Vulture, Scroll, Horse, Ankhra, Scarab, Vase, Riverbarge, Scorpion, Skull.

Each carries `EventName` "You have found the *X* Glyph". They are awarded through
`pquest_objectives.TokCompleted` on a PQ's final stage.

There are also 95 `Glyph of <god>: <name>` **items** (type 35, entries 2000521+) and four
`Glyph of Souls / Life / Warriors / Damnation` ToK entries (10451-10454, section 30). Those are
separate things and are not what the PQs award — do not conflate them.

**So yes, the server records glyph acquisition.** It is an ordinary ToK unlock, persisted with the
character's tome like any other. Nothing extra is needed for that part.

## What can actually be earned

Joining each glyph to the PQs that award it, and each of those PQs to its creature spawns
(`pquest_spawns` → `pquest_objectives.Guid`, **not** `pquest_spawns.Entry`, which is a creature id):

| ToK | Glyph | Awarded by | Spawn rows |
| --- | --- | --- | --- |
| 7960 | Reed (Dest) | Nikosi Temple, Sedjhet Temple | 9 |
| 7961 | Vulture (Dest) | Aerie of Death, The Carrion Nest | **0** |
| 7962 | Scroll (Dest) | Obelisk of Judgement, The Quarry of Bone | 52 |
| 7963 | Horse (Dest) | The Assault of Nekh Akhet | **0** |
| 7964 | Ankhra (Dest) | Forbidden Vaults, Tombs of the Bitter Wind | **0** |
| 7965 | Scarab (Dest) | Pit of Asaph, Pit of Kem Senef | **0** |
| 7966 | Vase (Dest) | Hall of the Heavens, The Library of Zandri | **0** |
| 7967 | Riverbarge (Dest) | The Quay of Seftu | **0** |
| 7968 | Scorpion (Dest) | Ricci's Raiders | **0** |
| 7969 | Skull (Dest) | Temple of Ualatp | **0** |
| 7970 | Reed (Order) | Sedjhet Temple | 1 |
| 7971 | Vulture (Order) | **nothing** | — |
| 7972 | Scroll (Order) | Obelisk of Judgement | 66 |
| 7973 | Horse (Order) | Amsu's Charge, The Assault of Nekh Akhet | **0** |
| 7974 | Ankhra (Order) | **nothing** | — |
| 7975 | Scarab (Order) | **nothing** | — |
| 7976 | Vase (Order) | **nothing** | — |
| 7977 | Riverbarge (Order) | **nothing** | — |
| 7978 | Scorpion (Order) | Ricci's Raiders | **0** |
| 7979 | Skull (Order) | **nothing** | — |

Two failures, not one:

1. **No creatures.** Only 4 of the 46 PQs in zone 191 have any spawn rows — Sedjhet Temple (556, 9),
   Obelisk of Judgement (558, 66) and their realm duplicates (886, 1; 887, 52). The other 42,
   including all three roaming PQs, have objectives defined and nothing to kill. Destruction can
   therefore earn only Reed and Scroll; Order only Reed and Scroll.
2. **Six Order glyphs have no source at all.** PQ rows 886-899 are the Order duplicates of 550-563,
   and eleven of them carry **zero objectives**, so their `TokCompleted` bindings do not exist.
   Vulture, Ankhra, Scarab, Vase, Riverbarge and Skull are unreachable for Order regardless of
   spawns.

### This is missing content, not deletion

Unlike the gameobject loss (`docs/GAMEOBJECT_DATA_LOSS.md`), nothing was removed here. The
pre-deletion dump at `a4995e92` holds **134** `pquest_spawns` rows for zone 191; the live database
holds **128**. The overall drop from 29,437 to 27,611 is accounted for by the ~1,842 rows moved to
`pquest_spawns_unresolved` by the earlier archival commit. The Land of the Dead PQs were never
populated in this database, so there is no dump to restore them from — placements have to come from
the client's zone data or from captures.

## Tomb entry is completely ungated

The four lair tombs and the Vulture Lord dungeon are reached by plain `zone_jumps` rows out of
zone 191:

| Target | Zone | InstanceID |
| --- | --- | --- |
| Tomb of the Stars | 241 | 252708392 |
| Tomb of the Moon | 242 | 253757096 |
| Tomb of the Sky | 243 | 254805608 |
| Tomb of the Sun | 244 | 255854056 |
| Tomb of the Vulture Lord | 179 | 187704296 / 187704360 |

`zone_jumps` is `Entry, ZoneId, WorldX, WorldY, WorldZ, WorldO, Enabled, Type, InstanceID`. There is
no requirement column, and no code reads one. Anyone standing at the entrance goes in.

### What live did, per the report

> The player would complete PQs, and then on the PQs completion, they would be able to go into a
> tomb. The entrance of the tomb would cost the glyphs that the player had earned from the PQs and
> the players glyph progress would be reset.

None of that exists: no gate on entry, no cost, no reset. Implementing it needs three things the
server does not have — a per-tomb glyph requirement, a check on the zone-jump path, and a way to
clear ToK unlocks 7960-7979 for a character (the tome is otherwise append-only). Which glyphs each
tomb costs is **not established here**; it is not in `zone_jumps` and has not been searched for in
the client or the captures.

Note that the gating work is independent of the spawn work and could be done first — but with only
two glyphs earnable per realm, any real requirement would lock every tomb.

## Answering "would importing all the CSVs improve ability mapping?"

No — and for `abilities.csv` specifically it would make things worse. That file is **already**
imported as `mythic_csv_abilities`, and joining on its ID column is exactly what corrupted
`mythic_src_abilities`: see `docs/ABILITY_TABLE_ALIGNMENT.md` and migrations 76 and 77. Its ID
column is an art-authoring row key that agrees with the client's real ability ids on **13 of
3,115**.

Re-keying it by name does not rescue it either. Of its 4,200 named rows:

- 1,489 map to exactly one client ability
- 1,230 are ambiguous (the name occurs on several)
- 1,481 match no client ability at all

The `data/gamedata` set is 103 files and is mostly art and animation metadata — `anim_*` (41,484
lines in `anim_db.csv` alone), `effect*`, `objects.csv`. `effects.csv` shares `abilities.csv`'s
authoring key space. None of it carries the gameplay ability graph.

**Per-file verification is the rule, not a blanket import.** Some CSVs *are* keyed on real ids —
`itemdata.csv` maps real item entries to icon, type and slot, and spot-checks resolve correctly
against `item_infos`. So the answer is not "CSVs are useless", it is "prove the key space against
`data/strings/english/abilitynames.txt` or the equivalent before joining anything".

What would actually improve ability mapping is already in the database and barely used: the
`mythic_bin_*` family imported from the client's own binary records — `mythic_bin_ability` (29,006
rows, 99.5% agreement with the client's string table) plus `mythic_bin_abilitycomponentbin`,
`mythic_bin_abilitycomponentlink`, `mythic_bin_abilityexpression` and
`mythic_bin_abilityrequirmentbin`. **20,590 client abilities have no server row, and every one of
them carries component data.** That is the gap, and it is a translation job from the binary tables,
not an import job from the CSVs.
