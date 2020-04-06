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
-- Table structure for table `world_settings`
--

DROP TABLE IF EXISTS `world_settings`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `world_settings` (
  `SettingId` int(11) NOT NULL AUTO_INCREMENT,
  `Setting` int(11) DEFAULT NULL,
  `Description` text,
  PRIMARY KEY (`SettingId`)
) ENGINE=InnoDB AUTO_INCREMENT=20 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `world_settings`
--

LOCK TABLES `world_settings` WRITE;
/*!40000 ALTER TABLE `world_settings` DISABLE KEYS */;
INSERT INTO `world_settings` VALUES (1,43,'This is bolster level used for T2 T3 and T4 during DoomsDay Event'),
(2,1,'This setting sets the medal generation system - 0 is classic, 1 is based on RR of KILLER.'),
(3,1,'This switch disables bags if pop requirement is not met.'),
(4,20,'This is Supplies Scaler.'),
(5,25,'Door regen scaler, 10000 is 100%.'),
(6,1,'Movement Packet Throtle, 0 - disabled 1 - enabled'),
(7,10,'This is amount of ammunition refreshed every 1 minute per 1 BO, this value is divided by 10 to allow for fractures.'),
(8,5,'Keep supplies decay value, end result is divided by 10.'),
(9,0,'Minimum number of BOs to prevent keep decay.'),
(10,0,'Enables (1) or disables (0) supplies from abandoned BOs.'),
(11,1,'Enables (1) or disables (0) new aggro system (with healing).'),
(19,0,'Killswitch for pet modifier code');
/*!40000 ALTER TABLE `world_settings` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2020-01-07 15:42:33
