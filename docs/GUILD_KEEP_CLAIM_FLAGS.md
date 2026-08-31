# Guild Keep-Claim Flag Recovery

`Database/04_restore_guild_keep_claim_flags.sql` restores guild-claim flags from original live-server packet captures. It does not derive positions from keep centers, oil placements, or other server data.

## Evidence Method

All 1,027 compressed captures under `WAR-RE-Toolkit/libs/protocolservices/Packet Logs` were streamed and inspected for `F_CREATE_STATIC`. Candidate packets had the live objective display ID (`3442`) and a trailing object name matching `keep_infos.Name`. The packet fields directly supply heading, Z, world X, and world Y. Repeated captures produced byte-identical coordinates for the same keep.

Examples include:

- `REIKLAND ... TAKING ORDER KEEP WITH LOCK` — Morr's Repose and Wilhelm's Fist
- `CALEDOR ... TAKING ORDER KEEP WITH LOCK` — Hatred's Way and Wrath's Resolve
- `PRAAG ... TAKING KEEP WITH LOCK` — Southern Garrison and Garrison of Skulls
- `2013-09-29-ZEALOT40RR100_PvE_T3Avelorn_noPQ` — Ghrond's Sacristy
- `PvE_T3CHAOS_TALABECLAND_part2_ZEAL40RR100` — Passwatch Castle
- `2013-09-30-ZEALOT40RR100_PvE_T3Saphery_noPQ` — Well of Qhaysh

The Reikland and Caledor take captures also contain `F_INTERACT` packets targeting the recovered Morr's Repose and Wrath's Resolve object IDs, confirming that these named statics are interactive keep flags rather than map markers.

## Coverage

The migration restores 21 of the 24 active non-fortress keeps. These entries use IDs `60005` through `60030`, keyed by keep ID where evidence exists, and set `KeepSpawn = 1` so they are excluded from ordinary battlefield-objective spawning.

Gnol Baraz (keep 3), Thickmuck Pit (keep 4), and Stoneclaw Castle (keep 14) remain unmapped. The capture corpus contains no named flag packet near those keeps. Tier 1 keeps are intentionally omitted because the current campaign does not enable guild claiming there.

Do not fill the remaining rows by copying keep X/Y or adding a fixed Z offset: observed floor offsets vary substantially between keep layouts. Add a row only when a named packet, client asset, or another authoritative source provides all four position fields.
