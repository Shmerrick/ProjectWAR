/*
Navicat MySQL Data Transfer

Source Server         : warserver
Source Server Version : 80014
Source Host           : localhost:3306
Source Database       : war_world

Target Server Type    : MYSQL
Target Server Version : 80014
File Encoding         : 65001

Date: 2019-02-10 18:27:42
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for instance_boss_spawns
-- ----------------------------
DROP TABLE IF EXISTS `instance_boss_spawns`;
CREATE TABLE `instance_boss_spawns` (
  `Instance_spawns_ID` varchar(255) NOT NULL,
  `Name` varchar(255) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  `Entry` int(10) unsigned DEFAULT NULL,
  `Realm` tinyint(3) unsigned DEFAULT NULL,
  `Level` tinyint(3) unsigned DEFAULT NULL,
  `Emote` tinyint(3) unsigned DEFAULT NULL,
  `ZoneID` smallint(5) unsigned DEFAULT NULL,
  `InstanceID` smallint(5) unsigned DEFAULT NULL,
  `BossID` int(10) unsigned DEFAULT NULL,
  `SpawnGroupID` int(10) unsigned DEFAULT NULL,
  `WorldX` int(11) DEFAULT NULL,
  `WorldY` int(11) DEFAULT NULL,
  `WorldZ` int(11) DEFAULT NULL,
  `WorldO` int(10) unsigned DEFAULT NULL,
  PRIMARY KEY (`Instance_spawns_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of instance_boss_spawns
-- ----------------------------
INSERT INTO `instance_boss_spawns` VALUES ('15501000', 'Hoarfrost', '10256', '0', '16', '0', '155', '155', '155', '1550', '188046', '219016', '7364', '1960');
INSERT INTO `instance_boss_spawns` VALUES ('15502000', 'Sebcraw the Discarded', '26812', '0', '16', '0', '155', '155', '156', '1551', '191446', '219064', '7546', '2082');
INSERT INTO `instance_boss_spawns` VALUES ('15503000', 'Lorth Thunderbelly', '26814', '0', '17', '0', '155', '155', '157', '1552', '193658', '221214', '7406', '3042');
INSERT INTO `instance_boss_spawns` VALUES ('15504000', 'Slorth Thunderbelly', '26815', '0', '17', '0', '155', '155', '158', '1553', '193671', '220797', '7426', '3042');
INSERT INTO `instance_boss_spawns` VALUES ('15601000', 'Ghalmar Ragehorn', '25721', '0', '14', '0', '156', '156', '156', '1560', '221358', '195471', '11520', '916');
INSERT INTO `instance_boss_spawns` VALUES ('15602000', 'Uzhak the Betrayer', '33180', '0', '14', '0', '156', '156', '157', '1570', '221648', '197173', '11398', '1326');
INSERT INTO `instance_boss_spawns` VALUES ('15603000', 'Vul the Bloodchosen', '33173', '0', '15', '0', '156', '156', '158', '1580', '224039', '200651', '11110', '3546');
INSERT INTO `instance_boss_spawns` VALUES ('1601000', 'Thar\'lgnan^M', '45084', '0', '34', '0', '163', '163', '160', '4000', '998977', '984755', '9004', '2048');
INSERT INTO `instance_boss_spawns` VALUES ('1602000', 'Lord Slaurith^M', '48112', '0', '36', '0', '164', '164', '161', '4001', '1020808', '1001549', '14401', '2048');
INSERT INTO `instance_boss_spawns` VALUES ('1603000', 'Kaarn the Vanquisher^M', '2000751', '0', '39', '0', '165', '165', '162', '4002', '1012370', '1000647', '9439', '2088');
INSERT INTO `instance_boss_spawns` VALUES ('1604000', 'Skull Lord Var\'Ithrok^M', '64106', '0', '40', '0', '166', '166', '163', '4003', '1015782', '1001792', '21999', '4078');
INSERT INTO `instance_boss_spawns` VALUES ('17301000', 'Snaptail the Breeder', '33172', '0', '18', '0', '173', '173', '173', '1730', '669218', '1399983', '15572', '1832');
INSERT INTO `instance_boss_spawns` VALUES ('17302000', 'Goremane', '33182', '0', '19', '0', '173', '173', '174', '1731', '670676', '1403578', '15584', '4002');
INSERT INTO `instance_boss_spawns` VALUES ('17303000', 'Viraxil the Broken', '33181', '0', '20', '0', '173', '173', '175', '1732', '674851', '1401934', '15604', '2866');
INSERT INTO `instance_boss_spawns` VALUES ('1951000', 'Culius Embervine^M', '2000763', '0', '42', '0', '195', '195', '195', '3000', '1557367', '1048053', '11500', '2798');
INSERT INTO `instance_boss_spawns` VALUES ('1952000', 'Sarloth Bloodtouched^M', '46995', '0', '42', '0', '195', '195', '196', '3001', '1589266', '1064332', '7274', '2025');
INSERT INTO `instance_boss_spawns` VALUES ('1953000', 'Korthuk the Raging^M', '2000757', '0', '43', '0', '195', '195', '197', '3002', '1576583', '1046218', '11044', '1012');
INSERT INTO `instance_boss_spawns` VALUES ('1954000', 'Barakus the Godslayer^M', '46205', '0', '44', '0', '195', '195', '198', '3003', '1570000', '1050447', '11232', '2013');
INSERT INTO `instance_boss_spawns` VALUES ('19601000', 'The Bile Lord^m', '52594', '0', '43', '0', '196', '196', '200', '5000', '1500978', '1048689', '11410', '1180');
INSERT INTO `instance_boss_spawns` VALUES ('19601001', 'Ssrydian Morbidae^M', '52462', '0', '42', '0', '196', '196', '201', '5001', '1507572', '1042951', '11762', '1314');
INSERT INTO `instance_boss_spawns` VALUES ('19601002', 'Bartholomeus the Sickly^M', '48128', '0', '43', '0', '196', '196', '202', '5002', '1496965', '1047444', '10506', '3910');
INSERT INTO `instance_boss_spawns` VALUES ('2600000', 'The Darkpromise Beast', '4276', '0', '43', '0', '260', '260', '330', '1995', '1410094', '1584920', '5952', '4039');
INSERT INTO `instance_boss_spawns` VALUES ('2600001', 'Ahzranok^M', '59211', '0', '43', '0', '260', '260', '331', '1996', '1394494', '1583074', '5860', '2844');
INSERT INTO `instance_boss_spawns` VALUES ('2600002', 'Malghor Greathorn^M', '6821', '0', '44', '0', '260', '260', '332', '1997', '1400575', '1582973', '7830', '1025');
INSERT INTO `instance_boss_spawns` VALUES ('2600003', 'Horgulul^F', '6841', '0', '44', '0', '260', '260', '333', '1998', '1395956', '1570861', '6799', '2957');
INSERT INTO `instance_boss_spawns` VALUES ('2600004', 'Dralel the Whitefire Matron^F', '6843', '0', '44', '0', '260', '260', '334', '1999', '1396923', '1559939', '6202', '182');
INSERT INTO `instance_boss_spawns` VALUES ('600000', 'Glomp da Squig Masta^GunbadBoss', '38829', '0', '41', '0', '63', '60', '600', '2000', '929548', '931428', '27229', '1696');
INSERT INTO `instance_boss_spawns` VALUES ('600001', 'Masta Mixa', '37967', '0', '42', '0', '64', '60', '601', '2001', '977574', '994827', '26311', '1540');
INSERT INTO `instance_boss_spawns` VALUES ('600002', '\'Ard ta Feed^GunbadBoss', '15102', '0', '44', '0', '65', '60', '602', '2002', '1261631', '1257872', '20440', '22');
INSERT INTO `instance_boss_spawns` VALUES ('600003', 'Wight Lord Solithex', '42207', '0', '43', '0', '66', '60', '603', '2003', '1110499', '1119641', '19203', '3732');
