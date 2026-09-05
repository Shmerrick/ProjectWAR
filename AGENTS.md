# ProjectWAR AI Rules

This file is the single source of truth for repository-specific AI-agent instructions.

## Start of session

Read **`docs/handoffs/2026-09-05-commit-handoff.md`** for the latest user retest and delivery
state. Destruction's Chaos Wastes entrance to Bastion Stair is broken; the blue-orb
identification was retracted. Earlier portal/PQ confirmations do not close those issues.

Read **`docs/handoffs/2026-09-05-stabilization.md`** first. It corrects the earlier checkpoint's
influence-key diagnosis using the client `maps/zone160/influenceids.csv` and
`maps/zone060/influenceids.csv`: runtime lookup uses `chapter_infos.InfluenceEntry`, not `Entry`.
Migrations 32/33 and the 32-bit influence repair must not be reverted to that mistaken premise.

Then read **`docs/handoffs/2026-09-05-checkpoint.md`** for the preceding state of the ward,
dungeon-influence and Bastion Stair work: what is verified in game, what is open with nothing
invented to cover it, the one blocker that shapes the rest (BUG-041, missing zone area bitmaps),
mistakes already made and reverted so they are not repeated, and the conventions in this codebase
that bite — parallel item tables, a `ChapterId` column that is not a chapter, and influence that
fails silently.

## Baseline Repository Rules

1. Read `README.md` before making code or database changes.
2. Pay close attention to the `Database Modification Rules` section in `README.md`.
3. Do not modify the base SQL dumps in `Database/` (`war_accounts.sql`, `war_characters.sql`, `war_world.sql`).
4. If work requires database changes, add a new incremental update script instead of editing the base dumps.
5. Run compile or validation checks before handing work off when the task materially changes code.
6. Apply every new incremental SQL script to the database configured by the local Release build,
   then verify the resulting schema and data so the local server is ready for end-to-end testing.
   Do not claim database-backed functionality was tested when only compilation or an isolated
   schema fixture was validated.
7. Ground restoration work in an authority outside this repository, and search for it before
   deriving it yourself. `docs/CROSS_REPO.md` maps the sources: the WAR-RE-Toolkit repo at
   `D:\Repos\Shmerrick\WAR-RE-Toolkit` (decoded findings in `RE_FINDINGS/`, 1,027 official packet
   captures under `libs/protocolservices/Packet Logs`), the live 1.4.8 client at
   `C:\Users\Admin\Videos\Warhammer Online - Age of Reckoning`, and the extracted client tree at
   `C:\Users\Admin\Downloads\myps`.
8. When sources disagree, the order of authority is: the 1.4.8 client, then official packet
   captures, then decoded toolkit findings, then this repository's own code and world database.
   Existing emulator behavior is the thing being corrected, so it never settles a question about
   what 1.4.8 did.
9. Do not invent game data. Where no authority establishes a value, leave it unpopulated, say so
   explicitly, and record the gap in `docs/INTERNAL_BUG_TRACKER.md` rather than filling it by
   inference from rank, level, name, or a nearby row.
10. Cite the evidence — file, offset, capture, or packet — in the commit message and in any doc
    the change touches. Every factual claim about 1.4.8 in these docs names its source; preserve
    that.

## P10-Inspired Workflow Rules

These rules adapt the parts of Gerard Holzmann's "The Power of Ten" that fit a C# multiplayer server project.

1. Keep control flow simple.
   Prefer straightforward branches, early returns, and explicit state transitions. Do not add recursion in runtime server code unless there is a documented reason it is required and safe.
2. Bound work in loops that run on live server paths.
   Packet handlers, world-update logic, campaign scans, queue drains, and database-driven iterations must have a clear bound, chunking strategy, or time-budget. Avoid unbounded scans in per-tick or per-request code.
3. Avoid unnecessary allocations in hot paths.
   The original rule bans dynamic allocation after initialization; in this project the practical version is to avoid avoidable allocations in per-packet, per-tick, and frequently repeated gameplay code.
4. Keep functions small and single-purpose.
   Split methods once they mix validation, mutation, notifications, persistence, and logging. Packet handlers and battlefront/state-management methods should stay readable as one logical unit.
5. Assert important invariants.
   Add explicit checks for impossible states and violated assumptions, especially around packet parsing, battlefront state, lockouts, object lifetime, and cross-service contracts. Assertions and invariant checks must be side-effect free.
6. Keep variable scope as small as possible.
   Declare variables near first use, avoid reusing locals for unrelated meanings, and prefer narrower scope to reduce state leakage and debugging ambiguity.
7. Check return values and validate inputs.
   Do not ignore meaningful return values from `Try*` methods, database calls, file IO, network operations, or internal helpers. Validate parameters and parsed packet data before acting on them.
8. Keep conditional compilation rare and justified.
   `#if` and related directives should be uncommon, documented, and used only when there is a real platform or build-configuration need.
9. Treat warnings as work, not noise.
   New code should compile cleanly with no new warnings. If a warning or analyzer finding is incorrect or unavoidable, rewrite the code or add a narrowly scoped suppression with a justification comment.
10. Apply stricter review standards to critical-path code.
    Packet handlers, world update loops, battlefront logic, persistence boundaries, and service startup/shutdown code must be reviewed for boundedness, input validation, invariant checks, and hot-path allocation behavior.
