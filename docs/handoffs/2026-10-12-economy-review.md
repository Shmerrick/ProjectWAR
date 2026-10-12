# Economy review follow-up — 2026-10-12

A second pass over every path that moves items or gold, starting from the untested
[2026-10-05 sweep](2026-10-05-economy-and-duplication.md), which is reviewed and kept. These are
server correctness repairs. Where retail behaviour is cited it comes from the official captures or
the client, named below; no game data was invented.

**Status:** Release/x64 builds with no warnings. `Test-RuntimeRegressions.ps1` passes, with a new
`CheckInventoryTransferGuards` group, and so do `Test-CrashWindowPersistence.ps1`, `Test-DatabaseNulls.ps1`
and `Test-ItemCooldownPersistence.ps1`. Migration 13 is applied to the Release `war_characters` and
verified. **Nothing has been tested in the client.** Tracker rows BUG-201 – BUG-214.

## Fixed

| Bug | What was wrong | Evidence |
|---|---|---|
| 201 | A forged `F_TRANSFER_ITEM` with count 0 copied any item; a partial stack moved onto itself grew to MaxStack. | 1,847 client 0x75 frames in the captures: count is never 0, source never equals destination. |
| 202 | Talisman strings overflowed `VARCHAR(40)`; strict mode failed the save and lost socketed items. | Reproduced `ERROR 1406`; migration 13. |
| 203 | Expired mail with attachments was deleted at boot; mail ids came from `COUNT(*)` without preloading. | Code path; `PreloadAllCharacters` is on in the shipped config. |
| 204 | Removal skipped the crafting bag, so cultivation's rollback took nothing and the harvest repeated. | Code path. |
| 205 | Boxes, conversions, crafting, salvaging, resin and login talisman return consumed their source when the result could not be delivered. | Code path. |
| 206 | Trade delivered a stack shrunk after offering; frames lost the offer on accept and sent 1 instead of slots. | 305 server 0x4B frames; client `tradewindow.lua` secure-trade timer; client trade error strings. |
| 207 | Unbought bank slots accepted items. | Code path. |
| 208 | Mail and vault deposits took equipped, quest and talisman-window items. | Code path. |
| 209 | Gold beyond the uint ceiling was destroyed by saturating `AddMoney`. | Code path. |
| 210 | Guildless players could buy 134 guild-rank vendor items. | Release `vendor_items`. |
| 211 | Banner and tactic edits ignored permissions; respec checked the wrong one. | Client `ea_guildwindow` Lua (lines cited in the tracker). |
| 212 | The auction expiry timer could crash the process on a deleted seller. | `System.Threading.Timer` semantics; code path. |
| 213 | Talisman effects not removed on unequip, double ranged bolster, and five smaller faults. | Code path. |

## Database

`Database/13_widen_talisman_columns.sql` widens `Talismans` in `characters_items`, `auctions` and
`guild_vault_item` to `VARCHAR(255)`. It is safe to re-run. It was run twice against the Release
database, and a 240-character worst case round-trips. The incremental series is now 00–13.

## Known, not changed

- Trade has no distance check (BUG-214): the client has a "too far" string, but no source gives
  the range.
- `SendBackpack` addresses slot 13 rather than the cloak's server slot 27; no ability uses it.
- Still open from the previous handoff: a row that can never be written blocks its persistence
  batch, and `CharMgr.CreateItem` forces a save on every item.

## Suggested in-client test

1. Trade a partial stack, change the offer, and accept immediately: refused with "changed the trade
   too recently". Accept after three seconds: completes. Confirm both windows show the offer
   throughout, including after one side accepts.
2. Socket three talismans into a weapon, relog, and check they persist.
3. Open a currency box and craft a potion with a full backpack: nothing is consumed.
4. Try to move an item into the unbought bank range: refused.
