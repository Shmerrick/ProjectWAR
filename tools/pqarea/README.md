# Zone area map generation

`WorldServer` resolves a player's zone area and public-quest area from two 1024x1024 PNG overlays
per zone, `zones/zoneNNN/areasNNN.png` and `zones/zoneNNN/pqareaNNN.png`. `ClientFileMgr` reads a
pixel as

    id = 1 + (R >> 4) + (G >> 4)

and looks it up at `[pinX >> 6, pinY >> 6]`, so one pixel covers 64 zone units. For the area map
the id is matched against `zone_areas.PieceId`; for the PQ map it is matched against
`pquest_info.PQAreaId`, with 29-31 reserved (31 means "no public quest here").

**These are not client assets.** The extracted 1.4.8 client contains no area bitmaps at all -- only
`terrain.pcx`, `shadow.pcx`, `offset.pcx` and `holemap.pcx`. Mythic defined spatial regions
analytically: `zones/zoneNNN/sector.dat` declares forest and skydome regions as `numrects` /
`numcircles` with world coordinates. PQ bounds were almost certainly server-side data, which is
why nothing in the client defines them. The PNGs shipped in `deps/zones` are a previous emulator's
reconstruction, and 180 of 219 zones have none (BUG-041).

## Usage

    # PQ areas, from public quest spawn positions
    ./Build-AreaMaps.ps1 -Spawns spawns_160.csv -Radius 18 -Out pqarea160.png

    # Zone areas, uniform PieceId
    ./Build-AreaMaps.ps1 -Spawns spawns_160.csv -Radius 0 -Out areas160.png -Uniform 1

    # Inspect any map: per-id pixel count, centroid, bounding box
    ./Read-AreaMap.ps1 -Path pqarea160.png -Label 'check'

`Spawns` is a headerless CSV of `PQAreaId,pixelX,pixelY`, produced with:

```sql
SELECT p.PQAreaId, (s.WorldX - (z.OffX*4096))>>6, (s.WorldY - (z.OffY*4096))>>6
  FROM pquest_info p
  JOIN pquest_objectives o ON o.Entry = p.Entry
  JOIN pquest_spawns s     ON s.Objective = o.Guid
  JOIN zone_infos z        ON z.ZoneId = p.ZoneId
 WHERE p.ZoneId = 160 AND p.PQAreaId > 0
   AND s.WorldX >= z.OffX*4096 AND s.WorldY >= z.OffY*4096
   AND (s.WorldX - (z.OffX*4096))>>6 < 1024 AND (s.WorldY - (z.OffY*4096))>>6 < 1024;
```

Quests are drawn largest-footprint-first so smaller, tighter quests win the overlaps.

## Accuracy

The generated boundaries are **an approximation of quest footprints, not recovered geometry**.
Scored against Mount Gunbad's existing painted `pqarea060.png`, a disc fit of this kind reaches
about **57% IoU** at radius 12-16: it covers roughly 82% of the painted area but also paints
outside it. Region centroids agree closely with `pquest_info.PinX/PinY` (1-20 px on most quests),
so the two describe the same places with different edges. Expect a quest to trigger somewhat
early or late at the boundary, and correct a radius per zone if it matters.

Zone 160's `areas160.png` is uniform on purpose: all three of its `zone_areas` rows carry the same
`OrderInfluenceId 129` / `DestroInfluenceId 128` after migration 32, matching client
`interface/interfacecore/maps/zone160/influenceids.csv:2-3`. A single area therefore selects
the same influence tracks everywhere. The displayed sub-area name is coarser, and `Steps of
Ruin` still lacks its own row. The earlier 6/2 values confused chapter row keys with tracks;
see `docs/handoffs/2026-09-05-stabilization.md`.
