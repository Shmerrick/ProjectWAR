UPDATE `war_world`.`characterinfo` SET `Region` = '8', `ZoneId` = '106', `WorldX` = '834641', `WorldY` = '936923', `WorldZ` = '7053', `WorldO` = '2440' WHERE (`CareerLine` = '1');
UPDATE `war_world`.`characterinfo` SET `Region` = '8', `ZoneId` = '106', `WorldX` = '834641', `WorldY` = '936923', `WorldZ` = '7053', `WorldO` = '2440' WHERE (`CareerLine` = '2');
UPDATE `war_world`.`characterinfo` SET `Region` = '8', `ZoneId` = '106', `WorldX` = '834641', `WorldY` = '936923', `WorldZ` = '7053', `WorldO` = '2440' WHERE (`CareerLine` = '3');
UPDATE `war_world`.`characterinfo` SET `Region` = '8', `ZoneId` = '106', `WorldX` = '834641', `WorldY` = '936923', `WorldZ` = '7053', `WorldO` = '2440' WHERE (`CareerLine` = '4');
UPDATE `war_world`.`characterinfo` SET `Region` = '8', `ZoneId` = '106', `WorldX` = '834641', `WorldY` = '936923', `WorldZ` = '7053', `WorldO` = '2440' WHERE (`CareerLine` = '9');
UPDATE `war_world`.`characterinfo` SET `Region` = '8', `ZoneId` = '106', `WorldX` = '834641', `WorldY` = '936923', `WorldZ` = '7053', `WorldO` = '2440' WHERE (`CareerLine` = '10');
UPDATE `war_world`.`characterinfo` SET `Region` = '8', `ZoneId` = '106', `WorldX` = '834641', `WorldY` = '936923', `WorldZ` = '7053', `WorldO` = '2440' WHERE (`CareerLine` = '11');
UPDATE `war_world`.`characterinfo` SET `Region` = '8', `ZoneId` = '106', `WorldX` = '834641', `WorldY` = '936923', `WorldZ` = '7053', `WorldO` = '2440' WHERE (`CareerLine` = '12');
UPDATE `war_world`.`characterinfo` SET `Region` = '8', `ZoneId` = '106', `WorldX` = '834641', `WorldY` = '936923', `WorldZ` = '7053', `WorldO` = '2440' WHERE (`CareerLine` = '17');
UPDATE `war_world`.`characterinfo` SET `Region` = '8', `ZoneId` = '106', `WorldX` = '834641', `WorldY` = '936923', `WorldZ` = '7053', `WorldO` = '2440' WHERE (`CareerLine` = '18');
UPDATE `war_world`.`characterinfo` SET `Region` = '8', `ZoneId` = '106', `WorldX` = '834641', `WorldY` = '936923', `WorldZ` = '7053', `WorldO` = '2440' WHERE (`CareerLine` = '19');
UPDATE `war_world`.`characterinfo` SET `Region` = '8', `ZoneId` = '106', `WorldX` = '834641', `WorldY` = '936923', `WorldZ` = '7053', `WorldO` = '2440' WHERE (`CareerLine` = '20');
UPDATE `war_world`.`characterinfo` SET `Region` = '8', `ZoneId` = '100', `WorldX` = '847879', `WorldY` = '829970', `WorldZ` = '8006', `WorldO` = '3254' WHERE (`CareerLine` = '5');
UPDATE `war_world`.`characterinfo` SET `Region` = '8', `ZoneId` = '100', `WorldX` = '847879', `WorldY` = '829970', `WorldZ` = '8006', `WorldO` = '3254' WHERE (`CareerLine` = '6');
UPDATE `war_world`.`characterinfo` SET `Region` = '8', `ZoneId` = '100', `WorldX` = '847879', `WorldY` = '829970', `WorldZ` = '8006', `WorldO` = '3254' WHERE (`CareerLine` = '7');
UPDATE `war_world`.`characterinfo` SET `Region` = '8', `ZoneId` = '100', `WorldX` = '847879', `WorldY` = '829970', `WorldZ` = '8006', `WorldO` = '3254' WHERE (`CareerLine` = '8');
UPDATE `war_world`.`characterinfo` SET `Region` = '8', `ZoneId` = '100', `WorldX` = '847879', `WorldY` = '829970', `WorldZ` = '8006', `WorldO` = '3254' WHERE (`CareerLine` = '13');
UPDATE `war_world`.`characterinfo` SET `Region` = '8', `ZoneId` = '100', `WorldX` = '847879', `WorldY` = '829970', `WorldZ` = '8006', `WorldO` = '3254' WHERE (`CareerLine` = '14');
UPDATE `war_world`.`characterinfo` SET `Region` = '8', `ZoneId` = '100', `WorldX` = '847879', `WorldY` = '829970', `WorldZ` = '8006', `WorldO` = '3254' WHERE (`CareerLine` = '15');
UPDATE `war_world`.`characterinfo` SET `Region` = '8', `ZoneId` = '100', `WorldX` = '847879', `WorldY` = '829970', `WorldZ` = '8006', `WorldO` = '3254' WHERE (`CareerLine` = '16');
UPDATE `war_world`.`characterinfo` SET `Region` = '8', `ZoneId` = '100', `WorldX` = '847879', `WorldY` = '829970', `WorldZ` = '8006', `WorldO` = '3254' WHERE (`CareerLine` = '21');
UPDATE `war_world`.`characterinfo` SET `Region` = '8', `ZoneId` = '100', `WorldX` = '847879', `WorldY` = '829970', `WorldZ` = '8006', `WorldO` = '3254' WHERE (`CareerLine` = '22');
UPDATE `war_world`.`characterinfo` SET `Region` = '8', `ZoneId` = '100', `WorldX` = '847879', `WorldY` = '829970', `WorldZ` = '8006', `WorldO` = '3254' WHERE (`CareerLine` = '23');
UPDATE `war_world`.`characterinfo` SET `Region` = '8', `ZoneId` = '100', `WorldX` = '847879', `WorldY` = '829970', `WorldZ` = '8006', `WorldO` = '3254' WHERE (`CareerLine` = '24');

delete from war_world.quests_creature_starter where entry = 30361;
delete from war_world.quests_creature_starter where entry = 51821;

INSERT INTO war_world.quests_creature_starter (Entry, CreatureID) VALUES ('30361', '98325');
INSERT INTO war_world.quests_creature_starter (Entry, CreatureID) VALUES ('51821', '198');

