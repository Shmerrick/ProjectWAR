-- 64_tome_tactic_dummies_cloned_from_live_creature.sql
--
-- Supersedes the hand-authored dummy protos from migrations 60, 62 and 63.
--
-- Those rows were built by copying "Practice Target" (36987), which is not spawned anywhere in the
-- world. Every field taken from it was therefore unvalidated, and three of them were wrong in ways
-- that only showed up in the client: Model1 999 is a prop model so the dummies rendered nothing,
-- and the Flag / Icone / States / FigLeafData values left them untargetable. Faction was a red
-- herring -- 65 is used by plenty of live creatures.
--
-- These are instead cloned wholesale from "Griffon War Striker" (114): an Order-faction, melee,
-- ordinary creature with 266 live placements, so every field is proven to work in a running world.
-- Only what must differ is overridden:
--
--   Entry, Name           identify the dummy
--   CreatureType          the whole point -- selects which tactic line acts on it
--   MinLevel / MaxLevel   40 instead of 55, so a rank 40 character can fight them
--   WoundsModifier 5      survive sustained testing
--   WeaponDPS 1           land hits without being a threat
--
-- Everything else -- Faction 75 (Order, attackable by Destruction), Flag, Icone, States,
-- FigLeafData, Model, scale, Ranged -- is inherited from the working creature and must not be
-- hand-edited. They will all look like Griffon War Strikers; the name identifies the line.
--
-- Spawn rows likewise copy the field values used by real zone 5 spawns (Icone 18, Faction 0 so the
-- proto's faction applies, RespawnMinutes 4) rather than invented ones.
--
-- Placement is unchanged from migration 61: a line of ten 200 units apart at Y 921987, 400-1,000
-- units from the Dragonslayer Ridge rally point, where no other creature spawns within ~1,900.
--   Reach them with:  .teleport map 5 1423580 921987 11264
--
-- LIMITATION: Faction 75 is Order, so these are attackable by DESTRUCTION characters only.
--
-- Re-runnable: deletes the dummy protos and spawns first.

START TRANSACTION;

DELETE FROM `creature_spawns` WHERE `Entry` BETWEEN 999500 AND 999509;
DELETE FROM `creature_protos` WHERE `Entry` BETWEEN 999500 AND 999509;

INSERT INTO `creature_protos` SELECT
  999500, 'Daemonic Tactic Dummy', Model1, Model2, MinScale, MaxScale, 40, 40, Faction, Ranged, Icone, Emote, Title,
  Unk, Unk1, Unk2, Unk3, Unk4, Unk5, Unk6, Flag, ScriptName, 10, 0, TokUnlock, LairBoss, VendorID,
  States, FigLeafData, BaseRadiusUnits, Career, PowerModifier, 5.00, Invulnerable, 1, ImmuneToCC, Source
FROM `creature_protos` WHERE `Entry` = 114;
INSERT INTO `creature_protos` SELECT
  999501, 'Beastial Tactic Dummy', Model1, Model2, MinScale, MaxScale, 40, 40, Faction, Ranged, Icone, Emote, Title,
  Unk, Unk1, Unk2, Unk3, Unk4, Unk5, Unk6, Flag, ScriptName, 1, 0, TokUnlock, LairBoss, VendorID,
  States, FigLeafData, BaseRadiusUnits, Career, PowerModifier, 5.00, Invulnerable, 1, ImmuneToCC, Source
FROM `creature_protos` WHERE `Entry` = 114;
INSERT INTO `creature_protos` SELECT
  999502, 'Giant Tactic Dummy', Model1, Model2, MinScale, MaxScale, 40, 40, Faction, Ranged, Icone, Emote, Title,
  Unk, Unk1, Unk2, Unk3, Unk4, Unk5, Unk6, Flag, ScriptName, 21, 0, TokUnlock, LairBoss, VendorID,
  States, FigLeafData, BaseRadiusUnits, Career, PowerModifier, 5.00, Invulnerable, 1, ImmuneToCC, Source
FROM `creature_protos` WHERE `Entry` = 114;
INSERT INTO `creature_protos` SELECT
  999503, 'Greenskin Tactic Dummy', Model1, Model2, MinScale, MaxScale, 40, 40, Faction, Ranged, Icone, Emote, Title,
  Unk, Unk1, Unk2, Unk3, Unk4, Unk5, Unk6, Flag, ScriptName, 15, 0, TokUnlock, LairBoss, VendorID,
  States, FigLeafData, BaseRadiusUnits, Career, PowerModifier, 5.00, Invulnerable, 1, ImmuneToCC, Source
FROM `creature_protos` WHERE `Entry` = 114;
INSERT INTO `creature_protos` SELECT
  999504, 'Chaos Tactic Dummy', Model1, Model2, MinScale, MaxScale, 40, 40, Faction, Ranged, Icone, Emote, Title,
  Unk, Unk1, Unk2, Unk3, Unk4, Unk5, Unk6, Flag, ScriptName, 11, 0, TokUnlock, LairBoss, VendorID,
  States, FigLeafData, BaseRadiusUnits, Career, PowerModifier, 5.00, Invulnerable, 1, ImmuneToCC, Source
FROM `creature_protos` WHERE `Entry` = 114;
INSERT INTO `creature_protos` SELECT
  999505, 'Mythical Tactic Dummy', Model1, Model2, MinScale, MaxScale, 40, 40, Faction, Ranged, Icone, Emote, Title,
  Unk, Unk1, Unk2, Unk3, Unk4, Unk5, Unk6, Flag, ScriptName, 22, 0, TokUnlock, LairBoss, VendorID,
  States, FigLeafData, BaseRadiusUnits, Career, PowerModifier, 5.00, Invulnerable, 1, ImmuneToCC, Source
FROM `creature_protos` WHERE `Entry` = 114;
INSERT INTO `creature_protos` SELECT
  999506, 'Man Tactic Dummy', Model1, Model2, MinScale, MaxScale, 40, 40, Faction, Ranged, Icone, Emote, Title,
  Unk, Unk1, Unk2, Unk3, Unk4, Unk5, Unk6, Flag, ScriptName, 16, 0, TokUnlock, LairBoss, VendorID,
  States, FigLeafData, BaseRadiusUnits, Career, PowerModifier, 5.00, Invulnerable, 1, ImmuneToCC, Source
FROM `creature_protos` WHERE `Entry` = 114;
INSERT INTO `creature_protos` SELECT
  999507, 'Skaven Tactic Dummy', Model1, Model2, MinScale, MaxScale, 40, 40, Faction, Ranged, Icone, Emote, Title,
  Unk, Unk1, Unk2, Unk3, Unk4, Unk5, Unk6, Flag, ScriptName, 18, 0, TokUnlock, LairBoss, VendorID,
  States, FigLeafData, BaseRadiusUnits, Career, PowerModifier, 5.00, Invulnerable, 1, ImmuneToCC, Source
FROM `creature_protos` WHERE `Entry` = 114;
INSERT INTO `creature_protos` SELECT
  999508, 'Undead Tactic Dummy', Model1, Model2, MinScale, MaxScale, 40, 40, Faction, Ranged, Icone, Emote, Title,
  Unk, Unk1, Unk2, Unk3, Unk4, Unk5, Unk6, Flag, ScriptName, 27, 0, TokUnlock, LairBoss, VendorID,
  States, FigLeafData, BaseRadiusUnits, Career, PowerModifier, 5.00, Invulnerable, 1, ImmuneToCC, Source
FROM `creature_protos` WHERE `Entry` = 114;
INSERT INTO `creature_protos` SELECT
  999509, 'Control Dummy (no tactic line)', Model1, Model2, MinScale, MaxScale, 40, 40, Faction, Ranged, Icone, Emote, Title,
  Unk, Unk1, Unk2, Unk3, Unk4, Unk5, Unk6, Flag, ScriptName, 0, 0, TokUnlock, LairBoss, VendorID,
  States, FigLeafData, BaseRadiusUnits, Career, PowerModifier, 5.00, Invulnerable, 1, ImmuneToCC, Source
FROM `creature_protos` WHERE `Entry` = 114;

-- Spawn rows use the field values real zone 5 placements use (Icone 18, Faction 0 so the proto's
-- faction applies, RespawnMinutes 4), not invented ones.
INSERT INTO `creature_spawns`
 (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`Icone`,`Emote`,`Faction`,`WaypointType`,
  `Level`,`Ward`,`Oid`,`RespawnMinutes`,`Enabled`)
VALUES
 (999500,5,1422680,921987,11264,2048,18,0,0,0,40,0,0,4,1),
 (999501,5,1422880,921987,11264,2048,18,0,0,0,40,0,0,4,1),
 (999502,5,1423080,921987,11264,2048,18,0,0,0,40,0,0,4,1),
 (999503,5,1423280,921987,11264,2048,18,0,0,0,40,0,0,4,1),
 (999504,5,1423480,921987,11264,2048,18,0,0,0,40,0,0,4,1),
 (999505,5,1423680,921987,11264,2048,18,0,0,0,40,0,0,4,1),
 (999506,5,1423880,921987,11264,2048,18,0,0,0,40,0,0,4,1),
 (999507,5,1424080,921987,11264,2048,18,0,0,0,40,0,0,4,1),
 (999508,5,1424280,921987,11264,2048,18,0,0,0,40,0,0,4,1),
 (999509,5,1424480,921987,11264,2048,18,0,0,0,40,0,0,4,1);

COMMIT;

-- Verification:
--   SELECT COUNT(*) FROM creature_protos WHERE Entry BETWEEN 999500 AND 999509;  -- 10
--   SELECT COUNT(*) FROM creature_spawns WHERE Entry BETWEEN 999500 AND 999509;  -- 10
--   -- every field except the deliberate overrides must equal the source creature:
--   SELECT COUNT(*) FROM creature_protos d JOIN creature_protos s ON s.Entry = 114
--    WHERE d.Entry BETWEEN 999500 AND 999509
--      AND (d.Model1 <> s.Model1 OR d.Faction <> s.Faction OR d.Flag <> s.Flag
--        OR d.Icone <> s.Icone OR d.Ranged <> s.Ranged
--        OR COALESCE(d.States,'') <> COALESCE(s.States,'')
--        OR COALESCE(d.FigLeafData,'') <> COALESCE(s.FigLeafData,''));   -- 0
