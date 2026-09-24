# Ability work resumed — September 20

Delivery follow-up: [September 24 fresh audit](2026-09-24-repository-audit.md)
revalidates these changes, repairs callback ownership and records the current
commit delivery. "Uncommitted" below describes this checkpoint's historical state.

Continues [September 15](2026-09-15-ability-conformance.md). Work remains uncommitted
on RESTART. The existing client matrix, ability code and migrations 00–10 were
already in the working tree; they were preserved. No SQL was added or applied in
this pass, and no base dumps were changed.

## Repair

`NewChannelHandler.Update` still converted a zero range to a 25-foot check while
cast-start and cast-time checks accepted targeted zero-range abilities without an
upper range limit. The tick now checks range only when it is nonzero. Existing
nonzero range tolerances are unchanged; no new tolerance is inferred here.

The source contract is migration 09's analysis of client
`C:/Users/Admin/Downloads/myps/data/bin/abilityexport.bin` TargetType/range and
official casts decoded by `tools/captures/extract_use_ability_targets.awk`, documented
in `Database/09_conform_ability_targeting_and_ranges_to_client.sql:9-26`.
This repair extends that established contract through a channel's lifetime.

## Verification

- Full solution Release/x64 build passed before and after the repair, with no
  reported warnings or errors.
- `Test-AbilityLoaders.ps1` passed against the configured Release database:
  8,416 mythic and 4,221 legacy abilities agree through both ORM binders, including
  nullable TargetType and the previously added timing columns.
- `Test-AbilityAlignment.ps1` passed: 4,221 shared entries agree on identity and
  mechanics; 6,012 names and 8,372 EffectIDs match the imported client records.
- A new actual channel-tick fixture failed against the old binary when its
  100-foot-away target triggered the erroneous cancellation. It passes after the
  repair for range zero and range 150. The complete runtime suite passed again.
  Expected missing/corrupt map diagnostics come from deliberate test fixtures.
- A fresh `ClientDataMatrix crosswalk abilities` read the extracted client BINs
  and database. It still reports 127 client channels missing in our implementation,
  49 subsecond cooldown cases, 574 fractional-foot ranges and five channels lacking
  a client component duration. Reports were written to
  `%TEMP%/ProjectWAR-resume-2026-09-20/crosswalk/`; regenerate rather than depend on
  temporary files. The preceding handoff's implementation queue remains applicable.

## Retest state

At inspection, no emulator services were running and no TCP listener occupied
port 8000. The former MortalAtlasHost collision was therefore not present at that
time. No scheduled task or other project's service was modified. This does not
establish whether the conflict can recur after a reboot.

No server stack was started, no game login was performed, and the Matrix GUI was
not visually inspected. Start through ServerLauncher for casting/targeting and
channel retests; the September 15 handoff still identifies those as the first
gameplay gate. No gameplay issue is closed by these automated checks.
