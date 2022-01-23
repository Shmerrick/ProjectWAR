USE `war_world`;

/*Table structure for table `characterinfo` */

DROP TABLE IF EXISTS `characterinfo`;

CREATE TABLE `characterinfo` (
  `CareerLine` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `Career` tinyint(3) unsigned NOT NULL,
  `CareerName` varchar(255) NOT NULL,
  `Realm` tinyint(3) unsigned NOT NULL,
  `Region` smallint(5) unsigned NOT NULL,
  `ZoneId` smallint(5) unsigned NOT NULL,
  `WorldX` int(11) NOT NULL,
  `WorldY` int(11) NOT NULL,
  `WorldZ` int(11) NOT NULL,
  `WorldO` int(11) NOT NULL,
  `RallyPt` smallint(5) unsigned NOT NULL,
  `Skills` int(10) unsigned NOT NULL,
  PRIMARY KEY (`CareerLine`),
  UNIQUE KEY `CareerLine` (`CareerLine`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

/*Data for the table `characterinfo` */

LOCK TABLES `characterinfo` WRITE;

insert  into `characterinfo`(`CareerLine`,`Career`,`CareerName`,`Realm`,`Region`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`RallyPt`,`Skills`) values 
(1,20,'Iron Breaker',1,8,6,760750,885123,8629,482,134,5252156),
(2,21,'Slayer',1,8,6,760750,885123,8629,482,134,6815756),
(3,22,'Rune Priest',1,8,6,760750,885123,8629,482,134,2112),
(4,23,'Engineer',1,8,6,760750,885123,8629,482,134,295432),
(5,24,'Black Orc',2,8,11,44301,50742,6909,790,133,5243966),
(6,25,'Choppa',2,8,11,44301,50742,6909,790,133,6815750),
(7,26,'Shaman',2,8,11,44301,50742,6909,790,133,2112),
(8,27,'Squig Herder',2,8,11,44301,50742,6909,790,133,278656),
(9,60,'Witch Hunter',1,8,106,834641,936923,7053,2440,135,2392066),
(10,61,'Knight of the Blazing Sun',1,8,106,834641,936923,7053,2440,135,5243954),
(11,62,'Bright Wizard',1,8,106,834641,936923,7053,2440,135,2112),
(12,63,'Warrior Priest',1,8,106,834641,936923,7053,2440,135,29360136),
(13,64,'Chosen',2,8,100,847879,829970,8006,3254,136,5242934),
(14,65,'Marauder',2,8,100,847879,829970,8006,3254,136,524300),
(15,66,'Zealot',2,8,100,847879,829970,8006,3254,136,16781376),
(16,67,'Magus',2,8,100,847879,829970,8006,3254,136,2112),
(17,100,'Sword Master',1,8,200,1056041,1018393,8328,438,292,5243954),
(18,101,'Shadow Warrior',1,8,200,1056041,1018393,8328,438,292,262274),
(19,102,'White Lion',1,8,200,1056041,1018393,8328,438,292,6815748),
(20,103,'Archmage',1,8,200,1056041,1018393,8328,438,292,2112),
(21,104,'Black Guard',2,8,200,1030178,1021802,4701,3324,138,5260338),
(22,105,'Witch Elf',2,8,200,1030178,1021802,4701,3324,138,2363392),
(23,106,'Disciple Of Khaine',2,8,200,1030178,1021802,4701,3324,138,27262978),
(24,107,'Sorceress',2,8,200,1030178,1021802,4701,3324,138,2112);

UNLOCK TABLES;