REPLACE INTO `creature_smart_abilities`(`UniqueId`,`CreatureTypeId`,`CreatureSubTypeId`,`CreatureTypeDescription`,`Name`,`Speech`,`Condition`,`Execution`,`ExecuteChance`,`CoolDown`,`Sound`) values 
(90,16,92,'HUMANOIDS_HUMANS_GHOUL','ClawSweep','ClawSweep','PlayerInMeleeRange','ClawSweep',100,15,NULL),
(91,16,92,'HUMANOIDS_HUMANS_GHOUL','FoulVomit','FoulVomit','PlayerInMeleeRange','FoulVomit',100,35,NULL);
UPDATE `creature_protos` SET `IsWandering` = 1 WHERE `CreatureType` = 16 AND `CreatureSubType` = 92;