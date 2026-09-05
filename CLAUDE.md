# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

ProjectWAR is a private-server emulator for *Warhammer Online: Age of Reckoning*, targeting the final official patch (1.4.8). C# on **.NET Framework 4.8, x64 only**, MSBuild + `packages.config` NuGet (not SDK-style, not `dotnet build`).

`AGENTS.md` is the repository's stated single source of truth for AI-agent rules — read it. Its key constraints are summarized under [Hard rules](#hard-rules) below.

## Build & run

```powershell
# Full solution (Release x64). Adjust the MSBuild path to your VS install.
& 'C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\amd64\MSBuild.exe' ProjectWAR.sln /p:Platform=x64 /p:Configuration=Release /t:Build /m /nologo /verbosity:minimal

# Single project (dependencies still build via project references)
msbuild WorldServer\WorldServer.csproj /p:Platform=x64 /p:Configuration=Release
```

All projects output to the shared `bin/$(Configuration)/`. `Directory.Build.targets` then relocates runtime DLLs into `bin/$(Configuration)/libs/` for every EXE, keeping `Common.dll` and `FrameWork.dll` beside the executables; `WorldServer` calls `SetDllDirectory("libs")` at startup so `DllImport` still resolves natives (e.g. `WarZone64.dll`).

Targeted automated checks now live in `tools/validation/`: run `Test-RuntimeRegressions.ps1`
after building for height/overlay, region-membership and influence-packet regressions.
`Get-WorldDataHealth.ps1` audits the configured Release database with SELECTs only. These do
not replace in-client testing. Compile with no new warnings (`AGENTS.md` rule 9).
If restore fails, restore NuGet packages in Visual Studio or run `nuget restore ProjectWAR.sln`.

Running the stack: always launch `bin/Release/ServerLauncher.exe` and use "Start All". The four services (`AccountCacher` -> `LauncherServer` -> `LobbyServer` -> `WorldServer`) have startup-order dependencies and race if started individually.

Kill a stuck stack:

```powershell
Get-Process | Where-Object { $_.Name -match 'AccountCacher|LauncherServer|LobbyServer|WorldServer|ServerLauncher' } | Stop-Process -Force
```

### Ports, and why startups fail intermittently

This machine's dynamic TCP range is **1024–65534** (Windows default is 49152+), so any outbound connection can occupy a fixed emulator port. This has bitten twice: `msedgewebview2` took 10300, and Unreal Engine's MCP server took 8000.

The 8000 case is the nasty one. Unreal binds `127.0.0.1:8000` while LauncherServer binds `0.0.0.0:8000` — **both succeed**, and Windows routes loopback connections to the more specific binding. The launcher then talks HTTP to Unreal instead of the raw-TCP patch service, ServerLauncher shows everything green, and LauncherServer's log is empty. Diagnose by comparing `127.0.0.1:8000` against the LAN IP: LauncherServer correctly *times out* on an HTTP request (it does not speak HTTP), while an instant HTTP 404 means something else holds loopback.

6000-6010, 6800, **8000**, 8048 and 10300 are reserved via `netsh int ipv4 add excludedportrange`. **Do not reserve 51932-51933** — exclusions block `HttpListener` (HTTP.SYS) even though raw sockets bind fine, which stops the bot editor API. 8000 was added on 2026-09-04 after LauncherServer failed to bind it (`SocketException: Only one usage of each socket address`, then `Can not start server on port : 8000`) with nothing else holding it by the time the stack was inspected — a transient dynamic-port grab, which is exactly what the exclusion prevents. LauncherServer binds it with a raw `TcpListener`, not `HttpListener`, so the HTTP.SYS caveat above does not apply to it.

Ports in play: 6800 AccountCacher RPC, 8000 LauncherServer, 8048 LobbyServer, 10300 world (from `war_accounts.realms.Port`), 51932/51933 the debug and bot-editor APIs, 6000+ RPC client ports.

### Database performance

Gameplay is served from in-memory caches — after boot, essentially the only recurring query is the character load. That makes the per-login lookups the scaling path, and four of them were unindexed full table scans until `Database/03_add_hot_path_indexes.sql`. If you add a per-character or per-account query, check `EXPLAIN` shows `type: ref` rather than `type: ALL`.

The ~2.1s "Slow SQL" warning at boot is the `Item_Info` load (88,727 rows) and is expected; it does not scale with players. The save pump batches every dirty object into a single unbounded transaction every 60s — fine at current scale, worth chunking if population grows a lot.

### Logging and shutdown

All four services load the same `bin/$(Configuration)/NLog.config` (copied from `WorldServer/NLog.config`), which is why every file name carries `${processname}`. Two things about it are deliberate and worth not undoing:

- The catch-all rules run at **Info**, not Trace. At Trace the world log reached 34 MB / 275,011 lines in a day with one player, mostly per-swing stat maths. `autoReload="true"` is on, so to debug something you raise `minlevel` in the config and save — the running server picks it up in a second or two. Put it back afterwards.
- `overflowAction="Discard"`. With `Block` a full async queue stalls the thread that produced the message, and on combat paths that thread is the 50 ms region tick, so the simulation would wait on disk. Dropping lines under load is the right trade here.

Because logging sits on the swing path, **gate trace/debug messages behind `_logger.IsTraceEnabled`**. On net48 there are no interpolated-string handlers, so `_logger.Trace($"...")` builds and allocates the string whether or not the level is enabled — see `StatsInterface.GetTotalStat` for the pattern, and never call a real method inside a log argument.

Shutdown runs through `Program.Shutdown(reason)`: idempotent, reachable from `SetConsoleCtrlHandler` (Ctrl+C, Ctrl+Break, window close, logoff, shutdown) and `ProcessExit`, with each step guarded and NLog flushed last. It persists campaign progression and force-saves both databases, so **stop services by closing them, never with `Process.Kill()`** — a hard kill delivers no signal and skips all of it. `ServerLauncher` closes first and only kills after a 15s timeout. A clean stop logs `Closing the server (...)`; if that line is missing from a session, the shutdown did not run.

Runtime config lives in `bin/$(Configuration)/Configs/*.xml` (`Account.xml`, `World.xml`, `Lobby.xml`, `Launcher.xml`), generated from `aConfig` subclasses such as `WorldServer/Configs/WorldConfigs.cs` — adding a public field there adds an XML key. `PROJECTWAR_ENABLE_TLS` is retired; do not set it.

Prerequisite data not in git: `deps/zones/` (download `zones.zip` from the `zones-data-v1` release) and the three MySQL databases `war_accounts`, `war_characters`, `war_world` (`war_world.sql` is inside `Database/war_world.7z`).

## Branch policy

**Work on `RESTART`. Do not propose merging, rebasing onto, or cherry-picking from `master`.**

`RESTART` is a deliberate cherry-picked reset to `1980c873` (2019-06-22), the last state consistent with the 1.4.8 restoration goal. The 716 commits master accumulated afterwards were **rejected, not missed** — they were degrading both the server and the world database. Git's default framing is misleading here: `master` is the abandoned line, and `origin/HEAD` still points at it.

The database layout shows the difference plainly. Master stores the world as two opaque archives (`Database.7z`, `cooler_sai_sql_changes.7z`) with no readable diff and no migration path; `RESTART` splits them into per-database dumps plus numbered incremental scripts, which is what makes hard rule 1 below possible at all. Master's DB history is ad-hoc content edits and revert-of-revert pairs — drift away from the authentic 1.4.8 sources of truth.

The open dependabot branch is cut from master and does not apply.

## Hard rules

1. **Never edit the base dumps** `Database/war_accounts.sql`, `war_characters.sql`, `war_world.sql`. Schema/data changes ship as a new numbered incremental script in `Database/`, taking the next free number (the series currently runs through `38_restore_squig_nursery_objects_and_squig_credit.sql`), delivered with the code change. `AGENTS.md` rule 6 goes further than "the user applies it": apply the script to the local Release build's database and verify the resulting schema and data — never report database-backed work as tested when only the compile was checked. Note that the ORM auto-provisions unknown tables — `ObjectDatabase` calls `CheckOrCreateTable`, which issues `CREATE TABLE IF NOT EXISTS` on registration — so a new `DataObject` entity does not strictly need a migration script to work; write one only when existing rows need changing.

   **Caret suffixes on name columns are data, not corruption.** `creature_protos`, `quests_maps` and `boss_spawn` carry `^m`/`^M`, `^f`/`^F`, `^n` and `^p` suffixes — grammatical gender markers (masculine, feminine, neuter, plural) for German and French localisation. The client uses the same convention, e.g. `Mad Mixas^n,in` in `data/strings/english/zones/zone060_area_names.txt`. `^M` looks like caret notation for a carriage return and was once stripped as an import artifact across 5,210 rows; that was wrong and had to be reverted from the base dump. **Never strip them** — make name-based lookups tolerate the suffix instead.

   **Items live in two parallel tables and the server reads only one of them.** `ItemService.LoadItem_Info` picks its source from `World.xml`: `UseMythicActionCoverageTables = true` — the shipped value — loads `mythic_src_item_infos`, false loads `item_infos`. The two are entry-for-entry identical (88,727 rows each). A migration that writes item columns to only one of them is invisible to the running server, with no error anywhere: this cost three scripts and a full debugging session (BUG-033), because the ward `TokUnlock3` mapping went to `item_infos` while the server read `mythic_src_item_infos`. **Any migration touching item columns must update both tables, or explain why one is correct.** Item data is cached at boot, so a restart is required either way.
2. `AGENTS.md` encodes a Power-of-Ten-derived discipline for server code. The rules that actually bite here: bound every loop on a live path (packet handlers, region ticks, campaign scans, queue drains); avoid allocation in per-packet/per-tick code; no recursion in runtime server code without a documented reason; check `Try*`/DB/IO/network return values and validate parsed packet data before acting on it; keep `#if` rare. Packet handlers, the region update loop, battlefront logic, and persistence boundaries get the strictest review.
3. **Two out-of-repo sources of truth outrank guessing, and are meant to be used actively — not only when stuck.** This is a restoration project: the client is the authoritative record of what the real 1.4.8 server did, and reasoning from this emulator's source alone just reproduces whatever it already got wrong.
   - **`D:\Repos\Shmerrick\WAR-RE-Toolkit`** — the private companion RE repo. `RE_FINDINGS/{combat,network,world,evidence}` holds verified findings, `docs/{reference,research,functionality,checkpoints}` the write-ups, `apps/` ~26 extraction/analysis tools (`warclient`, `assethashhunter`, `warmyptool`, `warprotoextract`, `geom2fbx`, `zone2unreal`, `bot-template-viewer`, ...), and `tools/ToolkitControlCenter` the WPF hub that fronts them. It builds on .NET 10 / `WAR-RE-Toolkit.slnx` — a different toolchain from this repo, so don't apply the net48 MSBuild invocation there.
   - **`C:\Users\Admin\Videos\Warhammer Online - Age of Reckoning`** — the live 1.4.8 client install: `WAR.exe`, the `.myp` archives (`art`, `world`, `data`, `interface`, `audio`, `vo_english`, `mft`, `patch`, `dev`), plus `assetdb`, `Interface`, `user`, and `notes`.

   Before implementing anything protocol-, asset-, or data-structure-shaped, search `RE_FINDINGS/` and the toolkit `docs/` first and check the client for ground truth; say what you found there when explaining the change. Guessing at packet layouts and data structures is exactly how master's rejected commits drifted from the authentic sources of truth.

   **`docs/CROSS_REPO.md` is the full map** — data roots (including the extracted client tree at `C:\Users\Admin\Downloads\myps` and the 1,027-capture packet corpus), a question-to-repo routing table, the order of authority when sources disagree, and the two cross-repo contracts (the bot editor API, and the private ward-sigil client component). Read it rather than re-deriving any of that.

## Architecture

### Influence identity correction (2026-09-05)

`ChapterService.GetChapterEntry` resolves `chapter_infos.InfluenceEntry`, **not** its `Entry`
row key. Client `interface/interfacecore/maps/zone160/influenceids.csv:2-3` specifies Bastion
Order/Destruction tracks **129/128**, and `zone060/influenceids.csv:2-3` specifies Gunbad
**64/65**. Migration 23 changed these to row IDs and broke awards; migration 32 restores them.
Migration 33 repairs seven additional client-verified area bindings. See
`docs/handoffs/2026-09-05-stabilization.md` before relying on older checkpoint claims.

### Service topology

Five processes, all talking to `127.0.0.1`:

- **AccountCacher** — account/login data plus the .NET Remoting **RPC hub**. Other services connect as `RpcClient`s and obtain shared singletons (`Program.AcctMgr => Client.GetServerObject<AccountMgr>()`). RPC objects live in `Common/Rpc/` (`AccountMgr`, `CharacterMgr`) and are marked `[Rpc(...)]` on `RpcObject`; the transport is `FrameWork/Remoting/`.
- **LauncherServer** — patch manifest + login handoff (`mythloginserviceconfig.xml`, `PatchMgr`).
- **LobbyServer** — client lobby connection.
- **WorldServer** — all gameplay. Owns the `war_world` and `war_characters` databases.
- **ServerLauncher** — WinForms control panel that sequences the other four.

Support projects: `Launcher` (game launcher client), `LosBuilder` (line-of-sight `.bin` generation/inspection), `ClientDataMatrix` (client-data analysis, GUI + CLI). `CharacterCacher` and `WebAPI.Shared` have `.csproj` files but are **not** in `ProjectWAR.sln`.

### FrameWork — the shared engine layer

`FrameWork/` is the reusable substrate every service builds on. Four subsystems matter most:

- **Database (`FrameWork/Database/`)** — a hand-rolled ORM. Entities are `DataObject` subclasses annotated `[DataTable(TableName = "item_infos", DatabaseName = "World", PreCache = false)]` with `[DataElement]` on properties; see `Common/Database/World/Items/Item_Info.cs`. `DBManager.Start(...)` builds an `IObjectDatabase`; loading uses compiled-expression binders (`MySqlExpressionDataBinder`). `PreCache = true` loads the whole table at boot. Writes are queued and flushed — synchronous `ForceSave()` stalls the game thread and is deliberately rare.
- **Loader (`FrameWork/Loader/`)** — startup dependency injection by reflection. `LoaderMgr.Start()` scans every loaded assembly for `[Service(typeof(DependencyA), ...)]` classes and invokes their `[LoadingFunction(immediate)]` static methods, resolving declared dependencies first. `immediate: true` runs during the scan; `false` defers until the whole scan completes. **This is how the ~27 services in `WorldServer/Services/World/` load their tables** — there is no explicit registration list, so a new service is wired up purely by attributes.
- **NetWork (`FrameWork/NetWork/`)** — `TCPManager.Listen<TCPServer>(port, name)`, `BaseClient` async socket handling, packet in/out marshalling, and the packet-handler registry. Handlers are static methods tagged `[PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_X, "onX")]` on an `IPacketHandler` class; `TCPManager` reflects them into an opcode table at startup. Handlers in `WorldServer/NetWork/Handler/` are grouped by domain, and `AckHandlers.cs` exists purely to silently absorb opcodes with no server action.
- **Config/Log/Console** — `ConfigMgr.LoadConfigs()` serializes `aConfig` subclasses to/from XML; `ConsoleMgr` provides the interactive server console.

`Common/` holds everything shared between services: all `DataObject` entity definitions (`Common/Database/{Account,Character,World,Patch}/`) and the RPC contracts.

### WorldServer

Startup order in `WorldServer/Program.cs` is the best map of the whole system: load configs -> open Character and World DB pools -> connect `RpcClient` to AccountCacher and fetch the `Realm` -> `LoaderMgr.Start()` (all services load their data) -> construct `UpperTierCampaignManager` (T4) and `LowerTierCampaignManager` (T1) from `rvr_progression` rows -> attach campaigns to regions, lock all battlefronts, open the active one -> `TCPManager.Listen<TCPServer>` -> start `BotManager` / `DynamicBotManager` -> start the bot editor HTTP API -> `ConsoleMgr.Start()`.

Key layers:

- **World simulation (`World/Map/`)** — `RegionMgr` is the per-region update loop (`REGION_UPDATE_INTERVAL = 50` ms) driving all objects in its regions; `ZoneMgr` handles zone coordinate/pin math (note `CalculPin` is called on the *destination* zone during cross-zone movement); `CellMgr` handles spatial partitioning and range visibility; `Occlusion` consumes the `los/*.bin` data.
- **Object hierarchy (`World/Objects/`)** — `Point3D` -> `Object` -> `Unit` -> `Player` / `Creature` / `Pet` / `Siege` etc. `Player.cs` is very large and central. Inactive objects are filtered out of range visibility — this is why bots must be activated post-load.
- **Services (`WorldServer/Services/World/`)** — one static class per data domain (`ItemService`, `ZoneService`, `QuestService`, `ScenarioService`, `BattlefrontService`, `WaypointService`, `RVRProgressionService`, ...), each deriving from `ServiceBase` (which exposes `WorldMgr.Database`) and loading its tables into static in-memory dictionaries via `[LoadingFunction]`. Gameplay code reads these caches, not the DB.
- **Managers (`WorldServer/Managers/`)** — `WorldMgr` (global state, region registry, campaign managers, script registration), `CharMgr` (character persistence), `CommandMgr` + `Commands/` (GM commands, declared in `CommandDeclarations.cs` and assembled by `CommandsBuilder`), `LootsMgr`, `AreaMapMgr`.
- **Scripting (`World/Scripting/`)** — DB rows carry a `ScriptName`; `WorldMgr` reflects over `AGeneralScript` subclasses and registers them as global or per-name local scripts, so NPC/object behavior attaches by data rather than code wiring.
- **Battlefronts (`World/Battlefronts/`)** — the RvR campaign: `Apocalypse/` holds the campaign/battlefront state machines and progression, plus `Keeps/`, `Objectives/`, `Bounty/`, `ContributionTracker`, `AAOTracker`. The README's *RvR Terminology* section defines the domain vocabulary (BO, keep, pairing, tier, battlefront; Unclaimed -> Contested -> Captured -> Secured; domination) — use those terms.
- **HTTP APIs (`WorldServer/API/`)** — `API.Server` is a DEBUG-only diagnostic API (`EnableAPI`, port 51932). `BotEditorHttpServer` is a local JSON API for bot gear/template editing (`EnableBotEditorAPI`, port 51933), consumed by WAR-RE-Toolkit; see `docs/bot-editor-api.md`.

### Bot system

Bots are real persisted `Player` characters on the shared account id `9999`, running through the normal world/range/combat/group code paths with a no-op `BotClient : GameClient` in place of a socket. `BotManager` creates/loads them, `BotLoadoutManager` + `BotGearOverrideService` + `BotTemplateProfileService` resolve gear (base template -> per-bot `bot_gear_overrides` row -> applied loadout), `BotBrain` drives 6-man group combat by role suffix, `BotPathfinder` builds routes by sampling `WaypointService.TableWaypoints`, and `DynamicBotManager` keeps groups populated per tier/realm on a timer. Read `BOT_SYSTEM.md` before touching any of it — it documents the naming convention, role suffixes, item-restriction filters, and the visibility-activation requirement (`Player.EndInit()` -> `ActivateBotAfterInit()`).

## Docs worth reading before related work

- `README.md` — setup, an extensive troubleshooting list keyed to specific known symptoms, and RvR terminology.
- `AGENTS.md` — the binding agent rules.
- `BOT_SYSTEM.md` — bot architecture and GM commands.
- `docs/STATUS.md` — per-project build outputs and current LOS-generation parity status.
- `docs/INTERNAL_BUG_TRACKER.md` — live ledger of known bugs; update it when you find or fix one.
- `docs/BASTION_STAIR.md`, `docs/MOUNT_GUNBAD.md`, `docs/LAND_OF_THE_DEAD.md`, `docs/WARD_SYSTEM.md`, `docs/SYSTEM_GUILDS.md`, `docs/GUILD_KEEP_CLAIM_FLAGS.md` — the systems most recently restored on `RESTART`; read the matching one before touching Gunbad, the Tomb Kings expedition, wards, guilds, or keep claims.
- `docs/MASTER_TO_RESTART_AUDIT.md` — the per-change record of what master did and why `RESTART` rejected it. Consult it before concluding something is "missing" from this branch.
- `docs/bot-editor-api.md`, `docs/client-data-matrix-usage.md`, `docs/los/occ-re-notes.md`, `docs/data-matrix/`.
- `docs/handoffs/` — dated session checkpoints. **`2026-09-05-gunbad-and-lotd.md` is current** (Mount Gunbad levels, Gunbad PQ spawn repair, Land of the Dead tracker UI and flight access); `2026-09-05-commit-handoff.md` precedes it; it records the Chaos Wastes entrance regression (BUG-062, diagnosed and fixed in its follow-up pass), PQ retest improvement, retracted blue-orb explanation and remaining work. The earlier checkpoint is historical; use the stabilization handoff for corrected influence-key evidence.
