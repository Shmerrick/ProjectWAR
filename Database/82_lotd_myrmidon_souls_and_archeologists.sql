-- 82_lotd_myrmidon_souls_and_archeologists.sql
--
-- Repairs the corrupted Demon Myrmidon's Soul row and puts the eight Land of the Dead soul
-- talismans on the two archeologist vendors, priced in Golden Scarabs.
--
-- ============================================================================
-- WHAT WAS ALREADY HERE
-- ============================================================================
--
-- All of it, as it turns out, except the bindings. The eight souls are item_infos 2005595-2005602
-- with their large-weapon twins at 2005663-2005670, carrying the exact tooltip text from the live
-- reward window -- "This talisman can only be used with a normal vessel weapon. CTL+Right Click to
-- convert between normal or large weapon versions" -- and the right stat on each. The currencies
-- exist too: 208408 Silver Scarab, 208409 Golden Scarab, 208410 Silver Ankh, 208411 Golden
-- Cartouche, 208414 Cluster of Golden Scarabs, 208415 Fused Cluster of Golden Cartouches. So do the
-- achievements (tok_infos 7514-7519 "Pyramid Purged"/"Defend Against Purge", 7500/7501 for all
-- lairs, 7764) and the titles (10902 Tomb Purger, 10911 Purge Master, 10912 The Unpurgeable).
--
-- THE STAT MAPPING, resolved. Five are confirmed directly by the live reward window; the other
-- three come from the Stats column of the Massive twins, which are not corrupted:
--
--   2005595 Demon        Stats 1  Strength       (from Massive Demon 2005663, "1:64")
--   2005596 Indominable  Stats 3  Willpower      confirmed on screen
--   2005597 Iron         Stats 4  Toughness
--   2005598 Conquering   Stats 5  Wounds
--   2005599 Alacritous   Stats 6  Initiative     confirmed on screen
--   2005600 Masterful    Stats 7  Weapon Skill   confirmed on screen
--   2005601 Trueshot     Stats 8  Ballistic Skill confirmed on screen
--   2005602 Omnipotent   Stats 9  Intelligence   confirmed on screen
--
-- Eight souls for the eight stats WAR actually uses -- 2 (Agility) is vestigial, which is why there
-- is no ninth. Note that Demon is STRENGTH, stat 1, the first rather than the last; Conquering is
-- Wounds.
--
-- ============================================================================
-- SECTION A. The corrupted Demon row
-- ============================================================================
--
-- 2005595 is damaged identically in both item tables:
--
--   Name         "mon Myrmidon's Soul"   truncated; the Massive twin reads "Massive Demon
--                                        Myrmidon's Soul", so the lost prefix is "De"
--   Description  empty                   its seven siblings all carry the talisman text
--   Stats        771 characters of text-shaped garbage (116:8259;116:28448;114:29728;...) rather
--                than the "1:48;0:0;..." its Massive twin's "1:64" implies
--   Bind         0                       siblings are 1 (bind on pickup, as the tooltip says)
--   MaxStack     1                       siblings are 20
--   SellPrice    20                      siblings are 0
--
-- Rebuilt from 2005596 for the shared text and from the Massive Demon twin for the identity, with
-- the stat set to Strength 48 to match the +48 every normal-vessel soul carries. Type is left
-- alone: the eight rows disagree with each other (23, 0, 0, 0, 0, 0, 0, 31) and nothing here
-- establishes which is right, so that is recorded rather than guessed at.
--
-- Both item tables are written, per CLAUDE.md hard rule 1 -- the server reads mythic_src_item_infos.
--
-- ============================================================================
-- SECTION B. The archeologist vendors
-- ============================================================================
--
-- The Golden Cartouche tooltip names them outright: "These may be traded to archeologists studying
-- Nehekhara for powerful supplies and equipment." Both are already spawned in zone 191, one per
-- warcamp, and each is identified by two independent signals -- faction and position:
--
--   93636 Archeologist Bergmann             faction 65  Order        Goldbarrow    (59670, 46918)
--   93656 Archeologist Sveinn Ravensight^M  faction 129 Destruction  Da Dusty Dry  (56803, 9019)
--
-- Faction 65 is Order and 129 Destruction, verified against the capitals: Altdorf holds 410
-- faction-65 NPCs, the Inevitable City 612 faction-129.
--
-- They currently share VendorID 1 with 213 other creatures, so their stock CANNOT go there -- it
-- would appear on 215 unrelated vendors across the world. New lists 453 and 454 are created and the
-- two creatures repointed; 452 was the previous maximum in both vendor_items and creature_protos.
--
-- Each list carries all sixteen souls, normal and Massive, since the tooltip says a talisman
-- converts between the two forms and both realms use the same stats.
--
-- >>> THE PRICE IS NOT ESTABLISHED <<<
-- Nothing in vendor_items is priced in any Land of the Dead currency, so there is no scale to copy,
-- and no capture or client file fixes what a soul cost. The two numbers below are placeholders
-- chosen to be replaced, not evidence:
--
--   normal vessel   5 Golden Scarabs
--   large vessel   10 Golden Scarabs
--
-- ReqItems format is "(count,itemId)", matching the existing rows, e.g. "(20,208470)".

START TRANSACTION;

-- ---------------------------------------------------------------------------
-- A. Rebuild the Demon Myrmidon's Soul row in both tables.
-- ---------------------------------------------------------------------------

SET @soulText = (SELECT Description FROM item_infos WHERE Entry = 2005596);
SET @demonStats = '1:48;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;';

UPDATE item_infos
   SET Name        = 'Demon Myrmidon\'s Soul',
       Description = @soulText,
       Stats       = @demonStats,
       Bind        = 1,
       MaxStack    = 20,
       SellPrice   = 0
 WHERE Entry = 2005595;

UPDATE mythic_src_item_infos
   SET Name        = 'Demon Myrmidon\'s Soul',
       Description = @soulText,
       Stats       = @demonStats,
       Bind        = 1,
       MaxStack    = 20,
       SellPrice   = 0
 WHERE Entry = 2005595;

-- ---------------------------------------------------------------------------
-- B. Stock the two archeologists.
-- ---------------------------------------------------------------------------

DELETE FROM vendor_items WHERE VendorId IN (453, 454);

INSERT INTO vendor_items (ItemGuid, VendorId, ItemId, Price, ReqTokUnlock, ReqGuildlvl, ReqItems)
SELECT 0, v.VendorId, s.ItemId, 0, 0, 0, s.ReqItems
  FROM (SELECT 453 AS VendorId UNION ALL SELECT 454) AS v
  JOIN (
        SELECT 2005595 AS ItemId, '(5,208409)'  AS ReqItems UNION ALL
        SELECT 2005596, '(5,208409)'  UNION ALL
        SELECT 2005597, '(5,208409)'  UNION ALL
        SELECT 2005598, '(5,208409)'  UNION ALL
        SELECT 2005599, '(5,208409)'  UNION ALL
        SELECT 2005600, '(5,208409)'  UNION ALL
        SELECT 2005601, '(5,208409)'  UNION ALL
        SELECT 2005602, '(5,208409)'  UNION ALL
        SELECT 2005663, '(10,208409)' UNION ALL
        SELECT 2005664, '(10,208409)' UNION ALL
        SELECT 2005665, '(10,208409)' UNION ALL
        SELECT 2005666, '(10,208409)' UNION ALL
        SELECT 2005667, '(10,208409)' UNION ALL
        SELECT 2005668, '(10,208409)' UNION ALL
        SELECT 2005669, '(10,208409)' UNION ALL
        SELECT 2005670, '(10,208409)'
       ) AS s;

UPDATE creature_protos SET VendorID = 453 WHERE Entry = 93636;  -- Order, Goldbarrow
UPDATE creature_protos SET VendorID = 454 WHERE Entry = 93656;  -- Destruction, Da Dusty Dry

COMMIT;
