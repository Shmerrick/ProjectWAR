# Dungeon retest follow-up — 2026-09-05

Continues the stabilization and PQ/Gunbad handoffs. Included in the RESTART stabilization commit.
Latest user retest and release summary: [commit handoff](2026-09-05-commit-handoff.md).
The user confirmed Bastion Destruction influence, its left-wing portal, Bastion ward PQ
credit and Holmsteinn supply visibility. They reported remaining Gunbad influence/levels,
cross-region travel returning to character select, dungeon relog to Avelorn, stale PQ
progress, stacked chests, lockout bypass, Kaarn scale, Skull Lord scripting and Holmsteinn
boundary coverage. Screenshots are user observations, not official restoration evidence.

## Implemented in this pass

- **Dungeon relog:** F_INIT_PLAYER used unset instance exit IDs (materialized as zero)
  against the real `zone_jumps.Entry=0` Avelorn row. Zero is now rejected centrally.
  Recovery selects a configured, in-bounds shared-world exit, otherwise the same-realm
  capital respawn, as explicitly requested by the user. It happens before region insertion;
  the obsolete OnLoad recovery was removed because OnLoad also runs on legitimate entry.
  Missing/invalid capital data fails explicitly without inventing coordinates. Release
  Gunbad and Bastion instance exit fields remain NULL; those cases currently use capitals,
  not purportedly restored dungeon exits. Capital respawns 6/34 are existing configured data.
- **Lockout restoration:** character strings contain `~zone:expiry:boss:boss`, while the
  world cache was keyed `~zone:expiry`. Looking up the full string returned no killed bosses.
  Instance creation now reconstructs progress from the character/leader's own saved list,
  independent of same-day rows belonging to other groups. Joining an existing fresh boss
  encounter with a conflicting personal lockout is rejected. Expired strings do not block.
  Runtime-only reconstructed lockout records are inserted when first persisted, not saved
  as if they were loaded DB objects. Existing character history is untouched. The existing
  midnight reset policy is unchanged; this does **not** claim a fresh 24-hour timer per kill
  or complete multi-player lockout parity. Duplicate legacy world lockout rows remain.
- **PQ progress:** F_OBJECTIVE_UPDATE now uses list kind 2, matching F_OBJECTIVE_INFO,
  rather than neutral dungeon realm 0. Authority: official
  `BASTION STAIR - RIGHT WING DOR LVL 40 RR 100 PQ PATH OF FURY.log.txt.gz`, #597 and #1170.
  End sends the completed/reset form of F_OBJECTIVE_INFO; previously the loot notification
  could appear while the tracker retained the last combat stage. Reference completion
  #25882 (also `bastion_stairs.txt.gz` #46268). Late events are ignored after completion;
  failure sets its reset timestamp and ignores repeated transitions.
- **Chest serialization:** GoldChest.SendMeTo sent a raw-world F_CREATE_STATIC and then
  GameObject.SendMeTo sent another for the same OID in the instance atlas. Removed the
  redundant override; the common writer retains interactability for prototype 188 and
  all Unit appearance/state notifications. This establishes a duplicate-packet defect,
  not proof of three independent PQs. The 09:07:12 log has only one Rise of Carnage chest
  creation. The reported visual stack still needs client verification.
- **Gunbad enclosing influence area:** its painted area map resolves IDs 1..8, but Release
  has only PieceId 1 / AreaId 31. The area lookup therefore returned NULL at most positions.
  Gunbad now retains its enclosing area 31 independently of local painted pieces; other
  zones and PQ boundaries are unchanged. Authority: extracted client
  `interface/interfacecore/maps/zone060/influenceids.csv:2-3` (only area 31, tracks 64/65),
  official `INSTANCE_GUNBAD_PART1.txt.gz` F_UPDATE_STATE #335/#381 and #71415/#71466;
  local PQ-area entries/removals coexist with that enclosing tracker. Gameplay PvP flags
  remain separate. Client tracker visibility and left-wing kill credit require retest.
- **Kaarn and Path of Fury data:** migration 35 applied twice to Release and queried.
  Kaarn prototype 2000751, used only by its zone-165 boss spawn locally, now has scale 55
  instead of 36. `bastion_stairs.txt.gz` F_CREATE_MONSTER #71889/#72001 carries model 1251,
  scale byte payload +20 = 0x37. Path of Fury chest moves to world
  `(1026736,996023,13992)`: F_CREATE_STATIC #46264 carries client `(51888,217783,13992)`,
  with S_PLAYER_INITTED #18276 establishing shift (1,25), and zone offset (240,240).
  The dedicated Path capture repeats the chest at #25878, immediately before completion.
- **Boss cleanup exception:** 09:40:39/49 WorldServer log shows
  StatsInterface.UnitStat.RemoveBonusMultiplier dereferencing an absent standard-bonus
  bucket on Skull Lord combat exit. Missing buckets now make standard cleanup a no-op;
  arithmetic for existing bonuses and stacking classes is unchanged.

All official captures above are under toolkit `libs/protocolservices/Packet Logs`.
Ordinals are 1-based across both directions, reproducible with Read-OfficialPackets.ps1.

## Investigated, not completed

- **Blue orb — explanation retracted:** the cited RvRFlagIndicator code was matched to
  the wrong visual element. The user's subsequent screenshot identifies the red WAR
  symbol as the RvR flag. The separate blue orb remains unidentified. The earlier claim
  that its absence was expected in PvE was unsupported; no UI fix was made.
- **Gunbad levels/layout:** current instance rows frequently override levels to 40;
  prototypes also include 40..42 ranges. Official PART1 contains Redeye Night Goblin
  level 27 #814, Tunnel Runna 27 #821, Blackfang Recluse 23 #923, Squig Herda 25 #926,
  Crystalspine Wyvern 25 #984, Bilebane 26 #8679. Some friendly NPCs genuinely appear at
  40 (e.g. Baagash #810), so a blanket cap/subtraction is wrong. The new read-only
  Get-GunbadLevelEvidence script checks both PART1/PART2 against exact current instance
  XYZ, model and gender-marker-normalized identity after each captured initialization.
  It found no exact matching placements; no level migration was generated. Restoring
  original spawn identities/layout/PQ levels remains open. No hard mode was created.
- **Cross-region character-select return:** user reproduced on a flight to Marshes and a
  Marshes-to-Badlands portal; selecting the character again successfully enters destination.
  Saved Marshes destination matched the enabled Destruction taxi and zone bounds. World
  and client logs gave no definitive failure. Packet logging was requested but no local
  PacketLogs file was available. No speculative handoff/race patch was made.
- **Skull Lord:** return-to-spawn/death choreography and approaching guard spawns remain
  unimplemented/unverified. SimpleSkullLordVarIthrok still has an empty add list and disabled
  scripted events; the bonus-cleanup guard does not restore those mechanics. Inspect the
  dedicated Skull Lord capture's movement/death sequence before implementing it.
- **Holmsteinn northern/eastern boundary:** crate visibility is user-confirmed. Current
  area coverage prevents some crate interactions/credit, per user test. No original
  boundary has been established; do not paint a larger rectangle or guess a radius.
- **Other known gaps:** initial dungeon exit metadata, broader lockout persistence/reset
  semantics, missing overlays, ability-1900 identity/casting, and stage-count discrepancies
  from previous handoffs remain open. No old lockouts or character influence were erased.

## Verification and retest

Release/x64 solution build and standalone runtime checks pass. New checks cover all eight
Gunbad pieces retaining area 31, actual PQ counter/completion packet kinds, late-event
rejection, chest use of the common writer, zero exit rejection, valid exit priority,
realm-specific capitals, invalid/missing recovery data, saved/expired/malformed lockouts,
killed-boss membership and empty/repeated standard bonus cleanup. Synthetic fixture error
messages are intentional (missing rasters and no live chest region).

SELECT-only Release-data PQ construction checks still pass: Holmsteinn 85 first-stage
objects (43 supplies), Destruction of the Weak 36 creatures, no duplicate construction.
Migration 35 data verified directly. None of these checks is an in-client boss fight,
network travel test, or live lockout persistence round trip.

The stack was already stopped cleanly (13:29:12 console-close log) before building. Start
through ServerLauncher, no commit required. Retest relog from Gunbad/Bastion, a locked
Slaurith entry, PQ counters/completion and a single loot chest, Path of Fury chest visibility,
Kaarn size, and Gunbad influence across the left wing. Cross-region failure still needs a
local `.packetlog Bigboy` reproduction; avoid calling the entire backlog fixed.
