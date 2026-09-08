# Ability table alignment

How the server's ability data lines up with the client's, what was wrong with it, where the wrong
data came from, and what is still missing. Measured 2026-09-07 against the Release database.

The premise, from `CLAUDE.md` hard rule 3: **the client is the record of what the real 1.4.8 server
did.** The emulator computes; the client executes. If the server names, numbers or describes an
ability differently from the client, the server is wrong by definition.

## Which file in the client is authoritative

Not every client file that lists abilities uses the same id space, and getting this wrong is what
caused the damage below. Four sources, checked against each other:

| Source | Rows | Agrees with `abilitynames.txt` |
| --- | --- | --- |
| `data/strings/english/abilitynames.txt` | 29,001 (12,934 named) | — it *is* the reference |
| `mythic_bin_ability` (toolkit import of the client's ability records) | 29,006 | **12,865 match, 69 differ** |
| `abilities` (emulator) | 4,221 | 3,878 match, 297 differ |
| `mythic_src_abilities` (emulator, the one the server loads) | 8,416 | 5,995 match, 324 differ |
| `mythic_csv_abilities` (import of `data/gamedata/abilities.csv`) | 5,209 | **13 match, 2,430 differ** |

`abilitynames.txt` is the localized string table the client's UI actually renders from, keyed by the
runtime ability id, so it settles the id space. `mythic_bin_ability` reproduces it at 99.5% — the 69
exceptions are encoding artifacts and trailing whitespace — and is therefore usable as client truth
for the columns the string table does not carry.

**`data/gamedata/abilities.csv` is not in that id space.** It is an art and animation authoring
sheet — its columns are Icon, Animation Build Up, Animation Action, Effect Special, Animation Play,
Mount Build, ActivateAgro — and its ID column is a row key of its own, drifted by the `;` comment
rows scattered through it. It agrees with the client's real ability ids on **13 of 3,115**. The
worked example:

| id | `abilitynames.txt` (client UI) | `data/gamedata/abilities.csv` |
| --- | --- | --- |
| 245 | Flee | Word of Command |
| 585 | Divine Fury | Avalanche |
| 692 | Rampaging Siphon | Hip Shot |
| 695 | Focused Mind | Firebomb |
| 841 | Dark Blessings | Da Greenest |

The left column is one career's action bar — a Disciple of Khaine's, matching a real bar read off
the wire. The right column is an Engineer/Black Orc/Warrior Priest mix, which no character can have.
"Hip Shot" is genuinely ability **1520** to the client; row 692 of that CSV is a different thing
entirely.

## Which table the server reads

`World.xml` ships `UseMythicActionCoverageTables = true`. With that set,
`AbilityMgr.LoadNewAbilityInfo` loads `mythic_src_abilities`, `mythic_src_ability_commands`,
`mythic_src_buff_infos`, `mythic_src_buff_commands`, `mythic_src_ability_damage_heals`,
`mythic_src_ability_modifiers` and `mythic_src_ability_modifier_checks` instead of their unprefixed
counterparts (`WorldServer/World/Abilities/AbilityMgr.cs:112`). Everything the server knows about an
ability at runtime comes from those rows. This is the same trap as the item tables in hard rule 1:
**a migration that writes only `abilities` is invisible to the running server, with no error.**

## What was wrong

`mythic_src_abilities` was bulk-populated from `mythic_csv_abilities` — that is, from
`data/gamedata/abilities.csv` — joined on the CSV's ID column. The signature is unmistakable:

- 3,601 of its `IconId` values equal `mythic_csv_abilities.IconId` (in `abilities`: 20)
- 4,193 of its `EffectID` values equal `mythic_csv_abilities.EffectAbilityId`
- 4,119 of its `EffectID` values were simply the row's own `Entry`, because that CSV's
  "Effect (Special)" column is its own row id on 5,086 of 5,184 rows

Every column copied across that join landed on the wrong ability.

The mechanics were untouched by it. Comparing the 4,221 entries the two server tables share, 3,214
were byte-identical and the other 1,007 differed in **exactly three columns**: `Name` (1,007),
`EffectID` (1,000), `IconId` (984). `CareerLine`, `MinRange`, `Range`, `CastTime`, `Cooldown`,
`ApCost`, `AbilityType`, `MasteryTree`, `Specline`, `MinimumRank` and the cast flags agreed on all
4,221 rows. (`MinimumRenown` differed on 4 and is not part of the identity block.)

**`EffectID` is not bookkeeping.** It is written straight into the cast packets
(`AbilityProcessor.cs:432`, `:945`, `:1074`, `:1094`; `AbilityInterface.cs:696`), so it is the visual
the client plays. Fabricated values there mean abilities playing another ability's effect, or none.
`IconId`, by contrast, is stored and read by no server code at all.

## The repair

**`76_realign_mythic_src_ability_identity.sql`** — took `Name`, `EffectID` and `IconId` from
`abilities` on the 1,007 divergent shared rows, and `Name` from the client on the 2,140 src-only
rows that disagreed with a non-empty client name. Client-name agreement: **2,954 → 5,995**.

**`77_restore_ability_effect_ids_from_client.sql`** — took `EffectID` from `mythic_bin_ability`
wherever the client has one (2,878 rows in `mythic_src_abilities`, 1,176 in `abilities`), cleared
the 2,441 provably CSV-derived values the client says should be none, and cleared 2,955 CSV-derived
`IconId` values on rows migration 76 could not reach. EffectID agreement with the client:

| | before | after |
| --- | --- | --- |
| `mythic_src_abilities` | 3,030 of 8,416 | **8,349** |
| `abilities` | 2,988 of 4,221 | **4,164** |

Ability data is cached at boot, so a restart is required.

### What was deliberately not changed

- **67 rows (`mythic_src_abilities`) / 57 (`abilities`) where the server carries an `EffectID` and
  the client record has none**, without the CSV signature. Most are unnamed emulator-authored rows —
  2701 through 2709 all share EffectID 2751. A zero in the client record is equally consistent with
  the import not having captured one, so there is no evidence to act on. Taking them out would be
  the same guessing that caused the original problem.
- **The 324 remaining name differences.** Mounts, where the client uses one generic "Summon Mount"
  for rows the server names individually ("Blue Roan Elven Mare", "Black Timber Wolf"); emulator
  disambiguation ("Enfeebling Strike Self AP", "Obsessive Focus Debuff", "Burn Away Lies 2");
  trailing whitespace; and one `?` encoding artifact ("Raven?s Bite"). The server name is more
  specific than the client's, not wrong.
- **2,051 src-only rows with no client name at all.** Nothing to align them to.

### The buff tables are fine

`buff_infos` (2,279) and `mythic_src_buff_infos` (2,280) agree with each other on **every** shared
name, and 1,857 match the client. The 378 that differ are the same annotation-and-mounts pattern.
Neither was populated from the CSV. No repair needed.

## What is still missing

The alignment problem is closed. The **coverage** problem is not.

- **20,590 client abilities have no server row at all**, and every one of them carries component
  data. 6,636 are named. This is the bulk of the client's ability corpus, including the Skaven
  monster forms, and is the milestone worth working towards: restoring those rows plus their
  component chains from `mythic_bin_ability`.
- No loaded ability is an id the client does not know — checked, zero. All 4,195 src-only rows carry
  `CareerLine 0`, so none is granted to a player career; they are creature and world abilities, and
  each has a real client row. The server mislabelled real abilities; it did not invent ids.

### Why component chains are the hard part

An ability is rarely self-contained. A component can apply, grant or act on **another** ability,
which has components of its own, so reading one row shows only the first link. The Skaven controls
are the plain case: "Order Controlled Warlock Engineer" (24857) does nothing by itself — its
components apply ability 27950 and issue a server command naming it, and the behaviour lives there.

`ClientDataMatrix.Services.AbilityChainCatalog` resolves those chains from the client data. It only
follows value slots whose meaning is established (`APPLY_ABILITY` 23 `Value[0]`, `GRANTED_ABILITY`
28 `Value[0]`, `SERVER_COMMAND` 36 `Value[1]` under command 304 alone), because component values are
bare integers and treating an arbitrary one as an ability id manufactures links that do not exist.

On the server side, `mythic_src_buff_commands` **is** the component chain: `CommandName` dispatches
to one of 96 handlers in `BuffEffectInvoker`. A restored ability needs its row *and* its chain; the
row alone gives the client an entry with no behaviour behind it.

## Checking it

```powershell
.\tools\validation\Test-AbilityAlignment.ps1
```

SELECT-only. Asserts the two server tables agree on identity and mechanics for all shared entries,
that no src-only row is mislabelled against the client, that no loaded ability lacks a client row,
that name and `EffectID` agreement have not fallen below what migrations 76 and 77 left, and that
the count of CSV-derived `EffectID` values has not climbed — the last being the tripwire for
something joining on `mythic_csv_abilities.AbilityId` again. It does not verify that a cast plays
the right visual in the client; that needs an in-client test.
