/*Table structure for table `gm_commands` */

DROP TABLE IF EXISTS `gm_commands`;

CREATE TABLE `gm_commands` (
  `name` varchar(50) NOT NULL DEFAULT '',
  `security` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `help` longtext,
  PRIMARY KEY (`name`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=FIXED COMMENT='Chat System';

/*Data for the table `gm_commands` */

LOCK TABLES `gm_commands` WRITE;

insert  into `gm_commands`(`name`,`security`,`help`) values 

('ADD XP TO PLAYER',40,'Syntax: .add xp <playername> <xp value>'),
('ADD ITEM TO',40,'Syntax: .add <itemID> count'),
('ADD MONEY TO',40,'Syntax: .add money <playername> value'),
('ADD TOK TO',40,'Syntax: .add tok <playername> <tokentry>'),
('ADD RENOWN TO',40,'Syntax: .add renown <playername> value'),
('ADD Infl TO',40,'Syntax: .add infl <playername> <chaptername value> <influence value>'),
('INVINCIBLE',40,'Syntax: .invincible <playername>'),
('FLY MODE',40,'Syntax: .fly 1 or 0 <playername>'),
('DEBUG MODE',40,'Syntax: .debug mode'),
('PLAY SOUND',40,'Syntax: .play sound <soundID>'),
('PLAY EFFECT',40,'Syntax: .play effect <playername> <effectID> '),
('PLAY ABILITY',40,'Syntax: .play ability <playername> <abilityID> <effectID>'),
('REMOVE EFFECT',40,'Syntax: .remove effect <playername> <effectID>'),
('PREVENT CASTING',40,'Syntax: .prevent casting <playername> 1 or 0 '),
('SHROUD TOGGLED',40,'Syntax: .shroud toggled on <playername>'),
('GET STATS',40,'Syntax: .get stats <playername>'),
('ANNOUNCE',40,'Syntax: .announce <message>'),
('MESSAGE ADVICE',40,'Syntax: .message advice <message>'),
('CSR MESSAGE',40,'Syntax: .csr message <message>'),
('CLEAR SERVER',40,'Syntax: .clear server'),
('INFO',40,'Syntax: .info <playername> '),
('CREATURE INFO',40,'Syntax: .creature info <creature target>'),
('GPS',40,'Syntax: .gps'),
('UNLOCK',40,'Syntax: .unlock <playername>'),
('KILL',40,'Syntax: .kill <playername>'),
('WOUND',40,'Syntax: .wound <playername> '),
('SET NPC MODEL',40,'Syntax: .setnpcmodel <creatureID> <modelID>'),
('XP MODE',40,'Syntax: .xpmode <playername> 0 or 1'),
('GET GUILD LEAD',40,'Syntax: .getguildlead <guildname>'),
('GET GUILD ID',40,'Syntax: .getguildid <guildname>'),
('FORCE ALLIANCE QUIT',40,'Syntax: .forcealliancequit <guildID>'),
('GET CHARS LOTS',40,'Syntax: .getcharslots <accountName>'),
('DELETE CHAR IN SLOT',40,'Syntax: .deletecharat <accountName> <slotid>'),
('REQUEST NAME CHANGE',40,'Syntax: .requestnamechange <playername> <new_player_name>'),
('BLOACK NAME',40,'Syntax: .blockname <name> <E(quals)|S(tartsWith)|C(ontains)>'),
('REMOVE QUESTS',40,'Syntax: .removequests <playername>'),
('HIDE',40,'Syntax: .hide <playername>'),
('MORPH',40,'Syntax: .morph <playername>'),
('SPOOKY',40,'Syntax: .spooky <playername>'),
('NOT SPOOKY',40,'Syntax: .notspooky <playername>');

UNLOCK TABLES;