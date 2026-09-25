# Item cooldown reliability — 2026-09-24

Subsequent work: [migration 12](2026-09-24-item-cooldown-persistence.md) now
persists character-owned deadlines. The unmapped inventory property described
below remains a display cache, not the source of persistence.

Follow-up to the millisecond migration. This is a server consistency repair,
not a claim of newly established 1.4.8 behavior or packet units.

`ItemsInterface.SendItemCooldown` and `SendItemGroupCooldown` previously
deduplicated before setting deadlines, leaving additional copies unchanged.
They also narrowed the notification entry count to a byte while serializing
the full list, and retained scratch entries when a send failed.

Both methods now share one inventory-bounded scan. Every matching copy receives
the same deadline; only outgoing entries are deduplicated. Packets use the
existing 12-byte entry layout, with at most 255 entries per packet. A `finally`
clears reused scratch collections. Zero/negative durations clear immediately.
`CharacterItem.RemainingCooldown` saturates rather than wrapping past ushort.

The review also corrected a prior documentation error: `NextAllowedUseTime`
in `Common/Database/Character/Characters_items.cs` is not ORM-mapped. These
deadlines are in memory only; no database-backed relog guarantee was tested or
implemented. BUG-175 remains open for that gap and wire fidelity. External
authority is still absent: toolkit `libs/protocolservices/Server Packet Protocol/
F_UPDATE_ITEM_COOLDOWN.cs` is a stub. Existing packet units remain unchanged.

README fresh-install instructions now require migrations 00–11, correcting the
contradictory claim that no incremental updates exist. No database schema or
data changes were needed for this batch; migration 11 only received a comment
correction.

Validation: Release/x64 rebuild, zero warnings/errors. Runtime regression suite
passes, including 260 distinct entries plus a duplicate, 255/5 packet batching,
exact packet lengths and rounded seconds, unrelated-item isolation, group reset,
injected send failure followed by another notification, and large deadline
saturation. Fixtures call the actual server methods with captured packets;
they do not open sockets, write character records or establish client acceptance.
