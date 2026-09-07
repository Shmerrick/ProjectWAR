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

## Recovery: done

Migrations 72 and 73 restore **all 25,167 deleted rows** and the 2,611 prototypes they need.
`gameobject_spawns` is back to 26,316 rows from 1,146, and every spawned entry now resolves to a
prototype.

| | Before | After |
|:---|---:|---:|
| gameobject_spawns | 1,146 | **26,316** |
| gameobject_protos | 210 | **2,779** |
| spawned entries with no prototype | 110 | **0** |
| PQ USE_GO/KILL_GO objectives with no prototype | 232 | **72** |
| pquest_spawns Type-2 entries with no prototype | 110 | **47** |
| zones containing game objects | few | 107 |

### How the rows were recovered

Verbatim from git, never reconstructed. Each row keeps its original Guid; the deletion removed
rows and added none, so nothing collides:

```
git cat-file blob a4995e92:Database/war_world.7z > old_world.7z
```

Opaque `Unks` payloads and the inconsistent NULL-versus-empty-string conventions between zones are
preserved exactly.

### How the names were recovered, and why DisplayID is not enough

The pre-deletion dump has no `gameobject_protos` table, so prototypes had to be derived, and the
name is the one field a spawn row does not carry.

**DisplayID is a model, not an identity.** 405 of the 811 DisplayIDs seen across the capture corpus
are shared by more than one named object -- 211 is *Weapon Wagon*, *Empire Wagon* and *Order Parts
Wagon*; 166 is *Nursery Slime* and *Cesspool*; 9290 is *Excavated Skaven Device*, *Thanquol's
Incursion* and *Gutter Runner*. Naming by dominant DisplayID would be wrong at scale, and it
produced exactly one such error before this was understood: migration 72 named 2000560 "Weapon
Wagon", which migration 73 corrects to "Empire Wagon".

So names are matched by **position**. All 1,027 official captures were scanned for
`F_CREATE_STATIC` (opcode **0x71**), yielding **257,185 placed objects** with model, coordinates
and name. A deleted row is identified only when a captured object has the same model at the same
coordinates. Captures record world coordinates directly for ordinary zones --
`GetClientWorldPosition` shifts only inside instanced copies -- so the comparison is exact.

That matched 11,798 rows outright and named **1,791 of 2,609 prototypes**, covering 76.8% of the
restored rows. Only 4 prototypes had a dominant name below 80% confidence.

### The 818 prototypes with no name

Objects that appear in no capture are inserted with an **empty name**, never an invented one. The
object exists, spawns and functions; the client simply shows no tooltip text. They are listed in
`docs/gameobject-unnamed-entries.txt` with their DisplayID and row count.

Guessing here would put fabricated text on objects throughout the world, which is the failure mode
this project exists to undo. The toolkit's `apps/warprotoextract` and the client are the remaining
candidate sources.

### Ordering note

These migrations must be applied **in numeric order**. Migration 68 deletes and re-inserts public
quest 911's objectives, so re-running it alone drops the `NoStageTimer` values migration 69 sets.
Applying 68 through 73 in sequence is stable and re-runnable; the chain was applied twice end to
end to confirm it.

## Tooling note

The toolkit's `apps/warprotoextract` and the client itself are the other candidate sources for
prototype names and any field a spawn row does not carry; neither has been evaluated for this yet.

**Nothing should be inserted blind.** The Practice Target dummies earlier in this project failed
precisely because prototype fields were copied from a row nobody had proven worked.
