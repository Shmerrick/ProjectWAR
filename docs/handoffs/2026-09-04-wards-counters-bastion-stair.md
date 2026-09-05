# Session handoff — 2026-09-04

Ward fragments, ward task counters, dungeon influence, and Bastion Stair realm instancing.
Everything below is committed and pushed on `RESTART` through `ea3dea7a`.

## Pending before anything is retested

1. **The server was left running.** The Land of the Dead fix (BUG-040) is committed as source but
   **was never built** — `MSBuild` could not copy `WarZone64.dll` while `WorldServer.exe` held it.
   Stop the stack and rebuild `WorldServer.csproj` before testing anything.
2. **Migrations 27 and 28 are applied to the live database but cached in the running process.**
   `InstanceService._InstanceBossSpawns` and the item/tok caches load at boot only.

So: stop the stack, rebuild, start, then test.

## Landed and confirmed in game

- **Ward fragments award on equip** (BUG-033). The cause was not the ward code: `ItemService`
  reads `mythic_src_item_infos` under the shipped `UseMythicActionCoverageTables = true`, while
  scripts 01/05/20 wrote `TokUnlock3` to `item_infos` alone, so every item had 0 in memory.
  Script 21 syncs them.
- **Cross-tier cascade** (task 2). Confirmed with three Supreme pieces and with two independent
  sources ticking one fragment.

## Landed, not yet confirmed

- **Realm instancing.** `instancetyp == 4` implemented and Bastion Stair moved to it (script 24).
- **Ward task counters.** All 32 bindings seeded (script 25); boss kills, Bastion PQ completion
  and RR-ranked kills hooked. `.ward counters` / `.ward add` / `.ward complete` drive them.
- **Dungeon influence.** Creature kills grant it, on the killer's own realm track, shared by
  damage dealt (BUG-036/037).
- **Comma-split ward tasks** (BUG-039), **duplicate boss spawns** (BUG-040/28), **LOTD title**
  (BUG-040) — the last needs the build.

## The blocker to look at first

**BUG-041 — 180 of 219 zones have no `pqareaNNN.png` or `areasNNN.png`.**

This is why no Bastion Stair public quest worked on either realm, and it is the root of several
things that looked like separate bugs:

- `Player.CurrentPQArea` comes from `pqareaNNN.png`. Without it `PublicQuest.Update` can never
  match a player to a quest, so no tracker, no objectives, no completion.
- `CurrentArea` comes from `areasNNN.png`. Without it the realm-aware influence path cannot
  resolve and falls back to `ChapterId`, and the creature-kill influence grants nothing.

The public quests themselves are fine — all ten load into region 160 cells at boot with
objectives and spawns. Zone 160 has only `influenceids.csv`, `offset.png`, `terrain.png`; Gunbad's
zone060 has `areas060.png`, `pqarea060.png` and `60_map.dds` as well, which is why its quests run.

Nothing in this repository fixes it; the data must be produced. WAR-RE-Toolkit's
`databaseimports` documents `convert_pcx_to_png.py`, which generates this class of asset from the
client's `.pcx` files — see `docs/CROSS_REPO.md`. **Until these exist, testing PQ behaviour or
area-based influence in Bastion Stair cannot succeed**, so treat that work as blocked rather than
broken.

## Open, with no answer yet

- **Order cannot enter Bastion Stair.** Portal `108856104` has no `zone_jumps` row and cannot
  reuse the Destruction arrival — `zone_jumps` carries `UNIQUE KEY (WorldX, WorldY, ZoneId)`.
  That constraint suggests each realm arrived at its own point. All eighteen Bastion captures are
  Destruction-side, so those coordinates need video or user knowledge.
- **Influence appearing on id 129 rather than 6** for an Order character. Script 23 repointed
  `zone_areas`, so either that influence predates the script or something still bypasses it.
  Recheck after a clean restart. Note BUG-041 may be the real cause.
- **RvR lake influence** shows before entering the lake; a player must enter and leave to see it
  correctly. Not investigated.
- **Steps of Ruin has no `zone_areas` row** at all — the middle wing awards no influence even
  once BUG-041 is resolved.
- **Bloodlord defines 5 of 24 career sets** (BUG-034), and no Bloodlord item appears in any loot
  table. Both need authentic sources.
- Counters `701` and `720` have no target; six creature names in the boss tasks resolve to no
  prototype. 14 counters in the keep/zone/scenario/city/fortress/lair families are unhooked.

## Two corrections made this session, worth not repeating

- **Caret suffixes are gender markers, not corruption.** `^m`/`^M`, `^f`/`^F`, `^n`, `^p` mark
  masculine, feminine, neuter and plural for German and French localisation; the client uses the
  same convention (`Mad Mixas^n,in`). A script stripping them from 5,210 names was written,
  applied, pushed, and had to be reverted from the base dump. Never strip them — make lookups
  tolerate the suffix. Recorded in `CLAUDE.md`.
- **Instancing a zone does not destroy its public quests.** A dungeon zone has `Region = ZoneId`,
  `CellSpawns` is pure data, and the loaded flag lives on the per-region `CellMgr`, so each realm
  region builds its own objects from shared definitions. The real hazard was double-spawning,
  which script 24 addresses.
