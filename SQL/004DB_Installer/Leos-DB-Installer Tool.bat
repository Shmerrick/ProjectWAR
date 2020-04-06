:TOP
@CLS
@ECHO OFF





ECHO Welcome to WarDB database installer.
ECHO.
ECHO Please enter your MySQL information.

set /p user=   Database login: 
set /p pass=   Password: 





SET dumppath=.\development\war_world
SET port=3306
SET host=127.0.0.1
SET mysqlpath=.\MySQL
SET devsql=.\development\war_world
SET changsql=.\development\changesets

:Begin
CLS
ECHO.
ECHO     leo228 Db V2 20.12.19
ECHO.
ECHO 	i - Import World Db, NOTE! World Database will be overwritten!
ECHO 	W - Dump World Db.
ECHO.
ECHO 	D - Dump your table.
ECHO.
ECHO 	X - Exit this tool
ECHO.
SET /p v= 		Enter a char:
IF %v%==* GOTO error
IF %v%==i GOTO import
IF %v%==I GOTO import
IF %v%==w GOTO dumpworld
IF %v%==W GOTO dumpworld
IF %v%==b GOTO import
IF %v%==B GOTO import
IF %v%==z GOTO dumpaccounts
IF %v%==Z GOTO dumpaccounts
IF %v%==e GOTO import
IF %v%==E GOTO import
IF %v%==c GOTO dumpchar
IF %v%==C GOTO dumpchar
IF %v%==r GOTO changeset
IF %v%==R GOTO changeset
IF %v%==D GOTO dumpever
IF %v%==d GOTO dumpever
IF %v%==x GOTO exit
IF %v%==X GOTO exit
GOTO error

:import
CLS
echo          Write the name of your world database, table that you want to import to.
echo.
set /p world_db=           Name:
IF %world_db%=="" GOTO error
CLS
Echo.
echo Importing..
echo.



ECHO import: war_world.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\war_world.sql

ECHO import: abilities.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\abilities.sql

ECHO import: ability_commands.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\ability_commands.sql

ECHO import: ability_damage_heals.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\ability_damage_heals.sql

ECHO import: ability_knockback_info.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\ability_knockback_info.sql

ECHO import: ability_modifier_checks.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\ability_modifier_checks.sql

ECHO import: ability_modifiers.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\ability_modifiers.sql

ECHO import: battlefront_guards.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\battlefront_guards.sql

ECHO import: battlefront_keep_status.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\battlefront_keep_status.sql

ECHO import: battlefront_objectives.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\battlefront_objectives.sql

ECHO import: battlefront_objects.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\battlefront_objects.sql

ECHO import: battlefront_resource_spawns.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\battlefront_resource_spawns.sql

ECHO import: battlefront_status.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\battlefront_status.sql

ECHO import: black_market_vendor_items.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\black_market_vendor_items.sql

ECHO import: boss_spawn.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\boss_spawn.sql

ECHO import: boss_spawn_abilities.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\boss_spawn_abilities.sql

ECHO import: boss_spawn_phases.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\boss_spawn_phases.sql


ECHO import: bounty_contribution_analytics.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\bounty_contribution_analytics.sql

ECHO import: bounty_contribution_analytics_details.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\bounty_contribution_analytics_details.sql

ECHO import: bounty_contribution_definition.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\bounty_contribution_definition.sql

ECHO import: buff_commands.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\buff_commands.sql

ECHO import: buff_infos.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\buff_infos.sql

ECHO import: campaign_objective_buff.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\campaign_objective_buff.sql

ECHO import: chapter_infos.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\chapter_infos.sql

ECHO import: chapter_rewards.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\chapter_rewards.sql

ECHO import: characterinfo.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\characterinfo.sql

ECHO import: characterinfo_items.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\characterinfo_items.sql

ECHO import: characterinfo_renown.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\characterinfo_renown.sql

ECHO import: characterinfo_stats.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\characterinfo_stats.sql

ECHO import: classes.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\classes.sql

ECHO import: creature_abilities.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\creature_abilities.sql

ECHO import: creature_items.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\creature_items.sql

ECHO import: creature_loots.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\creature_loots.sql

ECHO import: creature_protos.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\creature_protos.sql

ECHO import: creature_smart_abilities.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\creature_smart_abilities.sql

ECHO import: creature_spawns.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\creature_spawns.sql

ECHO import: creature_stats.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\creature_stats.sql

ECHO import: creature_texts.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\creature_texts.sql

ECHO import: creature_vendors.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\creature_vendors.sql

ECHO import: dye_infos.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\dye_infos.sql

ECHO import: entries.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\entries.sql

ECHO import: event_infos.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\event_infos.sql

ECHO import: event_spawns.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\event_spawns.sql

ECHO import: gameobject_loots.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\gameobject_loots.sql

ECHO import: gameobject_protos.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\gameobject_protos.sql

ECHO import: gameobject_spawns.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\gameobject_spawns.sql

ECHO import: guild_xp.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\guild_xp.sql

ECHO import: honor_history.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\honor_history.sql

ECHO import: honor_rewards.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\honor_rewards.sql

ECHO import: instance_attributes.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\instance_attributes.sql

ECHO import: instance_boss_spawns.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\instance_boss_spawns.sql

ECHO import: instance_creature_spawns.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\instance_creature_spawns.sql

ECHO import: instance_encounters.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\instance_encounters.sql

ECHO import: instance_event_commands.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\instance_event_commands.sql

ECHO import: instance_events.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\instance_events.sql

ECHO import: instance_infos.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\instance_infos.sql

ECHO import: instance_lockouts.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\instance_lockouts.sql

ECHO import: instance_objects.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\instance_objects.sql

ECHO import: instance_scripts.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\instance_scripts.sql

ECHO import: instance_spawn_state_abilities.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\instance_spawn_state_abilities.sql

ECHO import: instance_spawn_states.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\instance_spawn_states.sql

ECHO import: instance_statistics.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\instance_statistics.sql

ECHO import: item_bonus.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\item_bonus.sql

ECHO import: item_infos.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\item_infos.sql

ECHO import: item_rarity.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\item_rarity.sql

ECHO import: item_sets.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\item_sets.sql

ECHO import: item_slots.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\item_slots.sql

ECHO import: keep_creatures.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\keep_creatures.sql

ECHO import: keep_doors.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\keep_doors.sql

ECHO import: keep_infos.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\keep_infos.sql

ECHO import: keep_spawn_points.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\keep_spawn_points.sql

ECHO import: kill_tracker.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\kill_tracker.sql

ECHO import: liveevent_infos.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\liveevent_infos.sql

ECHO import: liveevent_reward_infos.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\liveevent_reward_infos.sql

ECHO import: liveevent_subtask_infos.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\liveevent_subtask_infos.sql

ECHO import: liveevent_task_infos.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\liveevent_task_infos.sql

ECHO import: loot_group_butchering.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\loot_group_butchering.sql

ECHO import: loot_group_items.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\loot_group_items.sql

ECHO import: loot_groups.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\loot_groups.sql

ECHO import: mount_infos.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\mount_infos.sql

ECHO import: pairing_infos.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\pairing_infos.sql

ECHO import: pet_mastery_modifiers.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\pet_mastery_modifiers.sql

ECHO import: pet_stat_override.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\pet_stat_override.sql

ECHO import: petinfo_stats.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\petinfo_stats.sql

ECHO import: player_keep_spawn.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\player_keep_spawn.sql

ECHO import: pquest_info.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\pquest_info.sql

ECHO import: pquest_loot.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\pquest_loot.sql

ECHO import: pquest_loot_crafting.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\pquest_loot_crafting.sql

ECHO import: pquest_objectives.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\pquest_objectives.sql

ECHO import: pquest_spawns.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\pquest_spawns.sql

ECHO import: quests.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\quests.sql

ECHO import: quests_creature_finisher.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\quests_creature_finisher.sql

ECHO import: quests_creature_starter.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\quests_creature_starter.sql

ECHO import: quests_maps.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\quests_maps.sql

ECHO import: quests_objectives.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\quests_objectives.sql

ECHO import: races.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\races.sql

ECHO import: rallypoints.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\rallypoints.sql

ECHO import: random_names.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\random_names.sql

ECHO import: renown_infos.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\renown_infos.sql

ECHO import: rvr_area_polygon.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\rvr_area_polygon.sql

ECHO import: rvr_keep_lock_bag_reward_history.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\rvr_keep_lock_bag_reward_history.sql

ECHO import: rvr_keep_lock_eligibility_history.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\rvr_keep_lock_eligibility_history.sql

ECHO import: rvr_keep_lock_reward.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\rvr_keep_lock_reward.sql

ECHO import: rvr_metrics.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\rvr_metrics.sql

ECHO import: rvr_objects.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\rvr_objects.sql

ECHO import: rvr_player_contribution.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\rvr_player_contribution.sql

ECHO import: rvr_player_gear_drop.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\rvr_player_gear_drop.sql

ECHO import: rvr_player_kill_reward_history.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\rvr_player_kill_reward_history.sql

ECHO import: rvr_progression.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\rvr_progression.sql

ECHO import: rvr_reward_fort_items.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\rvr_reward_fort_items.sql

ECHO import: rvr_reward_keep_items.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\rvr_reward_keep_items.sql

ECHO import: rvr_reward_objective_tick.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\rvr_reward_objective_tick.sql

ECHO import: rvr_reward_pairing_lock.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\rvr_reward_pairing_lock.sql

ECHO import: rvr_reward_player_kill.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\rvr_reward_player_kill.sql

ECHO import: rvr_reward_zone_lock.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\rvr_reward_zone_lock.sql

ECHO import: rvr_zone_lock_eligibility_history.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\rvr_zone_lock_eligibility_history.sql

ECHO import: rvr_zone_lock_item_option.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\rvr_zone_lock_item_option.sql

ECHO import: rvr_zone_lock_reward.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\rvr_zone_lock_reward.sql

ECHO import: rvr_zone_lock_summary.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\rvr_zone_lock_summary.sql

ECHO import: scenario_gauntlet_spawn.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\scenario_gauntlet_spawn.sql

ECHO import: scenario_infos.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\scenario_infos.sql

ECHO import: scenario_objects.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\scenario_objects.sql

ECHO import: timedannounces.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\timedannounces.sql

ECHO import: tok_bestary.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\tok_bestary.sql

ECHO import: tok_infos.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\tok_infos.sql

ECHO import: vendor_items.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\vendor_items.sql

ECHO import: waypoints.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\waypoints.sql

ECHO import: world_settings.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\world_settings.sql

ECHO import: xp_infos.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\xp_infos.sql

ECHO import: zone_areas.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\zone_areas.sql

ECHO import: zone_infos.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\zone_infos.sql

ECHO import: zone_jumps.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\zone_jumps.sql

ECHO import: zone_respawns.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\zone_respawns.sql

ECHO import: zone_taxis.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %devsql%\zone_taxis.sql




ECHO Done.
ECHO.
ECHO Press any key to exit.
PAUSE >NUL
GOTO exit

:dumpworld
@ECHO OFF
CLS
echo          Write the name of your world database, that you want to dump.
echo.
set /p world_db=           Name:
IF %world_db%=="" GOTO error
CLS
if not exist "%dumppath%" mkdir %dumppath%
echo %world_db% Database Export started...
ECHO Dumping: abilities
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% abilities > %dumppath%\abilities.sql
ECHO Dumping: ability_commands
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% ability_commands > %dumppath%\ability_commands.sql
ECHO Dumping: ability_damage_heals
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% ability_damage_heals > %dumppath%\ability_damage_heals.sql
ECHO Dumping: ability_knockback_info
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% ability_knockback_info > %dumppath%\ability_knockback_info.sql
ECHO Dumping: ability_modifier_checks
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% ability_modifier_checks > %dumppath%\ability_modifier_checks.sql
ECHO Dumping: ability_modifiers
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% ability_modifiers > %dumppath%\ability_modifiers.sql
ECHO Dumping: battlefront_guards
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% battlefront_guards > %dumppath%\battlefront_guards.sql
ECHO Dumping: battlefront_keep_status
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% battlefront_keep_status > %dumppath%\battlefront_keep_status.sql
ECHO Dumping: battlefront_objectives
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% battlefront_objectives > %dumppath%\battlefront_objectives.sql
ECHO Dumping: battlefront_objects
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% battlefront_objects > %dumppath%\battlefront_objects.sql
ECHO Dumping: battlefront_resource_spawns
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% battlefront_resource_spawns > %dumppath%\battlefront_resource_spawns.sql
ECHO Dumping: battlefront_status
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% battlefront_status > %dumppath%\battlefront_status.sql
ECHO Dumping: black_market_vendor_items
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% black_market_vendor_items > %dumppath%\black_market_vendor_items.sql
ECHO Dumping: boss_spawn
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% boss_spawn > %dumppath%\boss_spawn.sql
ECHO Dumping: boss_spawn_abilities
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% boss_spawn_abilities > %dumppath%\boss_spawn_abilities.sql
ECHO Dumping: boss_spawn_phases
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% boss_spawn_phases > %dumppath%\boss_spawn_phases.sql
ECHO Dumping: bounty_contribution_analytics
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% bounty_contribution_analytics > %dumppath%\bounty_contribution_analytics.sql
ECHO Dumping: bounty_contribution_analytics_details
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% bounty_contribution_analytics_details > %dumppath%\bounty_contribution_analytics_details.sql
ECHO Dumping: bounty_contribution_definition
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% bounty_contribution_definition > %dumppath%\bounty_contribution_definition.sql
ECHO Dumping: buff_commands
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% buff_commands > %dumppath%\buff_commands.sql
ECHO Dumping: buff_infos
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% buff_infos > %dumppath%\buff_infos.sql
ECHO Dumping: campaign_objective_buff
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% campaign_objective_buff > %dumppath%\campaign_objective_buff.sql
ECHO Dumping: chapter_infos
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% chapter_infos > %dumppath%\chapter_infos.sql
ECHO Dumping: chapter_rewards
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% chapter_rewards > %dumppath%\chapter_rewards.sql
ECHO Dumping: characterinfo
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% characterinfo > %dumppath%\characterinfo.sql
ECHO Dumping: characterinfo_items
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% characterinfo_items > %dumppath%\characterinfo_items.sql
ECHO Dumping: characterinfo_renown
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% characterinfo_renown > %dumppath%\characterinfo_renown.sql
ECHO Dumping: characterinfo_stats
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% characterinfo_stats > %dumppath%\characterinfo_stats.sql
ECHO Dumping: classes
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% classes > %dumppath%\classes.sql
ECHO Dumping: creature_abilities
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% creature_abilities > %dumppath%\creature_abilities.sql
ECHO Dumping: creature_items
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% creature_items > %dumppath%\creature_items.sql
ECHO Dumping: creature_loots
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% creature_loots > %dumppath%\creature_loots.sql
ECHO Dumping: creature_protos
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% creature_protos > %dumppath%\creature_protos.sql
ECHO Dumping: creature_smart_abilities
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% creature_smart_abilities > %dumppath%\creature_smart_abilities.sql
ECHO Dumping: creature_spawns
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% creature_spawns > %dumppath%\creature_spawns.sql
ECHO Dumping: creature_stats
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% creature_stats > %dumppath%\creature_stats.sql
ECHO Dumping: creature_texts
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% creature_texts > %dumppath%\creature_texts.sql
ECHO Dumping: creature_vendors
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% creature_vendors > %dumppath%\creature_vendors.sql
ECHO Dumping: dye_infos
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% dye_infos > %dumppath%\dye_infos.sql
ECHO Dumping: entries
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% entries > %dumppath%\entries.sql
ECHO Dumping: event_infos
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% event_infos > %dumppath%\event_infos.sql
ECHO Dumping: event_spawns
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% event_spawns > %dumppath%\event_spawns.sql
ECHO Dumping: gameobject_loots
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% gameobject_loots > %dumppath%\gameobject_loots.sql
ECHO Dumping: gameobject_protos
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% gameobject_protos > %dumppath%\gameobject_protos.sql
ECHO Dumping: gameobject_spawns
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% gameobject_spawns > %dumppath%\gameobject_spawns.sql
ECHO Dumping: guild_xp
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% guild_xp > %dumppath%\guild_xp.sql
ECHO Dumping: honor_history
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% honor_history > %dumppath%\honor_history.sql
ECHO Dumping: honor_rewards
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% honor_rewards > %dumppath%\honor_rewards.sql
ECHO Dumping: instance_attributes
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% instance_attributes > %dumppath%\instance_attributes.sql
ECHO Dumping: instance_boss_spawns
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% instance_boss_spawns > %dumppath%\instance_boss_spawns.sql
ECHO Dumping: instance_creature_spawns
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% instance_creature_spawns > %dumppath%\instance_creature_spawns.sql
ECHO Dumping: instance_encounters
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% instance_encounters > %dumppath%\instance_encounters.sql
ECHO Dumping: instance_event_commands
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% instance_event_commands > %dumppath%\instance_event_commands.sql
ECHO Dumping: instance_events
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% instance_events > %dumppath%\instance_events.sql
ECHO Dumping: instance_infos
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% instance_infos > %dumppath%\instance_infos.sql
ECHO Dumping: instance_lockouts
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% instance_lockouts > %dumppath%\instance_lockouts.sql
ECHO Dumping: instance_objects
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% instance_objects > %dumppath%\instance_objects.sql
ECHO Dumping: instance_scripts
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% instance_scripts > %dumppath%\instance_scripts.sql
ECHO Dumping: instance_spawn_state_abilities
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% instance_spawn_state_abilities > %dumppath%\instance_spawn_state_abilities.sql
ECHO Dumping: instance_spawn_states
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% instance_spawn_states > %dumppath%\instance_spawn_states.sql
ECHO Dumping: instance_statistics
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% instance_statistics > %dumppath%\instance_statistics.sql
ECHO Dumping: item_bonus
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% item_bonus > %dumppath%\item_bonus.sql
ECHO Dumping: item_infos
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% item_infos > %dumppath%\item_infos.sql
ECHO Dumping: item_rarity
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% item_rarity > %dumppath%\item_rarity.sql
ECHO Dumping: item_sets
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% item_sets > %dumppath%\item_sets.sql
ECHO Dumping: item_slots
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% item_slots > %dumppath%\item_slots.sql
ECHO Dumping: keep_creatures
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% keep_creatures > %dumppath%\keep_creatures.sql
ECHO Dumping: keep_doors
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% keep_doors > %dumppath%\keep_doors.sql
ECHO Dumping: keep_infos
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% keep_infos > %dumppath%\keep_infos.sql
ECHO Dumping: keep_spawn_points
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% keep_spawn_points > %dumppath%\keep_spawn_points.sql
ECHO Dumping: kill_tracker
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% kill_tracker > %dumppath%\kill_tracker.sql
ECHO Dumping: liveevent_infos
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% liveevent_infos > %dumppath%\liveevent_infos.sql
ECHO Dumping: liveevent_reward_infos
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% liveevent_reward_infos > %dumppath%\liveevent_reward_infos.sql
ECHO Dumping: liveevent_subtask_infos
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% liveevent_subtask_infos > %dumppath%\liveevent_subtask_infos.sql
ECHO Dumping: liveevent_task_infos
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% liveevent_task_infos > %dumppath%\liveevent_task_infos.sql
ECHO Dumping: loot_group_butchering
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% loot_group_butchering > %dumppath%\loot_group_butchering.sql
ECHO Dumping: loot_group_items
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% loot_group_items > %dumppath%\loot_group_items.sql
ECHO Dumping: loot_groups
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% loot_groups > %dumppath%\loot_groups.sql
ECHO Dumping: mount_infos
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% mount_infos > %dumppath%\mount_infos.sql
ECHO Dumping: pairing_infos
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% pairing_infos > %dumppath%\pairing_infos.sql
ECHO Dumping: pet_mastery_modifiers
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% pet_mastery_modifiers > %dumppath%\pet_mastery_modifiers.sql
ECHO Dumping: pet_stat_override
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% pet_stat_override > %dumppath%\pet_stat_override.sql
ECHO Dumping: petinfo_stats
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% petinfo_stats > %dumppath%\petinfo_stats.sql
ECHO Dumping: player_keep_spawn
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% player_keep_spawn > %dumppath%\player_keep_spawn.sql
ECHO Dumping: pquest_info
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% pquest_info > %dumppath%\pquest_info.sql
ECHO Dumping: pquest_loot
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% pquest_loot > %dumppath%\pquest_loot.sql
ECHO Dumping: pquest_loot_crafting
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% pquest_loot_crafting > %dumppath%\pquest_loot_crafting.sql
ECHO Dumping: pquest_objectives
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% pquest_objectives > %dumppath%\pquest_objectives.sql
ECHO Dumping: pquest_spawns
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% pquest_spawns > %dumppath%\pquest_spawns.sql
ECHO Dumping: quests
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% quests > %dumppath%\quests.sql
ECHO Dumping: quests_creature_finisher
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% quests_creature_finisher > %dumppath%\quests_creature_finisher.sql
ECHO Dumping: quests_creature_starter
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% quests_creature_starter > %dumppath%\quests_creature_starter.sql
ECHO Dumping: quests_maps
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% quests_maps > %dumppath%\quests_maps.sql
ECHO Dumping: quests_objectives
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% quests_objectives > %dumppath%\quests_objectives.sql
ECHO Dumping: races
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% races > %dumppath%\races.sql
ECHO Dumping: rallypoints
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% rallypoints > %dumppath%\rallypoints.sql
ECHO Dumping: random_names
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% random_names > %dumppath%\random_names.sql
ECHO Dumping: renown_infos
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% renown_infos > %dumppath%\renown_infos.sql
ECHO Dumping: rvr_area_polygon
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% rvr_area_polygon > %dumppath%\rvr_area_polygon.sql
ECHO Dumping: rvr_keep_lock_bag_reward_history
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% rvr_keep_lock_bag_reward_history > %dumppath%\rvr_keep_lock_bag_reward_history.sql
ECHO Dumping: rvr_keep_lock_eligibility_history
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% rvr_keep_lock_eligibility_history > %dumppath%\rvr_keep_lock_eligibility_history.sql
ECHO Dumping: rvr_keep_lock_reward
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% rvr_keep_lock_reward > %dumppath%\rvr_keep_lock_reward.sql
ECHO Dumping: rvr_metrics
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% rvr_metrics > %dumppath%\rvr_metrics.sql
ECHO Dumping: rvr_objects
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% rvr_objects > %dumppath%\rvr_objects.sql
ECHO Dumping: rvr_player_contribution
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% rvr_player_contribution > %dumppath%\rvr_player_contribution.sql
ECHO Dumping: rvr_player_gear_drop
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% rvr_player_gear_drop > %dumppath%\rvr_player_gear_drop.sql
ECHO Dumping: rvr_player_kill_reward_history
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% rvr_player_kill_reward_history > %dumppath%\rvr_player_kill_reward_history.sql
ECHO Dumping: rvr_progression
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% rvr_progression > %dumppath%\rvr_progression.sql
ECHO Dumping: rvr_reward_fort_items
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% rvr_reward_fort_items > %dumppath%\rvr_reward_fort_items.sql
ECHO Dumping: rvr_reward_keep_items
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% rvr_reward_keep_items > %dumppath%\rvr_reward_keep_items.sql
ECHO Dumping: rvr_reward_objective_tick
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% rvr_reward_objective_tick > %dumppath%\rvr_reward_objective_tick.sql
ECHO Dumping: rvr_reward_pairing_lock
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% rvr_reward_pairing_lock > %dumppath%\rvr_reward_pairing_lock.sql
ECHO Dumping: rvr_reward_player_kill
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% rvr_reward_player_kill > %dumppath%\rvr_reward_player_kill.sql
ECHO Dumping: rvr_reward_zone_lock
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% rvr_reward_zone_lock > %dumppath%\rvr_reward_zone_lock.sql
ECHO Dumping: rvr_zone_lock_eligibility_history
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% rvr_zone_lock_eligibility_history > %dumppath%\rvr_zone_lock_eligibility_history.sql
ECHO Dumping: rvr_zone_lock_item_option
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% rvr_zone_lock_item_option > %dumppath%\rvr_zone_lock_item_option.sql
ECHO Dumping: rvr_zone_lock_reward
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% rvr_zone_lock_reward > %dumppath%\rvr_zone_lock_reward.sql
ECHO Dumping: rvr_zone_lock_summary
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% rvr_zone_lock_summary > %dumppath%\rvr_zone_lock_summary.sql
ECHO Dumping: scenario_gauntlet_spawn
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% scenario_gauntlet_spawn > %dumppath%\scenario_gauntlet_spawn.sql
ECHO Dumping: scenario_infos
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% scenario_infos > %dumppath%\scenario_infos.sql
ECHO Dumping: scenario_objects
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% scenario_objects > %dumppath%\scenario_objects.sql
ECHO Dumping: timedannounces
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% timedannounces > %dumppath%\timedannounces.sql
ECHO Dumping: tok_bestary
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% tok_bestary > %dumppath%\tok_bestary.sql
ECHO Dumping: tok_infos
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% tok_infos > %dumppath%\tok_infos.sql
ECHO Dumping: vendor_items
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% vendor_items > %dumppath%\vendor_items.sql
ECHO Dumping: waypoints
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% waypoints > %dumppath%\waypoints.sql
ECHO Dumping: world_settings
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% world_settings > %dumppath%\world_settings.sql
ECHO Dumping: xp_infos
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% xp_infos > %dumppath%\xp_infos.sql
ECHO Dumping: zone_areas
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% zone_areas > %dumppath%\zone_areas.sql
ECHO Dumping: zone_infos
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% zone_infos > %dumppath%\zone_infos.sql
ECHO Dumping: zone_jumps
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% zone_jumps > %dumppath%\zone_jumps.sql
ECHO Dumping: zone_respawns
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% zone_respawns > %dumppath%\zone_respawns.sql
ECHO Dumping: zone_taxis
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %world_db% zone_taxis > %dumppath%\zone_taxis.sql


echo  Finished ... %world_db% exported to %dumppath% folder...
pause 
GOTO begin


:import
CLS
echo          Write the name of your accounts database, table that you want to import to.
echo.
set /p accounts_db=           Name:
IF %accounts_db%=="" GOTO error
CLS
Echo.
echo Importing..
echo.
ECHO import: account_sanction_logs.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %accounts_db% < %devsql%\account_sanction_logs.sql

ECHO import: account_value.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %accounts_db% < %devsql%\account_value.sql

ECHO import: accounts.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %accounts_db% < %devsql%\accounts.sql

ECHO import: ip_bans.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %accounts_db% < %devsql%\ip_bans.sql

ECHO import: realms.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %accounts_db% < %devsql%\realms.sql


ECHO Done.
ECHO.
ECHO Press any key to exit.
PAUSE >NUL
GOTO exit



:dumpaccounts
@ECHO OFF
CLS
echo          Write the name of your accounts database, that you want to dump.
echo.
set /p accounts_db=           Name:
IF %accounts_db%=="" GOTO error
CLS
if not exist "%dumppath%" mkdir %dumppath%
echo %accounts_db% Database Export started...
ECHO Dumping: account_sanction_logs
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %accounts_db% account_sanction_logs > %dumppath%\account_sanction_logs.sql
ECHO Dumping: account_value
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %accounts_db% account_value > %dumppath%\account_value.sql
ECHO Dumping: accounts
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %accounts_db% accounts > %dumppath%\accounts.sql
ECHO Dumping: ip_bans
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %accounts_db% ip_bans > %dumppath%\ip_bans.sql
ECHO Dumping: realms
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %accounts_db% realms > %dumppath%\realms.sql


echo  Finished ... %accounts_db% dumped to %dumppath% folder...
pause 
GOTO begin
COLOR 2A




:import
CLS
echo          Write the name of your characters database, table that you want to import to.
echo.
set /p characters_db=           Name:
IF %characters_db%=="" GOTO error
CLS
Echo.
echo Importing..
echo.

ECHO import: auctions.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %characters_db% < %charsql%\auctions.sql

ECHO import: banned_names.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %characters_db% < %charsql%\banned_names.sql

ECHO import: bug_report.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %characters_db% < %charsql%\bug_report.sql

ECHO import: character_abilities.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %characters_db% < %charsql%\character_abilities.sql

ECHO import: character_bag_pools.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %characters_db% < %charsql%\character_bag_pools.sql

ECHO import: character_client_data.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %characters_db% < %charsql%\character_client_data.sql

ECHO import: character_deletions.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %characters_db% < %charsql%\character_deletions.sql

ECHO import: character_influences.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %characters_db% < %charsql%\character_influences.sql

ECHO import: character_saved_buffs.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %characters_db% < %charsql%\character_saved_buffs.sql

ECHO import: characters.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %characters_db% < %charsql%\characters.sql

ECHO import: characters_items.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %characters_db% < %charsql%\characters_items.sql

ECHO import: characters_mails.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %characters_db% < %charsql%\characters_mails.sql

ECHO import: characters_quests.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %characters_db% < %charsql%\characters_quests.sql

ECHO import: characters_socials.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %characters_db% < %charsql%\characters_socials.sql

ECHO import: characters_toks.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %characters_db% < %charsql%\characters_toks.sql

ECHO import: characters_toks_kills.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %characters_db% < %charsql%\characters_toks_kills.sql

ECHO import: characters_value.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %characters_db% < %charsql%\characters_value.sql

ECHO import: gmcommandlogs.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %characters_db% < %charsql%\gmcommandlogs.sql

ECHO import: guild_alliance_info.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %characters_db% < %charsql%\guild_alliance_info.sql

ECHO import: guild_event.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %characters_db% < %charsql%\guild_event.sql

ECHO import: guild_info.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %characters_db% < %charsql%\guild_info.sql

ECHO import: guild_logs.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %characters_db% < %charsql%\guild_logs.sql

ECHO import: guild_members.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %characters_db% < %charsql%\guild_members.sql

ECHO import: guild_ranks.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %characters_db% < %charsql%\guild_ranks.sql

ECHO import: guild_vault_item.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %characters_db% < %charsql%\guild_vault_item.sql

ECHO import: scenario_durations.sql
%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %characters_db% < %charsql%\scenario_durations.sql


echo  Finished ... %characters_db% imported to db...

ECHO Done.
ECHO.
ECHO Press any key to exit.
PAUSE >NUL
GOTO exit


:dumpchar
@ECHO OFF
CLS
echo          Write the name of your world database, that you want to dump.
echo.
set /p char_db=           Name:
IF %char_db%=="" GOTO error
CLS
if not exist "%dumppath%" mkdir %dumppath%
echo %char_db% Database Export started...

ECHO Dumping: auctions
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %char_db% auctions > %dumppath%\auctions.sql
ECHO Dumping: banned_names
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %char_db% banned_names > %dumppath%\banned_names.sql
ECHO Dumping: bug_report
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %char_db% bug_report > %dumppath%\bug_report.sql
ECHO Dumping: character_abilities
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %char_db% character_abilities > %dumppath%\character_abilities.sql
ECHO Dumping: character_bag_pools
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %char_db% character_bag_pools > %dumppath%\character_bag_pools.sql
ECHO Dumping: character_client_data
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %char_db% character_client_data > %dumppath%\character_client_data.sql
ECHO Dumping: character_deletions
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %char_db% character_deletions > %dumppath%\character_deletions.sql
ECHO Dumping: character_influences
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %char_db% character_influences > %dumppath%\character_influences.sql
ECHO Dumping: character_saved_buffs
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %char_db% character_saved_buffs > %dumppath%\character_saved_buffs.sql
ECHO Dumping: characters
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %char_db% characters > %dumppath%\characters.sql
ECHO Dumping: characters_items
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %char_db% characters_items > %dumppath%\characters_items.sql
ECHO Dumping: characters_mails
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %char_db% characters_mails > %dumppath%\characters_mails.sql
ECHO Dumping: characters_quests
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %char_db% characters_quests > %dumppath%\characters_quests.sql
ECHO Dumping: characters_socials
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %char_db% characters_socials > %dumppath%\characters_socials.sql
ECHO Dumping: characters_toks
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %char_db% characters_toks > %dumppath%\characters_toks.sql
ECHO Dumping: characters_toks_kills
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %char_db% characters_toks_kills > %dumppath%\characters_toks_kills.sql
ECHO Dumping: characters_value
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %char_db% characters_value > %dumppath%\characters_value.sql
ECHO Dumping: gmcommandlogs
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %char_db% gmcommandlogs > %dumppath%\gmcommandlogs.sql
ECHO Dumping: guild_alliance_info
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %char_db% guild_alliance_info > %dumppath%\guild_alliance_info.sql
ECHO Dumping: guild_event
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %char_db% guild_event > %dumppath%\guild_event.sql
ECHO Dumping: guild_info
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %char_db% guild_info > %dumppath%\guild_info.sql
ECHO Dumping: guild_logs
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %char_db% guild_logs > %dumppath%\guild_logs.sql
ECHO Dumping: guild_members
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %char_db% guild_members > %dumppath%\guild_members.sql
ECHO Dumping: guild_ranks
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %char_db% guild_ranks > %dumppath%\guild_ranks.sql
ECHO Dumping: guild_vault_item
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %char_db% guild_vault_item > %dumppath%\guild_vault_item.sql
ECHO Dumping: scenario_durations
%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% %char_db% scenario_durations > %dumppath%\scenario_durations.sql


echo  Finished ... %char_db% dumped to %dumppath% folder...
pause 
GOTO begin
COLOR 2A




:changeset
CLS
ECHO   Please Write down number of changeset (not the number of rev!!!)
ECHO.
ECHO.
set /p ch=      Number:
ECHO.
ECHO      Importing...
if not exist "%changsql%\changeset_%ch%.sql" GOTO error2
ECHO.

%mysqlpath%\mysql -h %host% --user=%user% --password=%pass% --port=%port% %world_db% < %changsql%\changeset_%ch%.sql
ECHO.
ECHO      File changeset_%ch%.sql imported sucesfully!
ECHO.
PAUSE
GOTO begin

:dumpever
CLS
echo          Write the name of your database, where you have tables to dump.
echo.
set /p db=           Name:
IF %db%=="" GOTO error 
echo.
echo.
echo          Type there name of table, which you want to dump
echo          (if you want to dump more tables, type space between names):
echo.
set /p z=           Name(s):
IF %z%=="" GOTO error 
echo              Processing....
echo.
set bu1="%z%"

if not exist "%dumppath%" mkdir %dumppath%

%mysqlpath%\mysqldump -h %host% --user=%user% --password=%pass% --add-drop-table %db% "%bu1%" > "%dumppath%"\"%z%.sql"

@echo              Dump table(s) %z% from database %db% is/are succesfull !
set bu1=
set z=
echo.
pause
goto begin

:error
ECHO 	Please enter a correct database.
ECHO.
PAUSE
GOTO begin

:error2
ECHO 	Changeset with this number not found.
ECHO.
PAUSE
GOTO changeset

:exit