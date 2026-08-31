-- Restores guild-claim flags from original live-server packet captures archived in WAR-RE-Toolkit.
--
-- Only packet-proven locations are included. Gnol Baraz, Thickmuck Pit, and Stoneclaw Castle remain
-- unmapped because no authoritative flag packet has been found for them. Tier 1 keeps are omitted
-- because guild claiming is not active there. Safe to re-run: objective rows are updated in place,
-- and existing nonzero keep mappings are preserved.

USE `war_world`;

START TRANSACTION;

INSERT INTO `battlefront_objectives`
    (`Entry`, `RegionId`, `ZoneId`, `Name`, `X`, `Y`, `Z`, `O`, `TokDiscovered`, `TokUnlocked`, `KeepSpawn`)
VALUES
    (60005, 2, 9, 'Karaz Drengi', 1413569, 845990, 10573, 4073, 0, 0, 1),
    (60006, 2, 9, 'Kazad Dammaz', 1402315, 874105, 9919, 1695, 0, 0, 1),
    (60007, 2, 5, 'Bloodfist Rock', 1405708, 928192, 12626, 2082, 0, 0, 1),
    (60008, 2, 5, 'Karak Karag', 1370622, 928133, 12528, 4027, 0, 0, 1),
    (60009, 2, 3, 'Ironskin Skar', 1395536, 978626, 8332, 2537, 0, 0, 1),
    (60010, 2, 3, 'Badmoon Hole', 1414252, 1012022, 6596, 2036, 0, 0, 1),
    (60013, 6, 108, 'Passwatch Castle', 1250252, 892009, 12740, 3606, 0, 0, 1),
    (60015, 11, 109, 'Wilhelm\'s Fist', 1439716, 911819, 19292, 1171, 0, 0, 1),
    (60016, 11, 109, 'Morr\'s Repose', 1432068, 944132, 16765, 3003, 0, 0, 1),
    (60017, 11, 105, 'Southern Garrison', 1446127, 882319, 16263, 1570, 0, 0, 1),
    (60018, 11, 105, 'Garrison of Skulls', 1444245, 830946, 14938, 3117, 0, 0, 1),
    (60019, 11, 103, 'Zimmeron\'s Hold', 1440642, 763035, 13863, 2719, 0, 0, 1),
    (60020, 11, 103, 'Charon\'s Citadel', 1447496, 796629, 14624, 3060, 0, 0, 1),
    (60023, 16, 208, 'Well of Qhaysh', 1426368, 1490358, 5084, 409, 0, 0, 1),
    (60024, 16, 202, 'Ghrond\'s Sacristy', 1438183, 1463235, 4714, 0, 0, 0, 1),
    (60025, 4, 209, 'Arbor of Light', 1070405, 1639847, 6266, 989, 0, 0, 1),
    (60026, 4, 209, 'Pillars of Remembrance', 1033036, 1643776, 7832, 3891, 0, 0, 1),
    (60027, 4, 205, 'Covenant of Flame', 987829, 1635970, 12161, 375, 0, 0, 1),
    (60028, 4, 205, 'Drakebreaker\'s Scourge', 968865, 1636896, 8751, 2423, 0, 0, 1),
    (60029, 4, 203, 'Hatred\'s Way', 886888, 1637438, 6823, 1024, 0, 0, 1),
    (60030, 4, 203, 'Wrath\'s Resolve', 929708, 1637764, 9000, 910, 0, 0, 1)
ON DUPLICATE KEY UPDATE
    `RegionId` = VALUES(`RegionId`),
    `ZoneId` = VALUES(`ZoneId`),
    `Name` = VALUES(`Name`),
    `X` = VALUES(`X`),
    `Y` = VALUES(`Y`),
    `Z` = VALUES(`Z`),
    `O` = VALUES(`O`),
    `TokDiscovered` = VALUES(`TokDiscovered`),
    `TokUnlocked` = VALUES(`TokUnlocked`),
    `KeepSpawn` = VALUES(`KeepSpawn`);

UPDATE `keep_infos`
SET `GuildClaimObjectiveId` = CASE `KeepId`
    WHEN 5 THEN 60005 WHEN 6 THEN 60006 WHEN 7 THEN 60007 WHEN 8 THEN 60008
    WHEN 9 THEN 60009 WHEN 10 THEN 60010 WHEN 13 THEN 60013 WHEN 15 THEN 60015
    WHEN 16 THEN 60016 WHEN 17 THEN 60017 WHEN 18 THEN 60018 WHEN 19 THEN 60019
    WHEN 20 THEN 60020 WHEN 23 THEN 60023 WHEN 24 THEN 60024 WHEN 25 THEN 60025
    WHEN 26 THEN 60026 WHEN 27 THEN 60027 WHEN 28 THEN 60028 WHEN 29 THEN 60029
    WHEN 30 THEN 60030
END
WHERE `KeepId` IN (5, 6, 7, 8, 9, 10, 13, 15, 16, 17, 18, 19, 20, 23, 24, 25, 26, 27, 28, 29, 30)
  AND `GuildClaimObjectiveId` = 0;

COMMIT;
