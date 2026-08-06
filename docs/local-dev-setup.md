# Local development setup

Notes for running this server locally. **Local dev credentials only** — nothing
here is a production secret, and nothing here should ever be reused on a
reachable host.

## Accounts

| Username | Password | GmLevel |
|---|---|---:|
| `gm` | `gm123456` | 40 |

GmLevel 40 is developer level (see `EGmLevel` in
`WorldServer/Managers/Commands/`), which is what the `/gm` command set checks.

### Resetting the password

The plaintext is never stored, so a forgotten password cannot be recovered — only
overwritten. `accounts.CryptPassword` holds one of two formats:

- **BCrypt** (60 chars, starts `$2`) — the current path, written by
  `AccountMgr.SetPassword` / `CreateAccount`.
- **Legacy** (64 chars, plain hex) — an unsalted
  `SHA256(lower(username) + ":" + lower(password))`, compared verbatim by
  `AccountMgr` when the stored value is not a BCrypt hash. This is what the local
  `gm` account uses.

To set the legacy hash directly, compute it and write it:

```python
import hashlib
hashlib.sha256(("gm:" + "yourpassword").encode()).hexdigest()
```

```sql
UPDATE war_accounts.accounts
   SET CryptPassword = '<hash>'
 WHERE Username = 'gm';
```

Note the derivation lowercases **both** username and password
(`Account.ConvertClientPasswordHash`), so passwords are effectively
case-insensitive on the legacy path.

## Databases

From `bin/Release/Configs/World.xml`:

| Role | Database |
|---|---|
| World | `war_world_curated` |
| Characters | `war_characters` |
| Accounts | `war_accounts` |

MariaDB on `127.0.0.1:3306`, user `root`.

## Migrations

Apply in order, **to the right database** — the file header of each says which:

| File | Database |
|---|---|
| `01_add_tokunlock3.sql` | World |
| `02_restore_mailboxes.sql` | World |
| `04_kill_collector_world.sql` | World |
| `05_kill_collector_characters.sql` | Characters |

There is no `03` on this branch: it fixes a RESTART-only bug that does not exist
on the master line. See `docs/known-issues/open-items.md` R1.

## Building

```
"C:\VSBuildTools\MSBuild\Current\Bin\msbuild.exe" WorldServer\WorldServer.csproj -p:Configuration=Release
```

A fresh worktree needs its own package restore, since `packages/` is not tracked
and RESTART wants newer packages than older branches:

```
msbuild ProjectWAR.sln -t:Restore -p:RestorePackagesConfig=true
```

`Launcher` currently fails to build (NLog 4/6 type conflict) — see
`docs/known-issues/open-items.md` F3. `WorldServer` builds clean.
