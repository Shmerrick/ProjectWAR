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
-- Table structure for table `boss_spawn`
--

DROP TABLE IF EXISTS `boss_spawn`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `boss_spawn` (
  `BossSpawnId` int(11) NOT NULL,
  `ProtoId` int(11) NOT NULL,
  `SpawnGuid` int(11) NOT NULL,
  `Name` text,
  `Enabled` int(11) NOT NULL,
  PRIMARY KEY (`BossSpawnId`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `boss_spawn`
--

LOCK TABLES `boss_spawn` WRITE;
/*!40000 ALTER TABLE `boss_spawn` DISABLE KEYS */;
INSERT INTO `boss_spawn` VALUES (3650,3650,3,'The Bulbous One',1),
(7358,7358,9,'Grydal Bloodshroud^M',1),
(8530,8530,15,'Juggernaut^n',1),
(16078,16078,17,'Wrackspite^M',1),
(16081,16081,7,'Beastrip^M',1),
(19409,19409,2,'Kokrit',1),
(33182,33182,1,'Goremane',1),
(45084,45084,8,'Thar\'lgnan^M',1),
(45224,45224,6,'Gahlvoth Darkrage^M',1),
(46325,46325,20,'Zekaraz the Bloodcaller^M',1),
(46327,46327,19,'Clawfang^M',1),
(48112,48112,16,'Lord Slaurith^M',1),
(49164,49164,14,'Chorek the Unstoppable^M',1),
(64106,64106,22,'Skull Lord Var\'Ithrok',1),
(2000687,2000687,4,'Azuk\'Thul^M',1),
(2000690,2000690,5,'SummonBorzhar',1),
(2000692,2000692,18,'Doomspike^M',1),
(2000706,2000706,10,'Urlf Daemonblessed^M',1),
(2000707,2000707,11,'Garithex the Mountain^M',1),
(2000708,2000708,12,'Hargan the Furious^M',1),
(2000712,2000712,13,'Skulltaker Bloodgiant^m',1),
(2000751,2000751,21,'Kaarn the Vanquisher^M',1);
/*!40000 ALTER TABLE `boss_spawn` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2020-01-07 15:41:20
