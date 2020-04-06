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
-- Table structure for table `item_slots`
--

DROP TABLE IF EXISTS `item_slots`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `item_slots` (
  `Entry` int(11) NOT NULL,
  `SlotName` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`Entry`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `item_slots`
--

LOCK TABLES `item_slots` WRITE;
/*!40000 ALTER TABLE `item_slots` DISABLE KEYS */;
INSERT INTO `item_slots` VALUES (9,'Live Event'),
(10,'Main Hand'),
(11,'Off Hand'),
(12,'Ranged Slot'),
(13,'Main Hand or Off Hand/Both Hands or Off Hand'),
(14,'Banner'),
(15,'Trophy Top'),
(16,'Trophy Mid Top'),
(17,'Trophy Mid Mid'),
(18,'Trophy Mid Bottom'),
(19,'Trophy Bottom'),
(20,'Body'),
(21,'Gloves'),
(22,'Boots'),
(23,'Helm'),
(24,'Shoulders'),
(25,'Left Pocket'),
(26,'Right Pocket'),
(27,'Back'),
(28,'Belt'),
(31,'Jewelry Top'),
(32,'Jewelry Mid Top'),
(33,'Jewelry Mid Bottom'),
(34,'Jewelry Bottom');
/*!40000 ALTER TABLE `item_slots` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2020-01-07 15:41:59
