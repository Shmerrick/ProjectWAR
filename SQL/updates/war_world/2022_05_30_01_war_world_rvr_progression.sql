SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for rvr_progression
-- ----------------------------
DROP TABLE IF EXISTS `rvr_progression`;
CREATE TABLE `rvr_progression`  (
  `tier` int NULL DEFAULT NULL,
  `PairingId` int NULL DEFAULT NULL,
  `Description` varchar(80) CHARACTER SET latin1 COLLATE latin1_swedish_ci NULL DEFAULT NULL,
  `BattleFrontId` int NOT NULL,
  `OrderWinProgression` int NOT NULL,
  `DestWinProgression` int NOT NULL,
  `RegionId` int NOT NULL,
  `ZoneId` int NULL DEFAULT NULL,
  `DefaultRealmLock` int NULL DEFAULT NULL,
  `ResetProgressionOnEntry` int NULL DEFAULT NULL,
  `LastOwningRealm` int NOT NULL DEFAULT 0,
  `LastOpenedZone` int NOT NULL DEFAULT 0,
  `OrderVP` int NOT NULL DEFAULT 0,
  `DestroVP` int NOT NULL DEFAULT 0,
  `DestroKeepId` int NOT NULL,
  `OrderKeepId` int NOT NULL,
  PRIMARY KEY (`BattleFrontId`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of rvr_progression
-- ----------------------------
INSERT INTO `rvr_progression` VALUES (4, 2, 'Chaos Wastes', 1, 15, 2, 11, 103, 2, 0, 2, 0, 0, 0, 19, 20);
INSERT INTO `rvr_progression` VALUES (4, 2, 'Praag', 2, 1, 3, 11, 105, 0, 1, 0, 1, 300, 300, 18, 17);
INSERT INTO `rvr_progression` VALUES (4, 2, 'Reikland', 3, 2, 13, 11, 109, 1, 0, 1, 0, 0, 0, 15, 16);
INSERT INTO `rvr_progression` VALUES (4, 1, 'Black Crag', 4, 16, 5, 2, 3, 2, 0, 2, 0, 0, 0, 10, 9);
INSERT INTO `rvr_progression` VALUES (4, 1, 'Thunder Mountain', 5, 4, 6, 2, 5, 0, 0, 0, 0, 0, 0, 7, 8);
INSERT INTO `rvr_progression` VALUES (4, 1, 'Kadrin Valley', 6, 5, 14, 2, 9, 1, 0, 1, 0, 0, 0, 6, 5);
INSERT INTO `rvr_progression` VALUES (4, 3, 'Eataine', 7, 8, 17, 4, 209, 1, 0, 1, 0, 0, 0, 26, 25);
INSERT INTO `rvr_progression` VALUES (4, 3, 'Dragonwake', 8, 9, 7, 4, 205, 0, 0, 0, 0, 0, 0, 28, 27);
INSERT INTO `rvr_progression` VALUES (4, 3, 'Caledor', 9, 18, 8, 4, 203, 2, 0, 2, 0, 0, 0, 29, 30);
INSERT INTO `rvr_progression` VALUES (1, 2, 'Norsca / Nordland', 10, 11, 11, 8, 106, 0, 1, 2, 1, 50, 0, 0, 0);
INSERT INTO `rvr_progression` VALUES (1, 1, 'Ekrund / Mt Bloodhorn', 11, 12, 12, 1, 6, 2, 0, 2, 1, 0, 0, 0, 0);
INSERT INTO `rvr_progression` VALUES (1, 3, 'Chrace / Blighted Isle', 12, 10, 10, 3, 206, 1, 0, 1, 0, 0, 0, 0, 0);
INSERT INTO `rvr_progression` VALUES (4, 2, 'Reikwald', 13, 5, 5, 11, 110, 1, 0, 1, 0, 0, 0, 100, 100);
INSERT INTO `rvr_progression` VALUES (4, 1, 'Stonewatch', 14, 8, 8, 2, 10, 1, 0, 1, 0, 0, 0, 103, 103);
INSERT INTO `rvr_progression` VALUES (4, 2, 'The Maw', 15, 5, 5, 11, 104, 2, 0, 2, 0, 0, 0, 101, 101);
INSERT INTO `rvr_progression` VALUES (4, 1, 'Butchers Pass', 16, 8, 8, 2, 4, 2, 0, 2, 0, 0, 0, 102, 102);
INSERT INTO `rvr_progression` VALUES (4, 3, 'Shining Way', 17, 2, 2, 4, 210, 1, 0, 1, 0, 0, 0, 105, 105);
INSERT INTO `rvr_progression` VALUES (4, 3, 'Fell Landing', 18, 2, 2, 4, 204, 2, 0, 2, 0, 0, 0, 104, 104);

SET FOREIGN_KEY_CHECKS = 1;
