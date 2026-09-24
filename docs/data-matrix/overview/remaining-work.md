# Remaining Work

Generated UTC: `2026-09-14T19:42:02.8664820Z`

Extracted root: `C:\Users\Admin\Downloads\myps`

## Summary

- Areas: 6
- Items: 10
- Critical: 3
- High: 6
- Coverage gaps: 3171
- High-signal conflicts: 0
- Unknown fields: 0
- Structural fields: 0
- Requirement rows with unresolved fields: 0
- Token gaps: 0
- Identity-domain risks: 0
- Default next batch file: `overview/remaining-work-next.md`
- Default operation field packet file: `overview/remaining-work-operation-fields.md`
- Default control literal file: `overview/remaining-work-control-literals.md`

## Next Batch Preview

| Global | Area | Priority | Score | Title | Subject | Action | ExampleAbilityId |
| --- | --- | --- | --- | --- | --- | --- | ---: |
| 1 | Coverage Gaps | Critical | 190 | Common missing pattern: csv, bin, effect-text, effect-row, components | csv, bin, effect-text, effect-row, components | Resolve the missing root effect link so ability flow can be traced through the client effect chain. | 40 |
| 2 | Coverage Gaps | Critical | 175 | Common missing pattern: csv, effect-text, effect-row | csv, effect-text, effect-row | Resolve the missing root effect link so ability flow can be traced through the client effect chain. | 38 |
| 3 | Coverage Gaps | Critical | 175 | Common missing pattern: effect-text, effect-row | effect-text, effect-row | Resolve the missing root effect link so ability flow can be traced through the client effect chain. | 609 |
| 4 | Coverage Gaps | High | 167 | Common missing pattern: csv, effect-text, effect-row, components | csv, effect-text, effect-row, components | Resolve the missing root effect link so ability flow can be traced through the client effect chain. | 5979 |
| 5 | Coverage Gaps | High | 166 | Common missing pattern: csv, bin, effect-row, components | csv, bin, effect-row, components | Resolve the missing root effect link so ability flow can be traced through the client effect chain. | 20 |
| 6 | Coverage Gaps | High | 160 | StringsOnly ability bucket | StringsOnly | Find the missing BIN, effect, and component evidence so this stops being a text-only ability shell. | 20 |
| 7 | Coverage Gaps | High | 154 | Common missing pattern: effect-text, components | effect-text, components | Recover the missing component linkage before trying to interpret operation semantics. | 13761 |
| 8 | Coverage Gaps | High | 151 | Common missing pattern: csv, effect-row | csv, effect-row | Resolve the missing root effect link so ability flow can be traced through the client effect chain. | 23 |
| 9 | Coverage Gaps | High | 145 | Partial ability bucket | Partial | Close the missing extracted-client pieces called out in the pattern list before spending time on deeper semantic decoding. | 23 |

## Area Summary

| Area | Items | Critical | High | Peak | Bucket | Summary | Next Step |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Coverage Gaps | 9 | 3 | 6 | 190 | Critical | 3171 abilities remain below `Mapped`; the largest buckets are Partial, StringsOnly. | Push the largest shared missing-pattern buckets first so ability reports move from sparse or string-only states into repeatable mapped states. |
| Conflict Hotspots | 1 | 0 | 0 | 110 | Medium | 0 high-signal conflicts remain after noise suppression; the biggest groups are StringMismatch (EffectName). | Close the highest-signal conflict groups by codifying source precedence instead of treating every disagreement as equally actionable. |
| Unknown Field Hotspots | 0 | 0 | 0 | 0 | Low | No unknown or structural component fields are currently outstanding. | Use the unknown-triage evidence to turn structural layout roles into stable named semantics before widening emulator-side enums. |
| Requirement Semantics | 0 | 0 | 0 | 0 | Low | No requirement rows currently have unresolved field semantics. | Focus on requirement rows with direct ability usage and child links so later linkage work stays evidence-based. |
| Token Gaps | 0 | 0 | 0 | 0 | Low | No unknown or Londo token definitions are currently outstanding. | Prioritize tokens that still depend on Londo or that block natural-language rendering of high-signal component fields. |
| Identity Domain Risks | 0 | 0 | 0 | 0 | Low | No identity domains currently have duplicate-meaning or non-canonical warnings. | Finish the race or career identity collision pass before renaming any repeated client string-entry domains into runtime IDs. |

## Coverage Gaps Top Items

| Rank | Global | Priority | Score | Title | Subject | Summary | Evidence | Action | ExampleAbilityId |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | ---: |
| 1 | 1 | Critical | 190 | Common missing pattern: csv, bin, effect-text, effect-row, components | csv, bin, effect-text, effect-row, components | 1203 abilities still share this extracted-client gap pattern. | Statuses: StringsOnly. Samples: 40, 53, 62, 80, 82, 90, 92, 93, 94, 95, 135, 145. | Resolve the missing root effect link so ability flow can be traced through the client effect chain. | 40 |
| 2 | 2 | Critical | 175 | Common missing pattern: csv, effect-text, effect-row | csv, effect-text, effect-row | 1570 abilities still share this extracted-client gap pattern. | Statuses: Partial. Samples: 38, 71, 251, 252, 253, 254, 255, 256, 257, 259, 261, 262. | Resolve the missing root effect link so ability flow can be traced through the client effect chain. | 38 |
| 3 | 3 | Critical | 175 | Common missing pattern: effect-text, effect-row | effect-text, effect-row | 355 abilities still share this extracted-client gap pattern. | Statuses: Partial. Samples: 609, 612, 632, 634, 654, 655, 675, 676, 691, 693, 739, 760. | Resolve the missing root effect link so ability flow can be traced through the client effect chain. | 609 |
| 4 | 4 | High | 167 | Common missing pattern: csv, effect-text, effect-row, components | csv, effect-text, effect-row, components | 22 abilities still share this extracted-client gap pattern. | Statuses: Partial. Samples: 5979, 14059, 23500, 23639, 23689, 23690, 23691, 23692, 23693, 23696, 24598, 24855. | Resolve the missing root effect link so ability flow can be traced through the client effect chain. | 5979 |
| 5 | 5 | High | 166 | Common missing pattern: csv, bin, effect-row, components | csv, bin, effect-row, components | 6 abilities still share this extracted-client gap pattern. | Statuses: StringsOnly. Samples: 20, 121, 126, 999, 1000, 1003. | Resolve the missing root effect link so ability flow can be traced through the client effect chain. | 20 |
| 6 | 6 | High | 160 | StringsOnly ability bucket | StringsOnly | 1209 abilities still sit in `StringsOnly` coverage instead of a fully mapped state. | Samples: 20, 40, 53, 62, 80, 82, 90, 92, 93, 94, 95, 121. Common missing pieces: csv, bin, effect-row, components, csv, bin, effect-text, effect-row, components. | Find the missing BIN, effect, and component evidence so this stops being a text-only ability shell. | 20 |
| 7 | 7 | High | 154 | Common missing pattern: effect-text, components | effect-text, components | 9 abilities still share this extracted-client gap pattern. | Statuses: Partial. Samples: 13761, 15554, 15558, 22915, 23701, 23702, 23779, 24700, 27951. | Recover the missing component linkage before trying to interpret operation semantics. | 13761 |
| 8 | 8 | High | 151 | Common missing pattern: csv, effect-row | csv, effect-row | 6 abilities still share this extracted-client gap pattern. | Statuses: Partial. Samples: 23, 26, 29, 992, 993, 994. | Resolve the missing root effect link so ability flow can be traced through the client effect chain. | 23 |
| 9 | 9 | High | 145 | Partial ability bucket | Partial | 1962 abilities still sit in `Partial` coverage instead of a fully mapped state. | Samples: 23, 26, 29, 38, 71, 251, 252, 253, 254, 255, 256, 257. Common missing pieces: csv, effect-row, csv, effect-text, effect-row, effect-text, effect-row. | Close the missing extracted-client pieces called out in the pattern list before spending time on deeper semantic decoding. | 23 |

## Conflict Hotspots Top Items

| Rank | Global | Priority | Score | Title | Subject | Summary | Evidence | Action | ExampleAbilityId |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | ---: |
| 1 | 10 | Medium | 110 | StringMismatch (EffectName) | StringMismatch \| EffectName | 307 non-noise conflicts across 307 subjects. | Peak triage: 85. High-signal rows: 0. Sample subjects: Effect:1018, Effect:1019, Effect:1020, Effect:1021, Effect:1022, Effect:1031. | Inspect the claim-level evidence and encode an explicit canonical rule for this disagreement. |  |

## Unknown Field Hotspots Top Items

No rows found.

## Requirement Semantics Top Items

No rows found.

## Token Gaps Top Items

No rows found.

## Identity Domain Risks Top Items

No rows found.

