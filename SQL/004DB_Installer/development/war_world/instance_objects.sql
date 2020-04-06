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
-- Table structure for table `instance_objects`
--

DROP TABLE IF EXISTS `instance_objects`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `instance_objects` (
  `instance_objects_ID` varchar(255) NOT NULL,
  `Entry` int(10) unsigned DEFAULT NULL,
  `InstanceID` int(10) unsigned DEFAULT NULL,
  `EncounterID` int(10) unsigned DEFAULT NULL,
  `DoorID` int(10) unsigned DEFAULT NULL,
  `GameObjectSpawnID` int(10) unsigned DEFAULT NULL,
  `Name` text,
  `WorldX` int(10) unsigned DEFAULT NULL,
  `WorldY` int(10) unsigned DEFAULT NULL,
  `WorldZ` int(10) unsigned DEFAULT NULL,
  `WorldO` int(10) unsigned DEFAULT NULL,
  `DisplayID` int(10) unsigned DEFAULT NULL,
  `VfxState` int(10) unsigned DEFAULT NULL,
  PRIMARY KEY (`instance_objects_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `instance_objects`
--

LOCK TABLES `instance_objects` WRITE;
/*!40000 ALTER TABLE `instance_objects` DISABLE KEYS */;
INSERT INTO `instance_objects` VALUES ('1',100272,260,332,NULL,258490,NULL,1410071,1581747,6136,92,1789,0),
('2',100272,260,331,NULL,258492,NULL,1397228,1580169,6898,1683,1789,0),
('3',100272,260,NULL,NULL,258470,NULL,1404210,1573508,7952,3458,1789,0),
('4',44,160,NULL,NULL,260410,NULL,1016191,1029338,5828,4070,4496,0),
('5',2000576,176,NULL,NULL,2000576,NULL,1499152,220000,8872,1535,16,0);
/*!40000 ALTER TABLE `instance_objects` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2020-01-07 15:41:50
