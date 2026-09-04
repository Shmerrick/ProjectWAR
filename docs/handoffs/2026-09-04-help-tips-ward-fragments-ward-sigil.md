# Session handoff — 2026-09-03/04

Three workstreams. Two are finished and verified; the third is a client-patch project that
works in one form and is mid-flight in a better form. Read the ward sigil section in full
before touching that code — most of its value is the record of what was *disproved*.

---

## 1. Beginner help tips (BUG-028) — DONE

### Symptom
Every Tome unlock popped an empty help-tip window: frame, icon, Close button, "Show Beginner
help tips" checkbox, no text.

### Root cause
`F_TOK_ENTRY_UPDATE` (0xF8) carries a trailing **help-tip category byte** and
`TokInterface.SendTok` hardcoded it to `1`.

Client evidence, ToK handler `0x4d2cb9`:
```
byte [ebx+0]           mode: 0 = single entry, non-zero = bitfield
word [ebx+2]           count
word [ebx+4]           base entry id
then three stream bytes: unlocked flag, Tome announce flag, help-tip category
a non-zero category  -> calls 0x5dd0aa -> raises HELP_TIP_UPDATED(entry, category)
```
`EA_HelpTips` resolves title/body from the `HelpTipNames` / `HelpTipDescriptions` string tables
using `entry - 11799` (`HelpTips.INDEX_OFFSET`). A chapter/area unlock gives an out-of-range
index, so both strings come back empty. No Lua error, which matches the empty `ui_errors.log`.

Second cause: nothing granted the tips. No row in `zone_areas.TokExploreEntry`,
`chapter_infos.TokEntry` or `item_infos.TokUnlock` falls in 11800-11999.

### Fix
- `help_tips` table + `HelpTipService` + `TokInterface.FireHelpTips(trigger, value)`.
- 22 triggers wired: `Login, RankUp, RenownRankUp, XpGained, RenownGained, MoneyGained, Death,
  GroupJoined, GroupLeader, WarbandFormed, RvrFlagged, RvrAreaEntered, NpcInteract, Loot,
  QuestCompleted, PublicQuestEntered, PublicQuestBag, MailboxUsed, ScenarioJoined,
  TradeSkillLearned, ChapterEntered, InfluenceReward`.
- `SendTok` now sends `HelpTipService.GetTipType(Entry)`, which is 0 for anything that is not a
  configured help tip. That alone stops the blank popups.
- `Database/19_restore_help_tips.sql` — 59 tips. **Applied to the live DB.**

Tips unlock through the normal Tome path, so they persist in `characters_toks` and show once per
character. The Tome ticker is suppressed for them (`AddTok(..., announce: false)`).

### Verified
Boot log: `LoadHelpTips Loaded 59 Help_Tips`, zero rejections. All 59 entries have non-empty
title *and* body in the client's `helptipnames.txt` / `helptipdesc.txt`.

### Known gaps
- All rows ship as `TipType = 1` (Beginner). No 1.4.8 source carries a per-tip category —
  not the client data, not `tok_infos`. The column is per-row and editable.
- 40 of the 99 named client tips have no server trigger yet (Trophies, Talismans, Emotes,
  Resurrection, Morale Abilities, Dungeons, item quality tiers...). Adding one is a row plus a
  one-line `FireHelpTips` call.

---

## 2. Ward fragment equip tasks (BUG-029) — DONE

### Symptom
Tome showed "Equip Annihilator Helm" as incomplete while the helm was worn.

### Root causes (three)
1. `item_infos.TokUnlock3` was 0 for every ward set. Script 05's Invader values had never been
   applied to the live DB either.
2. Of four equip paths in `ItemsInterface`, only one granted `TokUnlock3`; the direct-move path
   granted `TokUnlock` alone and the swap path granted nothing.
3. Fragments and the tasks that award them are **separate** unlocks and nothing connected them.

### The encoding (client and server agree)
Tome section 5, from `interface/interfacecore/tome/unlockmapping.csv` and `tok_infos`:
- `Index` = sigil tier: 1 Lesser, 2 Greater, 3 Superior, 4 Excelsior, 5 Supreme
- `Flag` = `fragment * 10 + task`; fragment 1-5 = boots, gloves, shoulders, helm, chest
- task `0` = the fragment award itself (7600-7624); tasks 1-6 = alternative ways to earn it

Armour tasks:

| Task | Lesser | Greater | Superior | Excelsior | Supreme |
|:---|:---|:---|:---|:---|:---|
| 1 | Annihilator | Conqueror | Invader | Warlord | Sovereign |
| 2 | *next tier ward* | *next tier* | *next tier* | *next tier* | Doomflayer |
| 3 | Bloodlord | Sentinel | Darkpromise | — | Warpforged |

Task 4 = boss/PQ objective, task 5 = RvR objective. Both unimplemented.

### Fix
- Granting centralised in `ItemsInterface.EquipItem` (covers all four paths).
- `Player.OnLoad` calls `ItmInterface.GrantEquippedItemUnlocks()` after `TokInterface.Load`, so
  already-worn items backfill without re-equipping. Note ordering: `ItmInterface.Load` runs
  *before* `TokInterface.Load`, so `EquipItem(itm, grantUnlocks: false)` on the load path.
- `TokService.BuildWardTaskLookup` derives task→fragment from Section/Index/Flag;
  `TokInterface.AddTok` awards the fragment when a task unlocks (recursion is depth-1 and
  documented — a fragment has task digit 0 so it never resolves again).
- `Database/20_restore_ward_fragment_equip_tasks.sql` — **applied to the live DB.**
  - `TokUnlock3` set to the *task* entry for 10 sets, 1,377 items, keyed on
    `TokUnlock2` + `SlotId` (not names: Doomflayer and Warpforged share per-slot `TokUnlock`).
  - Restores 10 section-5 rows the world dump held as blank placeholders
    (7670-7674 Doomflayer, 7695-7699 Warpforged).

Item `SlotId`: 20 chest, 21 gloves, 22 boots, 23 helm, 24 shoulders.

### Verified
All 99 section-5 tasks resolve to a fragment. 1,377 items carry a task; 0 orphans; 0 items left
on the old fragment ids.

---

## 3. Ward sigil on the enemy target frame (BUG-030) — DONE

**Confirmed in game 2026-09-04.** Bilerot Burrow (`Ward = 1`) shows the Lesser Ward sigil on all
trash and bosses; The Lost Vale (`Ward = 2`) shows Greater. Both appear immediately on target, the
Tome click-through opens the matching sigil page, and unwarded creatures and world objects show
nothing. The tier therefore travels from the spawn row to the icon as real per-zone data.

### The server half (all of it that lives in this repository)

- `F_WARD_INFO = 0xDF` in `WorldServer/NetWork/Opcodes.cs`. The stock 1.4.8 client discards this
  opcode, so sending it is safe to every client and needs no capability negotiation.
- `Creature.SendWardInfo(plr)` fires from `SendCreateMonster` after the create packet, payload
  `(uint16 oid, byte tier)`, big-endian to match `PacketOut.WriteUInt16`.
- Sent for **every** creature including tier 0. Oids are reused, so an unwarded creature that
  inherits a previous occupant's oid must explicitly clear the tier.
- `Pet` and `Siege` are excluded; both override `SendCreateMonster` anyway, so the guard is
  belt-and-braces.

The ward tier itself comes from `Creature_spawn.Ward`, populated for instances by `Instance.cs`
copying `obj.Ward` onto the spawn, and assigned per zone by
`Database/18_restore_endgame_dungeon_ward_tiers.sql`.

### The client half

Not in this repository. The stock 1.4.8 client never writes the field its target frame reads for
the sigil, so a packet alone cannot light the icon; a separately distributed client is required.
That component, the static analysis behind it and the full investigation record are maintained in
the private WAR-RE-Toolkit repository (`docs/reference/ward-sigil-client-patch.md` and
`docs/checkpoints/2026-09-04-ward-sigil-full-record.md`). It reaches players through the
launcher's patch manifest, not through source control.

Nothing here depends on it: an ordinary client receives `F_WARD_INFO`, ignores it, and shows no
sigil.

### Environment gotchas worth keeping

- **`msbuild` silently leaves a stale exe when the server is running.** The C# compile succeeds and
  only the *copy* fails (`MSB3027`, `FrameWork.dll` / `WarZone64.dll` locked). Always stop the
  stack before building, and check the `bin/Release/WorldServer.exe` timestamp.
- `F_PLAYER_INFO` is a **periodic line-of-sight report**, not a target-change notification, and it
  is the only inbound path that calls `SetTarget` (`CombatHandlers.cs`). Anything wired to
  `SetTarget` inherits a delay of up to about a second. Do not hang on-demand data off it.
- `SystemData.TargetObjectType` cannot distinguish creatures from world objects: doors report
  `ENEMY_NON_PLAYER` (6) too, confirmed on the Bilerot Burrow entrance.
