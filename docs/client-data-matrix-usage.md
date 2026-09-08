# ClientDataMatrix Usage

`ClientDataMatrix` supports both a GUI workflow and the original command-line workflow.

## GUI Launch

Launch the GUI by running the executable without arguments, or by double-clicking the executable in Explorer:

```powershell
.\bin\Debug\ClientDataMatrix.exe
```

You can also launch the GUI explicitly:

```powershell
.\bin\Debug\ClientDataMatrix.exe gui
```

Default roots:

- extracted root: resolved by `ExtractedDataRootResolver`, which takes the first of these that
  exists — `--root` if passed, then `C:\Users\Admin\Pictures\WAR_extracted`, then
  `C:\Users\Admin\Downloads\myps`, then `data\WAR_extracted`, then `..\WAR_extracted`. On this
  machine only `C:\Users\Admin\Downloads\myps` is present, so that is what it resolves to; the
  `Pictures\WAR_extracted` default no longer exists. See `docs/CROSS_REPO.md` for the full data-root map.
- output root: `docs\data-matrix`

## GUI Workflow

1. Confirm the extracted root points at the WAR client extraction tree.
2. Confirm or change the output root where generated docs should be written.
3. Click `Reload Data`.
4. Use `Generate All` on the main page when you want to build every shared ledger in one pass. If the current ability id box contains a valid value, it also generates that ability report.
5. Use `Clean Temp` on the main page when you want the tool to remove `tmp-data-matrix*` folders and local `ClientDataMatrix` log files from the repository workspace.
6. Use `Ability Doctor` to:
   - search the catalog by ID or name
   - select an ability
   - generate the report
   - inspect the generated ability tree in-app
   - read the `What Happens` narrative tab for the inferred ability flow
   - open the `Definitions` tab to decode numeric fields like `CareerLine`, `TargetType`, `AbilityType`, `AttackType`, trigger values, and component operations
   - inspect inferred requirement links and linked `abilityrequirementexport.bin` rows when the tool finds `ExtData[*].Val6` values that match known `RequirementId` rows
   - double-click any definition row to open the field-domain explorer and see every known raw value, its plain-English meaning, and the source file or offset where that mapping came from
   - open the markdown file or output folder
7. Use `Conflict Ledger` to:
   - generate the full conflict ledger
   - browse conflict counts grouped by domain
   - use the `High` column in the domain grid to see which domains still contain real critical/high work after filters are applied
   - inspect the first 500 conflicts for the selected domain, ordered by triage score instead of raw subject order
   - toggle `Hide Blank String Noise` to suppress the high-volume blank-vs-localized text mismatches while triaging stronger conflicts like `EffectId`
   - leave `Hide AbilityId-Mirror EffectId Pattern` enabled when you want to suppress the routine `abilities.csv EffectId == AbilityId` cases and focus on the narrower `EffectId` disagreements that do not follow that mirror pattern
   - toggle `High-Signal Only` when you want the tab to collapse to just `Critical` and `High` conflicts after the other filters have been applied
   - use the summary panel and category column to separate the remaining `EffectId` work into `AbilityIdMirrorEffectId`, `MountOverlayEffectId`, and `ZeroVsEffectIdGap`, and to split string mismatches into `PlaceholderStringMismatch`, `InternalAbilityNameMismatch`, and `InternalOnlyAbilityNameMismatch` instead of treating every text disagreement as the same problem
   - use the `Resolve To` column when you want the tab to show the current canonical recommendation directly in the grid, including the preferred source family, raw value, and decoded effect name when available
   - inspect the `Conflict Profile` panel for the selected row to see the decoded subject, triage category, and source pattern without manually reading every claim first
   - inspect the `Value Meanings` grid to compare raw conflict values against decoded effect names or unit renderings before dropping to claim-level evidence
   - inspect the claim-evidence grid for the selected conflict so you can see the exact values, source file, field name, and confidence without leaving the GUI
   - double-click an ability-backed conflict row to jump straight into `Ability Doctor` and generate that ability report
   - open the full markdown ledger on disk
8. Use `Token Dictionary` to:
   - generate the COM token glossary from extracted client strings
   - review plain-English definitions for tokens like `COM_0_DURA_SECONDS`, `COM_0_RADI_FEET`, and `COM_0_VAL0_DAMAGE`
   - see whether each definition is `Confirmed`, `Inferred`, `Unknown`, or `Londo`
   - inspect context tags like `Knockback`, `Knockdown`, `Immunity`, and `CrowdControl` that were inferred from the surrounding client text
   - open the generated markdown glossary on disk
9. Use `Coverage` to:
   - generate a whole-dataset ledger of ability readiness
   - browse which abilities are `MappedWithRequirements`, `Mapped`, `Partial`, or `StringsOnly`
   - see exactly which extracted-client pieces are missing for each ability, such as BIN rows, effect rows, localized text, or component rows
   - open the generated markdown ledger on disk
10. Use `Remaining Work` to:
   - generate a consolidated backlog that pulls together coverage gaps, high-signal conflicts, unknown or structural operation fields, unresolved requirement rows, token gaps, and identity-domain risks
   - use the area grid to focus one backlog slice at a time instead of manually cross-reading multiple tabs
   - toggle `All Areas`, `Priority`, `Search`, and `Top` filters when you want a focused backlog slice instead of the full area list
   - inspect the item detail panel for the current summary, evidence, and suggested next action
   - double-click an item with an example ability when you want to jump straight into `Ability Doctor`
   - open the generated markdown overview on disk
   - open the generated `Next Batch` markdown when you want the default top-50 `Critical`/`High` work queue across all areas
   - open the generated `Field Packets` markdown when you want the top operation-field hotspots expanded into value evidence, companion-field summaries, and sample abilities
11. Use `Requirements` to:
   - generate the requirement ledger from `abilityrequirementexport.bin`
   - browse each `RequirementId` row in a summary grid with direct-ability counts, component counts, parent/child requirement links, and context tags
   - inspect the raw exported ext-data rows for the selected requirement
   - inspect active ext-data fields and see when `ExtData[*].Val6` behaves like a nested `RequirementId` pointer under the current narrow rule
   - inspect inbound and outbound links for the selected requirement, including which abilities or components pointed at it
   - open the generated markdown ledger on disk
12. Use `Operation Schemas` to:
   - generate an operation-family ledger from extracted client BIN rows and client string evidence
   - inspect which fields are non-zero for each component operation
   - see recurring sample values, COM-token renderings, semantic summaries, and confidence levels for those fields
   - inherit shared evidence for stable fields like `Duration`, `Interval`, and `Radius` when an operation lacks its own direct token rows
   - spot inline requirement-reference hints when `ExtData[*].Val6` values exactly match known `abilityrequirementexport.bin` rows
   - browse sample abilities that use the selected operation, including trigger text and client-text excerpts
   - focus first on priority operations such as `CC`, `APPLY_ABILITY`, `KNOCKBACK`, and `IMMUNITY`
13. Use `Unknown Triage` to:
   - rank remaining `Unknown` and `Structural` component fields by triage score, priority, and observation count
   - treat `Structural` confidence as a partial decode: the field role is inferred from extracted BIN clustering, but exact per-value semantics still need manual work
   - use those partial decodes to separate recurring layout roles such as `DAMAGE` / `BONUS_TYPE_ADJUST` / `APPLY_ABILITY` / `CC` / `KNOCKBACK` / `IMMUNITY` `ExtData[*].Val1/2/3/4/5/6/7/8/9` blocks, generic `FlagsRaw` masks, `CC` `Value15`, and `KNOCKBACK` `Value[0]` / `Value[1]` / `Value[2]` / `Value[3]` from truly opaque fields
   - treat `Inferred` named control fields such as `ActivationDelay`, `ConeAngle`, `FlightSpeed`, and `MaxTargets` as mostly solved: the field name and value shape are strong enough to use in the GUI even when no direct token row exists
   - leave `Hide Multiplier Noise` enabled when you want the list to stay focused on ext-data, flags, delays, target limits, and other higher-signal unknowns
   - focus on the highest-impact unresolved fields before lower-value tail work
   - inspect the `Value Evidence` grid for the selected hotspot to see which raw values dominate that field
   - inspect the `Value Profile` panel for the selected raw value to see its trigger mix, context tags, and strongest non-multiplier companion fields
   - inspect the `Correlated Fields` grid to find recurring `Val1`/`Val3`/`Val4`/`Val7` style clusters around a selected raw value without leaving the GUI
   - inspect sample abilities for the selected raw value so each unknown stays grounded in client text and trigger context
   - double-click a sample ability to jump straight into `Ability Doctor` for that row
14. Use `Source Status` to review file load success, row counts, and parse failures.
15. Use `Log` to keep a timestamped execution trail for the current session.

## Definition Explorer

The `Definitions` tab is now a field-domain browser, not just a static decode list.

When you double-click a definition row, `ClientDataMatrix` opens a modal explorer that shows:

- the selected field path
- the current raw value and decoded meaning
- the current confidence level: `Confirmed`, `Inferred`, `Unknown`, or `Londo`
- where the current raw value came from in the extracted client files
- the domain description for that field
- every other known raw value for the same field domain
- the plain-English meaning for each raw value
- provenance for each mapping, including file name and line or byte offset where available

This is intended to make it easier to lift client-facing variable domains into emulator code later without inventing ad hoc meanings.

Requirement links now show up there too. When the tool detects that an extracted BIN field points at a known `RequirementId`, the selected raw value is decoded through the requirement ledger instead of being left as an unexplained integer, so the explorer can show direct usage counts, context tags, and nested requirement chains.

## COM Token Dictionary

The COM token dictionary now does more than list token names.

It records:

- the plain-English meaning of the token
- the ability-local component slot being referenced
- nested COM-token decomposition when one token is rendered using another token's format
- the ability name and source text excerpt that demonstrated the token
- context tags inferred from the client text, such as `Knockback`, `Knockdown`, `Immunity`, `CrowdControl`, `Root`, `Snare`, and related combat-state terms

This makes the token glossary a real evidence ledger instead of a bare list of placeholders.

## Confidence Levels

`ClientDataMatrix` now distinguishes between evidence strengths:

- `Confirmed`: direct extracted-client evidence exists, usually from client strings or explicit extracted rows
- `Inferred`: the tool is aggregating a pattern from extracted client evidence, but the mapping is not directly named in one source row
- `Unknown`: the raw field is present, but the extracted client data does not yet explain what it means
- `Londo`: last-resort information from `WAR-RE-Toolkit` reference material; these rows must be treated cautiously and are intentionally segregated

## Requirement Link Rule

Current requirement linkage is deliberately narrow:

- if an extracted `ExtData[*].Val6` value exactly matches a known row in `abilityrequirementexport.bin`, `ClientDataMatrix` records an inferred requirement reference
- linked requirement rows are then expanded recursively if those requirement rows themselves point at more `RequirementId` values through the same field
- this is currently documented as `Inferred`, not `Confirmed`, because the client files expose the referential pattern but do not explicitly name the field as a requirement pointer

## Component Schema Overrides

The last-resort override ledger lives at:

- `ClientDataMatrix\Configuration\component-schema-overrides.tsv`

This file is optional and starts empty. It should only be used when extracted-client evidence is absent. If you add a row sourced from toolkit SQL, decompile, or packet material, mark it with `Confidence=Londo`.

## Ability chains: behaviour that lives in another ability

**Read this before concluding an ability "does nothing".** An ability is frequently not where its
behaviour is. A component can apply, grant or act on ANOTHER ability, and that ability has
components of its own, so reading a single ability's row shows only the first link.

Worked example. "Order Controlled Warlock Engineer" (24857) has four components and no visible
effect. Its chain is one line long and explains the whole thing:

```
Depth 1  27950  Play-As-Monster Master Client Controller
         from 24857 via component 26660, APPLY_ABILITY, Value[0]
```

The behaviour is in 27950. "Detonate" (24828) goes two deep: it acts on "Sabotage" (24826), which
in turn applies "Bbbbrrrrrttt!" (24827).

`doctor ability <id>` now resolves this automatically and prints an **Ability Chain** table,
breadth-first to depth 4. A revisited ability is listed once and not expanded again, so a cycle
terminates. The graph export gains the same links, so `<id>.edges.csv` is walkable rather than
stopping at the subject.

### Which values are followed, and why not more

A component value is only an integer, and the numeric ranges overlap badly -- operation 51's
`Value[0]` looks exactly like an ability id and provably is not. Following an arbitrary value would
manufacture links that do not exist, so only slots whose meaning is established are traversed:

| Operation | Slot | Basis |
|:---|:---|:---|
| 23 APPLY_ABILITY | `Value[0]` | Applies the ability named there |
| 28 GRANTED_ABILITY | `Value[0]` | Grants the ability named there |
| 36 SERVER_COMMAND, **command 304 only** | `Value[1]` | Command 304 names the ability whose persistent state it acts on |

A reference is also dropped unless the target resolves to a real ability, so a numeric coincidence
cannot invent a link. Operations not in that table have their values left unread.

Extending it is the natural way to grow this: add a row to `AbilityReferenceSlots` in
`Services/AbilityChainCatalog.cs` **only** once an operation's slot is established, and record the
evidence in the `Basis` string, which is printed in the report so a reader can judge it.

## CLI Usage

The CLI commands still work, although this is now a Windows GUI executable and console capture can be less predictable from automation:

```powershell
.\bin\Debug\ClientDataMatrix.exe doctor ability 1
```

```powershell
.\bin\Debug\ClientDataMatrix.exe clean
```

```powershell
.\bin\Debug\ClientDataMatrix.exe export graph ability 1
```

```powershell
.\bin\Debug\ClientDataMatrix.exe report conflicts
```

```powershell
.\bin\Debug\ClientDataMatrix.exe report coverage
```

```powershell
.\bin\Debug\ClientDataMatrix.exe report requirements
```

```powershell
.\bin\Debug\ClientDataMatrix.exe report tokens
```

```powershell
.\bin\Debug\ClientDataMatrix.exe report operations
```

```powershell
.\bin\Debug\ClientDataMatrix.exe report remaining
```

```powershell
.\bin\Debug\ClientDataMatrix.exe report remaining next
```

```powershell
.\bin\Debug\ClientDataMatrix.exe report remaining next --area Operations --priority Critical --search APPLY_ABILITY --top 20
```

```powershell
.\bin\Debug\ClientDataMatrix.exe report remaining packets
```

```powershell
.\bin\Debug\ClientDataMatrix.exe report remaining literals
```

The control-literal crosswalk now carries recovered interpretations for stable families when discovery is strong enough, so entries like `445`, `708`, `1014`, `1016`, `1119`, and `1120` are surfaced with semantic notes instead of only raw clustering.

If you want PowerShell to wait reliably for a CLI run and keep the printed status lines, use `Start-Process -Wait`:

```powershell
Start-Process -FilePath (Resolve-Path '.\bin\Debug\ClientDataMatrix.exe') -ArgumentList 'report','operations','--root','C:\Users\Admin\Downloads\myps','--output','docs\data-matrix' -Wait -NoNewWindow
```

Optional overrides:

- `--root <path>` to point at a different extracted client tree
- `--output <path>` to change the generated-doc output root
- `--area <area>` to filter remaining-work focus output to one area key such as `Coverage`, `Conflicts`, `Operations`, `Requirements`, `Tokens`, or `Domains`
- `--priority <bucket>` to keep only `Critical`, `High`, `Medium`, or `Low` backlog buckets and above
- `--search <text>` to filter remaining-work focus output by title, subject, evidence, action, path, or example ability id
- `--top <count>` to cap remaining-work focus output; use `0` for every match

## Output Files

Generated ability reports are written under:

- `docs\data-matrix\ability\`

Generated conflict ledgers are written under:

- `docs\data-matrix\conflicts\`

Generated coverage ledgers are written under:

- `docs\data-matrix\coverage\`

Generated remaining-work overviews are written under:

- `docs\data-matrix\overview\`

Generated token dictionaries are written under:

- `docs\data-matrix\reference\`

Generated requirement ledgers are also written under:

- `docs\data-matrix\reference\`

Generated operation schema ledgers are also written under:

- `docs\data-matrix\reference\`

Primary files:

- `<abilityId>.md`
- `<abilityId>.json`
- `<abilityId>.dot`
- `<abilityId>.nodes.csv`
- `<abilityId>.edges.csv`
- `<abilityId>.claims.csv`
- `client-conflicts.md`
- `client-conflicts.json`
- `client-conflicts.claims.csv`
- `ability-coverage.md`
- `ability-coverage.json`
- `ability-coverage.csv`
- `remaining-work.md`
- `remaining-work.json`
- `remaining-work-next.md`
- `remaining-work-next.json`
- `remaining-work-next.csv`
- `remaining-work-operation-fields.md`
- `remaining-work-operation-fields.json`
- `remaining-work-operation-fields.csv`
- `remaining-work-operation-fields.values.csv`
- `remaining-work-operation-fields.abilities.csv`
- `remaining-work-control-literals.md`
- `remaining-work-control-literals.json`
- `remaining-work-control-literals.csv`
- `remaining-work-control-literals.sources.csv`
- `remaining-work.areas.csv`
- `remaining-work.items.csv`
- `requirement-ledger.md`
- `requirement-ledger.json`
- `requirement-ledger.csv`
- `requirement-ledger.fields.csv`
- `requirement-ledger.references.csv`
- `requirement-ledger.rows.csv`
- `com-token-dictionary.md`
- `com-token-dictionary.json`
- `com-token-dictionary.csv`
- `component-operation-schemas.md`
- `component-operation-schemas.json`
- `component-operation-schemas.csv`
- `component-operation-schemas.fields.csv`
- `component-operation-schemas.abilities.csv`

## Reading the whole client: `report sources`

```powershell
.\bin\Release\ClientDataMatrix.exe report sources --root C:\Users\Admin\Downloads\myps --output docs\data-matrix
```

Everything else in this tool is about abilities and reads eight files. This one walks the extracted
client and reads **all 8,206 data files** in it -- every .csv, .xml, .txt, .lua, .ini and .dat under
the extraction root, 5.4 million data rows, in about 13 seconds. It writes two documents to
`docs/data-matrix/client-sources/`.

Discovery is a recursive sweep, not a list of directories. The first version named three and reached
701 files: it missed every string table below the top level of `data/strings` (4,343 of them), and
the whole of `interface/`, which is where the client's own Lua states what it expects the server to
send -- the contested-instance lobby's handler signature and its sixty-second timeout were read out
of exactly that.

**`client-data-inventory.md`** lists every file with its format, row and column counts, column
names, how many header and comment rows were skipped, and -- the useful part -- whether its first
column is a unique integer, meaning it can be joined against at all. 4,582 of the 8,206 can.

**`client-data-links.md`** lists every column whose values resolve into another file's key, with a
sample of the target's names for those values.

### Why the link report is a shortlist and not an answer

Numeric link detection does not survive contact with this data unaided. Searching every keyed table
against every other produced **280,761** candidates, all numerically valid: most of these files
number their rows from 1 with no gaps, so any column inside such a range resolves into it completely
whether or not it means to, and a per-zone texture list keyed 1..40 absorbs any small column in the
game.

So link searching is scoped to `data/gamedata` and `data/strings/english` -- the tables other files
actually reference -- with row-id columns refused as sources and an evidence floor of 50 distinct
values. That gives 1,154: a searchable shortlist, not a set of conclusions. Every file is still
inventoried.

Two things were tried for ranking and neither works: rate, because dense targets always score 100%,
and a specificity score weighting rate against the target's density, because that just promotes
whichever sparse table happens to absorb small numbers. **No arithmetic on these numbers separates a
real reference from a coincidence**, so the report says so and orders by weight of evidence instead.

What settles a link is the sample column. Nothing about 88,676 item rows resolving into `objects.csv`
proves `ModelId` means art; what proves it is that 8334 is named `tk_soultalisman_intelligence` and
the item carrying it grants Intelligence. Read the names.

The standing warning is in the report itself: `data/gamedata/abilities.csv` agrees with the client's
real ability ids on 13 of 3,115 while looking entirely plausible by these measures, and joining on it
is what filled `mythic_src_abilities` with another ability's names and effect ids.

### Notes on the readers

- CSVs carry one or two header rows — one in `itemdata.csv`, two in `abilities.csv` and
  `objects.csv` where the first groups columns and the second names them. The header is however many
  leading rows do not start with an integer, and the last of them is kept as the column names.
- Rows beginning with `;` are authoring comments and appear **throughout** the data, not just at the
  top. They are counted and dropped. Treating them as data is how `abilities.csv` ids drift.
- String tables are UTF-16 and keep their caret suffixes (`^n`, `^m`, `^f`) verbatim. Those are
  grammatical gender markers; stripping them once cost 5,210 rows of the world database.
- Some XML is not well-formed and the client reads it anyway. `maps/zone006/mappoints.xml` has a
  landmark called "Pick & Goggles" with a bare ampersand; bare ampersands are escaped and the parse
  retried. Six files are malformed past that -- unescaped `<` inside attributes, `=` inside element
  names, curly quotes around values -- and `keybindings.xml` and `command.xml` are among them, so
  refusing them would mean not reporting on the client's own key and command definitions. Those fall
  back to counting element names textually and are marked **degraded read** in the inventory.
- A `.txt` is usually a keyed string table but not always; the loader sniffs the shape and records
  the file as plain text rather than reporting a table with no rows.
- Lua and .ini are kept as lines. They are not tables and nothing can be joined against them, but
  they are the client stating its expectations, so they belong in the inventory.
- The sweep tolerates a live extraction: directories that vanish or lock mid-walk are skipped rather
  than aborting, and a report taken while `warmyptool` is still running is a snapshot of that moment.

### How the animation data is actually keyed

Worth recording, because it was got wrong once. Animations are **not** referenced by name, and the
lookup is not expensive.

- `anim_db.csv` holds 41,009 animations, each with an integer id and a name (`Root_L90`,
  `Or_Un_Cor_Ready-lo`).
- `anim_core.csv` is a matrix: one row per race or skeleton, one column per animation slot, and
  every value is an **anim_db integer id**. Orc's `Cor_Ready-lo` slot is 603, which is
  `Or_Un_Cor_Ready-lo`.
- `anim_statedef.csv` stores each state's motions as repeating **(CSV, Anim ID, State Phase)**
  triples. The `CSV` cell is a short tag — `core`, `Un`, `St` — and `anim_list.csv` maps those tags
  to file names (`Un` → `anim_grip_unarmed.csv`).

So a reference is a **qualified integer**: "id 150 in the core table". The only strings are about
thirty table selectors, resolved once through `anim_list.csv` into whichever file to look in — not a
string compare per lookup. It is a compact and cheap scheme, not a costly one.

That shape is also why link detection struggles here: `anim_statedef.Anim ID` does not point at one
table, it points at whichever of thirty tables the neighbouring `CSV` cell names. A column whose
target varies row by row cannot score against any single table, so **qualified references are
invisible to this analysis** and need to be read deliberately.

`anim_core` is the opposite case and shows the tool working: its `Cor_Ready-lo` column resolves 160
of 160 into `anim_db`, and the samples settle it instantly — `603 = Or_Un_Cor_Ready-lo`,
`1003 = Go_Un_Cor_Ready-lo`, animation names ending in the column's own name.

### The markdown is capped; the CSV is not

`client-data-links.md` shows the first 400 candidates. That cap was hiding true links — `anim_core`'s
columns sat below it because ordering by column size favours large noisy columns, so the readable
report showed `itemdata.icon` resolving 94.7% into `anim_db` (nonsense; icons are not animations)
while the genuine animation references fell off the bottom.

`client-data-links.csv` carries every candidate with the same columns, uncapped. Filter it by the
file you care about rather than scrolling the markdown:

```powershell
Import-Csv docs\data-matrix\client-sources\client-data-links.csv |
  Where-Object { $_.ToTable -like '*anim_db*' -and [double]$_.ResolveRate -eq 1 }
```

### Reader correctness

Four bugs were found by auditing the reader against the data rather than by testing the tool, and
all four were silent — the report looked fine while being wrong.

**Encoding.** The reader passed UTF-8 as the fallback encoding, so any file without a byte order
mark that is not valid UTF-8 was decoded as UTF-8 and every accented character became U+FFFD.
**86 files** are in that state, including nine in `data/gamedata` — `abilities.csv`, `anim_db.csv`,
`effects.csv` and `pregame_chars.xml` among them. Ability 1152 is `Raven’s Bite` with U+2019, and it
was being read as `Raven?s Bite`. A name that silently stops matching is exactly the failure this
tool exists to catch. It now checks for a byte order mark, then tries **strict** UTF-8 — which throws
rather than substituting — and falls back to Windows-1252, which is what a 2008 toolchain produced.

**Bare-id string table entries.** An entry whose text is empty may omit the tab and be nothing but
its id. `scenarionames.txt` is 2,208 entries of which **2,151** are written that way; requiring a tab
discarded all of them and left the file reading as 57 rows. Recovering them added 92,882 rows across
the extraction.

**String-table classification.** The test was "two or more rows parsed", which called an 8,054-line
file a string table on the strength of two lines that happened to start with a number. 71 files were
misclassified. It is now the share of non-empty lines that parse, at 80%.

**Blank rows and impure keys**, described above: spacer rows counted as data, and all-or-nothing key
detection discarding a 41,000-row table over 375 imperfect rows out of 41,384.

Two independent checks now agree end to end: row counts match an independent count using the same
rules across all 103 gamedata CSVs, and the inventory's per-family tables list exactly the 8,206
files the header claims.
