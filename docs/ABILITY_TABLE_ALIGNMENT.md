# Ability table alignment

How the server's ability data lines up with the client's, what was wrong with it, and what is still
missing. Measured 2026-09-07 against the Release database.

The premise, from `CLAUDE.md` hard rule 3: **the client is the record of what the real 1.4.8 server
did.** The emulator computes; the client executes. If the server names, numbers or describes an
ability differently from the client, the server is wrong by definition — the client cannot be
argued with, and anything it does not recognise simply does not happen.

## Which table the server actually reads

`World.xml` ships `UseMythicActionCoverageTables = true`. With that set,
`AbilityMgr.LoadNewAbilityInfo` loads `mythic_src_abilities`, `mythic_src_ability_commands`,
`mythic_src_buff_infos`, `mythic_src_buff_commands`, `mythic_src_ability_damage_heals`,
`mythic_src_ability_modifiers` and `mythic_src_ability_modifier_checks` instead of their unprefixed
counterparts (`WorldServer/World/Abilities/AbilityMgr.cs:112`). Everything the server knows about an
ability at runtime comes from those rows.

This is the same trap as the item tables in hard rule 1: **a migration that writes only
`abilities` is invisible to the running server, with no error anywhere.**

## The three tables

| Table | Rows | What it is |
| --- | --- | --- |
| `mythic_bin_ability` | 29,006 | The client's own ability records, extracted from the 1.4.8 install. Ground truth. |
| `abilities` | 4,221 | The emulator's hand-maintained ability table. Not loaded by default. |
| `mythic_src_abilities` | 8,416 | The coverage-extended table the server loads. Superset of `abilities`. |

Every `abilities` entry also exists in `mythic_src_abilities`; none exists only in `abilities`.

## What was wrong (fixed by migration 76)

Comparing the 4,221 entries the two server tables share, column by column:

- 3,214 rows were byte-identical.
- 1,007 rows differed in **exactly three columns and no others**: `Name` (1,007), `EffectID`
  (1,000), `IconId` (984).
- Every mechanical column agreed on all 4,221 rows — `CareerLine`, `MinRange`, `Range`, `CastTime`,
  `Cooldown`, `ApCost`, `AbilityType`, `MasteryTree`, `Specline`, `MinimumRank`, the cast flags.
  (`MinimumRenown` differed on 4 rows and is not part of the identity block.)

So `mythic_src_abilities` had correct mechanics under a **misaligned identity block**. The client
settled which side was right, on two independent columns across those 1,007 rows:

| | matches the client | |
| --- | --- | --- |
| `abilities.Name` | 918 | (46 rows have no client name to compare) |
| `mythic_src_abilities.Name` | **0** | |
| `abilities.EffectID` | 435 | |
| `mythic_src_abilities.EffectID` | **2** | |

Concretely, for a Disciple of Khaine's real action bar read off the wire (`we.txt`):

| Id | Client | `abilities` | `mythic_src_abilities` (before) |
| --- | --- | --- | --- |
| 245 | Flee | Flee | Word of Command |
| 585 | Divine Fury | Divine Fury | Avalanche |
| 692 | Rampaging Siphon | Rampaging Siphon | Hip Shot |
| 695 | Focused Mind | Focused Mind | Firebomb |
| 841 | Dark Blessings | Dark Blessings | Da Greenest |

**This was not cosmetic.** `EffectID` is written straight into the cast packets
(`AbilityProcessor.cs:432`, `:945`, `:1074`, `:1094`; `AbilityInterface.cs:696`), so 1,000 abilities
were telling the client to play another ability's visual. `Name` is what every log line, GM command
and future investigation reads, so the other 1,007 quietly misled anyone who looked.

The error looks like a shifted name column — src entry 7 carried "Death From Above", the client's
name for 6; src 8 carried "Spine Fling", the client's 7 — but it is not one global offset. Testing
`client ID = Entry ± 1` and `± 2` across the whole table gains nothing over `Entry` itself (2,954 at
offset 0 versus 273/262 either side, which is just duplicate-name noise). It could only be repaired
per row.

`Database/76_realign_mythic_src_ability_identity.sql` does that: it takes `Name`, `EffectID` and
`IconId` from `abilities` on the 1,007 shared rows, and `Name` from the client on the 2,140
src-only rows that disagreed with a non-empty client name. Client-name agreement went from **2,954
to 6,012**. Ability data is cached at boot, so a restart is required.

### What was deliberately not changed

- **`EffectID` on the src-only rows.** `abilities` and the client agree on `EffectID` for only about
  93% of the rows they otherwise agree on completely, so some server values are deliberate. There is
  no second column to corroborate a rewrite against here, and guessing is what put the table in this
  state. Open.
- **The 307 rows where `abilities` itself differs from the client.** These are trailing whitespace
  (`"Gut Ripper "`) and deliberate emulator disambiguation (`"Vehement Blades Self AP"`,
  `"Gift of Brutality Proc"`, `"Kiss of Agony Buff"`) on rows that are otherwise the right ability.
  Annotation, not misalignment.
- **2,051 src-only rows with no client name at all.** Nothing to align them to.

### The buff tables are fine

`buff_infos` (2,279) and `mythic_src_buff_infos` (2,280) agree with each other on **every** shared
name, and 1,857 match the client. The 378 that differ are the same annotation pattern plus mounts,
where the client uses one generic name ("Summon Mount") for rows the server names individually
("Blue Roan Elven Mare"). The server name is more specific, not wrong. No repair needed.

## What is still missing

The alignment problem is closed. The **coverage** problem is not.

- **20,590 client abilities have no server row at all**, and every one of them carries component
  data. 6,636 of those are named. This is the bulk of the client's ability corpus, including the
  Skaven monster forms, and is the milestone worth working towards: restoring those rows plus their
  component chains from `mythic_bin_ability`.
- No loaded ability is an id the client does not know — checked, zero. The server mislabels real
  abilities; it does not invent ids. All 4,195 src-only rows carry `CareerLine 0`, so none is
  granted to a player career; they are creature and world abilities, and each has a real client row.

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
and that client-name agreement has not fallen below the 6,012 migration 76 left it at. It does not
verify that a cast plays the right visual in the client — that needs an in-client test.
