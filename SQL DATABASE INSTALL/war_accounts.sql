/*
Navicat MySQL Data Transfer

Source Server         : warserver
Source Server Version : 80031
Source Host           : localhost:3306
Source Database       : war_accounts

Target Server Type    : MYSQL
Target Server Version : 80031
File Encoding         : 65001

Date: 2022-12-21 08:07:33
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for `accounts`
-- ----------------------------
DROP TABLE IF EXISTS `accounts`;
CREATE TABLE `accounts` (
  `AccountId` int NOT NULL AUTO_INCREMENT,
  `PacketLog` tinyint unsigned DEFAULT NULL,
  `Username` varchar(255) DEFAULT NULL,
  `Password` varchar(255) DEFAULT NULL,
  `CryptPassword` varchar(255) DEFAULT NULL,
  `Ip` varchar(255) DEFAULT NULL,
  `Token` varchar(255) DEFAULT NULL,
  `GmLevel` tinyint NOT NULL,
  `Banned` int NOT NULL,
  `BanReason` text,
  `AdviceBlockEnd` int DEFAULT NULL,
  `StealthMuteEnd` int DEFAULT NULL,
  `CoreLevel` int DEFAULT NULL,
  `LastLogged` int DEFAULT NULL,
  `LastNameChanged` int DEFAULT NULL,
  `LastPatcherLog` text,
  `InvalidPasswordCount` int unsigned NOT NULL,
  `noSurname` tinyint NOT NULL,
  `Email` text,
  PRIMARY KEY (`AccountId`),
  UNIQUE KEY `Username` (`Username`)
) ENGINE=InnoDB AUTO_INCREMENT=124104 DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of accounts
-- ----------------------------
INSERT INTO `accounts` VALUES ('124101', '0', 'test', '', '31f014b53e5861c8b28a8707a1d6a2a2737ce2c22fd671884173498510a063f0', '127.0.0.1', 'YjEwMjA5M2UtMWI4YS00NzYxLWE2NjktZjYyZjYxMjEwMGIy', '40', '0', '', '0', '0', '0', '1671609918', '0', '', '3', '0', '');

-- ----------------------------
-- Table structure for `accounts_pending`
-- ----------------------------
DROP TABLE IF EXISTS `accounts_pending`;
CREATE TABLE `accounts_pending` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Username` varchar(255) DEFAULT NULL,
  `Code` varchar(255) DEFAULT NULL,
  `Expires` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Username` (`Username`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of accounts_pending
-- ----------------------------

-- ----------------------------
-- Table structure for `account_sanction_logs`
-- ----------------------------
DROP TABLE IF EXISTS `account_sanction_logs`;
CREATE TABLE `account_sanction_logs` (
  `AccountId` int DEFAULT NULL,
  `IssuedBy` varchar(24) DEFAULT NULL,
  `ActionType` varchar(24) DEFAULT NULL,
  `IssuerGmLevel` int DEFAULT NULL,
  `ActionDuration` text,
  `ActionLog` varchar(255) DEFAULT NULL,
  `ActionTime` int DEFAULT NULL,
  `account_sanction_logs_ID` varchar(255) NOT NULL,
  PRIMARY KEY (`account_sanction_logs_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of account_sanction_logs
-- ----------------------------

-- ----------------------------
-- Table structure for `account_value`
-- ----------------------------
DROP TABLE IF EXISTS `account_value`;
CREATE TABLE `account_value` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `AccountId` int DEFAULT NULL,
  `InstallId` text,
  `IP` text,
  `MAC` text,
  `HDSerialHash` text,
  `CPUIDHash` text,
  `ModifyDate` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of account_value
-- ----------------------------

-- ----------------------------
-- Table structure for `characters_value_24hr`
-- ----------------------------
DROP TABLE IF EXISTS `characters_value_24hr`;
CREATE TABLE `characters_value_24hr` (
  `characterIdint` int NOT NULL,
  `Levelint` int DEFAULT NULL,
  `xpint` int DEFAULT NULL,
  `RenownRankint` int DEFAULT NULL,
  `Moneyint` int DEFAULT NULL,
  `timestampdatetime` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `characterId` int NOT NULL,
  PRIMARY KEY (`characterId`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of characters_value_24hr
-- ----------------------------

-- ----------------------------
-- Table structure for `ip_bans`
-- ----------------------------
DROP TABLE IF EXISTS `ip_bans`;
CREATE TABLE `ip_bans` (
  `Ip` varchar(255) NOT NULL,
  `Expire` int DEFAULT NULL,
  PRIMARY KEY (`Ip`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of ip_bans
-- ----------------------------

-- ----------------------------
-- Table structure for `launcher_files`
-- ----------------------------
DROP TABLE IF EXISTS `launcher_files`;
CREATE TABLE `launcher_files` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(2000) DEFAULT NULL,
  `CRC32` int unsigned DEFAULT NULL,
  `Size` bigint DEFAULT NULL,
  `GmLevel` int unsigned DEFAULT NULL,
  `ModifyDate` datetime DEFAULT NULL,
  `ModifyAccountId` int DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of launcher_files
-- ----------------------------

-- ----------------------------
-- Table structure for `launcher_hashes`
-- ----------------------------
DROP TABLE IF EXISTS `launcher_hashes`;
CREATE TABLE `launcher_hashes` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(2000) DEFAULT NULL,
  `File` varchar(4000) DEFAULT NULL,
  `CRC32` int unsigned DEFAULT NULL,
  `Hash` bigint unsigned DEFAULT NULL,
  `Size` bigint DEFAULT NULL,
  `MetaDataSize` int unsigned DEFAULT NULL,
  `ArchiveId` int DEFAULT NULL,
  `ModifyDate` datetime DEFAULT NULL,
  `ModifyAccountId` int DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of launcher_hashes
-- ----------------------------

-- ----------------------------
-- Table structure for `launcher_info`
-- ----------------------------
DROP TABLE IF EXISTS `launcher_info`;
CREATE TABLE `launcher_info` (
  `LauncherId` int NOT NULL AUTO_INCREMENT,
  `GmLevel` tinyint NOT NULL,
  `PatchNotes` text NOT NULL,
  `ServerState` int NOT NULL,
  `Version` int DEFAULT NULL,
  `FilePath` varchar(2000) DEFAULT NULL,
  `ModifiedDate` datetime DEFAULT NULL,
  `ModifyAccountId` int DEFAULT NULL,
  PRIMARY KEY (`LauncherId`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of launcher_info
-- ----------------------------

-- ----------------------------
-- Table structure for `launcher_myps`
-- ----------------------------
DROP TABLE IF EXISTS `launcher_myps`;
CREATE TABLE `launcher_myps` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(2000) DEFAULT NULL,
  `CRC32` int DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of launcher_myps
-- ----------------------------

-- ----------------------------
-- Table structure for `realms`
-- ----------------------------
DROP TABLE IF EXISTS `realms`;
CREATE TABLE `realms` (
  `RealmId` tinyint unsigned NOT NULL DEFAULT '0',
  `Name` varchar(255) DEFAULT NULL,
  `Language` varchar(255) DEFAULT NULL,
  `Adresse` varchar(255) DEFAULT NULL,
  `Port` int NOT NULL,
  `AllowTrials` varchar(32) DEFAULT NULL,
  `CharfxerAvailable` varchar(32) DEFAULT NULL,
  `Legacy` varchar(32) DEFAULT NULL,
  `BonusDestruction` varchar(32) DEFAULT NULL,
  `BonusOrder` varchar(32) DEFAULT NULL,
  `Redirect` varchar(32) DEFAULT NULL,
  `Region` varchar(32) DEFAULT NULL,
  `Retired` varchar(32) DEFAULT NULL,
  `WaitingDestruction` varchar(32) DEFAULT NULL,
  `WaitingOrder` varchar(32) DEFAULT NULL,
  `DensityDestruction` varchar(32) DEFAULT NULL,
  `DensityOrder` varchar(32) DEFAULT NULL,
  `OpenRvr` varchar(32) DEFAULT NULL,
  `Rp` varchar(32) DEFAULT NULL,
  `Status` varchar(32) DEFAULT NULL,
  `Online` tinyint unsigned NOT NULL,
  `OnlineDate` datetime DEFAULT NULL,
  `OnlinePlayers` int unsigned DEFAULT NULL,
  `OrderCount` int unsigned DEFAULT NULL,
  `DestructionCount` int unsigned DEFAULT NULL,
  `MaxPlayers` int unsigned DEFAULT NULL,
  `OrderCharacters` int unsigned DEFAULT NULL,
  `DestruCharacters` int unsigned DEFAULT NULL,
  `NextRotationTime` bigint DEFAULT NULL,
  `MasterPassword` text,
  `BootTime` int DEFAULT NULL,
  PRIMARY KEY (`RealmId`),
  UNIQUE KEY `RealmId` (`RealmId`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of realms
-- ----------------------------
INSERT INTO `realms` VALUES ('1', 'dan', 'EN', '127.0.0.1', '10300', '0', '0', '0', '0', '0', '0', 'STR_REGION_NORTHAMERICA', '0', '0', '0', '0', '0', '0', '0', '0', '1', '2022-12-21 07:45:07', '1', '0', '0', '1000', '0', '0', '1532563200', '', '1671608707');
