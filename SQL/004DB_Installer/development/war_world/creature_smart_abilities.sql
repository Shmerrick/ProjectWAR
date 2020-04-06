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
-- Table structure for table `creature_smart_abilities`
--

DROP TABLE IF EXISTS `creature_smart_abilities`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `creature_smart_abilities` (
  `UniqueId` int(11) NOT NULL AUTO_INCREMENT,
  `CreatureTypeId` int(11) NOT NULL,
  `CreatureSubTypeId` int(11) NOT NULL,
  `CreatureTypeDescription` text NOT NULL,
  `Name` text NOT NULL,
  `Speech` text,
  `Condition` text NOT NULL,
  `Execution` text NOT NULL,
  `ExecuteChance` int(11) NOT NULL,
  `CoolDown` int(11) NOT NULL,
  `Sound` text,
  PRIMARY KEY (`UniqueId`,`CreatureTypeId`,`CreatureSubTypeId`)
) ENGINE=InnoDB AUTO_INCREMENT=82 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `creature_smart_abilities`
--

LOCK TABLES `creature_smart_abilities` WRITE;
/*!40000 ALTER TABLE `creature_smart_abilities` DISABLE KEYS */;
INSERT INTO `creature_smart_abilities` VALUES (1,1,1,'ANIMALS_BEASTS_BASILISK','CorrosiveVomit','CorrosiveVomit','PlayerInMeleeRange','CorrosiveVomit',35,60,NULL),
(2,1,1,'ANIMALS_BEASTS_BASILISK','InfectiousBite','InfectiousBite','PlayerInMeleeRange','InfectiousBite',75,30,NULL),
(3,1,2,'ANIMALS_BEASTS_BEAR','LowBlow','LowBlow','PlayerInMeleeRange','LowBlow',35,60,NULL),
(4,1,2,'ANIMALS_BEASTS_BEAR','Shred','Shred','PlayerInMeleeRange','Shred',65,25,NULL),
(5,1,3,'ANIMALS_BEASTS_BOAR','LegTear','LegTear','PlayerInMeleeRange','LegTear',70,60,NULL),
(6,1,3,'ANIMALS_BEASTS_BOAR','Bite','Bite','PlayerInMeleeRange','Bite',75,20,NULL),
(7,1,4,'ANIMALS_BEASTS_GIANT_BAT','Bite','Bite','PlayerInMeleeRange','Bite',75,20,NULL),
(8,1,4,'ANIMALS_BEASTS_GIANT_BAT','GutRipper','GutRipper','PlayerInMeleeRange','GutRipper',60,30,NULL),
(9,1,5,'ANIMALS_BEASTS_GREAT_CAT','LegTear','LegTear','PlayerInMeleeRange','LegTear',70,60,NULL),
(10,1,5,'ANIMALS_BEASTS_GREAT_CAT','Shred','Shred','PlayerInMeleeRange','Shred',65,25,NULL),
(11,1,6,'ANIMALS_BEASTS_HOUND','LegTear','LegTear','PlayerInMeleeRange','LegTear',70,60,NULL),
(12,1,6,'ANIMALS_BEASTS_HOUND','Maul','Maul','PlayerInMeleeRange','Maul',100,25,NULL),
(13,1,7,'ANIMALS_BEASTS_RHINOX','Charge','Charge','PlayersWithinRange','Charge',55,30,NULL),
(14,1,7,'ANIMALS_BEASTS_RHINOX','CripplingBlow','CripplingBlow','PlayerInMeleeRange','CripplingBlow',70,60,NULL),
(15,1,8,'ANIMALS_BEASTS_WOLF','LegTear','LegTear','PlayerInMeleeRange','LegTear',70,60,NULL),
(16,1,8,'ANIMALS_BEASTS_WOLF','Bite','Bite','PlayerInMeleeRange','Bite',100,20,NULL),
(17,3,19,'ANIMALS_INSECTS_ARACHNIDS_GIANT_SCORPION','InfectiousBite','InfectiousBite','PlayerInMeleeRange','InfectiousBite',75,60,NULL),
(18,3,19,'ANIMALS_INSECTS_ARACHNIDS_GIANT_SCORPION','EnvenomedStinger','EnvenomedStinger','PlayerInMeleeRange','EnvenomedStinger',35,60,NULL),
(19,3,20,'ANIMALS_INSECTS_ARACHNIDS_GIANT_SPIDER','WhitefireWebBolt','WhitefireWebBolt','PlayersWithinRange','WhitefireWebBolt',70,50,NULL),
(20,3,20,'ANIMALS_INSECTS_ARACHNIDS_GIANT_SPIDER','EnvenomedStinger','EnvenomedStinger','PlayerInMeleeRange','EnvenomedStinger',35,60,NULL),
(21,3,21,'ANIMALS_INSECTS_ARACHNIDS_TOMB_SWARM','InfectiousBite','InfectiousBite','PlayerInMeleeRange','InfectiousBite',75,60,NULL),
(22,3,21,'ANIMALS_INSECTS_ARACHNIDS_TOMB_SWARM','CripplingThorns','CripplingThorns','PlayerInMeleeRange','CripplingThorns',35,30,NULL),
(23,5,29,'ANIMALS_REPTILES_COLD_ONE','Bite','Bite','PlayerInMeleeRange','Bite',75,20,NULL),
(24,5,29,'ANIMALS_REPTILES_COLD_ONE','CripplingBlow','CripplingBlow','PlayerInMeleeRange','CripplingBlow',45,60,NULL),
(25,5,30,'ANIMALS_REPTILES_GIANT_LIZARD','CorrosiveVomit','CorrosiveVomit','PlayerInMeleeRange','CorrosiveVomit',35,60,NULL),
(26,5,30,'ANIMALS_REPTILES_GIANT_LIZARD','InfectiousBite','InfectiousBite','PlayerInMeleeRange','InfectiousBite',75,30,NULL),
(27,6,31,'DAEMONS_KHORNE_BLOODBEAST','BloodStomp','BloodStomp','PlayerInMeleeRange','BloodStomp',35,60,NULL),
(28,6,31,'DAEMONS_KHORNE_BLOODBEAST','BoneyStrike','BoneyStrike','PlayerInMeleeRange','BoneyStrike',75,30,NULL),
(29,6,32,'DAEMONS_KHORNE_BLOODLETTER','Wotarmor','Wotarmor','PlayerInMeleeRange','Wotarmor',35,60,NULL),
(30,6,32,'DAEMONS_KHORNE_BLOODLETTER','Slice','Slice','PlayerInMeleeRange','Slice',65,30,NULL),
(31,6,32,'DAEMONS_KHORNE_BLOODLETTER','CripplingBlow','CripplingBlow','PlayerInMeleeRange','CripplingBlow',55,45,NULL),
(32,6,34,'DAEMONS_KHORNE_FLESH_HOUND','Bite','Bite','PlayerInMeleeRange','Bite',75,60,NULL),
(33,6,34,'DAEMONS_KHORNE_FLESH_HOUND','FeralBite','FeralBite','PlayerInMeleeRange','FeralBite',15,60,NULL),
(34,6,34,'DAEMONS_KHORNE_FLESH_HOUND','InfectiousBite','InfectiousBite','PlayerInMeleeRange','InfectiousBite',55,60,NULL),
(35,7,36,'DAEMONS_NURGLE_GREAT_UNCLEAN_ONE','SlimyVomit','SlimyVomit','PlayersWithinRange','SlimyVomit',45,60,NULL),
(36,7,37,'DAEMONS_NURGLE_NURGLING','PoisonSpit','PoisonSpit','PlayerInMeleeRange','PoisonSpit',35,60,NULL),
(37,7,37,'DAEMONS_NURGLE_NURGLING','InfectiousBite','InfectiousBite','PlayerInMeleeRange','InfectiousBite',75,30,NULL),
(38,7,38,'DAEMONS_NURGLE_PLAGUEBEARER','CorrosiveVomit','CorrosiveVomit','PlayerInMeleeRange','CorrosiveVomit',35,60,NULL),
(39,7,38,'DAEMONS_NURGLE_PLAGUEBEARER','PoisonSpit','PoisonSpit','PlayerInMeleeRange','PoisonSpit',50,45,NULL),
(41,7,39,'DAEMONS_NURGLE_PLAGUEBEAST','CripplingBlow','CripplingBlow','PlayerInMeleeRange','CripplingBlow',35,60,NULL),
(42,7,39,'DAEMONS_NURGLE_PLAGUEBEAST','LowBlow','LowBlow','PlayerInMeleeRange','LowBlow',55,60,NULL),
(43,7,39,'DAEMONS_NURGLE_PLAGUEBEAST','GroundRumble','GroundRumble','PlayerInMeleeRange','GroundRumble',15,80,NULL),
(44,7,40,'DAEMONS_NURGLE_SLIME_HOUND','CripplingBlow','CripplingBlow','PlayerInMeleeRange','CripplingBlow',55,45,NULL),
(45,7,40,'DAEMONS_NURGLE_SLIME_HOUND','CorrosiveVomit','CorrosiveVomit','PlayerInMeleeRange','CorrosiveVomit',35,60,NULL),
(46,7,40,'DAEMONS_NURGLE_SLIME_HOUND','InfectiousBite','InfectiousBite','PlayerInMeleeRange','InfectiousBite',55,60,NULL),
(47,7,40,'DAEMONS_NURGLE_SLIME_HOUND','Bite','Bite','PlayerInMeleeRange','Bite',75,60,NULL),
(48,8,41,'DAEMONS_SLAANESH_DAEMONETTE','LegTear','LegTear','PlayerInMeleeRange','LegTear',55,60,NULL),
(49,8,41,'DAEMONS_SLAANESH_DAEMONETTE','Shred','Shred','PlayerInMeleeRange','Shred',45,45,NULL),
(50,8,41,'DAEMONS_SLAANESH_DAEMONETTE','GutRipper','GutRipper','PlayerInMeleeRange','GutRipper',65,55,NULL),
(51,9,45,'DAEMONS_TZEENTCH_FLAMER','FlamesofChange','FlamesofChange','PlayersWithinRange','FlamesofChange',65,55,NULL),
(52,9,45,'DAEMONS_TZEENTCH_FLAMER','FlameofTzeentch','FlameofTzeentch','PlayersWithinRange','FlameofTzeentch',45,50,NULL),
(53,9,45,'DAEMONS_TZEENTCH_FLAMER','DaemonicFire','DaemonicFire','PlayersWithinRange','DaemonicFire',55,60,NULL),
(54,9,46,'DAEMONS_TZEENTCH_HORROR','DaemonicConsumption','DaemonicConsumption','PlayerInMeleeRange','DaemonicConsumption',45,60,NULL),
(55,9,46,'DAEMONS_TZEENTCH_HORROR','WarpingEnergy','WarpingEnergy','PlayerInMeleeRange','WarpingEnergy',55,55,NULL),
(56,9,46,'DAEMONS_TZEENTCH_HORROR','DaemonicFire','DaemonicFire','PlayerInMeleeRange','DaemonicFire',60,60,NULL),
(57,9,48,'DAEMONS_TZEENTCH_SCREAMER','FlamesofChange','FlamesofChange','PlayerInMeleeRange','FlamesofChange',45,60,NULL),
(58,9,48,'DAEMONS_TZEENTCH_SCREAMER','DaemonicFire','DaemonicFire','PlayerInMeleeRange','DaemonicFire',65,45,NULL),
(59,10,50,'DAEMONS_UNMARKED_DAEMONS_CHAOS_FURY','GutRipper','GutRipper','PlayerInMeleeRange','GutRipper',60,30,NULL),
(60,10,50,'DAEMONS_UNMARKED_DAEMONS_CHAOS_FURY','FeralBite','FeralBite','PlayerInMeleeRange','FeralBite',45,45,NULL),
(61,10,51,'DAEMONS_UNMARKED_DAEMONS_CHAOS_HOUND','Bite','Bite','PlayerInMeleeRange','Bite',75,20,NULL),
(62,10,51,'DAEMONS_UNMARKED_DAEMONS_CHAOS_HOUND','FeralBite','FeralBite','PlayerInMeleeRange','FeralBite',15,60,NULL),
(63,10,51,'DAEMONS_UNMARKED_DAEMONS_CHAOS_HOUND','InfectiousBite','InfectiousBite','PlayerInMeleeRange','InfectiousBite',55,60,NULL),
(64,10,52,'DAEMONS_UNMARKED_DAEMONS_CHAOS_SPAWN','LegTear','LegTear','PlayerInMeleeRange','LegTear',55,60,NULL),
(65,10,52,'DAEMONS_UNMARKED_DAEMONS_CHAOS_SPAWN','BloodStomp','BloodStomp','PlayerInMeleeRange','BloodStomp',45,60,NULL),
(66,10,54,'DAEMONS_UNMARKED_DAEMONS_DAEMONVINE','CorrosiveVomit','CorrosiveVomit','PlayerInMeleeRange','CorrosiveVomit',35,60,NULL),
(67,10,54,'DAEMONS_UNMARKED_DAEMONS_DAEMONVINE','InfectiousBite','InfectiousBite','PlayerInMeleeRange','InfectiousBite',75,30,NULL),
(68,10,55,'DAEMONS_UNMARKED_DAEMONS_WALKER','CorrosiveVomit','CorrosiveVomit','PlayerInMeleeRange','CorrosiveVomit',35,60,NULL),
(69,10,55,'DAEMONS_UNMARKED_DAEMONS_WALKER','InfectiousBite','InfectiousBite','PlayerInMeleeRange','InfectiousBite',75,30,NULL),
(70,11,56,'HUMANOIDS_BEASTMEN_BESTIGOR','BloodyClaw','BloodyClaw','PlayerInMeleeRange','BloodyClaw',60,60,NULL),
(71,11,56,'HUMANOIDS_BEASTMEN_BESTIGOR','ViciousSlash','ViciousSlash','PlayerInMeleeRange','ViciousSlash',45,45,NULL),
(72,11,57,'HUMANOIDS_BEASTMEN_BRAY_SHAMAN','CripplingBlow','CripplingBlow','PlayerInMeleeRange','CripplingBlow',55,45,NULL),
(73,11,57,'HUMANOIDS_BEASTMEN_BRAY_SHAMAN','BloodStomp','BloodStomp','PlayerInMeleeRange','BloodStomp',55,30,NULL),
(74,11,59,'HUMANOIDS_BEASTMEN_GOR','ViciousSlash','ViciousSlash','PlayerInMeleeRange','ViciousSlash',45,45,NULL),
(75,11,59,'HUMANOIDS_BEASTMEN_GOR','BloodyClaw','BloodyClaw','PlayerInMeleeRange','BloodyClaw',60,60,NULL),
(76,11,60,'HUMANOIDS_BEASTMEN_UNGOR','ViciousSlash','ViciousSlash','PlayerInMeleeRange','ViciousSlash',45,45,NULL),
(77,11,60,'HUMANOIDS_BEASTMEN_UNGOR','CripplingBlow','CripplingBlow','PlayerInMeleeRange','CripplingBlow',55,45,NULL),
(78,12,61,'HUMANOIDS_DARK_ELVES_BLACK_GUARD','MurderousWrath','MurderousWrath','PlayerInMeleeRange','MurderousWrath',70,45,NULL),
(79,12,61,'HUMANOIDS_DARK_ELVES_BLACK_GUARD','HatefulStrike','HatefulStrike','PlayerInMeleeRange','HatefulStrike',55,45,NULL),
(80,12,61,'HUMANOIDS_DARK_ELVES_BLACK_GUARD','SpitefulSlam','SpitefulSlam','PlayerInMeleeRange','SpitefulSlam',15,60,NULL),
(81,12,61,'HUMANOIDS_DARK_ELVES_BLACK_GUARD','AwayCretins','AwayCretins','PlayerInMeleeRange','AwayCretins',30,55,NULL);
/*!40000 ALTER TABLE `creature_smart_abilities` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2020-01-07 15:41:34
