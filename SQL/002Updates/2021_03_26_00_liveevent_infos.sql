-- MySQL dump 10.13  Distrib 8.0.22, for Win64 (x86_64)
--
-- Host: localhost    Database: war_world
-- ------------------------------------------------------
-- Server version	8.0.22

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `liveevent_infos`
--

DROP TABLE IF EXISTS `liveevent_infos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `liveevent_infos` (
  `Entry` int unsigned NOT NULL,
  `Title` text NOT NULL,
  `SubTitle` text NOT NULL,
  `Description` text NOT NULL,
  `TasksDescription` text NOT NULL,
  `ImageId` int unsigned NOT NULL,
  `StartDate` datetime DEFAULT NULL,
  `EndDate` datetime DEFAULT NULL,
  `Allowed` tinyint unsigned DEFAULT NULL,
  PRIMARY KEY (`Entry`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `liveevent_infos`
--

LOCK TABLES `liveevent_infos` WRITE;
/*!40000 ALTER TABLE `liveevent_infos` DISABLE KEYS */;
INSERT INTO `liveevent_infos` VALUES (1,'Wild Hunt','The Age of Reckoning\'s Most Dangerous Game','The Wild Hunt is upon the Elves, and Ulthuan is already sodden with blood. Hunters stalk the fields and forests in search of rare and savage prey, to demonstrate their skill to the God of the Hunt and perhaps gain his favor. Others seek merely to glimpse the fabled White Stag and gain Kurnous\'s blessing. But Kurnous is a true hunter, and incurring his wrath carries a price ... of poetic justice.','Players who please Kurnous may receive a Bleeding Hart charm of the Fleet Stag Mantle. In Elf RVR lands, players can vie to capture Vale Vines and unlock an exclusive new zone: the Hunter\'s Vale, where the God of the Hunt tests their mettle. The truly lucky may sight the White Stag as it wanders from zone to zone--and the truly unlucky may encounter the vicious Hounds of Kurnous, guardians of the sacred oaks.',9,'2019-03-16 00:00:00','2019-04-16 00:00:00',0),(2,'The Witching Night','Witching','The Witching Night Approaches in the Warhammer World. During this time, the divisions between the living and the dead grow thin, and the power of Shyish, the Purple Wind of Magic, waxes. Evil cults, witches, and necromancers use this time to their advantage, easily raising the dead and calling upon them to spread wickedness across the land.','The Witching Night is a Live Event and avaible only for a short time. Kill Restless Spirits, Withered Crones, and participate in the Conflict Public Quests in the RvR areas to unlock influance rewards and to aquire rare Witching Night masks!',2,'2019-03-16 00:00:00','2019-04-16 00:00:00',0),(3,'Heavy Metal','Metal','Every day of this event, from November 17th through December 1st, you can complete a single task to earn influence. That influence will contribute to the Event bar below (like a Chapter), and you can earn great rewards. Visit the Herald in Altdorf or the Inevitable City to receive your rewards.','The front lines of Order\'s righteous struggle against the malign forces of Destruction will soon be bolstered by the Knights of the Blazing Sun! The Knights, called from all corners of the Empire to perform their duty, Will gather together in numbers never seen before! As elite warriors, expert tacticians, and brave templars of the goddess Myrmidia, the Knight of the Blazing Sun are dedicated to claiming complete victory over Chaos and its allies! Do your part to ensure their succes! The Empire\'s survival depends upon your contribution!',15,'2019-03-16 00:00:00','2019-04-16 00:00:00',0),(4,'Keg End','','The Dwarf Celebrations of Keg End approaches in the Warhammer World.At this time,all of the year\'s ale must be consumed before the New Year arrives or else a terrible bad luck will befall the citizens of the Old World rise to the occasion.A certain amount of competitive boasting is traditional as well.The forces of destruction are eager as allways to spoil the event by drinking the ale for themselves or by stealing any of the holiday celebrations.','Gain Event Influence by completing any of the tasks below. Massive Ogres lurk in the open RvR Battlefields. Brew-Thirsty Ogres, Drunken Gnoblars, and Explosive Snotlings have stolen caches of beer and crates of fireworks. Scouting reports suggest their locations to be in PvE areas along the roads and near major landmarks or Public Quests. Enemy players are also a great scource for aquiring any of these items.                                                                                                                                                                Amazingly - rumors of the legendary Golden stein appearing have surfaced. Scour the RvR Battlefields of Nordland, Barak Varr, Black Fire Pass, Praag, Thunder Mountain, and Dragon Wake to obtain it. All those who bask in its foamy radiance will be rewarded. And for the exceptionally lucky participating in any of these tasks may yield the elusive Keg Backpack.',1,'2021-03-22 00:00:00','2021-03-31 00:00:00',1);
/*!40000 ALTER TABLE `liveevent_infos` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2021-03-23 12:19:32
