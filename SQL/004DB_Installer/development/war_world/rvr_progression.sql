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
-- Table structure for table `rvr_progression`
--

DROP TABLE IF EXISTS `rvr_progression`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `rvr_progression` (
  `tier` int(11) DEFAULT NULL,
  `PairingId` int(11) DEFAULT NULL,
  `Description` varchar(80) DEFAULT NULL,
  `BattleFrontId` int(11) NOT NULL,
  `OrderWinProgression` int(11) NOT NULL,
  `DestWinProgression` int(11) NOT NULL,
  `RegionId` int(11) NOT NULL,
  `ZoneId` int(11) DEFAULT NULL,
  `DefaultRealmLock` int(11) DEFAULT NULL,
  `ResetProgressionOnEntry` int(11) DEFAULT NULL,
  `LastOwningRealm` int(11) NOT NULL DEFAULT '0',
  `LastOpenedZone` int(11) NOT NULL DEFAULT '0',
  `OrderVP` int(11) NOT NULL DEFAULT '0',
  `DestroVP` int(11) NOT NULL DEFAULT '0',
  `DestroKeepId` int(11) NOT NULL,
  `OrderKeepId` int(11) NOT NULL,
  PRIMARY KEY (`BattleFrontId`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `rvr_progression`
--

LOCK TABLES `rvr_progression` WRITE;
/*!40000 ALTER TABLE `rvr_progression` DISABLE KEYS */;
INSERT INTO `rvr_progression` VALUES (4,2,'Chaos Wastes',1,15,2,11,103,2,0,2,0,0,0,19,20),
(4,2,'Praag',2,1,3,11,105,0,1,0,1,300,300,18,17),
(4,2,'Reikland',3,2,13,11,109,1,0,1,0,0,0,15,16),
(4,1,'Black Crag',4,16,5,2,3,2,0,2,0,0,0,10,9),
(4,1,'Thunder Mountain',5,4,6,2,5,0,0,0,0,0,0,7,8),
(4,1,'Kadrin Valley',6,5,14,2,9,1,0,1,0,0,0,6,5),
(4,3,'Eataine',7,8,17,4,209,1,0,1,0,0,0,26,25),
(4,3,'Dragonwake',8,9,7,4,205,0,0,0,0,0,0,28,27),
(4,3,'Caledor',9,18,8,4,203,2,0,2,0,0,0,29,30),
(1,2,'Norsca / Nordland',10,11,11,8,106,0,1,0,0,0,0,0,0),
(1,1,'Ekrund / Mt Bloodhorn',11,12,12,1,6,2,0,2,1,0,0,0,0),
(1,3,'Chrace / Blighted Isle',12,10,10,3,206,1,0,1,0,0,0,0,0),
(4,2,'Reikwald',13,5,5,11,110,1,0,1,0,0,0,100,100),
(4,1,'Stonewatch',14,8,8,2,10,1,0,1,0,0,0,103,103),
(4,2,'The Maw',15,5,5,11,104,2,0,2,0,0,0,101,101),
(4,1,'Butchers Pass',16,8,8,2,4,2,0,2,0,0,0,102,102),
(4,3,'Shining Way',17,2,2,4,210,1,0,1,0,0,0,105,105),
(4,3,'Fell Landing',18,2,2,4,204,2,0,2,0,0,0,104,104);
/*!40000 ALTER TABLE `rvr_progression` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2020-01-07 15:42:22
