-- 70_thanquol_gold_bag_rewards.sql
--
-- Wires the Doomflayer and Warpforged rank-40 sets into Thanquol's Incursion (public quest 911)
-- as gold-bag rewards, and switches the quest to the loot-bag type that actually awards them.
--
-- WHY THE PQType CHANGES. Migration 68 set PQType 0 deliberately, because the only reward the
-- captures show is the Warpstone token looted from each boss, and PQType 0 rolls no loot bag.
-- That was too narrow: the Doomflayer and Warpforged sets are this encounter's reward gear, they
-- already exist in the database (332 items, entries 5757300-5757778, all Rarity 5 and MinRank 40)
-- and nothing anywhere drops them -- no row in pquest_loot, loot_group_items or gameobject_loots.
--
-- PQType 2 is the right type rather than 1, for a reason that matters:
--   * GoldChest.GenerateLootBags case 2 guarantees one gold bag and scales the rest with player
--     count, which is the shape a raid encounter wants.
--   * Case 1 casts (PublicQuestDifficulty)(PQDifficult - 1). PQ 911 has PQDifficult 0, because
--     that is what serialises the difficulty byte as 0xFF, which is what all six captured
--     F_OBJECTIVE_INFO packets carry. Under case 1 that cast would be (PublicQuestDifficulty)(-1).
--     Case 2 never reads PQDifficult, so the captured packet value and working bags can coexist.
--
-- Known consequence, left as-is rather than papered over: GoldChest computes coin as
-- 100 * PQDifficult * Chapter * bagtype, so with PQDifficult 0 this quest awards no coin from the
-- chest. Raising PQDifficult to fix that would break the captured difficulty byte, so the coin
-- formula is the thing to revisit, not the data.
--
-- BAG NUMBERING. pquest_loot.Bag is the bagtype GoldChest derives from the loot-bag item id:
-- 9940 Minor -> 1, 9941 Lesser -> 2, 9942 Greater -> 3, 9943 Major -> 4, 9980 Massive -> 5.
-- LootBagRarity maps those to White/Green/Blue/Purple/Gold, so **Bag 5 is the gold bag**.
--
-- MATCHING. GoldChest selects loot with
--   PQType = info.PQType AND (Career & (1 << careerLine-1)) <> 0 AND Bag = bagtype
--   AND (Chapter = info.Chapter OR PQTier = info.PQTier OR PQEntry = info.Entry)
-- so PQEntry 911 binds these rows to this quest alone. Career is copied from each item's own
-- bitmask, which already encodes the correct careers and therefore the correct realm.
--
-- Items live in two parallel tables and both hold all 332 rows, so the source table below is
-- only a lookup for Entry and Career; no item columns are written, and CLAUDE.md hard rule 1
-- does not require a second write here.

START TRANSACTION;

-- ---------------------------------------------------------------------------
-- 1. Let the quest roll loot bags at all.
-- ---------------------------------------------------------------------------

UPDATE pquest_info SET PQType = 2 WHERE Entry = 911;

-- ---------------------------------------------------------------------------
-- 2. Gold-bag contents: every Doomflayer and Warpforged armour/weapon piece.
--
-- Career <> 0 excludes the Type-36 crest, insignia and exchange-bag currency items that share
-- the set names; those are vendor currency, not chest rewards. What remains is 332 pieces, all
-- Rarity 5 at MinRank 40, which is gold-bag tier throughout.
-- ---------------------------------------------------------------------------

DELETE FROM pquest_loot WHERE PQEntry = 911;

INSERT INTO pquest_loot (ItemID, Career, Chapter, Bag, PQTier, PQType, PQEntry)
SELECT i.Entry, i.Career, 0, 5, 4, 2, 911
  FROM mythic_src_item_infos i
 WHERE (i.Name LIKE '%Doomflayer%' OR i.Name LIKE '%Warpforged%')
   AND i.Career <> 0;

COMMIT;
