/*
Navicat MySQL Data Transfer

Source Server         : warserver
Source Server Version : 80014
Source Host           : localhost:3306
Source Database       : war_world

Target Server Type    : MYSQL
Target Server Version : 80014
File Encoding         : 65001

Date: 2019-02-10 18:27:54
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for instance_infos
-- ----------------------------
DROP TABLE IF EXISTS `instance_infos`;
CREATE TABLE `instance_infos` (
  `instance_infos_ID` varchar(255) NOT NULL,
  `Entry` smallint(5) unsigned DEFAULT NULL,
  `ZoneID` smallint(5) unsigned DEFAULT NULL,
  `Name` text,
  `LockoutTimer` int(10) unsigned DEFAULT NULL,
  `TrashRespawnTimer` int(10) unsigned DEFAULT NULL,
  `WardsNeeded` tinyint(3) unsigned DEFAULT NULL,
  `OrderExitZoneJumpID` int(11) NOT NULL,
  `DestrExitZoneJumpID` int(11) NOT NULL,
  PRIMARY KEY (`instance_infos_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of instance_infos
-- ----------------------------
INSERT INTO `instance_infos` VALUES ('155A', '155', '155', 'Sacellum Dungeons West Wing', '1440', '5', null, '0', '0');
INSERT INTO `instance_infos` VALUES ('156A', '156', '156', 'Sacellum Dungeons East Wing', '1440', '5', null, '0', '0');
INSERT INTO `instance_infos` VALUES ('160A', '160', '160', 'Bastion Stair', '1440', '5', null, '167782696', '168030760');
INSERT INTO `instance_infos` VALUES ('160B', '163', '163', 'Thar\'lgnan', '1440', '5', null, '167782696', '168030760');
INSERT INTO `instance_infos` VALUES ('160C', '164', '164', 'Lord Slaurith', '1440', '5', null, '167782696', '168030760');
INSERT INTO `instance_infos` VALUES ('160D', '165', '165', 'Kaarn the Vanquisher', '1440', '5', null, '167782696', '168030760');
INSERT INTO `instance_infos` VALUES ('160E', '166', '166', 'Skull Lord Var\'throk', '1440', '5', null, '167782696', '168030760');
INSERT INTO `instance_infos` VALUES ('173A', '173', '173', 'Sacellum Dungeons Middle Wing', '1440', '5', null, '181443176', '0');
INSERT INTO `instance_infos` VALUES ('179A', '179', '179', 'Tomb Of The Vulture Lord', '1440', '5', null, '0', '0');
INSERT INTO `instance_infos` VALUES ('195A', '195', '195', 'Bloodwrought Enclave', '1440', '5', null, '0', '0');
INSERT INTO `instance_infos` VALUES ('196A', '196', '196', 'Bilerot Burrow', '1440', '5', null, '0', '0');
INSERT INTO `instance_infos` VALUES ('260A', '260', '260', 'The Lost Vale', '1440', '5', null, '26000001', '26000002');
INSERT INTO `instance_infos` VALUES ('60A', '60', '60', 'Gunbad', '1440', '5', null, '0', '0');
INSERT INTO `instance_infos` VALUES ('60B', '60', '63', 'Gunbad Nursery', '1440', '5', null, '0', '0');
INSERT INTO `instance_infos` VALUES ('60C', '60', '64', 'Gunbad Lab', '1440', '5', null, '0', '0');
INSERT INTO `instance_infos` VALUES ('60D', '60', '65', 'Squig Boss', '1440', '5', null, '0', '0');
INSERT INTO `instance_infos` VALUES ('60E', '60', '66', 'Gunbad Baracks', '1440', '5', null, '0', '0');
