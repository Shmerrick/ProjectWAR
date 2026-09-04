# Cross-Repo Map

**This file is the canonical map of where ProjectWAR's evidence lives.** WAR-RE-Toolkit's
`docs/CROSS_REPO.md` is a short pointer back to this one; keep the path table here correct and
that one thin.

ProjectWAR is a restoration project, not a greenfield server. Almost every change needs an
authority outside this repository: the 1.4.8 client, an official packet capture, or a decoded
finding. Those live in two other places, and the point of this document is that you should not
have to rediscover where.

## The two repositories

| | ProjectWAR | WAR-RE-Toolkit |
|---|---|---|
| Path | `D:\Repos\Shmerrick\ProjectWAR` | `D:\Repos\Shmerrick\WAR-RE-Toolkit` |
| Role | The server emulator | RE tooling, findings, and the private client component |
| Branch | `RESTART` (never `master` — see `CLAUDE.md`) | default |
| Solution | `ProjectWAR.sln`, MSBuild, .NET Framework 4.8, **x64 only** | `WAR-RE-Toolkit.slnx`, .NET 10 + net48 mix |
| Build | `MSBuild.exe ProjectWAR.sln /p:Platform=x64 /p:Configuration=Release` | `dotnet build tools/ToolkitControlCenter/WarToolkitHub/WarToolkitHub.csproj` |
| Agent rules | `AGENTS.md` (P10-derived; binding for server code) | `AGENTS.md` (requires a `CHECKPOINT.md` plan entry) |

The toolchains are **not** interchangeable. `dotnet build` does not build ProjectWAR, and the
net48 MSBuild invocation does not build the toolkit hub. Each repo's `AGENTS.md` governs work
inside that repo; when a task spans both, satisfy both.

## Data roots

Paths verified present on this machine 2026-09-04.

| Root | What it is | Use it for |
|---|---|---|
| `C:\Users\Admin\Videos\Warhammer Online - Age of Reckoning` | **Live 1.4.8 client install.** `WAR.exe`, the `.myp` archives (`art`/`art2`/`art3`, `world`, `data`, `interface`, `audio`, `vo_english`, `mft`, `patch`, `dev`), `assetdb`, `Interface`, `user`, `notes` | Ground truth for client behavior. Disassembly, Lua/UI source, string tables, live archive contents |
| `C:\Users\Admin\Downloads\myps` | **Extracted client tree.** `art`, `assetdb`, `audio`, `data`, `interface`, `shaders`, `zones`, `videos`, `unknown_hashes` | Reading client data files directly (`data/gamedata/*.csv`, `data/bin/*.bin`, `data/strings/`, `interface/interfacecore/`). This is the root `ClientDataMatrix` and native LOS generation expect |
| `D:\Repos\Shmerrick\WAR-RE-Toolkit\libs\protocolservices\Packet Logs` | **1,027 official live-server packet captures** | Positions, spawn identity, packet layouts, opcode field offsets. The source behind scripts `04`, `10`, `11`, `12`, `14`, `17` |
| `D:\Repos\Shmerrick\WAR-RE-Toolkit\RE_FINDINGS` | Decoded findings by domain: `combat/`, `network/`, `world/`, `evidence/` | Check here **before** decoding anything yourself |
| `deps/zones/` (this repo) | Zone data, not in git — `zones.zip` from the `zones-data-v1` release | Runtime zone/LOS data |

Two paths appear in older docs and **do not exist**; do not reintroduce them:

- `C:\Users\Admin\Music\Warhammer\Warhammer\` — cited by `RE_FINDINGS/network/opcode_reference.md`
  (Opcodes.txt, PacketLogger) and `docs/reference/databaseimports.md` (`All_Items.xlsx`). Those
  findings stand; their source files are simply no longer on disk.
- `C:\Users\Admin\Pictures\WAR` — cited by the toolkit README as "canonical game data". Use the
  live install above instead.

## Which repo answers which question

| Question | Look here |
|---|---|
| Packet layout, opcode, field offset | `RE_FINDINGS/network/` (`opcode_reference.md`, `movement_packet.md`, `network_encryption.md`), then the packet-log corpus |
| Damage/combat formula parity | `RE_FINDINGS/combat/combat_formulas.md` |
| Zone, terrain, figleaf, NIF, archive structure | `RE_FINDINGS/world/`, `docs/research/client_file_architecture.md` |
| Asset hash / MYP archive contents | `RE_FINDINGS/world/` dehash reports, `apps/assethashhunter`, `apps/warmyptool` |
| Client UI, addon, Lua behavior | toolkit `docs/UI_ADDON_REFERENCE.md`, plus `interface.myp` in the live install |
| Ability/effect data semantics | **ProjectWAR** `ClientDataMatrix` + `docs/data-matrix/` (this repo owns that analysis) |
| Ward progression and scalars | **ProjectWAR** `docs/WARD_SYSTEM.md`; client-side sigil work is toolkit-private |
| Bot editor route contract | **ProjectWAR** `docs/bot-editor-api.md` is authoritative; the toolkit consumes it |
| What a known bug's status is | **ProjectWAR** `docs/INTERNAL_BUG_TRACKER.md` |
| Why something isn't on `RESTART` | **ProjectWAR** `docs/MASTER_TO_RESTART_AUDIT.md` |

## Direction of authority

When two sources disagree, this is the order:

1. The 1.4.8 client (binary, archives, data files)
2. Official live-server packet captures
3. Decoded toolkit findings in `RE_FINDINGS/`
4. ProjectWAR's own source and world database

Never invert it. The emulator's existing behavior is the thing being corrected, so "the server
already does X" is not evidence that X is right — that reasoning is how `master` drifted (see
`docs/MASTER_TO_RESTART_AUDIT.md`). Where no authority exists, leave the data unpopulated and say
so; several tracker entries (BUG-009, BUG-010, BUG-018, BUG-031) are open precisely because
inventing values was rejected.

## Working across both repos

A typical evidence-backed change:

1. **Search first.** Grep `RE_FINDINGS/` and the toolkit `docs/` before decoding anything — the
   answer is often already written down.
2. **Establish the authority.** Client file, capture, or disassembly. Record the exact
   file/offset/packet in the commit and in the doc you touch.
3. **Change ProjectWAR.** Code, plus a new numbered `Database/NN_*.sql` if existing rows need
   changing (`AGENTS.md` rules 3–4). Never edit the base dumps.
4. **Apply and verify the script** against the local Release database (`AGENTS.md` rule 6) — a
   clean compile alone is not verification of DB-backed work.
5. **Record it.** Update `docs/INTERNAL_BUG_TRACKER.md`, the relevant system doc, and a dated
   `docs/handoffs/` entry for a substantial session.
6. **If toolkit code or findings changed**, the toolkit's own `AGENTS.md` requires a
   `CHECKPOINT.md` entry there.

Cite the evidence path when explaining a change. Every doc in this repo that makes a factual
claim about 1.4.8 names its source; keep that property.

## Cross-repo contracts

Two surfaces are split across the repos and will break silently if only one side is changed.

**Bot editor API.** `WorldServer` serves it on `127.0.0.1:51933` under `EnableBotEditorAPI`.
`docs/bot-editor-api.md` in this repo defines the routes; the toolkit's
`apps/warclient/Services/BotEditorApiClient.cs`, the hub's embedded Bot Editor, and the standalone
`apps/bot-template-viewer` dashboard all consume them. Changing a route means updating that doc
and the toolkit client. The toolkit can also bypass the API and hit `war_characters` /
`war_world` directly, so schema changes to `bot_gear_overrides` reach it too.

**Ward sigil client component.** The stock 1.4.8 client never populates the target-frame sigil
field, so the server half alone cannot display it. ProjectWAR sends the tier as `F_WARD_INFO`
(`0xDF`) — an opcode a stock client discards, making it safe to send unconditionally. The client
component that reads it is maintained privately in the toolkit and shipped through the launcher's
patch manifest, not through source control. Nothing here depends on it. See `docs/WARD_SYSTEM.md`
and toolkit `docs/reference/ward-sigil-client-patch.md`.
