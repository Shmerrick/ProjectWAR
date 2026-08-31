-- Removes a CSV header that was imported as a creature ability row.
--
-- Both numeric identifiers are zero and the text fields contain their column names, so this row
-- can never resolve to either a creature prototype or an ability. The full signature keeps the
-- deletion narrowly scoped and makes the script safe to re-run.

USE `war_world`;

DELETE FROM `creature_abilities`
WHERE `ProtoEntry` = 0
  AND `AbilityId` = 0
  AND `creature_abilities_ID` = 'creature_abilities_ID'
  AND `Text` = 'Text';
