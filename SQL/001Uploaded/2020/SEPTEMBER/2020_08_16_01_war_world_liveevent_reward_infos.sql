/*
Navicat MySQL Data Transfer

Source Server         : warserver
Source Server Version : 80021
Source Host           : localhost:3306
Source Database       : war_world

Target Server Type    : MYSQL
Target Server Version : 80021
File Encoding         : 65001

Date: 2020-08-16 15:29:38
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for `liveevent_reward_infos`
-- ----------------------------
DROP TABLE IF EXISTS `liveevent_reward_infos`;
CREATE TABLE `liveevent_reward_infos` (
  `Entry` int unsigned NOT NULL,
  `LiveEventId` int unsigned NOT NULL,
  `RewardGroupId` int unsigned NOT NULL,
  `ItemId` int unsigned NOT NULL,
  PRIMARY KEY (`Entry`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of liveevent_reward_infos
-- ----------------------------
INSERT INTO `liveevent_reward_infos` VALUES ('1', '4', '1', '7190');
INSERT INTO `liveevent_reward_infos` VALUES ('2', '4', '2', '129838731');
INSERT INTO `liveevent_reward_infos` VALUES ('3', '4', '4', '206655');
