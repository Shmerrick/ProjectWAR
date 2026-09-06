-- 41_restore_tomb_of_the_vulture_lord_traps.sql
--
-- Restores the three trap prototypes Tomb of the Vulture Lord needs, whose absence stopped the
-- dungeon being enterable at all.
--
-- `TOTVL.createPenulums` looked up prototypes 98908, 100489 and 100490 with
-- `GameObjectProtos.TryGetValue` and discarded the result, and every trap constructor
-- dereferences `proto.Name`. None of the three exists, so construction threw out of the TOTVL
-- constructor, the instance was never added, and portal 200797160 appeared to do nothing.
-- `bin/Release/logs/WorldServer_2026-09-05.log` records the NullReferenceException in
-- `Pendulum..ctor` via `createPenulums` at 17:52:20.3048 and 18:00:25.2582. The code now also
-- guards the lookups, so a future gap costs the traps rather than the dungeon.
--
-- Authority: the twelve official Tomb of the Vulture Lord captures under
-- WAR-RE-Toolkit/libs/protocolservices/Packet Logs/ (TOMB OF THE VULTURE LORD DOK LVL 40 RR 100
-- PQ *.log.txt.gz), 2,232 F_CREATE_STATIC frames in total. Payload offsets are those of
-- GameObject.SendMeTo: +2 VfxState, +6 Z, +8 client X, +12 client Y, +16 DisplayID, +28 the
-- uint32 this schema stores as Unk3, and a Pascal string at +40.
--
-- Identification is threefold and independent of naming guesswork.
--
--   1. Names. The captures contain exactly three trap objects: "Pendulum" (186 sightings),
--      "Fire Trap" (264) and "Dart Trap" (71).
--   2. DisplayID. `TOTVL.cs` already hardcodes the display for two of them -- `sp.DisplayID =
--      7394` in the Pendulum constructor and `7471` in DartTrap -- and the captures give
--      Pendulum 7394 and Dart Trap 7471. Firetrap instead reads `proto.DisplayID`, which is why
--      its value has to come from the capture: 7549.
--   3. Position. Converting the coordinates hardcoded in `createPenulums` into the capture's
--      client frame for zone 179 -- OffX/OffY 72/60 with the (1,25) instance atlas shift, so
--      client = world - (Off << 12) + (shift << 13) -- lands each on a sighting of the same
--      name: the first pendulum within 2 units, the first fire trap within 7, the first dart
--      trap within 5.
--
-- All three carry Unk3 = 100 on every sighting, matching Holmsteinn Supplies (551) and the other
-- prototypes restored from captures. Remaining columns follow those prototypes rather than being
-- invented; VfxState is per-spawn and is already passed by the code.
--
-- Idempotent: INSERT IGNORE, then fixed assignments so a partially present row is corrected.

USE `war_world`;

INSERT IGNORE INTO gameobject_protos
    (Entry, Name, DisplayID, Scale, Level, Faction, HealthPoints, ScriptName, TokUnlock,
     Unk1, Unk2, Unk3, Unk4, UnksString, IsAttackable)
VALUES
    ( 98908, 'Pendulum',  7394, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    (100489, 'Dart Trap', 7471, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    (100490, 'Fire Trap', 7549, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0);

UPDATE gameobject_protos SET Name = 'Pendulum',  DisplayID = 7394, Unk3 = 100 WHERE Entry =  98908;
UPDATE gameobject_protos SET Name = 'Dart Trap', DisplayID = 7471, Unk3 = 100 WHERE Entry = 100489;
UPDATE gameobject_protos SET Name = 'Fire Trap', DisplayID = 7549, Unk3 = 100 WHERE Entry = 100490;

-- Verification: expect three rows with the display ids above.
SELECT Entry, Name, DisplayID, Unk3 FROM gameobject_protos WHERE Entry IN (98908, 100489, 100490) ORDER BY Entry;
