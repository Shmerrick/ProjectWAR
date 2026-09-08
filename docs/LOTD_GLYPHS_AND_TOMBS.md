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


## Instance invasion — not implemented

> "A realm owning the LOTD can invade instances of the opposing realm."

**None of this is wired.** The client carries the whole feature; the server has nothing that drives
it.

### What the client has

`interface/default/ea_contestedinstanceselectionwindow/` is a complete UI mod — a "Contested
Instance Selection" lobby offering **New Instance** or **Invade Instance**:

```
LABEL_CONTESTED_INSTANCE_LOBBY         Contested Instance Selection
LABEL_CONTESTED_INSTANCE_INSTRUCTIONS  Would you like to start a new instance or invade an
                                       enemy player instance?
LABEL_CONTESTED_INSTANCE_INVADE        Invade Instance
LABEL_CONTESTED_INSTANCE_NEW           New Instance
```

`contestedinstanceselectionwindow.lua` gives the exact contract:

- **Server → client** raises `CONTESTED_SCENARIO_SELECT_INSTANCE`, handled as
  `OnSelectInstance(zone, canInvade, canJoinAsWarband)`. So the packet carries the zone, a
  can-invade flag (which greys the Invade button when false) and a can-join-as-warband flag.
- **Client → server** sets `GameData.ContestedInstance.zone` and `.invade`, then broadcasts
  `CONTESTED_INSTANCE_ENTER`; Cancel broadcasts `CONTESTED_INSTANCE_CANCEL`.
- **The lobby auto-cancels after 60 seconds**, and the lua notes `AUTO_CANCEL_TIME` "Should match
  value on server."

And there is a penalty for entering an invasion and running away, in `componenteffects.txt`:

> "You have left an instance you invaded without defeating it's defenders. You will be unable to
> enter another Land of the Dead instance while branded a coward."

with "You are a coward!" as the debuff text. So the design is: invade, and either beat the defenders
or be locked out of Land of the Dead instances until the brand expires.

### What the server has

Nothing, and the tombs are the wrong shape for it:

- **The tombs are group instances.** Their `zone_jumps` rows are `Type = 6` — one copy per group,
  `instance_infos.LockoutTimer` 1440 (24h). A group instance has no realm; `Instance.Realm` is 0 for
  all of them and is only used to tell a realm instance from a group one.
- **A realm-instance model does exist** — jump `Type = 4`, one persistent uncapped copy per realm,
  selected by `player.Realm` in `InstanceMgr.ZoneIn`. But only Mount Gunbad (zone 60) and Bastion
  Stair (zone 160) use it, 13 rows in total. No tomb does.
- **`Join_Instance` has no realm check at all**, in either direction. Nothing forbids cross-realm
  entry and nothing enables it, because nothing knows which realm owns a copy.
- **No "invade" concept anywhere in the server source.**
- **The two opcodes that would carry it are declared and never used:** `F_INSTANCE_INFO` (0x94) and
  `F_INSTANCE_SELECTED` (0xA2) appear in `Opcodes.cs` and `FrameWork/Utils/Utils.cs` and nowhere
  else — no handler, no sender, not even an ack stub, so an incoming 0xA2 would hit the unknown
  opcode path.

That those two opcodes are the lobby's pair is **plausible but unconfirmed**. They are the right
shape and they are the only unused instance opcodes, but neither appears in `INSTANCE_tombofstars`
or `INSTANCE_tombofsky` — captures of actual tomb entries. That is consistent with the lobby only
appearing when an enemy instance exists to invade, which those sessions may not have had, but it is
not evidence. Confirming the pair needs a capture taken with a live enemy instance present.


### Invadability is evaluated live, never stored

The rule moves underneath a running instance:

> "If someone starts an instance while they own LOTD, they cannot be invaded, but if control of LOTD
> swaps to the other realm, their instance can now be invaded."

So a copy's invadability is not a property of the copy. Nothing about the instance changes when the
expedition flips — the answer changes because the world did. `Instance` therefore stores only
`OwningRealm`, a fact about who opened it, and `InstanceMgr.CanBeInvadedBy(instance, player)`
computes the rest on every call:

1. the zone is an invadable Land of the Dead instance -- the four lairs or the Tomb of the Vulture
   Lord (`LotdService.IsInvadableLairZone`);
2. the invader's realm currently holds the expedition (`LotdService.CanRealmAccessLotd`) — this is
   the part that moves;
3. the copy belongs to the other realm.

There is deliberately no `Invadable` field to go stale. `GetInvadableInstances(player, zoneId)`
lists the enemy copies open in a zone, skipping empty ones — invading an empty instance is a private
dungeon run with extra steps, and the coward brand is defined in terms of defeating its defenders.

`OwningRealm` is separate from `Instance.Realm` on purpose. `Realm` is the realm-*instance* marker:
non-zero makes a copy persistent and filters its spawns to that realm, which is Mount Gunbad and
Bastion Stair behaviour and wrong for a lair. It stays 0 on group instances.

`.lotd instances` lists every open lair copy with its owner and whether the caller could invade it
right now. Flip the expedition with `.lotd unlock` and run it again: the same copies change from
safe to invadable with nothing about them having changed. That is the behaviour to test.

**What this does not do.** There is still no way for a player to invade — that needs the lobby
packets, which are not established (see above). This is the ownership model underneath it, and it is
observable through the GM command only.


#### The Tomb of the Vulture Lord is invadable

It was briefly left out of the invadable set here on the reasoning that it carries no glyph cost in
the client's zone map. **That was a bad inference** — the entry cost and invadability are unrelated
properties, and the "unknown" recorded against its glyph cost said nothing about invasion.

The evidence runs the other way:

- The coward brand is scoped to *"another Land of the Dead instance"*, with no exception carved out.
- **It is the only instance zone in the game with a split entrance per realm.** `TOTVL.AddPlayer`
  overrides the base entry to send each realm to its own `zone_respawns` row rather than a shared
  door, and the comment on it notes this is unique: *"every instance zone has two realm respawn
  rows, but in the others they are ordinary interior respawn points rather than a split entrance."*
  A per-realm entrance is precisely what a zone needs when both realms can be inside it at once.

So `InvadableLairZones` is 179, 241, 242, 243, 244.


### The Purge public quests

Invading a tomb runs a public quest, and five already exist — one per invadable instance:

| Entry | Name | Zone |
| --- | --- | --- |
| 595 | Purge the Tomb of the Stars | 241 |
| 596 | Purge the Tomb of the Moon | 242 |
| 597 | Purge the Tomb of the Sky | 243 |
| 598 | Purge the Tomb of the Sun | 244 |
| **599** | **Purge the Tomb of the Vulture Lord** | **179** |

599 is the database's own confirmation that the Vulture Lord is invadable — it has a Purge quest
like the four lairs.

Each has a "Purge!" stage counting six enemy kills and a "Survive!" stage. The live tracker shows
the full shape:

```
Conflict Within the Tomb - Purge (Normal)
    Order Defenders Purged -            0/6
                Order - Purge
    Destruction Invaders Purged -       0/6
                                     or 29:54
            Outlast the Invaders
```

So both sides are tracked at once: the invaders purge six defenders, the defenders purge six
invaders **or** outlast them for thirty minutes.

`Database/81_lotd_purge_pquest_stages.sql` fixes what is unambiguous — the Tomb of the Sky was
missing its "Survive!" stage entirely, and all five Survive stages now carry `Type = 12`
(`QUEST_SCRIPTED_EVENT`) with `Time = 1800`. That is not new machinery: `ScheduleScriptedStageAdvance`
already completes a scripted stage after `Time` seconds, and the tracker packet already sends
`Stage.Time` as the countdown. Left at `Type 0` / `Time 0` the stage inherited the 540-second
`TIME_EACH_STAGE` default — nine minutes where the client showed thirty.

**The "Defenders Purged" objectives are deliberately left at `Type 0`.** `QUEST_KILL_PLAYERS` (5)
looks like the obvious correction and is the wrong one: `PublicQuest.HandleEvent` has no
`QUEST_KILL_PLAYERS` case, so type 5 would go from "manual, awaiting a driver" to "handled by
nothing". Type 0 shares a branch with `QUEST_SCRIPTED_EVENT` and advances when something calls
`HandleEvent` with the objective's own Guid — which is what an invasion kill handler will do. That
handler does not exist yet.

Three things are still missing and need evidence rather than a guess:

- **The defender's counter.** The tracker shows "Destruction **Invaders** Purged" alongside "Order
  **Defenders** Purged", so the names encode a role. This database has only "Defenders Purged" rows;
  there is no "Invaders Purged" objective anywhere in it.
- **The realm asymmetry.** 595 reads "Order Defenders Purged" while 596-599 read "Destruction
  Defenders Purged", which would mean each tomb can only be invaded from one direction. Either realm
  can hold the expedition, so that cannot be right — but whether live used one quest with two
  objectives or a pair per tomb is not established.
- **The name.** The tracker header reads "Conflict Within the Tomb - Purge (Normal)", not "Purge the
  Tomb of the Stars".



### What the second capture set adds

**The 30-minute timer, confirmed twice.** "or 29:54" on a lair, and 24:36 counting down on the
header of "Purge the Tomb of the Vulture Lord - Purge! (Normal)". Migration 81's `Time = 1800` is
right.

**The Vulture Lord's names are exactly ours.** The live tracker header reads *"Purge the Tomb of the
Vulture Lord"* with stage *"Purge!"* — `pquest_info` 599 and its `StageName` verbatim. So the
"Conflict Within the Tomb - Purge" header seen on a lair is a different name for the four lair
quests only; 599 needs no rename, and 595-598 remain open.

**The invader tracks the defender's realm.** Order invading shows "Destruction Defenders Purged
4/6", which is exactly what 599 carries. So the per-tomb realm wording is not necessarily wrong — it
matches this capture — but a Destruction invasion of the same tomb must show the opposite, and
nothing in the database provides that.

**Three Major bags, and nothing else.** The Public Quest Scoreboard reads "3 Rewards" with three
purple icons over six contributors, and no bag of another colour appears.

`GoldChest.GenerateLootBags` could not produce that for any of the five. The four lairs carry
`PQDifficult` 3, which resolves to Hard and pays gold and blue bags the live window never shows.
Worse, **Purge the Tomb of the Vulture Lord carries `PQDifficult` 0**, which becomes
`(PublicQuestDifficulty)(-1)` and falls through to the `default:` branch, where only the gold count
is assigned and the array keeps its initialiser — **one white bag** for an invasion that should pay
three purples. The purge quests now bypass the difficulty switch entirely.

(That default branch also catches 64 other `PQType 1` quests carrying `PQDifficult 0`, each paying a
single white bag. Whether that is intended for them is not established, so they are left alone.)

**The reward tables differ by realm, and the reason is a hypothesis.** One bag offered Cluster of
Golden Scarabs / Golden Scarab / Oaken Figurine; another offered Golden Cartouche / Omnipotent
Myrmidon's Soul / Trueshot Myrmidon's Soul. The suggestion is that the table scales with how far the
*defending* group had progressed — the richer bag came from a group killed near the Vulture Lord, the
poorer from one killed after a few bosses. **Not established**, and one of the two captures is
disputed (the wrong bag was opened), so no loot table is written from it.

### Two mechanics with no implementation at all

**Progressive respawn checkpoints in the Vulture Lord.** Killing a boss unlocks a checkpoint that
becomes the player's respawn point for the zone, and an NPC in the first room ports arrivals to the
furthest unlocked one — which is why invaders are not seen starting from the entrance. That NPC's
realm access follows Land of the Dead ownership, so an invading realm can use it too.

Zone 179 has exactly **two** `zone_respawns` rows — 635 Order and 636 Destruction, both at the
entrance — and there is no checkpoint state, no porter NPC identified and no boss-kill hook. The
dungeon's ten-plus bosses are in `instance_boss_spawns` (BossID 179 upward), so the trigger points
exist; nothing consumes them.

**The quest chain that ties the roaming PQs to glyphs.** "Four Tombs for Four Pillars" exists as
`quests` 50020-50026, and its objective text matches the live tracker word for word — "Amsu's Charge
completed", "Horse Glyph received", "Nikosi Temple completed", "Reed Glyph received", and so on. But
every objective carries `ObjType 0` and `ObjID 0`, so nothing progresses them, and **50022 has no
objectives at all**. Same shape as the Purge counters: the data is here, the binding is not.

### Purge rewards, achievements and the soul talismans

Nearly all of this was already in the database; what was missing was the bindings between the parts.

**Present and correct.** The eight soul talismans are `item_infos` 2005595-2005602 with large-weapon
twins at 2005663-2005670, carrying the live tooltip text verbatim. The currencies exist — 208408
Silver Scarab, 208409 Golden Scarab, 208410 Silver Ankh, 208411 Golden Cartouche, 208414 Cluster of
Golden Scarabs, 208415 Fused Cluster of Golden Cartouches. So do the achievements: `tok_infos` 7514
"Pyramid Purged 1" carries the exact string from the live unlock banner, *"You have Purged the
Pyramid"*, with 10 and 20 tiers at 7515/7516, the defender counterparts at 7517-7519, 7500/7501 for
purging or defending all lairs, 7764 for five purges, and three titles — 10902 Tomb Purger, 10911
Purge Master, 10912 The Unpurgeable.

**The stat mapping, resolved.** Five come straight from the live reward window; the other three from
the Stats column of the Massive twins, which were never corrupted:

| Entry | Soul | Stat | |
| --- | --- | --- | --- |
| 2005595 | Demon | 1 Strength | from Massive Demon's `1:64` |
| 2005596 | Indominable | 3 Willpower | on screen |
| 2005597 | Iron | 4 Toughness | |
| 2005598 | Conquering | 5 Wounds | |
| 2005599 | Alacritous | 6 Initiative | on screen |
| 2005600 | Masterful | 7 Weapon Skill | on screen |
| 2005601 | Trueshot | 8 Ballistic Skill | on screen |
| 2005602 | Omnipotent | 9 Intelligence | on screen |

Eight souls for the eight stats WAR actually uses; stat 2 (Agility) is vestigial, which is why there
is no ninth. **Demon is Strength — stat 1, the first, not the last**, and Conquering is Wounds.

**Repaired (migration 82).** 2005595 was damaged identically in both item tables: name truncated to
"mon Myrmidon's Soul", empty description, `Bind` 0, `MaxStack` 1, and 771 characters of text-shaped
garbage in `Stats` (`116:8259;116:28448;...`). Rebuilt from an intact sibling and the Massive twin.

**The vendors.** The Golden Cartouche tooltip names them: *"These may be traded to archeologists
studying Nehekhara for powerful supplies and equipment."* Both are already spawned in zone 191, one
per warcamp, each identified by faction and position agreeing:

| Creature | Faction | Warcamp |
| --- | --- | --- |
| 93636 Archeologist Bergmann | 65 Order | Goldbarrow |
| 93656 Archeologist Sveinn Ravensight | 129 Destruction | Da Dusty Dry |

They shared `VendorID` 1 with **213 other creatures**, so their stock could not go there — it would
have appeared on 215 unrelated vendors worldwide. New lists 453 and 454 hold all sixteen souls each.

**The price is not established.** Nothing in `vendor_items` is priced in any Land of the Dead
currency, so there was no scale to copy, and no capture or client file fixes it. Migration 82 uses 5
Golden Scarabs for a normal vessel soul and 10 for a Massive one as placeholders to be replaced.

### Still missing

- **No loot bindings at all.** `pquest_loot` has zero rows for 595-599, and the souls and cartouches
  are loot for nothing anywhere in the database. The live reward window offers Fused Cluster of
  Golden Cartouches, Golden Cartouche, all eight souls, and 65 silver coins.
- **Talisman durations are not implemented.** Every tooltip reads "Duration: 8h", but
  `Item.cs:226` constructs every applied talisman as `new Talisman(entry, SlotId, 1, 0)` — timer
  always zero. The `Timer` field is persisted and never populated from item data. This affects every
  timed talisman in the game, not just these.
- **`Type` disagrees across the eight rows** (23, 0, 0, 0, 0, 0, 0, 31) and nothing establishes
  which is right, so migration 82 leaves it alone.

### What implementing it would take

1. ~~Give the tomb instances a realm and a way to find open enemy copies per zone.~~ **Done** —
   `Instance.OwningRealm` plus `InstanceMgr.GetInvadableInstances`, evaluated live.
2. Send the lobby when a player of the expedition-holding realm uses a tomb portal and an enemy copy
   exists, with `canInvade` set accordingly; handle the reply, with a 60-second server-side timeout
   matching the client's.
3. ~~Gate invasion on `LotdService.CanRealmAccessLotd` — only the holder may invade.~~ **Done**,
   inside `CanBeInvadedBy`.
4. Drive the Purge public quest: an invasion kill must call `HandleEvent` with the objective Guid,
   and the missing "Invaders Purged" objective has to be sourced.
5. Implement the coward brand: applied on leaving an invaded instance with its defenders alive, and
   blocking further Land of the Dead instance entry while held.

Note that (2) needs the packet layouts, which are not established, so this cannot be built from the
emulator source alone — see `CLAUDE.md` hard rule 3.

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
