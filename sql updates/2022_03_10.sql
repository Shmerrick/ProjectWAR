/* Restrict engineers from casting turret on the go */
UPDATE `war_world`.`abilities` SET `MoveCast` = '0' WHERE (`Entry` = '1526');
UPDATE `war_world`.`abilities` SET `MoveCast` = '0' WHERE (`Entry` = '1518');
UPDATE `war_world`.`abilities` SET `MoveCast` = '0' WHERE (`Entry` = '1511');
/* Restrict white lion form casting call lion on the go */
UPDATE `war_world`.`abilities` SET `MoveCast` = '0' WHERE (`Entry` = '9159');
/* Restrict destro summoner from summoning on the go */
UPDATE `war_world`.`abilities` SET `MoveCast` = '0' WHERE (`Entry` = '8474');
UPDATE `war_world`.`abilities` SET `MoveCast` = '0' WHERE (`Entry` = '8476');
UPDATE `war_world`.`abilities` SET `MoveCast` = '0' WHERE (`Entry` = '8478');
