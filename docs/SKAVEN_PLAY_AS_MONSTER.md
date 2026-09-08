# Play as Skaven — what works, what does not, and what is still unknown

Status as of 2026-09-07. **The forms are not working.** This records what the captures actually
show, corrects two claims that were asserted without evidence, and says what is still missing.

## What the feature is

Shipped in Game Update 1.4.0 (2 November 2011), maintained through 1.4.5. From the 1.4.0 notes:

> While controlling a Skaven troop, the player takes on the form, abilities, and stats of the
> Skaven. Access to inventory or character screens are disabled.

Four forms — Gutter Runner, Warlock Engineer, Packmaster, Rat Ogre — taken at an Excavated Skaven
Device in a contested Tier 4 lake.

## Two corrections

**1. The control abilities have no components.** It was previously stated here, in
`SkavenFormService`, in `Player.ApplySkavenForm` and in `Database/75_skaven_control_ability_buffs.sql`
that each of the eight "Controlled &lt;form&gt;" abilities (24857-24864) carries a component with
operation 51 — career ability-set replacement — and that applying one as a buff therefore makes the
client swap the action bar out of its own data.

That is false. In the client's own records all eight have empty `ComponentData` and **zero rows** in
`mythic_bin_abilitycomponentlink`. They have no components of any kind. The op-51 description that
was quoted belongs to component 26389, which is attached to something else entirely; it was never
checked against these entries.

So the mechanism by which the bar is replaced is **not known**, and migration 75's buffs do not
supply it.

**2. There is a Pack Master capture.** `SkavenFormService` records Pack Master's kit as unknown
because "no capture shows anyone playing one". The corpus contains
`CONTROL A PACK MASTER (DOK LVL 40 RR 100).log.txt.gz`. Its kit has not yet been diffed out of it.

## What the captures do establish

From `CONTROL A GUTTER RUNNER (DOK LVL 40 RR 100)` and `play as a gutter runner`:

- **The kit is granted by F_CHARACTER_INFO subcode 1, one ability at a time.** Each packet is three
  bytes longer than the last; the count climbs 0x45 → 0x4C in one capture and 0x17 → 0x1E in the
  other. Diffing consecutive packets yields the granted set exactly.
- **That list is cumulative and never shrinks.** Across the whole 261,000-line session the count
  only ever rises. The live server did not use it to take abilities away, so neither can we —
  `SetGrantedAbilities` no longer re-sends on clear.
- **A control buff is applied.** F_INIT_EFFECTS (0xD7):

  ```
  00 13 D7 01 01 4C 46 16 EC 21 00 1E 61 00 EC 16 02 01 02 03 02 00
              ^^^^^ ^^^^^ ^^^^^ ^^^^^    ^^^^^ ^^ ^^^^^^^^^^^
              ?     target buffId entry  caster lines
  ```

  `1E 61` is 24862 little-endian — "Destruction Controlled Gutter Runner". Zero duration. This part
  our server reproduces correctly.

Two fields we do **not** reproduce:

| Field | Live | Ours |
| --- | --- | --- |
| the unnamed `ushort` after the update opcode | `4C 46` (0x4C46) | `0` |
| BuffLines | 2 lines: `(1, 1)` and `(3, 1)` | none |

The emulator only knows that first field for three abilities (9005, 8090, 8551/8556/8560) and writes
0 for everything else. The BuffLines come from buff commands, and migration 75 created
`buff_infos` rows with **no** `buff_commands` at all — so our buff has no component chain. The
client's `ComponentTriggers` for 24862 read `1,3,10,1,6,...`, and lines 1 and 3 are exactly what the
live buff carries, so those two lines are almost certainly derived from the trigger list.

## Why nothing casts

**21 of the 22 form abilities have no row in either server ability table.** Only 24824 Snare Net
does. `AbilityMgr.GetAbilityInfo` returns null for the rest.

Until 2026-09-07 the server responded to that by returning `false` and sending nothing at all. The
client had already started its cast bar and was never told how the cast ended, so the bar hung
forever — and a client stuck mid-cast will not interact with anything, which is why objects, doors
and NPCs stopped responding until relog (BUG-132). `AbilityInterface.RejectCast` now sends the
F_USE_ABILITY failure and the F_UPDATE_STATE/CastCompletion that clears the bar, on every path where
the server declines a cast. That stops the lock-up; it does not make the abilities work.

## What is needed

1. **Restore the 21 missing ability rows** into `abilities` and `mythic_src_abilities` from
   `mythic_bin_ability`, with their component chains in `mythic_src_buff_commands`. Part of the
   larger gap: 20,590 client abilities have no server row, every one of them carrying component
   data. See `docs/ABILITY_TABLE_ALIGNMENT.md`.
2. **Find what actually swaps the bar.** Not an op-51 component — that has been ruled out. The
   remaining candidates are the unnamed `0x4C46` field, the two BuffLines, or a packet in the
   capture that has not been read yet. The full ordered packet sequence around the transformation in
   `CONTROL A GUTTER RUNNER` has not been walked end to end; that is the next thing to do.
3. **Diff the Pack Master capture** for the fourth kit.
4. Retail also disabled the inventory and character screens and changed the player's model. Neither
   is reproduced, and no packet for either has been identified.

## Current behaviour

`.skavenform <gutterrunner|engineer|ratogre|off>` grants or clears the kit server-side and applies
or removes the control buff. The abilities appear on the bar, cannot be cast (they now fail cleanly
instead of hanging), and **do not disappear when the form ends**. The device at Guid 2900001 in
Reikwald is test scaffolding and should be removed with the tome tactic test range.
