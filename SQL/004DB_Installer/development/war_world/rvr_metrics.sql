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
-- Table structure for table `rvr_metrics`
--

DROP TABLE IF EXISTS `rvr_metrics`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `rvr_metrics` (
  `MetricId` int(11) NOT NULL AUTO_INCREMENT,
  `Tier` int(11) NOT NULL,
  `BattlefrontId` int(11) NOT NULL,
  `OrderVictoryPoints` int(11) NOT NULL,
  `DestructionVictoryPoints` int(11) NOT NULL,
  `BattlefrontName` text NOT NULL,
  `OrderPlayersInLake` int(11) NOT NULL,
  `DestructionPlayersInLake` int(11) NOT NULL,
  `Locked` int(11) NOT NULL,
  `Timestamp` datetime NOT NULL,
  `GroupId` text NOT NULL,
  `TotalPlayerCountInRegion` int(11) NOT NULL,
  `TotalOrderPlayerCountInRegion` int(11) NOT NULL,
  `TotalDestPlayerCountInRegion` int(11) NOT NULL,
  `TotalPlayerCount` int(11) NOT NULL,
  `TotalFlaggedPlayerCount` int(11) NOT NULL,
  PRIMARY KEY (`MetricId`)
) ENGINE=InnoDB AUTO_INCREMENT=2323211 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `rvr_metrics`
--

LOCK TABLES `rvr_metrics` WRITE;
/*!40000 ALTER TABLE `rvr_metrics` DISABLE KEYS */;
INSERT INTO `rvr_metrics` VALUES (2323191,1,11,0,0,'Ekrund / Mt Bloodhorn',0,0,0,'2020-01-07 13:02:55','455cc0b4-5584-4dac-a034-ff2720e1440f',0,0,0,1,1),
(2323192,4,2,300,300,'Praag',0,0,0,'2020-01-07 13:03:29','09ef6b8d-b015-4125-a090-11a8b526c923',0,0,0,1,1),
(2323193,1,11,0,0,'Ekrund / Mt Bloodhorn',0,0,0,'2020-01-07 13:12:55','221e07ba-6e14-418c-a2d8-d9703aee2131',0,0,0,1,1),
(2323194,4,2,300,300,'Praag',0,0,0,'2020-01-07 13:13:29','fa7b0417-370c-496b-a383-b007b7dc594a',0,0,0,1,1),
(2323195,1,11,0,0,'Ekrund / Mt Bloodhorn',0,0,0,'2020-01-07 13:22:55','30fc704a-9b8d-4ced-9489-5f1de75a188f',0,0,0,1,1),
(2323196,4,2,300,300,'Praag',0,0,0,'2020-01-07 13:23:29','64adf7c0-468d-4e09-83c9-e313d40f24ea',0,0,0,1,1),
(2323197,1,11,0,0,'Ekrund / Mt Bloodhorn',0,0,0,'2020-01-07 13:32:55','a59d535c-7c7d-4b1a-b6e0-e72db11cac04',0,0,0,1,1),
(2323198,4,2,300,300,'Praag',0,0,0,'2020-01-07 13:33:29','c79f4d8a-a037-437a-8cea-a0aef36fa0c9',0,0,0,1,1),
(2323199,1,11,0,0,'Ekrund / Mt Bloodhorn',0,0,0,'2020-01-07 13:47:58','16f47891-2819-4bdb-8463-b3163e866b6c',0,0,0,1,1),
(2323200,4,2,300,300,'Praag',0,0,0,'2020-01-07 13:48:32','cbd309e6-42e1-4d42-9c3f-879c81ee9858',0,0,0,1,1),
(2323201,1,11,0,0,'Ekrund / Mt Bloodhorn',0,0,0,'2020-01-07 14:06:34','baed9d7c-ae5e-4f92-b4ef-3802c162fde6',0,0,0,0,0),
(2323202,4,2,300,300,'Praag',0,0,0,'2020-01-07 14:07:06','390dc515-0368-4b36-ae7a-8c9cc40c599b',0,0,0,0,0),
(2323203,1,11,0,0,'Ekrund / Mt Bloodhorn',0,0,0,'2020-01-07 14:16:34','05d900f0-18be-4458-b4a6-6239c0147210',0,0,0,0,0),
(2323204,4,2,300,300,'Praag',0,0,0,'2020-01-07 14:17:06','e39b48c8-84d6-4d93-a70a-fae9c8f37ded',0,0,0,0,0),
(2323205,1,11,0,0,'Ekrund / Mt Bloodhorn',0,0,0,'2020-01-07 14:26:34','88c2fb7c-ced9-4acc-8db9-4f0f8935aef6',0,0,0,0,0),
(2323206,4,2,300,300,'Praag',0,0,0,'2020-01-07 14:27:06','e633f36b-3162-4b89-8dc8-91af3d77f20a',0,0,0,0,0),
(2323207,1,11,0,0,'Ekrund / Mt Bloodhorn',0,0,0,'2020-01-07 14:36:34','f8ab3d99-da5f-49e1-9a98-787512101048',0,0,0,1,1),
(2323208,4,2,300,300,'Praag',0,0,0,'2020-01-07 14:37:06','c189abc9-06b4-41ae-aaf7-5d00aee20b6c',0,0,0,1,1),
(2323209,1,11,0,0,'Ekrund / Mt Bloodhorn',0,0,0,'2020-01-07 14:50:45','b4cc1889-2fb0-4fc8-a36e-52ca2de268b3',0,0,0,0,0),
(2323210,4,2,300,300,'Praag',0,0,0,'2020-01-07 14:51:17','e4fae6f6-33cf-496c-a0d6-9040ee749516',0,0,0,0,0);
/*!40000 ALTER TABLE `rvr_metrics` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2020-01-07 15:42:20
