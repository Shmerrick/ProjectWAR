##Sorc changes

##update sorc pit of shades proc rate from 1500 to 900
UPDATE `war_world`.`buff_infos` SET `Interval` = '900' WHERE (`Entry` = '9485');



##BW changes

##update BW Rain of fire proc rate from 1500 to 900

UPDATE `war_world`.`buff_infos` SET `Interval` = '900' WHERE (`Entry` = '8177');
