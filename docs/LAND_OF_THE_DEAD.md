# Land of the Dead (1.4.8 Restoration Target)

How the Tomb Kings expedition is won, who may travel there, and what the client draws from the
server's tracker packet. Written 2026-09-05 from the official captures and the extracted client;
supersedes the "working as designed" note that previously stood in
`docs/INTERNAL_BUG_TRACKER.md` for the flight master.

Zone **191**, *Necropolis of Zandri*, region 9, pairing **100**.

## The resource race, as captured

The expedition is a **realm resource quest** (RRQ). Both realms accumulate points; the first to
the threshold takes the expedition. The server's `F_RRQ` (`0x74`) packet carries the whole state,
and four official captures pin every field down.

| Field | Value | Evidence |
|---|---|---|
| Threshold | 500 | every capture |
| Pause after a win | 30 minutes | `PvE_Landofdead_SHAMY40RR95` #21916 |
| Points per T4 battlefront lock | 100 (unconfirmed) | not visible in capture; existing server default |

The decisive sequence is in `PvE_Landofdead_SHAMY40RR95`, which happens to contain a live win:

```
#21808   timer=0    realm=2   Order 448/500   Destruction 256/500
#21916   timer=30   realm=1   Order   0/500   Destruction 256/500
#22058   timer=29   realm=1   Order   0/500   Destruction 256/500
```

Three things follow, and all three contradicted the previous implementation:

1. **Only the winner's score is spent.** Order drops 448 → 0; Destruction keeps its 256.
2. **The 30-minute timer is a pause on the race, not an access window.** During it neither
   realm's score moves — the Chaos Wastes capture holds Order at 431 for the whole 29 → 17 minute
   countdown — and when it expires the race resumes with both totals intact. The Inevitable City
   capture shows exactly that: timer 0, realm 2, Order climbing 26 → 30 → 42.
3. **The holder keeps the expedition after the pause ends,** until the other realm wins. The
   clincher is that the Destruction player in `PvE_Landofdead_SHAMY40RR95` quests inside zone 191
   for 21,000 packets while the header reads timer 0, realm 2.

The server previously required the tracker to be *Paused* for a realm to have access, which
limited Land of the Dead to 30 minutes per win, and cleared `OwningRealm` on every boot and every
unpause. That is BUG-068.

## The tracker packet

`LotdService.BuildTrackerPacket` matches the captured layout byte for byte. Payload, after the
3-byte frame header:

| Offset | Bytes | Meaning |
|---|---|---|
| 0 | 1 | tracker count (1) |
| 1 | 1 | tracker id (1) |
| 2-5 | 4 | header value (4) |
| 6 | 1 | display type — 2 is `ERRQDISPLAY_TOMB_KINGS` |
| 7 | 1 | 0 |
| 8 | 1 | minutes until the race unpauses, 0 when running |
| 9 | 1 | realm holding the expedition; 0 before anyone wins |
| 10 | 1 | 0 |
| 11 | 1 | 3 |
| 12-18 | 7 | zero fill |
| 19-29 | 11 | Order block: realm, 0, 1, threshold u32, score u32 |
| 30-40 | 11 | Destruction block, same shape |

Worked example, `2013-09-29 Chaos Wastes` #72 (`00 29 74` then 41 payload bytes):

```
01 01 00 00 00 04 02 00 1D 02 00 03 00 00 00 00 00 00 00
01 00 01 00 00 01 F4 00 00 01 AF
02 00 01 00 00 01 F4 00 00 00 00
```

`1D` = 29 minutes remaining, `02` = Destruction holds it, Order 431/500, Destruction 0/500.

## Where the client draws it

Client mod `EASystem_RRQ` (`interface/default/easystem_rrq/`) parses `F_RRQ` into one global
table, `RRQProgressBar.RealmResourceQuestData`, filled from `GetRRQData()`. Three mods render
from that table, and **none of them consults the player's zone**:

- `ea_worldmapwindow/source/worldmapwindow.lua` — the bars on the world map.
  `EA_Window_WorldMap.ShouldShowRRQ` returns true on the world map view, on *any* pairing map, or
  on zone 191 and its children.
- `EA_Window_RRQTracker`, in the same file — the HUD overlay. Default-hidden below
  `GameData.LandOfTheDead.MinAccessLevel`, shown automatically at that rank.
- `ea_interactionwindow/source/interactionflightmaster.lua` — the bars inside the flight window,
  shown whenever the Tomb Kings map is selected.

Every one of them first checks `RRQProgressBar.GetFirstQuestDataOfType(ERRQDISPLAY_TOMB_KINGS)`
and does nothing when the client has no RRQ data. So a server that withholds `F_RRQ` produces
exactly the reported symptom: no bars anywhere, no HUD element, nothing on the map.

The server used to send the packet only to players standing in zone 191, because it was sent
together with a zone-activation packet that makes the client show the "Necropolis of Zandri"
title card over whatever zone the player is really in. Those two are now separated: the title
card stays scoped to zone 191, the tracker packet goes to everyone. Retail sent `F_RRQ` 13 times
during a Chaos Wastes RvR session, 48 times during an Inevitable City siege and 29 times in
Caledor, the first inside the login burst each time. That is BUG-066.

## Travel

The client's own statement of the rules is `TOOLTIP_TRAVEL_WINDOW_LAND_OF_DEAD_REQUIREMENTS` in
`data/strings/english/default.txt`:

> Locked for one or more reasons:
> - You must be at least rank `<<1>>`.
> - Your realm currently does not have an active expedition to the Land of the Dead.
> - You have not yet visited a city Flight Master to learn about this location.
> - A massive war is currently underway in your realm's expedition camp. Airships cannot safely
>   land there at the moment. Check back in a few minutes.

Two server-side faults kept this from ever working.

**Pairing.** `zone_infos.Pairing` for zone 191 was 4. `WorldServer` writes that value straight
into the flight packet, and `EA_InteractionFlightMasterWindow.GetNewDataAndSort` keeps only
destinations in `1..NUM_PAIRINGS` (3) or `ExpansionMapRegion.FIRST..LAST`. 4 is in neither, so
the row was discarded before it reached the map — travel was impossible even for a realm that
held the expedition. The capture sends **100**, and the client corroborates that `FIRST` is 100
independently: `pairingview.lua:249` indexes `LABEL_EXPANSION_MAP_REGION_100 + index -
ExpansionMapRegion.FIRST`, and `worldview.lua:14` names the button
`EA_Window_WorldMapWorldViewPairingButton100`. Migration 37 sets pairing 100 and the captured
price of 3000. That is BUG-067.

Migration 37 alone was not enough. `ZoneService.NormalizeZoneInfo` force-sets zone 191's pairing
to `Pairing.PAIRING_LAND_OF_THE_DEAD` at every boot, and that enum member was also **4**, so the
restored value was overwritten before the first flight master was ever opened — visible in the
log as `Zone_Info Normalized zone 191 pairing from 100 to 4`. The enum is now 100, so the guard
and the data agree and the guard keeps the value correct rather than fighting it.

**Listing.** The destination used to be omitted from the flight list when the realm could not
use it. The client is built the other way round: `ZoneNumbersLookup` hard-codes zone 191 for both
realms, `ShowDefaultFrame` disables every button and re-enables only what the server lists, and
`OnMouseOverFlightMapPoint` has a dedicated zone-191 branch that prints the requirements above.
Omitting the row produced a blank Tomb Kings map with no explanation. `WorldMgr.GetTaxis` now
always lists it and `Creature.SendFlightInfo` writes the availability byte
(`LotdService.IsTaxiAvailable`); `F_FLIGHT` re-checks before moving anyone, so a client that
requests a disabled destination is refused.

The flight packet's record layout, from `MECHANIC_orderflymaster_NecropoleOFZandri(LoD)` #9 —
`0x0A`, count `0x1C` = 28, then 28 records of `[id:2][pairing:1][price:2][zone:2][flag:1]`. Its
last record is `00 3D 64 0B B8 00 BF 01`: id 61, pairing 100, price 3000, zone 191.

**The trailing byte is not the availability flag.** A sweep of the whole capture set found about
100 zone-191 flight records, from Order and Destruction players across many sessions, and every
one is byte-identical — `00 44 64 0B B8 00 BF 01`, differing only in the id (61, 62 or 68). A
value that never varies cannot be what greys the destination out, so whatever the client engine
uses to disable zone 191 is not carried here and is still unidentified. `LotdService.IsTaxiAvailable`
writes this byte and `F_FLIGHT` re-checks the same rule before moving anyone, so travel stays
gated server-side either way; but do not treat a 0 here as a supported way to present the
destination as locked, because retail never sent one.

## The warcamp assault

When the expedition changes hands, the winning realm attacks the losing realm's expedition camp,
which starts a public quest on both sides — one realm razes, the other defends. This is the
mechanism behind the fourth lock reason above ("a massive war is currently underway in your
realm's expedition camp; airships cannot safely land there at the moment").

Both quests exist in `pquest_info` for zone 191:

| Entry | Name | Type | Camp |
|---|---|---|---|
| 850 | Assault on Goldbarrow | 2 (Destruction) | Goldbarrow, the Order camp |
| 851 | Assault on Da Dusty Dry | 1 (Order) | Da Dusty Dry, the Destruction camp |

Two captures record them, and the tracker state in each is worth keeping:

- `LAND ON THE DEAD ... PQ ASSAULT ON DA DUSTY DRY - DEFEND THE WARCAMP` — the race is **paused**
  (timer 27 counting down to 18) with **Order** holding the expedition, Order 0/500 and
  Destruction 0/500. Order has just won, and the Destruction player is defending their own camp.
  This is the flip-triggered case.
- `LAND ON THE DEAD ... PQ ASSAULT ON GOLDBARROW - RAZE THE WARCAMP` — the race is **running**
  (timer 0) with **Destruction** holding, Order on 366/500. So the assault is not exclusively a
  flip event; the holding realm can also raid the other camp while the race runs.

**Neither quest can currently run.** Both carry `PQAreaId` 0 and pin 0,0, and neither has a single
`pquest_spawns` row, so `Player.CheckArea` (which only matches painted areas 1-28) can never
attach a player and there is nothing to spawn. They are inert data, not a live mechanic — which
also means they are not the cause of any flight lockout on this server. Restoring them is open
work; see BUG-075.

### An open question about who may travel

The defend capture undercuts the holder-exclusive access rule this server implements. Order holds
the expedition (header realm 1) while a **Destruction** player is inside zone 191 defending Da
Dusty Dry. A quest that exists for the losing realm to defend its own camp implies that realm can
reach the zone at all, and `CanRealmAccessLotd` as written would forbid it.

That is not conclusive — the capture begins mid-session, so the player may have flown in before
the flip and simply stayed, and being present is not the same as being offered the flight. No
capture yet pairs a flight list with a tracker showing the *other* realm as holder: every capture
that lists zone 191 is a Destruction character at a time when Destruction holds it. The access
rule is therefore left as it is rather than loosened on ambiguous evidence. See BUG-076.

## Operating it

Reaching an owned state legitimately needs five Tier 4 battlefront locks. The GM commands stage
it directly:

- `.lotd status` — who holds the expedition, both scores, whether the race is paused.
- `.lotd unlock <realm>` — award the expedition now, in its settled state (holder set, race running). It deliberately does not stage the 30-minute post-win pause, which is the state the client describes as airships being unable to land.
- `.lotd award <realm> <points>` — feed the real threshold path rather than bypassing it.
- `.lotd reset` — return to an unowned race.

With a fresh `lotd_resource_tracker` row nobody holds the expedition, so both realms correctly
see the destination greyed out with the requirements tooltip. `.lotd unlock` is what makes it
flyable.

## Not established

- `PointsPerBattlefrontLock` (100) and which zones count are server defaults, not capture-derived.
- `GameData.LandOfTheDead.MinAccessLevel` is a client-side constant; the server does not enforce
  a rank requirement for the expedition flight, and no capture here fixes its value.
- The fourth tooltip reason — a war in the expedition camp blocking airships — has no server
  implementation and no capture evidence here.
- Whether retail ejected players from zone 191 when their realm lost the expedition. Nothing in
  these captures shows it, and the server does not do it.
