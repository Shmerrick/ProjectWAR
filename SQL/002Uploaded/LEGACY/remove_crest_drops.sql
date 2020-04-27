SET SQL_SAFE_UPDATES = 0;
delete from war_world.loot_group_items where itemid = 208470;

delete from war_world.loot_groups where entry in (1,2);
