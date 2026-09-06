# Review corrections — 2026-09-06

Fixes the five findings from the review of `70aa1e4e..f1886013`. Changes are local;
no commit, push, server launch or in-client run was performed.

## Runtime corrections

- `Creature.CreditQuestKill` now calls the ordinary-creature PQ hook before the Tome
  subtype guard. The reviewed Release `creature_protos` rows for Bloodherd Champion
  2000682 and Eternal Vanguard 94104 both have CreatureSubType 0; objectives 1518 and
  2487 name those targets. A missing Tome classification no longer suppresses PQ credit.
- `Creature.CreditPublicQuestKill` counts a death once per attached quest instance,
  rather than once across all contributors' quests. The finite damage-source set bounds
  the work; a set is allocated only when an eligible quest is encountered. Separate
  realm definitions such as Release pquest_info 592/901 can both advance, while multiple
  contributors to the same quest cannot multiply its count. Existing contribution
  handling inside PublicQuest.HandleEvent is unchanged.
- `InstanceMgr.ResolveCharacterLockout` takes the entrance jump and resolves its ZoneID.
  Release zone_jumps 62915688/62915752/62915816/62915880 have destinations 63/64/66/65
  but all carry InstanceID 60. `Instance.ApplyLockout` writes the actual ZoneID into the
  character lockout. Reading the shared dungeon ID lost the saved boss list and caused
  the fresh-encounter compatibility check to reject returning players. All entry paths
  now use the destination map. Expiration and legacy duplicate-record policies remain open.

These are source/database contract corrections, not new claims about retail game data.

## Archival recovery and deletion audit

Migration 46 deleted 24 creature_spawns rows for entry 2000689, zone 163. Migration 49
restored only pquest_spawns archives. New migration
`51_archive_deleted_bastion_creature_placements.sql` preserves the 24 creatures in
`creature_spawns_unresolved` without introducing live spawns.

Source: untouched `Database/war_world.7z`, contained `war_world.sql`, creature_spawns
GUIDs 1081553 through 1081576. All 15 original columns are copied verbatim; all 24 source
rows already had Enabled=0. Later schema fields use table defaults. These historical
emulator records still lack an established retail identity/placement; no creature
prototype or coordinate conversion was invented. Keep that evidence gap open.

Migration 47's validation previously joined spawns before testing COUNT(*)=0, making
an empty objective impossible to detect. Its SELECT now uses EXISTS on the archive and
NOT EXISTS on live spawns. Migration 51 also reports affected empty objectives, so
existing installations receive the corrected audit without replaying migration 47's
mutations. An audit hit requires investigation: world/script spawns can serve an objective.

## Verification

- Release/x64 solution build passed with no reported compiler warnings or errors.
- Runtime suite covers subtype-zero kills, duplicate contributors, independent realm
  quests, unmatched/cross-zone quests, zero damage, PQ-owned creature exclusion, and all
  four Gunbad boss destinations carrying InstanceID 60. Existing regression checks pass.
- Migration 51 applied twice to the database selected by `bin/Release/Configs/World.xml`.
  Archive count 24, disabled count 24, matching live count 0. Every original column and
  expected archive schema column was verified with `Test-ArchiveRecovery.ps1`.
- The SELECT-only archive test also runs migration 47's actual count expression against
  derived fixtures and detects the affected empty objective while excluding populated
  and unrelated objectives. The actual Release empty-objective audit returns no rows.
- Existing database-backed PQ construction tests pass for Holmsteinn Revisited (85
  objects) and Destruction of the Weak (36 creatures).
- Base dumps remain unchanged. The 1,842 PQ archive rows verified during review remain
  separate from these 24 recovered creature records.

Client retests remain necessary: advance the named PQ stages, share a kill while attached
to different realm quests, and return to a Gunbad boss map with an unexpired saved kill.
Use ServerLauncher for gameplay testing; build/tests are not a live persistence round trip.
