ALTER TABLE `war_world`.`item_sets` 
ADD COLUMN `ItemSetList` VARCHAR(250) NULL COMMENT 'Auto generated list of item ids' AFTER `Comments`,
ADD COLUMN `ItemSetFullDescription` VARCHAR(8000) NULL COMMENT 'Auto generated set desciption' AFTER `ItemSetList`;
