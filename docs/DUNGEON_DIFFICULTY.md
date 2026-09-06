# Dungeon difficulty work — 2026-09-05

User-requested custom content, after normal Mount Gunbad is complete. This is a design and
delivery checklist, not an implemented feature or a claim about official 1.4.8 difficulty modes.
First scope: Gunbad and Bastion Stair, including their boss maps. Eventually every dungeon
should support Normal, Hard and Nightmare.

## Requirements supplied by the user

- For a below-40 dungeon, Hard starts at 40 and preserves each creature's offset from that
  dungeon's original baseline: `hardLevel = 40 + originalLevel - originalBaseline`.
  The user's hypothetical 35–38 dungeon therefore becomes 40–43; its level-38 boss becomes 43.
- Each dungeon gets its own difficulty definition.
- **Dungeons already at rank 40 or above (specified 2026-09-05):** Hard is the existing rank
  **+1** for every creature and the existing ward tier **+1**. Nightmare is a further **+1**
  level and **+1** ward tier — so +2 and +2 against Normal.
- Investigate Lesser Ward on Gunbad and Bastion Stair hard-mode enemies.
- Nightmare's mechanics and rewards beyond the level/ward steps above are still unspecified.

### What the rank-40+ rule resolves against current data

Both offsets are relative to whatever a creature already carries, so no baseline has to be
chosen for these dungeons and no level or ward is invented. Applied to the ward tiers actually
stored today (0 none, 1 Lesser, 2 Greater, 3 Superior, 4 Excelsior, 5 Supreme):

Measured from `instance_creature_spawns` on 2026-09-05, for every dungeon whose stored creature
levels reach 40 or above:

| Dungeon | Zone | Stored levels | Ward | Hard ward | Nightmare ward |
|---|---|---|---|---|---|
| Tomb of the Vulture Lord | 179 | 1–47 | 0 none | 1 Lesser | 2 Greater |
| Bilerot Burrow | 196 | 1–43 | 1 Lesser | 2 Greater | 3 Superior |
| Warpblade Tunnels 2 | 154 | 40–43 | 0 none | 1 Lesser | 2 Greater |
| Lost Vale | 260 | 1–42 | 2 Greater | 3 Superior | 4 Excelsior |
| Sigmar Crypts | 176 | 1–42 | 1 Lesser | 2 Greater | 3 Superior |
| Warpblade Tunnels 1 | 177 | 1–42 | 0 none | 1 Lesser | 2 Greater |
| Bloodwrought Enclave | 195 | 40–42 | 1 Lesser | 2 Greater | 3 Superior |
| Hunter's Vale | 50 | 1–41 | 0 none | 1 Lesser | 2 Greater |
| Tombs of the Stars/Moon/Sky/Sun | 241–244 | none stored | 0 none | 1 Lesser | 2 Greater |

Four things this surfaces, all of which need a decision before any of it is coded.

1. **Several of these store level 0 or 1 on some rows**, and the four Tombs store no level at
   all, so the effective level comes from `creature_protos.MinLevel..MaxLevel` at spawn time.
   "+1" has nothing to add to on those rows. The mode must resolve the effective level first
   and scale that, which is the same requirement already listed under implementation.
2. The low end of most of these ranges is utility and friendly NPCs, not encounter creatures.
   Scaling must be scoped to the encounter, not applied to every row in the zone.
3. **A `+1` ward from a stored 0** gives Tomb of the Vulture Lord, both Warpblade Tunnels,
   Hunter's Vale and the four Tombs a Lesser ward on Hard. That follows from the rule as
   stated but may not be intended for dungeons that carry no ward at all today.
4. `+1` from a stored 5 (Supreme) has no representable result. Nothing stores 5 today so the
   case is unreachable, but the rule needs a defined ceiling before it is coded.

Whether Hunter's Vale and the Warpblade Tunnels belong in this list at all is a content
question: their stored maxima reach 41–43, but that may itself be Return of Reckoning inflation
of the kind migration 36 removed from Gunbad, rather than an authentic rank-40 dungeon. None of
them has been checked against a capture.

Gunbad (21–33 after migration 36) and Bastion Stair are **not** rank-40 dungeons, so they take
the `40 + offset` rule above, not `+1`.

The capture-backed Gunbad levels restored by migration 36 span 21–33, rather than the
hypothetical 35–38 example. Source: official `INSTANCE_GUNBAD_PART1.txt.gz`, monster payload
offset +21, with identity/sighting counts in `Database/36_restore_gunbad_retail_levels.sql`;
atlas initialization #286846. Choosing 21 as one dungeon baseline would produce 40–52.
The user has been asked whether they intend that or separate wing baselines; neither choice
is configured yet. Do not derive a baseline with SQL MIN: current rows include level-1 utility
creatures, friendly camp NPCs and unverified legacy levels.

## Implementation requirements established by code review

- Freeze difficulty when creating an instance. Include it in instance selection and allocation;
  `InstanceMgr.ZoneIn` currently selects realm copies by instance entry and realm only.
- Carry the mode through boss portals, return portals, death, group joining and reconnects.
  Gunbad boss maps share instance entry 60; Bastion's boss maps have separate entries 163–166
  in the Release database (reproduce with `Get-DungeonReadiness.ps1`). A mode cannot simply
  be selected independently at each portal.
- Copy concrete spawn data before applying a mode. Never mutate cached prototypes or shared
  world/PQ/instance spawn definitions. Normal and Hard must coexist without cross-contamination.
- Cover world creatures, instance trash, bosses, PQ stages, respawns and scripted adds.
  `Instance.LoadSpawns/LoadBossSpawns`, `PQuestObjective.Reset`, region creature creation and
  `InstanceBossSpawn.SpawnAdds` are separate creation paths.
- Resolve the original level once, then scale before `Creature.SetCreatureStats` and initial
  health/packet construction. Audit explicit ability damage, wounds/weapon overrides and
  level-dependent stat lookups; a higher displayed level alone is insufficient validation.
- Review lockout identity/persistence, mode selection, encounter population, rewards and
  group mismatch handling before exposing modes. Shared versus separate lockouts and hard-mode
  loot have not been decided. No new rewards are authorized by inference from level.

## Lesser Ward feasibility

Level scaling review: [Current creature level scaling](CREATURE_LEVEL_SCALING.md) documents
the existing NPC auto-attack bonus beyond a three-level gap. Against effective level 40,
level 44 adds 40% and level 52 adds 360% to DamageBonus, before considering other bonuses.
This emulator rule has not been independently established as retail behavior and needs review
before balancing Hard/Nightmare.

The existing server supports the requested combat behavior. `Creature_spawn.Ward` carries
the concrete tier; `CombatManager.ApplyWardDamageScalar` reads it and applies the player's
earned Tome fragment count, including pet-owner progress. `PQuestObjective.Reset` and instance
loaders already copy stored ward fields. A Hard-only runtime spawn override can set Lesser
without changing normal dungeon assignments. Exclude friendly/utility NPCs through explicit
encounter scope, not by blindly updating every row in the zone.

The combat path currently yields 300% incoming/40% outgoing with zero fragments and
100% incoming/115% outgoing with five. Client mechanic evidence and its limits are recorded in
`docs/WARD_SYSTEM.md`: extracted `data/bin/abilityexport.bin`, `abilitycomponentexport.bin`,
abilities 12975–12979, and `interface/interfacecore/tome/sigils/`. This supports reusing the
mechanic; the user's request is the authority for assigning it to custom Hard encounters.
`Instance_Info.WardsNeeded` is not the creature ward. No entrance requirement is implied.

## Normal Gunbad completion gate

Use this as an end-to-end retest record; passing automated checks does not tick these off.

| Area | Required validation | Current state |
| --- | --- | --- |
| Nine PQs | Tracker, every stage, interaction, kill credit, reset and loot | User previously reported improvement; complete sweep pending |
| Squig Nursery | Nursery Slime, Writhing Effigy, squig credit and finale | Migration 38 applied; client retest pending |
| Shadowweb | Restore 24 missing prototype-387121 rows from evidence | BUG-069 open; nearest-name/position guesses rejected |
| Boss routes | All four entries, fights, return portals and death release, both realms | Full sweep pending; reported jump 629156888 unresolved (BUG-073) |
| Influence | Own-realm bar and awards across all wings | Left-wing improvement reported; full sweep pending |
| Rewards | Chest visible, winner can loot, inventory-full retry, unclaimed mail | Full sweep pending; BUG-081 reported in LotD, not proven in Gunbad |
| Persistence | Relog recovery, group joining, kill lockout, expiry/reset | Automated coverage only for part of this; live round trip pending |
| Original population | Remaining uncaptured levels/identities and scripted mechanics | Gaps in `MOUNT_GUNBAD.md`; no inferred replacements |

## Current Release audit

`tools/validation/Get-DungeonReadiness.ps1` was executed against the configured Release world
database on 2026-09-05, starting from commit f6f022c3. It changes no data.

- All nine Gunbad PQs have objectives and spawn rows. All nonzero-type Gunbad objectives
  have their own spawn rows; this does not establish playable stage progression.
- Gunbad still has 24 missing creature rows for prototype 387121. The repaired Nursery
  object prototypes are no longer missing. One zone-60 spawn belongs to a zone-103 PQ.
- Bastion has missing PQ objects 2000687 and 100536, one row each, and 24 zone-163 world
  creatures with missing prototypes. Arena Challenges (501) has no objectives/spawns.
  These require investigation, including whether particular rows are inactive metadata.
- All ten Gunbad/Bastion instance-info rows have empty exit jump fields.
- All audited creature spawn ward values are zero. No Hard ward assignments were made.

Runtime regressions and the existing SELECT-only PQ construction checks passed against the
current binaries. The latter covers Holmsteinn and Destruction of the Weak, not Gunbad.
No server was started, no game test performed, and no SQL or runtime gameplay code changed.
