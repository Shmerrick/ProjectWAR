-- Character-owned item deadlines, independent of consumable inventory rows.
-- Server continuity repair, not newly established retail timing/protocol data.
-- Evidence: AbilityInterface.SetItem[Group]Cooldown and Item.Load;
-- CharacterItem.NextAllowedUseTime is intentionally not ORM-mapped.
-- Apply before starting this build; no existing character/item rows are changed.
USE war_characters;
CREATE TABLE IF NOT EXISTS character_item_cooldowns (
    CharacterId INT UNSIGNED NOT NULL,
    CooldownKey INT UNSIGNED NOT NULL,
    ExpiresAtMilliseconds BIGINT NOT NULL DEFAULT 0,
    PRIMARY KEY (CharacterId, CooldownKey)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
