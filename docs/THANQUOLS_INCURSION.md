# Thanquol's Incursion — reference and test guide

Zone 410, `Thanquuol's Incursion` (the DB name carries that typo — do not "fix" it, name
lookups depend on it). Region 410, Tier 4, `Type` 3.

This is a **realm-versus-realm scenario, id 2304**, with **public quest 911** running inside
it. Not a dungeon with a PQ bolted on: both realms enter through their own pre-stage area,
fight over the same objectives, and are scored against each other to 500 — the bar the client
shows centre-screen.

---

## Warp coordinates

`.teleport map <zone> <x> <y> <z>` — GM level required, 4 arguments, world coordinates.

| Destination | Command |
|:---|:---|
| **Boss pad** (Skeetk, Throt, Thanquol all stand here) | `.teleport map 410 83240 83275 8510` |
| Boneripper (Thanquol's pet, beside him) | `.teleport map 410 83143 83310 8510` |
| **Pre-stage area A** (zone jump 2114953) | `.teleport map 410 86390 88292 8793` |
| **Pre-stage area B** (zone jump 2114954) | `.teleport map 410 78460 78210 9167` |

The two pre-stage areas are the realm staging points, at opposite corners roughly 8,000 units
apart on both axes.

### Siphoning Contraptions — the Stage I and III objective

Four of them, spawned by public quest 911. Prototype **100517**, display 7454.

| # | Command | Orientation |
|---:|:---|---:|
| 1 | `.teleport map 410 84600 84371 8698` | 512 |
| 2 | `.teleport map 410 82003 81925 9275` | 3777 |
| 3 | `.teleport map 410 80660 84725 8506` | 3083 |
| 4 | `.teleport map 410 85411 80422 9275` | 3879 |

There is also a **Warpfluid Pool** in the captures at `91425 91656 8481` (client 25889,26120)
which is not yet spawned and is not an objective.

### Getting out

Zone 410 has no exit portal spawned. Use `.teleport map` to somewhere else, e.g. Praag
(zone 105).

---

## The stage sequence

Decoded from three official full-run captures; all six stage packets agree across them.

| Stage | Tracker title | Objective | Need | Timer |
|:---|:---|:---|---:|:---|
| Setup | Setup | *(none shown)* | – | **300 s** |
| Stage I | Destroy Siphoning Contraptions | Siphoning Contraptions Destroyed | 2 | none |
| Stage II | Dispatch Warlock Engineer Skeetk | Warlock Engineer Skeetk | 1 | none |
| Stage III | Destroy the Siphoning Contraptions | Siphoning Contraptions Destroyed | 4 | none |
| Stage IV | Dispatch Throt the Unclean | Throt the Unclean | 1 | none |
| Stage V | Dispatch Thanquol | Thanquol | 1 | none |

Setup is the only timed stage and advances on its own timer. Stages I–V carry
`NoStageTimer = 1`, so they neither count down nor fail — which is what the captures show, and
without it the 540 s engine default would fail the quest partway through the Thanquol fight.

**Bosses are not stage-gated yet.** All three sit at the same spawn point and are up from the
start, so they can be killed out of order. Kill credit still works (`Creature.SetDeath` credits
any death in the quest's zone), so the stages do advance correctly.

---

## What to expect when testing

* Teleporting in should not evict you: zone 410 has no client overlay so `CurrentPQArea` is 0,
  and PQ 911's `PQAreaId` is 0, so the membership check matches.
* The PQ tracker should show the Setup stage with a 300 s countdown, then Stage I.
* Destroying two contraptions should advance to Stage II; killing Skeetk to Stage III; and so on.
* On completion a gold bag should roll (see Rewards below).

### Known gaps that will show up

| Gap | Effect |
|:---|:---|
| Scenario 2304 missing from `scenario_infos` | No score bar, no realm-vs-realm scoring, no scenario framing at all |
| No entrance to zone 410 | Teleport is the only way in. Portal 99891 is hardcoded in `GameObject.cs:259` but has no prototype and no spawn |
| Excavated Skaven Device (prototype 11637) missing | The Warpstone tokens cannot be spent, so Play as Skaven is unreachable |
| Bosses not stage-gated | All three killable in any order |
| Coin reward is 0 | `GoldChest` computes `100 * PQDifficult * Chapter * bagtype`, and `PQDifficult` is 0 |

---

## Rewards

Migration 70 wires the reward sets into the gold bag.

* **332 pieces**, entries 5757300–5757778, all Rarity 5 at rank 40 — the **Doomflayer** and
  **Warpforged** sets. Each player's career filters this to 13–14 candidates.
* `pquest_loot` rows are `Bag = 5` (gold), `PQType = 2`, `PQEntry = 911`. Bag numbering comes
  from the loot-bag item ids: 9940 Minor→1, 9941 Lesser→2, 9942 Greater→3, 9943 Major→4,
  **9980 Massive→5**, which `LootBagRarity` maps to White/Green/Blue/Purple/**Gold**.
* PQ 911 is `PQType 2`, which guarantees one gold bag and scales the rest with player count.
* The set names also cover Type-36 crest, insignia and exchange-bag **currency** items with
  `Career = 0`. Those are vendor currency and are deliberately excluded from the chest.

### Boss tokens — the Play as Skaven unlock

Each boss drops one, and all three already exist with the correct text:

| Item | Entry | Coerces |
|:---|---:|:---|
| Skeetk's Warpstone Supply | 86329 | Warlock Engineer |
| Throt's Warpstone Shard | 86330 | **Rat Ogre or Packmaster** |
| Thanquol's Warpstone Supply | 86331 | Gutter Runner |

They are spent *outside* the dungeon: "at any **Excavated Skaven Device** within a contested
tier four RvR lake, to do your bidding. This item will **decay in real time**." Three tokens
produce a four-option menu because Throt's covers two forms.

**Nothing drops them yet** — the bosses have no loot binding for these entries.

---

## How it worked on live

From the 1.4.0 patch notes (`docs/patch-notes/1.4.0.md`):

* Skaven tunnels **appear randomly during contested Tier 4 zone battles**.
* Entry is restricted to **Renown Rank 65+**, and both realms may enter.
* Repelling the Incursion unlocks Play as Skaven for **everyone in the lake, 15 minutes later**,
  with a capped, first-come-first-serve number of Skaven slots per battle.
* While controlling a Skaven the player takes on its form, abilities and stats; **inventory and
  character screens are disabled**; loot gained is delivered when they resume their own character.

---

## Sources

* Captures in `WAR-RE-Toolkit/libs/protocolservices/Packet Logs`:
  `Tanquollincursion.txt.gz`, `THANQUOL INCURSION (FULL RUN WITH EMPTY IGNORE LIST).log.txt.gz`,
  `thanquollfull+RvR.txt.gz`, and the Warpstone Supplies backpack capture.
* `docs/patch-notes/1.4.0.md` and `1.4.5.md`.
* Migrations `68`, `69`, `70`. Checks in `tools/validation/Test-ThanquolEncounter.ps1`.
* Full working notes in `docs/handoffs/2026-09-07-four-systems.md`.
