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

## Second retest pass — 2026-09-05 (evening)

User retest confirmed Gunbad: "Gunbad inf tracker is now showing for the left wing and pqs seem
to be functioning normally." Land of the Dead was still locked, and four further faults were
reported. All are addressed or explained below.

### Land of the Dead travel — the actual cause (BUG-067)

The previous pass fixed `zone_infos.Pairing` to 100 in migration 37, and the running server had
loaded it. It was still locked because of one line in the server's own startup:

```
WARN Zone_Info Normalized zone 191 pairing from 100 to 4 for Land of the Dead flight metadata.
```

`ZoneService.NormalizeZoneInfo` force-sets zone 191's pairing to
`Pairing.PAIRING_LAND_OF_THE_DEAD` on every boot, and that enum member was **4** — the one value
in the gap between `NUM_PAIRINGS` (3) and `ExpansionMapRegion.FIRST` (100) where the client
discards a flight destination outright. The guard was silently reverting the migration.

`PAIRING_LAND_OF_THE_DEAD` is now 100, so the guard and the data agree. Evidence for 100 is
unchanged and is now doubly confirmed: a sweep of every capture found **~100 zone-191 flight
records and all of them carry pairing 0x64**, byte-identical as `00 44 64 0B B8 00 BF 01`.

That sweep also retired the previous pass's reading of the trailing byte as `zoneAvailable`: it
is `01` in every one of those ~100 records, Order and Destruction alike, so it is a constant and
cannot be what greys the destination out. `LotdService.IsTaxiAvailable` still drives it and
`F_FLIGHT` still re-checks, so travel remains gated server-side, but the client-side mechanism
that greys zone 191 is not this byte and is still unidentified.

`ForceUnlock` no longer stages the win as a 30-minute pause. The pause is what the client
describes as "airships cannot safely land there at the moment", so `.lotd unlock` was handing
the tester the one state in which travel should fail. It now grants the settled state — holder
set, race running — which is also the state the captures spend most of their time in.
`.lotd status` additionally prints the exact flight record the packet will carry.

### The Squig Nursery (BUG-070, BUG-071) — migration 38

Three of its five objectives were unfinishable.

- Prototypes **100515** and **100516** did not exist in `gameobject_protos`, so "Break Nursery
  Slime" and the Foul Mouf da 'ungry trigger spawned nothing; the session log carries 19 x
  "missing gameobject prototype 100515" and one for 100516. Both were identified by position,
  not by name: converting each spawn into the capture's client frame and taking the nearest
  `F_CREATE_STATIC` sighting gives "Nursery Slime" (DisplayID 166) for all 19 rows of 100515 and
  "Writhing Effigy" (DisplayID 148) for the single 100516 row, every one at 11 units. Both carry
  `Unk3 = 100`, matching Holmsteinn Supplies (551) and Monastery Door (59), the only other
  prototype here used by a `QUEST_KILL_GO` objective.
- "Monstrous Squigs" needs 50 kills and credited 16 of the 30 creatures **it spawns for itself**;
  the 14 Deathspewin' Squig (38630) rows in its own spawn set counted for nothing. 38630 is now
  its `ObjectId3`.

After migration 38 all five objectives credit every one of their spawns.

### A systemic finding (BUG-072)

The two missing Gunbad prototypes are not isolated: **about 180 distinct `gameobject_protos`
entries referenced by `pquest_spawns` do not exist**, across nearly every zone, including 846
rows for entry 98827 in zone 106 and 658 for 100530. Every affected public-quest object stage
spawns nothing. Only the two Gunbad prototypes are fixed here; the rest need the same positional
identification against captures, zone by zone, and that is deliberately not attempted in bulk.

### Mutant Exiles (BUG-074) — not a server fault

Measured rather than assumed. The PQ's pin resolves through `pqarea002.png` to PQ area **8**,
exactly matching its `PQAreaId`, so area detection is correct. Through `areas002.png` it resolves
to area piece **2, "Black Tar Canyon" (AreaId 64)** — and both `zone_areas` and the client's own
`deps/zones/zone002/influenceids.csv` bind area 64 to Order influence 36 **only**. There is no
Destruction influence track there, so no chapter bar for a Destruction player is correct data.

Only the northern 1,493 of the PQ area's 10,129 pixels overlap piece 1, "Da Scrub" (AreaId 63),
which does carry Destruction influence 12 — which is exactly why influence appeared after moving
north. The PQ's own `ChapterId` is 12, so it awards Da Scrub's track from an area that never
displays it. Whether retail painted Mutant Exiles into Da Scrub is unresolved and was not
guessed at. The missing PQ progress tracker is a separate symptom and was not reproduced.

### Portal 629156888 (BUG-073)

No `zone_jumps` row exists for it. A sweep of every capture found 88 distinct client jump ids and
this is not among them, though the four Gunbad boss portals (62915688 / 62915752 / 62915816 /
62915880) all are. Its destination cannot be derived without inventing one, so no row was added.
`F_ZONEJUMP` now logs the zone and pin coordinates alongside an unknown id, because a jump id is
an opaque client object id that cannot otherwise be traced back to a place in the world.

### Validation

Release/x64 solution build clean, no new warnings. Migration 38 applied and re-applied against
the local Release database; base dumps untouched. `Test-RuntimeRegressions.ps1` passes. **Not
client-tested** — the stack was not started.

## Warcamp assault — user-supplied mechanism, 2026-09-05

The user reported the retail behaviour: when the Land of the Dead expedition changes hands, the
winning realm attacks the losing realm's expedition camp, starting a raze/defend public quest on
both sides, and suggested that quest might be what locks travel.

The mechanism is real and both quests are already in the database — `pquest_info` 850 "Assault on
Goldbarrow" (Type 2, the Order camp) and 851 "Assault on Da Dusty Dry" (Type 1, the Destruction
camp) — and two captures record them:

- `... PQ ASSAULT ON DA DUSTY DRY - DEFEND THE WARCAMP`: race **paused** (timer 27 -> 18), **Order**
  holding, both realms on 0/500. The flip-triggered case, with the Destruction player defending
  their own camp.
- `... PQ ASSAULT ON GOLDBARROW - RAZE THE WARCAMP`: race **running** (timer 0), **Destruction**
  holding, Order on 366/500. So the assault is not exclusively a flip event.

This is very likely the mechanism behind the client's fourth travel-lock reason, "airships cannot
safely land there at the moment". It is recorded as BUG-075.

**It is not what locked travel on this server.** Both quests are inert: `PQAreaId` 0, pin 0,0 and
zero `pquest_spawns` rows, so `Player.CheckArea` — which only matches painted areas 1-28 — can
never attach a player to them and there is nothing to spawn. Nothing in the server starts them.
The observed lock is fully accounted for by `ZoneService.NormalizeZoneInfo` rewriting zone 191's
pairing from 100 back to 4 at every boot, which the session log records verbatim and which the
`PAIRING_LAND_OF_THE_DEAD` enum change fixes.

The report did surface one thing worth acting on later. The defend capture has Order holding the
expedition while a **Destruction** player is inside zone 191 defending Da Dusty Dry, and a quest
that exists for the losing realm to defend its own camp implies that realm can reach the zone —
which `CanRealmAccessLotd`, holder-exclusive as written, would forbid. That is not conclusive (the
capture starts mid-session, and no capture yet pairs a flight list with a tracker naming the other
realm as holder), so the rule was left alone and the question recorded as BUG-076.

## Third retest pass — 2026-09-05, in Land of the Dead

Travel now works: the user flew from the Inevitable City to Land of the Dead, which confirms the
`PAIRING_LAND_OF_THE_DEAD` fix. Nine further faults were reported from inside the zone.

### Fixed

**Swapped warcamp taxis (BUG-077, migration 39).** A Destruction player taking the expedition
flight landed in the *Order* warcamp and was killed; an Order player landed in the *Destruction*
warcamp. Death respawn was correct for both, which is the clue: `zone_respawns` is right and
`zone_taxis` is not. Converting the respawn pins to world coordinates (zone 191 OffX 48 / OffY
364) lines the two sets up with the Z values matching to the unit:

```
respawn 274, realm 2 Destruction -> 254002, 1497939, 10328
respawn 275, realm 1 Order       -> 257638, 1536364, 10248
taxi RealmID 1 (Order)           -> 254486, 1498271, 10328   <- Destruction warcamp
taxi RealmID 2 (Destruction)     -> 257648, 1536559, 10248   <- Order warcamp
```

Ownership is settled by capture rather than by the respawn rows alone: in
`MECHANIC_orderflymaster_NecropoleOFZandri(LoD)` an Order player takes this exact flight,
`F_SWITCH_REGION` #38 carries zone 0x00BF = 191, and `S_PLAYER_INITTED` #101 places the arrival at
world 257326, 1536497 — the Order warcamp, and the point the database was giving Destruction.

**Sedjhet Temple jars (BUG-079, migration 39).** Objective 2404 places 8 objects of gameobject
prototype 98962, which did not exist — an instance of the ~180 missing prototypes in BUG-072.
Identified positionally from the Sedjhet Temple capture as **"Hieratic Jar", DisplayID 7869,
Unk3 100**: 10 sightings, one spawn row matching a sighting exactly and the rest 306-328 units off
across quest cycles, with the name matching the objective verbatim. Zone 191 is not an instance,
so capture coordinates are world coordinates with no atlas shift.

**Instance player cap (BUG-080).** `InstanceMgr._maxplayers` was a mutable field, initialised to
6 then set to 24 for a raid or 0 for a realm instance and never reset. The first Gunbad or Bastion
Stair entry left every later group instance on the wrong cap for the rest of the server's life,
including the branch that turns a joining group member away with "This instance is already full."
Capacity is now a per-entry local passed into `Join_Instance`. This is a plausible contributor to
the reported inaccessible Tomb of the Vulture Lord portal, but is not confirmed as its cause —
`zone_jumps` row 200797160 exists and is correct (zone 179, Type 6, InstanceID 179), and
`instance_infos` is keyed by ZoneID so it resolves.

### The largest finding — Land of the Dead has almost no public-quest data (BUG-078)

Of the 28 realm-paired PQ rows in zone 191, **26 have zero `pquest_spawns` rows**, and **13 of the
14 Order-side rows (886-899) have zero objectives at all**. Only Obelisk of Judgement (558/887)
and Sedjhet Temple (556/886) carry anything. That is the direct explanation for "no PQ trackers
show on screen" in Land of the Dead, and for it being worse on Order. `pqarea191.png` is present
and correct, so this is not an area-detection fault.

Restoring it means rebuilding objectives, stages, counts and spawn sets per quest from the eleven
Land of the Dead captures. Not attempted here, and it is a substantial piece of work.

### Reported, recorded, not fixed

- **BUG-081** PQ reward chests cannot be looted and go straight to the mailbox. The chest is
  created correctly (log: "Created reward chest for public quest 556 (Sedjhet Temple) in region 9
  zone 191"), so the fault is in the award path, not chest creation. Not diagnosed.
- **BUG-082** Destruction talisman vendors sell none of the decaying gems. No capture of these
  vendors located.
- **BUG-083** The four Tombs (241-244) can be entered without holding or consuming PQ gifts;
  `instance_infos` carries no gate for them and the requirement appears never to have been built.
- **BUG-084** Order PQs 594 "Colossus of the Vulture Lord" and 893 "Temple of Ualatp" share
  `PQAreaId` 7, so which one an Order player attaches to is arbitrary.
- **BUG-075** The warcamp siege not triggering after Order took the zone is the already-recorded
  unimplemented assault mechanic, not a regression.

The flight to Land of the Dead passing through character select before loading is the existing
BUG-059 cross-region symptom and was not investigated in this pass.

## Fourth pass — dungeon entry, Sigmar Crypts and difficulty design (2026-09-05)

Continues work started by another agent that ran out of budget mid-task. Its `InstanceMgr`
change, `DUNGEON_DIFFICULTY.md`, `CREATURE_LEVEL_SCALING.md`, ward-system additions and
`Get-DungeonReadiness.ps1` were reviewed and kept; its `Database/40_restore_sigmar_crypts_lesser_ward.sql`
was discarded.

### Gunbad boss encounters (BUG-087)

None of the four could be entered, by either realm. `instance_infos` gives zones 60, 63, 64, 65
and 66 the same `Entry` of 60 — Gunbad is the **only** dungeon in that table whose Entry spans
more than one zone; Bastion Stair's boss maps carry distinct entries 163-166. `ZoneIn` selected a
group instance on `Info.Entry` alone, so a Type 6 boss jump matched the zone-60 realm instance
the player was standing in, and `Join_Instance` teleported them to the boss zone's coordinates
inside a region containing no such zone.

Selection now also requires the instance's `ZoneID` to equal the destination's and its `Realm` to
be 0, so a realm copy can never satisfy a group jump. No data change is needed:
`Instance.LoadBossSpawns` already filters by ZoneID, so the shared Entry is harmless once
selection is right, and every other dungeon is one zone per Entry so nothing else changes.

### Tomb of the Vulture Lord (BUG-088, migration 41)

The portal reported inaccessible in the previous pass has a cause. `TOTVL.createPenulums` looked
up gameobject prototypes 98908, 100489 and 100490 and **discarded the `TryGetValue` result**,
and every trap constructor dereferences `proto.Name`. All three were absent, so construction
threw out of the TOTVL constructor and the instance was never added; the log records the
NullReferenceException at 17:52:20 and 18:00:25.

The three were identified from the twelve official Tomb of the Vulture Lord captures (2,232
`F_CREATE_STATIC` frames) by three independent routes: the only trap names present are
**Pendulum**, **Fire Trap** and **Dart Trap**; two of their DisplayIDs are already hardcoded in
`TOTVL.cs` and match the capture exactly (7394 and 7471); and the coordinates hardcoded in
`createPenulums` land within **2, 7 and 5 units** of same-name sightings once the zone-179
`OffX/OffY 72/60` and `(1,25)` atlas shift are applied. All three carry `Unk3 = 100`. The lookups
are now checked, so a future gap costs the trap hall rather than the dungeon.

### Sigmar Crypts Lesser Ward (BUG-089) — not a bug

The user reported it missing and then retracted ("I don't see it in old videos"). Both are
consistent with the data. All 143 `instance_creature_spawns` rows and all 8 boss rows in zone 176
already carry `Ward = 1`. The ward reaches the client through `Creature.SendWardInfo` over
`F_WARD_INFO`, and that method's own comment records the stock 1.4.8 client routing the opcode to
its no-op dispatcher case. Creature wards are a ProjectWAR extension visible only with the
private ward-sigil client component, so they would never appear in retail footage.

The discarded migration would have set `Ward = 1` on rows in zone 176 that already had it, and on
138 `creature_spawns` rows that are themselves partly duplicates (BUG-090). Its authority was a
2009 blog post, which does not meet the client-and-capture standard in `CLAUDE.md`.

While checking this, zone 176 turned out to double-spawn: it has `Region = 176`, so cell loading
spawns its 138 `creature_spawns` rows alongside the 143 instance rows, with **45 exact
Entry+WorldX+WorldY duplicates** — the same overlap migration 24 removed for Bastion Stair.
Recorded as BUG-090 and not fixed, because which table holds the authentic population has to be
established per row first.

### Difficulty modes

`DUNGEON_DIFFICULTY.md` now carries the rank-40+ rule the user specified: Hard is existing rank
+1 and existing ward tier +1, Nightmare a further +1 each. It is measured against the nine
dungeons whose stored levels actually reach 40, and records four decisions still needed — rows
storing level 0 or no level at all (the four Tombs) have nothing to add to and need their
effective level resolved first; scaling must be scoped to encounter creatures rather than every
row in a zone; a +1 ward from a stored 0 would give five currently ward-free dungeons a Lesser
ward; and the rule needs a ceiling above Supreme. Nothing is implemented.

### Validation

`WorldServer.csproj` compiles clean with no CS warnings; the full solution build could not run
because the running server stack held `bin/Release`. Migration 41 applied and re-applied against the
configured Release database; base dumps untouched. No server started, no in-client test.
