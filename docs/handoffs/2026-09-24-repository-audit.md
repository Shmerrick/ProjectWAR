# Repository audit and delivery — 2026-09-24

The user requested a fresh assessment, implementation, validation, documentation,
commit and push, then clarified: “Gain scope/context. I dont know where things
stand. Best to trust nothing.” This review treats previous handoffs as leads.
It does not declare the restoration backlog complete.

## Measured state

Sources: the current Release/x64 binaries, the database configured in
`bin/Release/Configs/World.xml`, extracted client `C:/Users/Admin/Downloads/myps`,
and the validation commands below. No game login or Matrix GUI inspection was
performed. No emulator services were running at the initial inspection.

| Area | Fresh result | Limit |
| --- | --- | --- |
| Solution | Release/x64 build passes | Compilation is not gameplay verification |
| Ability loaders | 8,416 Mythic and 4,221 legacy rows load identically through both ORM binders | Does not execute every ability |
| Ability alignment | 4,221 shared rows agree; 6,012 names and 8,372 EffectIDs agree with imported client records | Direct BIN comparison is a separate crosswalk |
| Influence | 419 areas; zero missing nonzero tracks; all 32 ward counters bound | Does not prove all ward events fire |
| Maps | 221 of 261 configured zones lack area maps; 13 of 53 PQ zones lack PQ maps, affecting 38 definitions | Counts include placeholders; missing geometry cannot be invented |
| Gunbad | 24 PQ spawns reference missing prototype 387121; dungeon exit metadata remains NULL | Do not manufacture a prototype or exterior coordinates |
| NPC/vanity lifecycle | Compiled isolation, health-gate and ownership checks pass | Not complete NPC content or a playable acquisition loop |
| Thanquol | Stage/data checks pass; 478 gold-bag rows cover every career | No admission/scenario/full encounter test |
| Skaven | Eight control buffs and three device placements pass data checks | Form abilities, appearance, action-bar behavior and acquisition remain incomplete |
| Tome tactics | 54 checks pass; four named tactics remain unobtainable | The check reports these gaps; PASS does not close them |

The crosswalk reads `data/bin/abilityexport.bin`, `abilitycomponentexport.bin`,
`upgradetableexport.bin` and client strings directly. It reproduces 15 comparable
base-damage disagreements, five buff-duration disagreements, 127 unimplemented
client channels, 49 subsecond cooldown cases (24 stored as zero, 25 not
representable), 574 fractional-foot ranges, and five existing channels lacking a
client component duration. Imported component lists differ on 60 of 11,692
comparisons, with 12 client lists absent from the import; three of 2,760 upgrade
items differ. These remain work, not new values inferred by this audit.

## Plan errors corrected

1. **Stale entry points.** AGENTS/README/STATUS directed readers to September 5
   as the latest state. Older migration numbers refer to a series folded into
   the base dump; the present incremental series is 00–10. Follow this handoff
   for current measurements and retain older documents for their evidence.
2. **Imported blanks were mistaken for client absence.**
   `SKAVEN_PLAY_AS_MONSTER.md` incorrectly ruled out components on controls
   24857–24864. Direct BIN records start at byte 2002266 and then every 194 bytes;
   ability ID is at record +40, and the second component is at +72. Those slots
   hold 26661–26668, all operation 51. Component 26661 starts at byte 2849165 in
   `abilitycomponentexport.bin`. The new check compares parsed records with raw
   bytes. This establishes linkage, not a complete transformation protocol.
   `CONTROL A PACK MASTER (DOK LVL 40 RR 100).log.txt.gz` also exists in the
   toolkit packet corpus; “no Pack Master capture” is not a valid blocker.
3. **Tooltip damage was equated with final damage.** `CombatManager` applies
   attacker/target modifiers, toughness, armor/resistance and critical results
   after `AbilityDamageInfo.GetDamageForLevel`. Compare equivalent stages and
   controlled conditions before changing scaling. This is a code-path observation,
   not verification of those emulator formulas against retail.
4. **PASS was overloaded.** Health audits report gaps; buff-row checks do not
   test the action bar; inferred/confirmed report labels do not prove all operation
   semantics. The four-systems plan must distinguish implemented scaffolding,
   database availability, external evidence and client acceptance.

## Implemented repairs

- `NewChannelHandler` binds queued initialization to the originating cast's
  `AbilityInfo`. Late successful/failed callbacks cannot replace or cancel a newer
  cast, and an obsolete buff cannot send the new channel's start packet.
  `AbilityMgr.GetAbilityInfo` supplies cloned cast definitions; the callback and
  `NewBuff.StartBuff` are the affected runtime paths. This is a lifecycle repair,
  not new retail timing or targeting data.
- The new upgrade-table parser validates its fixed 322-byte disk record count
  against file length before allocating. A corrupt count previously requested an
  enormous list. The disk layout is the existing parser contract, checked against
  the extracted client's 138 records / 2,760 items; it is distinct from the client's
  padded 336-byte in-memory structure described in `ABILITY_DAMAGE_MODEL.md`.
- Five PowerShell database audits now scope a build-local assembly resolver around
  their connections. Windows PowerShell 5.1 previously failed to bind
  `System.Threading.Tasks.Extensions, Version=4.2.0.1` to the staged dependency,
  masking the query result with a Dispose exception. The two failing tests now pass.
- Existing uncommitted ability/Matrix changes and migrations 00–10 are preserved
  for delivery, with their evidence in the September 15/20 handoffs and SQL headers.
  No new SQL or base-dump modification was needed for this audit's repairs.

## Verification

All 14 pre-existing `Test-*.ps1` checks were executed in fresh Windows PowerShell
processes. Twelve passed initially; ArchiveRecovery and WorldDiagnosticData failed
on assembly resolution, then passed after the repair. The added
`Test-ClientDataMatrix.ps1` passes in Windows PowerShell 5.1 and PowerShell 7.
Runtime regression checks pass with the new channel callback cases. Expected
corrupt-map and null-region diagnostics come from intentional fixtures.

Migrations 00–10 were executed in order twice against the configured Release
`war_world`. Both passes preserved all six affected table checksums:

| Table | CHECKSUM TABLE EXTENDED |
| --- | ---: |
| abilities | 3712655059 |
| mythic_src_abilities | 3188395327 |
| ability_damage_heals | 254284521 |
| mythic_src_ability_damage_heals | 1592430249 |
| buff_infos | 2730694477 |
| mythic_src_buff_infos | 3712381461 |

Reproduction: build Release/x64, run the scripts in `tools/validation`, then run
`ClientDataMatrix crosswalk abilities --root C:/Users/Admin/Downloads/myps --output <scratch>`.
`Get-WorldDataHealth.ps1` and `Get-DungeonReadiness.ps1` are read-only audits, not
assertions that the world is complete. Do not re-import base dumps over a populated
database to apply an upgrade.

## Remaining delivery plan

1. Client-test casting/target selection, cast-then-channel, cancellation/recast,
   AP ticks, portals/relog and Matrix filters/icons through the normal launchers.
2. Trace channel-end command execution before converting the 127 remaining
   channels. The independent fractional cooldown migration and its consumers are
   now implemented; see [subsequent handoff](2026-09-24-cooldown-milliseconds.md).
3. Resolve component-to-command/damage-row mappings and the upgrade evaluator's
   object/key join before further scaling changes (BUG-151).
4. Finish Thanquol admission/population ownership and the Skaven transformation
   packet sequence, including Pack Master, before exposing them as restored content.
5. Restore missing PQ geometry, prototypes, exits, acquisition and ward events only
   from identified evidence. Normal dungeon acceptance precedes custom difficulty.

These are separate evidence or gameplay gates. None is closed by committing this
audit. The bounded audit/repair work above is delivered; the restoration backlog
and in-client acceptance remain open.

## Documentation coverage

All repository Markdown files were inventoried and searched, including historical
handoffs, system/architecture docs, plans, patch-note archives and generated Matrix
reports. Current guides and relevant code/evidence were reviewed in detail. The
multi-megabyte generated ledgers and historical patch corpus were inspected by
summary and targeted searches, not manually verified row by row. Historical source
documents are preserved; current entry points and contradictory guidance are updated.
