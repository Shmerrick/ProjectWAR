DROP TABLE IF EXISTS `ability_line_to_buff_type`;

CREATE TABLE `ability_line_to_buff_type` (
  `ID` int(11) DEFAULT NULL,
  `TypeName` text,
  `ClientSideEnumerationValue` int(11) DEFAULT NULL,
  `BuffFrameRed` int(11) DEFAULT NULL,
  `BuffFrameGreen` int(11) DEFAULT NULL,
  `BuffFrameBlue` int(11) DEFAULT NULL,
  `ability_line_to_buff_type_ID` varchar(255) NOT NULL,
  PRIMARY KEY (`ability_line_to_buff_type_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/*Data for the table `ability_line_to_buff_type` */

LOCK TABLES `ability_line_to_buff_type` WRITE;

insert  into `ability_line_to_buff_type`(`ID`,`TypeName`,`ClientSideEnumerationValue`,`BuffFrameRed`,`BuffFrameGreen`,`BuffFrameBlue`,`ability_line_to_buff_type_ID`) values 
(1001,'Hex',1,184,0,9,'1'),
(1002,'Curse',2,130,0,197,'2'),
(1003,'Cripple',3,0,167,12,'3'),
(1004,'Ail',4,104,186,255,'4'),
(1005,'Bolster',5,104,186,255,'5'),
(1006,'Augment',6,104,186,255,'6'),
(1007,'Bless',7,255,220,80,'7'),
(1008,'Enchant',8,104,186,255,'8');

UNLOCK TABLES;

DROP TABLE IF EXISTS `rvr_player_contribution`;

CREATE TABLE `rvr_player_contribution` (
  `Id` int(11) NOT NULL,
  `CharacterId` int(10) unsigned NOT NULL,
  `BattleFrontId` int(11) NOT NULL,
  `ContributionSerialised` text NOT NULL,
  `Timestamp` datetime NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

/*Data for the table `rvr_player_contribution` */

LOCK TABLES `rvr_player_contribution` WRITE;

UNLOCK TABLES;