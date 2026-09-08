# Land of the Dead — glyphs, PQs and tomb entry

Findings from 2026-09-07 against the Release database. Companion to `docs/LAND_OF_THE_DEAD.md`,
which covers the expedition race and travel and says nothing about glyphs.

**Summary: glyph acquisition is real and is recorded. Every glyph now has a source (migration 78),
but almost none can be earned, because 42 of the 46 Land of the Dead public quests have no
creatures. Tomb entry now checks, spends and resets glyphs — but charges nothing until the
cost table is populated.**

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
| 7961 | Vulture (Dest) | Aerie of Death, The Carrion Nest | 0 |
| 7962 | Scroll (Dest) | Obelisk of Judgement, The Quarry of Bone | 52 |
| 7963 | Horse (Dest) | The Assault of Nekh Akhet | 0 |
| 7964 | Ankhra (Dest) | Forbidden Vaults, Tombs of the Bitter Wind | 0 |
| 7965 | Scarab (Dest) | Pit of Asaph, Pit of Kem Senef | 0 |
| 7966 | Vase (Dest) | Hall of the Heavens, The Library of Zandri | 0 |
| 7967 | Riverbarge (Dest) | The Quay of Seftu | 0 |
| 7968 | Scorpion (Dest) | Ricci's Raiders | 0 |
| 7969 | Skull (Dest) | Temple of Ualatp | 0 |
| 7970 | Reed (Order) | Nikosi Temple, Sedjhet Temple | 1 |
| 7971 | Vulture (Order) | Aerie of Death, The Carrion Nest | 0 |
| 7972 | Scroll (Order) | Obelisk of Judgement, The Quarry of Bone | 66 |
| 7973 | Horse (Order) | Amsu's Charge, The Assault of Nekh Akhet | 0 |
| 7974 | Ankhra (Order) | Forbidden Vaults, Tombs of the Bitter Wind | 0 |
| 7975 | Scarab (Order) | Pit of Asaph, Pit of Kem Senef | 0 |
| 7976 | Vase (Order) | Hall of the Heavens, The Library of Zandri | 0 |
| 7977 | Riverbarge (Order) | The Quay of Seftu | 0 |
| 7978 | Scorpion (Order) | Ricci's Raiders | 0 |
| 7979 | Skull (Order) | Temple of Ualatp | 0 |

Two failures, not one:

1. **No creatures.** Only 4 of the 46 PQs in zone 191 have any spawn rows — Sedjhet Temple (556, 9),
   Obelisk of Judgement (558, 66) and their realm duplicates (886, 1; 887, 52). The other 42,
   including all three roaming PQs, have objectives defined and nothing to kill. Destruction can
   therefore earn only Reed and Scroll; Order only Reed and Scroll.
2. **Six Order glyphs had no source at all — FIXED by migration 78.** PQ rows 886-899 are the Order
   duplicates of 550-563,
   and twelve of them carried **zero objectives**, so their `TokCompleted` bindings did not exist.
   Vulture, Ankhra, Scarab, Vase, Riverbarge and Skull were unreachable for Order regardless of
   spawns. Migration 78 copies each missing objective set from its Destruction twin and shifts
   `TokCompleted` into the Order block, exactly as whoever built 886 and 887 did — 41 rows across
   twelve PQs. All 20 glyphs now have at least one awarding PQ. The spawn problem above is
   untouched by it.

### This is missing content, not deletion

Unlike the gameobject loss (`docs/GAMEOBJECT_DATA_LOSS.md`), nothing was removed here. The
pre-deletion dump at `a4995e92` holds **134** `pquest_spawns` rows for zone 191; the live database
holds **128**. The overall drop from 29,437 to 27,611 is accounted for by the ~1,842 rows moved to
`pquest_spawns_unresolved` by the earlier archival commit. The Land of the Dead PQs were never
populated in this database, so there is no dump to restore them from — placements have to come from
the client's zone data or from captures.

## Tomb entry

`zone_jumps` is `Entry, ZoneId, WorldX, WorldY, WorldZ, WorldO, Enabled, Type, InstanceID`, and
`ZoneId` is the **destination**, not where the portal stands. The five tomb entrances are therefore
the rows whose `ZoneId` is the tomb, all `Type = 6` (instance entry, routed through
`InstanceMgr.ZoneIn`):

| Tomb | Zone | Jump id |
| --- | --- | --- |
| Tomb of the Stars | 241 | 200713256 |
| Tomb of the Moon | 242 | 200713576 |
| Tomb of the Sky | 243 | 200713320 |
| Tomb of the Sun | 244 | 200713512 |
| Tomb of the Vulture Lord | 179 | 200797160 |

(The rows with `ZoneId = 191` and matching `InstanceID` are the exits, going the other way.)

### The gate

`MovementHandlers.F_ZONEJUMP` gates the tombs the same way it already gates Type 1, 2 and 3 —
group, rank 30, guild rank 6 — with `LotdGlyphService`:

1. **Check** before the instance is entered, so a refusal costs nothing. The player is told which
   glyphs they lack by name.
2. **Spend** only after `InstanceMgr.ZoneIn` has actually accepted them. A lockout or a full
   instance would otherwise swallow the glyphs for a trip that never happened, and there is no way
   to give them back.
3. **Reset** clears all ten of the player's realm, not only the ones the tomb charged — reading
   "the players glyph progress would be reset" literally. Narrowing it to the charged glyphs is a
   two-line change in `ConsumeGlyphs`.

Removing a Tome entry is new. `TokInterface` was append-only — `Save` only ever calls `SaveObject`,
so dropping an entry from memory alone would let it reload at the next login. `TokInterface.RemoveTok`
deletes the `characters_toks` row as well and tells the client. **The client packet is unverified:**
no capture shows a Tome entry being revoked, so sending `F_TOK_ENTRY_UPDATE` with a zero count is
inference from the shape of `SendTok`. Server state is correct regardless, so a relog will show the
truth even if the live update is ignored.

### The costs are not populated, and the gate is inert until they are

`lotd_tomb_glyph_costs` (migration 79) is created empty. **A tomb with no rows is not gated at all**
and behaves exactly as it did before. Which glyphs each tomb charges is not in `zone_jumps`, not in
any capture in the corpus, and has not been found in the client — locking five instances on a guess
would be worse than leaving them open.

Glyphs are stored by index, not by Tome entry, because each exists twice (7960-7969 Destruction,
7970-7979 Order, same ten in the same order); the index is resolved to the player's realm at
runtime, so one row covers both realms:

| 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Reed | Vulture | Scroll | Horse | Ankhra | Scarab | Vase | Riverbarge | Scorpion | Skull |

```sql
-- e.g. the Tomb of the Sky costing Vulture and Horse
INSERT INTO lotd_tomb_glyph_costs (TombZoneId, GlyphIndex, Count) VALUES (243, 1, 1), (243, 3, 1);
```

Costs are cached at boot, so a restart is needed after changing them.

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
