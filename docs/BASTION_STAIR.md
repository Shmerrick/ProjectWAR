# Bastion Stair (1.4.8 Restoration Target)

Status of the Tier 4 Chaos/Empire dungeon and what restoring it requires. Written 2026-09-04 from
packet captures, the extracted client, contemporary documentation, and the live database.

Bastion Stair matters beyond itself: it is the sole source of the **Bloodlord** armour set, which
is ward task 3 for the Lesser sigil, and it owns ward counter `716` ("Complete Any Bastion Stair
PQ 5 Times", Lesser fragment 2, task 4). Counter work that depends on its IDs should not land
until its structure is settled — see `docs/WARD_SYSTEM.md`.

## Verified structure

A **realm-instanced** public dungeon: no population cap in the dungeon proper, each realm in its
own instance, with the wing bosses as six-man private instances on a 20-hour (1440 minute)
lockout. Themed on Khorne, beasts and daemons.

### Entry

Two portals, both in contested Tier 4 territory: the **north-east corner of Praag** (the quicker
route from flight points) and the **far south-east of the Chaos Wastes**. The entrance hall holds
quest givers, a healer, and a **rally master** dispensing the dungeon's influence rewards.

### Wings

Three wings, each with three public quests and a wing boss, gated on dungeon influence.

| Wing | Path | Mobs | Public quests | Boss | Bloodlord piece |
|---|---|---|---|---|---|
| Left | Trail of Carnage | 33+ | Rise of Carnage, Bloodherd Labyrinth, Trail of Carnage | Thar'lgnan (34, Doombull) | Boots |
| Right | Path of Fury | 35+ | Fury's Ascent, Halls of Rage, Path of Fury | Lord Slaurith (36) | Shoulders |
| Middle | Steps of Ruin | 37+ | Step of Anger, Step of Hatred, The Brass Legion | Kaarn the Vanquisher (40, Daemon Prince) | Helm |

A fourth boss, **Skull Lord Var'Ithrok** (40, Bloodthirster), sits beyond the middle wing in the
Rift of Rage and drops the chest. The middle wing's third PQ is mandatory before its bosses open.

**Influence gates boss access:** one bubble for the left wing boss, two for the right, full
influence for the middle. Unlike Mount Gunbad, this requirement was never revised.

### Bloodlord acquisition

Five pieces. Four drop from the wing bosses as above, one class-specific piece each. The
**gloves** come from a gold bag on any of the nine public quests (from patch 1.1a). So both PQ
completion and boss kills advance the set, which is what makes the dungeon the Bloodlord source.

### Skull mechanic

Skull items found on the floor can be placed on pikes for buffs or curses, or accumulated at
entrance skull piles — roughly 100 per wing — to spawn a boss referred to as The Collector. Not
yet corroborated by capture; treat as unverified.

## Current database state

What is already correct:

- Zones exist: `160` Bastion Stair plus boss maps `163` Thar'Ignan, `164` Lord Slaurith,
  `165` Kaarn the Vanquisher, `166` Skull Lord Var'Ithrok (all Type 6), and `instance_infos`
  carries the correct 1440-minute lockout for all five.
- **All nine public quests are present** in `pquest_info` for zone 160 and map cleanly onto the
  three wings above, plus a tenth, `Arena Challenges` (501), which has **zero objectives**.
- The influence track is populated per realm: `chapter_infos` `2` "Chaos & Empire Lands: Bastion
  Stair" (Destruction) and `6` "Empire & Chaos Lands: Bastion Stair" (Order), with three
  `chapter_rewards` tiers at 8,120 / 32,940 / 75,150 influence and one reward per career.
- 650 creature spawns and 473 PQ spawns in zone 160.

## Gaps

1. ~~**Zone 160 is not realm-instanced.**~~ **Fixed 2026-09-04.** Its 18 entry jumps were `Type 0`
   with `InstanceID = NULL`, set by `Database/15_restore_shared_bastion_stair.sql`, which made the
   base map a shared PvE zone on a premise the sources contradict. They are now `Type 4` /
   `InstanceID 160`, matching Mount Gunbad, and `instancetyp == 4` is implemented as a persistent
   per-realm instance. See *Realm instancing* below.
2. **No Order-side entrance NPCs.** Zone 160 has twelve Destruction-faction NPCs (factions 129
   and 130: `Valr the Maimed`, `Shakal Daemoncaller`, `Jodis Wolfscar`, `Krulnor Volheim`,
   `Injured Marauder`, ...) and **no Order equivalents**. Gunbad has the same asymmetry — its
   zone is even named "Mount Gunbad Realm 1" — so both dungeons currently exist only from the
   Destruction side.
3. **No influence gate on boss entry.** `instance_infos.WardsNeeded` is `NULL` for all five rows,
   and nothing enforces the one/two/full bubble requirement.
4. **No Bloodlord in any loot table.** `pquest_loot` holds 3,172 rows but every one has
   `PQEntry = 0` — it is a generic table keyed by Career, Bag, PQTier, PQType and Chapter, not
   per-PQ — and none of its rows is a Bloodlord item. So the gold-bag gloves cannot drop.
5. **Bloodlord defines only 5 of 24 career sets** (BUG-034): 25 of 120 armour pieces carry a
   non-zero `ItemSet`, against 120 of 120 for every other ward set.
6. **`Arena Challenges` (PQ 501) has no objectives and no spawns**, so it can never complete — and
   it sits at exactly the same position as `Trail of Carnage` (329), `PinX/PinY 8782, 11202`. It
   is an empty placeholder stacked on a real left-wing PQ. The other nine all have objectives
   and spawns (39-143 each).
7. **`zone_areas` defines only three areas for zone 160**: `Bastion Stair` (the entrance),
   `Path of Fury` (right wing) and `Trail of Carnage` (left wing), all `AreaId 31` with
   `PieceId` 1-3. **`Steps of Ruin`, the middle wing, has no area row at all.** Without one a
   player there has no `CurrentArea`, so the realm-aware influence path cannot resolve and the
   middle wing cannot credit influence correctly for either realm.

## Realm instancing

`MovementHandlers` routes `Jump.Type` 4-6 to `InstanceMgr.ZoneIn`, where the code comment has
always read `// jump type 4 = realm 5 = raid 6 = group instances` — but type 4 was never
implemented. The only branch on the type set the raid player cap, and selection was purely
group-leader based, so a Type 4 jump behaved exactly like a group instance. `Create_new_instance`
also passed realm `0` unconditionally, and `Instance` filters its spawns with
`obj.Realm == 0 || obj.Realm == Realm`, so realm-specific instance spawns could never load.

Type 4 now selects on `(Instance_Info.Entry, player.Realm)`: one copy per realm, uncapped, and
exempt from `CheckInstanceEmpty` so it is never torn down while empty. That persistence is the
point — closing it would reset the public quest cycle and creature respawns every time the last
player left, and the next player of that realm would arrive in a freshly initialised dungeon.
Group and raid instances keep their existing behaviour entirely.

**Public quests keep working through the existing path, with no new machinery.** A dungeon zone has
`Region = ZoneId`, so an instance's `RegionMgr` carries the same region id as the world region and
`RegionMgr.LoadSpawns` resolves the same `CellSpawnService` entry. `CellSpawns` is pure data —
spawn lists with no region reference and no loaded flag — while the `Loaded` flag and object list
live on the per-region `CellMgr`. Two realm regions therefore each build their own cells and
create their own creatures, gameobjects and `PublicQuest` objects from one shared set of
definitions, and cycle independently.

The one hazard was double-spawning: zone 160's population is in `creature_spawns` (650 rows,
loaded via cells) while `Instance.LoadSpawns` separately loads `instance_creature_spawns`, and all
195 of those rows were exact duplicates of world rows. `Database/24_bastion_stair_realm_instance.sql`
removes them, keeping the world table as the complete population. Mount Gunbad never had this
problem — its 550 instance rows are the population and its 10 world rows are the entrance NPCs,
with zero overlap.

## Public quests and realm separation

Public quests are not realm-gated in data — `pquest_info` has no realm column, and all ten
Bastion PQs are `Type 0`. Separation comes from the influence track, which `zone_areas` holds per
realm: zone 160 is `OrderInfluenceId 129` / `DestroInfluenceId 128`, and Mount Gunbad is `64`/`65`.

Both dungeons' PQs then store the **Destruction** id in `pquest_info.ChapterId` — 128 for all ten
Bastion PQs, 65 for all nine Gunbad PQs. That is not a chapter reference at all, despite the
column name; `chapter_infos` 128 is "Chapter 20: Surprise Attack" in zone 9, and 65 does not
exist. The real dungeon chapters are `chapter_infos` 2/6 (Bastion, Destruction/Order) and 1/5
(Gunbad), used by `chapter_rewards` for the rally master.

`PublicQuest` resolved the influence id per realm from `CurrentArea` on the objective tick, but
the two award paths that matter — 250 on stage completion and 500 on PQ completion — used
`Info.ChapterId` unconditionally. Order players in either dungeon were therefore paid
**Destruction** influence, so their own bar never filled and the Order influence gate on the wing
bosses could never open. Fixed 2026-09-04 by routing all three through one realm-aware helper
(BUG-036); the `Type == 0` guard confines the change to these two dungeons.

Since each realm gets its own region, each also gets its own `PublicQuest` object per entry —
`RegionMgr.PublicQuests` is keyed by entry per region — so the two realms' copies run
independently rather than sharing or stacking state.

### Influence ids pointed at the wrong chapters

Worse than the award path: the per-realm ids on `zone_areas` were themselves wrong. Zone 160 held
`DestroInfluenceId 128` / `OrderInfluenceId 129`, which are "Chapter 20: Surprise Attack" and
"Warcamp: Krung's Scrappin' Spot" — both in zone 9, Nordland. Zone 60 held `65`/`64`, **neither of
which exists in `chapter_infos`**, and `Player.AddInfluence` returns silently when
`ChapterService.GetChapterEntry` misses. So Gunbad influence was discarded without a log line and
Bastion Stair influence accumulated into two unrelated Nordland bars.

`Database/23_fix_dungeon_influence_ids.sql` repoints both to the chapters `chapter_rewards` is
actually built around — Bastion `2` Destruction / `6` Order, Gunbad `1` / `5` — and corrects the
`pquest_info.ChapterId` fallback to name the right dungeon.

### Influence from creature kills

`Creature.HandleDeathRewards` granted XP, loot and quest credit but **no influence at all**, so
killing dungeon trash advanced neither realm's bar and public quest completion was the only
source. It now grants dungeon influence on the killer's own realm track, shared across damage
sources in proportion to damage dealt, so a kill is worth the same however many players
contributed. The zone is identified as a dungeon by the presence of an `instance_infos` row,
resolved once per death rather than per player.

The per-kill amount is `WorldConfigs.DungeonKillInfluence` (default 15). **No 1.4.8 figure for it
has been recovered**, so it is a tunable rather than a restored constant; if a capture or source
establishes the real value, set it and say so here.

Because the ids come from the zone area, a player standing outside any defined area earns
nothing — which is why gap 7 above matters: kills in the middle wing award no influence until
`Steps of Ruin` has an area row.

## Restoration order

Structure before content, because later work keys off these IDs:

1. Settle the realm-instancing model for zone 160 against Gunbad, and correct or supersede
   script 15.
2. Restore the Order-side entrance NPCs, quest givers and rally master.
3. Wire the influence gate on the four boss instances.
4. Restore Bloodlord loot: boss drops per wing and the gold-bag gloves.
5. Complete the Bloodlord set definitions for the 19 missing careers (BUG-034).
6. Only then wire ward counter `716`.

## Evidence

- **Packet captures** (WAR-RE-Toolkit `libs/protocolservices/Packet Logs`) — eighteen Bastion
  Stair captures covering all three wings, seven of the nine PQs, all four bosses, the entrance,
  the exit, and a teleport scroll: `BASTION STAIR - LEFT WING ... ZONE AT ENTRANCE AND PQ RISE OF
  CARNAGE`, `... MIDDLE WING ... SKULL LORD VAR'ITHROK (BOSS)`, `MECHANIC_TeleportSCROLL_
  Bastionstair`, `bastion_stairs.txt.gz`, and others. All are Destruction-side (DOK/DOR
  characters), so they establish the Destruction entrance and cannot by themselves supply the
  Order side.
- **Extracted client** — `data/strings/english/zones/zone160_area_names.txt` names Trail of
  Carnage, Path of Fury, The Brass Legion, The Gate and Steps of Ruin.
- Contemporary documentation: an October 2010 dungeon guide and a November 2008 general
  info/maps post, which agree on wings, bosses, influence gating and Bloodlord sourcing.
  A third forum thread could not be retrieved (expired TLS certificate).

Nothing above should be treated as licence to synthesise data. Where the Order-side spawns,
Bloodlord set bonuses or boss loot tables are not established by capture or client, leave them
unpopulated and record the gap — see `AGENTS.md` rules 8 and 9.

## Bloodlord set evidence (2026-09-04)

A tooltip captured from video gives one career's complete set. It is the first authentic
Bloodlord data recovered and is the template for closing BUG-034.

**Bloodlord Nightrobe** — 240 Armor, Bind on pickup, Minimum Rank 36, one talisman slot,
alternate appearance available.

- `+19 Willpower`, `+14 Wounds`, `+12 Intelligence`, `+52 Hit Points Every 4 Seconds`

Set: **Bloodlord Warped Daemonhide Robes**, five pieces —
Nightrobe, Warpmantle, Barbute, Bracers, Daemonspurs.

| Pieces | Bonus |
|---:|---|
| 2 | +62 Intelligence |
| 3 | +320 Spiritual Resistance |
| 4 | +5% Disrupt |
| 5 | +2 Path of Daemonology Abilities |

The five-piece bonus names **Path of Daemonology**, a Magus mastery path, so this is the Magus
set. That gives the naming convention (`Bloodlord <Career Kit Name>`), the piece count, the slot
naming, and the 2/3/4/5-piece bonus structure — the same shape the other ward sets use. The
remaining 19 careers still need their own names and bonuses from a source; do not extrapolate the
numbers above across careers, since each set's stats differ by archetype.

## Influence: where it is earned

Video of the live dungeon shows influence accruing **throughout the Bastion Stair map but not
inside the instanced boss fights**. `Database/30_boss_maps_award_no_influence.sql` expresses that
by zeroing the influence ids on zones 163-166; `Player.AddInfluence` returns immediately on
chapter 0 and `Creature.GrantDungeonKillInfluence` skips a zero id, so boss kills award nothing
while respawns, Tome explore entries and area naming are untouched. Zone 160 keeps `6`/`2`.

Note that until BUG-041 is resolved, area-based influence resolves in exactly one place in the
dungeon: zone 164 carries a `zone_areas` row with `PieceId 0`, and with no `areasNNN.png` the
`AreaPixels` grid is all zeroes, so `GetZoneAreaFor` computes `areaId 0` and matches it. That is
the only reason any Bastion influence exists in the character database at all (46 points on the
pre-script-23 id 129). With script 30 that map now awards nothing, which is the correct live
behaviour, so **no area-driven influence will accrue anywhere in Bastion Stair until the zone
area bitmap problem is solved**. Public quest influence still flows through the `ChapterId`
fallback.
