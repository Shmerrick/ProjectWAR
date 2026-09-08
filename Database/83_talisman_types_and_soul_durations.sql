-- 83_talisman_types_and_soul_durations.sql
--
-- Makes the soul talismans socketable, and gives the Land of the Dead ones the eight-hour decay the
-- client already displays.
--
-- ============================================================================
-- A. 305 talismans cannot be socketed because of their Type
-- ============================================================================
--
-- Item.AddTalisman refuses anything whose Type is not 23:
--
--     if (info.Type != 23)
--         return false;
--
-- 23 is ITEMTYPES_ENHANCEMENT, which is what a talisman is. Of the 363 items carrying the talisman
-- description -- "This talisman can only be used with a normal/large vessel weapon. CTL+Right Click
-- to convert between normal or large weapon versions" -- only 58 have it. 301 are Type 0
-- (ITEMTYPES_NONE) and 4 are Type 31 (ITEMTYPES_POTION). None of those 305 can be put into a
-- weapon at all, silently: AddTalisman just returns false.
--
-- Four independent things agree that 23 is right for these rows:
--
--   * the item's own description calls it a talisman, and the live tooltip labels it "Talisman";
--   * the soul talismans that were never damaged -- 2005247 Violent, 2005497 Resolute -- are 23;
--   * 2005595 Demon kept Type 23 through the corruption that destroyed its name, description,
--     Bind, MaxStack and Stats, so 23 is what the row started as;
--   * AddTalisman accepts nothing else, so any other value makes the item inert.
--
-- Scoped to rows carrying the talisman description, which is self-evidencing. Items that merely
-- look talisman-ish by name are left alone.
--
-- ============================================================================
-- B. The eight-hour duration
-- ============================================================================
--
-- A stat entry may carry four fields rather than two, "type:value:0:seconds", and the fourth is how
-- long the bonus lasts once socketed. 4,882 items use the long form; all but 61 leave it zero, and
-- the ones that do not are talismans -- 28800 on the eight-hour souls, 43200 on the twelve-hour.
--
-- Item_Info's Stats parser read only the first two fields and discarded the rest, so the duration
-- never reached the server at all; that is fixed in code alongside this script, together with the
-- timer being written as 0 when a talisman was socketed and the wire field that carries it to the
-- client always being 0 as a result.
--
-- The sixteen Land of the Dead souls use the short two-field form and so carry no duration, even
-- though every one of their live tooltips reads "Duration: 8h". Rewritten here to the long form
-- with 28800, preserving each row's own stat and value:
--
--   2005595-2005602  normal vessel, +48    2005663-2005670  large vessel, +64
--
-- The large-vessel twins are given the same eight hours. The captured tooltips are all of
-- normal-vessel souls, and the two forms are the same talisman -- the description says CTL+Right
-- Click converts between them -- so a different duration would be the odd claim. Recorded as
-- inference, not capture.
--
-- Both item tables are written throughout, per CLAUDE.md hard rule 1: the server reads
-- mythic_src_item_infos. Item data is cached at boot, so a restart is required.

START TRANSACTION;

-- ---------------------------------------------------------------------------
-- A. Type
-- ---------------------------------------------------------------------------

UPDATE item_infos
   SET Type = 23
 WHERE Description LIKE 'This talisman can only be used%'
   AND Type <> 23;

UPDATE mythic_src_item_infos
   SET Type = 23
 WHERE Description LIKE 'This talisman can only be used%'
   AND Type <> 23;

-- ---------------------------------------------------------------------------
-- B. Duration, keeping each row's own stat and value.
--
-- "3:48;0:0;0:0;..." becomes "3:48:0:28800;", matching the shape the intact eight-hour souls
-- already use ("1:37:0:28800;") rather than the padded form.
-- ---------------------------------------------------------------------------

UPDATE item_infos
   SET Stats = CONCAT(SUBSTRING_INDEX(Stats, ':', 1), ':',
                      SUBSTRING_INDEX(SUBSTRING_INDEX(Stats, ';', 1), ':', -1),
                      ':0:28800;')
 WHERE (Entry BETWEEN 2005595 AND 2005602 OR Entry BETWEEN 2005663 AND 2005670)
   AND Stats NOT LIKE '%:0:28800;%';

UPDATE mythic_src_item_infos
   SET Stats = CONCAT(SUBSTRING_INDEX(Stats, ':', 1), ':',
                      SUBSTRING_INDEX(SUBSTRING_INDEX(Stats, ';', 1), ':', -1),
                      ':0:28800;')
 WHERE (Entry BETWEEN 2005595 AND 2005602 OR Entry BETWEEN 2005663 AND 2005670)
   AND Stats NOT LIKE '%:0:28800;%';

COMMIT;
