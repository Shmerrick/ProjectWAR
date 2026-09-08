# OCC LOS Reverse Engineering Notes

These notes track what the shipped `los/*.bin` files contain and how they compare to the current native `LosBuilder` output.

## Figleaf dependency scope (September 8, 2026)

[Canonical toolkit findings](../../../WAR-RE-Toolkit/RE_FINDINGS/world/figleaf_status.md)
record Figleaf source identity, partial decoding, and remaining gaps. The implemented
consumer is [FigleafMetadataReader](../../LosBuilder/Generation/FigleafMetadataReader.cs):
it reads strings, fixture references, and regions. [LosGenerator](../../LosBuilder/Generation/LosGenerator.cs)
combines these with separate zone and mesh inputs. The comparisons below validate their
recorded cases; they do not establish a complete Figleaf schema or every zone's parity.
The September documentation review did not rerun LOS generation.

## Format Summary

The shipped LOS files use the same custom `OCC` container that `WarZone` reads in `ZoneManager.cpp`.

- Header:
  - bytes `OCC`
  - version byte
  - header-size byte
- Chunk types:
  - `7` = region
  - `4` = terrain
  - `5` = collision
  - `8` = water

### Region Chunk

The region chunk contains:

- `RegionId`
- `ZoneCount`
- for each zone entry:
  - `ZoneId`
  - `OffsetX`
  - `OffsetY`
  - `ZoneNifCount`
  - `ZoneFixtureCount`

Current files appear to contain one zone entry per file.

### Terrain Chunk

The terrain chunk contains:

- `RegionId`
- `ZoneId`
- `Width`
- `Height`
- `HoleWidth`
- `HoleHeight`
- `ushort[Width * Height]` height samples
- `byte[HoleWidth * HoleHeight]` hole samples

Two terrain layouts exist in shipped data:

- `256x256` holemap present
- `0x0` holemap absent

### Collision and Water Chunks

Collision and water chunks share the same structure:

- `RegionId`
- `ZoneId`
- `VertexCount`
- `Vector3[VertexCount]`
- `FixtureCount`
- `TriangleCount`
- `IndexSize`
- `Triangle[TriangleCount]`

Each triangle stores:

- `i0`
- `i1`
- `i2`
- `uniqueID`

`uniqueID` layout matches runtime expectations:

- high 8 bits = `SurfaceType`
- low 24 bits = `FixtureId`

The triangle stream is grouped by `uniqueID`, and `WarZone` depends on that grouping when it builds fixture buckets at load time.

## Shipped Folder Survey

Survey of the current shipped runtime folder `bin/Release/los`:

- `249` files total
- `249` valid `OCC` files
- `129` files include a non-empty water chunk
- holemap histogram:
  - `124` files use `256x256`
  - `99` files use `0x0`

Collision density varies widely:

- smallest collision chunks include zones with `0` collision triangles
- largest sampled zone:
  - zone `201`
  - `2,749,332` collision triangles
  - `1,668,631` vertices
  - `11,540` unique fixtures

## Zone 280 Golden-Master Comparison

Shipped `280.bin` versus current native `LosBuilder` output for zone `280`:

- File size:
  - shipped: `2,284,132`
  - native: `6,225,992`
- Region offsets:
  - shipped: `753664,1277952`
  - native: `376832,638976`
- Terrain:
  - same `1024x1024` dimensions
  - same min/max height range: `4820..18996`
  - shipped has `0x0` holemap
  - native writes `256x256` holemap
  - terrain hashes do not match
- Collision:
  - shipped: `6,074` vertices, `7,115` triangles
  - native: `135,815` vertices, `152,088` triangles
  - both declare `77` fixtures and group triangles by `uniqueID`
- Water:
  - shipped has a water chunk with `6` vertices and `2` triangles
  - native currently emits no water chunk for zone `280`

### Zone 280 Bucket Pattern

Zone `280` shipped collision buckets are extremely uneven:

- `77` fixture buckets total
- `75` buckets use `33` triangles
- `1` bucket uses `1,788` triangles
- `1` bucket uses `2,852` triangles

The two large buckets are fixture ids `40` and `41`.

This is strong evidence that shipped LOS is not a naive dump of every raw fixture mesh triangle. It is either:

- built from alternate collision meshes, or
- passed through a simplification stage that the current native generator does not yet reproduce.

## Zone 201 Water Findings

Zone `201` is a useful water reference:

- `122` water triangles
- `24` water fixture buckets
- `4` water surface types

Observed water `uniqueID` examples:

- `218169344` = surface `13`, fixture `65536`
- `167837697` = surface `10`, fixture `65537`

This suggests water uses synthetic fixture ids starting around `65536`, with the surface type carried in the high byte exactly like collision fixtures.

## Resolved Parity Gaps (as of 2026-03-11)

All four structural parity gaps are now fixed:

### 1. Region Offsets (FigleafMetadataReader.cs)

- Root cause: figleaf.db stores zone cell offsets in 8192-unit cells, not 4096-unit cells.
- Fix: changed `zoneXOff << 12` to `zoneXOff << 13` (and same for Y).
- Verification: zone `280` gives `92 << 13 = 753664` which matches shipped.

### 2. Terrain Height Orientation (TerrainRasterReader.cs)

- Root cause: generator applied an X-axis flip (`sourceX = width - x - 1`) when reading terrain.pcx.
- Fix: removed the flip; column order is direct (`sourceX = x`).
- Verification: terrain height hash now matches `0x3E95E510C190B6D2`.

### 3. Holemap Behavior (TerrainRasterReader.cs)

- Root cause: when `holemap.pcx` is absent, generator emitted a `256×256` all-ones fallback.
  Shipped uses `0×0` (no holemap data at all) when the file is absent.
- Fix: set `holeWidth = 0, holeHeight = 0` when holemap.pcx is missing.
- Verification: holemap dimensions now match shipped (`0×0`), hole hash matches.

### 4. Collision Density (TriangleWalker.cs + LosGenerator.cs)

- Root cause: generator loaded all NIF geometry (visual + collision). WAR fixture NIFs contain
  embedded collision-proxy geometry in invisible NiTriShape nodes (flag bit 0 = 1). The shipped
  LOS tool uses only those invisible nodes. For trees the proxy is a 33-triangle bounding
  cylinder; for towers it is a simplified hull.
- Fix: added `GetCollisionTrianglesFromNode()` which filters to invisible-flagged nodes.
  Generator now loads collision geometry only; falls back to all geometry only if no invisible
  nodes exist.
- Verification: zone `280` collision counts now match shipped exactly (6074 vertices, 7115
  triangles, triangle hash `0xBAAEA5D12555DEB1`).

### Confirmed via MYP Extraction

- `he_el_tree01_var01_collision.nif` and `he_tower01_collision.nif` referenced by figleaf
  do NOT exist in any WAR MYP archive (searched ART.myp, ART2.myp, ART3.myp exhaustively).
- The collision geometry is embedded inside the visual NIF as invisible sub-nodes.
- This is confirmed: `fi.0.0.he_el_tree01_var01.nif` has `col=33` invisible triangles and
  `all=1954` total; `fi.0.0.he_tower01.nif` has `col=2852` invisible and `all=10893` total.

## Zone 280 Current Compare State (2026-03-11)

- Region: all fields match ✓
- Terrain: size, holemap, min/max, height hash, hole hash all match ✓
- Collision: vertex count, triangle count, all bucket counts, triangle hash all match ✓
- Vertex hash: minor floating-point difference due to NIF world-matrix accumulation (~16 units max Y)
- Water: absent in native (zone `280` has no `water.xml` in extracted source data)
- File size: 2284132 (shipped) vs 2283996 (native) — 136-byte gap = exactly the water chunk

## Remaining Open Items

1. **Vertex position precision** — ~16-unit max Y error from NIF transform accumulation.
   The triangle hash matches so topology is correct. This is a minor positional bias.
2. **Water generation for zone 280** — source data limitation (no `water.xml`).
   Other zones (e.g. `201`) do have `water.xml` but lack PCX rasters.
3. **Multi-zone coverage** — only zone `280` has all required source files in current extraction.
   More zone data needed to verify broad coverage.

## Practical Use

The shipped `los/*.bin` files are good enough to use as a golden master.

The recommended workflow is:

1. Inspect shipped bins with `LosBuilder inspect`.
2. Generate a native candidate for the same zone.
3. Compare shipped vs native with `LosBuilder compare`.
4. Fix native generation until chunk metadata, offsets, terrain layout, water layout, and collision density converge.
