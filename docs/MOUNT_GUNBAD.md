# Mount Gunbad (1.4.8 Restoration Target)

For the current completion gate, Release data audit and the user's planned Hard/Nightmare
extensions, see [Dungeon difficulty work](DUNGEON_DIFFICULTY.md). The custom modes are not
implemented; normal Gunbad still needs a complete client retest and the evidence gaps below.

What the live server's Mount Gunbad looked like, measured from the official capture, and what
this branch had instead. Written 2026-09-05.

Zone **60**, realm-instanced (`zone_infos.Type` 4), with four boss maps: **63** Gunbad Nursery,
**64** Gunbad Lab, **65** Squig Boss, **66** Gunbad Barracks.

## The capture

`WAR-RE-Toolkit/libs/protocolservices/Packet Logs/INSTANCE_GUNBAD_PART1.txt.gz` contains
**11,516 `F_CREATE_MONSTER` frames**, 11,303 of them inside Gunbad, covering 2,027 distinct
server object ids and 122 distinct creature names. It is the single best record of the dungeon.

`F_CREATE_MONSTER` payload offsets used throughout this document, taken from
`Creature.SendMeTo`:

| Offset | Field |
|---|---|
| +0 | object id |
| +4 | heading |
| +6 | Z |
| +8 | client X (u32) |
| +12 | client Y (u32) |
| +18 | model |
| +21 | **level** |
| +44 | state-block length, then the NUL-terminated name |

### Converting capture coordinates

Gunbad is an instance, so the client works in an atlas-shifted frame. For zone 60, `OffX`/`OffY`
are 200/200 and the shift is **(1, 9)** (`S_PLAYER_INITTED` #286846):

```
world = client - (shift << 13) + (Off << 12)
      = clientX - 8192  + 819200
      = clientY - 73728 + 819200
```

Checked against a row nobody has touched: Brood Mother Szikalax sits at world
`843823, 857429, 28390` and is sighted at client `32801, 111986, 28390` — the same point to
within 32 units, with Z identical.

**This is the step the earlier BUG-058 audit missed.** Comparing world coordinates directly
against client coordinates finds nothing, which is why that audit reported no matches.

## Levels — what Return of Reckoning changed

Gunbad had been rescaled to a rank-40 dungeon:

| Where | Was | Rows |
|---|---|---|
| `instance_creature_spawns.Level` (zone 60) | 40 | 345 |
| `pquest_spawns.Level` (zone 60) | 42 | 723 |
| `instance_boss_spawns.Level` | 41, 42, 44, 43 | 4 |
| `creature_protos.MinLevel/MaxLevel` | 40-42 | ~55 Gunbad-exclusive prototypes |

The capture shows the dungeon ran at **ranks 21-33**. A representative slice:

| Creature | Was | Retail | Sightings |
|---|---|---|---|
| Kezzen | 40 | 21 | 27 |
| Blackfang Recluse | 40 | 23 | 228 |
| Blightbreath War Troll | 42 | 23 | 96 |
| Redeye Squig Herda | 0 → proto 40-42 | 25 | 193 |
| Oozespawn Nurgling | 42 | 26 | 357 |
| Deathshadow Archer | 42 | 29 | 424 |
| Wight Lord Solithex | 43 | 30 | 1 |
| 'Ard ta Feed | 44 | 33 | 13 |

Restored by `Database/36_restore_gunbad_retail_levels.sql`. Every level there is the single
distinct value observed for that creature, and the observation count is recorded inline so each
claim can be rechecked. Creatures the capture does not contain — the Order-side dwarf NPCs at the
entrance, `Da Nanny`, `Goontz the Maniacal` — were left alone rather than guessed at.

The greenskin warcamp NPCs at the entrance (the `^M`-suffixed Bloody Sun Boyz) are level **40** in
the capture too, so they are correctly unchanged. Caret suffixes are gender markers, not
corruption — see `CLAUDE.md`.

`instance_creature_spawns.Level` and `pquest_spawns.Level` both take precedence over the
prototype range (`Creature.cs`: `else if (Spawn.Level != 0) Level = Spawn.Level`), so the level
work is scoped to Gunbad and cannot alter a creature elsewhere. One prototype, Stonemaw War Troll
(36620), is deliberately excluded from the prototype pass because it has a Badlands spawn.

## Placement — already correct

Gunbad's layout was **not** changed, because measurement says it does not need to be:

- 460 of 525 instance spawns (88%) sit within 50 units of a sighting of the same creature;
  487 within 1,000.
- Reconstructing public-quest spawn points independently from the capture — one point per
  distinct object id, first sighting, de-duplicated at 60 units — yields 398 points, and **360 of
  them fall within 150 units of a `pquest_spawns` row already in the database**.

The second result is the stronger one: an independent reconstruction from the packet log
rediscovered the placements already stored. Gunbad's geometry matches the live server.

## Public quests

Nine, all in zone 60, all `PQType` 1 / `PQTier` 3, on chapter 65:

| Entry | Name | Area |
|---|---|---|
| 181 | Redeye Nightmare | 16 |
| 507 | Kizzig's Gobbo Place | 2 |
| 508 | Mangle the Wrangla | 1 |
| 510 | Redeye Stompin Grounds | 7 |
| 511 | Shadowweb Spawning Grounds | 9 |
| 512 | Squig Crazy! | 4 |
| 513 | A Taint from Below | 5 |
| 514 | The Squig Nursery | 6 |
| 515 | Mad Mixas | 3 |

Their creatures come from `pquest_spawns`, keyed on the **objective's `Guid`**, and are built as
`PQuestCreature`s — which matters, because `PQuestCreature` is the only class that reports a kill
back to the quest (`PQuestCreature.cs:113`). An ordinary creature with the same prototype credits
regular quests but never a public quest, so PQ targets cannot be supplied by adding
`instance_creature_spawns` rows.

### The transposed rows (BUG-065)

43 Gunbad rows had `Objective` and `Entry` **swapped**:

| Stored Objective | Stored Entry | Should be | Rows |
|---|---|---|---|
| 36556 (Skewerin' Squig) | 2296 (Slay Squigs) | 2296 / 36556 | 17 |
| 36555 (Stinkspewin' Squig) | 2296 (Slay Squigs) | 2296 / 36555 | 18 |
| 36545 (Oozespawn Plaguebearer) | 2299 (Oozespawn Plaguebearer) | 2299 / 36545 | 4 |
| 15099 (Blackfang Widow) | 2293 (Blackfang Spiders) | 2293 / 15099 | 4 |

Each pair identifies itself: the value in `Objective` is a creature entry whose name matches the
objective, and the value in `Entry` is the id of the objective that names it.

The effect was twofold. `PQuestService` keys its spawn dictionary on `Objective`, so these rows
attached to no objective at all — *Squig Crazy!* lost 35 of its 77 squigs. And the entry they did
carry resolved to unrelated creatures: 2296 is **Cleansing Flame Warrior**, 2293 **Silken^f**,
2299 **Henri Kopler^M** — Empire creatures with no business in a greenskin mine.

Migration 36 swaps the columns for zone 60 only. The same corruption exists in eleven other zones
(49 rows in zone 1, 21 in zone 8, smaller counts elsewhere) and is deliberately left for a
separate, separately evidenced change.

### After the repair

Every kill objective of all nine quests now has at least one spawn row naming a creature it
actually credits, and `NoRespawn` is 0 throughout, so the required counts are reachable through
respawns even where an objective has fewer spawn points than its target count.

## Open

- **BUG-069.** 24 rows on objective 2293 name creature prototype **387121**, which does not
  exist. Positional matching against the capture makes Blackfang Hatchling (38720, level 28) the
  most frequent nearest sighting — 8 of the 18 points with any sighting within 600 units — but
  that is suggestive only, and Blackfang Hatchling is not one of that objective's credited
  targets (15099 / 38719), so correcting it would not change whether the quest completes.
- One stray row places `Soul of Oswin Breitenbach` (6455) in zone 60 on objective 1536, which
  belongs to public quest 332. Not a Gunbad quest; left alone.
- The Order-side dwarf NPCs at the entrance (`creature_protos` entries in the 2000xxx range, plus
  `Drugni Deepblade`, `Monira Grimgold`, `Oathbearer Stormguard`) appear nowhere in the capture
  and look like Return of Reckoning additions. They were not removed — that is a content
  decision, not a level correction.
- BUG-058's painted-area work (Gunbad's eight pieces resolving one area row) is unchanged.
- **BUG-073.** Portal jump id 629156888, reported as refusing entry, has no `zone_jumps` row and
  appears in none of the 88 distinct client jump ids across the capture set. `F_ZONEJUMP` now logs
  the zone and pin coordinates of an unknown id, so the next report locates the portal instead of
  inviting a guessed destination row.
- **Monstrous Squig respawn.** Objective Guid 2301 carries `RespawnSeconds` 15 (migration 44),
  honoured by `PQuestCreature.SetRespawnTimer` ahead of the flat 10-minute dungeon default. This
  was set at the user's direction and is not capture-derived. It covers all three creature types
  the objective names -- Spikestabba (38631), Warchargin' (38629) and Deathspewin' (38630) Squig,
  30 spawn rows -- which is the reading of "its add group mobs" the objective itself supports.
  **Open question for the user:** the same stage's objective 2302 "Swarmin' Lit'l Squig" (38628,
  35 kills, 71 spawns) was left at the flat 10-minute dungeon default. If the intent was the whole
  squig encounter rather than the Monstrous Squigs objective, 2302 needs the same 15 seconds; it
  was not changed because that is a difficulty decision, not a correction.
