# Core Identity Domains

Generated UTC: `2026-09-14T19:41:59.5333156Z`

Extracted root: `C:\Users\Admin\Downloads\myps`

## Purpose

- This ledger separates extracted-client identity domains so names like `CareerLine`, `CareerName`, and future `CareerId` work do not collapse into one ambiguous label.
- It does not invent a canonical `CareerId` domain when the extracted files do not prove one.

## Source Status

| Source | File | Loaded | Rows | Path | Error |
| --- | --- | --- | ---: | --- | --- |
| client_bin | abilitycomponentexport.bin | True | 18526 | C:\Users\Admin\Downloads\myps\data\bin\abilitycomponentexport.bin |  |
| client_bin | abilityexport.bin | True | 11736 | C:\Users\Admin\Downloads\myps\data\bin\abilityexport.bin |  |
| client_bin | abilityrequirementexport.bin | True | 655 | C:\Users\Admin\Downloads\myps\data\bin\abilityrequirementexport.bin |  |
| client_bin | upgradetableexport.bin | True | 138 | C:\Users\Admin\Downloads\myps\data\bin\upgradetableexport.bin |  |
| client_csv | abilities.csv | True | 5210 | C:\Users\Admin\Downloads\myps\data\gamedata\abilities.csv |  |
| client_csv | effects.csv | True | 4445 | C:\Users\Admin\Downloads\myps\data\gamedata\effects.csv |  |
| client_strings | abilitydesc.txt | True | 29001 | C:\Users\Admin\Downloads\myps\data\strings\english\abilitydesc.txt |  |
| client_strings | abilityeffect.txt | True | 73 | C:\Users\Admin\Downloads\myps\data\strings\english\abilityeffect.txt |  |
| client_strings | abilitynames.txt | True | 29001 | C:\Users\Admin\Downloads\myps\data\strings\english\abilitynames.txt |  |
| client_strings | careerlines_m.txt | True | 25 | C:\Users\Admin\Downloads\myps\data\strings\english\careerlines_m.txt |  |
| client_strings | careernames_m.txt | True | 132 | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt |  |
| client_strings | componenteffects.txt | True | 29001 | C:\Users\Admin\Downloads\myps\data\strings\english\componenteffects.txt |  |
| client_strings | racenames_m.txt | True | 8 | C:\Users\Admin\Downloads\myps\data\strings\english\racenames_m.txt |  |
| client_xml | pregame_chars.xml | True | 126 | C:\Users\Admin\Downloads\myps\data\gamedata\pregame_chars.xml |  |

## Domain Summary

| DomainKey | DisplayName | Confidence | Canonicality | Values | DistinctMeanings | DuplicateMeaningGroups | Sources | RecommendedUsage |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| CareerLine.EntryId | Career Line Entry IDs | Confirmed | Canonical for the extracted career-line domain | 25 | 25 | 0 | careerlines_m.txt | Use for fields explicitly named CareerLine, including abilityexport.bin CareerLine. |
| CareerName.EntryId | Career Name Entry IDs | Confirmed | Canonical as a multi-context string-entry table (5 display groups × 24 careers); NOT safe for use as CareerId | 132 | 25 | 24 | careernames_m.txt | Do NOT use as CareerId. Canonical career identity is CareerLine.EntryId (careerlines_m.txt, IDs 0-24). Use CareerName.EntryId only for resolving the specific UI display-context slot (group index = (EntryId - 12) / 24). |
| Race.EntryId | Race Entry IDs | Confirmed | Canonical for the extracted race display-name domain | 8 | 8 | 0 | racenames_m.txt | Use for extracted-client race display names only. |

## Career Line Entry IDs

- Domain key: `CareerLine.EntryId`
- Confidence: `Confirmed`
- Canonicality: `Canonical for the extracted career-line domain`
- Source files: `careerlines_m.txt`
- Recommended usage: Use for fields explicitly named CareerLine, including abilityexport.bin CareerLine.
- Notes: This is the numeric domain currently referenced by abilityexport.bin CareerLine. It should stay named CareerLine unless another extracted source proves a different identity domain.

## Values

| RawValue | Meaning | Confidence | Source | Path | Location | Notes |
| --- | --- | --- | --- | --- | --- | --- |
| 0 | None | Confirmed | careerlines_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careerlines_m.txt | line 1 | Referenced by 10512 ability BIN row(s). |
| 1 | Ironbreaker | Confirmed | careerlines_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careerlines_m.txt | line 2 | Referenced by 62 ability BIN row(s). |
| 2 | Slayer | Confirmed | careerlines_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careerlines_m.txt | line 3 | Referenced by 59 ability BIN row(s). |
| 3 | Runepriest | Confirmed | careerlines_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careerlines_m.txt | line 4 | Referenced by 59 ability BIN row(s). |
| 4 | Engineer | Confirmed | careerlines_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careerlines_m.txt | line 5 | Referenced by 53 ability BIN row(s). |
| 5 | Black Orc | Confirmed | careerlines_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careerlines_m.txt | line 6 | Referenced by 53 ability BIN row(s). |
| 6 | Choppa | Confirmed | careerlines_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careerlines_m.txt | line 7 | Referenced by 61 ability BIN row(s). |
| 7 | Shaman | Confirmed | careerlines_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careerlines_m.txt | line 8 | Referenced by 57 ability BIN row(s). |
| 8 | Squig Herder | Confirmed | careerlines_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careerlines_m.txt | line 9 | Referenced by 27 ability BIN row(s). |
| 9 | Witch Hunter | Confirmed | careerlines_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careerlines_m.txt | line 10 | Referenced by 54 ability BIN row(s). |
| 10 | Knight of the Blazing Sun | Confirmed | careerlines_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careerlines_m.txt | line 11 |  |
| 11 | Bright Wizard | Confirmed | careerlines_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careerlines_m.txt | line 12 | Referenced by 58 ability BIN row(s). |
| 12 | Warrior Priest | Confirmed | careerlines_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careerlines_m.txt | line 13 | Referenced by 77 ability BIN row(s). |
| 13 | Chosen | Confirmed | careerlines_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careerlines_m.txt | line 14 | Referenced by 58 ability BIN row(s). |
| 14 | Marauder | Confirmed | careerlines_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careerlines_m.txt | line 15 | Referenced by 58 ability BIN row(s). |
| 15 | Zealot | Confirmed | careerlines_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careerlines_m.txt | line 16 | Referenced by 54 ability BIN row(s). |
| 16 | Magus | Confirmed | careerlines_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careerlines_m.txt | line 17 | Referenced by 17 ability BIN row(s). |
| 17 | Swordmaster | Confirmed | careerlines_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careerlines_m.txt | line 18 | Referenced by 53 ability BIN row(s). |
| 18 | Shadow Warrior | Confirmed | careerlines_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careerlines_m.txt | line 19 | Referenced by 54 ability BIN row(s). |
| 19 | White Lion | Confirmed | careerlines_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careerlines_m.txt | line 20 | Referenced by 1 ability BIN row(s). |
| 20 | Archmage | Confirmed | careerlines_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careerlines_m.txt | line 21 | Referenced by 59 ability BIN row(s). |
| 21 | Blackguard | Confirmed | careerlines_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careerlines_m.txt | line 22 | Referenced by 64 ability BIN row(s). |
| 22 | Witch Elf | Confirmed | careerlines_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careerlines_m.txt | line 23 | Referenced by 54 ability BIN row(s). |
| 23 | Disciple of Khaine | Confirmed | careerlines_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careerlines_m.txt | line 24 | Referenced by 74 ability BIN row(s). |
| 24 | Sorcerer | Confirmed | careerlines_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careerlines_m.txt | line 25 | Referenced by 58 ability BIN row(s). |

## Career Name Entry IDs

- Domain key: `CareerName.EntryId`
- Confidence: `Confirmed`
- Canonicality: `Canonical as a multi-context string-entry table (5 display groups × 24 careers); NOT safe for use as CareerId`
- Source files: `careernames_m.txt`
- Recommended usage: Do NOT use as CareerId. Canonical career identity is CareerLine.EntryId (careerlines_m.txt, IDs 0-24). Use CareerName.EntryId only for resolving the specific UI display-context slot (group index = (EntryId - 12) / 24).
- Notes: careernames_m.txt has 120 entries (IDs 12-131) = 5 repetition groups × 24 careers. Each career name appears 5 times with 5 different numeric IDs — one per display context. The duplicates are expected and structurally explained. The canonical career identity domain is careerlines_m.txt (IDs 0-24), which maps 1:1 to the abilityexport.bin CareerLine field. Duplicate display-name groups: 24 (all expected — 5 display context repetitions per career).

## Values

| RawValue | Meaning | Confidence | Source | Path | Location | Notes |
| --- | --- | --- | --- | --- | --- | --- |
| 0 | (empty) | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 1 | Expected repetition: this display name appears across 12 entry ids (0, 1, 10, 11, 2, 3, 4, 5, 6, 7, 8, 9) — one per display context group. |
| 1 | (empty) | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 2 | Expected repetition: this display name appears across 12 entry ids (0, 1, 10, 11, 2, 3, 4, 5, 6, 7, 8, 9) — one per display context group. |
| 2 | (empty) | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 3 | Expected repetition: this display name appears across 12 entry ids (0, 1, 10, 11, 2, 3, 4, 5, 6, 7, 8, 9) — one per display context group. |
| 3 | (empty) | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 4 | Expected repetition: this display name appears across 12 entry ids (0, 1, 10, 11, 2, 3, 4, 5, 6, 7, 8, 9) — one per display context group. |
| 4 | (empty) | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 5 | Expected repetition: this display name appears across 12 entry ids (0, 1, 10, 11, 2, 3, 4, 5, 6, 7, 8, 9) — one per display context group. |
| 5 | (empty) | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 6 | Expected repetition: this display name appears across 12 entry ids (0, 1, 10, 11, 2, 3, 4, 5, 6, 7, 8, 9) — one per display context group. |
| 6 | (empty) | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 7 | Expected repetition: this display name appears across 12 entry ids (0, 1, 10, 11, 2, 3, 4, 5, 6, 7, 8, 9) — one per display context group. |
| 7 | (empty) | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 8 | Expected repetition: this display name appears across 12 entry ids (0, 1, 10, 11, 2, 3, 4, 5, 6, 7, 8, 9) — one per display context group. |
| 8 | (empty) | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 9 | Expected repetition: this display name appears across 12 entry ids (0, 1, 10, 11, 2, 3, 4, 5, 6, 7, 8, 9) — one per display context group. |
| 9 | (empty) | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 10 | Expected repetition: this display name appears across 12 entry ids (0, 1, 10, 11, 2, 3, 4, 5, 6, 7, 8, 9) — one per display context group. |
| 10 | (empty) | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 11 | Expected repetition: this display name appears across 12 entry ids (0, 1, 10, 11, 2, 3, 4, 5, 6, 7, 8, 9) — one per display context group. |
| 11 | (empty) | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 12 | Expected repetition: this display name appears across 12 entry ids (0, 1, 10, 11, 2, 3, 4, 5, 6, 7, 8, 9) — one per display context group. |
| 12 | Ironbreaker | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 13 | Expected repetition: this display name appears across 5 entry ids (12, 20, 28, 36, 44) — one per display context group. |
| 13 | Slayer | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 14 | Expected repetition: this display name appears across 5 entry ids (13, 21, 29, 37, 45) — one per display context group. |
| 14 | Runepriest | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 15 | Expected repetition: this display name appears across 5 entry ids (14, 22, 30, 38, 46) — one per display context group. |
| 15 | Engineer | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 16 | Expected repetition: this display name appears across 5 entry ids (15, 23, 31, 39, 47) — one per display context group. |
| 16 | Black Orc | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 17 | Expected repetition: this display name appears across 5 entry ids (16, 24, 32, 40, 48) — one per display context group. |
| 17 | Choppa | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 18 | Expected repetition: this display name appears across 5 entry ids (17, 25, 33, 41, 49) — one per display context group. |
| 18 | Shaman | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 19 | Expected repetition: this display name appears across 5 entry ids (18, 26, 34, 42, 50) — one per display context group. |
| 19 | Squig Herder | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 20 | Expected repetition: this display name appears across 5 entry ids (19, 27, 35, 43, 51) — one per display context group. |
| 20 | Ironbreaker | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 21 | Expected repetition: this display name appears across 5 entry ids (12, 20, 28, 36, 44) — one per display context group. |
| 21 | Slayer | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 22 | Expected repetition: this display name appears across 5 entry ids (13, 21, 29, 37, 45) — one per display context group. |
| 22 | Runepriest | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 23 | Expected repetition: this display name appears across 5 entry ids (14, 22, 30, 38, 46) — one per display context group. |
| 23 | Engineer | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 24 | Expected repetition: this display name appears across 5 entry ids (15, 23, 31, 39, 47) — one per display context group. |
| 24 | Black Orc | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 25 | Expected repetition: this display name appears across 5 entry ids (16, 24, 32, 40, 48) — one per display context group. |
| 25 | Choppa | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 26 | Expected repetition: this display name appears across 5 entry ids (17, 25, 33, 41, 49) — one per display context group. |
| 26 | Shaman | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 27 | Expected repetition: this display name appears across 5 entry ids (18, 26, 34, 42, 50) — one per display context group. |
| 27 | Squig Herder | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 28 | Expected repetition: this display name appears across 5 entry ids (19, 27, 35, 43, 51) — one per display context group. |
| 28 | Ironbreaker | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 29 | Expected repetition: this display name appears across 5 entry ids (12, 20, 28, 36, 44) — one per display context group. |
| 29 | Slayer | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 30 | Expected repetition: this display name appears across 5 entry ids (13, 21, 29, 37, 45) — one per display context group. |
| 30 | Runepriest | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 31 | Expected repetition: this display name appears across 5 entry ids (14, 22, 30, 38, 46) — one per display context group. |
| 31 | Engineer | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 32 | Expected repetition: this display name appears across 5 entry ids (15, 23, 31, 39, 47) — one per display context group. |
| 32 | Black Orc | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 33 | Expected repetition: this display name appears across 5 entry ids (16, 24, 32, 40, 48) — one per display context group. |
| 33 | Choppa | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 34 | Expected repetition: this display name appears across 5 entry ids (17, 25, 33, 41, 49) — one per display context group. |
| 34 | Shaman | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 35 | Expected repetition: this display name appears across 5 entry ids (18, 26, 34, 42, 50) — one per display context group. |
| 35 | Squig Herder | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 36 | Expected repetition: this display name appears across 5 entry ids (19, 27, 35, 43, 51) — one per display context group. |
| 36 | Ironbreaker | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 37 | Expected repetition: this display name appears across 5 entry ids (12, 20, 28, 36, 44) — one per display context group. |
| 37 | Slayer | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 38 | Expected repetition: this display name appears across 5 entry ids (13, 21, 29, 37, 45) — one per display context group. |
| 38 | Runepriest | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 39 | Expected repetition: this display name appears across 5 entry ids (14, 22, 30, 38, 46) — one per display context group. |
| 39 | Engineer | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 40 | Expected repetition: this display name appears across 5 entry ids (15, 23, 31, 39, 47) — one per display context group. |
| 40 | Black Orc | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 41 | Expected repetition: this display name appears across 5 entry ids (16, 24, 32, 40, 48) — one per display context group. |
| 41 | Choppa | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 42 | Expected repetition: this display name appears across 5 entry ids (17, 25, 33, 41, 49) — one per display context group. |
| 42 | Shaman | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 43 | Expected repetition: this display name appears across 5 entry ids (18, 26, 34, 42, 50) — one per display context group. |
| 43 | Squig Herder | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 44 | Expected repetition: this display name appears across 5 entry ids (19, 27, 35, 43, 51) — one per display context group. |
| 44 | Ironbreaker | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 45 | Expected repetition: this display name appears across 5 entry ids (12, 20, 28, 36, 44) — one per display context group. |
| 45 | Slayer | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 46 | Expected repetition: this display name appears across 5 entry ids (13, 21, 29, 37, 45) — one per display context group. |
| 46 | Runepriest | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 47 | Expected repetition: this display name appears across 5 entry ids (14, 22, 30, 38, 46) — one per display context group. |
| 47 | Engineer | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 48 | Expected repetition: this display name appears across 5 entry ids (15, 23, 31, 39, 47) — one per display context group. |
| 48 | Black Orc | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 49 | Expected repetition: this display name appears across 5 entry ids (16, 24, 32, 40, 48) — one per display context group. |
| 49 | Choppa | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 50 | Expected repetition: this display name appears across 5 entry ids (17, 25, 33, 41, 49) — one per display context group. |
| 50 | Shaman | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 51 | Expected repetition: this display name appears across 5 entry ids (18, 26, 34, 42, 50) — one per display context group. |
| 51 | Squig Herder | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 52 | Expected repetition: this display name appears across 5 entry ids (19, 27, 35, 43, 51) — one per display context group. |
| 52 | Witch Hunter | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 53 | Expected repetition: this display name appears across 5 entry ids (52, 60, 68, 76, 84) — one per display context group. |
| 53 | Knight of the Blazing Sun | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 54 | Expected repetition: this display name appears across 5 entry ids (53, 61, 69, 77, 85) — one per display context group. |
| 54 | Bright Wizard | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 55 | Expected repetition: this display name appears across 5 entry ids (54, 62, 70, 78, 86) — one per display context group. |
| 55 | Warrior Priest | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 56 | Expected repetition: this display name appears across 5 entry ids (55, 63, 71, 79, 87) — one per display context group. |
| 56 | Chosen | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 57 | Expected repetition: this display name appears across 5 entry ids (56, 64, 72, 80, 88) — one per display context group. |
| 57 | Marauder | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 58 | Expected repetition: this display name appears across 5 entry ids (57, 65, 73, 81, 89) — one per display context group. |
| 58 | Zealot | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 59 | Expected repetition: this display name appears across 5 entry ids (58, 66, 74, 82, 90) — one per display context group. |
| 59 | Magus | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 60 | Expected repetition: this display name appears across 5 entry ids (59, 67, 75, 83, 91) — one per display context group. |
| 60 | Witch Hunter | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 61 | Expected repetition: this display name appears across 5 entry ids (52, 60, 68, 76, 84) — one per display context group. |
| 61 | Knight of the Blazing Sun | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 62 | Expected repetition: this display name appears across 5 entry ids (53, 61, 69, 77, 85) — one per display context group. |
| 62 | Bright Wizard | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 63 | Expected repetition: this display name appears across 5 entry ids (54, 62, 70, 78, 86) — one per display context group. |
| 63 | Warrior Priest | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 64 | Expected repetition: this display name appears across 5 entry ids (55, 63, 71, 79, 87) — one per display context group. |
| 64 | Chosen | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 65 | Expected repetition: this display name appears across 5 entry ids (56, 64, 72, 80, 88) — one per display context group. |
| 65 | Marauder | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 66 | Expected repetition: this display name appears across 5 entry ids (57, 65, 73, 81, 89) — one per display context group. |
| 66 | Zealot | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 67 | Expected repetition: this display name appears across 5 entry ids (58, 66, 74, 82, 90) — one per display context group. |
| 67 | Magus | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 68 | Expected repetition: this display name appears across 5 entry ids (59, 67, 75, 83, 91) — one per display context group. |
| 68 | Witch Hunter | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 69 | Expected repetition: this display name appears across 5 entry ids (52, 60, 68, 76, 84) — one per display context group. |
| 69 | Knight of the Blazing Sun | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 70 | Expected repetition: this display name appears across 5 entry ids (53, 61, 69, 77, 85) — one per display context group. |
| 70 | Bright Wizard | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 71 | Expected repetition: this display name appears across 5 entry ids (54, 62, 70, 78, 86) — one per display context group. |
| 71 | Warrior Priest | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 72 | Expected repetition: this display name appears across 5 entry ids (55, 63, 71, 79, 87) — one per display context group. |
| 72 | Chosen | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 73 | Expected repetition: this display name appears across 5 entry ids (56, 64, 72, 80, 88) — one per display context group. |
| 73 | Marauder | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 74 | Expected repetition: this display name appears across 5 entry ids (57, 65, 73, 81, 89) — one per display context group. |
| 74 | Zealot | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 75 | Expected repetition: this display name appears across 5 entry ids (58, 66, 74, 82, 90) — one per display context group. |
| 75 | Magus | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 76 | Expected repetition: this display name appears across 5 entry ids (59, 67, 75, 83, 91) — one per display context group. |
| 76 | Witch Hunter | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 77 | Expected repetition: this display name appears across 5 entry ids (52, 60, 68, 76, 84) — one per display context group. |
| 77 | Knight of the Blazing Sun | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 78 | Expected repetition: this display name appears across 5 entry ids (53, 61, 69, 77, 85) — one per display context group. |
| 78 | Bright Wizard | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 79 | Expected repetition: this display name appears across 5 entry ids (54, 62, 70, 78, 86) — one per display context group. |
| 79 | Warrior Priest | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 80 | Expected repetition: this display name appears across 5 entry ids (55, 63, 71, 79, 87) — one per display context group. |
| 80 | Chosen | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 81 | Expected repetition: this display name appears across 5 entry ids (56, 64, 72, 80, 88) — one per display context group. |
| 81 | Marauder | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 82 | Expected repetition: this display name appears across 5 entry ids (57, 65, 73, 81, 89) — one per display context group. |
| 82 | Zealot | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 83 | Expected repetition: this display name appears across 5 entry ids (58, 66, 74, 82, 90) — one per display context group. |
| 83 | Magus | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 84 | Expected repetition: this display name appears across 5 entry ids (59, 67, 75, 83, 91) — one per display context group. |
| 84 | Witch Hunter | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 85 | Expected repetition: this display name appears across 5 entry ids (52, 60, 68, 76, 84) — one per display context group. |
| 85 | Knight of the Blazing Sun | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 86 | Expected repetition: this display name appears across 5 entry ids (53, 61, 69, 77, 85) — one per display context group. |
| 86 | Bright Wizard | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 87 | Expected repetition: this display name appears across 5 entry ids (54, 62, 70, 78, 86) — one per display context group. |
| 87 | Warrior Priest | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 88 | Expected repetition: this display name appears across 5 entry ids (55, 63, 71, 79, 87) — one per display context group. |
| 88 | Chosen | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 89 | Expected repetition: this display name appears across 5 entry ids (56, 64, 72, 80, 88) — one per display context group. |
| 89 | Marauder | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 90 | Expected repetition: this display name appears across 5 entry ids (57, 65, 73, 81, 89) — one per display context group. |
| 90 | Zealot | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 91 | Expected repetition: this display name appears across 5 entry ids (58, 66, 74, 82, 90) — one per display context group. |
| 91 | Magus | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 92 | Expected repetition: this display name appears across 5 entry ids (59, 67, 75, 83, 91) — one per display context group. |
| 92 | Swordmaster | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 93 | Expected repetition: this display name appears across 5 entry ids (100, 108, 116, 124, 92) — one per display context group. |
| 93 | Shadow Warrior | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 94 | Expected repetition: this display name appears across 5 entry ids (101, 109, 117, 125, 93) — one per display context group. |
| 94 | White Lion | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 95 | Expected repetition: this display name appears across 5 entry ids (102, 110, 118, 126, 94) — one per display context group. |
| 95 | Archmage | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 96 | Expected repetition: this display name appears across 5 entry ids (103, 111, 119, 127, 95) — one per display context group. |
| 96 | Blackguard | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 97 | Expected repetition: this display name appears across 5 entry ids (104, 112, 120, 128, 96) — one per display context group. |
| 97 | Witch Elf | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 98 | Expected repetition: this display name appears across 5 entry ids (105, 113, 121, 129, 97) — one per display context group. |
| 98 | Disciple of Khaine | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 99 | Expected repetition: this display name appears across 5 entry ids (106, 114, 122, 130, 98) — one per display context group. |
| 99 | Sorcerer | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 100 | Expected repetition: this display name appears across 5 entry ids (107, 115, 123, 131, 99) — one per display context group. |
| 100 | Swordmaster | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 101 | Expected repetition: this display name appears across 5 entry ids (100, 108, 116, 124, 92) — one per display context group. |
| 101 | Shadow Warrior | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 102 | Expected repetition: this display name appears across 5 entry ids (101, 109, 117, 125, 93) — one per display context group. |
| 102 | White Lion | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 103 | Expected repetition: this display name appears across 5 entry ids (102, 110, 118, 126, 94) — one per display context group. |
| 103 | Archmage | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 104 | Expected repetition: this display name appears across 5 entry ids (103, 111, 119, 127, 95) — one per display context group. |
| 104 | Blackguard | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 105 | Expected repetition: this display name appears across 5 entry ids (104, 112, 120, 128, 96) — one per display context group. |
| 105 | Witch Elf | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 106 | Expected repetition: this display name appears across 5 entry ids (105, 113, 121, 129, 97) — one per display context group. |
| 106 | Disciple of Khaine | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 107 | Expected repetition: this display name appears across 5 entry ids (106, 114, 122, 130, 98) — one per display context group. |
| 107 | Sorcerer | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 108 | Expected repetition: this display name appears across 5 entry ids (107, 115, 123, 131, 99) — one per display context group. |
| 108 | Swordmaster | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 109 | Expected repetition: this display name appears across 5 entry ids (100, 108, 116, 124, 92) — one per display context group. |
| 109 | Shadow Warrior | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 110 | Expected repetition: this display name appears across 5 entry ids (101, 109, 117, 125, 93) — one per display context group. |
| 110 | White Lion | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 111 | Expected repetition: this display name appears across 5 entry ids (102, 110, 118, 126, 94) — one per display context group. |
| 111 | Archmage | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 112 | Expected repetition: this display name appears across 5 entry ids (103, 111, 119, 127, 95) — one per display context group. |
| 112 | Blackguard | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 113 | Expected repetition: this display name appears across 5 entry ids (104, 112, 120, 128, 96) — one per display context group. |
| 113 | Witch Elf | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 114 | Expected repetition: this display name appears across 5 entry ids (105, 113, 121, 129, 97) — one per display context group. |
| 114 | Disciple of Khaine | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 115 | Expected repetition: this display name appears across 5 entry ids (106, 114, 122, 130, 98) — one per display context group. |
| 115 | Sorcerer | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 116 | Expected repetition: this display name appears across 5 entry ids (107, 115, 123, 131, 99) — one per display context group. |
| 116 | Swordmaster | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 117 | Expected repetition: this display name appears across 5 entry ids (100, 108, 116, 124, 92) — one per display context group. |
| 117 | Shadow Warrior | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 118 | Expected repetition: this display name appears across 5 entry ids (101, 109, 117, 125, 93) — one per display context group. |
| 118 | White Lion | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 119 | Expected repetition: this display name appears across 5 entry ids (102, 110, 118, 126, 94) — one per display context group. |
| 119 | Archmage | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 120 | Expected repetition: this display name appears across 5 entry ids (103, 111, 119, 127, 95) — one per display context group. |
| 120 | Blackguard | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 121 | Expected repetition: this display name appears across 5 entry ids (104, 112, 120, 128, 96) — one per display context group. |
| 121 | Witch Elf | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 122 | Expected repetition: this display name appears across 5 entry ids (105, 113, 121, 129, 97) — one per display context group. |
| 122 | Disciple of Khaine | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 123 | Expected repetition: this display name appears across 5 entry ids (106, 114, 122, 130, 98) — one per display context group. |
| 123 | Sorcerer | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 124 | Expected repetition: this display name appears across 5 entry ids (107, 115, 123, 131, 99) — one per display context group. |
| 124 | Swordmaster | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 125 | Expected repetition: this display name appears across 5 entry ids (100, 108, 116, 124, 92) — one per display context group. |
| 125 | Shadow Warrior | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 126 | Expected repetition: this display name appears across 5 entry ids (101, 109, 117, 125, 93) — one per display context group. |
| 126 | White Lion | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 127 | Expected repetition: this display name appears across 5 entry ids (102, 110, 118, 126, 94) — one per display context group. |
| 127 | Archmage | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 128 | Expected repetition: this display name appears across 5 entry ids (103, 111, 119, 127, 95) — one per display context group. |
| 128 | Blackguard | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 129 | Expected repetition: this display name appears across 5 entry ids (104, 112, 120, 128, 96) — one per display context group. |
| 129 | Witch Elf | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 130 | Expected repetition: this display name appears across 5 entry ids (105, 113, 121, 129, 97) — one per display context group. |
| 130 | Disciple of Khaine | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 131 | Expected repetition: this display name appears across 5 entry ids (106, 114, 122, 130, 98) — one per display context group. |
| 131 | Sorcerer | Confirmed | careernames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\careernames_m.txt | line 132 | Expected repetition: this display name appears across 5 entry ids (107, 115, 123, 131, 99) — one per display context group. |

## Race Entry IDs

- Domain key: `Race.EntryId`
- Confidence: `Confirmed`
- Canonicality: `Canonical for the extracted race display-name domain`
- Source files: `racenames_m.txt`
- Recommended usage: Use for extracted-client race display names only.
- Notes: These entry ids come directly from racenames_m.txt. They are stable for the race-name string table and are safe to call Race IDs in that specific client-string domain.

## Values

| RawValue | Meaning | Confidence | Source | Path | Location | Notes |
| --- | --- | --- | --- | --- | --- | --- |
| 0 | (empty) | Confirmed | racenames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\racenames_m.txt | line 1 |  |
| 1 | Dwarf | Confirmed | racenames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\racenames_m.txt | line 2 |  |
| 2 | Orc | Confirmed | racenames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\racenames_m.txt | line 3 |  |
| 3 | Goblin | Confirmed | racenames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\racenames_m.txt | line 4 |  |
| 4 | High Elf | Confirmed | racenames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\racenames_m.txt | line 5 |  |
| 5 | Dark Elf | Confirmed | racenames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\racenames_m.txt | line 6 |  |
| 6 | Empire | Confirmed | racenames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\racenames_m.txt | line 7 |  |
| 7 | Chaos | Confirmed | racenames_m.txt | C:\Users\Admin\Downloads\myps\data\strings\english\racenames_m.txt | line 8 |  |
