SET SQL_SAFE_UPDATES = 0;
update rvr_player_gear_drop set realm = 1 where career in (1,2,3,4, 9,10,11,12, 17,18,19,20);
update rvr_player_gear_drop set realm = 2 where career in (5,6,7,8,13,14,15,16,21,22,23,24);