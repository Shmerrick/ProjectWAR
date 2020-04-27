# Fix Sorc command to cost 20 DM

UPDATE `war_world`.`ability_commands` SET `PrimaryValue`='-20' WHERE `Entry`='9483' and`CommandID`='1' and`CommandSequence`='0';

# Fix BW command to cost 20 DM

UPDATE `war_world`.`ability_commands` SET `PrimaryValue`='-20' WHERE `Entry`='8163' and`CommandID`='1' and`CommandSequence`='0';

