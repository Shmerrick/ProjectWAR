-- 39_fix_lotd_warcamp_taxis_and_hieratic_jar.sql
--
-- Two Land of the Dead data faults found by in-client retest.
--
-- USE `war_world`;

USE `war_world`;

-- 1. The two zone-191 taxi rows have their realms swapped.
--
--    Reported: a Destruction player taking the expedition flight from the Inevitable City landed
--    in the *Order* warcamp and was killed there, and an Order player landed in the *Destruction*
--    warcamp. Death respawn was correct for both, which is the clue -- zone_respawns is right and
--    zone_taxis is not.
--
--    Converting the respawn pins to world coordinates (zone 191 OffX 48, OffY 364, so base
--    196608 / 1490944) lines the two sets up and shows the swap, with the Z values matching to
--    the unit:
--
--      respawn 274, realm 2 Destruction -> 254002, 1497939, 10328
--      respawn 275, realm 1 Order       -> 257638, 1536364, 10248
--      taxi RealmID 1 (Order)           -> 254486, 1498271, 10328   <- Destruction warcamp
--      taxi RealmID 2 (Destruction)     -> 257648, 1536559, 10248   <- Order warcamp
--
--    The official capture settles which point belongs to which realm rather than relying on the
--    respawn rows alone. In
--    WAR-RE-Toolkit/libs/protocolservices/Packet Logs/MECHANIC_orderflymaster_NecropoleOFZandri(LoD).txt.gz
--    an Order player takes this exact flight: F_SWITCH_REGION #38 carries zone 0x00BF = 191, and
--    S_PLAYER_INITTED #101 places the arrival at world 257326, 1536497 -- the Order warcamp, and
--    the point this database currently hands to Destruction.
--
--    Only RealmID is wrong, so the coordinates are exchanged between the two rows rather than
--    rewritten. Done via a temporary sentinel because RealmID is part of the row's identity.
--
--    Idempotent: the second run finds the values already in place and the swap is a no-op, since
--    each UPDATE is matched on the coordinates it is moving away from.

UPDATE zone_taxis SET RealmID = 9 WHERE ZoneID = 191 AND RealmID = 1 AND WorldX = 254486;
UPDATE zone_taxis SET RealmID = 1 WHERE ZoneID = 191 AND RealmID = 2 AND WorldX = 257648;
UPDATE zone_taxis SET RealmID = 2 WHERE ZoneID = 191 AND RealmID = 9 AND WorldX = 254486;

-- 2. The Hieratic Jar prototype used by Sedjhet Temple.
--
--    Reported: "Sedjhet Temple stage I jars are not spawning." Objective 2404, "Hieratic Jars
--    Gathered" (Type 3, QUEST_USE_GO, count 10), places 8 objects of prototype 98962, and that
--    prototype does not exist -- one of the ~180 missing public-quest gameobject prototypes
--    tracked as BUG-072.
--
--    Identified from
--    WAR-RE-Toolkit/libs/protocolservices/Packet Logs/LAND OF THE DEAD DOK LVL 40 RR 100 PQ SEDJET TEMPLE (NORMAL - REED GLUPH).log.txt.gz.
--    Zone 191 is not an instance, so capture coordinates are world coordinates directly with no
--    atlas shift. The capture holds 10 F_CREATE_STATIC sightings of an object named
--    "Hieratic Jar", DisplayID 7869, Unk3 100; one of the 8 spawn rows matches a sighting exactly
--    (distance 0) and the others fall 306-328 units away, the jars having moved between cycles of
--    the quest. The name matches the objective verbatim.
--
--    Columns other than the three the capture states follow "Monastery Door" (59) and Holmsteinn
--    Supplies (551), the comparable prototypes already present, rather than being invented.

INSERT IGNORE INTO gameobject_protos
    (Entry, Name, DisplayID, Scale, Level, Faction, HealthPoints, ScriptName, TokUnlock,
     Unk1, Unk2, Unk3, Unk4, UnksString, IsAttackable)
VALUES
    (98962, 'Hieratic Jar', 7869, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0);

UPDATE gameobject_protos SET Name = 'Hieratic Jar', DisplayID = 7869, Unk3 = 100 WHERE Entry = 98962;

-- Verification: expect realm 1 on the Order warcamp (257648) and realm 2 on the Destruction
-- warcamp (254486), and the jar prototype present.
SELECT t.RealmID,
       t.WorldX,
       t.WorldY,
       t.WorldZ,
       (SELECT r.Realm FROM zone_respawns r
         WHERE r.ZoneID = 191
         ORDER BY POW(CAST(196608 + r.PinX AS SIGNED) - CAST(t.WorldX AS SIGNED), 2)
                + POW(CAST(1490944 + r.PinY AS SIGNED) - CAST(t.WorldY AS SIGNED), 2)
         LIMIT 1) AS nearest_respawn_realm
  FROM zone_taxis t
 WHERE t.ZoneID = 191
 ORDER BY t.RealmID;

SELECT Entry, Name, DisplayID, Unk3 FROM gameobject_protos WHERE Entry = 98962;
