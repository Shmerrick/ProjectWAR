INSERT INTO `war_world`.`item_sets` (`ItemsString`, `BonusString`, `Entry`, `Name`, `Unk`) VALUES ('434918:Invader Ashboots|434930:Invader Scorchguards|434942:Invader Keysash|434954:Invader Illuminations|434966:Invader Grille|434978:Invader Flamerobe|', '34:9,72,0|35:4,72,0|36:31,5,0|85:10390|38:37,120,0|', '4438', 'Invader\'s Fire Robe', '55');
UPDATE `war_world`.`item_infos` SET `ItemSet`='4438' WHERE `Entry`='434918';

UPDATE `war_world`.`item_infos` SET `ItemSet`='4438' WHERE `Entry`='434930';

UPDATE `war_world`.`item_infos` SET `ItemSet`='4438' WHERE `Entry`='434942';

UPDATE `war_world`.`item_infos` SET `ItemSet`='4438' WHERE `Entry`='434954';

UPDATE `war_world`.`item_infos` SET `ItemSet`='4438' WHERE `Entry`='434966';

UPDATE `war_world`.`item_infos` SET `ItemSet`='4438' WHERE `Entry`='434978';

UPDATE `war_world`.`item_sets` SET `BonusString`='34:9,72,0|35:4,72,0|36:31,5,0|85:10409|38:37,120,0|' WHERE `Entry`='4438';

UPDATE `war_world`.`item_sets` SET `BonusString`='34:9,72,0|35:4,72,0|36:31,5,0|85:10409|38:37,120,0|' WHERE `Entry`='4455';
DELETE FROM `war_world`.`item_infos` WHERE `Entry`='852224';
