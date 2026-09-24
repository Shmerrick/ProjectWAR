# Millisecond cooldown migration — 2026-09-24

Follows the [repository audit](2026-09-24-repository-audit.md). Migration 11 and
the runtime now retain fractional cooldowns across casting, channels, modifiers,
AI and item scheduling. This closes the 49 fractional-value crosswalk gaps;
it does not close channel conversion, damage mapping or gameplay acceptance.

## Evidence and corrected plan

The extracted 1.4.8 client at `C:/Users/Admin/Downloads/myps` supplies
`data/bin/abilityexport.bin`: cooldown is uint32 LE at record +4 and ability ID
is uint16 LE at +40. Migration 11 cites each of the 49 absolute byte offsets.
`interface/default/easystem_tooltips/source/abilitytooltips.lua:100-108` renders
fractional seconds, including 1.5. These are fractional-second values, not all
values below one second: examples include 500, 1500, 1999, 4500 and 6250 ms.

Changing only the database would leave modifier truncation, byte narrowing in
pet AI and a 400 ms early-cast allowance. The implementation changes those
consumers together. Toolkit `libs/protocolservices/Server Packet Protocol/
F_SET_ABILITY_TIMER.cs` provides the uint32 timer layout at payload +4; it is a
decoded implementation, not an independently inspected official capture.

## Storage and compatibility

Both ability tables gain nullable `CooldownMilliseconds`. NULL falls back to
legacy `Cooldown * 1000`; explicit zero means zero. Migration 11 initializes
the new column once, repairs the 49 client-backed definitions (26 in the legacy
table), retains the old column and guards corrections against prior values.
Future edits must write the millisecond column; changing a populated row's old
seconds column alone does not change its runtime cooldown.

`AICooldown`, creature overrides, authored NPC constructor arguments, `CDcap`
and the `SetCooldown` modifier remain seconds at their input boundary.
`AddCooldownMS` and runtime ability/NPC fields are milliseconds. Arithmetic
clamps instead of wrapping. AI keeps the greater of client delay and existing
server pacing, including suspicious old 1999/1500/1100-second values. Their
retail NPC cadence remains unestablished; this migration does not invent it.

Individual ability and item-group timers enforce the full duration. Only the
global cooldown retains the existing 400 ms grace. Outgoing ability timers now
match the enforced no-reduction/cap floors; expired resends send zero.

Correction from the subsequent inventory review: item packet fields and the
in-memory `CharacterItem.NextAllowedUseTime` use seconds, rounded upward and
packet-saturated to ushort. That property has no `DataElement` attribute:
it is not persisted or restored after relog. The earlier claim that relog merely
adds up to 999 ms was incorrect. The toolkit's item cooldown stub does not
establish retail units; exact item wire fidelity and relog persistence remain
open. No character schema or base dump was changed.

The follow-up [inventory repair](2026-09-24-item-cooldown-reliability.md) updates
every duplicate item, bounds packet counts and tests transport failure cleanup.

## Verification and acceptance

- Release/x64 rebuild: zero warnings, zero errors.
- Runtime regressions: nullable fallback, explicit zero, saturation, modifier
  arithmetic, AI delay and clone isolation, timer payloads/floors, expiry,
  global versus individual grace, silent item scheduling and seconds rounding.
- Both ORM loaders and ability alignment checks pass against Release world DB.
- `Test-AbilityCooldownData.ps1` checks all 4,221/8,416 table rows and independently
  reads the raw client bytes for all fractional corrections.
- Migration 11 applied twice to the configured Release database; second-pass
  checksums unchanged: abilities `3816904373`, mythic_src_abilities `4235923952`.
- Fresh Matrix crosswalk: 5,864 cooldown agreements; zero differences, empty
  values or unrepresentable cooldowns.

Restart the server before testing. No in-client session was performed. Retest
fractional casts, cooldown modifiers, repeated attempts near expiry, channels,
pet cadence and items across relog. Existing portal/dungeon acceptance and the
127 missing channels remain open.
