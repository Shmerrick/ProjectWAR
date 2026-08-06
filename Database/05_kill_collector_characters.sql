-- Kill Collector: per-character progress. Apply to the CHARACTERS database.
-- The definitions and targets live in the WORLD database - see
-- 04_kill_collector_world.sql. Applying either file to the wrong database
-- will create the tables where the server cannot find them.

-- Per-character progress. Lives in the Characters database, unlike the two above.
-- AccumulatedKills only ever grows; ClaimedKills tracks what has been paid out,
-- so an unclaimed balance is (min(Accumulated, Cap) - Claimed).

DROP TABLE IF EXISTS `characters_kill_collector`;

CREATE TABLE `characters_kill_collector` (
  `CharacterId`      int unsigned      NOT NULL,
  `CollectorEntry`   int unsigned      NOT NULL,
  `AccumulatedKills` int unsigned      NOT NULL DEFAULT 0,
  `ClaimedKills`     int unsigned      NOT NULL DEFAULT 0,
  `RewardClaimed`    tinyint unsigned  NOT NULL DEFAULT 0,
  PRIMARY KEY (`CharacterId`,`CollectorEntry`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
