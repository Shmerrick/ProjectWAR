# Master-to-RESTART Commit Audit

Audit date: 2026-08-30
Range: `RESTART..master`
Merge base: `1980c8733975f8898aab1410a96b606d1aa1ea4a`

## Scope and Method

All 716 commits reachable only from `master` were inventoried in topological order. Each commit was classified from its parents, subject, changed paths, and patch. Non-merge code commits were also compared with the current RESTART tree: exact patch IDs, matching RESTART subjects, reverse applicability (already present), forward applicability (clean candidate), and manual review of divergent or large functional patches. Database commits were cross-checked against the curated-master import history and the current database dump lineage.

The initial, exclusive inventory accounts for every commit:

| Cohort | Commits | Disposition |
|---|---:|---|
| Merge commits | 97 | No independent patch to port |
| Exact patch equivalents | 11 | Already on RESTART under another hash |
| Same-subject equivalents | 14 | Already deliberately integrated |
| Reverts/superseded history | 77 | No useful standalone final state |
| Database/data | 125 | Covered by database provenance review |
| Code or mixed changes | 353 | Patch-tested and manually reviewed by subsystem |
| Documentation/metadata | 33 | Obsolete or already replaced |
| Build artifacts/assets | 6 | Do not import repository debris |

## Improvement Adopted

Master commit `be17d15d` exposed a real RESTART defect: several live SQL statements ignored configured schema names. The useful portion was reimplemented against current APIs in `AccountMgr`, `CharMgr`, `WorldMgr`, and `NpcCommands`. Nine queries now use `GetSchemaName()` (or the account RPC equivalent). Obsolete CharacterUtility and project-reference changes from that commit were intentionally excluded.

Master's world snapshot also retained creature packet values that helped identify the native ward field. A later audit established that prototypes are reused across locations with different levels, ranks, and wards, so those values validate the low-bit wire encoding but are not authoritative prototype-wide assignments. `08_move_creature_wards_to_spawns.sql` supersedes and reverses the 79 prototype changes from `07`; future recovery must target concrete spawns using location evidence.

Release/x64 compilation succeeds with zero reported warnings or errors.

## Already Represented or Superseded

The 2025 crash, networking, packet validation, transaction disposal, random-number, instance-spawn, Marauder AI, and performance fixes are already present through RESTART commits, frequently under different hashes. The mastery packet fix (`90533ec5`) is present through the later ability integration even though its patch ID differs.

Master's dependency updates are older than RESTART's current packages. For example, master moved `System.Text.Json` to 8.0.5; RESTART uses 10.0.3. Bepu packages removed on master are already absent. The master-only SSH.NET alert does not apply because RESTART does not reference SSH.NET.

The 2020-2022 movement, AI, quest, and ability histories are long edit/revert chains. Their useful stable outcomes are either already represented or replaced by newer RESTART work. Several apparent fixes are unsafe in final master form:

- random wandering is disabled by an always-true byte comparison, uses a shared `Random`, and assumes eager global LOS initialization;
- the 2022 PQ reward change contains an impossible `killer == null && killer is Pet` condition;
- the final Hearts and Minds script constructs but never spawns the replacement farmer;
- the instance-reset fix is later removed again on master;
- command documentation is manually duplicated, stale, unpaged, and advertises unimplemented commands.

## Database Finding

There are 167 non-merge commits touching SQL/database paths. The prior curated import indexed 112 cooler-SAI SQL commits (including merge metadata), started from a full master database snapshot, retained master ability/AI and creature tables, restored known-good baseline tables, and preserved tables master had dropped. RESTART's `war_world.7z` then received additional schema and data repairs through April 2026.

No master database commit should now be cherry-picked wholesale. Useful retained values must be recovered through reviewed, idempotent incremental scripts and tied to the correct persistence scope; ward evidence, in particular, must resolve to concrete locations rather than prototypes. Commit `4ba4fa07` merely republishes base dumps, and `28e5fb24` repackages archives while deleting account/character base SQL; both conflict with current repository database rules. The 2023 Nordland PQ commit (`7328f2fd`) contains only a database archive and two compiled zone images; validate it against the separately distributed current zone asset bundle rather than importing the commit.

## Candidates Requiring New Work

- `7161f9d7` raises creature follow ranges from 5-6 feet to 10-12 feet. This may improve melee spacing, but it changes combat behavior and leaves a possible null creature dereference. Reimplement only with melee, ranged, and pet chase tests.
- The wandering commits contain a useful feature idea, not reusable production code. A replacement should use the current lazy LOS loader, `StaticRandom`, bounded scheduling, and explicit `IsWandering`/emote semantics.
- `68713f27` suggests discoverable commands. If wanted, generate paginated help from registered handlers instead of copying its static description table.

## Conclusion

RESTART should not merge or bulk-cherry-pick master. The only immediately safe code improvement found in the complete audit was configurable schema usage, now applied. Remaining master-only material is already represented, deliberately superseded, database-history input already consumed by the curated dump, or requires a fresh implementation with targeted tests.
