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

**There is no automated test suite.** A clean compile with no new warnings is the verification bar (`AGENTS.md` rule 9). If a package restore fails, restore NuGet packages in Visual Studio or run `nuget restore ProjectWAR.sln`.

Running the stack: always launch `bin/Release/ServerLauncher.exe` and use "Start All". The four services (`AccountCacher` -> `LauncherServer` -> `LobbyServer` -> `WorldServer`) have startup-order dependencies and race if started individually.

Kill a stuck stack:

```powershell
Get-Process | Where-Object { $_.Name -match 'AccountCacher|LauncherServer|LobbyServer|WorldServer|ServerLauncher' } | Stop-Process -Force
```

### Ports, and why startups fail intermittently

This machine's dynamic TCP range is **1024–65534** (Windows default is 49152+), so any outbound connection can occupy a fixed emulator port. This has bitten twice: `msedgewebview2` took 10300, and Unreal Engine's MCP server took 8000.

The 8000 case is the nasty one. Unreal binds `127.0.0.1:8000` while LauncherServer binds `0.0.0.0:8000` — **both succeed**, and Windows routes loopback connections to the more specific binding. The launcher then talks HTTP to Unreal instead of the raw-TCP patch service, ServerLauncher shows everything green, and LauncherServer's log is empty. Diagnose by comparing `127.0.0.1:8000` against the LAN IP: LauncherServer correctly *times out* on an HTTP request (it does not speak HTTP), while an instant HTTP 404 means something else holds loopback.

6000-6010, 6800, 8048 and 10300 are reserved via `netsh int ipv4 add excludedportrange`. **Do not reserve 51932-51933** — exclusions block `HttpListener` (HTTP.SYS) even though raw sockets bind fine, which stops the bot editor API.

Ports in play: 6800 AccountCacher RPC, 8000 LauncherServer, 8048 LobbyServer, 10300 world (from `war_accounts.realms.Port`), 51932/51933 the debug and bot-editor APIs, 6000+ RPC client ports.

### Database performance

Gameplay is served from in-memory caches — after boot, essentially the only recurring query is the character load. That makes the per-login lookups the scaling path, and four of them were unindexed full table scans until `Database/03_add_hot_path_indexes.sql`. If you add a per-character or per-account query, check `EXPLAIN` shows `type: ref` rather than `type: ALL`.

The ~2.1s "Slow SQL" warning at boot is the `Item_Info` load (88,727 rows) and is expected; it does not scale with players. The save pump batches every dirty object into a single unbounded transaction every 60s — fine at current scale, worth chunking if population grows a lot.

Runtime config lives in `bin/$(Configuration)/Configs/*.xml` (`Account.xml`, `World.xml`, `Lobby.xml`, `Launcher.xml`), generated from `aConfig` subclasses such as `WorldServer/Configs/WorldConfigs.cs` — adding a public field there adds an XML key. `PROJECTWAR_ENABLE_TLS` is retired; do not set it.

Prerequisite data not in git: `deps/zones/` (download `zones.zip` from the `zones-data-v1` release) and the three MySQL databases `war_accounts`, `war_characters`, `war_world` (`war_world.sql` is inside `Database/war_world.7z`).

## Branch policy

**Work on `RESTART`. Do not propose merging, rebasing onto, or cherry-picking from `master`.**

`RESTART` is a deliberate cherry-picked reset to `1980c873` (2019-06-22), the last state consistent with the 1.4.8 restoration goal. The 716 commits master accumulated afterwards were **rejected, not missed** — they were degrading both the server and the world database. Git's default framing is misleading here: `master` is the abandoned line, and `origin/HEAD` still points at it.

The database layout shows the difference plainly. Master stores the world as two opaque archives (`Database.7z`, `cooler_sai_sql_changes.7z`) with no readable diff and no migration path; `RESTART` splits them into per-database dumps plus numbered incremental scripts, which is what makes hard rule 1 below possible at all. Master's DB history is ad-hoc content edits and revert-of-revert pairs — drift away from the authentic 1.4.8 sources of truth.

The open dependabot branch is cut from master and does not apply.

## Hard rules

1. **Never edit the base dumps** `Database/war_accounts.sql`, `war_characters.sql`, `war_world.sql`. Schema/data changes ship as a new numbered incremental script in `Database/` (e.g. `03_*.sql`), delivered with the code change and applied by the user before starting the server. Note that the ORM auto-provisions unknown tables — `ObjectDatabase` calls `CheckOrCreateTable`, which issues `CREATE TABLE IF NOT EXISTS` on registration — so a new `DataObject` entity does not strictly need a migration script to work; write one only when existing rows need changing.
2. `AGENTS.md` encodes a Power-of-Ten-derived discipline for server code. The rules that actually bite here: bound every loop on a live path (packet handlers, region ticks, campaign scans, queue drains); avoid allocation in per-packet/per-tick code; no recursion in runtime server code without a documented reason; check `Try*`/DB/IO/network return values and validate parsed packet data before acting on it; keep `#if` rare. Packet handlers, the region update loop, battlefront logic, and persistence boundaries get the strictest review.
3. Reverse-engineering companion tooling and findings live in a separate private repo at `D:\Repos\Shmerrick\WAR-RE-Toolkit` (`WarClientTool`, `AssetHashHunter`, `Diffuser`, DB scripts). Consult it for protocol/asset/structure questions rather than guessing.

## Architecture

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
- `docs/SYSTEM_GUILDS.md`, `docs/bot-editor-api.md`, `docs/client-data-matrix-usage.md`, `docs/los/occ-re-notes.md`.
- `docs/handoffs/` — dated session checkpoints from prior work.
