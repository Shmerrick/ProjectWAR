# Economy bug sweep, duplication hardening and system-guild GM rank — 2026-10-05

A code-review sweep of every path that moves items or gold between players, followed by two
general duplication protections and a system-guild change. Nothing here is a 1.4.8 fidelity
claim: these are server correctness repairs, found by reading the code, with no retail data
changed. No database migration was needed.

**Status:** builds on Release/x64 with no warnings; `Test-RuntimeRegressions.ps1` and the new
`Test-CrashWindowPersistence.ps1` pass. **Nothing has been tested in the client.** Every tracker
entry below is *In Progress* until it has been.

## Bugs fixed (BUG-177 – BUG-200)

All are recorded in full in `docs/INTERNAL_BUG_TRACKER.md`. In brief:

| Area | Bugs | What was wrong |
|---|---|---|
| Trade | 177, 178, 179 | Any player could complete another's accepted trade; a stack offered twice was duplicated; a full bag destroyed the item. |
| Mail | 180, 181, 182, 200 | An out-of-range COD byte created gold; returned/expired mail lost its attachments and gold; Black Market rewards multiplied per listing; a failed take removed the attachment. |
| Item creation | 183, 189 | N non-stackables created nothing while reporting success; items could be created into equipment and invalid slots. |
| Vendors | 184, 185, 186, 187 | Buyback of count 0 duplicated any sellable item; equipped items could be sold; the Black Market vendor rewarded for free; purchase totals wrapped. |
| Guild vault | 188, 190, 191 | Slots cost whatever the client sent; deposits ignored bought slots, per-vault permissions and index bounds; locks of departed players never released. |
| Loot | 192, 193 | Anyone in range could loot any kill; a loot-roll voter could vote twice, and a full bag lost the won item. |
| Auction | 194 | Cross-realm buyouts, a double-buy race, posting from any slot. |
| Repair | 195 | Any of 1,278 items with a `Craftresult` could be "repaired" into it. |
| Crafting | 196 | Lock leaks disabled crafting until relog; ingredients could be swapped after adding; stability could be gamed. Both apothecary and talisman crafting. |
| Cultivation | 197 | Rare seeds duplicated on retried harvests; a rollback used an item id as a slot number. |
| Talisman window | 198 | Full bags destroyed talismans; slots past the item's count consumed them. |
| Persistence | 199 | A hard crash could keep both sides of a partial-stack or gold transfer. |

## Duplication protections

1. **Crash window (BUG-199).** New item rows and deletions are queued at once, but stack counts
   and gold were written only by the owner's 60s autosave. `Item.Count` and
   `Player.AddMoney`/`RemoveMoney` now queue their rows on change, so both sides of a transfer
   land in one persistence pass. `ForceSave()` never wrote synchronously — it only makes the
   persistence thread's next 5s wake-up a save pass; CLAUDE.md is corrected. Verified against the
   local Release database by `tools/validation/Test-CrashWindowPersistence.ps1`.
2. **Item audit log.** `WorldServer/Managers/ItemAudit.cs` writes one line per player-to-player
   item or gold movement (trade, mail, guild vault, auction, Black Market) to
   `logs/ITEMS_{date}.log`, routed by a final rule in `NLog.config`. Loot, vendors and crafting are
   excluded by design. About 1 MB/day at 50 concurrent players, under the shared archive caps.
   Unique item serials were considered and deferred: they would not cover stackables, the usual
   duplication target.

## System guilds: GM characters hold rank 9

GM characters (account GM level above 1) in Forces of Order / Forces of Destruction now hold rank
9 "System" (every permission bit), so a GM can administer and test the guild. Applied on join
(`Guild.JoinGuild`, covering auto-join and `.guildadd`) and on every login
(`Guild.ApplySystemGuildRank` from `Player.StartInit`); a former GM drops back to Member. The boot
duplicate-leader check in `CharMgr.LoadGuilds` now skips leaderless system guilds, which would
otherwise demote every GM on each restart. See `docs/SYSTEM_GUILDS.md`.

## Known, not changed

- **A persistence row that can never be written blocks its batch.** In the per-row fallback,
  failed inserts and updates are only logged (the row is lost at restart), and a failing delete
  throws, so everything after it in that batch is retried and never saved. Not driven by volume.
  Fixing it touches the shared persistence layer; it should be its own change.
- `CharMgr.CreateItem` calls `ForceSave` on every item, so saves run on the 5s cycle during busy play.
- Trade still has no distance check.

## Suggested in-client test

Trade part of a stack, mail some gold, deposit into and withdraw from the guild vault, buy and
cancel an auction, then read `logs/ITEMS_{date}.log`. Log a GM character into a system guild and
confirm the guild window shows the System rank with every permission editable.
