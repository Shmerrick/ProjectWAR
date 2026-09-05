# Checkpoint — 2026-09-05

**Correction later on 2026-09-05:** read [the stabilization handoff](2026-09-05-stabilization.md)
before using this historical checkpoint. Migration 23 confused `chapter_infos.Entry` with
`InfluenceEntry`. The client `maps/zone160/influenceids.csv:2-3` and
`maps/zone060/influenceids.csv:2-3` establish tracks 129/128 and 64/65, restored by migration 32.
The 231-area count and the proposed map-rectangle fallback below are also superseded there.

Current state of the ward, dungeon-influence and Bastion Stair work. Everything described here is
committed and pushed on `RESTART` through `70aa1e4e`. Read this before continuing that work.

## Start here

1. **Migrations 21 and 23-31 are applied to the local database already.** There is no `22`; it was
   written, applied, pushed and then reverted (see *Mistakes* below). If you are setting up fresh,
   apply them in order; the numbering gap is deliberate.
2. **The binary is current** with every `.cs` as of this checkpoint.
3. **Almost everything here is boot-cached.** Zone types, jump types, instance spawns, item data
   and tok data all load at startup, so a database change does nothing until the server restarts.
4. `bin/Release/WorldServer.exe` cannot be rebuilt while the server runs — it holds
   `WarZone64.dll`. Stop the stack first. A build that fails only on that copy has still compiled
   the assembly, so check timestamps before assuming the change is missing.

## What works now, verified in game

- **Ward fragments award on equip**, and the cross-tier cascade works: acquiring fragment N of a
  tier awards fragment N of every tier below it. Confirmed with Supreme, Annihilator, Conqueror,
  Sentinel and Bloodlord pieces.
- **Ward task counters** exist for all 32 client-defined bindings, with boss kills, Bastion Stair
  PQ completion, and RR-ranked player kills hooked. `.ward counters`, `.ward add <acId> <n>` and
  `.ward complete <acId>` drive them without grinding.
- **Public quests run in Bastion Stair** — a first. A PQ was completed end to end with a loot bag
  and reward chest.
- **Dungeon influence accrues** from creature kills, on the killer's own realm track.
- **`.boot`** performs the full canonical shutdown and logs `Closing the server (...)`.
- **Bosses spawn once.** They were duplicated in `instance_boss_spawns`.

## The one blocker that shapes everything — BUG-041

**180 of 219 zones have no `areasNNN.png` or `pqareaNNN.png`.**

`ClientFileMgr` resolves a player's zone area and public-quest area from two 1024x1024 PNG
overlays per zone, read as `id = 1 + (R >> 4) + (G >> 4)` at `[pinX >> 6, pinY >> 6]`. Without
them `CurrentArea` and `CurrentPQArea` are never set, so **no public quest can activate and no
area-driven influence can resolve** in those zones.

These are **not client assets**. The extracted 1.4.8 client has no area bitmaps at all — only
`terrain.pcx`, `shadow.pcx`, `offset.pcx`, `holemap.pcx`. Mythic defined regions analytically:
`zones/zoneNNN/sector.dat` declares forest and skydome regions as `numrects` / `numcircles` in
world coordinates. PQ bounds were almost certainly server-side data, which is why the client has
none. The 39 PNGs that exist are a previous emulator's reconstruction, complete with
anti-aliasing debris (regions of 1-9 pixels).

Bastion Stair was unblocked by generating both maps — see `tools/pqarea/` for the generator,
reader and source query. **The PQ boundaries there are an approximation**: scored against Mount
Gunbad's painted map, this style of disc fit reaches about 57% IoU. Centroids land within 1-20 px
of `pquest_info.PinX/PinY`, so the regions are in the right places, but the edges are invented.
`areas160.png` is uniform and is exactly correct rather than approximate, because all three of
zone 160's `zone_areas` rows carry the same influence ids.

**A better representation exists and is worth doing**: `mappieces.csv` already defines each
`PieceId` as a rectangle in the same pixel space, so the *zone area* map is redundant with data
we already have and could be computed instead of painted. PQ areas would be better as geometry in
the database — rectangles or circles per `PQAreaId`, matching `sector.dat`'s idiom — rather than
pixels. Neither is done.

## Open, with nothing invented to cover it

- **Order cannot enter Bastion Stair.** Portal `108856104` has no `zone_jumps` row and cannot
  reuse the Destruction arrival: `zone_jumps` carries `UNIQUE KEY (WorldX, WorldY, ZoneId)`, so
  two jumps may not share a destination. That constraint is itself evidence each realm arrived at
  its own point. All eighteen Bastion Stair packet captures are Destruction-side, so the Order
  coordinates are not recoverable from them.
- **Several internal wing portals drop the player through the floor or out of bounds**, and the
  Destruction respawn is wrong. Those coordinates are unchanged from the base dump. Reproduce and
  note the portal id from the GM `Portal Id:` message to fix a specific row.
- **Tomb of the Vulture Lord should be cross-realm**: the opposing realm can invade an instance in
  progress. Instance selection is currently group-leader based, so two groups never share an
  instance and invasion is impossible. **Unresolved design question: one shared TOVL instance for
  the whole server, or one per opening group that others may join?** Do not guess.
- **Reward chests stacked three deep once** in Bastion Stair. `GoldChest.Create` now logs the
  quest, region and position of every chest so the next occurrence identifies the cause. Ruled
  out: the sound-player object at the same coordinates (its proto does not exist and every
  Bastion PQ has `SoundPQEnd = 0`), and `CreatePQuest` duplicate detection (its log line never
  appeared). Note chests self-destroy after 180s while the PQ resets on a shorter cycle, so
  overlapping completions may be the whole explanation.
- **Bloodlord defines 5 of 24 career sets** (BUG-034), and no Bloodlord item appears in any loot
  table. One authentic tooltip was recovered — the Magus set, `Bloodlord Warped Daemonhide Robes`,
  with its 2/3/4/5-piece bonuses — recorded in `docs/BASTION_STAIR.md`. It establishes naming and
  structure only; the other 19 careers need their own sources, and the stats differ by archetype.
- **Ward counters 701 and 720 have no target.** Six creature names in the boss tasks resolve to no
  `creature_protos` row: Warlock Peenk, Necromancer Malcidious, Seraphine, Ssyridian Morbidae,
  Twin Lectors, and the Lost Vale mini-boss set. 14 counters in the keep, zone, scenario, city,
  fortress and lair families are not hooked at all.
- **`Steps of Ruin` has no `zone_areas` row**, so Bastion Stair's middle wing has no area of its
  own even with the generated map.
- **231 of 419 `zone_areas` rows name chapters that do not exist** (BUG-038), so influence earned
  there is silently discarded. Only the two dungeon zones had a determinable correct answer.

## Mistakes made here, so they are not repeated

- **Caret suffixes are gender markers, not corruption.** `^m`/`^M`, `^f`/`^F`, `^n`, `^p` mark
  masculine, feminine, neuter and plural for German and French localisation, and the client uses
  the same convention (`Mad Mixas^n,in`). A script stripping them from 5,210 names was written,
  applied, pushed, and had to be reverted from the base dump. **Never strip them**; make lookups
  tolerate the suffix. That is why migration `22` does not exist.
- **`Player.OnLoad` is not only the login path.** It runs again whenever a player enters an
  instance. A crash-recovery relocate hooked there ejected players from every dungeon they
  entered. Anything login-only needs its own guard.
- **Instancing a zone does not destroy its public quests.** A dungeon zone has `Region = ZoneId`,
  `CellSpawns` is pure data, and the loaded flag lives on the per-region `CellMgr`, so each region
  builds its own objects from shared definitions. The real hazard is double-spawning where world
  and instance spawn tables overlap.
- **Reserve low id ranges, not high ones.** Realm instance ids were first placed at 60000, which
  the sequential allocator could eventually reach. They are now a small block at the bottom with
  the dynamic allocator starting at 2000.

## Conventions that bite

- Items live in **two parallel tables**. `ItemService` reads `mythic_src_item_infos` under the
  shipped `UseMythicActionCoverageTables = true`, not `item_infos`. A migration touching item
  columns must update both. This cost three scripts and a full debugging session (BUG-033).
- `pquest_info.ChapterId` is **not** a chapter reference; it holds an influence id, and both
  dungeons stored the Destruction one for every quest.
- `Zone_Area.IsRvR` is simply `Realm == 0`, which every dungeon area satisfies.
- `Player.AddInfluence` **returns silently** when the chapter does not exist, which is how
  BUG-037 and BUG-038 stayed invisible.
- `zone_jumps` has a junk row at `Entry 0` pointing at Avelorn, so any portal resolving to id 0
  used to teleport there rather than fail.

## Where the evidence lives

`docs/CROSS_REPO.md` is the map. In short: the WAR-RE-Toolkit repo at
`D:\Repos\Shmerrick\WAR-RE-Toolkit` (decoded findings in `RE_FINDINGS/`, 1,027 official packet
captures under `libs/protocolservices/Packet Logs`), the live 1.4.8 client at
`C:\Users\Admin\Videos\Warhammer Online - Age of Reckoning`, and the extracted client tree at
`C:\Users\Admin\Downloads\myps`.

Order of authority when sources disagree: the client, then official captures, then decoded
findings, then this repository's own code and database. Where nothing establishes a value, leave
it unpopulated and record the gap — several tracker entries are open precisely because inventing
data was refused.
