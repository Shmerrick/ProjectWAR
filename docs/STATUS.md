# ProjectWAR Status

ProjectWAR is a C# private server emulator for Warhammer Online: Age of Reckoning. It targets .NET 4.8, x64. The solution is `ProjectWAR.sln`.

## Latest stabilization (2026-09-05)

Current handoff: [commit summary and latest retest](handoffs/2026-09-05-commit-handoff.md).
User reports PQs seem fixed, but the Destruction entrance from Chaos Wastes to Bastion
Stair now lands incorrectly (BUG-062). This entrance regression is unresolved. The blue-orb
explanation has been retracted (BUG-063); its identity is unverified. This is a partial
stabilization delivery, not a declaration that the dungeon backlog is fixed.

Latest follow-up: [dungeon retest handoff](handoffs/2026-09-05-dungeon-retest.md).
Migration 35 is applied locally. Dungeon login recovery, saved boss suppression, PQ counter/
completion updates, single chest serialization and Gunbad's enclosing influence area have
new regression coverage. User confirmed Bastion Destruction influence, the left-wing portal,
ward PQ counters and Holmsteinn supply visibility before this pass. Remaining failures are
listed in the handoff; these new changes still require an in-client retest.

Follow-up to the in-client test reports: [PQ/Gunbad handoff](handoffs/2026-09-05-pq-gunbad.md).
Migration 34 is applied locally; supply construction, deferred PQ startup and tracker/jump
packet checks pass. The affected PQs and Gunbad still need an in-client retest. Remaining
data/ability warnings are recorded rather than suppressed.

See [the current handoff](handoffs/2026-09-05-stabilization.md) for evidence and verification.
Migrations 32/33 restore client-verified influence tracks; the earlier dungeon repair used
chapter row keys instead of `InfluenceEntry`. Source: extracted client
`interface/interfacecore/maps/zone160/influenceids.csv:2-3`, `zone060/influenceids.csv:2-3`,
and the seven exact CSV bindings cited by migration 33. Influence arithmetic and packets now
preserve 32-bit totals/costs (live client `WAR.exe` `0x4C5359`, `0x4DDF60`).

Height sampling now caches immutable data and preserves unavailable results; region membership
publishes safe snapshots; dependency staging reads resolved source files. Reproducible checks
and a read-only Release database audit are in `tools/validation/`. The current audit finds
42 area rows with missing influence tracks, not the older 231 count based on the wrong key.
In-client dungeon testing and the documented restoration data gaps remain outstanding.

## Built Projects

All of the following produce binaries under `bin/Debug/` and `bin/Release/`:

| Project | Output |
|---------|--------|
| WorldServer | `WorldServer.exe` — main game server |
| AccountCacher | `AccountCacher.exe` — auth/account service |
| LauncherServer | `LauncherServer.exe` — patch/launcher auth |
| LobbyServer | `LobbyServer.exe` — lobby service |
| Launcher | `Launcher/Launcher.exe` — game launcher client |
| ServerLauncher | `ServerLauncher.exe` — WinForms server control panel |
| LosBuilder | `LosBuilder.exe` — LOS generation and inspection tool |
| ClientDataMatrix | `ClientDataMatrix.exe` — WAR client data analysis tool (GUI + CLI) |

The C++ runtime extension `WarZone64.dll` ships prebuilt in `WorldServer/`.

Build command:
```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\amd64\MSBuild.exe' ProjectWAR.sln /p:Platform=x64 /p:Configuration=Release /t:Build /m /nologo /verbosity:minimal
```

## LOS Generation Status

Native LOS generation is implemented in `LosBuilder`. All four structural parity gaps vs. the shipped `bin/Release/los/*.bin` files are resolved for zone 280:

- Region offsets: match (figleaf.db cell size is 8192 units, shift is `<< 13`)
- Terrain height orientation: match (no X-axis flip)
- Holemap behavior: match (`0x0` when `holemap.pcx` absent)
- Collision geometry: match (invisible NiTriShape nodes inside fixture NIFs only; no separate `_collision.nif` files exist)

Zone 280 triangle hash matches shipped exactly (`0xBAAEA5D12555DEB1`).

Remaining gaps:
- Vertex position precision: ~16-unit max Y error from NIF world-matrix accumulation. Topology is correct.
- Water generation: zone 280 has no `water.xml` in current extracted data; file-size difference vs. shipped is exactly one water chunk (136 bytes).
- Multi-zone coverage: only zone 280 has all required source files in the current extraction.

See `docs/los/occ-re-notes.md` for OCC format documentation and full per-chunk comparison details.

## ClientDataMatrix Tool

`ClientDataMatrix` is a read-only analysis tool that ingests extracted WAR client files and builds a provenance-tracked graph of ability, effect, and identity data. It produces per-ability reports, conflict ledgers, coverage summaries, and operation schema ledgers.

Source files it reads are all under `C:\Users\Admin\Downloads\myps`:
- `data/gamedata/abilities.csv`, `effects.csv`, `pregame_chars.xml`
- `data/strings/english/abilitynames.txt`, `abilitydesc.txt`, `abilityeffect.txt`, `careernames_m.txt`, `careerlines_m.txt`, `racenames_m.txt`
- `data/bin/abilityexport.bin`, `abilitycomponentexport.bin`, `abilityrequirementexport.bin`

Reports are generated at runtime to a local output directory. They are not committed to the repo.

Tool usage: `docs/client-data-matrix-usage.md`.

### Component Field Decode Status (as of commit b673271f, 2026-03-28)

All 18,526 component records across all operation types have been fully decoded. **Unknown = 0, Structural = 0.**

Every field in `abilitycomponentexport.bin` is at Confirmed or Inferred confidence:
- **ExtData Val1–Val4**: universal cross-op decode (ApplicationTarget, ComponentOperation type, ApplicationProfile, LayoutTag)
- **Values[0–7]**: per-operation semantics for all 40+ operation types including unnamed ops (29, 30, 32, 40, 41, 43, 47, 51)
- **Multipliers[0–7]**: scaling percentages per operation
- **FlagsRaw**: CrowdControlTypes bitmasks, bit-field flags, sequential enums per operation
- **Value08**: universal binary flag; **Value15**: universal CC gate (CrowdControlTypes bits, mask 0x8FF)

Confirmed resolves this session:
- `DAMAGE Value[1]` → **`MaxCounter`** (counter cap/tick limit) — from decompiled server `DAMAGE.cs`
- `DAMAGE FlagsRaw` → **`DamageFlag`** enum (NONE=0, UNMITIGATABLE=1) — same source

All 558 requirement rows decoded (Val1=AbilitySourceType, Val2=AbilityOperation, Val3=AbilityCondition, Val4=AbilityLogicOperator — from decompiled client `AbilityExport.cs`).

### Coverage Status (as of commit 6fb7cfa3)

| Status | Count | Notes |
|--------|------:|-------|
| MappedWithRequirements | 390 | Fully mapped with requirement links |
| Mapped | 1,984 | Fully mapped |
| Partial | 11,635 | Have BIN or CSV but missing some pieces |
| StringsOnly | 1,029 | Named in strings but no BIN/CSV/effect/component |
| BlankSlot | 13,963 | Empty sequential string-table slots — excluded from gap count as irrecoverable |
| **Coverage gap total** | **12,664** | Partial + StringsOnly only |

`BlankSlot` was introduced this session to filter the 13,963 empty placeholder IDs from the 29,001-entry string files that were inflating the gap count.

### Identity Domains (as of commit 2a4c1bd7)

- `Race.EntryId`: Confirmed — canonical race string IDs
- `CareerLine.EntryId`: Confirmed — canonical career IDs 0–24, used by `abilityexport.bin` CareerLine field
- `CareerName.EntryId`: Confirmed — **NOT CareerId**; 5 display-context groups × 24 careers (IDs 12–131). Duplicates are expected and structurally explained.

### Remaining Open Work

- **Coverage gaps (12,664)**: inherent data gaps — Partial abilities have broken effect chains or missing CSV rows; StringsOnly have names but no client BIN evidence
- **SERVER_COMMAND field mapping**: field that holds the command code needs reconciliation — BIN analysis says FlagsRaw (8 distinct), Londos DB (`War_AbilityComponentBin.sql`) shows Values[0] (72+ distinct), Flags=0 always; likely a Londos remapping; 72+ command codes with per-code argument patterns now documented
- **Unknown op names**: ops 29, 30, 32, 40, 41, 47, 51 remain unnamed — all server-side only (absent from client `ComponentOP` enum). Op=43 **resolved as MOVEMENT_AUTOATTACK** (commit b673271f; added to `DefinitionCatalog.ComponentOperations`). Full row counts and patterns documented in `docs/data-matrix/overview/path-forward.md`. No further naming possible without decompiled WarServer.dll.

See `docs/data-matrix/overview/path-forward.md` for full roadmap and source-search notes.

## External Data Locations

Verified 2026-09-04. `docs/CROSS_REPO.md` is the canonical map; this table is the subset that
matters for the analysis tooling above.

| Path | Contents |
|------|----------|
| `C:\Users\Admin\Downloads\myps` | Extracted WAR client files (gamedata CSV, strings TXT, BIN exports, zones, assetdb). The root `ClientDataMatrix` and native LOS generation read from |
| `C:\Users\Admin\Videos\Warhammer Online - Age of Reckoning` | Live 1.4.8 client install: `WAR.exe`, the `.myp` archives, `assetdb`, `Interface` |
| `D:\Repos\Shmerrick\WAR-RE-Toolkit` | RE toolkit: decoded findings (`RE_FINDINGS/`), 1,027 official packet captures (`libs/protocolservices/Packet Logs`), extraction and analysis apps |

Paths in earlier revisions of this table are gone and should not be reintroduced:
`C:\Users\Admin\Music\Warhammer` (MYP archives, PacketLogger, reference docs),
`C:\Users\Admin\source\repos\Shmerrick\WAR-RE-Toolkit` (the toolkit moved to `D:\Repos\`), and the
`C:\temp\*` extraction and LOS working areas (`world_extract`, `los`, `los_baseline`, `los_fix1`,
`col_extract`, `art*_extract`, `zone_fill`), which were transient scratch space rather than
durable inputs. Regenerate scratch areas from the extracted root when a task needs them.

## Bot System

A fully integrated, player-like Bot System populates the world with autonomous entities. They participate in RvR and Scenarios with zero network overhead and use player-style character, stat, and item systems. Bot gear is currently template-driven with per-bot override support. Bot movement and pathing are under active rework; the current build uses direct movement fallback rather than the earlier waypoint-guided route path. See `BOT_SYSTEM.md` for full architecture details and GM commands.

### Bot Editor API and Toolkit Integration (2026-04-05)

`WorldServer` now exposes a template-aware local HTTP JSON bot editor surface for external UI tooling. The API serves bot summaries, full bot sheets, slot-valid item search, direct base-template patching, explicit career-template routes, and persisted per-bot override updates. Per-bot edits remain stored in `war_characters.bot_gear_overrides` and can be reapplied live to loaded bots.

`WAR-RE-Toolkit` now has two active consumers for this surface: the embedded `Bot Editor` in `WarToolkitHub` for direct per-bot override work, and the standalone `Bot Template Armory` viewer for the paperdoll workflow, filter cascade, and base-template versus bot-result editing split. This is no longer just a planned integration milestone; it is the live working gear-edit path for the current toolkit. Remaining gap: tactic-slot and mastery-template editing are still not exposed by the API. See `docs/bot-editor-api.md` for the current route contract and toolkit integration details.

## System Guilds

A new automated guild system provides "Forces of Order" and "Forces of Destruction" as starter guilds for all new players. These guilds are automatically maintained at level 40. Players who leave are tracked to prevent re-entry via regular invites. See `docs/SYSTEM_GUILDS.md` for full details.

## Recently Resolved Issues

### Deadlock and Performance Pass (2026-03-30)

- **Region Update Loop**: `RegionMgr.UpdateActors` optimized to use a dense `HashSet<Object>` for active objects. This eliminates the 65,000-slot sparse array scan performed every 50ms, significantly reducing CPU overhead in empty or low-population zones.
- **Networking Deadlocks**: Replaced `lock(this)` with private `_syncLock` objects in `GameClient`, `LobbyClient`, `LauncherClient`, and `CellMgr`. This removes a widespread architectural risk where external callers could inadvertently cause deadlocks by locking on the public client objects.
- **NPC Waypoint System**: Fixed a critical 5-second stall in the waypoint system by removing a `Thread.Sleep(5000)` call from `AIInterface.AddWaypoint`. Waypoint ID generation migrated from synchronous DB `MAX(GUID)` queries to a thread-safe static counter in `WaypointService`.
- **Database Optimization**: Audited and removed redundant `ForceSave()` calls in hot paths (e.g., waypoint creation) to prevent synchronous game-thread stalls.
- **Internal Bug Tracker**: Established `docs/INTERNAL_BUG_TRACKER.md` as the central ledger for tracking known issues, including the Invader ward unlock bug and RvR zone transition crashes.

### Bot Cross-Zone Movement Snap-Back Fixed (2026-03-29)

`MovementInterface.UpdateMove()` was computing zone-pin coordinates with
`_unit.Zone.CalculPin()`. When a bot's interpolated position crossed into a new
zone, the source zone's pin offset was applied to destination-zone coordinates,
producing nonsensical values and snapping bots back to spawn after ~5 seconds
of movement. Fixed by using `destZone.CalculPin()` — the zone that owns the
current interpolated world position.

### Bot Item Equipping Restrictions Added (2026-03-29)

`BotLoadoutManager` lacked two item filters:

- **Realm** — Order bots (careers 1–12) were equipping Destruction-only items
  and vice versa. All item queries now include `Realm == 0 || Realm == realmByte`.
- **MinRenown cap** — RR40 bots were equipping Sovereign/Warpforged gear.
  New `GetMaxRenownForTier()` returns 40/70/80/90/100 for T4 sub-tiers (0 = no
  cap for T1–T3). All item queries filter `MinRenown <= maxRenown`.
- Sorting also corrected: `ThenByDescending(MinRenown)` was actively preferring
  the highest-renown item; replaced with `ThenByDescending(Rarity)`.

### Waypoint-Guided Bot Navigation Added (2026-03-29)

New `BotPathfinder` samples `WaypointService.TableWaypoints` (NPC patrol data
already in memory) to build intermediate waypoint lists between warcamp and
battlefield objectives. `BotBrain.MarchAlongPath()` replaces all direct
`Move(objectivePosition)` calls so bots follow existing NPC road waypoints
instead of walking straight through terrain. Falls back to direct movement
when the zone has no suitable patrol data.



### LOTD Flight Node and Taxi Coordinates Fixed

Zone 191 (Land of the Dead) had two data defects that prevented the flight-master from working:

1. **Unclickable flight node**: `zone_infos.Pairing` was shipped as `100`; the flight packet sends this value directly to the client, which expected `4` (PAIRING_LAND_OF_THE_DEAD). `ZoneService.NormalizeZoneInfoMetadata()` now corrects this at load time. The fix is baked into the current `war_world` dump (included in `Database/war_world.7z`).

2. **Wrong taxi destinations**: The `zone_taxis` rows for zone 191 stored local zone pins instead of world coordinates. `ZoneService.NormalizeTaxiWorldPosition()` now detects and converts pin-format rows at load time. The corrected Order/Destruction world coordinates are baked into the current `war_world` dump.

### Parallel Build Copy Stabilized

`Directory.Build.targets` now uses `DestinationFiles` (explicit per-file output paths) instead of `DestinationFolder`, adds an `Exists(...)` condition guard, and retries three times with a 100 ms delay. This prevents transient "file not found" failures during parallel MSBuild solution builds where shared output files may be momentarily locked or missing.

### T1 RvR Functionality Restored

`LowerTierCampaignManager.OpenActiveBattlefront()` now correctly calls `flag.OpenBattleFront()`. The FSM correctly starts, and flag captures now register as expected.

### Career Vendor Ability Purchase Fixed

The trainer packet handler has been updated to correctly map the 1-based index from the client to the list of unpurchased career abilities sorted by rank and name. Players now purchase the correct ability from vendors.

### System Guild Leader Validation Fixed (2026-03-29)

`CharMgr.LoadGuilds` logged `ERROR Missing Guild Leader Guild Leader 0` on every startup because the early-exit guard (`isSystemGuild && LeaderId == 0 && Members.Count == 0`) only bypassed leader validation when the guild had no members. Once players or bots joined the Forces of Order / Forces of Destruction guilds the guard no longer fired, triggering a spurious error. The condition now skips leader validation for any system guild with `LeaderId == 0` regardless of member count.

### BaseClient Shutdown NullReferenceException Fixed (2026-03-29)

`BaseClient.OnReceiveHandler` threw `System.NullReferenceException` on every clean shutdown. When `CloseConnections()` sets `_socket = null`, a pending async receive callback can fire before the socket reference is cleared, causing `baseClient.Socket.EndReceive(ar)` to dereference null. A null guard (`if (baseClient == null || baseClient.Socket == null) return;`) before the `EndReceive` call eliminates the race.

### rvr_player_contribution AUTO_INCREMENT Missing (2026-03-29)

The `rvr_player_contribution` table was created with `Id int NOT NULL PRIMARY KEY` but without `AUTO_INCREMENT`. The C# model has `[PrimaryKey(AutoIncrement = true)]`, so the ORM always inserts with `Id = 0`. The first insert per session succeeded (creating the Id=0 row), but every subsequent `SavePlayerContribution` call hit `MySqlException: Duplicate entry '0' for key PRIMARY`. Fixed with `ALTER TABLE rvr_player_contribution MODIFY Id int NOT NULL AUTO_INCREMENT`. If the database is re-imported from a dump, rerun this ALTER.

### Bot AI Performance Optimizations (2026-03-29)

`BotBrain` contained several hot-path inefficiencies identified by static analysis:

- **`UpdateScenarioObjective`**: was scanning `scenario.Region.Objects` (all region objects, potentially large) every 2 seconds for every group-anchor bot. Now maintains a `_cachedScenarioObjectives` list populated once per scenario instance, with incremental disposal pruning on subsequent ticks. Full region scan replaced by a tight foreach loop using squared distance (no `Math.Sqrt`).
- **`SelectCombatTarget`**: was making three separate LINQ `OrderBy(p => player.GetDistanceTo(p))` passes over `RangedEnemies`, each invoking `Math.Sqrt` per element. Replaced with a single pass tracking best healer, best ranged DPS, and best fallback simultaneously using inline squared-distance comparisons.
- **`UpdateObjectiveGoal`**: replaced `OrderBy(o => player.GetDistanceTo(o))` with a single-pass squared-distance loop.
- **`CheckForDeadGroupMembers`**: was calling `GetDistanceTo` twice per dead member (once in `Where`, once in `OrderBy`). Replaced with a single-pass loop computing distance once.
- **`GetLowestHealthAlly`**: replaced `Where(m => GetDistanceTo(m) <= range)` (sqrt per member) with `IsWithinRadiusFeet` (squared comparison, no sqrt) as the range pre-filter.


