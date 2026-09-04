-- Restores the beginner help tip system.
--
-- A help tip is an ordinary Tome entry in section 101 (entries 11800-11999). The client raises
-- its HELP_TIP_UPDATED event when a ToK unlock arrives with a non-zero category byte, then
-- resolves the tip title and body from its HelpTipNames / HelpTipDescriptions string tables
-- using (entry - 11799). Nothing in the world database granted those entries, so no tip ever
-- appeared; every other unlock sent a hardcoded category byte, which is what produced the empty
-- tip window. This table binds each tip to the server event that unlocks it.
--
-- Entry selection is evidenced, not invented:
--   * Only tips present in the client's data/gamedata/helptips.csv are listed. That file's 131
--     rows match the 131 named section 101 rows in tok_infos for 130 entries.
--   * Every trigger below is taken from the tip's own text in data/strings/english/helptipdesc.txt
--     ("You've gained a Rank!", "This is a mailbox.", "You are now a Party Leader.", and so on).
--   * NpcInteract.TriggerValue is a GameData.CreatureTitle ordinal; TradeSkillLearned.TriggerValue
--     is the crafting/gathering skill id set by Creature.SendInteract.
--
-- TipType is the client tip category: 1 Beginner, 2 Gameplay, 3 UI, 4 Advanced. Neither the 1.4.8
-- client data nor tok_infos carries a per-tip category, so every row ships as Beginner (1) — the
-- setting the tip window itself offers. Change the column per row to move a tip under one of the
-- other three client toggles; nothing in the server depends on the value beyond passing it through.
--
-- Tips are unlocked through the normal Tome path, so each one is stored in characters_toks and
-- shown once per character. Set Enabled = 0 to retire a tip without deleting its row.
--
-- REPLACE makes the script safe to re-run.
USE `war_world`;

CREATE TABLE IF NOT EXISTS `help_tips` (
  `TokEntry`     SMALLINT UNSIGNED NOT NULL,
  `TipType`      TINYINT  UNSIGNED NOT NULL DEFAULT 1,
  `TriggerName`  VARCHAR(32)       NOT NULL DEFAULT '',
  `TriggerValue` INT      UNSIGNED NOT NULL DEFAULT 0,
  `MaxRank`      TINYINT  UNSIGNED NOT NULL DEFAULT 0,
  `Enabled`      TINYINT  UNSIGNED NOT NULL DEFAULT 1,
  PRIMARY KEY (`TokEntry`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

REPLACE INTO `help_tips` (`TokEntry`, `TipType`, `TriggerName`, `TriggerValue`, `MaxRank`, `Enabled`) VALUES
-- Character start. MaxRank keeps these off an existing high-rank character.
(11877, 1, 'Login',              0,  5, 1),  -- Movement
(11828, 1, 'Login',              0,  5, 1),  -- Combat
(11854, 1, 'Login',              0,  5, 1),  -- Health and Action Points
(11801, 1, 'Login',              0,  5, 1),  -- Action Bar
(11823, 1, 'Login',              0,  5, 1),  -- Chat
(11869, 1, 'Login',              0,  5, 1),  -- Menu
(11884, 1, 'Login',              0,  5, 1),  -- Quests

-- Progression
(11840, 1, 'XpGained',           0, 10, 1),  -- Experience Points
(11885, 1, 'RankUp',             2,  0, 1),  -- Rank 2
(11913, 1, 'RankUp',             2,  0, 1),  -- Tome of Knowledge
(11856, 1, 'RankUp',             2,  0, 1),  -- Help Window
(11824, 1, 'RankUp',             8,  0, 1),  -- Chat II
(11846, 1, 'RankUp',             8,  0, 1),  -- Friends
(11819, 1, 'RankUp',            11,  0, 1),  -- Career Mastery
(11875, 1, 'RankUp',            20,  0, 1),  -- Mounts
(11889, 1, 'RenownGained',       0,  0, 1),  -- Renown
(11892, 1, 'RenownRankUp',       2,  0, 1),  -- Renown Rank 2
(11890, 1, 'RenownRankUp',       3,  0, 1),  -- Renown Items

-- Items and money
(11867, 1, 'Loot',               0, 20, 1),  -- Looting
(11847, 1, 'MoneyGained',        0, 20, 1),  -- Money

-- Dying
(11836, 1, 'Death',              0,  0, 1),  -- Death
(11852, 1, 'Death',              0,  0, 1),  -- Healers

-- Quests
(11882, 1, 'QuestCompleted',     0,  0, 1),  -- Quest Completed
(11883, 1, 'QuestCompleted',     0,  0, 1),  -- Quest Tracker

-- Parties
(11848, 1, 'GroupJoined',        0,  0, 1),  -- Parties
(11922, 1, 'GroupLeader',        0,  0, 1),  -- Party Leader
(11812, 1, 'WarbandFormed',      0,  0, 1),  -- Warbands

-- RvR
(11896, 1, 'RvrFlagged',         0,  0, 1),  -- RvR I
(11825, 1, 'RvrFlagged',         0,  0, 1),  -- Chickens
(11811, 1, 'RvrAreaEntered',     0,  0, 1),  -- Battlefield Objectives
(11865, 1, 'RvrAreaEntered',     0,  0, 1),  -- Keeps
(11921, 1, 'RvrAreaEntered',     0,  0, 1),  -- Zone Control
(11919, 1, 'RvrAreaEntered',     0,  0, 1),  -- Victory Points
(11900, 1, 'ScenarioJoined',     0,  0, 1),  -- Scenarios

-- Chapters and public quests
(11821, 1, 'ChapterEntered',     0,  0, 1),  -- Chapter Hubs
(11881, 1, 'PublicQuestEntered', 0,  0, 1),  -- Public Quests
(11822, 1, 'PublicQuestEntered', 0,  0, 1),  -- Chapters
(11879, 1, 'PublicQuestBag',     0,  0, 1),  -- PQ loot
(11880, 1, 'PublicQuestBag',     0,  0, 1),  -- PQ Scoreboard
(11859, 1, 'InfluenceReward',    0,  0, 1),  -- Influence rewards

-- Mail
(11936, 1, 'MailboxUsed',        0,  0, 1),  -- Mail

-- NPCs. TriggerValue is the GameData.CreatureTitle ordinal.
(11904, 1, 'NpcInteract',        1,  0, 1),  -- Skill Trainer      <- Trainer
(11939, 1, 'NpcInteract',        2,  0, 1),  -- Trainer NPCs       <- CareerTrainer
(11870, 1, 'NpcInteract',       10,  0, 1),  -- Merchants          <- Merchant
(11901, 1, 'NpcInteract',       14,  0, 1),  -- Siege Merchant     <- SiegeWeaponMerchant
(11891, 1, 'NpcInteract',       16,  0, 1),  -- Renown Merchant    <- RenownGearMerchant
(11924, 1, 'NpcInteract',       17,  0, 1),  -- Rally Master NPCs  <- RallyMaster
(11818, 1, 'NpcInteract',       18,  0, 1),  -- Flight Masters     <- FlightMaster
(11849, 1, 'NpcInteract',       19,  0, 1),  -- Guild Registrar    <- GuildRegistrar
(11851, 1, 'NpcInteract',       19,  0, 1),  -- Guilds             <- GuildRegistrar
(11810, 1, 'NpcInteract',       22,  0, 1),  -- Banks              <- Banker
(11807, 1, 'NpcInteract',       23,  0, 1),  -- Auction House      <- Auctioneer
(11814, 1, 'NpcInteract',       32,  0, 1),  -- Kill Collector     <- KillCollector

-- Trade skills. TriggerValue is the skill id assigned by Creature.SendInteract.
(11815, 1, 'TradeSkillLearned',  1,  0, 1),  -- Butchering       <- GatheringSkill 1
(11898, 1, 'TradeSkillLearned',  2,  0, 1),  -- Scavenging       <- GatheringSkill 2
(11835, 1, 'TradeSkillLearned',  3,  0, 1),  -- Cultivating      <- GatheringSkill 3
(11805, 1, 'TradeSkillLearned',  4,  0, 1),  -- Apothecary       <- CraftingSkill 4
(11909, 1, 'TradeSkillLearned',  5,  0, 1),  -- Talisman Making  <- CraftingSkill 5
(11897, 1, 'TradeSkillLearned',  6,  0, 1);  -- Salvaging        <- GatheringSkill 6
