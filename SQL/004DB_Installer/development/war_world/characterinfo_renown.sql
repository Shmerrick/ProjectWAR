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
-- Table structure for table `characterinfo_renown`
--

DROP TABLE IF EXISTS `characterinfo_renown`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `characterinfo_renown` (
  `SpellId` smallint(5) unsigned NOT NULL,
  `Name` text NOT NULL,
  `ID` tinyint(3) unsigned NOT NULL,
  `CommandName` text NOT NULL,
  `Stat` tinyint(3) unsigned NOT NULL,
  `Value` int(11) NOT NULL,
  `Passive` tinyint(3) unsigned NOT NULL,
  `Tree` tinyint(3) unsigned NOT NULL,
  `Position` tinyint(3) unsigned NOT NULL,
  `Renown_Costs` tinyint(3) unsigned NOT NULL,
  `Slotreq` tinyint(3) unsigned NOT NULL,
  `Unk` text,
  PRIMARY KEY (`SpellId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `characterinfo_renown`
--

LOCK TABLES `characterinfo_renown` WRITE;
/*!40000 ALTER TABLE `characterinfo_renown` DISABLE KEYS */;
INSERT INTO `characterinfo_renown` VALUES (9902,'Might I',1,'ModifyStat',1,4,1,9,1,1,0,''),
(9903,'Might II',2,'ModifyStat',1,12,1,9,2,3,1,''),
(9904,'Might III',3,'ModifyStat',1,22,1,9,3,6,2,''),
(9905,'Might IV',4,'ModifyStat',1,34,1,9,4,10,3,''),
(9906,'Might V',5,'ModifyStat',1,48,1,9,5,14,4,''),
(9908,'Blade Master I',6,'ModifyStat',7,4,1,9,6,1,0,''),
(9909,'Blade Master II',7,'ModifyStat',7,12,1,9,7,3,6,''),
(9910,'Blade Master III',8,'ModifyStat',7,22,1,9,8,6,7,''),
(9911,'Blade Master IV',9,'ModifyStat',7,34,1,9,9,10,8,''),
(9912,'Blade Master V',10,'ModifyStat',7,48,1,9,10,14,9,''),
(9914,'Marksmen I',11,'ModifyStat',8,4,1,9,11,1,0,''),
(9915,'Marksmen II',12,'ModifyStat',8,12,1,9,12,3,11,''),
(9916,'Marksmen III',13,'ModifyStat',8,22,1,9,13,6,12,''),
(9917,'Marksmen IV',14,'ModifyStat',8,34,1,9,14,10,13,''),
(9918,'Marksmen V',15,'ModifyStat',8,48,1,9,15,14,14,''),
(9920,'Impetus I',16,'ModifyStat',6,4,1,9,16,1,0,''),
(9921,'Impetus II',17,'ModifyStat',6,12,1,9,17,3,16,''),
(9922,'Impetus III',18,'ModifyStat',6,22,1,9,18,6,17,''),
(9923,'Impetus IV',19,'ModifyStat',6,34,1,9,19,10,18,''),
(9924,'Impetus V',20,'ModifyStat',6,48,1,9,20,14,19,''),
(9926,'Acumen I',21,'ModifyStat',9,4,1,10,1,1,0,''),
(9927,'Acumen II',22,'ModifyStat',9,12,1,10,2,3,1,''),
(9928,'Acumen III',23,'ModifyStat',9,22,1,10,3,6,2,''),
(9929,'Acumen IV',24,'ModifyStat',9,34,1,10,4,10,3,''),
(9930,'Acumen V',25,'ModifyStat',9,48,1,10,5,14,4,''),
(9932,'Resolve I',26,'ModifyStat',3,4,1,10,6,1,0,''),
(9933,'Resolve II',27,'ModifyStat',3,12,1,10,7,3,6,''),
(9934,'Resolve III',28,'ModifyStat',3,22,1,10,8,6,7,''),
(9935,'Resolve IV',29,'ModifyStat',3,34,1,10,9,10,8,''),
(9936,'Resolve V',30,'ModifyStat',3,48,1,10,10,14,9,''),
(9938,'Fortitude I',31,'ModifyStat',4,4,1,10,11,1,0,''),
(9939,'Fortitude II',32,'ModifyStat',4,12,1,10,12,3,11,''),
(9940,'Fortitude III',33,'ModifyStat',4,22,1,10,13,6,12,''),
(9941,'Fortitude IV',34,'ModifyStat',4,34,1,10,14,10,13,''),
(9942,'Fortitude V',35,'ModifyStat',4,48,1,10,15,14,14,''),
(9944,'Vigor I',36,'ModifyStat',5,4,1,10,16,1,0,''),
(9945,'Vigor II',37,'ModifyStat',5,12,1,10,17,3,16,''),
(9946,'Vigor III',38,'ModifyStat',5,22,1,10,18,6,17,''),
(9947,'Vigor IV',39,'ModifyStat',5,34,1,10,19,10,18,''),
(9948,'Vigor V',40,'ModifyStat',5,48,1,10,20,14,19,''),
(9952,'Reflexes I',76,'ModifyStat',29,3,1,14,1,1,0,''),
(9953,'Reflexes II',77,'ModifyStat',29,4,1,14,2,3,1,''),
(9954,'Reflexes III',78,'ModifyStat',29,5,1,14,3,6,2,''),
(9955,'Reflexes IV',79,'ModifyStat',29,6,1,14,4,10,3,''),
(9958,'Defender I',80,'ModifyStat',28,1,1,14,5,1,0,''),
(9959,'Defender II',81,'ModifyStat',28,2,1,14,6,3,5,''),
(9960,'Defender III',82,'ModifyStat',28,3,1,14,7,6,6,''),
(9961,'Defender IV',83,'ModifyStat',28,4,1,14,8,10,7,''),
(9964,'Hardy Concessions I',84,'AddBuff',0,27801,0,14,9,1,0,'02 00 00 6C 99 00 01 14 10 01 00 00 00 00 00 00 00 00 00 00 00 00 00'),
(9965,'Hardy Concessions II',85,'AddBuff',0,27803,0,14,10,3,9,'02 00 00 6C 9B 00 01 14 10 01 00 00 00 00 00 00 00 00 00 00 00 00 00'),
(9966,'Hardy Concessions III',86,'AddBuff',0,27804,0,14,11,6,10,'02 00 00 6C 9C 00 01 14 10 01 00 00 00 00 00 00 00 00 00 00 00 00 00'),
(9967,'Hardy Concessions IV',87,'AddBuff',0,27805,0,14,12,10,11,'02 00 00 6C 9D 00 01 14 10 01 00 00 00 00 00 00 00 00 00 00 00 00 00'),
(9968,'Hardy Concessions V',88,'AddBuff',0,27806,0,14,13,14,12,'02 00 00 6C 9E 00 01 14 10 01 00 00 00 00 00 00 00 00 00 00 00 00 00'),
(9970,'Deft Defender I',89,'ModifyEvasion',30,3,1,14,14,1,0,'02 00 00 6C A0 00 01 14 11 01 00 00 00 00 00 00 00 00 00 00 00 00 00'),
(9971,'Deft Defender II',90,'ModifyEvasion',30,4,1,14,15,3,14,'02 00 00 6C A1 00 01 14 11 01 00 00 00 00 00 00 00 00 00 00 00 00 00'),
(9972,'Deft Defender III',91,'ModifyEvasion',30,5,1,14,16,6,15,'02 00 00 6C A2 00 01 14 11 01 00 00 00 00 00 00 00 00 00 00 00 00 00'),
(9973,'Deft Defender IV',92,'ModifyEvasion',30,6,1,14,17,10,16,'02 00 00 6C A3 00 01 14 11 01 00 00 00 00 00 00 00 00 00 00 00 00 00'),
(9977,'Opportunist I',41,'ModifyStat',76,2,1,11,1,5,0,''),
(9978,'Opportunist II',42,'ModifyStat',76,3,1,11,2,10,1,''),
(9979,'Opportunist III',43,'ModifyStat',76,4,1,11,3,15,2,''),
(9980,'Opportunist IV',44,'ModifyStat',76,5,1,11,4,15,3,''),
(9983,'Sure Shot I',45,'ModifyStat',77,2,1,11,5,5,0,''),
(9984,'Sure Shot II',46,'ModifyStat',77,3,1,11,6,10,5,''),
(9985,'Sure Shot III',47,'ModifyStat',77,4,1,11,7,15,6,''),
(9986,'Sure Shot IV',48,'ModifyStat',77,5,1,11,8,15,7,''),
(9989,'Focused Power I',49,'ModifyStat',78,2,1,11,9,5,0,''),
(9990,'Focused Power II',50,'ModifyStat',78,3,1,11,10,10,9,''),
(9991,'Focused Power III',51,'ModifyStat',78,4,1,11,11,15,10,''),
(9992,'Focused Power IV',52,'ModifyStat',78,5,1,11,12,15,11,''),
(9995,'Spiritual Refinement I',53,'ModifyStat',89,2,1,11,13,5,0,''),
(9996,'Spiritual Refinement II',54,'ModifyStat',89,3,1,11,14,10,13,''),
(9997,'Spiritual Refinement III',55,'ModifyStat',89,4,1,11,15,15,14,''),
(9998,'Spiritual Refinement IV',56,'ModifyStat',89,5,1,11,16,15,15,''),
(10001,'Futile Strikes I',57,'ModifyStat',84,3,1,12,1,5,0,''),
(10002,'Futile Strikes II',58,'ModifyStat',84,5,1,12,2,10,1,''),
(10003,'Futile Strikes III',59,'ModifyStat',84,7,1,12,3,15,2,''),
(10004,'Futile Strikes IV',60,'ModifyStat',84,9,1,12,4,15,3,''),
(10007,'Trivial Blows I',61,'Nope',0,0,1,12,5,5,0,''),
(10008,'Trivial Blows II',62,'No way',0,0,1,12,6,10,5,''),
(10009,'Trivial Blows III',63,'Not in a million years',0,0,1,12,7,15,6,''),
(10016,'Expanded Capacity I',65,'IncreaseAPPool',0,10,1,13,1,10,0,''),
(10017,'Expanded Capacity II',66,'IncreaseAPPool',0,15,1,13,2,15,1,''),
(10018,'Expanded Capacity III',67,'IncreaseAPPool',0,25,1,13,3,20,2,''),
(10022,'Regeneration I',68,'ModifyStat',79,7,1,13,4,10,0,''),
(10023,'Regeneration II',69,'ModifyStat',79,10,1,13,5,15,4,''),
(10024,'Regeneration III',70,'ModifyStat',79,18,1,13,6,20,5,''),
(10028,'Quick Escape I',71,'AddBuff',0,27814,0,13,7,10,0,'02 00 00 6C A6 00 01 14 04 01 00 00 00 00 00 00 00 00 00 00 00 00 00'),
(10029,'Quick Escape II',72,'AddBuff',0,27815,0,13,8,15,7,'02 00 00 6C A7 00 01 14 04 01 00 00 00 00 00 00 00 00 00 00 00 00 00'),
(10030,'Quick Escape III',73,'AddBuff',0,27816,0,13,9,20,8,'0200006CA8000114040100000000000000000000000000'),
(10034,'Improved Flee I',74,'AddBuff',0,27820,0,13,10,10,0,'0200006CAC0001141A0100000000000000000000000000'),
(10035,'Improved Flee II',75,'AddBuff',0,27821,0,13,11,10,10,'0200006CAD0000141A0100000000000000000000000000'),
(10051,'Cleansing Wind I',93,'AddAbility',0,27826,0,15,1,10,0,'02 00 00 6C B2 00 01 14 01 01 00 00 00 00 00 00 00 00 00 00 00 00 00'),
(10052,'Cleansing Wind II',94,'AddBuff',0,27827,0,15,2,10,1,'02 00 00 6C B3 00 01 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00'),
(10053,'Cleansing Wind III',95,'AddBuff',0,27828,0,15,3,10,2,'02 00 00 6C B4 00 01 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00'),
(10057,'Resolute Defenses I',96,'AddAbility',0,27832,0,15,4,10,0,'02 00 00 6C B8 00 01 13 85 01 00 00 00 00 00 00 00 00 00 00 00 00 00'),
(10058,'Resolute Defenses II',97,'AddBuff',0,27833,0,15,5,10,4,'02 00 00 6C B9 00 01 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00'),
(10059,'Resolute Defenses III',98,'AddBuff',0,27834,0,15,6,10,5,'02 00 00 6C BA 00 01 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00'),
(10063,'Efficiency	 I',99,'AddAbility',0,27838,0,15,7,10,0,'02 00 00 6C BE 00 01 14 02 01 00 00 00 00 00 00 00 00 00 00 00 00 00'),
(10064,'Efficiency	 II',100,'AddBuff',0,27839,0,15,8,10,7,'02 00 00 6C BF 00 01 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00'),
(10069,'Last Stand I',101,'AddAbility',0,27844,0,15,9,10,0,'02 00 00 6C C4 00 01 14 03 01 00 00 00 00 00 00 00 00 00 00 00 00 00'),
(10070,'Last Stand II',102,'AddBuff',0,27845,0,15,10,10,9,'02 00 00 6C C5 00 01 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00');
/*!40000 ALTER TABLE `characterinfo_renown` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2020-01-07 15:41:27
