# Stabilization delivery and latest retest — 2026-09-05

The user requested documentation, staging, commit and push of all current changes on
RESTART. No additional gameplay changes were made in this documentation/commit pass.
Earlier handoffs saying "uncommitted" describe their historical state; this delivery
collects the stabilization, PQ/Gunbad and dungeon-retest work together.

## Latest user evidence — takes precedence over earlier test status

- "PQs seem to be fixed." Record as user-observed improvement, not blanket confirmation
  of every PQ, chest or boundary.
- Destruction entry from Chaos Wastes into Bastion Stair landed incorrectly (BUG-062).
  **Diagnosed and fixed in the follow-up pass at the end of this document.** Earlier
  left-wing portal success referred to an internal portal and must not be used to claim
  that the external entrance works; the in-client walk is still the confirming test.
- The blue-orb explanation was wrong. RvRFlagIndicator source was linked to the wrong
  visual element; the user's screenshot shows the red WAR symbol as the RvR flag.
  The blue orb remains unidentified (BUG-063). The incorrect explanation is retracted
  from the current tracker and dungeon handoff. No orb/UI fix was made.

## Changes included

- Terrain-height caching, image disposal and bounds; independent missing/malformed overlay
  handling; stable region player membership snapshots/counts; reliable dependency staging.
- Influence lookup by InfluenceEntry, 32-bit totals/reward costs and GM input validation.
  Migrations 32/33 restore verified dungeon and seven world-area track bindings.
- Migration 34 restores Holmsteinn's missing supply prototype; deferred PQ startup and
  missing-prototype handling prevent empty/partially aborted spawn sets.
- Chapter/PQ packet ordering, Gunbad tracker classification/enclosing area, objective-list
  update kind, completion refresh and late-event guards; instance-coordinate death jumps.
- Dungeon relog rejects jump zero and selects a valid configured exit or own capital.
  Gunbad/Bastion exit metadata remains absent, so capital fallback is currently expected.
- Character boss-list restoration and conflicting fresh-instance entry checks. Existing
  midnight timer policy and legacy duplicate lockout records were not comprehensively fixed.
- Single common chest creation writer; migration 35 restores Kaarn scale and Path of Fury
  chest position. Missing standard-bonus cleanup no longer throws on boss combat exit.
- Repeatable standalone runtime checks, SELECT-only PQ construction/data audits and
  read-only official capture/level-evidence tools. Documentation records limits and gaps.

## Evidence and validation

Detailed source references are preserved in the three preceding handoffs and migrations:

- [Stabilization](2026-09-05-stabilization.md): client maps/zone060 and zone160
  influenceids.csv; WAR.exe uint32 readers at 0x4C5359 and 0x4DDF60.
- [PQ/Gunbad](2026-09-05-pq-gunbad.md): official Holmsteinn F_CREATE_STATIC #266-268;
  INSTANCE_GUNBAD_PART1 tracker #335/#381 and jump #326292 / initialization #286846.
- [Dungeon retest](2026-09-05-dungeon-retest.md): official Path of Fury objective packets
  #597/#1170/#25882; bastion_stairs Kaarn #71889, chest #46264, atlas initialization #18276.

Release/x64 build and runtime regression suite passed after gameplay edits. SELECT-only
Release-data checks constructed Holmsteinn's 85 objects and Destruction of the Weak's
36 creatures without duplicates. Migrations 32–35 were applied twice and verified against
the configured local Release database. Base SQL dumps are unchanged. These checks are
not a live lockout persistence round trip or proof of correct client travel/rendering.

## Next work, not delivered as fixed

1. ~~Diagnose Chaos Wastes -> Bastion entrance regression.~~ Done — see the follow-up pass below.
2. Diagnose cross-region flight/portal returns to character select (Marshes/Badlands).
3. Restore original Gunbad levels/layout with source-backed identities; exact-placement
   audit of both captures returned no matches. No level migration or hard mode was created.
4. Restore Skull Lord guards and return/death choreography; establish Holmsteinn boundaries.
5. Retest chest stacks/placement, Gunbad influence, relog recovery and lockout enforcement;
   investigate lockout reset/persistence semantics, missing overlays and ability-1900 data.
6. Identify the blue orb from the correct UI element; do not repeat the retracted claim.

No server restart is needed merely to commit. Test the built output through ServerLauncher;
a Git commit is not required for local gameplay testing.

## Follow-up pass — BUG-062 diagnosed and fixed (2026-09-05)

The entrance regression above is resolved. It was not entrance data.

**Diagnosis, from `bin/Release/logs/WorldServer_2026-09-05.log`:**

- `15:07:12.3230` `Opening Realm Instance Instance ID 643 Realm 2 Map: Bastion Stair` — the
  portal, the jump row and the realm instance were all correct.
- `15:07:12.4976` `F_INIT_PLAYER Recovered Bigboy from dungeon 160 to zone 161` — 175 ms
  later, and three minutes after the session's only `F_CONNECT` (`15:04:07`), so this was
  not a login.
- `15:07:46.1774` `SendInited Instance position is outside zone 160 for Bigboy: 439793,143152`
  — the character now held Inevitable City world coordinates (zone 161, OffX 100 / OffY 24)
  while still reporting zone 160.

**Cause.** The BUG-054 dungeon-login recovery was added to `F_INIT_PLAYER` under
`!Plr.IsInWorld()`. The client also re-sends `F_INIT_PLAYER` after the load screen of a
cross-region teleport, and `IsInWorld()` is false during the window where the destination
`RegionMgr` still has the add queued. The saved zone is the Type 4 dungeon at that moment,
so a deliberate portal entry matched the "logged in stuck inside a dungeon" branch, and
`instance_infos` row 160 has NULL exit jump ids, so the capital fallback sent the player to
the Inevitable City.

**Fix.** `WorldServer/NetWork/Handler/MovementHandlers.cs` gates the recovery on
`string.IsNullOrEmpty(Plr.InstanceID)`. `Instance.AddPlayer` assigns `InstanceID` before it
teleports, so legitimate entry is excluded; `Player.Teleport` clears it when the destination
zone Type is below 4, and the field is in-memory only, so a real cold login inside a dungeon
zone still recovers. No database change was needed and no base dump was touched.

**Still open, deliberately not guessed at.** Only The Lost Vale has
`instance_infos.OrderExitZoneJumpID`/`DestrExitZoneJumpID` populated. Every other dungeon,
Bastion Stair and Gunbad included, therefore recovers a genuine stuck login to the realm
capital rather than to its portal exterior. Restoring those ids needs client evidence and
remains item 5 above.

Release/x64 solution build is clean with no new warnings. This is a code-path fix verified
by log analysis and compilation; the in-client walk through the Chaos Wastes portal has not
been repeated and remains the confirming test.
