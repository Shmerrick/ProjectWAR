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
-- Table structure for table `bounty_contribution_definition`
--

DROP TABLE IF EXISTS `bounty_contribution_definition`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `bounty_contribution_definition` (
  `ContributionId` int(11) NOT NULL,
  `ContributionDescription` text NOT NULL,
  `ContributionValue` tinyint(3) unsigned NOT NULL,
  `MaxContributionCount` tinyint(3) unsigned NOT NULL,
  PRIMARY KEY (`ContributionId`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `bounty_contribution_definition`
--

LOCK TABLES `bounty_contribution_definition` WRITE;
/*!40000 ALTER TABLE `bounty_contribution_definition` DISABLE KEYS */;
INSERT INTO `bounty_contribution_definition` VALUES (0,' BO Big Tick',1,4),
(1,' BO Small Tick',1,12),
(2,' BO Unlock Tick',2,4),
(3,' Player Kill DB',4,15),
(4,' Player Kill on BO',2,20),
(5,' Kill Keep Lord',8,2),
(6,' Destroy Outer Door',2,4),
(7,' Destroy Inner Door',3,4),
(8,' Destroy Siege',1,8),
(9,' Player Kill Assist',1,30),
(10,' Play Scenario',4,6),
(11,' Win Scenario',8,6),
(12,' Keep Defence Tick',5,2),
(13,' Player Kill under AAO',2,20),
(14,' Player Kill Assist under AAO',1,20),
(15,' Warband Leader BO Big Tick',3,3),
(16,' Warband Leader Kill Keep Lord',5,2),
(17,' Resurrect Player',2,10),
(18,' Out of group healing',1,15),
(19,' Party Kill Assist',1,20),
(20,' Realm Captain Kill',8,3),
(21,' Player Kill Assist on BO',1,12),
(22,' Punt Enemy',1,5),
(23,' Hold the Line',1,5),
(24,' Tank Guard',1,10),
(25,' General Healing',1,30),
(26,' Knockdown',1,5),
(27,' AOE Root',1,5),
(28,' Morale Root (ChCh)',1,5);
/*!40000 ALTER TABLE `bounty_contribution_definition` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2020-01-07 15:41:22
