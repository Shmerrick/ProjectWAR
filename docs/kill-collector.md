# Kill Collector

Ambient bonus-XP mechanic. Killing a collector's wanted creatures accrues credit
whether or not you have met that collector; talking to it pays out everything
accrued, up to a per-collector cap.

## Where the data came from

Collector, quest, target label, kill cap and XP come from Return of Reckoning's
public GraphQL API (`https://production-api.waremu.com/graphql/`), which serves
RoR's live collector quests. All 132 ProjectWAR NPCs with `creature_protos.Title
= 32` resolve there.

The API returns a human-readable target ("Kill Snotwood Spites") and **no
creature ids** — `QuestObjective` exposes only `description` and `count`, and
neither `Quest` nor `Creature` offers a reverse link. Target creature entries
were therefore resolved locally against `creature_protos`:

| How | Count |
|---|---:|
| Zone-scoped exact name match | 106 |
| Hand-mapped (label names a camp or faction, not a mob) | 22 |
| Unmapped — no such creature in ProjectWAR's world data | 4 |

Unmapped collectors are seeded with a definition but no targets. They are inert
(no kill can credit them) and listed at the bottom of
`Database/04_kill_collector.sql` so they can be filled in as data later:

- 2312 Gottfried Holz — "Darkthorn Gors"; period guides call these Gashthorn Gors
- 5963 Krathar Dreyalan — "Tormented and Lost Spirits"; none in Dragonwake
- 6992 Grimor — "Reichert's Humans"
- 8511 Gurglesmear — "Moltenhide Hounds"

## Deliberate design decisions

**Targets are exact creature entries, not Bestiary subtypes.** 22 collectors want
creatures spanning more than one `CreatureSubType`, and even inside one subtype
only some variants qualify. `TokInterface.AddKill` and its Bestiary counter are
untouched and remain a separate concern.

**Caps are per-collector data.** The seeded values are current RoR's 20–60 by
chapter. Retail is documented as 60 for every collector (RoR patch notes, March
2024). Changing that is a data edit, not a code change.

**No influence message.** Retail printed a spurious "you have received an
influence reward" line once a collector was maxed. No influence was granted; it
was cosmetic. Not reproduced.

**RoR's modern rewards are not imported.** Current RoR gives dyes and consumables
on a standardised schedule; those are RoR balancing, not retail behaviour.
`CompletionTokEntry` is seeded 0 for every collector and left for the ~20% of
retail collectors that granted a one-time ToK reward.

**Praag's three collectors share one target set.** RoR splits the Raven Host into
Plunderers / Reinforcements / Scouts; ProjectWAR's data has no such split, so all
three accept the same 15 Raven Host creatures. This over-credits across those
three collectors but never credits an unrelated mob.

## Verification

Automated so far:

- Solution builds clean — `WorldServer.csproj` Release, zero CS errors or warnings.
- Migration applies to an empty schema: 132 definitions, 312 target rows.
- Referential integrity: every `CreatureEntry` exists in `creature_protos`; no
  target is itself a collector; no target references an unknown collector.
- Spot checks against the API: Wobna Slipsquig → Crazy Squig (cap 20, 50 xp),
  Barin Grimbeard → Snotwood Spite (cap 40, 200 xp), Xobz Madgut → the four
  Irontoe dwarfs in Mount Bloodhorn.

The repository has no test project, so runtime behaviour is **not** covered by
automated tests. Manual checklist:

1. **Hint on first talk.** Fresh character, talk to a collector with no kills.
   Expect the "I am interested in \<target\>" line and no XP.
2. **Retroactive credit.** Kill 3 of a collector's targets *without* having talked
   to it, then talk. Expect XP for exactly 3 kills (`3 × Xp`).
3. **Partial claim, then more.** Kill 2 more, talk again. Expect XP for 2 only —
   not 5.
4. **Cap.** Kill past `KillCap`, claim, then kill more and talk again. Expect the
   "already been rewarded all you can" line and no further XP.
5. **Wrong mob.** Kill a creature of the same subtype that is *not* in the
   collector's target list. Expect no collector credit (the Bestiary counter
   should still tick — that is the point of keeping them separate).
6. **Group credit.** In a group within 150 units, confirm every member accrues.
7. **Unmapped collector.** Talk to 6992 Grimor. Expect the hint line and no
   errors — the definition loads with no targets.
8. **Restart persistence.** Claim partially, restart the server, confirm
   `characters_kill_collector` retains `AccumulatedKills`/`ClaimedKills`.

## Not implemented

Map pips. `MAPPIPS_KILL_COLLECTOR_QUEST_PENDING_NPC` (29) and
`..._COMPLETE_NPC` (30) are still unreferenced. The pending/complete state is
per-player, while `CreatureService` builds `States`/`FigLeafData` once per
creature *prototype* at load time and shares it across all players — so driving
the orange indicator needs a per-player state dispatch, not a proto edit. Left
out rather than done wrongly at the proto level.
