# ProjectWAR AI Rules

This file is the single source of truth for repository-specific AI-agent instructions.

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
