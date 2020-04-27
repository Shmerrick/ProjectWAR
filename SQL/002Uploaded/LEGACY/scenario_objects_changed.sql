/*
Navicat MySQL Data Transfer

Source Server         : warserver
Source Server Version : 80014
Source Host           : localhost:3306
Source Database       : war_world

Target Server Type    : MYSQL
Target Server Version : 80014
File Encoding         : 65001

Date: 2019-02-20 21:45:20
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for scenario_objects
-- ----------------------------
DROP TABLE IF EXISTS `scenario_objects`;
CREATE TABLE `scenario_objects` (
  `ScenarioId` smallint(5) unsigned DEFAULT NULL,
  `Identifier` smallint(5) unsigned DEFAULT NULL,
  `ObjectiveName` varchar(255) DEFAULT NULL,
  `Type` varchar(255) DEFAULT NULL,
  `WorldPosX` int(10) DEFAULT NULL,
  `WorldPosY` int(10) DEFAULT NULL,
  `PosZ` smallint(5) unsigned DEFAULT NULL,
  `Heading` smallint(5) unsigned DEFAULT NULL,
  `PointGain` tinyint(3) unsigned DEFAULT NULL,
  `PointOverTimeGain` tinyint(3) unsigned DEFAULT NULL,
  `ProtoEntry` int(10) unsigned DEFAULT NULL,
  `CaptureObjectiveText` text,
  `CaptureObjectiveDescription` text,
  `HoldObjectiveText` text,
  `HoldObjectiveDescription` text,
  `CaptureAnnouncement` text,
  `scenario_objects_ID` varchar(36) NOT NULL,
  `Realm` tinyint(3) unsigned DEFAULT NULL,
  PRIMARY KEY (`scenario_objects_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of scenario_objects
-- ----------------------------
INSERT INTO `scenario_objects` VALUES ('2104', '15', 'Glyph of Fury', 'Murderball', '556598', '364774', '5516', '3106', null, null, '99855', null, null, null, null, null, '1', null);
INSERT INTO `scenario_objects` VALUES ('2109', '7019', 'Fallen Bridge', 'Flag', '621043', '364903', '5734', '4084', '10', '2', null, null, null, null, null, null, '11dc5e47-59b9-11e6-9679-00ff0731887a', null);
INSERT INTO `scenario_objects` VALUES ('2104', '16', 'Glyph of Madness', 'Murderball', '554130', '364594', '5469', '1024', null, null, '99857', null, null, null, null, null, '2', null);
INSERT INTO `scenario_objects` VALUES ('2003', '5003', 'Gungnir\'s Bridge', 'Flag', null, null, null, null, '15', '2', null, null, null, null, null, null, '20032003', null);
INSERT INTO `scenario_objects` VALUES ('2003', '5004', 'Kordhal\'s Lookout', 'Flag', null, null, null, null, '15', '2', null, null, null, null, null, null, '20032004', null);
INSERT INTO `scenario_objects` VALUES ('2003', '5005', 'Slayer Basin', 'Flag', null, null, null, null, '15', '2', null, null, null, null, null, null, '20032005', null);
INSERT INTO `scenario_objects` VALUES ('2003', '5006', 'Swamp Rat Marsh', 'Flag', null, null, null, null, '15', '2', null, null, null, null, null, null, '20032006', null);
INSERT INTO `scenario_objects` VALUES ('2003', '5007', 'Borradin Ruins', 'Flag', null, null, null, null, '15', '2', null, null, null, null, null, null, '20032007', null);
INSERT INTO `scenario_objects` VALUES ('2005', '5026', 'Fireball Mortar', 'Flag', '729101', '200491', '6416', '2034', '15', '2', null, null, null, null, null, null, '20052005', '1');
INSERT INTO `scenario_objects` VALUES ('2005', '5027', 'Gungnir\'s Fist', 'Flag', '721102', '200376', '5542', '2096', '15', '2', null, null, null, null, null, null, '20052006', '1');
INSERT INTO `scenario_objects` VALUES ('2005', '5025', 'Gyrocopter Hangar', 'Flag', '724993', '196724', '5635', '2074', '15', '2', null, null, null, null, null, null, '20052007', '0');
INSERT INTO `scenario_objects` VALUES ('2005', '5028', 'Blasting Cart', 'Flag', '720456', '191533', '6385', '2012', '15', '2', null, null, null, null, null, null, '20052008', '2');
INSERT INTO `scenario_objects` VALUES ('2005', '5029', 'Firing Range', 'Flag', '729408', '191501', '5484', '1876', '15', '2', null, null, null, null, null, null, '20052009', '2');
INSERT INTO `scenario_objects` VALUES ('2007', '5015', 'Northern Ruins', 'Flag', null, null, null, null, '15', '2', null, null, null, null, null, null, '20072007', null);
INSERT INTO `scenario_objects` VALUES ('2007', '5016', 'Central Ruins', 'Flag', null, null, null, null, '15', '2', null, null, null, null, null, null, '20072008', null);
INSERT INTO `scenario_objects` VALUES ('2007', '5017', 'Southern Ruins', 'Flag', null, null, null, null, '15', '2', null, null, null, null, null, null, '20072009', null);
INSERT INTO `scenario_objects` VALUES ('2012', '7014', 'Militia Quarters North', 'Flag', null, null, null, null, null, null, null, null, null, null, null, null, '20122012', null);
INSERT INTO `scenario_objects` VALUES ('2012', '7015', 'Militia Quarters South', 'Flag', null, null, null, null, null, null, null, null, null, null, null, null, '20122013', null);
INSERT INTO `scenario_objects` VALUES ('2012', '7013', 'Warmachine Quarters South', 'Flag', null, null, null, null, null, null, null, null, null, null, null, null, '20122014', null);
INSERT INTO `scenario_objects` VALUES ('2012', '7012', 'Warmachine Quarters North', 'Flag', null, null, null, null, null, null, null, null, null, null, null, null, '20122015', null);
INSERT INTO `scenario_objects` VALUES ('2012', '7001', 'The Courtyard', 'Flag', null, null, null, null, null, null, null, null, null, null, null, null, '20122016', null);
INSERT INTO `scenario_objects` VALUES ('2013', '7017', 'Corrupters Crown', 'Flag', null, null, null, null, null, null, null, null, null, null, null, null, '20132013', null);
INSERT INTO `scenario_objects` VALUES ('2013', '7018', 'Soul Blight Stone', 'Flag', null, null, null, null, null, null, null, null, null, null, null, null, '20132014', null);
INSERT INTO `scenario_objects` VALUES ('2013', '7040', 'Soul Blight North', 'Flag', null, null, null, null, null, null, null, null, null, null, null, null, '20132015', null);
INSERT INTO `scenario_objects` VALUES ('2013', '7042', 'Soul Blight Court', 'Flag', null, null, null, null, null, null, null, null, null, null, null, null, '20132016', null);
INSERT INTO `scenario_objects` VALUES ('2013', '7043', 'Soul Blight OverlookOverlook', 'Flag', null, null, null, null, null, null, null, null, null, null, null, null, '20132017', null);
INSERT INTO `scenario_objects` VALUES ('2013', '7049', 'Upper Temple', 'Flag', null, null, null, null, null, null, null, null, null, null, null, null, '20132018', null);
INSERT INTO `scenario_objects` VALUES ('2013', '7048', 'Lower Temple', 'Flag', null, null, null, null, null, null, null, null, null, null, null, null, '20132019', null);
INSERT INTO `scenario_objects` VALUES ('2109', '7020', 'The Factory', 'Flag', '625879', '367208', '6199', '4084', '10', '2', null, null, null, null, null, null, '21dc5e47-59b9-11e6-9679-00ff0731887a', null);
INSERT INTO `scenario_objects` VALUES ('2104', '24', 'Warpstone Talisman', 'Murderball', '555424', '364779', '5516', '2139', null, null, '99856', null, null, null, null, null, '3', null);
INSERT INTO `scenario_objects` VALUES ('2109', '7021', 'The Mill', 'Flag', '615971', '362595', '5867', '4084', '10', '2', null, null, null, null, null, null, '31dc5e47-59b9-11e6-9679-00ff0731887a', null);
INSERT INTO `scenario_objects` VALUES ('2205', '7065', 'The Summoning Tower', 'Flag', '688188', '370079', '7481', '2058', '10', '2', null, null, null, null, null, null, 'b13c5e47-59b9-11e6-9679-00ff0731887a', null);
INSERT INTO `scenario_objects` VALUES ('2205', '7066', 'The Commorancy', 'Flag', '688225', '358806', '7193', '3808', '10', '2', null, null, null, null, null, null, 'b14c5e47-59b9-11e6-9679-00ff0731887a', null);
INSERT INTO `scenario_objects` VALUES ('2205', '7063', 'The Academy', 'Flag', '688391', '364403', '6758', '1240', '20', '5', null, null, null, null, null, null, 'b1bc5e47-59b9-11e6-9679-00ff0731887a', null);
INSERT INTO `scenario_objects` VALUES ('2006', '5013', 'Logrin\'s Hammer', 'Capture Point', '577497', '359733', '8927', '3060', null, null, '99920', 'Interact with the grave to capture %n!', 'Fight for control of %n!', 'Maintain control of %n!', 'Your realm controls %n', 'has captured', 'b1dba587-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2006', '5012', 'Logrin\'s Anvil', 'Capture Point', '585444', '359691', '8927', '2036', null, null, '99919', 'Interact with %n to capture it!', 'Fight for control of %n!', 'Maintain control of %n!', 'Your realm controls %n', 'has captured', 'b1dbc8c7-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2004', '5033', 'Da Base Camp', 'Flag', '314461', '511916', '8115', null, '35', '10', null, null, null, null, null, null, 'b1dbd3fb-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2004', '5032', 'Pile O\' Warstuff', 'Flag', '321601', '509599', '8603', null, '25', '5', null, null, null, null, null, null, 'b1dbda89-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2004', '5031', 'The Bridge', 'Flag', '319048', '516336', '8806', null, '15', '2', null, null, null, null, null, null, 'b1dbe37a-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2004', '5022', 'The Repair Depot', 'Flag', '316619', '522275', '8798', null, '25', '5', null, null, null, null, null, null, 'b1dbe426-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2004', '5021', 'Engine Number 9', 'Flag', '323797', '520961', '8101', null, '35', '10', null, null, null, null, null, null, 'b1dbe4bf-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2011', '1', 'Doomfist Ore', 'Murderball', '587219', '193715', '3672', '0', null, null, '99928', null, null, null, null, null, 'b1dbf47d-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2011', '2', 'Doomfist Ore', 'Murderball', '589839', '199230', '3264', null, null, null, '99928', null, null, null, null, null, 'b1dbf520-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2011', '3', 'Doomfist Ore', 'Murderball', '592801', '193937', '3248', null, null, null, '99928', null, null, null, null, null, 'b1dc0105-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2011', '5030', 'Doomfist Crater', 'Flag', '589497', '194879', '3695', '856', '30', '3', null, null, null, null, null, null, 'b1dc01b4-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2201', '22', 'Order Flag', 'Flag', '421291', '365550', '6573', '2048', '150', null, '98730', null, null, null, null, null, 'b1dc0e30-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2201', '23', 'Destruction Flag', 'Flag', '422464', '355272', '6470', '0', '150', null, '98728', null, null, null, null, null, 'b1dc19f7-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2002', '26', 'Flag', 'Flag', '460414', '357514', '5514', '2048', '80', null, '98730', null, null, null, null, null, 'b1dc1a96-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2101', '7069', 'Chuck Rock Trolls', 'Capture Point', '423239', '364217', '7920', null, '35', null, '99926', 'Pacify the %n', 'Pacify the %n. You can only control this point while holding the Troll Pacifier. You can find the Pacifier at the top of Stone Troll Hill.', 'Maintain control of the %n!', 'Your realm has pacified the %n', 'has pacified the', 'b1dc1b3c-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2101', '7070', 'Gravel Bottom Trolls', 'Capture Point', '420888', '358267', '7904', null, '35', null, '99926', 'Pacify the %n', 'Pacify the %n. You can only control this point while holding the Troll Pacifier. You can find the Pacifier at the top of Stone Troll Hill.', 'Maintain control of the %n!', 'Your realm has pacified the %n', 'has pacified the', 'b1dc1bf1-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2101', '7071', 'Boulder Fist Trolls', 'Capture Point', '424956', '357502', '7721', null, '35', null, '99926', 'Pacify the %n', 'Pacify the %n. You can only control this point while holding the Troll Pacifier. You can find the Pacifier at the top of Stone Troll Hill.', 'Maintain control of the %n!', 'Your realm has pacified the %n', 'has pacified the', 'b1dc2700-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2103', '5043', 'The Stag', 'Capture Point', '425389', '423476', '15830', '1012', null, null, '200004', 'Interact with the grave to capture %n!', 'Fight for control of %n!', 'Maintain control of %n!', 'Your realm controls the %n', 'has captured', 'b1dc34b5-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2202', '1', 'Brimstone Bauble', null, '489442', '362040', '6900', '3882', null, null, '99928', null, null, null, null, null, 'b1dc4019-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2203', '5018', 'Temple of Isha', 'Flag', '753811', '366392', '11103', null, '30', '3', null, null, null, null, null, null, 'b1dc40cd-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2101', '12', 'Troll Pacifier', 'Flag', '423095', '360652', '9127', null, null, null, null, null, null, null, null, null, 'b1dc415b-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2203', '1', 'Isha\'s Will', 'Pityball', '753553', '358741', '12480', '134', null, null, '99922', null, null, null, null, null, 'b1dc41e6-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2100', '5100', 'The Fortress', 'Flag', '353573', '357844', '4832', '1251', '30', '4', null, null, null, null, null, null, 'b1dc4273-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2100', '5101', 'The Barracks', 'Flag', '358163', '354382', '3384', '3276', '15', '2', null, null, null, null, null, null, 'b1dc4d37-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2100', '5102', 'The Lighthouse', 'Flag', '358994', '360908', '4857', '3219', '15', '2', null, null, null, null, null, null, 'b1dc4dcb-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2015', '5053', 'The Engine Room', 'Flag', '308074', '185029', '3708', '1420', '15', '2', null, null, null, null, null, null, 'b1dc4dcb-59b9-11e6-9679-00ff073118aa', null);
INSERT INTO `scenario_objects` VALUES ('2015', '5052', 'The Central Gangway', 'Flag', '309026', '185720', '4199', '1394', '15', '2', null, null, null, null, null, null, 'b1dc4dcb-59b9-11e6-9679-00ff073118ab', null);
INSERT INTO `scenario_objects` VALUES ('2015', '5050', 'The Upper Deck', 'Flag', '310035', '186408', '4769', '3470', '15', '2', null, null, null, null, null, null, 'b1dc4dcb-59b9-11e6-9679-00ff073118ac', null);
INSERT INTO `scenario_objects` VALUES ('2108', '7007', 'The Warehouse', 'Flag', '362559', '423347', '6420', '512', '15', '3', null, null, null, null, null, null, 'b1dc5b71-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2108', '7008', 'The Machine Shop', 'Flag', '358119', '423514', '6408', '3608', '15', '3', null, null, null, null, null, null, 'b1dc5c10-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2000', '5000', 'Gate Switch', 'Flag', '194377', '194176', '9800', '1251', '30', '2', null, null, null, null, null, null, 'b1dc5cad-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2108', '7009', 'The Steamtank Plant', 'Flag', '360454', '426358', '6431', null, '50', '1', null, null, null, null, null, null, 'b1dc5d41-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2000', '5001', 'Ammunitions Cache', 'Flag', '192283', '194607', '10100', null, '15', '2', null, null, null, null, null, null, 'b1dc5dce-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2105', '7003', 'The Atrium', 'Flag', '688158', '358116', '19331', '3632', '15', '2', null, null, null, null, null, null, 'b1dc5e47-19b9-11e6-9679-00ff0731887a', null);
INSERT INTO `scenario_objects` VALUES ('2105', '7004', 'The Loft', 'Flag', '688153', '358097', '21963', '514', '15', '2', null, null, null, null, null, null, 'b1dc5e47-29b9-11e6-9679-00ff0731887a', null);
INSERT INTO `scenario_objects` VALUES ('2000', '5002', 'Supply Room', 'Flag', '196495', '193736', '10100', '3276', '15', '2', null, null, null, null, null, null, 'b1dc5e47-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2001', '1', 'Mourkain Artifact', null, '331700', '191645', '2871', null, null, null, null, null, null, null, null, null, 'b1dc6c4a-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2008', '1', 'Powder Keg', null, '453266', '522218', '5829', '2436', null, null, null, null, null, null, null, null, 'b1dc6cf3-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2008', '2', 'Gun Powder', null, '455823', '516373', '6600', '3816', null, null, null, null, null, null, null, null, 'b1dc6d8c-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2103', '5042', 'The Crypt', 'Capture Point', '419119', '423491', '15870', '2969', null, null, '200004', 'Interact with the grave to capture %n!', 'Fight for control of %n!', 'Maintain control of %n!', 'Your realm controls the %n', 'has captured', 'b1dc7665-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2204', '25', 'Salvage', null, '626704', '368658', '6888', '2161', '75', null, null, null, null, null, null, null, 'b1dc7ed8-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2008', '1', 'Gun Powder', null, '450844', '527483', '6547', '1968', null, null, null, null, null, null, null, null, 'b1dc8d69-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2102', '1', 'Powder Keg', null, '489778', '360241', '5542', '1224', null, null, null, null, null, null, null, null, 'b1dc8d69-59b9-11e6-9679-00ff0731187c', null);
INSERT INTO `scenario_objects` VALUES ('2102', '2', 'Gun Powder', null, '483372', '365971', '6319', '1346', null, null, null, null, null, null, null, null, 'b1dc8d69-59b9-11e6-9679-00ff0731187d', null);
INSERT INTO `scenario_objects` VALUES ('2102', '1', 'Gun Powder', null, '495788', '354490', '6160', '2924', null, null, null, null, null, null, null, null, 'b1dc8d69-59b9-11e6-9679-00ff0731187e', null);
INSERT INTO `scenario_objects` VALUES ('2204', '2', 'Destruction Parts Wagon', null, '623031', '360404', '6072', '3822', null, null, null, null, null, null, null, null, 'b1dc97c7-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2207', '7067', 'Caledor Woods', 'Flag', '808246', '360225', '5546', '100', '25', '5', null, null, null, null, null, null, 'b1dc9866-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2204', '0', 'Center Glow', null, '626668', '368672', '6888', '2036', null, null, null, null, null, null, null, null, 'b1dcc746-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2106', '5138', 'Central Praag', 'Flag', '757852', '364479', '6536', '1251', '15', '2', null, null, null, null, null, null, 'b1dcc7ec-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2106', '5137', 'North Central Praag', 'Flag', '759321', '361670', '7022', null, '25', '5', null, null, null, null, null, null, 'b1dcd35c-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2106', '5139', 'South Central Praag', 'Flag', '756354', '367284', '7102', null, '25', '5', null, null, null, null, null, null, 'b1dcd3f8-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2106', '5140', 'South Praag', 'Flag', '753351', '368066', '7261', null, '35', '10', null, null, null, null, null, null, 'b1dcd483-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2106', '5136', 'North Praag', 'Flag', '762697', '361030', '7261', null, '35', '10', null, null, null, null, null, null, 'b1dcd50a-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2204', '1', 'Order Parts Wagon', null, '630185', '360408', '6064', '2343', null, null, null, null, null, null, null, null, 'b1dcd586-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2200', '7024', 'Dance of Swords', 'Flag', '361437', '361707', '11155', '830', '75', null, null, null, null, null, null, null, 'b1dcd5ec-59b9-11e6-9679-00ff0731187a', null);
INSERT INTO `scenario_objects` VALUES ('2206', '5150', 'The N. Fountain', 'Flag', '355022', '421723', '5742', '2100', '15', '10', null, null, null, null, null, null, 'b1dcd64d-59b9-11e6-9679-00ff07311871', '1');
INSERT INTO `scenario_objects` VALUES ('2206', '5151', 'The Orchard', 'Flag', '354705', '430693', '5261', '1320', '15', '10', null, null, null, null, null, null, 'b1dcd64d-59b9-11e6-9679-00ff07311872', '2');
INSERT INTO `scenario_objects` VALUES ('2206', '5152', 'The Crossroads', 'Flag', '352467', '425900', '4802', '2222', '15', '5', null, null, null, null, null, null, 'b1dcd64d-59b9-11e6-9679-00ff07311873', '0');
INSERT INTO `scenario_objects` VALUES ('2206', '5153', 'The Tower', 'Flag', '349389', '421725', '6069', '2130', '15', '10', null, null, null, null, null, null, 'b1dcd64d-59b9-11e6-9679-00ff07311874', '1');
INSERT INTO `scenario_objects` VALUES ('2206', '5154', 'The S. Fountain', 'Flag', '349749', '430640', '5062', '58', '15', '10', null, null, null, null, null, null, 'b1dcd64d-59b9-11e6-9679-00ff07311875', '2');
INSERT INTO `scenario_objects` VALUES ('2200', '7023', 'Death\'s Charge', 'Flag', '367380', '361665', '11191', '3857', '75', null, null, null, null, null, null, null, 'b1dcd64d-59b9-11e6-9679-00ff0731187a', null);
