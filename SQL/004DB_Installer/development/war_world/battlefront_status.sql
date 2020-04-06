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
-- Table structure for table `battlefront_status`
--

DROP TABLE IF EXISTS `battlefront_status`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `battlefront_status` (
  `RegionId` int(11) NOT NULL AUTO_INCREMENT,
  `OpenZoneIndex` int(11) DEFAULT NULL,
  `ActiveRegionOrZone` int(11) DEFAULT NULL,
  `ControlingRealm` int(11) DEFAULT NULL,
  PRIMARY KEY (`RegionId`)
) ENGINE=InnoDB AUTO_INCREMENT=451 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `battlefront_status`
--

LOCK TABLES `battlefront_status` WRITE;
/*!40000 ALTER TABLE `battlefront_status` DISABLE KEYS */;
INSERT INTO `battlefront_status` VALUES (2,1,5,0),
(4,1,205,0),
(5,1,NULL,NULL),
(6,0,0,1),
(7,1,NULL,NULL),
(9,1,NULL,NULL),
(10,0,0,1),
(11,1,105,0),
(12,1,1,0),
(14,1,1,0),
(15,1,1,0),
(16,0,0,2),
(17,1,NULL,NULL),
(30,1,0,0),
(31,1,0,0),
(33,1,0,0),
(36,1,0,0),
(37,1,0,0),
(44,1,0,0),
(50,1,NULL,NULL),
(60,1,NULL,NULL),
(61,1,0,0),
(62,1,0,0),
(63,1,NULL,NULL),
(64,1,0,0),
(65,1,NULL,NULL),
(66,1,0,0),
(130,1,0,0),
(131,1,0,0),
(132,1,0,0),
(133,1,0,0),
(134,1,0,0),
(136,1,0,0),
(137,1,0,0),
(152,1,0,0),
(157,1,0,0),
(159,1,0,0),
(160,1,NULL,NULL),
(167,1,0,0),
(168,1,0,0),
(169,1,0,0),
(171,1,0,0),
(179,1,0,0),
(195,1,0,0),
(196,1,0,0),
(197,1,0,0),
(231,1,0,0),
(232,1,0,0),
(234,1,0,0),
(235,1,0,0),
(241,1,0,0),
(242,1,0,0),
(244,1,0,0),
(252,1,0,0),
(260,1,NULL,NULL),
(400,1,0,0),
(410,1,0,0),
(411,1,0,0),
(412,1,0,0),
(450,1,0,0);
/*!40000 ALTER TABLE `battlefront_status` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2020-01-07 15:41:19
