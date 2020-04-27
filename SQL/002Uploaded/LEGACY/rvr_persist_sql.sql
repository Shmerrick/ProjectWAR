ALTER TABLE `war_world`.`rvr_progression` 
ADD COLUMN `LastOwningRealm` INT(11) NOT NULL DEFAULT 0 AFTER `ResetProgressionOnEntry`,
ADD COLUMN `LastOpenedZone` INT(11) NOT NULL DEFAULT 0 AFTER `LastOwningRealm`,
ADD COLUMN `OrderVP` INT(11) NOT NULL DEFAULT 0 AFTER `LastOpenedZone`,
ADD COLUMN `DestroVP` INT(11) NOT NULL DEFAULT 0 AFTER `OrderVP`;




UPDATE `war_world`.`rvr_progression` SET `LastOwningRealm`='0', `LastOpenedZone`='1', `OrderVP`='100', `DestroVP`='200' WHERE `BattleFrontId`='1';
UPDATE `war_world`.`rvr_progression` SET `LastOwningRealm`='1' WHERE `BattleFrontId`='3';
UPDATE `war_world`.`rvr_progression` SET `LastOwningRealm`='2' WHERE `BattleFrontId`='4';
UPDATE `war_world`.`rvr_progression` SET `LastOwningRealm`='1' WHERE `BattleFrontId`='6';
UPDATE `war_world`.`rvr_progression` SET `LastOwningRealm`='1' WHERE `BattleFrontId`='7';
UPDATE `war_world`.`rvr_progression` SET `LastOwningRealm`='2' WHERE `BattleFrontId`='9';
UPDATE `war_world`.`rvr_progression` SET `LastOwningRealm`='2' WHERE `BattleFrontId`='11';
UPDATE `war_world`.`rvr_progression` SET `LastOwningRealm`='0', `LastOpenedZone`='1', `OrderVP`='200', `DestroVP`='300' WHERE `BattleFrontId`='12';
UPDATE `war_world`.`rvr_progression` SET `LastOwningRealm`='1' WHERE `BattleFrontId`='10';
UPDATE `war_world`.`rvr_progression` SET `LastOwningRealm`='1' WHERE `BattleFrontId`='2';