# Land of the Dead — glyphs, PQs and tomb entry

Findings from 2026-09-07 against the Release database. Companion to `docs/LAND_OF_THE_DEAD.md`,
which covers the expedition race and travel and says nothing about glyphs.

**Summary: glyph acquisition is real and is recorded. Every glyph now has a source (migration 78),
but almost none can be earned, because 42 of the 46 Land of the Dead public quests have no
creatures. Tomb entry now requires and spends the right glyphs, taken from the client zone map.**

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
3. **Spend only the door's own glyphs.** A tomb takes the glyphs it charges and leaves the rest
   of the player's progress alone, so a group can bank keys for several lairs at once.

Removing a Tome entry is new. `TokInterface` was append-only — `Save` only ever calls `SaveObject`,
so dropping an entry from memory alone would let it reload at the next login. `TokInterface.RemoveTok`
deletes the `characters_toks` row as well and tells the client. **The client packet is unverified:**
no capture shows a Tome entry being revoked, so sending `F_TOK_ENTRY_UPDATE` with a zero count is
inference from the shape of `SendTok`. Server state is correct regardless, so a relog will show the
truth even if the live update is ignored.

### The costs, from the client

`interface/interfacecore/maps/zone191/mappoints.xml` is the Necropolis of Zandri zone map, and it
holds both halves of the system — exactly as the in-game tip promises: *"Lairs, the Glyphs they
require for entry, and the Public Quests that award those Glyphs can all be found on the Land of the
Dead zone map."* Each `<landmark>` tomb lists the glyphs it needs; each `<publicQuest>` lists the
glyph it awards.

The map's glyph numbers are its own, not Tome entries, but the public quests appear in **both** the
map and `pquest_objectives`, so the two join on the PQ:

| map # | Tome (Dest) | Glyph | Awarding PQs on the map |
| --- | --- | --- | --- |
| 1 | 7960 | Reed | Sedjhet Temple 556, Nikosi Temple 550 |
| 2 | 7961 | Vulture | Aerie of Death 557, The Carrion Nest 551 |
| 3 | 7963 | Horse | *(roaming — not on the map)* |
| 4 | 7962 | Scroll | Obelisk of Judgment 558, The Quarry of Bone 552 |
| 5 | 7964 | Ankhra | Forbidden Vaults 561, Tombs of the Bitter Wind 555 |
| 6 | 7968 | Scorpion | *(roaming — not on the map)* |
| 7 | 7965 | Scarab | Pit of Asaph 559, Pit of Kem Senef 554 |
| 8 | 7966 | Vase | Hall of the Heavens 560, The Library of Zandri 553 |
| 9 | 7967 | Riverbarge | The Quay of Seftu 562 |
| 10 | 7969 | Skull | Temple of Ualatp 563 |

Two map numbers are left unexplained by the mapped PQs — 3 and 6 — and exactly two glyphs are left
over on the Tome side: Horse and Scorpion. Those are the two awarded by the **roaming** public
quests (Amsu's Charge, The Assault of Nekh Akhet, Ricci's Raiders), which have no map entry
precisely because they roam. Ten for ten, none spare.

The `<glyph>20</glyph>` on Temple of Ualatp is a typo for 10 in the client's own file: 10 is
required by the Tomb of the Sun and awarded by nothing else, 20 is required by nothing, and
Ualatp's `TokCompleted` is 7969 Skull — glyph 10's slot.

Which gives, as `lotd_tomb_glyph_costs` (migration 80), stored 0-based:

| Tomb | Zone | Glyphs |
| --- | --- | --- |
| Tomb of the Stars | 241 | Reed (0), Vulture (1), Horse (3) |
| Tomb of the Sky | 243 | Scroll (2), Ankhra (4), Scorpion (8) |
| Tomb of the Moon | 242 | Scarab (5), Vase (6) |
| Tomb of the Sun | 244 | Riverbarge (7), Skull (9) |
| Tomb of the Vulture Lord | 179 | **none** |

All ten glyphs are spent across the four lairs, each by exactly one. The Vulture Lord's landmark
carries no `<glyphs>` element at all, and live footage shows a player's tracker still holding every
glyph inside it, so it is left ungated.

### Only the door's own glyphs are taken

`ConsumeGlyphs` removes the glyphs that tomb charges and nothing else. An earlier draft wiped all
ten, reading "the players glyph progress would be reset" as a full wipe; that was wrong and would
have been brutal, since a group working the Necropolis banks glyphs for several lairs at once and
the first door would have thrown away everything not yet spent. The glyphs are a set of keys and a

door takes only its own.

## Death and respawn

> "After dying, you will respawn inside the Land of the Dead if your realm currently controls the
> dungeon. Otherwise you will respawn in your capital city."

Implemented in `WorldMgr.GetZoneRespawn`. When the dead player is in zone 191 and
`LotdService.CanRealmAccessLotd` says their realm does not hold the expedition, they go to
`GetCapitalCityRespawn` — the Inevitable City for Destruction, Altdorf for Order — instead of a
graveyard inside the dungeon. Only the holding realm can fly in, so a player of the losing realm
would otherwise be stranded at a respawn point they have no way to leave or return to.

Three details:

- The check runs **before** the public quest and scenario branches, because it overrides them.
  Dying to a Necropolis public quest after your realm has lost the expedition still sends you home.
- A player inside a scenario is excluded. The scenario owns its own respawn; the zone it happens to
  sit in does not get to override it.
- The holding realm needs a respawn point of its own inside zone 191 or the capital fallback would
  swallow them too. Both exist — `zone_respawns` 274 (Destruction, beside Da Dusty Dry) and 275
  (Order, beside Goldbarrow) — and `Test-LotdGlyphs.ps1` now asserts it, because losing either would
  present as the rule misfiring for the winners rather than as missing data.

`GetCapitalCityRespawn` was factored out of the old `GetRealmSafeFallback`, which is still the
last-resort path when a zone cannot supply a respawn at all. The difference is only in intent and
logging: the fallback warns because it means something is missing, whereas sending the losing realm
home is correct behaviour and logs at debug.

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
