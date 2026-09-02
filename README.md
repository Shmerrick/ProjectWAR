# ProjectWAR Emulator Beginner Guide

This guide is written for complete beginners.

## What this project does

ProjectWAR is a local server emulator for Warhammer Online. It lets you run the game server stack on your own computer so you can test and play in a private local environment.

The current focus of the project is the **1.4.8 Restoration Plan**, which aims to restore the emulator's database and logic to the state of the final official patch (1.4.8) using authentic "Sources of Truth".

## How the emulator is organized

When you start the emulator, these services work together, managed by the **ServerLauncher**:

- `ServerLauncher`: The central GUI application to start, monitor, and stop all server services.
- `AccountCacher`: account/login data and RPC hub.
- `LauncherServer`: patch/login handoff service.
- `LobbyServer`: client lobby connection.
- `WorldServer`: game world and gameplay logic.

Your local launcher/client connects to these services on `127.0.0.1` (your own machine).

## One-time checklist

Install these first:

- Windows
- Visual Studio 2022 with `.NET desktop development`
- .NET Framework 4.8 Developer Pack
- MySQL
- Warhammer Online client files

## Setup steps (follow in order)

### 1. Download zone data

Zone files are too large for git and must be downloaded separately.

1. Download `zones.zip` from:
   - https://github.com/Shmerrick/ProjectWAR/releases/tag/zones-data-v1
2. Extract to `deps/zones/`

Checkpoint: you should see folders like `deps/zones/zone001/`.

### Optional: Generate LOS Data Natively

By default, `WorldServer` still uses prebuilt LOS binaries from `deps/los/`.

If you want the repo to generate `los/*.bin` natively from an extracted WAR client:

1. Make sure your extracted client root contains:
   - `assetdb/figleaf.db`
   - `assetdb/`
   - `zones/`
   - raw zone terrain inputs such as `terrain.pcx`, `offset.pcx`, and `sector.dat` for the zones you want to build
2. Set these environment variables before building:
   - `PROJECTWAR_GENERATE_LOS=1`
   - `PROJECTWAR_EXTRACTED_ROOT=C:\path\to\WAR_extracted`
   - optional: `PROJECTWAR_LOS_ZONE=280` to generate one zone while testing
3. Build `WorldServer` or the full solution.

This will:
- build `LosBuilder`
- regenerate `bin/Release/los/` from the extracted client data
- skip copying `deps/los`

If your extracted client only has partial zone terrain data, native LOS generation will only work for the zones that still have those raw terrain files. In that case, use `PROJECTWAR_LOS_ZONE` to test one zone at a time or keep the existing `deps/los` fallback.

Manual example:

```powershell
$env:PROJECTWAR_GENERATE_LOS = '1'
$env:PROJECTWAR_EXTRACTED_ROOT = 'C:\Users\Admin\Downloads\myps'
msbuild ProjectWAR.sln /p:Configuration=Release /p:Platform=x64
```

Direct tool usage:

```powershell
bin\Release\LosBuilder.exe generate --input-root C:\Users\Admin\Downloads\myps --output-root bin\Release\los
```

Inspect shipped or generated LOS binaries:

```powershell
bin\Release\LosBuilder.exe inspect --input-bin bin\Release\los\280.bin
bin\Release\LosBuilder.exe compare --left-bin bin\Release\los\280.bin --right-bin C:\temp\los\280.bin
```

Reverse-engineering notes for the shipped `OCC` format are tracked in [docs/los/occ-re-notes.md](docs/los/occ-re-notes.md).

### 2. Create and import databases

Create three databases:

- `war_accounts`
- `war_characters`
- `war_world`

Import the SQL files. `war_world` is shipped as a compressed archive — extract it first:

```powershell
# Extract war_world.sql from the 7z archive
7z e Database\war_world.7z -o Database\

mysql -u root -p -e "CREATE DATABASE war_accounts; CREATE DATABASE war_characters; CREATE DATABASE war_world;"
mysql -u root -p war_accounts < Database/war_accounts.sql
mysql -u root -p war_characters < Database/war_characters.sql
mysql -u root -p war_world < Database/war_world.sql
```

Then apply the incremental update scripts, **in numerical order**. The base dumps are never edited, so every schema and data change since they were captured lives in these files. Skipping them leaves the server running against an out-of-date schema.

```powershell
# Each script selects its own database, so no database argument is needed.
Get-ChildItem Database\*.sql |
    Where-Object { $_.Name -match '^\d+_' } |
    Sort-Object Name |
    ForEach-Object { Write-Host "applying $($_.Name)"; mysql -u root -p < $_.FullName }
```

Current scripts, oldest first:

| Script | What it does |
|--------|--------------|
| `01_add_tokunlock3.sql` | Adds `item_infos.TokUnlock3`, needed for the third Tome unlock on equip |
| `02_restore_mailboxes.sql` | Restores the mailbox gameobject prototypes |
| `03_add_hot_path_indexes.sql` | Indexes the per-login character lookups; without it every login full-scans `characters_items` |
| `04_restore_guild_keep_claim_flags.sql` | Restores 21 packet-verified guild keep-claim flags; three keeps remain disabled pending authoritative coordinates |
| `05_restore_invader_superior_ward_unlocks.sql` | Restores the five client-defined Superior Ward unlocks across all 24 Invader armor sets |
| `06_remove_invalid_creature_ability_header.sql` | Removes a CSV header accidentally imported as an unusable creature ability row |
| `07_restore_known_creature_ward_tiers.sql` | Historical prototype-level ward restoration; superseded by `08` because prototypes are reused across locations |
| `08_move_creature_wards_to_spawns.sql` | Adds ward fields to concrete world, instance, boss, and PQ spawns and reverses the unsafe prototype assignments from `07` |
| `09_normalize_spawn_ward_columns.sql` | Normalizes pre-existing ward columns to validated unsigned, non-null, default-zero fields |
| `10_restore_ruinous_powers_tombstones.sql` | Restores the 25 capture-verified Perished Soul objects and their three-second interactions for Ruinous Powers stage II |
| `11_restore_ruinous_powers_finale.sql` | Restores Mathus's timed ritual movement and the capture-verified Bloodhowler finale as separate phases |
| `12_restore_norsca_chapter_state.sql` | Corrects Chaos Chapter 2 influence, restores Ruinous Powers scenery/ToK objects, and moves its reward chest to the official position |
| `13_restore_mailbox_spawns.sql` | Restores 190 historical Order and Destruction mailbox spawns across 32 zones |

Every script selects its own database and is safe to re-run: `01` and `03` skip existing work, `02` uses `REPLACE INTO`, `04` upserts its objective rows while preserving existing nonzero keep mappings, `05` fills only missing Invader ward mappings, `06` deletes only the exact malformed header signature, `07` fills only empty legacy prototype bits, `08` adds missing spawn columns before reversing the 79 rows changed by `07`, `09` enforces the final ward-column definition, `10` replaces only the spawn rows belonging to Ruinous Powers objective 800, `11` upserts only the two capture-backed finale phases and their spawns, `12` upserts only the three capture-backed Norsca objects while correcting the associated chapter and chest rows, and `13` replaces only the 190 historical mailbox GUIDs while preserving unrelated custom spawns.

Checkpoint: all three databases exist and contain tables.

### 3. Build the solution

1. Open `ProjectWAR.sln` in Visual Studio.
2. Set build configuration to `Release` and platform to `x64`.
3. Build the solution.

Checkpoint: build output is in `bin/Release/`.

Command-line example:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\amd64\MSBuild.exe' ProjectWAR.sln /p:Configuration=Release /p:Platform=x64 /v:minimal
```

### 4. Verify local config files

Check these files in `bin/Release/Configs/`:

- `Account.xml`
- `Launcher.xml`
- `Lobby.xml`
- `World.xml`
- `mythloginserviceconfig.xml`

Default local values:

- host `127.0.0.1`
- DB port `3306`
- DB user `root`
- DB password `password`

Networking note:

- ProjectWAR currently expects legacy launcher/client traffic over raw TCP.
- `PROJECTWAR_ENABLE_TLS` is retired and should not be set.

If your DB password is different, update:

- `bin/Release/Configs/Account.xml`
- `bin/Release/Configs/World.xml`

### 5. Start server services

**IMPORTANT**: ALWAYS use `ServerLauncher.exe` to start the server stack. Do NOT start individual executables separately, as they will not initialize correctly.

1. Navigate to `bin/Release/`.
2. Run `ServerLauncher.exe`.
3. Click "Start All" (or individual start buttons in order: Account, Launcher, Lobby, World).

### 6. Start the game launcher

Start your local Warhammer Online client.

## Quick health checks

Check running emulator services:

```powershell
Get-Process | Where-Object { $_.Name -match 'AccountCacher|LauncherServer|LobbyServer|WorldServer|ServerLauncher' } | Select-Object Name, Id
```

## Stop all services

Use the "Stop All" button in `ServerLauncher.exe`, or force stop via PowerShell:

```powershell
Get-Process | Where-Object { $_.Name -match 'AccountCacher|LauncherServer|LobbyServer|WorldServer|ServerLauncher' } | Stop-Process -Force
```

## Troubleshooting

- Build fails or package errors:
  - restore NuGet packages in Visual Studio, then rebuild.
- Database connection failures:
  - verify credentials in `Account.xml` and `World.xml`.
  - verify MySQL/MariaDB is running on `127.0.0.1:3306`.
- Client cannot connect:
  - verify all services are running via `ServerLauncher`.
  - verify config files still point to localhost values.
  - verify `PROJECTWAR_ENABLE_TLS` is not set in your shell or system environment.
- Missing terrain/zone data:
  - verify `deps/zones/` extraction.
  - rebuild so assets are copied into `bin/Release/zones/`.
- Server startup races or cascading dependency failures:
  - use `ServerLauncher.exe` and let it bring services up in sequence.
  - do not manually start `AccountCacher`, `LauncherServer`, `LobbyServer`, and `WorldServer` in parallel.
- Characters stuck after a failed teleport or zone move:
  - update to the current `WorldServer` build.
  - the server now repairs invalid saved login positions and falls back to safe realm locations instead of leaving the character in a load loop.
- GM `.teleport center` or `.teleport entry` lands in a bad spot:
  - the command now prefers respawns, taxis, rally points, chapter pins, and validated portal arrivals before using `zone_infos`-derived fallbacks.
  - if a zone still has no reliable anchors, curate its respawn/taxi/rally/chapter data rather than relying on geometric center points.
- Scenario starts are missing, College of Corruption starts in the wrong place, or capital fallback teleports are unsafe:
  - update to the current `WorldServer` build and re-import `war_world.sql` from the latest `Database/war_world.7z`.
  - the stable scenario pool, missing scenario respawns, College of Corruption starts, and capital spawn data are all baked into the current `war_world` dump.
- Land of the Dead expedition flights never appear or never unlock:
  - the LOTD tracker, taxi data, and zone pairing fixes are all included in the current `war_world` dump — re-import from `Database/war_world.7z` if upgrading from an older dump.
  - the LOTD tracker uses T4 battlefront locks to award realm points, unlocks expedition access for one realm at a time, then resets after the configured ownership window.
  - RoR refers to the visible LOTD bar as the `expedition tracker`, but current client evidence still points at the Tomb Kings `F_RRQ` / RRQ tracker container for that UI.
  - if the `lotd_resource_tracker` table is missing, the server now keeps the LOTD flights hidden instead of exposing them to both realms.
  - if a realm owns LOTD but the client still cannot see the zone `191` flight path, update to the current `WorldServer` build; LOTD taxis now bypass the generic T4 Tome-token gate once `LotdService` has unlocked them for that realm.
  - `WorldServer` normalizes the shipped `zone_infos.Pairing = 100` metadata for zone `191` to the proper Land of the Dead pairing id (`4`) on load, so the flight-master node is clickable.
  - `WorldServer` also normalizes malformed LOTD taxi rows on load; zone `191` taxi destinations previously stored as local pins are converted to world coordinates at boot.
  - if the expedition tracker is still invisible, confirm the server log reaches `Loaded Land of the Dead resource tracker` on the current build before debugging packet display behavior.
- Live event tables are missing or the live-event UI is empty because `war_world.liveevent_*` was dropped or truncated:
  - re-import from the current `Database/war_world.7z`; the dump includes the live event tables with all events disabled by default (`Allowed = 0`).
- The active T4 battlefront opens, but its objectives still behave as `ZoneLocked` or Praag immediately aborts domination checks:
  - update to the current `WorldServer` build.
  - battlefront objective lock/open calls now drive the FSM consistently and force a neutral-safe reset if an objective stays stuck in `ZoneLocked`.
- Battlefield objectives can be clicked, but flags never capture:
  - the `buff_infos.Entry = 60000` (`Interaction`) row is included in the current `war_world` dump.
  - the server also installs a runtime fallback for buff `60000`, so BO capture no longer depends entirely on the DB row being present.
- Entering a warcamp incorrectly RvR-flags the player or counts as being in the lake:
  - update to the current `WorldServer` build.
  - lake state is now computed separately from raw `zone_areas` so warcamp entrances suppress RvR-lake behavior until the player actually leaves the warcamp buffer.
- Greenskin starters appear in the wrong Mt Bloodhorn position:
  - re-import from the current `Database/war_world.7z`; the corrected Greenskin `characterinfo` templates are included.
- Random name suggestions repeat, are sequential, or offer taken names:
  - update to the current `WorldServer` build.
  - random suggestions still draw from `war_world.random_names`, but they are now shuffled per request, checked against existing character names, and replaced with generated valid names only if the curated pool is exhausted.
- `.boot` does not preserve the live campaign or leaves the server in a bad shutdown state:
  - update to the current `WorldServer` build.
  - `.boot` now saves player state and active RvR progression, blocks new connections, disconnects players cleanly, updates realm population to zero, then exits.
- Need a clean rebuild:
  - delete generated directories such as `.vs/`, `bin/`, `*/obj/`, and `packages/`, then restore/build again.

## Developer Documentation

For contributors and AI agents, please refer to the following architectural documents:

- **[System Guilds](docs/SYSTEM_GUILDS.md)**: Details the automated guild experience for new players.
- **[Bot System](BOT_SYSTEM.md)**: Details the architecture, logic, and GM commands for the integrated player-like Bot System.
- **[Internal Bug Tracker](docs/INTERNAL_BUG_TRACKER.md)**: Live ledger of known issues and regressions.
- **[Ward System](docs/WARD_SYSTEM.md)**: Confirmed 1.4.8 ward progression and damage-scaling target, historical changes, and remaining evidence gaps.
- **[AI Agent Rules](AGENTS.md)**: Single source of truth for repository-specific AI instructions.

## Recent Optimizations & Fixes (2026-03-30)

- **Performance**: `RegionMgr` update loop optimized using a dense `HashSet` for active objects, eliminating the 65k sparse array scan per tick.
- **Concurrency**: `lock(this)` deadlocks resolved in all networking clients and `CellMgr` using private sync objects.
- **NPC Waypoints**: Critical `Thread.Sleep(5000)` removed from waypoint creation; implemented thread-safe static ID generation.
- **Database**: Synchronous `ForceSave()` calls audited and reduced to prevent game-thread stalls.

## Development Resources

When analyzing network protocols, game assets, structures, or looking for reverse engineering findings, all contributors and AI agents should reference the **WAR-RE-Toolkit** repository:
- **Local Path**: `D:\Repos\Shmerrick\WAR-RE-Toolkit`
- **Remote**: Private GitHub repo `Shmerrick/WAR-RE-Toolkit`

This toolkit contains essential companion tools like `WarClientTool`, `AssetHashHunter`, `Diffuser`, and various database scripts required for emulator improvement.

### Database Modification Rules

**CRITICAL RULE FOR ALL CONTRIBUTORS AND AI AGENTS:**

1. **NEVER modify** the base `.sql` files located in the `Database/` folder (`war_accounts.sql`, `war_characters.sql`, `war_world.sql`). These are meant for the initial setup by end-users.
2. If a source code change requires a database schema or data modification, you **MUST create a new update script** (e.g., `update_001.sql`).
3. These update scripts should be provided alongside the code changes, and end-users must be prompted to apply them to their database prior to loading the emulator for the server to run correctly.

## RvR Terminology

### Global Concepts
- **Battlefield Objective (BO)**: A location on the RvR section of a map (zone) that players must control. Typically represented by a flag.
- **Keep**: A large-scale BO with deeper capture and hold mechanics (guards, doors, lords).
- **Faction**: The two opposing sides: **Order** (Dwarf, Empire, High Elf) and **Destruction/Chaos** (Greenskin, Chaos, Dark Elf).
- **Race**: Specific ethnic groups within factions (e.g., Dwarf vs Greenskin).
- **Pairing**: A specific conflict between an Order race and its corresponding Destruction rival (e.g., Dwarf vs Greenskin).
- **Tier**: Level-bracketed gameplay areas (1-4) consisting of one or more zones.
- **Zone**: An individual map with a unique ID.
- **Battlefront**: The active RvR area of a pairing and its associated tier. A battlefront can span one or multiple zones.

### Flag States
- **Unclaimed**: The default state; no faction holds the flag.
- **Contested**: A faction has interacted with the flag, triggering a countdown timer. The opposing faction can attempt to reclaim it during this period.
- **Captured**: The countdown timer has reached zero. The claiming faction now owns the flag. It enters a **lockout state** where the opposing faction cannot interact with it. This triggers a lockout countdown timer.
- **Secured**: The lockout timer has reached zero. The faction still controls the flag, but it is now open for conflict and can be assaulted by the opposing faction.

### Domination
- **Domination Status**: Occurs when a single faction controls **all** Battlefield Objectives within a battlefront, and all those objectives are in the **Secured** state.
- **Domination Victory**: Domination supersedes the Victory Point (VP) requirement for controlling and locking a battlefront.
- **Rewards & Progression**: Achieving domination awards RvR lock rewards to the winning faction. The battlefront locks for 30 minutes before shifting to a new battlefront within the same tier.
