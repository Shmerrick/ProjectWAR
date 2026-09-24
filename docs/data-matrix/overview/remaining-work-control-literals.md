# Remaining Work Control Literal Crosswalk

Generated UTC: `2026-09-14T19:42:02.8903680Z`

Extracted root: `C:\Users\Admin\Downloads\myps`

Filter: Focused on repeated numeric literals from CC, APPLY_ABILITY, KNOCKBACK, IMMUNITY, and requirement `ExtData[*].Val6`; returning 24 literal(s).

Literals: 24

## Literal Summary

| Rank | Score | RawValue | Obs | Sources | Abilities | Requirements | Interpretation | Contexts | SourceKeys |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 609147 | 1016 | 22 | 6 | 165 | 5 | Root-side control/status literal | CrowdControl, Damage, Disarm, Immunity, Knockback, Root, Silence, Snare, Stagger | CC ExtData[*].Val6, IMMUNITY ExtData[*].Val6, Requirements ExtData[*].Val6, KNOCKBACK ExtData[*].Val6, APPLY_ABILITY ExtData[*].Val6, IMMUNITY Value[0] |
| 2 | 503418 | 708 | 43 | 5 | 95 | 1 | Movement-control profile with strongest knockback usage | Damage, Knockback, Snare, Stun | KNOCKBACK ExtData[*].Val6, APPLY_ABILITY ExtData[*].Val6, CC ExtData[*].Val6, IMMUNITY Value[0], Requirements ExtData[*].Val6 |
| 3 | 454207 | 100 | 432 | 4 | 431 | 43 |  | Damage, Disarm, Heal, Silence, Snare | APPLY_ABILITY ExtData[*].Val6, Requirements ExtData[*].Val6, CC ExtData[*].Val6, KNOCKBACK ExtData[*].Val6 |
| 4 | 426178 | 445 | 53 | 4 | 5 | 26 | Hatred threshold selector family | Damage, Knockback | Requirements ExtData[*].Val6, CC ExtData[*].Val6, KNOCKBACK ExtData[*].Val6, APPLY_ABILITY ExtData[*].Val6 |
| 5 | 413192 | 48 | 17 | 4 | 367 | 4 |  | Damage, Disarm, Heal, Immunity, Knockdown, Root, Silence, Stagger, Stun | APPLY_ABILITY ExtData[*].Val6, Requirements ExtData[*].Val6, CC ExtData[*].Val6, IMMUNITY Value[0] |
| 6 | 412668 | 1 | 168 | 4 | 140 | 9 |  | Damage, Heal, Immunity, Knockdown, Root, Snare, Stun | CC FlagsRaw, APPLY_ABILITY ExtData[*].Val6, IMMUNITY Value[0], Requirements ExtData[*].Val6 |
| 7 | 406635 | 2 | 60 | 4 | 23 | 6 |  | Damage, Heal | APPLY_ABILITY ExtData[*].Val6, Requirements ExtData[*].Val6, CC ExtData[*].Val6, CC FlagsRaw |
| 8 | 404019 | 1030 | 44 | 4 | 39 | 3 | Stun control/status literal | CrowdControl, Damage, Snare, Stagger | IMMUNITY ExtData[*].Val6, KNOCKBACK ExtData[*].Val6, Requirements ExtData[*].Val6, IMMUNITY Value[0] |
| 9 | 403361 | 1019 | 11 | 4 | 14 | 3 | Snare control/status literal | CrowdControl, Damage, Disarm, Immunity, Root, Snare, Stun | IMMUNITY ExtData[*].Val6, Requirements ExtData[*].Val6, IMMUNITY Value[0], APPLY_ABILITY ExtData[*].Val6 |
| 10 | 402949 | 21 | 24 | 4 | 77 | 1 |  | Damage, Immunity, Knockback, Knockdown, Root, Silence, Snare, Stun | CC ExtData[*].Val6, APPLY_ABILITY ExtData[*].Val6, IMMUNITY Value[0], Requirements ExtData[*].Val6 |
| 11 | 401260 | 1014 | 10 | 4 | 10 | 1 | Knockback-side control/status literal | CrowdControl, Immunity, Knockback, Knockdown, Root, Stagger | IMMUNITY ExtData[*].Val6, IMMUNITY Value[0], APPLY_ABILITY ExtData[*].Val6, Requirements ExtData[*].Val6 |
| 12 | 308235 | 1002 | 10 | 3 | 9 | 8 |  | Damage | Requirements ExtData[*].Val6, APPLY_ABILITY ExtData[*].Val6, IMMUNITY Value[0] |
| 13 | 307184 | 1001 | 9 | 3 | 7 | 7 |  | Damage | Requirements ExtData[*].Val6, CC ExtData[*].Val6, IMMUNITY Value[0] |
| 14 | 305492 | 3 | 42 | 3 | 18 | 5 |  | Damage, Heal | APPLY_ABILITY ExtData[*].Val6, Requirements ExtData[*].Val6, IMMUNITY Value[0] |
| 15 | 305456 | 4 | 31 | 3 | 17 | 5 |  | Damage, Heal | APPLY_ABILITY ExtData[*].Val6, Requirements ExtData[*].Val6, CC FlagsRaw |
| 16 | 305351 | 12 | 76 | 3 | 51 | 4 |  | CrowdControl, Immunity, Knockback, Knockdown, Root, Snare, Stagger | IMMUNITY Value[0], Requirements ExtData[*].Val6, CC FlagsRaw |
| 17 | 305241 | 1018 | 41 | 3 | 48 | 4 |  | CrowdControl, Damage, Disarm, Root, Silence | IMMUNITY ExtData[*].Val6, Requirements ExtData[*].Val6, IMMUNITY Value[0] |
| 18 | 304515 | 24 | 15 | 3 | 60 | 3 |  | CrowdControl, Immunity, Knockback, Knockdown, Root, Silence, Stagger | IMMUNITY Value[0], CC FlagsRaw, Requirements ExtData[*].Val6 |
| 19 | 303916 | 1015 | 41 | 3 | 35 | 3 | Knockdown control/status literal | CrowdControl | IMMUNITY ExtData[*].Val6, Requirements ExtData[*].Val6, IMMUNITY Value[0] |
| 20 | 303184 | 1020 | 9 | 3 | 7 | 3 |  | Immunity | IMMUNITY Value[0], Requirements ExtData[*].Val6, IMMUNITY ExtData[*].Val6 |
| 21 | 301079 | 1023 | 4 | 3 | 3 | 1 |  |  | IMMUNITY Value[0], CC FlagsRaw, Requirements ExtData[*].Val6 |
| 22 | 300320 | 38 | 20 | 3 | 12 | 0 |  | Damage, Disarm | CC FlagsRaw, IMMUNITY Value[0], KNOCKBACK Value[0] |
| 23 | 206182 | 1004 | 7 | 2 | 7 | 6 |  | Damage | Requirements ExtData[*].Val6, IMMUNITY Value[0] |
| 24 | 206139 | 420 | 14 | 2 | 5 | 6 |  | Damage | Requirements ExtData[*].Val6, APPLY_ABILITY ExtData[*].Val6 |

## Literal `1016`

- Rank: `1`
- Score: `609147`
- Observations: `22`
- Distinct sources: `6`
- Distinct components: `17`
- Distinct abilities: `165`
- Distinct requirements: `5`
- Sample ability ids: `5, 122, 408, 411, 608, 648, 670, 672, 1365, 1370, 1377, 1418`
- Sample requirement ids: `9093, 9214, 9410, 9441, 9487`
- Interpretation: Root-side control/status literal
- Notes: High-confidence from `Immovable`, breakable-root CC families, paired APPLY_ABILITY control overlays, and requirement `9093`.
- Contexts: CrowdControl, Damage, Disarm, Immunity, Knockback, Root, Silence, Snare, Stagger

Summary: Observed 22 time(s) across 6 source group(s): CC ExtData[*].Val6, IMMUNITY ExtData[*].Val6, Requirements ExtData[*].Val6, KNOCKBACK ExtData[*].Val6, APPLY_ABILITY ExtData[*].Val6, IMMUNITY Value[0]

## Source Breakdown

| Source | Obs | Components | Abilities | Requirements | AbilityIds | RequirementIds | Triggers | Contexts | Companions |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| CC ExtData[*].Val6 | 6 | 6 | 15 | 0 | 1370, 1418, 1519, 1681, 8024, 8069, 8168, 8336, 8449, 8480, 9018, 9224 |  | OnApply, OnPreviousComponentApplied | Damage, Root, Snare | FlagsRaw=1 (6), Val1=2 (6), Val2=6 (6), Val3=1 (6), Val4=8 (6) |
| IMMUNITY ExtData[*].Val6 | 6 | 6 | 7 | 0 | 408, 411, 3255, 8408, 9173, 14271, 27832 |  | OnApply, OnPreviousComponentApplied | CrowdControl, Immunity, Root, Snare | Val1=7 (6), Val2=8 (6), Val3=6 (6), Value[0]=12 (6), Value[1]=100 (6) |
| Requirements ExtData[*].Val6 | 5 | 0 | 12 | 5 | 122, 1377, 1740, 3631, 4800, 5209, 8019, 8330, 9016, 9186, 9330, 13003 | 9093, 9214, 9410, 9441, 9487 |  | Damage, Disarm, Root | Val1=1 (5), Val2=6 (4), Val3=1 (3), Val3=6 (2), Val4=8 (2) |
| KNOCKBACK ExtData[*].Val6 | 3 | 3 | 4 | 0 | 1831, 8094, 9092, 9396 |  | OnApply, OnPreviousComponentApplied | Damage, Snare, Stagger | Val1=2 (3), Val2=6 (3), Val3=1 (3), Val4=8 (3), Value[0]=25180 (2) |
| APPLY_ABILITY ExtData[*].Val6 | 1 | 1 | 141 | 0 | 5, 608, 648, 670, 672, 1365, 1370, 1418, 1519, 1520, 1531, 1540 |  | OnBuffEnded, OnPreviousComponentApplied | Damage, Knockback, Root, Silence, Snare | Val1=2 (1), Val2=8 (1), Val3=6 (1), Val4=9 (1), Val9=1 (1) |
| IMMUNITY Value[0] | 1 | 1 | 1 | 0 | 13196 |  | OnApply |  | FlagsRaw=1 (1), Value[1]=100 (1) |

## Literal `708`

- Rank: `2`
- Score: `503418`
- Observations: `43`
- Distinct sources: `5`
- Distinct components: `41`
- Distinct abilities: `95`
- Distinct requirements: `1`
- Sample ability ids: `4651, 4652, 4653, 4654, 4655, 4656, 4657, 4658, 4659, 4660, 4661, 4662`
- Sample requirement ids: `9091`
- Interpretation: Movement-control profile with strongest knockback usage
- Notes: Anchored by generic `Knockback`, `Triumphant Blasting`, `Exile`, `Snare Net`, and `Boss Immunities`. Safe reading: reused movement-control/displacement family spanning `KNOCKBACK`, `VELOCITY`, and `CC`; exact retail enum name is still unresolved.
- Contexts: Damage, Knockback, Snare, Stun

Summary: Observed 43 time(s) across 5 source group(s): KNOCKBACK ExtData[*].Val6, APPLY_ABILITY ExtData[*].Val6, CC ExtData[*].Val6, IMMUNITY Value[0], Requirements ExtData[*].Val6

## Source Breakdown

| Source | Obs | Components | Abilities | Requirements | AbilityIds | RequirementIds | Triggers | Contexts | Companions |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| KNOCKBACK ExtData[*].Val6 | 39 | 38 | 92 | 0 | 4651, 4652, 4653, 4654, 4655, 4656, 4657, 4658, 4659, 4660, 4661, 4662 |  | OnApply, OnBuffEnded, OnPreviousComponentApplied | Knockback, Stun | Val2=6 (39), Val3=1 (39), Val4=8 (39), Val9=1 (38), Val1=2 (25) |
| APPLY_ABILITY ExtData[*].Val6 | 1 | 1 | 1 | 0 | 9514 |  | OnEventTriggered |  | Val1=2 (1), Val2=6 (1), Val3=1 (1), Val4=8 (1), Val9=1 (1) |
| CC ExtData[*].Val6 | 1 | 1 | 1 | 0 | 24824 |  | OnPreviousComponentTick | Damage, Snare | Val1=2 (1), Val2=6 (1), Val3=6 (1), Val4=8 (1), Val7=1 (1) |
| IMMUNITY Value[0] | 1 | 1 | 1 | 0 | 23894 |  | OnApply |  | FlagsRaw=1 (1), Val1=3 (1), Val2=11 (1), Val3=1 (1), Val4=8 (1) |
| Requirements ExtData[*].Val6 | 1 | 0 | 4 | 1 | 14157, 14301, 14302, 14303 | 9091 |  |  | Val1=1 (1), Val2=6 (1), Val3=1 (1), Val4=8 (1) |

## Literal `100`

- Rank: `3`
- Score: `454207`
- Observations: `432`
- Distinct sources: `4`
- Distinct components: `389`
- Distinct abilities: `431`
- Distinct requirements: `43`
- Sample ability ids: `758, 776, 779, 797, 800, 818, 1432, 1438, 1439, 1443, 1447, 1449`
- Sample requirement ids: `9004, 9005, 9006, 9007, 9008, 9116, 9117, 9126, 9128, 9130, 9132, 9133`
- Contexts: Damage, Disarm, Heal, Silence, Snare

Summary: Observed 432 time(s) across 4 source group(s): APPLY_ABILITY ExtData[*].Val6, Requirements ExtData[*].Val6, CC ExtData[*].Val6, KNOCKBACK ExtData[*].Val6

## Source Breakdown

| Source | Obs | Components | Abilities | Requirements | AbilityIds | RequirementIds | Triggers | Contexts | Companions |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| APPLY_ABILITY ExtData[*].Val6 | 373 | 373 | 384 | 0 | 758, 776, 779, 797, 800, 818, 1432, 1438, 1439, 1443, 1447, 1449 |  | OnApply, OnBuffEndedRemoved, OnEventTriggered, OnPreviousComponentApplied, OnPreviousComponentBuffTick, OnPreviousComponentTick | Damage, Heal, Silence, Snare | Val2=9 (373), Val3=4 (373), Val4=8 (373), Val1=1 (366), Val7=5 (171) |
| Requirements ExtData[*].Val6 | 43 | 0 | 16 | 43 | 8158, 8159, 8160, 8163, 8165, 8166, 8169, 8170, 9470, 9471, 9472, 9475 | 9004, 9005, 9006, 9007, 9008, 9116, 9117, 9126, 9128, 9130, 9132, 9133 |  | Damage, Disarm, Heal, Silence, Snare | Val1=1 (43), Val2=9 (42), Val3=4 (39), Val4=8 (25), Val4=9 (7) |
| CC ExtData[*].Val6 | 15 | 15 | 31 | 0 | 3636, 4091, 4334, 5011, 5707, 14379, 14380, 14383, 14384, 14387, 14388, 14391 |  | OnApply, OnPreviousComponentApplied, OnPreviousComponentTick |  | Val2=9 (15), Val3=4 (15), Val4=8 (15), Val1=1 (12), Value15=4 (12) |
| KNOCKBACK ExtData[*].Val6 | 1 | 1 | 6 | 0 | 17005, 17013, 17021, 17029, 17037, 17045 |  | OnPreviousComponentApplied |  | Val1=2 (1), Val2=9 (1), Val3=4 (1), Val4=8 (1), Val7=50 (1) |

## Literal `445`

- Rank: `4`
- Score: `426178`
- Observations: `53`
- Distinct sources: `4`
- Distinct components: `16`
- Distinct abilities: `5`
- Distinct requirements: `26`
- Sample ability ids: `3263, 3482, 3483, 9328, 9345`
- Sample requirement ids: `9373, 9374, 9375, 9376, 9377, 9378, 9379, 9380, 9381, 9382, 9383, 9384`
- Interpretation: Hatred threshold selector family
- Notes: High-confidence from `Exile`, `None Shall Pass`, `Hatred`, and Black Guard requirement ladders. `Val7` behaves like bracket edges or floor thresholds, and paired `3/5` plus `3/4` rows create ranges like `0-29`, `30-59`, `60-89`, and `90-100`. Do not use effect-id `445` (`Siege Wrecker`) as semantic evidence for this literal; that is a cross-domain numeric collision.
- Contexts: Damage, Knockback

Summary: Observed 53 time(s) across 4 source group(s): Requirements ExtData[*].Val6, CC ExtData[*].Val6, KNOCKBACK ExtData[*].Val6, APPLY_ABILITY ExtData[*].Val6

## Source Breakdown

| Source | Obs | Components | Abilities | Requirements | AbilityIds | RequirementIds | Triggers | Contexts | Companions |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Requirements ExtData[*].Val6 | 26 | 0 | 0 | 26 |  | 9373, 9374, 9375, 9376, 9377, 9378, 9379, 9380, 9381, 9382, 9383, 9384 |  |  | Val1=1 (26), Val2=3 (26), Val3=5 (26), Val4=8 (26), Val7=15 (7) |
| CC ExtData[*].Val6 | 12 | 7 | 1 | 0 | 3482 |  | OnApply |  | FlagsRaw=2175 (12), Val1=1 (12), Val2=3 (12), Val4=8 (12), Val5=3 (12) |
| KNOCKBACK ExtData[*].Val6 | 8 | 5 | 1 | 0 | 3483 |  | OnApply |  | Val1=1 (8), Val2=3 (8), Val4=8 (8), Val5=3 (8), Value[3]=2 (6) |
| APPLY_ABILITY ExtData[*].Val6 | 7 | 4 | 3 | 0 | 3263, 9328, 9345 |  | OnApply, OnEventTriggered, OnPreviousComponentApplied | Damage, Knockback | Val1=1 (7), Val2=3 (7), Val4=8 (7), Val5=3 (7), Val3=5 (4) |

## Literal `48`

- Rank: `5`
- Score: `413192`
- Observations: `17`
- Distinct sources: `4`
- Distinct components: `13`
- Distinct abilities: `367`
- Distinct requirements: `4`
- Sample ability ids: `3, 6, 230, 379, 885, 906, 1369, 1384, 1443, 1494, 1525, 1536`
- Sample requirement ids: `9092, 9094, 9095, 9096`
- Contexts: Damage, Disarm, Heal, Immunity, Knockdown, Root, Silence, Stagger, Stun

Summary: Observed 17 time(s) across 4 source group(s): APPLY_ABILITY ExtData[*].Val6, Requirements ExtData[*].Val6, CC ExtData[*].Val6, IMMUNITY Value[0]

## Source Breakdown

| Source | Obs | Components | Abilities | Requirements | AbilityIds | RequirementIds | Triggers | Contexts | Companions |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| APPLY_ABILITY ExtData[*].Val6 | 9 | 9 | 362 | 0 | 3, 6, 230, 379, 1369, 1384, 1443, 1494, 1525, 1536, 1607, 1613 |  | OnPreviousComponentApplied | Damage, Disarm, Heal, Immunity, Knockdown, Root, Silence, Stagger, Stun | Val1=2 (9), Val2=6 (9), Val3=1 (9), Val4=8 (9), Val9=1 (9) |
| Requirements ExtData[*].Val6 | 4 | 0 | 17 | 4 | 6, 885, 906, 1443, 1755, 3218, 3650, 8405, 8412, 8423, 8607, 9253 | 9092, 9094, 9095, 9096 |  | Damage, Disarm, Silence | Val1=1 (4), Val2=6 (4), Val3=1 (4), Val4=8 (4) |
| CC ExtData[*].Val6 | 3 | 3 | 8 | 0 | 3581, 3582, 3583, 3584, 3585, 9028, 27749, 27773 |  | OnApply, OnPreviousComponentApplied | Damage, Disarm, Immunity, Knockdown, Silence, Stun | FlagsRaw=2175 (3), Val1=2 (3), Val2=6 (3), Val3=1 (3), Val4=8 (3) |
| IMMUNITY Value[0] | 1 | 1 | 1 | 0 | 23894 |  | OnApply |  | FlagsRaw=1 (1), Val1=3 (1), Val2=11 (1), Val3=1 (1), Val4=8 (1) |

## Literal `1`

- Rank: `6`
- Score: `412668`
- Observations: `168`
- Distinct sources: `4`
- Distinct components: `159`
- Distinct abilities: `140`
- Distinct requirements: `9`
- Sample ability ids: `122, 264, 334, 608, 1370, 1418, 1444, 1519, 1602, 1681, 1734, 1748`
- Sample requirement ids: `9166, 9253, 9254, 9255, 9298, 9403, 9407, 9416, 9554`
- Contexts: Damage, Heal, Immunity, Knockdown, Root, Snare, Stun

Summary: Observed 168 time(s) across 4 source group(s): CC FlagsRaw, APPLY_ABILITY ExtData[*].Val6, IMMUNITY Value[0], Requirements ExtData[*].Val6

## Source Breakdown

| Source | Obs | Components | Abilities | Requirements | AbilityIds | RequirementIds | Triggers | Contexts | Companions |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| CC FlagsRaw | 73 | 73 | 82 | 0 | 122, 608, 1370, 1418, 1519, 1681, 3631, 3941, 4023, 4032, 4093, 4196 |  | OnApply, OnBuffEnded, OnPreviousComponentApplied | Damage, Immunity, Knockdown, Root, Snare, Stun | Value15=4 (65), Val3=1 (55), Val4=8 (55), Val7=1 (53), Val1=2 (50) |
| APPLY_ABILITY ExtData[*].Val6 | 60 | 60 | 22 | 0 | 264, 334, 1602, 1734, 1748, 3263, 3270, 3313, 3330, 5499, 9035, 9166 |  | OnApply, OnPreviousComponentApplied | Damage, Heal | Val1=1 (59), Val2=17 (59), Val4=8 (58), Val3=1 (54), FlagsRaw=16 (40) |
| IMMUNITY Value[0] | 26 | 26 | 32 | 0 | 4499, 5197, 5262, 5853, 10777, 10781, 10782, 13055, 13162, 13163, 13164, 13165 |  | OnApply, OnPreviousComponentApplied | Damage, Immunity | Value[1]=100 (24), Val4=8 (19), Val3=1 (15), Val7=1 (11), Val1=7 (9) |
| Requirements ExtData[*].Val6 | 9 | 0 | 4 | 9 | 1444, 1756, 3915, 9258 | 9166, 9253, 9254, 9255, 9298, 9403, 9407, 9416, 9554 |  | Heal | Val1=1 (9), Val3=6 (5), Val2=17 (4), Val3=1 (4), Val4=8 (4) |

## Literal `2`

- Rank: `7`
- Score: `406635`
- Observations: `60`
- Distinct sources: `4`
- Distinct components: `52`
- Distinct abilities: `23`
- Distinct requirements: `6`
- Sample ability ids: `1465, 1690, 1748, 1906, 3313, 3330, 3915, 4132, 5469, 5499, 8081, 9032`
- Sample requirement ids: `9076, 9295, 9403, 9407, 9438, 9554`
- Contexts: Damage, Heal

Summary: Observed 60 time(s) across 4 source group(s): APPLY_ABILITY ExtData[*].Val6, Requirements ExtData[*].Val6, CC ExtData[*].Val6, CC FlagsRaw

## Source Breakdown

| Source | Obs | Components | Abilities | Requirements | AbilityIds | RequirementIds | Triggers | Contexts | Companions |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| APPLY_ABILITY ExtData[*].Val6 | 52 | 50 | 16 | 0 | 1465, 1748, 1906, 3313, 3330, 5499, 8081, 9250, 24552, 24553, 24554, 24555 |  | OnApply, OnPreviousComponentApplied | Damage, Heal | Val1=1 (51), Val4=8 (50), Val2=17 (46), Val3=1 (46), FlagsRaw=16 (32) |
| Requirements ExtData[*].Val6 | 6 | 0 | 5 | 6 | 1690, 3915, 9032, 9258, 9321 | 9076, 9295, 9403, 9407, 9438, 9554 |  | Damage, Heal | Val1=1 (6), Val3=6 (5), Val4=9 (5), Val2=17 (3), Val2=82 (3) |
| CC ExtData[*].Val6 | 1 | 1 | 1 | 0 | 5469 |  | OnApply |  | FlagsRaw=2559 (1), Val1=2 (1), Val2=68 (1), Val3=6 (1), Val4=8 (1) |
| CC FlagsRaw | 1 | 1 | 1 | 0 | 4132 |  | OnApply |  | Val1=2 (1), Val2=2 (1), Val3=1 (1), Val4=8 (1), Val7=1 (1) |

## Literal `1030`

- Rank: `8`
- Score: `404019`
- Observations: `44`
- Distinct sources: `4`
- Distinct components: `41`
- Distinct abilities: `39`
- Distinct requirements: `3`
- Sample ability ids: `158, 159, 160, 161, 162, 163, 164, 165, 166, 167, 168, 169`
- Sample requirement ids: `9217, 9409, 9487`
- Interpretation: Stun control/status literal
- Notes: Recovered from toolkit enum-backed immunity notes and reused throughout the `Unstoppable` immunity bundle.
- Contexts: CrowdControl, Damage, Snare, Stagger

Summary: Observed 44 time(s) across 4 source group(s): IMMUNITY ExtData[*].Val6, KNOCKBACK ExtData[*].Val6, Requirements ExtData[*].Val6, IMMUNITY Value[0]

## Source Breakdown

| Source | Obs | Components | Abilities | Requirements | AbilityIds | RequirementIds | Triggers | Contexts | Companions |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| IMMUNITY ExtData[*].Val6 | 36 | 36 | 33 | 0 | 158, 159, 160, 161, 162, 163, 164, 165, 166, 167, 168, 169 |  | OnApply, OnPreviousComponentApplied | CrowdControl | Val2=8 (36), Val3=6 (36), Val4=9 (36), Value[0]=12 (36), Val1=3 (30) |
| KNOCKBACK ExtData[*].Val6 | 3 | 3 | 4 | 0 | 1831, 8094, 9092, 9396 |  | OnApply, OnPreviousComponentApplied | Damage, Snare, Stagger | Val1=2 (3), Val2=6 (3), Val3=1 (3), Val4=8 (3), Value[0]=25180 (2) |
| Requirements ExtData[*].Val6 | 3 | 0 | 0 | 3 |  | 9217, 9409, 9487 |  |  | Val1=1 (3), Val2=8 (2), Val3=6 (2), Val2=6 (1), Val3=1 (1) |
| IMMUNITY Value[0] | 2 | 2 | 2 | 0 | 3176, 13196 |  | OnApply |  | FlagsRaw=1 (2), Val1=2 (1), Val2=1 (1), Val3=1 (1), Val4=8 (1) |

## Literal `1019`

- Rank: `9`
- Score: `403361`
- Observations: `11`
- Distinct sources: `4`
- Distinct components: `8`
- Distinct abilities: `14`
- Distinct requirements: `3`
- Sample ability ids: `1377, 1740, 1774, 3882, 8019, 8330, 8408, 9016, 9173, 9194, 9330, 13196`
- Sample requirement ids: `9220, 9221, 9410`
- Interpretation: Snare control/status literal
- Notes: Recovered from toolkit enum-backed immunity notes and consistent with snare-linked control families.
- Contexts: CrowdControl, Damage, Disarm, Immunity, Root, Snare, Stun

Summary: Observed 11 time(s) across 4 source group(s): IMMUNITY ExtData[*].Val6, Requirements ExtData[*].Val6, IMMUNITY Value[0], APPLY_ABILITY ExtData[*].Val6

## Source Breakdown

| Source | Obs | Components | Abilities | Requirements | AbilityIds | RequirementIds | Triggers | Contexts | Companions |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| IMMUNITY ExtData[*].Val6 | 5 | 5 | 4 | 0 | 9173, 9194, 14271, 27832 |  | OnApply | CrowdControl, Damage, Immunity, Root, Snare, Stun | Val1=7 (5), Val2=8 (5), Val3=6 (5), Value[0]=8 (5), Value[1]=100 (5) |
| Requirements ExtData[*].Val6 | 3 | 0 | 6 | 3 | 1377, 1740, 8019, 8330, 9016, 9330 | 9220, 9221, 9410 |  | Disarm, Root | Val1=1 (3), Val2=6 (3), Val4=9 (2), Val3=1 (1), Val3=5 (1) |
| IMMUNITY Value[0] | 2 | 2 | 3 | 0 | 3882, 8408, 13196 |  | OnApply, OnPreviousComponentApplied | Immunity, Root, Snare | FlagsRaw=1 (2), Value[1]=100 (2) |
| APPLY_ABILITY ExtData[*].Val6 | 1 | 1 | 1 | 0 | 1774 |  | OnPreviousComponentApplied | Damage, Snare | Val1=1 (1), Val2=6 (1), Val3=1 (1), Val4=8 (1), Value[0]=3302 (1) |

## Literal `21`

- Rank: `10`
- Score: `402949`
- Observations: `24`
- Distinct sources: `4`
- Distinct components: `23`
- Distinct abilities: `77`
- Distinct requirements: `1`
- Sample ability ids: `5, 10, 648, 670, 672, 882, 903, 1010, 1365, 1520, 1531, 1540`
- Sample requirement ids: `9090`
- Contexts: Damage, Immunity, Knockback, Knockdown, Root, Silence, Snare, Stun

Summary: Observed 24 time(s) across 4 source group(s): CC ExtData[*].Val6, APPLY_ABILITY ExtData[*].Val6, IMMUNITY Value[0], Requirements ExtData[*].Val6

## Source Breakdown

| Source | Obs | Components | Abilities | Requirements | AbilityIds | RequirementIds | Triggers | Contexts | Companions |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| CC ExtData[*].Val6 | 13 | 13 | 14 | 0 | 4022, 4073, 4091, 4094, 4126, 4209, 5011, 5015, 5024, 5043, 5046, 5993 |  | OnApply, OnPreviousComponentApplied, OnPreviousComponentTick | Stun | Val2=6 (13), Val3=1 (13), Val4=8 (13), Val9=1 (13), FlagsRaw=255 (11) |
| APPLY_ABILITY ExtData[*].Val6 | 9 | 9 | 60 | 0 | 5, 10, 648, 670, 672, 882, 903, 1010, 1365, 1520, 1531, 1540 |  | OnApply, OnPreviousComponentApplied | Damage, Immunity, Knockback, Knockdown, Root, Silence, Snare | Val1=1 (9), Val2=6 (9), Val3=1 (9), Val4=8 (9), Val9=1 (9) |
| IMMUNITY Value[0] | 1 | 1 | 2 | 0 | 5976, 23894 |  | OnApply |  | Val1=3 (1), Val2=11 (1), Val3=1 (1), Val4=8 (1), Val5=3 (1) |
| Requirements ExtData[*].Val6 | 1 | 0 | 1 | 1 | 3665 | 9090 |  |  | Val1=1 (1), Val2=8 (1), Val3=1 (1), Val4=9 (1), Val7=1 (1) |

## Literal `1014`

- Rank: `11`
- Score: `401260`
- Observations: `10`
- Distinct sources: `4`
- Distinct components: `9`
- Distinct abilities: `10`
- Distinct requirements: `1`
- Sample ability ids: `186, 187, 408, 3180, 8499, 13196, 14045, 14271, 27832, 28300`
- Sample requirement ids: `9615`
- Interpretation: Knockback-side control/status literal
- Notes: Observed beside `IMMUNITY Value[0]=24/46` and adjacent to known knockback-side immunity families. Strong domain read; exact client label is still unresolved.
- Contexts: CrowdControl, Immunity, Knockback, Knockdown, Root, Stagger

Summary: Observed 10 time(s) across 4 source group(s): IMMUNITY ExtData[*].Val6, IMMUNITY Value[0], APPLY_ABILITY ExtData[*].Val6, Requirements ExtData[*].Val6

## Source Breakdown

| Source | Obs | Components | Abilities | Requirements | AbilityIds | RequirementIds | Triggers | Contexts | Companions |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| IMMUNITY ExtData[*].Val6 | 6 | 6 | 6 | 0 | 186, 187, 408, 3180, 14271, 27832 |  | OnApply | CrowdControl | Val1=7 (6), Val2=8 (6), Val3=6 (6), Val4=8 (6), Value[1]=100 (6) |
| IMMUNITY Value[0] | 2 | 2 | 2 | 0 | 13196, 14045 |  | OnApply |  | FlagsRaw=1 (2), Value[1]=100 (2), Val1=2 (1), Val2=40 (1), Val3=6 (1) |
| APPLY_ABILITY ExtData[*].Val6 | 1 | 1 | 1 | 0 | 8499 |  | OnPreviousComponentApplied |  | Val1=1 (1), Val2=6 (1), Val3=1 (1), Val4=8 (1), Val9=1 (1) |
| Requirements ExtData[*].Val6 | 1 | 0 | 1 | 1 | 28300 | 9615 |  | Immunity, Knockback, Knockdown, Root, Stagger | Val1=1 (1), Val2=8 (1), Val3=6 (1), Val4=8 (1) |

## Literal `1002`

- Rank: `12`
- Score: `308235`
- Observations: `10`
- Distinct sources: `3`
- Distinct components: `2`
- Distinct abilities: `9`
- Distinct requirements: `8`
- Sample ability ids: `1602, 1906, 8167, 8246, 8554, 9167, 9580, 13191, 27826`
- Sample requirement ids: `9246, 9300, 9301, 9303, 9447, 9454, 9603, 9614`
- Contexts: Damage

Summary: Observed 10 time(s) across 3 source group(s): Requirements ExtData[*].Val6, APPLY_ABILITY ExtData[*].Val6, IMMUNITY Value[0]

## Source Breakdown

| Source | Obs | Components | Abilities | Requirements | AbilityIds | RequirementIds | Triggers | Contexts | Companions |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Requirements ExtData[*].Val6 | 8 | 0 | 7 | 8 | 1602, 1906, 8167, 8246, 8554, 9167, 27826 | 9246, 9300, 9301, 9303, 9447, 9454, 9603, 9614 |  | Damage | Val1=1 (8), Val2=6 (8), Val3=6 (7), Val4=9 (7), Val9=1 (4) |
| APPLY_ABILITY ExtData[*].Val6 | 1 | 1 | 1 | 0 | 9580 |  | OnPreviousComponentApplied | Damage | Val1=1 (1), Val2=6 (1), Val3=5 (1), Val4=8 (1), Val7=1 (1) |
| IMMUNITY Value[0] | 1 | 1 | 1 | 0 | 13191 |  | OnPreviousComponentApplied |  | FlagsRaw=1 (1), Value[1]=100 (1) |

## Literal `1001`

- Rank: `13`
- Score: `307184`
- Observations: `9`
- Distinct sources: `3`
- Distinct components: `2`
- Distinct abilities: `7`
- Distinct requirements: `7`
- Sample ability ids: `8167, 8246, 8554, 9244, 9556, 13191, 27826`
- Sample requirement ids: `9301, 9302, 9303, 9447, 9454, 9603, 9614`
- Contexts: Damage

Summary: Observed 9 time(s) across 3 source group(s): Requirements ExtData[*].Val6, CC ExtData[*].Val6, IMMUNITY Value[0]

## Source Breakdown

| Source | Obs | Components | Abilities | Requirements | AbilityIds | RequirementIds | Triggers | Contexts | Companions |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Requirements ExtData[*].Val6 | 7 | 0 | 6 | 7 | 8167, 8246, 8554, 9244, 9556, 27826 | 9301, 9302, 9303, 9447, 9454, 9603, 9614 |  | Damage | Val1=1 (7), Val2=6 (7), Val3=6 (6), Val9=1 (4), Val4=9 (2) |
| CC ExtData[*].Val6 | 1 | 1 | 0 | 0 |  |  |  |  | FlagsRaw=2175 (1), Val1=1 (1), Val2=6 (1), Val3=6 (1), Val4=8 (1) |
| IMMUNITY Value[0] | 1 | 1 | 1 | 0 | 13191 |  | OnPreviousComponentApplied |  | FlagsRaw=1 (1), Value[1]=100 (1) |

## Literal `3`

- Rank: `14`
- Score: `305492`
- Observations: `42`
- Distinct sources: `3`
- Distinct components: `37`
- Distinct abilities: `18`
- Distinct requirements: `5`
- Sample ability ids: `1465, 1748, 3313, 3330, 3915, 5499, 9250, 9258, 10320, 24552, 24553, 24554`
- Sample requirement ids: `9152, 9213, 9403, 9407, 9554`
- Contexts: Damage, Heal

Summary: Observed 42 time(s) across 3 source group(s): APPLY_ABILITY ExtData[*].Val6, Requirements ExtData[*].Val6, IMMUNITY Value[0]

## Source Breakdown

| Source | Obs | Components | Abilities | Requirements | AbilityIds | RequirementIds | Triggers | Contexts | Companions |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| APPLY_ABILITY ExtData[*].Val6 | 36 | 36 | 15 | 0 | 1465, 1748, 3313, 3330, 5499, 9250, 10320, 24552, 24553, 24554, 24555, 24556 |  | OnApply | Damage, Heal | Val1=1 (35), Val4=8 (34), Val3=1 (32), Val2=17 (30), FlagsRaw=16 (24) |
| Requirements ExtData[*].Val6 | 5 | 0 | 2 | 5 | 3915, 9258 | 9152, 9213, 9403, 9407, 9554 |  | Heal | Val1=1 (5), Val3=6 (4), Val4=9 (4), Val2=17 (3), Val2=82 (2) |
| IMMUNITY Value[0] | 1 | 1 | 1 | 0 | 24830 |  | OnApply |  | Val1=7 (1), Val2=8 (1), Val3=6 (1), Val4=8 (1), Val6=4211 (1) |

## Literal `4`

- Rank: `15`
- Score: `305456`
- Observations: `31`
- Distinct sources: `3`
- Distinct components: `26`
- Distinct abilities: `17`
- Distinct requirements: `5`
- Sample ability ids: `1748, 3313, 3330, 3915, 4043, 5499, 9032, 9250, 9258, 24552, 24553, 24554`
- Sample requirement ids: `9295, 9403, 9407, 9438, 9554`
- Contexts: Damage, Heal

Summary: Observed 31 time(s) across 3 source group(s): APPLY_ABILITY ExtData[*].Val6, Requirements ExtData[*].Val6, CC FlagsRaw

## Source Breakdown

| Source | Obs | Components | Abilities | Requirements | AbilityIds | RequirementIds | Triggers | Contexts | Companions |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| APPLY_ABILITY ExtData[*].Val6 | 25 | 25 | 13 | 0 | 1748, 3313, 3330, 5499, 9250, 24552, 24553, 24554, 24555, 24556, 24557, 24558 |  | OnApply | Damage, Heal | Val1=1 (24), Val4=8 (24), Val3=1 (22), Val2=17 (21), FlagsRaw=16 (16) |
| Requirements ExtData[*].Val6 | 5 | 0 | 3 | 5 | 3915, 9032, 9258 | 9295, 9403, 9407, 9438, 9554 |  | Damage, Heal | Val1=1 (5), Val4=9 (5), Val3=6 (4), Val2=17 (3), Val2=82 (2) |
| CC FlagsRaw | 1 | 1 | 1 | 0 | 4043 |  | OnApply |  |  |

## Literal `12`

- Rank: `16`
- Score: `305351`
- Observations: `76`
- Distinct sources: `3`
- Distinct components: `72`
- Distinct abilities: `51`
- Distinct requirements: `4`
- Sample ability ids: `158, 159, 160, 161, 162, 163, 164, 165, 166, 167, 168, 169`
- Sample requirement ids: `9620, 9621, 9623, 9624`
- Contexts: CrowdControl, Immunity, Knockback, Knockdown, Root, Snare, Stagger

Summary: Observed 76 time(s) across 3 source group(s): IMMUNITY Value[0], Requirements ExtData[*].Val6, CC FlagsRaw

## Source Breakdown

| Source | Obs | Components | Abilities | Requirements | AbilityIds | RequirementIds | Triggers | Contexts | Companions |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| IMMUNITY Value[0] | 70 | 70 | 49 | 0 | 158, 159, 160, 161, 162, 163, 164, 165, 166, 167, 168, 169 |  | OnApply, OnPreviousComponentApplied | CrowdControl, Immunity, Knockback, Knockdown, Root, Snare, Stagger | Val4=8 (61), Value[1]=100 (61), Val3=1 (52), Val2=8 (47), Val1=3 (46) |
| Requirements ExtData[*].Val6 | 4 | 0 | 0 | 4 |  | 9620, 9621, 9623, 9624 |  |  | Val1=1 (4), Val2=44 (4), Val5=3 (4), Val3=1 (2), Val3=6 (2) |
| CC FlagsRaw | 2 | 2 | 2 | 0 | 13389, 24882 |  | OnApply, OnPreviousComponentTick |  | Val1=2 (2), Val2=15 (2), Val3=1 (2), Val4=8 (2), Val7=1 (2) |

## Literal `1018`

- Rank: `17`
- Score: `305241`
- Observations: `41`
- Distinct sources: `3`
- Distinct components: `37`
- Distinct abilities: `48`
- Distinct requirements: `4`
- Sample ability ids: `158, 159, 160, 161, 162, 163, 164, 165, 166, 167, 168, 169`
- Sample requirement ids: `9094, 9216, 9409, 9410`
- Contexts: CrowdControl, Damage, Disarm, Root, Silence

Summary: Observed 41 time(s) across 3 source group(s): IMMUNITY ExtData[*].Val6, Requirements ExtData[*].Val6, IMMUNITY Value[0]

## Source Breakdown

| Source | Obs | Components | Abilities | Requirements | AbilityIds | RequirementIds | Triggers | Contexts | Companions |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| IMMUNITY ExtData[*].Val6 | 36 | 36 | 33 | 0 | 158, 159, 160, 161, 162, 163, 164, 165, 166, 167, 168, 169 |  | OnApply, OnPreviousComponentApplied | CrowdControl | Val2=8 (36), Val3=6 (36), Val4=9 (36), Value[0]=12 (36), Val1=3 (30) |
| Requirements ExtData[*].Val6 | 4 | 0 | 14 | 4 | 885, 906, 1377, 1740, 3218, 8019, 8330, 8607, 9016, 9253, 9304, 9330 | 9094, 9216, 9409, 9410 |  | Damage, Disarm, Root, Silence | Val1=1 (4), Val3=6 (3), Val2=6 (2), Val2=8 (2), Val3=1 (1) |
| IMMUNITY Value[0] | 1 | 1 | 1 | 0 | 13196 |  | OnApply |  | FlagsRaw=1 (1), Value[1]=100 (1) |

## Literal `24`

- Rank: `18`
- Score: `304515`
- Observations: `15`
- Distinct sources: `3`
- Distinct components: `12`
- Distinct abilities: `60`
- Distinct requirements: `3`
- Sample ability ids: `186, 398, 408, 608, 3180, 10784, 12007, 12017, 12136, 12276, 12279, 12336`
- Sample requirement ids: `9028, 9047, 9103`
- Contexts: CrowdControl, Immunity, Knockback, Knockdown, Root, Silence, Stagger

Summary: Observed 15 time(s) across 3 source group(s): IMMUNITY Value[0], CC FlagsRaw, Requirements ExtData[*].Val6

## Source Breakdown

| Source | Obs | Components | Abilities | Requirements | AbilityIds | RequirementIds | Triggers | Contexts | Companions |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| IMMUNITY Value[0] | 8 | 8 | 9 | 0 | 186, 398, 408, 608, 3180, 10784, 14271, 27832, 28300 |  | OnApply, OnPreviousComponentApplied | CrowdControl, Immunity, Knockback, Knockdown, Root, Stagger | Value[1]=100 (8), Val4=8 (5), Val1=7 (4), Val3=6 (4), Val2=8 (3) |
| CC FlagsRaw | 4 | 4 | 51 | 0 | 12007, 12017, 12136, 12276, 12279, 12336, 12456, 12475, 12638, 12646, 12665, 12718 |  | OnApply, OnPreviousComponentApplied | Silence | Val1=1 (4), Val1=2 (4), Val2=15 (4), Val2=2 (4), Val2=88 (4) |
| Requirements ExtData[*].Val6 | 3 | 0 | 0 | 3 |  | 9028, 9047, 9103 |  |  | Val1=1 (3), Val2=8 (3), Val3=6 (3), Val4=8 (1), Val4=9 (1) |

## Literal `1015`

- Rank: `19`
- Score: `303916`
- Observations: `41`
- Distinct sources: `3`
- Distinct components: `38`
- Distinct abilities: `35`
- Distinct requirements: `3`
- Sample ability ids: `158, 159, 160, 161, 162, 163, 164, 165, 166, 167, 168, 169`
- Sample requirement ids: `9409, 9487, 9488`
- Interpretation: Knockdown control/status literal
- Notes: Recovered from toolkit enum-backed immunity notes and reinforced by nearby requirement and immunity families.
- Contexts: CrowdControl

Summary: Observed 41 time(s) across 3 source group(s): IMMUNITY ExtData[*].Val6, Requirements ExtData[*].Val6, IMMUNITY Value[0]

## Source Breakdown

| Source | Obs | Components | Abilities | Requirements | AbilityIds | RequirementIds | Triggers | Contexts | Companions |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| IMMUNITY ExtData[*].Val6 | 36 | 36 | 33 | 0 | 158, 159, 160, 161, 162, 163, 164, 165, 166, 167, 168, 169 |  | OnApply, OnPreviousComponentApplied | CrowdControl | Val2=8 (36), Val3=6 (36), Val4=9 (36), Value[0]=12 (36), Val1=3 (30) |
| Requirements ExtData[*].Val6 | 3 | 0 | 0 | 3 |  | 9409, 9487, 9488 |  |  | Val1=1 (3), Val2=8 (2), Val3=6 (2), Val4=8 (2), Val2=6 (1) |
| IMMUNITY Value[0] | 2 | 2 | 2 | 0 | 3176, 13196 |  | OnApply |  | FlagsRaw=1 (2), Val1=2 (1), Val2=1 (1), Val3=1 (1), Val4=8 (1) |

## Literal `1020`

- Rank: `20`
- Score: `303184`
- Observations: `9`
- Distinct sources: `3`
- Distinct components: `5`
- Distinct abilities: `7`
- Distinct requirements: `3`
- Sample ability ids: `3948, 13401, 13410, 13416, 13697, 14045, 14477`
- Sample requirement ids: `9065, 9264, 9296`
- Contexts: Immunity

Summary: Observed 9 time(s) across 3 source group(s): IMMUNITY Value[0], Requirements ExtData[*].Val6, IMMUNITY ExtData[*].Val6

## Source Breakdown

| Source | Obs | Components | Abilities | Requirements | AbilityIds | RequirementIds | Triggers | Contexts | Companions |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| IMMUNITY Value[0] | 4 | 4 | 6 | 0 | 13401, 13410, 13416, 13697, 14045, 14477 |  | OnApply | Immunity | FlagsRaw=1 (4), Value[1]=100 (4), Val4=8 (2), Val1=2 (1), Val1=3 (1) |
| Requirements ExtData[*].Val6 | 3 | 0 | 1 | 3 | 3948 | 9065, 9264, 9296 |  |  | Val1=1 (3), Val2=8 (2), Val3=6 (2), Val4=8 (2), Val2=6 (1) |
| IMMUNITY ExtData[*].Val6 | 2 | 2 | 2 | 0 | 14045, 14477 |  | OnApply |  | FlagsRaw=1 (2), Val1=3 (2), Val2=8 (2), Val3=6 (2), Val4=8 (2) |

## Literal `1023`

- Rank: `21`
- Score: `301079`
- Observations: `4`
- Distinct sources: `3`
- Distinct components: `3`
- Distinct abilities: `3`
- Distinct requirements: `1`
- Sample ability ids: `490, 5999, 14045`
- Sample requirement ids: `9156`
- Contexts: 

Summary: Observed 4 time(s) across 3 source group(s): IMMUNITY Value[0], CC FlagsRaw, Requirements ExtData[*].Val6

## Source Breakdown

| Source | Obs | Components | Abilities | Requirements | AbilityIds | RequirementIds | Triggers | Contexts | Companions |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| IMMUNITY Value[0] | 2 | 2 | 1 | 0 | 14045 |  | OnApply |  | FlagsRaw=1 (2), Value[1]=100 (2), Val1=3 (1), Val2=8 (1), Val3=6 (1) |
| CC FlagsRaw | 1 | 1 | 1 | 0 | 5999 |  | OnPreviousComponentApplied |  | Val1=2 (1), Val2=15 (1), Val2=6 (1), Val3=1 (1), Val4=8 (1) |
| Requirements ExtData[*].Val6 | 1 | 0 | 1 | 1 | 490 | 9156 |  |  | Val1=1 (1), Val2=8 (1), Val3=6 (1), Val4=8 (1) |

## Literal `38`

- Rank: `22`
- Score: `300320`
- Observations: `20`
- Distinct sources: `3`
- Distinct components: `20`
- Distinct abilities: `12`
- Distinct requirements: `0`
- Sample ability ids: `1536, 1837, 1865, 3628, 3713, 4129, 8086, 8405, 8495, 9098, 9482, 23034`
- Sample requirement ids: ``
- Contexts: Damage, Disarm

Summary: Observed 20 time(s) across 3 source group(s): CC FlagsRaw, IMMUNITY Value[0], KNOCKBACK Value[0]

## Source Breakdown

| Source | Obs | Components | Abilities | Requirements | AbilityIds | RequirementIds | Triggers | Contexts | Companions |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| CC FlagsRaw | 18 | 18 | 12 | 0 | 1536, 1837, 1865, 3628, 3713, 4129, 8086, 8405, 8495, 9098, 9482, 23034 |  | OnApply, OnEventTriggered, OnPreviousComponentApplied | Damage, Disarm | Val1=2 (14), Val2=2 (14), Val3=1 (14), Val4=8 (14), Val7=1 (14) |
| IMMUNITY Value[0] | 1 | 1 | 0 | 0 |  |  |  |  |  |
| KNOCKBACK Value[0] | 1 | 1 | 0 | 0 |  |  |  |  | Val1=2 (1), Val2=2 (1), Val3=1 (1), Val4=8 (1), Val7=1 (1) |

## Literal `1004`

- Rank: `23`
- Score: `206182`
- Observations: `7`
- Distinct sources: `2`
- Distinct components: `1`
- Distinct abilities: `7`
- Distinct requirements: `6`
- Sample ability ids: `1602, 1906, 8167, 9244, 9556, 13191, 27826`
- Sample requirement ids: `9300, 9302, 9303, 9454, 9603, 9614`
- Contexts: Damage

Summary: Observed 7 time(s) across 2 source group(s): Requirements ExtData[*].Val6, IMMUNITY Value[0]

## Source Breakdown

| Source | Obs | Components | Abilities | Requirements | AbilityIds | RequirementIds | Triggers | Contexts | Companions |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Requirements ExtData[*].Val6 | 6 | 0 | 6 | 6 | 1602, 1906, 8167, 9244, 9556, 27826 | 9300, 9302, 9303, 9454, 9603, 9614 |  | Damage | Val1=1 (6), Val2=6 (6), Val3=6 (5), Val4=9 (3), Val9=1 (3) |
| IMMUNITY Value[0] | 1 | 1 | 1 | 0 | 13191 |  | OnPreviousComponentApplied |  | FlagsRaw=1 (1), Value[1]=100 (1) |

## Literal `420`

- Rank: `24`
- Score: `206139`
- Observations: `14`
- Distinct sources: `2`
- Distinct components: `3`
- Distinct abilities: `5`
- Distinct requirements: `6`
- Sample ability ids: `287, 291, 292, 3436, 9490`
- Sample requirement ids: `9146, 9183, 9184, 9185, 9186, 9187`
- Contexts: Damage

Summary: Observed 14 time(s) across 2 source group(s): Requirements ExtData[*].Val6, APPLY_ABILITY ExtData[*].Val6

## Source Breakdown

| Source | Obs | Components | Abilities | Requirements | AbilityIds | RequirementIds | Triggers | Contexts | Companions |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Requirements ExtData[*].Val6 | 11 | 0 | 5 | 6 | 287, 291, 292, 3436, 9490 | 9146, 9183, 9184, 9185, 9186, 9187 |  | Damage | Val1=1 (11), Val2=3 (11), Val3=4 (5), Val3=5 (5), Val4=8 (5) |
| APPLY_ABILITY ExtData[*].Val6 | 3 | 3 | 0 | 0 |  |  |  |  | Val1=1 (3), Val2=3 (3), Val3=4 (3), Val4=8 (3), Val5=3 (3) |

