# Four-system restoration implementation — September 7

The user approved the [plan](../plans/2026-09-06-four-systems-restoration.md).
Implementation is underway; the four systems are **not yet restored end to end**.
This batch changes only NPC casting and vanity companion lifecycle code. It adds
no SQL or invented content. Existing Tome, dungeon and diagnostic edits were preserved.

## Implemented engine repairs

- `AbilityMgr.GetCreatureAbilities` instantiates cached definitions per creature.
  Previously the list and its mutable `NPCAbility` timers/one-shot flags were
  shared by every spawn of a prototype. Disposal could also clear the shared list.
- `ABrain` and `InstanceBossBrain` evaluate each ability's own health-cycle gate.
  Eligibility no longer leaks from a preceding ability in the loop; threshold
  multiplication uses a wider intermediate in this gate.
- Combat reset cancels delayed casts and chase callbacks. Delayed casts validate
  their payload, owning caster and lifetime, reject missing definitions, and only
  change movement after `StartCast` accepts the cast. Delayed chase rejects absent,
  dead or disposing targets. This does not fix every scheduling issue listed below.
- Vanity summons use spawn-local level and a creature-instance scale override,
  instead of modifying the cached prototype. Existing scale-50 behavior is preserved;
  this is not a new claim about retail size.
- Each vanity pet owns its exact summon buff. Replacement, expiry, removal and
  destruction release that source and clear the companion slot only if still owned.
  Old callbacks cannot remove a replacement or another owner's companion. Queued
  loads reject already-dismissed pets and expired sources. Vanity pets no longer
  register the owner's attack callback; career-pet registration remains separate.

These are repository code defects and lifecycle checks, not inferred retail content.
Relevant paths are `WorldServer/World/Objects/{NPCAbility,Creature,Pet}.cs`,
`WorldServer/World/Abilities/AbilityMgr.cs`,
`WorldServer/World/Abilities/Buffs/BuffEffectInvoker.cs` and both named brains.

## New external evidence

`tools/validation/Read-EncounterCaptureEvidence.ps1` streams official gzip logs.
It attributes `F_CAST_PLAYER_EFFECT` caster OIDs to preceding `F_CREATE_MONSTER`
records, invalidating identities on removal, player creation and region switch.
Unknown identities remain unknown. Output counts are **effect packets**, not cast
counts, encounter cooldowns or phase timings.

Layout sources: WAR-RE-Toolkit `libs/protocolservices/Server Packet Protocol/`
`F_CREATE_MONSTER.cs` and `F_CAST_PLAYER_EFFECT.cs`. With the three-byte server
header included, monster OID is at 3, model at 21, state count at 47 and name at
49 + state count. Effect caster is at 3 and ability at 7. The full Thanquol capture's
first monster packet was also inspected in raw hex to check the name offset.

The complete toolkit directory contained 1,027 gzip captures. The effect-attribution
scan found no control effects for 24857–24864 or 24873; this does **not** establish
absence of Skaven control in other packet types. `INSTANCE_SACELLUM_17-20.txt.gz`
attributes effect 5362 to Goremane at packet 17313 (creation 1200).

The [scoped evidence CSV](../evidence/2026-09-07-thanquol-effects.csv) records the
full Thanquol capture's identities, models, creation/effect ordinals and counts.
Source is `THANQUOL INCURSION (FULL RUN WITH EMPTY IGNORE LIST).log.txt.gz`:

| Actor | Abilities observed | First supporting effect packets |
| --- | --- | --- |
| Skeetk | 24865, 24867, 24868, 24893, 24895, 24896 | 6636, 7705, 7079, 7521, 20306, 20424 |
| Throt | 24872, 24874, 24875 | 102023, 70662, 71140 |
| Thanquol | 24879, 24880, 24882 | 104825, 104912, 104781 |
| Boneripper | 24884, 24885 | 109931, 110254 |
| Cloud of Corruption | 23582, 24881 | 108864, 105192 |

The capture has 216,288 frames, 352 monster creations and 56,498 effect packets;
3,434 effects were attributed to known monster identities. The named Lord Slaurith
capture and `thanquollfull+RvR.txt.gz` have zero `F_CAST_PLAYER_EFFECT` packets.
Their empty results cannot establish an inert boss.

Client doctor report for 24879: extracted `data/bin/abilityexport.bin` byte 2006633
has cast time 3000, cooldown 2000, range 1000 and component 26758. That component is
at `abilitycomponentexport.bin` byte 2864209, operation 1, values
`100,0,0,0,0,0,0,0`, extension `2,15,1,8,0,0,1,0,1`. The client name is Warp Lightning
(`data/strings/english/abilitynames.txt:24880`). These raw fields do not by
themselves establish server damage scaling or encounter timing.

## Refreshed local database audit and gates

SELECTs used `bin/Release/Configs/World.xml`; the selected mythic action tables are
enabled. Among 24857–24896, the selected ability table contains only 24881 and
24887, both with `InvokeBuff` commands. Thus Thanquol requires missing action
implementation as well as creature bindings and instance/PQ definitions.

The ten imported itemability links to summon buffs resolve to three existing
selected items (214073, 214223, 214173), all already linked correctly, and seven
missing item definitions (230000, 214002, 214101, 214193, 214162, 214102, 214222).
Exact IDs were searched in extracted `data/gamedata/items.csv` and `itemdata.csv`
without matches. Imported link rows alone do not establish complete item stats,
models or acquisition. Sample summon commands use InvokeOn 5, which includes
the runtime Ended bit 4; the newly handled natural-expiry path is reachable.

Remaining work, with no placeholders represented as completed content:

1. NPCs: accepted-cast versus attempted-cast scheduling, startup action/buff
   identity, one-shot retry semantics and reflection boss timing/dispatch still
   need repairs and tests. The unused state tables do not become functional by
   inserting rows. Complete an evidence-backed ordinary NPC/boss pilot before expansion.
2. Pets: source missing item definitions/acquisition and resolve the three missing
   prototype references in BUG-125. Existing item use needs an actual client
   summon/replacement/RvR/death/relog/career-pet coexistence test.
3. Thanquol: decode admission, session lifetime, PQ stages/rewards and population
   ownership; implement the missing actions. Client zone410 `mappoints.xml:1-7`
   specifies both realms/PQ911 and a 48-player recommendation, not a capacity cap.
   Do not insert a guessed generic instance row or delete repeated populations.
4. Skaven: resolve operation 51 and control/action ownership before form runtime.
   Client 24857 at abilityexport byte 2002266 links component 26661 at
   abilitycomponentexport byte 2849165. Argument 20760's identity domain remains
   unresolved. No transformation/session code or form data has been activated.

## Validation and local delivery

Full solution Release/x64 build passed in isolated output, without compiler
warnings/errors. The WorldServer Release/x64 build then passed into `bin/Release`.
`Test-FourSystems.ps1` and existing `Test-RuntimeRegressions.ps1` passed against
that Release output. Expected malformed-map fixtures log errors while passing;
these are fixture diagnostics, not a clean-startup claim.

New checks exercise actual compiled definition isolation, field preservation,
health thresholds/order/overflow, two-owner buff ownership, replacement and
repeated cleanup, natural/manual end callbacks, cancelled queued loads and invalid
delayed callback/lifetime guards. They do not simulate complete combat or client use.

The prior stack was stopped through ServerLauncher; WorldServer recorded console
control event 2 and WorldMgr stop at 02:51:24. ServerLauncher was reopened to start
the selected services after rebuilding. All four service processes were running
and WorldServer finished startup at 02:53:17. The startup log contained no ERROR
entries; it retained the five known unresolved taxi warnings and one slow-SELECT
warning. No new migration needs application. This is startup verification, not a
gameplay retest.

Reproduce with `tools/validation/Test-FourSystems.ps1` and
`tools/validation/Test-RuntimeRegressions.ps1`. Local generated reports and isolated
binaries live under ignored `bin/FourSystemsArtifacts/`; durable scoped evidence
and this handoff are tracked separately. No in-client four-system test is claimed.

## Thanquol's Incursion: the PQ stage sequence, decoded and implemented

This closes the "PQ stages" half of remaining item 3 above. Rewards, admission,
session lifetime, population ownership and the missing actions remain open.

### What the captures say

The live server runs zone 410 as a **public quest**, entry **911 (0x038F)**, not as a
`boss_spawn` instance encounter — which is why `boss_spawn_abilities` being empty was
never the blocker here. `F_OBJECTIVE_INFO` (0xC1) was decoded out of all three full-run
captures (`Tanquollincursion.txt.gz`, `THANQUOL INCURSION (FULL RUN WITH EMPTY IGNORE
LIST).log.txt.gz`, `thanquollfull+RvR.txt.gz`); the six distinct stage packets agree
across all three:

| Stage label | Tracker title | Objective | Need | Timer | Objective id |
| --- | --- | --- | ---: | ---: | ---: |
| Setup | Setup | *(none published)* | – | 300s | 2531 |
| Stage I | Destroy Siphoning Contraptions | Siphoning Contraptions Destroyed | 2 | none | 2532 |
| Stage II | Dispatch Warlock Engineer Skeetk | Warlock Engineer Skeetk | 1 | none | 2533 |
| Stage III | Destroy the Siphoning Contraptions | Siphoning Contraptions Destroyed | 4 | none | 2534 |
| Stage IV | Dispatch Throt the Unclean | Throt the Unclean | 1 | none | 2535 |
| Stage V | Dispatch Thanquol | Thanquol | 1 | none | 2536 |

Setup is the only timed stage (captured 300 total / 193 remaining). All five numbered
stages send a stage total *and* remaining of zero.

### Three protocol corrections that fell out of the decode

The emulator's `F_OBJECTIVE_INFO` writer was wrong for **every** public quest, not just
this one. `INSTANCE_GUNBAD_PART1.txt.gz` was decoded as a control and confirms all three.

1. **The trailer was two bytes out of place and omitted a string.** The packet carries a
   short stage label ("Setup", "Stage I" … "Stage V") after the stage times, then the
   influence id as a **uint32**, then two zero bytes:

   | | bytes after the description |
   | --- | --- |
   | Gunbad capture | `00 00` `00` (empty label) `00 00 00 41` `00 00` |
   | emitted before | `00 00 00 00` `41` `00 00 00 00` |
   | emitted now | `00 00` `07 "Stage I"` `00 00 00 41` `00 00` |

   Gunbad's `0x41` is influence 65, which `pquest_info.ChapterId` already carried for all
   five of its public quests — the value was right, its position and width were not.

2. **The post-name byte is data, not the literal 2.** Gunbad sends 2 for every stage of
   every one of its public quests; Thanquol's Incursion sends 0 for all six of its.
   `pquest_info.PQType` is what separates the two sets (1 vs 0), so it now drives the byte.

3. **A stage can publish zero objectives.** The Setup stage sends objective count 0 and 2
   in the byte before it, where every stage with objectives sends 0. This is now keyed off
   `QUEST_SCRIPTED_EVENT`.

The client needs two stage strings and there was only one column, so
`pquest_objectives.StageTitle` was added for the long tracker title while `StageName`
stays the short label. Every pre-existing row leaves it empty and falls back to
`StageName`, which is exactly what those rows sent before.

`ClientObjectiveId` was added for the same reason: the ephemeral id in the packet is not
the creature or gameobject entry. Gunbad public quest 181 sends 870/871 while its
`ObjectId` column holds creature 15106. Zero keeps the old behaviour.

### The stage timer problem

`NextStage` arms a `Failed` timer on every stage, defaulting to `TIME_EACH_STAGE`
(540s) when `pquest_objectives.Time` is zero. **2748 of the 2766 objective rows leave
Time at zero**, so that default is load-bearing across the whole world database and
"Time = 0 means no timer" is not a reinterpretation that can be made safely.

But leaving it in place would fail this public quest partway through the Thanquol fight,
which the captures show has no timer at all. Migration 69 therefore adds an explicit
per-stage `NoStageTimer` opt-out, defaulting to 0 so nothing else changes, and sets it on
stages I–V. Setup keeps its captured 300s timer, and a new
`ScheduleScriptedStageAdvance` is what actually advances it — a scripted-event stage has
no kill or click target, so before this nothing would ever have completed it.

`IsDungeon()` now includes zone 410, so a completed run resets on the 8-hour dungeon
timer rather than the 3-minute open-world one.

### Object positions

The four Siphoning Contraptions are `F_CREATE_STATIC` objects with DisplayID 7454; no
prototype existed, so entry **100517** was added, modelled on the Gunbad Nursery Slime
(100515) — the only other destructible PQ gameobject in the database — with the client
"attackable" bit carried in the spawn's `Unks` rather than the prototype.

Captured client positions convert with `worldX = clientX + 57344` (OffX 16 minus the
captures' instance shift of 1) plus the constant +53 / −52 the existing zone-410 rows
carry. That transform reproduces the stored Boneripper row exactly (capture 25746,26018
→ stored 83143,83310), which is what establishes it rather than assumption.

Both contraption stages stage all four objects: the capture creates four at once, Stage I
asks for two, and Stage III ("make sure all four … cease to function") asks for all four
after the stage reset respawns them.

### What is still open here

* **The three bosses are static spawns, not stage-gated.** Skeetk, Throt and Thanquol sit
  in `creature_spawns` at one shared point, all present from the start, in three copies at
  Y offsets 0/+196608/+262144 — one per instance slot. Kill credit works regardless
  (`Creature.SetDeath` credits any death in the public quest's zone, so the stages do
  advance correctly), but a group can currently reach all three in any order. Moving them
  into `pquest_spawns` needs the three-instance-copy question resolved first: the PQ engine
  builds one `PublicQuest` per `pquest_info` row and cannot serve three concurrent copies.
* **Boss stages send influence 1 in the capture; we send 0.** The slot is the influence id
  — Gunbad's 65 proves that — but it varies *within* this public quest (0 on Setup and the
  contraption stages, 1 on the three boss stages), which an influence track should not.
  `pquest_info.ChapterId` is 0 for 911, so all six stages send 0. Cosmetic; unresolved.
* **Gunbad's stage timers are missing from the database.** The captures show 720s for
  public quest 181's boss stage and 600s for 507/508/510, where `Time` is 0 and the 540s
  default is used instead. Worth backfilling from the same captures.
* **Rewards.** `pquest_info.PQType` is 0, so no gold-chest bag is rolled. The capture
  "WARPSTONE SUPPLIES (SKEETK'S, THROT'S AND THANQUOL'S)_ITEMS IN BACKPACK" shows rewards
  arriving as looted containers from the bosses; that loot has not been verified.

### Delivery

`Database/68_thanquols_incursion_encounter.sql` and
`Database/69_pquest_stage_timer_opt_out.sql`, both applied to the local Release
`war_world` and verified by SELECT. WorldServer rebuilt Release/x64 with no warnings.

`tools/validation/Test-ThanquolEncounter.ps1` is new and passes: it checks the stage
order, titles, objective texts, counts, object ids, timers and all four contraption
positions against the captured values, and drives the real `PublicQuest` constructor.
`Test-RuntimeRegressions.ps1`, `Test-PublicQuestData.ps1`, `Test-DatabaseNulls.ps1`,
`Test-TomeTactics.ps1` and `Test-TomeTacticPackets.ps1` all pass. The runtime regression
fixture was updated for the corrected trailer layout and now carries `PQType = 1` to
match the real Gunbad row. **No in-client test of this encounter is claimed.**

## Playable Skaven: operation 51 resolved, form data located

Remaining item 4 above was blocked on client component **operation 51**, whose semantics the
ClientDataMatrix translator reports as `Unknown component operation (51)`. It is resolved.
No Skaven runtime code exists yet; this is evidence, not implementation.

### Operation 51 is career ability-set replacement

Operation 51 has only **39 component rows in the whole client**, and one of them carries its
own description. `mythic_bin_abilitycomponentbin` component 26389:

> *"Manifesting an Aspect of Fire. Normal career abilities have been replaced with those of
> the Aspect's."*

That is the form mechanic: swap the actor's career action set for another one. The same
operation carries the eight Skaven control components.

### The eight control abilities are four forms by two realms

`mythic_bin_ability` names them outright, which the plan had listed only as ID ranges:

| Ability | Name | Op-51 component | Values `[0],[2],[3]` |
| ---: | --- | ---: | --- |
| 24857 | Order Controlled Warlock Engineer | 26661 | 20760, 1, 42 |
| 24858 | Order Controlled Gutter Runner | 26662 | 20761, 0, 41 |
| 24859 | Order Controlled Rat Ogre | 26663 | 20762, 2, 40 |
| 24860 | Order Controlled Pack Master | 26664 | 20763, 3, 39 |
| 24861 | Destruction Controlled Warlock Engineer | 26665 | 20764, 1, 42 |
| 24862 | Destruction Controlled Gutter Runner | 26666 | 20765, 0, 41 |
| 24863 | Destruction Controlled Rat Ogre | 26667 | 20766, 2, 40 |
| 24864 | Destruction Controlled Pack Master | 26668 | 20767, 3, 39 |

Ability **24855** is "Skaven Play-as-Monster". `Value[2]` is a realm-invariant form index
(Gutter Runner 0, Warlock Engineer 1, Rat Ogre 2, Pack Master 3) and `Value[3]` pairs with it
one-to-one; both repeat exactly across the two realm blocks, which is what establishes the
4 x 2 shape independently of the names. The component-to-ability mapping is not inferred from
that pattern — ClientDataMatrix `doctor ability` reads it from `abilityexport.bin`, and 24857
and 24859 were generated to confirm 26661 and 26663 respectively.

### The shared controller

All four forms carry the same three non-51 components, so only the op-51 component differs:

| Component | Operation | Values |
| ---: | --- | --- |
| 26660 | 23 APPLY_ABILITY | 27950 |
| 26669 | 36 SERVER_COMMAND | 304, 27950 |
| 26671 | 13 EVENT_LISTENER | 29 |

Client `abilitynames.txt:27951` gives **27950 = "Play-As-Monster Master Client Controller"**,
and every control ability resolves to effect **4860 = "Skaven PaM - FORM OF... A SKAVEN!"**
(`effects.csv:4471`). SERVER_COMMAND 304's argument list is the server-side entry point and
its command identity is not yet resolved.

### The form action sets, read off the wire

My first pass grouped `data/gamedata/effects.csv:4430-4501` (the delimited `[Start] 4820 - 4889
-- Skaven Play-As-Monster` block) into four kits by ordering, and flagged that the boundaries
between adjacent forms were not independently confirmed. They were wrong, and the corpus settles
it. Three captures show a real form change:

- `CONTROL A WARLOCK ENGINEER (DOK LVL 40 RR 100).log.txt.gz`
- `CONTROL A RAT OGRE (DOK LVL 40 RR 100).log.txt.gz`
- `CONTROL A GUTTER RUNNER (DOK LVL 40 RR 100).log.txt.gz` and `play as a gutter runner.txt.gz`

The server grants the kit by re-sending **F_CHARACTER_INFO (0xBE) subcode 1** once per ability,
each packet three bytes longer than the last. Layout: `uint16 size`, `byte opcode`, `byte
subcode=1`, `byte count`, two header bytes, then `count` entries of `uint16 abilityId` +
`byte flags`, then a trailing zero. Diffing consecutive packets gives the granted set exactly:

| Form | Granted abilities |
| --- | --- |
| Warlock Engineer | 24802 Stored Warp-Energy, 24805 Death Globe, 24806 Warpfire Thrower, 24807 Repair, 24808 Doomrocket, 24809 Warp-Energy Condenser, 24818 Warp-Energy Accumulator, 24853 Running with the Pack |
| Rat Ogre | 24830 Frenzy, 24832 Savage Assault, 24833 Roar, 24834 Charge, 24835 Hurl, 24836 Bash, 24853 Running with the Pack |
| Gutter Runner | 24822 Spin Slash, 24823 Leap, 24824 Snare Net, 24825 Gutter Run, 24826 Sabotage, 24852 Spy, 24853 Running with the Pack |
| Pack Master | **not captured** - no Pack Master log exists in the corpus |

Every granted entry carries flags `0x2E` in the two DoK captures and `0x00` in
`play as a gutter runner`, so the flag is context, not part of the ability identity. Gutter
Runner is confirmed by two independent captures that agree on the whole set.

Corrections this forces on the effects.csv reading: **24853 Running with the Pack is shared by
all three forms**, not a Pack Master ability; Death Globe belongs to the Warlock Engineer; and
Residual Charge, Warp Lightning and Warp Energy Grenade are *not* granted despite sitting inside
the Warlock Engineer stretch of the effect block. Treat the effects.csv ordering as a hint only.
`24854 Tear Down` is also absent from the Warlock Engineer grant, which fits it being offered
only once a deployable exists.

### How a player actually enters a form

Decoded from `CONTROL A WARLOCK ENGINEER (DOK LVL 40 RR 100).log.txt.gz`. It is **a quest, not a
buff**, and the client says so in its own words.

1. The player interacts (`F_INTERACT`, 0xD2) with an object, OID 0x356E, entry 0x2D75 = **11637**.
2. The server answers `F_INTERACT_RESPONSE` (0xE9) subcode 0 with a four-option menu, each entry
   shaped `byte index, uint16 id, 08 01, byte length, string`:

   | Index | ID | Option |
   | ---: | ---: | --- |
   | 0 | 53055 (0xCF3F) | Control a Gutter Runner |
   | 1 | 53056 (0xCF40) | Control a Warlock Engineer |
   | 2 | 53057 (0xCF41) | Control a Rat Ogre |
   | 3 | 53058 (0xCF42) | Control a Packmaster |

3. Choosing one returns `F_INTERACT_RESPONSE` subcode 1 carrying that id plus quest title, body
   and accept text. The body identifies the object - "From the portal emanates a sickly glow" -
   and the accept line is verbatim: **"Accept this quest to control a Warlock Engineer until its
   death."**
4. The client accepts with `F_QUEST` (0x02) carrying the id 0xCF40 and the same object OID/entry.
5. Only then does the server grant the kit, via the repeated `F_CHARACTER_INFO` subcode 1 writes
   described above.

Two consequences worth stating plainly, because both contradict a reasonable prior reading:

* **The form is not permanent and is not a buff.** It is quest-scoped and the client's own accept
  text bounds it at the form's death. A `SkavenFormSession` should therefore hang off quest state
  and a death hook, not off a persistent buff with a duration.
* **All four forms are offered from one object.** Pack Master is selectable at the portal even
  though no capture of playing one exists, so the menu is not evidence for a Pack Master kit.

Nothing on this path exists in the local database: quests 53055-53058, prototype 11637 (neither
creature nor gameobject), and abilities 24857-24864 in `mythic_src_abilities` all return zero rows.

### Was this PTR content that never shipped? No - it shipped in 1.4.0

Settled by Mythic's own patch notes, now saved under `docs/patch-notes/` (the complete
1.0-1.4.8 run; see the README there for provenance). Two earlier drafts of this section were
wrong in opposite directions and are both retracted: the first claimed the captures proved live
deployment on the strength of player chat and a Praag campaign broadcast, which does not
discriminate at all because the public test realm ran the same campaign code with PvP and
players on it; the second concluded the question was unanswerable.

**Game Update 1.4.0 (2 November 2011)** introduced Play as Skaven, and it is tied directly to
Thanquol's Incursion:

> Players that assist in fighting back Thanquol's Incursion during a contested zone battle will
> be able to help take over Skaven troops and use them to assist in the zone battle when the
> Incursion has been repelled. Play as Skaven will be unlocked for all players 15 minutes
> following activation of Skaven by participants in 'Thanquol's Incursion."

> There will be a limited number of Skaven troops available within a lake. These slots are on a
> first-come, first-serve basis and limited to a set amount per battle. The available Skaven
> classes are: Gutter Runner, Engineer, Packmaster, and Rat Ogre.

> While controlling a Skaven troop, the player takes on the form, abilities, and stats of the
> Skaven. Access to inventory or character screens are disabled. Any loot gained while in
> possession of the Skaven will show up when the player resumes control of their own character
> again.

It then stayed in the game and was maintained, which no unshipped feature is: **1.4.1** rebalanced
Packmaster and Gutter Runner abilities, **1.4.3** fixed "Player controlled Skaven Gutter Runners
are no longer able to use Sabotage on Ram's or Oil", and **1.4.5** rebuilt it as a scenario.

**Game Update 1.4.5 (31 January 2012)** - the Grovod Caverns form:

> Participants on either side can assume the role of a Skaven Engineer, Gutter Runner, or Pack
> Master. Each will have a unique role in completing the objectives, so teams will need to
> coordinate to be successful.

> Grabbing the warpstone bomb will cause a Skaven to mutate into a Skaven Rat Orge granting them
> unique powers while running the bomb.

**1.4.7** then lists Grovod Caverns in Bracket 2 of the permanently available scenario line-up,
so it was still live one patch before the end.

Three things this changes about the implementation:

* **There are two deployments, not one.** The RvR lake version from 1.4.0 (portal, quest, all
  four forms, capped first-come-first-serve slots) and the Grovod Caverns scenario version from
  1.4.5, where only three forms are selectable and **Rat Ogre is not chosen at all** - a player
  mutates into it by picking up the warpstone bomb. The `CONTROL A ...` captures are the RvR lake
  version; their Praag campaign broadcast fits that exactly.
* **Play as Skaven is gated behind Thanquol's Incursion.** The unlock is repelling the Incursion,
  then a 15-minute delay, then it opens to all players in the lake. The two workstreams in this
  handoff are one feature.
* **Entry to Thanquol's Incursion is Renown Rank 65+**, stated in the same 1.4.0 notes. That is
  the admission rule BUG-126 was missing, and it is not a player cap.

### Still unresolved - but no longer blocking the server

* **`Value[0]` and `Value[3]` are client-side only, and the server never needs them.** The byte
  sequences for 20760-20767 appear nowhere in any of the form-change captures. The server grants
  a form purely by re-sending F_CHARACTER_INFO subcode 1 with the ability list; the op-51 payload
  is read by the client out of its own BIN to pick the local presentation. Their domain is still
  unidentified - `careerlines_m.txt` (0-24), `careernames_m.txt` (rejected as a career identity
  by this tool's own domain ledger), Londos `Career` (130-154) and Londos `CareerType`
  (20-27/60-67/100-107) are all excluded by range - but that identification is no longer a
  prerequisite for implementing the server side. This retracts the earlier framing of these two
  fields as the blocking gate.
* **Pack Master has no capture.** It is selectable in both deployments - the portal menu offers
  it and the 1.4.5 notes name it - but no log shows anyone playing one, so its kit is the only
  one that would have to be inferred from the effects block, and the other three forms proved
  that inference unreliable. Do not ship a guessed Pack Master kit.
* **Acquisition is decoded for the RvR lake version only.** The portal interact, the four-option
  menu and the quest accept are read off the capture above, and the 1.4.0 notes give the unlock
  (repel Thanquol's Incursion, then 15 minutes, then open to the lake), the capped
  first-come-first-serve slots and the Renown Rank 65+ gate on the Incursion itself. The Grovod
  Caverns entry path from 1.4.5 is **not** decoded: the three Grovod captures never show their
  player taking a form, and the scenario version differs from the lake version in that Rat Ogre
  is not selectable at all.
* **SERVER_COMMAND 304's two readings** (teardown versus controller rebind) are not separated.
* None of the eight control abilities, 24873, or any effect in the 4820-4889 block exists in
  either server ability table, so nothing here is reachable at runtime yet.

### Where this now lives

These findings are encoded in **ClientDataMatrix**, not just in this handoff.
`ComponentSchemaCatalog.TryBuildUnknownOp51StructuralInference` previously described the four
op-51 fields only by value distribution ("high-range ID reference", "small sequential enum").
It now carries the operation's behaviour, the 4x2 Skaven mapping that establishes Value[2] and
Value[3] as a realm-invariant pair, the Value[0]/Value[3] independence proof from components
27992/27993, and the four excluded domains, so the next reader gets it from
`report operations` rather than re-deriving it. `docs/data-matrix/reference/
component-operation-schemas.*` was regenerated to match.

That regeneration also reordered roughly 3,900 lines of the operation tables. The generator is
deterministic - two consecutive runs are byte-identical apart from the timestamp - so this is a
one-time reordering against a copy produced by an older build, not churn that will repeat.

Operation 51 is deliberately left named `SERVER_OP_51` in
`DefinitionCatalog.ComponentOperations`. Its behaviour is established but its retail enum name
is not, and the placeholder convention already used for operations 29, 30, 32, 40, 41 and 47 is
the honest label; inventing a retail-looking name would be the exact mistake the plan warns
against.

### SERVER_COMMAND 304 identified

The other named Skaven blocker. Command 304 takes an **ability ID** in Value[1], naming the
ability whose persistent state it acts on. There are exactly ten rows in the client and all ten
resolve:

| Owning ability | Component | 304 argument |
| --- | ---: | --- |
| Tear Down (24854) | 26677-26680 | 24810, 24819, 24809, 24818 - the deployable Warp-Energy Condenser and Accumulator |
| Detonate (24828) | 26580 | 24826 Sabotage |
| Sabotage (24826) | 26577 | 24826 itself |
| Bolster Down (24945) | 26966 | 24945 itself |
| (unlocated) | 26741 | 5976 Boss Immunities |
| Skaven PaM controls 24857-24864 | 26669 | 27950 Play-As-Monster Master Client Controller |
| (unlocated) | 27996 | 27950 |

"Tear Down" pairs its four 304 components with two DISPEL_BUFF components, and "Detonate"
targets the very ability that planted the charge it detonates, so teardown of the named
ability's persistent object fits every row but the last two. The PaM controls both
APPLY_ABILITY 27950 (component 26660) and issue 304 against it (26669) in the same ability,
which reads more naturally as clearing or rebinding a prior controller before installing this
one - the mutual exclusion a form switch needs. The two readings are not yet separated, and
the owners of components 26741 and 27996 were not located (they fall outside the 24790-24870
sweep).

Recorded in ClientDataMatrix's SERVER_COMMAND Value[1] schema entry, alongside the command
codes the tool already documented (32, 50, 327/328, 332, 173, 87/88/101).
