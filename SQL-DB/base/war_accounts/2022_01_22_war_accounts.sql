/*
SQLyog Community v13.1.1 (64 bit)
MySQL - 8.0.18 : Database - war_accounts
*********************************************************************
*/

/*!40101 SET NAMES utf8 */;

/*!40101 SET SQL_MODE=''*/;

/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;
CREATE DATABASE /*!32312 IF NOT EXISTS*/`war_accounts` /*!40100 DEFAULT CHARACTER SET latin1 */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `war_accounts`;

/*Table structure for table `account_sanction_logs` */

DROP TABLE IF EXISTS `account_sanction_logs`;

CREATE TABLE `account_sanction_logs` (
  `AccountId` int(11) DEFAULT NULL,
  `IssuedBy` varchar(24) DEFAULT NULL,
  `ActionType` varchar(24) DEFAULT NULL,
  `IssuerGmLevel` int(11) DEFAULT NULL,
  `ActionDuration` text,
  `ActionLog` varchar(255) DEFAULT NULL,
  `ActionTime` int(11) DEFAULT NULL,
  `account_sanction_logs_ID` varchar(255) NOT NULL,
  PRIMARY KEY (`account_sanction_logs_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

/*Data for the table `account_sanction_logs` */

LOCK TABLES `account_sanction_logs` WRITE;

UNLOCK TABLES;

/*Table structure for table `account_value` */

DROP TABLE IF EXISTS `account_value`;

CREATE TABLE `account_value` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `AccountId` int(11) DEFAULT NULL,
  `InstallId` text,
  `IP` text,
  `MAC` text,
  `HDSerialHash` text,
  `CPUIDHash` text,
  `ModifyDate` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

/*Data for the table `account_value` */

LOCK TABLES `account_value` WRITE;

UNLOCK TABLES;

/*Table structure for table `accounts` */

DROP TABLE IF EXISTS `accounts`;

CREATE TABLE `accounts` (
  `AccountId` int(11) NOT NULL AUTO_INCREMENT,
  `PacketLog` tinyint(3) unsigned DEFAULT NULL,
  `Username` varchar(255) DEFAULT NULL,
  `Password` varchar(255) DEFAULT NULL,
  `CryptPassword` varchar(255) DEFAULT NULL,
  `Ip` varchar(255) DEFAULT NULL,
  `Token` varchar(255) DEFAULT NULL,
  `GmLevel` tinyint(4) NOT NULL,
  `Banned` int(11) NOT NULL,
  `BanReason` text,
  `AdviceBlockEnd` int(11) DEFAULT NULL,
  `StealthMuteEnd` int(11) DEFAULT NULL,
  `CoreLevel` int(11) DEFAULT NULL,
  `LastLogged` int(11) DEFAULT NULL,
  `LastNameChanged` int(11) DEFAULT NULL,
  `LastPatcherLog` text,
  `InvalidPasswordCount` int(10) unsigned NOT NULL,
  `noSurname` tinyint(4) NOT NULL,
  `Email` text,
  PRIMARY KEY (`AccountId`),
  UNIQUE KEY `Username` (`Username`)
) ENGINE=InnoDB AUTO_INCREMENT=125397 DEFAULT CHARSET=latin1;

/*Data for the table `accounts` */

LOCK TABLES `accounts` WRITE;

insert  into `accounts`(`AccountId`,`PacketLog`,`Username`,`Password`,`CryptPassword`,`Ip`,`Token`,`GmLevel`,`Banned`,`BanReason`,`AdviceBlockEnd`,`StealthMuteEnd`,`CoreLevel`,`LastLogged`,`LastNameChanged`,`LastPatcherLog`,`InvalidPasswordCount`,`noSurname`,`Email`) values 
(125394,0,'ander','','cb10e3a1f9de52f46f6d1e06435875e0155bdeeb4625f1fe1c5266f10a70000e','127.0.0.1','ZGVmYjEwMGMtOGNjYi00NDlkLWFmMTAtZGMzM2Q4YTg0Zjcz',40,0,'',0,0,0,1642329541,0,'',69,0,''),
(125395,0,'test','test','31f014b53e5861c8b28a8707a1d6a2a2737ce2c22fd671884173498510a063f0','127.0.0.1','',1,0,'',0,0,0,1591030657,0,'',0,0,NULL),
(125396,0,'dude','dude','8227cb39837f58f3825547a9e8baddbc06d24ca4e6fdb9512d886604a9579aca','127.0.0.1','',1,0,'',0,0,0,1615329290,0,'',0,0,NULL);

UNLOCK TABLES;

/*Table structure for table `blogs` */

DROP TABLE IF EXISTS `blogs`;

CREATE TABLE `blogs` (
  `BlogId` int(11) NOT NULL AUTO_INCREMENT,
  `BlogTimestamp` datetime(6) DEFAULT NULL,
  `BlogText` mediumtext,
  `BlogUrl` varchar(200) DEFAULT NULL,
  `BlogTitle` varchar(80) DEFAULT NULL,
  PRIMARY KEY (`BlogId`)
) ENGINE=InnoDB AUTO_INCREMENT=32 DEFAULT CHARSET=latin1;

/*Data for the table `blogs` */

LOCK TABLES `blogs` WRITE;

UNLOCK TABLES;

/*Table structure for table `characters_value_24hr` */

DROP TABLE IF EXISTS `characters_value_24hr`;

CREATE TABLE `characters_value_24hr` (
  `characterIdint` int(11) NOT NULL,
  `Levelint` int(11) DEFAULT NULL,
  `xpint` int(11) DEFAULT NULL,
  `RenownRankint` int(11) DEFAULT NULL,
  `Moneyint` int(11) DEFAULT NULL,
  `timestampdatetime` timestamp NOT NULL,
  `characterId` int(11) NOT NULL,
  PRIMARY KEY (`characterId`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

/*Data for the table `characters_value_24hr` */

LOCK TABLES `characters_value_24hr` WRITE;

UNLOCK TABLES;

/*Table structure for table `ip_bans` */

DROP TABLE IF EXISTS `ip_bans`;

CREATE TABLE `ip_bans` (
  `Ip` varchar(255) NOT NULL,
  `Expire` int(11) DEFAULT NULL,
  PRIMARY KEY (`Ip`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

/*Data for the table `ip_bans` */

LOCK TABLES `ip_bans` WRITE;

UNLOCK TABLES;

/*Table structure for table `launcher_files` */

DROP TABLE IF EXISTS `launcher_files`;

CREATE TABLE `launcher_files` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(2000) DEFAULT NULL,
  `CRC32` int(10) unsigned DEFAULT NULL,
  `Size` bigint(20) DEFAULT NULL,
  `GmLevel` int(10) unsigned DEFAULT NULL,
  `ModifyDate` datetime DEFAULT NULL,
  `ModifyAccountId` int(11) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

/*Data for the table `launcher_files` */

LOCK TABLES `launcher_files` WRITE;

UNLOCK TABLES;

/*Table structure for table `launcher_hashes` */

DROP TABLE IF EXISTS `launcher_hashes`;

CREATE TABLE `launcher_hashes` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(2000) DEFAULT NULL,
  `File` varchar(4000) DEFAULT NULL,
  `CRC32` int(10) unsigned DEFAULT NULL,
  `Hash` bigint(20) unsigned DEFAULT NULL,
  `Size` bigint(20) DEFAULT NULL,
  `MetaDataSize` int(10) unsigned DEFAULT NULL,
  `ArchiveId` int(11) DEFAULT NULL,
  `ModifyDate` datetime DEFAULT NULL,
  `ModifyAccountId` int(11) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

/*Data for the table `launcher_hashes` */

LOCK TABLES `launcher_hashes` WRITE;

UNLOCK TABLES;

/*Table structure for table `launcher_info` */

DROP TABLE IF EXISTS `launcher_info`;

CREATE TABLE `launcher_info` (
  `LauncherId` int(11) NOT NULL AUTO_INCREMENT,
  `GmLevel` tinyint(4) NOT NULL,
  `PatchNotes` text NOT NULL,
  `ServerState` int(11) NOT NULL,
  `Version` int(11) DEFAULT NULL,
  `FilePath` varchar(2000) DEFAULT NULL,
  `ModifiedDate` datetime DEFAULT NULL,
  `ModifyAccountId` int(11) DEFAULT NULL,
  PRIMARY KEY (`LauncherId`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

/*Data for the table `launcher_info` */

LOCK TABLES `launcher_info` WRITE;

UNLOCK TABLES;

/*Table structure for table `launcher_myps` */

DROP TABLE IF EXISTS `launcher_myps`;

CREATE TABLE `launcher_myps` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(2000) DEFAULT NULL,
  `CRC32` int(11) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

/*Data for the table `launcher_myps` */

LOCK TABLES `launcher_myps` WRITE;

UNLOCK TABLES;

/*Table structure for table `realms` */

DROP TABLE IF EXISTS `realms`;

CREATE TABLE `realms` (
  `RealmId` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `Name` varchar(255) DEFAULT NULL,
  `Language` varchar(255) DEFAULT NULL,
  `Adresse` varchar(255) DEFAULT NULL,
  `Port` int(11) NOT NULL,
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
  `Online` tinyint(3) unsigned NOT NULL,
  `OnlineDate` datetime DEFAULT NULL,
  `OnlinePlayers` int(10) unsigned DEFAULT NULL,
  `OrderCount` int(10) unsigned DEFAULT NULL,
  `DestructionCount` int(10) unsigned DEFAULT NULL,
  `MaxPlayers` int(10) unsigned DEFAULT NULL,
  `OrderCharacters` int(10) unsigned DEFAULT NULL,
  `DestruCharacters` int(10) unsigned DEFAULT NULL,
  `NextRotationTime` bigint(20) DEFAULT NULL,
  `MasterPassword` text,
  `BootTime` int(11) DEFAULT NULL,
  PRIMARY KEY (`RealmId`),
  UNIQUE KEY `RealmId` (`RealmId`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

/*Data for the table `realms` */

LOCK TABLES `realms` WRITE;

insert  into `realms`(`RealmId`,`Name`,`Language`,`Adresse`,`Port`,`AllowTrials`,`CharfxerAvailable`,`Legacy`,`BonusDestruction`,`BonusOrder`,`Redirect`,`Region`,`Retired`,`WaitingDestruction`,`WaitingOrder`,`DensityDestruction`,`DensityOrder`,`OpenRvr`,`Rp`,`Status`,`Online`,`OnlineDate`,`OnlinePlayers`,`OrderCount`,`DestructionCount`,`MaxPlayers`,`OrderCharacters`,`DestruCharacters`,`NextRotationTime`,`MasterPassword`,`BootTime`) values 
(1,'ProjectWAR','EN','127.0.0.1',10300,'0','0','0','0','0','0','STR_REGION_NORTHAMERICA','0','0','0','0','0','0','1','0',1,'2022-01-22 19:39:55',0,0,0,1000,20,13,1532563200,'',1642869595);

UNLOCK TABLES;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;
