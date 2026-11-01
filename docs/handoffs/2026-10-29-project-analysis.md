# Project analysis and fixes — 2026-10-29

The user asked for a complete analysis of the project, then to "complete all work". This pass
measured the repository fresh rather than trusting earlier handoffs, fixed the defects the analysis
found that could be fixed and verified here, and recorded the rest. **Nothing was tested in the
game client.** Tracker rows BUG-215 – BUG-219.

## Measured state

Taken on `RESTART` at `15df227b` against the Release/x64 binaries and the Release database.

| Area | Result |
|---|---|
| Build | Full `/t:Rebuild` passes with 0 warnings; no `NoWarn` or `#pragma warning disable` anywhere |
| Automated checks | All 18 `Test-*.ps1` pass (~90 s); the auction NRE and NULL-region lines in the runtime log are intended fixtures |
| Size | ≈260k lines of first-party C#; WorldServer 168k in 522 files; `Player.cs` 7.7k lines |
| Surface | 86 packet handlers, 292 GM commands, 36 data services, 16 script classes, migrations 00–13 |
| History | 1,745 commits, 293 since the `1980c873` reset; bursts in 2026-03 and 2026-09 |
| Tracker | 217 rows: 51 open, 31 in progress, **70 fixed but untested in the client**, 58 resolved, 7 closed |

The 70 is the main finding. Ability conformance (migrations 00–10), millisecond cooldowns, item
cooldown persistence and both economy sweeps (BUG-177–213) all sit there. Each further server
change on top makes a failure in the client harder to attribute.

## Fixed

| Bug | What was wrong | Verification |
|---|---|---|
| 215 | `GameClient.OnReceive` discarded a frame split across TCP reads (`BaseClient` resets `ReceiveBufferOffset` first), so the next read parsed the middle of a frame as a header. | New `CheckWorldFrameReassembly`: coalesced frames, cuts mid-header, mid-payload and over three reads, and the oversized-frame disconnect. It fails on the old code (`A frame cut mid-header is retained`) and passes now. |
| 216 | `RpcServer`/`RpcClient` built `TcpServerChannel(name, port)`, which binds every interface, so the BinaryFormatter remoting hub (6800) and callback ports (6001+) were reachable from the LAN. | Full boot: 6800 and 6001–6003 answer on 127.0.0.1 and refuse on 192.168.4.55; 8048 and 10300 still answer on both. All services log `Connected to : 127.0.0.1:6800` and stop by console close; WorldServer logs `Closing the server`. Zero boot errors. |
| 217 | Bot editor API sent `Access-Control-Allow-Origin: *` on write routes. | Removed. Both toolkit consumers call it from C#, which was confirmed by reading `bot-template-viewer` (`wwwroot/app.js` calls only its own `/api/...`; `ConnectionController` proxies) and `warclient/Services/BotEditorApiClient.cs`. Not run. |
| 218 | `TCPManager.HandlePacket` built a debug string and two `packet.ToArray()` copies per inbound packet regardless of level; `GameClient` and the lobby `Client` built one string per read. | Gated by `Log.CanDebug()` / `Log.CanDump()`. Build clean, checks pass. |

The frame layout used by the reassembly is the one the old loop already consumed: 2-byte
big-endian payload length, 8-byte header (sequence, session, unk1, unk2, opcode), payload, 2-byte
trailer. The world RC4 key is static per frame (`RC4Crypto` decrypts each frame with
`key.GetbKey()`), and only complete frames are decrypted, so the retained tail is always raw bytes.
The receive buffer is the pooled 64 KiB `BUF_SIZE`, so only a frame above 65,524 payload bytes
cannot complete; that now disconnects instead of filling the buffer.

## Not changed

- **BUG-219, LobbyServer framing.** Same partial-message defect, plus `PacketIn.DecodeMythicSize`
  returns 0 for any multi-byte length because `Length == Capacity` always holds for a
  `MemoryStream` over a byte array. Login works today, which suggests client lobby messages stay
  under 128 bytes, but nothing has measured that. Measure message sizes in the capture corpus
  before changing the login path.
- **SQL construction** escapes strings with `Database.Escape` instead of using parameters. Every
  player-supplied string found goes through it, and the rest are internal ids, so no injection path
  was found; it is fragile rather than broken.
- **`WorldServer/Configs/LocalDevelopment/*.xml`** carry a placeholder root password for
  127.0.0.1. Harmless for local development; do not reuse it on a reachable database.

## Tracker

`docs/INTERNAL_BUG_TRACKER.md` had about 50 phrasings for five states. Its guidelines now define
the vocabulary and `tools/validation/Get-BugTrackerSummary.ps1` maps every row onto it, failing on
a malformed row or an unplaceable status. Historical cells keep their wording, with four exceptions:

- BUG-177–213 were `In Progress` while both economy handoffs list them as fixed and untested in the
  client; they are now `Fixed; client retest pending`. The leftovers they mention are tracked
  separately (trade distance is BUG-214).
- BUG-172, 174 and 175 had free-text states that start with a canonical term now; no evidence text
  was removed.
- BUG-158 had unescaped pipes inside a code span, which split its row into 8 cells under a
  6-column header.

## Suggested in-client test, in priority order

1. Combat: a cast, a cast-then-channel, cancel and recast, a fractional cooldown, relog with an item
   cooldown running (BUG-171, 174, 175).
2. Economy: trade a partial stack, mail with COD and attachment, list and buy at auction, deposit and
   withdraw from the guild vault, buy back from a vendor (BUG-177–213).
3. Network: move items in bulk and post a long mail body; a frame split across reads should no
   longer drop the session (BUG-215).

Then run `Get-BugTrackerSummary.ps1 -Status Retest` and move each row that held up to `Fixed`.

**Superseded on 2026-10-30 by [`docs/PLAYTEST_CHECKLIST.md`](../PLAYTEST_CHECKLIST.md).** It
places all the remaining retest rows into five play sessions that one person can run. It also
moves 15 rows to `Fixed; verified by automated check`: an existing check covers each one, and each
needed input an unmodified client never sends. That leaves 55 rows awaiting the client.

## Reproduction

```powershell
.\tools\validation\Invoke-AllValidation.ps1 -Build      # build with no warnings + 19 checks
.\tools\validation\Get-BugTrackerSummary.ps1 -Status Retest
```

The loopback/LAN probe was a scratch script that started the four services in ServerLauncher's
order, waited for each port, connected to 6800, 6001–6003, 8048 and 10300 on both addresses, and
closed each console. `netstat` in the agent sandbox showed only part of the listener table, so
reachability, not `netstat`, is the evidence.
