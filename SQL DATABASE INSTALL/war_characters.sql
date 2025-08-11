/*
Navicat MySQL Data Transfer

Source Server         : warserver
Source Server Version : 80031
Source Host           : localhost:3306
Source Database       : war_characters

Target Server Type    : MYSQL
Target Server Version : 80031
File Encoding         : 65001

Date: 2022-12-21 08:07:51
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for `auctions`
-- ----------------------------
DROP TABLE IF EXISTS `auctions`;
CREATE TABLE `auctions` (
  `AuctionId` bigint unsigned NOT NULL,
  `Realm` tinyint unsigned NOT NULL,
  `SellerId` int unsigned NOT NULL,
  `ItemId` int unsigned NOT NULL,
  `SellPrice` int unsigned NOT NULL,
  `Count` smallint unsigned NOT NULL,
  `StartTime` int unsigned NOT NULL,
  `Talismans` varchar(40) DEFAULT NULL,
  `PrimaryDye` smallint unsigned NOT NULL,
  `SecondaryDye` smallint unsigned NOT NULL,
  PRIMARY KEY (`AuctionId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of auctions
-- ----------------------------

-- ----------------------------
-- Table structure for `banned_names`
-- ----------------------------
DROP TABLE IF EXISTS `banned_names`;
CREATE TABLE `banned_names` (
  `NameString` varchar(255) NOT NULL,
  `FilterTypeString` text,
  PRIMARY KEY (`NameString`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of banned_names
-- ----------------------------

-- ----------------------------
-- Table structure for `bug_report`
-- ----------------------------
DROP TABLE IF EXISTS `bug_report`;
CREATE TABLE `bug_report` (
  `AccountId` int unsigned NOT NULL,
  `CharacterId` int unsigned NOT NULL,
  `ZoneId` smallint unsigned NOT NULL,
  `X` smallint unsigned NOT NULL,
  `Y` smallint unsigned NOT NULL,
  `Time` int unsigned NOT NULL,
  `Type` tinyint unsigned NOT NULL,
  `Category` tinyint unsigned NOT NULL,
  `Message` text NOT NULL,
  `ReportType` text NOT NULL,
  `FieldSting` text NOT NULL,
  `Assigned` text,
  `bug_report_ID` varchar(255) NOT NULL,
  PRIMARY KEY (`bug_report_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of bug_report
-- ----------------------------

-- ----------------------------
-- Table structure for `characters`
-- ----------------------------
DROP TABLE IF EXISTS `characters`;
CREATE TABLE `characters` (
  `CharacterId` int unsigned NOT NULL,
  `Name` varchar(24) NOT NULL,
  `Surname` varchar(24) NOT NULL,
  `RealmId` int NOT NULL,
  `AccountId` int NOT NULL,
  `SlotId` tinyint unsigned NOT NULL,
  `ModelId` tinyint unsigned NOT NULL,
  `Career` tinyint unsigned NOT NULL,
  `CareerLine` tinyint unsigned NOT NULL,
  `Realm` tinyint unsigned NOT NULL,
  `HeldLeft` int NOT NULL,
  `Race` tinyint unsigned NOT NULL,
  `Traits` text NOT NULL,
  `Sex` tinyint unsigned NOT NULL,
  `Anonymous` tinyint unsigned NOT NULL,
  `Hidden` tinyint unsigned NOT NULL,
  `OldName` varchar(24) NOT NULL,
  `PetName` varchar(24) NOT NULL,
  `PetModel` smallint unsigned NOT NULL,
  `HonorPoints` smallint unsigned NOT NULL,
  `HonorRank` smallint unsigned NOT NULL,
  PRIMARY KEY (`CharacterId`),
  UNIQUE KEY `Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of characters
-- ----------------------------

-- ----------------------------
-- Table structure for `characters_items`
-- ----------------------------
DROP TABLE IF EXISTS `characters_items`;
CREATE TABLE `characters_items` (
  `Guid` bigint NOT NULL,
  `CharacterId` int unsigned NOT NULL,
  `Entry` int unsigned NOT NULL,
  `SlotId` smallint unsigned NOT NULL,
  `ModelId` int unsigned NOT NULL,
  `Counts` smallint unsigned NOT NULL,
  `Talismans` varchar(40) DEFAULT NULL,
  `PrimaryDye` smallint unsigned NOT NULL,
  `SecondaryDye` smallint unsigned NOT NULL,
  `BoundtoPlayer` tinyint unsigned NOT NULL,
  `Alternate_AppereanceEntry` int unsigned NOT NULL,
  `characters_items_ID` varchar(255) NOT NULL,
  PRIMARY KEY (`characters_items_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of characters_items
-- ----------------------------

-- ----------------------------
-- Table structure for `characters_mails`
-- ----------------------------
DROP TABLE IF EXISTS `characters_mails`;
CREATE TABLE `characters_mails` (
  `Guid` int NOT NULL AUTO_INCREMENT,
  `AuctionType` tinyint unsigned NOT NULL,
  `CharacterId` int unsigned NOT NULL,
  `CharacterIdSender` int unsigned NOT NULL,
  `SenderName` varchar(255) NOT NULL,
  `ReceiverName` varchar(255) NOT NULL,
  `SendDate` int unsigned NOT NULL,
  `ReadDate` int unsigned NOT NULL,
  `Title` varchar(255) NOT NULL,
  `Content` text NOT NULL,
  `Money` int unsigned NOT NULL,
  `Cr` tinyint unsigned NOT NULL,
  `Opened` tinyint unsigned NOT NULL,
  `ItemsString` text NOT NULL,
  PRIMARY KEY (`Guid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of characters_mails
-- ----------------------------

-- ----------------------------
-- Table structure for `characters_quests`
-- ----------------------------
DROP TABLE IF EXISTS `characters_quests`;
CREATE TABLE `characters_quests` (
  `CharacterId` int unsigned NOT NULL,
  `QuestID` smallint unsigned NOT NULL,
  `Objectives` varchar(64) NOT NULL,
  `Done` tinyint unsigned NOT NULL,
  PRIMARY KEY (`CharacterId`,`QuestID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of characters_quests
-- ----------------------------

-- ----------------------------
-- Table structure for `characters_socials`
-- ----------------------------
DROP TABLE IF EXISTS `characters_socials`;
CREATE TABLE `characters_socials` (
  `CharacterId` int unsigned NOT NULL,
  `DistCharacterId` int unsigned NOT NULL,
  `DistName` varchar(255) NOT NULL,
  `Friend` tinyint unsigned NOT NULL,
  `Ignore` tinyint unsigned NOT NULL,
  PRIMARY KEY (`CharacterId`,`DistCharacterId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of characters_socials
-- ----------------------------

-- ----------------------------
-- Table structure for `characters_toks`
-- ----------------------------
DROP TABLE IF EXISTS `characters_toks`;
CREATE TABLE `characters_toks` (
  `CharacterId` int unsigned NOT NULL,
  `TokEntry` smallint unsigned NOT NULL,
  `Count` int unsigned DEFAULT NULL,
  PRIMARY KEY (`CharacterId`,`TokEntry`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of characters_toks
-- ----------------------------

-- ----------------------------
-- Table structure for `characters_toks_kills`
-- ----------------------------
DROP TABLE IF EXISTS `characters_toks_kills`;
CREATE TABLE `characters_toks_kills` (
  `CharacterId` int unsigned NOT NULL,
  `NPCEntry` smallint unsigned NOT NULL,
  `Count` int unsigned DEFAULT NULL,
  PRIMARY KEY (`CharacterId`,`NPCEntry`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of characters_toks_kills
-- ----------------------------

-- ----------------------------
-- Table structure for `characters_value`
-- ----------------------------
DROP TABLE IF EXISTS `characters_value`;
CREATE TABLE `characters_value` (
  `CharacterId` int unsigned NOT NULL,
  `Level` tinyint unsigned NOT NULL,
  `Xp` int unsigned NOT NULL,
  `XpMode` int NOT NULL,
  `RestXp` int unsigned NOT NULL,
  `Renown` int unsigned NOT NULL,
  `RenownRank` tinyint unsigned NOT NULL,
  `Money` int unsigned NOT NULL,
  `Speed` int NOT NULL,
  `PlayedTime` int unsigned NOT NULL,
  `LastSeen` int DEFAULT NULL,
  `RegionId` int NOT NULL,
  `ZoneId` smallint unsigned NOT NULL,
  `WorldX` int NOT NULL,
  `WorldY` int NOT NULL,
  `WorldZ` int NOT NULL,
  `WorldO` int NOT NULL,
  `RallyPoint` smallint unsigned NOT NULL,
  `BagBuy` tinyint unsigned NOT NULL,
  `BankBuy` tinyint unsigned NOT NULL,
  `Skills` int unsigned NOT NULL,
  `Online` tinyint unsigned NOT NULL,
  `GearShow` tinyint unsigned NOT NULL,
  `TitleId` smallint unsigned NOT NULL,
  `RenownSkills` text NOT NULL,
  `MasterySkills` text NOT NULL,
  `Morale1` smallint unsigned DEFAULT NULL,
  `Morale2` smallint unsigned DEFAULT NULL,
  `Morale3` smallint unsigned DEFAULT NULL,
  `Morale4` smallint unsigned DEFAULT NULL,
  `Tactic1` smallint unsigned DEFAULT NULL,
  `Tactic2` smallint unsigned DEFAULT NULL,
  `Tactic3` smallint unsigned DEFAULT NULL,
  `Tactic4` smallint unsigned DEFAULT NULL,
  `GatheringSkill` tinyint unsigned NOT NULL,
  `GatheringSkillLevel` tinyint unsigned NOT NULL,
  `CraftingSkill` tinyint unsigned NOT NULL,
  `CraftingSkillLevel` tinyint unsigned NOT NULL,
  `ExperimentalMode` tinyint unsigned NOT NULL,
  `RVRKills` int unsigned NOT NULL,
  `RVRDeaths` int unsigned NOT NULL,
  `CraftingBags` tinyint unsigned NOT NULL,
  `PendingXp` int unsigned DEFAULT NULL,
  `PendingRenown` int unsigned DEFAULT NULL,
  `Lockouts` text NOT NULL,
  `DisconcetTime` int NOT NULL,
  PRIMARY KEY (`CharacterId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of characters_value
-- ----------------------------

-- ----------------------------
-- Table structure for `character_abilities`
-- ----------------------------
DROP TABLE IF EXISTS `character_abilities`;
CREATE TABLE `character_abilities` (
  `CharacterID` int DEFAULT NULL,
  `AbilityID` smallint unsigned DEFAULT NULL,
  `LastCast` int DEFAULT NULL,
  `character_abilities_ID` varchar(255) NOT NULL,
  PRIMARY KEY (`character_abilities_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of character_abilities
-- ----------------------------

-- ----------------------------
-- Table structure for `character_bag_bonus`
-- ----------------------------
DROP TABLE IF EXISTS `character_bag_bonus`;
CREATE TABLE `character_bag_bonus` (
  `BonusId` bigint NOT NULL AUTO_INCREMENT,
  `GoldBag` int NOT NULL,
  `PurpleBag` int NOT NULL,
  `BlueBag` int NOT NULL,
  `GreenBag` int NOT NULL,
  `WhiteBag` int NOT NULL,
  `Timestamp` datetime NOT NULL,
  `CharacterId` int NOT NULL,
  `CharacterName` text NOT NULL,
  PRIMARY KEY (`BonusId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of character_bag_bonus
-- ----------------------------

-- ----------------------------
-- Table structure for `character_bag_pools`
-- ----------------------------
DROP TABLE IF EXISTS `character_bag_pools`;
CREATE TABLE `character_bag_pools` (
  `CharacterId` int NOT NULL,
  `Bag_Type` int NOT NULL,
  `BagPool_Value` int NOT NULL,
  PRIMARY KEY (`CharacterId`,`Bag_Type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of character_bag_pools
-- ----------------------------

-- ----------------------------
-- Table structure for `character_client_data`
-- ----------------------------
DROP TABLE IF EXISTS `character_client_data`;
CREATE TABLE `character_client_data` (
  `CharacterId` int unsigned NOT NULL,
  `ClientDataString` text NOT NULL,
  PRIMARY KEY (`CharacterId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of character_client_data
-- ----------------------------

-- ----------------------------
-- Table structure for `character_deletions`
-- ----------------------------
DROP TABLE IF EXISTS `character_deletions`;
CREATE TABLE `character_deletions` (
  `DeletionIP` text,
  `AccountID` int DEFAULT NULL,
  `AccountName` text,
  `CharacterID` int unsigned DEFAULT NULL,
  `CharacterName` text,
  `DeletionTimeSeconds` int DEFAULT NULL,
  `character_deletions_ID` varchar(255) NOT NULL,
  PRIMARY KEY (`character_deletions_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of character_deletions
-- ----------------------------

-- ----------------------------
-- Table structure for `character_honor_reward_cooldown`
-- ----------------------------
DROP TABLE IF EXISTS `character_honor_reward_cooldown`;
CREATE TABLE `character_honor_reward_cooldown` (
  `CharacterId` int unsigned NOT NULL,
  `ItemId` int NOT NULL,
  `Cooldown` bigint NOT NULL,
  PRIMARY KEY (`CharacterId`,`ItemId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of character_honor_reward_cooldown
-- ----------------------------

-- ----------------------------
-- Table structure for `character_influences`
-- ----------------------------
DROP TABLE IF EXISTS `character_influences`;
CREATE TABLE `character_influences` (
  `CharacterId` int NOT NULL,
  `InfluenceId` smallint unsigned NOT NULL,
  `InfluenceCount` int unsigned NOT NULL,
  `Tier_1_Itemtaken` tinyint unsigned NOT NULL,
  `Tier_2_Itemtaken` tinyint unsigned NOT NULL,
  `Tier_3_Itemtaken` tinyint unsigned NOT NULL,
  PRIMARY KEY (`CharacterId`,`InfluenceId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of character_influences
-- ----------------------------

-- ----------------------------
-- Table structure for `character_saved_buffs`
-- ----------------------------
DROP TABLE IF EXISTS `character_saved_buffs`;
CREATE TABLE `character_saved_buffs` (
  `CharacterId` int unsigned NOT NULL,
  `BuffId` smallint unsigned NOT NULL,
  `Level` tinyint unsigned DEFAULT NULL,
  `StackLevel` tinyint unsigned DEFAULT NULL,
  `EndTimeSeconds` int unsigned DEFAULT NULL,
  PRIMARY KEY (`CharacterId`,`BuffId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of character_saved_buffs
-- ----------------------------

-- ----------------------------
-- Table structure for `gmcommandlogs`
-- ----------------------------
DROP TABLE IF EXISTS `gmcommandlogs`;
CREATE TABLE `gmcommandlogs` (
  `AccountId` int unsigned DEFAULT NULL,
  `PlayerName` varchar(255) DEFAULT NULL,
  `Command` text,
  `Date` datetime DEFAULT NULL,
  `gmcommandlogs_ID` varchar(255) NOT NULL,
  PRIMARY KEY (`gmcommandlogs_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of gmcommandlogs
-- ----------------------------

-- ----------------------------
-- Table structure for `guild_alliance_info`
-- ----------------------------
DROP TABLE IF EXISTS `guild_alliance_info`;
CREATE TABLE `guild_alliance_info` (
  `AllianceId` int unsigned NOT NULL,
  `Name` varchar(255) NOT NULL,
  PRIMARY KEY (`AllianceId`),
  UNIQUE KEY `Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of guild_alliance_info
-- ----------------------------

-- ----------------------------
-- Table structure for `guild_event`
-- ----------------------------
DROP TABLE IF EXISTS `guild_event`;
CREATE TABLE `guild_event` (
  `SlotId` tinyint unsigned NOT NULL,
  `GuildId` int unsigned NOT NULL,
  `CharacterId` int unsigned NOT NULL,
  `Begin` int unsigned NOT NULL,
  `End` int unsigned NOT NULL,
  `Name` text NOT NULL,
  `Description` text NOT NULL,
  `Alliance` tinyint unsigned NOT NULL,
  `Locked` tinyint unsigned NOT NULL,
  `Signups` text NOT NULL,
  `guild_event_ID` varchar(255) NOT NULL,
  PRIMARY KEY (`guild_event_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of guild_event
-- ----------------------------

-- ----------------------------
-- Table structure for `guild_info`
-- ----------------------------
DROP TABLE IF EXISTS `guild_info`;
CREATE TABLE `guild_info` (
  `GuildId` int unsigned NOT NULL,
  `Name` varchar(255) NOT NULL,
  `Level` tinyint unsigned NOT NULL,
  `Realm` tinyint unsigned NOT NULL,
  `LeaderId` int unsigned NOT NULL,
  `CreateDate` int NOT NULL,
  `Motd` text NOT NULL,
  `AboutUs` text NOT NULL,
  `Xp` int unsigned NOT NULL,
  `Renown` bigint unsigned NOT NULL,
  `BriefDescription` text NOT NULL,
  `Summary` text NOT NULL,
  `PlayStyle` tinyint unsigned NOT NULL,
  `Atmosphere` tinyint unsigned NOT NULL,
  `CareersNeeded` int unsigned NOT NULL,
  `Interests` tinyint unsigned NOT NULL,
  `ActivelyRecruiting` tinyint unsigned NOT NULL,
  `RanksNeeded` tinyint unsigned NOT NULL,
  `Tax` tinyint unsigned NOT NULL,
  `Money` bigint unsigned NOT NULL,
  `guildvaultpurchased` text NOT NULL,
  `Banners` text NOT NULL,
  `Heraldry` text NOT NULL,
  `GuildTacticsPurchased` text NOT NULL,
  `AllianceId` int unsigned DEFAULT NULL,
  PRIMARY KEY (`GuildId`),
  UNIQUE KEY `Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of guild_info
-- ----------------------------

-- ----------------------------
-- Table structure for `guild_logs`
-- ----------------------------
DROP TABLE IF EXISTS `guild_logs`;
CREATE TABLE `guild_logs` (
  `GuildId` int unsigned NOT NULL,
  `Time` int unsigned NOT NULL,
  `Type` tinyint unsigned NOT NULL,
  `Text` text NOT NULL,
  `guild_logs_ID` varchar(255) NOT NULL,
  PRIMARY KEY (`guild_logs_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of guild_logs
-- ----------------------------

-- ----------------------------
-- Table structure for `guild_members`
-- ----------------------------
DROP TABLE IF EXISTS `guild_members`;
CREATE TABLE `guild_members` (
  `GuildId` int unsigned NOT NULL,
  `CharacterId` int unsigned NOT NULL,
  `RankId` tinyint unsigned NOT NULL,
  `PublicNote` text NOT NULL,
  `OfficerNote` text NOT NULL,
  `JoinDate` int unsigned NOT NULL,
  `LastSeen` int unsigned NOT NULL,
  `RealmCaptain` tinyint unsigned NOT NULL,
  `StandardBearer` tinyint unsigned NOT NULL,
  `GuildRecruiter` tinyint unsigned NOT NULL,
  `RenownContributed` bigint unsigned NOT NULL,
  `Tithe` tinyint unsigned NOT NULL,
  `TitheContributed` bigint unsigned NOT NULL,
  PRIMARY KEY (`CharacterId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of guild_members
-- ----------------------------

-- ----------------------------
-- Table structure for `guild_ranks`
-- ----------------------------
DROP TABLE IF EXISTS `guild_ranks`;
CREATE TABLE `guild_ranks` (
  `GuildId` int unsigned NOT NULL,
  `RankId` tinyint unsigned NOT NULL,
  `Name` text NOT NULL,
  `Permissions` text NOT NULL,
  `Enabled` tinyint unsigned NOT NULL,
  PRIMARY KEY (`GuildId`,`RankId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of guild_ranks
-- ----------------------------

-- ----------------------------
-- Table structure for `guild_vault_item`
-- ----------------------------
DROP TABLE IF EXISTS `guild_vault_item`;
CREATE TABLE `guild_vault_item` (
  `GuildId` int unsigned NOT NULL,
  `Entry` int unsigned NOT NULL,
  `VaultId` tinyint unsigned NOT NULL,
  `SlotId` smallint unsigned NOT NULL,
  `Counts` smallint unsigned NOT NULL,
  `Talismans` varchar(40) DEFAULT NULL,
  `PrimaryDye` smallint unsigned NOT NULL,
  `SecondaryDye` smallint unsigned NOT NULL,
  PRIMARY KEY (`GuildId`,`VaultId`,`SlotId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of guild_vault_item
-- ----------------------------

-- ----------------------------
-- Table structure for `scenario_durations`
-- ----------------------------
DROP TABLE IF EXISTS `scenario_durations`;
CREATE TABLE `scenario_durations` (
  `Guid` int NOT NULL AUTO_INCREMENT,
  `ScenarioId` smallint unsigned DEFAULT NULL,
  `Tier` tinyint unsigned DEFAULT NULL,
  `StartTime` bigint DEFAULT NULL,
  `DurationSeconds` int unsigned DEFAULT NULL,
  PRIMARY KEY (`Guid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of scenario_durations
-- ----------------------------
