ALTER TABLE `war_world`.`ability_component` 
CHANGE COLUMN `Values` `Values` VARCHAR(255) NULL DEFAULT NULL ;

ALTER TABLE `war_world`.`ability_line_to_buff_type` 
CHANGE COLUMN `Line ID` `Line ID` INT NOT NULL ,
ADD PRIMARY KEY (`Line ID`);
;
