/*
Navicat MySQL Data Transfer

Source Server         : warserver
Source Server Version : 80021
Source Host           : localhost:3306
Source Database       : war_world

Target Server Type    : MYSQL
Target Server Version : 80021
File Encoding         : 65001

Date: 2020-08-16 15:29:55
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for `liveevent_task_infos`
-- ----------------------------
DROP TABLE IF EXISTS `liveevent_task_infos`;
CREATE TABLE `liveevent_task_infos` (
  `Entry` int unsigned NOT NULL,
  `LiveEventId` int unsigned NOT NULL,
  `Name` text,
  `Description` text,
  `TotalTasks` int NOT NULL,
  PRIMARY KEY (`Entry`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of liveevent_task_infos
-- ----------------------------
INSERT INTO `liveevent_task_infos` VALUES ('29', '4', 'Massive Ogres Tyrants Slain', 'Ogres Tyrants Slain', '20');
INSERT INTO `liveevent_task_infos` VALUES ('30', '4', '/Boast or /Toast a member of every career', 'Boast or Toast', '24');
INSERT INTO `liveevent_task_infos` VALUES ('31', '4', 'Drink from 3 steins within the pubs and feast halls of the capital cities', 'Drink from steins', '3');
INSERT INTO `liveevent_task_infos` VALUES ('32', '4', '/Boast 20 different dead enemy players', 'Boast dead enemy players', '20');
INSERT INTO `liveevent_task_infos` VALUES ('33', '4', 'Drink Dwarf Beer Kegs retrieved from Brew-Thirsty Ogres, Drunken Gnoblars, and Explosive Snotlings', 'Drink Dwarf Beer Kegs', '100');
INSERT INTO `liveevent_task_infos` VALUES ('34', '4', 'Explosive Snotlings Slain', 'Snotlings', '50');
INSERT INTO `liveevent_task_infos` VALUES ('35', '4', 'Brew-Thirsty Ogres Slain', 'Ogres', '50');
INSERT INTO `liveevent_task_infos` VALUES ('36', '4', 'Drunken Gnoblar Slain', 'Gnoblar', '50');
INSERT INTO `liveevent_task_infos` VALUES ('37', '4', 'Keg Caches Guzzled', 'Keg Caches', '50');
INSERT INTO `liveevent_task_infos` VALUES ('38', '4', 'Launch 10 Common Fireworks', 'Common Fireworks', '10');
INSERT INTO `liveevent_task_infos` VALUES ('39', '4', 'Launch 5 Impressive Fireworks', 'Impressive Fireworks', '5');
INSERT INTO `liveevent_task_infos` VALUES ('40', '4', 'Launch 1 Magnificent Firework', 'Magnificent Firework', '1');
INSERT INTO `liveevent_task_infos` VALUES ('41', '4', 'Defeat 20 Enemy Players', 'Enemy Players', '20');
INSERT INTO `liveevent_task_infos` VALUES ('42', '4', 'Complete all Keg End Tasks Title: Keg Slayer!', 'Complete all Keg End Tasks', '13');
