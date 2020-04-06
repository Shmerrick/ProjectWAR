CREATE DATABASE  IF NOT EXISTS `war_characters` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `war_characters`;
-- MySQL dump 10.13  Distrib 8.0.18, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: war_characters
-- ------------------------------------------------------
-- Server version	8.0.18

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `auctions`
--

DROP TABLE IF EXISTS `auctions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `auctions` (
  `AuctionId` bigint(20) unsigned NOT NULL,
  `Realm` tinyint(3) unsigned NOT NULL,
  `SellerId` int(10) unsigned NOT NULL,
  `ItemId` int(10) unsigned NOT NULL,
  `SellPrice` int(10) unsigned NOT NULL,
  `Count` smallint(5) unsigned NOT NULL,
  `StartTime` int(10) unsigned NOT NULL,
  `Talismans` varchar(40) DEFAULT NULL,
  `PrimaryDye` smallint(5) unsigned NOT NULL,
  `SecondaryDye` smallint(5) unsigned NOT NULL,
  PRIMARY KEY (`AuctionId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `auctions`
--

LOCK TABLES `auctions` WRITE;
/*!40000 ALTER TABLE `auctions` DISABLE KEYS */;
/*!40000 ALTER TABLE `auctions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `banned_names`
--

DROP TABLE IF EXISTS `banned_names`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `banned_names` (
  `NameString` varchar(255) NOT NULL,
  `FilterTypeString` text,
  PRIMARY KEY (`NameString`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `banned_names`
--

LOCK TABLES `banned_names` WRITE;
/*!40000 ALTER TABLE `banned_names` DISABLE KEYS */;
/*!40000 ALTER TABLE `banned_names` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `bug_report`
--

DROP TABLE IF EXISTS `bug_report`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `bug_report` (
  `AccountId` int(10) unsigned NOT NULL,
  `CharacterId` int(10) unsigned NOT NULL,
  `ZoneId` smallint(5) unsigned NOT NULL,
  `X` smallint(5) unsigned NOT NULL,
  `Y` smallint(5) unsigned NOT NULL,
  `Time` int(10) unsigned NOT NULL,
  `Type` tinyint(3) unsigned NOT NULL,
  `Category` tinyint(3) unsigned NOT NULL,
  `Message` text NOT NULL,
  `ReportType` text NOT NULL,
  `FieldSting` text NOT NULL,
  `Assigned` text,
  `bug_report_ID` varchar(255) NOT NULL,
  PRIMARY KEY (`bug_report_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `bug_report`
--

LOCK TABLES `bug_report` WRITE;
/*!40000 ALTER TABLE `bug_report` DISABLE KEYS */;
/*!40000 ALTER TABLE `bug_report` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `character_abilities`
--

DROP TABLE IF EXISTS `character_abilities`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `character_abilities` (
  `CharacterID` int(11) DEFAULT NULL,
  `AbilityID` smallint(5) unsigned DEFAULT NULL,
  `LastCast` int(11) DEFAULT NULL,
  `character_abilities_ID` varchar(255) NOT NULL,
  PRIMARY KEY (`character_abilities_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `character_abilities`
--

LOCK TABLES `character_abilities` WRITE;
/*!40000 ALTER TABLE `character_abilities` DISABLE KEYS */;
/*!40000 ALTER TABLE `character_abilities` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `character_bag_bonus`
--

DROP TABLE IF EXISTS `character_bag_bonus`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `character_bag_bonus` (
  `BonusId` bigint(20) NOT NULL AUTO_INCREMENT,
  `GoldBag` int(11) NOT NULL,
  `PurpleBag` int(11) NOT NULL,
  `BlueBag` int(11) NOT NULL,
  `GreenBag` int(11) NOT NULL,
  `WhiteBag` int(11) NOT NULL,
  `Timestamp` datetime NOT NULL,
  `CharacterId` int(11) NOT NULL,
  `CharacterName` text NOT NULL,
  PRIMARY KEY (`BonusId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `character_bag_bonus`
--

LOCK TABLES `character_bag_bonus` WRITE;
/*!40000 ALTER TABLE `character_bag_bonus` DISABLE KEYS */;
/*!40000 ALTER TABLE `character_bag_bonus` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `character_bag_pools`
--

DROP TABLE IF EXISTS `character_bag_pools`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `character_bag_pools` (
  `CharacterId` int(11) NOT NULL,
  `Bag_Type` int(11) NOT NULL,
  `BagPool_Value` int(11) NOT NULL,
  PRIMARY KEY (`CharacterId`,`Bag_Type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `character_bag_pools`
--

LOCK TABLES `character_bag_pools` WRITE;
/*!40000 ALTER TABLE `character_bag_pools` DISABLE KEYS */;
/*!40000 ALTER TABLE `character_bag_pools` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `character_client_data`
--

DROP TABLE IF EXISTS `character_client_data`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `character_client_data` (
  `CharacterId` int(10) unsigned NOT NULL,
  `ClientDataString` text NOT NULL,
  PRIMARY KEY (`CharacterId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `character_client_data`
--

LOCK TABLES `character_client_data` WRITE;
/*!40000 ALTER TABLE `character_client_data` DISABLE KEYS */;
/*!40000 ALTER TABLE `character_client_data` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `character_deletions`
--

DROP TABLE IF EXISTS `character_deletions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `character_deletions` (
  `DeletionIP` text,
  `AccountID` int(11) DEFAULT NULL,
  `AccountName` text,
  `CharacterID` int(10) unsigned DEFAULT NULL,
  `CharacterName` text,
  `DeletionTimeSeconds` int(11) DEFAULT NULL,
  `character_deletions_ID` varchar(255) NOT NULL,
  PRIMARY KEY (`character_deletions_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `character_deletions`
--

LOCK TABLES `character_deletions` WRITE;
/*!40000 ALTER TABLE `character_deletions` DISABLE KEYS */;
/*!40000 ALTER TABLE `character_deletions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `character_honor_reward_cooldown`
--

DROP TABLE IF EXISTS `character_honor_reward_cooldown`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `character_honor_reward_cooldown` (
  `CharacterId` int(10) unsigned NOT NULL,
  `ItemId` int(11) NOT NULL,
  `Cooldown` bigint(20) NOT NULL,
  PRIMARY KEY (`CharacterId`,`ItemId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `character_honor_reward_cooldown`
--

LOCK TABLES `character_honor_reward_cooldown` WRITE;
/*!40000 ALTER TABLE `character_honor_reward_cooldown` DISABLE KEYS */;
/*!40000 ALTER TABLE `character_honor_reward_cooldown` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `character_influences`
--

DROP TABLE IF EXISTS `character_influences`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `character_influences` (
  `CharacterId` int(11) NOT NULL,
  `InfluenceId` smallint(5) unsigned NOT NULL,
  `InfluenceCount` int(10) unsigned NOT NULL,
  `Tier_1_Itemtaken` tinyint(3) unsigned NOT NULL,
  `Tier_2_Itemtaken` tinyint(3) unsigned NOT NULL,
  `Tier_3_Itemtaken` tinyint(3) unsigned NOT NULL,
  PRIMARY KEY (`CharacterId`,`InfluenceId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `character_influences`
--

LOCK TABLES `character_influences` WRITE;
/*!40000 ALTER TABLE `character_influences` DISABLE KEYS */;
/*!40000 ALTER TABLE `character_influences` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `character_saved_buffs`
--

DROP TABLE IF EXISTS `character_saved_buffs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `character_saved_buffs` (
  `CharacterId` int(10) unsigned NOT NULL,
  `BuffId` smallint(5) unsigned NOT NULL,
  `Level` tinyint(3) unsigned DEFAULT NULL,
  `StackLevel` tinyint(3) unsigned DEFAULT NULL,
  `EndTimeSeconds` int(10) unsigned DEFAULT NULL,
  PRIMARY KEY (`CharacterId`,`BuffId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `character_saved_buffs`
--

LOCK TABLES `character_saved_buffs` WRITE;
/*!40000 ALTER TABLE `character_saved_buffs` DISABLE KEYS */;
/*!40000 ALTER TABLE `character_saved_buffs` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `characters`
--

DROP TABLE IF EXISTS `characters`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `characters` (
  `CharacterId` int(10) unsigned NOT NULL,
  `Name` varchar(24) NOT NULL,
  `Surname` varchar(24) NOT NULL,
  `RealmId` int(11) NOT NULL,
  `AccountId` int(11) NOT NULL,
  `SlotId` tinyint(3) unsigned NOT NULL,
  `ModelId` tinyint(3) unsigned NOT NULL,
  `Career` tinyint(3) unsigned NOT NULL,
  `CareerLine` tinyint(3) unsigned NOT NULL,
  `Realm` tinyint(3) unsigned NOT NULL,
  `HeldLeft` int(11) NOT NULL,
  `Race` tinyint(3) unsigned NOT NULL,
  `Traits` text NOT NULL,
  `Sex` tinyint(3) unsigned NOT NULL,
  `Anonymous` tinyint(3) unsigned NOT NULL,
  `Hidden` tinyint(3) unsigned NOT NULL,
  `OldName` varchar(24) NOT NULL,
  `PetName` varchar(24) NOT NULL,
  `PetModel` smallint(5) unsigned NOT NULL,
  `HonorPoints` smallint(5) unsigned NOT NULL,
  `HonorRank` smallint(5) unsigned NOT NULL,
  PRIMARY KEY (`CharacterId`),
  UNIQUE KEY `Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `characters`
--

LOCK TABLES `characters` WRITE;
/*!40000 ALTER TABLE `characters` DISABLE KEYS */;
/*!40000 ALTER TABLE `characters` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `characters_items`
--

DROP TABLE IF EXISTS `characters_items`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `characters_items` (
  `Guid` bigint(20) NOT NULL,
  `CharacterId` int(10) unsigned NOT NULL,
  `Entry` int(10) unsigned NOT NULL,
  `SlotId` smallint(5) unsigned NOT NULL,
  `ModelId` int(10) unsigned NOT NULL,
  `Counts` smallint(5) unsigned NOT NULL,
  `Talismans` varchar(40) DEFAULT NULL,
  `PrimaryDye` smallint(5) unsigned NOT NULL,
  `SecondaryDye` smallint(5) unsigned NOT NULL,
  `BoundtoPlayer` tinyint(3) unsigned NOT NULL,
  `Alternate_AppereanceEntry` int(10) unsigned NOT NULL,
  `characters_items_ID` varchar(255) NOT NULL,
  PRIMARY KEY (`characters_items_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `characters_items`
--

LOCK TABLES `characters_items` WRITE;
/*!40000 ALTER TABLE `characters_items` DISABLE KEYS */;
/*!40000 ALTER TABLE `characters_items` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `characters_mails`
--

DROP TABLE IF EXISTS `characters_mails`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `characters_mails` (
  `Guid` int(11) NOT NULL AUTO_INCREMENT,
  `AuctionType` tinyint(3) unsigned NOT NULL,
  `CharacterId` int(10) unsigned NOT NULL,
  `CharacterIdSender` int(10) unsigned NOT NULL,
  `SenderName` varchar(255) NOT NULL,
  `ReceiverName` varchar(255) NOT NULL,
  `SendDate` int(10) unsigned NOT NULL,
  `ReadDate` int(10) unsigned NOT NULL,
  `Title` varchar(255) NOT NULL,
  `Content` text NOT NULL,
  `Money` int(10) unsigned NOT NULL,
  `Cr` tinyint(3) unsigned NOT NULL,
  `Opened` tinyint(3) unsigned NOT NULL,
  `ItemsString` text NOT NULL,
  PRIMARY KEY (`Guid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `characters_mails`
--

LOCK TABLES `characters_mails` WRITE;
/*!40000 ALTER TABLE `characters_mails` DISABLE KEYS */;
/*!40000 ALTER TABLE `characters_mails` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `characters_quests`
--

DROP TABLE IF EXISTS `characters_quests`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `characters_quests` (
  `CharacterId` int(10) unsigned NOT NULL,
  `QuestID` smallint(5) unsigned NOT NULL,
  `Objectives` varchar(64) NOT NULL,
  `Done` tinyint(3) unsigned NOT NULL,
  PRIMARY KEY (`CharacterId`,`QuestID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `characters_quests`
--

LOCK TABLES `characters_quests` WRITE;
/*!40000 ALTER TABLE `characters_quests` DISABLE KEYS */;
/*!40000 ALTER TABLE `characters_quests` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `characters_socials`
--

DROP TABLE IF EXISTS `characters_socials`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `characters_socials` (
  `CharacterId` int(10) unsigned NOT NULL,
  `DistCharacterId` int(10) unsigned NOT NULL,
  `DistName` varchar(255) NOT NULL,
  `Friend` tinyint(3) unsigned NOT NULL,
  `Ignore` tinyint(3) unsigned NOT NULL,
  PRIMARY KEY (`CharacterId`,`DistCharacterId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `characters_socials`
--

LOCK TABLES `characters_socials` WRITE;
/*!40000 ALTER TABLE `characters_socials` DISABLE KEYS */;
/*!40000 ALTER TABLE `characters_socials` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `characters_toks`
--

DROP TABLE IF EXISTS `characters_toks`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `characters_toks` (
  `CharacterId` int(10) unsigned NOT NULL,
  `TokEntry` smallint(5) unsigned NOT NULL,
  `Count` int(10) unsigned DEFAULT NULL,
  PRIMARY KEY (`CharacterId`,`TokEntry`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `characters_toks`
--

LOCK TABLES `characters_toks` WRITE;
/*!40000 ALTER TABLE `characters_toks` DISABLE KEYS */;
/*!40000 ALTER TABLE `characters_toks` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `characters_toks_kills`
--

DROP TABLE IF EXISTS `characters_toks_kills`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `characters_toks_kills` (
  `CharacterId` int(10) unsigned NOT NULL,
  `NPCEntry` smallint(5) unsigned NOT NULL,
  `Count` int(10) unsigned DEFAULT NULL,
  PRIMARY KEY (`CharacterId`,`NPCEntry`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `characters_toks_kills`
--

LOCK TABLES `characters_toks_kills` WRITE;
/*!40000 ALTER TABLE `characters_toks_kills` DISABLE KEYS */;
/*!40000 ALTER TABLE `characters_toks_kills` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `characters_value`
--

DROP TABLE IF EXISTS `characters_value`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `characters_value` (
  `CharacterId` int(10) unsigned NOT NULL,
  `Level` tinyint(3) unsigned NOT NULL,
  `Xp` int(10) unsigned NOT NULL,
  `XpMode` int(11) NOT NULL,
  `RestXp` int(10) unsigned NOT NULL,
  `Renown` int(10) unsigned NOT NULL,
  `RenownRank` tinyint(3) unsigned NOT NULL,
  `Money` int(10) unsigned NOT NULL,
  `Speed` int(11) NOT NULL,
  `PlayedTime` int(10) unsigned NOT NULL,
  `LastSeen` int(11) DEFAULT NULL,
  `RegionId` int(11) NOT NULL,
  `ZoneId` smallint(5) unsigned NOT NULL,
  `WorldX` int(11) NOT NULL,
  `WorldY` int(11) NOT NULL,
  `WorldZ` int(11) NOT NULL,
  `WorldO` int(11) NOT NULL,
  `RallyPoint` smallint(5) unsigned NOT NULL,
  `BagBuy` tinyint(3) unsigned NOT NULL,
  `BankBuy` tinyint(3) unsigned NOT NULL,
  `Skills` int(10) unsigned NOT NULL,
  `Online` tinyint(3) unsigned NOT NULL,
  `GearShow` tinyint(3) unsigned NOT NULL,
  `TitleId` smallint(5) unsigned NOT NULL,
  `RenownSkills` text NOT NULL,
  `MasterySkills` text NOT NULL,
  `Morale1` smallint(5) unsigned DEFAULT NULL,
  `Morale2` smallint(5) unsigned DEFAULT NULL,
  `Morale3` smallint(5) unsigned DEFAULT NULL,
  `Morale4` smallint(5) unsigned DEFAULT NULL,
  `Tactic1` smallint(5) unsigned DEFAULT NULL,
  `Tactic2` smallint(5) unsigned DEFAULT NULL,
  `Tactic3` smallint(5) unsigned DEFAULT NULL,
  `Tactic4` smallint(5) unsigned DEFAULT NULL,
  `GatheringSkill` tinyint(3) unsigned NOT NULL,
  `GatheringSkillLevel` tinyint(3) unsigned NOT NULL,
  `CraftingSkill` tinyint(3) unsigned NOT NULL,
  `CraftingSkillLevel` tinyint(3) unsigned NOT NULL,
  `ExperimentalMode` tinyint(3) unsigned NOT NULL,
  `RVRKills` int(10) unsigned NOT NULL,
  `RVRDeaths` int(10) unsigned NOT NULL,
  `CraftingBags` tinyint(3) unsigned NOT NULL,
  `PendingXp` int(10) unsigned DEFAULT NULL,
  `PendingRenown` int(10) unsigned DEFAULT NULL,
  `Lockouts` text NOT NULL,
  `DisconcetTime` int(11) NOT NULL,
  PRIMARY KEY (`CharacterId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `characters_value`
--

LOCK TABLES `characters_value` WRITE;
/*!40000 ALTER TABLE `characters_value` DISABLE KEYS */;
/*!40000 ALTER TABLE `characters_value` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `characters_value_24hr`
--

DROP TABLE IF EXISTS `characters_value_24hr`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `characters_value_24hr` (
  `characterId` int(11) NOT NULL,
  `Level` int(11) DEFAULT NULL,
  `xp` int(11) DEFAULT NULL,
  `RenownRank` int(11) DEFAULT NULL,
  `Money` int(11) DEFAULT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`characterId`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `characters_value_24hr`
--

LOCK TABLES `characters_value_24hr` WRITE;
/*!40000 ALTER TABLE `characters_value_24hr` DISABLE KEYS */;
/*!40000 ALTER TABLE `characters_value_24hr` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `characters_value_hourly`
--

DROP TABLE IF EXISTS `characters_value_hourly`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `characters_value_hourly` (
  `characterId` int(11) NOT NULL,
  `Level` int(11) DEFAULT NULL,
  `xp` int(11) DEFAULT NULL,
  `RenownRank` int(11) DEFAULT NULL,
  `Money` int(11) DEFAULT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `Renown` int(11) DEFAULT NULL,
  PRIMARY KEY (`characterId`,`timestamp`),
  KEY `IX_RR` (`RenownRank`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `characters_value_hourly`
--

LOCK TABLES `characters_value_hourly` WRITE;
/*!40000 ALTER TABLE `characters_value_hourly` DISABLE KEYS */;
/*!40000 ALTER TABLE `characters_value_hourly` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `gmcommandlogs`
--

DROP TABLE IF EXISTS `gmcommandlogs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `gmcommandlogs` (
  `AccountId` int(10) unsigned DEFAULT NULL,
  `PlayerName` varchar(255) DEFAULT NULL,
  `Command` text,
  `Date` datetime DEFAULT NULL,
  `gmcommandlogs_ID` varchar(255) NOT NULL,
  PRIMARY KEY (`gmcommandlogs_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `gmcommandlogs`
--

LOCK TABLES `gmcommandlogs` WRITE;
/*!40000 ALTER TABLE `gmcommandlogs` DISABLE KEYS */;
/*!40000 ALTER TABLE `gmcommandlogs` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `guild_alliance_info`
--

DROP TABLE IF EXISTS `guild_alliance_info`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `guild_alliance_info` (
  `AllianceId` int(10) unsigned NOT NULL,
  `Name` varchar(255) NOT NULL,
  PRIMARY KEY (`AllianceId`),
  UNIQUE KEY `Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `guild_alliance_info`
--

LOCK TABLES `guild_alliance_info` WRITE;
/*!40000 ALTER TABLE `guild_alliance_info` DISABLE KEYS */;
/*!40000 ALTER TABLE `guild_alliance_info` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `guild_event`
--

DROP TABLE IF EXISTS `guild_event`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `guild_event` (
  `SlotId` tinyint(3) unsigned NOT NULL,
  `GuildId` int(10) unsigned NOT NULL,
  `CharacterId` int(10) unsigned NOT NULL,
  `Begin` int(10) unsigned NOT NULL,
  `End` int(10) unsigned NOT NULL,
  `Name` text NOT NULL,
  `Description` text NOT NULL,
  `Alliance` tinyint(3) unsigned NOT NULL,
  `Locked` tinyint(3) unsigned NOT NULL,
  `Signups` text NOT NULL,
  `guild_event_ID` varchar(255) NOT NULL,
  PRIMARY KEY (`guild_event_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `guild_event`
--

LOCK TABLES `guild_event` WRITE;
/*!40000 ALTER TABLE `guild_event` DISABLE KEYS */;
/*!40000 ALTER TABLE `guild_event` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `guild_info`
--

DROP TABLE IF EXISTS `guild_info`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `guild_info` (
  `GuildId` int(10) unsigned NOT NULL,
  `Name` varchar(255) NOT NULL,
  `Level` tinyint(3) unsigned NOT NULL,
  `Realm` tinyint(3) unsigned NOT NULL,
  `LeaderId` int(10) unsigned NOT NULL,
  `CreateDate` int(11) NOT NULL,
  `Motd` text NOT NULL,
  `AboutUs` text NOT NULL,
  `Xp` int(10) unsigned NOT NULL,
  `Renown` bigint(20) unsigned NOT NULL,
  `BriefDescription` text NOT NULL,
  `Summary` text NOT NULL,
  `PlayStyle` tinyint(3) unsigned NOT NULL,
  `Atmosphere` tinyint(3) unsigned NOT NULL,
  `CareersNeeded` int(10) unsigned NOT NULL,
  `Interests` tinyint(3) unsigned NOT NULL,
  `ActivelyRecruiting` tinyint(3) unsigned NOT NULL,
  `RanksNeeded` tinyint(3) unsigned NOT NULL,
  `Tax` tinyint(3) unsigned NOT NULL,
  `Money` bigint(20) unsigned NOT NULL,
  `guildvaultpurchased` text NOT NULL,
  `Banners` text NOT NULL,
  `Heraldry` text NOT NULL,
  `GuildTacticsPurchased` text NOT NULL,
  `AllianceId` int(10) unsigned DEFAULT NULL,
  PRIMARY KEY (`GuildId`),
  UNIQUE KEY `Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `guild_info`
--

LOCK TABLES `guild_info` WRITE;
/*!40000 ALTER TABLE `guild_info` DISABLE KEYS */;
/*!40000 ALTER TABLE `guild_info` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `guild_logs`
--

DROP TABLE IF EXISTS `guild_logs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `guild_logs` (
  `GuildId` int(10) unsigned NOT NULL,
  `Time` int(10) unsigned NOT NULL,
  `Type` tinyint(3) unsigned NOT NULL,
  `Text` text NOT NULL,
  `guild_logs_ID` varchar(255) NOT NULL,
  PRIMARY KEY (`guild_logs_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `guild_logs`
--

LOCK TABLES `guild_logs` WRITE;
/*!40000 ALTER TABLE `guild_logs` DISABLE KEYS */;
/*!40000 ALTER TABLE `guild_logs` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `guild_members`
--

DROP TABLE IF EXISTS `guild_members`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `guild_members` (
  `GuildId` int(10) unsigned NOT NULL,
  `CharacterId` int(10) unsigned NOT NULL,
  `RankId` tinyint(3) unsigned NOT NULL,
  `PublicNote` text NOT NULL,
  `OfficerNote` text NOT NULL,
  `JoinDate` int(10) unsigned NOT NULL,
  `LastSeen` int(10) unsigned NOT NULL,
  `RealmCaptain` tinyint(3) unsigned NOT NULL,
  `StandardBearer` tinyint(3) unsigned NOT NULL,
  `GuildRecruiter` tinyint(3) unsigned NOT NULL,
  `RenownContributed` bigint(20) unsigned NOT NULL,
  `Tithe` tinyint(3) unsigned NOT NULL,
  `TitheContributed` bigint(20) unsigned NOT NULL,
  PRIMARY KEY (`CharacterId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `guild_members`
--

LOCK TABLES `guild_members` WRITE;
/*!40000 ALTER TABLE `guild_members` DISABLE KEYS */;
/*!40000 ALTER TABLE `guild_members` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `guild_ranks`
--

DROP TABLE IF EXISTS `guild_ranks`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `guild_ranks` (
  `GuildId` int(10) unsigned NOT NULL,
  `RankId` tinyint(3) unsigned NOT NULL,
  `Name` text NOT NULL,
  `Permissions` text NOT NULL,
  `Enabled` tinyint(3) unsigned NOT NULL,
  PRIMARY KEY (`GuildId`,`RankId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `guild_ranks`
--

LOCK TABLES `guild_ranks` WRITE;
/*!40000 ALTER TABLE `guild_ranks` DISABLE KEYS */;
/*!40000 ALTER TABLE `guild_ranks` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `guild_vault_item`
--

DROP TABLE IF EXISTS `guild_vault_item`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `guild_vault_item` (
  `GuildId` int(10) unsigned NOT NULL,
  `Entry` int(10) unsigned NOT NULL,
  `VaultId` tinyint(3) unsigned NOT NULL,
  `SlotId` smallint(5) unsigned NOT NULL,
  `Counts` smallint(5) unsigned NOT NULL,
  `Talismans` varchar(40) DEFAULT NULL,
  `PrimaryDye` smallint(5) unsigned NOT NULL,
  `SecondaryDye` smallint(5) unsigned NOT NULL,
  PRIMARY KEY (`GuildId`,`VaultId`,`SlotId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `guild_vault_item`
--

LOCK TABLES `guild_vault_item` WRITE;
/*!40000 ALTER TABLE `guild_vault_item` DISABLE KEYS */;
/*!40000 ALTER TABLE `guild_vault_item` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `patcher_fileassets`
--

DROP TABLE IF EXISTS `patcher_fileassets`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `patcher_fileassets` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `FilePath` varchar(450) DEFAULT NULL,
  `CRC32` int(11) DEFAULT NULL,
  `Size` int(11) DEFAULT NULL,
  `ModifyDate` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `patcher_fileassets`
--

LOCK TABLES `patcher_fileassets` WRITE;
/*!40000 ALTER TABLE `patcher_fileassets` DISABLE KEYS */;
INSERT INTO `patcher_fileassets` VALUES (9,'dev.myp',-1776282501,222369,'2018-11-09 03:05:05');
/*!40000 ALTER TABLE `patcher_fileassets` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `scenario_durations`
--

DROP TABLE IF EXISTS `scenario_durations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `scenario_durations` (
  `Guid` int(11) NOT NULL AUTO_INCREMENT,
  `ScenarioId` smallint(5) unsigned DEFAULT NULL,
  `Tier` tinyint(3) unsigned DEFAULT NULL,
  `StartTime` bigint(20) DEFAULT NULL,
  `DurationSeconds` int(10) unsigned DEFAULT NULL,
  PRIMARY KEY (`Guid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `scenario_durations`
--

LOCK TABLES `scenario_durations` WRITE;
/*!40000 ALTER TABLE `scenario_durations` DISABLE KEYS */;
/*!40000 ALTER TABLE `scenario_durations` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2019-12-29 14:11:39
