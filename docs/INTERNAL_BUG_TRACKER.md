# Internal Bug Tracker

This document tracks internal bugs, regression, and known issues within the ProjectWAR emulator.

## Active Bugs

| ID | Summary | Description | Status | Priority | Reported |
|:---|:---|:---|:---|:---|:---|
| BUG-001 | Invader items not unlocking Superior Ward | Equipping the Invader items does not unlock the equip entries in the tome of knowledge for the section about wards, specifically the Superior Ward. There is a third tokunlock that is not being activated on equip. | Open | Medium | 2026-03-30 |
| BUG-002 | Floating chest objects | Chest objects are not always finding the nearest Z-height collision, resulting in the chest object floating above the ground. Likely caused by `deps/zones` never having been extracted, so `ClientFileMgr` could not load the `offset.png` / `terrain.png` height maps that Z-resolution reads. Zone data restored 2026-08-29; needs in-client confirmation. | In Progress | Low | 2026-03-30 |
| BUG-003 | RvR zone transition crash | Transitioning from Reikland to Reikwald crashes the server. Two root causes found and fixed 2026-08-29, both on the zone-transition path: (1) `RegionMgr.CheckZone` dereferenced `obj.Zone.Info` although `Object.Zone` is documented nullable and is null between zones — thrown from `SetOffset` on the movement path, outside the region tick's exception handler, so it terminated the process; (2) `RegionMgr.ZonesMgr` was a plain `List<ZoneMgr>` mutated by on-demand zone creation while `WorldMgr.GetZonesFightLevel` enumerated it from the world thread. Entering Reikwald for the first time creates its `ZoneMgr`, hitting both. Now a lock-guarded find-or-create plus a `GetZones()` snapshot for readers. Awaiting in-client reproduction to close. | In Progress | High | 2026-03-30 |
| BUG-004 | LOS vertex position precision error | ~16-unit max Y error from NIF world-matrix accumulation during native LOS generation. | Open | Low | 2026-03-30 |
| BUG-005 | LOS missing water generation | Zone 280 (and others) missing `water.xml` in current extracted data, leading to missing water chunks in generated LOS. | Open | Low | 2026-03-30 |
| BUG-006 | LOS multi-zone coverage gap | Native LOS generation currently only fully supported for zone 280 due to missing source files for other zones in current extraction. | Open | Medium | 2026-03-30 |
| BUG-007 | Ability data gaps | 12,664 abilities identified with Partial or StringsOnly coverage in `ClientDataMatrix` analysis. | Open | Low | 2026-03-30 |
| BUG-009 | Orphaned `creature_abilities` rows | 23 rows in `war_world.creature_abilities` reference ability ids that `AbilityMgr.GetAbilityInfo` cannot resolve, so those NPCs silently load without the ability (`AbilityMgr.cs:887`, DEBUG only). Concentrated in a few ids — `7843` accounts for 12 references, plus `3384`, `3375`, `5054`, `5057`, `5063`, `10811`, `10812`. One row has `AbilityId = 0`, which is malformed regardless of source data. 23 of 3,546 rows (0.6%). Fixing needs real ability definitions from a source of truth, not invented rows; the `AbilityId = 0` row can be dropped outright. | Open | Low | 2026-08-30 |
| BUG-008 | Unknown server-side operations | Component operations 29, 30, 32, 40, 41, 47, 51 remain unnamed and their semantics are opaque. | Open | Low | 2026-03-30 |

| BUG-010 | Five taxi destinations land outside their own zone | `zone_taxis` rows for zones 62, 132, 139, 168 and 204 hold coordinates that fall outside the zone they name. `MovementHandlers.cs:949` teleports to `(ZoneID, WorldX, WorldY)` as a unit, so taking one of these flights puts the player outside the world geometry. The coordinates appear to belong to a neighbouring zone rather than being random: 139 "High Pass Cemetery" lands in 102 "High Pass", 168 "Altdorf Contested" lands in 162 "Altdorf", and 132/168 hold byte-identical coordinates, suggesting a copy-paste. Zone 62 (Karaz-A-Karak) lands in 191 (Land of the Dead) and is simply wrong. As of 2026-08-30 `ZoneService.NormalizeTaxiWorldPosition` disables any taxi it cannot normalize, so these flights are hidden rather than offered — the same fail-safe already used for LOTD. Closing this needs correct coordinates from a source of truth; do not invent them. | In Progress | Medium | 2026-08-30 |
| BUG-011 | `RegionMgr.Players` read across threads | `Players` is documented "should only be accessed from within this region's thread" and is mutated unsynchronized by `AddNewObjects`/`RemoveOldObjects` on the region thread. `InstanceMgr` enumerates it from GM commands on the packet-handler thread (`InstanceMgr.cs:243`) and `Instance.CheckInstanceEmpty` reads `.Count` from the instance thread. Same class as the BUG-003 crash, but contained: packet dispatch wraps handlers in try/catch, so the symptom is a GM command that intermittently fails rather than a server crash. A real fix needs either a lock in the region tick or a copy-on-write list; both touch a hot path used in ~15 places, so this is deliberately deferred. | Open | Low | 2026-08-30 |

## Not Bugs (investigated and explained)

| Topic | Finding |
|:---|:---|
| Land of the Dead flight master unreachable | Working as designed. `LotdService.CanRealmAccessLotd` requires the tracker to be `Paused` **with** a non-neutral owning realm; the default state is `Active`/Neutral at 0 of 500 points, earned 100 per T4 battlefront lock. So the expedition opens only after one realm wins the race, then stays open for `UnlockDurationMinutes` (30). Zone 191's taxi rows are healthy for both realms. This is indistinguishable in-game from a broken flight master, and there was previously no way to control it — hence the `.lotd` GM commands added 2026-08-30 (`status`, `unlock`, `reset`, `award`). |
| "Slow SQL" warnings at startup | Not a scaling problem. The ~2.1s query is the one-time `Item_Info` load of 88,727 rows into memory (~24µs/row) so that gameplay reads the cache rather than the database. It does not grow with player count. The genuine scaling issue was unindexed per-login lookups — see `Database/03_add_hot_path_indexes.sql`. |

## Bug Reporting Guidelines

When adding a new bug, please include:
- **Summary**: A concise title.
- **Description**: Detailed steps to reproduce and expected vs. actual behavior.
- **Priority**: Low, Medium, High, or Critical.
- **Status**: Open, In Progress, Resolved, or Closed.
