-- 48_restore_public_quest_object_prototypes.sql
--
-- Restores 34 missing public-quest game-object prototypes, identified positionally from the
-- official captures.
--
-- Method, the same one migrations 38, 41, 43 and 46 used one prototype at a time. Every
-- affected spawn row is in a Type-0 (non-instance) zone, so its stored world coordinates are
-- the capture's coordinates directly with no atlas shift. For each missing prototype the
-- quest that places it was matched to the capture named for that quest -- the capture set
-- includes 192 files named PQ_<pairing>_<difficulty>_<quest>_CH<n> -- and each spawn position
-- was matched to the nearest F_CREATE_STATIC sighting in it.
--
-- The corroboration that makes this more than nearest-neighbour guessing is that the object's
-- captured NAME matches the objective's own wording: objective \
-- on \
-- \
-- resolved to it, and the closest distance.
--
-- Rejected rather than restored, and left open under BUG-072:
--   100112  -- matched "Reikland Tent", which is not a powder keg
--   2000564  -- matched the generic invisible ground-target marker
--   382  -- matched "Oathbearer Banner" for a Bloody Sun objective; opposing factions
--   547  -- matched the generic invisible ground-target marker
--   99111  -- matched "Fire", which is not a Grudgebreaker Cannon
--   99115  -- matched the generic invisible ground-target marker
--   99659  -- matched the generic invisible ground-target marker
--   99689  -- matched the generic invisible ground-target marker
--   99811  -- matched the generic invisible ground-target marker
--
-- F_CREATE_STATIC payload offsets are those of GameObject.SendMeTo: +16 DisplayID, +28 the
-- uint32 stored here as Unk3, and a Pascal string at +40.
--
-- Idempotent: INSERT IGNORE then fixed assignments.

USE `war_world`;

INSERT IGNORE INTO gameobject_protos
    (Entry, Name, DisplayID, Scale, Level, Faction, HealthPoints, ScriptName, TokUnlock,
     Unk1, Unk2, Unk3, Unk4, UnksString, IsAttackable)
VALUES
    -- Silkens: Cocooned Marauder -- 23 spawn rows, 17/23 resolved here, closest 0 units
    (80      , 'Cocooned Marauder', 132, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- The Webworks: Cocooned Citizen -- 20 spawn rows, 13/20 resolved here, closest 15 units
    (506     , 'Cocooned Citizen', 57, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Kolaz Umgal: Gather Blacksmithing Resources -- 20 spawn rows, 2/20 resolved here, closest 0 units
    (98884   , 'Engineering Supplies', 10, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Salzenmund: Supplies Collected -- 20 spawn rows, 20/20 resolved here, closest 0 units
    (89      , 'Supplies', 16, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Burn Rock Tower: Burn Rock Box -- 19 spawn rows, 7/19 resolved here, closest 0 units
    (98844   , 'Burn Rock Box', 105, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Gassy Mines: Destroy Mining Supplies -- 19 spawn rows, 16/19 resolved here, closest 0 units
    (98849   , 'Mining Supplies', 240, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Da Drakk Cult: Prisoner Cage -- 18 spawn rows, 18/18 resolved here, closest 0 units
    (98752   , 'Prisoner Cage', 4801, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Suderheim: Chests Ransacked -- 14 spawn rows, 10/14 resolved here, closest 0 units
    (100186  , 'Suderheim Chest', 16, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Suderheim: Crates Crushed -- 14 spawn rows, 7/14 resolved here, closest 0 units
    (100188  , 'Suderheim Crate', 174, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Pillagers' Approach: Citizen Militia -- 14 spawn rows, 13/14 resolved here, closest 68 units
    (27      , 'Citizen Militia', 619, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Suderheim: Barrels Destroyed -- 13 spawn rows, 5/13 resolved here, closest 0 units
    (100187  , 'Suderheim Barrel', 260, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Rain of Fire: Destroy Empire Cannons -- 12 spawn rows, 6/12 resolved here, closest 0 units
    (2000565 , 'Empire Cannon', 214, 50, 1, 0, 1, NULL, NULL, 0, 0, 0, 0, '0', 0),
    -- Grundadrakk's Wharf: Stack of Supplies -- 10 spawn rows, 10/10 resolved here, closest 0 units
    (98790   , 'Stack of Supplies', 18, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Spirits of the Shadow: Light the Kindling and Burn the Forest -- 10 spawn rows, 4/10 resolved here, closest 0 units
    (57      , 'Tree Fire', 1462, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Gassy Mines: Destroy Mining Valves -- 10 spawn rows, 10/10 resolved here, closest 0 units
    (98850   , 'Mining Valve', 50, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Ambush at Garrisonburg: Raven Banner -- 10 spawn rows, 7/10 resolved here, closest 0 units
    (100413  , 'Raven Banner', 629, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Suderheim: Baskets Destroyed -- 9 spawn rows, 3/9 resolved here, closest 0 units
    (100209  , 'Basket o'' Fish', 261, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- The Specter of Battle: Hammerstriker Supplies -- 8 spawn rows, 5/8 resolved here, closest 0 units
    (98831   , 'Ammo Crate', 18, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Firebeard's Slayers: Piles of Goods -- 8 spawn rows, 8/8 resolved here, closest 0 units
    (98789   , 'Pile of Goods', 4211, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Da Drakk Cult: Prisoner Cage -- 8 spawn rows, 8/8 resolved here, closest 0 units
    (100645  , 'Magical Dome', 5399, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Broketoof Camp: Claim Broketoof Huts (Orc Flag) -- 7 spawn rows, 6/7 resolved here, closest 0 units
    (98775   , 'Orc Flag', 108, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Duraz Dok: Destroy Cannons -- 7 spawn rows, 7/7 resolved here, closest 0 units
    (119     , 'Cannon', 80, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Altstadt: Claim buildings (Pile of Dirt) -- 6 spawn rows, 6/6 resolved here, closest 0 units
    (98880   , 'Greenskin Flag', 12, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Kolaz Umgal: Gather Blacksmithing Resources -- 6 spawn rows, 6/6 resolved here, closest 0 units
    (98881   , 'Engineering Supplies', 10, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Altstadt: Claim buildings (Pile of Dirt) -- 6 spawn rows, 6/6 resolved here, closest 0 units
    (100642  , 'Greenskin Flag', 12, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Flamerock Mine: Oathbearer Banner -- 5 spawn rows, 4/5 resolved here, closest 0 units
    (100355  , 'Oathbearer Banner', 270, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Mud Flats: Close Small Rifts -- 5 spawn rows, 5/5 resolved here, closest 0 units
    (100619  , 'Small Rift', 1583, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Dragonwatch Falls: Take and defend the Lunar Braziers -- 4 spawn rows, 4/4 resolved here, closest 0 units
    (99594   , 'Brazier Flame', 1761, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Sundered Fortress: Destroy the Fortress Mortars -- 4 spawn rows, 2/4 resolved here, closest 0 units
    (100415  , 'Fortress Mortar', 193, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Steinbruck Manor: Farmhouse Burned -- 4 spawn rows, 4/4 resolved here, closest 0 units
    (2000562 , 'Kindling', 367, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Ruins of Anlec: Ruined Chest -- 3 spawn rows, 1/3 resolved here, closest 0 units
    (100067  , 'Ruined Chest', 1520, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- The Specter of Battle: Hammerstriker Supplies -- 2 spawn rows, 2/2 resolved here, closest 0 units
    (98832   , 'Dynamite Crate', 100, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Broketoof Camp: Claim Shaman Huts (Orc Flag) -- 2 spawn rows, 2/2 resolved here, closest 0 units
    (100638  , 'Orc Flag', 108, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- The Specter of Battle: Destroy Muzinko's Remains -- 1 spawn rows, 1/1 resolved here, closest 0 units
    (98829   , 'Muzinko''s Remains', 230, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    -- Recompense: Captain Syrkin's corpse returned -- 1 spawn rows, 1/1 resolved here, closest 0 units
    (98628   , 'Captain Syrkin''s Corpse', 1541, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0);

-- Correct any row that was already partially present.
UPDATE gameobject_protos SET Name = 'Cocooned Marauder', DisplayID = 132, Unk3 = 100 WHERE Entry = 80;
UPDATE gameobject_protos SET Name = 'Cocooned Citizen', DisplayID = 57, Unk3 = 100 WHERE Entry = 506;
UPDATE gameobject_protos SET Name = 'Engineering Supplies', DisplayID = 10, Unk3 = 100 WHERE Entry = 98884;
UPDATE gameobject_protos SET Name = 'Supplies', DisplayID = 16, Unk3 = 100 WHERE Entry = 89;
UPDATE gameobject_protos SET Name = 'Burn Rock Box', DisplayID = 105, Unk3 = 100 WHERE Entry = 98844;
UPDATE gameobject_protos SET Name = 'Mining Supplies', DisplayID = 240, Unk3 = 100 WHERE Entry = 98849;
UPDATE gameobject_protos SET Name = 'Prisoner Cage', DisplayID = 4801, Unk3 = 100 WHERE Entry = 98752;
UPDATE gameobject_protos SET Name = 'Suderheim Chest', DisplayID = 16, Unk3 = 100 WHERE Entry = 100186;
UPDATE gameobject_protos SET Name = 'Suderheim Crate', DisplayID = 174, Unk3 = 100 WHERE Entry = 100188;
UPDATE gameobject_protos SET Name = 'Citizen Militia', DisplayID = 619, Unk3 = 100 WHERE Entry = 27;
UPDATE gameobject_protos SET Name = 'Suderheim Barrel', DisplayID = 260, Unk3 = 100 WHERE Entry = 100187;
UPDATE gameobject_protos SET Name = 'Empire Cannon', DisplayID = 214, Unk3 = 0 WHERE Entry = 2000565;
UPDATE gameobject_protos SET Name = 'Stack of Supplies', DisplayID = 18, Unk3 = 100 WHERE Entry = 98790;
UPDATE gameobject_protos SET Name = 'Tree Fire', DisplayID = 1462, Unk3 = 100 WHERE Entry = 57;
UPDATE gameobject_protos SET Name = 'Mining Valve', DisplayID = 50, Unk3 = 100 WHERE Entry = 98850;
UPDATE gameobject_protos SET Name = 'Raven Banner', DisplayID = 629, Unk3 = 100 WHERE Entry = 100413;
UPDATE gameobject_protos SET Name = 'Basket o'' Fish', DisplayID = 261, Unk3 = 100 WHERE Entry = 100209;
UPDATE gameobject_protos SET Name = 'Ammo Crate', DisplayID = 18, Unk3 = 100 WHERE Entry = 98831;
UPDATE gameobject_protos SET Name = 'Pile of Goods', DisplayID = 4211, Unk3 = 100 WHERE Entry = 98789;
UPDATE gameobject_protos SET Name = 'Magical Dome', DisplayID = 5399, Unk3 = 100 WHERE Entry = 100645;
UPDATE gameobject_protos SET Name = 'Orc Flag', DisplayID = 108, Unk3 = 100 WHERE Entry = 98775;
UPDATE gameobject_protos SET Name = 'Cannon', DisplayID = 80, Unk3 = 100 WHERE Entry = 119;
UPDATE gameobject_protos SET Name = 'Greenskin Flag', DisplayID = 12, Unk3 = 100 WHERE Entry = 98880;
UPDATE gameobject_protos SET Name = 'Engineering Supplies', DisplayID = 10, Unk3 = 100 WHERE Entry = 98881;
UPDATE gameobject_protos SET Name = 'Greenskin Flag', DisplayID = 12, Unk3 = 100 WHERE Entry = 100642;
UPDATE gameobject_protos SET Name = 'Oathbearer Banner', DisplayID = 270, Unk3 = 100 WHERE Entry = 100355;
UPDATE gameobject_protos SET Name = 'Small Rift', DisplayID = 1583, Unk3 = 100 WHERE Entry = 100619;
UPDATE gameobject_protos SET Name = 'Brazier Flame', DisplayID = 1761, Unk3 = 100 WHERE Entry = 99594;
UPDATE gameobject_protos SET Name = 'Fortress Mortar', DisplayID = 193, Unk3 = 100 WHERE Entry = 100415;
UPDATE gameobject_protos SET Name = 'Kindling', DisplayID = 367, Unk3 = 100 WHERE Entry = 2000562;
UPDATE gameobject_protos SET Name = 'Ruined Chest', DisplayID = 1520, Unk3 = 100 WHERE Entry = 100067;
UPDATE gameobject_protos SET Name = 'Dynamite Crate', DisplayID = 100, Unk3 = 100 WHERE Entry = 98832;
UPDATE gameobject_protos SET Name = 'Orc Flag', DisplayID = 108, Unk3 = 100 WHERE Entry = 100638;
UPDATE gameobject_protos SET Name = 'Muzinko''s Remains', DisplayID = 230, Unk3 = 100 WHERE Entry = 98829;
UPDATE gameobject_protos SET Name = 'Captain Syrkin''s Corpse', DisplayID = 1541, Unk3 = 100 WHERE Entry = 98628;

-- Verification: these 34 prototypes should now exist, and the public-quest object rows that
-- reference a missing prototype should have fallen by the covered-row count.
SELECT COUNT(*) AS restored_prototypes FROM gameobject_protos WHERE Entry IN (
    100067, 100186, 100187, 100188, 100209, 100355, 100413, 100415, 100619, 100638, 100642, 100645, 119, 2000562, 2000565, 27, 506, 57, 80, 89, 98628, 98752, 98775, 98789, 98790, 98829, 98831, 98832, 98844, 98849, 98850, 98880, 98881, 98884, 99594);

SELECT COUNT(*) AS pq_object_rows_still_missing_prototype
  FROM pquest_spawns s LEFT JOIN gameobject_protos g ON g.Entry = s.Entry
 WHERE s.Type = 2 AND g.Entry IS NULL;
