# Taxi waypoint investigation — 2026-09-06

BUG-010 remains unresolved. No taxi coordinates, realms, enable flags, or runtime
routing were changed. This investigation addresses the five invalid startup rows;
it does not diagnose BUG-059's return to character select after ordinary travel.

## Client evidence

Paths below are relative to `C:/Users/Admin/Downloads/myps`.

- `interface/default/ea_interactionwindow/source/interactionflightmaster.lua:47-112`
  defines the flight UI's zone lookups. **Fell Landing (204) is present**, including
  both realm lists. The other four BUG-010 zones are absent from these lists.
  The earlier blanket description of these five as nonretail taxis is not established.
- `data/gamedata/jumppoints.csv:202` supplies a named generic jump `D-City` for
  zone 62: local X/Y 34020/40956, Z 25554, heading 180.
- `data/gamedata/jumppoints.csv:239` supplies `RvR-E-city` for zone 168:
  local X/Y 25160/44763, Z 13760, heading 0.
- `data/gamedata/jumppoints.csv:30` supplies `FellLanding` for zone 204:
  local X/Y 20000/20000, Z 12000, heading 0.
  These three records do not identify flight arrivals, owning realms, or access
  rules. Their headings have not been converted into the taxi table's units.
  No records with destination-zone field 132 or 139 were found in this table.
- `data/gamedata/cityzonesjumppoints.csv:2` contains only the safe-city mapping
  `162,0,0,109`; it does not supply replacement flight coordinates.
- `interface/interfacecore/maps/zone132/mappoints.xml` defines Talabec Dam's
  bomb targets; `zone139/mappoints.xml` defines High Pass Cemetery's two objectives;
  `zone168/mappoints.xml` defines Altdorf siege objectives; `zone204/mappoints.xml`
  defines portals to Butcher's Pass/The Maw and a transition to Caledor.
  None of these pins is identified as a flight arrival.

## Official capture search

Searched all 1,027 `.gz` files under
`D:/Repos/Shmerrick/WAR-RE-Toolkit/libs/protocolservices/Packet Logs`, expanding
the earlier two-catalog search: 201 subtype-10 `F_INTERACT_RESPONSE` flight menus,
containing 5,206 destination records. None named 62, 132, 139, 168, or 204.
Absence in this corpus does not prove that no flight route ever existed.

Other travel into the affected maps is present:

- `HIGHPASS CEMETERY DOK LVL 40 RR 100 SCENARIO 2.log.txt.gz`,
  `S_PLAYER_INITTED` #379: X/Y 37318/34477, Z 15875, orientation 11.
  Four other Highpass scenario captures repeat this arrival. It is a scenario
  arrival, not an established flight waypoint.
- `SIEGE_PORTAL_ENTERINGinALOTDORF.txt.gz`, `S_PLAYER_INITTED` #106:
  X/Y 33376/193955, Z 13743, orientation 2070. Other siege captures use different
  atlas Y coordinates; copying these directly into world taxi coordinates is unsafe.

The repeatable search is `tools/validation/Read-TaxiCaptureEvidence.ps1`.
It reads compressed captures and writes a CSV to the temporary directory by default.
`MENU` rows are exact matches for the five destination IDs. `INIT` rows include
affected-map arrivals and arrivals following flight requests; `JUMP` rows follow
flight requests. The latter correlations are research candidates, not proof that
the request caused the jump. Packet numbers include both client and server frames.
Packet layouts are cross-referenced in the script to toolkit protocol decoders.

## Delivery limits

The configured Release `zone_taxis` rows were queried and still match migration 53's
five preserved originals. The existing runtime validation continues to reject
their out-of-zone destinations. No new SQL migration was justified by the evidence.
No base dump was edited and no server restart or gameplay retest was performed.
The final scanner compiled and completed the whole corpus; report checks verified
the cited Highpass arrival, zero matching menus, and 56 arrival candidates in the
named flight-master captures.

Closing BUG-010 still needs flight-specific destination and realm evidence, or an
explicit decision to introduce custom routes using separately validated arrival
points. Generic client jumps, scenario starts, and siege portal arrivals do not
establish those missing flight contracts.
