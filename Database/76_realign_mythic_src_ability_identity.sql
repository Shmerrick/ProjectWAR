-- 76_realign_mythic_src_ability_identity.sql
--
-- Realigns the client-facing identity columns of mythic_src_abilities -- Name, EffectID, IconId --
-- with the client. The mechanics in that table are already correct; only the identity block is
-- misaligned, and the running server reads this table.
--
-- WHY THIS MATTERS. World.xml ships UseMythicActionCoverageTables = true, and
-- AbilityMgr.LoadNewAbilityInfo then loads mythic_src_abilities instead of abilities
-- (WorldServer/World/Abilities/AbilityMgr.cs:112). Everything the server knows about an ability at
-- runtime comes from that row. EffectID is not internal bookkeeping: it is written straight into
-- the cast packets (AbilityProcessor.cs:432, 945, 1074, 1094 and AbilityInterface.cs:696), so a
-- wrong value tells the client to play the wrong visual for the ability being cast. Name is what
-- every log line, GM command and future investigation reads, so a wrong one silently misleads the
-- next person who looks.
--
-- WHAT IS ACTUALLY WRONG. Comparing the two server tables entry-for-entry:
--
--   4221 entries exist in both; 4195 more exist only in mythic_src_abilities; none only in
--   abilities. Of the 4221 shared entries, 3214 are byte-identical. The other 1007 differ in
--   exactly three columns and no others -- Name (1007), EffectID (1000), IconId (984). Every
--   mechanical column agrees on all 4221: CareerLine, MinRange, Range, CastTime, Cooldown, ApCost,
--   AbilityType, MasteryTree, Specline, MinimumRank, the cast flags, all of it. (MinimumRenown
--   differs on 4 rows and is left alone; it is not part of the identity block.)
--
-- WHICH SIDE IS RIGHT. The client decides, per CLAUDE.md hard rule 3. Two independent columns were
-- checked against mythic_bin_ability over the 1007 divergent rows:
--
--   Name     abilities matches the client 918 times, mythic_src_abilities 0 times (46 rows have
--            no client name to check).
--   EffectID abilities matches the client 435 times, mythic_src_abilities 2 times.
--
-- Nothing about that is ambiguous: mythic_src_abilities is wrong on every one of them. The visible
-- shape of the error is a shifted name column -- src entry 7 carries "Death From Above", which is
-- the client's name for 6; src 8 carries "Spine Fling", the client's 7 -- but the shift is not one
-- global offset (testing client ID = Entry +/- 1 and +/- 2 across the whole table gains nothing over
-- Entry itself), so it cannot be undone arithmetically. It is repaired per row from the table that
-- agrees with the client.
--
-- The 307 rows where abilities itself differs from the client are a different thing and are not
-- touched: they are trailing whitespace ("Gut Ripper ") and deliberate emulator disambiguation
-- ("Vehement Blades Self AP", "Gift of Brutality Proc", "Kiss of Agony Buff") on rows that are
-- otherwise the right ability. That is annotation, not misalignment.
--
-- SCOPE, AND WHAT IS DELIBERATELY LEFT OUT.
--
--   Section A repairs the 1007 shared rows from abilities.
--
--   Section B repairs Name only on the 2140 src-only rows whose name disagrees with a non-empty
--   client name -- and Name only. All 4195 src-only rows carry CareerLine 0, so none of them is
--   granted to a player career; they are creature and world abilities, and every one has a real
--   client row at its id, so the server is mislabelling real abilities rather than inventing ids.
--   Their EffectID is left as it stands: abilities and the client agree on EffectID for only about
--   93% of the rows they otherwise agree on completely, so some server EffectID values are
--   deliberate and there is no second column here to corroborate a rewrite against. Fixing those
--   needs its own evidence; see docs/ABILITY_TABLE_ALIGNMENT.md.
--
--   The remaining 2051 src-only rows have no client name at all and are left untouched.
--
-- Both tables are written where they overlap, per CLAUDE.md hard rule 1 -- here that falls out
-- naturally, because abilities is the source and is already correct.
--
-- Ability data is cached at boot by AbilityMgr, so the server must be restarted for this to take
-- effect.

START TRANSACTION;

-- ---------------------------------------------------------------------------
-- A. Shared entries: take the identity block from abilities.
-- ---------------------------------------------------------------------------

UPDATE mythic_src_abilities m
  JOIN abilities a ON a.Entry = m.Entry
   SET m.Name     = a.Name,
       m.EffectID = a.EffectID,
       m.IconId   = a.IconId
 WHERE NOT (m.Name     <=> a.Name)
    OR NOT (m.EffectID <=> a.EffectID)
    OR NOT (m.IconId   <=> a.IconId);

-- ---------------------------------------------------------------------------
-- B. Entries that exist only in mythic_src_abilities: take the name from the client.
-- ---------------------------------------------------------------------------

UPDATE mythic_src_abilities m
  JOIN mythic_bin_ability b ON b.ID = m.Entry
  LEFT JOIN abilities a ON a.Entry = m.Entry
   SET m.Name = b.Name
 WHERE a.Entry IS NULL
   AND b.Name <> ''
   AND NOT (m.Name <=> b.Name);

COMMIT;
