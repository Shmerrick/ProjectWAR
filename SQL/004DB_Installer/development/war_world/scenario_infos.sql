-- MySQL dump 10.13  Distrib 5.5.37, for Win32 (AMD64)
--
-- Host: 127.0.0.1    Database: war_world
-- ------------------------------------------------------
-- Server version	8.0.18

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `scenario_infos`
--

DROP TABLE IF EXISTS `scenario_infos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `scenario_infos` (
  `ScenarioID` smallint(5) unsigned NOT NULL DEFAULT '0',
  `Name` varchar(255) DEFAULT NULL,
  `MinLevel` tinyint(3) unsigned NOT NULL,
  `MaxLevel` tinyint(3) unsigned NOT NULL,
  `MinPlayers` tinyint(3) unsigned NOT NULL,
  `MaxPlayers` tinyint(3) unsigned NOT NULL,
  `Type` int(11) NOT NULL,
  `Tier` int(11) NOT NULL,
  `MapID` smallint(5) unsigned NOT NULL,
  `KillPointScore` tinyint(3) unsigned DEFAULT NULL,
  `RewardScaler` float NOT NULL,
  `DeferKills` tinyint(3) unsigned DEFAULT NULL,
  `Enabled` tinyint(3) unsigned NOT NULL,
  `QueueType` int(11) NOT NULL,
  `RegionId` smallint(5) unsigned NOT NULL,
  PRIMARY KEY (`ScenarioID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `scenario_infos`
--

LOCK TABLES `scenario_infos` WRITE;
/*!40000 ALTER TABLE `scenario_infos` DISABLE KEYS */;
INSERT INTO `scenario_infos` VALUES (2000,'Gates of Ekrund',1,40,0,12,1,4,30,5,1,0,0,0,30),
(2001,'Mourkain Temple',1,40,0,12,2,4,31,10,1,0,0,0,31),
(2002,'Black Fire Basin',1,40,0,12,10,4,38,1,1,0,0,0,38),
(2003,'Kadrin Valley Pass',1,40,0,12,6,4,36,5,1,0,0,0,36),
(2004,'Gromril Crossing',1,40,0,12,6,4,43,2,1,0,0,0,43),
(2005,'Thunder Valley',1,40,0,12,1,4,34,5,1,0,0,0,34),
(2006,'Logrin\'s Forge',1,40,0,12,3,4,39,2,1,0,0,0,39),
(2007,'Black Crag Keep',1,40,0,12,1,4,37,5,1,0,0,0,37),
(2008,'Howling Gorge',1,40,0,12,4,4,44,2,1,0,0,0,44),
(2009,'Karaz-a-Karak Gates',1,40,0,12,1,4,0,0,1,0,0,0,0),
(2010,'Eight Peaks Gates',1,40,0,12,1,4,0,0,1,0,0,0,0),
(2011,'Doomfist Crater',1,40,0,12,1,4,33,2,1,0,0,0,33),
(2012,'Altdorf War Quarters',1,40,0,24,6,4,41,0,1,0,1,0,41),
(2013,'The Undercroft',1,40,0,24,6,4,42,0,1,0,0,0,42),
(2015,'The Ironclad',1,40,0,12,1,4,32,5,1,0,0,0,32),
(2100,'Nordenwatch',1,40,0,12,1,4,130,5,1,0,0,0,130),
(2101,'Stonetroll Crossing',1,40,0,12,9,4,131,2,1,0,0,0,131),
(2102,'Talabec Dam',1,40,0,12,4,4,132,2,1,0,0,0,132),
(2103,'High Pass Cemetery',1,40,0,12,3,4,139,5,1,0,0,0,139),
(2104,'Maw of Madness',1,40,0,12,2,4,133,5,1,0,0,0,133),
(2105,'Twisting Tower',1,40,0,12,1,4,135,5,1,0,1,0,135),
(2106,'Battle For Praag',1,40,0,12,6,4,136,2,1,0,0,0,136),
(2107,'Grovod Caverns',1,40,0,12,7,4,137,5,1,0,0,0,137),
(2108,'Reikland Factory',1,40,0,12,1,4,138,5,1,0,0,0,138),
(2109,'Reikland Hills',1,40,0,12,1,4,134,5,1,0,0,0,134),
(2110,'The Inevitable City',1,40,0,24,6,4,167,10,1,0,1,0,167),
(2111,'Altdorf',1,40,0,24,6,4,168,10,1,0,1,0,168),
(2123,'The Eternal Citadel',1,40,0,12,7,4,172,5,1,0,0,0,172),
(2136,'College of Corruption',1,40,0,12,2,4,411,5,1,0,1,0,411),
(2200,'Khaine\'s Embrace',1,40,0,12,8,4,230,5,1,0,0,0,230),
(2201,'Phoenix Gate',1,40,0,12,10,4,231,2,1,0,0,0,231),
(2202,'Tor Anroc',1,40,0,12,2,4,232,10,1,0,0,0,232),
(2203,'Lost Temple of Isha',1,40,0,12,1,4,236,2,1,0,0,0,236),
(2204,'Serpent\'s Passage',1,40,0,12,5,4,234,2,1,0,0,0,234),
(2205,'Dragon\'s Bane',1,40,0,12,1,4,235,5,1,0,0,0,235),
(2206,'Blood of the Black Cairn',1,40,0,12,11,4,238,5,1,0,0,0,238),
(2207,'Caledor Woods',1,40,0,12,1,4,237,2,1,0,0,0,237);
/*!40000 ALTER TABLE `scenario_infos` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2020-01-07 15:42:29
