# Stabilization and influence corrections — 2026-09-05

This supersedes the influence diagnosis in `2026-09-05-checkpoint.md`, BUG-037 and BUG-038.
Work is on `RESTART`; changes are uncommitted. The toolkit and client were read as evidence;
all source and SQL changes in this pass are in ProjectWAR.

## The checkpoint had the influence key backwards

`ChapterService.GetChapterEntry` resolves **`chapter_infos.InfluenceEntry`**, not `Entry`.
Migration 23 changed correct client track IDs into unrelated database row IDs. The Release
database before this pass had Bastion area tracks 6/2 and Gunbad 5/1. Track 2 resolves to
Skarzag's Warcamp; 5/6 resolve to other Greenskin chapters; track 1 has no chapter.
Observing a bar increase therefore did not establish that dungeon influence was fixed.

The authoritative client records are in
`C:\Users\Admin\Downloads\myps\interface\interfacecore\maps`:

| Client file, lines | Zone | Order track | Destruction track | Chapter row keys (Order / Destruction) |
| --- | ---: | ---: | ---: | --- |
| `zone160/influenceids.csv:2-3` | 160 | 129 | 128 | 6 / 2 |
| `zone060/influenceids.csv:2-3` | 60 | 64 | 65 | 5 / 1 |

`Database/32_restore_client_dungeon_influence_tracks.sql` restores these four area bindings
(across four area rows) and the 19 PQ fallback IDs. Boss maps 163-166 retain migration 30's
zero influence. Character history is untouched: `character_influences` does not identify
the source of each award, so old miscredited points cannot be safely moved automatically.

`Database/33_restore_verified_area_influence_tracks.sql` repairs seven more populated
owning-realm bindings in zones 11, 101, 107, 120 and 209. Each statement cites the exact
client CSV line and guards the old value. No names, ranks or adjacent rows supply values.

## Influence overflow

The two Bastion chapter rows have a 75,150-point cap. `Player.AddInfluence` cast that cap to
`ushort` (9,614), while both update packets and reward costs also discarded the upper bits.
The persistence entity already stores a `uint`.

Direct inspection of the live client
`C:\Users\Admin\Videos\Warhammer Online - Age of Reckoning\WAR.exe` establishes the widths:

- `F_INFLUENCE_UPDATE`: dispatcher block `0x4C533C`, especially `0x4C5359` and `0x4C5368`,
  reads and byte-swaps a **32-bit value at payload offset 4**.
- `F_INFLUENCE_DETAILS`: handler `0x4DDF1A`, tier loop `0x4DDF60`, calls stream reader
  `0x91EDA9` for each reward cost. That reader requests four bytes at `0x91EDAC` and combines
  them in big-endian order.
- The toolkit's corresponding `libs/protocolservices/Server Packet Protocol/F_INFLUENCE_*.cs`
  are empty parser stubs; its `war_ghidra_all.c` also fails to decompile these handlers.
  These findings come from the executable instructions, not those stubs.

Updates and reward costs now preserve all 32 bits without changing packet sizes. Addition
uses a wider intermediate before clamping. `.modify influence` accepts values above 65,535
without wrapping and rejects negative values. Existing per-kill tuning is unchanged.

## Runtime and build repairs

- `ClientFileMgr`: height lookup no longer clones two entire GDI+ bitmaps per call without
  disposing them. A thread-safe lazy cache publishes one immutable height raster; source images
  are disposed after loading. Negative/out-of-range pins and mismatched raster sizes fail
  safely. The unavailable result stays `-1` instead of becoming zero through integer division.
  The existing height arithmetic is preserved; this is not a claim of new terrain parity.
- `ClientZoneInfo`: missing maps remain absent instead of allocating two empty megabyte grids
  and accidentally resolving `PieceId = 0`. Wrong-size/corrupt maps fail atomically; one bad
  overlay cannot suppress the other. Missing paths are logged once during lazy zone loading.
- `RegionMgr`: public membership is an immutable snapshot. Only the owning region thread
  mutates the private list; other threads can enumerate a stable snapshot. Realm counts now
  decrement on departures, and duplicate joins/removals do not corrupt them. This protects
  membership, not arbitrary mutable state inside each `Player`.
- `ChapterService`: influence lookup uses an index instead of allocating an unused list and
  scanning chapters on each award. Shared track IDs preserve the previous first-match choice
  and are reported at startup. Missing area-to-influence references are diagnosed after all
  immediate data loaders finish, without per-award logging.
- `Directory.Build.targets`: the baseline parallel build reproduced `MSB3030` for
  `System.Threading.Tasks.Extensions.dll`. Dependency staging now copies resolved reference
  inputs and DLL content, preserving destination subdirectories, instead of globbing the shared
  output folder while other projects write it. Required copy failures are errors, not warnings.

## Measured remaining work

`tools/validation/Get-WorldDataHealth.ps1` reads the database and zone path configured in the
local Release `World.xml`; it uses SELECTs only. After migrations 32/33:

- **42 of 419 area rows** reference a missing nonzero `InfluenceEntry`. The old **231** figure
  tested the wrong key. Replacement chapter definitions remain unpopulated.
- **261 configured zones**, **221 missing area maps**. Of **52 zones with PQ definitions**,
  **12 lack PQ maps**, affecting **37 definitions** (including metadata/placeholder PQs).
  This measures database-configured zones, unlike the checkpoint's count of 219 disk folders.
- All **32 ward counters** have nonzero Tome bindings.
- Optional client comparison finds no remaining populated owning-realm mismatch against a
  **nonzero** client CSV track. It still reports 22 differences where client CSVs specify
  zero (zones 179, 191, 241-244). Whether those are dynamic server assignments requires packet
  evidence before removing them. NULL opposite-realm bindings are deliberately excluded.

BUG-041 remains open. The map rectangles are not sufficient evidence for replacing area
boundaries: client `interface/interfacecore/maps/zone009/mappieces.csv`, rows 2-12, overlaps
multiple rectangles, including a whole-map 1024x1024 piece. They do not define a unique spatial
partition. Do not turn the previous checkpoint's suggested rectangle fallback into invented
area or PQ geometry. The existing approximate Bastion overlays remain unchanged.

The prior evidence gaps also remain: Order Bastion entrance coordinates, missing Bloodlord
sets/loot, missing ward targets/events, and the unresolved TOVL invasion model. No gameplay
claim in the earlier checkpoint removes the need for correct identifiers and authoritative data.

## Verification and reproduction

- Migrations **32 and 33 applied twice** to the Release world database, then queried and
  compared with the client CSVs. All four dungeon area rows resolve to chapters in the same
  zone through `InfluenceEntry`; all 19 PQ fallbacks match; boss maps stay at zero.
- `tools/validation/Test-RuntimeRegressions.ps1` compiles a standalone net48 harness against
  the built server. It covers concurrent first height loads, bounds and missing-data sentinels,
  image-handle disposal, independent overlay failure, stable region enumeration under concurrent
  membership changes, counts, influence lookup identity, 65,535 crossing, 75,150 clamping,
  uint overflow protection, and actual update/reward packet bytes.
- Full Release/x64 solution **Rebuild passed with no reported warnings or errors** after
  the final code edits; the compiled-code regression suite passed again against that output.
  Staged `WarZone64.dll` and `System.Threading.Tasks.Extensions.dll` match their source hashes.
  These checks do **not** establish an in-client dungeon run; use ServerLauncher for the
  subsequent game test. No server stack was left running by this pass.

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\amd64\MSBuild.exe' ProjectWAR.sln /p:Configuration=Release /p:Platform=x64 /t:Rebuild /m /nologo /verbosity:minimal
./tools/validation/Test-RuntimeRegressions.ps1
./tools/validation/Get-WorldDataHealth.ps1 -ExtractedRoot 'C:\Users\Admin\Downloads\myps' | Format-List
```

Review coverage: repository rules, README, Claude's architecture notes and checkpoints, system,
bot/API, ward/dungeon, LOS, status and bug docs; data-matrix overview/usage and targeted ledger
searches; toolkit index/checkpoint, world/protocol findings, client architecture and relevant
tool references; live client instructions and extracted map CSVs. Generated multi-megabyte
ledgers and the complete packet/client corpus were searched selectively, not read exhaustively.
