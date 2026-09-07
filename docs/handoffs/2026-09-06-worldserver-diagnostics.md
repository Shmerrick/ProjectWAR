# WorldServer diagnostic repairs — 2026-09-06

**Incomplete: BUG-010's five taxi destinations and their access remain broken.** This
pass archived the original rows for a later repair; it did not repair them.
`bin/Release/logs/WorldServer_2026-09-06.log`, final boot starting at line 8004:
13:50:20 loads all 88,727 items; 13:50:25 validates all area influence references;
13:50:35 starts the bot-editor API, reaches console initialization, then shuts down.
No stack processes remain running. Changes are local and uncommitted.

This supersedes this handoff's earlier incomplete delivery and the 05:29 user retest.
The earlier assertion that all 42 capital errors required missing chapter definitions
was wrong: the client explicitly defines peaceful capitals as having no influence.

## Root causes and repairs

### Capital influence data — migration 52

Authoritative extracted client files under `C:/Users/Admin/Downloads/myps/interface/default`:

- `ea_alerttextwindow/source/alerttextwindow.lua:528-533` explicitly says peaceful
  cities have public quests but no influence and intentionally lack influence CSVs.
- `ea_objectivetrackers/source/publicquesttrackerwindow.lua:819-823` suppresses the
  influence bar in peaceful cities while allowing it in contested cities.
- `easystem_utils/source/gamedefs.lua:405-409` identifies peaceful zones 161/162.
- `../interfacecore/maps/zone161/influenceids.csv:1` and `zone162/influenceids.csv:1`
  contain only headers, consistent with those explicit semantics.

Migration 52 changes only the observed capital bindings 280/281 and 282/283 to zero.
All 42 area rows are now correct; all 16 city PQ ChapterId fallbacks were already zero.
Contested/siege maps and character history are unchanged. No chapter, reward or cap
was invented. This is a data correction, not removal of influence validation.

### Shared-track lookup

Client `interface/interfacecore/maps/zone001/influenceids.csv:7-8` and
`zone007/influenceids.csv:7-8` both bind area 1 to tracks 56/47. Shared influence
identifiers therefore do not imply duplicate chapters or duplicate geographical areas.

The old dictionary reported every shared ID and kept whichever row appeared first.
A fortress map-only row could therefore displace an existing complete reward definition.
`ChapterService.BuildInfluenceIndex` now selects an existing populated definition
consistently, breaking equal-quality ties by row key. It never copies or invents
thresholds. Conflicting nonzero thresholds still produce ERROR; missing area references
still produce ERROR. The 30 shared-ID warnings are gone because the lookup now handles
the many-chapters-to-one-track relationship, not because a log level was switched off.
A successful reference audit now reports success rather than WARN for zero failures.

### Invalid routing records — migration 53

Official capture root: `D:/Repos/Shmerrick/WAR-RE-Toolkit/libs/protocolservices/Packet Logs`.
`MECHANIC_orderflymaster_alldestination(missing LoD).txt.gz` contains 27 flight lists
(first F_INTERACT_RESPONSE #14); `MECHANIC_destroflymaster_alldestination.txt.gz` has
28 (first #6). Neither catalog contains zones 62,132,139,168,204. The full Order
union has 28 destinations; Destruction has 29, including 191. These are observations
of those captures, not a claim that all possible future/custom routes are forbidden.

The five inherited rows also have coordinates outside their named zones. Four lack
an owning realm and were already disabled. Rows 132/168 copy the complete Altdorf
162/1 XYZ/orientation; row 62's XY falls in zone 191, not 62. Authentic Altdorf and
LOTD routes already exist. No evidence justifies relabeling these records.

Absence from the inspected captures does not establish that these routes should be
removed, so nothing is deleted or disabled here. Migration 53 only copies the five
originals into `zone_taxis_unresolved` so a later repair keeps the exact coordinates;
live routing is untouched. The coordinate and access problems remain open, and a clean
startup is not evidence that taxi functionality was repaired.

### Item load cost

Both item tables already used compiled mapping; the early suggestion to switch them
from reflection was incorrect and no such attribute change was made. The active
compiled loop nevertheless inspected PropertyInfo/Type for every field of every row.
BindingInfo now caches member type and scalar classification once, removing that
repeated reflection from the millions of field assignments during startup.

SELECT-only checks compared every mapped property and persistence flag on all 88,727
items in both tables with the reflected reference mapper. Final compiled measurements
were 1,699 ms (Mythic) and 1,696 ms (legacy); the final actual startup produced no slow
query warning. The 2,000 ms warning threshold is unchanged; performance under unrelated
machine/database load is not guaranteed.

### Earlier repairs retained

- The 00:42 log at lines 959-998 rejected LOTD's nullable UnlockEndsOnUtc because the
  ORM emitted an empty quoted string. INSERT/UPDATE now write SQL NULL for a `Nullable<T>`
  member with no value, and nullable read types are unwrapped before conversion. Every
  other member keeps the legacy empty-string handling: `DataElement` defaults
  `AllowDbNull` to true, so keying the NULL off that flag would have written NULL into
  the many base-dump columns declared NOT NULL. Empty strings, zero and false remain
  distinct.
- Removed the unused global lockout dictionary and its non-unique zone/reset-key
  assumption. Character progress supplies returning-player boss lists. World lockout
  rows and statistics are preserved; different groups' progress is never merged.

## Verification

- Release/x64 solution Rebuild: no compiler warnings or errors.
- `Test-RuntimeRegressions.ps1`: passed, including reversed shared-track row order,
  placeholder-before-reward selection, unchanged placeholder data, influence packet
  widths/caps, lockout restoration and independent group instance selection.
- `Test-DatabaseNulls.ps1`: strict SQL INSERT/UPDATE round trips against session-local
  tables on the configured Release connection, including a copy of the actual LOTD
  schema, optional scalars and escaping. No persistent test rows.
- Migrations 52 and 53 applied to the configured Release database; both are re-runnable
  and were re-applied to confirm it.
- `Test-WorldDiagnosticData.ps1`: all 42 capital area rows, city PQ fallbacks, all
  original taxi columns and matching archive schema, live routes left intact,
  and zero missing area influence references verified with read-only queries.
- `Test-ItemLoaders.ps1`: both tables, all mapped values and persistence flags match.
- A bounded harness invoked the existing startup/shutdown methods from built
  ServerLauncher.exe, pumped their asynchronous workflow and waited for actual console
  initialization. Services were not launched independently. The final run was outside
  the execution sandbox: a constructor-only probe proved the sandbox reports
  HttpListener.IsSupported=False, while outside it the API starts normally.

Base SQL dumps are untouched. Apply migrations 52 and 53 before running this build on
another installation. This verifies the observed startup diagnostics; it is not a
claim that every gameplay path is complete. BUG-041's missing area/PQ geometry and
other independently tracked in-client restoration work remain open.
