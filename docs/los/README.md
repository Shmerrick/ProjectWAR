# LOS Work README

This folder tracks the server-side line-of-sight (`LOS`) and occlusion reverse-engineering work for `ProjectWAR`.

## Current State

- Runtime LOS is implemented by `WarZone64.dll`.
- Runtime data is loaded from `bin/Release/los/*.bin`.
- Those `.bin` files use the custom `OCC` container format read by `WarZone/WarZone/ZoneManager.cpp`.
- `LosBuilder` can now:
  - generate native LOS from extracted WAR assets
  - inspect shipped or generated `OCC` files
  - compare two `OCC` files directly

## Key Files

- Runtime loader:
  - `WarZone/WarZone/ZoneManager.cpp`
  - `WorldServer/World/Map/Occlusion.cs`
- Native generator:
  - `LosBuilder/Generation/LosGenerator.cs`
  - `LosBuilder/Generation/OccWriter.cs`
- Reverse-engineering helpers:
  - `LosBuilder/Generation/OccReader.cs`
  - `LosBuilder/Generation/OccInspector.cs`
  - `LosBuilder/Generation/OccModels.cs`
- Notes:
  - `docs/los/occ-re-notes.md`

## Useful Commands

Build `LosBuilder`:

```powershell
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe LosBuilder\LosBuilder.csproj /p:Configuration=Release /p:Platform=x64
```

Inspect a shipped LOS file:

```powershell
bin\Release\LosBuilder.exe inspect --input-bin bin\Release\los\280.bin
```

Compare shipped vs native output:

```powershell
bin\Release\LosBuilder.exe compare --left-bin bin\Release\los\280.bin --right-bin C:\temp\los\280.bin
```

Generate one zone natively:

```powershell
bin\Release\LosBuilder.exe generate --input-root C:\Users\Admin\Downloads\myps --output-root C:\temp\los --zone 280
```

## What Matters

The shipped `los/*.bin` files are the golden master.

The four main structural parity gaps (region offset scaling, terrain orientation, holemap serialization, collision geometry selection) are all resolved. Zone 280 triangle hash matches shipped exactly.

Remaining gaps:

- vertex position precision (~16-unit max Y error from NIF world-matrix accumulation)
- water chunk generation (zone 280 has no `water.xml` in current extracted data)
- multi-zone coverage (only zone 280 has all required source files in current extraction)

See `docs/los/occ-re-notes.md` for full format documentation and per-chunk details.
