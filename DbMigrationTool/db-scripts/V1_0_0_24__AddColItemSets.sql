ALTER TABLE `war_world`.`item_sets` 
ADD COLUMN `ClassId` INT NULL AFTER `Unk`,
ADD COLUMN `Comments` VARCHAR(80) NULL AFTER `ClassId`;
