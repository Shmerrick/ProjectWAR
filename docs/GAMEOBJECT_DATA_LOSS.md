# The gameobject data loss, and how to recover it

**95.6% of the world's game objects were deleted from the world database. The rows are still
recoverable from this repository's own git history.**

This is the single largest data problem in the project and it explains most of the "missing
content" encountered anywhere else: broken public quests, unspawnable portals, prototypes that
do not exist, objectives that can never complete.

## The measurement

`Database/war_world.7z` at commit `a4995e92` (2026-03-06) contains a 187 MB `war_world.sql`.
The current one, from `ba84ae14` (2026-03-06, "Database Compress"), contains 115 MB. Counting
rows in both:

| Table | Before | After | Delta |
|:---|---:|---:|---:|
| **gameobject_spawns** | **26,313** | **1,146** | **-25,167** |
| creature_protos | 13,754 | 13,754 | 0 |
| creature_spawns | 69,228 | 69,226 | -2 |
| pquest_spawns | 29,437 | 29,437 | 0 |
| pquest_objectives | 2,759 | 2,759 | 0 |
| quests | 5,744 | 5,744 | 0 |
| scenario_infos | 37 | 37 | 0 |

* Distinct game-object entries: **2,759 -> 155**.
* Of the 26,313 original rows, 1,146 survive, 25,167 were removed and **zero were added**. It is
  a pure deletion, not a replacement or a re-export with different keys - the surviving rows keep
  their original Guids.
* The `gameobject_spawns` CREATE TABLE is **byte-identical** before and after, so this was not a
  schema normalisation. The columns that would justify moving data elsewhere (DisplayID, Unks,
  VfxState, DoorId) are all still on the spawn row.
* Every other table of consequence is untouched, which is why the loss went unnoticed: creatures,
  quests, PQ definitions and scenarios all survived intact.

Losses are spread across the whole world, not one area - zone 100 lost 1,652 rows, zone 108
1,008, zone 1 968, zone 5 788, zone 162 776, and so on through every populated zone.

## What this breaks, concretely

* **Pillage and Plunder (PQ 199), Stage II "Destroy Wagons"** needs 5 wagons. Entry 2000560 had
  **exactly 5 spawns in zone 106** plus 2 in zone 110. All 7 were deleted, so the stage can never
  complete.
* **Thanquol's Incursion has no entrance.** Portal 99891 is hardcoded in `GameObject.cs:259` and
  had **3 spawns in zone 410**. All deleted.
* **232** `pquest_objectives` of type USE_GO or KILL_GO point at a prototype that no longer exists.
* **110 of 152** game-object entries referenced by `pquest_spawns` have no prototype.

## Why an earlier check said nothing was missing

Comparing the live database against `Database/war_world.7z` shows no loss, because **that dump is
already the post-deletion one**. `pquest_spawns_unresolved` accounts for every row migration 47
archived, and live + archive slightly exceeds the base dump. That comparison is only capable of
detecting damage done *after* the base dump was committed. The deletion happened *to* the base
dump, and is visible only against git history.

## The prototype problem

The pre-deletion dump has **no `gameobject_protos` table at all**; it was introduced later and
currently holds 157 rows (210 live, after migrations added more). So restoring spawn rows alone is
not sufficient: `PQuestObjective.Reset` and `GameObjectService.GetGameObjectProto` look the
prototype up, log "missing gameobject prototype", and skip the spawn.

The deleted spawn rows do carry `DisplayID`, `Unks`, `VfxState`, `DoorId` and `AlternativeName`,
which is most of what a prototype needs; `Name` is the notable gap and would come from the client.

This is what migrations `02`, `10`, `12`, `34`, `38` and `46` have been doing by hand: each
reconstructs one public quest's objects from packet captures and toolkit StaticObject data.
That work is re-deriving, one quest at a time, data that is sitting in git history.

## Recovery

The pre-deletion dump is reachable without touching the working tree:

```
git cat-file blob a4995e92:Database/war_world.7z > old_world.7z
```

A restore needs to, in order: recover the 25,167 spawn rows, derive or extract the ~2,600 missing
prototypes (the toolkit's `apps/warprotoextract` and the client are the sources for names and any
field the spawn row does not carry), and verify against captures for the areas that have them.

**Nothing here should be inserted blind.** The Practice Target dummies earlier in this project
failed precisely because prototype fields were copied from a row nobody had proven worked.
