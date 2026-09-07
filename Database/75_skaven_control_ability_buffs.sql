-- 75_skaven_control_ability_buffs.sql
--
-- Adds the eight Skaven "Controlled <form>" abilities as buffs, so taking a monster form can
-- apply one and the client performs the action-bar swap itself.
--
-- WHY THIS IS THE MECHANISM. Component operation 51 is career ability-set replacement -- the
-- client's own description of it reads "Normal career abilities have been replaced with those of
-- the Aspect's" (component 26389). Each of these eight abilities carries an op-51 component
-- naming the form to switch to, and all eight resolve to effect 4860, "Skaven PaM - FORM OF... A
-- SKAVEN!". The swap is therefore performed by the CLIENT, out of its own ability data, when the
-- ability is applied to the player. The server only has to apply it.
--
-- That is why granting the form's abilities alone was not enough: F_CHARACTER_INFO subcode 1 is
-- cumulative -- the "play as a gutter runner" capture shows the list growing 23 -> 30 as the kit
-- is added, never shrinking or being replaced -- so it can add actions but cannot swap a bar or
-- take actions away. The bar swap and its reversal both hang off this buff.
--
-- REALM SPLIT. The eight are four forms times two realms, which the names state outright:
--
--   Order        24857 Warlock Engineer  24858 Gutter Runner  24859 Rat Ogre  24860 Pack Master
--   Destruction  24861 Warlock Engineer  24862 Gutter Runner  24863 Rat Ogre  24864 Pack Master
--
-- Names are read from mythic_bin_ability, which carries all eight from the client.
--
-- FriendlyEffectID is left NULL rather than set to 4860. That column is a tinyint holding a
-- small visual-effect index, not an effects.csv id, so 4860 does not belong in it. The bar swap
-- does not depend on it: the client resolves the op-51 component from the ability entry itself,
-- which is the buff's Entry.
--
-- Duration is NULL, i.e. indefinite: the form ends when the buff is removed, which
-- Player.RemoveSkavenForm does on death, on leaving the region, and on ending the form at a
-- device. PersistsOnDeath is 0 to match the captured accept text, "control a Warlock Engineer
-- until its death".
--
-- Buffs live in two parallel tables and the server reads only one of them
-- (UseMythicActionCoverageTables is true in the shipped World.xml, so AbilityMgr loads
-- mythic_src_buff_infos). Writing one table only is invisible at runtime with no error anywhere;
-- that cost a debugging session on the tome tactics (BUG-120). Both are written here.

START TRANSACTION;

DELETE FROM buff_infos            WHERE Entry BETWEEN 24857 AND 24864;
DELETE FROM mythic_src_buff_infos WHERE Entry BETWEEN 24857 AND 24864;

INSERT INTO buff_infos
    (Entry, Name, BuffClassString, TypeString, `Group`, AuraPropagation, MaxCopies, MaxStack,
     UseMaxStackAsInitial, StackLine, StacksFromCaster, Duration, LeadInDelay, `Interval`,
     PersistsOnDeath, CanRefresh, FriendlyEffectID, EnemyEffectID, Silent)
VALUES
    (24857, 'Order Controlled Warlock Engineer',       'Tactic', NULL, NULL, NULL, 1, 1, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL),
    (24858, 'Order Controlled Gutter Runner',          'Tactic', NULL, NULL, NULL, 1, 1, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL),
    (24859, 'Order Controlled Rat Ogre',               'Tactic', NULL, NULL, NULL, 1, 1, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL),
    (24860, 'Order Controlled Pack Master',            'Tactic', NULL, NULL, NULL, 1, 1, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL),
    (24861, 'Destruction Controlled Warlock Engineer', 'Tactic', NULL, NULL, NULL, 1, 1, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL),
    (24862, 'Destruction Controlled Gutter Runner',    'Tactic', NULL, NULL, NULL, 1, 1, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL),
    (24863, 'Destruction Controlled Rat Ogre',         'Tactic', NULL, NULL, NULL, 1, 1, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL),
    (24864, 'Destruction Controlled Pack Master',      'Tactic', NULL, NULL, NULL, 1, 1, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL);

INSERT INTO mythic_src_buff_infos
    (Entry, Name, BuffClassString, TypeString, `Group`, AuraPropagation, MaxCopies, MaxStack,
     UseMaxStackAsInitial, StackLine, StacksFromCaster, Duration, LeadInDelay, `Interval`,
     PersistsOnDeath, CanRefresh, FriendlyEffectID, EnemyEffectID, Silent)
SELECT Entry, Name, BuffClassString, TypeString, `Group`, AuraPropagation, MaxCopies, MaxStack,
       UseMaxStackAsInitial, StackLine, StacksFromCaster, Duration, LeadInDelay, `Interval`,
       PersistsOnDeath, CanRefresh, FriendlyEffectID, EnemyEffectID, Silent
  FROM buff_infos
 WHERE Entry BETWEEN 24857 AND 24864;

COMMIT;
