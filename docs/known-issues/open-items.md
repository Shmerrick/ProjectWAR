# Open items

Running list of things found that need fixing or investigating. Append as more
turn up; move entries to **Resolved** with the commit that closed them rather
than deleting, so the evidence survives.

Each entry records what was actually observed, not a guess. Where something is
unverified it says so.

---

## Framework / build

### F1. `CheckOrCreateTable` silently DROPs columns it does not recognise — HIGH

`DataConnection.CheckOrCreateTable` (`FrameWork/Database/Connection/DataConnection.cs:164`)
diffs the live schema against the DataObject's properties and adds any column
whose name is not a property to `columnsToRemove`, then issues
`ALTER TABLE ... DROP COLUMN`. Every `[DataTable]` type is registered at startup
(`DBManager.cs:114`), and MySQL's `IsSQLConnection` is `true`, so this runs on
every boot.

Consequence: renaming a property in C# without a matching migration **destroys
the column's data on next startup**, with only a log line. This is what happened
to `tok_bestiary` on RESTART (see R1).

Observed live during a RESTART boot against this database:

```
characters  Added columns: `FirstConnect` TINYINT UNSIGNED NOT NULL
characters  Removed column: honorpoints
characters  Removed column: honorrank
```

Two columns dropped from a live table on startup, reported at INFO level among
thousands of lines. Not verified whether the master line drops the same two.

Suggested: make column removal opt-in, or at minimum refuse to drop a non-empty
column and fail loudly instead.

### F2. Row binding is by ordinal position, not column name — MEDIUM

`MysqlObjectDatabase.StaticBindSelect` binds result columns to properties with
`data[field++]` ("we can use hard index access because we iterate the same order
here", line ~980), while the SELECT is built from property *names*
(`MysqlObjectDatabase.cs:794`).

Consequence: reordering properties in a DataObject, or inserting a column in the
middle of a table, silently shifts every field after the change. No error, wrong
data. Any new table must declare columns in the same order as its DataObject's
properties.

Suggested: at minimum a comment on `StaticBindSelect` and on each DataObject;
better, an assertion at load that the reader's column names match the binding
order.

### F3. `Launcher` does not build: NLog 4 / NLog 6 type conflict — MEDIUM

```
Launcher\NetWork\Client.cs(19,33): error CS0433: The type 'Logger' exists in both
'NLog, Version=4.0.0.0' and 'NLog, Version=6.0.0.0'
```
Also `Patcher.cs(18,26)`, `Patcher.cs(26,24)`, `LauncherForm.cs(24,33)`.

`Launcher/Launcher.csproj:89` and `Launcher/packages.config:5` both reference
only NLog 6.1.0, so the 4.x reference is coming from somewhere else. Pre-existing
and unrelated to Kill Collector work; `WorldServer` builds clean.

Unverified: whether this also fails on a clean checkout of `master`.

### F4. `WorldServer` emits MSB3277 assembly-unification warnings — LOW

`System.Runtime` 4.1.1.1 vs 4.1.2.0, and similar for `Appccelerate.StateMachine`
and `BehaviourTree`. Not new, does not fail the build, but it is noise that hides
real warnings.

---

## Database / data quality

### D1. `tok_bestary` duplicate table still present — LOW

`war_world` contains both `tok_bestiary` (latin1) and `tok_bestary` (utf8mb4),
137 identical rows each. The typo'd one is the pre-rename original and is now
dead. Left in place deliberately — dropping a table is destructive and buys
nothing — but it should be removed once someone confirms nothing reads it.

### D2. Four Kill Collector targets do not exist in ProjectWAR's world data — MEDIUM

Seeded as definitions with no targets, so they are inert rather than broken:

| NPC | Collector | RoR target | Note |
|---:|---|---|---|
| 2312 | Gottfried Holz | Darkthorn Gors | no `Darkthorn` creature anywhere; period guides say *Gashthorn* Gors |
| 5963 | Krathar Dreyalan | Tormented and Lost Spirits | none in Dragonwake |
| 6992 | Grimor | Reichert's Humans | no match anywhere |
| 8511 | Gurglesmear | Moltenhide Hounds | no match anywhere |

Fixable as data once the correct creatures are identified. See
`Database/04_kill_collector.sql`.

### D3. Praag's three collectors share one target set — MEDIUM

RoR splits the Raven Host into Plunderers (711), Reinforcements (6223) and Scouts
(6234). ProjectWAR's `creature_protos` has no such split — no
`Raven Host Plunderer` etc. exists — so all three currently accept the same 15
`Raven *` creatures. Over-credits across those three collectors; never credits an
unrelated mob. Correctable in data.

### D4. `Seleis Soronil` (4280) has `TokUnlock = 0` — LOW

Every other Kill Collector has a `tok_infos` entry. Its position as Dark Elf
chapter 12 is confirmed by the RoR API and by the otherwise unbroken chapter
sequence, so this looks like a local data gap rather than a real difference.

### D5. NPC 4437 name mismatch with RoR — LOW

ProjectWAR calls it `Bjorn Bulweis`; the RoR API and the RoR wiki both call it
`Fnord Bulweis`. Same entry, same chapter (Chaos 11), same quest
(`Meat Fit For A King`). Data-version drift, not a second NPC.

### D6. `creature_texts` rows are truncated mid-sentence — LOW

Only 420 rows total, 42 touching Kill Collectors, and the text is cut off in the
data itself, e.g. entry 2218: `"In doing so, we fed the enemy mu"`. Whatever
import produced this table lost the tail of each string.

### D7. `Sigrid Widmann` (1341) finishes two Beastlord Hunt quests — LOW

Quests 1932 and 1933 are attached to a Kill Collector NPC. Whether that is
correct or an import artefact is unverified. It is a trap for anything reading
`questsFinisher[0]` — that returns "Kill Enemy Players x30" rather than her actual
collector quest (`Reikland Bandits`, 60486).

---

## Kill Collector feature

### K1. Head icon done; map pips and interaction dialogue still missing — MEDIUM

**Correction.** This entry previously claimed the orange indicator was blocked
because `CreatureService` builds `States`/`FigLeafData` per creature *prototype*.
That was wrong: a per-player channel already exists —
`QuestsInterface.GetQuestStatusFor` feeds `Packets.UpdateQuestState`, pushed to a
single player, and `QuestStateOpcode.QuestCompleted` is the orange turn-in marker.

Now implemented: collectors with unclaimed progress report `QuestCompleted`, and
the icon is refreshed on the 0 -> 1 kill transition and cleared on claim.

Still missing:

- **Interaction dialogue.** Talking to a collector prints a chat line only; no
  window opens. The dialogue is `F_INTERACT_RESPONSE` built by
  `SendQuestDoneInfo` / `BuildQuestInteract` / `BuildQuestComplete`, all of which
  need a real `Quest` and `Character_quest` row. Two ways to get it: seed genuine
  quest rows for all 132 collectors (which is how RoR does it, and would give
  dialogue and icon from the existing quest machinery), or hand-build an
  `F_INTERACT_RESPONSE` payload that imitates the quest window without a quest
  row. Neither is small.
- **Map pips.** `MAPPIPS_KILL_COLLECTOR_QUEST_PENDING_NPC` (29),
  `MAPPIPS_KILL_COLLECTOR_QUEST_COMPLETE_NPC` (30) and
  `CHAPTERHUBSERVICE_KILL_COLLECTOR` (9) are still referenced nowhere.

### K2. Kill caps are current RoR values, not retail — LOW

Seeded caps are RoR's 20–60 by chapter. RoR's own patch notes (March 2024) state
that live retail required 60 for every collector, and RoR's July 2025 notes say
Tier 4 was changed from all-60 to 30–60. `KillCap` is per-collector data
precisely so this stays a decision, not a code change.

### K3. Completion rewards are unseeded — LOW

`CompletionTokEntry` is 0 for all 132. Roughly 20% of retail collectors granted a
one-time ToK reward (title, tactic fragment, cloak, jewellery, pocket item). RoR's
current rewards are standardised dyes and consumables and are deliberately not
imported. A retail reward table has not been recovered.

### K4. Runtime behaviour is not covered by automated tests — MEDIUM

The solution contains no test project at all, so nothing here is verified beyond
compilation and migration/data integrity. `docs/kill-collector.md` has a manual
checklist. Adding a test project is a larger decision than this feature.

---

## Observed in game

### G1. BO capture renown appears in the Combat tab, not RvR — PROBABLY BY DESIGN

Observed 2026-08-06: "You gain 200 renown from capturing Kurlov's Armory."
printed to Combat.

The server does not choose the tab. `RVRRewardManager.cs:150` calls
`AddRenown(rr, false, RewardType.ObjectiveCapture, objectiveName)`, which sends
`F_PLAYER_RENOWN` with the reward type as a byte (`ObjectiveCapture = 28`); the
client composes the sentence and routes it.

The client's routing looks intentional: `CHATLOGFILTERS_RENOWN = 1008` sits in the
same 1000-block as `CHATLOGFILTERS_COMBAT_DEFAULT = 1000`, so *all* renown gain
goes to Combat, while the RvR tab carries realm-war events
(`CHATLOGFILTERS_RVR = 17`, `CHATLOGFILTERS_C_BATTLEFIELD_OBJECTIVE = 281`).

To change it: either send an additional chat message on filter 281 alongside the
renown packet (server-side, duplicates the line), or change the routing in the
client UI Lua (client-side). Decide whether it is worth either.

### G2. Possible duplicate capture rewards — UNVERIFIED

Same screenshot shows "You gain 200 renown from capturing Kurlov's Armory."
**twice**, and Martyr's Square awarding 200 then 100. May be legitimate separate
reward ticks, or the objective reward path running more than once per capture.
Needs a controlled capture with `RewardLogger` trace before drawing a conclusion.

### G3. 223 quests are uncompletable: objectives reference missing gameobjects — HIGH

Spotted from the client quest tracker showing
`Invalid GameObject - QuestID 30001, ObjId=131402`, then measured:

```
quests_objectives with ObjType=3 (gameobject) : 1352
  ... whose ObjID is absent from gameobject_protos : 294   (22%)
distinct quests affected                            : 223
```

The example is quest 30001 *Grimmenhagen Burning*, objective "House searched"
(ObjType 3, ObjID 131402, count 3). Entry 131402 does not exist in
`gameobject_protos`, so the objective can never be credited and the quest cannot
be completed.

This is the same missing-`gameobject_protos` gap that aborts PQ spawn loops, so
every row added to that table fixes quests and public quests together.

Not yet determined: whether these gameobjects are absent from the published dump
generally, or were dropped from this particular curated database.

## Upstream data source

### U1. RoR's GraphQL API returns target labels, not creature ids — INFO

`https://production-api.waremu.com/graphql/` resolves all 132 collectors, but
`QuestObjective` exposes only `description` and `count`; neither `Quest` nor
`Creature` offers a reverse link to objective creatures. Target entries therefore
have to be resolved locally by name against `creature_protos`. Confirmed by
introspecting all three types.

Also note the default `urllib` User-Agent is rejected with HTTP 403; any scripted
access must set one.

### U2. One collector's XP is not on the RoR formula — LOW

RoR's XP is `50 × chapter` through chapter 18, then 925/950/975/1000 for 19–22.
Exactly one collector deviates: **4018 Kurgan Ironfist** (Dwarf chapter 5,
`Going Batty`) gives **336** rather than 250 — the only value in all 132 that is
not a multiple of 25. Likely legacy data or a data error on RoR's side; worth
confirming before treating the formula as authoritative.

---

## Resolved

### R1. `tok_bestiary.Bestary_ID` vs `Bestiary_ID` destroyed the bestiary — RESTART ONLY, was CRITICAL

**Does not affect this branch.** On the master line the class, table and property
are consistently `Tok_Bestary` / `tok_bestary` / `Bestary_ID`, so there is nothing
to fix and no migration is shipped here. Recorded because the bug is real and
still live on RESTART, and because it is the clearest illustration of F1.


Commit `fad63164` renamed the class, table and property; the DB column was never
renamed. Combined with F1, the first RESTART boot would DROP the populated
`Bestary_ID` and add an empty `Bestiary_ID`, after which `TokInterface.AddKill`
records every kill against `NPCEntry 0` and no bestiary threshold or ToK unlock
fires.

Verified: the ORM's generated SELECT fails with
`ERROR 1054: Unknown column 'Bestiary_ID'` against the live world database.

Fixed by `Database/03_fix_tok_bestiary_column.sql` (commit `ec5ef33a`).
