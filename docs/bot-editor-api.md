# Bot Editor API

`WorldServer` now exposes a local HTTP JSON API for both base-template editing and per-bot gear overrides from external tools.

This surface is actively consumed by `WAR-RE-Toolkit` through both the embedded hub editor and the standalone `Bot Template Armory` viewer.

## Purpose

The bot loadout stack has three live layers:

- base gear templates in `BotLoadoutManager`
- per-bot persisted gear overrides in `bot_gear_overrides`
- normal bot loadout application through `BotManager`

The HTTP API sits above those layers so external tools can:

- list bot characters
- inspect one bot's base template, bot override set, effective bot result, and currently equipped gear
- search slot-valid items for one bot or one career-template variant
- patch a base template slot
- apply or clear a per-bot override slot
- reapply the resulting loadout to already loaded bots

## Server Configuration

`WorldServer` starts the bot editor API at process startup when enabled in `Configs/World.xml`.

Config fields:

- `EnableBotEditorAPI` default: `true`
- `BotEditorAPIAddress` default: `127.0.0.1`
- `BotEditorAPIPort` default: `51933`

The default binding is intentionally local-only.

## Persistence Model

Per-bot override edits are stored in `war_characters.bot_gear_overrides`.

Schema:

- `CharacterId`
- `SlotId`
- `ItemEntry`

The table is created by `Database/update_011_bot_gear_overrides.sql`.

Base templates are still held in `BotLoadoutManager` and are edited in-memory through the patch routes below.

Resolution order for a bot sheet:

1. Determine the base template for `career + tier + role`
2. Load any per-bot overrides
3. Validate each override against career, race, skills, rank, renown, slot legality, and unique-equipped restrictions
4. Merge valid overrides into the effective loadout
5. Reapply the effective loadout to a loaded bot when requested

## Routes

Base URL:

```text
http://127.0.0.1:51933/api/bot-editor/
```

### Bot-sheet routes

- `GET health`
- `GET bots`
- `GET bots/{characterId}`
- `GET bots/{characterId}/items?q={query}&slotId={slotId}&limit={limit}`
- `PUT bots/{characterId}/gear`
- `DELETE bots/{characterId}/gear?reapply=true`
- `PATCH bots/{characterId}/template`

### Career-template routes

- `GET career-templates`
- `GET career-templates/{careerLine}/{tier}/{variantIndex}/items?q={query}&slotId={slotId}&limit={limit}`
- `PATCH career-templates/{careerLine}/{tier}/{variantIndex}`

The API sends **no CORS headers** (removed 2026-10-29). It used to send
`Access-Control-Allow-Origin: *`, which let any web page open on the machine drive the
PUT/PATCH/DELETE routes. Its consumers do not need CORS: `bot-template-viewer` proxies through its
own ASP.NET backend (`Controllers/ConnectionController.cs` -> `BotEditorApiClient`), and `warclient`
uses `BotEditorApiClient` directly. A browser page that calls this API cross-origin will now be
refused; route it through a backend instead.

## Response Shapes

### `GET bots`

Bot summaries contain:

- `characterId`
- `name`
- `groupPrefix`
- `loaded`
- `role`
- `careerLine`
- `careerName`
- `realm`
- `realmName`
- `level`
- `renownRank`
- `zoneId`
- `hasGearOverrides`

### `GET bots/{characterId}`

A full bot sheet contains:

- `characterId`
- `name`
- `groupPrefix`
- `loaded`
- `role`
- `careerLine`
- `careerName`
- `realm`
- `realmName`
- `level`
- `renownRank`
- `zoneId`
- `templateGear`
- `customGearOverrides`
- `effectiveLoadout`
- `equippedGear`

Each gear list entry contains:

- `slotId`
- `slotName`
- `itemEntry`
- `itemName`
- `modelId`
- `objectLevel`
- `minRank`
- `minRenown`
- `rarity`
- `iconId`
- `primaryDye`
- `secondaryDye`

### `GET career-templates`

Returns a career-template tree grouped as:

- `careerLine`
- `careerName`
- `realm`
- `realmName`
- `tiers[]`
  - `tierName`
  - `variants[]`
    - `variantIndex`
    - `label`
    - `gear[]`

### `PATCH career-templates/{careerLine}/{tier}/{variantIndex}`

Returns the updated template variant:

- `variantIndex`
- `label`
- `gear[]`

## Update Contracts

### `PUT bots/{characterId}/gear`

Accepts:

```json
{
  "replaceOverrides": true,
  "reapply": true,
  "slots": [
    { "slotId": 10, "itemEntry": 2000157 },
    { "slotId": 11, "itemEntry": 0 }
  ]
}
```

Rules:

- `replaceOverrides=true` starts from an empty override set
- `replaceOverrides=false` modifies the existing override set in place
- `itemEntry=0` or `null` removes the override for that slot
- `reapply=true` reapplies the effective bot loadout if the bot is currently loaded

### `PATCH bots/{characterId}/template`

Accepts the same body shape, but patches the base template resolved for that bot's `career + tier + role` instead of the selected bot override set.

### `PATCH career-templates/{careerLine}/{tier}/{variantIndex}`

Accepts the same body shape and patches the explicit template variant directly.

When `reapply=true` on a `T4` or `SC` template patch, the server attempts to reapply that template to matching loaded bots of the same career.

### `DELETE bots/{characterId}/gear`

Clears all custom overrides for that bot.

## Validation Rules

Every override and item-search result is validated with the same bot loadout checks:

- slot must be a managed equipment slot
- item must be allowed in that slot
- item must be usable by the bot's career, race, skills, level, and renown
- unique-equipped items cannot duplicate another unique item already present in the resolved loadout

Invalid overrides are rejected by the API or ignored during effective-loadout assembly.

## Toolkit Integration Status

This API is consumed by `WAR-RE-Toolkit` through:

- `apps/warclient/Services/ServerRPC.cs`
- `apps/warclient/Services/BotEditorApiClient.cs`
- `tools/ToolkitControlCenter/WarToolkitHub/Views/BotEditorView.xaml`
- `apps/bot-template-viewer/MainWindow.xaml`
- `tools/ToolkitControlCenter/program_ui_profiles.json`

Current toolkit-side behavior:

- the embedded `Bot Editor` focuses on per-bot override work
- the standalone `Bot Template Armory` supports the richer paperdoll workflow, filter cascade, and base-template vs bot-result editing split
- runtime usage has moved beyond the original read-only milestone into active gear-template and per-bot editing

Current functional gap:

- tactic-slot editing is not yet exposed
- mastery-template editing is not yet exposed

## Operational Notes

- This API does not create or delete bots; it edits loadout state for existing bots
- Existing bots can be edited even when unloaded because overrides persist in `war_characters`
- Loaded bots can be updated live through `reapply=true`
- The API is separate from the legacy debug-only binary API in `WorldServer/API/Server.cs`

## Verification

The current implementation builds successfully with:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\amd64\MSBuild.exe' WorldServer\WorldServer.csproj /t:Build /p:Configuration=Release /p:Platform=x64
```
