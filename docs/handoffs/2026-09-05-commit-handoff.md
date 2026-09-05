# Stabilization delivery and latest retest — 2026-09-05

The user requested documentation, staging, commit and push of all current changes on
RESTART. No additional gameplay changes were made in this documentation/commit pass.
Earlier handoffs saying "uncommitted" describe their historical state; this delivery
collects the stabilization, PQ/Gunbad and dungeon-retest work together.

## Latest user evidence — takes precedence over earlier test status

- "PQs seem to be fixed." Record as user-observed improvement, not blanket confirmation
  of every PQ, chest or boundary.
- Destruction entry from Chaos Wastes into Bastion Stair now lands incorrectly. **Open,
  high priority (BUG-062)**. Earlier left-wing portal success referred to an internal
  portal and must not be used to claim that the external entrance works. Cause not established.
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

1. Diagnose Chaos Wastes -> Bastion entrance regression; capture portal ID and actual arrival.
2. Diagnose cross-region flight/portal returns to character select (Marshes/Badlands).
3. Restore original Gunbad levels/layout with source-backed identities; exact-placement
   audit of both captures returned no matches. No level migration or hard mode was created.
4. Restore Skull Lord guards and return/death choreography; establish Holmsteinn boundaries.
5. Retest chest stacks/placement, Gunbad influence, relog recovery and lockout enforcement;
   investigate lockout reset/persistence semantics, missing overlays and ability-1900 data.
6. Identify the blue orb from the correct UI element; do not repeat the retracted claim.

No server restart is needed merely to commit. Test the built output through ServerLauncher;
a Git commit is not required for local gameplay testing.
