-- 74_place_excavated_skaven_devices.sql
--
-- Corrects the ZoneId on the two Excavated Skaven Devices so they actually appear, and adds a
-- third at a reachable spot for testing Play as Skaven.
--
-- THE PROBLEM. Migration 73 restored both devices (prototype 98811) verbatim from the
-- pre-deletion dump, where both carry ZoneId 100 (Norsca). Their coordinates do not fit Norsca:
-- with OffX/OffY 200/200 they resolve to local (149271, 817757) and (624598, 12195), far outside
-- the 0-65535 a zone spans. An out-of-bounds spawn is never sent to a client, so both were
-- invisible.
--
-- WHERE THEY ACTUALLY BELONG. Each coordinate pair fits exactly one Tier 4 zone:
--
--   968471, 1636957  ->  Dragonwake (205), local 18199, 31325
--   1443798, 831395  ->  Praag (105),      local 34774, 12195
--
-- (The second also fits Dangerous Territory - Kadrin East, a Type-0 sub-zone of no relevance
-- here.) Both are contested Tier 4 RvR lakes, which is exactly where the boss tokens say the
-- devices stand: "at any Excavated Skaven Device within a contested tier four RvR lake, to do
-- your bidding. This item will decay in real time." Praag is also the zone the official
-- "CONTROL A ..." captures were recorded in, where the player interacts with one of these and is
-- offered the four monster forms.
--
-- So the coordinates are right and the ZoneId is wrong. That error is inherited: both rows carry
-- ZoneId 100 in the pre-deletion dump at a4995e92 as well, so it predates the deletion and this
-- project. Only the ZoneId is changed; positions are untouched.

START TRANSACTION;

UPDATE gameobject_spawns SET ZoneId = 205 WHERE Guid = 256544 AND Entry = 98811;
UPDATE gameobject_spawns SET ZoneId = 105 WHERE Guid = 257772 AND Entry = 98811;

-- ---------------------------------------------------------------------------
-- A third device, for testing only.
--
-- Both original placements are in Tier 4 lakes, which need a rank-40 character and an active
-- campaign to reach. This one sits beside the tome tactic test range already staged in Reikwald
-- by migration 67, so the form system can be exercised without that. It is NOT retail data:
-- no capture places a device here. Remove it with the test range.
--
-- Z is the ground height at that pin, matching the range's own creatures.
-- ---------------------------------------------------------------------------

DELETE FROM gameobject_spawns WHERE Guid = 2900001;

INSERT INTO gameobject_spawns
    (Guid, Entry, ZoneId, WorldX, WorldY, WorldZ, WorldO, DisplayID, Unk1, Unk2, Unk3, Unk4,
     Unks, DoorId, VfxState, TokUnlock, SoundId, AllowVfxUpdate, AlternativeName)
VALUES
    (2900001, 98811, 110, 1459268, 969594, 16441, 2048, 9290, 0, 0, 100, 0,
     '7680 1 20966 36 1465 42012', NULL, 0, NULL, 0, 1, NULL);

COMMIT;
