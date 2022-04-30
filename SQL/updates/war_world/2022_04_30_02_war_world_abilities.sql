/* Restrict engineers from casting turret on the go */
UPDATE `abilities` SET `MoveCast` = '0' WHERE (`Entry` = '1526');
UPDATE `abilities` SET `MoveCast` = '0' WHERE (`Entry` = '1518');
UPDATE `abilities` SET `MoveCast` = '0' WHERE (`Entry` = '1511');
/* Restrict white lion form casting call lion on the go */
UPDATE `abilities` SET `MoveCast` = '0' WHERE (`Entry` = '9159');
/* Restrict destro summoner from summoning on the go */
UPDATE `abilities` SET `MoveCast` = '0' WHERE (`Entry` = '8474');
UPDATE `abilities` SET `MoveCast` = '0' WHERE (`Entry` = '8476');
UPDATE `abilities` SET `MoveCast` = '0' WHERE (`Entry` = '8478');
