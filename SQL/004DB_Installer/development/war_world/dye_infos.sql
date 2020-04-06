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
-- Table structure for table `dye_infos`
--

DROP TABLE IF EXISTS `dye_infos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `dye_infos` (
  `Entry` smallint(5) unsigned NOT NULL,
  `Price` int(10) unsigned NOT NULL,
  `Name` varchar(255) NOT NULL,
  PRIMARY KEY (`Entry`),
  UNIQUE KEY `Entry` (`Entry`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `dye_infos`
--

LOCK TABLES `dye_infos` WRITE;
/*!40000 ALTER TABLE `dye_infos` DISABLE KEYS */;
INSERT INTO `dye_infos` VALUES (156,35000,'Order of the Gryphon Black'),
(172,35000,'Norscan Farmer Overall Blue'),
(179,200,'Chainmail Grey'),
(183,200,'Fortress Grey'),
(190,500,'Graveyard Earth'),
(192,200,'Bone Brown'),
(193,1000,'Bestial Brown'),
(194,1000,'Tanned Flesh'),
(195,200,'Vermin Brown'),
(197,200,'Brazen Brass'),
(203,200,'Orc Green'),
(204,200,'Snot Green'),
(207,200,'Scaly Green'),
(208,200,'Shadow Grey'),
(215,1000,'Warlock Purple'),
(219,200,'Scab Red'),
(306,20000,'Codex Grey'),
(309,20000,'Desert Yellow'),
(314,20000,'Ice Blue'),
(316,35000,'Mithril Silver'),
(328,7500,'Calthan Brown'),
(329,20000,'Charadon Granite'),
(333,20000,'Hormagaunt Purple'),
(336,20000,'Knarloc Green'),
(337,20000,'Macharius Solar Orange'),
(338,20000,'Mechrite Red'),
(340,20000,'Necron Abyss'),
(346,50000,'Badab Black'),
(347,20000,'Devlan Mud'),
(351,20000,'Thraka Green');
/*!40000 ALTER TABLE `dye_infos` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2020-01-07 15:41:39
