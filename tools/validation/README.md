# Validation tools

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
