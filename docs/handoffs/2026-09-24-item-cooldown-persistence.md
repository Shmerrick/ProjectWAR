# Character-owned item cooldown persistence — 2026-09-24

Migration 12 adds `war_characters.character_item_cooldowns`, keyed by character
and spell/shared-group identity. Absolute deadlines retain milliseconds.
Consuming, deleting or moving the last inventory copy cannot delete this state.
Existing character and inventory rows are unchanged; old sessions had no durable
deadlines to backfill.

Evidence for this engineering repair is the server's item cooldown setters,
`CharacterItem.NextAllowedUseTime` (unmapped), `Player.Load` inventory ordering,
and `ObjectDatabase`'s delayed write queue. It does not establish new 1.4.8 values
or retail persistence semantics. Toolkit `F_UPDATE_ITEM_COOLDOWN.cs` remains a
stub; item packet units are unchanged and retail wire fidelity stays open.

One synchronous indexed upsert runs on each item cooldown change. This avoids
relogging before the ORM flushes a queued write; no database work occurs per tick.
A database outage causes the persistence operation to fail visibly, not silently
claim success. Database latency therefore affects item-use handling. This is not
a transaction spanning item consumption and ability effects.

Login reads accepted keys using the composite primary key (bounded key space),
removes expired deadlines, and restores timers before inventory loading. Inventory
display deadlines are rebuilt in seconds from those exact runtime timers; stale
cached display state is cleared. Spell and group keys cannot collide. Character
deletion queues removal of its cooldown rows through the normal ORM lifecycle.
Bots do not persist these records. Ordinary non-item ability cooldowns are outside
this change, except resets/changes to already tracked item spell IDs.

## Verification

- Migration 12 applied twice to the configured Release character database; table
  verified as InnoDB, unsigned composite primary key and signed BIGINT deadline.
- Release/x64 build: zero warnings/errors; runtime regression suite passes.
- `Test-ItemCooldownPersistence.ps1` uses the actual Release DB and actual server
  setters/loaders. It checks exact deadline round trips, immediate fresh-interface
  reload, empty inventory, independent spell/group keys, resets, expired-row cleanup,
  invalid keys/deadlines and cached inventory display reconstruction.
- The DB test uses character ID 4294967295 only after verifying no character or
  cooldown rows own it, obtains an advisory lock to serialize test runs, and deletes
  its fixture rows in `finally`. It is not SELECT-only. It starts no game services.

Restart with migrations 00–12 applied. In-client use/relog/reacquisition, shared
group UI and restart acceptance remain pending. No live gameplay test is claimed.
