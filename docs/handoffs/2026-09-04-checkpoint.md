# Checkpoint — 2026-09-04

A verification checkpoint over the help tip, ward fragment, ward sigil, and instance-entry work.
This records what was checked and how, so the next session can trust it without redoing it.

## Verification summary

| Check | Result |
|:---|:---|
| All 20 SQL scripts apply to a raw database in order | pass, 20/20, no errors |
| Scripts are idempotent (applied twice) | pass, no errors, no double-application |
| Clean replay reproduces the live database | pass, **every** `war_world` table byte-identical |
| `war_characters` indexes from script 03 | pass, 8 non-primary indexes in both |
| Full solution rebuild, Release x64 | pass, **0 warnings, 0 errors** |
| Diff contains no line-ending noise | pass, after restoring 90 terminators |
| No client-patch internals in this public repo | pass |

## SQL verification method

The scripts were **not** checked against the live database, which would only prove they had
already been run. They were replayed from nothing:

1. Extracted `Database/war_world.7z` (110 MB of SQL) to a scratch directory.
2. Rewrote every `` `war_world` ``/`` `war_characters` ``/`` `war_accounts` `` identifier to a
   `vfy_`-prefixed scratch name. This is safe because every script scopes itself with a single
   `USE` and none uses a cross-database qualified reference.
3. Loaded the three base dumps into the scratch databases — the raw state a new deployment starts
   from. 152 world tables, 31 character tables, 11 account tables.
4. Applied `01_*` through `20_*` in filename order, capturing each script's exit status
   separately. All 20 succeeded.
5. Applied all 20 **again** to test idempotency. All 20 succeeded, and the outcome counts were
   unchanged, so a re-run does not double-apply.
6. Compared outcomes against the live database, then compared `CHECKSUM TABLE` for **every** table
   in `war_world`.
7. Dropped the scratch databases and deleted the extracted dump.

Outcome counts, identical in the clean replay and live:

```
help_tips=59  tok3_items=1377  sec5_rows=142
ward195=110   ward196=230      ward260=1197   mailboxes=190
```

**Every table checksum matched.** That is a stronger result than "the scripts run": it proves the
scripts fully reproduce the live state, and that the live database carries no manual drift that
only exists on this machine. A fresh deployment from the dumps plus scripts 01-20 lands in exactly
the state the server is running today.

## Code review notes

Reviewed the full staged diff hunk by hunk. Two issues found and fixed:

- **`F_WARD_INFO` was declared out of order** in `Opcodes.cs`, sitting before
  `F_OBJECT_EFFECT_STATE = 0xDE`. Harmless (the values are explicit) but wrong in an otherwise
  ordered file. Moved after `0xDE`.
- **90 lines of line-ending noise**, 88 in `Player.cs` and 2 in `GameObject.cs`. This repository has
  genuinely mixed line endings — many files are LF in the blob, `Player.cs` is 7,188 CRLF plus 94
  LF — and earlier edits normalised whole files. Every line that differed *only* by its terminator
  was restored to the committed version's ending, verified by asserting the content is identical
  once terminators are ignored. `Player.cs` went from 425 changed lines to 249 real ones.

Points confirmed as correct rather than changed:

- `TokInterface.AddTok`'s ward-task recursion is depth-1 by construction: a fragment award has
  task digit 0, so it is never itself a task and never resolves again.
- `HelpTipService.GetTips` returns a cached list or a shared empty one — no allocation on the
  trigger path, which matters because triggers fire on loot and rank-up.
- `FireHelpTips` loops over a small preloaded list with no DB access of its own.
- Every help tip row is validated at load (named section 101 record, category 1-4, known trigger,
  no duplicates) and rejections are logged with the offending entry.
- `Group`'s warband trigger correctly takes the member read lock.

## Instance entry fixes (BUG-031)

Three defects, all found from one stack trace in the world log:

1. `Instance.LoadBossSpawns` used `spawn.Proto` without checking it. `instance_boss_spawns` Entry
   4275 (BossID 401, zone 260) has no `creature_protos` row, so the constructor threw on
   `spawn.Proto.Name`, instance creation aborted, and the Lost Vale portal failed silently.
   `LoadSpawns` had always guarded this; `LoadBossSpawns` never did. Now logs and skips.
2. `Player.Teleport` created an instance only for zone `Type == 4`. Instanced dungeons are also
   `Type == 6` (24 zones, including Lost Vale and the city dungeons), so teleporting into one
   dropped the player into an empty uninstanced map.
3. `InstanceMgr.ZoneIn` dereferenced a `TryGetValue` result on `instance_infos` without checking
   it.

Confirmed in game: `Creature Proto not found boss spawn entry 4275 (bossId 401, zone 260) skipped`
followed in the same millisecond by `Opening Instance Instance ID 1  Map: The Lost Vale`.

Entry 4275 is **deliberately left in the database**. BossID 401 sits between Zaar the Painseeker
(400) and Sechar (402), so it is a genuine 1.4.8 encounter slot whose proto is missing from the
world dump. Deleting it would discard evidence and inventing a proto would be fabrication. It is
now skipped harmlessly and logged. The same class of orphan exists in zones 176 (6 bosses, 1
trash), 177 (2 and 1) and 179 (2); those dungeons now load instead of failing.

## Port 8000

`LauncherServer` failed to bind port 8000 (`SocketException: Only one usage of each socket
address`), which presents as the whole stack failing to start even though WorldServer comes up
normally. The dynamic port range on this machine starts at 1024, and 8000 was the one service port
never added to the exclusion list. Reserved persistently; documented in `CLAUDE.md`.

## What is deliberately not in this repository

ProjectWAR is public. The component that renders the ward sigil client-side, the static analysis
behind it, and the full investigation record are maintained in the private WAR-RE-Toolkit
repository and are distributed to players through the launcher's patch manifest rather than
through source control.

The server half is complete here and safe on its own: `F_WARD_INFO` is an opcode the stock client
discards, so an ordinary client receives it, ignores it, and shows no sigil.

## Still open

- Ward fragment tasks 4 (named boss kills) and 5 (RvR kills) are unimplemented. The client's own
  wording for them is now recorded in `docs/WARD_SYSTEM.md`, so the requirement is specified: both
  need a counter persisted per character and per fragment.
- About 40 of the 99 named help tips have no server trigger yet. Adding one is a `help_tips` row
  plus a one-line `FireHelpTips` call.
- `instance_boss_spawns` Entry 4275 needs a proto if a source is ever found.
