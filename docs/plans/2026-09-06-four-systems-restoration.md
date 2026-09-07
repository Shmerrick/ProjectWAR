# Creature abilities, item pets, Thanquol, and playable Skaven

Status: **approved by the user's “Go ahead”; implementation in progress**.
Scope authority: the user's September 6 request and subsequent approval. The
original planning pass below used SELECTs and source inspection only. The
[September 7 implementation handoff](../handoffs/2026-09-07-four-systems.md)
records subsequent code changes, validation and remaining evidence gates.

## Recommended delivery order

1. Verify the selected ability sources and repair the NPC casting paths needed by
   a small, capture-backed pilot. Prove one ordinary NPC and one boss work, then
   restore additional encounters in individually verified batches.
2. Repair the vanity companion lifecycle and prove an existing item works safely.
   Restore additional authentic item links and acquisition paths where evidenced.
3. Establish Thanquol's admission, shared encounter, and PQ contracts; build a
   zone-410-specific implementation and restore its encounters using the NPC work.
4. Establish the Skaven control protocol; implement reversible form sessions,
   one form first, then all four forms for both realms and their acquisition rules.

Evidence gathering for all four starts in the first approved batch. Pet work does
not depend on completing every NPC, and Skaven research does not wait for Thanquol.
Do not make completion of the entire world's NPC catalogue a gate on either.
Deliver playable, evidence-backed subsets while keeping the unresolved ledger open.

## Corrections to the initial assessment

These are measurements of the configured local Release database on September 6,
not counts inferred from the untouched base dumps or proof of in-client behavior.

| Initial claim | Audit result | Planning consequence |
| --- | --- | --- |
| 3,545 creature-ability rows; 2,301 of 9,796 spawned prototypes have abilities | Reproduced for distinct `creature_spawns.Entry`; the ability table contains 2,734 distinct prototype IDs overall | The 76.5% complement includes utility/friendly objects and excludes other spawn sources. It is not a measured percentage of hostile creatures that only auto-attack. |
| All 22 bosses are inert | `boss_spawn` has 22 rows, but `instance_boss_spawns` has 82 rows / 76 distinct prototypes. Nineteen instance boss rows already reference creature abilities. Five of the 22 `boss_spawn` rows also reference them | Map actual spawn-to-brain wiring; do not mass-author against the wrong boss ID domain. Existing abilities are not proof their runtime effects work. |
| Empty boss/state ability tables imply a ready engine needing only inserts | `boss_spawn_abilities`, `instance_spawn_states`, and `instance_spawn_state_abilities` are empty. State entities have no discovered runtime loader/consumer. Most inspected instance classes select `InstanceBossBrain`, not `BossBrain` | Inserting state rows alone cannot activate this path. Choose an engine by actual consumer and required mechanics. |
| 111 pet buffs, 23 items, 89 pets missing | Both source families contain 111 summon commands/buffs for 106 distinct non-null prototype IDs. The 23 items reference 22 distinct buffs. Eighty-nine buffs lack direct item links, spanning 87 non-null prototype IDs | Do not invent 89 items or equate buffs with unique pets. Follow the whole item/ability/buff/prototype chain. |
| Pet mechanism already works | Three summon commands have unresolved prototype references: 28741 and 28743 have NULL PrimaryValue; 28742 names absent prototype 33602 | Repair lifecycle first; restore only evidenced identities. |
| Thanquol just lacks instance metadata | No `instance_infos` row, no zone-410 instance trash, no PQ row for zone 410 or client ID 911. World spawns include three each of Throt, Skeetk, Thanquol and Boneripper; 15 clouds; 76 invisible markers; three objects | Do not create a second population over the existing one, or delete triplicates by assuming they are duplicates. |
| Skaven is server-blank and client-complete | The eight control IDs and 24873 are absent from both ability tables; 13666 exists in both. `Chickenize`/`IsPolymorphed` already provide partial transformation machinery. Client control abilities have BIN components but incomplete semantic coverage | Reuse verified primitives, not chicken behavior wholesale. Eight wrapper abilities do not establish all form actions, permissions, costs, or lifecycle. |

Ability identity needs its own check: 14 current `creature_abilities` rows lack an
entry in `mythic_src_abilities`. This differs from the older BUG-009 count of 22;
do not silently replace that count without documenting source selection and query
scope. Both item and action loaders use `UseMythicActionCoverageTables`; preserve
Claude's current Tome changes and the two source families.

## Evidence and implementation boundaries

Authority order stays client, official captures, decoded toolkit findings, then
emulator code/data. A client ability defines an action; it does not by itself say
which NPC casts it or when. A packet effect ID is not automatically an ability ID.

An evidence row must identify system, concrete subject, field, value, source file,
offset/packet/record key, coordinate or ID domain, confidence, and unresolved parts.
No SQL row graduates from a name match or similar nearby creature alone. Existing
emulator tables are leads. The toolkit's `data/database-tables/Londos Server v2/
War_ItemAbility.sql` is an item-link lead, not higher authority than the client.
The local `mythic_bin_itemability` has ten direct matches to summon buffs; provenance
and actual runtime reachability still need checking.

Only changes required by these four systems are proposed. Shared files may receive
small, necessary hooks or bug fixes, with regression coverage for their existing
users. No general ability rewrite, ORM optimization, bot work, Tome-tactic work,
taxi repair, global PQ/LOS reconstruction, new difficulty modes, blanket creature
level/ward changes, or unrelated loot authoring belongs in this work.

Existing uncommitted work is preserved. New SQL uses the next free incremental
number at implementation time, guards its original values, preserves originals,
and is applied and verified on the configured Release database twice. Do not
reserve numbers now: migrations through 58 already exist in the working tree.

## A — NPC and boss abilities

### Evidence and wiring

Inventory world, PQ, instance-trash and instance-boss spawns separately. Resolve
each pilot's concrete spawn, prototype, boss key, runtime class, brain, selected
ability definition, commands, buffs, target rules and every referenced entity.
Exclude friendly/utility NPCs by verified identity and purpose, not missing skills.

Start with an ordinary creature from a captured encounter and a named boss with a
complete capture. Lord Slaurith is a research candidate, not a predetermined skill
assignment: `BASTION STAIR - RIGHT WING DOR LVL 40 RR 100 LORD SLAURITH (BOSS).log.txt.gz`.
Other available sources include `bastion_stairs.txt.gz`, `INSTANCE_GUNBAD_PART1/2`,
and named Bilerot/Bloodwrought boss captures. Index object creation/destruction and
OID reuse before attributing casts. Separate player/pet casts from the boss's casts.
Record observed timing separately from claimed cooldowns or phase thresholds.

### Required engine checks before authoring

- `ABrain.BuffAtCombatStart` looks up a buff using an ability ID and queues it even
  when missing. BUG-052 already documents the 1900 identity conflict. Resolve the
  ability-to-command-to-buff chain and correct casting for the intended target;
  do not manufacture a same-ID buff.
- `ABrain` and `InstanceBossBrain.TryUseAbilities` share an
  `AllowPercentAbilityCycle` local across the ability loop. Prevent one eligible
  ability from authorizing another's health gate. Review delayed casts for target
  changes, reset/death/disposal and repeated scheduling before the delay fires.
- `BossBrain.FilterAbilities` initially schedules with `GetTimeStamp()` seconds,
  while execution compares against tick milliseconds and subsequently uses
  `CoolDown * 1000`. Use one clock unit and explicit first-cast/cooldown semantics.
- `BossBrain.ExecuteStartUpAbilities` reflects on the brain, whereas normal
  execution reflects on `ExecutionManager`. Validate method names/signatures at
  load, resolve delegates once for a used path, and fail an invalid definition
  without bringing down the region thread.
- Recheck phase, condition and target at execution. A queued action must not remain
  eligible after leaving its phase. Reset cancels pending casts/adds/timers and
  restores only this encounter's state. One boss must not mutate another's timers.

Prefer existing `creature_abilities`/`InstanceBossBrain` for ordinary casts.
Use a bounded encounter controller for phases, adds and objectives when needed;
repair/reuse `BossBrain` only where actual wiring and mechanic requirements justify
it. Do not activate both schedulers for the same action. Do not build a new generic
scripting framework solely to populate unused tables.

### Acceptance

Exercise combat-start enemy action versus self buff; failed cast; interrupted cast;
range/LOS and immune targets; exact health boundaries; crossing multiple thresholds
in one hit; heal-back behavior; target death during the delay; wipe/re-pull; boss
death; instance disposal; two copies of one boss. Verify actual cast/effect packets,
damage or buff effects, add cleanup and reward-once behavior. Then run the pilot in
the client. Table counts and successful compilation do not pass this gate.

Expected change surface: `ABrain`, `InstanceBossBrain`, conditionally `BossBrain`
and its condition/execution binding, relevant per-encounter classes, and narrow
ability-service validation. Only the verified pilot rows and their dependencies
are eligible for each data migration.

## B — Item pets

First demonstrate the whole existing chain: usable inventory item -> selected
SpellId action -> summon buff/command -> prototype -> companion -> dismissal.
Prove identity from source before choosing a pilot: low-numbered existing items
are emulator records, not automatically authentic retail items.

Required lifecycle changes are local to `SummonVanityPet` and vanity ownership:

- Never write scale or level into `CreatureService`'s cached prototype. The current
  command does both; `Creature_spawn.BuildFromProto` retains the same reference.
  Use per-spawn data or a fully isolated copy as appropriate, without altering
  combat pets or ordinary creatures sharing that prototype.
- Bind the companion to its specific owner and source buff. Only that binding may
  remove it. Re-summoning replaces the prior companion once; a late callback from
  the prior buff must not remove the replacement.
- Replace the hardcoded 15188/15190 cleanup in `Pet.RemoveVanityPet` for this path.
  It currently attempts to remove 15188 even after finding 15190. Handle normal
  expiry as well as manual removal, owner death, world departure, logout and RvR
  changes; confirm the intended combat rule from client/capture evidence.
- Check null/type/state guards and combat-pet coexistence. A vanity companion must
  not acquire combat-pet attacks simply because it shares `Pet` infrastructure.

Audit direct and indirect item links, buffs, models, prototypes and acquisition
sources. Restore legitimate existing missing links before considering new items.
Unknown colour variants, item IDs, vendors, prices and loot sources remain unknown;
do not generate them by subtracting counts. A GM-only diagnostic proves mechanism,
not that normal players can acquire the pet.

Acceptance: two owners using the same prototype at different levels; repeated
summon; old-buff removal after replacement; expiry; death; logout/relogin; region
change; RvR/combat transitions; summon failure; simultaneous career pet. Verify
source-table parity and one evidenced acquisition-to-use-to-dismiss loop in-client.
Expected surface: vanity buff command, `Pet`'s vanity-specific cleanup, minimal
owner lifecycle hooks and verified rows in both item/action source families.

## C — Thanquol's Incursion

Direct client evidence: `interface/interfacecore/maps/zone410/mappoints.xml:1-7`
names Thanquol's Incursion, public quest 911, `realm Both`, and `players min=48`.
The 48 value is a UI participation hint, **not proof of a server admission cap**.
It does establish that simply assuming an ordinary isolated six/24-player raid is
insufficient. Read the encounter in
`THANQUOL INCURSION (FULL RUN WITH EMPTY IGNORE LIST).log.txt.gz` and
`thanquollfull+RvR.txt.gz` before deciding admission and realm interaction.
These files were located in this planning pass; their full encounter choreography
has not yet been decoded or claimed verified.

Both current jumps (2114953/2114954) are enabled Type 5 with InstanceID 410.
`InstanceMgr.ZoneIn` requires missing `instance_infos` metadata and treats Type 5
as a generic 24-player raid. Zone 410 currently has Type 3. The prototype-specific
portal case in `GameObject` calls `ZoneJump`; it does not implement the encounter.
Release has `los/410.bin`, but neither area/PQ overlay was found under
`bin/Release/zones/zone410`. Resolve the configured zone root before changing assets.

Sequence after evidence establishes the contract:

1. Define encounter identity, both-realm admission/queue/entry eligibility, capacity,
   ownership, lifetime, reset, reentry and lockout policy. Do not copy another
   dungeon's lockout or turn the map hint into a cap.
2. Build a zone-410 encounter session and admission adapter. Reuse instance transport
   and packet transforms where valid, with a policy scoped to this encounter.
3. Reconcile world populations against capture identity, position and lifecycle.
   Establish why there are three of each major NPC before moving/disabling rows.
   Migrate only with preserved originals and exactly one population owner.
4. Restore the captured PQ stages, interaction triggers and boss mechanics. Determine
   whether entry-based PQ membership is supported by evidence; do not draw a whole-
   zone PQ boundary merely because an overlay is missing. Keep global BUG-041 work
   outside this plan.
5. Add evidenced completion/rewards and exit/death/reentry handling. Do not claim an
   encounter complete until progression, reset and rewards work for both realms.

Acceptance: both entrances, correct world/instance coordinates, appropriate realm
visibility and combat, admission races, full-instance refusal, wipe, disconnect,
boss death, repeated completion, exit/death/reentry, simultaneous copies and restart
recovery. Existing Gunbad/Bastion group and realm-instance regressions must pass.
Expected surface: zone-410 definitions/controller and portal/admission adapter,
minimal instance/PQ hooks, and evidenced 410-only world/PQ/instance rows.

## D — Playable Skaven

The eight control entries 24857–24864 and Command Rat Ogre 24873 have client BIN
records; existing coverage lists them as Partial. A fresh ClientDataMatrix report
for 24857 reads `data/bin/abilityexport.bin` at byte 2002266 and components
26660/26661/26669/26671 at bytes 2849032/2849165/2850229/2850495 in
`abilitycomponentexport.bin`. Component 26661 has operation 51, whose meaning is
not resolved by the current translator. Do not turn its numeric arguments into
invented model/career IDs. `docs/data-matrix/overview/path-forward.md` explicitly
records that remaining semantic gap.

The client also exposes keep Skaven current/max information in
`interface/default/easystem_tooltips/source/maptooltips.lua:575-607`. Tunnel pins,
such as zone001 `mappoints.xml:125-133`, do not establish playable-form eligibility
or prove a Tier-1 unlock mechanic. Research acquisition and control separately from
Thanquol's encounter; no dependency between those gameplay unlocks is assumed.

Proposed server abstraction: a transient `SkavenFormSession` with explicit owner,
form, realm, source, session generation, granted actions, stat/effect handles and
termination reason. Keep the persisted career, purchased abilities, equipment,
Tome progress and other permanent state intact. Apply temporary overlays/grants
and remove only resources owned by this session; never restore a stale snapshot
over legitimate intervening changes. Separate Rat Ogre/Packmaster control links
from ordinary attack targeting and from vanity companion ownership.

State transitions: Normal -> Acquiring -> Active -> Releasing -> Normal.
Failed acquisition rolls back every applied grant and reserved slot. Every delayed
callback checks the session generation. Death, logout, disallowed zoning, source
loss or release must end the form exactly once; exact retail end conditions and
restart/reconnect behavior remain evidence gates. Persist only explicitly proven
durable state; a crash must not leave a permanently transformed character.

First prove one evidenced form's appearance, action bar, targeting, movement,
combat and complete reversal with a development-only entry path. Then expand to
the four forms for both realms, implement Command Rat Ogre/Packmaster interactions,
and expose the evidenced normal acquisition path and keep availability updates.
A transformed model alone is not a playable form.

Acceptance: both realms, form-specific actions, forbidden base-career actions,
resource/cooldown handling, death, disconnect/reconnect, zoning, re-acquisition,
simultaneous requests, role exhaustion, opponent control attempts, controller loss,
failed casts, session teardown and unchanged normal-player behavior.
Expected surface: a focused form-session service, selected buff/ability commands,
minimal player/action-bar/lifecycle hooks and verified form definitions. Campaign
changes are limited to proven Skaven acquisition/availability requirements.

## Logical fault review

| Counterexample | Why the initial shortcut fails | Required plan constraint |
| --- | --- | --- |
| Insert state abilities into an unused table | No runtime consumer means no effect | Trace and test the actual loader/brain first |
| Use prototype IDs as every boss key | Goremane's `BossId` comes from an instance row; other tables have distinct keys | Resolve concrete spawn/key domains |
| Queue an ability at a seconds timestamp and compare to milliseconds | A first-cast delay can expire immediately | One clock unit; fake-clock boundary test |
| Queue a phase-0 skill, move to phase 1 before it fires | Stale queue fires the wrong phase | Revalidate conditions and generation at execution |
| First health-gated skill authorizes the rest of the loop | Later skills bypass their own threshold | Per-action eligibility and multi-skill boundary tests |
| Infer hostile inactivity from all world prototypes | Friendly/utility records distort the denominator | Separate populations and measure actual casting |
| Summon one pet at level 10 and another at level 40 | Shared prototype writes change other users | Immutable definitions; per-instance overrides |
| Old summon buff expires after replacement | Owner.Companion may now refer to the new pet | Source-buff/session ownership check |
| Create 89 items from 89 unreferenced buffs | Buffs are not unique pets; item acquisition is unproven | Evidence-backed joins and distinct counts |
| Insert only Thanquol instance metadata | No PQ, encounter policy or population transfer | Admission, controller, data and transport gates |
| Treat `players min=48` as capacity 48 | UI recommendation is not admission policy | Packet/client protocol evidence for capacity |
| Remove triplicate Thanquol NPCs | They may encode distinct placements/phases | Preserve rows until lifecycle is established |
| Paint an entire PQ map because its pin exists | A map pin does not define membership geometry | Evidence-backed membership, no global overlay invention |
| Restore a player's entire pre-form snapshot | Legitimate intervening changes get overwritten | Session-owned reversible overlays |
| Populate legacy tables and test those counts | Release selects Mythic source tables | Test selected runtime definitions and both supported sources |
| Require every NPC restored before starting pets/Skaven | Unrelated evidence gaps stall all four systems | Independent workstreams and per-content gates |

These are source/data contract checks and design counterexamples, not successful
gameplay tests. The companion manifest provides the dependency and lifecycle
scaffolding used for the executable plan-consistency check.

### Planning checks performed

- Read-only Release queries reproduced the counts above, including both source
  families, instance/world populations and missing action/prototype references.
- The existing ClientDataMatrix executable generated a fresh 24857 report from
  the extracted client, successfully resolving the cited BIN offsets. Output was
  confined to the temporary directory; the existing generated ledgers were not
  overwritten.
- The manifest parsed successfully. All 19 task IDs are unique, every dependency
  resolves, the dependency graph is acyclic, every system has an evidence stage,
  and every data task directly depends on its own system's evidence stage.
- Independence checks confirm Thanquol does not depend on completing worldwide
  NPC expansion and Skaven research does not depend on Thanquol completion.
- Twenty abstract lifecycle cases (five candidate termination reasons from each
  of four states) reach Normal through cleanup where needed; repeated termination
  remains stable. Failed acquisition routes through cleanup. These checks prove
  reachability in the proposed model, not that runtime cleanup already works or
  that every candidate termination reason is a retail rule.
- Planning/tracker whitespace checks passed. No server build was required for
  this documentation-only change. Implementation regression tests remain pending.

## Validation and delivery gates after approval

For each implementation batch: targeted compiled-runtime checks, no new warnings,
Release/x64 build, relevant existing regressions, guarded SQL application twice
where applicable, SELECT verification and runtime loader verification, then a
ServerLauncher-mediated client test. Do not start the stack or mutate live data
as part of this planning pass. Test failures keep that batch unverified.

Separate code readiness, applied-data readiness, verified gameplay and remaining
restoration coverage in the report. Unknown optional content does not block a
verified subset; an unknown admission/identity/mechanic contract blocks only the
dependent path. Do not fill it with a guess. Rollback removes only this batch's
configuration/definitions and restores its archived originals; character rewards
or purchases must not be erased as a generic rollback step.

## Documentation context

The context pass inventoried and searched all 50 Markdown documents present under
the repository's normal documentation/source roots (excluding packages, binaries,
dependencies and build output). Large generated data-matrix ledgers were searched
for relevant entries and their limitations; this is not a claim of manually
reviewing every generated ledger row. System docs and handoffs were read alongside
the code and SELECT audit; newer retests supersede earlier confirmations.

Relevant constraints come from AGENTS/README/CLAUDE, CROSS_REPO, STATUS, the internal
tracker, all handoff inventories, BASTION_STAIR, MOUNT_GUNBAD, WARD_SYSTEM,
DUNGEON_DIFFICULTY, CREATURE_LEVEL_SCALING, LAND_OF_THE_DEAD, the master audit,
client-data-matrix usage/overviews/reference entries, and validation docs.
Bot/API, guild and LOS docs establish neighboring systems to protect, not extra
work authorized by this request. In particular, retain migrations 32/33's influence
keys, per-spawn wards, instance atlas handling, lockout corrections and current
Tome purchases/tactics; none is a shortcut for these four systems.
