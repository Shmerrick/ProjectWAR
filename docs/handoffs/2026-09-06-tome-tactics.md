# Tome tactics — 2026-09-06

Status: **fragment/unlock chain works end to end. The librarian purchase window does not open.**
One known blocker remains, identified but not yet fixed.

Read this before touching tome tactics again. It records what is proven, what is ruled out, and
what is still unknown, so the same ground is not re-covered.

---

## 1. What is proven working

Verified against the live Release database for character 233 (Bigboy, Destruction, IC):

| Step | State | Evidence |
|---|---|---|
| Bestiary kill counters | works | migration 54; 136 distinct client counters |
| Kill milestone -> Tome entry | works | `TokInterface.AddKill` |
| Tome entry -> fragment counter | works | all 9 counters populated: 24/32/16/5/18/12/6/3/22 |
| Fragment counter -> tier unlock | works | all 27 Section 26 toks held (6200-6226) |
| Counters pushed to client | works | `SendTomeTacticCounters` in `Player.OnLoad` |
| Advance packets built | works | byte-identical to live capture, see §3 |
| Advance packets sent | works | log: `Sent 27 tome tactic advances (category 16) to Bigboy; first=15100 last=15126` |
| Librarian offers the option | works | "Buy Tome Abilities" appears on Disciple Voidlost |
| **Training window opens** | **BROKEN** | see §2 |

## 2. The blocker — found and fixed 2026-09-06 22:40

`interactiontraining.lua` shows any training UI **only** on the client event
`SystemData.Events.INTERACT_SHOW_TRAINING`, which the engine raises from a server packet we were
never sending. The advance data alone can never open the window.

Found in the corpus. The live server answers the client's `F_INTERACT` with:

```
F_INTERACT_RESPONSE (0xE9), payload:  05 29 tt 00 00
   +0  5     "show training"
   +1  0x29  constant
   +2  tt    trainer type
   +3,+4     zero
```

Two independent confirmations, both immediately following a client `F_INTERACT`:

| Capture | Payload | Byte +2 |
|---|---|---|
| `renownpoint attribution.txt.gz` @197 | `05 29 08 00 00` | 8 = `InteractTrainerType.Renown` |
| `2013-09-25-SORC40RR84_RvR_inevitablecity_sieges.txt.gz` @25164 | `05 29 06 00 00` | 6 = CareerCore\|CareerMastery, exactly what `CreatureService` assigns career trainers |

Implemented as `Creature.SendShowTraining`, called from `SendTomeTacticList` with trainer type 16.

**This very likely fixes the career trainer too**, which has never opened for the same reason.
Untested — the career trainer path is separate (`case 6`) and was not touched.

## 3. Previous analysis of the blocker (superseded by §2)

`interactiontraining.lua` shows the tome training window **only** in response to the client event
`SystemData.Events.INTERACT_SHOW_TRAINING`:

```lua
function EA_Window_InteractionTraining.Initialize()
    RegisterEventHandler( SystemData.Events.INTERACT_SHOW_TRAINING, "EA_Window_InteractionTraining.Show")
```

`Show()` then dispatches on `InteractionUtils.GetLastRequestedTrainingType()`, and for
`InteractTrainerType.TOME` calls `EA_Window_InteractionTomeTraining.Show()`.

That event is raised by the client engine from a **server packet we never send**. Sending the
advance data alone can never open the window, no matter how correct it is.

`F_INTERACT_RESPONSE` (0xE9) carries a window type in its first payload byte. Known values from
this codebase: `0` menu, `16` barbershop, `19` healer, `0x1A`, `0x1B` dye, `0x1D`. The value that
raises `INTERACT_SHOW_TRAINING` is **not yet identified**.

**Next step:** find a capture in the official corpus containing an actual trainer visit (career,
renown or tome) and read the `F_INTERACT_RESPONSE` type byte the live server sends after the
client selects a training option. The "Inevitable City Shaman" capture does not contain one — all
of its `F_CAREER_CATEGORY` bursts are preceded by `S_PLAYER_INITTED`, i.e. they are login/zone
bursts, not trainer visits. Its `F_INTERACT_RESPONSE` types are only 7, 8, 9 and 12.

Corollary worth knowing: **no trainer window in this emulator has ever been verified to open.**
The career trainer path (`Creature.cs` case 6/43, added by an earlier AI change) has the same gap.
Its comment claims the packets "mirror the mastery format which is proven to work client-side" —
that is true of the mastery UI, which opens by a different route, not of the interaction window.

## 3. Packet layout — settled, do not re-derive

The advance packets are **byte-identical to the live 1.4.8 server**. Verified offline by
`tools/validation/Test-TomeTacticPackets.ps1`, which builds them with the real `PacketOut` and
diffs against bytes captured from
`WAR-RE-Toolkit/libs/protocolservices/Packet Logs/Inevitable City Shaman 40 94 Defense.txt.gz`
(that capture contains the live server's own category-16 exchange for these same 27 tactics):

```
F_CAREER_CATEGORY (category 16): matches (92 payload bytes)
F_CAREER_PACKAGE_INFO (package 1): matches (78 payload bytes)
```

Payload layout of `F_CAREER_PACKAGE_INFO`, offsets payload-relative:

| Offset | Meaning |
|---|---|
| +0 | category, 16 (`CAREERCATEGORY_TOME_CC_A`) |
| +3 | package index, 1-based; echoed back on purchase |
| +14 | Section 26 Tome entry (6199 + package) |
| +27 | advance id (809 + package) |
| +32 | ability entry (15099 + package) |
| +36 | `EffectID` (1861/1862/1864/1865/1866/1867/1868/1869/1870, one per line) |
| +52 | Pascal name |
| tail | prerequisite count, then prereqs |

Timing is also correct: live sends the burst after `S_PLAYER_INITTED`; we send at
`Player.cs` line ~963, between `SendInited()` (837) and `SendInitComplete()` (993).

**If the harness ever reports a two-byte offset, it is the test's own frame configuration** —
`PacketOut` defaults to a 4-byte size field, while `WorldServer/NetWork/TCPServer.cs` sets
`SizeLen = 2`. The test sets this itself.

## 4. Ruled out — do not re-investigate

- Packet byte layout (§3, proven identical).
- Send timing relative to `S_PLAYER_INITTED` (§3).
- Category number: `InteractionUtils.IsTomeAdvance` accepts only `TOME_CC_A` (16) and
  `TOME_CC_B` (17); we send 16, which is also the `Category` all 27 carry in `abilities`.
- The 27 being filtered out of `AbilityMgr`: `AbilityInfo.Convert` drops only
  `AbilityType.Effect = 255`; tome tactics are `None = 0`.
- Fragments/unlocks being incomplete: all 27 unlocks held, all 9 counters full.
- `.alltoks` "not granting anything": expected. `GrantAllToks` skips entries already held and
  Bigboy holds all 10,397.

## 5. Bugs found and fixed along the way

Recorded in `docs/INTERNAL_BUG_TRACKER.md` as BUG-115 through BUG-123. The ones that cost the most
time, and why:

- **BUG-120** — buff rows written to `buff_infos` only. Abilities and buffs live in two parallel
  tables exactly like items; `UseMythicActionCoverageTables` defaults to true so the server reads
  `mythic_src_buff_infos`. This is the BUG-033 trap CLAUDE.md hard rule 1 already warns about.
  **Any migration touching ability, buff or item columns must write both tables.**
- **BUG-121** — purchases were never persisted. Added `characters_tome_tactics`.
- **BUG-123** — the trainer option sends `Menu=7`, not the `case 6` the career-trainer code
  assumed; and purchases arrive on `F_BUY_CAREER_PACKAGE`, whose `resource` byte is the category,
  where anything that is not 7 (mastery) falls through to renown and answers
  "This ability is not implemented."

## 6. Process note

Five rounds of this were spent guessing protocol behaviour from reading C# and Lua, each burning
an in-client test cycle. Every one of those questions was answerable offline from the official
capture corpus, which `docs/CROSS_REPO.md` names as the authority for protocol layouts and which
CLAUDE.md rule 3 says to use *actively, not only when stuck*.

**For the remaining blocker and any future protocol question here: go to the corpus first.**
`tools/validation/Read-OfficialPackets.ps1` decodes the gzip captures; 220 of the 1,027 contain
`F_CAREER_PACKAGE_INFO`.

## 7. Files

Migrations 54-59 (all applied and re-runnable):

| Script | Purpose |
|---|---|
| `54_populate_bestiary_action_counters.sql` | per-species kill counters (BUG-115) |
| `55_tome_tactic_lines_and_fragments.sql` | 9 lines, 138 fragments |
| `56_tome_tactic_buff_infos.sql` | buff rows (incomplete, see 58) |
| `57_tome_tactic_line_creature_types.sql` | creature types per line |
| `58_tome_tactic_buff_infos_mythic_source.sql` | the same buff rows in the table the server reads |
| `59_tome_tactic_effect_ids.sql` | `EffectID` from `mythic_bin_ability` |

Code: `WorldServer/Services/World/TomeTacticService.cs`,
`Common/Database/World/Tome_Tactic_{Line,Fragment,Line_Creature_Type}.cs`,
`Common/Database/Character/Characters_tome_tactic.cs`, plus changes in `TokInterface`,
`TacticsInterface`, `AbilityInterface`, `AIInterface`, `Creature`, `Unit`, `CreatureService`.

Validation: `Test-TomeTactics.ps1` (52 data checks) and `Test-TomeTacticPackets.ps1` (byte diff
against the live capture).

Diagnostic logging currently left on: `TomeTacticLibrarian` logs every interact with its menu
numbers; `TomeTactic` logs how many advances were sent. Remove both once the window opens.
