# Mount Gunbad levels and Land of the Dead access — 2026-09-05

Follows `2026-09-05-commit-handoff.md`. Four tasks were requested: revert the Return of
Reckoning level changes to Mount Gunbad and match its spawns to the live-server capture; make
the Gunbad public quests fire; make the Land of the Dead UI element appear on the map and
overlay; and make flight masters allow travel to Land of the Dead.

All four are addressed. Nothing here has been retested in the client.

## What changed

### Mount Gunbad levels — migration 36

`Database/36_restore_gunbad_retail_levels.sql`. Gunbad had been levelled as a rank-40 dungeon:
345 instance spawns at 40, all 723 public-quest spawns at 42, the four wing bosses at 41-44, and
~55 Gunbad-exclusive prototypes at 40-42. `INSTANCE_GUNBAD_PART1.txt.gz` — 11,516
`F_CREATE_MONSTER` frames — shows ranks 21-33. Every level in the migration is the single
distinct value observed for that creature, with its sighting count recorded inline. Creatures the
capture does not contain are untouched. Details in `docs/MOUNT_GUNBAD.md`.

### Gunbad placement — measured, not changed

The earlier BUG-058 line "exact-placement capture audit found no matches" was wrong: it compared
world coordinates against client coordinates without the (1,9) instance atlas shift. With the
shift applied, 460 of 525 instance spawns sit within 50 units of a same-creature sighting, and an
independent reconstruction of 398 public-quest spawn points from the packet log lands 360 of them
within 150 units of the row already in the database. The layout already matches the live server,
so no placement was changed.

### Gunbad public quests — migration 36, section 5

43 `pquest_spawns` rows had `Objective` and `Entry` transposed. Because `PQuestService` keys its
spawn dictionary on `Objective`, they attached to no objective; and the entry they did carry
resolved to Empire creatures — Cleansing Flame Warrior, Silken^f, Henri Kopler^M — spawning in a
greenskin mine. Squig Crazy! was missing 35 of its 77 squigs. Swapping the columns for zone 60
restores them. Afterwards every kill objective of all nine quests has spawn rows naming a
creature it credits, `orphaned_pq_spawns` is 0, and `NoRespawn` is 0 throughout so the counts are
reachable.

### Land of the Dead tracker UI — `LotdService.SendResourceTracker`

`SendResourceTracker` returned early unless the player was already inside zone 191. That packet
is the only source of the client's RRQ table, and the world map, HUD tracker and flight-master
bars all render from it while gating on the map *view*, never the player's zone. So the bars
could only ever appear to somebody already standing in Land of the Dead.

The early return existed to stop a "Necropolis of Zandri" title card appearing for players who
had never been there. Those are two different packets; the zone activation stays scoped to zone
191, the `F_RRQ` tracker packet now goes to everyone, and `SendRvrTracker` — which re-activates
the tracker for the player's *current* zone — moved inside the zone-scoped branch where it
belongs.

Retail sent `F_RRQ` regardless of zone: 13 times in `2013-09-29 Chaos Wastes`, 48 in the
`2013-09-25 Inevitable City` siege, 29 in `Caledor part 1`, the first inside the login burst each
time. Our packet layout was already byte-for-byte correct against those captures.

### Land of the Dead travel — migration 37 and the taxi path

Two independent faults.

`zone_infos.Pairing` for zone 191 was 4. The client keeps two disjoint pairing ranges and
`GetNewDataAndSort` discards anything in neither; 4 is above `NUM_PAIRINGS` (3) and below
`ExpansionMapRegion.FIRST` (100), so the destination was thrown away before it reached the map —
travel was impossible even for a realm holding the expedition. Capture
`MECHANIC_orderflymaster_NecropoleOFZandri(LoD)` #9 sends pairing **100** and price **3000**;
migration 37 restores both.

The destination was also being omitted from the flight list whenever it could not be used. The
client expects the opposite — it hard-codes zone 191 into `ZoneNumbersLookup`, disables every
button by default, re-enables what the server lists, and has a dedicated zone-191 mouseover
branch printing `TOOLTIP_TRAVEL_WINDOW_LAND_OF_DEAD_REQUIREMENTS`. It is now always listed, with
the availability byte the client reads as `flightData.zoneAvailable`, and `F_FLIGHT` re-checks
before moving anyone.

### Land of the Dead access model — `LotdService`

Access had been limited to the 30-minute Paused window after a win, and `OwningRealm` was cleared
on every unpause and every boot. Captures show the pause freezes *scoring*, not access, and the
holder keeps the expedition until the other realm wins:

```
PvE_Landofdead_SHAMY40RR95
  #21808   timer=0    realm=2   Order 448/500   Destruction 256/500
  #21916   timer=30   realm=1   Order   0/500   Destruction 256/500
```

Order crosses the threshold and only Order's score is spent; Destruction keeps 256. Meanwhile a
Destruction player quests inside zone 191 for the whole session at timer 0. `CanRealmAccessLotd`,
`SetPausedState`, `ResumeRace`, `NormalizeTrackerState` and `BuildHeaderRealmValue` were corrected
to match, and `ForceReset` now performs a real reset rather than relying on unpause to clear
state. Full derivation in `docs/LAND_OF_THE_DEAD.md`.

## Evidence

All under `WAR-RE-Toolkit/libs/protocolservices/Packet Logs/`. Packet ordinals are 1-based across
both directions and reproducible with `tools/validation/Read-OfficialPackets.ps1`.

- `INSTANCE_GUNBAD_PART1.txt.gz` — 11,516 `F_CREATE_MONSTER` frames; levels and placement.
- `MECHANIC_orderflymaster_NecropoleOFZandri(LoD).txt.gz` #9 — the 28-destination flight list.
- `PvE_Landofdead_SHAMY40RR95.txt.gz` — 55 `F_RRQ` packets including a live win at #21916.
- `2013-09-29-SORC40RR85_RvR_T4_ChaosWaste.txt.gz`, `2013-09-25-SORC40RR84_RvR_inevitablecity_sieges.txt.gz`,
  `2013-09-30-ZEALOT40RR100_PvE_T4Caledorpart1_noPQ.txt.gz` — `F_RRQ` outside zone 191.

Client, from the extracted tree at `C:\Users\Admin\Downloads\myps`:
`interface/default/easystem_rrq/source/rrqprogressbar.lua`,
`ea_worldmapwindow/source/worldmapwindow.lua`, `.../pairingview.lua`, `.../worldview.lua`,
`ea_interactionwindow/source/interactionflightmaster.lua`, and
`data/strings/english/default.txt` for `TOOLTIP_TRAVEL_WINDOW_LAND_OF_DEAD_REQUIREMENTS`.

## Validation performed

- Release/x64 solution build, clean, no new warnings.
- Migrations 36 and 37 applied to the local Release database and re-applied to confirm
  idempotency. Post-state: `bosses_rank40_plus` 0, `orphaned_pq_spawns` 0, zone 191 pairing 100 /
  price 3000 with two enabled taxi rows.
- `tools/validation/Test-RuntimeRegressions.ps1` — PASS.
- `tools/validation/Get-WorldDataHealth.ps1` — unchanged (419 area rows, 42 without tracks; a
  pre-existing figure unrelated to this work).

**Not done: no in-client testing.** The stack was not started, so nothing here is confirmed
against a running server or a real client. The confirming tests are: enter Mount Gunbad and check
creature ranks and that each public quest advances; open the world map and the flight master and
look for the Tomb Kings bars; and `.lotd unlock <realm>` then fly.

## Still open

- **BUG-069** — 24 Gunbad `pquest_spawns` rows name prototype 387121, which does not exist.
  Positional evidence points weakly at Blackfang Hatchling (38720) but that creature is not a
  credited target for the objective, so it was not guessed at.
- The `Objective`/`Entry` transposition exists in eleven zones outside Gunbad (49 rows in zone 1,
  21 in zone 8). Not touched here.
- Order-side dwarf NPCs at the Gunbad entrance appear in no capture and look like Return of
  Reckoning additions. Removing them is a content decision, not a level correction; left in place.
- `PointsPerBattlefrontLock` (100) and the eligible-zone list for the expedition race remain
  server defaults with no capture behind them.
- The fourth Land of the Dead travel requirement — a war in the expedition camp grounding
  airships — has no server implementation.
- BUG-058's painted-area work for Gunbad is unchanged.
