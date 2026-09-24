# Ability conformance to the client — September 15 checkpoint

Delivery follow-up: [September 24 fresh audit](2026-09-24-repository-audit.md).
The uncommitted state and port conflict below are historical observations, not
instructions to alter another project's service without reproducing the conflict.

Follow-up: [September 20 resume and channel range repair](2026-09-20-ability-resume.md).

Resume here. Nothing below is committed, and nothing has been tested in game.

## The rule this work follows

The repository owner, 2026-09-14: *"I want to go with what the client says always. We should always
be conforming to the client unless we are physically unable to."* Client values win, balance values
included, and over a packet capture that disagrees. Where the server reads a field differently from
the client, the server code changes. A client disagreement is never parked as a design question: it
is either conformed or queued with the reason it cannot be yet. See
`docs/DATABASE_FIDELITY_PLAN.md` ("The rule that governs all of it").

## State of the working tree

Branch `RESTART`, uncommitted. Migrations `Database/00`–`10` are **applied to the local Release
`war_world`** and each was re-run to prove it changes nothing (table checksums identical).
`war_accounts` and `war_characters` were not touched.

| Migration | What it did | Tracker |
|---|---|---|
| `00_fix_ability_timing_units.sql` | 26 cooldowns and 3 cast times stored in the wrong unit | BUG-166 |
| `01_restore_ability_effect_ids_from_client.sql` | 22 EffectIDs where the toolkit import was wrong | BUG-129 |
| `02_restore_ability_cast_times_from_client_and_captures.sql` | 119 cast times the client and captures agree on | BUG-167 |
| `03_conform_ability_cooldowns_to_client.sql` | 1,007 cooldowns; adds `AICooldown` | BUG-169 |
| `04_conform_ability_cast_times_to_client.sql` | 524 cast times | BUG-169 |
| `05_conform_ability_ap_costs_to_client.sql` | 1,309 AP costs | BUG-169 |
| `06_clear_ability_effect_ids_the_client_lacks.sql` | 9 EffectIDs | BUG-169 |
| `07_conform_ability_damage_to_client.sql` | 12 base damage values | BUG-169 |
| `08_conform_buff_durations_to_client.sql` | 10 buff durations | BUG-169 |
| `09_conform_ability_targeting_and_ranges_to_client.sql` | adds `TargetType`; 2,122 ranges | BUG-168 |
| `10_conform_ability_channels_to_client.sql` | adds `ChannelDuration`, `ChannelInterval`; 87 channels | BUG-170 |

All ability migrations write both halves (`abilities` + `mythic_src_abilities`, and likewise the buff
and damage tables). The server must run the build that maps the new columns, and the columns must
exist before it starts.

### Server code changed

- `Common/Database/World/Ability/DBAbilityInfo.cs` — `AICooldown`, `TargetType` (nullable: NULL means
  no client record), `ChannelDuration`, `ChannelInterval`.
- `AbilityConstants.cs` — `ClientTargetType`, `ChannelDuration`, `ChannelInterval`.
- `AbilityInfo.cs` — `AICooldown`; `TargetsCaster` (client TargetType 0 or 3; falls back to
  `Range == 0` only where the client has no record).
- `AbilityProcessor.cs` — target selection by `TargetsCaster`; a range of 0 on a targeted ability
  sets no limit; a channel with a cast time casts first and then channels; the channel-start timer
  carries `ChannelDuration` with type 0x21, as the live server sent.
- `NewChannelHandler.cs` — runs on `ChannelDuration`, ticks and charges `ApCost` every
  `ChannelInterval` (once a second where the client gives none).
- `AbilityMgr.cs`, `Pet.cs`, `SimpleLVHealerBrain.cs` — AI paces by the larger of `Cooldown` and
  `AICooldown`. `ABrain.cs` — a creature stands still for a channel's `ChannelDuration`.

### ClientDataMatrix changed

Earlier in the session: the abilities.csv identity fix (BUG-165), the crosswalk moved onto the client
BINs, the upgrade-table parser. Then: `BinaryAbilityRecord.IsChanneled` (FlagsRaw bit 22) and three
crosswalk columns, `Channel`, `ChannelDuration`, `ChannelInterval`. **The GUI has not been checked**
(ability 692's icon, the Ability Crosswalk tab filters and its "How this works" text).

### Tools and checks added

- `tools/captures/extract_use_ability.awk` — EffectID, cast time and channel length from 0xDA frames.
- `tools/captures/extract_use_ability_targets.awk` — who each cast landed on (self / other / none).
- `tools/validation/Test-AbilityLoaders.ps1` + `AbilityLoaderChecks.cs` — loads both ability tables
  through the server ORM by both binders, including the new columns.
- `tools/validation/AbilityAlignmentChecks.cs` — floors raised to 8,372 / 4,177 EffectIDs; the
  cross-table comparison now includes `TargetType`, `AICooldown` and the channel columns.

### Last verification (2026-09-14)

Full solution Release x64: 0 warnings, 0 errors. `Test-AbilityLoaders.ps1`,
`Test-AbilityAlignment.ps1` and `Test-RuntimeRegressions.ps1` pass. WorldServer booted on the
migrated database at 18:35 (8,109 abilities, 2,801 buffs, no errors, listening on 10300).

## The evidence the changes rest on

- **Targeting:** in the captures, client TargetType 0/3 abilities were cast on their caster
  (772 of 773) and TargetType 1 on another unit (844 of 844), 163 of those at client range 0. The
  client tooltip calls a zero range "Self", but the live server did not target the caster by range.
- **Channel flag:** FlagsRaw bit 22 of `abilityexport.bin` is set on 99 of 99 live channels and 1 of
  1,663 live casts. `mythic_bin_ability.CastType` is useless (0 on 11,734 of 11,735).
- **Channel length:** the first timed component's `Duration` is the live channel length on 90 of 93
  live channels; all 64 of ours the captures cover agree after migration 10.
- **Channel AP:** ours was the client's `ApCost` rescaled to one-second ticks on 47 of 48.
- **Channel timer:** `F_SET_ABILITY_TIMER` type 0x21 carries the channel length on 14 of 14 captured
  channels; type 0x23 frames carry a falling remaining time (probably setbacks, not implemented).

## Not yet done — the conformance queue

Full reasons in `docs/DATABASE_FIDELITY_PLAN.md`. In rough order of value:

1. **In-game test first**, especially casting (every cast's target selection changed) and channels.
2. **127 abilities the client flags as channels that we do not channel.** 112 have no data; the rest
   carry ability commands the channel handler never runs (the teleport scrolls 14478-14480, 4980,
   14420). Needs channel-end command execution before any are converted.
3. **Cooldowns below a second: 49.** `Cooldown` must move to milliseconds (every `Cooldown * 1000`
   site, item cooldowns in seconds, AI pacing in bytes of seconds).
4. **Damage: 15** abilities with several rows or tokens, and **buff durations: 5** (1677, 8481,
   8494, 10780, 14245) — both need the component-to-row mapping.
5. **Career lines:** 402 client-empty, 91 ours-empty, 2 differing; client granting data not found.
6. **Channel setbacks** (the 0x23 frames) and **channel buff tick intervals**.
7. **Ranges finer than a foot: 574**, rounded because the server measures whole feet.
8. BUG-151, the upgrade table join (`FUN_00926777` decode is in `docs/ABILITY_DAMAGE_MODEL.md`).

## Environment problem found at the end of the session

The launcher could not log in with `aaa`. Not a database issue: the scheduled task
`\MortalAtlasHost` (boot trigger) runs `bun ... atlas-static-host.mjs ... 8000` for the Mortal_RE
Atlas site on `127.0.0.1:8000`. The launcher hard-codes `127.0.0.1:8000`, and Windows routes loopback
to that more specific listener instead of LauncherServer's `0.0.0.0:8000`. The port exclusion does
not stop an explicit bind. **Unresolved — it was not changed**, because the task belongs to another
project. It restarts at every boot. Fix by moving the task to another port (elevated PowerShell):

```powershell
$task = Get-ScheduledTask -TaskName MortalAtlasHost
$task.Actions[0].Arguments = $task.Actions[0].Arguments -replace ' 8000$', ' 8001'
Set-ScheduledTask -TaskName MortalAtlasHost -Action $task.Actions
Stop-ScheduledTask -TaskName MortalAtlasHost; Start-ScheduledTask -TaskName MortalAtlasHost
```

## Session files outside the repository

The generator scripts, capture extracts and database backups live in the session scratchpad under
`%TEMP%\claude\D--Repos-Shmerrick-ProjectWAR\93cccf26-ca75-4013-b35c-4ea716f75d85\scratchpad`
(`gen_*.py`, `awk_use_ability_*.txt`, `backup_before_03.sql`, `backup_before_09.sql`,
`backup_before_10.sql`). Temp can be cleaned, so do not rely on them. The migrations are
self-contained, the capture extracts regenerate from `tools/captures/`, and the pre-migration state is
the base dump in `Database/war_world.7z`.

## How to resume

1. Fix port 8000 (above), start the stack with ServerLauncher, log in, test casting and channels.
2. Check the ClientDataMatrix GUI.
3. Rebuild if in doubt, then run `tools/validation/Test-AbilityLoaders.ps1`,
   `Test-AbilityAlignment.ps1` and `Test-RuntimeRegressions.ps1`.
4. Re-run `.\bin\Release\ClientDataMatrix.exe crosswalk abilities --root "C:/Users/Admin/Downloads/myps"`
   to see the current disagreement counts before taking the next queue item.
5. Commit only when the repository owner asks.
