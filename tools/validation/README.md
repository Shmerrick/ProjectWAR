# Validation tools

`./tools/validation/Test-WorldDiagnosticData.ps1` verifies migrations 52 and 53 against
the Release database using read-only queries: no missing area influence references,
42 peaceful-city areas without influence, zero city PQ fallbacks, all eight original
columns of the five archived taxi records, and that live routing still matches them
verbatim. This does not establish that the taxi destinations work.

`./tools/validation/Test-ItemLoaders.ps1` compares the reflected reference mapper with
the active compiled mapper using both Release item tables. It measures elapsed load
time and verifies every mapped property and persistence flag on all 88,727 items in
each table. It is SELECT-only and starts no ORM save thread. The slower reference
mapper may emit an expected slow-query warning in the test's own log.

`./tools/validation/Test-DatabaseNulls.ps1` tests the built MySQL writer against
session-local temporary tables in the configured Release database, including a copy
of the actual `lotd_resource_tracker` schema. Strict-mode INSERT/UPDATE checks cover
NULL and populated dates, nullable reads, optional numbers/booleans, empty strings,
required-string compatibility and escaping. It registers no ORM tables, starts no
services and changes no persistent rows. See the
[WorldServer diagnostic handoff](../../docs/handoffs/2026-09-06-worldserver-diagnostics.md).

`./tools/validation/Test-TomeTactics.ps1` verifies the bestiary kill counter repair
(migration 54) and the tome tactic data set (migrations 55-57) against the Release
database. It resolves each new `DataObject` through the real ORM binder and selects
exactly the bound columns, so a property with no column — or a NULL behind a
non-nullable value type, which is the bug that killed every bestiary counter — fails
here rather than as a `LoadingFunction` exception at boot. It then checks the data
against the 1.4.8 client files it came from: 136 distinct species counters with no
collisions, nine tactic lines with ascending thresholds, 138 fragments with the
client's per-line totals, every line's final tier reachable, 31 unambiguous creature
type bindings, and the 27 ability, Tome and buff rows. Included are the values read
off the live client's own Greenskin fragment tooltip (2 / 3 / 5). SELECT-only.

For the normal Gunbad/Bastion completion gate and planned custom difficulty work:

```powershell
./tools/validation/Get-DungeonReadiness.ps1 | ConvertTo-Json -Depth 5
```

This SELECT-only Release audit reports PQ coverage, missing prototypes, unattached/cross-zone
spawns, objectives without their own spawns, instance exits and stored level/ward ranges across
both dungeons and their boss maps. Ranges include friendly and utility NPCs; they are not
authoritative difficulty baselines. Empty objectives may have scripted sources. See
[difficulty requirements and retest gate](../../docs/DUNGEON_DIFFICULTY.md).

Build the Release/x64 solution before running:

```powershell
./tools/validation/Test-RuntimeRegressions.ps1
./tools/validation/Get-WorldDataHealth.ps1 | Format-List
```

Both scripts accept `-BuildRoot` (default `bin/Release`). The regression script compiles a
standalone .NET Framework executable beside the server and tests the actual built code using
temporary image fixtures and inert players. It starts no services and writes no database rows.
Expected missing/corrupt-fixture diagnostics appear before the final PASS line. Its uniquely
named temporary fixture directory is removed even if a check fails.

Coverage: height values/bounds, missing data, concurrent first loads and image disposal;
independent area/PQ map failures; immutable region membership under concurrent enumeration;
realm counts; influence key identity, overflow/capping and update/reward packet bytes.

The suite also checks Gunbad tracker packets against `INSTANCE_GUNBAD_PART1` packets 335/381,
realm-specific PQ influence, untimed-stage timers, instance jump coordinate conversion,
zero optional respawn-zone semantics, invalid flight input and deferred PQ startup.

Dungeon retest coverage adds the eight Gunbad painted pieces retaining area 31, real PQ
counter/completion packets, late-event rejection, jump-zero rejection, configured exit vs
realm-capital recovery, saved/expired/malformed lockout parsing and empty boss bonus cleanup.
The completion fixture deliberately has no live region, so its chest-creation diagnostic is
expected. It does not exercise a live lockout persistence transaction or client rendering.

`Get-GunbadLevelEvidence.ps1` reads Release instance spawns and both official Gunbad captures,
proposing only exact XYZ/name/model matches with one observed level. It changes no data;
the 2026-09-05 run found no matching placements. Broader identity/layout restoration remains
open, not permission to subtract a fixed level offset from the RoR data.

```powershell
./tools/validation/Test-PublicQuestData.ps1
```

This SELECT-only check uses the Release database to construct the actual first-stage objects
for Holmsteinn Revisited and Destruction of the Weak. It checks deferred startup and duplicate
prevention, with no region thread, AI tick or character writes. It does not test client visibility.

```powershell
./tools/validation/Test-ThanquolEncounter.ps1
```

This SELECT-only check verifies that Thanquol's Incursion (public quest 911, zone 410) still
matches the three official full-run captures it was decoded from: the Setup stage plus five
numbered stages in order, their tracker titles, objective texts, counts, object ids and
timers, all four Siphoning Contraption positions on both contraption stages, the 100517
prototype, and that Skeetk, Throt and Thanquol are spawned in the zone. It drives the real
`PublicQuest` constructor but starts no region, AI or networking, and is not an in-client test
of the encounter. See the [handoff](../../docs/handoffs/2026-09-07-four-systems.md).

`Read-OfficialPackets.ps1 -CapturePath <gzip log> -OpcodePattern <regex>` decodes the toolkit's
text capture format without writing files. `Index` is the 1-based ordinal across both directions;
`Bytes` includes the frame header (three bytes for server packets). See the
[PQ/Gunbad handoff](../../docs/handoffs/2026-09-05-pq-gunbad.md) for exact evidence references.

The health script loads connection settings and the zone folder from the selected build's
`Configs/World.xml` and executes SELECT queries. Credentials are never printed. It reports
missing influence references, shared tracks, caps above 65,535, missing zone/PQ maps and ward
bindings. To compare populated owning-realm bindings with available client CSV evidence:

```powershell
./tools/validation/Get-WorldDataHealth.ps1 -ExtractedRoot 'C:\Users\Admin\Downloads\myps' | Format-List
```

This comparison matches exact `(ZoneId, AreaId, Realm)` keys. NULL bindings and CSV keys with
multiple rows are excluded. Reported client-zero differences require further packet research;
the script never changes them. Source details and current measurements are in
[`docs/handoffs/2026-09-05-stabilization.md`](../../docs/handoffs/2026-09-05-stabilization.md).

Passing these tools is not an end-to-end game test. After migrations/build changes, use
ServerLauncher to start the stack and retest the affected gameplay in the client.

`Read-TaxiCaptureEvidence.ps1` scans the official gzip packet corpus for BUG-010's
five destination IDs in flight menus and for candidate travel arrivals. It writes
`ProjectWAR-taxi-capture-evidence.csv` to the temporary directory; use `-OutputPath`
to choose another report location. It never connects to the database. Findings and
the limits of interpreting scenario/siege coordinates as flight arrivals are recorded
in [the taxi evidence handoff](../../docs/handoffs/2026-09-06-taxi-evidence.md).

The September 6 review adds ordinary-creature PQ checks to the runtime suite: subtype-zero
targets, multiple contributors to one quest, independent realm quests, unmatched/cross-zone
quests, zero damage and exclusion of PQ-owned creatures. Lockout fixtures now use Gunbad
boss-map destinations 63-66 with shared InstanceID 60, matching the Release zone_jumps rows.

After applying `Database/51_archive_deleted_bastion_creature_placements.sql`, run:

```powershell
./tools/validation/Test-ArchiveRecovery.ps1
```

This SELECT-only check verifies all 15 original columns of the 24 archived creature records,
their absence from the live spawn table, and the archive schema. It exercises migration 47's
corrected empty-objective count with derived-table fixtures containing an affected empty
objective, an affected populated objective and an unrelated empty objective. The archival
source is the untouched `Database/war_world.7z` creature_spawns records, reproduced in migration
51; these are preserved emulator data, not newly established retail placements.
