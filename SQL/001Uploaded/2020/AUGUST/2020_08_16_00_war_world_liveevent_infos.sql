/*
Navicat MySQL Data Transfer

Source Server         : warserver
Source Server Version : 80021
Source Host           : localhost:3306
Source Database       : war_world

Target Server Type    : MYSQL
Target Server Version : 80021
File Encoding         : 65001

Date: 2020-08-16 15:29:22
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for `liveevent_infos`
-- ----------------------------
DROP TABLE IF EXISTS `liveevent_infos`;
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

-- ----------------------------
-- Records of liveevent_infos
-- ----------------------------
INSERT INTO `liveevent_infos` VALUES ('1', 'Wild Hunt', 'The Age of Reckoning\'s Most Dangerous Game', 'The Wild Hunt is upon the Elves, and Ulthuan is already sodden with blood. Hunters stalk the fields and forests in search of rare and savage prey, to demonstrate their skill to the God of the Hunt and perhaps gain his favor. Others seek merely to glimpse the fabled White Stag and gain Kurnous\'s blessing. But Kurnous is a true hunter, and incurring his wrath carries a price ... of poetic justice.', 'Players who please Kurnous may receive a Bleeding Hart charm of the Fleet Stag Mantle. In Elf RVR lands, players can vie to capture Vale Vines and unlock an exclusive new zone: the Hunter\'s Vale, where the God of the Hunt tests their mettle. The truly lucky may sight the White Stag as it wanders from zone to zone--and the truly unlucky may encounter the vicious Hounds of Kurnous, guardians of the sacred oaks.', '9', '2019-03-16 00:00:00', '2019-04-16 00:00:00', '0');
INSERT INTO `liveevent_infos` VALUES ('2', 'The Witching Night', 'Witching', 'The Witching Night Approaches in the Warhammer World. During this time, the divisions between the living and the dead grow thin, and the power of Shyish, the Purple Wind of Magic, waxes. Evil cults, witches, and necromancers use this time to their advantage, easily raising the dead and calling upon them to spread wickedness across the land.', 'The Witching Night is a Live Event and avaible only for a short time. Kill Restless Spirits, Withered Crones, and participate in the Conflict Public Quests in the RvR areas to unlock influance rewards and to aquire rare Witching Night masks!', '2', '2019-03-16 00:00:00', '2019-04-16 00:00:00', '0');
INSERT INTO `liveevent_infos` VALUES ('3', 'Heavy Metal', 'Metal', 'Every day of this event, from November 17th through December 1st, you can complete a single task to earn influence. That influence will contribute to the Event bar below (like a Chapter), and you can earn great rewards. Visit the Herald in Altdorf or the Inevitable City to receive your rewards.', 'The front lines of Order\'s righteous struggle against the malign forces of Destruction will soon be bolstered by the Knights of the Blazing Sun! The Knights, called from all corners of the Empire to perform their duty, Will gather together in numbers never seen before! As elite warriors, expert tacticians, and brave templars of the goddess Myrmidia, the Knight of the Blazing Sun are dedicated to claiming complete victory over Chaos and its allies! Do your part to ensure their succes! The Empire\'s survival depends upon your contribution!', '15', '2019-03-16 00:00:00', '2019-04-16 00:00:00', '0');
INSERT INTO `liveevent_infos` VALUES ('4', 'Keg End', '', 'The Dwarf Celebrations of Keg End approaches in the Warhammer World.At this time,all of the year\'s ale must be consumed before the New Year arrives or else a terrible bad luck will befall the citizens of the Old World rise to the occasion.A certain amount of competitive boasting is traditional as well.The forces of destruction are eager as allways to spoil the event by drinking the ale for themselves or by stealing any of the holiday celebrations.', 'Gain Event Influence by completing any of the tasks below. Massive Ogres lurk in the open RvR Battlefields. Brew-Thirsty Ogres, Drunken Gnoblars, and Explosive Snotlings have stolen caches of beer and crates of fireworks. Scouting reports suggest their locations to be in PvE areas along the roads and near major landmarks or Public Quests. Enemy players are also a great scource for aquiring any of these items.                                                                                                                                                                Amazingly - rumors of the legendary Golden stein appearing have surfaced. Scour the RvR Battlefields of Nordland, Barak Varr, Black Fire Pass, Praag, Thunder Mountain, and Dragon Wake to obtain it. All those who bask in its foamy radiance will be rewarded. And for the exceptionally lucky participating in any of these tasks may yield the elusive Keg Backpack.', '1', '2019-03-16 00:00:00', '2019-04-16 00:00:00', '1');
