-- 71_fix_metal_construct_kill_tiers.sql
--
-- Corrects the Metal Construct (Creature_Sub_Type 137) bestiary kill milestones, which are
-- shifted by one because a non-kill task is interleaved among them in the client's entry order.
--
-- The Tome entries for this species run:
--
--   4350  "You have encountered a Living Armor"                    <- Kill1
--   4351  "You have killed 25 Living Armors"                       <- Kill25
--   4352  "You have completed: Anyone Have a Can Opener?"          <- a TASK, not a kill tier
--   4353  "You have killed 100 Living Armors"                      <- Kill100
--   4354  "You have killed 1,000 Living Armors"                    <- Kill1000
--   4355  "You have killed 10,000 Living Armors"                   <- Kill10000
--
-- The stored row assumed the six tiers were consecutive and so absorbed the task at 4352,
-- pushing everything after it down a slot:
--
--   Kill100  = 4352  (awards the "Can Opener" task for 100 kills)
--   Kill1000 = 4353  (awards the 100-kill entry for 1,000 kills)
--   Kill10000 = 4355 (correct, because the shift ends where the sequence resynchronises)
--
-- Two consequences. Killing Living Armors awarded the wrong Tome entry at two milestones, and
-- **4354 was awarded by nothing at all** - it is a Man-line tome tactic fragment, so that line
-- could earn only 3 of the 4 fragments its second tactic requires. This fix is what makes
-- "Boon of Tenacity" obtainable (see BUG-117 and docs/TOME_TACTIC_FRAGMENTS.md).
--
-- Scope: this is the ONLY affected species. A check across all 131 counted bestiary rows,
-- comparing each milestone column against the "killed N" text of the entry it points at, finds
-- exactly one mismatched Kill100 and one mismatched Kill1000, both on this row. 4352 remains
-- unreferenced by any kill milestone, which is correct - it is a completion task and needs its
-- own trigger, which this database does not yet have for any of the 20 task-style fragments.
--
-- Kill10000 keeps its ";10846" suffix: the column holds a semicolon-separated list and the
-- loader reads the first id, so the trailing entry is preserved exactly as stored.

START TRANSACTION;

UPDATE tok_bestiary
   SET Kill100  = '4353',
       Kill1000 = '4354'
 WHERE Creature_Sub_Type = 137
   AND Kill100  = '4352'
   AND Kill1000 = '4353';

COMMIT;
