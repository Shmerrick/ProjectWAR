/*
Navicat MySQL Data Transfer

Source Server         : warserver
Source Server Version : 80021
Source Host           : localhost:3306
Source Database       : war_world

Target Server Type    : MYSQL
Target Server Version : 80021
File Encoding         : 65001

Date: 2020-08-16 15:30:06
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for `liveevent_subtask_infos`
-- ----------------------------
DROP TABLE IF EXISTS `liveevent_subtask_infos`;
CREATE TABLE `liveevent_subtask_infos` (
  `Entry` int unsigned NOT NULL,
  `LiveEventTaskId` int unsigned NOT NULL,
  `Name` text,
  `Description` text,
  `TaskCount` int unsigned NOT NULL,
  PRIMARY KEY (`Entry`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of liveevent_subtask_infos
-- ----------------------------
