delete  from war_world.item_infos  where Entry = 7;
delete from war_world.buff_infos where Entry= 9999;
delete from war_world.buff_commands where Entry= 9999;
delete from war_world.abilities where Entry= 9999;
delete from war_world.ability_commands where Entry= 9999;
insert into war_world.item_infos values(
'7',#Entry
'Windblade',#Item Name
 'Like all the favours given by the Changer of Ways, it is an erratic and unpredictable weapon.',#description
 '2',#type
 '64', #Race
 '5467',#ModelID
 '13',#SlotID
 '4',#rarity
 '0', #career
 '2', #skills
 '1',#bind
 '0',#armor
 '0',#spellid
 '827',#dps 160=16.0dps
 '1000',#speed 340=3.4speed
 '40',#MinRank
 '0', #MinRenown
 '0', #startquest,keep zero
 '1:54;5:45;4:11;80:15;76:4;0:0;0:0;0:0;0:0;0:0;0:0;0:0',  #stats
 '324',  #sellprice
 '2', #TalismanSlots
 '1', #MaxtStack,keep 1
 '', #scriptName, keep ''
 '40',#Objectlevel=itemlvl
 '0', #unique Equiped 
 '', #crafts,keep null
 '', #ink27,keep null
 '', #sellrequireditems, keep null
 '1', #twohanded
 '0',#itemset number
 '',#craft result, keep null
 NULL,#dyeable
 '1', #salvageable
 NULL, #basecolor1
 NULL, #basecolor2
 '0', #tokunlock, keep 0
'9999', #item effects
 '0');	#tokunlock																																			
#select * from war_world.item_infos  where Entry = 7;

insert into war_world.buff_infos values(
9999,
'Test',
NULL,
'Blessing',
2,
NULL,
1,
1,
NULL,
NULL,
NULL,
NULL,
NULL,
NULL,
1,
1,
NULL,
NULL,
1);
#change '9186=pounce, 9057=wingsofheaven to what ever
 insert into  war_world.buff_commands values(
'9999', 'Test Ability', '1', '0', 'GrantTempAbility', '9057', NULL, NULL, '5', 'Host', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '2', '0', NULL );

 insert into  war_world.buff_commands values(
'9999', 'Test fury', '0', '0', 'ModifyStat', '1', '2', '80', '5', 'Host', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '1', NULL, NULL);


 insert into war_world.abilities  values(
'9999', '0', 'Test Ability', NULL, '100', NULL, '60', '0', NULL, NULL, NULL, NULL, '2087', NULL, NULL, NULL, NULL, '3', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '5137', '0', '10', '0', NULL, NULL, '100', NULL, NULL, NULL, NULL, NULL, NULL);



insert into war_world.ability_commands values('9999', 'Test', '0', '0', 'InvokeBuff', '9999', NULL, 'WithinGroup', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);




